namespace LabelVerificationSystem.Domain.Entities.Auth;

public sealed class RoleModuleAuthorization
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public RoleCatalog Role { get; set; } = null!;
    public Guid ModuleId { get; set; }
    public ModuleCatalog Module { get; set; } = null!;
    public bool Authorized { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
