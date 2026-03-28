using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.API.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? ImageUrl { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int ViewCount { get; set; } = 0;
        public double TrendingScore { get; set; } = 0;

        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    }
}
