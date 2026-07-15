"""
Infrastructure base classes for Inference service.
Defines the abstract BaseEmbedder with integrated Telemetry (Traces, Metrics, Logs).
"""
import io
import logging
import time
from pathlib import Path
from typing import Any, List, Union

import httpx
from embedding.core.telemetry import get_meter, get_tracer
from embedding.schemas import ImageResults, InferenceResults, ValueResult
from PIL import Image

from core.constants import Constants

logger = logging.getLogger(__name__)
tracer = get_tracer(__name__)
meter = get_meter(__name__)

# Define: Base metrics
inference_duration = meter.create_histogram(
    name="inference.duration",
    description="Duration of model inference in milliseconds",
    unit="ms"
)
image_load_duration = meter.create_histogram(
    name="image.load_duration",
    description="Duration of image fetching and loading in milliseconds",
    unit="ms"
)
model_init_duration = meter.create_histogram(
    name="engine.model_init_duration",
    description="Time spent in the model constructor",
    unit="ms"
)


class BaseEmbedder:
    """
    Abstract base class for all embedding models.
    Orchestrates the extraction pipeline with full observability.

    Invariant: self.name is never None; self.dim > 0.
    Subclasses MUST implement _forward().
    """

    def __init__(self, name: str, dim: int):
        """Initializes the embedder with metadata and records init duration metric.

        Args:
            name: Unique model identifier (e.g. 'efficientnet_b0').
            dim: Output embedding dimension (e.g. 1280).
        """
        start_init = time.perf_counter()
        self.name = name
        self.dim = dim
        self._device = None
        # Record: Model initialization duration metric
        duration = (time.perf_counter() - start_init) * 1000
        model_init_duration.record(duration, {"model": self.name})
        logger.info(f"[{self.name}] Initialized (dim={dim}) in {duration:.2f}ms")

    @property
    def device(self):
        """Lazy-resolves the execution device (CUDA if available, otherwise CPU).

        Returns:
            torch.device pointing to 'cuda' or 'cpu'.
        """
        if self._device is None:
            import torch
            # Resolve: Prefer CUDA GPU, fall back to CPU
            self._device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
        return self._device

    def _load_image(
        self, image_input: Union[str, Path, bytes, Image.Image]
    ) -> ValueResult[Image.Image]:
        """Loads an RGB PIL image with OpenTelemetry instrumentation.

        Contract: pre=image_input is str|Path|bytes|Image.Image,
                  post=return.is_success implies return.value is RGB PIL Image,
                  throws=Exception (caught and returned as LoadError)

        Args:
            image_input: URL string, local file path, raw bytes, or PIL Image.

        Returns:
            ValueResult containing the loaded RGB PIL Image, or a LoadError on failure.
        """
        start_time = time.perf_counter()
        with tracer.start_as_current_span(f"{self.name}.load_image") as span:
            try:
                # Check: Direct PIL Image — convert to RGB and return immediately
                if isinstance(image_input, Image.Image):
                    return ImageResults.Success.Ok(image_input.convert("RGB"))

                # Check: URL string or local file path
                if isinstance(image_input, (str, Path)):
                    input_str = str(image_input)
                    span.set_attribute("image.source", input_str)

                    if input_str.startswith(("http://", "https://")):
                        # Call: Download remote image via HTTP with User-Agent header
                        headers = {"User-Agent": "Mozilla/5.0 inference/1.0"}
                        with httpx.Client(timeout=Constants.Constraints.HTTP_TIMEOUT) as client:
                            response = client.get(input_str, headers=headers)
                            response.raise_for_status()
                            img = Image.open(io.BytesIO(response.content)).convert("RGB")
                            return ImageResults.Success.Ok(img)

                    # Load: Local file path as PIL Image
                    img = Image.open(input_str).convert("RGB")
                    return ImageResults.Success.Ok(img)

                # Check: Raw bytes — decode into PIL Image
                if isinstance(image_input, bytes):
                    img = Image.open(io.BytesIO(image_input)).convert("RGB")
                    return ImageResults.Success.Ok(img)

            except Exception as e:
                # Log: Image loading failure with error details
                logger.error(f"[{self.name}] Failed to load image: {e}")
                span.record_exception(e)
                return ValueResult.failure_value(ImageResults.Errors.LoadError(str(e)))
            finally:
                # Record: Image load duration metric
                duration = (time.perf_counter() - start_time) * 1000
                image_load_duration.record(duration, {"model": self.name})

        return ValueResult.failure_value(
            ImageResults.Errors.UnsupportedType(type(image_input).__name__)
        )

    def _normalize(self, features: Any) -> List[float]:
        """Standardizes the output vector using L2 normalization.

        Args:
            features: Raw model output in PyTorch tensor, numpy array, or framework wrapper format.

        Returns:
            A flat list of floats representing the L2-normalized embedding vector.
        """
        import numpy as np

        # Normalize: Extract raw tensor from various framework wrapper formats
        if hasattr(features, "image_embeds"):
            features = features.image_embeds
        elif hasattr(features, "pooler_output"):
            features = features.pooler_output
        elif hasattr(features, "last_hidden_state") and hasattr(features.last_hidden_state, "mean"):
            features = features.last_hidden_state.mean(dim=1)

        # Convert: Framework tensor → numpy array on CPU
        if hasattr(features, "detach"):
            features = features.detach().cpu().numpy()
        elif hasattr(features, "numpy"):
            features = features.numpy()
        else:
            features = np.array(features)

        # Compute: L2 norm then divide, with epsilon to avoid division by zero
        features = features.flatten()
        norm = np.linalg.norm(features)
        return (features / (norm + Constants.Constraints.L2_EPSILON)).tolist()

    def extract(self, image_input: Union[str, bytes]) -> ValueResult[List[float]]:
        """Public interface for embedding extraction with full observability.

        Contract: pre=image_input is str|bytes,
                  post=return.is_success implies return.value is L2-normalized List[float],
                  raises=Exception (caught, logged, returned as InferenceFailed)

        Args:
            image_input: URL string or raw image bytes to embed.

        Returns:
            ValueResult containing the embedding vector, or an error result.
        """
        with tracer.start_as_current_span(f"{self.name}.extract") as span:
            span.set_attribute("model.name", self.name)
            span.set_attribute("model.dimension", self.dim)

            # 1. Load Image
            # Call: Load the image from URL, file path, or bytes
            img_result = self._load_image(image_input)
            if not img_result.is_success:
                return ValueResult.failure_value(img_result.errors)

            # 2. Forward Pass (Inference)
            start_inference = time.perf_counter()
            try:
                with tracer.start_as_current_span(f"{self.name}.forward"):
                    # Call: Subclass-specific forward pass
                    raw_features = self._forward(img_result.value)

                # 3. Normalize
                # Compute: Convert raw outputs to L2-normalized vector
                vector = self._normalize(raw_features)

                # Record Metrics
                duration = (time.perf_counter() - start_inference) * 1000
                inference_duration.record(
                    duration, {"model": self.name, "device": str(self.device)}
                )

                span.set_attribute("inference.duration_ms", duration)
                return InferenceResults.Success.Ok(vector)

            except Exception as e:
                # Log: Inference failure with model name and error
                logger.error(f"[{self.name}] Inference failed: {e}")
                span.record_exception(e)
                return ValueResult.failure_value(
                    InferenceResults.Errors.InferenceFailed(self.name, str(e))
                )

    def _forward(self, image: Image.Image) -> Any:
        """Subclass hook: executes the model-specific forward pass.

        Must be implemented by subclasses. Receives a preprocessed PIL Image
        and must return a tensor or array that _normalize can process.

        Args:
            image: Preprocessed PIL Image ready for inference.

        Raises:
            NotImplementedError: If the subclass does not override this method.
        """
        raise NotImplementedError
