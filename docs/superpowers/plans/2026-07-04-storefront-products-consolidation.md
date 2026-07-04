# Storefront Products Consolidation & Images Fix — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Remove duplicated purpose endpoints (Search + Filter + Collections), create a unified product listing endpoint, and fix the Digitals download stub by renaming it to Images and wiring up real storage service streaming.

**Architecture:** Follows existing Clean Architecture + Vertical Slice pattern (CQRS-lite with MediatR + Carter Minimal API). Each endpoint is a `static partial class` with Query, Handler, Parameters, Response, and Endpoint files. The unified list handler merges text search and faceted filter logic into a single `IPagedQuery`. The Images download handler uses `IQueryHandler` returning a `Stream`-bearing response, with the Carter endpoint mapping to `Results.File()`.

**Tech Stack:** .NET 9, ASP.NET Core Minimal API, Carter, MediatR, Entity Framework Core (PostgreSQL), Moq (tests)

## Global Constraints

- All optional query parameters compose additively — no mutual exclusion
- `StoreProductListItemResponse` is the unified list response type
- Response types for non-list endpoints use `Result<T>` → `.ToResult()` extension
- Image download returns `Results.File()` with `Content-Disposition: attachment`
- All routes defined in `CatalogFeature.Storefront.cs` as nested static classes
- Test files use InMemory database + Moq for service dependencies

---

## File Structure

```
CREATE:
  Products/Get/List/ListProducts.cs           # Query + unified handler (search + filter)
  Products/Get/List/ListProducts.Endpoint.cs  # Carter ICarterModule
  Products/Get/List/ListProducts.Parameters.cs # Combined query parameters
  Products/Get/List/ListProducts.Response.cs  # Response inheriting StoreProductListItemResponse
  Images/Get/Download/DownloadImage.cs        # Query + handler (DB lookup + storage stream)
  Images/Get/Download/DownloadImage.Endpoint.cs # Carter endpoint → Results.File()

MODIFY:
  Features/Shared/CatalogFeature.Storefront.cs # Remove Search/Filter/Collections/NewArrivals/Digitals; add List + Images

DELETE:
  Products/Get/Search/                        # Entire directory (merged into List)
  Products/Get/Filter/                        # Entire directory (merged into List)
  Products/Get/Collections/                   # Entire directory (superseded by Taxons/Products)
  Digitals/                                   # Entire directory (renamed to Images, rewritten)

CREATE (tests):
  tests/.../Storefront/List/ListProducts.Tests.cs
  tests/.../Storefront/Images/Download/DownloadImage.Tests.cs

DELETE (tests):
  tests/.../Storefront/Search/SearchProducts.Tests.cs
  tests/.../Storefront/Collections/GetCollectionPage.Tests.cs
  tests/.../Storefront/Digitals/GenerateDownloadLink/GenerateDigitalDownloadLink.Tests.cs
```

---

## Task 1: Update Route Constants

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Storefront.cs`

**Interfaces:**
- Produces: `CatalogFeature.Storefront.Products.Get.List.Route`, `Summary`, `Description`
- Produces: `CatalogFeature.Storefront.Images.Get.Download.Route`, `Summary`, `Description`

- [ ] **Step 1: Remove old route constants and add new ones**

Remove the four nested classes inside `Storefront.Products.Get`:
- `Search` (lines 50-55)
- `Filter` (lines 57-62)
- `NewArrivals` (lines 64-69)
- `Collections` (lines 71-77)

Replace the `Digitals` class (lines 130-141) with `Images`:

```csharp
namespace Module.Catalog.Features.Shared;

public static partial class CatalogFeature
{
    public static class Storefront
    {
        public const string Route = "api/storefront";

        public static class Products
        {
            public const string BaseRoute = $"{Storefront.Route}/products";

            public static class Get
            {
                public static class Detail
                {
                    public const string Route = $"{BaseRoute}/{{slug}}";
                    public const string Description = "Retrieve full product detail page for the storefront by slug";
                    public const string Summary = "Get product detail page";
                }

                public static class Availability
                {
                    public const string Route = $"{BaseRoute}/{{id:guid}}/availability";
                    public const string Description = "Retrieve style matrix availability grid for a product";
                    public const string Summary = "Get product availability";
                }

                public static class Related
                {
                    public const string Route = $"{BaseRoute}/{{id:guid}}/related";
                    public const string Description = "Retrieve related products for a given product";
                    public const string Summary = "Get related products";
                }

                public static class Similar
                {
                    public const string Route = $"{BaseRoute}/{{id:guid}}/similar";
                    public const string Description = "Retrieve visually similar products using image embedding similarity";
                    public const string Summary = "Get similar products by image";
                }

