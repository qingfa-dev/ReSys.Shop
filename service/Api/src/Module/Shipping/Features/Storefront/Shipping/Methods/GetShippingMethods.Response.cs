namespace Module.Shipping.Features.Storefront.Shipping.Methods;

public static partial class GetShippingMethods
{
    public sealed record Response(List<ShippingMethodDto> Methods);

    public sealed record ShippingMethodDto(Guid Id, string Name, string? AdminName, string? Code, string CalculatorType, int Position);
}