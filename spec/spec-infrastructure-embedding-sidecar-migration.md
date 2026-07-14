---
title: Embedding Service — Sidecar Migration & Restructure
version: 1.0
date_created: 2026-07-14
owner: ReSys.Shop Platform
tags: [infrastructure, python, embedding, migration, sidecar]
---

# Introduction

Two Python FastAPI sidecar implementations coexist in the repository:
- `service/sidecar/` — mature, production-ready with 85+ tests, OpenTelemetry, ONNX support, API-key auth, rate limiting, and a Result monad matching the .NET BuildingBlocks pattern.
- `service/Embedding/` — partially implemented (~60% stubs), wired into Aspire orchestration (`AppHost.cs:23`), CI (`ci.yml:59-65`), and all project documentation.

The Aspire host, CI pipeline, README, AGENTS.md, and thesis documentation all reference `service/Embedding/` as the canonical ML sidecar location. `service/sidecar/` is never referenced by Aspire or CI.

This specification defines the migration of the mature sidecar implementation into `service/Embedding/`, replacing the stub-heavy structure with a clean, production-ready layout, while preserving the `embedding.*` import namespace required by Aspire and CI.

---

## 1. Purpose & Scope

**Purpose:** Consolidate the two Python sidecar implementations into a single, production-ready service at `service/Embedding/` by migrating the mature `service/sidecar/` codebase, dropping all stub modules, and merging unique features from the current `service/Embedding/`.

**Scope:**
- `service/Embedding/` — destination; full restructure
- `service/sidecar/` — source of mature implementation; deleted after migration
- `infra/Aspire/src/ReSys.AppHost/AppHost.cs` — no changes (import path `embedding.main:app` preserved)
- `.github/workflows/ci.yml` — no changes (working directory remains `service/Embedding`)
- `docs/` — no changes (all references already point to `service/Embedding/`)

**Out of scope:**
- Changing the .NET Catalog module's HTTP client that calls the sidecar
- Adding new model architectures beyond what already exists
- Database migrations or pgvector schema changes

---

## 2. Definitions

| Term | Definition |
|------|-----------|
| **Sidecar** | A companion service running alongside the .NET monolith, called via HTTP for ML inference |
| **Embedding** | A high-dimensional float32 vector representing an image's visual features |
| **Result monad** | `Result` / `ValueResult[T]` pattern from .NET BuildingBlocks, mirrored in Python as Pydantic models |
| **ONNX** | Open Neural Network Exchange — serialized model format for optimized inference via ONNX Runtime |
| **OpenTelemetry (OTel)** | Observability framework providing traces, metrics, and logs |
| **Aspire** | .NET Aspire orchestration layer that launches all services for local development |
| **Stub** | A file containing only a docstring with no implementation |
| **Hatchling** | PEP 517 build backend used by sidecar |
| **Setuptools** | PEP 517 build backend used by Embedding; required for `embedding.*` namespace mapping |

---

## 3. Requirements, Constraints & Guidelines

### Functional Requirements

- **REQ-001**: The migrated service must support `POST /embeddings` (image URL) with the same request/response contract as the current sidecar's `POST /inference/embeddings`.
- **REQ-002**: The migrated service must support `POST /embeddings/bytes` (multipart image upload) with the contract from the current Embedding's implementation.
- **REQ-003**: The migrated service must support `GET /models` returning metadata for all registered PyTorch models and auto-discovered ONNX models.
- **REQ-004**: The migrated service must support `GET /health` (readiness probe) and `GET /alive` (liveness probe).
- **REQ-005**: All 5 vision models must be registered and selectable at runtime: EfficientNet-B0, CLIP ViT-B/16, Fashion-CLIP, DINOv2 ViT-S/14, ResNet-50.
- **REQ-006**: The service must support optional ONNX-exported model inference via ONNX Runtime.
- **REQ-007**: All endpoints (except `/health` and `/alive`) must require `X-API-Key` header authentication.
- **REQ-008**: Rate limiting must be applied to the `/embeddings` endpoint, configurable via `RATE_LIMIT` env var.
- **REQ-009**: All domain operations must return `ValueResult[T]` or `Result` — no bare exceptions for domain errors.
- **REQ-010**: Global exception handlers must map HTTP 404, 401, 403, 422, and 500 errors to `Result` + `Failure` objects.

### Non-Functional Requirements

