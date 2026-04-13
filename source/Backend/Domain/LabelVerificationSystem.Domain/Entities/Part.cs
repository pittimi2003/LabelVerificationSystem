namespace LabelVerificationSystem.Domain.Entities;

public sealed class Part
{
    public Guid Id { get; set; }
    public string PartNumber { get; set; } = string.Empty;
    public string MinghuaDescription { get; set; } = string.Empty;
    public string Caducidad { get; set; } = string.Empty;
    public string Cco { get; set; } = string.Empty;
    public string CertificationEac { get; set; } = string.Empty;
    public string FirstFourNumbers { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
