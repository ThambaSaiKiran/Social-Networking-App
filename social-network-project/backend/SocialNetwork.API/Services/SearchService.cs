using Microsoft.EntityFrameworkCore;
using SocialNetwork.API.Algorithms;
using SocialNetwork.API.Data;
using SocialNetwork.API.DTOs.Users;

namespace SocialNetwork.API.Services
{
    public interface ISearchService
    {
        Task<List<UserDto>> SearchUsersAsync(string query, int currentUserId, int page = 1, int pageSize = 20);
        Task<List<string>> AutoCompleteAsync(string prefix);
        Task RebuildTrieAsync();
    }

    public class SearchService : ISearchService
    {
        private readonly AppDbContext _db;
        private readonly Trie _usernameTrie = new();
        private readonly Trie _fullNameTrie = new();
        private bool _trieBuilt = false;
        private readonly SemaphoreSlim _lock = new(1, 1);

        public SearchService(AppDbContext db)
        {
            _db = db;
        }

        private async Task EnsureTrieBuiltAsync()
        {
            if (_trieBuilt) return;
            await _lock.WaitAsync();
            try
            {
                if (_trieBuilt) return;
                await RebuildTrieAsync();
                _trieBuilt = true;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Rebuild the Trie index from the database.
        /// Each username and fullname token is inserted for O(L) lookup.
        /// </summary>
        public async Task RebuildTrieAsync()
        {
            var users = await _db.Users
                .Where(u => u.IsActive)
                .Select(u => new { u.Id, u.Username, u.FullName })
                .ToListAsync();

            foreach (var user in users)
            {
                _usernameTrie.Insert(user.Username, user.Id);
                foreach (var token in user.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    _fullNameTrie.Insert(token.ToLower(), user.Id);
            }
        }

        /// <summary>
        /// Search users by username or full name using Trie prefix lookup O(L)
        /// then fetch full details from DB for matched IDs.
        /// Falls back to SQL LIKE for edge cases.
        /// </summary>
        public async Task<List<UserDto>> SearchUsersAsync(string query, int currentUserId, int page = 1, int pageSize = 20)
        {
            await EnsureTrieBuiltAsync();

            query = query.Trim().ToLower();
            if (string.IsNullOrEmpty(query)) return new List<UserDto>();

            // Use Trie for prefix-based lookup
            var matchedIds = _usernameTrie.SearchByPrefix(query, 100)
                .Union(_fullNameTrie.SearchByPrefix(query, 100))
                .Distinct()
                .ToHashSet();

            // SQL fallback for substring matching
            List<UserDto> results;
            if (matchedIds.Count > 0)
            {
                results = await _db.Users
                    .Include(u => u.UserInterests).ThenInclude(ui => ui.Interest)
                    .Include(u => u.UserSkills).ThenInclude(us => us.Skill)
                    .Include(u => u.Posts)
                    .Where(u => matchedIds.Contains(u.Id) && u.IsActive)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => MapUserToDto(u, currentUserId))
                    .ToListAsync();
            }
            else
            {
                // SQL fallback
                results = await _db.Users
                    .Include(u => u.UserInterests).ThenInclude(ui => ui.Interest)
                    .Include(u => u.UserSkills).ThenInclude(us => us.Skill)
                    .Include(u => u.Posts)
                    .Where(u => u.IsActive &&
                        (u.Username.Contains(query) ||
                         u.FullName.Contains(query) ||
                         u.Bio.Contains(query)))
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => MapUserToDto(u, currentUserId))
                    .ToListAsync();
            }

            return results;
        }

        /// <summary>
        /// Return autocomplete suggestions for a prefix. Time: O(L)
        /// </summary>
        public async Task<List<string>> AutoCompleteAsync(string prefix)
        {
            await EnsureTrieBuiltAsync();
            if (string.IsNullOrEmpty(prefix)) return new List<string>();

            var suggestions = _usernameTrie.AutoComplete(prefix, 5)
                .Union(_fullNameTrie.AutoComplete(prefix, 5))
                .Distinct()
                .Take(10)
                .ToList();

            return suggestions;
        }

        private static UserDto MapUserToDto(User u, int currentUserId)
        {
            return new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FullName = u.FullName,
                Bio = u.Bio,
                ProfilePictureUrl = u.ProfilePictureUrl,
                Location = u.Location,
                CreatedAt = u.CreatedAt,
                PostCount = u.Posts.Count,
                Interests = u.UserInterests.Select(ui => ui.Interest.Name).ToList(),
                Skills = u.UserSkills.Select(us => us.Skill.Name).ToList(),
            };
        }
    }
}
