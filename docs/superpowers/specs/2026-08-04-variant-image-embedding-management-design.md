# Variant Image Embedding Management UI

> Status: Approved for implementation planning
> Date: 2026-08-04
> Branch: `feature/implement-admin-panel`
> Applies to: `app/Admin` (Vue 3 + TypeScript, PrimeVue 5), `service/Api/src/Module/Catalog`

## Context

The backend already supports embedding generation via the Python Fashion-CLIP
sidecar. Variant images have an `ImageEmbedding` collection (pgvector `Vector`,
model name/version, dimensions), a registry of 8 model specs, an
`IEmbeddingOrchestrator` that calls the inference service and persists results,
and Admin endpoints for `Create` and `Regenerate`. Auto-embed on upload is
wired for Search-type images via Hangfire.

However, no Admin UI surfaces embeddings: there is no read endpoint to check
embedding status, and the variant image cards in VariantDetail show only
filename/size/type — no embedding state.

## Goal

Add inline embedding management to the VariantDetail Images tab (Admin SPA):
view, generate, regenerate, and delete vector embeddings for variant images,
with Hangfire background execution and status polling.

- Single default-model embedding per image (Fashion-CLIP, 512-dim).
- Background Hangfire job + polled status — no synchronous blocking.
- Pre-created Pending row in the database so the UI always has a status row to
  read, regardless of whether the job has run yet.

## Non-Goals

- No catalog-wide Embeddings dashboard view (per-image inline only).
- No multi-model support per image (single default-model embedding).
- No changes to the Storefront SPA or `SearchByImage` feature.
- No changes to the Python Embedding sidecar.

## Backend design

### 1. Domain changes (`ImageEmbedding`)

Add fields:

| Field | Type | Migrate? | Purpose |
|-------|------|----------|---------|
| `Status` | `Pending`/`Processing`/`Completed`/`Failed` enum | Yes | Lifecycle tracking |
| `Error` | `string?` | Yes | Error message when Failed |
| `HangfireJobId` | `string?` | Yes | Job identifier for correlation |
| `CompletedAtUtc` | `DateTimeOffset?` | Yes | Completion timestamp |

Add to `ImageEmbeddingMethod.cs`:

| Method | Behavior |
|--------|----------|
| `MarkProcessing()` | Sets `Status = Processing` |
| `MarkCompleted(vector, dims, modelVersion)` | Sets Vector, Dimensions, ModelVersion, `Status = Completed`, `CompletedAtUtc = now` |
| `MarkFailed(error)` | Sets `Error`, `Status = Failed` |
| `MarkPending()` | Sets `Status = Pending`, clears `HangfireJobId` + `Error` |
| `Create(variantImageId, modelName, modelVersion, vectorData)` | Creates with `Status = Completed` (existing behaviour — used when the job finishes) |
| `CreatePending(variantImageId, modelName, modelVersion)` | Creates with `Status = Pending`, empty vector, no data — used when enqueuing the job |

All methods return `Result` or `Result<ImageEmbedding>` — no exceptions.

### 2. New vertical slice: `GetEmbedding`

- `GET /variant-image-embeddings/{variantImageId}`
- Read-only query (no Request/Validator files).
- Returns `EmbeddingDetailResponse` (with `Status`, `Error`, `HangfireJobId`,
  `CompletedAtUtc`) or `404`.
- Files: `GetEmbedding.cs` (handler + query), `GetEmbedding.Endpoint.cs`,
  `GetEmbedding.Response.cs`.

### 3. Modified: `CreateEmbedding`

Handler behaviour:
1. Check for existing Pending/Processing row for this variantImageId + default
   model → `409 Conflict`.
2. Create Pending `ImageEmbedding` (empty vector, `Status = Pending`).
3. Enqueue Hangfire job:
   `IBackgroundJobClient.Create<IEmbeddingOrchestrator>(o => o.RunAsync(embeddingId, ct))`.
4. Store returned `HangfireJobId` + save → `201 Created`.

No synchronous inference call remains.

### 4. Modified: `RegenerateEmbedding`

Handler behaviour:
1. Load existing row by variantImageId. If none (was deleted), create a new
   Pending row.
2. Call `MarkPending()` to reset status, clear Error and HangfireJobId.
3. Enqueue Hangfire job → store `HangfireJobId` + save → `200 OK`.

### 5. New vertical slice: `DeleteEmbedding`

- `DELETE /variant-image-embeddings/{variantImageId}`
- Removes the `ImageEmbedding` row by variantImageId.
- `404` if absent; `200` + message on success.
- Files: `DeleteEmbedding.cs` (handler), `DeleteEmbedding.Endpoint.cs`.

### 6. Orchestrator refactor — canonical `RunAsync`

New method on `IEmbeddingOrchestrator`:

```
RunAsync(Guid embeddingId, CancellationToken ct):
  1. Load ImageEmbedding by id
  2. MarkProcessing() → save
  3. Load VariantImage by VariantImageId (get Url)
  4. Call inference client with image Url
  5. On success: MarkCompleted(vector, dims, version) → save
  6. On failure: MarkFailed(error) → save
  7. If VariantImage deleted mid-job → MarkFailed("Image was deleted")
```

Migrate `UploadVariantImage` auto-embed path from fire-and-forget
`GenerateAndPersistAsync(imageId, modelName)` to the status-tracked pattern:
after image upload, create a Pending `ImageEmbedding` row, enqueue
`RunAsync(embeddingId)`.

Keep `GenerateAndPersistAsync` (the old sync method) but mark it as the
non-status-tracked path (can be deprecated later).

