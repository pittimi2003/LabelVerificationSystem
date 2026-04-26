using LabelVerificationSystem.Application.Contracts.LabelTypes;

namespace LabelVerificationSystem.Application.Interfaces.LabelTypes;

public interface ILabelTypeAdministrationService
{
    Task<LabelTypeListResponse> ListAsync(LabelTypeListQuery query, CancellationToken cancellationToken);
    Task<LabelTypeDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<LabelTypeDetailDto> CreateAsync(CreateLabelTypeRequest request, CancellationToken cancellationToken);
    Task<LabelTypeDetailDto> UpdateAsync(Guid id, UpdateLabelTypeRequest request, CancellationToken cancellationToken);
    Task<LabelTypeDetailDto> SetActivationAsync(Guid id, LabelTypeActivationRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<string>> GetAvailableColumnsAsync(CancellationToken cancellationToken);
}
