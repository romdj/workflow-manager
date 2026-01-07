using WorkflowManager.Core.Enums;
using WorkflowManager.Core.ValueObjects;

namespace WorkflowManager.Core.Entities;

/// <summary>
/// Defines the structure and configuration of a workflow
/// Templates are versioned and market role-specific
/// Stored in PostgreSQL for structured querying
/// </summary>
public class WorkflowTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MarketRole MarketRole { get; set; }
    public string ElsaWorkflowDefinitionId { get; set; } = string.Empty; // Elsa's workflow definition ID
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public WorkflowDefinition Definition { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public WorkflowTemplate() { }

    public WorkflowTemplate(
        string name,
        MarketRole marketRole,
        string elsaWorkflowDefinitionId,
        WorkflowDefinition definition)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        MarketRole = marketRole;
        ElsaWorkflowDefinitionId = elsaWorkflowDefinitionId ??
            throw new ArgumentNullException(nameof(elsaWorkflowDefinitionId));
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        Version = 1;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Defines the structure of a workflow (steps, transitions, rules)
/// Stored as JSONB in PostgreSQL
/// </summary>
public sealed class WorkflowDefinition
{
    public List<WorkflowStep> Steps { get; set; } = new();
    public Dictionary<string, List<string>> Transitions { get; set; } = new(); // stepId -> allowedNextSteps
    public List<ValidationRule>? ValidationRules { get; set; }

    public WorkflowDefinition() { }
}

/// <summary>
/// Defines a single step in the workflow
/// </summary>
public sealed class WorkflowStep
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public StepType Type { get; set; }
    public StepConfiguration Configuration { get; set; } = new();
    public bool Required { get; set; } = true;
    public int Order { get; set; }
    public List<string> AllowedTransitions { get; set; } = new(); // Next step IDs

    public WorkflowStep() { }

    public WorkflowStep(
        string id,
        string name,
        StepType type,
        StepConfiguration configuration,
        int order)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Type = type;
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        Order = order;
    }
}

/// <summary>
/// Validation rule for workflow completion
/// </summary>
public sealed class ValidationRule
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty; // "required-field", "custom-validator", etc.
    public Dictionary<string, object> Parameters { get; set; } = new();

    public ValidationRule() { }
}
