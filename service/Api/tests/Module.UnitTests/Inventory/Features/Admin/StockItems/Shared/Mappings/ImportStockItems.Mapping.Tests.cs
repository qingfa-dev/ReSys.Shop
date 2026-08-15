using Module.Inventory.Features.Admin.StockItems.Import;
using Module.Inventory.Features.Admin.StockItems.Shared.Mappings;

namespace Module.UnitTests.Inventory.Features.Admin.StockItems.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "ImportStockItemsMapping")]
public class ImportStockItemsMappingTests
{
    [Fact(DisplayName = "MapToImportResult: Should map counts to import response")]
    public void MapToImportResult_ShouldMapCounts()
    {
        var errors = new List<string> { "Line 2: Invalid VariantId", "Line 3: Invalid CountOnHand" };

        var response = (created: 5, updated: 3, errors).MapToImportResult<ImportStockItems.Response>();

        response.Should().NotBeNull();
        response.Created.Should().Be(5);
        response.Updated.Should().Be(3);
        response.Failed.Should().Be(errors.Count);
        response.Errors.Should().BeEquivalentTo(errors);
    }
}
