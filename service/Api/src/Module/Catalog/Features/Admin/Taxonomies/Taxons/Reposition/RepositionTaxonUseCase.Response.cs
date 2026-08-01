namespace Module.Catalog.Features.Admin.Taxons.Reposition;

// EXCEPTION: minimal confirmation response — no domain entity
public sealed record Response
{
    public Guid Id { get; init; }
}