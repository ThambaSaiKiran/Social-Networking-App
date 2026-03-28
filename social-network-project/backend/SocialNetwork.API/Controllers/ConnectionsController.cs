using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SocialNetwork.API.Data;
using SocialNetwork.API.DTOs.Connections;
using SocialNetwork.API.Models;

namespace SocialNetwork.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConnectionsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ConnectionsController(AppDbContext db)
        {
            _db = db;
        }

        private int GetCurrentUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub") ?? "0");

        /// <summary>Send a connection request</summary>
        [HttpPost("request/{targetUserId:int}")]
        public async Task<IActionResult> SendRequest(int targetUserId)
        {
            int currentUserId = GetCurrentUserId();
            if (currentUserId == targetUserId)
                return BadRequest(new { message = "Cannot connect with yourself." });

            bool targetExists = await _db.Users.AnyAsync(u => u.Id == targetUserId && u.IsActive);
            if (!targetExists) return NotFound(new { message = "User not found." });

            bool alreadyExists = await _db.Connections.AnyAsync(c =>
                (c.SenderId == currentUserId && c.ReceiverId == targetUserId) ||
                (c.SenderId == targetUserId && c.ReceiverId == currentUserId));

            if (alreadyExists)
                return Conflict(new { message = "Connection already exists or request already sent." });

            var connection = new Connection
            {
                SenderId = currentUserId,
                ReceiverId = targetUserId,
                Status = ConnectionStatus.Pending
            };

            _db.Connections.Add(connection);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Connection request sent.", connectionId = connection.Id });
        }

        /// <summary>Accept a connection request</summary>
        [HttpPut("accept/{connectionId:int}")]
        public async Task<IActionResult> AcceptRequest(int connectionId)
        {
            int currentUserId = GetCurrentUserId();
            var connection = await _db.Connections.FindAsync(connectionId);
            if (connection == null) return NotFound();
            if (connection.ReceiverId != currentUserId)
                return Forbid();
            if (connection.Status != ConnectionStatus.Pending)
                return BadRequest(new { message = "Request is not pending." });

            connection.Status = ConnectionStatus.Accepted;
            connection.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(new { message = "Connection accepted." });
        }

        /// <summary>Reject a connection request</summary>
        [HttpPut("reject/{connectionId:int}")]
        public async Task<IActionResult> RejectRequest(int connectionId)
        {
            int currentUserId = GetCurrentUserId();
            var connection = await _db.Connections.FindAsync(connectionId);
            if (connection == null) return NotFound();
            if (connection.ReceiverId != currentUserId) return Forbid();

            connection.Status = ConnectionStatus.Rejected;
            connection.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(new { message = "Connection rejected." });
        }

        /// <summary>Remove/disconnect from a user</summary>
        [HttpDelete("{targetUserId:int}")]
        public async Task<IActionResult> RemoveConnection(int targetUserId)
        {
            int currentUserId = GetCurrentUserId();
            var connection = await _db.Connections.FirstOrDefaultAsync(c =>
                ((c.SenderId == currentUserId && c.ReceiverId == targetUserId) ||
                 (c.SenderId == targetUserId && c.ReceiverId == currentUserId)) &&
                c.Status == ConnectionStatus.Accepted);

            if (connection == null) return NotFound(new { message = "Connection not found." });

            _db.Connections.Remove(connection);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Connection removed." });
        }

        /// <summary>Get pending connection requests (received)</summary>
        [HttpGet("requests/received")]
        public async Task<IActionResult> GetReceivedRequests()
        {
            int currentUserId = GetCurrentUserId();
            var requests = await _db.Connections
                .Include(c => c.Sender)
                .Where(c => c.ReceiverId == currentUserId && c.Status == ConnectionStatus.Pending)
                .Select(c => new ConnectionDto
                {
                    Id = c.Id,
                    SenderId = c.SenderId,
                    SenderUsername = c.Sender.Username,
                    SenderFullName = c.Sender.FullName,
                    SenderProfilePic = c.Sender.ProfilePictureUrl,
                    ReceiverId = c.ReceiverId,
                    Status = c.Status.ToString(),
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
            return Ok(requests);
        }

        /// <summary>Get all accepted connections (friends list)</summary>
        [HttpGet("my")]
        public async Task<IActionResult> GetMyConnections()
        {
            int currentUserId = GetCurrentUserId();
            var connections = await _db.Connections
                .Include(c => c.Sender)
                .Include(c => c.Receiver)
                .Where(c => (c.SenderId == currentUserId || c.ReceiverId == currentUserId)
                         && c.Status == ConnectionStatus.Accepted)
                .Select(c => new
                {
                    UserId = c.SenderId == currentUserId ? c.ReceiverId : c.SenderId,
                    Username = c.SenderId == currentUserId ? c.Receiver.Username : c.Sender.Username,
                    FullName = c.SenderId == currentUserId ? c.Receiver.FullName : c.Sender.FullName,
                    ProfilePic = c.SenderId == currentUserId ? c.Receiver.ProfilePictureUrl : c.Sender.ProfilePictureUrl,
                    ConnectedAt = c.UpdatedAt ?? c.CreatedAt
                })
                .ToListAsync();
            return Ok(connections);
        }
    }
}
