# Storefront Image Display Endpoint — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the download endpoint with a display endpoint using `TypedResults.PhysicalFile()` for inline image serving.

**Architecture:** Replace `Images/Get/Download/` with `Images/Get/Image/`. The new handler uses `IOptions<LocalStorageProviderSetting>` to resolve the physical file path from `VariantImage.StoragePath`, then the endpoint returns `TypedResults.PhysicalFile(fullPath, contentType)` — no `fileDownloadName`, so browsers display inline.

**Tech Stack:** .NET 10, ASP.NET Core Minimal API, Carter, MediatR, Entity Framework Core, Moq (tests)

## Global Constraints

- Route: `api/storefront/images/{id:guid}` — serves image for inline display
- Uses `TypedResults.PhysicalFile(fullPath, contentType)` — no forced download
- Handler uses `IOptions<LocalStorageProviderSetting>` (not IStorageService)
- Path construction mirrors `LocalStorageProvider.ResolvePath()`: `Path.GetFullPath(Path.Combine(localPath, storagePath))` with traversal guard
- File existence checked with `File.Exists()` before returning
- Not found → `VariantImageResult.Failure.ById(id)`
- Tags: `CatalogFeature.Tags.Variant`

---

### Task 1: Update Route Constant

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Storefront.cs`

**Interfaces:**
- Produces: `CatalogFeature.Storefront.Images.Get.Image.Route`, `.Summary`, `.Description`

- [ ] **Step 1: Replace `Download` route constant with `Image`**

In `CatalogFeature.Storefront.cs`, replace the `Images.Get.Download` nested class:

**Before (lines 114-120):**
```csharp
                public static class Download
                {
                    public const string Route = $"{Storefront.Route}/images/{{id:guid}}/download";
                    public const string Description = "Download a variant image file by its ID";
                    public const string Summary = "Download image";
                }
```

**After:**
```csharp
                public static class Image
                {
                    public const string Route = $"{Storefront.Route}/images/{{id:guid}}";
                    public const string Description = "Display a variant image file inline by its ID";
                    public const string Summary = "Display image";
                }
```

- [ ] **Step 2: Verify build**

Run: `dotnet build service/Api/src/Module/Module.csproj --no-restore 2>&1 | tail -5`
Expected: Build fails (old `DownloadImage.cs` references removed constant) — fixed in Task 2

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Storefront.cs
git commit -m "feat(catalog): replace Images download route constant with image display route"
```

---

### Task 2: Create GetImage Handler and Endpoint

**Files:**
- Create: `service/Api/src/Module/Catalog/Features/Storefront/Images/Get/Image/GetImage.cs`
- Create: `service/Api/src/Module/Catalog/Features/Storefront/Images/Get/Image/GetImage.Endpoint.cs`
- Delete: `service/Api/src/Module/Catalog/Features/Storefront/Images/Get/Download/DownloadImage.cs`
- Delete: `service/Api/src/Module/Catalog/Features/Storefront/Images/Get/Download/DownloadImage.Endpoint.cs`

**Interfaces:**
- Consumes: `CatalogFeature.Storefront.Images.Get.Image.Route`, `.Summary`, `.Description` (Task 1)
- Consumes: `IApplicationDbContext`, `IOptions<LocalStorageProviderSetting>`, `VariantImage`, `VariantImageResult`
- Produces: `GetImage.Query`, `GetImage.Response`, `GetImage.Endpoint`

- [ ] **Step 1: Create handler file**

Create directory: `mkdir -p service/Api/src/Module/Catalog/Features/Storefront/Images/Get/Image/`

File: `service/Api/src/Module/Catalog/Features/Storefront/Images/Get/Image/GetImage.cs`

```csharp
using Microsoft.Extensions.Options;

using Module.Catalog.Domain.Products.Variants.Images;

using Shared.Operational.Storages.Providers.Options;

namespace Module.Catalog.Features.Storefront.Images.Get.Image;

public static partial class GetImage
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed record Response(string FullPath, string ContentType);

    public sealed class QueryHandler(
        IApplicationDbContext dbContext,
        IOptions<LocalStorageProviderSetting> storageOptions)
        : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var image = await dbContext.Set<VariantImage>()
                .FirstOrDefaultAsync(i => i.Id == query.Id, cancellationToken);

            if (image is null)
                return VariantImageResult.Failure.ById(query.Id);

            var root = Path.GetFullPath(storageOptions.Value.LocalPath);
            var fullPath = Path.GetFullPath(Path.Combine(
                root,
                image.StoragePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));

            if (!fullPath.StartsWith(root, StringComparison.Ordinal))
                return VariantImageResult.Failure.ById(query.Id);

            if (!File.Exists(fullPath))
                return VariantImageResult.Failure.ById(query.Id);

            return new Response(fullPath, image.ContentType);
        }
    }
}
```

- [ ] **Step 2: Create endpoint file**

File: `service/Api/src/Module/Catalog/Features/Storefront/Images/Get/Image/GetImage.Endpoint.cs`

