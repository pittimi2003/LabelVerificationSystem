namespace LabelVerificationSystem.Domain.Entities.Auth;

public sealed class PasswordResetToken
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string TokenHash { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? UsedAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? RevocationReason { get; set; }
    public string? CreatedByIp { get; set; }
    public string? CreatedByUserAgent { get; set; }
}
