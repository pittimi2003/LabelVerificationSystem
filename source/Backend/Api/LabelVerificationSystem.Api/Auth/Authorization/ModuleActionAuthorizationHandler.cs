using System.Security.Claims;
using LabelVerificationSystem.Application.Interfaces.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace LabelVerificationSystem.Api.Auth.Authorization;

public sealed class ModuleActionAuthorizationHandler : AuthorizationHandler<ModuleActionAuthorizationRequirement>
{
    private readonly IAuthorizationMatrixService _authorizationMatrixService;
    private readonly ILogger<ModuleActionAuthorizationHandler> _logger;

    public ModuleActionAuthorizationHandler(
        IAuthorizationMatrixService authorizationMatrixService,
        ILogger<ModuleActionAuthorizationHandler> logger)
    {
        _authorizationMatrixService = authorizationMatrixService;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ModuleActionAuthorizationRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub");

        var cancellationToken = (context.Resource as HttpContext)?.RequestAborted ?? CancellationToken.None;
        var result = await _authorizationMatrixService.AuthorizeAsync(
            userId,
            requirement.ModuleCode,
            requirement.ActionCode,
            context.User,
            cancellationToken);

        if (result.IsAuthorized)
        {
            context.Succeed(requirement);
            return;
        }

        _logger.LogDebug(
            "authorization.denied userId={UserId} module={Module} action={Action} robust={Robust} legacyFallback={LegacyFallback} reason={Reason}",
            userId,
            requirement.ModuleCode,
            requirement.ActionCode,
            result.UsedRobustModel,
            result.UsedLegacyFallback,
            result.DenyReason);
    }
}
