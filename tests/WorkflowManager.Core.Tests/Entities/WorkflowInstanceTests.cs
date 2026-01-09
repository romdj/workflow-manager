using FluentAssertions;
using WorkflowManager.Core.Entities;
using WorkflowManager.Core.Enums;
using WorkflowManager.Core.ValueObjects;
using Xunit;

namespace WorkflowManager.Core.Tests.Entities;

public class WorkflowInstanceIndexTests
{
    [Fact]
    public void Constructor_ShouldCreateWorkflowInstanceIndex_WithValidParameters()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var templateId = Guid.NewGuid();
        var marketRole = MarketRole.BRP;
        var elsaInstanceId = "elsa-instance-123";
        var createdBy = Guid.NewGuid();

        // Act
        var index = new WorkflowInstanceIndex(
            id,
            tenantId,
            templateId,
            marketRole,
            elsaInstanceId,
            createdBy);

        // Assert
        index.Id.Should().Be(id);
        index.TenantId.Should().Be(tenantId);
        index.TemplateId.Should().Be(templateId);
        index.MarketRole.Should().Be(marketRole);
        index.ElsaInstanceId.Should().Be(elsaInstanceId);
        index.Status.Should().Be(WorkflowStatus.Draft);
        index.CreatedBy.Should().Be(createdBy);
        index.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        index.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        index.CurrentStepId.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenElsaInstanceIdIsNull()
    {
        // Act
        var act = () => new WorkflowInstanceIndex(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            MarketRole.BRP,
            null!,
            Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("elsaInstanceId");
    }
}

public class WorkflowInstanceTests
{
    [Fact]
    public void Constructor_ShouldCreateWorkflowInstance_WithValidParameters()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenantName = "Test Company";
        var templateId = Guid.NewGuid();
        var marketRole = MarketRole.BSP;
        var elsaInstanceId = "elsa-instance-456";
        var createdBy = Guid.NewGuid();

        // Act
        var instance = new WorkflowInstance(
            tenantId,
            tenantName,
            templateId,
            marketRole,
            elsaInstanceId,
            createdBy);

        // Assert
        instance.Id.Should().NotBeEmpty();
        instance.TenantId.Should().Be(tenantId);
        instance.TenantName.Should().Be(tenantName);
        instance.TemplateId.Should().Be(templateId);
        instance.MarketRole.Should().Be(marketRole);
        instance.ElsaInstanceId.Should().Be(elsaInstanceId);
        instance.Status.Should().Be(WorkflowStatus.Draft);
        instance.CreatedBy.Should().Be(createdBy);
        instance.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        instance.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        instance.State.Should().NotBeNull();
        instance.State.Metadata.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenTenantNameIsNull()
    {
        // Act
        var act = () => new WorkflowInstance(
            Guid.NewGuid(),
            null!,
            Guid.NewGuid(),
            MarketRole.BRP,
            "elsa-id",
            Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("tenantName");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenElsaInstanceIdIsNull()
    {
        // Act
        var act = () => new WorkflowInstance(
            Guid.NewGuid(),
            "Tenant",
            Guid.NewGuid(),
            MarketRole.BRP,
            null!,
            Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("elsaInstanceId");
    }

    [Fact]
    public void Constructor_ShouldInitializeEmptyStepStates()
    {
        // Arrange & Act
        var instance = new WorkflowInstance(
            Guid.NewGuid(),
            "Tenant",
            Guid.NewGuid(),
            MarketRole.GU,
            "elsa-id",
            Guid.NewGuid());

        // Assert
        instance.State.StepStates.Should().BeEmpty();
        instance.State.CurrentStepId.Should().BeNull();
    }
}
