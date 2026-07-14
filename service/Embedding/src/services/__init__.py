"""Embedding generation orchestration.

Exports the model registry and encoding service.
"""

from embedding.services.embedding_service import (
    MODEL_REGISTRY,
    encode_image,
    get_model,
    list_available_models,
    warmup_default_model,
)

__all__ = [
    "MODEL_REGISTRY",
    "encode_image",
    "get_model",
    "list_available_models",
    "warmup_default_model",
]
