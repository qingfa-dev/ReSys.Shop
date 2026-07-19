namespace Module.Inventory.Features.Admin.StockReservations.Shared.Models;

public record StockReservationDetailResponse : StockReservationParameters, IResponse
{
    /// <summary>Gets or sets the unique identifier of the stock reservation.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets or sets the UTC timestamp when this reservation was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>Gets or sets the UTC timestamp when this reservation was last modified.</summary>
    public DateTimeOffset? ModifiedAtUtc { get; set; }
}

public record StockReservationListItemResponse : StockReservationParameters, IResponse
{
    /// <summary>Gets or sets the unique identifier of the stock reservation.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets or sets the UTC timestamp when this reservation was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }
}