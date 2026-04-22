using System.Security.Claims;
using System.Text.Json;
using LabelVerificationSystem.Application.Interfaces.Authorization;
using LabelVerificationSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LabelVerificationSystem.Infrastructure.Authorization;

public sealed class AuthorizationMatrixService : IAuthorizationMatrixService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly AppDbContext _dbContext;
    private readonly AuthorizationRuntimeOptions _options;
    private readonly ILogger<AuthorizationMatrixService> _logger;

    public AuthorizationMatrixService(
        AppDbContext dbContext,
        IOptions<AuthorizationRuntimeOptions> options,
        ILogger<AuthorizationMatrixService> logger)
    {
        _dbContext = dbContext;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AuthorizationCheckResult> AuthorizeAsync(
        string? userId,
        string moduleCode,
        string? actionCode,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(moduleCode))
        {
            return new AuthorizationCheckResult(false, false, false, "module_code_required");
        }

        var normalizedUserId = string.IsNullOrWhiteSpace(userId) ? null : userId.Trim();
        var robustOnlyCutoverSubset = IsRobustOnlyCutoverSubset(normalizedUserId, moduleCode, actionCode);

        if (_options.UseRobustMatrix)
        {
            if (!string.IsNullOrWhiteSpace(normalizedUserId))
            {
                var robustResult = await TryAuthorizeWithRobustModelAsync(
                    normalizedUserId,
                    moduleCode,
                    actionCode,
                    allowLegacyRoleFallback: !robustOnlyCutoverSubset,
                    cancellationToken);
                if (robustResult is not null)
                {
                    return robustResult;
                }
            }
        }

        if (_options.EnableLegacyFallback && !robustOnlyCutoverSubset)
        {
            var allowedByLegacy = IsAllowedByLegacyClaims(principal, moduleCode, actionCode);
            return new AuthorizationCheckResult(
                allowedByLegacy,
                false,
                true,
                allowedByLegacy ? null : "legacy_fallback_denied");
        }

        return new AuthorizationCheckResult(false, _options.UseRobustMatrix, false, "authorization_not_resolved");
    }

    private async Task<AuthorizationCheckResult?> TryAuthorizeWithRobustModelAsync(
        string userId,
        string moduleCode,
        string? actionCode,
        bool allowLegacyRoleFallback,
        CancellationToken cancellationToken)
    {
        var normalizedModuleCode = moduleCode.Trim();
        var normalizedActionCode = actionCode?.Trim();

        var userSnapshot = await _dbContext.SystemUsers
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => new
            {
                x.Id,
                x.RolesJson
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (userSnapshot is null)
        {
            _logger.LogDebug("authorization.robust.user_not_found userId={UserId}", userId);
            return null;
        }

        var roleCodes = await _dbContext.SystemUserRoles
            .AsNoTracking()
            .Where(x => x.SystemUserId == userSnapshot.Id && x.Role.IsActive)
            .Select(x => x.Role.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (roleCodes.Count == 0 && allowLegacyRoleFallback && _options.EnableLegacyFallback)
        {
            roleCodes = DeserializeList(userSnapshot.RolesJson)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        if (roleCodes.Count == 0)
        {
            return new AuthorizationCheckResult(false, true, false, "no_user_roles");
        }

        var moduleAuthorizations = await _dbContext.RoleModuleAuthorizations
            .AsNoTracking()
            .Where(x => x.Module.IsActive
                        && x.Module.Code == normalizedModuleCode
                        && roleCodes.Contains(x.Role.Code))
            .Select(x => x.Authorized)
            .ToListAsync(cancellationToken);

        if (!moduleAuthorizations.Any(x => x))
        {
            return new AuthorizationCheckResult(false, true, false, "module_not_authorized");
        }

        if (string.IsNullOrWhiteSpace(normalizedActionCode))
        {
            return new AuthorizationCheckResult(true, true, false, null);
        }

        var actionAuthorizations = await _dbContext.RoleModuleActionAuthorizations
            .AsNoTracking()
            .Where(x => x.ModuleAction.IsActive
                        && x.ModuleAction.Module.IsActive
                        && x.ModuleAction.Module.Code == normalizedModuleCode
                        && x.ModuleAction.Code == normalizedActionCode
                        && roleCodes.Contains(x.Role.Code))
            .Select(x => x.Authorized)
            .ToListAsync(cancellationToken);

        if (!actionAuthorizations.Any(x => x))
        {
            return new AuthorizationCheckResult(false, true, false, "action_not_authorized");
        }

        return new AuthorizationCheckResult(true, true, false, null);
    }

    private bool IsRobustOnlyCutoverSubset(string? userId, string moduleCode, string? actionCode)
    {
        var cutover = _options.RobustOnlyCutover;
        if (!cutover.Enabled || string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        var userIncluded = cutover.UserIds.Any(x => string.Equals(x?.Trim(), userId, StringComparison.OrdinalIgnoreCase));
        if (!userIncluded)
        {
            return false;
        }

        var normalizedModule = moduleCode.Trim();
        var normalizedAction = actionCode?.Trim();

        return cutover.Scopes.Any(scope =>
        {
            if (string.IsNullOrWhiteSpace(scope))
            {
                return false;
            }

            var parts = scope.Split(':', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2)
            {
                return false;
            }

            if (!string.Equals(parts[0], normalizedModule, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return parts[1] == "*"
                   || string.Equals(parts[1], normalizedAction, StringComparison.OrdinalIgnoreCase);
        });
    }

    private static bool IsAllowedByLegacyClaims(ClaimsPrincipal principal, string moduleCode, string? actionCode)
    {
        var roleSet = principal.FindAll(ClaimTypes.Role)
            .Select(x => x.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var permissionSet = principal.FindAll("permission")
            .Select(x => x.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return moduleCode switch
        {
            "ExcelUploads" when string.Equals(actionCode, "View", StringComparison.OrdinalIgnoreCase) =>
                roleSet.Contains("Administrator")
                || permissionSet.Contains("excel.uploads.read")
                || permissionSet.Contains("excel.upload.create"),
            "ExcelUploads" when string.Equals(actionCode, "Upload", StringComparison.OrdinalIgnoreCase) =>
                roleSet.Contains("Administrator") || permissionSet.Contains("excel.upload.create"),
            "UsersAdministration" when string.Equals(actionCode, "View", StringComparison.OrdinalIgnoreCase) =>
                roleSet.Contains("Administrator") || permissionSet.Contains("users.read") || permissionSet.Contains("users.manage"),
            "UsersAdministration" when string.Equals(actionCode, "Create", StringComparison.OrdinalIgnoreCase) =>
                roleSet.Contains("Administrator") || permissionSet.Contains("users.manage"),
            "UsersAdministration" when string.Equals(actionCode, "Edit", StringComparison.OrdinalIgnoreCase) =>
                roleSet.Contains("Administrator") || permissionSet.Contains("users.manage"),
            "UsersAdministration" when string.Equals(actionCode, "ActivateDeactivate", StringComparison.OrdinalIgnoreCase) =>
                roleSet.Contains("Administrator") || permissionSet.Contains("users.manage"),
            "AuthorizationMatrixAdministration" when string.Equals(actionCode, "Manage", StringComparison.OrdinalIgnoreCase) =>
                roleSet.Contains("Administrator")
                || permissionSet.Contains("authorization.matrix.manage")
                || permissionSet.Contains("users.manage"),
            _ => false
        };
    }

    private static List<string> DeserializeList(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }
}