                public static class SearchByImage
                {
                    public const string Route = $"{Storefront.Route}/search-by-image";
                    public const string Description = "Search products by uploading an image for visual similarity";
                    public const string Summary = "Search by image upload";
                }

                public static class List
                {
                    public const string Route = BaseRoute;
                    public const string Description = "Unified product listing with optional text search, faceted filters, sorting, and pagination";
                    public const string Summary = "List or search products";
                }
            }
        }

        public static class Taxonomies
        {
            public const string BaseRoute = $"{Storefront.Route}/taxonomies";

            public static class Get
            {
                public static class Tree
                {
                    public const string Route = $"{BaseRoute}/{{id:guid}}";
                    public const string Description = "Retrieve taxonomy tree with nested taxons for mega-menu";
                    public const string Summary = "Get taxonomy tree";
                }
            }
        }

        public static class Taxons
        {
            public const string BaseRoute = $"{Storefront.Route}/taxons";

            public static class Get
            {
                public static class All
                {
                    public const string Route = BaseRoute;
                    public const string Description = "Retrieve taxons filtered by depth and taxonomy";
                    public const string Summary = "List taxons";
                }

                public static class Products
                {
                    public const string Route = $"{BaseRoute}/{{id:guid}}/products";
                    public const string Description = "Retrieve paginated products by taxon with sorting";
                    public const string Summary = "Get products by taxon";
                }
            }
        }

        public static class OptionTypes
        {
            public static class Get
            {
                public static class All
                {
                    public const string Route = $"{Storefront.Route}/option-types";
                    public const string Description = "Retrieve all option types with values for filter facets";
                    public const string Summary = "List option types";
                }
            }
        }

        public static class Images
        {
            public static class Get
            {
                public static class Download
                {
                    public const string Route = $"{Storefront.Route}/images/{{id:guid}}/download";
                    public const string Description = "Download a variant image file by its ID";
                    public const string Summary = "Download image";
                }
            }
        }
    }
}
```

- [ ] **Step 2: Verify the file compiles**

Run: `dotnet build service/Api/src/Module/Module.csproj --no-restore 2>&1 | tail -5`
Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Storefront.cs
git commit -m "feat(catalog): consolidate storefront route constants — add List + Images, remove Search/Filter/Collections/NewArrivals/Digitals"
```

---

## Task 2: Create Unified ListProducts Handler

**Files:**
- Create: `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/ListProducts.cs`
- Create: `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/ListProducts.Endpoint.cs`
- Create: `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/ListProducts.Parameters.cs`
- Create: `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/ListProducts.Response.cs`

**Interfaces:**
- Consumes: `CatalogFeature.Storefront.Products.Get.List.Route`, `.Summary`, `.Description` (Task 1)
- Consumes: `ProductStoreMapping.MapToStoreListItem<T>()`, `StoreProductListItemResponse` (existing)
- Consumes: `IPagedQuery<Response>`, `IPagedQueryHandler<Query, Response>`, `QueryingParameters`
- Produces: `ListProducts.Query`, `ListProducts.Parameters`, `ListProducts.Response`, `ListProducts.Endpoint`

- [ ] **Step 1: Create Parameters file**

File: `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/ListProducts.Parameters.cs`

```csharp
namespace Module.Catalog.Features.Storefront.Products.Get.List;

public static partial class ListProducts
{
    public record Parameters : QueryingParameters
    {
        public string? Q { get; init; }
        public string? Color { get; init; }
        public string? Size { get; init; }
        public decimal? MinPrice { get; init; }
        public decimal? MaxPrice { get; init; }
        public string? Material { get; init; }
    }
}
```

- [ ] **Step 2: Create Response file**

File: `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/ListProducts.Response.cs`

```csharp
using Module.Catalog.Features.Storefront.Products.Shared.Models;

namespace Module.Catalog.Features.Storefront.Products.Get.List;

public static partial class ListProducts
{
    public record Response : StoreProductListItemResponse;
}
```

- [ ] **Step 3: Create handler file**

File: `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/ListProducts.cs`

