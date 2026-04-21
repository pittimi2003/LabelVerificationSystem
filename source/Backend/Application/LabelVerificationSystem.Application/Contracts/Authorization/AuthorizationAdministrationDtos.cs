namespace LabelVerificationSystem.Application.Contracts.Authorization;

public sealed record AuthorizationRoleDto(Guid RoleId, string RoleCode, string RoleName);

public sealed record AuthorizationModuleActionDto(
    Guid ActionId,
    string ActionCode,
    string ActionName,
    bool Authorized,
    int DisplayOrder);

public sealed record AuthorizationModulePermissionDto(
    Guid ModuleId,
    string ModuleCode,
    string ModuleName,
    bool ModuleAuthorized,
    int DisplayOrder,
    IReadOnlyList<AuthorizationModuleActionDto> Actions);

public sealed record RoleAuthorizationMatrixDto(
    Guid RoleId,
    string RoleCode,
    string RoleName,
    IReadOnlyList<AuthorizationModulePermissionDto> Modules);

public sealed record UpdateRoleAuthorizationMatrixRequest(
    IReadOnlyList<UpdateRoleModuleAuthorizationDto> Modules);

public sealed record UpdateRoleModuleAuthorizationDto(
    Guid ModuleId,
    bool ModuleAuthorized,
    IReadOnlyList<UpdateRoleModuleActionAuthorizationDto> Actions);

public sealed record UpdateRoleModuleActionAuthorizationDto(
    Guid ActionId,
    bool Authorized);
