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
        var partValues = GetNormalizedPartValues(part);

        var activeTypes = await _dbContext.Set<LabelType>()
            .AsNoTracking()
            .Include(x => x.Rules)
            .Where(x => x.IsActive)
            .ToListAsync(cancellationToken);

        var matches = activeTypes
            .Where(x => x.Rules.Count > 0)
            .Where(x => RuleMatches(partValues, x.Rules))
            .OrderByDescending(x => x.Rules.Count)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var match = matches.FirstOrDefault();
        if (match is not null)
        {
            return (match.Id, match.Name);
        }

        var fallback = activeTypes.FirstOrDefault(x => x.Name == LabelTypeAdministrationService.UnassignedName)
                       ?? await _dbContext.Set<LabelType>().AsNoTracking().FirstAsync(x => x.Id == LabelTypeAdministrationService.UnassignedId, cancellationToken);

        return (fallback.Id, fallback.Name);
    }

    private static bool RuleMatches(IReadOnlyDictionary<string, string> partValues, IEnumerable<LabelTypeRule> rules)
    {
        foreach (var rule in rules)
        {
            if (!partValues.TryGetValue(rule.ColumnName, out var partValue))
            {
                return false;
            }

            var expected = NormalizeValue(rule.ExpectedValue);
            if (!string.Equals(partValue, expected, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private static IReadOnlyDictionary<string, string> GetNormalizedPartValues(Part part)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["PartNumber"] = NormalizeValue(part.PartNumber),
            ["Model"] = NormalizeValue(part.Model),
            ["MinghuaDescription"] = NormalizeValue(part.MinghuaDescription),
            ["Cco"] = NormalizeValue(part.Cco),
            ["FirstFourNumbers"] = part.FirstFourNumbers.ToString()
        };

        if (part.Caducidad.HasValue)
        {
            values["Caducidad"] = part.Caducidad.Value.ToString();
        }

        if (part.CertificationEac.HasValue)
        {
            values["CertificationEac"] = part.CertificationEac.Value ? "YES" : "NO";
        }

        return values;
    }

    private static string NormalizeValue(string? value) => (value ?? string.Empty).Trim();
}
