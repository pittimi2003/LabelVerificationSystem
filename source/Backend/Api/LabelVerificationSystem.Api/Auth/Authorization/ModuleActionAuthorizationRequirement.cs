using Microsoft.AspNetCore.Authorization;

namespace LabelVerificationSystem.Api.Auth.Authorization;

public sealed class ModuleActionAuthorizationRequirement : IAuthorizationRequirement
{
    public ModuleActionAuthorizationRequirement(string moduleCode, string? actionCode)
    {
        ModuleCode = moduleCode;
        ActionCode = actionCode;
    }

    public string ModuleCode { get; }
    public string? ActionCode { get; }
}
