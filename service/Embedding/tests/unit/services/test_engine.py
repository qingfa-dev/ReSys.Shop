"""
Unit tests for InferenceEngine.
Uses a Mocked ModelRegistry so no real models are instantiated.
"""
import pytest
from unittest.mock import MagicMock, patch
from embedding.services.inference_engine import InferenceEngine
from embedding.schemas import ValueResult, Failure, InferenceResults, ImageResults, RegistryResults


# ── Helper ────────────────────────────────────────────────────────────────────

def make_mock_embedder(vector=None):
    """Return a mock embedder whose extract() returns a successful ValueResult."""
    embedder = MagicMock()
    embedder.extract.return_value = InferenceResults.Success.Ok(vector or [0.1, 0.2, 0.3])
    return embedder


# ── get_embedder ──────────────────────────────────────────────────────────────

class TestGetEmbedder:
    def test_unsupported_model_returns_not_found_failure(self):
        engine = InferenceEngine()
        # Mock Registry to return failure
        registry_failure = RegistryResults.Errors.NotRegistered("non_existent_model")
        with patch("src.services.inference_engine.ModelRegistry.get_model_class", return_value=ValueResult.failure_value(registry_failure)):
            result = engine.get_embedder("non_existent_model")
        
        assert result.is_success is False
        assert result.failures[0].code == "Model.NotFound"
        assert result.status_code == 404

    def test_supported_models_are_lazy_loaded(self):
        """Each known model name should trigger instantiation from registry."""
        engine = InferenceEngine()
        mock_instance = MagicMock()
        mock_cls = MagicMock(return_value=mock_instance)

        # Mock Registry to return Success with mock_cls
        registry_success = RegistryResults.Success.Ok(mock_cls)
        with patch("src.services.inference_engine.ModelRegistry.get_model_class", return_value=registry_success):
            result = engine.get_embedder("any_model")

        assert result.is_success is True
        assert result.value is mock_instance
        mock_cls.assert_called_once()

    def test_model_cached_after_first_load(self):
        """The second call to get_embedder must return the cached instance, not reload from registry."""
        engine = InferenceEngine()
        mock_instance = MagicMock()
        mock_cls = MagicMock(return_value=mock_instance)

        registry_success = RegistryResults.Success.Ok(mock_cls)
        with patch("src.services.inference_engine.ModelRegistry.get_model_class", return_value=registry_success) as mock_reg:
            engine.get_embedder("cached_model")
            engine.get_embedder("cached_model")

        # Registry should have been hit only once
        mock_reg.assert_called_once()

    def test_model_load_exception_returns_internal_error_failure(self):
        engine = InferenceEngine()
        # Model class instantiation raises
        mock_cls = MagicMock(side_effect=RuntimeError("CUDA OOM"))
        
        registry_success = RegistryResults.Success.Ok(mock_cls)
        with patch("src.services.inference_engine.ModelRegistry.get_model_class", return_value=registry_success):
            result = engine.get_embedder("broken_model")

        assert result.is_success is False
        assert result.failures[0].code == "Model.LoadError"
        assert result.status_code == 500

    def test_each_engine_instance_has_isolated_cache(self):
        """
        Two separate engines must not share loaded models.
        """
        engine_a = InferenceEngine()
        engine_b = InferenceEngine()

        mock_a = MagicMock()
        mock_b = MagicMock()
        
        mock_cls_a = MagicMock(return_value=mock_a)
        mock_cls_b = MagicMock(return_value=mock_b)

        with patch("src.services.inference_engine.ModelRegistry.get_model_class", side_effect=[
            RegistryResults.Success.Ok(mock_cls_a),
            RegistryResults.Success.Ok(mock_cls_b)
        ]):
            engine_a.get_embedder("model_x")
            result = engine_b.get_embedder("model_x")

        # engine_b should have loaded its own instance
        assert result.value is mock_b
        assert result.value is not mock_a


# ── embed() ───────────────────────────────────────────────────────────────────

class TestEmbed:
    def test_embed_success_propagates_vector(self):
        engine = InferenceEngine()
        expected_vector = [0.1, 0.2, 0.3]
        mock_embedder = make_mock_embedder(expected_vector)

        with patch.object(
            engine, "get_embedder", return_value=InferenceResults.Success.Ok(mock_embedder)
        ):
            result = engine.embed("http://example.com/img.jpg", "efficientnet_b0")

        assert result.is_success is True
        assert result.value == expected_vector

    def test_embed_propagates_get_embedder_failure(self):
        engine = InferenceEngine()
        failure = InferenceResults.Errors.ModelNotFound("bad_model")

        with patch.object(
            engine, "get_embedder", return_value=ValueResult.failure_value(failure)
        ):
            result = engine.embed("http://example.com/img.jpg", "bad_model")

        assert result.is_success is False
        assert result.failures[0].code == "Model.NotFound"

    def test_embed_propagates_extract_failure(self):
        engine = InferenceEngine()
        broken_embedder = MagicMock()
        broken_embedder.extract.return_value = ValueResult.failure_value(
            ImageResults.Errors.LoadError("404 from remote")
        )

        with patch.object(
            engine, "get_embedder", return_value=InferenceResults.Success.Ok(broken_embedder)
        ):
            result = engine.embed("http://broken.invalid/img.jpg", "efficientnet_b0")

        assert result.is_success is False
        assert result.failures[0].code == "Image.LoadError"

    def test_embed_default_model_is_efficientnet(self):
        engine = InferenceEngine()
        mock_embedder = make_mock_embedder()

        captured = {}

        def fake_get_embedder(name):
            captured["name"] = name
            return InferenceResults.Success.Ok(mock_embedder)

        with patch.object(engine, "get_embedder", side_effect=fake_get_embedder):
            engine.embed("http://example.com/img.jpg")  # no model_name

        assert captured["name"] == "efficientnet_b0"
