# Embedding Sidecar Migration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Consolidate `service/sidecar/` (mature, 85+ tests, OTel, ONNX, auth) into `service/Embedding/` (stub-heavy, Aspire-wired, CI-tested), preserving the `embedding.*` import namespace.

**Architecture:** Setuptools build backend with `"embedding" = "src"` package-dir mapping preserves `embedding.main:app` for Aspire. All sidecar source files copied into `service/Embedding/src/` with mechanical `from src.` → `from embedding.` import rewrites. Unique Embedding features (ResNet50, `/embeddings/bytes`) merged into the sidecar's proven `api/ → services/ → models/` layered architecture.

**Tech Stack:** Python 3.12, FastAPI, PyTorch (CPU), ONNX Runtime, OpenTelemetry, SlowAPI, setuptools, uv

## Global Constraints

- **CON-001**: Import namespace must be `embedding.*` — Aspire expects `embedding.main:app` (`AppHost.cs:23`)
- **CON-002**: Build backend must remain setuptools — `[tool.setuptools.package-dir]` mapping `"embedding" = "src"` enables `embedding.*` imports
- **CON-003**: All `from src.` → `from embedding.` in every `.py` file under `src/` and `tests/`
- **CON-004**: Python `>=3.12` — matches sidecar Dockerfile `python:3.12-slim`
- **CON-005**: Default HTTP port `8000` — Aspire `AppHost.cs:26` expects port 8000
- **CON-006**: `.python-version` must say `3.12`
- **CON-007**: `uv.lock` regenerated after dependency changes
- **CON-008**: Zero stub files — no file containing only a docstring
- **CON-009**: No `build/`, `egg-info/` artifacts committed; `.gitignore` must cover them
- **CON-010**: `uv run ruff check .` must pass with zero errors
- **CON-011**: `uv run pytest` must pass all tests
- **CON-012**: Don't touch `infra/Aspire/`, `.github/workflows/ci.yml`, or `docs/`

---

### Task 1: Delete stub files and dead directories from Embedding

**Files:**
- Delete: `service/Embedding/src/controllers/` (entire dir)
- Delete: `service/Embedding/src/dependencies/` (entire dir)
- Delete: `service/Embedding/src/infra/` (entire dir — cache, models, preprocessing, storage, utils)
- Delete: `service/Embedding/src/models/` (domain model stubs — `__init__.py`, `embedding.py`, `metadata.py`, `cache.py`)
- Delete: `service/Embedding/src/utils/` (entire dir — `__init__.py`, `constants.py`, `exceptions.py`, `logger.py`)
- Delete: `service/Embedding/src/services/gateways/` (entire dir)
- Delete: `service/Embedding/src/services/cache_service.py`
- Delete: `service/Embedding/src/services/validation_service.py`
- Delete: `service/Embedding/src/services/preprocessing_service.py`
- Delete: `service/Embedding/src/middleware/auth.py`
- Delete: `service/Embedding/src/middleware/logging.py`
- Delete: `service/Embedding/src/config/logging.py`
- Delete: `service/Embedding/src/config/dependency.py`
- Delete: `service/Embedding/src/routers/cache_router.py`
- Delete: `service/Embedding/src/schemas/common.py`
- Delete: `service/Embedding/docs/` (entire dir — stub architecture.md, openapi.yaml)

**Interfaces:**
- Produces: A clean `service/Embedding/src/` tree containing only files that will be replaced in subsequent tasks

- [ ] **Step 1: Delete all stub directories and files**

```bash
rm -rf service/Embedding/src/controllers
rm -rf service/Embedding/src/dependencies
rm -rf service/Embedding/src/infra
rm -rf service/Embedding/src/models
rm -rf service/Embedding/src/utils
rm -rf service/Embedding/src/services/gateways
rm service/Embedding/src/services/cache_service.py
rm service/Embedding/src/services/validation_service.py
rm service/Embedding/src/services/preprocessing_service.py
rm service/Embedding/src/middleware/auth.py
rm service/Embedding/src/middleware/logging.py
rm service/Embedding/src/config/logging.py
rm service/Embedding/src/config/dependency.py
rm service/Embedding/src/routers/cache_router.py
rm service/Embedding/src/schemas/common.py
rm -rf service/Embedding/docs
```

- [ ] **Step 2: Verify directories are gone**

```bash
ls service/Embedding/src/
# Should show only: config/  middleware/  routers/  schemas/  services/  __init__.py  main.py
```

- [ ] **Step 3: Commit**

```bash
git add service/Embedding/src/
git commit -m "chore(embedding): delete all stub files and dead directories"
```

---

### Task 2: Delete build artifacts and add .gitignore

**Files:**
- Delete: `service/Embedding/build/` (entire dir)
- Delete: `service/Embedding/embedding.egg-info/` (entire dir)
- Create: `service/Embedding/.gitignore`

**Interfaces:**
- Produces: Clean service root with `.gitignore` matching sidecar's coverage

- [ ] **Step 1: Delete build artifacts**

```bash
rm -rf service/Embedding/build
rm -rf service/Embedding/embedding.egg-info
```

- [ ] **Step 2: Copy .gitignore from sidecar**

```bash
cp service/sidecar/.gitignore service/Embedding/.gitignore
```

- [ ] **Step 3: Commit**

```bash
git add service/Embedding/.gitignore
git add service/Embedding/
git commit -m "chore(embedding): delete build artifacts, add .gitignore"
```

---

### Task 3: Copy core/ module with import rewrite

**Files:**
- Create: `service/Embedding/src/core/__init__.py` (empty)
- Create: `service/Embedding/src/core/config.py`
- Create: `service/Embedding/src/core/constants.py`
- Create: `service/Embedding/src/core/rate_limit.py`
- Create: `service/Embedding/src/core/security.py`
- Create: `service/Embedding/src/core/telemetry.py`
- Delete: `service/Embedding/src/config/settings.py` (superseded by core/config.py)
- Delete: `service/Embedding/src/config/__init__.py` (config/ dir becomes core/)

**Interfaces:**
- Produces: `embedding.core.config.settings`, `embedding.core.constants.Constants`, `embedding.core.rate_limit.limiter`, `embedding.core.security.resolve_ssl_paths`, `embedding.core.telemetry.setup_telemetry`/`get_tracer`/`get_meter`

- [ ] **Step 1: Create target directory and copy files**

