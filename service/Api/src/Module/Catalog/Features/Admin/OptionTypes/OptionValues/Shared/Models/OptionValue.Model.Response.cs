namespace Module.Catalog.Features.Admin.OptionTypes.OptionValues.Shared.Models;

/// <summary>
/// Represents the response data for an option value.
/// </summary>
public record OptionValueListItemResponse : OptionValueParameters
{
    /// <summary>
    /// Gets or initializes the unique identifier of the option value.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets or initializes the unique identifier of the parent option type.
    /// </summary>
    public Guid OptionTypeId { get; init; }

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