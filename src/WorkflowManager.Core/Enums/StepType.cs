namespace WorkflowManager.Core.Enums;

/// <summary>
/// Type of workflow step defining its behavior
/// </summary>
public enum StepType
{
    /// <summary>
    /// Form input step - User fills out structured form
    /// </summary>
    Form,

    /// <summary>
    /// Approval step - Requires approval from designated approvers
    /// </summary>
    Approval,

    /// <summary>
    /// API call step - Calls external API (Kong, notification service, etc.)
    /// </summary>
    ApiCall,

    /// <summary>
    /// Notification step - Sends email/alert to users
    /// </summary>
    Notification,

    /// <summary>
    /// Validation step - Validates accumulated workflow data
    /// </summary>
    Validation,

    /// <summary>
    /// Decision step - Conditional branching based on workflow state
    /// </summary>
    Decision,

    /// <summary>
    /// Manual task - Free-form task for user to complete
    /// </summary>
    ManualTask
}
