using TE_Project.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TE_Project.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Use the 'new' keyword to hide the inherited 'Users' property
        public new DbSet<User> Users { get; set; }

        public DbSet<Plant> Plants { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<UploadedFile> UploadedFiles { get; set; }
        public new DbSet<UserToken> UserTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User - Plant relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.Plant)
                .WithMany(p => p.Admins)
                .HasForeignKey(u => u.PlantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Submission - Plant relationship
            modelBuilder.Entity<Submission>()
                .HasOne(s => s.Plant)
                .WithMany(p => p.Submissions)
                .HasForeignKey(s => s.PlantId)
                .OnDelete(DeleteBehavior.Restrict);

            // UploadedFile - Submission relationship
            modelBuilder.Entity<UploadedFile>()
                .HasOne(u => u.Submission)
                .WithMany(s => s.Files)
                .HasForeignKey(u => u.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserToken - User relationship
            modelBuilder.Entity<UserToken>()
                .HasOne(ut => ut.User)
                .WithMany()
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add indexes for frequently queried columns
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.PlantId);

            modelBuilder.Entity<Submission>()
                .HasIndex(s => s.PlantId);

            modelBuilder.Entity<Submission>()
                .HasIndex(s => s.CreatedAt);

            modelBuilder.Entity<UserToken>()
                .HasIndex(ut => ut.Token)
                .IsUnique();

            modelBuilder.Entity<UserToken>()
                .HasIndex(ut => ut.UserId);
        }
    }
}