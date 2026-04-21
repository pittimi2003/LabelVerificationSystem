namespace LabelVerificationSystem.Domain.Entities.Auth;

public sealed class RoleCatalog
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public ICollection<SystemUserRole> UserRoles { get; set; } = new List<SystemUserRole>();
    public ICollection<RoleModuleAuthorization> ModuleAuthorizations { get; set; } = new List<RoleModuleAuthorization>();
    public ICollection<RoleModuleActionAuthorization> ModuleActionAuthorizations { get; set; } = new List<RoleModuleActionAuthorization>();
}
