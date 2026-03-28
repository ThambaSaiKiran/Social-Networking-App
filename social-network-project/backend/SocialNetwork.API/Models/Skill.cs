using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.API.Models
{
    public class Skill
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        public ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
    }

    public class UserSkill
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int SkillId { get; set; }
        public Skill Skill { get; set; } = null!;

        [MaxLength(20)]
        public string Level { get; set; } = "Intermediate";
    }
}
