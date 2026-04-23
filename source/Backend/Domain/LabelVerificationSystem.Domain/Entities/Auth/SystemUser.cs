namespace LabelVerificationSystem.Domain.Entities.Auth;

public sealed class SystemUser
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public ICollection<SystemUserRole> Roles { get; set; } = new List<SystemUserRole>();
    public ICollection<SystemUserRole> AssignedRoles { get; set; } = new List<SystemUserRole>();
}
