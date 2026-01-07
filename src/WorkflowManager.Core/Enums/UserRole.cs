namespace WorkflowManager.Core.Enums;

/// <summary>
/// User roles defining access levels and permissions
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Elia Market Operations staff - Full access across all tenants
    /// </summary>
    MarketOps,

    /// <summary>
    /// Tenant administrator - Full access within their tenant
    /// </summary>
    TenantAdmin,

    /// <summary>
    /// Tenant operator - Can execute workflows within their tenant
    /// </summary>
    TenantOperator,

    /// <summary>
    /// Tenant viewer - Read-only access within their tenant
    /// </summary>
    TenantViewer,

    /// <summary>
    /// Compliance team - Can approve/reject workflows
    /// </summary>
    ComplianceReviewer
}
