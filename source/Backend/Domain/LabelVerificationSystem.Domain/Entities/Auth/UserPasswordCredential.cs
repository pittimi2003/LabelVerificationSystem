namespace LabelVerificationSystem.Domain.Entities.Auth;

public sealed class UserPasswordCredential
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public string? UpdatedByIp { get; set; }
    public string? UpdatedByUserAgent { get; set; }
}
