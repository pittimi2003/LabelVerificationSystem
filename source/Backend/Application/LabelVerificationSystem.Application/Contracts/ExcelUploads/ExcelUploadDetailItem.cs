namespace LabelVerificationSystem.Application.Contracts.ExcelUploads;

public sealed record ExcelUploadDetailItem(
    Guid UploadId,
    string OriginalFileName,
    DateTime UploadedAtUtc,
    string Status,
    int TotalRows,
    int InsertedRows,
    int RejectedRows,
    IReadOnlyList<ExcelUploadRowResultItem> Rows);

public sealed record ExcelUploadRowResultItem(
    int RowNumber,
    string PartNumber,
    string Model,
    string Status,
    string? ErrorCode,
    string? ErrorMessage);
