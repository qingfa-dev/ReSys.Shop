---
goal: Add POST /models/onnx/export endpoint with background export tracking and status reporting
version: "1.0"
date_created: "2026-07-27"
owner: "ReSys.Shop"
status: "Planned"
tags:
  - feature
  - api
  - onnx
  - background-task
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Add a new endpoint `POST /models/onnx/export` that triggers ONNX model export as a background process. The endpoint tracks export state globally and returns a status report showing per-model export progress and overall completion. If an export is already running, subsequent calls return the current status without restarting.

## 1. Requirements & Constraints

- **REQ-001**: `POST /models/onnx/export` starts background ONNX export for all 4 models (efficientnet_b0, clip_vit_b16, fashion_clip, dinov2_vits14)
- **REQ-002**: If no ONNX models exist on disk, endpoint starts export immediately
- **REQ-003**: If ONNX models already exist, endpoint still allows re-export (overwrites)
- **REQ-004**: If export is already in progress, subsequent calls return current status (no duplicate processes)
- **REQ-005**: Background process tracks per-model status: `pending`, `exporting`, `completed`, `failed`
- **REQ-006**: Response includes overall status: `idle`, `running`, `completed`, `failed`
- **REQ-007**: Response includes per-model report with model name, status, duration, error message
- **REQ-008**: Endpoint requires API key authentication (same as other inference endpoints)
- **CON-001**: Export runs in a background thread (CPU-bound PyTorch/ONNX work, not async)
- **CON-002**: Thread-safe global state using `threading.Lock`
- **CON-003**: Reuses existing `scripts.export.vision` export functions (no duplication)
- **CON-004**: Must work with `HUGGING_FACE_TOKEN` from settings for gated models (fashion_clip)
- **GUD-001**: Follow existing `ValueResult<T>` response pattern for consistency
- **GUD-002**: Follow existing router file naming and registration pattern
- **PAT-001**: Use `threading.Thread(daemon=True)` for background work (FastAPI lifespan safe)

## 2. Implementation Steps

### Implementation Phase 1 — Export State Manager

- GOAL-001: Create thread-safe singleton to track background export process state

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `src/models/onnx/export_state.py` with `ExportStatus` enum (`idle`, `running`, `completed`, `failed`), `ModelExportStatus` dataclass (model_name, status, duration_ms, error), and `ExportState` singleton class with `start_export()`, `get_report()`, `is_running()` methods using `threading.Lock` | | |
| TASK-002 | Export state must store: overall status, per-model statuses dict, start_time, end_time, total_duration_ms | | |
| TASK-003 | `start_export()` must acquire lock, check if already running, spawn `threading.Thread(daemon=True)` targeting `_run_export()`, return immediately | | |
| TASK-004 | `_run_export()` must iterate over models, call each export function from `scripts.export.vision`, update per-model status, handle exceptions per-model (mark failed, continue others) | | |
| TASK-005 | `get_report()` must return a dict with overall_status, models list, start_time, end_time, total_duration_ms — thread-safe read | | |

### Implementation Phase 2 — Export Schemas

- GOAL-002: Define Pydantic response models for the export status report

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | Create `src/schemas/inferences/export.py` with `ModelExportReport` (model_name: str, status: str, duration_ms: Optional[float], error: Optional[str]) and `OnnxExportResponse` (overall_status: str, models: List[ModelExportReport], start_time: Optional[str], end_time: Optional[str], total_duration_ms: Optional[float]) | | |
| TASK-007 | Update `src/schemas/inferences/__init__.py` to export `OnnxExportResponse` and `ModelExportReport` | | |
| TASK-008 | Update `src/schemas/__init__.py` to include new exports in `__all__` | | |

### Implementation Phase 3 — Export Router

- GOAL-003: Create the API endpoint and register it in the router

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Create `src/api/routers/models.py` with `APIRouter(tags=["models"])` and `POST /models/onnx/export` endpoint that: (1) verifies API key, (2) checks `ExportState.is_running()`, (3) if not running calls `ExportState.start_export()`, (4) returns `ValueResult[OnnxExportResponse]` with 202 if just started or 200 if already running/completed | | |
| TASK-010 | Update `src/api/router.py` to include the new `models_router` | | |
| TASK-011 | Add `EXPORT_IN_PROGRESS` error code to `src/core/constants.py` under `ErrorCodeConstants` | | |

### Implementation Phase 4 — Integration

- GOAL-004: Wire everything together and ensure export functions work from the new context

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-012 | Ensure `scripts.export.vision` functions can be called from `src/` context (they use relative imports from `core.constants` and `scripts.export.base` — verify import paths work when called from the background thread) | | |
| TASK-013 | Add `os.environ["HF_TOKEN"]` propagation in `_run_export()` before calling export functions (matching `scripts/export_onnx.py` pattern) | | |
| TASK-014 | Add error code `EXPORT_IN_PROGRESS: str = "Export.InProgress"` to `ErrorCodeConstants` and HTTP status 409 to `HttpStatusConstants` | | |

