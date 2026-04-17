namespace LabelVerificationSystem.Application.Contracts.Users;

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

public sealed record UserListResponse(
    IReadOnlyList<UserListItemDto> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

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

public sealed record CreateUserRequest(
    string Username,
    string DisplayName,
    string? Email,
    string Password,
    IReadOnlyList<string>? Roles,
    IReadOnlyList<string>? Permissions,
    bool IsActive);

public sealed record UpdateUserRequest(
    string DisplayName,
    string? Email,
    IReadOnlyList<string>? Roles,
    IReadOnlyList<string>? Permissions,
    bool IsActive,
    string? NewPassword);

public sealed record SetUserActivationRequest(bool IsActive);
