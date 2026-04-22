namespace LabelVerificationSystem.Web.Components.Users;

public sealed record UserListQueryDto(
    string? Query,
    string? UserId,
    string? Username,
    string? DisplayName,
    string? Email,
    string? Role,
    string? Permission,
    bool? IsActive,
    int Page,
    int PageSize);

public sealed record UserListItemDto(
    string UserId,
    string Username,
    string DisplayName,
    string? Email,
    bool IsActive,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record UserListResponseDto(
    IReadOnlyList<UserListItemDto> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public sealed record UserRoleCatalogItemDto(Guid RoleId, string RoleCode, string RoleName, bool IsActive);

public sealed record UserDetailDto(
    string UserId,
    string Username,
    string DisplayName,
    string? Email,
    bool IsActive,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record CreateUserRequestDto(
    string Username,
    string DisplayName,
    string? Email,
    string Password,
    IReadOnlyList<string>? Roles,
    IReadOnlyList<string>? Permissions,
    bool IsActive);

public sealed record UpdateUserRequestDto(
    string DisplayName,
    string? Email,
    IReadOnlyList<string>? Roles,
    IReadOnlyList<string>? Permissions,
    bool IsActive,
    string? NewPassword);

public sealed record SetUserActivationRequestDto(bool IsActive);

public sealed record ApiErrorResponseDto(string Error);