```csharp
using Module.Catalog.Domain.Products;
using Module.Catalog.Features.Storefront.Products.Shared.Mappings;

namespace Module.Catalog.Features.Storefront.Products.Get.List;

public static partial class ListProducts
{
    public sealed record Query(Parameters Parameters) : IPagedQuery<Response>;

    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
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
                .Where(x => !x.IsDeleted && x.AvailableOn <= DateTimeOffset.UtcNow)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(parameters.Q))
            {
                var searchTerm = parameters.Q.ToLowerInvariant();
                query = query.Where(x =>
                    EF.Functions.ILike(x.Name, $"%{searchTerm}%")
                    || EF.Functions.ILike(x.Slug, $"%{searchTerm}%")
                    || (x.Description != null && EF.Functions.ILike(x.Description, $"%{searchTerm}%")));
            }

            if (!string.IsNullOrWhiteSpace(parameters.Color))
            {
                query = query.Where(x => x.Variants
                    .Any(v => v.OptionValueVariants
                        .Any(ov => ov.OptionValue != null
                            && ov.OptionValue.OptionType.Name == "Color"
                            && EF.Functions.ILike(ov.OptionValue.Name, parameters.Color))));
            }

            if (!string.IsNullOrWhiteSpace(parameters.Size))
            {
                query = query.Where(x => x.Variants
                    .Any(v => v.OptionValueVariants
                        .Any(ov => ov.OptionValue != null
                            && ov.OptionValue.OptionType.Name == "Size"
                            && EF.Functions.ILike(ov.OptionValue.Name, parameters.Size))));
            }

            if (parameters.MinPrice.HasValue)
            {
                query = query.Where(x => x.Variants
                    .Any(v => v.Prices.Any(p => p.Amount >= parameters.MinPrice.Value)));
            }

            if (parameters.MaxPrice.HasValue)
            {
                query = query.Where(x => x.Variants
                    .Any(v => v.Prices.Any(p => p.Amount <= parameters.MaxPrice.Value)));
            }

            if (!string.IsNullOrWhiteSpace(parameters.Material))
            {
                query = query.Where(x => x.Variants
                    .Any(v => v.OptionValueVariants
                        .Any(ov => ov.OptionValue != null
                            && ov.OptionValue.OptionType.Name == "Material"
                            && EF.Functions.ILike(ov.OptionValue.Name, parameters.Material))));
            }

            var parsing = parameters.ParseAll();
            if (parsing.IsFailure)
                return parsing.Errors;

            var pagedResult = await query
                .OrderByDescending(x => x.CreatedAtUtc)
                .ApplyQuerying(parsing.Value)
                .ToPagedOrAllAsync(parsing.Value, x => x.MapToStoreListItem<Response>(), cancellationToken);

            return pagedResult;
        }
    }
}
```

- [ ] **Step 4: Create endpoint file**

File: `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/ListProducts.Endpoint.cs`

```csharp
using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Products.Get.List;

public static partial class ListProducts
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Storefront.Products.Get.List.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(ListProducts))
            .WithTags(CatalogFeature.Tags.Product)
            .WithSummary(CatalogFeature.Storefront.Products.Get.List.Summary)
            .WithDescription(CatalogFeature.Storefront.Products.Get.List.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}
```

- [ ] **Step 5: Verify build**

Run: `dotnet build service/Api/src/Module/Module.csproj --no-restore 2>&1 | tail -5`
Expected: Build succeeded

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/
git commit -m "feat(catalog): add unified ListProducts handler merging search and filter"
```

---

## Task 3: Rename Digitals → Images and Rewrite Download Handler

**Files:**
- Create: `service/Api/src/Module/Catalog/Features/Storefront/Images/Get/Download/DownloadImage.cs`
- Create: `service/Api/src/Module/Catalog/Features/Storefront/Images/Get/Download/DownloadImage.Endpoint.cs`
- Delete: `service/Api/src/Module/Catalog/Features/Storefront/Digitals/` (entire directory)

**Interfaces:**
- Consumes: `CatalogFeature.Storefront.Images.Get.Download.Route`, `.Summary`, `.Description` (Task 1)
- Consumes: `IApplicationDbContext`, `IStorageService`, `VariantImage`
- Produces: `DownloadImage.Query`, `DownloadImage.Response`, `DownloadImage.Endpoint`

- [ ] **Step 1: Create the handler file**

File: `service/Api/src/Module/Catalog/Features/Storefront/Images/Get/Download/DownloadImage.cs`

```csharp
using Module.Catalog.Domain.Products.Variants.Images;

namespace Module.Catalog.Features.Storefront.Images.Get.Download;

