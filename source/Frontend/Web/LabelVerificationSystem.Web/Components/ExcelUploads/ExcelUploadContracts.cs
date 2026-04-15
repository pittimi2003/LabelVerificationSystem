namespace LabelVerificationSystem.Web.Components.ExcelUploads;

public sealed record ExcelUploadHistoryItemDto(
    Guid UploadId,
    string OriginalFileName,
    DateTime UploadedAtUtc,
    string Status,
    int TotalRows,
    int InsertedRows,
    int RejectedRows);

public sealed record ExcelUploadRowErrorDto(
    int RowNumber,
    string PartNumber,
    string Error);

public sealed record ExcelUploadResultDto(
    Guid UploadId,
    string FileName,
    int TotalRows,
    int InsertedRows,
    int RejectedRows,
    IReadOnlyList<ExcelUploadRowErrorDto> RowErrors);

public sealed record ExcelUploadDetailDto(
    Guid UploadId,
    string OriginalFileName,
    DateTime UploadedAtUtc,
    string Status,
    int TotalRows,
    int InsertedRows,
    int RejectedRows,
    IReadOnlyList<ExcelUploadRowDetailDto> Rows);

public sealed record ExcelUploadRowDetailDto(
    int RowNumber,
    string PartNumber,
    string Model,
    string Status,
    string? ErrorCode,
    string? ErrorMessage);

public sealed record ApiErrorResponseDto(string Error);
