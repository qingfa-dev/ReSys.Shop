using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Features.Admin.StockReservations.Shared.Mappings;
using Module.Inventory.Features.Admin.StockReservations.Shared.Models;

namespace Module.UnitTests.Inventory.Features.Admin.StockReservations.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "StockReservationMapping")]
public class StockReservationMappingTests
{
    [Fact(DisplayName = "MapToDetail: Should map StockReservation to detail response")]
    public void MapToDetail_ShouldMapEntityToDetail()
    {
        var reservation = CreateStockReservation();

        var response = reservation.MapToDetail<StockReservationDetailResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(reservation.Id);
        response.VariantId.Should().Be(reservation.VariantId);
        response.StockLocationId.Should().Be(reservation.StockLocationId);
        response.OrderId.Should().Be(reservation.OrderId);
        response.Quantity.Should().Be(reservation.Quantity);
        response.State.Should().Be(reservation.State);
        response.ExpiresAtUtc.Should().Be(reservation.ExpiresAtUtc);
        response.Reason.Should().Be(reservation.Reason ?? string.Empty);
        response.CreatedAtUtc.Should().Be(reservation.CreatedAtUtc);
        response.ModifiedAtUtc.Should().Be(reservation.ModifiedAtUtc);
    }

    [Fact(DisplayName = "MapToListItem: Should map StockReservation to list item response")]
    public void MapToListItem_ShouldMapEntityToList()
    {
        var reservation = CreateStockReservation();

        var response = reservation.MapToListItem<StockReservationListItemResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(reservation.Id);
        response.VariantId.Should().Be(reservation.VariantId);
        response.StockLocationId.Should().Be(reservation.StockLocationId);
        response.OrderId.Should().Be(reservation.OrderId);
        response.Quantity.Should().Be(reservation.Quantity);
        response.State.Should().Be(reservation.State);
        response.ExpiresAtUtc.Should().Be(reservation.ExpiresAtUtc);
        response.Reason.Should().Be(reservation.Reason ?? string.Empty);
        response.CreatedAtUtc.Should().Be(reservation.CreatedAtUtc);
    }

    [Fact(DisplayName = "MapToDomain: Should map request to new StockReservation entity")]
    public void MapToDomain_ShouldMapRequestToEntity()
    {
        var request = new StockReservationRequest
        {
            VariantId = Guid.NewGuid(),
            Quantity = 5,
            StockLocationId = Guid.NewGuid(),
            OrderId = Guid.NewGuid()
        };

        var result = request.MapToDomain();
        var entity = result.Value;

        result.IsSuccess.Should().BeTrue();
        entity.Should().NotBeNull();
        entity.VariantId.Should().Be(request.VariantId);
        entity.Quantity.Should().Be(request.Quantity);
        entity.StockLocationId.Should().Be(request.StockLocationId);
        entity.OrderId.Should().Be(request.OrderId);
    }

    [Fact(DisplayName = "MapToDomain: Should fail with invalid quantity")]
    public void MapToDomain_WithInvalidQuantity_ShouldFail()
    {
        var request = new StockReservationRequest
        {
            VariantId = Guid.NewGuid(),
            Quantity = 0,
            StockLocationId = Guid.NewGuid()
        };

        var result = request.MapToDomain();

        result.IsFailure.Should().BeTrue();
    }

    private static StockReservation CreateStockReservation()
    {
        var result = StockReservationMethod.Reserve(
            Guid.NewGuid(), 5, Guid.NewGuid(), Guid.NewGuid(), 30);
        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }
}
