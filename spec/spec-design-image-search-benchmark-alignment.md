---
title: Image Search — Benchmark Alignment Specification
version: 1.0
date_created: 2026-07-16
last_updated: 2026-07-16
tags: [infrastructure, process, catalog, embedding, search, benchmark]
---

# Introduction

The .NET image search endpoints (`SearchByImage`, `GetSimilarProducts`) and the
Python benchmarking pipeline use different model identifiers, produce embeddings
from different model variants, and employ divergent SQL query patterns. This
specification defines the remediation required so that production search results
are reproducible by the benchmark, and vice versa.

## 1. Purpose & Scope

**Purpose**: Eliminate discrepancies between the .NET image search runtime and
the Python benchmark so that benchmark metrics (P@K, R@K, nDCG, mAP) predict
production behavior.

**Scope**:
- .NET: `SearchByImage` handler, `GetSimilarProducts` handler, inference client, model constants
- Python: `inference_engine.py` fallback logic, `clip.py` model adapter, model registry
- Both: model identifier strings, L2 normalization equivalence, vector dimension alignment

**Out of scope**: FAISS retrieval mode, ONNX model deployment, admin embedding orchestration.

## 2. Definitions

| Term | Definition |
|------|-----------|
| Model key | Canonical string identifier registered in `ModelRegistry` (e.g. `clip_vit_b16`, `fashion_clip`) |
| Model spec | `ModelSpecification` record in `ImageEmbedding.Constant.cs` describing a model's dimensions, role, and profile |
| AIModel constant | Static string in `VariantImageConstant.AIModels` mapping to a specific architecture (e.g. `OpenClipB32 = "openclip-vit-b-32"`) |
| `<=>` | pgvector cosine distance operator; `1 - (<=>)` = cosine similarity on L2-normalised vectors |
| L2 normalization | Dividing a float vector by its L2 norm so the result has unit length; required for cosine similarity via dot product |
| Self-exclusion | Excluding the query item from its own results when the query is part of the gallery |

## 3. Requirements, Constraints & Guidelines

### Model Identity & Registry

