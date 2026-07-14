"""
Infrastructure base classes for Inference service.
Defines the abstract BaseEmbedder with integrated Telemetry (Traces, Metrics, Logs).
"""
import logging
import io
import time
import httpx
from typing import Union, List, Any
from pathlib import Path
from PIL import Image

from src.schemas import ValueResult, Failure, ImageResults, InferenceResults
from src.core.telemetry import get_tracer, get_meter

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
    """

    def __init__(self, name: str, dim: int):
        """Initializes the embedder metadata."""
        start_init = time.perf_counter()
        self.name = name
        self.dim = dim
        self._device = None
        # Record Metric
        duration = (time.perf_counter() - start_init) * 1000
        model_init_duration.record(duration, {"model": self.name})
        logger.info(f"[{self.name}] Initialized (dim={dim}) in {duration:.2f}ms")

    @property
    def device(self):
        """Lazy-resolves the execution device (CUDA if available, otherwise CPU)."""
        if self._device is None:
            import torch
            self._device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
        return self._device

    def _load_image(self, image_input: Union[str, Path, bytes, Image.Image]) -> ValueResult[Image.Image]:
        """
        Loads an RGB PIL image with instrumentation.
        """
        start_time = time.perf_counter()
        with tracer.start_as_current_span(f"{self.name}.load_image") as span:
            try:
                if isinstance(image_input, Image.Image):
                    return ImageResults.Success.Ok(image_input.convert("RGB"))

                if isinstance(image_input, (str, Path)):
                    input_str = str(image_input)
                    span.set_attribute("image.source", input_str)
                    
                    if input_str.startswith(("http://", "https://")):
                        headers = {"User-Agent": "Mozilla/5.0 inference/1.0"}
                        with httpx.Client(timeout=10) as client:
                            response = client.get(input_str, headers=headers)
                            response.raise_for_status()
                            img = Image.open(io.BytesIO(response.content)).convert("RGB")
                            return ImageResults.Success.Ok(img)

                    img = Image.open(input_str).convert("RGB")
                    return ImageResults.Success.Ok(img)

                if isinstance(image_input, bytes):
                    img = Image.open(io.BytesIO(image_input)).convert("RGB")
                    return ImageResults.Success.Ok(img)

            except Exception as e:
                logger.error(f"[{self.name}] Failed to load image: {e}")
                span.record_exception(e)
                return ValueResult.failure_value(ImageResults.Errors.LoadError(str(e)))
            finally:
                duration = (time.perf_counter() - start_time) * 1000
                image_load_duration.record(duration, {"model": self.name})

        return ValueResult.failure_value(
            ImageResults.Errors.UnsupportedType(type(image_input).__name__)
        )

    def _normalize(self, features: Any) -> List[float]:
        """Standardizes the output vector using L2 normalization."""
        import numpy as np
        
        # Extract features from various framework formats
        if hasattr(features, "image_embeds"):
            features = features.image_embeds
        elif hasattr(features, "pooler_output"):
            features = features.pooler_output
        elif hasattr(features, "last_hidden_state") and hasattr(features.last_hidden_state, "mean"):
            features = features.last_hidden_state.mean(dim=1)

        if hasattr(features, "detach"):
            features = features.detach().cpu().numpy()
        elif hasattr(features, "numpy"):
            features = features.numpy()
        else:
            features = np.array(features)

        features = features.flatten()
        norm = np.linalg.norm(features)
        return (features / (norm + 1e-9)).tolist()

    def extract(self, image_input: Union[str, bytes]) -> ValueResult[List[float]]:
        """Public interface: orchestrates extraction with traces and metrics."""
        with tracer.start_as_current_span(f"{self.name}.extract") as span:
            span.set_attribute("model.name", self.name)
            span.set_attribute("model.dimension", self.dim)

            # 1. Load Image
            img_result = self._load_image(image_input)
            if not img_result.is_success:
                return ValueResult.failure_value(img_result.failures)

            # 2. Forward Pass (Inference)
            start_inference = time.perf_counter()
            try:
                with tracer.start_as_current_span(f"{self.name}.forward"):
                    raw_features = self._forward(img_result.value)
                
                # 3. Normalize
                vector = self._normalize(raw_features)
                
                # Record Metrics
                duration = (time.perf_counter() - start_inference) * 1000
                inference_duration.record(duration, {"model": self.name, "device": str(self.device)})
                
                span.set_attribute("inference.duration_ms", duration)
                return InferenceResults.Success.Ok(vector)

            except Exception as e:
                logger.error(f"[{self.name}] Inference failed: {e}")
                span.record_exception(e)
                return ValueResult.failure_value(
                    InferenceResults.Errors.InferenceFailed(self.name, str(e))
                )

    def _forward(self, image: Image.Image) -> Any:
        """Must be implemented by subclasses."""
        raise NotImplementedError
