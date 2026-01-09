using FluentAssertions;
using WorkflowManager.Core.Enums;
using WorkflowManager.Core.ValueObjects;
using Xunit;

namespace WorkflowManager.Core.Tests.ValueObjects;

public class StepConfigurationTests
{
    [Fact]
    public void CreateFormStep_ShouldCreateConfigurationWithFormSchema()
    {
        // Arrange
        var formSchema = new FormSchema
        {
            Title = "Company Information",
            Fields = new List<FormField>
            {
                new()
                {
                    Name = "companyName",
                    Type = "string",
                    Required = true,
                    Label = "Company Name"
                }
            }
        };

        // Act
        var config = StepConfiguration.CreateFormStep(formSchema);

        // Assert
        config.Type.Should().Be(StepType.Form);
        config.FormSchema.Should().Be(formSchema);
        config.Approvers.Should().BeNull();
        config.ApiUrl.Should().BeNull();
    }

    [Fact]
    public void CreateApprovalStep_ShouldCreateConfigurationWithApprovers()
    {
        // Arrange
        var approvers = new List<string> { "approver1@example.com", "approver2@example.com" };
        var title = "Compliance Approval";
        var description = "Approval required for compliance";

        // Act
        var config = StepConfiguration.CreateApprovalStep(title, description, approvers);

        // Assert
        config.Type.Should().Be(StepType.Approval);
        config.Approvers.Should().BeEquivalentTo(approvers);
        config.ApprovalTitle.Should().Be(title);
        config.ApprovalDescription.Should().Be(description);
        config.FormSchema.Should().BeNull();
        config.ApiUrl.Should().BeNull();
    }

    [Fact]
    public void CreateApiCallStep_ShouldCreateConfigurationWithApiDetails()
    {
        // Arrange
        var apiUrl = "https://api.example.com/verify";
        var method = "POST";
        var headers = new Dictionary<string, string>
        {
            { "Authorization", "Bearer token" },
            { "Content-Type", "application/json" }
        };

        // Act
        var config = StepConfiguration.CreateApiCallStep(apiUrl, method, headers);

        // Assert
        config.Type.Should().Be(StepType.ApiCall);
        config.ApiUrl.Should().Be(apiUrl);
        config.HttpMethod.Should().Be(method);
        config.Headers.Should().BeEquivalentTo(headers);
        config.FormSchema.Should().BeNull();
        config.Approvers.Should().BeNull();
    }

    [Fact]
    public void CreateApiCallStep_ShouldUseDefaultMethod_WhenNotSpecified()
    {
        // Arrange
        var apiUrl = "https://api.example.com/verify";

        // Act
        var config = StepConfiguration.CreateApiCallStep(apiUrl);

        // Assert
        config.HttpMethod.Should().Be("POST");
    }

    [Fact]
    public void Constructor_ShouldCreateConfiguration_WithType()
    {
        // Arrange & Act
        var config = new StepConfiguration(StepType.Notification);

        // Assert
        config.Type.Should().Be(StepType.Notification);
    }

    [Fact]
    public void CreateFormStep_ShouldThrow_WhenFormSchemaIsNull()
    {
        // Act
        var act = () => StepConfiguration.CreateFormStep(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateApprovalStep_ShouldThrow_WhenTitleIsNull()
    {
        // Act
        var act = () => StepConfiguration.CreateApprovalStep(null!, "description", new List<string>());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateApprovalStep_ShouldThrow_WhenApproversIsNull()
    {
        // Act
        var act = () => StepConfiguration.CreateApprovalStep("title", "description", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateApiCallStep_ShouldThrow_WhenUrlIsNull()
    {
        // Act
        var act = () => StepConfiguration.CreateApiCallStep(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
