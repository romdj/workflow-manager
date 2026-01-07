using WorkflowManager.Core.Entities;
using WorkflowManager.Core.Enums;

namespace WorkflowManager.Core.Interfaces;

/// <summary>
/// Repository for complete workflow instance state stored in MongoDB
/// Handles rich, nested workflow state documents
/// </summary>
public interface IWorkflowInstanceRepository
{
    /// <summary>
    /// Retrieves a workflow instance by ID
    /// </summary>
    Task<WorkflowInstance?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves workflow instance by Elsa instance ID (for correlation)
    /// </summary>
    Task<WorkflowInstance?> GetByElsaInstanceIdAsync(
        string elsaInstanceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a new workflow instance
    /// </summary>
    Task InsertAsync(WorkflowInstance instance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing workflow instance
    /// Typically called after step execution or state changes
    /// </summary>
    Task UpdateAsync(WorkflowInstance instance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates only the workflow state (for performance)
    /// </summary>
    Task UpdateStateAsync(
        Guid id,
        ValueObjects.WorkflowState state,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates workflow status
    /// </summary>
    Task UpdateStatusAsync(
        Guid id,
        WorkflowStatus status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds workflow instances for a tenant with filtering
    /// Rich queries on nested MongoDB documents
    /// </summary>
    Task<IReadOnlyList<WorkflowInstance>> FindByTenantAsync(
        Guid tenantId,
        WorkflowStatus? status = null,
        MarketRole? marketRole = null,
        int? limit = null,
        int? skip = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds workflows where specific step has a status
    /// Example: Find all workflows with pending approvals
    /// </summary>
    Task<IReadOnlyList<WorkflowInstance>> FindByStepStatusAsync(
        Guid tenantId,
        StepStatus stepStatus,
        int? limit = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a workflow instance (soft delete recommended)
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
