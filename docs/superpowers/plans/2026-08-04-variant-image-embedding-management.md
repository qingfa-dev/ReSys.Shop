# Variant Image Embedding Management UI — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add inline embedding status/management to VariantDetail's Images tab with Hangfire background jobs and polling.

**Architecture:** Add `Status`/`Error`/`HangfireJobId`/`CompletedAtUtc` fields to the `ImageEmbedding` domain entity with status-transition methods (`MarkPending`/`MarkProcessing`/`MarkCompleted`/`MarkFailed`). Add a `RunAsync(embeddingId)` method to `IEmbeddingOrchestrator` that drives the Hangfire job. New backend vertical slices: GetEmbedding (GET by variantImageId), DeleteEmbedding (DELETE). Modify Create/Regenerate to pre-create a Pending row + enqueue the Hangfire job. Migrate `UploadVariantImage` auto-embed to the Pending-row pattern. Frontend: extend `imageEmbeddingApi`, new `useEmbeddingStatus` composable with polling, embedding badge UI in `VariantDetail.vue` Images tab per the Code Commenting Standard v3.0.

**Tech Stack:** .NET 10 C#, EF Core + Npgsql, MediatR, Carter, Hangfire, Vue 3 + TypeScript + PrimeVue 5, Vitest, xUnit + FluentAssertions

## Global Constraints

- Result objects, not exceptions — all domain operations return `Result<T>` or `Result`.
- Modules must not reference each other — only Shared dependency.
- Vertical slice feature files under `Features/{Admin|Storefront}/{Feature}/{Action}/`, each with Handler, Request, Response, Endpoint, Validator. Read-only queries may omit Request/Validator.
- Warnings-as-errors — `TreatWarningsAsErrors=true`.
- Code Commenting Standard v3.0 for Vue views (`// Label:` format, `<!-- Section: -->` template tags).
- Single default-model embedding per image (Fashion-CLIP, 512-dim).
- No cross-module references.

---

### Task 1: Domain — EmbeddingStatus enum + ImageEmbedding fields

**Files:**
- Modify: `service/Api/src/Module/Catalog/Domain/Products/Variants/Images/Embeddings/ImageEmbedding.Constant.cs`
- Modify: `service/Api/src/Module/Catalog/Domain/Products/Variants/Images/Embeddings/ImageEmbedding.cs`
- Test: `service/Api/tests/Module.UnitTests/Catalog/Domain/Products/Variants/Images/Embeddings/ImageEmbedding.Method.Tests.cs`

**Interfaces:**
- Produces: `EmbeddingStatus` enum (`Pending`, `Processing`, `Completed`, `Failed`); properties `Status`, `Error`, `HangfireJobId`, `CompletedAtUtc` on `ImageEmbedding`.

- [ ] **Step 1: Add EmbeddingStatus enum to Constant.cs**

Open `service/Api/src/Module/Catalog/Domain/Products/Variants/Images/Embeddings/ImageEmbedding.Constant.cs`. After the closing `}` of the `Constraints` class (before `ModelRole`), insert:

```csharp
public enum EmbeddingStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}
```

- [ ] **Step 2: Add Status/Error/HangfireJobId/CompletedAtUtc to ImageEmbedding.cs**

Open `service/Api/src/Module/Catalog/Domain/Products/Variants/Images/Embeddings/ImageEmbedding.cs`. After the `Dimensions` property (before `#endregion Properties`), insert:

```csharp
/// <summary>Current processing status of the embedding.</summary>
public EmbeddingStatus Status { get; set; } = EmbeddingStatus.Completed;

/// <summary>Error message when Status is Failed.</summary>
public string? Error { get; set; }

/// <summary>Hangfire job identifier for correlation and status polling.</summary>
public string? HangfireJobId { get; set; }

/// <summary>Timestamp when the embedding completed (UTC).</summary>
public DateTimeOffset? CompletedAtUtc { get; set; }
```

- [ ] **Step 3: Add test for default Completed status**

Open `service/Api/tests/Module.UnitTests/Catalog/Domain/Products/Variants/Images/Embeddings/ImageEmbedding.Method.Tests.cs`. After the existing `Create_WithValidParameters` test, add:

```csharp
[Fact(DisplayName = "Create: Should set Status to Completed by default")]
public void Create_ShouldSetStatusToCompleted()
{
    var variantImageId = Guid.NewGuid();
    var vectorData = new float[] { 0.1f, 0.2f, 0.3f };

    var result = ImageEmbeddingMethod.Create(variantImageId, "resnet50", "v1", vectorData);

    result.Status.Should().Be(EmbeddingStatus.Completed);
    result.Error.Should().BeNull();
    result.HangfireJobId.Should().BeNull();
}
```

- [ ] **Step 4: Build and run tests**

```bash
dotnet build service/Api
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~ImageEmbeddingMethod"
```
Expected: build passes, all tests pass.

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Catalog/Domain/Products/Variants/Images/Embeddings/ImageEmbedding.Constant.cs \
        service/Api/src/Module/Catalog/Domain/Products/Variants/Images/Embeddings/ImageEmbedding.cs \
        service/Api/tests/Module.UnitTests/Catalog/Domain/Products/Variants/Images/Embeddings/ImageEmbedding.Method.Tests.cs
git commit -m "feat: add EmbeddingStatus enum and tracking fields to ImageEmbedding domain"
```

---

### Task 2: Domain — CreatePending + status transition methods

**Files:**
- Modify: `service/Api/src/Module/Catalog/Domain/Products/Variants/Images/Embeddings/ImageEmbedding.Method.cs`
- Modify: `service/Api/src/Module/Catalog/Domain/Products/Variants/Images/Embeddings/ImageEmbedding.Result.cs`
- Test: `service/Api/tests/Module.UnitTests/Catalog/Domain/Products/Variants/Images/Embeddings/ImageEmbedding.Method.Tests.cs`

**Interfaces:**
- Produces: `ImageEmbeddingMethod.CreatePending(variantImageId, modelName, modelVersion)`; `MarkPending(entity)`; `MarkProcessing(entity)`; `MarkCompleted(entity, vector, dims, modelVersion)`; `MarkFailed(entity, error)`; `ImageEmbeddingResult.Errors.Conflict(Guid)`; `InvalidStatusTransition(Guid, from, to)`.

- [ ] **Step 1: Add CreatePending and transition methods**

Open `service/Api/src/Module/Catalog/Domain/Products/Variants/Images/Embeddings/ImageEmbedding.Method.cs`. After the existing `Create` method, add (ensure `using Pgvector;` at top):

```csharp
public static ImageEmbedding CreatePending(
    Guid variantImageId,
    string modelName,
    string modelVersion)
{
    return new ImageEmbedding
    {
        Id = Guid.NewGuid(),
        VariantImageId = variantImageId,
        ModelName = modelName,
        ModelVersion = modelVersion,
        Vector = new Vector(Array.Empty<float>()),
        Dimensions = 0,
        Status = EmbeddingStatus.Pending
    };
}

public static Result<ImageEmbedding> MarkPending(ImageEmbedding embedding)
{
    if (embedding.Status != EmbeddingStatus.Completed && embedding.Status != EmbeddingStatus.Failed)
        return Result<ImageEmbedding>.Ok(embedding);

    embedding.Status = EmbeddingStatus.Pending;
    embedding.Error = null;
    embedding.HangfireJobId = null;
    embedding.CompletedAtUtc = null;
    return Result<ImageEmbedding>.Ok(embedding);
}

public static Result<ImageEmbedding> MarkProcessing(ImageEmbedding embedding)
{
    if (embedding.Status != EmbeddingStatus.Pending)
        return ImageEmbeddingResult.Errors.InvalidStatusTransition(
            embedding.Id, embedding.Status, EmbeddingStatus.Processing);

    embedding.Status = EmbeddingStatus.Processing;
    return Result<ImageEmbedding>.Ok(embedding);
}

public static Result<ImageEmbedding> MarkCompleted(
    ImageEmbedding embedding, float[] vector, int dimensions, string modelVersion)
{
    if (embedding.Status != EmbeddingStatus.Processing)
        return ImageEmbeddingResult.Errors.InvalidStatusTransition(
            embedding.Id, embedding.Status, EmbeddingStatus.Completed);

    embedding.Status = EmbeddingStatus.Completed;
    embedding.Vector = new Vector(vector);
    embedding.Dimensions = dimensions;
    embedding.ModelVersion = modelVersion;
    embedding.CompletedAtUtc = DateTimeOffset.UtcNow;
    embedding.Error = null;
    return Result<ImageEmbedding>.Ok(embedding);
}

public static Result<ImageEmbedding> MarkFailed(ImageEmbedding embedding, string error)
{
    if (embedding.Status != EmbeddingStatus.Processing)
        return ImageEmbeddingResult.Errors.InvalidStatusTransition(
            embedding.Id, embedding.Status, EmbeddingStatus.Failed);

    embedding.Status = EmbeddingStatus.Failed;
    embedding.Error = error;
    embedding.CompletedAtUtc = DateTimeOffset.UtcNow;
    return Result<ImageEmbedding>.Ok(embedding);
}
```

- [ ] **Step 2: Add result errors**

Open `service/Api/src/Module/Catalog/Domain/Products/Variants/Images/Embeddings/ImageEmbedding.Result.cs`. In the `Errors.Business` region, before `NotFound`, add:

```csharp
public static Error Conflict(Guid variantImageId) => Error.Conflict(
    code: "ImageEmbedding.Conflict",
    message: $"An embedding with a pending or processing status already exists for variant image '{variantImageId}'.");
