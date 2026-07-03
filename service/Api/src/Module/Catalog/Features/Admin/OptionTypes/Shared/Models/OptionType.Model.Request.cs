namespace Module.Catalog.Features.Admin.OptionTypes.Shared.Models;

/// <summary>
/// Represents a base request for option type creation or update operations.
/// Inherits common option type properties from <see cref="OptionTypeParameters"/>.
/// </summary>
public record OptionTypeRequest : OptionTypeParameters;