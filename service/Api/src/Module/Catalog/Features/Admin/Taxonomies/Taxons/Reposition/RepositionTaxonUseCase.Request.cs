namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Reposition;

public sealed class Request
{
    public Guid? ParentId { get; set; }
    public int Position { get; set; }
}