```

After `#endregion Business`, add:

```csharp
#region Lifecycle
public static Error InvalidStatusTransition(Guid embeddingId, EmbeddingStatus from, EmbeddingStatus to)
    => Error.Validation(
        code: "ImageEmbedding.InvalidStatusTransition",
        message: $"Cannot transition embedding '{embeddingId}' from {from} to {to}.");
#endregion
```

- [ ] **Step 3: Write tests**

Open `service/Api/tests/Module.UnitTests/Catalog/Domain/Products/Variants/Images/Embeddings/ImageEmbedding.Method.Tests.cs`. Append before the closing `}` of the class:

```csharp
[Fact(DisplayName = "CreatePending: Should create embedding with Pending status")]
public void CreatePending_ShouldCreatePendingEmbedding()
{
    var variantImageId = Guid.NewGuid();
    var result = ImageEmbeddingMethod.CreatePending(variantImageId, "fashion-clip", "v2");
    result.Status.Should().Be(EmbeddingStatus.Pending);
    result.Dimensions.Should().Be(0);
    result.HangfireJobId.Should().BeNull();
    result.Error.Should().BeNull();
}

[Fact(DisplayName = "MarkProcessing: Should transition Pending to Processing")]
public void MarkProcessing_ShouldTransitionPendingToProcessing()
{
    var embedding = ImageEmbeddingMethod.CreatePending(Guid.NewGuid(), "m", "v1");
    var result = ImageEmbeddingMethod.MarkProcessing(embedding);
    result.IsSuccess.Should().BeTrue();
    embedding.Status.Should().Be(EmbeddingStatus.Processing);
}

[Fact(DisplayName = "MarkProcessing: Should fail when not Pending")]
public void MarkProcessing_ShouldFail_WhenNotPending()
{
    var embedding = ImageEmbeddingMethod.Create(Guid.NewGuid(), "m", "v1", [0.1f]);
    var result = ImageEmbeddingMethod.MarkProcessing(embedding);
    result.IsFailure.Should().BeTrue();
}

[Fact(DisplayName = "MarkCompleted: Should store vector and transition")]
public void MarkCompleted_ShouldStoreVectorAndTransition()
{
    var embedding = ImageEmbeddingMethod.CreatePending(Guid.NewGuid(), "m", "v1");
    ImageEmbeddingMethod.MarkProcessing(embedding);
    var vector = new float[] { 0.1f, 0.2f, 0.3f };

    var result = ImageEmbeddingMethod.MarkCompleted(embedding, vector, 3, "v2");

    result.IsSuccess.Should().BeTrue();
    embedding.Status.Should().Be(EmbeddingStatus.Completed);
    embedding.Dimensions.Should().Be(3);
    embedding.ModelVersion.Should().Be("v2");
    embedding.CompletedAtUtc.Should().NotBeNull();
}

[Fact(DisplayName = "MarkCompleted: Should fail when not Processing")]
public void MarkCompleted_ShouldFail_WhenNotProcessing()
{
    var embedding = ImageEmbeddingMethod.CreatePending(Guid.NewGuid(), "m", "v1");
    var result = ImageEmbeddingMethod.MarkCompleted(embedding, [0.1f], 1, "v1");
    result.IsFailure.Should().BeTrue();
}

[Fact(DisplayName = "MarkFailed: Should set error and transition")]
public void MarkFailed_ShouldSetErrorAndTransition()
{
    var embedding = ImageEmbeddingMethod.CreatePending(Guid.NewGuid(), "m", "v1");
    ImageEmbeddingMethod.MarkProcessing(embedding);

    var result = ImageEmbeddingMethod.MarkFailed(embedding, "Inference timeout");

    result.IsSuccess.Should().BeTrue();
    embedding.Status.Should().Be(EmbeddingStatus.Failed);
    embedding.Error.Should().Be("Inference timeout");
    embedding.CompletedAtUtc.Should().NotBeNull();
}

[Fact(DisplayName = "MarkFailed: Should fail when not Processing")]
public void MarkFailed_ShouldFail_WhenNotProcessing()
{
    var embedding = ImageEmbeddingMethod.CreatePending(Guid.NewGuid(), "m", "v1");
    var result = ImageEmbeddingMethod.MarkFailed(embedding, "error");
    result.IsFailure.Should().BeTrue();
}

[Fact(DisplayName = "MarkPending: Should reset Completed to Pending")]
public void MarkPending_ShouldResetToPending()
{
    var embedding = ImageEmbeddingMethod.Create(Guid.NewGuid(), "m", "v1", [0.1f]);
    embedding.HangfireJobId = "job-1";
    embedding.Error = "old error";

    var result = ImageEmbeddingMethod.MarkPending(embedding);

    result.IsSuccess.Should().BeTrue();
    embedding.Status.Should().Be(EmbeddingStatus.Pending);
    embedding.Error.Should().BeNull();
    embedding.HangfireJobId.Should().BeNull();
}

[Fact(DisplayName = "MarkPending: Should be no-op when already Pending")]
public void MarkPending_ShouldBeNoOp_WhenAlreadyPending()
{
    var embedding = ImageEmbeddingMethod.CreatePending(Guid.NewGuid(), "m", "v1");
    var result = ImageEmbeddingMethod.MarkPending(embedding);
    result.IsSuccess.Should().BeTrue();
    embedding.Status.Should().Be(EmbeddingStatus.Pending);
}
```

- [ ] **Step 4: Verify tests pass**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~ImageEmbeddingMethod"
```
Expected: 11 tests pass (2 existing + 9 new).

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Catalog/Domain/Products/Variants/Images/Embeddings/ImageEmbedding.Method.cs \
        service/Api/src/Module/Catalog/Domain/Products/Variants/Images/Embeddings/ImageEmbedding.Result.cs \
        service/Api/tests/Module.UnitTests/Catalog/Domain/Products/Variants/Images/Embeddings/ImageEmbedding.Method.Tests.cs
git commit -m "feat: add status transition methods and CreatePending to ImageEmbedding domain"
```

---

### Task 3: EF Core Migration

**Files:**
- Create: `service/Api/src/Migrations/<timestamp>_AddEmbeddingStatusColumns.cs`

- [ ] **Step 1: Add the migration**

```bash
dotnet ef migrations add AddEmbeddingStatusColumns \
  --project service/Api/src/Migrations \
  --startup-project service/Api/src/Api
```

- [ ] **Step 2: Verify generated migration**

Open the generated migration. Confirm it adds: `Status` (int, default 2 = Completed), `Error` (text, nullable), `HangfireJobId` (text, nullable), `CompletedAtUtc` (timestamptz, nullable).

- [ ] **Step 3: Build**

```bash
dotnet build service/Api
```
Expected: build passes.

- [ ] **Step 4: Run all tests**

```bash
dotnet test
```
Expected: all tests pass.

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Migrations/
git commit -m "feat: add EmbeddingStatus migration with Status/Error/HangfireJobId/CompletedAtUtc columns"
```

---

### Task 4: Orchestrator — RunAsync interface, loggers, and implementation

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Services/ImageEmbedding.Orchestrator.Interface.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Services/ImageEmbedding.Orchestrator.Loggers.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Services/ImageEmbedding.Orchestrator.cs`
- Create: `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Services/EmbeddingOrchestrator.RunAsync.Tests.cs`

**Interfaces:**
- Produces: `IEmbeddingOrchestrator.RunAsync(Guid embeddingId, CancellationToken ct)` returns `Task<Result>`.

- [ ] **Step 1: Add RunAsync to interface**

Open `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Services/ImageEmbedding.Orchestrator.Interface.cs`. After `GenerateAndPersistFromBytesAsync`, add:

```csharp
Task<Result> RunAsync(Guid embeddingId, CancellationToken ct = default);
```

- [ ] **Step 2: Add log events**

Open `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Services/ImageEmbedding.Orchestrator.Loggers.cs`. Append inside `Loggers`:

```csharp
[LoggerMessage(EventId = 6003, Level = LogLevel.Information,
    Message = "RunAsync started EmbeddingId={EmbeddingId} VariantImageId={VariantImageId} Model={ModelName}")]
public static partial void RunStarted(ILogger logger, Guid EmbeddingId, Guid VariantImageId, string ModelName);

[LoggerMessage(EventId = 6004, Level = LogLevel.Information,
    Message = "RunAsync processing EmbeddingId={EmbeddingId}")]
public static partial void RunProcessing(ILogger logger, Guid EmbeddingId);

[LoggerMessage(EventId = 6005, Level = LogLevel.Information,
    Message = "RunAsync completed EmbeddingId={EmbeddingId} Dimensions={Dimensions}")]
public static partial void RunCompleted(ILogger logger, Guid EmbeddingId, int Dimensions);

[LoggerMessage(EventId = 6006, Level = LogLevel.Error,
    Message = "RunAsync failed EmbeddingId={EmbeddingId} Error={Error}")]
public static partial void RunFailed(ILogger logger, Guid EmbeddingId, string Error);
```

- [ ] **Step 3: Add RunAsync implementation**

Open `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Services/ImageEmbedding.Orchestrator.cs`. Before the closing `}` of the class (after `IsForeignKeyViolation`), add:

```csharp
public async Task<Result> RunAsync(Guid embeddingId, CancellationToken ct = default)
{
    var embedding = await _dbContext.Set<ImageEmbedding>()
        .FirstOrDefaultAsync(e => e.Id == embeddingId, ct);
    if (embedding is null)
    {
        Loggers.RunFailed(_logger, embeddingId, "Embedding not found");
        return ImageEmbeddingResult.Errors.NotFound(embeddingId);
    }

    Loggers.RunStarted(_logger, embeddingId, embedding.VariantImageId, embedding.ModelName);

    var markResult = ImageEmbeddingMethod.MarkProcessing(embedding);
    if (markResult.IsFailure)
    {
        Loggers.RunFailed(_logger, embeddingId,
            $"Status transition failed: {markResult.Errors.First().Message}");
        return markResult.Errors;
    }
    await _dbContext.SaveChangesAsync(ct);

    Loggers.RunProcessing(_logger, embeddingId);

    var image = await _dbContext.Set<VariantImage>()
        .FirstOrDefaultAsync(x => x.Id == embedding.VariantImageId, ct);
    if (image is null)
    {
        ImageEmbeddingMethod.MarkFailed(embedding, "Image was deleted");
        await _dbContext.SaveChangesAsync(ct);
        Loggers.RunFailed(_logger, embeddingId, "Image was deleted");
        return Result.Ok();
    }

    if (string.IsNullOrEmpty(image.Url))
    {
        ImageEmbeddingMethod.MarkFailed(embedding, "VariantImage has no public URL");
        await _dbContext.SaveChangesAsync(ct);
        Loggers.RunFailed(_logger, embeddingId, "No public URL");
        return Result.Ok();
    }

    var request = new EmbeddingRequest { ImageUrl = image.Url, Model = embedding.ModelName };
    var inferenceResult = await _inferenceClient.CreateEmbeddingAsync(request, ct);
    if (inferenceResult.IsFailure)
    {
        var errorMsg = inferenceResult.Errors.First().Message;
        ImageEmbeddingMethod.MarkFailed(embedding, errorMsg);
        await _dbContext.SaveChangesAsync(ct);
        Loggers.RunFailed(_logger, embeddingId, errorMsg);
        return Result.Ok();
    }

    var inference = inferenceResult.Value;
    var completeResult = ImageEmbeddingMethod.MarkCompleted(
        embedding, inference.Vector.ToArray(), inference.Dimension, inference.ModelVersion);
    if (completeResult.IsFailure)
    {
        Loggers.RunFailed(_logger, embeddingId,
            $"MarkCompleted failed: {completeResult.Errors.First().Message}");
        return completeResult.Errors;
    }
    await _dbContext.SaveChangesAsync(ct);

    Loggers.RunCompleted(_logger, embeddingId, inference.Dimension);
    return Result.Ok();
}

public static bool IsForeignKeyViolation(DbUpdateException ex)  // make this static if not already
{
    return ex.InnerException is Npgsql.PostgresException postgresEx
        && postgresEx.SqlState == "23503";
}
```

Note: Check if `IsForeignKeyViolation` is already `static` — make it so. Also ensure `RunAsync` is inside the class (before `IsForeignKeyViolation`).

- [ ] **Step 4: Write orchestrator tests**

Create `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Services/EmbeddingOrchestrator.RunAsync.Tests.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Domain.Products.Variants.Images.Embeddings;
using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Clients;
using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Models;
using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Services;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "EmbeddingOrchestrator")]
public class EmbeddingOrchestratorRunAsyncTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IInferenceClient> _inferenceClientMock;
    private readonly EmbeddingOrchestrator _orchestrator;

    public EmbeddingOrchestratorRunAsyncTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(ImageEmbedding).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _inferenceClientMock = new Mock<IInferenceClient>();
        _orchestrator = new EmbeddingOrchestrator(
            _inferenceClientMock.Object, _dbContext,
            Options.Create(new EmbeddingOrchestratorOptions { DefaultModel = "fashion-clip" }),
            NullLogger<EmbeddingOrchestrator>.Instance);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "RunAsync: Happy path Pending -> Completed")]
    public async Task RunAsync_ShouldComplete_Successfully()
    {
        var image = VariantImageMethod.Create(
            "image/jpeg", "test.jpg", 1000, "https://cdn.test.com/test.jpg",
            "u/test.jpg", position: 0, variantId: Guid.NewGuid()).Value;
        _dbContext.Set<VariantImage>().Add(image);
        var embedding = ImageEmbeddingMethod.CreatePending(image.Id, "fashion-clip", "v1");
        _dbContext.Set<ImageEmbedding>().Add(embedding);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _inferenceClientMock.Setup(c => c.CreateEmbeddingAsync(
            It.Is<EmbeddingRequest>(r => r.ImageUrl == image.Url), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<EmbeddingResponse>.Ok(new EmbeddingResponse
                { Vector = [0.1f, 0.2f], Dimension = 2, ModelVersion = "v1" }));

        var result = await _orchestrator.RunAsync(embedding.Id, TestContext.Current.CancellationToken);
        result.IsSuccess.Should().BeTrue();

        var updated = await _dbContext.Set<ImageEmbedding>().FindAsync(embedding.Id);
        updated!.Status.Should().Be(EmbeddingStatus.Completed);
        updated.Dimensions.Should().Be(2);
        updated.CompletedAtUtc.Should().NotBeNull();
    }

    [Fact(DisplayName = "RunAsync: Should mark Failed on inference failure")]
    public async Task RunAsync_ShouldFail_WhenInferenceFails()
    {
        var image = VariantImageMethod.Create(
            "image/jpeg", "test.jpg", 1000, "https://cdn.test.com/test.jpg",
            "u/test.jpg", position: 0, variantId: Guid.NewGuid()).Value;
        _dbContext.Set<VariantImage>().Add(image);
        var embedding = ImageEmbeddingMethod.CreatePending(image.Id, "fashion-clip", "v1");
        _dbContext.Set<ImageEmbedding>().Add(embedding);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _inferenceClientMock.Setup(c => c.CreateEmbeddingAsync(
            It.IsAny<EmbeddingRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImageEmbeddingResult.Errors.RequestTimeout);

        await _orchestrator.RunAsync(embedding.Id, TestContext.Current.CancellationToken);

        var updated = await _dbContext.Set<ImageEmbedding>().FindAsync(embedding.Id);
        updated!.Status.Should().Be(EmbeddingStatus.Failed);
        updated.Error.Should().NotBeNullOrEmpty();
    }

    [Fact(DisplayName = "RunAsync: Should mark Failed when image deleted")]
    public async Task RunAsync_ShouldFail_WhenImageNotFound()
    {
        var embedding = ImageEmbeddingMethod.CreatePending(Guid.NewGuid(), "fashion-clip", "v1");
        _dbContext.Set<ImageEmbedding>().Add(embedding);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        await _orchestrator.RunAsync(embedding.Id, TestContext.Current.CancellationToken);

        var updated = await _dbContext.Set<ImageEmbedding>().FindAsync(embedding.Id);
        updated!.Status.Should().Be(EmbeddingStatus.Failed);
        updated.Error.Should().Contain("Image was deleted");
    }

    [Fact(DisplayName = "RunAsync: Should return failure when embedding not found")]
    public async Task RunAsync_ShouldReturnFailure_WhenEmbeddingNotFound()
    {
        var result = await _orchestrator.RunAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
    }
}
```

- [ ] **Step 5: Verify tests pass**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~EmbeddingOrchestratorRunAsync"
```
Expected: 4 tests pass.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Services/ImageEmbedding.Orchestrator.Interface.cs \
        service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Services/ImageEmbedding.Orchestrator.Loggers.cs \
        service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Services/ImageEmbedding.Orchestrator.cs \
        service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Services/
git commit -m "feat: add RunAsync method to embedding orchestrator with status transitions"
```

---

### Task 5: Response model — add Status fields

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Models/ImageEmbedding.Model.Response.cs`

- [ ] **Step 1: Add fields**

After `CreatedAtUtc`, add:

```csharp
public string Status { get; init; } = "Completed";
public string? Error { get; init; }
public string? HangfireJobId { get; init; }
public DateTimeOffset? CompletedAtUtc { get; init; }
```

- [ ] **Step 2: Build**

```bash
dotnet build service/Api
```

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Models/ImageEmbedding.Model.Response.cs
git commit -m "feat: add Status/Error/HangfireJobId/CompletedAtUtc to EmbeddingDetailResponse"
```

---

### Task 6: CatalogFeature route constants

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Admin.cs`

- [ ] **Step 1: Add Get and Delete routes**

After the `Regenerate` class inside `VariantImageEmbeddings` (before closing `}}`), add:

```csharp
public static class Get
{
    public const string Route = $"{BaseRoute}/{{variantImageId:guid}}";
    public const string Description = "Get the embedding for a variant image";
    public const string Summary = "Get image embedding";
    public static PermissionMetadata Permission => CatalogFeatureMetadata.VariantImages.ManageEmbeddings;
}

