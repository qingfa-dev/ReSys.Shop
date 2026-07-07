namespace Module.Shipping.Features.Storefront.Shipping.Methods;

public static partial class GetShippingMethods
{
    public class Response
    {
        public List<ShippingMethodDto> Methods { get; init; } = [];
    }

    public class ShippingMethodDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? AdminName { get; init; }
        public string? Code { get; init; }
        public string CalculatorType { get; init; } = string.Empty;
        public int Position { get; init; }
    }
}
