# Storefront Product List + Detail Merge — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Merge Availability into Product List + Detail endpoints, enrich responses with master variant, option values, stock info, and taxons inline.

**Architecture:** Backend: extend C# response models with `StoreVariantOptionValueResponse`, `StoreVariantStockInfo`, `MasterVariant`, `Taxons`; update mappings; inject `IStockAvailabilityCalculator` into List and Detail handlers for batch stock lookup; delete standalone Availability endpoint. Frontend: update TypeScript types, remove availability API, update ProductCard/ProductOptions/ProductDetailView to use new shapes.

**Tech Stack:** .NET 10 (C#), EF Core, MediatR, Carter, Mapster, Vue 3, TypeScript, PrimeVue

## Global Constraints

- `TreatWarningsAsErrors=true` globally — any warning fails build
- Vertical slice feature files — every C# feature action is `static partial class` split across files
- Modules must not cross-reference — communication via MediatR `ISender` only
- Result objects, not exceptions — all domain operations return `Result<T>`
- Frontend follows Store SPA commenting standard — `// Label: Sentence.` format in script, `<!-- Section: Title — purpose -->` in template

---

## Task Structure

---

### Task 1: Add New C# Response Models

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/Shared/Models/Store.Variant.Model.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/Shared/Models/Store.Product.Model.cs`

**Interfaces:**
- Consumes: `VariantListItemResponse` (existing base class)
- Produces: `StoreVariantOptionValueResponse`, `StoreVariantStockInfo`, `StoreProductVariantResponse` (updated), `StoreProductListItemResponse` (updated), `StoreProductDetailResponse` (updated)

- [ ] **Step 1: Update Store.Variant.Model.cs**

Replace the entire file content with:

```csharp
using Module.Catalog.Features.Admin.Products.Variants.Shared.Models;
using Module.Catalog.Features.Storefront.Options.Shared.Models;

namespace Module.Catalog.Features.Storefront.Products.Shared.Models;

public record StoreVariantOptionValueResponse
{
    public Guid VariantOptionValueId { get; init; }
    public Guid OptionValueId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Presentation { get; init; }
    public int Position { get; init; }
    public Guid OptionTypeId { get; init; }
    public string? OptionTypeName { get; init; }
}

public record StoreVariantStockInfo
{
    public string Status { get; init; } = "unknown";
    public int AvailableQuantity { get; init; }
    public bool Backorderable { get; init; }
}

public record StoreProductVariantResponse : VariantListItemResponse
{
    public List<StoreVariantOptionValueResponse> OptionValues { get; init; } = [];
    public List<StoreVariantImageResponse> Images { get; init; } = [];
    public List<StoreVariantPriceResponse> Prices { get; init; } = [];
    public StoreVariantStockInfo Stock { get; init; } = new();
}
```

- [ ] **Step 2: Update Store.Product.Model.cs**

Replace the entire file content with:

```csharp
using Module.Catalog.Features.Admin.Products.Shared.Models;

namespace Module.Catalog.Features.Storefront.Products.Shared.Models;

public record StoreProductListItemResponse : ProductListItemResponse
{
    public StoreProductVariantResponse? MasterVariant { get; init; }
    public List<StoreProductTaxonResponse> Taxons { get; init; } = [];
}

public record StoreProductDetailResponse : ProductDetailResponse
{
    public StoreProductVariantResponse? MasterVariant { get; init; }
    public List<StoreProductVariantResponse> Variants { get; init; } = [];
    public List<StoreProductTaxonResponse> Taxons { get; init; } = [];
}
```

- [ ] **Step 3: Verify build passes**

Run: `dotnet build service/Api/src/Module/`
Expected: PASS (may have warnings about unused imports, but no errors)

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Storefront/Products/Shared/Models/
git commit -m "feat(catalog): add StoreVariantOptionValueResponse, StoreVariantStockInfo, update product response models"
```

---

### Task 2: Update Variant Mapping

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/Shared/Mappings/Store.Variant.Mapping.cs`

**Interfaces:**
- Consumes: `OptionValueVariant` entity with `OptionValue` → `OptionType` included
- Produces: `StoreVariantOptionValueResponse` list on each variant

- [ ] **Step 1: Update Store.Variant.Mapping.cs**

Replace the `MapToStoreVariant` method to use `OptionValues` list with full model data:

```csharp
using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Features.Storefront.Products.Shared.Models;

namespace Module.Catalog.Features.Storefront.Products.Shared.Mappings;

public static class StoreProductVariantMapping
{
    public static StoreProductVariantResponse MapToStoreVariant(this Variant variant)
    {
        var firstPrice = variant.Prices.FirstOrDefault();

        var optionValues = variant.OptionValueVariants
            .Where(ov => ov.OptionValue is not null)
            .OrderBy(ov => ov.OptionValue!.OptionType?.Position)
            .Select(ov => new StoreVariantOptionValueResponse
            {
                VariantOptionValueId = ov.Id,
                OptionValueId = ov.OptionValueId,
                Name = ov.OptionValue!.Name,
                Presentation = ov.OptionValue.Presentation,
                Position = ov.OptionValue.Position,
                OptionTypeId = ov.OptionValue.OptionTypeId,
                OptionTypeName = ov.OptionValue.OptionType?.Name,
            })
            .ToList();

        return new StoreProductVariantResponse
        {
            Id = variant.Id,
            Sku = variant.Sku,
            IsMaster = variant.IsMaster,
            Price = firstPrice?.Amount,
            Currency = firstPrice?.Currency,
            OptionValues = optionValues,
            Images = variant.VariantImages
                .OrderBy(i => i.Position)
                .Select(i => i.MapToStoreImage())
                .ToList(),
        };
    }

    public static StoreVariantImageResponse MapToStoreImage(this VariantImage image)
    {
        return new StoreVariantImageResponse
        {
            Id = image.Id,
            Url = image.Url,
            Alt = image.Alt,
            Position = image.Position,
            ContentType = image.ContentType,
        };
    }
}
```

- [ ] **Step 2: Verify build passes**

Run: `dotnet build service/Api/src/Module/`
Expected: PASS

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Storefront/Products/Shared/Mappings/Store.Variant.Mapping.cs
git commit -m "feat(catalog): update variant mapping to use StoreVariantOptionValueResponse with join table ID"
```

---

### Task 3: Update Product Mapping (List Item)

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/Shared/Mappings/Store.Product.Mapping.cs`

**Interfaces:**
- Consumes: `StoreProductVariantResponse` (from Task 2), `StoreProductTaxonResponse`
- Produces: `MasterVariant` and `Taxons` on `StoreProductListItemResponse`

- [ ] **Step 1: Update MapToStoreListItem to populate MasterVariant and Taxons**

In `Store.Product.Mapping.cs`, update `MapToStoreListItem` to include `MasterVariant` and `Taxons`:

```csharp
using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Features.Storefront.Products.Shared.Models;

namespace Module.Catalog.Features.Storefront.Products.Shared.Mappings;

public static class StoreProductMapping
{
    public static T MapToStoreDetail<T>(this Product entity) where T : StoreProductDetailResponse, new()
    {
        var masterVariant = entity.Variants.FirstOrDefault(v => v.IsMaster);

        var response = new T
        {
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            Slug = entity.Slug ?? string.Empty,
            Description = entity.Description,
            MetaTitle = entity.MetaTitle,
            MetaDescription = entity.MetaDescription,
            MetaKeywords = entity.MetaKeywords,
            AvailableOn = entity.AvailableOn,
            DiscontinueOn = entity.DiscontinueOn,
            MasterVariantId = entity.MasterVariantId,
            MasterVariant = masterVariant?.MapToStoreVariant(),
            Variants = entity.Variants
                .Where(v => !v.IsDeleted)
                .Select(v => v.MapToStoreVariant())
                .ToList(),
            Taxons = entity.Classifications
                .Select(c => new StoreProductTaxonResponse
                {
                    Id = c.TaxonId == null ? Guid.Empty : c.TaxonId.Value,
                    Name = c.Taxon?.Name ?? string.Empty,
                    Permalink = c.Taxon?.Permalink ?? string.Empty,
                    Depth = c.Taxon?.Depth ?? 0,
                })
                .ToList(),
        };

        return response;
    }

    public static T MapToStoreListItem<T>(this Product entity) where T : StoreProductListItemResponse, new()
    {
        var masterVariant = entity.Variants.FirstOrDefault(v => v.IsMaster);
        var firstPrice = masterVariant?.Prices.FirstOrDefault();
        var firstImage = masterVariant?.VariantImages
            .Where(i => i.Type == VariantImageType.Default || i.Type == VariantImageType.Thumbnail)
            .MinBy(i => i.Position);

        return new T
        {
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            Description = entity.Description,
            Status = entity.Status,
            Slug = entity.Slug ?? string.Empty,
            MetaTitle = entity.MetaTitle,
            MetaDescription = entity.MetaDescription,
            MetaKeywords = entity.MetaKeywords,
            AvailableOn = entity.AvailableOn,
            DiscontinueOn = entity.DiscontinueOn,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            MakeActiveAt = entity.MakeActiveAt,
            CreatedAtUtc = entity.CreatedAtUtc,
            MasterVariantId = entity.MasterVariantId,
            StyleCode = entity.StyleCode,
            SeasonName = entity.SeasonName,
            MaterialComposition = entity.MaterialComposition,
            CareInstructions = entity.CareInstructions,
            FitNotes = entity.FitNotes,
            Department = entity.Department,
            GenderTarget = entity.GenderTarget,
            MinPrice = firstPrice?.Amount,
            Currency = firstPrice?.Currency,
            ThumbnailUrl = firstImage?.Url,
            ThumbnailAlt = firstImage?.Alt,
            VariantsCount = entity.Variants.Count(v => !v.IsDeleted),
            ClassificationsCount = entity.Classifications.Count,
            TrackInventory = masterVariant?.TrackInventory ?? false,
            MasterVariant = masterVariant?.MapToStoreVariant(),
            Taxons = entity.Classifications
                .Select(c => new StoreProductTaxonResponse
                {
                    Id = c.TaxonId == null ? Guid.Empty : c.TaxonId.Value,
                    Name = c.Taxon?.Name ?? string.Empty,
                    Permalink = c.Taxon?.Permalink ?? string.Empty,
                    Depth = c.Taxon?.Depth ?? 0,
                })
                .ToList(),
        };
    }
}
```

- [ ] **Step 2: Verify build passes**

Run: `dotnet build service/Api/src/Module/`
Expected: PASS

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Storefront/Products/Shared/Mappings/Store.Product.Mapping.cs
git commit -m "feat(catalog): add MasterVariant and Taxons to product list item mapping"
```

---

### Task 4: Update GetStorefrontProducts Handler (Stock + Taxon Breadcrumbs)

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/List/GetStorefrontProducts.cs`

**Interfaces:**
- Consumes: `IStockAvailabilityCalculator` (from Inventory module), `StoreProductListItemResponse` (from Task 1)
- Produces: List items with `MasterVariant.Stock` populated, `Taxons` with breadcrumbs

- [ ] **Step 1: Inject IStockAvailabilityCalculator and add stock + breadcrumb logic**

In `GetStorefrontProducts.cs`, update the `PagedQueryHandler` constructor and `Handle` method:

```csharp
using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Storefront.Products.Shared.Mappings;
using Module.Catalog.Features.Storefront.Products.Shared.Models;
using Module.Inventory.Services;

namespace Module.Catalog.Features.Storefront.Products.Get.List;

public static partial class GetStorefrontProducts
{
    public sealed record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(
        IApplicationDbContext dbContext,
        IStockAvailabilityCalculator calculator)
        : IPagedQueryHandler<Query, Response>
    {
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var parameters = request.Parameters;

            var query = dbContext.Set<Product>()
                .Include(x => x.Variants)
                    .ThenInclude(v => v.Prices)
                .Include(x => x.Variants)
                    .ThenInclude(v => v.VariantImages)
                .Include(x => x.Variants)
                    .ThenInclude(v => v.OptionValueVariants)
                        .ThenInclude(ov => ov.OptionValue!)
                            .ThenInclude(o => o.OptionType!)
                .Include(x => x.Classifications)
                    .ThenInclude(c => c.Taxon)
                .Where(x => !x.IsDeleted && x.AvailableOn <= DateTimeOffset.UtcNow)
                .AsNoTracking();

            if (parameters.OptionValueId is { Length: > 0 })
            {
                var optionValueIds = parameters.OptionValueId;
                query = query.Where(p => p.Variants.Any(v =>
                    v.OptionValueVariants.Any(ov =>
                        ov.OptionValue != null && optionValueIds.Contains(ov.OptionValue.Id))));
            }

            if (parameters.TaxonId is { Length: > 0 })
            {
                var taxonIds = parameters.TaxonId;
                query = query.Where(p => p.Classifications.Any(c =>
                    c.Taxon != null && taxonIds.Contains(c.Taxon.Id)));
            }

            if (parameters.MinPrice.HasValue)
            {
                var minPrice = parameters.MinPrice.Value;
                query = query.Where(p => p.Variants.Any(v =>
                    v.Prices.Any(pr => pr.Amount >= minPrice)));
            }

            if (parameters.MaxPrice.HasValue)
            {
                var maxPrice = parameters.MaxPrice.Value;
                query = query.Where(p => p.Variants.Any(v =>
                    v.Prices.Any(pr => pr.Amount <= maxPrice)));
            }

            var parsing = parameters.ParseAll(
                allowedFilterFields: ProductConstant.Query.AllowedFilterFields,
                allowedSearchFields: ProductConstant.Query.AllowedSearchFields,
                allowedSortFields: ProductConstant.Query.AllowedSortFields);
            if (parsing.IsFailure)
                return parsing.Errors;

            FacetAggregate? facets = null;
            if (parameters.IncludeFacets)
            {
                var productIds = await query
                    .Select(p => p.Id)
                    .ToListAsync(cancellationToken);

                var optionValueCounts = await dbContext.Set<Product>()
                    .Where(p => productIds.Contains(p.Id))
                    .SelectMany(p => p.Variants)
                    .SelectMany(v => v.OptionValueVariants)
                    .Where(ov => ov.OptionValue != null && ov.OptionValue.OptionType != null)
                    .GroupBy(ov => new
                    {
                        ov.OptionValue!.OptionTypeId,
                        OptionTypeName = ov.OptionValue.OptionType.Name,
                        OptionTypePosition = ov.OptionValue.OptionType.Position,
                        OptionValueId = ov.OptionValue.Id,
                        OptionValueName = ov.OptionValue.Name,
                        OptionValuePosition = ov.OptionValue.Position
                    })
                    .Select(g => new
                    {
                        g.Key.OptionTypeId,
                        g.Key.OptionTypeName,
                        g.Key.OptionTypePosition,
                        g.Key.OptionValueId,
                        g.Key.OptionValueName,
                        g.Key.OptionValuePosition,
                        Count = g.Select(ov => ov.Variant!.ProductId).Distinct().Count()
                    })
                    .ToListAsync(cancellationToken);

                var optionValueGroups = optionValueCounts
                    .GroupBy(c => new { c.OptionTypeId, c.OptionTypeName, c.OptionTypePosition })
                    .Select(g => new FacetGroup(
                        g.Key.OptionTypeName,
                        g
                            .OrderBy(c => c.OptionValuePosition)
                            .Select(c => new FacetValue(
                                c.OptionValueId.ToString(),
                                c.OptionValueName,
                                c.Count,
                                parameters.OptionValueId?.Contains(c.OptionValueId) == true))
                            .ToList()))
                    .OrderBy(g => g.Values.FirstOrDefault()?.Id)
                    .ToList();

                var taxonCounts = await dbContext.Set<Product>()
                    .Where(p => productIds.Contains(p.Id))
                    .SelectMany(p => p.Classifications)
                    .Where(c => c.Taxon != null)
                    .GroupBy(c => new
                    {
                        c.Taxon!.Id,
                        c.Taxon.Name,
                        c.Taxon.Position
                    })
                    .Select(g => new
                    {
                        g.Key.Id,
                        g.Key.Name,
                        g.Key.Position,
                        Count = g.Count()
                    })
                    .ToListAsync(cancellationToken);

                if (taxonCounts.Count != 0)
                {
                    optionValueGroups.Add(new FacetGroup(
                        "Category",
                        taxonCounts
                            .OrderBy(c => c.Position)
                            .Select(c => new FacetValue(
                                c.Id.ToString(),
                                c.Name,
                                c.Count,
                                parameters.TaxonId?.Contains(c.Id) == true))
                            .ToList()));
                }

                facets = new FacetAggregate(optionValueGroups);
            }

            var pagedResult = await query
                .OrderByDescending(x => x.CreatedAtUtc)
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, x => x.MapToStoreListItem<Response>(), cancellationToken);

            // Batch: Load stock info for all master variants on this page
            var masterVariantIds = pagedResult.Items
                .Where(i => i.MasterVariant != null)
                .Select(i => i.MasterVariant!.Id)
                .Distinct()
                .ToList();

            var availableByVariant = await calculator.GetAvailableByVariantAsync(masterVariantIds, cancellationToken);
            var backorderableByVariant = await calculator.GetBackorderableByVariantAsync(masterVariantIds, cancellationToken);

            // Batch: Load all taxons once for breadcrumb computation
            var allTaxons = await dbContext.Set<Taxon>()
                .AsNoTracking()
                .ToListAsync(cancellationToken);
            var taxonLookup = allTaxons.ToDictionary(t => t.Id, t => t);

            // Attach: Stock info + taxon breadcrumbs to each list item
            var items = pagedResult.Items.Select(item =>
            {
                // Attach stock info to master variant
                if (item.MasterVariant is not null)
                {
                    var available = availableByVariant.GetValueOrDefault(item.MasterVariant.Id, 0);
                    var backorderable = backorderableByVariant.GetValueOrDefault(item.MasterVariant.Id, false);
                    item = item with
                    {
                        MasterVariant = item.MasterVariant with
                        {
                            Stock = ComputeStockInfo(available, backorderable)
                        }
                    };
                }

                // Attach taxon breadcrumbs
                var taxonsWithBreadcrumbs = item.Taxons.Select(t =>
                {
                    var taxonEntity = taxonLookup.GetValueOrDefault(t.Id);
                    if (taxonEntity is null) return t;

                    var breadcrumb = new List<TaxonBreadcrumbItem>();
                    Taxon? current = taxonEntity;
                    while (current is not null)
                    {
                        breadcrumb.Insert(0, new TaxonBreadcrumbItem(current.Id, current.Name, current.Permalink));
                        current = current.ParentId is not null && taxonLookup.TryGetValue(current.ParentId.Value, out var parent)
                            ? parent
                            : null;
                    }

                    return new StoreProductTaxonResponse
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Permalink = t.Permalink,
                        Depth = t.Depth,
                        Breadcrumb = breadcrumb
                    };
                }).ToList();

                return item with { Taxons = taxonsWithBreadcrumbs };
            }).ToList();

            if (facets is not null)
            {
                items = items.Select(item => (Response)item with { Facets = facets }).ToList();
            }

            return PagedResult<Response>.Create(items, pagedResult.PageNumber, pagedResult.PageSize, pagedResult.TotalCount);
        }

        private static StoreVariantStockInfo ComputeStockInfo(int available, bool backorderable)
        {
            var status = available switch
            {
                > 5 => "in_stock",
                > 0 => "low_stock",
                _ when backorderable => "backorderable",
                _ => "out_of_stock"
            };

            return new StoreVariantStockInfo
            {
                Status = status,
                AvailableQuantity = available,
                Backorderable = backorderable,
            };
        }
    }
}
```

- [ ] **Step 2: Verify build passes**

Run: `dotnet build service/Api/src/Module/`
Expected: PASS

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Storefront/Products/List/GetStorefrontProducts.cs
git commit -m "feat(catalog): inject IStockAvailabilityCalculator into list handler, batch stock lookup + taxon breadcrumbs"
```