public static class Delete
{
    public const string Route = $"{BaseRoute}/{{variantImageId:guid}}";
    public const string Description = "Delete the embedding for a variant image";
    public const string Summary = "Delete image embedding";
    public static PermissionMetadata Permission => CatalogFeatureMetadata.VariantImages.ManageEmbeddings;
}
```

- [ ] **Step 2: Build + commit**

```bash
dotnet build service/Api
git add service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Admin.cs
git commit -m "feat: add Get and Delete route constants for variant image embeddings"
```

---

### Task 7: GetEmbedding feature (new vertical slice)

**Files:**
- Create: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Get/GetEmbedding.cs`
- Create: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Get/GetEmbedding.Endpoint.cs`
- Create: `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Get/GetEmbedding.Tests.cs`

**Interfaces:**
- Produces: `GET /api/catalog/variant-image-embeddings/{variantImageId:guid}` → `EmbeddingDetailResponse` (with Status) or 404.

- [ ] **Step 1: Write the failing test**

Create `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Get/GetEmbedding.Tests.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Module.Catalog.Domain.Products.Variants.Images.Embeddings;
using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Get;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Get;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "GetEmbedding")]
public class GetEmbeddingTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetEmbedding.QueryHandler _handler;

    public GetEmbeddingTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(ImageEmbedding).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetEmbedding.QueryHandler(_dbContext);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "Handle: Returns EmbeddingDetailResponse with status fields")]
    public async Task Handle_ShouldReturnEmbeddingDetail()
    {
        var variantImageId = Guid.NewGuid();
        var embedding = ImageEmbeddingMethod.Create(variantImageId, "fashion-clip", "v1", [0.1f, 0.2f]);
        embedding.Status = EmbeddingStatus.Completed;
        embedding.HangfireJobId = "job-123";
        embedding.CompletedAtUtc = DateTimeOffset.UtcNow;
        _dbContext.Set<ImageEmbedding>().Add(embedding);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetEmbedding.Query(variantImageId), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Completed");
        result.Value.HangfireJobId.Should().Be("job-123");
        result.Value.CompletedAtUtc.Should().NotBeNull();
    }

    [Fact(DisplayName = "Handle: Returns 404 when no embedding exists")]
    public async Task Handle_ShouldReturnNotFound()
    {
        var result = await _handler.Handle(
            new GetEmbedding.Query(Guid.NewGuid()), TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
        result.Errors.First().Code.Should().Be("ImageEmbedding.NotFound");
    }
}
```

- [ ] **Step 2: Verify test fails (compilation error)**

```bash
dotnet build service/Api
```
Expected: compilation fails — `GetEmbedding`, `Query`, `QueryHandler` not found.

- [ ] **Step 3: Write GetEmbedding handler**

Create `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Get/GetEmbedding.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Module.Catalog.Domain.Products.Variants.Images.Embeddings;
using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Get;

public static partial class GetEmbedding
{
    public sealed record Query(Guid VariantImageId) : IQuery<EmbeddingDetailResponse>;

    public sealed class QueryHandler(IApplicationDbContext dbContext)
        : IQueryHandler<Query, EmbeddingDetailResponse>
    {
        public async Task<Result<EmbeddingDetailResponse>> Handle(
            Query query, CancellationToken cancellationToken)
        {
            var embedding = await dbContext.Set<ImageEmbedding>()
                .FirstOrDefaultAsync(e => e.VariantImageId == query.VariantImageId, cancellationToken);

            if (embedding is null)
                return ImageEmbeddingResult.Errors.NotFound(query.VariantImageId);

            return Result<EmbeddingDetailResponse>.Ok(new EmbeddingDetailResponse
            {
                Id = embedding.Id,
                VariantImageId = embedding.VariantImageId,
                ModelName = embedding.ModelName,
                ModelVersion = embedding.ModelVersion,
                Vector = embedding.Vector?.ToArray() ?? [],
                Dimensions = embedding.Dimensions,
                CreatedAtUtc = embedding.CreatedAtUtc,
                Status = embedding.Status.ToString(),
                Error = embedding.Error,
                HangfireJobId = embedding.HangfireJobId,
                CompletedAtUtc = embedding.CompletedAtUtc
            });
        }
    }
}
```

- [ ] **Step 4: Write GetEmbedding endpoint**

Create `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Get/GetEmbedding.Endpoint.cs`:

```csharp
using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Models;
using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Get;

public static partial class GetEmbedding
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                CatalogFeature.Admin.VariantImageEmbeddings.Get.Route,
                async (Guid variantImageId, ISender sender, CancellationToken ct) =>
                {
                    var result = await sender.Send(new Query(variantImageId), ct);
                    return result.ToResult();
                })
            .WithName(nameof(GetEmbedding))
            .WithTags(CatalogFeature.Tags.Variant)
            .HasPermission(CatalogFeature.Admin.VariantImageEmbeddings.Get.Permission)
            .WithSummary(CatalogFeature.Admin.VariantImageEmbeddings.Get.Summary)
            .WithDescription(CatalogFeature.Admin.VariantImageEmbeddings.Get.Description)
            .Produces<Result<EmbeddingDetailResponse>>(StatusCodes.Status200OK)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
```

- [ ] **Step 5: Verify tests pass**

```bash
dotnet build service/Api
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~GetEmbedding"
```
Expected: 2 tests pass.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Get/ \
        service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Get/
git commit -m "feat: add GetEmbedding GET endpoint for variant image embeddings"
```

---

### Task 8: Modify CreateEmbedding — Hangfire job + Pending row

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Create/ImageEmbedding.Create.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Create/ImageEmbedding.Create.Endpoint.cs`
- Create: `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Create/CreateEmbedding.Tests.cs`

**Interfaces:**
- Consumes: `ImageEmbeddingMethod.CreatePending`, `IBackgroundJobClient`, `IEmbeddingOrchestrator.RunAsync`.
- Produces: Modified handler — pre-creates Pending row, enqueues job, returns 201. Conflict (409) if Pending exists.

- [ ] **Step 1: Write failing test**

Create `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Create/CreateEmbedding.Tests.cs`:

```csharp
using Hangfire;
using Hangfire.States;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Module.Catalog.Domain.Products.Variants.Images.Embeddings;
using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Create;
using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Services;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Create;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "CreateEmbedding")]
public class CreateEmbeddingTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IBackgroundJobClient> _bgJobMock;
    private readonly Mock<IEmbeddingOrchestrator> _orchestratorMock;
    private readonly CreateEmbedding.CommandHandler _handler;

    public CreateEmbeddingTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(ImageEmbedding).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _bgJobMock = new Mock<IBackgroundJobClient>();
        _orchestratorMock = new Mock<IEmbeddingOrchestrator>();
        _handler = new CreateEmbedding.CommandHandler(
            _orchestratorMock.Object, _dbContext, _bgJobMock.Object,
            NullLogger<CreateEmbedding.CommandHandler>.Instance);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "Handle: Creates Pending row and enqueues Hangfire job")]
    public async Task Handle_ShouldCreatePendingAndEnqueue()
    {
        var variantImageId = Guid.NewGuid();
        var jobId = "bg-job-1";
        _bgJobMock.Setup(b => b.Create(
            It.IsAny<Job>(), It.IsAny<EnqueuedState>())).Returns(jobId);

        var command = new CreateEmbedding.Command(new CreateEmbedding.Request
            { VariantImageId = variantImageId, ModelName = "fashion-clip" });

        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Pending");
        result.Value.HangfireJobId.Should().Be(jobId);

        var saved = await _dbContext.Set<ImageEmbedding>()
            .FirstOrDefaultAsync(e => e.VariantImageId == variantImageId);
        saved.Should().NotBeNull();
        saved!.Status.Should().Be(EmbeddingStatus.Pending);
        _bgJobMock.Verify(b => b.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>()), Times.Once);
    }

    [Fact(DisplayName = "Handle: Returns Conflict when Pending row exists")]
    public async Task Handle_ShouldReturnConflict_WhenPendingExists()
    {
        var variantImageId = Guid.NewGuid();
        _dbContext.Set<ImageEmbedding>().Add(
            ImageEmbeddingMethod.CreatePending(variantImageId, "fashion-clip", "v1"));
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var command = new CreateEmbedding.Command(new CreateEmbedding.Request
            { VariantImageId = variantImageId, ModelName = "fashion-clip" });

        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);
        result.IsFailure.Should().BeTrue();
        result.Errors.First().Code.Should().Be("ImageEmbedding.Conflict");
    }
}
```

- [ ] **Step 2: Verify test fails (handler still sync)**

```bash
dotnet build service/Api
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~CreateEmbeddingTests"
```
Expected: tests fail — handler runs synchronously, doesn't create Pending/enqueue.

- [ ] **Step 3: Rewrite CreateEmbedding handler**

Open `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Create/ImageEmbedding.Create.cs`. Replace the entire `CommandHandler` class with:

```csharp
public sealed class CommandHandler(
    IEmbeddingOrchestrator orchestrator,
    IApplicationDbContext dbContext,
    IBackgroundJobClient? backgroundJobClient,
    ILogger<CommandHandler> logger)
    : ICommandHandler<Command, EmbeddingDetailResponse>
{
    public async Task<Result<EmbeddingDetailResponse>> Handle(
        Command command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var modelName = string.IsNullOrEmpty(request.ModelName)
            ? VariantImageConstant.Defaults.DefaultEmbeddingModel
            : request.ModelName;

        var existingPending = await dbContext.Set<ImageEmbedding>()
            .AnyAsync(e => e.VariantImageId == request.VariantImageId
                && e.ModelName == modelName
                && (e.Status == EmbeddingStatus.Pending || e.Status == EmbeddingStatus.Processing),
                cancellationToken);
        if (existingPending)
            return ImageEmbeddingResult.Errors.Conflict(request.VariantImageId);

        var embedding = ImageEmbeddingMethod.CreatePending(request.VariantImageId, modelName, "1.0");
        dbContext.Set<ImageEmbedding>().Add(embedding);

        var jobId = backgroundJobClient?.Create<IEmbeddingOrchestrator>(
            o => o.RunAsync(embedding.Id, CancellationToken.None),
            new EnqueuedState());
        embedding.HangfireJobId = jobId;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<EmbeddingDetailResponse>.Created(
            new EmbeddingDetailResponse
            {
                Id = embedding.Id,
                VariantImageId = embedding.VariantImageId,
                ModelName = embedding.ModelName,
                ModelVersion = embedding.ModelVersion,
                Vector = [],
                Dimensions = 0,
                Status = embedding.Status.ToString(),
                Error = embedding.Error,
                HangfireJobId = embedding.HangfireJobId,
                CompletedAtUtc = embedding.CompletedAtUtc
            },
            ImageEmbeddingResult.Success.Created(embedding.Id));
    }
}
```

Add these usings (after existing):
```csharp
using Hangfire;
using Hangfire.States;
using Microsoft.EntityFrameworkCore;
```

Also remove the old synchronous `orchestrator.GenerateAndPersistAsync` call — the new handler doesn't call it directly; it only enqueues.

- [ ] **Step 4: Update endpoint to add 409 response**

Open `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Create/ImageEmbedding.Create.Endpoint.cs`. Add `.Produces<Result>(StatusCodes.Status409Conflict)` among the existing `.Produces<>` chain (after the 404 line).

- [ ] **Step 5: Verify tests pass**

```bash
dotnet build service/Api
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~CreateEmbeddingTests"
```
Expected: 2 tests pass.

- [ ] **Step 6: Run broader suite to check nothing broke**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Catalog"
```
Expected: all catalog tests pass.

