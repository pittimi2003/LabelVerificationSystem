namespace LabelVerificationSystem.Infrastructure.Authorization;

public sealed class AuthorizationRuntimeOptions
{
    public const string SectionName = "Authorization";

    public bool UseRobustMatrix { get; set; } = true;
    public bool EnableLegacyFallback { get; set; } = true;
}
