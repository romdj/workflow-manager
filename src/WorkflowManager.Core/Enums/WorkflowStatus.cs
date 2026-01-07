namespace WorkflowManager.Core.Enums;

/// <summary>
/// Current status of a workflow instance
/// </summary>
public enum WorkflowStatus
{
    /// <summary>
    /// Workflow created but not yet started
    /// </summary>
    Draft,

    /// <summary>
    /// Workflow is actively being executed
    /// </summary>
    InProgress,

    /// <summary>
    /// Workflow explicitly paused by user (waiting for external input)
    /// </summary>
    Paused,

    /// <summary>
    /// Workflow waiting for validation before submission
    /// </summary>
    AwaitingValidation,

    /// <summary>
    /// Workflow submitted for final approval
    /// </summary>
    Submitted,

    /// <summary>
    /// Workflow successfully completed
    /// </summary>
    Completed,

    /// <summary>
    /// Workflow failed with errors
    /// </summary>
    Failed,

    /// <summary>
    /// Workflow was rolled back to a previous step
    /// </summary>
    RolledBack,

    /// <summary>
    /// Workflow cancelled by user
    /// </summary>
    Cancelled
}
