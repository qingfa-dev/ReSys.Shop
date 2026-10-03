namespace Module.Catalog.Features.Admin.Shared.Models;

public abstract record OptionTypeParameters : INamedParameters, ISortableParameters
{
    public string Name { get; init; } = string.Empty;
    public string? Presentation { get; init; }
    public int Position { get; init; } = 0;
    public bool Filterable { get; init; } = false;
}

/// <summary>
/// Represents a base request for option type creation or update operations.
/// Inherits common option type properties from <see cref="OptionTypeParameters"/>.
/// </summary>
public record OptionTypeRequest : OptionTypeParameters;

public record OptionTypeListItemResponse : OptionTypeParameters
{
    /// <summary>
    /// Gets or initializes the unique identifier of the option type.
    /// </summary>
    public Guid Id { get; init; }

    // Auditing properties:
    /// <summary>
    /// Gets or sets the timestamp when the entity was created.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the entity was last modified.
    /// </summary>
    public DateTimeOffset? ModifiedAtUtc { get; set; }

    // Stats:
    /// <summary>
    /// Gets or initializes the count of option values associated with this option type.
    /// </summary>
    public int OptionValuesCount { get; init; }

    /// <summary>
    /// Gets or initializes the count of products that utilize this option type.
    /// </summary>
    public int ProductsCount { get; init; }
}

/// <summary>
/// Represents the response data for an option type.
/// Inherits common option type properties from <see cref="OptionTypeParameters"/>.
/// </summary>
public record OptionTypeDetailResponse : OptionTypeParameters
{
    /// <summary>
    /// Gets or initializes the unique identifier of the option type.
    /// </summary>
    public Guid Id { get; init; }

    // Auditing properties:
    /// <summary>
    /// Gets or sets the timestamp when the entity was created.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the entity was last modified.
    /// </summary>
    public DateTimeOffset? ModifiedAtUtc { get; set; }
}