---

### Task 5: Update GetProductDetail Handler (Stock Info)

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/Detail/GetProductDetail.cs`

**Interfaces:**
- Consumes: `IStockAvailabilityCalculator`, `StoreProductDetailResponse` (from Task 1)
- Produces: Detail response with `Stock` populated on each variant

- [ ] **Step 1: Inject IStockAvailabilityCalculator and add stock info logic**

In `GetProductDetail.cs`, update the `QueryHandler`:

```csharp
using Module.Catalog.Domain.Products;
using Module.Catalog.Features.Storefront.Products.Shared.Mappings;
using Module.Catalog.Features.Storefront.Products.Shared.Models;
using Module.Inventory.Services;

namespace Module.Catalog.Features.Storefront.Products.Get.Detail;

public static partial class GetProductDetail
{
    public sealed record Query(string Slug) : IQuery<Response>;

    public record Response : StoreProductDetailResponse;

    public sealed class QueryHandler(
        IApplicationDbContext dbContext,
        IStockAvailabilityCalculator calculator,
        ILogger<QueryHandler> logger) : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var entity = await dbContext.Set<Product>()
                .Include(x => x.Variants)
                    .ThenInclude(v => v.Prices)
                .Include(x => x.Variants)
                    .ThenInclude(v => v.VariantImages)
                .Include(x => x.Variants)
                    .ThenInclude(v => v.OptionValueVariants)
                        .ThenInclude(ov => ov.OptionValue!)
                            .ThenInclude(o => o.OptionType!)
                .Include(x => x.Classifications)
                    .ThenInclude(c => c.Taxon)
                .FirstOrDefaultAsync(x => x.Slug == query.Slug
                    && !x.IsDeleted
                    && x.AvailableOn <= DateTimeOffset.UtcNow, cancellationToken);

