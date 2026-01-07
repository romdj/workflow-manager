using WorkflowManager.Core.Entities;
using WorkflowManager.Core.Enums;

namespace WorkflowManager.Core.Interfaces;

/// <summary>
/// Repository for workflow instance index stored in PostgreSQL
/// Provides fast queries with tenant isolation via Row-Level Security
/// </summary>
public interface IWorkflowIndexRepository
{
    /// <summary>
    /// Retrieves workflow index by ID
    /// </summary>
    Task<WorkflowInstanceIndex?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves workflow index by Elsa instance ID
    /// </summary>
    Task<WorkflowInstanceIndex?> GetByElsaInstanceIdAsync(
        string elsaInstanceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a new workflow index entry
    /// </summary>
    Task InsertAsync(WorkflowInstanceIndex index, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates workflow status in index
    /// </summary>
    Task UpdateStatusAsync(
        Guid id,
        WorkflowStatus status,
        string? currentStepId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates current step in index
    /// </summary>
    Task UpdateCurrentStepAsync(
        Guid id,
        string currentStepId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries workflow indexes by tenant with filtering and pagination
    /// Fast queries leveraging PostgreSQL indexes and RLS
    /// </summary>
    Task<IReadOnlyList<WorkflowInstanceIndex>> QueryByTenantAsync(
        Guid tenantId,
        WorkflowStatus? status = null,
        MarketRole? marketRole = null,
        Guid? templateId = null,
        DateTime? createdAfter = null,
        DateTime? createdBefore = null,
        int? limit = null,
        int? skip = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts workflows by tenant and status
    /// Used for dashboards and reporting
    /// </summary>
    Task<int> CountByTenantAsync(
        Guid tenantId,
        WorkflowStatus? status = null,
        MarketRole? marketRole = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries workflows created by specific user
    /// </summary>
    Task<IReadOnlyList<WorkflowInstanceIndex>> QueryByCreatorAsync(
        Guid createdBy,
        int? limit = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes workflow index entry
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
