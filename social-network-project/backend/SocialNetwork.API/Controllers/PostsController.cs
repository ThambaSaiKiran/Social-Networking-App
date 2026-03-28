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
    public class PostsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IPostRankingService _rankingService;

        public PostsController(AppDbContext db, IPostRankingService rankingService)
        {
            _db = db;
            _rankingService = rankingService;
        }

        private int GetCurrentUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub") ?? "0");

        /// <summary>Get personalized feed (posts from connections ranked by score)</summary>
        [HttpGet("feed")]
        public async Task<IActionResult> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            int userId = GetCurrentUserId();
            var posts = await _rankingService.GetPersonalizedFeedAsync(userId, page, pageSize);
            return Ok(posts);
        }

        /// <summary>Get trending posts</summary>
        [HttpGet("trending")]
        public async Task<IActionResult> GetTrending([FromQuery] int count = 20)
        {
            int userId = GetCurrentUserId();
            var posts = await _rankingService.GetTrendingPostsAsync(userId, count);
            return Ok(posts);
        }

        /// <summary>Create a new post</summary>
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            int userId = GetCurrentUserId();

            var post = new Post
            {
                UserId = userId,
                Content = dto.Content,
                ImageUrl = dto.ImageUrl,
            };

            foreach (var tag in dto.Tags.Select(t => t.ToLower().TrimStart('#')))
                post.PostTags.Add(new PostTag { Tag = tag });

            _db.Posts.Add(post);
            await _db.SaveChangesAsync();

            var user = await _db.Users.FindAsync(userId);
            return CreatedAtAction(nameof(GetPostById), new { id = post.Id }, new PostDto
            {
                Id = post.Id,
                Content = post.Content,
                ImageUrl = post.ImageUrl,
                UserId = post.UserId,
                AuthorUsername = user?.Username ?? "",
                AuthorFullName = user?.FullName ?? "",
                AuthorProfilePic = user?.ProfilePictureUrl ?? "",
                LikeCount = 0,
                CommentCount = 0,
                ViewCount = 0,
                TrendingScore = 0,
                CreatedAt = post.CreatedAt,
                Tags = dto.Tags
            });
        }

        /// <summary>Get a specific post</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPostById(int id)
        {
            int userId = GetCurrentUserId();
            var post = await _db.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Include(p => p.PostTags)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();

            // Increment view count
            post.ViewCount++;
            await _db.SaveChangesAsync();

            bool isLiked = await _db.Likes.AnyAsync(l => l.UserId == userId && l.PostId == id);

            return Ok(new PostDto
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
                TrendingScore = post.TrendingScore,
                CreatedAt = post.CreatedAt,
                Tags = post.PostTags.Select(pt => pt.Tag).ToList(),
                IsLikedByCurrentUser = isLiked
            });
        }

        /// <summary>Delete a post</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            int userId = GetCurrentUserId();
            var post = await _db.Posts.FindAsync(id);
            if (post == null) return NotFound();
            if (post.UserId != userId) return Forbid();

            _db.Posts.Remove(post);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Like or unlike a post</summary>
        [HttpPost("{id:int}/like")]
        public async Task<IActionResult> ToggleLike(int id)
        {
            int userId = GetCurrentUserId();
            var post = await _db.Posts.Include(p => p.Likes).FirstOrDefaultAsync(p => p.Id == id);
            if (post == null) return NotFound();

            var existingLike = post.Likes.FirstOrDefault(l => l.UserId == userId);
            bool liked;
            if (existingLike != null)
            {
                _db.Likes.Remove(existingLike);
                liked = false;
            }
            else
            {
                _db.Likes.Add(new Like { UserId = userId, PostId = id });
                liked = true;
            }
            await _db.SaveChangesAsync();
            return Ok(new { liked, likeCount = post.Likes.Count + (liked ? 0 : -1) });
        }

        /// <summary>Add a comment to a post</summary>
        [HttpPost("{id:int}/comments")]
        public async Task<IActionResult> AddComment(int id, [FromBody] CommentCreateDto dto)
        {
            int userId = GetCurrentUserId();
            var post = await _db.Posts.FindAsync(id);
            if (post == null) return NotFound();

            var comment = new Comment { UserId = userId, PostId = id, Content = dto.Content };
            _db.Comments.Add(comment);
            await _db.SaveChangesAsync();

            var user = await _db.Users.FindAsync(userId);
            return Ok(new
            {
                comment.Id,
                comment.Content,
                comment.CreatedAt,
                AuthorUsername = user?.Username,
                AuthorFullName = user?.FullName,
                AuthorProfilePic = user?.ProfilePictureUrl
            });
        }

        /// <summary>Get comments for a post</summary>
        [HttpGet("{id:int}/comments")]
        public async Task<IActionResult> GetComments(int id)
        {
            var comments = await _db.Comments
                .Include(c => c.User)
                .Where(c => c.PostId == id)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.CreatedAt,
                    AuthorUsername = c.User.Username,
                    AuthorFullName = c.User.FullName,
                    AuthorProfilePic = c.User.ProfilePictureUrl,
                    AuthorId = c.UserId
                })
                .ToListAsync();

            return Ok(comments);
        }
    }

    public class CommentCreateDto
    {
        public string Content { get; set; } = string.Empty;
    }
}
