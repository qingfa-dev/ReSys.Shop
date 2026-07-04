# Storefront Image Display Endpoint — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the download endpoint with a display endpoint using `TypedResults.PhysicalFile()`. Add `ResolvePath` to the storage abstraction so the handler stays provider-agnostic.

**Architecture:** Add `ResolvePath(string key)` to `IStorageProvider` → implements it in local (returns physical path) and S3 (returns error). Expose through `IStorageService`. The handler calls `IStorageService.ResolvePath(image.StoragePath)` — no provider-specific logic in the storefront. The endpoint returns `TypedResults.PhysicalFile(fullPath, contentType)` for inline display.

**Tech Stack:** .NET 10, ASP.NET Core Minimal API, Carter, MediatR, Entity Framework Core, Moq (tests)

## Global Constraints

- Route: `api/storefront/images/{id:guid}` — serves image for inline display
- Uses `TypedResults.PhysicalFile(fullPath, contentType)` — no forced download
- Handler uses `IStorageService.ResolvePath(key)` — not `IOptions<LocalStorageProviderSetting>` directly
- `IStorageProvider.ResolvePath` returns `Result<string>` — physical path or error
- Local provider reuses existing `ResolvePath` (make public); S3 returns `ProviderError`
- `IStorageService.ResolvePath` delegates to the resolved provider (same pattern as DownloadAsync)
- Not found cases: `VariantImageResult.Failure.ById(id)` for missing image or file
- Tags: `CatalogFeature.Tags.Variant`

---

### Task 1: Add `ResolvePath` to Storage Provider Interface

**Files:**
- Modify: `service/Api/src/Shared/Operational/Storages/Providers/StorageProvider.Interface.cs`
- Modify: `service/Api/src/Shared/Operational/Storages/Providers/Local.StorageProvider.Implementation.cs`
- Modify: `service/Api/src/Shared/Operational/Storages/Providers/S3.StorageProvider.Implementation.cs`

**Interfaces:**
- Produces: `IStorageProvider.ResolvePath(string key)` → `Result<string>`

- [ ] **Step 1: Add method to `IStorageProvider`**

File: `service/Api/src/Shared/Operational/Storages/Providers/StorageProvider.Interface.cs`

Add after `DownloadAsync`:

```csharp
/// <summary>Resolves the physical file path for <paramref name="key"/> if this is a file-based provider.</summary>
/// <param name="key">Object key as stored.</param>
/// <returns>The full physical file path, or an error for non-file-based providers.</returns>
Result<string> ResolvePath(string key);
```

- [ ] **Step 2: Implement in `LocalStorageProvider`**

File: `service/Api/src/Shared/Operational/Storages/Providers/Local.StorageProvider.Implementation.cs`

Change the existing private `ResolvePath` method (line 197) from `private` to `public`:

```csharp
public Result<string> ResolvePath(string key)
{
    try
    {
        var root = Path.GetFullPath(options.Value.LocalPath);
        var combined = Path.GetFullPath(Path.Combine(root, key.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));

        if (!combined.StartsWith(root, StringComparison.Ordinal))
            return StorageResult.Failure.PathTraversalDetected(key);

        return Result<string>.Ok(combined);
    }
    catch (Exception ex)
    {
        return StorageResult.Failure.ProviderError(ex.Message);
    }
}
```

- [ ] **Step 3: Implement in `S3StorageProvider`**

File: `service/Api/src/Shared/Operational/Storages/Providers/S3.StorageProvider.Implementation.cs`

Add the method:

```csharp
public Result<string> ResolvePath(string key) =>
    StorageResult.Failure.ProviderError("S3 is not a file-based provider. Use DownloadAsync to get a stream.");
```

- [ ] **Step 4: Verify build**

