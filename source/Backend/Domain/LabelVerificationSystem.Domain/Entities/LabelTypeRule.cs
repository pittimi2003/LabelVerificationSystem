namespace LabelVerificationSystem.Domain.Entities;

public sealed class LabelTypeRule
{
    public Guid Id { get; set; }
    public Guid LabelTypeId { get; set; }
    public string ColumnName { get; set; } = string.Empty;
    public string ExpectedValue { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public LabelType LabelType { get; set; } = null!;
}
