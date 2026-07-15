"""Verify every .NET AIModels constant value maps to a Python registry key."""

import pytest
from embedding.services.inference_engine import InferenceEngine


AIMODEL_VALUES = [
    "openclip-vit-b-32",
    "openclip-vit-l-14",
    "siglip-vit-b-16",
    "fashion_clip",
    "deepfashion-embed-v2",
    "dinov2_vits14",
    "dinov2-vit-base",
    "ibot-vit-base",
    "swin-base",
    "convnext-v2-tiny",
    "efficientnet_b0",
]

DEFERRED_MODELS = [
    "openclip-vit-l-14",
    "siglip-vit-b-16",
    "deepfashion-embed-v2",
    "dinov2-vit-base",
    "ibot-vit-base",
    "swin-base",
    "convnext-v2-tiny",
]

EXPECTED_EXISTING = list(set(AIMODEL_VALUES) - set(DEFERRED_MODELS))


class TestModelRegistryCompleteness:
    @pytest.mark.parametrize("model_key", sorted(EXPECTED_EXISTING))
    def test_model_is_registered(self, model_key: str):
        engine = InferenceEngine()
        result = engine.get_embedder(model_key)
        assert result.is_success is True, (
            f"Model key '{model_key}' must be registered. "
            f"Error: {result.errors}"
        )

    @pytest.mark.parametrize("model_key", sorted(DEFERRED_MODELS))
    def test_deferred_model_returns_not_found_not_fallback(self, model_key: str):
        engine = InferenceEngine()
        result = engine.get_embedder(model_key)
        assert result.is_success is False, (
            f"Deferred model '{model_key}' must return NotFound "
            f"(not silently fall back to another model)"
        )
        assert result.errors[0].code == "Model.NotFound"
