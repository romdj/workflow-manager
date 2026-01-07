using WorkflowManager.Core.Enums;
using WorkflowManager.Core.ValueObjects;

namespace WorkflowManager.Core.Entities;

/// <summary>
/// Represents an actual execution of a workflow template
/// PostgreSQL Index: For fast queries (tenant, status, etc.)
/// MongoDB Document: For complete workflow state
/// </summary>
public class WorkflowInstanceIndex
{
    // PostgreSQL table: workflow_instances_index
    // Purpose: Fast filtering by tenant, status, dates
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TemplateId { get; set; }
    public MarketRole MarketRole { get; set; }
    public WorkflowStatus Status { get; set; }
    public string? CurrentStepId { get; set; }
    public string ElsaInstanceId { get; set; } = string.Empty; // Correlation with Elsa
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public WorkflowTemplate Template { get; set; } = null!;
    public User Creator { get; set; } = null!;

    public WorkflowInstanceIndex() { }

    public WorkflowInstanceIndex(
        Guid id,
        Guid tenantId,
        Guid templateId,
        MarketRole marketRole,
        string elsaInstanceId,
        Guid createdBy)
    {
        Id = id;
        TenantId = tenantId;
        TemplateId = templateId;
        MarketRole = marketRole;
        ElsaInstanceId = elsaInstanceId ?? throw new ArgumentNullException(nameof(elsaInstanceId));
        Status = WorkflowStatus.Draft;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Complete workflow instance with full state
/// Stored in MongoDB for rich, nested state
/// </summary>
public class WorkflowInstance
{
    // MongoDB collection: workflow_instances
    // Purpose: Complete workflow state, rich queries on nested data
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty; // Denormalized
    public Guid TemplateId { get; set; }
    public MarketRole MarketRole { get; set; }
    public string ElsaInstanceId { get; set; } = string.Empty; // Correlation with Elsa
    public WorkflowStatus Status { get; set; }
    public WorkflowState State { get; set; } = new();
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public WorkflowInstance() { }

    public WorkflowInstance(
        Guid tenantId,
        string tenantName,
        Guid templateId,
        MarketRole marketRole,
        string elsaInstanceId,
        Guid createdBy)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        TenantName = tenantName ?? throw new ArgumentNullException(nameof(tenantName));
        TemplateId = templateId;
        MarketRole = marketRole;
        ElsaInstanceId = elsaInstanceId ?? throw new ArgumentNullException(nameof(elsaInstanceId));
        Status = WorkflowStatus.Draft;
        State = new WorkflowState
        {
            Metadata = new WorkflowMetadata
            {
                StartedAt = DateTime.UtcNow
            }
        };
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
