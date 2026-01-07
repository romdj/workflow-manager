using WorkflowManager.Core.Enums;

namespace WorkflowManager.Core.ValueObjects;

/// <summary>
/// Complete state of a workflow instance at a point in time
/// Stored in MongoDB for rich querying and event sourcing
/// </summary>
public sealed class WorkflowState
{
    public string? CurrentStepId { get; set; }
    public Dictionary<string, StepState> StepStates { get; set; } = new();
    public WorkflowMetadata Metadata { get; set; } = new();

    public WorkflowState() { }
}

/// <summary>
/// State of an individual step within a workflow
/// </summary>
public sealed class StepState
{
    public string StepId { get; set; } = string.Empty;
    public StepStatus Status { get; set; }
    public Dictionary<string, object> Data { get; set; } = new(); // Form data, API responses, etc.
    public List<ValidationError>? ValidationErrors { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? PausedAt { get; set; }
    public Guid? CompletedBy { get; set; } // User ID
    public string? ErrorMessage { get; set; }

    public StepState() { }

    public StepState(string stepId, StepStatus status)
    {
        StepId = stepId ?? throw new ArgumentNullException(nameof(stepId));
        Status = status;
    }
}

/// <summary>
/// Metadata about the workflow execution
/// </summary>
public sealed class WorkflowMetadata
{
    public DateTime StartedAt { get; set; }
    public DateTime? PausedAt { get; set; }
    public TimeSpan PausedDuration { get; set; }
    public int TotalSteps { get; set; }
    public int CompletedSteps { get; set; }
    public string? Priority { get; set; } // low, medium, high
    public DateTime? SLA { get; set; } // Service Level Agreement deadline
    public List<string>? Tags { get; set; } // For categorization
    public Dictionary<string, object>? CustomMetadata { get; set; } // Extensibility

    public WorkflowMetadata() { }
}

/// <summary>
/// Validation error for a field or step
/// </summary>
public sealed class ValidationError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Code { get; set; } // Error code for i18n

    public ValidationError() { }

    public ValidationError(string field, string message, string? code = null)
    {
        Field = field ?? throw new ArgumentNullException(nameof(field));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Code = code;
    }
}