```bash
mkdir -p service/Embedding/src/core
touch service/Embedding/src/core/__init__.py
cp service/sidecar/src/core/config.py service/Embedding/src/core/config.py
cp service/sidecar/src/core/constants.py service/Embedding/src/core/constants.py
cp service/sidecar/src/core/rate_limit.py service/Embedding/src/core/rate_limit.py
cp service/sidecar/src/core/security.py service/Embedding/src/core/security.py
cp service/sidecar/src/core/telemetry.py service/Embedding/src/core/telemetry.py
```

- [ ] **Step 2: Rewrite imports from `src.` to `embedding.`**

```bash
find service/Embedding/src/core -name "*.py" -exec sed -i 's/from src\./from embedding./g' {} +
find service/Embedding/src/core -name "*.py" -exec sed -i 's/import src\./import embedding./g' {} +
```

- [ ] **Step 3: Fix config.py — rename SIDECAR_ROOT to SERVICE_ROOT, change PORT to 8000, rename project, add embedding_model**

The config.py was copied verbatim from sidecar. Apply these targeted edits:

```bash
# Rename SIDECAR_ROOT → SERVICE_ROOT
sed -i 's/SIDECAR_ROOT/SERVICE_ROOT/g' service/Embedding/src/core/config.py

# Change PORT default from 5002 to 8000
sed -i 's/default=5002/default=8000/' service/Embedding/src/core/config.py

# Change HTTPS_PORT default from 5003 to 8001
sed -i 's/default=5003/default=8001/' service/Embedding/src/core/config.py

# Change PROJECT_NAME default from "Inference" to "Embedding Service"
sed -i 's/default="Inference"/default="Embedding Service"/' service/Embedding/src/core/config.py

# Change RATE_LIMIT default from "100/minute" to "50/minute"
sed -i 's/default="100\/minute"/default="50\/minute"/' service/Embedding/src/core/config.py
```

Now add `embedding_model` field. Insert after the `ONNX_MODEL_DIR` field block (after line ~103, after the `json_schema_extra` line for ONNX):

```bash
# Add embedding_model field after ONNX_MODEL_DIR
python3 -c "
content = open('service/Embedding/src/core/config.py').read()
# Insert after the ONNX_MODEL_DIR json_schema_extra line
marker = '        json_schema_extra={\"example\": \"/app/models\"}\n    )'
insertion = '''
    EMBEDDING_MODEL: str = Field(
        default=\"fashion_clip\",
        description=\"Default model name used when request does not specify one.\",
        json_schema_extra={\"example\": \"fashion_clip\"}
    )
'''
content = content.replace(marker, marker + insertion)
open('service/Embedding/src/core/config.py', 'w').write(content)
"
```

- [ ] **Step 4: Remove old config/ directory**

```bash
rm -rf service/Embedding/src/config
```

- [ ] **Step 5: Verify rewrite — no remaining `from src.` imports**

```bash
grep -r "from src\." service/Embedding/src/core/ || echo "OK: No src. imports found"
grep -r "import src\." service/Embedding/src/core/ || echo "OK: No src. imports found"
```

- [ ] **Step 6: Commit**

```bash
git add service/Embedding/src/core/ service/Embedding/src/config/
git commit -m "feat(embedding): migrate core/ module from sidecar with import rewrite"
```

---

### Task 4: Copy schemas/ module with import rewrite

**Files:**
- Create: `service/Embedding/src/schemas/__init__.py`
- Create: `service/Embedding/src/schemas/results/__init__.py`
- Create: `service/Embedding/src/schemas/results/result.py`
- Create: `service/Embedding/src/schemas/results/failure.py`
- Create: `service/Embedding/src/schemas/inferences/__init__.py`
- Create: `service/Embedding/src/schemas/inferences/models.py`
- Create: `service/Embedding/src/schemas/images/__init__.py`
- Create: `service/Embedding/src/schemas/images/__init__.py` (contains ImageResults)
- Create: `service/Embedding/src/schemas/registries/__init__.py`
- Delete: `service/Embedding/src/schemas/requests.py` (superseded by inferences/models.py)
- Delete: `service/Embedding/src/schemas/responses.py` (superseded by inferences/models.py)

**Interfaces:**
- Produces: `embedding.schemas.Result`, `ValueResult[T]`, `Failure`, `FailureType`, `InferenceResults`, `ImageResults`, `RegistryResults`, `EmbeddingRequest`, `EmbeddingResponse`, `ModelMetadata`

- [ ] **Step 1: Copy all schema files from sidecar**

```bash
# Save existing schemas/__init__.py if needed (we'll overwrite it)
mkdir -p service/Embedding/src/schemas/results
mkdir -p service/Embedding/src/schemas/inferences
mkdir -p service/Embedding/src/schemas/images
mkdir -p service/Embedding/src/schemas/registries

cp service/sidecar/src/schemas/__init__.py service/Embedding/src/schemas/__init__.py
cp service/sidecar/src/schemas/results/__init__.py service/Embedding/src/schemas/results/__init__.py
cp service/sidecar/src/schemas/results/result.py service/Embedding/src/schemas/results/result.py
cp service/sidecar/src/schemas/results/failure.py service/Embedding/src/schemas/results/failure.py
cp service/sidecar/src/schemas/inferences/__init__.py service/Embedding/src/schemas/inferences/__init__.py
cp service/sidecar/src/schemas/inferences/models.py service/Embedding/src/schemas/inferences/models.py
cp service/sidecar/src/schemas/images/__init__.py service/Embedding/src/schemas/images/__init__.py
cp service/sidecar/src/schemas/registries/__init__.py service/Embedding/src/schemas/registries/__init__.py
```

- [ ] **Step 2: Rewrite imports from `src.` to `embedding.`**

```bash
find service/Embedding/src/schemas -name "*.py" -exec sed -i 's/from src\./from embedding./g' {} +
find service/Embedding/src/schemas -name "*.py" -exec sed -i 's/import src\./import embedding./g' {} +
```

- [ ] **Step 3: Delete old schemas/ files**

```bash
rm service/Embedding/src/schemas/requests.py
rm service/Embedding/src/schemas/responses.py
```

- [ ] **Step 4: Verify — no remaining `from src.` imports**

```bash
grep -r "from src\." service/Embedding/src/schemas/ || echo "OK"
```

- [ ] **Step 5: Commit**

```bash
git add service/Embedding/src/schemas/
git commit -m "feat(embedding): migrate schemas/ module from sidecar with import rewrite"
```

