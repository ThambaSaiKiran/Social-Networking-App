using Microsoft.EntityFrameworkCore;
using SocialNetwork.API.Algorithms;
using SocialNetwork.API.Data;
using SocialNetwork.API.DTOs.Users;
using SocialNetwork.API.Models;

namespace SocialNetwork.API.Services
{
    public interface IRecommendationService
    {
        Task<List<SuggestedUserDto>> GetSuggestedUsersAsync(int currentUserId, int count = 10);
    }

    public class RecommendationService : IRecommendationService
    {
        private readonly AppDbContext _db;

        public RecommendationService(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Suggests users based on:
        /// 1. Mutual friends (BFS second-degree connections) — highest weight
        /// 2. Common interests
        /// 3. Common skills
        /// 4. Geographic proximity
        /// 5. Recency (newer accounts get slight boost for discoverability)
        /// </summary>
        public async Task<List<SuggestedUserDto>> GetSuggestedUsersAsync(int currentUserId, int count = 10)
        {
            // Build the social graph
            var acceptedConnections = await _db.Connections
                .Where(c => c.Status == ConnectionStatus.Accepted)
                .Select(c => new { c.SenderId, c.ReceiverId })
                .ToListAsync();

            var graph = new SocialGraph();
            graph.BuildGraph(acceptedConnections.Select(c => (c.SenderId, c.ReceiverId)));

            var directFriends = graph.GetDirectConnections(currentUserId);

            // Get users within 2 degrees who are NOT already connected
            var usersWithinTwoDegrees = graph.BfsGetUsersWithinDepth(currentUserId, 2);
            var secondDegreeUserIds = usersWithinTwoDegrees
                .Where(kv => kv.Value == 2 && !directFriends.Contains(kv.Key))
                .Select(kv => kv.Key)
                .ToHashSet();

            // Get all user data needed for scoring
            var currentUser = await _db.Users
                .Include(u => u.UserInterests).ThenInclude(ui => ui.Interest)
                .Include(u => u.UserSkills).ThenInclude(us => us.Skill)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (currentUser == null) return new List<SuggestedUserDto>();

            var currentInterestIds = currentUser.UserInterests.Select(ui => ui.InterestId).ToHashSet();
            var currentSkillIds = currentUser.UserSkills.Select(us => us.SkillId).ToHashSet();

            // Get already-connected or pending user IDs to exclude
            var existingConnections = await _db.Connections
                .Where(c => c.SenderId == currentUserId || c.ReceiverId == currentUserId)
                .Select(c => c.SenderId == currentUserId ? c.ReceiverId : c.SenderId)
                .ToHashSet();
            existingConnections.Add(currentUserId);

            // Fetch candidate users (second-degree + extra for variety)
            var candidateIds = secondDegreeUserIds.Except(existingConnections).ToList();
            if (candidateIds.Count < count)
            {
                // Add random active users if not enough 2nd-degree connections
                var extraUsers = await _db.Users
                    .Where(u => !existingConnections.Contains(u.Id) && u.IsActive)
                    .Select(u => u.Id)
                    .Take(50)
                    .ToListAsync();
                candidateIds = candidateIds.Union(extraUsers).ToList();
            }

            var candidates = await _db.Users
                .Include(u => u.UserInterests).ThenInclude(ui => ui.Interest)
                .Include(u => u.UserSkills).ThenInclude(us => us.Skill)
                .Include(u => u.Posts)
                .Where(u => candidateIds.Contains(u.Id) && u.IsActive)
                .ToListAsync();

            // Score each candidate using a max heap for top-K retrieval O(n log k)
            var scoringHeap = new MaxHeap<SuggestedUserDto>(u => u.RelevanceScore);

            foreach (var candidate in candidates)
            {
                int mutualCount = graph.CountMutualConnections(currentUserId, candidate.Id);
                int commonInterests = candidate.UserInterests.Count(ui => currentInterestIds.Contains(ui.InterestId));
                int commonSkills = candidate.UserSkills.Count(us => currentSkillIds.Contains(us.SkillId));

                bool isSecondDegree = secondDegreeUserIds.Contains(candidate.Id);
                double recencyBoost = 1.0 / (Math.Max(1, (DateTime.UtcNow - candidate.CreatedAt).TotalDays));

                // Weighted relevance score
                double score = (mutualCount * 15.0)
                             + (commonInterests * 10.0)
                             + (commonSkills * 8.0)
                             + (isSecondDegree ? 20.0 : 0.0)
                             + (recencyBoost * 5.0);

                var reasons = new List<string>();
                if (mutualCount > 0) reasons.Add($"{mutualCount} mutual connection{(mutualCount > 1 ? "s" : "")}");
                if (commonInterests > 0) reasons.Add($"{commonInterests} shared interest{(commonInterests > 1 ? "s" : "")}");
                if (commonSkills > 0) reasons.Add($"{commonSkills} matching skill{(commonSkills > 1 ? "s" : "")}");
                if (isSecondDegree && mutualCount == 0) reasons.Add("2nd-degree connection");

                var dto = new SuggestedUserDto
                {
                    Id = candidate.Id,
                    Username = candidate.Username,
                    FullName = candidate.FullName,
                    Email = candidate.Email,
                    Bio = candidate.Bio,
                    ProfilePictureUrl = candidate.ProfilePictureUrl,
                    Location = candidate.Location,
                    CreatedAt = candidate.CreatedAt,
                    PostCount = candidate.Posts.Count,
                    Interests = candidate.UserInterests.Select(ui => ui.Interest.Name).ToList(),
                    Skills = candidate.UserSkills.Select(us => us.Skill.Name).ToList(),
                    RelevanceScore = score,
                    SuggestionReasons = reasons,
                    MutualFriendCount = mutualCount,
                    CommonInterests = commonInterests,
                    CommonSkills = commonSkills
                };

                scoringHeap.Insert(dto);
            }

            // Extract top-K from the heap in O(k log n)
            return scoringHeap.ExtractTopK(count);
        }
    }
}
