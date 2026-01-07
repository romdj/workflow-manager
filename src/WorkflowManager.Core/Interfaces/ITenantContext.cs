namespace WorkflowManager.Core.Interfaces;

/// <summary>
/// Provides context about the current tenant and user for multi-tenancy isolation
/// Implemented by middleware/authentication layer
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Current tenant ID (null for MarketOps users who can access all tenants)
    /// </summary>
    Guid? TenantId { get; }

    /// <summary>
    /// Current authenticated user ID
    /// </summary>
    Guid UserId { get; }

    /// <summary>
    /// Current user's role
    /// </summary>
    Enums.UserRole UserRole { get; }

    /// <summary>
    /// Checks if current user has access to specified tenant
    /// </summary>
    bool HasAccessToTenant(Guid tenantId);

    /// <summary>
    /// Ensures current user has access to specified tenant, throws if not
    /// </summary>
    void EnsureAccessToTenant(Guid tenantId);
}
