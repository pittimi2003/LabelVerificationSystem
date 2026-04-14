using LabelVerificationSystem.Application.Contracts.ExcelUploads;

namespace LabelVerificationSystem.Application.Interfaces.ExcelUploads;

public interface IExcelUploadService
{
    Task<ExcelUploadResult> ProcessUploadAsync(Stream fileStream, string originalFileName, CancellationToken cancellationToken);
    Task<IReadOnlyList<ExcelUploadHistoryItem>> GetHistoryAsync(CancellationToken cancellationToken);
    Task<ExcelUploadHistoryItem?> GetHistoryItemByIdAsync(Guid uploadId, CancellationToken cancellationToken);
}