---

### Task 5: Copy models/ module with import rewrite

**Files:**
- Create: `service/Embedding/src/models/__init__.py`
- Create: `service/Embedding/src/models/base.py`
- Create: `service/Embedding/src/models/registry.py`
- Create: `service/Embedding/src/models/onnx/__init__.py`
- Create: `service/Embedding/src/models/onnx/onnx_embedder.py`
- Create: `service/Embedding/src/models/onnx/utils.py`
- Create: `service/Embedding/src/models/vision/__init__.py`
- Create: `service/Embedding/src/models/vision/clip.py`
- Create: `service/Embedding/src/models/vision/dinov2.py`
- Create: `service/Embedding/src/models/vision/efficientnet.py`

**Interfaces:**
- Produces: `embedding.models.BaseEmbedder`, `embedding.models.ModelRegistry`, `embedding.models.onnx.OnnxEmbedder`, `embedding.models.vision.CLIPEmbedder`, `DINOEmbedder`, `EfficientNetEmbedder`, `FashionCLIPEmbedder`

- [ ] **Step 1: Copy all model files from sidecar**

```bash
mkdir -p service/Embedding/src/models/onnx
mkdir -p service/Embedding/src/models/vision

cp service/sidecar/src/models/__init__.py service/Embedding/src/models/__init__.py
cp service/sidecar/src/models/base.py service/Embedding/src/models/base.py
cp service/sidecar/src/models/registry.py service/Embedding/src/models/registry.py
cp service/sidecar/src/models/onnx/__init__.py service/Embedding/src/models/onnx/__init__.py 2>/dev/null || touch service/Embedding/src/models/onnx/__init__.py
cp service/sidecar/src/models/onnx/onnx_embedder.py service/Embedding/src/models/onnx/onnx_embedder.py
cp service/sidecar/src/models/onnx/utils.py service/Embedding/src/models/onnx/utils.py
cp service/Embedding/src/models/vision/__init__.py 2>/dev/null || touch service/Embedding/src/models/vision/__init__.py  # placeholder
cp service/sidecar/src/models/vision/clip.py service/Embedding/src/models/vision/clip.py
cp service/sidecar/src/models/vision/dinov2.py service/Embedding/src/models/vision/dinov2.py
cp service/sidecar/src/models/vision/efficientnet.py service/Embedding/src/models/vision/efficientnet.py
```

- [ ] **Step 2: Rewrite imports from `src.` to `embedding.`**

```bash
find service/Embedding/src/models -name "*.py" -exec sed -i 's/from src\./from embedding./g' {} +
find service/Embedding/src/models -name "*.py" -exec sed -i 's/import src\./import embedding./g' {} +
```

- [ ] **Step 3: Verify — no remaining `from src.` imports**

```bash
grep -r "from src\." service/Embedding/src/models/ || echo "OK"
```

- [ ] **Step 4: Commit**

```bash
git add service/Embedding/src/models/
git commit -m "feat(embedding): migrate models/ module from sidecar with import rewrite"
```

---

### Task 6: Copy services/ module with import rewrite

**Files:**
- Create: `service/Embedding/src/services/__init__.py`
- Create: `service/Embedding/src/services/inference_engine.py`
- Delete: `service/Embedding/src/services/embedding_service.py` (superseded by inference_engine.py)

**Interfaces:**
- Produces: `embedding.services.inference_engine.InferenceEngine` with `get_embedder()`, `embed()`, `embed_bytes()`

- [ ] **Step 1: Copy inference engine from sidecar**

```bash
cp service/sidecar/src/services/inference_engine.py service/Embedding/src/services/inference_engine.py
```

- [ ] **Step 2: Rewrite imports**

```bash
sed -i 's/from src\./from embedding./g' service/Embedding/src/services/inference_engine.py
sed -i 's/import src\./import embedding./g' service/Embedding/src/services/inference_engine.py
```

- [ ] **Step 3: Add `embed_bytes` method to InferenceEngine**

The current `embed()` method takes `image_url: str`. Add a new method that accepts raw bytes:

```bash
python3 -c "
content = open('service/Embedding/src/services/inference_engine.py').read()

# Find the last line of the embed() method and add embed_bytes() after it
# Insert before the last empty line/newline at end of class
new_method = '''
    def embed_bytes(self, image_bytes: bytes, model_name: str = \"efficientnet_b0\") -> ValueResult[List[float]]:
        \"\"\"
        High-level interface to extract a normalized embedding from raw image bytes.
        \"\"\"
        with tracer.start_as_current_span(\"engine.embed_bytes\") as span:
            span.set_attribute(\"image.source\", \"bytes\")
            span.set_attribute(\"image.size_bytes\", len(image_bytes))
            span.set_attribute(\"model.requested\", model_name)

            embedder_result = self.get_embedder(model_name)
            if not embedder_result.is_success:
                return embedder_result

            return embedder_result.value.extract(image_bytes)
'''

# Find the end of the embed() method (the last 'return' before class ends)
# Insert embed_bytes() before the final blank line
lines = content.split('\n')
# Find the last line of the class by looking for the end
insert_pos = len(lines)
for i in range(len(lines) - 1, -1, -1):
    if lines[i].strip().startswith('return embedder_result.value.extract'):
        insert_pos = i + 1
        break

lines.insert(insert_pos, new_method)
open('service/Embedding/src/services/inference_engine.py', 'w').write('\n'.join(lines))
"
```

- [ ] **Step 4: Update services/__init__.py**

```bash
cat > service/Embedding/src/services/__init__.py << 'EOF'
"""Service layer for embedding operations."""
EOF
```

- [ ] **Step 5: Delete old embedding_service.py**

```bash
rm service/Embedding/src/services/embedding_service.py
```

- [ ] **Step 6: Verify — no remaining `from src.` imports**

```bash
grep -r "from src\." service/Embedding/src/services/ || echo "OK"
```

- [ ] **Step 7: Commit**

```bash
git add service/Embedding/src/services/
git commit -m "feat(embedding): migrate services/ module, add embed_bytes method"
```

---

### Task 7: Copy api/ module with import rewrite

