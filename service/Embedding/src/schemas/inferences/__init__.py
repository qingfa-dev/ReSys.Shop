"""
Specialized results, errors, and models for Inference operations.
"""
from typing import Any, Dict, List, Optional

from embedding.schemas.inferences.models import EmbeddingRequest, EmbeddingResponse, ModelMetadata
from embedding.schemas.results.failure import Failure
from embedding.schemas.results.result import ValueResult


class InferenceResults:
    """Namespace for inference success and error results."""

    class Success:
        """Success result factories for inferences."""

        @staticmethod
        def Ok(value: Any) -> ValueResult[Any]:
            return ValueResult.ok_value(value)

        @staticmethod
        def Models(models: List[ModelMetadata]) -> ValueResult[List[ModelMetadata]]:
            """Standardized response for listing available models."""
            return ValueResult.ok_value(models)

        @staticmethod
        def Embedding(
            vector: List[float],
            model_name: str,
            duration_ms: float,
            metadata: Optional[Dict[str, Any]] = None
        ) -> ValueResult[EmbeddingResponse]:
            """Standardized success response for an embedding generation."""
            data = EmbeddingResponse(
                vector=vector,
                model_version=model_name,
                dimension=len(vector),
                metadata={
                    "processing_time_ms": round(duration_ms, 2),
                    **(metadata or {})
                }
            )
            return ValueResult.ok_value(data)

    class Errors:
        """Error result factories for inferences."""

        @staticmethod
        def ModelNotFound(model_name: str) -> Failure:
            return Failure.not_found(
                "Model.NotFound",
                f"Model skill '{model_name}' is not supported."
            )

        @staticmethod
        def OnnxNotFound(path_or_message: str) -> Failure:
            """ONNX file missing on disk or error detail."""
            return Failure.not_found(
                "Model.NotFound",
                f"ONNX model not found: {path_or_message}"
            )

        @staticmethod
        def LoadError(model_name: str, detail: str) -> Failure:
            return Failure.internal_error(
                "Model.LoadError",
                f"Failed to load model '{model_name}': {detail}"
            )

        @staticmethod
        def InferenceFailed(model_name: str, detail: str) -> Failure:
            return Failure.internal_error(
                "Inference.Error",
                f"[{model_name}] Inference failed: {detail}"
            )

        @staticmethod
        def DeviceError(model_name: str, device: str, detail: str) -> Failure:
            """Hardware-specific failure (e.g. CUDA out of memory)."""
            return Failure.internal_error(
                "Inference.DeviceError",
                f"[{model_name}] Hardware failure on {device}: {detail}"
            )
