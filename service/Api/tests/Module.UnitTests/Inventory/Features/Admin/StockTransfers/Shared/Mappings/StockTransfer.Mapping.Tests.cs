using Module.Inventory.Domain.StockTransfers;
using Module.Inventory.Features.Admin.StockTransfers.Shared.Mappings;
using Module.Inventory.Features.Admin.StockTransfers.Shared.Models;

namespace Module.UnitTests.Inventory.Features.Admin.StockTransfers.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "StockTransferMapping")]
public class StockTransferMappingTests
{
    [Fact(DisplayName = "MapToDetail: Should map StockTransfer to detail response")]
    public void MapToDetail_ShouldMapEntityToDetail()
    {
        var transfer = CreateStockTransfer();

        var response = transfer.MapToDetail<StockTransferDetailResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(transfer.Id);
        response.Number.Should().Be(transfer.Number);
        response.Reference.Should().Be(transfer.Reference);
        response.State.Should().Be(transfer.State.ToString());
        response.SourceLocationId.Should().Be(transfer.SourceLocationId);
        response.DestinationLocationId.Should().Be(transfer.DestinationLocationId);
        response.CreatedAtUtc.Should().Be(transfer.CreatedAtUtc);
        response.ModifiedAtUtc.Should().Be(transfer.ModifiedAtUtc);
        response.Items.Should().HaveCount(2);
        response.Items.First().Id.Should().Be(transfer.TransferItems.First().Id);
        response.Items.First().VariantId.Should().Be(transfer.TransferItems.First().VariantId);
        response.Items.First().Quantity.Should().Be(transfer.TransferItems.First().Quantity);
    }

    [Fact(DisplayName = "MapToListItem: Should map StockTransfer to list item response")]
    public void MapToListItem_ShouldMapEntityToList()
    {
        var transfer = CreateStockTransfer();

        var response = transfer.MapToListItem<StockTransferListItemResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(transfer.Id);
        response.Number.Should().Be(transfer.Number);
        response.Reference.Should().Be(transfer.Reference);
        response.State.Should().Be(transfer.State.ToString());
        response.SourceLocationId.Should().Be(transfer.SourceLocationId);
        response.DestinationLocationId.Should().Be(transfer.DestinationLocationId);
        response.TotalItems.Should().Be(2);
        response.CreatedAtUtc.Should().Be(transfer.CreatedAtUtc);
    }

    [Fact(DisplayName = "MapToDomain: Should map request to new StockTransfer entity")]
    public void MapToDomain_ShouldMapRequestToEntity()
    {
        var request = new StockTransferRequest
        {
            Reference = "REF-001",
            SourceLocationId = Guid.NewGuid(),
            DestinationLocationId = Guid.NewGuid(),
            Items =
            [
                new() { VariantId = Guid.NewGuid(), Quantity = 5 },
                new() { VariantId = Guid.NewGuid(), Quantity = 10 }
            ]
        };

        var result = request.MapToDomain();
        var entity = result.Value;

        result.IsSuccess.Should().BeTrue();
        entity.Should().NotBeNull();
        entity.Reference.Should().Be(request.Reference);
        entity.SourceLocationId.Should().Be(request.SourceLocationId);
        entity.DestinationLocationId.Should().Be(request.DestinationLocationId);
        entity.TransferItems.Should().HaveCount(2);
    }

    private static StockTransfer CreateStockTransfer()
    {
        var items = new List<(Guid VariantId, int Quantity)>
        {
            (Guid.NewGuid(), 5),
            (Guid.NewGuid(), 10)
        };
        var result = StockTransferExtensions.Create(
            "REF-001", Guid.NewGuid(), Guid.NewGuid(), items);
        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }
}