**Files:**
- Create: `service/Embedding/src/api/__init__.py`
- Create: `service/Embedding/src/api/router.py`
- Create: `service/Embedding/src/api/middleware/__init__.py`
- Create: `service/Embedding/src/api/middleware/exception_handlers.py`
- Create: `service/Embedding/src/api/routers/__init__.py`
- Create: `service/Embedding/src/api/routers/inference.py`
- Create: `service/Embedding/src/api/routers/system.py`
- Delete: `service/Embedding/src/routers/health_router.py` (superseded by system.py)
- Delete: `service/Embedding/src/routers/embedding_router.py` (superseded by inference.py)
- Delete: `service/Embedding/src/routers/model_router.py` (superseded by inference.py)
- Delete: `service/Embedding/src/routers/__init__.py` (old location)
- Delete: `service/Embedding/src/middleware/exception_handler.py` (superseded by exception_handlers.py)
- Delete: `service/Embedding/src/middleware/__init__.py` (old location)

**Interfaces:**
- Produces: `embedding.api.router.api_router` (aggregates system + inference routers), exception handlers

- [ ] **Step 1: Create api/ directory structure and copy files**

```bash
mkdir -p service/Embedding/src/api/middleware
mkdir -p service/Embedding/src/api/routers

cp service/sidecar/src/api/router.py service/Embedding/src/api/router.py
cp service/sidecar/src/api/middleware/exception_handlers.py service/Embedding/src/api/middleware/exception_handlers.py
cp service/sidecar/src/api/routers/inference.py service/Embedding/src/api/routers/inference.py
cp service/sidecar/src/api/routers/system.py service/Embedding/src/api/routers/system.py

touch service/Embedding/src/api/__init__.py
touch service/Embedding/src/api/middleware/__init__.py
touch service/Embedding/src/api/routers/__init__.py
```

- [ ] **Step 2: Rewrite imports from `src.` to `embedding.`**

```bash
find service/Embedding/src/api -name "*.py" -exec sed -i 's/from src\./from embedding./g' {} +
find service/Embedding/src/api -name "*.py" -exec sed -i 's/import src\./import embedding./g' {} +
```

- [ ] **Step 3: Remove `/inference` prefix from api_router**

The sidecar's router.py includes the inference router with `prefix="/inference"`. We need endpoints at `/embeddings` and `/models` (no `/inference/` prefix):

```bash
sed -i 's/api_router.include_router(inference_router, prefix="\/inference")/api_router.include_router(inference_router)/' service/Embedding/src/api/router.py
```

- [ ] **Step 4: Delete old routers and middleware**

```bash
rm -rf service/Embedding/src/routers
rm -rf service/Embedding/src/middleware
```

- [ ] **Step 5: Add `/embeddings/bytes` endpoint to inference router**

```bash
python3 -c "
content = open('service/Embedding/src/api/routers/inference.py').read()

# Add UploadFile import if not present
if 'from fastapi import' in content and 'UploadFile' not in content:
    content = content.replace(
        'from fastapi import APIRouter, Depends, Request, Response, status, Security',
        'from fastapi import APIRouter, Depends, File, Request, Response, status, Security, UploadFile'
    )

# Add embed_bytes route after the create_embedding function
bytes_route = '''

@router.post(
    \"/embeddings/bytes\",
    response_model=ValueResult[EmbeddingResponse],
    status_code=status.HTTP_200_OK,
    summary=\"Generate Image Embedding from Bytes\",
    description=\"Generates a high-dimensional vector embedding from an uploaded image file.\"
)
@limiter.limit(settings.RATE_LIMIT)
async def create_embedding_from_bytes(
    request: Request,
    response: Response,
    image: UploadFile = File(...),
    model: str = settings.EMBEDDING_MODEL,
    key: str = Depends(verify_api_key),
    engine: InferenceEngine = Depends(get_engine),
):
    \"\"\"Generates an embedding from a multipart image upload.\"\"\"
    import time as _time
    import asyncio as _asyncio

    start_time = _time.time()
    image_bytes = await image.read()

    result = await _asyncio.to_thread(engine.embed_bytes, image_bytes, model)

    if not result.is_success:
        response.status_code = result.status_code
        return result

    duration = (_time.time() - start_time) * 1000

    return InferenceResults.Success.Embedding(
        vector=result.value,
        model_name=model,
        duration_ms=duration
    )
'''

# Insert before the list_models function or at end of file
if 'async def list_models' in content:
    content = content.replace('async def list_models', bytes_route + 'async def list_models')
else:
    content += bytes_route

open('service/Embedding/src/api/routers/inference.py', 'w').write(content)
"
```

- [ ] **Step 6: Verify — no remaining `from src.` imports**

```bash
grep -r "from src\." service/Embedding/src/api/ || echo "OK"
```

- [ ] **Step 7: Commit**

```bash
git add service/Embedding/src/api/ service/Embedding/src/routers/ service/Embedding/src/middleware/
git commit -m "feat(embedding): migrate api/ module, add /embeddings/bytes endpoint"
```

---

### Task 8: Rewrite main.py entry point

**Files:**
- Modify: `service/Embedding/src/main.py`

**Interfaces:**
- Produces: `embedding.main:app` (FastAPI app instance), `embedding.main.create_app()` factory

- [ ] **Step 1: Replace main.py with sidecar's, then rewrite imports**

```bash
cp service/sidecar/src/main.py service/Embedding/src/main.py

# Rewrite imports
sed -i 's/from src\./from embedding./g' service/Embedding/src/main.py
sed -i 's/import src\./import embedding./g' service/Embedding/src/main.py
```

- [ ] **Step 2: Fix the module-level app singleton reference**

The sidecar's main.py has `app = create_app()` at module level (line 75). The uvicorn invocation on line 86 references `"src.main:app"`. Change to `"embedding.main:app"` and `"src.main:app"` → `"embedding.main:app"`:

```bash
sed -i 's/"app": "src\.main:app"/"app": "embedding.main:app"/g' service/Embedding/src/main.py
```

- [ ] **Step 3: Verify — no remaining `from src.` or `src.` references**

```bash
grep -r "from src\." service/Embedding/src/main.py || echo "OK: No src. imports"
grep '"src\.' service/Embedding/src/main.py || echo "OK: No src. uvicorn refs"
grep "import src\." service/Embedding/src/main.py || echo "OK: No src. imports"
```

- [ ] **Step 4: Commit**

```bash
git add service/Embedding/src/main.py
git commit -m "feat(embedding): migrate main.py entry point with import rewrite"
```

---

### Task 9: Port ResNet50 and SigLIP from old Embedding

