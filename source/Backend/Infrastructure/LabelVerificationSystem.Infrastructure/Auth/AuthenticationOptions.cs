namespace LabelVerificationSystem.Infrastructure.Auth;

public sealed class AuthenticationOptions
{
    public const string SectionName = "Authentication";

    public JwtOptions Jwt { get; set; } = new();
    public RefreshTokenOptions RefreshToken { get; set; } = new();
    public PasswordResetOptions PasswordReset { get; set; } = new();
    public BypassOptions Bypass { get; set; } = new();
    public List<ConfiguredUser> Users { get; set; } = [];
}

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "LabelVerificationSystem.Api";
    public string Audience { get; set; } = "LabelVerificationSystem.Web";
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenTtlMinutes { get; set; } = 20;
    public int RefreshProactiveWindowMinutes { get; set; } = 3;
    public int ClockSkewSeconds { get; set; } = 60;
}

public sealed class RefreshTokenOptions
{
    public int TtlMinutes { get; set; } = 1440;
}

public sealed class PasswordResetOptions
{
    public int TokenTtlMinutes { get; set; } = 30;
    public bool RevokeAllSessionsOnPasswordReset { get; set; } = true;
}

public sealed class BypassOptions
{
    public bool Enabled { get; set; }
    public List<string> AllowedEnvironments { get; set; } = [];
    public string UserId { get; set; } = "bypass-system";
    public string Username { get; set; } = "system-bypass";
    public string DisplayName { get; set; } = "System Bypass";
    public string? Email { get; set; }
    public List<string> Roles { get; set; } = ["Administrator"];
    public List<string> Permissions { get; set; } = ["excel.upload.create"];
}

public sealed class ConfiguredUser
{
    public string UserId { get; set; } = Guid.NewGuid().ToString("N");
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
    public List<string> Roles { get; set; } = [];
    public List<string> Permissions { get; set; } = [];
}
