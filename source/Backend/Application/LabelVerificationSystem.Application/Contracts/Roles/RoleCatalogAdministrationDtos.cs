namespace LabelVerificationSystem.Application.Contracts.Roles;

public sealed record RoleCatalogListQuery(
    string? Query,
    string? Code,
    string? Name,
    bool? IsActive,
    int Page,
    int PageSize);

public sealed record RoleCatalogListItemDto(
    Guid RoleId,
    string RoleCode,
    string RoleName,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record RoleCatalogListResponse(
    IReadOnlyList<RoleCatalogListItemDto> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public sealed record RoleCatalogDetailDto(
    Guid RoleId,
    string RoleCode,
    string RoleName,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
