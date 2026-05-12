using KyInfo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KyInfo.Infrastructure.Persistence;

/// <summary>
/// 基于 EF Core 的数据库上下文。
/// Infrastructure 层负责与 EF Core/数据库相关的实现细节。
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<School> Schools => Set<School>();
    public DbSet<Major> Majors => Set<Major>();
    public DbSet<ScoreLine> ScoreLines => Set<ScoreLine>();
    public DbSet<RecruitInfo> RecruitInfos => Set<RecruitInfo>();
    public DbSet<ExamScore> ExamScores => Set<ExamScore>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.UserName).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
        });

        // School
        modelBuilder.Entity<School>(entity =>
        {
            entity.HasIndex(s => s.Name);
            entity.Property(s => s.LevelTag).HasMaxLength(50);
            entity.Property(s => s.Type).HasMaxLength(50);
            entity.Property(s => s.Property).HasMaxLength(50);
        });

        // Major
        modelBuilder.Entity<Major>(entity =>
        {
            entity.HasIndex(m => m.Code);
            entity.Property(m => m.TuitionPerYear).HasPrecision(18, 2);

            entity.HasOne(m => m.School)
                  .WithMany(s => s.Majors)
                  .HasForeignKey(m => m.SchoolId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ScoreLine
        modelBuilder.Entity<ScoreLine>(entity =>
        {
            entity.HasIndex(x => new { x.Year, x.IsNational });
            entity.HasIndex(x => x.SchoolId);
            entity.HasIndex(x => x.MajorId);

            entity.Property(x => x.Note).HasMaxLength(2000);

            entity.HasOne(x => x.School)
                  .WithMany()
                  .HasForeignKey(x => x.SchoolId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.Major)
                  .WithMany()
                  .HasForeignKey(x => x.MajorId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // RecruitInfo
        modelBuilder.Entity<RecruitInfo>(entity =>
        {
            entity.HasIndex(x => new { x.Year, x.SchoolId, x.MajorId }).IsUnique();

            entity.Property(x => x.ExamSubjects).HasMaxLength(1000);
            entity.Property(x => x.ExtraRequirements).HasMaxLength(2000);
            entity.Property(x => x.SourceUrl).HasMaxLength(500);

            entity.HasOne(x => x.School)
                  .WithMany()
                  .HasForeignKey(x => x.SchoolId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.Major)
                  .WithMany()
                  .HasForeignKey(x => x.MajorId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // ExamScore
        modelBuilder.Entity<ExamScore>(entity =>
        {
            entity.HasIndex(x => new { x.UserId, x.Year, x.SchoolId, x.MajorId }).IsUnique();

            entity.HasOne(x => x.User)
                  .WithMany()
                  .HasForeignKey(x => x.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.School)
                  .WithMany()
                  .HasForeignKey(x => x.SchoolId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.Major)
                  .WithMany()
                  .HasForeignKey(x => x.MajorId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(x => x.CreatedAtUtc);
            entity.Property(x => x.ActorRole).HasMaxLength(32);
            entity.Property(x => x.Action).HasMaxLength(128);
            entity.Property(x => x.ResourceType).HasMaxLength(64);
            entity.Property(x => x.Summary).HasMaxLength(2000);
        });
    }
}

