using Module.Catalog.Domain.Products;

namespace Module.Catalog.Features.Admin.Shared.Models;

public abstract record ProductParameters
{
    #region Properties
    public string Name { get; set; } = string.Empty;
    public ProductStatus Status { get; init; }
    public string Description { get; set; } = string.Empty;
    #endregion Properties

    #region SEO
    public string Slug { get; set; } = string.Empty;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    #endregion SEO

    #region Timestamp
    public DateTimeOffset? AvailableOn { get; set; }
    public DateTimeOffset? DiscontinueOn { get; set; }
    public DateTimeOffset? MakeActiveAt { get; set; }
    public bool TrackInventory { get; init; } = true;
    #endregion Timestamp

    #region Fashion
    public string? StyleCode { get; set; }
    public string? SeasonName { get; set; }
    public string? MaterialComposition { get; set; }
    public string? CareInstructions { get; set; }
    public string? FitNotes { get; set; }
    public string? Department { get; set; }
    public string? GenderTarget { get; set; }
    #endregion Fashion
}

public record ProductRequest : ProductParameters;

public record ProductDetailResponse : ProductParameters
{
    public Guid Id { get; init; }
    public Guid MasterVariantId { get; init; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
}

public record ProductListItemResponse : ProductParameters
{
    public Guid Id { get; init; }
    public Guid MasterVariantId { get; init; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public int VariantsCount { get; init; }
    public int ClassificationsCount { get; init; }
}
