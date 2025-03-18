using ArtSharing.Data.Models.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ArtSharing.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Report> Reports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Like>()
             .HasOne(l => l.User)
             .WithMany(u => u.Likes)
             .HasForeignKey(l => l.UserId)
             .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Comment>()
             .HasOne(c => c.User)
             .WithMany(u => u.Comments)
             .HasForeignKey(c => c.UserId)
             .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Post>()
             .HasOne(p => p.User)
             .WithMany(u => u.Posts)
             .HasForeignKey(p => p.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Report>()
            .HasOne(r => r.ReportedBy)
            .WithMany(u => u.Reports)
            .HasForeignKey(r => r.ReportedById)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
            .HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<SavedPost>()
            .HasOne(sp => sp.User)
            .WithMany(u => u.SavedPosts)
            .HasForeignKey(sp => sp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SavedPost>()
             .HasOne(sp => sp.Post)
             .WithMany(p => p.SavedByUsers)
             .HasForeignKey(sp => sp.PostId)
             .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserFollow>()
            .HasOne(f => f.Follower)
            .WithMany(u => u.Following)
            .HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserFollow>()
            .HasOne(f => f.Following)
            .WithMany(u => u.Followers)
            .HasForeignKey(f => f.FollowingId)
            .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
