using LabelVerificationSystem.Application.Contracts.Roles;
using LabelVerificationSystem.Application.Interfaces.Auth;
using LabelVerificationSystem.Application.Interfaces.Roles;
using LabelVerificationSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LabelVerificationSystem.Infrastructure.Roles;

public sealed class RoleCatalogAdministrationService : IRoleCatalogAdministrationService
{
    private readonly AppDbContext _dbContext;

    public RoleCatalogAdministrationService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RoleCatalogListResponse> ListAsync(RoleCatalogListQuery query, CancellationToken cancellationToken)
    {
        if (query.Page <= 0)
        {
            throw new AuthValidationException("page debe ser mayor o igual a 1.");
        }

        if (query.PageSize is < 1 or > 100)
        {
            throw new AuthValidationException("pageSize debe estar entre 1 y 100.");
        }

        var rolesQuery = _dbContext.RoleCatalogs.AsNoTracking();

        var normalizedQuery = NormalizeFilter(query.Query);
        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            rolesQuery = rolesQuery.Where(x =>
                x.Code.ToLower().Contains(normalizedQuery)
                || x.Name.ToLower().Contains(normalizedQuery));
        }

        var normalizedCode = NormalizeFilter(query.Code);
        if (!string.IsNullOrWhiteSpace(normalizedCode))
        {
            rolesQuery = rolesQuery.Where(x => x.Code.ToLower().Contains(normalizedCode));
        }

        var normalizedName = NormalizeFilter(query.Name);
        if (!string.IsNullOrWhiteSpace(normalizedName))
        {
            rolesQuery = rolesQuery.Where(x => x.Name.ToLower().Contains(normalizedName));
        }

        if (query.IsActive.HasValue)
        {
            rolesQuery = rolesQuery.Where(x => x.IsActive == query.IsActive.Value);
        }

        var totalItems = await rolesQuery.CountAsync(cancellationToken);
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)query.PageSize);

        var items = await rolesQuery
            .OrderBy(x => x.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(role => new RoleCatalogListItemDto(role.Id, role.Code, role.Name, role.IsActive, role.CreatedAtUtc, role.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return new RoleCatalogListResponse(items, query.Page, query.PageSize, totalItems, totalPages);
    }

    public async Task<RoleCatalogDetailDto> GetByCodeAsync(string roleCode, CancellationToken cancellationToken)
    {
        var normalizedRoleCode = ValidateRequiredTrimmed(roleCode, nameof(roleCode));

        var role = await _dbContext.RoleCatalogs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == normalizedRoleCode, cancellationToken);

        if (role is null)
        {
            throw new AuthUnauthorizedException("Rol no encontrado.");
        }

        return MapToDetail(role);
    }

    public async Task<RoleCatalogDetailDto> CreateAsync(CreateRoleCatalogRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalizedRoleCode = ValidateRoleCode(request.RoleCode);
        var normalizedName = ValidateRoleName(request.Name);

        var roleExists = await _dbContext.RoleCatalogs
            .AnyAsync(x => x.Code == normalizedRoleCode, cancellationToken);
        if (roleExists)
        {
            throw new AuthValidationException($"Ya existe un rol con RoleCode '{normalizedRoleCode}'.");
        }

        var utcNow = DateTime.UtcNow;
        var role = new Domain.Entities.Auth.RoleCatalog
        {
            Id = Guid.NewGuid(),
            Code = normalizedRoleCode,
            Name = normalizedName,
            IsActive = request.IsActive,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow
        };

        _dbContext.RoleCatalogs.Add(role);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDetail(role);
    }

    public async Task<RoleCatalogDetailDto> UpdateAsync(string roleCode, UpdateRoleCatalogRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalizedRoleCode = ValidateRequiredTrimmed(roleCode, nameof(roleCode));
        var normalizedName = ValidateRoleName(request.Name);

        var role = await _dbContext.RoleCatalogs.FirstOrDefaultAsync(x => x.Code == normalizedRoleCode, cancellationToken);
        if (role is null)
        {
            throw new AuthUnauthorizedException("Rol no encontrado.");
        }

        role.Name = normalizedName;
        role.IsActive = request.IsActive;
        role.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDetail(role);
    }

    public async Task<RoleCatalogDetailDto> SetActivationAsync(string roleCode, bool isActive, CancellationToken cancellationToken)
    {
        var normalizedRoleCode = ValidateRequiredTrimmed(roleCode, nameof(roleCode));

        var role = await _dbContext.RoleCatalogs.FirstOrDefaultAsync(x => x.Code == normalizedRoleCode, cancellationToken);
        if (role is null)
        {
            throw new AuthUnauthorizedException("Rol no encontrado.");
        }

        role.IsActive = isActive;
        role.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDetail(role);
    }


    private static RoleCatalogDetailDto MapToDetail(Domain.Entities.Auth.RoleCatalog role)
        => new(role.Id, role.Code, role.Name, role.IsActive, role.CreatedAtUtc, role.UpdatedAtUtc);

    private static string ValidateRequiredTrimmed(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new AuthValidationException($"{fieldName} es requerido.");
        }

        return value.Trim();
    }

    private static string ValidateRoleCode(string? value)
    {
        var normalized = ValidateRequiredTrimmed(value, "roleCode");
        if (normalized.Length is < 2 or > 64)
        {
            throw new AuthValidationException("roleCode debe tener entre 2 y 64 caracteres.");
        }

        return normalized;
    }

    private static string ValidateRoleName(string? value)
    {
        var normalized = ValidateRequiredTrimmed(value, "name");
        if (normalized.Length is < 2 or > 120)
        {
            throw new AuthValidationException("name debe tener entre 2 y 120 caracteres.");
        }

        return normalized;
    }

    private static string? NormalizeFilter(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
}
