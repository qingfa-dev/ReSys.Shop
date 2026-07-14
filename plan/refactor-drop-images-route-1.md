---
goal: Remove /images/upload and /images/{name} routes from embedding sidecar
version: 1.0
date_created: 2026-07-14
last_updated: 2026-07-14
owner: ReSys.Shop Platform
status: Completed
tags: [refactor, removal, embedding, routing, upload]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Remove the `POST /images/upload` and `GET /images/{name}` endpoints from the embedding sidecar, along with all associated code — router file, config field, test file, and router registration.

---

## 1. Requirements & Constraints

- **REQ-001**: Delete `service/Embedding/src/api/routers/upload.py`
- **REQ-002**: Remove `upload_router` import from `service/Embedding/src/api/router.py`
- **REQ-003**: Remove `api_router.include_router(upload_router)` from `service/Embedding/src/api/router.py`
- **REQ-004**: Remove `UPLOAD_DIR` field from `service/Embedding/src/core/config.py`
- **REQ-005**: Delete `service/Embedding/tests/integration/api/test_image_upload.py`
- **REQ-006**: Remove empty `service/Embedding/uploads/` directory if it exists
- **REQ-007**: All pre-existing tests still pass (zero regressions)
- **REQ-008**: `uv run ruff check .` passes with zero errors
- **CON-001**: Must not affect `/embeddings`, `/embeddings/bytes`, `/models`, `/health`, `/alive` endpoints
- **CON-002**: Must not affect the .NET `EmbeddingOrchestrator` — it uses `IInferenceClient` (URL-based and bytes-based), not the upload router directly

---

## 2. Implementation Steps

### Implementation Phase 1: Delete upload router and clean up references

- GOAL-001: Remove all files and references related to the `/images/upload` and `/images/{name}` endpoints

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Delete `service/Embedding/src/api/routers/upload.py` | |  |
| TASK-002 | Remove upload_router import and registration from `router.py` | |  |
| TASK-003 | Remove `UPLOAD_DIR` field from `config.py` | |  |
| TASK-004 | Delete `service/Embedding/tests/integration/api/test_image_upload.py` | |  |
| TASK-005 | Remove empty `service/Embedding/uploads/` dir | |  |
| TASK-006 | Run ruff + pytest to verify no regressions | |  |

**TASK-001**: Delete upload router file

```bash
rm service/Embedding/src/api/routers/upload.py
```

**TASK-002**: Remove upload_router from `service/Embedding/src/api/router.py`

Edit `service/Embedding/src/api/router.py`:

```diff
 from embedding.api.routers.inference import router as inference_router
 from embedding.api.routers.system import router as system_router
-from embedding.api.routers.upload import router as upload_router
 from fastapi import APIRouter

 api_router = APIRouter()

 api_router.include_router(system_router)
 api_router.include_router(inference_router)
-api_router.include_router(upload_router)
```

Expected final `router.py`:

```python
"""
Main API router aggregator.
"""
from fastapi import APIRouter
from embedding.api.routers.inference import router as inference_router
from embedding.api.routers.system import router as system_router

api_router = APIRouter()

api_router.include_router(system_router)
api_router.include_router(inference_router)
```

**TASK-003**: Remove `UPLOAD_DIR` from `service/Embedding/src/core/config.py`

Edit `service/Embedding/src/core/config.py`. Remove the `UPLOAD_DIR` field block (currently between `EMBEDDING_MODEL` and `SSL_CERT_FILE`):

```diff
     EMBEDDING_MODEL: str = Field(
         default="fashion_clip",
         description="Default model name used when request does not specify one.",
         json_schema_extra={"example": "fashion_clip"}
     )
-    UPLOAD_DIR: str = Field(
-        default=str(SERVICE_ROOT / "uploads"),
-        description="Directory where uploaded images are stored locally.",
-        json_schema_extra={"example": "/app/uploads"}
-    )
     # ── SSL Certificate Configuration ─────────────────────────────────────────────
     SSL_CERT_FILE: Optional[str] = Field(
```

**TASK-004**: Delete upload test file

```bash
rm service/Embedding/tests/integration/api/test_image_upload.py
```

**TASK-005**: Remove empty uploads directory (if exists and empty)

```bash
rmdir service/Embedding/uploads/ 2>/dev/null || true
```

**TASK-006**: Run lint and test verification

```bash
cd service/Embedding && uv run ruff check --fix src/ && uv run ruff check src/
# Expected: 0 errors

cd service/Embedding && uv run pytest
# Expected: all pre-existing tests pass (no regressions)
```

---

## 3. Alternatives

- **ALT-001**: Keep the routes but deprecate them with a warning — rejected because unused endpoints add maintenance burden and surface area for security issues.
- **ALT-002**: Move the upload logic into `POST /embeddings/bytes` — rejected because the original plan intentionally separated file storage from embedding generation. If upload is no longer needed, remove it cleanly.
- **ALT-003**: Comment out the code instead of deleting — rejected because deleted code is tracked in git history; commenting just leaves dead code.

---

## 4. Dependencies

- **DEP-001**: `service/Embedding/src/api/routers/upload.py` — deleted entirely
- **DEP-002**: `service/Embedding/src/api/router.py` — updated to remove import and registration
- **DEP-003**: `service/Embedding/src/core/config.py` — updated to remove `UPLOAD_DIR` field
- **DEP-004**: `service/Embedding/tests/integration/api/test_image_upload.py` — deleted entirely

---

## 5. Files

| File | Action | Details |
|------|--------|---------|
| `service/Embedding/src/api/routers/upload.py` | Delete | Entire router with `POST /images/upload` and `GET /images/{name}` |
| `service/Embedding/src/api/router.py` | Modify | Remove upload_router import and `include_router` line |
| `service/Embedding/src/core/config.py` | Modify | Remove `UPLOAD_DIR` field from Settings |
| `service/Embedding/tests/integration/api/test_image_upload.py` | Delete | All upload/serve integration tests |
| `service/Embedding/uploads/` | Delete | Empty uploads directory (if present) |

---

## 6. Testing

- **TEST-001**: `ruff check src/` passes with zero errors (no orphaned references)
- **TEST-002**: `POST /images/upload` returns 404 (route no longer exists)
- **TEST-003**: `GET /images/{name}` returns 404 (route no longer exists)
- **TEST-004**: All pre-existing tests still pass (no regressions in `/embeddings`, `/embeddings/bytes`, `/models`, `/health`, `/alive`)

---

## 7. Risks & Assumptions

- **RISK-001**: The `uploads/` directory may contain files from prior runs — `rmdir` only deletes if empty. Acceptable — the directory is gitignored and harmless.
- **RISK-002**: Any external script or `.http` file referencing `POST /images/upload` will now receive 404. This plan assumes no such scripts exist outside the deleted test file.
- **ASSUMPTION-001**: The .NET `EmbeddingOrchestrator` does not depend on the sidecar's upload/serve endpoints — verified: it uses `IInferenceClient.CreateEmbeddingAsync` (URL-based via `image.Url` from DB) and `CreateEmbeddingFromBytesAsync` (bytes-based), which map to `/embeddings` and `/embeddings/bytes` respectively, neither of which are affected.

---

## 8. Related Specifications / Further Reading

- `service/Embedding/src/api/routers/upload.py` — File to delete (contains both routes)
- `service/Embedding/src/api/router.py` — File to modify (remove upload_router registration)
- `service/Embedding/src/core/config.py` — File to modify (remove UPLOAD_DIR)
- `service/Embedding/tests/integration/api/test_image_upload.py` — File to delete
- `plan/feature-embedding-image-upload-1.md` — Original upload images plan (now superseded)
