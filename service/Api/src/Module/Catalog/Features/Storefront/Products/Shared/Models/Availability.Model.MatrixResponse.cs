namespace Module.Catalog.Features.Storefront.Products.Shared.Models;

public class StoreAvailabilityMatrixResponse
{
    public List<AvailabilityAxis> Axes { get; init; } = [];
    public List<AvailabilityCell> Cells { get; init; } = [];
}

public class AvailabilityAxis
{
    public string Name { get; init; } = string.Empty;
    public string? Presentation { get; init; }
    public List<AvailabilityAxisValue> Values { get; init; } = [];
}

public class AvailabilityAxisValue
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Presentation { get; init; }
}

public class AvailabilityCell
{
    public Guid VariantId { get; init; }
    public Guid OptionValue1Id { get; init; }
    public Guid? OptionValue2Id { get; init; }
    public string Status { get; init; } = "unknown";
    public decimal? Price { get; init; }
    public string? Currency { get; init; }
}