## Frontend design (`app/Admin`)

### `types/imageEmbedding.ts`

Add to `EmbeddingDetailResponse`:
- `status: 'Pending' | 'Processing' | 'Completed' | 'Failed'`
- `error?: string`
- `hangfireJobId?: string`
- `completedAtUtc?: string`

No change to `CreateEmbeddingRequest` / `RegenerateEmbeddingRequest`.

### `services/imageEmbeddingApi.ts`

Add:
- `static get(variantImageId: string): Promise<Result<EmbeddingDetailResponse>>`
  → `GET {BASE}/{variantImageId}`
- `static deleteEmbedding(variantImageId: string): Promise<Result<{ message: string }>>`
  → `DELETE {BASE}/{variantImageId}`

### `composables/useEmbeddingStatus.ts` (new)

- `useEmbeddingStatus(variantImageId: Ref<string | null>)` returns:
  - `embedding: Ref<EmbeddingDetailResponse | null>` (null = no row/404)
  - `loading: Ref<boolean>`
  - `error: Ref<string | null>`
  - `poll(): Promise<void>` — calls GET; if Pending/Processing, repeats
    after 1.5s (max 20 attempts = 30s); stops on Completed, Failed, or 404.
  - `refresh(): Promise<void>` — immediate single fetch, no polling.
- Used per image card to track status after Generate/Regenerate.

### `views/VariantDetail.vue` (Images tab)

For each image card, add below the existing metadata (filename, size, type
tag, delete button) a new section `Image Embedding`:

State behaviour:
- No embedding (404 / not loaded yet): show `Tag` "No embedding" (warn) +
  `Generate` button.
- Pending: show `Tag` "Pending..." (info) + `ProgressSpinner`.
- Processing: show `Tag` "Processing..." (info) + `ProgressSpinner`.
- Completed: show `Tag` "modelName · dims-dim" (success) + `Regenerate` +
  `Delete` buttons with tooltip showing model version.
- Failed: show `Tag` "Failed" (danger) + error text + `Retry` (=
  Regenerate) + `Delete`.

Script additions:
- `const embeddingStates = ref<Map<string, EmbeddingStatus>>(new Map())` for
  per-image state.
- `async function generateEmbedding(image: VariantImage)` — calls Create API,
  sets Pending, starts polling.
- `async function regenerateEmbedding(image: VariantImage)` — calls
  Regenerate API, sets Pending, starts polling.
- `async function deleteEmbedding(image: VariantImage)` — calls Delete API,
  removes from state.
- `async function generateAllMissing()` — iterates images; for each 404 image,
  calls `generateEmbedding`.

All code below the existing `onMounted` / image management code, following the
Code Commenting Standard v3.0 (inline `// Label:` format).

Template section order (per `app/Admin/AGENTS.md`):
1. Existing sections 1..8 (Page Header, Scrollable Content, ...)
2. Within the Images tab (TabPanel value="3"), after the upload grid, add
   section: `Image Embedding` — embedding status badge and actions per image +
   tab-level "Generate all missing".

### Routes

No new route. Embedding management is inline in VariantDetail only.

## Error handling

- Result objects throughout (no domain exceptions).
- Inference failure → `MarkFailed(error)` with descriptive message.
- Sidecar unreachable → `MarkFailed("Inference service unavailable")`.
- Create while Pending/Processing exists for same image → `409 Conflict`.
- Delete of missing → `404 Not Found`.
- VariantImage deleted while job was queued → `MarkFailed("Image was deleted")`.

## Testing

### Backend (Module.UnitTests — no Docker)

`ImageEmbeddingMethod` (domain):
- Status transitions: Pending → Processing → Completed, Pending → Processing
  → Failed.
- Create defaults to Pending status.

`GetEmbedding` handler:
- Existing row returns `EmbeddingDetailResponse` with all fields.
- No row returns `404 NotFound`.

`CreateEmbedding` handler:
- Success: row created with Pending status + job enqueued + HangfireJobId set.
- Conflict: existing Pending row returns `409 Conflict`.

`RegenerateEmbedding` handler:
- Transitions existing row to Pending + enqueues job.
- Creates new Pending row if existing was deleted.

`DeleteEmbedding` handler:
- Removes existing row → `200` success message.
- Missing row → `404 NotFound`.

`IEmbeddingOrchestrator.RunAsync`:
- Happy path: loads row → Processing → inference → Completed with correct
  vector/dims/modelVersion.
- Inference failure: MarkFailed with error message.
- VariantImage deleted mid-job: MarkFailed.

All tests use stub `IBackgroundJobClient` + fake orchestrator.

### Frontend (vitest — `app/Admin`)

`useEmbeddingStatus`:
- Poll terminates on Completed → state updates, polling stops.
- Poll terminates on Failed → error exposed, polling stops.
- Poll terminates on 404 → embedding = null, polling stops.
- Poll retries on Pending/Processing → up to 20 attempts.
- Times out after 30s → error set.

`VariantDetail` embedding card rendering:
- None state: renders "No embedding" + Generate button.
- Pending state: renders spinner + no action buttons.
- Processing state: renders spinner + no action buttons.
- Completed state: renders model/dims + Regenerate + Delete.
- Failed state: renders error tag + error text + Retry + Delete.

## Migration

Single EF Core migration adding 4 columns to `ImageEmbedding`:
- `Status` (default: `Completed` for existing rows — they already have vectors).
- `Error` (null).
- `HangfireJobId` (null).
- `CompletedAtUtc` (null — set for existing embeddings after migration
  if needed, or leave null).
