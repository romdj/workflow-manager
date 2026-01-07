namespace WorkflowManager.Core.Enums;

/// <summary>
/// Current status of a workflow step
/// </summary>
public enum StepStatus
{
    /// <summary>
    /// Step not yet started
    /// </summary>
    Pending,

    /// <summary>
    /// Step currently being executed
    /// </summary>
    InProgress,

    /// <summary>
    /// Step successfully completed
    /// </summary>
    Completed,

    /// <summary>
    /// Step paused (waiting for user input)
    /// </summary>
    Paused,

    /// <summary>
    /// Step failed with errors
    /// </summary>
    Failed,

    /// <summary>
    /// Step skipped (conditional logic)
    /// </summary>
    Skipped
}
