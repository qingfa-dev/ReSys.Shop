using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Domain.Products.Variants.Images.Embeddings;
using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Services;

namespace Module.Catalog.Persistence.Seeders;

public sealed class CatalogEmbeddingSeeder : AbstractDataSeeder
{
    private readonly DemoJsonHelper _helper;
    private readonly IConfiguration _configuration;
    private readonly IBackgroundJobClient? _backgroundJobClient;
    private readonly IEmbeddingOrchestrator? _orchestrator;

    public CatalogEmbeddingSeeder(
        IApplicationDbContext context,
        DemoJsonHelper helper,
        IConfiguration configuration,
        IBackgroundJobClient? backgroundJobClient = null,
        IEmbeddingOrchestrator? orchestrator = null) : base(context)
    {
        _helper = helper;
        _configuration = configuration;
        _backgroundJobClient = backgroundJobClient;
        _orchestrator = orchestrator;
    }

    public override int Order => 135;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var mode = _configuration.GetValue<string>("Seeders:EmbeddingMode") ?? "direct";

        switch (mode)
        {
            case "skip":
                return Result.Ok();
            case "job":
                return await SeedViaJobsAsync(cancellationToken);
            default:
                return await SeedFromJsonAsync(cancellationToken);
        }
    }

    private async Task<Result> SeedFromJsonAsync(CancellationToken cancellationToken)
    {
        var hasData = await HasDataAsync<ImageEmbedding>(cancellationToken);
        if (hasData)
            return Result.Ok();

        var json = _helper.LoadIfExists<DemoEmbeddingJson>("demo_embeddings.json");
        if (json is null)
            return Result.Ok();

        foreach (var e in json)
        {
            var embedding = ImageEmbeddingMethod.Create(
                variantImageId: Guid.Parse(e.VariantImageId),
                modelName: e.ModelName,
                modelVersion: e.ModelVersion,
                vectorData: e.Vector);
            Context.Set<ImageEmbedding>().Add(embedding);
        }
        await Context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    private async Task<Result> SeedViaJobsAsync(CancellationToken cancellationToken)
    {
        var hasData = await HasDataAsync<ImageEmbedding>(cancellationToken);
        if (hasData)
            return Result.Ok();

        var images = await Context.Set<VariantImage>()
            .Where(i => i.Type == VariantImageType.Search && i.VariantId != null)
            .ToListAsync(cancellationToken);

        if (images.Count == 0)
            return Result.Ok();

        foreach (var image in images)
        {
            _backgroundJobClient?.Enqueue<IEmbeddingOrchestrator>(
                orchestrator => orchestrator.GenerateAndPersistAsync(image.Id, VariantImageConstant.Defaults.DefaultEmbeddingModel, CancellationToken.None));
        }

        return Result.Ok();
    }

    private record DemoEmbeddingJson
    {
        public string VariantImageId { get; init; } = default!;
        public string ModelName { get; init; } = default!;
        public string ModelVersion { get; init; } = default!;
        public float[] Vector { get; init; } = default!;
        public int Dimensions { get; init; }
    }
}