Run: `dotnet build service/Api/src/Shared/Shared.csproj --no-restore 2>&1 | tail -5`
Expected: Build succeeded

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Shared/Operational/Storages/Providers/
git commit -m "feat(storage): add ResolvePath to IStorageProvider for physical file path resolution"
```

---

### Task 2: Add `ResolvePath` to `IStorageService`

**Files:**
- Modify: `service/Api/src/Shared/Operational/Storages/Services/Storage.Service.Interface.cs`
- Modify: `service/Api/src/Shared/Operational/Storages/Services/Storage.Service.Implementation.cs`

**Interfaces:**
- Consumes: `IStorageProvider.ResolvePath(string key)` (Task 1)
- Produces: `IStorageService.ResolvePath(string key, string? providerName = null, CancellationToken ct = default)`

- [ ] **Step 1: Add method to `IStorageService` interface**

File: `service/Api/src/Shared/Operational/Storages/Services/Storage.Service.Interface.cs`

Add after `DownloadAsync`:

```csharp
/// <summary>Resolves the physical file path using the <paramref name="providerName"/> provider.</summary>
/// <param name="key">Object key as stored.</param>
/// <param name="providerName">Provider name; pass <c>null</c> for default.</param>
/// <param name="ct">Cancellation token.</param>
/// <returns>The full physical file path, or an error for non-file-based providers.</returns>
Task<Result<string>> ResolvePathAsync(
    string key,
    string? providerName = null,
    CancellationToken ct = default);
```

- [ ] **Step 2: Implement in `StorageService`**

File: `service/Api/src/Shared/Operational/Storages/Services/Storage.Service.Implementation.cs`

Add after `DownloadAsync`:

```csharp
/// <inheritdoc />
public Task<Result<string>> ResolvePathAsync(
    string key,
    string? providerName = null,
    CancellationToken ct = default)
{
    if (!TryResolve(providerName, out IStorageProvider provider))
        return Task.FromResult<Result<string>>(StorageResult.Failure.ProviderNotFound(providerName ?? defaultProviderName));

    Result<string> result = provider.ResolvePath(key);
    return Task.FromResult(result);
}
```

- [ ] **Step 3: Verify build**

Run: `dotnet build service/Api/src/Shared/Shared.csproj --no-restore 2>&1 | tail -5`
Expected: Build succeeded

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Shared/Operational/Storages/Services/
git commit -m "feat(storage): add ResolvePathAsync to IStorageService"
```

---

### Task 3: Update Route Constant

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Storefront.cs`

**Interfaces:**
- Produces: `CatalogFeature.Storefront.Images.Get.Image.Route`, `.Summary`, `.Description`

- [ ] **Step 1: Replace `Download` route constant with `Image`**

In `CatalogFeature.Storefront.cs`, replace:

```csharp
                public static class Download
                {
                    public const string Route = $"{Storefront.Route}/images/{{id:guid}}/download";
                    public const string Description = "Download a variant image file by its ID";
                    public const string Summary = "Download image";
                }
```

With:

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
Expected: Build fails (old DownloadImage.cs references removed constant) — fixed in Task 4

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Storefront.cs
git commit -m "feat(catalog): replace Images download route constant with image display route"
```

---

### Task 4: Create GetImage Handler and Endpoint

**Files:**
- Create: `service/Api/src/Module/Catalog/Features/Storefront/Images/Get/Image/GetImage.cs`
- Create: `service/Api/src/Module/Catalog/Features/Storefront/Images/Get/Image/GetImage.Endpoint.cs`
- Delete: `service/Api/src/Module/Catalog/Features/Storefront/Images/Get/Download/DownloadImage.cs`
- Delete: `service/Api/src/Module/Catalog/Features/Storefront/Images/Get/Download/DownloadImage.Endpoint.cs`

**Interfaces:**
- Consumes: `CatalogFeature.Storefront.Images.Get.Image.Route`, `.Summary`, `.Description` (Task 3)
- Consumes: `IApplicationDbContext`, `IStorageService.ResolvePathAsync()`, `VariantImage`, `VariantImageResult`
- Produces: `GetImage.Query`, `GetImage.Response`, `GetImage.Endpoint`

