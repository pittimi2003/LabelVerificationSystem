using System.Security.Claims;

namespace LabelVerificationSystem.Application.Interfaces.Authorization;

public interface IAuthorizationMatrixService
{
    Task<AuthorizationCheckResult> AuthorizeAsync(
        string? userId,
        string moduleCode,
        string? actionCode,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken);
}

public sealed record AuthorizationCheckResult(
    bool IsAuthorized,
    bool UsedRobustModel,
    bool UsedLegacyFallback,
    string? DenyReason);