- [ ] **Step 7: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Create/ \
        service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Create/
git commit -m "feat: convert CreateEmbedding to Hangfire job with Pending-row pattern"
```

---

### Task 9: Modify RegenerateEmbedding — Hangfire job + MarkPending

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Regenerate/ImageEmbedding.Regenerate.cs`
- Create: `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Regenerate/RegenerateEmbedding.Tests.cs`

**Interfaces:**
- Consumes: `ImageEmbeddingMethod.MarkPending`, `IBackgroundJobClient`, `IEmbeddingOrchestrator.RunAsync`.
- Produces: Modified handler — transitions to Pending (or creates if absent), enqueues job, returns 200.

- [ ] **Step 1: Write failing test**

Create `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Regenerate/RegenerateEmbedding.Tests.cs`:

```csharp
using Hangfire;
using Hangfire.States;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Module.Catalog.Domain.Products.Variants.Images.Embeddings;
using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Regenerate;
using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Services;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Regenerate;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "RegenerateEmbedding")]
public class RegenerateEmbeddingTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IBackgroundJobClient> _bgJobMock;
    private readonly Mock<IEmbeddingOrchestrator> _orchestratorMock;
    private readonly RegenerateEmbedding.CommandHandler _handler;

    public RegenerateEmbeddingTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(ImageEmbedding).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _bgJobMock = new Mock<IBackgroundJobClient>();
        _orchestratorMock = new Mock<IEmbeddingOrchestrator>();
        _handler = new RegenerateEmbedding.CommandHandler(
            _orchestratorMock.Object, _dbContext, _bgJobMock.Object,
            NullLogger<RegenerateEmbedding.CommandHandler>.Instance);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "Handle: Transitions existing Completed to Pending and enqueues")]
    public async Task Handle_ShouldResetExistingAndEnqueue()
    {
        var variantImageId = Guid.NewGuid();
        var embedding = ImageEmbeddingMethod.Create(variantImageId, "fashion-clip", "v1", [0.1f]);
        _dbContext.Set<ImageEmbedding>().Add(embedding);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var jobId = "regenerate-job";
        _bgJobMock.Setup(b => b.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>())).Returns(jobId);

        var command = new RegenerateEmbedding.Command(new RegenerateEmbedding.Request
            { VariantImageId = variantImageId, ModelName = "fashion-clip", ModelVersion = "v2" });

        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Pending");
        result.Value.HangfireJobId.Should().Be(jobId);

        var saved = await _dbContext.Set<ImageEmbedding>()
            .FirstAsync(e => e.VariantImageId == variantImageId);
        saved.Status.Should().Be(EmbeddingStatus.Pending);
        saved.Error.Should().BeNull();
        _bgJobMock.Verify(b => b.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>()), Times.Once);
    }

    [Fact(DisplayName = "Handle: Creates new Pending row if embedding was deleted")]
    public async Task Handle_ShouldCreateNewRow_WhenNoneExists()
    {
        var variantImageId = Guid.NewGuid();
        var jobId = "regenerate-job-2";
        _bgJobMock.Setup(b => b.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>())).Returns(jobId);

        var command = new RegenerateEmbedding.Command(new RegenerateEmbedding.Request
            { VariantImageId = variantImageId, ModelName = "fashion-clip" });

        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Pending");

        var saved = await _dbContext.Set<ImageEmbedding>()
            .FirstOrDefaultAsync(e => e.VariantImageId == variantImageId);
        saved.Should().NotBeNull();
        saved!.Status.Should().Be(EmbeddingStatus.Pending);
    }
}
```

- [ ] **Step 2: Verify test fails**

```bash
dotnet build service/Api
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~RegenerateEmbeddingTests"
```
Expected: tests fail.

- [ ] **Step 3: Rewrite RegenerateEmbedding handler**

Open `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Regenerate/ImageEmbedding.Regenerate.cs`. Replace the `CommandHandler`:

```csharp
public sealed class CommandHandler(
    IEmbeddingOrchestrator orchestrator,
    IApplicationDbContext dbContext,
    IBackgroundJobClient? backgroundJobClient,
    ILogger<CommandHandler> logger)
    : ICommandHandler<Command, EmbeddingDetailResponse>
{
    public async Task<Result<EmbeddingDetailResponse>> Handle(
        Command command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var modelName = string.IsNullOrEmpty(request.ModelName)
            ? VariantImageConstant.Defaults.DefaultEmbeddingModel
            : request.ModelName;
        var modelVersion = request.ModelVersion ?? "1.0";

        var existing = await dbContext.Set<ImageEmbedding>()
            .FirstOrDefaultAsync(e => e.VariantImageId == request.VariantImageId
                && e.ModelName == modelName, cancellationToken);

        ImageEmbedding embedding;
        if (existing is null)
        {
            embedding = ImageEmbeddingMethod.CreatePending(request.VariantImageId, modelName, modelVersion);
            dbContext.Set<ImageEmbedding>().Add(embedding);
        }
        else
        {
            var pendingResult = ImageEmbeddingMethod.MarkPending(existing);
            if (pendingResult.IsFailure)
                return pendingResult.Errors;
            embedding = existing;
        }

        var jobId = backgroundJobClient?.Create<IEmbeddingOrchestrator>(
            o => o.RunAsync(embedding.Id, CancellationToken.None),
            new EnqueuedState());
        embedding.HangfireJobId = jobId;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<EmbeddingDetailResponse>.Ok(new EmbeddingDetailResponse
        {
            Id = embedding.Id,
            VariantImageId = embedding.VariantImageId,
            ModelName = embedding.ModelName,
            ModelVersion = embedding.ModelVersion,
            Vector = embedding.Vector?.ToArray() ?? [],
            Dimensions = embedding.Dimensions,
            Status = embedding.Status.ToString(),
            Error = embedding.Error,
            HangfireJobId = embedding.HangfireJobId,
            CompletedAtUtc = embedding.CompletedAtUtc
        });
    }
}
```

Add usings:
```csharp
using Hangfire;
using Hangfire.States;
using Microsoft.EntityFrameworkCore;
```

- [ ] **Step 4: Verify tests pass**

```bash
dotnet build service/Api
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~RegenerateEmbeddingTests"
```
Expected: 2 tests pass.

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Regenerate/ \
        service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Regenerate/
git commit -m "feat: convert RegenerateEmbedding to Hangfire job with MarkPending"
```

---

### Task 10: DeleteEmbedding feature (new vertical slice)

> Follows the `DeleteVariantImage` house pattern (ICommand<Response> with Message) — user ruling 2026-08-04.

**Files:**
- Create: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Delete/DeleteEmbedding.cs`
- Create: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Delete/DeleteEmbedding.Response.cs`
- Create: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Delete/DeleteEmbedding.Endpoint.cs`
- Create: `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Delete/DeleteEmbedding.Tests.cs`

**Interfaces:**
- Produces: `DELETE /api/catalog/variant-image-embeddings/{variantImageId:guid}` → 200 + `Response.Message` (`ImageEmbeddingResult.Success.Deleted(id)`) or 404.

- [ ] **Step 1: Write failing test**

