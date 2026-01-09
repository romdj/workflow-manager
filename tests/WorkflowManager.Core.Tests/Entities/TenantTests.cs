using FluentAssertions;
using WorkflowManager.Core.Entities;
using WorkflowManager.Core.Enums;
using Xunit;

namespace WorkflowManager.Core.Tests.Entities;

public class TenantTests
{
    [Fact]
    public void Constructor_ShouldCreateTenant_WithValidParameters()
    {
        // Arrange
        var companyName = "Test Company BVBA";
        var vatNumber = "BE0123456789";

        // Act
        var tenant = new Tenant(companyName, vatNumber);

        // Assert
        tenant.Id.Should().NotBeEmpty();
        tenant.CompanyName.Should().Be(companyName);
        tenant.VATNumber.Should().Be(vatNumber);
        tenant.Status.Should().Be(TenantStatus.Onboarding);
        tenant.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        tenant.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        tenant.MarketRoles.Should().BeEmpty();
        tenant.Users.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenCompanyNameIsNull()
    {
        // Act
        var act = () => new Tenant(null!, "BE0123456789");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenVATNumberIsNull()
    {
        // Act
        var act = () => new Tenant("Company", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}

public class TenantMarketRoleTests
{
    [Fact]
    public void Constructor_ShouldCreateTenantMarketRole_WithValidParameters()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var marketRole = MarketRole.BRP;

        // Act
        var tenantMarketRole = new TenantMarketRole(tenantId, marketRole);

        // Assert
        tenantMarketRole.Id.Should().NotBeEmpty();
        tenantMarketRole.TenantId.Should().Be(tenantId);
        tenantMarketRole.MarketRole.Should().Be(marketRole);
        tenantMarketRole.Status.Should().Be(TenantStatus.Onboarding);
        tenantMarketRole.OnboardedAt.Should().BeNull();
        tenantMarketRole.ContractReference.Should().BeNull();
        tenantMarketRole.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(MarketRole.BRP)]
    [InlineData(MarketRole.BSP)]
    [InlineData(MarketRole.GU)]
    [InlineData(MarketRole.ACH)]
    [InlineData(MarketRole.CRM)]
    [InlineData(MarketRole.ESP)]
    [InlineData(MarketRole.DSO)]
    [InlineData(MarketRole.TSO)]
    [InlineData(MarketRole.SA)]
    [InlineData(MarketRole.OPA)]
    [InlineData(MarketRole.VSP)]
    public void Constructor_ShouldAcceptAllMarketRoles(MarketRole marketRole)
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var tenantMarketRole = new TenantMarketRole(tenantId, marketRole);

        // Assert
        tenantMarketRole.MarketRole.Should().Be(marketRole);
    }
}
