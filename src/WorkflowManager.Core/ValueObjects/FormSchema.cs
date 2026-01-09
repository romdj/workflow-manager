namespace WorkflowManager.Core.ValueObjects;

/// <summary>
/// Defines the structure and validation rules for a form step
/// </summary>
public sealed class FormSchema
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public List<FormField> Fields { get; init; } = new();

    public FormSchema() { }

    public FormSchema(string title, List<FormField> fields, string? description = null)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Fields = fields ?? throw new ArgumentNullException(nameof(fields));
        Description = description;
    }
}

/// <summary>
/// Represents a single field in a form
/// </summary>
public sealed class FormField
{
    public string Name { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Type { get; init; } = "string"; // string, number, date, select, file, etc.
    public bool Required { get; init; }
    public string? Pattern { get; init; } // Regex pattern for validation
    public string? ErrorMessage { get; init; } // Custom error message for validation
    public string? Placeholder { get; init; } // Placeholder text for the input
    public int? MinLength { get; init; }
    public int? MaxLength { get; init; }
    public int? MinItems { get; init; } // For arrays
    public int? MaxItems { get; init; }
    public object? DefaultValue { get; init; }
    public List<string>? Options { get; init; } // For select/radio fields
    public string? HelpText { get; init; }

    public FormField() { }

    public FormField(
        string name,
        string label,
        string type = "string",
        bool required = false)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Label = label ?? throw new ArgumentNullException(nameof(label));
        Type = type;
        Required = required;
    }
}
