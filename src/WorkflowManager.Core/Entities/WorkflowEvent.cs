using WorkflowManager.Core.Enums;

namespace WorkflowManager.Core.Entities;

/// <summary>
/// Represents a single event in a workflow's history
/// Stored in MongoDB for event sourcing and audit trail
/// Immutable - events are never updated, only appended
/// </summary>
public class WorkflowEvent
{
    // MongoDB collection: workflow_events
    // Purpose: Complete audit trail, event sourcing, point-in-time recovery
    public Guid EventId { get; set; }
    public Guid WorkflowInstanceId { get; set; }
    public Guid TenantId { get; set; }
    public WorkflowEventType EventType { get; set; }
    public string? StepId { get; set; } // Null for workflow-level events
    public Dictionary<string, object> EventData { get; set; } = new();
    public Guid PerformedBy { get; set; }
    public DateTime OccurredAt { get; set; }

    public WorkflowEvent() { }

    public WorkflowEvent(
        Guid workflowInstanceId,
        Guid tenantId,
        WorkflowEventType eventType,
        Guid performedBy,
        Dictionary<string, object>? eventData = null,
        string? stepId = null)
    {
        EventId = Guid.NewGuid();
        WorkflowInstanceId = workflowInstanceId;
        TenantId = tenantId;
        EventType = eventType;
        StepId = stepId;
        EventData = eventData ?? new Dictionary<string, object>();
        PerformedBy = performedBy;
        OccurredAt = DateTime.UtcNow;
    }
}
