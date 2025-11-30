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

            // -------- Classroom
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

                // FK на власника класу (ApplicationUser) без каскадування
                b.HasOne<ApplicationUser>()
                 .WithMany()
                 .HasForeignKey(x => x.OwnerId)
                 .OnDelete(DeleteBehavior.NoAction);
            });

            // -------- ClassMember
            modelBuilder.Entity<ClassMember>(b =>
            {
                b.HasKey(x => x.Id);

                // Унікальність: один користувач не може двічі вступити в один клас
                b.HasIndex(x => new { x.ClassroomId, x.UserId }).IsUnique();

                // Enum → string (читабельно в БД)
                b.Property(x => x.RoleInClass)
                 .HasConversion<string>()
                 .HasMaxLength(20)
                 .IsRequired();

                // Члени класу видаляються при видаленні класу (каскад з Classroom)
                b.HasOne(x => x.Classroom)
                 .WithMany(c => c.Members)
                 .HasForeignKey(x => x.ClassroomId)
                 .OnDelete(DeleteBehavior.Cascade);

                // Посилання на користувача — без каскаду (щоб не було multiple cascade paths)
                b.HasOne<ApplicationUser>()
                 .WithMany()
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.NoAction);
            });

            // -------- Assignment
            modelBuilder.Entity<Assignment>(b =>
            {
                b.HasKey(x => x.Id);

                b.Property(x => x.Title)
                 .IsRequired()
                 .HasMaxLength(200);

                b.Property(x => x.Status)
                 .IsRequired()
                 .HasMaxLength(20); // Draft|Published|Closed

                b.HasIndex(x => new { x.ClassroomId, x.DueAt });

                // Хто створив — користувач, без каскаду
                b.HasOne<ApplicationUser>()
                 .WithMany()
                 .HasForeignKey(x => x.CreatedBy)
                 .OnDelete(DeleteBehavior.NoAction);
            });

            // -------- Submission
            modelBuilder.Entity<Submission>(b =>
            {
                b.HasKey(x => x.Id);

                // Один студент → одна подача на завдання (поки так)
                b.HasIndex(x => new { x.AssignmentId, x.StudentId }).IsUnique();

                // Enum (SubmissionStatus) як int — швидко/компактно
                b.Property(x => x.Status).HasConversion<int>();
            });

            // -------- Grade
            modelBuilder.Entity<Grade>(b =>
            {
                b.HasKey(x => x.Id);

                // Одна оцінка на Submission (оновлюється при переоцінці)
                b.HasIndex(x => x.SubmissionId).IsUnique();

                b.Property(x => x.Score)
                 .HasPrecision(18, 2);
            });

            // -------- Message
            modelBuilder.Entity<Message>(b =>
            {
                b.HasKey(x => x.Id);

                // Індекс для вивантаження стрічки
                b.HasIndex(x => new { x.ChannelId, x.CreatedAt });

                // Автор повідомлення — без каскаду
                b.HasOne<ApplicationUser>()
                 .WithMany()
                 .HasForeignKey(x => x.AuthorId)
                 .OnDelete(DeleteBehavior.NoAction);
            });

            // -------- Channel / ChannelMember / Meeting / MeetingParticipant
            modelBuilder.Entity<Channel>()
                        .Property(x => x.Type)
                        .HasConversion<int>();

            // Якщо є зв’язки з ApplicationUser — теж ставимо NoAction.

            // -------- Notification
            modelBuilder.Entity<Notification>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAt });
            });

            // --------- FileResource   
            modelBuilder.Entity<FileResource>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.FileName).IsRequired().HasMaxLength(260);
                b.Property(x => x.ContentType).IsRequired().HasMaxLength(200);
                b.Property(x => x.BlobPath).IsRequired().HasMaxLength(1000);

                b.HasIndex(x => new { x.OwnerId, x.SubmissionId, x.UploadedAt });
                b.HasIndex(x => new { x.ClassroomId, x.MaterialId });
            });
            // -------- RefreshToken
            modelBuilder.Entity<RefreshToken>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasIndex(x => new { x.UserId, x.Token, x.IsRevoked });
            });
        }
    }
}
