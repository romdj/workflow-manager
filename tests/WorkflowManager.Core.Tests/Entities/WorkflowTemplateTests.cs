using FluentAssertions;
using WorkflowManager.Core.Entities;
using WorkflowManager.Core.Enums;
using WorkflowManager.Core.ValueObjects;
using Xunit;

namespace WorkflowManager.Core.Tests.Entities;

public class WorkflowTemplateTests
{
    [Fact]
    public void Constructor_ShouldCreateWorkflowTemplate_WithValidParameters()
    {
        // Arrange
        var name = "BRP Onboarding";
        var marketRole = MarketRole.BRP;
        var elsaWorkflowDefinitionId = "brp-onboarding-v1";
        var definition = new WorkflowDefinition
        {
            Steps = new List<WorkflowStep>
            {
                new("step-1", "Company Info", StepType.Form, new StepConfiguration(), 1)
            }
        };

        // Act
        var template = new WorkflowTemplate(name, marketRole, elsaWorkflowDefinitionId, definition);

        // Assert
        template.Id.Should().NotBeEmpty();
        template.Name.Should().Be(name);
        template.MarketRole.Should().Be(marketRole);
        template.ElsaWorkflowDefinitionId.Should().Be(elsaWorkflowDefinitionId);
        template.Definition.Should().Be(definition);
        template.Version.Should().Be(1);
        template.IsActive.Should().BeTrue();
        template.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        template.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenNameIsNull()
    {
        // Arrange
        var definition = new WorkflowDefinition();

        // Act
        var act = () => new WorkflowTemplate(null!, MarketRole.BRP, "elsa-id", definition);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenElsaWorkflowDefinitionIdIsNull()
    {
        // Arrange
        var definition = new WorkflowDefinition();

        // Act
        var act = () => new WorkflowTemplate("Template", MarketRole.BRP, null!, definition);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenDefinitionIsNull()
    {
        // Act
        var act = () => new WorkflowTemplate("Template", MarketRole.BRP, "elsa-id", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}

public class WorkflowStepTests
{
    [Fact]
    public void Constructor_ShouldCreateWorkflowStep_WithValidParameters()
    {
        // Arrange
        var id = "step-1";
        var name = "Company Information";
        var type = StepType.Form;
        var configuration = StepConfiguration.CreateFormStep(new FormSchema
        {
            Title = "Company Info",
            Fields = new List<FormField>()
        });
        var order = 1;

        // Act
        var step = new WorkflowStep(id, name, type, configuration, order);

        // Assert
        step.Id.Should().Be(id);
        step.Name.Should().Be(name);
        step.Type.Should().Be(type);
        step.Configuration.Should().Be(configuration);
        step.Order.Should().Be(order);
        step.Required.Should().BeTrue();
        step.AllowedTransitions.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenIdIsNull()
    {
        // Act
        var act = () => new WorkflowStep(null!, "Name", StepType.Form, new StepConfiguration(), 1);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenNameIsNull()
    {
        // Act
        var act = () => new WorkflowStep("id", null!, StepType.Form, new StepConfiguration(), 1);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenConfigurationIsNull()
    {
        // Act
        var act = () => new WorkflowStep("id", "Name", StepType.Form, null!, 1);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
