namespace LabelVerificationSystem.Domain.Entities.Auth;

public sealed class ModuleActionCatalog
{
    public Guid Id { get; set; }
    public Guid ModuleId { get; set; }
    public ModuleCatalog Module { get; set; } = null!;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public ICollection<RoleModuleActionAuthorization> RoleAuthorizations { get; set; } = new List<RoleModuleActionAuthorization>();
}
