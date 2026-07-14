"""
Unified schema registry for the Inference service.
Combines domain-specific models and standardized response envelopes.
"""
from embedding.schemas.results import (
    Result, 
    ValueResult, 
    Failure, 
    FailureType
)
from embedding.schemas.inferences import (
    InferenceResults,
    EmbeddingRequest,
    EmbeddingResponse,
    ModelMetadata
)
from embedding.schemas.images import ImageResults
from embedding.schemas.registries import RegistryResults

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
