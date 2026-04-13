using LabelVerificationSystem.Application.Contracts.ExcelUploads;

namespace LabelVerificationSystem.Application.Interfaces.ExcelUploads;

public interface IExcelUploadService
{
    Task<ExcelUploadResult> ProcessUploadAsync(Stream fileStream, string originalFileName, CancellationToken cancellationToken);
}