            if (entity is null)
            {
                ProductLoggers.StorefrontProductNotFoundBySlug(logger, query.Slug);
                return ProductResult.Errors.NotFoundBySlug(query.Slug);
            }

            ProductLoggers.StorefrontProductDetailLoaded(logger, query.Slug, entity.Id);

            var response = entity.MapToStoreDetail<Response>();

            // Batch: Load stock info for all variants
            var variantIds = response.Variants.Select(v => v.Id).Distinct().ToList();
            var availableByVariant = await calculator.GetAvailableByVariantAsync(variantIds, cancellationToken);
            var backorderableByVariant = await calculator.GetBackorderableByVariantAsync(variantIds, cancellationToken);

            // Attach: Stock info to each variant
            for (int i = 0; i < response.Variants.Count; i++)
            {
                var variant = response.Variants[i];
                var available = availableByVariant.GetValueOrDefault(variant.Id, 0);
                var backorderable = backorderableByVariant.GetValueOrDefault(variant.Id, false);
                response.Variants[i] = variant with
                {
                    Stock = ComputeStockInfo(available, backorderable)
                };
            }

            // Attach: Taxon breadcrumbs
            var taxons = await dbContext.Set<Module.Catalog.Domain.Taxonomies.Taxons.Taxon>()
                .AsNoTracking()
                .ToListAsync(cancellationToken);
            var taxonLookup = taxons.ToDictionary(t => t.Id, t => t);

