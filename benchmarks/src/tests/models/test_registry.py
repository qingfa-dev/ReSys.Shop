"""Tests for the lazy model registry.

Registry tests that require torch/transformers/open_clip are marked with
``pytest.mark.integration`` and skipped automatically when those packages
are absent (normal in CI without GPU or heavy ML deps).

Tests that only inspect registry metadata (key names, EmbeddingModel
subclass relationship, name/dim/slug properties) run without any ML deps.
"""
from __future__ import annotations

import pytest

from benchmark.models.base import EmbeddingModel

# ── pure metadata tests (no ML deps) ─────────────────────────────────────────
# We inspect the registry lazily — only checking what's available


_EXPECTED_KEYS = {
    "fashion_clip", "clip_b32", "clip_l14", "clip_vit_b16",
    "siglip", "eva_clip", "efficientnet_b0", "convnext_tiny", "dinov2_vits14",
    "resnet50", "clip_generic",
}

# Try to build the registry; skip individual tests gracefully if deps missing
def _try_build_registry():
    try:
        from benchmark.models import REGISTRY
        _ = list(REGISTRY.keys())   # trigger lazy build
        return REGISTRY
    except ImportError:
        return None


@pytest.fixture(scope="module")
def registry():
    """Return the populated registry or skip if ML deps are missing."""
    from benchmark.models import REGISTRY
    try:
        _ = list(REGISTRY.keys())
    except ImportError as exc:
        pytest.skip(f"ML dependencies not installed: {exc}")
    return REGISTRY


def test_registry_contains_expected_keys(registry) -> None:
    assert set(registry.keys()) == _EXPECTED_KEYS


def test_registry_len(registry) -> None:
    assert len(registry) == 11


def test_all_values_are_embedding_models(registry) -> None:
    for key, model in registry.items():
        assert isinstance(model, EmbeddingModel), (
            f"REGISTRY['{key}'] is {type(model)}, expected EmbeddingModel"
        )


def test_all_models_have_non_empty_name(registry) -> None:
    for key, model in registry.items():
        assert isinstance(model.name, str) and model.name, f"{key}: name is empty"


def test_all_models_have_positive_embedding_dim(registry) -> None:
    for key, model in registry.items():
        assert isinstance(model.embedding_dim, int) and model.embedding_dim > 0, (
            f"{key}: embedding_dim={model.embedding_dim}"
        )


def test_all_models_have_filesystem_safe_slug(registry) -> None:
    for key, model in registry.items():
        slug = model.slug
        assert " " not in slug, f"{key}: slug '{slug}' contains spaces"
        assert "/" not in slug, f"{key}: slug '{slug}' contains '/'"
        assert len(slug) > 0, f"{key}: slug is empty"


def test_registry_getitem_unknown_key_raises(registry) -> None:
    with pytest.raises(KeyError):
        _ = registry["does-not-exist"]


def test_registry_iter_yields_all_keys(registry) -> None:
    keys = list(registry)
    assert len(keys) == 11


def test_model_repr_contains_name_and_dim(registry) -> None:
    for model in registry.values():
        r = repr(model)
        assert model.name in r
        assert str(model.embedding_dim) in r


# ── pure-Python tests that don't touch the registry at all ───────────────────

def test_embedding_model_is_abstract() -> None:
    """EmbeddingModel cannot be instantiated directly."""
    import inspect
    assert inspect.isabstract(EmbeddingModel)


def test_dummy_subclass_satisfies_interface() -> None:
    """A minimal concrete subclass passes the interface contract."""
    import numpy as np
    from PIL import Image

    class _Dummy(EmbeddingModel):
        @property
        def name(self) -> str:
            return "Dummy"
        @property
        def embedding_dim(self) -> int:
            return 8
        def load(self) -> None:
            pass
        def embed(self, image):
            return np.zeros(8, dtype=np.float32)

    m = _Dummy()
    assert m.slug == "dummy"
    assert "Dummy" in repr(m)
    img = Image.new("RGB", (32, 32))
    vec = m.embed(img)
    assert vec.shape == (8,)
    batch = m.embed_batch([img, img])
    assert batch.shape == (2, 8)
