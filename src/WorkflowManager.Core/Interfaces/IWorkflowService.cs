using WorkflowManager.Core.Entities;
using WorkflowManager.Core.Enums;
using WorkflowManager.Core.ValueObjects;

namespace WorkflowManager.Core.Interfaces;

/// <summary>
/// Core workflow orchestration service
/// Coordinates between Elsa engine, state management, and event sourcing
/// </summary>
public interface IWorkflowService
{
    /// <summary>
    /// Creates a new workflow instance from a template
    /// Initializes both PostgreSQL index and MongoDB document
    /// Starts Elsa workflow execution
    /// </summary>
    Task<WorkflowInstance> CreateWorkflowAsync(
        Guid tenantId,
        Guid templateId,
        MarketRole marketRole,
        ITenantContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a workflow step
    /// Updates state, records event, advances Elsa workflow
    /// </summary>
    Task<StepExecutionResult> ExecuteStepAsync(
        Guid workflowInstanceId,
        string stepId,
        Dictionary<string, object> stepData,
        ITenantContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Pauses workflow execution
    /// Sets bookmark in Elsa, updates status
    /// </summary>
    Task PauseWorkflowAsync(
        Guid workflowInstanceId,
        string reason,
        ITenantContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes a paused workflow
    /// Resumes from Elsa bookmark
    /// </summary>
    Task ResumeWorkflowAsync(
        Guid workflowInstanceId,
        ITenantContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back workflow to a previous step
    /// Uses event sourcing to restore state
    /// Creates compensating Elsa activities
    /// </summary>
    Task RollbackWorkflowAsync(
        Guid workflowInstanceId,
        string targetStepId,
        string reason,
        ITenantContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates workflow is ready for submission
    /// Checks all required steps completed, runs validation rules
    /// </summary>
    Task<ValidationResult> ValidateWorkflowAsync(
        Guid workflowInstanceId,
        ITenantContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Submits workflow for final approval
    /// Transitions to AwaitingValidation or Submitted status
    /// </summary>
    Task SubmitWorkflowAsync(
        Guid workflowInstanceId,
        ITenantContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves a workflow (Market Ops action)
    /// Completes the workflow
    /// </summary>
    Task ApproveWorkflowAsync(
        Guid workflowInstanceId,
        string comments,
        ITenantContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rejects a workflow (Market Ops action)
    /// Returns to previous step or specific step
    /// </summary>
    Task RejectWorkflowAsync(
        Guid workflowInstanceId,
        string reason,
        ITenantContext context,
        string? returnToStepId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a workflow
    /// Stops Elsa execution, sets status to Cancelled
    /// </summary>
    Task CancelWorkflowAsync(
        Guid workflowInstanceId,
        string reason,
        ITenantContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves workflow instance with full state
    /// </summary>
    Task<WorkflowInstance?> GetWorkflowAsync(
        Guid workflowInstanceId,
        ITenantContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists workflows for current tenant with filtering
    /// </summary>
    Task<IReadOnlyList<WorkflowInstance>> ListWorkflowsAsync(
        ITenantContext context,
        WorkflowStatus? status = null,
        MarketRole? marketRole = null,
        int? limit = null,
        int? skip = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves workflow execution history (events)
    /// </summary>
    Task<IReadOnlyList<WorkflowEvent>> GetWorkflowHistoryAsync(
        Guid workflowInstanceId,
        ITenantContext context,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of step execution
/// </summary>
public sealed class StepExecutionResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public string? NextStepId { get; init; }
    public WorkflowStatus WorkflowStatus { get; init; }
    public Dictionary<string, object> OutputData { get; init; } = new();
}

/// <summary>
/// Result of workflow validation
/// </summary>
public sealed class ValidationResult
{
    public bool Valid { get; init; }
    public List<ValidationError> Errors { get; init; } = new();
}
