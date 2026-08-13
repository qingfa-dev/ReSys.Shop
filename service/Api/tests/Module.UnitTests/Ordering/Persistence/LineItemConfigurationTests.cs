using Microsoft.EntityFrameworkCore.Metadata;

using Module.Catalog.Domain.Variants;
using Module.Inventory.Domain.StockItems;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;

namespace Module.UnitTests.Ordering.Persistence;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "LineItemConfiguration")]
public class LineItemConfigurationTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;

    public LineItemConfigurationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [
            typeof(Order).Assembly,
            typeof(StockItem).Assembly,
            typeof(Variant).Assembly,
        ];
        _dbContext = new ApplicationDbContext(options);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "LineItem: Should expose exactly one FK to Variant")]
    public void LineItem_ShouldHaveSingleVariantForeignKey()
    {
        var entityType = _dbContext.Model.FindEntityType(typeof(LineItem));

        entityType.Should().NotBeNull();

        var variantForeignKeys = entityType!
            .GetForeignKeys()
            .Where(fk => fk.PrincipalEntityType.ClrType == typeof(Variant))
            .ToList();

        variantForeignKeys.Should().HaveCount(1);
        variantForeignKeys[0].Properties.Select(p => p.Name).Should().Contain(nameof(LineItem.VariantId));
    }

    [Fact(DisplayName = "LineItem: Should have no shadow VariantId1 FK property")]
    public void LineItem_ShouldHaveNoShadowVariantId1()
    {
        var entityType = _dbContext.Model.FindEntityType(typeof(LineItem));

        entityType.Should().NotBeNull();
        entityType!.FindProperty("VariantId1").Should().BeNull();
    }
}
