using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Storefront.Shared.Mappings;
using Module.Shipping.Features.Storefront.Shipping.Calculate;

namespace Module.UnitTests.Shipping.Features.Storefront.Shipping.Calculate.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "CalculateShippingMapping")]
public class CalculateShippingMappingTests
{
    [Fact(DisplayName = "MapToResponse: Should map calculation inputs to response")]
    public void MapToResponse_ShouldMapCalculationToResponse()
    {
        var method = ShippingMethodMethod.Create("Standard", "flat_rate").Value;
        var source = (
            Method: method,
            Currency: "USD",
            Cost: 5.99m,
            IsFreeShipping: true,
            ShippingRateId: Guid.NewGuid());

        var response = source.MapToResponse<CalculateShipping.Response>();

        response.Should().NotBeNull();
        response.ShippingMethodId.Should().Be(method.Id);
        response.MethodName.Should().Be(method.Name);
        response.Cost.Should().Be(source.Cost);
        response.Currency.Should().Be(source.Currency);
        response.IsFreeShipping.Should().Be(source.IsFreeShipping);
        response.ShippingRateId.Should().Be(source.ShippingRateId);
    }
}
