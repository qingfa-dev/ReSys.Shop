"""Model registry and embedding service implementing the Strategy pattern.

The service maintains a registry of BaseEmbeddingModel subclasses.
Model selection happens at runtime via:
1. The `model` query parameter on POST /embeddings
2. The `EMBEDDING_MODEL` environment variable (fallback default)

This design enables the comparative evaluation in thesis §11.5:
- Swap models by restarting the sidecar with a different EMBEDDING_MODEL.
- No code changes required in the .NET backend or database schema.
"""

import os
import time
from typing import Any

import numpy as np
from PIL import Image

from embedding.infra.models.base import BaseEmbeddingModel
from embedding.infra.models.clip import CLIPGenericModel
from embedding.infra.models.efficientnet_model import EfficientNetB0Model
from embedding.infra.models.fashion_clip import FashionCLIPModel
from embedding.infra.models.resnet_model import ResNet50Model

# ---------------------------------------------------------------------------
# Registry
# ---------------------------------------------------------------------------

MODEL_REGISTRY: dict[str, type[BaseEmbeddingModel]] = {
    FashionCLIPModel.model_name: FashionCLIPModel,
    ResNet50Model.model_name: ResNet50Model,
    EfficientNetB0Model.model_name: EfficientNetB0Model,
    CLIPGenericModel.model_name: CLIPGenericModel,
}

_DEFAULT_MODEL_NAME = os.getenv("EMBEDDING_MODEL", FashionCLIPModel.model_name)

# ---------------------------------------------------------------------------
# Singleton cache of loaded models (process-scoped)
# ---------------------------------------------------------------------------

_loaded_models: dict[str, BaseEmbeddingModel] = {}


def get_model(model_name: str | None = None) -> BaseEmbeddingModel:
    """Return (and cache) an embedding model instance by name.

    Args:
        model_name: One of the keys in MODEL_REGISTRY. If None, uses
            the EMBEDDING_MODEL env var (defaults to "fashion-clip").

    Raises:
        ValueError: If the requested model is not registered.
    """
    name = (model_name or _DEFAULT_MODEL_NAME).lower().strip()
    if name not in MODEL_REGISTRY:
        available = ", ".join(MODEL_REGISTRY.keys())
        raise ValueError(f"Unknown model '{name}'. Available: {available}")

    if name not in _loaded_models:
        model_cls = MODEL_REGISTRY[name]
        _loaded_models[name] = model_cls()

    return _loaded_models[name]


def list_available_models() -> list[dict[str, Any]]:
    """Return metadata for every registered model (used by GET /models)."""
    return [
        {
            "id": cls.model_name,
            "name": cls.__doc__.split("\n")[0] if cls.__doc__ else cls.__name__,
            "dimension": cls.vector_dim,
            "description": cls.__doc__,
            "is_onnx": False,
            "tags": [],
        }
        for cls in MODEL_REGISTRY.values()
    ]


def encode_image(image: Image.Image, model_name: str | None = None) -> dict[str, Any]:
    """Generate an embedding and telemetry for a single image.

    Returns:
        dict with keys:
            - embedding: list[float] (L2-normalized)
            - model_name: str
            - vector_dim: int
            - elapsed_ms: float (embedding generation time only)
    """
    model = get_model(model_name)
    model.warmup()  # no-op if already loaded

    start = time.perf_counter()
    vector = model.encode_image(image)
    elapsed_ms = (time.perf_counter() - start) * 1000.0

    return {
        "embedding": vector.tolist(),
        "model_name": model.model_name,
        "vector_dim": model.vector_dim,
        "elapsed_ms": round(elapsed_ms, 2),
    }


def warmup_default_model() -> None:
    """Preload the default model at sidecar startup."""
    model = get_model()
    model.warmup()
