using WorkflowManager.Core.Entities;
using WorkflowManager.Core.Enums;

namespace WorkflowManager.Core.Interfaces;

/// <summary>
/// Service for tenant (market participant) management operations
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Retrieves a tenant by ID
    /// </summary>
    Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a tenant by VAT number
    /// </summary>
    Task<Tenant?> GetByVATNumberAsync(string vatNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new tenant (market participant)
    /// </summary>
    Task<Tenant> CreateTenantAsync(
        string companyName,
        string vatNumber,
        string? legalEntityId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates tenant information
    /// </summary>
    Task UpdateTenantAsync(Tenant tenant, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a market role to a tenant
    /// Creates a new onboarding workflow automatically
    /// </summary>
    Task<TenantMarketRole> AddMarketRoleAsync(
        Guid tenantId,
        MarketRole marketRole,
        string? contractReference = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates market role status (e.g., from Onboarding to Active)
    /// </summary>
    Task UpdateMarketRoleStatusAsync(
        Guid tenantMarketRoleId,
        TenantStatus status,
        DateTime? onboardedAt = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all market roles for a tenant
    /// </summary>
    Task<IReadOnlyList<TenantMarketRole>> GetMarketRolesAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if tenant has a specific market role
    /// </summary>
    Task<bool> HasMarketRoleAsync(
        Guid tenantId,
        MarketRole marketRole,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all tenants with filtering
    /// </summary>
    Task<IReadOnlyList<Tenant>> ListTenantsAsync(
        TenantStatus? status = null,
        int? limit = null,
        int? skip = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a tenant
    /// </summary>
    Task DeactivateTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
