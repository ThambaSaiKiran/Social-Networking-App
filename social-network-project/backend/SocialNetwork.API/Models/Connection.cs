namespace SocialNetwork.API.Models
{
    public enum ConnectionStatus
    {
        Pending,
        Accepted,
        Rejected,
        Blocked
    }

    public class Connection
    {
        public int Id { get; set; }

        public int SenderId { get; set; }
        public User Sender { get; set; } = null!;

        public int ReceiverId { get; set; }
        public User Receiver { get; set; } = null!;

        public ConnectionStatus Status { get; set; } = ConnectionStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
