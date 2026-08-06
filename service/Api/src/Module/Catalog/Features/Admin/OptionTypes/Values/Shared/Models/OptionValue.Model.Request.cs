namespace Module.Catalog.Features.Admin.Optiontypes.Values.Shared.Models;

/// <summary>
/// Represents a request for option value creation or update operations.
/// Supports batch operations with an optional unique identifier.
/// </summary>
public record OptionValueRequest : OptionValueParameters
{
    public Guid OptionTypeId { get; init; } = Guid.Empty;
}