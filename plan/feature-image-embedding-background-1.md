---
goal: Auto-Generate Image Embeddings via Background Jobs on Search-Type Image Upload
version: 1.0
date_created: 2026-07-05
last_updated: 2026-07-05
owner: Catalog Module
status: 'Completed'
tags: ['feature', 'embedding', 'background-jobs', 'catalog', 'search']
---

# Introduction

![Status: completed](https://img.shields.io/badge/status-completed-brightgreen)

This plan describes extending the Catalog module so that when a `VariantImage` is uploaded with `Type = Search`, a background job is automatically enqueued via Hangfire to generate a vector embedding by calling the Python inference sidecar (`IInferenceClient`). The plan also covers admin endpoints for on-demand embedding creation/regeneration, plugging the scaffold `SearchByImage` storefront endpoint into the real inference pipeline, and extending `IInferenceClient` to accept raw image bytes (for upload-based workflows) in addition to the existing URL mode. The existing `IInferenceClient.CreateEmbeddingAsync(EmbeddingRequest)` signature is preserved; a new byte-oriented method is added. A new orchestrator service (`IEmbeddingOrchestrator`) is introduced to encapsulate the call-inference-then-persist pattern, keeping handlers thin. The plan follows the existing CQRS + MediatR + Carter + Hangfire architecture.

## 1. Requirements & Constraints

- **REQ-001**: When a `VariantImage` is uploaded with `Type = VariantImageType.Search`, enqueue a Hangfire fire-and-forget job that calls the inference service and persists the resulting `ImageEmbedding` entity.
- **REQ-002**: The background job handler must be idempotent — if an embedding already exists for the same `(VariantImageId, ModelName)` tuple, it must upsert (overwrite).
- **REQ-003**: Provide admin endpoints `POST api/catalog/variants/images/{id:guid}/embeddings` and `PUT api/catalog/variants/images/{id:guid}/embeddings` for on-demand create/regenerate.
- **REQ-004**: Extend `IInferenceClient` with a `CreateEmbeddingFromBytesAsync(byte[] imageBytes, string contentType, string? model, CancellationToken ct)` method that sends raw image data to the inference service.
- **REQ-005**: The scaffold `SearchByImage` storefront endpoint (`POST api/storefront/search-by-image`) must be wired to call `IEmbeddingOrchestrator` to get an embedding, then use pgvector cosine-distance search (reuse pattern from `GetSimilarProducts`) to return ranked products.
- **REQ-006**: The default model must be `VariantImageConstant.Defaults.DefaultEmbeddingModel` (`openclip-vit-b-32`).
- **REQ-007**: Changes must pass all existing tests and the build must succeed with `TreatWarningsAsErrors=true`.
- **SEC-001**: Only admin users with appropriate permissions may trigger embedding creation/regeneration.
- **CON-001**: Must follow existing CQRS pattern: `Command → CommandHandler → Result<T>`, registered via Carter minimal API.
- **CON-002**: Must follow existing folder convention: `Features/{Admin|Storefront}/{FeatureName}/{Action}/`.
- **CON-003**: Must use the `IApplicationDbContext` interface (not a typed repository) for data access, as done throughout the codebase.
- **CON-004**: Must use `Result<T>` / `ValueResult<T>` pattern; no throwing exceptions for business errors.
- **CON-005**: Background jobs must use the Hangfire `IBackgroundJobClient` (already registered in `Background.Extension.cs:AddBackgroundJobs`), following the same pattern as `NotificationService` at `Shared/Operational/Notifications/Services/Notification.Service.Implementation.cs:48`.
- **PAT-001**: New orchestrator service follows the pattern of `NotificationService` — a dedicated interface + implementation containing all coordination logic.
- **PAT-002**: Background job handler (the method Hangfire invokes) must be a public method on the orchestrator, following the `SendInternalAsync` pattern in `NotificationService`.

## 2. Implementation Steps

### Implementation Phase 1: Extend `IInferenceClient` with Byte-Based Embedding

- GOAL-001: Add `CreateEmbeddingFromBytesAsync` to `IInferenceClient` and `InferenceClient`, so the orchestrator can send raw image bytes to the Python inference service.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Add `Task<Result<EmbeddingResponse>> CreateEmbeddingFromBytesAsync(byte[] imageBytes, string contentType, string? model = null, CancellationToken ct = default)` to `IInferenceClient` in `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Clients/ImageEmbedding.Inference.Interface.cs` | ✅ | 2026-07-05 |
| TASK-002 | Implement `CreateEmbeddingFromBytesAsync` in `InferenceClient` at `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Clients/ImageEmbedding.Inference.cs`. Use `MultipartFormDataContent` to POST to `/embeddings/bytes` endpoint. Include `contentType` as a header or form field so the Python sidecar can decode the raw bytes. | ✅ | 2026-07-05 |
| TASK-003 | Add new error code `EmbeddingFromBytesFailed` to `ImageEmbeddingResult.Errors` in `service/Api/src/Module/Catalog/Domain/Products/Variants/Images/Embeddings/ImageEmbedding.Result.cs` | ✅ | 2026-07-05 |
| TASK-004 | Add unit tests for `CreateEmbeddingFromBytesAsync` in `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Clients/ImageEmbedding.Inference.Tests.cs` | ✅ | 2026-07-05 |

### Implementation Phase 2: Create `IEmbeddingOrchestrator` Service

- GOAL-002: Introduce a new coordinator service that encapsulates the "call inference → map response → persist `ImageEmbedding`" flow, with both synchronous and background-job-compatible methods.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | Create directory `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Services/` | ✅ | 2026-07-05 |
| TASK-006 | Create `IEmbeddingOrchestrator` interface at `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Services/ImageEmbedding.Orchestrator.Interface.cs` with methods: `Task<Result<EmbeddingDetailResponse>> GenerateAndPersistAsync(Guid variantImageId, string modelName, CancellationToken ct)` and `Task<Result<EmbeddingDetailResponse>> GenerateAndPersistFromBytesAsync(Guid variantImageId, byte[] imageBytes, string contentType, string modelName, CancellationToken ct)` | ✅ | 2026-07-05 |
| TASK-007 | Create `EmbeddingOrchestrator` implementation at `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Services/ImageEmbedding.Orchestrator.cs`. Dependencies: `IInferenceClient`, `IApplicationDbContext`. Logic: (1) load `VariantImage` by ID to get URL/contentType, (2) if byte[] provided use `CreateEmbeddingFromBytesAsync`, else `CreateEmbeddingAsync`, (3) on success: upsert `ImageEmbedding` via EF — query existing by `(VariantImageId, ModelName)`, keep same `Id` if updating, (4) save changes, (5) map to `EmbeddingDetailResponse`. | ✅ | 2026-07-05 |
| TASK-008 | Create `ImageEmbedding.Orchestrator.Options.cs` at same location with default model name set to `VariantImageConstant.Defaults.DefaultEmbeddingModel` | ✅ | 2026-07-05 |
| TASK-009 | Create DI extension `AddEmbeddingOrchestrator` at `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Services/ImageEmbedding.Orchestrator.DependencyInjection.cs` registering `IEmbeddingOrchestrator` as scoped | ✅ | 2026-07-05 |

### Implementation Phase 3: Background Job Integration in Upload

- GOAL-003: Modify `UploadVariantImage.CommandHandler` to enqueue a Hangfire background job when the uploaded image has `Type = VariantImageType.Search`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-010 | Inject `IBackgroundJobClient` into `UploadVariantImage.CommandHandler` at `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Upload/UploadVariantImage.cs`. Add nullable `IBackgroundJobClient? backgroundJobClient` parameter. | ✅ | 2026-07-05 |
| TASK-011 | After successful image persistence (line 95 `await dbContext.SaveChangesAsync`), check `imageType == VariantImageType.Search`. If true, call `backgroundJobClient?.Enqueue<IEmbeddingOrchestrator>(s => s.GenerateAndPersistAsync(image.Id, VariantImageConstant.Defaults.DefaultEmbeddingModel, CancellationToken.None))`. Wrap in try/catch — log warning if Hangfire client is null but do not fail the upload. | ✅ | 2026-07-05 |
| TASK-012 | Register `IEmbeddingOrchestrator` in `Catalog.Extension.cs` via `builder.Services.AddEmbeddingOrchestrator()` at `service/Api/src/Module/Catalog/Catalog.Extension.cs:23` area | ✅ | 2026-07-05 |
| TASK-013 | Update existing `UploadVariantImage` unit tests at `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Images/Upload/UploadVariantImage.Tests.cs` to verify: (a) when `Type != Search`, no background job is enqueued, (b) when `Type == Search`, a job is enqueued with correct args | ✅ | 2026-07-05 |

### Implementation Phase 4: Admin Embedding Endpoints

- GOAL-004: Create admin Carter endpoints for creating and regenerating embeddings on-demand.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-014 | Create `CreateEmbedding` command/handler at `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Create/ImageEmbedding.Create.cs`. Handler accepts `CreateEmbeddingRequest` (VariantImageId, ModelName), calls `IEmbeddingOrchestrator.GenerateAndPersistAsync` synchronously (not via Hangfire — admin on-demand), returns `EmbeddingDetailResponse`. | ✅ | 2026-07-05 |
| TASK-015 | Create `CreateEmbedding.Endpoint.cs` at same location. POST route: `POST api/catalog/variants/images/{id:guid}/embeddings`. Map request body to handler command. | ✅ | 2026-07-05 |
| TASK-016 | Create `RegenerateEmbedding` command/handler at `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Regenerate/ImageEmbedding.Regenerate.cs`. Extends `RegenerateEmbeddingRequest` (ModelName, ModelVersion). Handler calls `IEmbeddingOrchestrator.GenerateAndPersistAsync`, overrides existing embedding. | ✅ | 2026-07-05 |
| TASK-017 | Create `RegenerateEmbedding.Endpoint.cs` at same location. PUT route: `PUT api/catalog/variants/images/{id:guid}/embeddings`. | ✅ | 2026-07-05 |
| TASK-018 | Add route constants under `CatalogFeature.Admin.Products.Variants.Images` namespace in `service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Admin.cs` for the new embedding routes | ✅ | 2026-07-05 |
| TASK-019 | Add permissions `Embeddings.Create` and `Embeddings.Regenerate` to `VariantImages` section in `CatalogFeatureMetadata` at `service/Api/src/Shared/Security/Authorization/Features/CatalogFeatureMetadata.cs` | ✅ | 2026-07-05 |

### Implementation Phase 5: Wire `SearchByImage` Storefront Endpoint

- GOAL-005: Replace the scaffold `SearchByImage.QueryHandler` at `service/Api/src/Module/Catalog/Features/Storefront/Products/SearchByImage/SearchByImage.cs` with real inference + pgvector search.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-020 | Rewrite `SearchByImage.QueryHandler.Handle` to: (1) read uploaded `IFormFile` bytes, (2) call `IEmbeddingOrchestrator.GenerateAndPersistFromBytesAsync` with a temporary model to get `float[] vector` (do NOT persist — or persist as ephemeral), (3) use raw SQL with pgvector `<=>` cosine distance to find top 20 similar products (reuse query pattern from `GetSimilarProducts.QueryHandler` at `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/Similar/GetSimilarProducts.cs:46-60`), (4) map results to `SearchByImage.Response` items. | ✅ | 2026-07-05 |
| TASK-021 | Inject `IEmbeddingOrchestrator` and `IApplicationDbContext` into `SearchByImage.QueryHandler` | ✅ | 2026-07-05 |
| TASK-022 | Add unit tests for the real `SearchByImage` handler at `service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/Products/SearchByImage/` | ✅ | 2026-07-05 |

### Implementation Phase 6: Python Embedding Service Endpoint Alignment

- GOAL-006: Add or update Python embedding service endpoints to support byte-based image embedding generation, matching what the C# client expects.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-023 | Add `POST /embeddings/bytes` endpoint in `service/Embedding/src/routers/embedding_router.py` that accepts multipart form data with raw image bytes and content type, preprocesses the image, and returns an embedding vector using the specified model | ✅ | 2026-07-05 |
| TASK-024 | Align `POST /embeddings` route path so the C# `InferenceClient.CreateEmbeddingAsync` at `/embeddings` matches (currently router is mounted at `/api/v1/embeddings` but client sends `/embeddings` — fix router prefix to `/` or update client base path) | ✅ | 2026-07-05 |
| TASK-025 | Ensure `GET /models` route exists in `model_router.py` returning list of available models matching `ModelMetadata` DTO shape | ✅ | 2026-07-05 |
| TASK-026 | Add Python unit tests for the new `/embeddings/bytes` endpoint | ✅ | 2026-07-05 |

## 3. Alternatives

- **ALT-001**: Run embedding generation synchronously in the upload handler instead of via background job. Rejected because inference service calls may be slow (500ms–5s), and blocking the HTTP upload response is poor UX. Background job allows fast upload confirmation.
- **ALT-002**: Use a dedicated Hangfire recurring job that polls for un-embedded images. Rejected because fire-and-forget via `IBackgroundJobClient.Enqueue` is simpler, provides immediate processing, and avoids polling overhead. Recurring jobs would add latency.
- **ALT-003**: Store raw image bytes in the Hangfire job arguments. Rejected because Hangfire serializes job arguments and large byte arrays are inefficient. Instead the job receives the `VariantImageId` and loads the image URL from the database.
- **ALT-004**: Create a separate `ISearchImageService` instead of extending `IEmbeddingOrchestrator`. Rejected because the orchestrator already encapsulates the inference+persist pattern, and the same flow is needed for both admin and storefront use cases. Avoids interface proliferation.

## 4. Dependencies

- **DEP-001**: Hangfire (`IBackgroundJobClient`) — already registered via `AddBackgroundJobs()` in `Shared/Operational/Backgrounds/Background.Extension.cs`. Must be enabled in configuration (`BackgroundJobs:Enabled=true`).
- **DEP-002**: `IInferenceClient` (typed `HttpClient`) — already registered via `AddInferenceClient()` in `ImageEmbedding.Inference.DependencyInjection.cs`. Requires the Python embedding service to be running.
- **DEP-003**: Python embedding service must be running and have the `/embeddings/bytes` route implemented. In development this is started by Aspire orchestration.
- **DEP-004**: `IApplicationDbContext` — already registered via `AddPersistence()` in `Shared/Operational/Persistence/Persistence.Extensions.cs`.
- **DEP-005**: PostgreSQL with pgvector extension enabled — already configured in the Aspire AppHost and EF Core migrations.

## 5. Files

- **FILE-001**: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Clients/ImageEmbedding.Inference.Interface.cs` — add `CreateEmbeddingFromBytesAsync` signature
- **FILE-002**: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Clients/ImageEmbedding.Inference.cs` — implement byte-based method
- **FILE-003**: `service/Api/src/Module/Catalog/Domain/Products/Variants/Images/Embeddings/ImageEmbedding.Result.cs` — add new error code
- **FILE-004**: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Services/ImageEmbedding.Orchestrator.Interface.cs` (NEW) — `IEmbeddingOrchestrator`
- **FILE-005**: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Services/ImageEmbedding.Orchestrator.cs` (NEW) — orchestrator implementation
- **FILE-006**: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Services/ImageEmbedding.Orchestrator.DependencyInjection.cs` (NEW) — DI registration
- **FILE-007**: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Services/ImageEmbedding.Orchestrator.Options.cs` (NEW) — config options
- **FILE-008**: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Upload/UploadVariantImage.cs` — inject `IBackgroundJobClient`, add enqueue logic
- **FILE-009**: `service/Api/src/Module/Catalog/Catalog.Extension.cs` — register orchestrator
- **FILE-010**: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Create/ImageEmbedding.Create.cs` (NEW) — admin create embedding handler
- **FILE-011**: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Create/ImageEmbedding.Create.Endpoint.cs` (NEW)
- **FILE-012**: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Regenerate/ImageEmbedding.Regenerate.cs` (NEW)
- **FILE-013**: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Regenerate/ImageEmbedding.Regenerate.Endpoint.cs` (NEW)
- **FILE-014**: `service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Admin.cs` — add route constants
- **FILE-015**: `service/Api/src/Shared/Security/Authorization/Features/CatalogFeatureMetadata.cs` — add embedding permissions
- **FILE-016**: `service/Api/src/Module/Catalog/Features/Storefront/Products/SearchByImage/SearchByImage.cs` — replace scaffold with real implementation
- **FILE-017**: `service/Embedding/src/routers/embedding_router.py` — add `/embeddings/bytes` endpoint, fix route prefix
- **FILE-018**: `service/Embedding/src/routers/model_router.py` — implement `/models` endpoint
- **FILE-019**: `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Clients/ImageEmbedding.Inference.Tests.cs` — add byte method tests
- **FILE-020**: `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Images/Upload/UploadVariantImage.Tests.cs` — add background job tests
- **FILE-021**: `service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/Products/SearchByImage/` (NEW) — new test class

## 6. Testing

- **TEST-001**: Unit test `InferenceClient.CreateEmbeddingFromBytesAsync` returns success with valid bytes and valid response from mocked `HttpMessageHandler`
- **TEST-002**: Unit test `InferenceClient.CreateEmbeddingFromBytesAsync` handles OperationCanceledException → returns `RequestTimeout` error
- **TEST-003**: Unit test `InferenceClient.CreateEmbeddingFromBytesAsync` handles general exception → returns `CommunicationFailed` error
- **TEST-004**: Unit test `EmbeddingOrchestrator.GenerateAndPersistAsync` creates new `ImageEmbedding` when none exists for `(VariantImageId, ModelName)` tuple
- **TEST-005**: Unit test `EmbeddingOrchestrator.GenerateAndPersistAsync` updates existing `ImageEmbedding` (same Id, new Vector) when one already exists
- **TEST-006**: Unit test `EmbeddingOrchestrator.GenerateAndPersistAsync` returns `ImageEmbeddingResult.Errors.NotFound` when `VariantImageId` does not exist
- **TEST-007**: Unit test `UploadVariantImage.CommandHandler` enqueues Hangfire job when `Type == Search` (verify `IBackgroundJobClient.Enqueue` called with correct expression)
- **TEST-008**: Unit test `UploadVariantImage.CommandHandler` does NOT enqueue Hangfire job when `Type != Search`
- **TEST-009**: Unit test `UploadVariantImage.CommandHandler` still succeeds when `Type == Search` but `IBackgroundJobClient` is null (graceful degradation)
- **TEST-010**: Unit test `SearchByImage.QueryHandler` returns ranked product results using pgvector similarity search
- **TEST-011**: Integration test (Api.Tests, requires Docker) verifies full flow: upload Search-type image → Hangfire job processes → embedding stored in DB → SearchByImage returns matching products
- **TEST-012**: `dotnet test` all projects pass
- **TEST-013**: `dotnet build` succeeds with `TreatWarningsAsErrors=true`

## 7. Risks & Assumptions

- **RISK-001**: The Python embedding service (`/embeddings/bytes` endpoint) must be implemented before the C# byte-based inference method can work end-to-end. If the Python service is not ready, the feature will fail at runtime with `CommunicationFailed` errors.
- **RISK-002**: Hangfire jobs are fire-and-forget; if the job fails (e.g., inference service down), there is currently no retry mechanism. Mitigation: Hangfire's built-in automatic retry on exception can be configured via `[AutomaticRetry]` attribute on the orchestrator method.
- **RISK-003**: The `IBackgroundJobClient` is registered conditionally — if `BackgroundJobs:Enabled=false`, `IBackgroundJobClient` still exists (InMemory storage is registered) but no server processes jobs. In that case, jobs are stored but never executed. This is acceptable for development but must be documented.
- **ASSUMPTION-001**: The Python embedding service can decode raw image bytes from common formats (JPEG, PNG, WebP, GIF) via `PIL.Image.open(io.BytesIO(data))`. This is standard PIL behavior.
- **ASSUMPTION-002**: The `IApplicationDbContext.Set<ImageEmbedding>()` can perform LINQ queries for upsert logic (`.Where(e => e.VariantImageId == id && e.ModelName == model).FirstOrDefaultAsync()`).
- **ASSUMPTION-003**: The `VariantImage.Url` property is a publicly accessible URL that the inference service can fetch (for URL-based embedding). This is already the case since images are uploaded to the storage service which provides public URIs.

## 8. Related Specifications / Further Reading

- [CONCERNS.md — Known broken / WIP items, including SearchByImage scaffold status](/home/qingfa/Repos/ReSys.Shop/docs/codebase/CONCERNS.md)
- [ARCHITECTURE.md — Module structure, CQRS pipeline, feature folder convention](/home/qingfa/Repos/ReSys.Shop/docs/codebase/ARCHITECTURE.md)
- `ImageEmbedding.Constant.cs` — `ModelSpecification` records and `ImageEmbeddingConstraint` values at `service/Api/src/Module/Catalog/Domain/Products/Variants/Images/Embeddings/ImageEmbedding.Constant.cs`
- `VariantImage.Constant.cs` — AIModel identifiers and defaults at `service/Api/src/Module/Catalog/Domain/Products/Variants/Images/VariantImage.Constant.cs`
- `Background.Extension.cs` — Hangfire setup and `IBackgroundJobClient` registration at `service/Api/src/Shared/Operational/Backgrounds/Background.Extension.cs`
- `Notification.Service.Implementation.cs` — reference implementation of Hangfire enqueue pattern at `service/Api/src/Shared/Operational/Notifications/Services/Notification.Service.Implementation.cs:48`