```csharp
using Microsoft.AspNetCore.Http;

using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Images.Get.Image;

public static partial class GetImage
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Storefront.Images.Get.Image.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);

                if (result.IsFailure)
                    return result.ToResult();

                return TypedResults.PhysicalFile(result.Value.FullPath, result.Value.ContentType);
            })
            .WithName(nameof(GetImage))
            .WithTags(CatalogFeature.Tags.Variant)
            .WithSummary(CatalogFeature.Storefront.Images.Get.Image.Summary)
            .WithDescription(CatalogFeature.Storefront.Images.Get.Image.Description)
            .Produces(StatusCodes.Status200OK)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
```

- [ ] **Step 3: Delete old DownloadImage files**

```bash
rm -rf service/Api/src/Module/Catalog/Features/Storefront/Images/Get/Download/
```

- [ ] **Step 4: Verify build**

Run: `dotnet build service/Api/src/Module/Module.csproj --no-restore 2>&1 | tail -5`
Expected: Build succeeded

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Storefront/Images/Get/
git commit -m "feat(catalog): replace download endpoint with PhysicalFile inline display endpoint"
```

---

### Task 3: Update Tests

**Files:**
- Delete: `service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/Images/Download/DownloadImage.Tests.cs`
- Create: `service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/Images/Image/GetImage.Tests.cs`

**Interfaces:**
- Consumes: `GetImage.QueryHandler` (Task 2)
- Consumes: `IOptions<LocalStorageProviderSetting>`, `LocalStorageProviderSetting` (from Shared)

- [ ] **Step 1: Delete old test file**

```bash
rm service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/Images/Download/DownloadImage.Tests.cs
```

- [ ] **Step 2: Create new test file**

Create directory: `mkdir -p service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/Images/Image/`

File: `service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/Images/Image/GetImage.Tests.cs`

```csharp
using Microsoft.Extensions.Options;

using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Features.Storefront.Images.Get.Image;

using Shared.Operational.Storages.Providers.Options;

namespace Module.UnitTests.Catalog.Features.Storefront.Images.Get.Image;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "StorefrontGetImage")]
public class GetImageTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly string _tempDir;
    private readonly GetImage.QueryHandler _handler;

    public GetImageTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Product).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _tempDir = Path.Combine(Path.GetTempPath(), $"imgtest-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);

        var setting = new LocalStorageProviderSetting { LocalPath = _tempDir };
        var optionsWrapper = Options.Create(setting);

        _handler = new GetImage.QueryHandler(_dbContext, optionsWrapper);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return full path when VariantImage exists and file is on disk")]
    public async Task Handle_ShouldReturnFullPath_WhenImageAndFileExist()
    {
        var fileName = "test.jpg";
        var storagePath = $"images/{fileName}";
        var fileDir = Path.Combine(_tempDir, "images");
        var filePath = Path.Combine(fileDir, fileName);
        Directory.CreateDirectory(fileDir);
        await File.WriteAllTextAsync(filePath, "fake-image-data", TestContext.Current.CancellationToken);

        var image = new VariantImage
        {
            Id = Guid.NewGuid(),
            FileName = "test.jpg",
            ContentType = "image/jpeg",
            StoragePath = storagePath,
            Url = string.Empty
        };
        _dbContext.Set<VariantImage>().Add(image);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetImage.Query(image.Id),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.FullPath.Should().Be(filePath);
        result.Value.ContentType.Should().Be("image/jpeg");
    }

    [Fact(DisplayName = "Handler: Should return failure when VariantImage does not exist")]
    public async Task Handle_ShouldReturnFailure_WhenImageDoesNotExist()
    {
        var result = await _handler.Handle(
            new GetImage.Query(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when file does not exist on disk")]
    public async Task Handle_ShouldReturnFailure_WhenFileDoesNotExist()
    {
        var image = new VariantImage
        {
            Id = Guid.NewGuid(),
            FileName = "missing.jpg",
            ContentType = "image/jpeg",
            StoragePath = "images/missing.jpg",
            Url = string.Empty
        };
        _dbContext.Set<VariantImage>().Add(image);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetImage.Query(image.Id),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }
}
```

- [ ] **Step 3: Run storefront tests**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --no-restore --filter "FullyQualifiedName~Storefront" 2>&1 | tail -15`
Expected: All Storefront tests pass

- [ ] **Step 4: Run full test suite**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --no-restore 2>&1 | tail -5`
Expected: All tests pass

- [ ] **Step 5: Commit**

```bash
git add service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/Images/
git commit -m "test(catalog): update image tests for PhysicalFile display endpoint"
```

---

### Task 4: Final Build and Verification

- [ ] **Step 1: Build module**

Run: `dotnet build service/Api/src/Module/Module.csproj --no-restore 2>&1 | tail -5`
Expected: Build succeeded, 0 errors

- [ ] **Step 2: Run all tests**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --no-restore 2>&1 | tail -5`
Expected: All tests pass