**Files:**
- Create: `service/Embedding/src/models/vision/resnet.py`
- Create: `service/Embedding/src/models/vision/siglip.py`
- Modify: `service/Embedding/src/models/__init__.py` (register ResNet50)
- Modify: `service/Embedding/src/core/constants.py` (add ResNet50 dimension)

**Interfaces:**
- Produces: `ResNet50Model` registered in ModelRegistry as `resnet50` with 2048-dim output

- [ ] **Step 1: Copy and adapt ResNet50 model**

The old ResNet50 used `BaseEmbeddingModel` from `embedding.infra.models.base`. It must be adapted to use `BaseEmbedder` from `embedding.models.base` and registered via the `@ModelRegistry.register` decorator:

```bash
cat > service/Embedding/src/models/vision/resnet.py << 'PYEOF'
"""ResNet-50 model implementation for visual features (CNN baseline)."""
import logging
import torch
from torchvision import transforms, models as tv_models

from embedding.models.base import BaseEmbedder
from embedding.core.constants import Constants
from embedding.models.registry import ModelRegistry

logger = logging.getLogger(__name__)


@ModelRegistry.register(
    "resnet50",
    metadata={
        "name": "ResNet-50",
        "dimension": 2048,
        "description": "ImageNet-pretrained ResNet-50 CNN baseline for comparative evaluation.",
        "tags": ["vision", "cnn", "baseline", "imagenet"]
    }
)
class ResNet50Embedder(BaseEmbedder):
    """ResNet-50 feature extractor via torchvision."""

    def __init__(self):
        super().__init__("resnet50", 2048)

        weights = tv_models.ResNet50_Weights.DEFAULT
        self.model = tv_models.resnet50(weights=weights)
        self.model.fc = torch.nn.Identity()
        self.model = self.model.to(self.device).eval()
        self.preprocess = weights.transforms()

    def _forward(self, image):
        tensor = self.preprocess(image).unsqueeze(0).to(self.device)
        with torch.no_grad():
            return self.model(tensor)
PYEOF
```

- [ ] **Step 2: Update models/__init__.py to trigger ResNet50 registration**

```bash
cat > service/Embedding/src/models/__init__.py << 'PYEOF'
"""ML model implementations with decorator-based registry."""
from embedding.models.registry import ModelRegistry
from embedding.models.base import BaseEmbedder

# Trigger decorator registrations for all vision models
from embedding.models.vision import clip       # noqa: F401 — CLIPEmbedder, FashionCLIPEmbedder
from embedding.models.vision import dinov2     # noqa: F401 — DINOEmbedder
from embedding.models.vision import efficientnet  # noqa: F401 — EfficientNetEmbedder
from embedding.models.vision import resnet     # noqa: F401 — ResNet50Embedder

__all__ = ["ModelRegistry", "BaseEmbedder"]
PYEOF
```

- [ ] **Step 3: Add SigLIP stub**

```bash
cat > service/Embedding/src/models/vision/siglip.py << 'PYEOF'
"""SigLIP model implementation — placeholder for future support."""
PYEOF
```

- [ ] **Step 4: Commit**

```bash
git add service/Embedding/src/models/
git commit -m "feat(embedding): port ResNet50 model, add SigLIP stub"
```

---

### Task 10: Copy scripts/ directory

**Files:**
- Create: `service/Embedding/scripts/setup.py`
- Create: `service/Embedding/scripts/test_inference.py`
- Create: `service/Embedding/scripts/export_onnx.py`
- Create: `service/Embedding/scripts/export/__init__.py`
- Create: `service/Embedding/scripts/export/base.py`
- Create: `service/Embedding/scripts/export/vision.py`

**Interfaces:**
- Produces: Setup, test, and ONNX export CLI tools

- [ ] **Step 1: Copy scripts**

```bash
mkdir -p service/Embedding/scripts/export
cp service/sidecar/scripts/setup.py service/Embedding/scripts/setup.py
cp service/sidecar/scripts/test_inference.py service/Embedding/scripts/test_inference.py
cp service/sidecar/scripts/export_onnx.py service/Embedding/scripts/export_onnx.py
cp service/sidecar/scripts/export/base.py service/Embedding/scripts/export/base.py
cp service/sidecar/scripts/export/vision.py service/Embedding/scripts/export/vision.py
touch service/Embedding/scripts/export/__init__.py
```

- [ ] **Step 2: Rewrite `from src.` → `from embedding.` in scripts**

```bash
find service/Embedding/scripts -name "*.py" -exec sed -i 's/from src\./from embedding./g' {} +
find service/Embedding/scripts -name "*.py" -exec sed -i 's/import src\./import embedding./g' {} +
```

- [ ] **Step 3: Fix path references in export scripts**

The export scripts reference the sidecar root. They use relative path logic that should still work after copy:

```bash
# Fix the default key in test_inference.py to match Embedding context
sed -i 's/inference-sidecar-key/embedding-service-key/g' service/Embedding/scripts/test_inference.py
```

- [ ] **Step 4: Commit**

```bash
git add service/Embedding/scripts/
git commit -m "feat(embedding): copy scripts/ from sidecar with import rewrite"
```

---

### Task 11: Copy Dockerfile, env files, and model directories

**Files:**
- Create: `service/Embedding/Dockerfile`
- Create: `service/Embedding/.env`
- Create: `service/Embedding/.env.template`
- Create: `service/Embedding/.env.test`
- Create: `service/Embedding/models/clip_vit_b16/.gitkeep`
- Create: `service/Embedding/models/dinov2_vits14/.gitkeep`
- Create: `service/Embedding/models/efficientnet_b0/.gitkeep`
- Create: `service/Embedding/models/fashion_clip/.gitkeep`

**Interfaces:**
- Produces: Production Dockerfile, environment configs, model artifact directories

- [ ] **Step 1: Copy Dockerfile and env files**

```bash
cp service/sidecar/Dockerfile service/Embedding/Dockerfile
cp service/sidecar/.env service/Embedding/.env
cp service/sidecar/.env.template service/Embedding/.env.template
cp service/sidecar/.env.test service/Embedding/.env.test
```

- [ ] **Step 2: Update Dockerfile references**

```bash
# Change port from 5002 to 8000
sed -i 's/PORT=5002/PORT=8000/g' service/Embedding/Dockerfile
sed -i 's/--port", "5002"/--port", "8000"/g' service/Embedding/Dockerfile
sed -i 's/:5002/:8000/g' service/Embedding/Dockerfile

# Update uvicorn app reference
sed -i 's/uvicorn", "src\.main:app"/uvicorn", "embedding.main:app"/g' service/Embedding/Dockerfile
```

