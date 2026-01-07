namespace WorkflowManager.Core.Enums;

/// <summary>
/// Types of events that can occur in a workflow's lifecycle
/// Used for event sourcing and audit trail
/// </summary>
public enum WorkflowEventType
{
    // Workflow-level events
    WorkflowCreated,
    WorkflowStarted,
    WorkflowPaused,
    WorkflowResumed,
    WorkflowSubmitted,
    WorkflowCompleted,
    WorkflowFailed,
    WorkflowCancelled,
    WorkflowRolledBack,

    // Step-level events
    StepStarted,
    StepCompleted,
    StepFailed,
    StepValidated,
    StepPaused,
    StepResumed,
    StepSkipped,
    StepCompensated,

    // Approval events
    ApprovalRequested,
    ApprovalGranted,
    ApprovalRejected,

    // Data events
    DataUpdated,
    ValidationFailed,
    ValidationPassed,

    // Integration events
    ApiCallStarted,
    ApiCallCompleted,
    ApiCallFailed,
    NotificationSent,
    NotificationFailed
}
