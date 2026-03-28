using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.API.Models
{
    public class Interest
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        public ICollection<UserInterest> UserInterests { get; set; } = new List<UserInterest>();
    }

    public class UserInterest
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int InterestId { get; set; }
        public Interest Interest { get; set; } = null!;
    }
}
