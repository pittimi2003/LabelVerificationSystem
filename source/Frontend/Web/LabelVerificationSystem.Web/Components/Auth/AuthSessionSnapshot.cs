namespace LabelVerificationSystem.Web.Components.Auth;

public sealed class AuthSessionSnapshot
{
    public bool IsAuthenticated { get; set; }
    public string AuthenticationMode { get; set; } = "Anonymous";
    public string? AccessToken { get; set; }
    public DateTime? AccessTokenExpiresAtUtc { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAtUtc { get; set; }
    public AuthUserDto? User { get; set; }

    public bool IsUserMode => string.Equals(AuthenticationMode, "User", StringComparison.OrdinalIgnoreCase);
    public bool IsBypassMode => string.Equals(AuthenticationMode, "Bypass", StringComparison.OrdinalIgnoreCase);

    public static AuthSessionSnapshot Anonymous() => new();
}
