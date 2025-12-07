using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartClass.Domain.Entities;
using SmartClass.Infrastructure.Identity.Entities;

namespace SmartClass.Infrastructure.Persistence
{
    public class AppDbContext
        : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Core
        public DbSet<Classroom> Classrooms => Set<Classroom>();
        public DbSet<ClassMember> ClassMembers => Set<ClassMember>();
        public DbSet<Material> Materials => Set<Material>();
        public DbSet<FileResource> Files => Set<FileResource>();
        public DbSet<Assignment> Assignments => Set<Assignment>();
        public DbSet<Submission> Submissions => Set<Submission>();
        public DbSet<Grade> Grades => Set<Grade>();
        public DbSet<Notification> Notifications => Set<Notification>();

        // Communications
        public DbSet<Channel> Channels => Set<Channel>();
        public DbSet<ChannelMember> ChannelMembers => Set<ChannelMember>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<Meeting> Meetings => Set<Meeting>();
        public DbSet<MeetingParticipant> MeetingParticipants => Set<MeetingParticipant>();

        // Auth
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================
            // CLASSROOM
            // =========================
            modelBuilder.Entity<Classroom>(b =>
            {
                b.HasKey(x => x.Id);

                b.Property(x => x.Title)
                    .IsRequired()
                    .HasMaxLength(120);

                b.Property(x => x.Section)
                    .HasMaxLength(60);

                b.Property(x => x.JoinCode)
                    .IsRequired()
                    .HasMaxLength(12);

                b.HasIndex(x => x.JoinCode).IsUnique();

                b.HasOne<ApplicationUser>()
                 .WithMany()
                 .HasForeignKey(x => x.OwnerId)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasMany(x => x.Members)
                 .WithOne(m => m.Classroom)
                 .HasForeignKey(m => m.ClassroomId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasMany(x => x.Channels)
                 .WithOne(ch => ch.Classroom)
                 .HasForeignKey(ch => ch.ClassroomId)
                 .OnDelete(DeleteBehavior.Cascade);

                // Навігації на Materials / Assignments налаштовуються в їхніх конфігах
            });

            // =========================
            // CLASS MEMBER
            // =========================
            modelBuilder.Entity<ClassMember>(b =>
            {
                b.HasKey(x => x.Id);

                b.HasIndex(x => new { x.ClassroomId, x.UserId }).IsUnique();

                b.Property(x => x.RoleInClass)
                 .HasConversion<string>()
                 .HasMaxLength(20)
                 .IsRequired();

                b.HasOne<ApplicationUser>()
                 .WithMany()
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.NoAction);
            });

