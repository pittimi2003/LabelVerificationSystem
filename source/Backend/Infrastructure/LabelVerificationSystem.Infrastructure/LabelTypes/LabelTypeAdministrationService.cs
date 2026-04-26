using System.Linq;
using LabelVerificationSystem.Application.Contracts.LabelTypes;
using LabelVerificationSystem.Application.Interfaces.Auth;
using LabelVerificationSystem.Application.Interfaces.LabelTypes;
using LabelVerificationSystem.Domain.Entities;
using LabelVerificationSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LabelVerificationSystem.Infrastructure.LabelTypes;

public sealed class LabelTypeAdministrationService : ILabelTypeAdministrationService
{
    public const string UnassignedName = "Por asignar";
    public static readonly Guid UnassignedId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly AppDbContext _dbContext;

    public LabelTypeAdministrationService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<IReadOnlyList<string>> GetAvailableColumnsAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<string>>(LabelTypeAvailableColumns.Values);

    public async Task<LabelTypeListResponse> ListAsync(LabelTypeListQuery query, CancellationToken cancellationToken)
    {
        if (query.Page <= 0 || query.PageSize is < 1 or > 100) throw new AuthValidationException("Parámetros de paginación inválidos.");

        var dbQuery = _dbContext.Set<LabelType>().AsNoTracking().Include(x => x.Rules).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Query))
        {
            var q = query.Query.Trim().ToLowerInvariant();
            dbQuery = dbQuery.Where(x =>
                x.Name.ToLower().Contains(q) ||
                x.Columns.ToLower().Contains(q) ||
                x.Rules.Any(r => r.ColumnName.ToLower().Contains(q) || r.ExpectedValue.ToLower().Contains(q)));
        }

        if (query.IsActive.HasValue) dbQuery = dbQuery.Where(x => x.IsActive == query.IsActive.Value);

        var totalItems = await dbQuery.CountAsync(cancellationToken);
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)query.PageSize);
        var items = await dbQuery
            .OrderBy(x => x.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new LabelTypeListResponse(items.Select(MapList).ToList(), query.Page, query.PageSize, totalItems, totalPages);
    }

    public async Task<LabelTypeDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await _dbContext.Set<LabelType>()
            .AsNoTracking()
            .Include(x => x.Rules)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return item is null ? throw new AuthUnauthorizedException("Tipo de etiqueta no encontrado.") : MapDetail(item);
    }

    public async Task<LabelTypeDetailDto> CreateAsync(CreateLabelTypeRequest request, CancellationToken cancellationToken)
    {
        var name = NormalizeRequired(request.Name, "name");
        var normalizedRules = NormalizeRules(request.Rules);

        await ValidateNameUniquenessAsync(name, null, cancellationToken);
        await ValidateRuleCombinationUniquenessAsync(normalizedRules, null, cancellationToken);

        var now = DateTime.UtcNow;
        var entity = new LabelType
        {
            Id = Guid.NewGuid(),
            Name = name,
            Columns = BuildColumnsProjection(normalizedRules),
            IsActive = true,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            CreatedByUserId = NormalizeActor(request.ActorUserId),
            CreatedByUserName = NormalizeActor(request.ActorUserName),
            UpdatedByUserId = NormalizeActor(request.ActorUserId),
            UpdatedByUserName = NormalizeActor(request.ActorUserName),
            Rules = normalizedRules.Select(rule => new LabelTypeRule
            {
                Id = Guid.NewGuid(),
                ColumnName = rule.ColumnName,
                ExpectedValue = rule.ExpectedValue,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            }).ToList()
        };

        _dbContext.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapDetail(entity);
    }

    public async Task<LabelTypeDetailDto> UpdateAsync(Guid id, UpdateLabelTypeRequest request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Set<LabelType>()
            .Include(x => x.Rules)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw new AuthUnauthorizedException("Tipo de etiqueta no encontrado.");

        var name = NormalizeRequired(request.Name, "name");
        var normalizedRules = NormalizeRules(request.Rules);

        await ValidateNameUniquenessAsync(name, id, cancellationToken);
        await ValidateRuleCombinationUniquenessAsync(normalizedRules, id, cancellationToken);

        if (entity.Id == UnassignedId && !request.IsActive)
        {
            throw new AuthValidationException("No se puede desactivar el tipo por defecto 'Por asignar'.");
        }

        var now = DateTime.UtcNow;
        entity.Name = name;
        entity.Columns = BuildColumnsProjection(normalizedRules);
        entity.IsActive = request.IsActive;
        entity.UpdatedAtUtc = now;
        entity.UpdatedByUserId = NormalizeActor(request.ActorUserId);
        entity.UpdatedByUserName = NormalizeActor(request.ActorUserName);

        _dbContext.RemoveRange(entity.Rules.ToList());
        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var rule in normalizedRules)
        {
            _dbContext.Add(new LabelTypeRule
            {
                Id = Guid.NewGuid(),
                LabelTypeId = entity.Id,
                ColumnName = rule.ColumnName,
                ExpectedValue = rule.ExpectedValue,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _dbContext.Entry(entity).Collection(x => x.Rules).LoadAsync(cancellationToken);
        return MapDetail(entity);
    }

    public async Task<LabelTypeDetailDto> SetActivationAsync(Guid id, LabelTypeActivationRequest request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Set<LabelType>().Include(x => x.Rules).FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new AuthUnauthorizedException("Tipo de etiqueta no encontrado.");

        if (entity.Id == UnassignedId && !request.IsActive)
            throw new AuthValidationException("No se puede desactivar el tipo por defecto 'Por asignar'.");

        entity.IsActive = request.IsActive;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        entity.UpdatedByUserId = NormalizeActor(request.ActorUserId);
        entity.UpdatedByUserName = NormalizeActor(request.ActorUserName);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapDetail(entity);
    }

    private async Task ValidateNameUniquenessAsync(string name, Guid? currentId, CancellationToken ct)
    {
        var exists = await _dbContext.Set<LabelType>()
            .AnyAsync(x => x.Name.ToLower() == name.ToLower() && (!currentId.HasValue || x.Id != currentId.Value), ct);

        if (exists) throw new AuthConflictException("name ya está en uso.");
    }

    private async Task ValidateRuleCombinationUniquenessAsync(IReadOnlyList<LabelTypeRuleDto> normalizedRules, Guid? currentId, CancellationToken ct)
    {
        var activeLabelTypes = await _dbContext.Set<LabelType>()
            .AsNoTracking()
            .Include(x => x.Rules)
            .Where(x => x.IsActive && (!currentId.HasValue || x.Id != currentId.Value))
            .ToListAsync(ct);

        var candidateKey = BuildRuleSetKey(normalizedRules);
        var duplicate = activeLabelTypes.FirstOrDefault(x => BuildRuleSetKey(x.Rules.Select(MapRule).ToList()) == candidateKey);
        if (duplicate is not null)
        {
            throw new AuthConflictException("Ya existe un tipo de etiqueta con la misma combinación de columnas y valores.");
        }
    }

    private static string NormalizeRequired(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new AuthValidationException($"{field} es requerido.");
        return value.Trim();
    }

    private static string NormalizeActor(string? actor) => string.IsNullOrWhiteSpace(actor) ? "unknown" : actor.Trim();

    private static IReadOnlyList<LabelTypeRuleDto> NormalizeRules(IReadOnlyList<LabelTypeRuleDto>? rules)
    {
        if (rules is null || rules.Count == 0) throw new AuthValidationException("rules es requerido.");

        var normalized = new List<LabelTypeRuleDto>(rules.Count);
        foreach (var rule in rules)
        {
            var columnName = NormalizeRequired(rule.ColumnName, "rules.columnName");
            if (!LabelTypeAvailableColumns.Values.Contains(columnName, StringComparer.OrdinalIgnoreCase))
            {
                throw new AuthValidationException($"Columna no soportada: {columnName}.");
            }

            if (string.IsNullOrWhiteSpace(rule.ExpectedValue))
            {
                throw new AuthValidationException($"El valor esperado para la columna {columnName} es requerido.");
            }

            normalized.Add(new LabelTypeRuleDto(columnName, rule.ExpectedValue.Trim()));
        }

        var duplicateColumn = normalized
            .GroupBy(x => x.ColumnName, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(x => x.Count() > 1);

        if (duplicateColumn is not null)
        {
            throw new AuthValidationException($"Columna duplicada: {duplicateColumn.Key}.");
        }

        return normalized;
    }

    private static string BuildColumnsProjection(IReadOnlyList<LabelTypeRuleDto> rules) =>
        string.Join('|', rules.Select(x => x.ColumnName).OrderBy(x => x, StringComparer.OrdinalIgnoreCase));

    private static string BuildRuleSetKey(IReadOnlyList<LabelTypeRuleDto> rules)
    {
        var normalized = rules
            .Select(x => $"{x.ColumnName.Trim().ToLowerInvariant()}={x.ExpectedValue.Trim().ToLowerInvariant()}")
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();

        return string.Join('|', normalized);
    }

    private static LabelTypeRuleDto MapRule(LabelTypeRule x) => new(x.ColumnName, x.ExpectedValue);

    private static LabelTypeListItemDto MapList(LabelType x) =>
        new(x.Id, x.Name, x.Columns, x.Rules.OrderBy(r => r.ColumnName).Select(MapRule).ToList(), x.IsActive, x.CreatedAtUtc, x.CreatedByUserId, x.CreatedByUserName, x.UpdatedAtUtc, x.UpdatedByUserId, x.UpdatedByUserName);

    private static LabelTypeDetailDto MapDetail(LabelType x) =>
        new(x.Id, x.Name, x.Columns, x.Rules.OrderBy(r => r.ColumnName).Select(MapRule).ToList(), x.IsActive, x.CreatedAtUtc, x.CreatedByUserId, x.CreatedByUserName, x.UpdatedAtUtc, x.UpdatedByUserId, x.UpdatedByUserName);
}
