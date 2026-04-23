namespace LabelVerificationSystem.Web.Components.Roles;

public sealed record RoleCatalogListQueryDto(
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

public sealed record RoleCatalogListResponseDto(
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

public sealed record SetRoleActivationRequestDto(bool IsActive);

public sealed record ApiErrorResponseDto(string Error);