            for (int i = 0; i < response.Taxons.Count; i++)
            {
                var taxon = taxonLookup.GetValueOrDefault(response.Taxons[i].Id);
                if (taxon is null)
                    continue;

                var breadcrumb = new List<TaxonBreadcrumbItem>();
                Module.Catalog.Domain.Taxonomies.Taxons.Taxon? current = taxon;
                while (current is not null)
                {
                    breadcrumb.Insert(0, new TaxonBreadcrumbItem(current.Id, current.Name, current.Permalink));
                    current = current.ParentId is not null && taxonLookup.TryGetValue(current.ParentId.Value, out var parent)
                        ? parent
                        : null;
                }

                response.Taxons[i] = new StoreProductTaxonResponse
                {
                    Id = taxon.Id,
                    Name = taxon.Name,
                    Permalink = taxon.Permalink,
                    Depth = taxon.Depth,
                    Breadcrumb = breadcrumb
                };
            }

            return response;
        }

        private static StoreVariantStockInfo ComputeStockInfo(int available, bool backorderable)
        {
            var status = available switch
            {
                > 5 => "in_stock",
                > 0 => "low_stock",
                _ when backorderable => "backorderable",
                _ => "out_of_stock"
            };

            return new StoreVariantStockInfo
            {
                Status = status,
                AvailableQuantity = available,
                Backorderable = backorderable,
            };
        }
    }
}
```

- [ ] **Step 2: Verify build passes**

Run: `dotnet build service/Api/src/Module/`
Expected: PASS

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Storefront/Products/Detail/GetProductDetail.cs
git commit -m "feat(catalog): inject IStockAvailabilityCalculator into detail handler, batch stock lookup per variant"
```

