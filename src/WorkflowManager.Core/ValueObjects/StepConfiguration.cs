using WorkflowManager.Core.Enums;

namespace WorkflowManager.Core.ValueObjects;

/// <summary>
/// Configuration for a workflow step
/// Contains type-specific settings for forms, approvals, API calls, etc.
/// </summary>
public sealed class StepConfiguration
{
    public StepType Type { get; init; }

    // Form step configuration
    public FormSchema? FormSchema { get; init; }

    // Approval step configuration
    public List<string>? Approvers { get; init; } // User IDs or roles
    public string? ApprovalTitle { get; init; }
    public string? ApprovalDescription { get; init; }

    // API call configuration
    public string? ApiUrl { get; init; }
    public string? HttpMethod { get; init; } // GET, POST, PUT, DELETE
    public Dictionary<string, string>? Headers { get; init; }
    public object? RequestBody { get; init; }

    // Notification configuration
    public string? NotificationTemplate { get; init; }
    public List<string>? Recipients { get; init; }

    // Validation configuration
    public string? ValidationSchema { get; init; } // Name of validation schema to use
    public Dictionary<string, object>? ValidationRules { get; init; }

    // General configuration
    public Dictionary<string, object>? Metadata { get; init; } // Extensibility for custom data

    public StepConfiguration() { }

    public StepConfiguration(StepType type)
    {
        Type = type;
    }

    /// <summary>
    /// Creates a form step configuration
    /// </summary>
    public static StepConfiguration CreateFormStep(FormSchema formSchema)
    {
        return new StepConfiguration
        {
            Type = StepType.Form,
            FormSchema = formSchema ?? throw new ArgumentNullException(nameof(formSchema))
        };
    }

    /// <summary>
    /// Creates an approval step configuration
    /// </summary>
    public static StepConfiguration CreateApprovalStep(
        string title,
        string description,
        List<string> approvers)
    {
        return new StepConfiguration
        {
            Type = StepType.Approval,
            ApprovalTitle = title ?? throw new ArgumentNullException(nameof(title)),
            ApprovalDescription = description,
            Approvers = approvers ?? throw new ArgumentNullException(nameof(approvers))
        };
    }

    /// <summary>
    /// Creates an API call step configuration
    /// </summary>
    public static StepConfiguration CreateApiCallStep(
        string url,
        string method = "POST",
        Dictionary<string, string>? headers = null,
        object? body = null)
    {
        return new StepConfiguration
        {
            Type = StepType.ApiCall,
            ApiUrl = url ?? throw new ArgumentNullException(nameof(url)),
            HttpMethod = method,
            Headers = headers,
            RequestBody = body
        };
    }
}
