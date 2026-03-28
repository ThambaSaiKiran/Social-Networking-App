using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SocialNetwork.API.Data;
using SocialNetwork.API.DTOs.Users;
using SocialNetwork.API.Models;
using SocialNetwork.API.Services;

namespace SocialNetwork.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IRecommendationService _recommendationService;

        public UsersController(AppDbContext db, IRecommendationService recommendationService)
        {
            _db = db;
            _recommendationService = recommendationService;
        }

        private int GetCurrentUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub") ?? "0");

        /// <summary>Get all users (user list)</summary>
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            int currentUserId = GetCurrentUserId();

            var users = await _db.Users
                .Include(u => u.UserInterests).ThenInclude(ui => ui.Interest)
                .Include(u => u.UserSkills).ThenInclude(us => us.Skill)
                .Include(u => u.Posts)
                .Include(u => u.SentConnections)
                .Include(u => u.ReceivedConnections)
                .Where(u => u.IsActive)
                .OrderBy(u => u.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var existingConnections = await _db.Connections
                .Where(c => c.SenderId == currentUserId || c.ReceiverId == currentUserId)
                .ToListAsync();

            var dtos = users.Select(u => new UserDto
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
                ConnectionCount = u.SentConnections.Count(c => c.Status == ConnectionStatus.Accepted)
                                + u.ReceivedConnections.Count(c => c.Status == ConnectionStatus.Accepted),
                Interests = u.UserInterests.Select(ui => ui.Interest.Name).ToList(),
                Skills = u.UserSkills.Select(us => us.Skill.Name).ToList(),
                ConnectionStatus = GetConnectionStatus(existingConnections, currentUserId, u.Id)
            }).ToList();

            var totalCount = await _db.Users.CountAsync(u => u.IsActive);

            return Ok(new { users = dtos, totalCount, page, pageSize });
        }

        /// <summary>Get a user profile by ID</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            int currentUserId = GetCurrentUserId();

            var user = await _db.Users
                .Include(u => u.UserInterests).ThenInclude(ui => ui.Interest)
                .Include(u => u.UserSkills).ThenInclude(us => us.Skill)
                .Include(u => u.Posts).ThenInclude(p => p.Likes)
                .Include(u => u.Posts).ThenInclude(p => p.Comments)
                .Include(u => u.Posts).ThenInclude(p => p.PostTags)
                .Include(u => u.SentConnections)
                .Include(u => u.ReceivedConnections)
                .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);

            if (user == null) return NotFound(new { message = "User not found." });

            var existingConnections = await _db.Connections
                .Where(c => (c.SenderId == currentUserId && c.ReceiverId == id)
                          || (c.SenderId == id && c.ReceiverId == currentUserId))
                .ToListAsync();

            // Get mutual friends
            var myFriends = await _db.Connections
                .Where(c => (c.SenderId == currentUserId || c.ReceiverId == currentUserId)
                         && c.Status == ConnectionStatus.Accepted)
                .Select(c => c.SenderId == currentUserId ? c.ReceiverId : c.SenderId)
                .ToHashSet();

            var theirFriends = await _db.Connections
                .Where(c => (c.SenderId == id || c.ReceiverId == id)
                         && c.Status == ConnectionStatus.Accepted)
                .Select(c => c.SenderId == id ? c.ReceiverId : c.SenderId)
                .ToHashSet();

            var mutualIds = myFriends.Intersect(theirFriends).ToList();
            var mutualUsers = await _db.Users
                .Where(u => mutualIds.Contains(u.Id))
                .Select(u => new UserDto
                {
                    Id = u.Id, Username = u.Username, FullName = u.FullName, ProfilePictureUrl = u.ProfilePictureUrl
                })
                .Take(6)
                .ToListAsync();

            var likedPostIds = await _db.Likes
                .Where(l => l.UserId == currentUserId)
                .Select(l => l.PostId)
                .ToHashSet();

            var dto = new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Bio = user.Bio,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Location = user.Location,
                CreatedAt = user.CreatedAt,
                PostCount = user.Posts.Count,
                ConnectionCount = user.SentConnections.Count(c => c.Status == ConnectionStatus.Accepted)
                                + user.ReceivedConnections.Count(c => c.Status == ConnectionStatus.Accepted),
                Interests = user.UserInterests.Select(ui => ui.Interest.Name).ToList(),
                Skills = user.UserSkills.Select(us => us.Skill.Name).ToList(),
                ConnectionStatus = GetConnectionStatus(existingConnections, currentUserId, id),
                MutualFriends = mutualUsers,
                RecentPosts = user.Posts
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(10)
                    .Select(p => new PostDto
                    {
                        Id = p.Id,
                        Content = p.Content,
                        ImageUrl = p.ImageUrl,
                        UserId = p.UserId,
                        AuthorUsername = user.Username,
                        AuthorFullName = user.FullName,
                        AuthorProfilePic = user.ProfilePictureUrl,
                        LikeCount = p.Likes.Count,
                        CommentCount = p.Comments.Count,
                        ViewCount = p.ViewCount,
                        TrendingScore = p.TrendingScore,
                        CreatedAt = p.CreatedAt,
                        Tags = p.PostTags.Select(pt => pt.Tag).ToList(),
                        IsLikedByCurrentUser = likedPostIds.Contains(p.Id)
                    }).ToList()
            };

            return Ok(dto);
        }

        /// <summary>Get AI-powered user suggestions</summary>
        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSuggestions([FromQuery] int count = 10)
        {
            int currentUserId = GetCurrentUserId();
            var suggestions = await _recommendationService.GetSuggestedUsersAsync(currentUserId, count);
            return Ok(suggestions);
        }

        /// <summary>Get the currently logged-in user's profile</summary>
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            int currentUserId = GetCurrentUserId();
            return await GetUserById(currentUserId);
        }

        /// <summary>Get all available interests</summary>
        [HttpGet("interests")]
        [AllowAnonymous]
        public async Task<IActionResult> GetInterests()
        {
            var interests = await _db.Interests.Select(i => new { i.Id, i.Name, i.Category }).ToListAsync();
            return Ok(interests);
        }

        /// <summary>Get all available skills</summary>
        [HttpGet("skills")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSkills()
        {
            var skills = await _db.Skills.Select(s => new { s.Id, s.Name, s.Category }).ToListAsync();
            return Ok(skills);
        }

        private static string GetConnectionStatus(List<Connection> connections, int currentUserId, int targetUserId)
        {
            if (currentUserId == targetUserId) return "self";
            var conn = connections.FirstOrDefault(c =>
                (c.SenderId == currentUserId && c.ReceiverId == targetUserId) ||
                (c.SenderId == targetUserId && c.ReceiverId == currentUserId));
            if (conn == null) return "none";
            if (conn.Status == ConnectionStatus.Accepted) return "connected";
            if (conn.Status == ConnectionStatus.Pending)
                return conn.SenderId == currentUserId ? "pending_sent" : "pending_received";
            return "none";
        }
    }
}
