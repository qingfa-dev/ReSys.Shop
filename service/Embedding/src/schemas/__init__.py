"""
Unified schema registry for the Inference service.
Combines domain-specific models and standardized response envelopes.
"""
from embedding.schemas.images import ImageResults
from embedding.schemas.inferences import (
    EmbeddingRequest,
    EmbeddingResponse,
    InferenceResults,
    ModelMetadata,
)
from embedding.schemas.registries import RegistryResults
from embedding.schemas.results import Failure, FailureType, Result, ValueResult

__all__ = [
    "Result",
    "ValueResult",
    "Failure",
    "FailureType",
    "InferenceResults",
    "ImageResults",
    "RegistryResults",
    "EmbeddingRequest",
    "EmbeddingResponse",
    "ModelMetadata",
]
