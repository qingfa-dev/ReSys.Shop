"""
Unified schema registry for the Inference service.
Combines domain-specific models and standardized response envelopes.
"""
from src.schemas.results import (
    Result, 
    ValueResult, 
    Failure, 
    FailureType
)
from src.schemas.inferences import (
    InferenceResults,
    EmbeddingRequest,
    EmbeddingResponse,
    ModelMetadata
)
from src.schemas.images import ImageResults
from src.schemas.registries import RegistryResults

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
