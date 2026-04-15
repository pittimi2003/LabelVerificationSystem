namespace LabelVerificationSystem.Domain.Entities;

public sealed class ExcelUpload
{
    public Guid Id { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFilePath { get; set; } = string.Empty;
    public DateTime UploadedAtUtc { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int InsertedRows { get; set; }
    public int RejectedRows { get; set; }
    public ICollection<Part> CreatedParts { get; set; } = new List<Part>();
    public ICollection<ExcelUploadRowResult> RowResults { get; set; } = new List<ExcelUploadRowResult>();
}
