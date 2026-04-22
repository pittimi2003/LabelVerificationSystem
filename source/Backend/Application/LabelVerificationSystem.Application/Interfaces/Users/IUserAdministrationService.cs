using LabelVerificationSystem.Application.Contracts.Users;
using LabelVerificationSystem.Application.Interfaces.Auth;

namespace LabelVerificationSystem.Application.Interfaces.Users;

public interface IUserAdministrationService
{
    Task<IReadOnlyList<UserRoleCatalogItemDto>> ListRolesAsync(CancellationToken cancellationToken);
    Task<UserListResponse> ListAsync(UserListQuery query, CancellationToken cancellationToken);
    Task<UserDetailDto> GetByUserIdAsync(string userId, CancellationToken cancellationToken);
    Task<UserDetailDto> CreateAsync(CreateUserRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken);
    Task<UserDetailDto> UpdateAsync(string userId, UpdateUserRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken);
    Task<UserDetailDto> SetActivationAsync(string userId, bool isActive, CancellationToken cancellationToken);
}
