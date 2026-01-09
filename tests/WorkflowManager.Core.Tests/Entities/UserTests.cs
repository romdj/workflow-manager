using FluentAssertions;
using WorkflowManager.Core.Entities;
using WorkflowManager.Core.Enums;
using Xunit;

namespace WorkflowManager.Core.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Constructor_ShouldCreateUser_WithValidParameters()
    {
        // Arrange
        var email = "test@example.com";
        var name = "Test User";
        var role = UserRole.TenantAdmin;
        var tenantId = Guid.NewGuid();

        // Act
        var user = new User(email, name, role, tenantId);

        // Assert
        user.Id.Should().NotBeEmpty();
        user.Email.Should().Be(email);
        user.Name.Should().Be(name);
        user.Role.Should().Be(role);
        user.TenantId.Should().Be(tenantId);
        user.IsActive.Should().BeTrue();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_ShouldCreateMarketOpsUser_WithoutTenantId()
    {
        // Arrange
        var email = "marketops@elia.be";
        var name = "Market Ops User";
        var role = UserRole.MarketOps;

        // Act
        var user = new User(email, name, role, null);

        // Assert
        user.Role.Should().Be(UserRole.MarketOps);
        user.TenantId.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenMarketOpsHasTenantId()
    {
        // Arrange
        var email = "bad@example.com";
        var name = "Bad User";
        var role = UserRole.MarketOps;
        var tenantId = Guid.NewGuid();

        // Act
        var act = () => new User(email, name, role, tenantId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("MarketOps users cannot belong to a tenant");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenNonMarketOpsLacksTenantId()
    {
        // Arrange
        var email = "tenant@example.com";
        var name = "Tenant User";
        var role = UserRole.TenantAdmin;

        // Act
        var act = () => new User(email, name, role, null);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Non-MarketOps users must belong to a tenant");
    }

    [Theory]
    [InlineData(UserRole.TenantAdmin)]
    [InlineData(UserRole.TenantOperator)]
    [InlineData(UserRole.TenantViewer)]
    [InlineData(UserRole.ComplianceReviewer)]
    public void Constructor_ShouldRequireTenantId_ForNonMarketOpsRoles(UserRole role)
    {
        // Arrange
        var email = "user@example.com";
        var name = "User";

        // Act
        var act = () => new User(email, name, role, null);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Non-MarketOps users must belong to a tenant");
    }

    [Fact]
    public void HasAccessToTenant_ShouldReturnTrue_ForMarketOpsUser()
    {
        // Arrange
        var user = new User("marketops@elia.be", "Market Ops", UserRole.MarketOps, null);
        var anyTenantId = Guid.NewGuid();

        // Act
        var hasAccess = user.HasAccessToTenant(anyTenantId);

        // Assert
        hasAccess.Should().BeTrue("MarketOps users can access all tenants");
    }

    [Fact]
    public void HasAccessToTenant_ShouldReturnTrue_WhenTenantMatches()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var user = new User("user@example.com", "User", UserRole.TenantAdmin, tenantId);

        // Act
        var hasAccess = user.HasAccessToTenant(tenantId);

        // Assert
        hasAccess.Should().BeTrue();
    }

    [Fact]
    public void HasAccessToTenant_ShouldReturnFalse_WhenTenantDoesNotMatch()
    {
        // Arrange
        var userTenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var user = new User("user@example.com", "User", UserRole.TenantAdmin, userTenantId);

        // Act
        var hasAccess = user.HasAccessToTenant(otherTenantId);

        // Assert
        hasAccess.Should().BeFalse("tenant users can only access their own tenant");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenEmailIsNull()
    {
        // Act
        var act = () => new User(null!, "Name", UserRole.TenantAdmin, Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenNameIsNull()
    {
        // Act
        var act = () => new User("email@test.com", null!, UserRole.TenantAdmin, Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
