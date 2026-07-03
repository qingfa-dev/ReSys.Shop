using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Clients;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.AutoClassification;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.AutoClassification.Abstractions;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.Hierarchy;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.Hierarchy.Abstractions;

namespace Module.Catalog;

// @CAT-10 Boundary: Domain → Infrastructure — Catalog module entry point. Do not import persistence concerns.
// Boundary: Infrastructure → Catalog Module — AddCatalogModule is the single entry point;
//            do NOT register catalog services directly in other modules
public static class CatalogExtensions
{
    /// <summary>
    /// Adds the Catalog module services to the specified service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection with catalog services registered.</returns>
    public static IServiceCollection AddCatalogModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddInferenceClient(configuration);

        services.AddScoped<ITaxonHierarchyService, TaxonHierarchyService>();
        services.AddScoped<IAutoClassificationService, AutoClassificationService>();
        services.AddSingleton<ITaxonRuleEvaluator, TaxonRuleEvaluator>();

        return services;
    }
}
