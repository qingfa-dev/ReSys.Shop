"""Tests for openclip-vit-b-32 model registration."""
from unittest.mock import patch

import pytest
from embedding.models import ModelRegistry
from embedding.services.inference_engine import InferenceEngine


class TestOpenClipB32Registration:
    def test_openclip_vit_b32_is_registered(self):
        engine = InferenceEngine()
        result = engine.get_embedder("openclip-vit-b-32")
        assert result.is_success is True, f"Expected success, got: {result.errors}"
        assert result.value.name == "openclip-vit-b-32"

    def test_openclip_vit_b32_produces_512_dim_embedding(self):
        engine = InferenceEngine()
        result = engine.get_embedder("openclip-vit-b-32")
        assert result.is_success is True
        embedder = result.value
        assert embedder.dimension == 512
