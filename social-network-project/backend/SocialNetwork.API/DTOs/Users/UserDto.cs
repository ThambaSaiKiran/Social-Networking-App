namespace SocialNetwork.API.DTOs.Users
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string ProfilePictureUrl { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int PostCount { get; set; }
        public int ConnectionCount { get; set; }
        public List<string> Interests { get; set; } = new();
        public List<string> Skills { get; set; } = new();
        public string ConnectionStatus { get; set; } = "none";
    }

    public class UserProfileDto : UserDto
    {
        public List<PostDto> RecentPosts { get; set; } = new();
        public List<UserDto> MutualFriends { get; set; } = new();
    }

    public class SuggestedUserDto : UserDto
    {
        public double RelevanceScore { get; set; }
        public List<string> SuggestionReasons { get; set; } = new();
        public int MutualFriendCount { get; set; }
        public int CommonInterests { get; set; }
        public int CommonSkills { get; set; }
    }
}
