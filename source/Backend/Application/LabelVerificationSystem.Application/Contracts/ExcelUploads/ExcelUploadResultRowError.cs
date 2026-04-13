namespace LabelVerificationSystem.Application.Contracts.ExcelUploads;

public sealed record ExcelUploadResultRowError(int RowNumber, string PartNumber, string Error);