---

### Task 6: Delete Availability Feature + Remove Constants

**Files:**
- Delete: `service/Api/src/Module/Catalog/Features/Storefront/Products/Availability/GetAvailability.cs`
- Delete: `service/Api/src/Module/Catalog/Features/Storefront/Products/Availability/GetAvailability.Endpoint.cs`
- Delete: `service/Api/src/Module/Catalog/Features/Storefront/Products/Availability/GetAvailability.Response.cs`
- Delete: `service/Api/src/Module/Catalog/Features/Storefront/Products/Shared/Models/Store.Availability.Model.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Storefront.cs`

**Interfaces:**
- Consumes: Nothing (deletion)
- Produces: Availability endpoint returns 404, unused models removed

- [ ] **Step 1: Delete Availability feature files**

```bash
rm -f service/Api/src/Module/Catalog/Features/Storefront/Products/Availability/GetAvailability.cs
rm -f service/Api/src/Module/Catalog/Features/Storefront/Products/Availability/GetAvailability.Endpoint.cs
rm -f service/Api/src/Module/Catalog/Features/Storefront/Products/Availability/GetAvailability.Response.cs
rm -f service/Api/src/Module/Catalog/Features/Storefront/Products/Shared/Models/Store.Availability.Model.cs
```

- [ ] **Step 2: Remove Availability constants from CatalogFeature.Storefront.cs**

In `CatalogFeature.Storefront.cs`, remove the `Availability` static class (lines 20-25):

```csharp
// DELETE these lines:
public static class Availability
{
    public const string Route = $"{BaseRoute}/availability";
    public const string Description = "Retrieve style matrix availability grid for a product (productId query)";
    public const string Summary = "Get product availability";
}
```

- [ ] **Step 3: Verify build passes**

Run: `dotnet build service/Api/src/Module/`
Expected: PASS

- [ ] **Step 4: Commit**

```bash
git add -A service/Api/src/Module/Catalog/Features/Storefront/Products/Availability/
git add service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Storefront.cs
git commit -m "feat(catalog): delete standalone Availability endpoint and models, remove constants"
```

---

### Task 7: Update Frontend Types

**Files:**
- Modify: `app/Store/src/features/catalog/types/product.ts`

**Interfaces:**
- Consumes: Backend response shapes (from Tasks 1-5)
- Produces: `StoreVariantStockInfo`, `StoreVariantOptionValueResponse`, updated `StoreProductVariantResponse`, updated `StoreProductListItemResponse`, updated `StoreProductDetailResponse`

- [ ] **Step 1: Update product.ts types**

Replace the entire file content with:

```typescript
export interface StoreProductListItemResponse {
  id: string
  masterVariantId: string
  name: string
  status: string
  description: string | null
  slug: string
  minPrice: number | null
  currency: string | null
  thumbnailUrl: string | null
  thumbnailAlt: string | null
  styleCode: string | null
  seasonName: string | null
  materialComposition: string | null
  careInstructions: string | null
  fitNotes: string | null
  department: string | null
  genderTarget: string | null
  variantsCount: number
  availableOn: string | null
  masterVariant: StoreProductVariantResponse | null
  taxons: StoreProductTaxonResponse[]
}

export interface StoreProductDetailResponse extends StoreProductListItemResponse {
  masterVariant: StoreProductVariantResponse | null
  variants: StoreProductVariantResponse[]
  taxons: StoreProductTaxonResponse[]
}

export interface StoreProductTaxonResponse {
  id: string
  name: string
  permalink: string
  depth: number
  breadcrumb?: Array<{ id: string; name: string; permalink: string }>
}

export interface StoreVariantStockInfo {
  status: 'in_stock' | 'low_stock' | 'backorderable' | 'out_of_stock' | 'unknown'
  availableQuantity: number
  backorderable: boolean
}

export interface StoreVariantOptionValueResponse {
  variantOptionValueId: string
  optionValueId: string
  name: string
  presentation: string | null
  position: number
  optionTypeId: string
  optionTypeName: string | null
}

export interface StoreProductVariantResponse {
  id: string
  sku: string | null
  isMaster: boolean
  price: number | null
  currency: string | null
  optionValues: StoreVariantOptionValueResponse[]
  images: StoreProductImageResponse[]
  stock: StoreVariantStockInfo
}

export interface StoreProductImageResponse {
  id: string
  url: string
  alt: string | null
  position: number
}
```

- [ ] **Step 2: Verify lint passes**

Run: `cd app/Store && pnpm run lint`
Expected: PASS

- [ ] **Step 3: Commit**

```bash
git add app/Store/src/features/catalog/types/product.ts
git commit -m "feat(store): update product types with StoreVariantStockInfo, StoreVariantOptionValueResponse, remove availability types"
```

---

### Task 8: Update Frontend API Layer

**Files:**
- Modify: `app/Store/src/features/catalog/services/productApi.ts`
- Modify: `app/Store/src/shared/constants/api.ts`

**Interfaces:**
- Consumes: Updated types from Task 7
- Produces: Clean API layer without availability references

- [ ] **Step 1: Update productApi.ts — remove getAvailability**

In `productApi.ts`, remove the `getAvailability` function and `AvailabilityMatrixResponse` import:

```typescript
import { get } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types/result'
import type { QueryingParameters } from '@/shared/types/querying'
import { getPaged } from '@/shared/api/paged'
import type {
  StoreProductListItemResponse,
  StoreProductDetailResponse,
} from '../types/product'

export function getPagedProducts(params: QueryingParameters): Promise<PagedResult<StoreProductListItemResponse>> {
  return getPaged<StoreProductListItemResponse>(ENDPOINTS.products, params)
}

export function getProductBySlug(slug: string): Promise<Result<StoreProductDetailResponse>> {
  return get<Result<StoreProductDetailResponse>>(ENDPOINTS.productBySlug(slug))
}

export function getSimilarProducts(productId: string, topK = 20): Promise<PagedResult<StoreProductListItemResponse>> {
  return getPaged<StoreProductListItemResponse>(
    `${ENDPOINTS.productSimilar}?productId=${productId}&topK=${topK}`,
    { pageNumber: 1, pageSize: topK },
  )
}

export function getRelatedProducts(productId: string, params: QueryingParameters): Promise<PagedResult<StoreProductListItemResponse>> {
  return getPaged<StoreProductListItemResponse>(
    `${ENDPOINTS.productRelated}?productId=${productId}`,
    params,
  )
}
```

