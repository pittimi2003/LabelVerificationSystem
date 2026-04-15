using LabelVerificationSystem.Domain.Entities;
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
    }
}
