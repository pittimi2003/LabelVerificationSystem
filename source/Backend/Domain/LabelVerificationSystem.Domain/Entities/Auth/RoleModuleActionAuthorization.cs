namespace LabelVerificationSystem.Domain.Entities.Auth;

public sealed class RoleModuleActionAuthorization
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public RoleCatalog Role { get; set; } = null!;
    public Guid ModuleActionId { get; set; }
    public ModuleActionCatalog ModuleAction { get; set; } = null!;
    public bool Authorized { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
