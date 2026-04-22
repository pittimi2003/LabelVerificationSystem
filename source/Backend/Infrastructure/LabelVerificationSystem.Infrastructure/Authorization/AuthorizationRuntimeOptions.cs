namespace LabelVerificationSystem.Infrastructure.Authorization;

public sealed class AuthorizationRuntimeOptions
{
    public const string SectionName = "Authorization";

    public bool UseRobustMatrix { get; set; } = true;
    public bool EnableLegacyFallback { get; set; } = true;
    public RobustOnlyCutoverOptions RobustOnlyCutover { get; set; } = new();
}

public sealed class RobustOnlyCutoverOptions
{
    public bool Enabled { get; set; }
    public List<string> UserIds { get; set; } = [];
    public List<string> Scopes { get; set; } = [];
}