- [ ] **Step 1: Create handler file**

Create directory: `mkdir -p service/Api/src/Module/Catalog/Features/Storefront/Images/Get/Image/`

File: `service/Api/src/Module/Catalog/Features/Storefront/Images/Get/Image/GetImage.cs`

```csharp
using Module.Catalog.Domain.Products.Variants.Images;

using Shared.Operational.Storages.Services;

namespace Module.Catalog.Features.Storefront.Images.Get.Image;

public static partial class GetImage
{
    public sealed record Query(Guid Id) : IQuery<Response>;

    public sealed record Response(string FullPath, string ContentType);

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

            var pathResult = await storageService.ResolvePathAsync(image.StoragePath, ct: cancellationToken);

            if (pathResult.IsFailure)
                return pathResult.Errors;

            var fullPath = pathResult.Value;

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
git commit -m "feat(catalog): replace download endpoint with ResolvePath-based inline display endpoint"
```

---

### Task 5: Update Tests

**Files:**
- Delete: `service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/Images/Download/DownloadImage.Tests.cs`
- Create: `service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/Images/Image/GetImage.Tests.cs`

**Interfaces:**
- Consumes: `GetImage.QueryHandler` (Task 4)
- Consumes: `IStorageService.ResolvePathAsync()` → mocked via Moq

- [ ] **Step 1: Delete old test file**

```bash
rm service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/Images/Download/DownloadImage.Tests.cs
```

- [ ] **Step 2: Create new test file**

Create directory: `mkdir -p service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/Images/Image/`

File: `service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/Images/Image/GetImage.Tests.cs`

```csharp
using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Features.Storefront.Images.Get.Image;

using Shared.Operational.Storages.Services;

using Moq;

namespace Module.UnitTests.Catalog.Features.Storefront.Images.Get.Image;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "StorefrontGetImage")]
public class GetImageTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IStorageService> _storageServiceMock;
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

        _storageServiceMock = new Mock<IStorageService>();

        _handler = new GetImage.QueryHandler(_dbContext, _storageServiceMock.Object);
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

        _storageServiceMock
            .Setup(s => s.ResolvePathAsync(storagePath, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Ok(filePath));

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
        var storagePath = "images/missing.jpg";
        var missingPath = Path.Combine(_tempDir, "images", "missing.jpg");

        var image = new VariantImage
        {
            Id = Guid.NewGuid(),
            FileName = "missing.jpg",
            ContentType = "image/jpeg",
            StoragePath = storagePath,
            Url = string.Empty
        };
        _dbContext.Set<VariantImage>().Add(image);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _storageServiceMock
            .Setup(s => s.ResolvePathAsync(storagePath, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Ok(missingPath));

        var result = await _handler.Handle(
            new GetImage.Query(image.Id),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when storage resolves to error")]
    public async Task Handle_ShouldReturnFailure_WhenResolvePathFails()
    {
        var storagePath = "images/test.jpg";

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

        _storageServiceMock
            .Setup(s => s.ResolvePathAsync(storagePath, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.NotFound("Path not found"));

        var result = await _handler.Handle(
            new GetImage.Query(image.Id),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }
}
```

- [ ] **Step 3: Run storefront tests**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --no-restore --filter "FullyQualifiedName~StorefrontGetImage" 2>&1 | tail -15`
Expected: All GetImage tests pass

- [ ] **Step 4: Run full test suite**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --no-restore 2>&1 | tail -5`
Expected: All tests pass

- [ ] **Step 5: Commit**

```bash
git add service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/Images/
git commit -m "test(catalog): update image tests for ResolvePath-based display endpoint"
```

---

### Task 6: Final Build and Verification

- [ ] **Step 1: Build entire solution**

Run: `dotnet build service/Api/src/Module/Module.csproj --no-restore 2>&1 | tail -5`
Expected: Build succeeded, 0 errors

- [ ] **Step 2: Run all tests**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --no-restore 2>&1 | tail -5`
Expected: All tests pass