### Implementation Phase 5 — Tests

- GOAL-005: Unit and integration tests for the new endpoint

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-015 | Create `tests/unit/models/test_export_state.py` testing ExportState: initial state is idle, start_export sets running, get_report returns correct structure, concurrent calls don't start duplicate exports | | |
| TASK-016 | Create `tests/unit/schemas/test_export.py` testing ModelExportReport and OnnxExportResponse schema serialization and camelCase aliases | | |
| TASK-017 | Create `tests/integration/api/test_api_export.py` testing POST /models/onnx/export: returns 202 on first call, returns 200 with status on subsequent calls, requires API key, returns proper ValueResult envelope | | |

## 3. Alternatives

- **ALT-001**: Use FastAPI `BackgroundTasks` instead of `threading.Thread` — rejected because `BackgroundTasks` doesn't expose progress tracking or allow status queries from separate requests
- **ALT-002**: Use `asyncio.to_thread` with a global Future — rejected because Futures can't be safely shared across independent request scopes without complex cleanup
- **ALT-003**: Use a database/Redis for export state — rejected as overkill for a single-process sidecar; in-memory singleton is sufficient and matches the existing architecture
- **ALT-004**: Use `multiprocessing.Process` instead of `threading.Thread` — rejected because ONNX export is I/O-bound (model downloads) + CPU-bound but within same process; threading avoids serialization overhead and shared memory issues

## 4. Dependencies

- **DEP-001**: `scripts.export.vision` — existing export functions (export_efficientnet, export_clip, export_fashion_clip, export_dinov2)
- **DEP-002**: `scripts.export.base` — get_model_path, verify_export utilities
- **DEP-003**: `embedding.core.config` — settings.HUGGING_FACE_TOKEN for gated model access
- **DEP-004**: `threading` stdlib — for background process execution and lock

## 5. Files

- **FILE-001**: `src/models/onnx/export_state.py` — NEW: Export state manager singleton
- **FILE-002**: `src/schemas/inferences/export.py` — NEW: Export response schemas
- **FILE-003**: `src/api/routers/models.py` — NEW: Export endpoint router
- **FILE-004**: `src/api/router.py` — MODIFY: Register models_router
- **FILE-005**: `src/schemas/inferences/__init__.py` — MODIFY: Export new schemas
- **FILE-006**: `src/schemas/__init__.py` — MODIFY: Add to __all__
- **FILE-007**: `src/core/constants.py` — MODIFY: Add EXPORT_IN_PROGRESS error code
- **FILE-008**: `tests/unit/models/test_export_state.py` — NEW: Unit tests for ExportState
- **FILE-009**: `tests/unit/schemas/test_export.py` — NEW: Schema serialization tests
- **FILE-010**: `tests/integration/api/test_api_export.py` — NEW: Integration tests

## 6. Testing

- **TEST-001**: Unit test ExportState initial state is `idle`
- **TEST-002**: Unit test start_export transitions to `running` and spawns thread
- **TEST-003**: Unit test concurrent start_export calls are rejected (returns existing status)
- **TEST-004**: Unit test get_report returns correct structure with all fields
- **TEST-005**: Unit test OnnxExportResponse serializes with camelCase aliases
- **TEST-006**: Integration test POST /models/onnx/export returns 202 with ValueResult envelope
- **TEST-007**: Integration test POST /models/onnx/export requires valid API key (403 without)
- **TEST-008**: Integration test second call returns 200 with current status

## 7. Risks & Assumptions

- **RISK-001**: Long-running export (5-15 minutes for all models) could timeout if client has short HTTP timeout — mitigated by immediate 202 response, client polls for status
- **RISK-002**: Background thread crash could leave state stuck in `running` — mitigated by try/finally in _run_export that always sets final status
- **RISK-003**: Model download failures (network, HF token) during export — mitigated by per-model error handling, one model failure doesn't abort others
- **ASSUMPTION-001**: Service runs as single process (sidecar pattern) — global singleton state is sufficient
- **ASSUMPTION-002**: Export functions from scripts/export/vision.py are thread-safe (they create new model instances per call)
- **ASSUMPTION-003**: HUGGING_FACE_TOKEN is configured in .env for gated model access during export

## 8. Related Specifications / Further Reading

- `scripts/export_onnx.py` — existing CLI export orchestrator (reference for export flow)
- `scripts/export/vision.py` — individual model export implementations
- `src/models/onnx/onnx_embedder.py` — ONNX Runtime wrapper (consumer of exported models)
- `src/api/routers/inference.py` — existing endpoint pattern to follow
- FastAPI Background Tasks: https://fastapi.tiangolo.com/tutorial/background-tasks/
