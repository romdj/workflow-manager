using FluentAssertions;
using WorkflowManager.Core.Entities;
using WorkflowManager.Core.Enums;
using Xunit;

namespace WorkflowManager.Core.Tests.Entities;

public class WorkflowEventTests
{
    [Fact]
    public void Constructor_ShouldCreateWorkflowEvent_WithValidParameters()
    {
        // Arrange
        var workflowInstanceId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var eventType = WorkflowEventType.WorkflowStarted;
        var performedBy = Guid.NewGuid();
        var eventData = new Dictionary<string, object> { { "key", "value" } };
        var stepId = "step-1";

        // Act
        var workflowEvent = new WorkflowEvent(
            workflowInstanceId,
            tenantId,
            eventType,
            performedBy,
            eventData,
            stepId);

        // Assert
        workflowEvent.EventId.Should().NotBeEmpty();
        workflowEvent.WorkflowInstanceId.Should().Be(workflowInstanceId);
        workflowEvent.TenantId.Should().Be(tenantId);
        workflowEvent.EventType.Should().Be(eventType);
        workflowEvent.PerformedBy.Should().Be(performedBy);
        workflowEvent.EventData.Should().BeEquivalentTo(eventData);
        workflowEvent.StepId.Should().Be(stepId);
        workflowEvent.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_ShouldCreateEvent_WithoutEventData()
    {
        // Arrange
        var workflowInstanceId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var eventType = WorkflowEventType.WorkflowPaused;
        var performedBy = Guid.NewGuid();

        // Act
        var workflowEvent = new WorkflowEvent(
            workflowInstanceId,
            tenantId,
            eventType,
            performedBy);

        // Assert
        workflowEvent.EventData.Should().BeEmpty();
        workflowEvent.StepId.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldCreateEvent_WithNullStepId_ForWorkflowLevelEvents()
    {
        // Arrange
        var workflowInstanceId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var eventType = WorkflowEventType.WorkflowCompleted;
        var performedBy = Guid.NewGuid();

        // Act
        var workflowEvent = new WorkflowEvent(
            workflowInstanceId,
            tenantId,
            eventType,
            performedBy,
            stepId: null);

        // Assert
        workflowEvent.StepId.Should().BeNull("workflow-level events don't have a step ID");
    }

    [Theory]
    [InlineData(WorkflowEventType.WorkflowStarted)]
    [InlineData(WorkflowEventType.WorkflowPaused)]
    [InlineData(WorkflowEventType.WorkflowResumed)]
    [InlineData(WorkflowEventType.WorkflowCompleted)]
    [InlineData(WorkflowEventType.WorkflowCancelled)]
    [InlineData(WorkflowEventType.StepStarted)]
    [InlineData(WorkflowEventType.StepCompleted)]
    [InlineData(WorkflowEventType.StepFailed)]
    [InlineData(WorkflowEventType.ApprovalRequested)]
    [InlineData(WorkflowEventType.ApprovalGranted)]
    [InlineData(WorkflowEventType.ApprovalRejected)]
    public void Constructor_ShouldAcceptAllEventTypes(WorkflowEventType eventType)
    {
        // Arrange
        var workflowInstanceId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var performedBy = Guid.NewGuid();

        // Act
        var workflowEvent = new WorkflowEvent(
            workflowInstanceId,
            tenantId,
            eventType,
            performedBy);

        // Assert
        workflowEvent.EventType.Should().Be(eventType);
    }

    [Fact]
    public void Constructor_ParameterlessConstructor_ShouldCreateEvent()
    {
        // Act
        var workflowEvent = new WorkflowEvent();

        // Assert
        workflowEvent.Should().NotBeNull();
        workflowEvent.EventData.Should().NotBeNull();
    }
}