- [ ] **Step 2: Update api.ts — remove productAvailability endpoint**

In `api.ts`, remove the `productAvailability` line:

```typescript
// DELETE this line:
productAvailability: `${API_STOREFRONT}/products/availability`,
```

- [ ] **Step 3: Verify lint passes**

Run: `cd app/Store && pnpm run lint`
Expected: PASS

- [ ] **Step 4: Commit**

```bash
git add app/Store/src/features/catalog/services/productApi.ts app/Store/src/shared/constants/api.ts
git commit -m "feat(store): remove getAvailability API function and productAvailability endpoint constant"
```

---

### Task 9: Update ProductCard.vue — Show Stock Badge

**Files:**
- Modify: `app/Store/src/features/catalog/components/ProductCard.vue`

**Interfaces:**
- Consumes: `StoreProductListItemResponse` with `masterVariant.stock` (from Task 7)
- Produces: Stock badge display on product cards

- [ ] **Step 1: Update ProductCard.vue to show stock badge**

In `ProductCard.vue`, add stock badge below the price:

```vue
<script setup lang="ts">
import { computed } from 'vue'
import type { StoreProductListItemResponse } from '../types/product'
import { formatVnd } from '@/shared/utils/currency'
import ProductBadge from './ProductBadge.vue'

const props = defineProps<{ product: StoreProductListItemResponse; loading?: boolean }>()
const emit = defineEmits<{ addToCart: [variantId: string] }>()

function displayPrice(): string {
  return props.product.minPrice != null ? formatVnd(props.product.minPrice) : 'Contact'
}

const isNew = computed(() => {
  if (!props.product.availableOn) return false
  const diff = Date.now() - new Date(props.product.availableOn).getTime()
  return diff >= 0 && diff <= 14 * 24 * 60 * 60 * 1000
})

const stockStatus = computed(() => props.product.masterVariant?.stock?.status ?? 'unknown')

const stockLabel = computed(() => {
  const stock = props.product.masterVariant?.stock
  if (!stock) return null
  if (stock.status === 'low_stock') return `Only ${stock.availableQuantity} left`
  if (stock.status === 'out_of_stock') return stock.backorderable ? 'Available for backorder' : 'Out of stock'
  return null
})

const stockColor = computed(() => {
  switch (stockStatus.value) {
    case 'in_stock': return 'text-emerald-600'
    case 'low_stock': return 'text-amber-600'
    case 'backorderable': return 'text-blue-600'
    case 'out_of_stock': return 'text-red-500'
    default: return 'text-stone-400'
  }
})
</script>
<template>
  <!-- Section: Product Card -->
  <div class="group bg-white rounded-xl border border-stone-200 overflow-hidden shadow-sm hover:shadow-md transition-shadow">
    <!-- Section: Thumbnail -->
    <div class="relative">
      <ProductBadge v-if="isNew" variant="new" />
      <router-link :to="`/products/${product.slug}`" class="block aspect-square bg-stone-100 relative overflow-hidden">
          <img
            v-if="product.thumbnailUrl"
            :src="product.thumbnailUrl"
            :alt="product.thumbnailAlt ?? product.name"
            class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-200"
          />
          <div v-else class="w-full h-full flex items-center justify-center text-stone-400">
            <i class="pi pi-image text-4xl" />
          </div>
          <!-- Section: Quick Add Overlay -->
          <div class="absolute inset-x-0 bottom-0 p-3 bg-gradient-to-t from-black/60 to-transparent opacity-0 group-hover:opacity-100 transition-opacity">
            <Button
              label="Quick Add"
              icon="pi pi-plus"
              size="small"
              class="w-full"
              :loading="loading"
              :disabled="loading"
              @click.prevent="emit('addToCart', product.masterVariantId)"
            />
          </div>
        </router-link>
      </div>
    <!-- Section: Product Info -->
    <div class="p-4">
      <router-link :to="`/products/${product.slug}`" class="text-sm font-medium text-stone-900 line-clamp-2 hover:text-stone-600">
        {{ product.name }}
      </router-link>
      <p class="mt-1 text-lg font-bold text-stone-900">{{ displayPrice() }}</p>
      <p v-if="stockLabel" class="mt-1 text-xs font-medium" :class="stockColor">{{ stockLabel }}</p>
    </div>
  </div>
</template>
```

- [ ] **Step 2: Verify lint passes**

Run: `cd app/Store && pnpm run lint`
Expected: PASS

- [ ] **Step 3: Commit**

```bash
git add app/Store/src/features/catalog/components/ProductCard.vue
git commit -m "feat(store): add stock badge to ProductCard using masterVariant.stock"
```

---

### Task 10: Update ProductOptions.vue — Use optionValues Array

**Files:**
- Modify: `app/Store/src/features/catalog/components/ProductOptions.vue`

**Interfaces:**
- Consumes: `StoreProductVariantResponse` with `optionValues` array (from Task 7)
- Produces: Option buttons rendered from `optionValues`, stock status shown per option

- [ ] **Step 1: Update ProductOptions.vue to use optionValues array**

Replace the entire file content with:

