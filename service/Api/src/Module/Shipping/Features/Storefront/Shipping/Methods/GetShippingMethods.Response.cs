namespace Module.Shipping.Features.Storefront.Shipping.Methods;

public static partial class GetShippingMethods
{
    // EXCEPTION: wraps list of DTOs — collection wrapper, not a single shipping method entity
    // EXCEPTION: collection wrapper — inner ShippingMethodDto is the domain DTO
public sealed record Response
{
    public List<ShippingMethodDto> Methods { get; init; } = default!;
}

    public sealed record ShippingMethodDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = default!;
        public string? AdminName { get; init; }
        public string? Code { get; init; }
        public string CalculatorType { get; init; } = default!;
        public int Position { get; init; }
    }
}