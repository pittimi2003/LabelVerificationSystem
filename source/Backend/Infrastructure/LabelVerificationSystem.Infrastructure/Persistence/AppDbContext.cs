using Microsoft.EntityFrameworkCore;
using LabelVerificationSystem.Domain.Entities;

namespace LabelVerificationSystem.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Part> Parts => Set<Part>();
}
