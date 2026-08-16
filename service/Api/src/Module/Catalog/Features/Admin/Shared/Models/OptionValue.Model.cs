namespace Module.Catalog.Features.Admin.Shared.Models;

// public abstract record OptionValueParameters(string Name = "", string Presentation = "", int Position = 0);

public abstract record OptionValueParameters : INamedParameters, ISortableParameters
{
    public string Name { get; init; } = string.Empty;
    public string? Presentation { get; init; }
    public int Position { get; init; } = 0;
}

/// <summary>
/// Represents a request for option value creation or update operations.
/// Supports batch operations with an optional unique identifier.
/// </summary>
public record OptionValueRequest : OptionValueParameters
{
    public Guid OptionTypeId { get; init; } = Guid.Empty;
}

/// <summary>
/// Represents the response data for an option value.
/// </summary>
public record OptionValueListItemResponse : OptionValueParameters
{
    /// <summary>
    /// Gets or initializes the unique identifier of the option value.
    /// </summary>
    public Guid Id { get; init; }
    public Guid OptionTypeId { get; init; } = Guid.Empty;

    /// <summary> 
    /// Gets or initializes the name of the parent option type, included for convenience in responses.
    /// </summary>
    public string? OptionTypeName { get; init; }

    /// <summary>
    /// Gets or initializes the UTC date and time when the option value was created.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>
    /// Gets or initializes the UTC date and time when the option value was last modified.
    /// </summary>
    public DateTimeOffset? ModifiedAtUtc { get; init; }
}

public record OptionValueDetailResponse : OptionValueListItemResponse;
