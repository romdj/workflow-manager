namespace WorkflowManager.Core.Enums;

/// <summary>
/// Status of a tenant (market participant company)
/// </summary>
public enum TenantStatus
{
    /// <summary>
    /// Tenant is active and can use the system
    /// </summary>
    Active,

    /// <summary>
    /// Tenant is inactive (temporarily disabled)
    /// </summary>
    Inactive,

    /// <summary>
    /// Tenant is suspended (compliance or contractual issues)
    /// </summary>
    Suspended,

    /// <summary>
    /// Tenant onboarding in progress
    /// </summary>
    Onboarding
}
