namespace Module.Catalog.Features.Admin.OptionTypes.OptionValues.Shared.Models;

/// <summary>
/// Represents a request for option value creation or update operations.
/// Supports batch operations with an optional unique identifier.
/// </summary>
public record OptionValueRequest : OptionValueParameters;