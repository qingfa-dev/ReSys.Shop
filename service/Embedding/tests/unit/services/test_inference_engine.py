"""
Unit tests for the InferenceEngine using the new Skill Registry.
"""
from unittest.mock import patch

import pytest
from embedding.models import BaseEmbedder, ModelRegistry
from embedding.services.inference_engine import InferenceEngine


@pytest.fixture(autouse=True)
def clear_registry():
    """Ensure the global registry is clean before and after each test."""
    original_models = ModelRegistry._models.copy()
    yield
    ModelRegistry._models = original_models

class MockSkill(BaseEmbedder):
    def __init__(self, *args, **kwargs):
        super().__init__("mock_skill", 128)
    def _forward(self, image):
        return [0.0] * 128

def test_engine_resolves_registered_skill():
    # Setup: Register a mock skill
    ModelRegistry.register("test_skill")(MockSkill)

    engine = InferenceEngine()
    result = engine.get_embedder("test_skill")

    assert result.is_success is True
    assert isinstance(result.value, MockSkill)
    # Check: Caching
    assert engine.get_embedder("test_skill").value is result.value

def test_engine_returns_not_found_for_unsupported():
    engine = InferenceEngine()
    result = engine.get_embedder("ghost_model")
    assert result.is_success is False
    assert result.failures[0].code == "Model.NotFound"

def test_engine_fuzzy_matches_clip():
    # Setup: Ensure clip_vit_b16 is registered
    ModelRegistry.register("clip_vit_b16")(MockSkill)

    engine = InferenceEngine()
    # Try fuzzy match
    result = engine.get_embedder("clip_something_else")
    assert result.is_success is True
    assert isinstance(result.value, MockSkill)

@patch("embedding.services.inference_engine.infer_onnx_dim")
@patch("pathlib.Path.exists")
@patch("embedding.core.config.settings.ONNX_MODEL_DIR", "/models")
def test_engine_resolves_onnx_skill(mock_exists, mock_infer):
    # Setup: Register ONNX skill
    ModelRegistry.register("onnx")(MockSkill)
    mock_exists.return_value = True
    mock_infer.return_value = 512

    engine = InferenceEngine()
    result = engine.get_embedder("onnx/my_model")

    assert result.is_success is True
    # In reality it would be an OnnxEmbedder, but we registered MockSkill as "onnx"
    assert isinstance(result.value, MockSkill)
    mock_infer.assert_called_once()