```vue
<script setup lang="ts">
import { computed } from 'vue'
import type { StoreProductVariantResponse } from '../types/product'

const props = defineProps<{
  variants: StoreProductVariantResponse[]
  modelValue: string | null
}>()
const emit = defineEmits<{ 'update:modelValue': [id: string] }>()

interface OptionDimension {
  optionTypeId: string
  optionTypeName: string
  values: Array<{ id: string; name: string; variantIds: string[] }>
}

// Map: Distinct option dimensions derived from the variant list using optionValues array
const dimensions = computed<OptionDimension[]>(() => {
  const dimMap = new Map<string, OptionDimension>()

  for (const variant of props.variants) {
    for (const ov of variant.optionValues) {
      if (!dimMap.has(ov.optionTypeId)) {
        dimMap.set(ov.optionTypeId, {
          optionTypeId: ov.optionTypeId,
          optionTypeName: ov.optionTypeName ?? 'Option',
          values: [],
        })
      }
      const dim = dimMap.get(ov.optionTypeId)!
      const existing = dim.values.find(v => v.id === ov.optionValueId)
      if (existing) {
        existing.variantIds.push(variant.id)
      } else {
        dim.values.push({
          id: ov.optionValueId,
          name: ov.presentation ?? ov.name,
          variantIds: [variant.id],
        })
      }
    }
  }

  return [...dimMap.values()].sort((a, b) => a.optionTypeId.localeCompare(b.optionTypeId))
})

// Map: Resolve selected variant's option values per dimension
const selectedOptionIds = computed(() => {
  const variant = props.variants.find(v => v.id === props.modelValue)
  if (!variant) return new Map<string, string>()
  const map = new Map<string, string>()
  for (const ov of variant.optionValues) {
    map.set(ov.optionTypeId, ov.optionValueId)
  }
  return map
})

// Map: Check if an option value is selected
function isSelected(optionTypeId: string, optionValueId: string): boolean {
  return selectedOptionIds.value.get(optionTypeId) === optionValueId
}

// Trigger: Select an option value and resolve to the matching variant
function selectValue(optionTypeId: string, optionValueId: string): void {
  const currentSelected = new Map(selectedOptionIds.value)
  currentSelected.set(optionTypeId, optionValueId)

  // Find variant that matches all currently selected option values
  const match = props.variants.find(v => {
    return v.optionValues.every(ov => currentSelected.get(ov.optionTypeId) === ov.optionValueId)
  })

  if (match) emit('update:modelValue', match.id)
}

// Map: Get stock status for a specific option value combination
function getOptionStockStatus(optionTypeId: string, optionValueId: string): string | null {
  const match = props.variants.find(v =>
    v.optionValues.some(ov => ov.optionTypeId === optionTypeId && ov.optionValueId === optionValueId)
  )
  return match?.stock?.status ?? null
}
</script>
<template>
  <!-- Section: Product Options -->
  <div class="space-y-4">
    <div v-for="dim in dimensions" :key="dim.optionTypeId">
      <p class="text-sm font-medium text-stone-900 mb-2">{{ dim.optionTypeName }}</p>
      <div class="flex flex-wrap gap-2">
        <button
          v-for="value in dim.values"
          :key="value.id"
          class="px-4 py-2 rounded-lg border text-sm transition-colors"
          :class="[
            isSelected(dim.optionTypeId, value.id)
              ? 'border-stone-900 bg-stone-900 text-white'
              : 'border-stone-300 text-stone-700 hover:border-stone-400',
            getOptionStockStatus(dim.optionTypeId, value.id) === 'out_of_stock' ? 'opacity-50 line-through' : ''
          ]"
          @click="selectValue(dim.optionTypeId, value.id)"
        >
          {{ value.name }}
        </button>
      </div>
    </div>
  </div>
</template>
```

- [ ] **Step 2: Verify lint passes**

Run: `cd app/Store && pnpm run lint`
Expected: PASS

- [ ] **Step 3: Commit**

```bash
git add app/Store/src/features/catalog/components/ProductOptions.vue
git commit -m "feat(store): rewrite ProductOptions to use optionValues array with stock status per option"
```

---

### Task 11: Update ProductDetailView.vue — Use New Response Shape

**Files:**
- Modify: `app/Store/src/features/catalog/views/ProductDetailView.vue`

**Interfaces:**
- Consumes: `StoreProductDetailResponse` with `variants` containing `stock` info (from Task 7)
- Produces: Variant picker showing stock status

- [ ] **Step 1: Update ProductDetailView.vue to use variants with stock info**

The view already uses `product.variants` and `product.variants.length`. The main change is that the `ProductOptions` component now handles stock display. No major changes needed to the view itself — the stock info flows through `ProductOptions`.

However, we should add a stock summary display near the variant picker. Update the template section after `ProductOptions`:

