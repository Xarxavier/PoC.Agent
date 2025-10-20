namespace SSO.Domain.Entities;

/// <summary>
/// User entity for SSO system
/// </summary>
public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