- **NFR-001**: OpenTelemetry must provide traces, metrics (histograms for inference duration, image load duration, model init duration), and logs with OTLP export support.
- **NFR-002**: The Docker multi-stage build must produce a production image with tini, non-root user, and healthcheck, matching the sidecar's existing Dockerfile.
- **NFR-003**: Model warmup at startup must preload the default model (configurable via `EMBEDDING_MODEL` env var).
- **NFR-004**: SSL support (optional HTTPS listener) must be preserved from the sidecar's `main.py` dual-process orchestration.

### Constraints

- **CON-001**: The import namespace must remain `embedding.*` (not `src.*`) — Aspire expects `embedding.main:app` and cannot be changed without breaking the orchestration layer.
- **CON-002**: The build backend must remain setuptools — the `[tool.setuptools.package-dir]` mapping `"embedding" = "src"` is the mechanism that enables `embedding.*` imports. Hatchling with `packages = ["src"]` would produce `src.*` imports incompatible with Aspire.
- **CON-003**: All internal `from src.xxx` imports from sidecar source files must be rewritten to `from embedding.xxx`.
- **CON-004**: Python version must be `>=3.12` (matches sidecar Dockerfile's `python:3.12-slim`; 3.14 is unreleased and the current Embedding's `>=3.14` is invalid).
- **CON-005**: The default HTTP port must be 8000 (Aspire's `AppHost.cs:26` expects port 8000; sidecar uses 5002).
- **CON-006**: The `.python-version` file must say `3.12`.
- **CON-007**: `uv.lock` must be regenerated after dependency changes.

### Guidelines

- **GUD-001**: Delete all stub files — empty docstrings provide no value and create confusion about what is implemented.
- **GUD-002**: ML model implementations live in `src/models/vision/` (not `src/infra/models/`).
- **GUD-003**: Route handlers live in `src/api/routers/` (not bare `src/routers/`).
- **GUD-004**: Configuration lives in `src/core/config.py` as a Pydantic `BaseSettings` class.
- **GUD-005**: Scripts (setup, export, test) live in `scripts/` at the service root.
- **GUD-006**: Copy sidecar's `.gitignore` — it already covers `.onnx`, `build/`, `*.egg-info/`, `.env`, and all standard Python/OS artifacts.

---

## 4. Interfaces & Data Contracts

### 4.1 HTTP Endpoints

| Method | Path | Auth | Rate Limit | Description |
|--------|------|------|------------|-------------|
| `GET` | `/health` | No | No | Readiness probe — `{"status": "ok", "service": "...", "environment": "...", "version": "..."}` |
| `GET` | `/alive` | No | No | Liveness probe — `{"status": "alive"}` |
| `GET` | `/models` | API Key | No | List registered PyTorch + auto-discovered ONNX models |
| `POST` | `/embeddings` | API Key | Yes | Generate embedding from image URL |
| `POST` | `/embeddings/bytes` | API Key | Yes | Generate embedding from multipart image upload |

### 4.2 Request/Response Contracts

**POST /embeddings** — Request:
```json
{
  "image_url": "https://example.com/image.jpg",
  "model": "fashion_clip"
}
```

**POST /embeddings** — Response (success):
```json
{
  "isSuccess": true,
  "statusCode": 200,
  "message": null,
  "failures": [],
  "value": {
    "vector": [0.123, -0.456, ...],
    "model_name": "fashion_clip",
    "dimension": 512,
    "duration_ms": 45.2
  }
}
```

**POST /embeddings** — Response (failure):
```json
{
  "isSuccess": false,
  "statusCode": 400,
  "message": null,
  "failures": [
    {
      "type": "BadRequest",
      "code": "Image.LoadError",
      "description": "Failed to load image: Connection timeout",
      "status_code": 400
    }
  ],
  "value": null
}
```

**POST /embeddings/bytes** — Request: `multipart/form-data` with field `image` (file) and optional query param `model`.

**POST /embeddings/bytes** — Response: Same `ValueResult[EmbeddingResponse]` envelope as `/embeddings`.

**GET /models** — Response:
```json
{
  "isSuccess": true,
  "statusCode": 200,
  "message": null,
  "failures": [],
  "value": [
    {
      "id": "efficientnet_b0",
      "name": "EfficientNet-B0",
      "dimension": 1280,
      "description": "General visual features via torchvision.",
      "is_onnx": false,
      "tags": ["vision", "cnn"]
    }
  ]
}
```

### 4.3 Failure Types

| FailureType | Code | HTTP Status |
|-------------|------|-------------|
| `Validation` | 1 | 422 |
| `Conflict` | 2 | 409 |
| `NotFound` | 3 | 404 |
| `BadRequest` | 4 | 400 |
| `InternalError` | 5 | 500 |
| `Unauthorized` | 6 | 401 |
| `Forbidden` | 7 | 403 |
| `Unexpected` | 8 | 500 |

### 4.4 Registered Models

| Model ID | Dimension | Library | Description |
|----------|-----------|---------|-------------|
| `efficientnet_b0` | 1280 | torchvision | General visual features (CNN baseline) |
| `clip_vit_b16` | 512 | openai/clip or transformers | Semantic (CLIP ViT-B/16) |
| `fashion_clip` | 512 | transformers (patrickjohncyh) | Fashion-specific semantic |
| `dinov2_vits14` | 384 | torch.hub (facebookresearch) | Self-supervised structural |
| `resnet50` | 2048 | torchvision | ImageNet-pretrained CNN (thesis baseline) |

### 4.5 Environment Variables

| Variable | Default | Required | Description |
|----------|---------|----------|-------------|
| `EMBEDDING_MODEL` | `fashion_clip` | No | Default model for requests without explicit model param |
| `API_KEY` | (none) | Yes | Shared secret for X-API-Key header (min 16 chars) |
| `PORT` | `8000` | No | HTTP listen port |
| `HTTPS_PORT` | `8001` | No | Optional HTTPS listen port |
| `RATE_LIMIT` | `50/minute` | No | SlowAPI rate limit string |
| `ONNX_MODEL_DIR` | `models/` | No | Directory for ONNX model discovery |
| `CORS_ORIGINS` | `["*"]` | No | Allowed CORS origins |
| `ENVIRONMENT` | `dev` | No | `dev`, `test`, or `production` |
| `LOG_LEVEL` | `INFO` | No | Python logging level |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | (none) | No | OTLP collector endpoint |
| `HUGGING_FACE_TOKEN` | (none) | No | Token for gated HuggingFace models (must start with `hf_`) |

---

## 5. Acceptance Criteria

- **AC-001**: Given the service is running, When `GET /health` is called, Then it returns 200 with `{"status": "ok"}`.
- **AC-002**: Given the service is running, When `GET /alive` is called, Then it returns 200 with `{"status": "alive"}`.
- **AC-003**: Given a valid API key, When `POST /embeddings` is called with `{"image_url": "<valid_url>", "model": "efficientnet_b0"}`, Then it returns 200 with a `ValueResult` containing a 1280-dim float vector.
- **AC-004**: Given a valid API key, When `POST /embeddings/bytes` is called with a valid image file, Then it returns 200 with a `ValueResult` containing a float vector.
- **AC-005**: Given no or invalid API key, When `POST /embeddings` is called, Then it returns 403 with `FailureType.Forbidden`.
- **AC-006**: Given an invalid image URL, When `POST /embeddings` is called, Then it returns 400 with `Failure` objects in the `failures` array.
- **AC-007**: Given the service is running, When `GET /models` is called with a valid API key, Then it returns a list of 5 PyTorch models with correct `id`, `name`, and `dimension` fields.
- **AC-008**: Given ONNX models exist in `ONNX_MODEL_DIR`, When `GET /models` is called, Then it returns ONNX models with `is_onnx: true` alongside PyTorch models.
- **AC-009**: Given the service starts, When no model is preloaded, Then the default model warms up during the startup event without blocking health checks.
- **AC-010**: The import `embedding.main:app` resolves correctly as a FastAPI application instance (verified by Aspire orchestration and `uv run uvicorn embedding.main:app`).
- **AC-011**: `uv run ruff check .` passes with zero errors from `service/Embedding/`.
- **AC-012**: `uv run pytest` passes all tests from `service/Embedding/` (includes migrated sidecar tests + Embedding-specific tests).
- **AC-013**: Given rate limit is exceeded, When `POST /embeddings` is called, Then it returns 429.
- **AC-014**: Given the Docker image is built, When the container starts, Then the healthcheck `curl localhost:8000/health` succeeds within 30 seconds.

---

## 6. Test Automation Strategy

- **Test Levels**: Unit, Integration
- **Frameworks**: pytest >=8, pytest-asyncio, pytest-env, httpx (via TestClient)
- **Test Data Management**: 
  - `conftest.py` injects env vars (`API_KEY`, `ENVIRONMENT=test`, `OTEL_EXPORTER_OTLP_ENDPOINT=""`, `ONNX_MODEL_DIR`) before any src import
  - Session-scoped `TestClient` fixtures with pre-set `X-API-Key` header
  - Mock `InferenceEngine` via FastAPI `dependency_overrides` where needed
- **CI/CD Integration**: `ci.yml` already runs `uv run ruff check . && uv run pytest` from `service/Embedding/` — no changes needed
- **Coverage Requirements**: Opt-in only (`dotnet test /p:CollectCoverage=true` for .NET; no Python coverage threshold set)
- **Performance Testing**: Not automated; `scripts/test_inference.py` provides manual CLI benchmarking

---

## 7. Rationale & Context

### Why migrate into `service/Embedding/` instead of keeping `service/sidecar/`?

1. **Aspire already references `service/Embedding/`** (`AppHost.cs:23`). Changing Aspire to reference `service/sidecar/` would require updating the app host, CI, README, AGENTS.md, and all thesis documentation. Migration the other direction is lower churn.
2. **CI only tests `service/Embedding/`** (`ci.yml:59`). The sidecar's 85+ tests have never run in CI. Moving them into `service/Embedding/` fixes this.
3. **The documentation is already written for `service/Embedding/`** — thesis chapters, codebase docs, README, and AGENTS.md all reference it.

### Why keep setuptools instead of hatchling?

The `[tool.setuptools.package-dir]` mapping `"embedding" = "src"` is the mechanism that enables `embedding.main:app` as an import path. Hatchling's `packages = ["src"]` produces `src.main:app`. Changing Aspire's reference adds risk with no benefit — the build backend is irrelevant for a service run via `uv run`.

### Why delete stubs instead of filling them in?

The stub files (controllers, dependencies, preprocessing pipelines, cache backends, storage backends, gateways, domain models, utilities) represent a planned architecture that was never implemented. The sidecar's proven architecture (api → services → models, with core infrastructure) is simpler, tested, and fully implemented. Filling in stubs would duplicate the sidecar's existing work.

### Why `src/models/vision/` instead of `src/infra/models/`?

The `infra/` prefix suggested these models were infrastructure, but they are the core domain of the service. Naming them `models/` (for ML models) is clearer. The Embedding's `src/models/` (domain models) is being deleted — it was all stubs — so there is no collision.

---

## 8. Dependencies & External Integrations

### External Systems

- **EXT-001**: .NET Catalog Module (`Module/Catalog`) — calls `POST /embeddings` via `ImageEmbedding.Inference.cs`; response contract must not change.
- **EXT-002**: Aspire AppHost (`infra/Aspire`) — orchestrates service startup; expects `embedding.main:app` on port 8000 with `/health` endpoint.

### Third-Party Services

- **SVC-001**: HuggingFace Hub — model weight downloads for CLIP and Fashion-CLIP; requires `HUGGING_FACE_TOKEN` for gated models.
- **SVC-002**: PyTorch Hub — DINOv2 weight downloads from `facebookresearch/dinov2`.
- **SVC-003**: OTLP Collector (optional) — OpenTelemetry telemetry export endpoint.

### Infrastructure Dependencies

- **INF-001**: ONNX Runtime >=1.17 — optimized inference for pre-exported models.
- **INF-002**: tini (Docker) — init process for correct SIGTERM forwarding.
- **INF-003**: libgomp1 (Docker) — OpenMP runtime for ONNX/PyTorch multi-threading.

### Data Dependencies

- **DAT-001**: ONNX model files — expected at `{ONNX_MODEL_DIR}/{model_name}/model.onnx`; auto-discovered at startup and on `GET /models`.

### Technology Platform Dependencies

- **PLT-001**: Python >=3.12 — runtime; 3.12 is the current LTS and matches the sidecar Dockerfile.
- **PLT-002**: FastAPI >=0.115 — HTTP framework.
- **PLT-003**: PyTorch >=2.0 — ML inference runtime (CPU-optimized index for Docker).
- **PLT-004**: PostgreSQL pgvector — embedding storage; managed by the .NET API, not the sidecar.

### Compliance Dependencies

- **COM-001**: API key must be >=16 characters (validated by `Settings` Pydantic model).
- **COM-002**: HuggingFace token must start with `hf_` prefix (validated by `Settings`).

---

## 9. Examples & Edge Cases

### Edge Case: Model not found

```python
# Request
POST /embeddings
{"image_url": "https://example.com/img.jpg", "model": "nonexistent"}

# Response (400)
{
  "isSuccess": false,
  "statusCode": 400,
  "failures": [{
    "type": "BadRequest",
    "code": "Registry.ModelNotRegistered",
    "description": "Model 'nonexistent' is not registered. Available: efficientnet_b0, clip_vit_b16, fashion_clip, dinov2_vits14, resnet50",
    "status_code": 400
  }],
  "value": null
}
```

### Edge Case: Image URL unreachable

```python
# Request
POST /embeddings
{"image_url": "https://invalid.example/notfound.jpg", "model": "fashion_clip"}

# Response (400)
{
  "isSuccess": false,
  "statusCode": 400,
  "failures": [{
    "type": "BadRequest",
    "code": "Image.LoadError",
    "description": "Failed to load image: HTTPSConnectionPool(...)",
    "status_code": 400
  }],
  "value": null
}
```

### Edge Case: ONNX model discovery

```python
# Given: models/efficientnet_b0/model.onnx exists
# When: GET /models
# Then: Response includes both PyTorch and ONNX entries

{
  "isSuccess": true,
  "statusCode": 200,
  "value": [
    {"id": "efficientnet_b0", "is_onnx": false, "dimension": 1280, ...},
    {"id": "onnx/efficientnet_b0", "is_onnx": true, "dimension": 1280, ...}
  ]
}
```

### Edge Case: Multipart upload with default model

```bash
# Request
curl -X POST http://localhost:8000/embeddings/bytes \
  -H "X-API-Key: <key>" \
  -F "image=@product.jpg"

# Uses EMBEDDING_MODEL env var default (fashion_clip)
# Response: ValueResult[EmbeddingResponse] with 512-dim vector
```

### Edge Case: SSL fallback

```python
# Given: SSL cert path is set but file is missing
# When: Service starts with --ssl-cert /missing/cert.pem --ssl-key /missing/key.pem
# Then: Logs warning "SSL configuration incomplete. Falling back to HTTP."
# And: Starts single HTTP listener only
```

---

## 10. Validation Criteria

- **VAL-001**: All 85+ tests from `service/sidecar/tests/` pass from `service/Embedding/` after migration.
- **VAL-002**: Existing Embedding tests pass: `test_health.py`, `test_embedding.py`, `test_exception_handler.py`.
- **VAL-003**: `uv run ruff check .` passes with zero errors.
- **VAL-004**: `uv run uvicorn embedding.main:app` starts without import errors (validates `embedding.*` namespace).
- **VAL-005**: `dotnet build` succeeds (Aspire app host references the service).
- **VAL-006**: No files under `service/Embedding/` contain `from src.` imports — all must be `from embedding.`.
- **VAL-007**: `service/sidecar/` directory no longer exists.
- **VAL-008**: `service/Embedding/build/` and `service/Embedding/embedding.egg-info/` no longer exist.
- **VAL-009**: `service/Embedding/.gitignore` exists and covers `build/`, `*.egg-info/`, `.env`, `*.onnx`, and `__pycache__/`.
- **VAL-010**: All stub files are deleted — no file in `src/` contains only a docstring with no implementation.
- **VAL-011**: `pyproject.toml` `requires-python` is `>=3.12`.
- **VAL-012**: `pyproject.toml` `[tool.setuptools.packages]` lists only packages that actually exist after migration.
- **VAL-013**: `POST /embeddings/bytes` returns `ValueResult[EmbeddingResponse]` (not the old flat `EmbeddingResult` schema).

---

## 11. Related Specifications / Further Reading

- `.harness/domains.yml` — Infrastructure domain definition (lines 227-232)
- `.harness/principles.yml` — Golden principles (Result objects, vertical slices)
- `docs/codebase/ARCHITECTURE.md` — Service architecture and layer responsibilities
- `docs/codebase/STACK.md` — Full technology stack and versions
- `docs/codebase/CONCERNS.md` — Known tech debt (build artifacts, egg-info)
- `docs/codebase/CONVENTIONS.md` — Python conventions (ruff, import naming)
- `docs/thesis/07-detailed-design.md` — ML sidecar detailed design
- `infra/Aspire/src/ReSys.AppHost/AppHost.cs` — Aspire orchestration (line 20-27)
- `.github/workflows/ci.yml` — CI pipeline (line 54-65)
- `service/sidecar/README.md` — Sidecar documentation (architecture diagrams, API reference)
