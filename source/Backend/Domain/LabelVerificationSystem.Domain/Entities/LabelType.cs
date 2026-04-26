namespace LabelVerificationSystem.Domain.Entities;

public sealed class LabelType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Columns { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public string CreatedByUserName { get; set; } = string.Empty;
    public DateTime UpdatedAtUtc { get; set; }
    public string UpdatedByUserId { get; set; } = string.Empty;
    public string UpdatedByUserName { get; set; } = string.Empty;

    public ICollection<Part> Parts { get; set; } = new List<Part>();
}
