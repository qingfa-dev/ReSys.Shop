"""
Unit tests for the ModelRegistry.
"""
import pytest
from src.models.registry import ModelRegistry
from src.models.base import BaseEmbedder

@pytest.fixture(autouse=True)
def clear_registry():
    """Ensure the global registry is clean before and after each test."""
    original_models = ModelRegistry._models.copy()
    yield
    ModelRegistry._models = original_models

class FakeModel(BaseEmbedder):
    def _forward(self, image):
        return [0.1, 0.2]

def test_registry_stores_and_retrieves_class():
    # Register
    ModelRegistry.register("fake")(FakeModel)
    
    # Retrieve
    result = ModelRegistry.get_model_class("fake")
    assert result.is_success is True
    assert result.value == FakeModel

def test_registry_returns_failure_for_unknown():
    result = ModelRegistry.get_model_class("unknown_skill_99")
    assert result.is_success is False
    assert result.failures[0].code == "Registry.Error"

def test_list_models_includes_registered():
    ModelRegistry.register("list_test")(FakeModel)
    models = ModelRegistry.list_models()
    assert "list_test" in models

def test_decorator_syntax():
    @ModelRegistry.register("decorator_test")
    class DecModel(BaseEmbedder):
        def _forward(self, image): pass
        
    result = ModelRegistry.get_model_class("decorator_test")
    assert result.is_success is True
    assert result.value == DecModel
