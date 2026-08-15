using Module.Catalog.Features.Admin.Variants.Images.Embeddings.Shared.Clients;
using Module.Catalog.Features.Storefront.Products.Images.Inferences.Shared.Mappings;

namespace Module.Catalog.Features.Storefront.Products.Images.Inferences.Get;

public static partial class GetVisualSearchModels
{
    public sealed record Query(QueryingParameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IInferenceClient inferenceClient)
        : IPagedQueryHandler<Query, Response>
    {
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var result = await inferenceClient.ListModelsAsync(cancellationToken);
            if (result.IsFailure)
                return result.Errors;

            var items = result.Value
                .Select(m => m.MapToVisualSearchModel<Response>())
                .ToList();

            return PagedResult<Response>.Ok(
                items: items,
                page: 1,
                pageSize: items.Count,
                totalCount: items.Count);
        }
    }
}
