"""
Core service for managing machine learning model instances and executing inference.
Implements a singleton factory pattern with deep instrumentation (Traces, Metrics, Logs).
"""
import logging
import time
from pathlib import Path
from typing import Dict, List

from embedding.core.config import settings
from embedding.core.telemetry import get_meter, get_tracer
from embedding.models import BaseEmbedder, ModelRegistry
from embedding.models.onnx.utils import infer_onnx_dim
from embedding.schemas import InferenceResults, ValueResult

logger = logging.getLogger(__name__)
tracer = get_tracer(__name__)
meter = get_meter(__name__)

# Define: Engine metrics
model_cache_hits = meter.create_counter(
    name="engine.cache_hits",
    description="Number of times a model was retrieved from memory cache"
)
model_cache_misses = meter.create_counter(
    name="engine.cache_misses",
    description="Number of times a model had to be loaded from disk/registry"
)
model_load_errors = meter.create_counter(
    name="engine.load_errors",
    description="Number of model loading failures"
)
model_load_duration = meter.create_histogram(
    name="engine.model_load_duration",
    description="Time spent loading and initializing a model instance",
    unit="ms"
)
model_init_duration = meter.create_histogram(
    name="engine.model_init_duration",
    description="Time spent in the model constructor (PyTorch weights or ONNX session)",
    unit="ms"
)


class InferenceEngine:
    """
    Manages the lifecycle of embedding models using a dynamic skill registry.
    """

    def __init__(self):
        """Initializes an empty cache for model instances."""
        self._models: Dict[str, BaseEmbedder] = {}

    def get_embedder(self, model_name: str) -> ValueResult[BaseEmbedder]:
        """
        Retrieves a model instance from cache or initializes a new one.
        """
        with tracer.start_as_current_span("engine.get_embedder") as span:
            span.set_attribute("model.requested", model_name)

            # 1. Cache: Return existing instance if already loaded
            if model_name in self._models:
                logger.debug("Cache hit for model: %s", model_name)
                model_cache_hits.add(1, {"model": model_name})
                span.set_attribute("cache.hit", True)
                return InferenceResults.Success.Ok(self._models[model_name])

            # 2. Loading logic
            logger.info("Cache miss. Loading model skill: %s", model_name)
            model_cache_misses.add(1, {"model": model_name})
            span.set_attribute("cache.hit", False)

            start_load = time.perf_counter()
            try:
                if model_name.startswith("onnx/"):
                    instance_result = self._load_onnx_model(model_name, span)
                else:
                    instance_result = self._load_torch_skill(model_name, span)

                if not instance_result.is_success:
                    return instance_result

                instance = instance_result.value
                if instance:
                    # Record Metric
                    duration = (time.perf_counter() - start_load) * 1000
                    model_load_duration.record(duration, {"model": model_name})
                    span.set_attribute("load.duration_ms", duration)

                    self._models[model_name] = instance
                    return InferenceResults.Success.Ok(instance)

                return ValueResult.failure_value(
                    InferenceResults.Errors.ModelNotFound(model_name)
                )

            except FileNotFoundError as e:
                logger.warning("Model file not found '%s': %s", model_name, e)
                return ValueResult.failure_value(
                    InferenceResults.Errors.OnnxNotFound(str(e))
                )
            except Exception as e:
                logger.error("Critical error loading model '%s': %s", model_name, e, exc_info=True)
                model_load_errors.add(1, {"model": model_name, "error": type(e).__name__})
                span.record_exception(e)
                return ValueResult.failure_value(
                    InferenceResults.Errors.LoadError(model_name, str(e))
                )

    def _load_onnx_model(self, model_name: str, span) -> ValueResult[BaseEmbedder]:
        """Helper to resolve and load an ONNX model."""
        model_id = model_name.removeprefix("onnx/")
        # Reverted to singular ONNX_MODEL_DIR
        model_path = Path(settings.ONNX_MODEL_DIR) / model_id / "model.onnx"

        if not model_path.exists():
            legacy_path = Path(settings.ONNX_MODEL_DIR) / f"{model_id}.onnx"
            if legacy_path.exists():
                model_path = legacy_path
            else:
                return ValueResult.failure_value(
                    InferenceResults.Errors.OnnxNotFound(str(model_path))
                )

        span.set_attribute("onnx.path", str(model_path))

        registry_result = ModelRegistry.get_model_class("onnx")
        if not registry_result.is_success:
            return registry_result

        onnx_cls = registry_result.value
        dim = infer_onnx_dim(str(model_path))
        return InferenceResults.Success.Ok(onnx_cls(str(model_path), dim=dim))

    def _load_torch_skill(self, model_name: str, span) -> ValueResult[BaseEmbedder]:
        """Helper to resolve and load a Torch skill from registry."""
        registry_result = ModelRegistry.get_model_class(model_name)

        if not registry_result.is_success and "clip" in model_name and "fashion" not in model_name:
            registry_result = ModelRegistry.get_model_class("clip_vit_b16")

        if not registry_result.is_success:
            return ValueResult.failure_value(InferenceResults.Errors.ModelNotFound(model_name))

        model_cls = registry_result.value
        return InferenceResults.Success.Ok(model_cls())

    def embed(
        self, image_url: str, model_name: str = "efficientnet_b0"
    ) -> ValueResult[List[float]]:
        """
        High-level interface to extract a normalized embedding with tracing.
        """
        with tracer.start_as_current_span("engine.embed") as span:
            span.set_attribute("image.url", image_url)
            span.set_attribute("model.requested", model_name)

            embedder_result = self.get_embedder(model_name)
            if not embedder_result.is_success:
                return embedder_result

            return embedder_result.value.extract(image_url)

    def embed_bytes(
        self, image_bytes: bytes, model_name: str = "efficientnet_b0"
    ) -> ValueResult[List[float]]:
        """
        High-level interface to extract a normalized embedding from raw image bytes.
        """
        with tracer.start_as_current_span("engine.embed_bytes") as span:
            span.set_attribute("image.source", "bytes")
            span.set_attribute("image.size_bytes", len(image_bytes))
            span.set_attribute("model.requested", model_name)

            embedder_result = self.get_embedder(model_name)
            if not embedder_result.is_success:
                return embedder_result

            return embedder_result.value.extract(image_bytes)

