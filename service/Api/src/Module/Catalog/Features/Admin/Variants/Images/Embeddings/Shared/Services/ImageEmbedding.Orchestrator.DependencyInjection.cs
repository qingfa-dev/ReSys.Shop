using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Module.Catalog.Features.Admin.Variants.Images.Embeddings.Shared.Services;

public static class EmbeddingOrchestratorDependencyInjection
{
    public static IServiceCollection AddEmbeddingOrchestrator(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(EmbeddingOrchestratorOptions.SectionName);
        services.Configure<EmbeddingOrchestratorOptions>(section);
        services.AddScoped<IEmbeddingOrchestrator, EmbeddingOrchestrator>();
        return services;
    }
}