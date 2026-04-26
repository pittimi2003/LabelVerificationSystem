namespace LabelVerificationSystem.Domain.Entities;

public sealed class Part
{
    public Guid Id { get; set; }
    public string PartNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string MinghuaDescription { get; set; } = string.Empty;
    public int? Caducidad { get; set; }
    public string Cco { get; set; } = string.Empty;
    public bool? CertificationEac { get; set; }
    public int FirstFourNumbers { get; set; }
    public Guid? CreatedByExcelUploadId { get; set; }
    public ExcelUpload? CreatedByExcelUpload { get; set; }
    public Guid? LabelTypeId { get; set; }
    public LabelType? LabelType { get; set; }
    public string LabelTypeName { get; set; } = "Por asignar";
    public DateTime CreatedAtUtc { get; set; }
}
