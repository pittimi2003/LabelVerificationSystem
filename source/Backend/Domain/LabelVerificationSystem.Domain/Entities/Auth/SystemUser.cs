namespace LabelVerificationSystem.Domain.Entities.Auth;

public sealed class SystemUser
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
    public string RolesJson { get; set; } = "[]";
    public string PermissionsJson { get; set; } = "[]";
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
