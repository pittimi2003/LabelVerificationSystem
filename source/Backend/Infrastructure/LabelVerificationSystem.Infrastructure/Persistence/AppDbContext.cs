using LabelVerificationSystem.Domain.Entities;
using LabelVerificationSystem.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace LabelVerificationSystem.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Part> Parts => Set<Part>();
    public DbSet<ExcelUpload> ExcelUploads => Set<ExcelUpload>();
    public DbSet<ExcelUploadRowResult> ExcelUploadRowResults => Set<ExcelUploadRowResult>();
    public DbSet<AuthSession> AuthSessions => Set<AuthSession>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<UserPasswordCredential> UserPasswordCredentials => Set<UserPasswordCredential>();
    public DbSet<SystemUser> SystemUsers => Set<SystemUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Part>(entity =>
        {
            entity.ToTable("Parts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PartNumber).IsRequired();
            entity.Property(x => x.Model).IsRequired();
            entity.Property(x => x.MinghuaDescription).IsRequired();
            entity.Property(x => x.Cco).IsRequired();
            entity.Property(x => x.FirstFourNumbers).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.HasIndex(x => x.PartNumber).IsUnique();

            entity.HasOne(x => x.CreatedByExcelUpload)
                .WithMany(x => x.CreatedParts)
                .HasForeignKey(x => x.CreatedByExcelUploadId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ExcelUpload>(entity =>
        {
            entity.ToTable("ExcelUploads");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OriginalFileName).IsRequired();
            entity.Property(x => x.StoredFilePath).IsRequired();
            entity.Property(x => x.UploadedAtUtc).IsRequired();
            entity.Property(x => x.Status).IsRequired();
            entity.Property(x => x.TotalRows).IsRequired();
            entity.Property(x => x.InsertedRows).IsRequired();
            entity.Property(x => x.RejectedRows).IsRequired();
        });

        modelBuilder.Entity<ExcelUploadRowResult>(entity =>
        {
            entity.ToTable("ExcelUploadRowResults");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RowNumber).IsRequired();
            entity.Property(x => x.PartNumber).IsRequired();
            entity.Property(x => x.Model).IsRequired();
            entity.Property(x => x.Status).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();

            entity.HasOne(x => x.ExcelUpload)
                .WithMany(x => x.RowResults)
                .HasForeignKey(x => x.ExcelUploadId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.ExcelUploadId, x.RowNumber });
        });

        modelBuilder.Entity<AuthSession>(entity =>
        {
            entity.ToTable("AuthSessions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SessionId).IsRequired();
            entity.Property(x => x.Username).IsRequired();
            entity.Property(x => x.DisplayName).IsRequired();
            entity.Property(x => x.AuthMode).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.HasIndex(x => x.SessionId).IsUnique();
            entity.HasIndex(x => new { x.UserId, x.RevokedAtUtc });
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TokenHash).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.ExpiresAtUtc).IsRequired();
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.HasIndex(x => new { x.SessionId, x.ExpiresAtUtc });
            entity.HasIndex(x => new { x.SessionId, x.UsedAtUtc, x.RevokedAtUtc });

            entity.HasOne(x => x.Session)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.ReplacedByToken)
                .WithMany()
                .HasForeignKey(x => x.ReplacedByTokenId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.ToTable("PasswordResetTokens");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserId).IsRequired();
            entity.Property(x => x.TokenHash).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.ExpiresAtUtc).IsRequired();
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.HasIndex(x => new { x.UserId, x.ExpiresAtUtc });
            entity.HasIndex(x => new { x.UserId, x.UsedAtUtc, x.RevokedAtUtc });
        });

        modelBuilder.Entity<UserPasswordCredential>(entity =>
        {
            entity.ToTable("UserPasswordCredentials");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserId).IsRequired();
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.UpdatedAtUtc).IsRequired();
            entity.HasIndex(x => x.UserId).IsUnique();
        });
    }
}
