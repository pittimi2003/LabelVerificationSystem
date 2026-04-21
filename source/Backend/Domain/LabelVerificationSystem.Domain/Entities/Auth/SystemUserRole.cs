namespace LabelVerificationSystem.Domain.Entities.Auth;

public sealed class SystemUserRole
{
    public Guid Id { get; set; }
    public Guid SystemUserId { get; set; }
    public SystemUser SystemUser { get; set; } = null!;
    public Guid RoleId { get; set; }
    public RoleCatalog Role { get; set; } = null!;
    public bool IsPrimary { get; set; }
    public DateTime AssignedAtUtc { get; set; }
    public Guid? AssignedByUserId { get; set; }
    public SystemUser? AssignedByUser { get; set; }
}
