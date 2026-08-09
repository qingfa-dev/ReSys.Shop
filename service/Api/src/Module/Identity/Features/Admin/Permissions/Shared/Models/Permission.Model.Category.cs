namespace Module.Identity.Features.Shared.Admin.Permissions.Shared.Models;

public abstract record CategoryGroupListItemResponse<TResource>
{
    public string Category { get; init; } = default!;
    public string? Description { get; init; }
    public List<TResource> Resources { get; init; } = [];
}

public abstract record CategoryGroupListResponse<TCategory, TResource>
    where TCategory : CategoryGroupListItemResponse<TResource>
{
    public List<TCategory> Categories { get; set; } = [];
}