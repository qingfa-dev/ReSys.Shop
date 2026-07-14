"""
Inference-related API endpoints.
"""
import time
import asyncio
from typing import List
from pathlib import Path
from fastapi import APIRouter, Depends, Request, Response, status, Security
from fastapi.security import APIKeyHeader

from src.schemas import (
    ValueResult, 
    InferenceResults, 
    EmbeddingRequest, 
    EmbeddingResponse, 
    ModelMetadata
)
from src.services.inference_engine import InferenceEngine
from src.core.config import settings
from src.core.rate_limit import limiter
from src.models.registry import ModelRegistry
from src.models.onnx.utils import infer_onnx_dim

router = APIRouter(tags=["inference"])

# API Key header scheme for sidecar security
api_key_header = APIKeyHeader(name="X-API-Key", auto_error=False)


async def verify_api_key(api_key: str = Security(api_key_header)) -> str:
    """Validates the sidecar API key."""
    if api_key != settings.API_KEY:
        from fastapi import HTTPException
        raise HTTPException(status_code=403, detail="Invalid API Key")
    return api_key


def get_engine() -> InferenceEngine:
    """Dependency provider for the InferenceEngine singleton."""
    # Note: In a real app, this might be a singleton managed by the app state
    from functools import lru_cache
    @lru_cache(maxsize=1)
    def _get():
        return InferenceEngine()
    return _get()


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
    key: str = Depends(verify_api_key),
    engine: InferenceEngine = Depends(get_engine),
):
    """Generates a high-dimensional vector embedding for the provided image URL."""
    start_time = time.time()
    
    # Move CPU-intensive inference to a thread pool
    result = await asyncio.to_thread(engine.embed, body.image_url, body.model)

    if not result.is_success:
        response.status_code = result.status_code
        return result

    duration = (time.time() - start_time) * 1000
    
    return InferenceResults.Success.Embedding(
        vector=result.value,
        model_name=body.model,
        duration_ms=duration
    )


@router.get(
    "/models",
    response_model=ValueResult[List[ModelMetadata]],
    summary="List Available Models",
    description="Returns metadata for both registered skills and discovered ONNX models."
)
async def list_models(key: str = Depends(verify_api_key)):
    """Dynamic discovery of all models including disk-based ONNX models."""
    all_meta = ModelRegistry.get_all_metadata().copy()
    models = []
    
    # 1. Add explicitly registered skills (PyTorch)
    for model_id, meta in all_meta.items():
        if model_id == "onnx": continue # Skip the generic wrapper
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
        # Look for model.onnx in subfolders
        for model_dir in onnx_root.iterdir():
            if not model_dir.is_dir(): continue
            
            onnx_file = model_dir / "model.onnx"
            if onnx_file.exists():
                model_id = f"onnx/{model_dir.name}"
                try:
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
                    continue
                    
    return InferenceResults.Success.Models(models)
