using LabelVerificationSystem.Application.Contracts.Authorization;

namespace LabelVerificationSystem.Application.Interfaces.Authorization;

public interface IAuthorizationAdministrationService
{
    Task<IReadOnlyList<AuthorizationRoleDto>> ListRolesAsync(CancellationToken cancellationToken);
    Task<RoleAuthorizationMatrixDto> GetRoleMatrixAsync(string roleCode, CancellationToken cancellationToken);
    Task<RoleAuthorizationMatrixDto> UpdateRoleMatrixAsync(string roleCode, UpdateRoleAuthorizationMatrixRequest request, CancellationToken cancellationToken);
}