            // =========================
            // MATERIAL
            // =========================
            modelBuilder.Entity<Material>(b =>
            {
                b.HasKey(x => x.Id);

                b.Property(x => x.Title)
                 .IsRequired()
                 .HasMaxLength(200);

                b.Property(x => x.Description)
                 .HasMaxLength(1000);

                b.HasIndex(x => new { x.ClassroomId, x.Title });

                b.HasOne(x => x.Classroom)
                 .WithMany(c => c.Materials)
                 .HasForeignKey(x => x.ClassroomId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne<ApplicationUser>()
                 .WithMany()
                 .HasForeignKey(x => x.CreatedBy)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasMany(x => x.Assignments)
                 .WithOne(a => a.Material)
                 .HasForeignKey(a => a.MaterialId)
                 .OnDelete(DeleteBehavior.SetNull);

                b.HasMany(x => x.Files)
                 .WithOne(f => f.Material)
                 .HasForeignKey(f => f.MaterialId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // =========================
            // ASSIGNMENT
            // =========================
            modelBuilder.Entity<Assignment>(b =>
            {
                b.HasKey(x => x.Id);

                b.Property(x => x.Title)
                 .IsRequired()
                 .HasMaxLength(200);

                b.Property(x => x.Status)
                 .IsRequired()
                 .HasMaxLength(20);

                b.HasIndex(x => new { x.ClassroomId, x.DueAt });
                b.HasIndex(x => x.MaterialId);

                b.HasOne<Classroom>()
                 .WithMany(c => c.Assignments)
                 .HasForeignKey(x => x.ClassroomId)
                 .OnDelete(DeleteBehavior.NoAction); // щоб не створювати multiple cascade paths

                b.HasOne<ApplicationUser>()
                 .WithMany()
                 .HasForeignKey(x => x.CreatedBy)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(x => x.Material)
                 .WithMany(m => m.Assignments)
                 .HasForeignKey(x => x.MaterialId)
                 .OnDelete(DeleteBehavior.SetNull);

                b.HasMany(x => x.Files)
                 .WithOne(f => f.Assignment)
                 .HasForeignKey(f => f.AssignmentId)
                 .OnDelete(DeleteBehavior.NoAction);

                b.HasMany(x => x.Submissions)
                 .WithOne(s => s.Assignment)
                 .HasForeignKey(s => s.AssignmentId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // =========================
            // SUBMISSION
            // =========================
            modelBuilder.Entity<Submission>(b =>
            {
                b.HasKey(x => x.Id);

                b.HasIndex(x => new { x.AssignmentId, x.StudentId }).IsUnique();

                b.Property(x => x.Status)
                 .HasConversion<int>();

                b.HasOne(x => x.Assignment)
                 .WithMany(a => a.Submissions)
                 .HasForeignKey(x => x.AssignmentId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(x => x.Grade)
                 .WithOne(g => g.Submission)
                 .HasForeignKey<Grade>(g => g.SubmissionId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // =========================
            // GRADE
            // =========================
            modelBuilder.Entity<Grade>(b =>
            {
                b.HasKey(x => x.Id);

                b.Property(x => x.Score)
                 .HasPrecision(5, 2);

                b.Property(x => x.Comment)
                 .HasMaxLength(1000);
            });

            // =========================
            // FILE RESOURCE
            // =========================
            modelBuilder.Entity<FileResource>(b =>
            {
                b.HasKey(x => x.Id);

                b.Property(x => x.FileName)
                    .IsRequired()
                    .HasMaxLength(260);

                b.Property(x => x.ContentType)
                    .IsRequired()
                    .HasMaxLength(200);

                b.Property(x => x.BlobPath)
                    .IsRequired()
                    .HasMaxLength(1000);

                b.HasIndex(x => new { x.OwnerId, x.SubmissionId, x.UploadedAt });
                b.HasIndex(x => new { x.ClassroomId, x.MaterialId });
                b.HasIndex(x => new { x.ClassroomId, x.AssignmentId });

                b.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(x => x.OwnerId)
                    .OnDelete(DeleteBehavior.NoAction);

                b.HasOne<Classroom>()
                    .WithMany()
                    .HasForeignKey(x => x.ClassroomId)
                    .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(x => x.Material)
                    .WithMany(m => m.Files)
                    .HasForeignKey(x => x.MaterialId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(x => x.Assignment)
                    .WithMany(a => a.Files)
                    .HasForeignKey(x => x.AssignmentId)
                    .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(x => x.Submission)
                    .WithMany(s => s.Files)
                    .HasForeignKey(x => x.SubmissionId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // =========================
            // CHANNEL
            // =========================
            modelBuilder.Entity<Channel>(b =>
            {
                b.HasKey(x => x.Id);

                b.Property(x => x.Title)
                 .IsRequired()
                 .HasMaxLength(200);

                b.Property(x => x.Type)
                 .HasConversion<int>()
                 .IsRequired();

                b.HasOne(x => x.Classroom)
                 .WithMany(c => c.Channels)
                 .HasForeignKey(x => x.ClassroomId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // =========================
            // CHANNEL MEMBER
            // =========================
            modelBuilder.Entity<ChannelMember>(b =>
            {
                b.HasKey(x => x.Id);

                b.HasIndex(x => new { x.ChannelId, x.UserId }).IsUnique();

                b.HasOne(x => x.Channel)
                 .WithMany(c => c.Members)
                 .HasForeignKey(x => x.ChannelId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne<ApplicationUser>()
                 .WithMany()
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.NoAction);
            });

            // =========================
            // MESSAGE
            // =========================
            modelBuilder.Entity<Message>(b =>
            {
                b.HasKey(x => x.Id);

                b.Property(x => x.Text)
                 .IsRequired();

                b.HasIndex(x => new { x.ChannelId, x.CreatedAt });

                b.HasOne(x => x.Channel)
                 .WithMany(c => c.Messages)
                 .HasForeignKey(x => x.ChannelId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne<ApplicationUser>()
                 .WithMany()
                 .HasForeignKey(x => x.AuthorId)
                 .OnDelete(DeleteBehavior.NoAction);
            });

            // =========================
            // NOTIFICATION
            // =========================
            modelBuilder.Entity<Notification>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAt });
            });
            // =========================
            // REFRESH TOKEN
            // =========================
            modelBuilder.Entity<RefreshToken>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasIndex(x => new { x.UserId, x.Token, x.IsRevoked });
            });
        }
    }
}
