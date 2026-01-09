using FluentAssertions;
using WorkflowManager.Core.ValueObjects;
using Xunit;

namespace WorkflowManager.Core.Tests.ValueObjects;

public class FormSchemaTests
{
    [Fact]
    public void FormSchema_ShouldBeInitializable_WithTitleAndFields()
    {
        // Arrange
        var title = "Company Information Form";
        var fields = new List<FormField>
        {
            new()
            {
                Name = "companyName",
                Type = "string",
                Required = true,
                Label = "Company Name"
            },
            new()
            {
                Name = "vatNumber",
                Type = "string",
                Required = true,
                Label = "VAT Number",
                Pattern = "^BE[0-9]{10}$",
                ErrorMessage = "Invalid Belgian VAT number format"
            }
        };

        // Act
        var schema = new FormSchema
        {
            Title = title,
            Fields = fields
        };

        // Assert
        schema.Title.Should().Be(title);
        schema.Fields.Should().HaveCount(2);
        schema.Fields.Should().BeEquivalentTo(fields);
    }

    [Fact]
    public void FormField_ShouldSupport_RequiredValidation()
    {
        // Arrange & Act
        var field = new FormField
        {
            Name = "email",
            Type = "email",
            Required = true,
            Label = "Email Address"
        };

        // Assert
        field.Required.Should().BeTrue();
        field.Type.Should().Be("email");
    }

    [Fact]
    public void FormField_ShouldSupport_PatternValidation()
    {
        // Arrange & Act
        var field = new FormField
        {
            Name = "phoneNumber",
            Type = "string",
            Required = true,
            Label = "Phone Number",
            Pattern = "^\\+32[0-9]{9}$",
            ErrorMessage = "Invalid Belgian phone number"
        };

        // Assert
        field.Pattern.Should().Be("^\\+32[0-9]{9}$");
        field.ErrorMessage.Should().Be("Invalid Belgian phone number");
    }

    [Fact]
    public void FormField_ShouldSupport_MinMaxLengthValidation()
    {
        // Arrange & Act
        var field = new FormField
        {
            Name = "description",
            Type = "textarea",
            Required = false,
            Label = "Description",
            MinLength = 10,
            MaxLength = 500
        };

        // Assert
        field.MinLength.Should().Be(10);
        field.MaxLength.Should().Be(500);
        field.Required.Should().BeFalse();
    }

    [Fact]
    public void FormField_ShouldSupport_OptionsForSelectFields()
    {
        // Arrange
        var options = new List<string> { "Option A", "Option B", "Option C" };

        // Act
        var field = new FormField
        {
            Name = "category",
            Type = "select",
            Required = true,
            Label = "Category",
            Options = options
        };

        // Assert
        field.Type.Should().Be("select");
        field.Options.Should().BeEquivalentTo(options);
    }

    [Fact]
    public void FormField_ShouldSupport_PlaceholderAndHelpText()
    {
        // Arrange & Act
        var field = new FormField
        {
            Name = "iban",
            Type = "string",
            Required = true,
            Label = "IBAN",
            Placeholder = "BE00 0000 0000 0000",
            HelpText = "Your company's Belgian IBAN number"
        };

        // Assert
        field.Placeholder.Should().Be("BE00 0000 0000 0000");
        field.HelpText.Should().Be("Your company's Belgian IBAN number");
    }

    [Fact]
    public void FormField_ShouldSupport_DefaultValue()
    {
        // Arrange & Act
        var field = new FormField
        {
            Name = "country",
            Type = "string",
            Required = true,
            Label = "Country",
            DefaultValue = "Belgium"
        };

        // Assert
        field.DefaultValue.Should().Be("Belgium");
    }

    [Fact]
    public void FormSchema_ShouldSupport_EmptyFieldsList()
    {
        // Arrange & Act
        var schema = new FormSchema
        {
            Title = "Empty Form"
        };

        // Assert
        schema.Fields.Should().BeEmpty();
    }

    [Fact]
    public void FormField_ShouldSupport_ComplexValidationScenario()
    {
        // Arrange & Act
        var field = new FormField
        {
            Name = "businessEmail",
            Type = "email",
            Required = true,
            Label = "Business Email",
            Pattern = "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$",
            ErrorMessage = "Please enter a valid business email address",
            Placeholder = "your.name@company.be",
            HelpText = "Use your company email address",
            MinLength = 5,
            MaxLength = 100
        };

        // Assert
        field.Name.Should().Be("businessEmail");
        field.Type.Should().Be("email");
        field.Required.Should().BeTrue();
        field.Pattern.Should().NotBeNullOrEmpty();
        field.ErrorMessage.Should().NotBeNullOrEmpty();
        field.Placeholder.Should().NotBeNullOrEmpty();
        field.HelpText.Should().NotBeNullOrEmpty();
        field.MinLength.Should().Be(5);
        field.MaxLength.Should().Be(100);
    }
}
