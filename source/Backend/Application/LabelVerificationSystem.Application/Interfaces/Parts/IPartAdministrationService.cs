using LabelVerificationSystem.Application.Contracts.Parts;

namespace LabelVerificationSystem.Application.Interfaces.Parts;

public interface IPartAdministrationService
{
    Task<PartListResponse> ListAsync(PartListQuery query, CancellationToken cancellationToken);
    Task<PartDetailDto> GetByIdAsync(Guid partId, CancellationToken cancellationToken);
    Task<PartDetailDto> CreateAsync(CreatePartRequest request, CancellationToken cancellationToken);
    Task<PartDetailDto> UpdateAsync(Guid partId, UpdatePartRequest request, CancellationToken cancellationToken);
}
