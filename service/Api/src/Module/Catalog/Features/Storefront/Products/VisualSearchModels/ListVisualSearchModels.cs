using Module.Catalog.Features.Admin.Variants.Images.Embeddings.Shared.Clients;

namespace Module.Catalog.Features.Storefront.Products.VisualSearchModels;

public static partial class ListVisualSearchModels
{
    public sealed record Query : IRequest<Result<Response>>;

    public sealed record Response
    {
        public IReadOnlyList<ModelItem> Models { get; init; } = [];
    }

    public sealed record ModelItem
    {
        public string Id { get; init; } = "";
        public string Name { get; init; } = "";
        public string? Description { get; init; }
        public int Dimension { get; init; }
        public bool IsOnnx { get; init; }
    }

    internal sealed class Handler(IInferenceClient inferenceClient) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query query, CancellationToken ct)
        {
            var result = await inferenceClient.ListModelsAsync(ct);

            if (result.IsFailure)
                return Result<Response>.Unexpected(errors: result.Errors);

            var models = result.Value
                .Where(m => m.Id != "onnx")
                .Select(m => new ModelItem
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    Dimension = m.Dimension,
                    IsOnnx = m.IsOnnx,
                })
                .ToList();

            return Result<Response>.Ok(new Response { Models = models });
        }
    }
}
