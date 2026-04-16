namespace LabelVerificationSystem.Domain.Entities.Auth;

public sealed class AuthSession
{
    public Guid Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string AuthMode { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastActivityAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? RevocationReason { get; set; }
    public string? CreatedByIp { get; set; }
    public string? CreatedByUserAgent { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