- **MOD-001**: Every `AIModels.*` constant in `VariantImageConstant.cs` must map 1:1 to a `ModelRegistry.register()` key in Python.
- **MOD-002**: `DefaultEmbeddingModel` constant must resolve to the model actually used at runtime without silent fallback.
- **MOD-003**: The Python `_load_torch_skill` fallback (checking `"clip" in model_name`) must be removed. Unknown model keys must return a clear error.
- **MOD-004**: `VariantImageConstant.AIModels.OpenClipB32` must map to ViT-B/**32**, not ViT-B/16. If B/16 is intended, rename the constant.

### SQL Query Correctness

- **SQL-001**: `SearchByImage` must use `DISTINCT ON (v.id)` with deterministic ordering, not bare `SELECT DISTINCT`.
- **SQL-002**: All raw SQL column references must use PostgreSQL-compliant snake_case (matching `UseSnakeCaseNamingConvention()`). No double-quoted PascalCase identifiers.
- **SQL-003**: `SearchByImage` must filter `product_images` by `Type = 'Default'` (matching `GetSimilarProducts`).
- **SQL-004**: `GetSimilarProducts` must filter image embeddings by the same model name used to generate the query vector.
- **SQL-005**: `SearchByImage` must self-exclude if the uploaded image's variant happens to exist in the gallery (match benchmark `exclude_self=True` default).

### Embedding Space Consistency

- **EMB-001**: Query-side embedding and gallery-side embeddings must use identical: model architecture, preprocessing pipeline, and L2 normalization algorithm (epsilon value).
- **EMB-002**: Both services must use `L2_EPSILON = 1e-9` (matches `ConstraintConstants.L2_EPSILON`).
- **EMB-003**: When `GetSimilarProducts` selects a query embedding, it must record and pass the model name to the gallery SQL, ensuring both sides query the same embedding space.

### Configurability

- **CFG-001**: `LIMIT 20` (top-K) must be configurable via query parameter, matching benchmark's `k_values = [1, 5, 10, 20]` convention. Default remains 20.
- **CFG-002**: `SearchByImage` must accept an optional `model` parameter (query string / form field) to override the default embedding model for the query image.

### Requirements Summary Table

| ID | Category | Description |
|----|----------|-------------|
| MOD-001 | Model | 1:1 mapping between `AIModels.` constants and Python `ModelRegistry` keys |
| MOD-002 | Model | No silent fallback; `DefaultEmbeddingModel` must match registered key |
| MOD-003 | Model | Remove `"clip" in model_name` heuristic from `_load_torch_skill` |
| MOD-004 | Model | `OpenClipB32` must load ViT-B/32, or be renamed |
| SQL-001 | SQL | `DISTINCT ON (v.id)` with deterministic order |
| SQL-002 | SQL | All raw SQL columns in snake_case (no `"IsDeleted"`) |
| SQL-003 | SQL | Filter `vi."Type" = 'Default'` in SearchByImage |
| SQL-004 | SQL | Filter `ie.model_name` in GetSimilarProducts gallery query |
| SQL-005 | SQL | Self-exclude in SearchByImage |
| EMB-001 | Embedding | Identical preprocessing + normalization across Python service and benchmark |
| EMB-002 | Embedding | Unified `L2_EPSILON = 1e-9` |
| EMB-003 | Embedding | Model name passed from query to gallery in GetSimilarProducts |
| CFG-001 | Config | Top-K configurable via query parameter |
| CFG-002 | Config | Model overridable in SearchByImage request |

## 4. Interfaces & Data Contracts

### 4.1 Python Model Registry — updated keys

```python
# After remediation, every .NET AIModel constant must have a matching entry:
ModelRegistry.register("openclip-vit-b-32", ...)   # ViT-B/32 adapter
ModelRegistry.register("openclip-vit-l-14", ...)    # ViT-L/14 adapter
ModelRegistry.register("siglip-vit-b-16", ...)       # SigLIP adapter
ModelRegistry.register("fashion-clip-v1", ...)       # maps to existing fashion_clip
ModelRegistry.register("deepfashion-embed-v2", ...)  # DeepFashion adapter
ModelRegistry.register("dinov2-vit-small", ...)      # maps to existing dinov2_vits14
ModelRegistry.register("dinov2-vit-base", ...)       # DINOv2 ViT-B adapter
ModelRegistry.register("ibot-vit-base", ...)         # iBOT adapter
ModelRegistry.register("swin-base", ...)             # Swin adapter
ModelRegistry.register("convnext-v2-tiny", ...)      # maps to existing convnext_tiny
ModelRegistry.register("efficientnet-b0", ...)       # maps to existing efficientnet_b0
```

### 4.2 SearchByImage — updated SQL

```sql
SELECT DISTINCT ON (v.id) v.*
FROM catalog.variants v
INNER JOIN catalog.product_images vi ON vi.variant_id = v.id
INNER JOIN catalog.product_image_embeddings ie ON ie.variant_image_id = vi.id
WHERE v.is_deleted = false
  AND vi.type = 'Default'
  AND ie.model_name = {1}
ORDER BY v.id, ie.vector <=> {0}::vector
LIMIT {2}
```
Parameters:
- `{0}`: query embedding vector
- `{1}`: model name string (from config or request)
- `{2}`: top-K integer (from request, default 20)

### 4.3 GetSimilarProducts — updated SQL

```sql
SELECT DISTINCT ON (v.id) v.*
FROM catalog.variants v
INNER JOIN catalog.product_images vi ON vi.variant_id = v.id
INNER JOIN catalog.product_image_embeddings ie ON ie.variant_image_id = vi.id
WHERE v.is_deleted = false
  AND v.product_id != {0}
  AND vi.type = 'Default'
  AND ie.model_name = {2}
ORDER BY v.id, ie.vector <=> {1}::vector
LIMIT 20
```
Parameters:
- `{0}`: current product ID (for self-exclusion)
- `{1}`: query embedding vector
- `{2}`: model name (from the query embedding record)

### 4.4 SearchByImage Request DTO — updated

```csharp
public sealed record Request(
    IFormFile? Image,
    int TopK = 20,          // added, matches benchmark default
    string? Model = null    // added, overrides DefaultEmbeddingModel
);
```

### 4.5 GetSimilarProducts Query DTO — updated

```csharp
public sealed record Query(
    Guid Id,
    int TopK = 20           // added
);
```

### 4.6 Python `_load_torch_skill` — after removal of fallback

```python
def _load_torch_skill(self, model_name: str, span) -> ValueResult[BaseEmbedder]:
    registry_result = ModelRegistry.get_model_class(model_name)
    if not registry_result.is_success:
        return ValueResult.failure_value(
            InferenceResults.Errors.ModelNotFound(model_name)
        )
    model_cls = registry_result.value
    return InferenceResults.Success.Ok(model_cls())
```

## 5. Acceptance Criteria

- **AC-001**: Given a product catalog with embeddings generated by benchmark model `clip-b32`, when `SearchByImage` is called with model `openclip-vit-b-32`, then the top-20 results must match the benchmark pgvector pipeline's top-20 results (identical order, identical variant IDs).
- **AC-002**: Given the same catalog, when `GetSimilarProducts` is called for a product whose embedding was generated by `dinov2-vits14`, then the response must contain only products with `dinov2-vits14` embeddings (no cross-model results).
- **AC-003**: When `SearchByImage` is called with `model=fashion-clip-v1` and the Python registry has no matching key, the endpoint must return an error (HTTP 400 or 422), not silently fall back to `clip_vit_b16`.
- **AC-004**: Given a variant has two `ImageEmbedding` records (one from `openclip-vit-b-32`, one from `dinov2-vit-small`), when `GetSimilarProducts` is called, the query must use only the embedding matching the current default similarity model (or the closest available).
- **AC-005**: `SearchByImage` with `TopK=5` must return exactly 5 results (or fewer if gallery < 5).
- **AC-006**: Raw SQL in `GetSimilarProducts` must use `is_deleted` (lowercase, unquoted), not `"IsDeleted"`.
- **AC-007**: `SearchByImage` SQL must use `DISTINCT ON (v.id) ORDER BY v.id, ie.vector <=> ...` and return the same result set regardless of how many `product_images` a variant has.
- **AC-008**: The benchmark pgvector pipeline must be runnable against the production database with identical results to the in-memory cosine baseline (within IVFFlat approximation tolerance).

## 6. Test Automation Strategy

- **Test Levels**: Unit (handler logic, model resolution), Integration (embedded pgvector queries), End-to-End (full search flow vs benchmark pipeline).
- **.NET Frameworks**: xUnit (or MSTest as per existing project), FluentAssertions, NSubstitute/Moq.
- **Python Frameworks**: pytest, pytest-cov, using `benchmark` package infrastructure.
- **Test Data Management**: Shared seed catalog (JSON fixture with ~50 products, pre-computed embeddings for all 11 models). Import into testcontainers pgvector at test start.
- **CI/CD Integration**: Run `SearchByImage` / `GetSimilarProducts` unit tests on every PR. Integration tests run nightly (require Docker/pgvector).
- **Coverage Requirements**: 90%+ on handler request validation, model resolution, SQL correctness.
- **Performance Testing**: Benchmark pipeline must measure latency of `SearchByImage` endpoint at k=20; result must be within 2x of raw pgvector query latency.

## 7. Rationale & Context

### Current state (overview of bugs found during review)

| # | File:Line | Problem | Severity |
|---|-----------|---------|----------|
| 1 | `VariantImage.Constant.cs:40` | `DefaultEmbeddingModel = "openclip-vit-b-32"` — no Python registry entry | 🔴 |
| 2 | `inference_engine.py:141` | Falls back to `clip_vit_b16` (ViT-B/16) when model name contains "clip" | 🔴 |
| 3 | `clip.py:27` | `CLIPEmbedder(variant="ViT-B/16")` — not B/32 as constant name implies | 🔴 |
| 4 | `SearchByImage.cs:55-63` | `SELECT DISTINCT` with ORDER BY on non-selected `ie.vector` | 🔴 |
| 5 | `SearchByImage.cs:61` | No `vi.type = 'Default'` filter | 🔴 |
| 6 | `GetSimilarProducts.cs:53` | `v."IsDeleted"` — double-quoted PascalCase, actual column is `is_deleted` | 🔴 |
| 7 | `GetSimilarProducts.cs:35-39` | `FirstOrDefaultAsync` on embeddings — no model name filter | 🔴 |
| 8 | `SearchByImage.cs:63` | `LIMIT 20` hardcoded | 🟡 |
| 9 | `ImageEmbedding.Inference.Models.cs:11` | `Model = "efficientnet_b0"` default contradicts `SearchByImage` | 🟡 |
| 10 | `SearchByImage.cs:52` | `Vector.ToArray()` allocates on hot path | 🔵 |

### Why these matter

1. **Model mismatch renders benchmarks useless**: If production uses ViT-B/16 (via silent fallback) but the benchmark evaluates ViT-B/32, the benchmark metrics do not reflect production quality. Every P@K, mAP, and nDCG number in the thesis report would be for the wrong model.

2. **SQL bugs cause silent wrong results**: `SELECT DISTINCT` with non-deterministic ORDER BY can return different results on successive calls, making search unpredictable. Missing `type = 'Default'` filter can surface packaging images, alt shots, and thumbnails as search results.

3. **Column name mismatch breaks `GetSimilarProducts`**: `"IsDeleted"` (double-quoted) in PostgreSQL is a case-sensitive identifier. With snake_case naming, the column is `is_deleted`, so this query will fail at runtime — not silently, but every call.

4. **Cross-model embedding contamination**: When a variant has embeddings from multiple models (common with admin re-embedding), the query vector and gallery vectors may be in incompatible 512d vs 384d spaces, producing random results.

## 8. Dependencies & External Integrations

### External Systems
- **EXT-001**: Python Embedding Service (FastAPI sidecar) — must have all model keys registered
- **EXT-002**: PostgreSQL + pgvector (production database) — must maintain `vector_cosine_ops` index

### Infrastructure Dependencies
- **INF-001**: pgvector IVFFlat index on `catalog.product_image_embeddings(embedding)` using `vector_cosine_ops`
- **INF-002**: Python sidecar accessible at configured `Http:Clients:Inference:BaseAddress`

### Data Dependencies
- **DAT-001**: `catalog.product_image_embeddings` table must include a `model_name` column (string, indexed) — if not present, requires migration

### Technology Platform Dependencies
- **PLT-001**: .NET `Pgvector` NuGet package for `Vector` type and `<=>` operator support
- **PLT-002**: Python `open_clip` package for ViT-B/32 adapter, or HuggingFace `transformers` for `openai/clip-vit-base-patch32`

## 9. Examples & Edge Cases

### Example 1: Correct model-agnostic query embedding selection

```csharp
// GetSimilarProducts — pick the right embedding
var queryEmbedding = await dbContext.Set<ImageEmbedding>()
    .Include(ie => ie.VariantImage)
    .Where(ie => ie.VariantImage.VariantId == variant.Id
              && ie.ModelName == SimilarityModel)   // <-- filter by model
    .Select(ie => new { ie.Vector, ie.ModelName })
    .FirstOrDefaultAsync(cancellationToken);

// Pass model name to gallery SQL
var similarVariants = await dbContext.Set<Variant>()
    .FromSqlRaw(@"
        SELECT DISTINCT ON (v.id) v.*
        FROM catalog.variants v
        INNER JOIN catalog.product_images vi ON vi.variant_id = v.id
        INNER JOIN catalog.product_image_embeddings ie ON ie.variant_image_id = vi.id
        WHERE v.is_deleted = false
          AND v.product_id != {0}
          AND vi.type = 'Default'
          AND ie.model_name = {2}
        ORDER BY v.id, ie.vector <=> {1}::vector
        LIMIT {3}",
        variant.ProductId, queryEmbedding.Vector,
        queryEmbedding.ModelName, topK)
    ...
```

### Edge Cases

| Case | Expected Behavior |
|------|------------------|
| No embeddings exist for any variant | Return empty list (not error) |
| Query image has zero bytes | Return empty result (current behavior, keep) |
| Uploaded file is not an image | Return `Error.Validation` (current, keep) |
| `topK=0` | Return empty list |
| `topK > gallery size` | Return all gallery items |
| Model specified in request has no gallery embeddings | Log warning, fall back to `DefaultEmbeddingModel` |
| `GetSimilarProducts` called for product with no image embeddings | Return empty list (current, keep) |
| Two variants share the same product image (duplicate `product_images` rows) | `DISTINCT ON` deduplicates to one result per variant |
| A variant has embeddings from 3 different models | Each model's embedding is treated as a separate gallery space; only matching model queries surface that variant |
| Benchmark query with `exclude_self=False` | Not applicable to production — `SearchByImage` always self-excludes (uploaded image is not in gallery) |

## 10. Validation Criteria

- **VAL-001**: All existing unit tests in `Module.UnitTests` must pass after remediation.
- **VAL-002**: A new integration test must confirm `SearchByImage` returns identical order to benchmark `pgvector.query()` for identical embeddings.
- **VAL-003**: `GetSimilarProducts` must not throw `Npgsql.PostgresException` (column not found) for `"IsDeleted"`.
- **VAL-004**: Calling `inference_engine.get_embedder("openclip-vit-b-32")` must return a ViT-B/32 embedder (768 `patch_size` or correct architecture), not ViT-B/16.
- **VAL-005**: `GetSimilarProducts` with a product that has `dinov2-vit-small` embeddings must produce results ordered by `dinov2-vit-small` cosine distance, not CLIP distance.
- **VAL-006**: `SearchByImage` with `model=fashion-clip-v1` must encode via `FashionCLIPEmbedder`, not `CLIPEmbedder`.
- **VAL-007**: Benchmark pipeline run against production catalog (with matching model) must show recall@20 > 0.95 for pgvector vs exact cosine.

## 11. Related Specifications / Further Reading

- [spec-design-feature-conventions-remediation.md](spec-design-feature-conventions-remediation.md) — existing feature conventions spec
- `benchmarks/docs/06-thesis-protocol.md` — §11.5 benchmark evaluation protocol
- `benchmarks/docs/08-replication-guide.md` — step-by-step benchmark replication
- `benchmarks/docs/codebase/ARCHITECTURE.md` — benchmark architecture (3 modes, layers)
- `service/Api/docs/codebase/ARCHITECTURE.md` — .NET service architecture
- `service/Embedding/README.md` — Python embedding service overview
