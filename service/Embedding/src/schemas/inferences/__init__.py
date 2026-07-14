"""
Specialized results, errors, and models for Inference operations.
"""
from typing import Any, Dict, List, Optional

from embedding.schemas.inferences.models import (  # noqa: F401
    EmbeddingRequest,
    EmbeddingResponse,
    ModelMetadata,
)
from embedding.schemas.results.error import Error
from embedding.schemas.results.result import ValueResult


class InferenceResults:
    """Namespace for inference success and error results."""

    class Success:
        """Success result factories for inferences."""

        @staticmethod
        def Ok(value: Any) -> ValueResult[Any]:
            """Creates a generic success result with the given value.

            Args:
                value: The data payload to wrap.
            """
            return ValueResult.ok_value(value)

        @staticmethod
        def Models(models: List[ModelMetadata]) -> ValueResult[List[ModelMetadata]]:
            """Standardized response for listing available models.

            Args:
                models: A list of model metadata objects.

            Returns:
                A ValueResult containing the model list.
            """
            return ValueResult.ok_value(models)

        @staticmethod
        def Embedding(
            vector: List[float],
            model_name: str,
            duration_ms: float,
            metadata: Optional[Dict[str, Any]] = None
        ) -> ValueResult[EmbeddingResponse]:
            """Standardized success response for an embedding generation.

            Args:
                vector: The L2-normalized embedding vector.
                model_name: Identifier of the model that produced the vector.
                duration_ms: Processing time in milliseconds.
                metadata: Optional additional context for the response.

            Returns:
                A ValueResult containing the EmbeddingResponse.
            """
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
        def ModelNotFound(model_name: str) -> Error:
            """Creates a not-found error for an unknown model name.

            Args:
                model_name: The requested model identifier that was not found.
            """
            return Error.not_found(
                "Model.NotFound",
                f"Model skill '{model_name}' is not supported."
            )

        @staticmethod
        def OnnxNotFound(path_or_message: str) -> Error:
            """Creates a not-found error for a missing ONNX model file.

            Args:
                path_or_message: File path or error detail describing what is missing.
            """
            return Error.not_found(
                "Model.NotFound",
                f"ONNX model not found: {path_or_message}"
            )

        @staticmethod
        def LoadError(model_name: str, detail: str) -> Error:
            """Creates an internal error for model loading failures.

            Args:
                model_name: The model that failed to load.
                detail: Error details describing the failure.
            """
            return Error.internal_error(
                "Model.LoadError",
                f"Failed to load model '{model_name}': {detail}"
            )

        @staticmethod
        def InferenceFailed(model_name: str, detail: str) -> Error:
            """Creates an internal error for inference execution failures.

            Args:
                model_name: The model that failed during inference.
                detail: Error details describing the failure.
            """
            return Error.internal_error(
                "Inference.Error",
                f"[{model_name}] Inference failed: {detail}"
            )

        @staticmethod
        def DeviceError(model_name: str, device: str, detail: str) -> Error:
            """Creates an internal error for hardware-specific failures (e.g. CUDA OOM).

            Args:
                model_name: The model that encountered the hardware issue.
                device: The device name (e.g. 'cuda:0').
                detail: Error details describing the hardware failure.
            """
            return Error.internal_error(
                "Inference.DeviceError",
                f"[{model_name}] Hardware failure on {device}: {detail}"
            )