Create `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Delete/DeleteEmbedding.Tests.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Module.Catalog.Domain.Products.Variants.Images.Embeddings;
using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Delete;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Delete;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "DeleteEmbedding")]
public class DeleteEmbeddingTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DeleteEmbedding.CommandHandler _handler;

    public DeleteEmbeddingTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(ImageEmbedding).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new DeleteEmbedding.CommandHandler(_dbContext);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "Handle: Removes embedding and returns 200 with message")]
    public async Task Handle_ShouldDeleteAndReturn200()
    {
        var variantImageId = Guid.NewGuid();
        var embedding = ImageEmbeddingMethod.CreatePending(variantImageId, "fashion-clip", "v1");
        _dbContext.Set<ImageEmbedding>().Add(embedding);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var command = new DeleteEmbedding.Command(variantImageId);
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Message.Should().Contain("deleted");

        var deleted = await _dbContext.Set<ImageEmbedding>()
            .FirstOrDefaultAsync(e => e.VariantImageId == variantImageId);
        deleted.Should().BeNull();
    }

    [Fact(DisplayName = "Handle: Returns 404 when no embedding exists")]
    public async Task Handle_ShouldReturnNotFound()
    {
        var command = new DeleteEmbedding.Command(Guid.NewGuid());
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors.First().Code.Should().Be("ImageEmbedding.NotFound");
    }
}
```

- [ ] **Step 2: Write DeleteEmbedding handler + Response**

Create `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Delete/DeleteEmbedding.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Module.Catalog.Domain.Products.Variants.Images.Embeddings;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Delete;

public static partial class DeleteEmbedding
{
    public sealed record Command(Guid VariantImageId) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(
            Command command, CancellationToken cancellationToken)
        {
            var embedding = await dbContext.Set<ImageEmbedding>()
                .FirstOrDefaultAsync(e => e.VariantImageId == command.VariantImageId, cancellationToken);

            if (embedding is null)
                return ImageEmbeddingResult.Errors.NotFound(command.VariantImageId);

            dbContext.Set<ImageEmbedding>().Remove(embedding);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<Response>.Ok(new Response { Message = ImageEmbeddingResult.Success.Deleted(embedding.Id) });
        }
    }
}
```

Create `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Delete/DeleteEmbedding.Response.cs`:

```csharp
namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Delete;

public static partial class DeleteEmbedding
{
    // EXCEPTION: minimal confirmation response — no domain entity
    public sealed record Response
    {
        public string Message { get; init; } = default!;
    }
}
```

- [ ] **Step 3: Write DeleteEmbedding endpoint**

Create `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Delete/DeleteEmbedding.Endpoint.cs`:

```csharp
using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Delete;

public static partial class DeleteEmbedding
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(
                CatalogFeature.Admin.VariantImageEmbeddings.Delete.Route,
                async (Guid variantImageId, ISender sender, CancellationToken ct) =>
                {
                    var result = await sender.Send(new Command(variantImageId), ct);
                    return result.ToResult();
                })
            .WithName(nameof(DeleteEmbedding))
            .WithTags(CatalogFeature.Tags.Variant)
            .HasPermission(CatalogFeature.Admin.VariantImageEmbeddings.Delete.Permission)
            .WithSummary(CatalogFeature.Admin.VariantImageEmbeddings.Delete.Summary)
            .WithDescription(CatalogFeature.Admin.VariantImageEmbeddings.Delete.Description)
            .Produces<Result<DeleteEmbedding.Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
```

- [ ] **Step 4: Verify tests pass**

```bash
dotnet build service/Api
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~DeleteEmbedding"
```
Expected: 2 tests pass.

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Delete/ \
        service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Delete/
git commit -m "feat: add DeleteEmbedding DELETE endpoint for variant image embeddings"
```

---

### Task 11: Migrate UploadVariantImage to Pending-row pattern

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Upload/UploadVariantImage.cs`

- [ ] **Step 1: Replace the fire-and-forget enqueue**

Open `UploadVariantImage.cs`. The existing code (~line 110-117):

```csharp
// Enqueue: Trigger background embedding generation for search-type images
if (imageType == VariantImageType.Search)
{
    var modelName = VariantImageConstant.Defaults.DefaultEmbeddingModel;
    backgroundJobClient?.Create<IEmbeddingOrchestrator>(
        orchestrator => orchestrator.GenerateAndPersistAsync(image.Id, modelName, CancellationToken.None),
        new EnqueuedState());
}
```

Replace with:

```csharp
// Enqueue: Create Pending embedding row and enqueue background job for status tracking
if (imageType == VariantImageType.Search)
{
    var modelName = VariantImageConstant.Defaults.DefaultEmbeddingModel;
    var pendingEmbedding = ImageEmbeddingMethod.CreatePending(image.Id, modelName, "1.0");
    dbContext.Set<ImageEmbedding>().Add(pendingEmbedding);
    await dbContext.SaveChangesAsync(cancellationToken);

    var jobId = backgroundJobClient?.Create<IEmbeddingOrchestrator>(
        orchestrator => orchestrator.RunAsync(pendingEmbedding.Id, CancellationToken.None),
        new EnqueuedState());
    pendingEmbedding.HangfireJobId = jobId;
    await dbContext.SaveChangesAsync(cancellationToken);
}
```

Note: The embedding is saved BEFORE the main `SaveChanges` at line 105 (the image entity is saved there). The new code must run AFTER the image is saved (after `await dbContext.SaveChangesAsync(cancellationToken)` on the image) and needs its own separate `SaveChanges` for the embedding + HangfireJobId update. Check the exact line order: image save is at line 105, the enqueue block starts at line 111. Since the image is already saved, the embedding save is fine inline. The HangfireJobId update needs a second SaveChanges.

- [ ] **Step 2: Add necessary usings**

Ensure `using Module.Catalog.Domain.Products.Variants.Images.Embeddings;` is present at the top.

- [ ] **Step 3: Build and run catalog tests**

```bash
dotnet build service/Api
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Catalog"
```
Expected: all catalog tests pass.

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Upload/UploadVariantImage.cs
git commit -m "feat: migrate UploadVariantImage auto-embed to Pending-row + RunAsync pattern"
```

---

### Task 12: Frontend types — add Status fields to EmbeddingDetailResponse

**Files:**
- Modify: `app/Admin/src/features/catalog/types/imageEmbedding.ts`

- [ ] **Step 1: Add status fields**

Open `app/Admin/src/features/catalog/types/imageEmbedding.ts`. After `createdAtUtc: string`, add:

```typescript
status: 'Pending' | 'Processing' | 'Completed' | 'Failed'
error?: string
hangfireJobId?: string
completedAtUtc?: string
```

- [ ] **Step 2: Verify typecheck**

```bash
cd app/Admin && pnpm exec vue-tsc --build
```
Expected: clean.

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/catalog/types/imageEmbedding.ts
git commit -m "feat: add Status/Error/HangfireJobId/CompletedAtUtc to EmbeddingDetailResponse type"
```

---

### Task 13: Frontend API service — add get + deleteEmbedding

**Files:**
- Modify: `app/Admin/src/features/catalog/services/imageEmbeddingApi.ts`
- Test: `app/Admin/src/features/catalog/__tests__/services/imageEmbeddingApi.spec.ts`

- [ ] **Step 1: Add get and deleteEmbedding methods**

Open `app/Admin/src/features/catalog/services/imageEmbeddingApi.ts`. Add `get` import:

```typescript
import { get, post, put, del } from '@/shared/api/client'
```

After the `regenerate` method, add:

```typescript
static get(variantImageId: string): Promise<Result<EmbeddingDetailResponse>> {
  return get<Result<EmbeddingDetailResponse>>(`${ImageEmbeddingApi.BASE}/${variantImageId}`)
}

static deleteEmbedding(variantImageId: string): Promise<Result<{ message: string }>> {
  return del<Result<{ message: string }>>(`${ImageEmbeddingApi.BASE}/${variantImageId}`)
}
```

- [ ] **Step 2: Add tests**

Open `app/Admin/src/features/catalog/__tests__/services/imageEmbeddingApi.spec.ts`. Add `mockGet`, `mockDel` to the hoisted mocks, add the mock for `get` and `del`, then add tests:

Update the hoisted mock line 3-6 to:

```typescript
const { mockPost, mockPut, mockGet, mockDel } = vi.hoisted(() => ({
  mockPost: vi.fn<(...args: unknown[]) => unknown>(),
  mockPut: vi.fn<(...args: unknown[]) => unknown>(),
  mockGet: vi.fn<(...args: unknown[]) => unknown>(),
  mockDel: vi.fn<(...args: unknown[]) => unknown>(),
}))
```

Update the mock line 9-11 to:

```typescript
vi.mock('@/shared/api/client', () => ({
  post: mockPost,
  put: mockPut,
  get: mockGet,
  del: mockDel,
}))
```

Append before the final closing `}` of the file:

```typescript
describe('ImageEmbeddingApi.get', () => {
  it('calls GET with variantImageId path', async () => {
    const result = { value: embeddingResult.value, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null }
    mockGet.mockResolvedValue(result)
    await ImageEmbeddingApi.get('img-1')
    expect(mockGet).toHaveBeenCalledWith('api/catalog/variant-image-embeddings/img-1')
  })
})

describe('ImageEmbeddingApi.deleteEmbedding', () => {
  it('calls DELETE with variantImageId path', async () => {
    mockDel.mockResolvedValue({ isSuccess: true, statusCode: 200, message: 'Deleted', errors: [], metadata: null })
    await ImageEmbeddingApi.deleteEmbedding('img-1')
    expect(mockDel).toHaveBeenCalledWith('api/catalog/variant-image-embeddings/img-1')
  })
})
```

- [ ] **Step 3: Verify**

