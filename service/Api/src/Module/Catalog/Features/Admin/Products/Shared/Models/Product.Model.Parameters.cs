using Module.Catalog.Domain.Products;

namespace Module.Catalog.Features.Admin.Products.Shared.Models;

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