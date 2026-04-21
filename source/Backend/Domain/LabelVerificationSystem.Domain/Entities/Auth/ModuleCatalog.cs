namespace LabelVerificationSystem.Domain.Entities.Auth;

public sealed class ModuleCatalog
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public ICollection<ModuleActionCatalog> Actions { get; set; } = new List<ModuleActionCatalog>();
    public ICollection<RoleModuleAuthorization> RoleAuthorizations { get; set; } = new List<RoleModuleAuthorization>();
}