```bash
cd app/Admin && pnpm exec vue-tsc --build && pnpm run lint
pnpm run test:unit -- --run src/features/catalog/__tests__/services/imageEmbeddingApi.spec.ts
```
Expected: typecheck clean, lint clean, tests pass.

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/catalog/services/imageEmbeddingApi.ts \
        app/Admin/src/features/catalog/__tests__/services/imageEmbeddingApi.spec.ts
git commit -m "feat: add get and deleteEmbedding methods to ImageEmbeddingApi"
```

---

### Task 14: Frontend composable — useEmbeddingStatus with polling

**Files:**
- Create: `app/Admin/src/features/catalog/composables/useEmbeddingStatus.ts`
- Create: `app/Admin/src/features/catalog/composables/__tests__/useEmbeddingStatus.spec.ts`
- Modify: `app/Admin/src/features/catalog/composables/index.ts`

**Interfaces:**
- Produces: `useEmbeddingStatus(variantImageId)` → `{ embedding, loading, error, poll, refresh }`.

- [ ] **Step 1: Write the composable**

Create `app/Admin/src/features/catalog/composables/useEmbeddingStatus.ts`:

```typescript
import { ref, type Ref } from 'vue'
import { ImageEmbeddingApi } from '../services/imageEmbeddingApi'
import type { EmbeddingDetailResponse } from '../types/imageEmbedding'

const ACTIVE_POLLS = new Map<string, ReturnType<typeof setTimeout>>()

