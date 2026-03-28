using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.API.Models
{
    public class PostTag
    {
        public int Id { get; set; }

        public int PostId { get; set; }
        public Post Post { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Tag { get; set; } = string.Empty;
    }
}
