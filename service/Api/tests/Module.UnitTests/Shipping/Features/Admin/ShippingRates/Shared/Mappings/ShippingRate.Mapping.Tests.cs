using Module.Shipping.Domain.ShippingRates;
using RateDomain = Module.Shipping.Domain.ShippingRates.ShippingRate;
using Module.Shipping.Features.Admin.ShippingRates.Shared.Mappings;
using Module.Shipping.Features.Admin.ShippingRates.Shared.Models;

namespace Module.UnitTests.Shipping.Features.Admin.ShippingRates.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "ShippingRateMapping")]
public class ShippingRateMappingTests
{
    [Fact(DisplayName = "MapToDetail: Should map ShippingRate to detail response")]
    public void MapToDetail_ShouldMapEntityToDetail()
    {
        var rate = CreateShippingRate();

        var response = rate.MapToDetail<ShippingRateDetailResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(rate.Id);
        response.Name.Should().Be(rate.Name);
        response.Cost.Should().Be(rate.Cost);
        response.FinalPrice.Should().Be(rate.FinalPrice);
        response.Selected.Should().Be(rate.Selected);
        response.DeliveryRange.Should().Be(rate.DeliveryRange);
        response.MinWeight.Should().Be(rate.MinWeight);
        response.MaxWeight.Should().Be(rate.MaxWeight);
        response.FreeShippingThreshold.Should().Be(rate.FreeShippingThreshold);
        response.ShippingMethodId.Should().Be(rate.ShippingMethodId);
        response.CreatedAtUtc.Should().Be(rate.CreatedAtUtc);
        response.ModifiedAtUtc.Should().Be(rate.ModifiedAtUtc);
        response.CreatedBy.Should().Be(rate.CreatedBy);
        response.ModifiedBy.Should().Be(rate.ModifiedBy);
    }

    [Fact(DisplayName = "MapToListItem: Should map ShippingRate to list item response")]
    public void MapToListItem_ShouldMapEntityToList()
    {
        var rate = CreateShippingRate();

        var response = rate.MapToListItem<ShippingRateListItemResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(rate.Id);
        response.Name.Should().Be(rate.Name);
        response.Cost.Should().Be(rate.Cost);
        response.FinalPrice.Should().Be(rate.FinalPrice);
        response.Selected.Should().Be(rate.Selected);
        response.DeliveryRange.Should().Be(rate.DeliveryRange);
        response.MinWeight.Should().Be(rate.MinWeight);
        response.MaxWeight.Should().Be(rate.MaxWeight);
        response.FreeShippingThreshold.Should().Be(rate.FreeShippingThreshold);
        response.ShippingMethodId.Should().Be(rate.ShippingMethodId);
        response.CreatedAtUtc.Should().Be(rate.CreatedAtUtc);
        response.ModifiedAtUtc.Should().Be(rate.ModifiedAtUtc);
    }

    [Fact(DisplayName = "MapToDomain: Should map request to new ShippingRate entity")]
    public void MapToDomain_Create_ShouldMapRequestToEntity()
    {
        var shippingMethodId = Guid.NewGuid();
        var request = new ShippingRateRequest
        {
            Name = "Standard",
            Cost = 10.00m,
            ShippingMethodId = shippingMethodId,
            DeliveryRange = "3-5 days",
            MinWeight = 0.1m,
            MaxWeight = 10.0m,
            FreeShippingThreshold = 100.0m
        };

        var result = request.MapToDomain();
        var entity = result.Value;

        result.IsSuccess.Should().BeTrue();
        entity.Should().NotBeNull();
        entity.Name.Should().Be(request.Name);
        entity.Cost.Should().Be(request.Cost);
        entity.ShippingMethodId.Should().Be(request.ShippingMethodId);
        entity.DeliveryRange.Should().Be(request.DeliveryRange);
        entity.MinWeight.Should().Be(request.MinWeight);
        entity.MaxWeight.Should().Be(request.MaxWeight);
        entity.FreeShippingThreshold.Should().Be(request.FreeShippingThreshold);
    }

    [Fact(DisplayName = "MapToDomain (Update): Should update existing ShippingRate")]
    public void MapToDomain_Update_ShouldUpdateEntity()
    {
        var rate = CreateShippingRate();
        var request = new ShippingRateRequest
        {
            Name = "Updated Rate",
            Cost = 15.00m,
            ShippingMethodId = rate.ShippingMethodId,
            DeliveryRange = "1-2 days",
            MinWeight = 0.5m,
            MaxWeight = 20.0m,
            FreeShippingThreshold = 200.0m
        };

        var result = request.MapToDomain(rate);

        result.IsSuccess.Should().BeTrue();
        rate.Name.Should().Be("Updated Rate");
        rate.Cost.Should().Be(15.00m);
        rate.DeliveryRange.Should().Be("1-2 days");
    }

    // [Fact(DisplayName = "MapUpdateToDomain: Should apply partial update")]
    // public void MapUpdateToDomain_ShouldApplyPatch() { }

    private static RateDomain CreateShippingRate()
    {
        var result = ShippingRateExtensions.Create(
            "Standard", 10.00m, Guid.NewGuid(), "3-5 days", 0.1m, 10.0m, 100.0m);
        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }
}
