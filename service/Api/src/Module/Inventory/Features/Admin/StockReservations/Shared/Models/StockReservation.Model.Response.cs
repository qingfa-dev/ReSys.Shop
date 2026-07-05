namespace Module.Inventory.Features.Admin.StockReservations.Shared.Models;

/// <summary>Detail response for a stock reservation, including audit timestamps.</summary>
public class StockReservationDetailResponse : StockReservationParameters
{
    /// <summary>Gets or sets the unique identifier of the stock reservation.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets or sets the UTC timestamp when this reservation was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>Gets or sets the UTC timestamp when this reservation was last modified.</summary>
    public DateTimeOffset? ModifiedAtUtc { get; set; }
}

/// <summary>List item response for a stock reservation.</summary>
public class StockReservationListItemResponse : StockReservationParameters
{
    /// <summary>Gets or sets the unique identifier of the stock reservation.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets or sets the UTC timestamp when this reservation was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }
}
