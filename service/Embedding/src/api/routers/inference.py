"""
Inference-related API endpoints.
"""
import asyncio
import logging
import time
from functools import lru_cache
from pathlib import Path
from typing import List

from embedding.core.config import settings
from embedding.core.constants import Constants
from embedding.core.rate_limit import limiter
from embedding.models.onnx.utils import infer_onnx_dim
from embedding.models.registry import ModelRegistry
from embedding.schemas import (
    EmbeddingRequest,
    EmbeddingResponse,
    InferenceResults,
    ModelMetadata,
    ValueResult,
)
from embedding.services.inference_engine import InferenceEngine
from fastapi import APIRouter, Depends, File, Request, Response, UploadFile, status

router = APIRouter(tags=["inference"])
logger = logging.getLogger(__name__)


@lru_cache(maxsize=1)
def get_engine() -> InferenceEngine:
    """Dependency provider: Returns a cached InferenceEngine singleton.

    Uses lru_cache(maxsize=1) to ensure a single instance per process.
    """
    return InferenceEngine()


@router.post(
    "/embeddings",
    response_model=ValueResult[EmbeddingResponse],
    status_code=status.HTTP_200_OK,
    summary="Generate Image Embedding",
    description="Generates a high-dimensional vector embedding for the provided image URL."
)
@limiter.limit(settings.RATE_LIMIT)
async def create_embedding(
    request: Request,
    body: EmbeddingRequest,
    response: Response,
    engine: InferenceEngine = Depends(get_engine),
):
    """Generates a high-dimensional vector embedding for the provided image URL.

    Args:
        request: FastAPI request object (injected by the framework).
        body: The embedding request containing image_url and optional model.
        response: FastAPI response object (injected, used to set status code).
        engine: Cached InferenceEngine instance (injected by Depends).

    Returns:
        ValueResult containing EmbeddingResponse on success, or an error result.
    """
    start_time = time.time()

    try:
        # Defer: Run CPU-intensive inference in a thread pool to avoid blocking the event loop
        result = await asyncio.to_thread(engine.embed, body.image_url, body.model_name)

        if not result.is_success:
            response.status_code = result.status_code
            return result

        duration = (time.time() - start_time) * 1000

        return InferenceResults.Success.Embedding(
            vector=result.value,
            model_name=body.model_name,
            duration_ms=duration
        )
    except MemoryError:
        logger.critical(
            "Out of memory during embedding inference for model=%s",
            body.model_name,
            exc_info=True
        )
        response.status_code = status.HTTP_503_SERVICE_UNAVAILABLE
        return InferenceResults.Errors.CommunicationFailed(
            "Out of memory during inference"
        )
    except Exception as e:
        logger.error(
            "Embedding inference failed: %s: %s",
            type(e).__name__,
            e,
            exc_info=True
        )
        response.status_code = status.HTTP_500_INTERNAL_SERVER_ERROR
        error_msg = f"Inference error: {type(e).__name__}: {e}"
        return InferenceResults.Errors.CommunicationFailed(error_msg)


@router.post(
    "/embeddings/bytes",
    response_model=ValueResult[EmbeddingResponse],
    status_code=status.HTTP_200_OK,
    summary="Generate Image Embedding from Bytes",
    description="Generates a high-dimensional vector embedding from an uploaded image file."
)
@limiter.limit(settings.RATE_LIMIT)
async def create_embedding_from_bytes(
    request: Request,
    response: Response,
    image: UploadFile = File(...),
    model_name: str = settings.EMBEDDING_MODEL,
    engine: InferenceEngine = Depends(get_engine),
):
    """Generates an embedding from a multipart image upload.

    Args:
        request: FastAPI request object (injected).
        response: FastAPI response object (injected, used to set status code).
        image: The uploaded image file (multipart form data).
        model_name: Model identifier (default from settings.EMBEDDING_MODEL).
        engine: Cached InferenceEngine instance (injected by Depends).

    Returns:
        ValueResult containing EmbeddingResponse on success, or an error result.
    """
    import asyncio as _asyncio
    import time as _time

    start_time = _time.time()

    try:
        # Read: Load uploaded file bytes into memory
        image_bytes = await image.read()

        # Defer: Run CPU-intensive inference in a thread pool
        result = await _asyncio.to_thread(engine.embed_bytes, image_bytes, model_name)

        if not result.is_success:
            response.status_code = result.status_code
            return result

        duration = (_time.time() - start_time) * 1000

        return InferenceResults.Success.Embedding(
            vector=result.value,
            model_name=model_name,
            duration_ms=duration
        )
    except MemoryError:
        logger.critical(
            "Out of memory during byte embedding inference for model=%s",
            model_name,
            exc_info=True
        )
        response.status_code = status.HTTP_503_SERVICE_UNAVAILABLE
        return InferenceResults.Errors.CommunicationFailed(
            "Out of memory during inference"
        )
    except Exception as e:
        logger.error(
            "Byte embedding inference failed: %s: %s",
            type(e).__name__,
            e,
            exc_info=True
        )
        response.status_code = status.HTTP_500_INTERNAL_SERVER_ERROR
        error_msg = f"Inference error: {type(e).__name__}: {e}"
        return InferenceResults.Errors.CommunicationFailed(error_msg)


@router.get(
    "/models",
    response_model=ValueResult[List[ModelMetadata]],
    summary="List Available Models",
    description="Returns metadata for both registered skills and discovered ONNX models."
)
async def list_models():
    """Dynamic discovery of all models including disk-based ONNX models.

    Combines explicitly registered PyTorch skills with ONNX models discovered
    on disk under the configured ONNX_MODEL_DIR.

    Returns:
        ValueResult containing a list of ModelMetadata for all available models.
    """
    all_meta = ModelRegistry.get_all_metadata().copy()
    models = []

    # 1. Add explicitly registered skills (PyTorch)
    for model_id, meta in all_meta.items():
        if model_id == "onnx":
            # Skip: Generic ONNX wrapper — not user-selectable directly
            continue
        models.append(ModelMetadata(
            id=model_id,
            name=meta.get("name", model_id),
            dimension=meta.get("dimension", 0),
            description=meta.get("description"),
            is_onnx=False,
            tags=meta.get("tags", [])
        ))

    # 2. Discover ONNX models on disk
    onnx_root = Path(settings.ONNX_MODEL_DIR)
    if onnx_root.exists():
        # Discover: Scan subdirectories for model.onnx files
        for model_dir in onnx_root.iterdir():
            if not model_dir.is_dir():
                continue

            onnx_file = model_dir / Constants.Strings.ONNX_FILENAME
            if onnx_file.exists():
                model_id = f"onnx/{model_dir.name}"
                try:
                    # Read: Infer output dimension from ONNX graph metadata
                    dim = infer_onnx_dim(str(onnx_file))
                    models.append(ModelMetadata(
                        id=model_id,
                        name=f"{model_dir.name.replace('_', ' ').title()} (ONNX)",
                        dimension=dim,
                        description=f"Optimized ONNX version of {model_dir.name}.",
                        is_onnx=True,
                        tags=["onnx", "optimized", "vision"]
                    ))
                except Exception:
                    # Suppress: Skip ONNX files that fail dimension inference
                    continue

    return InferenceResults.Success.Models(models)
