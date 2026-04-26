using LabelVerificationSystem.Domain.Entities;

namespace LabelVerificationSystem.Application.Interfaces.LabelTypes;

public interface ILabelTypeResolver
{
    Task<(Guid? LabelTypeId, string LabelTypeName)> ResolveForPartAsync(Part part, CancellationToken cancellationToken);
}
