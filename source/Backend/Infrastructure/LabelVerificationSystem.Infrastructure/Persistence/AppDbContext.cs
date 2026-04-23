using LabelVerificationSystem.Domain.Entities;
using LabelVerificationSystem.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LabelVerificationSystem.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    private static readonly ValueConverter<Guid, string> GuidToLowerStringConverter =
        new(
            value => value.ToString("D").ToLowerInvariant(),
            value => Guid.Parse(value));

    private static readonly ValueConverter<Guid?, string?> NullableGuidToLowerStringConverter =
        new(
            value => value.HasValue ? value.Value.ToString("D").ToLowerInvariant() : null,
            value => string.IsNullOrWhiteSpace(value) ? null : Guid.Parse(value));

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
    public DbSet<RoleCatalog> RoleCatalogs => Set<RoleCatalog>();
    public DbSet<ModuleCatalog> ModuleCatalogs => Set<ModuleCatalog>();
    public DbSet<ModuleActionCatalog> ModuleActionCatalogs => Set<ModuleActionCatalog>();
    public DbSet<RoleModuleAuthorization> RoleModuleAuthorizations => Set<RoleModuleAuthorization>();
    public DbSet<RoleModuleActionAuthorization> RoleModuleActionAuthorizations => Set<RoleModuleActionAuthorization>();
    public DbSet<SystemUserRole> SystemUserRoles => Set<SystemUserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ApplySqliteGuidConverters(modelBuilder);

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

        modelBuilder.Entity<SystemUser>(entity =>
        {
            entity.ToTable("SystemUsers");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserId).IsRequired();
            entity.Property(x => x.Username).IsRequired();
            entity.Property(x => x.DisplayName).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.UpdatedAtUtc).IsRequired();
            entity.HasIndex(x => x.UserId).IsUnique();
            entity.HasIndex(x => x.Username).IsUnique();
            entity.HasIndex(x => x.Email);
            entity.HasIndex(x => x.IsActive);
        });

        modelBuilder.Entity<RoleCatalog>(entity =>
        {
            entity.ToTable("RoleCatalog");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).IsRequired().UseCollation("NOCASE");
            entity.Property(x => x.Name).IsRequired();
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.UpdatedAtUtc).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<ModuleCatalog>(entity =>
        {
            entity.ToTable("ModuleCatalog");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).IsRequired().UseCollation("NOCASE");
            entity.Property(x => x.Name).IsRequired();
            entity.Property(x => x.DisplayOrder).IsRequired();
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.UpdatedAtUtc).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.DisplayOrder).IsUnique();
        });

        modelBuilder.Entity<ModuleActionCatalog>(entity =>
        {
            entity.ToTable("ModuleActionCatalog");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).IsRequired().UseCollation("NOCASE");
            entity.Property(x => x.Name).IsRequired();
            entity.Property(x => x.DisplayOrder).IsRequired();
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.UpdatedAtUtc).IsRequired();
            entity.HasIndex(x => new { x.ModuleId, x.Code }).IsUnique();
            entity.HasIndex(x => new { x.ModuleId, x.DisplayOrder }).IsUnique();

            entity.HasOne(x => x.Module)
                .WithMany(x => x.Actions)
                .HasForeignKey(x => x.ModuleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RoleModuleAuthorization>(entity =>
        {
            entity.ToTable("RoleModuleAuthorization");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Authorized).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.UpdatedAtUtc).IsRequired();
            entity.HasIndex(x => new { x.RoleId, x.ModuleId }).IsUnique();

            entity.HasOne(x => x.Role)
                .WithMany(x => x.ModuleAuthorizations)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Module)
                .WithMany(x => x.RoleAuthorizations)
                .HasForeignKey(x => x.ModuleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RoleModuleActionAuthorization>(entity =>
        {
            entity.ToTable("RoleModuleActionAuthorization");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Authorized).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.UpdatedAtUtc).IsRequired();
            entity.HasIndex(x => new { x.RoleId, x.ModuleActionId }).IsUnique();

            entity.HasOne(x => x.Role)
                .WithMany(x => x.ModuleActionAuthorizations)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ModuleAction)
                .WithMany(x => x.RoleAuthorizations)
                .HasForeignKey(x => x.ModuleActionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SystemUserRole>(entity =>
        {
            entity.ToTable("SystemUserRole");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.IsPrimary).IsRequired().HasDefaultValue(false);
            entity.Property(x => x.AssignedAtUtc).IsRequired();
            entity.HasIndex(x => new { x.SystemUserId, x.RoleId }).IsUnique();
            entity.HasIndex(x => new { x.SystemUserId, x.IsPrimary }).HasFilter("IsPrimary = 1").IsUnique();

            entity.HasOne(x => x.SystemUser)
                .WithMany(x => x.Roles)
                .HasForeignKey(x => x.SystemUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Role)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.AssignedByUser)
                .WithMany(x => x.AssignedRoles)
                .HasForeignKey(x => x.AssignedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ApplySqliteGuidConverters(ModelBuilder modelBuilder)
    {
        foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(x => x.GetProperties()))
        {
            if (property.ClrType == typeof(Guid))
            {
                property.SetValueConverter(GuidToLowerStringConverter);
                continue;
            }

            if (property.ClrType == typeof(Guid?))
            {
                property.SetValueConverter(NullableGuidToLowerStringConverter);
            }
        }
    }
}