```vue
<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useRoute } from 'vue-router'
import { getProductBySlug, getSimilarProducts } from '../services/productApi'
import { useCartStore } from '@/features/ordering/stores/cartStore'
import { useNotify } from '@/shared/composables/useNotify'
import ProductGallery from '../components/ProductGallery.vue'
import ProductOptions from '../components/ProductOptions.vue'
import SimilarProductsRow from '../components/SimilarProductsRow.vue'
import SizeGuideModal from '../components/SizeGuideModal.vue'
import ProductDetailsInfo from '../components/ProductDetailsInfo.vue'
import { useRecentlyViewed } from '@/shared/composables/useRecentlyViewed'
import type { StoreProductDetailResponse, StoreProductListItemResponse } from '../types/product'

const route = useRoute()
const cart = useCartStore()
const notify = useNotify()
const product = ref<StoreProductDetailResponse | null>(null)
const similar = ref<StoreProductListItemResponse[]>([])
const loading = ref(true)
const error = ref<string | null>(null)
const adding = ref(false)
const selectedVariantId = ref<string | null>(null)
const quantity = ref(1)

const breadcrumbItems = computed(() => [
  { label: 'Home', to: '/' },
  { label: 'Shop', to: '/shop' },
  { label: product.value?.name ?? 'Product' },
])

async function loadProduct(slug: string): Promise<void> {
  loading.value = true
  error.value = null
  similar.value = []
  quantity.value = 1
  const result = await getProductBySlug(slug)
  if (result.isSuccess) {
    product.value = result.value
    useRecentlyViewed().add({
      productId: result.value.id,
      productName: result.value.name,
      slug: result.value.slug,
      thumbnailUrl: result.value.thumbnailUrl,
      minPrice: result.value.minPrice,
      viewedAt: Date.now(),
    })
    selectedVariantId.value = result.value.masterVariant?.id ?? null
    const simResult = await getSimilarProducts(result.value.id)
    if (simResult.isSuccess) similar.value = simResult.items
  } else {
    error.value = result.message ?? 'Product not found'
  }
  loading.value = false
}

async function addToCart(): Promise<void> {
  if (!product.value || !selectedVariantId.value) {
    notify.error('Add to cart failed', 'Select a variant first')
    return
  }
  adding.value = true
  try {
    const ok = await cart.addItem(selectedVariantId.value, quantity.value)
    if (ok) notify.success('Added to cart', product.value.name)
    else notify.error('Add to cart failed', cart.error ?? undefined)
  } catch {
    notify.error('Add to cart failed', cart.error ?? undefined)
  } finally {
    adding.value = false
  }
}

// Map: Selected variant's stock info
const selectedVariant = computed(() =>
  product.value?.variants.find(v => v.id === selectedVariantId.value)
)

const stockLabel = computed(() => {
  const stock = selectedVariant.value?.stock
  if (!stock) return null
  if (stock.status === 'low_stock') return `Only ${stock.availableQuantity} left!`
  if (stock.status === 'out_of_stock') return stock.backorderable ? 'Available for backorder' : 'Out of stock'
  return null
})

const stockColor = computed(() => {
  switch (selectedVariant.value?.stock?.status) {
    case 'in_stock': return 'text-emerald-600'
    case 'low_stock': return 'text-amber-600'
    case 'backorderable': return 'text-blue-600'
    case 'out_of_stock': return 'text-red-500'
    default: return 'text-stone-400'
  }
})

watch(() => route.params.slug, (slug) => {
  if (typeof slug === 'string') loadProduct(slug)
}, { immediate: true })
</script>
<template>
  <!-- Section: Product Detail Page -->
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <!-- Section: Error State -->
    <div v-if="error" class="text-center py-16">
      <i class="pi pi-exclamation-circle text-4xl text-stone-300 mb-4" />
      <h2 class="text-xl font-semibold text-stone-900">{{ error }}</h2>
      <router-link to="/shop" class="text-primary hover:underline mt-2 inline-block">Browse products</router-link>
    </div>

    <!-- Section: Loading State -->
    <div v-else-if="loading" class="animate-pulse space-y-8">
      <div class="flex flex-col md:flex-row gap-8">
        <div class="w-full md:w-1/2 aspect-square bg-stone-200 rounded-xl" />
        <div class="w-full md:w-1/2 space-y-4">
          <div class="h-8 bg-stone-200 rounded w-3/4" />
          <div class="h-6 bg-stone-200 rounded w-1/4" />
          <div class="h-4 bg-stone-200 rounded w-full" />
          <div class="h-12 bg-stone-200 rounded w-1/3" />
        </div>
      </div>
    </div>

    <!-- Section: Product Content -->
    <template v-else-if="product">
      <div class="flex flex-col md:flex-row gap-8">
        <!-- Section: Image Gallery -->
        <div class="w-full md:w-1/2">
          <ProductGallery :images="product.images" :alt="product.name" />
        </div>

        <!-- Section: Product Info -->
        <div class="w-full md:w-1/2 space-y-6">
          <!-- Section: Breadcrumb -->
          <Breadcrumb :model="breadcrumbItems" class="mb-4" />

          <h1 class="text-2xl font-bold text-stone-900">{{ product.name }}</h1>

          <!-- Section: Size Guide -->
          <SizeGuideModal v-if="product.variants.length > 0" :variants="product.variants" :product-name="product.name" />

          <!-- Section: Price -->
          <p v-if="product.minPrice" class="text-3xl font-bold text-stone-900">
            {{ new Intl.NumberFormat('vi-VN', { style: 'currency', currency: product.currency ?? 'VND' }).format(product.minPrice) }}
          </p>

          <!-- Section: Stock Status -->
          <p v-if="stockLabel" class="text-sm font-medium" :class="stockColor">{{ stockLabel }}</p>

          <!-- Section: Product Details Info -->
          <ProductDetailsInfo :product="product" />

          <!-- Section: Variant Options -->
          <ProductOptions
            v-if="product.variants.length > 0"
            :variants="product.variants"
            :model-value="selectedVariantId"
            @update:model-value="(id: string) => selectedVariantId = id"
          />

          <!-- Section: Quantity + Add to Cart -->
          <div class="flex items-center gap-4">
            <InputNumber v-model="quantity" :min="1" :max="99" class="w-24" />
            <Button label="Add to Cart" icon="pi pi-shopping-cart" class="flex-1" :loading="adding" @click="addToCart" />
          </div>

          <!-- Section: Description -->
          <Accordion v-if="product.description" class="space-y-2">
            <AccordionPanel value="description">
              <AccordionHeader>Description</AccordionHeader>
              <AccordionContent>
                <p class="text-stone-600">{{ product.description }}</p>
              </AccordionContent>
            </AccordionPanel>
          </Accordion>
        </div>
      </div>

      <!-- Section: Similar Products -->
      <SimilarProductsRow
        v-if="similar.length > 0"
        :products="similar"
        class="mt-16"
      />
    </template>
  </div>
</template>
```

- [ ] **Step 2: Verify lint passes**

Run: `cd app/Store && pnpm run lint`
Expected: PASS

- [ ] **Step 3: Commit**

```bash
git add app/Store/src/features/catalog/views/ProductDetailView.vue
git commit -m "feat(store): add stock status display to ProductDetailView variant picker"
```

---

### Task 12: Final Verification

**Files:**
- None (verification only)

- [ ] **Step 1: Verify C# build passes**

Run: `dotnet build`
Expected: PASS with no warnings

- [ ] **Step 2: Verify unit tests pass**

Run: `dotnet test service/Api/tests/Module.UnitTests`
Expected: PASS

- [ ] **Step 3: Verify Store frontend lint passes**

Run: `cd app/Store && pnpm run lint`
Expected: PASS

- [ ] **Step 4: Verify Admin frontend lint passes (no regressions)**

Run: `cd app/Admin && pnpm run lint`
Expected: PASS

- [ ] **Step 5: Verify feature conventions**

Run: `bash scripts/check-feature-conventions.sh`
Expected: PASS (or only pre-existing issues)

- [ ] **Step 6: Verify cross-module references**

Run: `bash scripts/check-cross-module-refs.sh`
Expected: PASS (or only pre-existing issues)

---

## Summary

| Task | What | Backend/Frontend |
|------|------|-----------------|
| 1 | Add new C# response models | Backend |
| 2 | Update variant mapping | Backend |
| 3 | Update product mapping (list item) | Backend |
| 4 | Update GetStorefrontProducts handler | Backend |
| 5 | Update GetProductDetail handler | Backend |
| 6 | Delete Availability feature | Backend |
| 7 | Update frontend types | Frontend |
| 8 | Update frontend API layer | Frontend |
| 9 | Update ProductCard.vue | Frontend |
| 10 | Update ProductOptions.vue | Frontend |
| 11 | Update ProductDetailView.vue | Frontend |
| 12 | Final verification | Both |
