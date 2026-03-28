using Microsoft.EntityFrameworkCore;
using SocialNetwork.API.Algorithms;
using SocialNetwork.API.Data;
using SocialNetwork.API.DTOs.Users;
using SocialNetwork.API.Models;

namespace SocialNetwork.API.Services
{
    public interface IPostRankingService
    {
        Task<List<PostDto>> GetTrendingPostsAsync(int currentUserId, int count = 20);
        Task<List<PostDto>> GetPersonalizedFeedAsync(int currentUserId, int page = 1, int pageSize = 20);
        Task UpdateTrendingScoresAsync();
    }

    public class PostRankingService : IPostRankingService
    {
        private readonly AppDbContext _db;

        public PostRankingService(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Returns top-K trending posts using a Max-Heap.
        /// Trending score is computed as engagement / time^gravity (decay).
        /// Time: O(n log k)
        /// </summary>
        public async Task<List<PostDto>> GetTrendingPostsAsync(int currentUserId, int count = 20)
        {
            var posts = await _db.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Include(p => p.PostTags)
                .OrderByDescending(p => p.CreatedAt)
                .Take(500) // candidate pool
                .ToListAsync();

            var likedPostIds = await _db.Likes
                .Where(l => l.UserId == currentUserId)
                .Select(l => l.PostId)
                .ToHashSet();

            var heap = new MaxHeap<PostDto>(p => p.TrendingScore);

            foreach (var post in posts)
            {
                double score = PostRankingHelper.CalculateTrendingScore(
                    post.Likes.Count, post.Comments.Count, post.ViewCount, post.CreatedAt);

                heap.Insert(new PostDto
                {
                    Id = post.Id,
                    Content = post.Content,
                    ImageUrl = post.ImageUrl,
                    UserId = post.UserId,
                    AuthorUsername = post.User.Username,
                    AuthorFullName = post.User.FullName,
                    AuthorProfilePic = post.User.ProfilePictureUrl,
                    LikeCount = post.Likes.Count,
                    CommentCount = post.Comments.Count,
                    ViewCount = post.ViewCount,
                    TrendingScore = score,
                    CreatedAt = post.CreatedAt,
                    Tags = post.PostTags.Select(pt => pt.Tag).ToList(),
                    IsLikedByCurrentUser = likedPostIds.Contains(post.Id)
                });
            }

            return heap.ExtractTopK(count);
        }

        /// <summary>
        /// Personalized feed: posts from connections ranked by trending score.
        /// </summary>
        public async Task<List<PostDto>> GetPersonalizedFeedAsync(int currentUserId, int page = 1, int pageSize = 20)
        {
            var connectedUserIds = await _db.Connections
                .Where(c => (c.SenderId == currentUserId || c.ReceiverId == currentUserId)
                         && c.Status == ConnectionStatus.Accepted)
                .Select(c => c.SenderId == currentUserId ? c.ReceiverId : c.SenderId)
                .ToListAsync();

            connectedUserIds.Add(currentUserId);

            var posts = await _db.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Include(p => p.PostTags)
                .Where(p => connectedUserIds.Contains(p.UserId))
                .OrderByDescending(p => p.CreatedAt)
                .Take(200)
                .ToListAsync();

            var likedPostIds = await _db.Likes
                .Where(l => l.UserId == currentUserId)
                .Select(l => l.PostId)
                .ToHashSet();

            var heap = new MaxHeap<PostDto>(p => p.TrendingScore);
            foreach (var post in posts)
            {
                double score = PostRankingHelper.CalculateTrendingScore(
                    post.Likes.Count, post.Comments.Count, post.ViewCount, post.CreatedAt);

                heap.Insert(new PostDto
                {
                    Id = post.Id,
                    Content = post.Content,
                    ImageUrl = post.ImageUrl,
                    UserId = post.UserId,
                    AuthorUsername = post.User.Username,
                    AuthorFullName = post.User.FullName,
                    AuthorProfilePic = post.User.ProfilePictureUrl,
                    LikeCount = post.Likes.Count,
                    CommentCount = post.Comments.Count,
                    ViewCount = post.ViewCount,
                    TrendingScore = score,
                    CreatedAt = post.CreatedAt,
                    Tags = post.PostTags.Select(pt => pt.Tag).ToList(),
                    IsLikedByCurrentUser = likedPostIds.Contains(post.Id)
                });
            }

            return heap.ExtractTopK(pageSize * page)
                       .Skip((page - 1) * pageSize)
                       .Take(pageSize)
                       .ToList();
        }

        public async Task UpdateTrendingScoresAsync()
        {
            var posts = await _db.Posts.Include(p => p.Likes).Include(p => p.Comments).ToListAsync();
            foreach (var post in posts)
            {
                post.TrendingScore = PostRankingHelper.CalculateTrendingScore(
                    post.Likes.Count, post.Comments.Count, post.ViewCount, post.CreatedAt);
            }
            await _db.SaveChangesAsync();
        }
    }
}
