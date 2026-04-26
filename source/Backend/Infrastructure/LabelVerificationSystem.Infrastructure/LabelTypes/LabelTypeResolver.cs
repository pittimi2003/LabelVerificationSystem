using LabelVerificationSystem.Application.Interfaces.LabelTypes;
using LabelVerificationSystem.Domain.Entities;
using LabelVerificationSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LabelVerificationSystem.Infrastructure.LabelTypes;

public sealed class LabelTypeResolver : ILabelTypeResolver
{
    private readonly AppDbContext _dbContext;

    public LabelTypeResolver(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(Guid? LabelTypeId, string LabelTypeName)> ResolveForPartAsync(Part part, CancellationToken cancellationToken)
    {
        var relevantColumns = GetRelevantColumns(part);
        var activeTypes = await _dbContext.Set<LabelType>()
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.Columns.Length)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var match = activeTypes.FirstOrDefault(x => ParseColumns(x.Columns).SetEquals(relevantColumns));
        if (match is not null)
        {
            return (match.Id, match.Name);
        }

        var fallback = activeTypes.FirstOrDefault(x => x.Name == LabelTypeAdministrationService.UnassignedName)
                       ?? await _dbContext.Set<LabelType>().AsNoTracking().FirstAsync(x => x.Id == LabelTypeAdministrationService.UnassignedId, cancellationToken);
        return (fallback.Id, fallback.Name);
    }

    private static HashSet<string> GetRelevantColumns(Part part)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(part.PartNumber)) set.Add("PartNumber");
        if (!string.IsNullOrWhiteSpace(part.Model)) set.Add("Model");
        if (!string.IsNullOrWhiteSpace(part.MinghuaDescription)) set.Add("MinghuaDescription");
        if (part.Caducidad.HasValue) set.Add("Caducidad");
        if (!string.IsNullOrWhiteSpace(part.Cco)) set.Add("Cco");
        if (part.CertificationEac.HasValue) set.Add("CertificationEac");
        set.Add("FirstFourNumbers");
        return set;
    }

    private static HashSet<string> ParseColumns(string columns)
    {
        if (string.IsNullOrWhiteSpace(columns)) return new(StringComparer.OrdinalIgnoreCase);
        return columns.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
