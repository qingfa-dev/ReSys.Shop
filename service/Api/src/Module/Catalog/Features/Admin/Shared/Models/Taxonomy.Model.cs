namespace Module.Catalog.Features.Admin.Shared.Models;

public abstract record TaxonomyParameters : INamedParameters, ISortableParameters
{
    public string Name { get; init; } = string.Empty;
    public string? Presentation { get; init; } = string.Empty;
    public int Position { get; init; } = 0;
}

public record TaxonomyRequest : TaxonomyParameters;

public record TaxonomyListItemResponse : TaxonomyParameters
{
    public Guid Id { get; init; }

    // Stats:
    public int TaxonsCount { get; init; }
    public DateTimeOffset? CreatedAtUtc { get; init; }
    public DateTimeOffset? ModifiedAtUtc { get; init; }
}

public record TaxonomyDetailResponse : TaxonomyParameters
{
    public Guid Id { get; init; }

    public DateTimeOffset? CreatedAtUtc { get; init; }
    public DateTimeOffset? ModifiedAtUtc { get; init; }
}
