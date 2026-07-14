"""Tests for the model registry and base class contract."""
from __future__ import annotations

import numpy as np
import pytest

from benchmark.models import REGISTRY, EmbeddingModel
from benchmark.models.base import EmbeddingModel


def test_registry_not_empty():
    assert len(REGISTRY) > 0


def test_registry_keys():
    expected = {"fashion-clip", "clip-b32", "clip-l14", "clip-vit-b16",
                "siglip", "eva-clip", "efficientnet-b0", "convnext-tiny", "dinov2-vits14",
                "resnet-50", "clip-generic"}
    assert set(REGISTRY.keys()) == expected


def test_all_models_are_embedding_model_subclasses():
    for key, model in REGISTRY.items():
        assert isinstance(model, EmbeddingModel), f"{key} is not an EmbeddingModel"


def test_all_models_have_name():
    for key, model in REGISTRY.items():
        assert isinstance(model.name, str) and model.name, f"{key} has empty name"


def test_all_models_have_embedding_dim():
    for key, model in REGISTRY.items():
        assert isinstance(model.embedding_dim, int) and model.embedding_dim > 0, \
            f"{key} has invalid embedding_dim"


def test_all_models_have_slug():
    for key, model in REGISTRY.items():
        slug = model.slug
        assert isinstance(slug, str) and len(slug) > 0
        # slug must be filesystem-safe
        assert " " not in slug, f"slug '{slug}' for {key} contains spaces"


def test_slug_is_deterministic():
    for model in REGISTRY.values():
        assert model.slug == model.slug


def test_model_repr():
    for model in REGISTRY.values():
        r = repr(model)
        assert model.name in r
        assert str(model.embedding_dim) in r
