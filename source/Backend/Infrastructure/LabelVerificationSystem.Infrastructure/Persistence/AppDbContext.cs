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
            entity.Property(x => x.Caducidad).IsRequired();
            entity.Property(x => x.Cco).IsRequired();
            entity.Property(x => x.CertificationEac).IsRequired();
            entity.Property(x => x.FirstFourNumbers).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.HasIndex(x => x.PartNumber).IsUnique();
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
    }
}