- [ ] **Step 3: Update .env and .env.template — port and project name**

```bash
# Update PORT in .env
sed -i 's/^PORT=.*/PORT=8000/' service/Embedding/.env
sed -i 's/^HTTPS_PORT=.*/HTTPS_PORT=8001/' service/Embedding/.env

# Update PORT in .env.template
sed -i 's/5002/8000/g' service/Embedding/.env.template
sed -i 's/5003/8001/g' service/Embedding/.env.template

# Update .env.test — set test API key
cat > service/Embedding/.env.test << 'EOF'
# Test environment overrides
ENVIRONMENT=test
API_KEY=test-key-for-embedding-integration-tests
RATE_LIMIT=1000/minute
OTEL_EXPORTER_OTLP_ENDPOINT=
OMP_NUM_THREADS=1
MKL_NUM_THREADS=1
NUMEXPR_NUM_THREADS=1
EOF
```

- [ ] **Step 4: Create model artifact directories**

```bash
mkdir -p service/Embedding/models/clip_vit_b16
mkdir -p service/Embedding/models/dinov2_vits14
mkdir -p service/Embedding/models/efficientnet_b0
mkdir -p service/Embedding/models/fashion_clip
touch service/Embedding/models/clip_vit_b16/.gitkeep
touch service/Embedding/models/dinov2_vits14/.gitkeep
touch service/Embedding/models/efficientnet_b0/.gitkeep
touch service/Embedding/models/fashion_clip/.gitkeep
```

- [ ] **Step 5: Commit**

```bash
git add service/Embedding/Dockerfile service/Embedding/.env service/Embedding/.env.template service/Embedding/.env.test service/Embedding/models/
git commit -m "feat(embedding): add Dockerfile, env files, model artifact dirs"
```

---

### Task 12: Migrate tests with import rewrite