export function useEmbeddingStatus(variantImageId: Ref<string | null>) {
  const embedding = ref<EmbeddingDetailResponse | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function refresh(): Promise<void> {
    if (!variantImageId.value) return
    loading.value = true
    error.value = null
    try {
      const result = await ImageEmbeddingApi.get(variantImageId.value)
      if (result.isSuccess) {
        embedding.value = result.value
        loading.value = false
      } else {
        embedding.value = null
        loading.value = false
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to load embedding'
      embedding.value = null
      loading.value = false
    }
  }

  async function poll(maxAttempts = 20, intervalMs = 1500): Promise<void> {
    const key = variantImageId.value
    if (!key) return

    if (ACTIVE_POLLS.has(key)) {
      clearTimeout(ACTIVE_POLLS.get(key)!)
      ACTIVE_POLLS.delete(key)
    }

    for (let attempt = 0; attempt < maxAttempts; attempt++) {
      await refresh()

      if (embedding.value) {
        const status = embedding.value.status
        if (status === 'Completed' || status === 'Failed') break
      } else {
        break
      }

      if (attempt < maxAttempts - 1) {
        await new Promise<void>((resolve) => {
          const timer = setTimeout(() => {
            ACTIVE_POLLS.delete(key)
            resolve()
          }, intervalMs)
          ACTIVE_POLLS.set(key, timer)
        })
      }
    }

    if (embedding.value && embedding.value.status === 'Pending') {
      error.value = 'Embedding timed out after 30 seconds'
    }
  }

  return { embedding, loading, error, poll, refresh }
}
```

- [ ] **Step 2: Write tests**

Create `app/Admin/src/features/catalog/composables/__tests__/useEmbeddingStatus.spec.ts`:

```typescript
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { ref, nextTick } from 'vue'
import { useEmbeddingStatus } from '../useEmbeddingStatus'
import type { EmbeddingDetailResponse } from '../../types/imageEmbedding'

const { mockGet } = vi.hoisted(() => ({
  mockGet: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('../../services/imageEmbeddingApi', () => ({
  ImageEmbeddingApi: { get: mockGet },
}))

function okEmbedding(overrides: Partial<EmbeddingDetailResponse> = {}): { isSuccess: true; value: EmbeddingDetailResponse } {
  return {
    isSuccess: true,
    value: {
      id: 'e-1', variantImageId: 'img-1', modelName: 'fashion-clip', modelVersion: 'v1',
      vector: [], dimensions: 512,
      status: 'Completed', error: undefined, hangfireJobId: 'job-1', completedAtUtc: '2026-01-01T00:00:00Z',
      createdAtUtc: '2026-01-01T00:00:00Z',
      ...overrides,
    },
  }
}

function notFound(): { isSuccess: false; errors: Array<{ code: string }> } {
  return { isSuccess: false, errors: [{ code: 'ImageEmbedding.NotFound' }] }
}

beforeEach(() => { vi.clearAllMocks(); vi.useFakeTimers() })
afterEach(() => { vi.useRealTimers() })

describe('useEmbeddingStatus', () => {
  it('refresh: sets embedding on success', async () => {
    mockGet.mockResolvedValue(okEmbedding())
    const imageId = ref<string | null>('img-1')
    const { embedding, loading, refresh } = useEmbeddingStatus(imageId)

    await refresh()

    expect(embedding.value).not.toBeNull()
    expect(embedding.value!.status).toBe('Completed')
    expect(loading.value).toBe(false)
  })

  it('refresh: sets null on 404', async () => {
    mockGet.mockResolvedValue(notFound())
    const imageId = ref<string | null>('img-1')
    const { embedding, loading, refresh } = useEmbeddingStatus(imageId)

    await refresh()

    expect(embedding.value).toBeNull()
    expect(loading.value).toBe(false)
  })

  it('refresh: does nothing when variantImageId is null', async () => {
    const imageId = ref<string | null>(null)
    const { refresh } = useEmbeddingStatus(imageId)

    await refresh()

    expect(mockGet).not.toHaveBeenCalled()
  })

  it('poll: stops on Completed', async () => {
    mockGet.mockResolvedValue(okEmbedding({ status: 'Pending' }))
    const imageId = ref<string | null>('img-1')
    const { embedding, poll } = useEmbeddingStatus(imageId)

    const pollPromise = poll()
    await vi.advanceTimersByTimeAsync(1500)
    mockGet.mockResolvedValue(okEmbedding({ status: 'Processing' }))
    await vi.advanceTimersByTimeAsync(1500)
    mockGet.mockResolvedValue(okEmbedding({ status: 'Completed' }))
    await vi.advanceTimersByTimeAsync(1500)
    await pollPromise

    expect(embedding.value!.status).toBe('Completed')
    expect(mockGet).toHaveBeenCalledTimes(3)
  })

  it('poll: stops on Failed', async () => {
    mockGet.mockResolvedValue(okEmbedding({ status: 'Pending' }))
    const imageId = ref<string | null>('img-1')
    const { embedding, poll } = useEmbeddingStatus(imageId)

    const pollPromise = poll()
    await vi.advanceTimersByTimeAsync(1500)
    mockGet.mockResolvedValue(okEmbedding({ status: 'Failed', error: 'Inference timeout' }))
    await vi.advanceTimersByTimeAsync(1500)
    await pollPromise

    expect(embedding.value!.status).toBe('Failed')
    expect(embedding.value!.error).toBe('Inference timeout')
    expect(mockGet).toHaveBeenCalledTimes(2)
  })

  it('poll: times out after max attempts', async () => {
    mockGet.mockResolvedValue(okEmbedding({ status: 'Pending' }))
    const imageId = ref<string | null>('img-1')
    const { error, poll } = useEmbeddingStatus(imageId)

    const pollPromise = poll(3, 100)
    await vi.advanceTimersByTimeAsync(100)
    await vi.advanceTimersByTimeAsync(100)
    await vi.advanceTimersByTimeAsync(100)
    await pollPromise

    expect(error.value).toContain('timed out')
    expect(mockGet).toHaveBeenCalledTimes(3)
  })
})
```

- [ ] **Step 3: Add barrel export**

Open `app/Admin/src/features/catalog/composables/index.ts`. Add:

```typescript
export { useEmbeddingStatus } from './useEmbeddingStatus'
```

- [ ] **Step 4: Verify**

```bash
cd app/Admin && pnpm exec vue-tsc --build && pnpm run lint
pnpm run test:unit -- --run src/features/catalog/composables/__tests__/useEmbeddingStatus.spec.ts
```
Expected: typecheck clean, lint clean, tests pass.

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/catalog/composables/useEmbeddingStatus.ts \
        app/Admin/src/features/catalog/composables/__tests__/useEmbeddingStatus.spec.ts \
        app/Admin/src/features/catalog/composables/index.ts
git commit -m "feat: add useEmbeddingStatus composable with polling"
```

---

### Task 15: VariantDetail.vue — embedding UI in Images tab

**Files:**
- Modify: `app/Admin/src/features/catalog/views/VariantDetail.vue`

**Interfaces:**
- Consumes: `useEmbeddingStatus`, `ImageEmbeddingApi.create/regenerate/deleteEmbedding/get`.
- Produces: Embedding badge + control buttons per image card + "Generate all missing" button.

- [ ] **Step 1: Add imports**

In the `<script setup>` block, after existing image-related imports (line 22-28), add:

```typescript
import { ImageEmbeddingApi } from '../services/imageEmbeddingApi'
import { useEmbeddingStatus } from '../composables/useEmbeddingStatus'
import type { EmbeddingDetailResponse } from '../types/imageEmbedding'
import ProgressSpinner from 'primevue/progressspinner'
```

- [ ] **Step 2: Add embedding state and methods**

After the existing image-related state (after `imagesLoaded` ref, around line 230), add:

```typescript
// Embedding: per-image embedding state (id -> EmbeddingDetailResponse)
const embeddingMap = ref<Record<string, EmbeddingDetailResponse | null>>({})
// Loading: per-image generation loading state
const embeddingLoading = ref<Record<string, boolean>>({})
// Generate-all-missing: tabs-level batch loading
const batchGenerating = ref(false)

// Load: fetch embedding status for all images in the current variant
async function loadAllEmbeddings() {
  if (!images.value.length) return
  const results = await Promise.allSettled(
    images.value.map(async (img) => {
      const result = await ImageEmbeddingApi.get(img.id)
      if (result.isSuccess) {
        embeddingMap.value[img.id] = result.value
      } else {
        embeddingMap.value[img.id] = null
      }
    }),
  )
}

// Generate: create embedding for an image (enqueues Hangfire job)
async function generateEmbedding(image: VariantImage) {
  embeddingLoading.value[image.id] = true
  const result = await ImageEmbeddingApi.create({ variantImageId: image.id })
  if (result.isSuccess) {
    embeddingMap.value[image.id] = result.value
    // Poll: require status poll until terminal
    const { poll } = useEmbeddingStatus(ref(image.id))
    await poll()
  } else {
    notify.error('Failed to generate embedding')
  }
  embeddingLoading.value[image.id] = false
}

// Regenerate: re-run embedding generation
async function regenerateEmbedding(image: VariantImage) {
  embeddingLoading.value[image.id] = true
  const result = await ImageEmbeddingApi.regenerate({ variantImageId: image.id })
  if (result.isSuccess) {
    embeddingMap.value[image.id] = result.value
    const { poll } = useEmbeddingStatus(ref(image.id))
    await poll()
  } else {
    notify.error('Failed to regenerate embedding')
  }
  embeddingLoading.value[image.id] = false
}

// Delete: remove the embedding row
async function deleteEmbedding(image: VariantImage) {
  const hasEmbedding = embeddingMap.value[image.id]
  if (hasEmbedding) {
    const current = embeddingMap.value[image.id]!
    // Confirm: must confirm before permanently deleting embedding
    confirm.require({
      message: `Delete ${current.modelName} (${current.dimensions}d) embedding?`,
      header: 'Delete Embedding',
      accept: async () => {
        const result = await ImageEmbeddingApi.deleteEmbedding(image.id)
        if (result.isSuccess) {
          embeddingMap.value[image.id] = null
          notify.success('Embedding deleted')
        } else {
          notify.error('Failed to delete embedding')
        }
      },
    })
  }
}

// Batch: generate embeddings for all images without one
async function generateAllMissing() {
  batchGenerating.value = true
  for (const image of images.value) {
    if (!embeddingMap.value[image.id]) {
      await generateEmbedding(image)
    }
  }
  batchGenerating.value = false
}
```

- [ ] **Step 3: Add embedding loading to the existing tab-change handler**

In the existing `watch` handler (around line 131-138), change:

```typescript
watch(activeTab, (tab) => {
  if (isEdit.value && tab === '3' && images.value.length === 0 && !imagesLoaded.value) {
    loadImages()
  }
  if (isEdit.value && tab === '4' && optionValueAssignments.value.length === 0) {
    loadOptionValues()
  }
})
```

to:

```typescript
watch(activeTab, async (tab) => {
  if (isEdit.value && tab === '3') {
    if (images.value.length === 0 && !imagesLoaded.value) {
      await loadImages()
    }
    // Load: fetch embedding status after images are loaded
    if (images.value.length > 0) {
      await loadAllEmbeddings()
    }
  }
  if (isEdit.value && tab === '4' && optionValueAssignments.value.length === 0) {
    loadOptionValues()
  }
})
```

No changes needed to `loadImages()` itself — it stays as:

- [ ] **Step 4: Add template section for Image Embedding**

After the existing upload grid `<div v-else class="grid grid-cols-4 gap-4">` block (after the closing `</div>` of the image cards grid, around line 567), add:

```
                  </div>

                  <!-- Section: Image Embedding — no additional wrapper needed, button added to header above -->
```

Wait, that should go BEFORE the grid, as a tab-level button. Let me think about placement.

The "Generate all missing" button should be a tab-level action, placed alongside the upload button. Let me put it next to the Upload button in the header div at line 550.

Open VariantDetail template. At line 549-552:

```html
                  <!-- Section: Images — upload button and grid of uploaded images -->
                  <div class="mb-3">
                    <input type="file" ... />
                    <Button label="Upload Image" icon="pi pi-upload" severity="secondary" :loading="uploadLoading" @click="fileInputRef?.click()" />
                  </div>
```

Add the Generate All Missing button after the Upload button in the same div:

```html
                  <!-- Section: Images — upload button and grid of uploaded images -->
                  <div class="mb-3 flex items-center gap-2">
                    <input type="file" accept="image/jpeg,image/png,image/gif,image/webp" class="hidden" ref="fileInputRef" @change="onFileSelect" />
                    <Button label="Upload Image" icon="pi pi-upload" severity="secondary" :loading="uploadLoading" @click="fileInputRef?.click()" />
                    <Button
                      v-if="images.length > 0"
                      label="Generate All Missing"
                      icon="pi pi-play"
                      severity="help"
                      size="small"
                      :loading="batchGenerating"
                      @click="generateAllMissing"
                    />
                  </div>
```

Now, inside each image card in the grid (line 556-567), add the embedding section after the delete button. The current per-image card ends with:

```html
                       <div class="flex justify-between items-center mt-1">
                         <Tag :value="image.type" severity="info" />
                         <Button icon="pi pi-trash" severity="secondary" text rounded size="small" aria-label="Delete image" @click="confirmDeleteImage(image)" />
                       </div>
```

Add after the `</div>` inside the card (but before the enclosing `</div>` of the image card):

```html
                      <!-- Embedding Status: show badge + actions per image -->
                      <div v-if="embeddingMap[image.id] !== undefined" class="border-t mt-1 pt-1">
                        <div v-if="!embeddingMap[image.id]" class="text-xs text-muted-color mb-1">
                          No embedding
                        </div>
                        <div v-else>
                          <Tag
                            v-if="embeddingMap[image.id]!.status === 'Pending' || embeddingMap[image.id]!.status === 'Processing'"
                            :value="embeddingMap[image.id]!.status"
                            severity="info"
                          />
                          <Tag
                            v-else-if="embeddingMap[image.id]!.status === 'Completed'"
                            :value="embeddingMap[image.id]!.modelName + ' · ' + embeddingMap[image.id]!.dimensions + 'd'"
                            severity="success"
                          />
                          <Tag
                            v-else-if="embeddingMap[image.id]!.status === 'Failed'"
                            :value="'Failed'"
                            severity="danger"
                          />
                        </div>
                        <div class="flex items-center gap-1 mt-1">
                          <template v-if="embeddingMap[image.id] === null">
                            <Button
                              label="Generate"
                              size="small"
                              severity="info"
                              :loading="embeddingLoading[image.id]"
                              @click="generateEmbedding(image)"
                            />
                          </template>
                          <template v-else-if="embeddingMap[image.id]!.status === 'Pending' || embeddingMap[image.id]!.status === 'Processing'">
                            <ProgressSpinner style="width:16px;height:16px" strokeWidth="4" />
                            <span class="text-xs text-muted-color">Processing...</span>
                          </template>
                          <template v-else-if="embeddingMap[image.id]!.status === 'Completed'">
                            <Button
                              label="Regen"
                              size="small"
                              severity="secondary"
                              :loading="embeddingLoading[image.id]"
                              @click="regenerateEmbedding(image)"
                            />
                            <Button
                              label="Del"
                              size="small"
                              severity="danger"
                              @click="deleteEmbedding(image)"
                            />
                          </template>
                          <template v-else-if="embeddingMap[image.id]!.status === 'Failed'">
                            <Button
                              label="Retry"
                              size="small"
                              severity="warn"
                              :loading="embeddingLoading[image.id]"
                              @click="regenerateEmbedding(image)"
                            />
                            <Button
                              label="Del"
                              size="small"
                              severity="danger"
                              @click="deleteEmbedding(image)"
                            />
                            <div v-if="embeddingMap[image.id]!.error" class="text-xs text-red-500 mt-1 truncate max-w-[120px]">
                              {{ embeddingMap[image.id]!.error }}
                            </div>
                          </template>
                        </div>
                      </div>
```

- [ ] **Step 5: Verify**

```bash
cd app/Admin && pnpm exec vue-tsc --build && pnpm run lint
pnpm run test:unit --run
```
Expected: typecheck clean, lint clean, all 1076+ tests pass.

- [ ] **Step 6: Commit**

```bash
git add app/Admin/src/features/catalog/views/VariantDetail.vue
git commit -m "feat: add inline embedding status and management UI to VariantDetail Images tab"
```

---

### Task 16: Final verification

- [ ] **Step 1: Full backend build + test**

```bash
dotnet build service/Api
dotnet test service/Api/tests/Module.UnitTests
```
Expected: all tests pass, build clean.

- [ ] **Step 2: Full frontend build + lint + test**

```bash
cd app/Admin
pnpm exec vue-tsc --build
pnpm run lint
pnpm run test:unit --run
```
Expected: typecheck clean, lint clean, all tests pass.

- [ ] **Step 3: Check feature conventions**

```bash
bash scripts/check-feature-conventions.sh
bash scripts/check-cross-module-refs.sh
```
Expected: no new violations.

- [ ] **Step 4: Commit any remaining files**

```bash
git status
git add -A
git commit -m "chore: final verification — all tests pass, conventions clean"
```
