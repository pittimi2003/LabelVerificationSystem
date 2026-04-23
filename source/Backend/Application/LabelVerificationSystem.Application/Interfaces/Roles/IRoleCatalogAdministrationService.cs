using LabelVerificationSystem.Application.Contracts.Roles;

namespace LabelVerificationSystem.Application.Interfaces.Roles;

public interface IRoleCatalogAdministrationService
{
    Task<RoleCatalogListResponse> ListAsync(RoleCatalogListQuery query, CancellationToken cancellationToken);
    Task<RoleCatalogDetailDto> GetByCodeAsync(string roleCode, CancellationToken cancellationToken);
    Task<RoleCatalogDetailDto> SetActivationAsync(string roleCode, bool isActive, CancellationToken cancellationToken);
}
