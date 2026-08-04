using Microsoft.Extensions.DependencyInjection;

namespace Module.Catalog.Features.Storefront.Products.Shared.Services;

public static class VectorSearchServiceDependencyInjection
{
    public static IServiceCollection AddVectorSearchService(this IServiceCollection services)
    {
        services.AddScoped<IVectorSearchService, VectorSearchService>();
        return services;
    }
}
