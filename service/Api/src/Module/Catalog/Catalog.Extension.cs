using Microsoft.Extensions.DependencyInjection;

using Module.Catalog.Features.Admin.Variants.Images.Embeddings.Shared.Clients;
using Module.Catalog.Features.Admin.Variants.Images.Embeddings.Shared.Services;
using Module.Catalog.Features.Admin.Taxons.Services.AutoClassification;
using Module.Catalog.Features.Admin.Taxons.Services.AutoClassification.Abstractions;
using Module.Catalog.Features.Admin.Taxons.Services.Hierarchy;
using Module.Catalog.Features.Admin.Taxons.Services.Hierarchy.Abstractions;
using Module.Catalog.Features.Storefront.Products.Shared.Services;
using Module.Catalog.Persistence.Seeders;

namespace Module.Catalog;

// @CAT-10 Boundary: Domain → Infrastructure — Catalog module entry point. Do not import persistence concerns.
// Boundary: Infrastructure → Catalog Module — AddCatalogModule is the single entry point;
//            do NOT register catalog services directly in other modules
public static class CatalogExtensions
{
    /// <summary>
    /// Adds the Catalog module services to the specified service collection.
    /// </summary>
    public static WebApplicationBuilder AddCatalogModule(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddInferenceClient(builder.Configuration);
        builder.Services.AddEmbeddingOrchestrator(builder.Configuration);

        builder.Services.AddScoped<ITaxonHierarchyService, TaxonHierarchyService>();
        builder.Services.AddScoped<IAutoClassificationService, AutoClassificationService>();
        builder.Services.AddSingleton<ITaxonRuleEvaluator, TaxonRuleEvaluator>();

        builder.AddSeeder<CatalogOptionTypeSeeder>();
        builder.AddSeeder<CatalogOptionValueSeeder>();
        builder.AddSeeder<CatalogTaxonomySeeder>();
        builder.AddSeeder<CatalogTaxonSeeder>();
        builder.AddSeeder<CatalogProductSeeder>();
        builder.AddSeeder<CatalogVariantSeeder>();
        builder.AddSeeder<CatalogVariantImageSeeder>();
        builder.AddSeeder<CatalogProductTaxonSeeder>();
        builder.AddSeeder<CatalogEmbeddingSeeder>();

        builder.Services.AddScoped<DemoJsonHelper>();
        builder.Services.AddVectorSearchService();

        return builder;
    }
}