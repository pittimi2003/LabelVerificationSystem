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
        var dbQuery = _dbContext.Set<LabelType>().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Query))
        {
            var q = query.Query.Trim().ToLowerInvariant();
            dbQuery = dbQuery.Where(x => x.Name.ToLower().Contains(q) || x.Columns.ToLower().Contains(q));
        }

        if (query.IsActive.HasValue) dbQuery = dbQuery.Where(x => x.IsActive == query.IsActive.Value);
        var totalItems = await dbQuery.CountAsync(cancellationToken);
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)query.PageSize);
        var items = await dbQuery.OrderBy(x => x.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(MapList())
            .ToListAsync(cancellationToken);
        return new LabelTypeListResponse(items, query.Page, query.PageSize, totalItems, totalPages);
    }

    public async Task<LabelTypeDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await _dbContext.Set<LabelType>().AsNoTracking().Where(x => x.Id == id).Select(MapDetail()).FirstOrDefaultAsync(cancellationToken);
        return item ?? throw new AuthUnauthorizedException("Tipo de etiqueta no encontrado.");
    }

    public async Task<LabelTypeDetailDto> CreateAsync(CreateLabelTypeRequest request, CancellationToken cancellationToken)
    {
        var name = NormalizeRequired(request.Name, "name");
        var columns = NormalizeColumns(request.Columns);
        await ValidateUniqueness(name, null, cancellationToken);
        var now = DateTime.UtcNow;
        var entity = new LabelType
        {
            Id = Guid.NewGuid(),
            Name = name,
            Columns = string.Join('|', columns),
            IsActive = true,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            CreatedByUserId = NormalizeActor(request.ActorUserId),
            CreatedByUserName = NormalizeActor(request.ActorUserName),
            UpdatedByUserId = NormalizeActor(request.ActorUserId),
            UpdatedByUserName = NormalizeActor(request.ActorUserName)
        };
        _dbContext.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapDetail(entity);
    }

    public async Task<LabelTypeDetailDto> UpdateAsync(Guid id, UpdateLabelTypeRequest request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Set<LabelType>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw new AuthUnauthorizedException("Tipo de etiqueta no encontrado.");
        var name = NormalizeRequired(request.Name, "name");
        var columns = NormalizeColumns(request.Columns);
        await ValidateUniqueness(name, id, cancellationToken);
        if (entity.Id == UnassignedId && !request.IsActive)
        {
            throw new AuthValidationException("No se puede desactivar el tipo por defecto 'Por asignar'.");
        }

        entity.Name = name;
        entity.Columns = string.Join('|', columns);
        entity.IsActive = request.IsActive;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        entity.UpdatedByUserId = NormalizeActor(request.ActorUserId);
        entity.UpdatedByUserName = NormalizeActor(request.ActorUserName);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapDetail(entity);
    }

    public async Task<LabelTypeDetailDto> SetActivationAsync(Guid id, LabelTypeActivationRequest request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Set<LabelType>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw new AuthUnauthorizedException("Tipo de etiqueta no encontrado.");
        if (entity.Id == UnassignedId && !request.IsActive)
            throw new AuthValidationException("No se puede desactivar el tipo por defecto 'Por asignar'.");
        entity.IsActive = request.IsActive;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        entity.UpdatedByUserId = NormalizeActor(request.ActorUserId);
        entity.UpdatedByUserName = NormalizeActor(request.ActorUserName);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapDetail(entity);
    }

    private async Task ValidateUniqueness(string name, Guid? currentId, CancellationToken ct)
    {
        var exists = await _dbContext.Set<LabelType>().AnyAsync(x => x.Name.ToLower() == name.ToLower() && (!currentId.HasValue || x.Id != currentId.Value), ct);
        if (exists) throw new AuthConflictException("name ya está en uso.");
    }

    private static string NormalizeRequired(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new AuthValidationException($"{field} es requerido.");
        return value.Trim();
    }

    private static string NormalizeActor(string? actor) => string.IsNullOrWhiteSpace(actor) ? "unknown" : actor.Trim();

    private static IReadOnlyList<string> NormalizeColumns(IReadOnlyList<string>? columns)
    {
        if (columns is null || columns.Count == 0) throw new AuthValidationException("columns es requerido.");
        var normalized = columns.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();
        if (normalized.Count == 0) throw new AuthValidationException("columns es requerido.");
        var duplicates = normalized.GroupBy(x => x, StringComparer.OrdinalIgnoreCase).FirstOrDefault(x => x.Count() > 1);
        if (duplicates is not null) throw new AuthValidationException($"Columna duplicada: {duplicates.Key}.");
        var unknown = normalized.FirstOrDefault(x => !LabelTypeAvailableColumns.Values.Contains(x, StringComparer.OrdinalIgnoreCase));
        if (unknown is not null) throw new AuthValidationException($"Columna no soportada: {unknown}.");
        return normalized;
    }

    private static System.Linq.Expressions.Expression<Func<LabelType, LabelTypeListItemDto>> MapList() => x => new LabelTypeListItemDto(x.Id, x.Name, x.Columns, x.IsActive, x.CreatedAtUtc, x.CreatedByUserId, x.CreatedByUserName, x.UpdatedAtUtc, x.UpdatedByUserId, x.UpdatedByUserName);
    private static System.Linq.Expressions.Expression<Func<LabelType, LabelTypeDetailDto>> MapDetail() => x => new LabelTypeDetailDto(x.Id, x.Name, x.Columns, x.IsActive, x.CreatedAtUtc, x.CreatedByUserId, x.CreatedByUserName, x.UpdatedAtUtc, x.UpdatedByUserId, x.UpdatedByUserName);
    private static LabelTypeDetailDto MapDetail(LabelType x) => new(x.Id, x.Name, x.Columns, x.IsActive, x.CreatedAtUtc, x.CreatedByUserId, x.CreatedByUserName, x.UpdatedAtUtc, x.UpdatedByUserId, x.UpdatedByUserName);
}