public static partial class DownloadImage
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed record Response(Stream Stream, string FileName, string ContentType) : IDisposable
    {
        public void Dispose() => Stream.Dispose();
    }

    public sealed class QueryHandler(
        IApplicationDbContext dbContext,
        IStorageService storageService)
        : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var image = await dbContext.Set<VariantImage>()
                .FirstOrDefaultAsync(i => i.Id == query.Id, cancellationToken);

            if (image is null)
                return VariantImageResult.Failure.ById(query.Id);

            var downloadResult = await storageService.DownloadAsync(image.StoragePath, cancellationToken: cancellationToken);

            if (downloadResult.IsFailure)
                return downloadResult.Errors;

            return new Response(
                downloadResult.Value.Content,
                image.FileName,
                image.ContentType);
        }
    }
}
```

- [ ] **Step 2: Create the endpoint file**

File: `service/Api/src/Module/Catalog/Features/Storefront/Images/Get/Download/DownloadImage.Endpoint.cs`

```csharp
using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Images.Get.Download;

public static partial class DownloadImage
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Storefront.Images.Get.Download.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);

                if (result.IsFailure)
                    return Results.NotFound(result);

                return Results.File(result.Value.Stream, result.Value.ContentType, result.Value.FileName);
            })
            .WithName(nameof(DownloadImage))
            .WithTags(CatalogFeature.Tags.Variant)
            .WithSummary(CatalogFeature.Storefront.Images.Get.Download.Summary)
            .WithDescription(CatalogFeature.Storefront.Images.Get.Download.Description)
            .Produces(StatusCodes.Status200OK)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
```

- [ ] **Step 3: Verify build**

Run: `dotnet build service/Api/src/Module/Module.csproj --no-restore 2>&1 | tail -10`
Expected: Build succeeded

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Storefront/Images/
git commit -m "feat(catalog): add Images download endpoint with real storage streaming"
```

---

## Task 4: Delete Old Endpoint Directories and HTTP Test Files

**Files:**
- Delete: `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/Search/`
- Delete: `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/Filter/`
- Delete: `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/Collections/`
- Delete: `service/Api/src/Module/Catalog/Features/Storefront/Digitals/`
- Delete: `ApiTests/Catalog/Storefront/search.http`
- Delete: `ApiTests/Catalog/Storefront/collections.http`
- Delete: `ApiTests/Catalog/Storefront/digitals.http`

- [ ] **Step 1: Delete old directories and HTTP test files**

```bash
rm -rf service/Api/src/Module/Catalog/Features/Storefront/Products/Get/Search/
rm -rf service/Api/src/Module/Catalog/Features/Storefront/Products/Get/Filter/
rm -rf service/Api/src/Module/Catalog/Features/Storefront/Products/Get/Collections/
rm -rf service/Api/src/Module/Catalog/Features/Storefront/Digitals/
rm ApiTests/Catalog/Storefront/search.http
rm ApiTests/Catalog/Storefront/collections.http
rm ApiTests/Catalog/Storefront/digitals.http
```

- [ ] **Step 2: Verify build still passes**

Run: `dotnet build service/Api/src/Module/Module.csproj --no-restore 2>&1 | tail -5`
Expected: Build succeeded

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Storefront/Products/Get/Search/ \
        service/Api/src/Module/Catalog/Features/Storefront/Products/Get/Filter/ \
        service/Api/src/Module/Catalog/Features/Storefront/Products/Get/Collections/ \
        service/Api/src/Module/Catalog/Features/Storefront/Digitals/ \
        ApiTests/Catalog/Storefront/search.http \
        ApiTests/Catalog/Storefront/collections.http \
        ApiTests/Catalog/Storefront/digitals.http
git commit -m "feat(catalog): remove duplicated Search, Filter, Collections, and Digitals endpoints"
```

---

## Task 5: Update Tests

**Files:**
- Delete: `service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/Search/SearchProducts.Tests.cs`
- Delete: `service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/Collections/GetCollectionPage.Tests.cs`
- Delete: `service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/Digitals/GenerateDownloadLink/GenerateDigitalDownloadLink.Tests.cs`
- Create: `service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/List/ListProducts.Tests.cs`
- Create: `service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/Images/Download/DownloadImage.Tests.cs`

**Interfaces:**
- Consumes: `ListProducts.PagedQueryHandler` (Task 2)
- Consumes: `DownloadImage.QueryHandler` (Task 3)

- [ ] **Step 1: Delete old test files**

```bash
rm service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/Search/SearchProducts.Tests.cs
rm service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/Collections/GetCollectionPage.Tests.cs
rm service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/Digitals/GenerateDownloadLink/GenerateDigitalDownloadLink.Tests.cs
```

- [ ] **Step 2: Create ListProducts tests**

File: `service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/List/ListProducts.Tests.cs`

```csharp
using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Features.Storefront.Products.Get.List;

