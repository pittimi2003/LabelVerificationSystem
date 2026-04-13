namespace LabelVerificationSystem.Application.Contracts.ExcelUploads;

public sealed record ExcelUploadResult(
    Guid UploadId,
    string FileName,
    int TotalRows,
    int InsertedRows,
    int RejectedRows,
    IReadOnlyList<ExcelUploadResultRowError> RowErrors);
