---
goal: "Add embedding modes (skip|job|direct), default 1000 products, C# Hangfire job enqueue support"
version: 4.0
date_created: 2026-07-15
last_updated: 2026-07-15
owner: Platform
status: "Planned"
tags: ["feature", "data", "infrastructure"]
---

# Introduction

![Status: planned](https://img.shields.io/badge/status-planned-blue)

V4 replaces the binary `--skip-embeddings` flag with a `--embedding-mode` enum (skip|job|direct).
`direct` mode loads PyTorch models locally in the Python script (no sidecar). `job` mode defers
embedding generation to .NET Hangfire background jobs. Default product count lowered to 1000.

## 1. Requirements & Constraints

- **REQ-001**: `run_all.py` replaces `--skip-embeddings` with `--embedding-mode` accepting `skip`, `job`, or `direct`
- **REQ-002**: `skip` mode — no embeddings generated; `CatalogEmbeddingSeeder` returns Ok immediately
- **REQ-003**: `direct` mode — `generate_embeddings.py` loads PyTorch models from `transformers`/`fashion-clip`/`torchvision` locally; one model at a time; writes `demo_embeddings.json`; no sidecar API call
- **REQ-004**: `job` mode — Python skips embedding step; `CatalogEmbeddingSeeder` queries all Search-type `VariantImage` records and enqueues Hangfire `IBackgroundJobClient.Enqueue<IEmbeddingOrchestrator>(o => o.GenerateAndPersistAsync(image.Id, modelName, ct))` for each
- **REQ-005**: Default `--count` lowered from 2000 to 1000 (faster pipeline, still sufficient for demo)
- **REQ-006**: `appsettings.Development.json` adds `Seeders:EmbeddingMode` config key (default `direct`); `CatalogEmbeddingSeeder` reads this to decide behavior
- **CON-001**: `direct` mode reuses existing `benchmarks` project deps (torch, transformers, fashion-clip, Pillow) — no new packages
- **CON-002**: `job` mode reuses existing `IBackgroundJobClient` and `IEmbeddingOrchestrator` — no new DI registrations needed (already wired in `Shared/Operational/Backgrounds/`)
- **CON-003**: Warnings-as-errors — any warning fails the .NET build

## 2. Implementation Steps

### Implementation Phase 1: Replace skip-embeddings with embedding-mode + 1000 default

GOAL-001: Add `--embedding-mode` argument, set default count to 1000.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Update `run_all.py`: replace `--skip-embeddings` with `--embedding-mode` (choices: `skip`, `job`, `direct`, default `direct`). Change `--count` default from 2000 to 1000. In `job` and `skip` modes, skip the `generate_embeddings.py` step | | |
| TASK-002 | Update `extract_products.py`: change `--count` default from 2000 to 1000 | | |

### Implementation Phase 2: Direct mode — local model loading

GOAL-002: Rewrite `generate_embeddings.py` to load models from PyTorch locally (no sidecar HTTP call).

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-003 | Rewrite `generate_embeddings.py`: remove sidecar API dependency (`httpx`). Add local model loading via `transformers` + `fashion-clip` + `torchvision`. Process one model at a time. Available models: `fashion_clip` (via `transformers`), `efficientnet_b0` (via `torchvision`), `clip_vit_b16` (via `transformers`), `dinov2_vits14` (via `torch.hub`). Write `demo_embeddings.json` | | |
| TASK-004 | Remove `--base-url` parameter from `generate_embeddings.py` and `run_all.py` (no longer needed for direct mode) | | |

### Implementation Phase 3: Job mode — C# Hangfire enqueue

GOAL-003: `CatalogEmbeddingSeeder` supports `job` mode — enqueues Hangfire background jobs.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | Add `Seeders:EmbeddingMode` to `appsettings.Development.json` (default `"direct"`) | | |
| TASK-006 | Update `CatalogEmbeddingSeeder`: inject `IConfiguration`. In `SeedAsync`, read `Seeders:EmbeddingMode`. If `"skip"` → return Ok. If `"job"` → inject `IBackgroundJobClient`, query all Search-type `VariantImage` records, enqueue `GenerateAndPersistAsync(image.Id, modelName, ct)` for each. If `"direct"` → current JSON import behavior | | |
| TASK-007 | Register `CatalogEmbeddingSeeder` to receive `IBackgroundJobClient` via constructor (already scoped) — verify Hangfire is available in the seeder's DI scope | | |

### Implementation Phase 4: Verification

GOAL-004: Validate all three modes.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-008 | Test `direct` mode: `uv run python benchmarks/scripts/demo-seed/run_all.py --count 10 --embedding-mode direct --deploy --force` — verify embeddings generated in JSON | | |
| TASK-009 | Test `skip` mode: `uv run python benchmarks/scripts/demo-seed/run_all.py --count 10 --embedding-mode skip --deploy --force` — verify no embedding step runs, no `demo_embeddings.json` produced | | |
| TASK-010 | Test `job` mode ETL: `uv run python benchmarks/scripts/demo-seed/run_all.py --count 10 --embedding-mode job --deploy --force` — verify no embedding step runs | | |
| TASK-011 | Full build: `dotnet build` — 0 warnings, 0 errors | | |
| TASK-012 | Run app with `Seeders:EmbeddingMode=direct`, verify seeders run, check DB for embeddings | | |
| TASK-013 | Run app with `Seeders:EmbeddingMode=skip`, verify no embeddings created | | |

## 3. Alternatives

- **ALT-001**: Keep sidecar API for `direct` mode — rejected; requires running a separate service, the benchmarks project already has all ML deps for local inference
- **ALT-002**: Use a mode flag file instead of appsettings — rejected; config is the standard .NET pattern and is already used for `DemoDataPath`
- **ALT-003**: Generate embeddings in-process during seeder (not Hangfire jobs) — rejected; embedding generation is I/O+CPU-heavy and would block startup; Hangfire is already the established pattern (used by `UploadVariantImage`)

## 4. Dependencies

- **DEP-001**: `benchmarks/pyproject.toml` — torch, torchvision, transformers, fashion-clip (already present)
- **DEP-002**: Hangfire (`Shared/Operational/Backgrounds/`) — already registered in DI; `IBackgroundJobClient` available
- **DEP-003**: `IEmbeddingOrchestrator` (`Module/Catalog/.../Shared/Services/`) — already registered

## 5. Files

- **FILE-001**: `benchmarks/scripts/demo-seed/run_all.py` — `--embedding-mode`, `--count`=1000, remove `--base-url`
- **FILE-002**: `benchmarks/scripts/demo-seed/extract_products.py` — `--count`=1000
- **FILE-003**: `benchmarks/scripts/demo-seed/generate_embeddings.py` — rewrite for local model loading
- **FILE-004**: `service/Api/src/Api/appsettings.Development.json` — add `Seeders:EmbeddingMode`
- **FILE-005**: `service/Api/src/Module/Catalog/Persistence/Seeders/Embedding.Seeder.cs` — add mode-based logic + Hangfire enqueue
- **FILE-006**: `service/Api/src/Module/Catalog/Catalog.Extension.cs` — may need Hangfire registration verification

## 6. Testing

- **TEST-001**: `direct` mode produces valid `demo_embeddings.json` with embeddings for all models × all search images
- **TEST-002**: `skip` mode produces no `demo_embeddings.json` and seeder returns Ok
- **TEST-003**: `job` mode produces no `demo_embeddings.json`; after app start, Hangfire dashboard shows enqueued embedding jobs
- **TEST-004**: Build passes with 0 warnings, 0 errors for all modes

## 7. Risks & Assumptions

- **RISK-001**: Loading 4 PyTorch models in sequence may exhaust GPU/CPU memory — process one model at a time, call `del model; torch.cuda.empty_cache()` if CUDA
- **RISK-002**: `dinov2_vits14` uses `torch.hub.load("facebookresearch/dinov2")` which downloads weights on first use — ensure network access or pre-download
- **ASSUMPTION-001**: `IBackgroundJobClient` is available in the seeder's DI scope — Hangfire is registered at the host level (`Shared/Operational`), seeders run during startup after DI is built
- **ASSUMPTION-002**: `IEmbeddingOrchestrator.GenerateAndPersistAsync` works correctly from a background job — already used by `UploadVariantImage` for on-upload embedding generation

## 8. Related Specifications / Further Reading

- [V3 Plan](feature-demo-seeders-v3-1.md)
- [V2 Plan](feature-demo-seeders-v2-1.md)
- [Design Spec](../docs/superpowers/specs/2026-07-15-demo-seeders-design.md)
