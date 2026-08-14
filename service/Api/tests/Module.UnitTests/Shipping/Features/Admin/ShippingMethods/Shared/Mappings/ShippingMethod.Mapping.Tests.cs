using Module.Shipping.Domain.ShippingMethods;
using ShippingDomain = Module.Shipping.Domain.ShippingMethods.ShippingMethod;
using Module.Shipping.Features.Admin.ShippingMethods.Shared.Mappings;
using Module.Shipping.Features.Admin.ShippingMethods.Shared.Models;

namespace Module.UnitTests.Shipping.Features.Admin.ShippingMethods.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "ShippingMethodMapping")]
public class ShippingMethodMappingTests
{
    [Fact(DisplayName = "MapToDetail: Should map ShippingMethod to detail response")]
    public void MapToDetail_ShouldMapEntityToDetail()
    {
        var method = CreateShippingMethod();

        var response = method.MapToDetail<ShippingMethodDetailResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(method.Id);
        response.Name.Should().Be(method.Name);
        response.Code.Should().Be(method.Code);
        response.TrackingUrl.Should().Be(method.TrackingUrl);
        response.AdminName.Should().Be(method.AdminName);
        response.Position.Should().Be(method.Position);
        response.AvailableToUsers.Should().Be(method.AvailableToUsers);
        response.CalculatorType.Should().Be(method.CalculatorType);
        response.Presentation.Should().Be(method.Presentation);
        response.CreatedAtUtc.Should().Be(method.CreatedAtUtc);
        response.ModifiedAtUtc.Should().Be(method.ModifiedAtUtc);
        response.CreatedBy.Should().Be(method.CreatedBy);
        response.ModifiedBy.Should().Be(method.ModifiedBy);
        response.IsDeleted.Should().Be(method.IsDeleted);
        response.DeletedAtUtc.Should().Be(method.DeletedAtUtc);
    }

    [Fact(DisplayName = "MapToListItem: Should map ShippingMethod to list item response")]
    public void MapToListItem_ShouldMapEntityToList()
    {
        var method = CreateShippingMethod();

        var response = method.MapToListItem<ShippingMethodListItemResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(method.Id);
        response.Name.Should().Be(method.Name);
        response.Code.Should().Be(method.Code);
        response.TrackingUrl.Should().Be(method.TrackingUrl);
        response.AdminName.Should().Be(method.AdminName);
        response.Position.Should().Be(method.Position);
        response.AvailableToUsers.Should().Be(method.AvailableToUsers);
        response.CalculatorType.Should().Be(method.CalculatorType);
        response.Presentation.Should().Be(method.Presentation);
        response.CreatedAtUtc.Should().Be(method.CreatedAtUtc);
        response.ModifiedAtUtc.Should().Be(method.ModifiedAtUtc);
    }

    [Fact(DisplayName = "MapToDomain: Should map request to new ShippingMethod entity")]
    public void MapToDomain_Create_ShouldMapRequestToEntity()
    {
        var request = new ShippingMethodRequest
        {
            Name = "Express",
            CalculatorType = "flat_rate",
            Code = "EXP",
            TrackingUrl = "https://track.example.com",
            AdminName = "Express Admin",
            Position = 1,
            AvailableToUsers = true,
            Presentation = "Express Delivery"
        };

        var result = request.MapToDomain();
        var entity = result.Value;

        result.IsSuccess.Should().BeTrue();
        entity.Should().NotBeNull();
        entity.Name.Should().Be(request.Name);
        entity.CalculatorType.Should().Be(request.CalculatorType);
        entity.Code.Should().Be(request.Code);
        entity.TrackingUrl.Should().Be(request.TrackingUrl);
        entity.AdminName.Should().Be(request.AdminName);
        entity.Position.Should().Be(request.Position);
        entity.AvailableToUsers.Should().Be(request.AvailableToUsers);
        entity.Presentation.Should().Be(request.Presentation);
    }

    [Fact(DisplayName = "MapToDomain (Update): Should update existing ShippingMethod")]
    public void MapToDomain_Update_ShouldUpdateEntity()
    {
        var method = CreateShippingMethod();
        var request = new ShippingMethodRequest
        {
            Name = "Updated",
            CalculatorType = "weight",
            Code = "UPD"
        };

        var result = request.MapToDomain(method);

        result.IsSuccess.Should().BeTrue();
        method.Name.Should().Be("Updated");
        method.CalculatorType.Should().Be("weight");
    }

    // [Fact(DisplayName = "MapUpdateToDomain: Should apply partial update")]
    // public void MapUpdateToDomain_ShouldApplyPatch() { }

    private static ShippingDomain CreateShippingMethod()
    {
        var result = ShippingMethodExtensions.Create("Express", "flat_rate", "EXP");
        result.IsSuccess.Should().BeTrue();
        var method = result.Value;
        method.TrackingUrl = "https://track.example.com";
        method.AdminName = "Express Admin";
        method.Position = 1;
        method.AvailableToUsers = true;
        method.Presentation = "Express Delivery";
        return method;
    }
}
