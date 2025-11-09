using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartClass.Domain.Entities;
using SmartClass.Domain.Enums;
using SmartClass.Infrastructure.Identity.Entities;


namespace SmartClass.Infrastructure.Persistence
{
    public class AppDbContext
       : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Classroom> Classrooms => Set<Classroom>();
        public DbSet<ClassMember> ClassMembers => Set<ClassMember>();
        public DbSet<Material> Materials => Set<Material>();
        public DbSet<FileResource> Files => Set<FileResource>();
        public DbSet<Assignment> Assignments => Set<Assignment>();
        public DbSet<Submission> Submissions => Set<Submission>();
        public DbSet<Grade> Grades => Set<Grade>();
        public DbSet<Channel> Channels => Set<Channel>();
        public DbSet<ChannelMember> ChannelMembers => Set<ChannelMember>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<Meeting> Meetings => Set<Meeting>();
        public DbSet<MeetingParticipant> MeetingParticipants => Set<MeetingParticipant>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ClassMember>(b =>
            {
                b.HasOne(cm => cm.Classroom)
                 .WithMany(c => c.Members)
                 .HasForeignKey(cm => cm.ClassroomId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.Property(cm => cm.ClassroomId).IsRequired();
                b.HasIndex(cm => new { cm.ClassroomId, cm.UserId }).IsUnique();
            });

            modelBuilder.Entity<Channel>().Property(x => x.Type).HasConversion<int>();
            modelBuilder.Entity<Submission>().Property(x => x.Status).HasConversion<int>();

            modelBuilder.Entity<Classroom>().HasIndex(x => x.JoinCode).IsUnique();
            modelBuilder.Entity<Assignment>().HasIndex(x => new { x.ClassroomId, x.DueAt });
            modelBuilder.Entity<Message>().HasIndex(x => new { x.ChannelId, x.CreatedAt });
            modelBuilder.Entity<Notification>().HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAt });
            modelBuilder.Entity<Submission>().HasIndex(x => new { x.AssignmentId, x.StudentId }).IsUnique();

            modelBuilder.Entity<ClassMember>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Assignment>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.CreatedBy)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Message>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.AuthorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Grade>(b =>
            {
                b.Property(g => g.Score).HasPrecision(18, 2);
            });

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(x => new { x.UserId, x.Token, x.IsRevoked });
        }
    }
}
