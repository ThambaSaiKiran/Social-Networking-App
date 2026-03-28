using Microsoft.EntityFrameworkCore;
using SocialNetwork.API.Models;

namespace SocialNetwork.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Connection> Connections { get; set; }
        public DbSet<Interest> Interests { get; set; }
        public DbSet<UserInterest> UserInterests { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<UserSkill> UserSkills { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<PostTag> PostTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
            });

            // UserInterest composite key
            modelBuilder.Entity<UserInterest>(entity =>
            {
                entity.HasKey(ui => new { ui.UserId, ui.InterestId });
                entity.HasOne(ui => ui.User)
                      .WithMany(u => u.UserInterests)
                      .HasForeignKey(ui => ui.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(ui => ui.Interest)
                      .WithMany(i => i.UserInterests)
                      .HasForeignKey(ui => ui.InterestId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // UserSkill composite key
            modelBuilder.Entity<UserSkill>(entity =>
            {
                entity.HasKey(us => new { us.UserId, us.SkillId });
                entity.HasOne(us => us.User)
                      .WithMany(u => u.UserSkills)
                      .HasForeignKey(us => us.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(us => us.Skill)
                      .WithMany(s => s.UserSkills)
                      .HasForeignKey(us => us.SkillId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Connection: prevent cascade cycles
            modelBuilder.Entity<Connection>(entity =>
            {
                entity.HasOne(c => c.Sender)
                      .WithMany(u => u.SentConnections)
                      .HasForeignKey(c => c.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(c => c.Receiver)
                      .WithMany(u => u.ReceivedConnections)
                      .HasForeignKey(c => c.ReceiverId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Like: unique per user per post
            modelBuilder.Entity<Like>(entity =>
            {
                entity.HasIndex(l => new { l.UserId, l.PostId }).IsUnique();
                entity.HasOne(l => l.User)
                      .WithMany(u => u.Likes)
                      .HasForeignKey(l => l.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(l => l.Post)
                      .WithMany(p => p.Likes)
                      .HasForeignKey(l => l.PostId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Comment
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasOne(c => c.User)
                      .WithMany(u => u.Comments)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(c => c.Post)
                      .WithMany(p => p.Comments)
                      .HasForeignKey(c => c.PostId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Post
            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasOne(p => p.User)
                      .WithMany(u => u.Posts)
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // PostTag
            modelBuilder.Entity<PostTag>(entity =>
            {
                entity.HasOne(pt => pt.Post)
                      .WithMany(p => p.PostTags)
                      .HasForeignKey(pt => pt.PostId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
