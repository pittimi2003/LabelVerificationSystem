namespace LabelVerificationSystem.Domain.Entities;

public sealed class ExcelUploadRowResult
{
    public Guid Id { get; set; }
    public Guid ExcelUploadId { get; set; }
    public ExcelUpload ExcelUpload { get; set; } = null!;
    public int RowNumber { get; set; }
    public string PartNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
