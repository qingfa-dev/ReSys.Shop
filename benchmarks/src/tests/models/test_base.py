"""Tests for the EmbeddingModel interface contract."""

from __future__ import annotations

import numpy as np
import pytest
from PIL import Image

from benchmark.models.base import EmbeddingModel


class DummyModel(EmbeddingModel):
    """Minimal concrete implementation for testing the base interface."""

    @property
    def name(self) -> str:
        return "Dummy"

    @property
    def embedding_dim(self) -> int:
        return 4

    def load(self) -> None:
        pass

    def embed(self, image: Image.Image) -> np.ndarray:
        return np.array([0.5, 0.5, 0.5, 0.5], dtype=np.float32)


@pytest.fixture
def dummy_model() -> DummyModel:
    m = DummyModel()
    m.load()
    return m


@pytest.fixture
def sample_image() -> Image.Image:
    return Image.new("RGB", (224, 224), color=(128, 64, 32))


def test_embed_returns_correct_shape(dummy_model: DummyModel, sample_image: Image.Image) -> None:
    vec = dummy_model.embed(sample_image)
    assert vec.shape == (4,)


def test_embed_returns_float32(dummy_model: DummyModel, sample_image: Image.Image) -> None:
    vec = dummy_model.embed(sample_image)
    assert vec.dtype == np.float32


def test_embed_batch_default_implementation(dummy_model: DummyModel) -> None:
    images = [Image.new("RGB", (224, 224)) for _ in range(3)]
    batch = dummy_model.embed_batch(images)
    assert batch.shape == (3, 4)


def test_repr_contains_name(dummy_model: DummyModel) -> None:
    assert "Dummy" in repr(dummy_model)
