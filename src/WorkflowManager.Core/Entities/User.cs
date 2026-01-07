using WorkflowManager.Core.Enums;

namespace WorkflowManager.Core.Entities;

/// <summary>
/// Represents a user in the system
/// Users belong to a tenant (except MarketOps users)
/// </summary>
public class User
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; } // Null for MarketOps users
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Tenant? Tenant { get; set; }

    public User() { }

    public User(
        string email,
        string name,
        UserRole role,
        Guid? tenantId = null)
    {
        Id = Guid.NewGuid();
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Role = role;
        TenantId = tenantId;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // Validation: MarketOps must not have tenantId, others must
        if (role == UserRole.MarketOps && tenantId.HasValue)
        {
            throw new InvalidOperationException("MarketOps users cannot belong to a tenant");
        }

        if (role != UserRole.MarketOps && !tenantId.HasValue)
        {
            throw new InvalidOperationException("Non-MarketOps users must belong to a tenant");
        }
    }

    /// <summary>
    /// Checks if user has access to a specific tenant
    /// </summary>
    public bool HasAccessToTenant(Guid tenantId)
    {
        // MarketOps can access all tenants
        if (Role == UserRole.MarketOps)
        {
            return true;
        }

        // Other users can only access their own tenant
        return TenantId == tenantId;
    }
}
