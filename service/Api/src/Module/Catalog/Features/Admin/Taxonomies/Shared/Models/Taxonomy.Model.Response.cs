namespace Module.Catalog.Features.Admin.Taxonomies.Shared.Models;

public record TaxonomyListItemResponse : TaxonomyParameters, IResponse
{
    public Guid Id { get; init; }

    // Stats:
    public int TaxonsCount { get; init; }
    public DateTimeOffset? CreatedAtUtc { get; init; }
    public DateTimeOffset? ModifiedAtUtc { get; init; }
}

public record TaxonomyDetailResponse : TaxonomyParameters, IResponse
{
    public Guid Id { get; init; }

    public DateTimeOffset? CreatedAtUtc { get; init; }
    public DateTimeOffset? ModifiedAtUtc { get; init; }
}