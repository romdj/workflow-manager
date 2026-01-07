using WorkflowManager.Core.Enums;

namespace WorkflowManager.Core.Entities;

/// <summary>
/// Represents a market participant company in the system
/// Tenant = Company (not market role)
/// Multi-tenancy isolation at database level via RLS
/// </summary>
public class Tenant
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string VATNumber { get; set; } = string.Empty; // Belgian VAT: BE + 10 digits
    public string? LegalEntityId { get; set; } // Legal entity identifier
    public TenantStatus Status { get; set; } = TenantStatus.Onboarding;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<TenantMarketRole> MarketRoles { get; set; } = new List<TenantMarketRole>();
    public ICollection<User> Users { get; set; } = new List<User>();

    public Tenant() { }

    public Tenant(string companyName, string vatNumber)
    {
        Id = Guid.NewGuid();
        CompanyName = companyName ?? throw new ArgumentNullException(nameof(companyName));
        VATNumber = vatNumber ?? throw new ArgumentNullException(nameof(vatNumber));
        Status = TenantStatus.Onboarding;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Junction table for Tenant-MarketRole many-to-many relationship
/// A tenant can have multiple market roles (e.g., both BRP and BSP)
/// </summary>
public class TenantMarketRole
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public MarketRole MarketRole { get; set; }
    public TenantStatus Status { get; set; } = TenantStatus.Onboarding;
    public DateTime? OnboardedAt { get; set; }
    public string? ContractReference { get; set; }
    public Dictionary<string, object>? Metadata { get; set; } // Extensibility for role-specific data
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Tenant Tenant { get; set; } = null!;

    public TenantMarketRole() { }

    public TenantMarketRole(Guid tenantId, MarketRole marketRole)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        MarketRole = marketRole;
        Status = TenantStatus.Onboarding;
        CreatedAt = DateTime.UtcNow;
    }
}
