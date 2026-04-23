using LabelVerificationSystem.Application.Contracts.Authorization;
using LabelVerificationSystem.Application.Interfaces.Auth;
using LabelVerificationSystem.Application.Interfaces.Authorization;
using LabelVerificationSystem.Domain.Entities.Auth;
using LabelVerificationSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LabelVerificationSystem.Infrastructure.Authorization;

public sealed class AuthorizationAdministrationService : IAuthorizationAdministrationService
{
    private readonly AppDbContext _dbContext;

    public AuthorizationAdministrationService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AuthorizationRoleDto>> ListRolesAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.RoleCatalogs
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new AuthorizationRoleDto(x.Id, x.Code, x.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<RoleAuthorizationMatrixDto> GetRoleMatrixAsync(string roleCode, CancellationToken cancellationToken)
    {
        var role = await ResolveRoleAsync(roleCode, cancellationToken);
        var modules = await BuildRoleMatrixModulesAsync(role.Id, cancellationToken);

        return new RoleAuthorizationMatrixDto(role.Id, role.Code, role.Name, modules);
    }

    public async Task<RoleAuthorizationMatrixDto> UpdateRoleMatrixAsync(
        string roleCode,
        UpdateRoleAuthorizationMatrixRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Modules is null || request.Modules.Count == 0)
        {
            throw new AuthValidationException("modules es obligatorio y debe incluir al menos un módulo.");
        }

        var role = await ResolveRoleAsync(roleCode, cancellationToken);

        var activeModules = await _dbContext.ModuleCatalogs
            .Where(x => x.IsActive)
            .Select(x => new { x.Id })
            .ToListAsync(cancellationToken);
        var activeModuleIds = activeModules.Select(x => x.Id).ToHashSet();

        var activeActions = await _dbContext.ModuleActionCatalogs
            .Where(x => x.IsActive && x.Module.IsActive)
            .Select(x => new { x.Id, x.ModuleId })
            .ToListAsync(cancellationToken);
        var activeActionIds = activeActions.Select(x => x.Id).ToHashSet();
        var actionModuleMap = activeActions.ToDictionary(x => x.Id, x => x.ModuleId);

        foreach (var moduleRequest in request.Modules)
        {
            if (!activeModuleIds.Contains(moduleRequest.ModuleId))
            {
                throw new AuthValidationException($"moduleId '{moduleRequest.ModuleId}' no existe o no está activo.");
            }

            foreach (var actionRequest in moduleRequest.Actions)
            {
                if (!activeActionIds.Contains(actionRequest.ActionId))
                {
                    throw new AuthValidationException($"actionId '{actionRequest.ActionId}' no existe o no está activo.");
                }

                if (actionModuleMap[actionRequest.ActionId] != moduleRequest.ModuleId)
                {
                    throw new AuthValidationException($"actionId '{actionRequest.ActionId}' no pertenece al moduleId '{moduleRequest.ModuleId}'.");
                }
            }
        }

        var requestedModules = request.Modules
            .GroupBy(x => x.ModuleId)
            .Select(x => x.Last())
            .ToList();

        var requestedModuleIds = requestedModules
            .Select(x => x.ModuleId)
            .ToHashSet();

        var requestedActions = requestedModules
            .SelectMany(m => m.Actions.Select(a => new { m.ModuleId, a.ActionId, a.Authorized }))
            .GroupBy(x => x.ActionId)
            .Select(x => x.Last())
            .ToList();

        var requestedActionIds = requestedActions
            .Select(x => x.ActionId)
            .ToHashSet();

        await _dbContext.RoleModuleActionAuthorizations
            .Where(x => x.RoleId == role.Id && requestedActionIds.Contains(x.ModuleActionId))
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.RoleModuleAuthorizations
            .Where(x => x.RoleId == role.Id && requestedModuleIds.Contains(x.ModuleId))
            .ExecuteDeleteAsync(cancellationToken);

        var now = DateTime.UtcNow;
        _dbContext.RoleModuleAuthorizations.AddRange(requestedModules.Select(moduleRequest => new RoleModuleAuthorization
        {
            Id = Guid.NewGuid(),
            RoleId = role.Id,
            ModuleId = moduleRequest.ModuleId,
            Authorized = moduleRequest.ModuleAuthorized,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        }));

        _dbContext.RoleModuleActionAuthorizations.AddRange(requestedActions.Select(actionRequest => new RoleModuleActionAuthorization
        {
            Id = Guid.NewGuid(),
            RoleId = role.Id,
            ModuleActionId = actionRequest.ActionId,
            Authorized = actionRequest.Authorized,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        }));

        await _dbContext.SaveChangesAsync(cancellationToken);
        var modules = await BuildRoleMatrixModulesAsync(role.Id, cancellationToken);

        return new RoleAuthorizationMatrixDto(role.Id, role.Code, role.Name, modules);
    }

    private async Task<RoleCatalog> ResolveRoleAsync(string roleCode, CancellationToken cancellationToken)
    {
        var normalizedRoleCode = roleCode?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedRoleCode))
        {
            throw new AuthValidationException("roleCode es obligatorio.");
        }

        var role = await _dbContext.RoleCatalogs
            .FirstOrDefaultAsync(x => x.IsActive && x.Code == normalizedRoleCode, cancellationToken);

        if (role is null)
        {
            throw new AuthUnauthorizedException($"Rol '{normalizedRoleCode}' no encontrado.");
        }

        return role;
    }

    private async Task<IReadOnlyList<AuthorizationModulePermissionDto>> BuildRoleMatrixModulesAsync(
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var modules = await _dbContext.ModuleCatalogs
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Name,
                x.DisplayOrder,
                ModuleAuthorized = _dbContext.RoleModuleAuthorizations
                    .Where(a => a.RoleId == roleId && a.ModuleId == x.Id)
                    .Select(a => (bool?)a.Authorized)
                    .FirstOrDefault(),
                Actions = x.Actions
                    .Where(a => a.IsActive)
                    .OrderBy(a => a.DisplayOrder)
                    .ThenBy(a => a.Name)
                    .Select(a => new
                    {
                        a.Id,
                        a.Code,
                        a.Name,
                        a.DisplayOrder,
                        Authorized = _dbContext.RoleModuleActionAuthorizations
                            .Where(ra => ra.RoleId == roleId && ra.ModuleActionId == a.Id)
                            .Select(ra => (bool?)ra.Authorized)
                            .FirstOrDefault()
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return modules
            .Select(module => new AuthorizationModulePermissionDto(
                module.Id,
                module.Code,
                module.Name,
                module.ModuleAuthorized ?? false,
                module.DisplayOrder,
                module.Actions.Select(action => new AuthorizationModuleActionDto(
                    action.Id,
                    action.Code,
                    action.Name,
                    action.Authorized ?? false,
                    action.DisplayOrder)).ToList()))
            .ToList();
    }
}
