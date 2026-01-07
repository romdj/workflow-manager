using WorkflowManager.Core.Entities;

namespace WorkflowManager.Core.Interfaces;

/// <summary>
/// Event sourcing store for workflow events
/// Stores immutable events in MongoDB for audit trail and replay capabilities
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Appends a new event to the event store
    /// Events are immutable and append-only
    /// </summary>
    Task AppendAsync(WorkflowEvent workflowEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Appends multiple events atomically
    /// </summary>
    Task AppendManyAsync(IEnumerable<WorkflowEvent> events, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all events for a specific workflow instance
    /// Ordered by OccurredAt ascending
    /// </summary>
    Task<IReadOnlyList<WorkflowEvent>> GetEventsAsync(
        Guid workflowInstanceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves events for a workflow instance within a date range
    /// </summary>
    Task<IReadOnlyList<WorkflowEvent>> GetEventsAsync(
        Guid workflowInstanceId,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves events for a tenant (for audit reports)
    /// </summary>
    Task<IReadOnlyList<WorkflowEvent>> GetEventsByTenantAsync(
        Guid tenantId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int? limit = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Replays events to rebuild workflow state up to a specific point in time
    /// Used for point-in-time recovery and audit
    /// </summary>
    Task<ValueObjects.WorkflowState> ReplayEventsAsync(
        Guid workflowInstanceId,
        DateTime? pointInTime = null,
        CancellationToken cancellationToken = default);
}
