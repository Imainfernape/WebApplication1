using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Model
{
    public class AuthDbContext : IdentityDbContext
    {
        private readonly IConfiguration _configuration;

        public AuthDbContext(DbContextOptions<AuthDbContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = _configuration.GetConnectionString("AuthConnectionString");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        // Add DbSet for CustomerData and UserSessions
        public DbSet<CustomerData> CustomerData { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<UserPasswordHistory> UserPasswordHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CustomerData>().HasKey(cd => cd.Id);
            modelBuilder.Entity<UserSession>().HasKey(us => us.Id);
        }

        public async Task TrackSessionAsync(string userId, string sessionToken)
        {
            var existingSession = await UserSessions.FirstOrDefaultAsync(s => s.UserId == userId);

            if (existingSession != null)
            {
                UserSessions.Remove(existingSession); // Remove previous session (only one active session per user)
            }

            UserSessions.Add(new UserSession
            {
                UserId = userId,
                SessionToken = sessionToken,
                CreatedAt = DateTime.UtcNow
            });

            await SaveChangesAsync();
        }

        public async Task RemoveSessionAsync(string userId, string sessionToken)
        {
            var session = await UserSessions.FirstOrDefaultAsync(s => s.UserId == userId && s.SessionToken == sessionToken);
            if (session != null)
            {
                UserSessions.Remove(session);
                await SaveChangesAsync();
            }
        }
    }

    public class CustomerData
    {
        [Key] public int Id { get; set; }
        [Required] public string UserId { get; set; } = null!;
        [Required] public string EncryptedNRIC { get; set; } = null!;
        [Required] public string EncryptedAESKey { get; set; } = null!;
    }

    public class UserProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public string LastName { get; set; } = null!;

        public string? Gender { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string? WhoAmI { get; set; }
    }

    public class UserPasswordHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string HashedPassword { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class UserSession
    {
        [Key] public int Id { get; set; }
        [Required] public string UserId { get; set; } = null!;
        [Required] public string SessionToken { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