namespace Module.UnitTests.Catalog.Features.Storefront.Products.Get.List;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "StorefrontListProducts")]
public class ListProductsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ListProducts.PagedQueryHandler _handler;

    public ListProductsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _handler = new ListProducts.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return all active products with empty parameters")]
    public async Task Handle_ShouldReturnAllActiveProducts_WhenNoFilters()
    {
        var product = ProductMethod.Create("Blue T-Shirt", "blue-tshirt", status: ProductStatus.Active).Value;
        product.AvailableOn = DateTimeOffset.UtcNow.AddDays(-1);
        product.MasterVariantId = VariantMethod.Create(product.Id, "M", isMaster: true).Value.Id;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new ListProducts.Query(new ListProducts.Parameters()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Blue T-Shirt");
    }

    [Fact(DisplayName = "Handler: Should exclude discontinued products")]
    public async Task Handle_ShouldExcludeDiscontinuedProducts()
    {
        var product = ProductMethod.Create("Shoes", "shoes", status: ProductStatus.Archived).Value;
        product.MasterVariantId = VariantMethod.Create(product.Id, "M", isMaster: true).Value.Id;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new ListProducts.Query(new ListProducts.Parameters()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should exclude future products")]
    public async Task Handle_ShouldExcludeFutureProducts()
    {
        var product = ProductMethod.Create("Future Item", "future-item", status: ProductStatus.Active).Value;
        product.AvailableOn = DateTimeOffset.UtcNow.AddDays(7);
        product.MasterVariantId = VariantMethod.Create(product.Id, "M", isMaster: true).Value.Id;
        _dbContext.Set<Product>().Add(product);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new ListProducts.Query(new ListProducts.Parameters()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should return empty when no parameters and no products exist")]
    public async Task Handle_ShouldReturnEmpty_WhenNoProducts()
    {
        var result = await _handler.Handle(
            new ListProducts.Query(new ListProducts.Parameters()),
            TestContext.Current.CancellationToken);

        result.Items.Should().BeEmpty();
    }
}
```

- [ ] **Step 3: Create DownloadImage tests**

File: `service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/Images/DownloadImage.Tests.cs`

```csharp
using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Features.Storefront.Images.Get.Download;

using Shared.Operational.Storages.Models;
using Shared.Application.Models.Results;
using Shared.Operational.Storages.Services;

using Moq;

namespace Module.UnitTests.Catalog.Features.Storefront.Images.Get.Download;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "StorefrontDownloadImage")]
public class DownloadImageTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IStorageService> _storageServiceMock;
    private readonly DownloadImage.QueryHandler _handler;

    public DownloadImageTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _storageServiceMock = new Mock<IStorageService>();

        _handler = new DownloadImage.QueryHandler(_dbContext, _storageServiceMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return file stream when VariantImage exists")]
    public async Task Handle_ShouldReturnStream_WhenImageExists()
    {
        using var stream = new MemoryStream([1, 2, 3]);
        var storedInfo = new StoredObjectInfo("images/test.jpg", "local", 3, DateTimeOffset.UtcNow, "image/jpeg");
        var image = new VariantImage
        {
            Id = Guid.NewGuid(),
            FileName = "test.jpg",
            ContentType = "image/jpeg",
            StoragePath = "images/test.jpg",
            Url = "/media/test.jpg"
        };
        _dbContext.Set<VariantImage>().Add(image);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _storageServiceMock
            .Setup(s => s.DownloadAsync(image.StoragePath, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DownloadResult>.Ok(new DownloadResult(stream, storedInfo)));

        var result = await _handler.Handle(
            new DownloadImage.Query(image.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.FileName.Should().Be("test.jpg");
        result.Value.ContentType.Should().Be("image/jpeg");
    }

    [Fact(DisplayName = "Handler: Should return failure when VariantImage does not exist")]
    public async Task Handle_ShouldReturnFailure_WhenImageDoesNotExist()
    {
        var result = await _handler.Handle(
            new DownloadImage.Query(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --no-restore --filter "FullyQualifiedName~Storefront" 2>&1 | tail -20`
Expected: All Storefront tests pass

- [ ] **Step 5: Commit**

```bash
git add service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/
git commit -m "test(catalog): update storefront tests for List and Images download endpoints"
```

---

## Task 6: Full Build and Test Verification

- [ ] **Step 1: Build entire solution**

Run: `dotnet build service/Api/src/Module/Module.csproj --no-restore 2>&1 | tail -10`
Expected: Build succeeded with 0 errors

- [ ] **Step 2: Run all catalog tests**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --no-restore --filter "FullyQualifiedName~Catalog" 2>&1 | tail -20`
Expected: All tests pass, 0 failures

- [ ] **Step 3: Run all unit tests**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --no-restore 2>&1 | tail -10`
Expected: All tests pass