**Files:**
- Create: `service/Embedding/tests/conftest.py` (from sidecar, updated)
- Create: `service/Embedding/tests/__init__.py`
- Create: `service/Embedding/tests/integration/__init__.py`
- Create: `service/Embedding/tests/integration/api/test_api.py`
- Create: `service/Embedding/tests/integration/api/test_api_health.py`
- Create: `service/Embedding/tests/integration/api/test_api_inference.py`
- Create: `service/Embedding/tests/integration/api/test_api_security.py`
- Create: `service/Embedding/tests/integration/core/test_rate_limit_integration.py`
- Create: `service/Embedding/tests/integration/core/test_telemetry_integration.py`
- Create: `service/Embedding/tests/unit/__init__.py`
- Create: `service/Embedding/tests/unit/api/test_error_handlers.py`
- Create: `service/Embedding/tests/unit/core/test_config.py`
- Create: `service/Embedding/tests/unit/core/test_constants.py`
- Create: `service/Embedding/tests/unit/core/test_rate_limit.py`
- Create: `service/Embedding/tests/unit/core/test_security.py`
- Create: `service/Embedding/tests/unit/core/test_telemetry.py`
- Create: `service/Embedding/tests/unit/models/test_base.py`
- Create: `service/Embedding/tests/unit/models/test_registry.py`
- Create: `service/Embedding/tests/unit/schemas/__init__.py`
- Create: `service/Embedding/tests/unit/schemas/test_failures.py`
- Create: `service/Embedding/tests/unit/schemas/test_result.py`
- Create: `service/Embedding/tests/unit/services/test_engine.py`
- Create: `service/Embedding/tests/unit/services/test_inference_engine.py`
- Delete: `service/Embedding/tests/test_health.py` (superseded by integration tests)
- Delete: `service/Embedding/tests/test_embedding.py` (superseded by integration tests)
- Delete: `service/Embedding/tests/test_exception_handler.py` (superseded by unit tests)
- Delete: `service/Embedding/tests/conftest.py` (replaced by sidecar's)

**Interfaces:**
- Produces: Full test suite with 85+ tests, all imports rewritten to `embedding.*`

- [ ] **Step 1: Copy all sidecar tests**

```bash
# Create directory structure
mkdir -p service/Embedding/tests/integration/api
mkdir -p service/Embedding/tests/integration/core
mkdir -p service/Embedding/tests/unit/api
mkdir -p service/Embedding/tests/unit/core
mkdir -p service/Embedding/tests/unit/models
mkdir -p service/Embedding/tests/unit/schemas
mkdir -p service/Embedding/tests/unit/services

# Copy conftest.py
cp service/sidecar/tests/conftest.py service/Embedding/tests/conftest.py

# Copy integration tests
cp service/sidecar/tests/integration/__init__.py service/Embedding/tests/integration/__init__.py 2>/dev/null || touch service/Embedding/tests/integration/__init__.py
cp service/sidecar/tests/integration/api/test_api.py service/Embedding/tests/integration/api/test_api.py 2>/dev/null || true
cp service/sidecar/tests/integration/api/test_api_health.py service/Embedding/tests/integration/api/test_api_health.py 2>/dev/null || true
cp service/sidecar/tests/integration/api/test_api_inference.py service/Embedding/tests/integration/api/test_api_inference.py 2>/dev/null || true
cp service/sidecar/tests/integration/api/test_api_security.py service/Embedding/tests/integration/api/test_api_security.py 2>/dev/null || true
cp service/sidecar/tests/integration/core/test_rate_limit_integration.py service/Embedding/tests/integration/core/test_rate_limit_integration.py 2>/dev/null || true
cp service/sidecar/tests/integration/core/test_telemetry_integration.py service/Embedding/tests/integration/core/test_telemetry_integration.py 2>/dev/null || true

# Copy unit tests
cp service/sidecar/tests/unit/__init__.py service/Embedding/tests/unit/__init__.py 2>/dev/null || touch service/Embedding/tests/unit/__init__.py
cp service/sidecar/tests/unit/api/test_error_handlers.py service/Embedding/tests/unit/api/test_error_handlers.py 2>/dev/null || true
cp service/sidecar/tests/unit/core/test_config.py service/Embedding/tests/unit/core/test_config.py 2>/dev/null || true
cp service/sidecar/tests/unit/core/test_constants.py service/Embedding/tests/unit/core/test_constants.py 2>/dev/null || true
cp service/sidecar/tests/unit/core/test_rate_limit.py service/Embedding/tests/unit/core/test_rate_limit.py 2>/dev/null || true
cp service/sidecar/tests/unit/core/test_security.py service/Embedding/tests/unit/core/test_security.py 2>/dev/null || true
cp service/sidecar/tests/unit/core/test_telemetry.py service/Embedding/tests/unit/core/test_telemetry.py 2>/dev/null || true
cp service/sidecar/tests/unit/models/test_base.py service/Embedding/tests/unit/models/test_base.py 2>/dev/null || true
cp service/sidecar/tests/unit/models/test_registry.py service/Embedding/tests/unit/models/test_registry.py 2>/dev/null || true
cp service/sidecar/tests/unit/schemas/__init__.py service/Embedding/tests/unit/schemas/__init__.py 2>/dev/null || touch service/Embedding/tests/unit/schemas/__init__.py
cp service/sidecar/tests/unit/schemas/test_failures.py service/Embedding/tests/unit/schemas/test_failures.py 2>/dev/null || true
cp service/sidecar/tests/unit/schemas/test_result.py service/Embedding/tests/unit/schemas/test_result.py 2>/dev/null || true
cp service/sidecar/tests/unit/services/test_engine.py service/Embedding/tests/unit/services/test_engine.py 2>/dev/null || true
cp service/sidecar/tests/unit/services/test_inference_engine.py service/Embedding/tests/unit/services/test_inference_engine.py 2>/dev/null || true
```

- [ ] **Step 2: Rewrite imports in all tests**

```bash
find service/Embedding/tests -name "*.py" -exec sed -i 's/from src\./from embedding./g' {} +
find service/Embedding/tests -name "*.py" -exec sed -i 's/import src\./import embedding./g' {} +
```

- [ ] **Step 3: Update conftest.py references**

```bash
# Remove old conftest.py references to "sidecar"
sed -i 's/sidecar/embedding/g' service/Embedding/tests/conftest.py

# Update the app fixture import
sed -i 's/from src\.main import app as _app/from embedding.main import app as _app/' service/Embedding/tests/conftest.py

# Update the test key identifier
sed -i 's/test-key-for-sidecar-integration-tests/test-key-for-embedding-integration-tests/g' service/Embedding/tests/conftest.py
sed -i 's/sidecar\/tests\/conftest.py/embedding\/tests\/conftest.py/g' service/Embedding/tests/conftest.py
```

- [ ] **Step 4: Delete old Embedding test files**

```bash
rm -f service/Embedding/tests/test_health.py
rm -f service/Embedding/tests/test_embedding.py
rm -f service/Embedding/tests/test_exception_handler.py
rm -f service/Embedding/tests/__init__.py 2>/dev/null
touch service/Embedding/tests/__init__.py
rm -rf service/Embedding/tests/e2e 2>/dev/null
```

- [ ] **Step 5: Verify — no remaining `from src.` imports in tests**

```bash
grep -r "from src\." service/Embedding/tests/ || echo "OK: No src. imports in tests"
```

- [ ] **Step 6: Commit**

```bash
git add service/Embedding/tests/
git commit -m "test(embedding): migrate 85+ tests from sidecar with import rewrite"
```

---

### Task 13: Merge and update pyproject.toml

**Files:**
- Modify: `service/Embedding/pyproject.toml`

**Interfaces:**
- Produces: Merged pyproject.toml with all sidecar deps, setuptools packages matching new structure, python >=3.12, pytest config

- [ ] **Step 1: Write the merged pyproject.toml**

```bash
cat > service/Embedding/pyproject.toml << 'TOML'
[build-system]
requires = ["setuptools>=75"]
build-backend = "setuptools.build_meta"

[project]
name = "embedding"
version = "0.1.0"
description = "Embedding generation service for ReSys.Shop"
requires-python = ">=3.12"
dependencies = [
    "fastapi[standard]>=0.115.0",
    "uvicorn>=0.20.0",
    "pydantic>=2.0",
    "pydantic-settings>=2.0.0",
    "python-multipart>=0.0.6",
    "httpx>=0.24.0",
    "slowapi>=0.1.9",
    "scalar-fastapi",
    "torch>=2.0.0",
    "torchvision>=0.15.0",
    "numpy>=1.24.0",
    "pillow>=10.0.0",
    "transformers>=4.30.0",
    "onnxruntime>=1.17.0",
    "ftfy",
    "regex",
    "opentelemetry-api>=1.30.0",
    "opentelemetry-sdk>=1.30.0",
    "opentelemetry-exporter-otlp>=1.30.0",
    "opentelemetry-instrumentation-fastapi>=0.51b0",
    "opentelemetry-instrumentation-logging>=0.51b0",
    "python-json-logger>=2.0.0",
    "rich>=13.0.0",
    "psutil>=5.9",
]

[[tool.uv.index]]
name = "pytorch-cpu"
url = "https://download.pytorch.org/whl/cpu"
explicit = true

[tool.uv.sources]
torch = { index = "pytorch-cpu" }
torchvision = { index = "pytorch-cpu" }

[tool.setuptools.package-dir]
"embedding" = "src"

[tool.setuptools]
packages = [
    "embedding",
    "embedding.core",
    "embedding.api",
    "embedding.api.middleware",
    "embedding.api.routers",
    "embedding.models",
    "embedding.models.onnx",
    "embedding.models.vision",
    "embedding.schemas",
    "embedding.schemas.results",
    "embedding.schemas.inferences",
    "embedding.schemas.images",
    "embedding.schemas.registries",
    "embedding.services",
]

[tool.ruff]
line-length = 100
target-version = "py312"

[tool.ruff.lint]
select = ["E", "F", "W", "I"]

[dependency-groups]
dev = [
    "pytest>=8.0.0",
    "pytest-env>=1.5.0",
    "pytest-asyncio>=1.3.0",
    "httpx>=0.28",
    "ruff>=0.15.20",
    "onnxscript",
]

[tool.pytest.ini_options]
asyncio_mode = "auto"
asyncio_default_fixture_loop_scope = "function"
env_files = ".env.test"
testpaths = "tests"
addopts = "-v --tb=short"
markers = [
    "integration: marks end-to-end tests that hit the full FastAPI stack"
]
log_cli = true
log_cli_level = "WARNING"
TOML
```

- [ ] **Step 2: Commit**

```bash
git add service/Embedding/pyproject.toml
git commit -m "build(embedding): merge pyproject.toml — sidecar deps, setuptools packages, py312"
```

---

### Task 14: Update .python-version and regenerate uv.lock

**Files:**
- Modify: `service/Embedding/.python-version`
- Modify: `service/Embedding/uv.lock` (regenerated)

- [ ] **Step 1: Set Python version to 3.12**

```bash
echo "3.12" > service/Embedding/.python-version
```

- [ ] **Step 2: Regenerate uv.lock**

```bash
cd service/Embedding && rm -f uv.lock && uv lock
```

Expected: Lock file regenerated without errors. This may take a few minutes as it resolves all dependencies.

- [ ] **Step 3: Sync dependencies**

```bash
cd service/Embedding && uv sync
```

Expected: All dependencies installed, no version conflicts.

- [ ] **Step 4: Commit**

```bash
git add service/Embedding/.python-version service/Embedding/uv.lock
git commit -m "build(embedding): set python 3.12, regenerate uv.lock"
```

---

### Task 15: Run ruff lint check and fix issues

**Files:**
- (any files with lint errors)

- [ ] **Step 1: Run ruff check**

```bash
cd service/Embedding && uv run ruff check .
```

Expected: May report import ordering issues (I001) or unused imports (F401). Fix each reported issue.

- [ ] **Step 2: Auto-fix fixable issues**

```bash
cd service/Embedding && uv run ruff check --fix .
```

- [ ] **Step 3: Verify zero errors**

```bash
cd service/Embedding && uv run ruff check .
```

Expected: No output (all clean).

- [ ] **Step 4: Commit any fixes**

```bash
git add service/Embedding/
git commit -m "style(embedding): fix ruff lint issues"
```

---

### Task 16: Run pytest and fix failures

- [ ] **Step 1: Run the full test suite**

```bash
cd service/Embedding && uv run pytest
```

Expected: Tests may fail due to:
- Missing `__init__.py` files in test subdirectories
- Import path mismatches in test files
- Hardcoded paths referencing "sidecar" in test assertions

- [ ] **Step 2: Fix any import errors**

Common fixes:
- Ensure all test subdirectories have `__init__.py` (even empty)
- Check `grep -r "sidecar" service/Embedding/tests/` for leftover references

```bash
# Ensure __init__.py in all test directories
find service/Embedding/tests -type d -exec touch {}/__init__.py \;
```

- [ ] **Step 3: Fix any assertion errors referencing "sidecar"**

```bash
grep -r "sidecar" service/Embedding/tests/ && echo "Found sidecar refs — fix manually" || echo "Clean"
```

If found, replace with "embedding":
```bash
find service/Embedding/tests -name "*.py" -exec sed -i 's/sidecar/embedding/g' {} +
```

- [ ] **Step 4: Re-run tests until they pass**

```bash
cd service/Embedding && uv run pytest
```

Expected: All tests pass.

- [ ] **Step 5: Commit**

```bash
git add service/Embedding/tests/
git commit -m "test(embedding): fix test import paths and sidecar references"
```

---

### Task 17: Verify uvicorn startup and dotnet build

- [ ] **Step 1: Test uvicorn import**

```bash
cd service/Embedding && timeout 5 uv run python -c "from embedding.main import app; print('OK: app imported, title =', app.title)" 2>&1
```

Expected: `OK: app imported, title = Embedding Service`

- [ ] **Step 2: Test dotnet build (Aspire references)**

```bash
cd /home/qingfa/Repos/ReSys.Shop && dotnet build 2>&1 | tail -5
```

Expected: Build succeeds. The Aspire app host references `service/Embedding/` and expects `embedding.main:app` — both unchanged.

- [ ] **Step 3: Commit (if any build fixes needed)**

```bash
git add -A
git commit -m "chore(embedding): verify uvicorn startup and dotnet build"
```

---

### Task 18: Delete service/sidecar/

- [ ] **Step 1: Remove the sidecar directory**

```bash
rm -rf service/sidecar
```

- [ ] **Step 2: Verify no references remain**

```bash
grep -r "service/sidecar" /home/qingfa/Repos/ReSys.Shop --include="*.py" --include="*.cs" --include="*.csproj" --include="*.json" --include="*.yaml" --include="*.yml" --include="*.toml" --include="*.md" --exclude-dir=".git" 2>/dev/null || echo "OK: No remaining references"
```

- [ ] **Step 3: Commit**

```bash
git add service/sidecar/
git commit -m "chore: delete service/sidecar/ — migrated to service/Embedding/"
```

---

### Task 19: Final verification

- [ ] **Step 1: Full test suite**

```bash
cd service/Embedding && uv run ruff check . && uv run pytest
```

- [ ] **Step 2: dotnet build**

```bash
cd /home/qingfa/Repos/ReSys.Shop && dotnet build
```

- [ ] **Step 3: Verify structure matches spec**

```bash
echo "=== Expected structure ==="
echo "service/Embedding/"
echo "  pyproject.toml"
echo "  .python-version (3.12)"
echo "  Dockerfile"
echo "  .gitignore"
echo "  .env / .env.template / .env.test"
echo "  src/"
echo "    main.py (create_app factory)"
echo "    api/ (router, middleware, routers/)"
echo "    core/ (config, constants, rate_limit, security, telemetry)"
echo "    models/ (base, registry, onnx/, vision/)"
echo "    schemas/ (results/, inferences/, images/, registries/)"
echo "    services/ (inference_engine.py)"
echo "  tests/ (conftest, integration/, unit/)"
echo "  scripts/ (setup, test_inference, export_onnx, export/)"
echo "  models/ (clip_vit_b16, dinov2_vits14, efficientnet_b0, fashion_clip)"
echo ""

echo "=== Actual structure ==="
ls -la service/Embedding/
echo "---"
ls -la service/Embedding/src/
echo "---"
ls -la service/Embedding/src/api/
echo "---"
ls -la service/Embedding/src/models/
echo "---"
ls -la service/Embedding/tests/
```

- [ ] **Step 4: Verify no sidecar directory**

```bash
test -d service/sidecar && echo "ERROR: sidecar still exists" || echo "OK: sidecar removed"
```

- [ ] **Step 5: Verify no build artifacts**

```bash
test -d service/Embedding/build && echo "ERROR: build/ still exists" || echo "OK: build/ removed"
test -d service/Embedding/embedding.egg-info && echo "ERROR: egg-info still exists" || echo "OK: egg-info removed"
```

- [ ] **Step 6: Final commit if needed**

```bash
git status
```
