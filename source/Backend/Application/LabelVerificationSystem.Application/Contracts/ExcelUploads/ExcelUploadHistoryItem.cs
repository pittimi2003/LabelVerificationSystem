namespace LabelVerificationSystem.Application.Contracts.ExcelUploads;

public sealed record ExcelUploadHistoryItem(
    Guid UploadId,
    string OriginalFileName,
    DateTime UploadedAtUtc,
    string Status,
    int TotalRows,
    int InsertedRows,
    int RejectedRows);
