"""
Integration tests — end-to-end inference with real model weights.

These tests download weights from HuggingFace / PyTorch Hub on the first run
and are therefore marked @pytest.mark.slow. Run them with:

    pytest -m slow
    pytest -m "slow and integration"

Skip them in CI with:

    pytest -m "not slow"

Each test validates the full request → FastAPI → InferenceEngine → Embedder
→ normalised vector pipeline, including:
  - HTTP response shape (Result pattern)
  - Vector dimensionality
  - L2 normalisation
  - Determinism (same image → same vector)
  - Cross-image discriminability (different images → different vectors)
  - Response metadata fields
"""
import math
from pathlib import Path

import pytest
from embedding.core.config import settings

from tests.conftest import TEST_IMAGE_URL, TEST_IMAGE_URL_2

pytestmark = [pytest.mark.integration]

EMBED_URL = "/embeddings"


def _onnx_model_exists(model_id: str) -> bool:
    onnx_file = Path(settings.ONNX_MODEL_DIR) / model_id / "model.onnx"
    return onnx_file.exists()


# ── Helpers ───────────────────────────────────────────────────────────────────

def cosine_similarity(a: list[float], b: list[float]) -> float:
    dot = sum(x * y for x, y in zip(a, b))
    norm_a = math.sqrt(sum(x * x for x in a))
    norm_b = math.sqrt(sum(x * x for x in b))
    return dot / (norm_a * norm_b + 1e-9)


def assert_valid_embedding(data: dict, expected_dim: int):
    """Assert the full EmbeddingResponse contract."""
    assert data["isSuccess"] is True
    assert data["statusCode"] == 200
    assert data["errors"] == []

    val = data["value"]
    assert val["dimension"] == expected_dim, (
        f"Expected dimension {expected_dim}, got {val['dimension']}"
    )
    assert len(val["vector"]) == expected_dim
    assert "processing_time_ms" in val["metadata"]
    assert val["metadata"]["processing_time_ms"] >= 0

    # All vector elements must be finite floats
    assert all(math.isfinite(v) for v in val["vector"]), "Vector contains NaN or Inf"

    # L2 norm must be ~1.0 (verifies normalisation)
    l2 = sum(x * x for x in val["vector"])
    assert l2 == pytest.approx(1.0, rel=1e-3), f"L2 norm was {l2}, expected ~1.0"


# ── Parametrised: all supported models ────────────────────────────────────────

_ALL_MODELS = [
    ("efficientnet_b0", 1280),
    ("fashion_clip", 512),
    ("clip_vit_b16", 512),
    ("dinov2_vits14", 384),
]

# Add ONNX models only if they exist on disk
_ONNX_MODELS = [
    ("onnx/efficientnet_b0", 1280),
    ("onnx/fashion_clip", 768),
    ("onnx/clip_vit_b16", 768),
    ("onnx/dinov2_vits14", 384),
]
for mid, dim in _ONNX_MODELS:
    onnx_name = mid.replace("onnx/", "")
    if _onnx_model_exists(onnx_name):
        _ALL_MODELS.append((mid, dim))

_ONNX_COUNT = sum(1 for m, _ in _ALL_MODELS if m.startswith("onnx/"))


@pytest.mark.parametrize("model_name, expected_dim", _ALL_MODELS)

def test_all_models_produce_valid_embeddings(authed_client, model_name, expected_dim):
    """
    Full pipeline test for every supported model.
    Verifies: HTTP 200, result shape, vector dimension, L2 normalisation.
    """
    response = authed_client.post(
        EMBED_URL,
        json={"image_url": TEST_IMAGE_URL, "model": model_name},
    )
    assert response.status_code == 200, (
        f"[{model_name}] Expected 200, got {response.status_code}: {response.text}"
    )
    assert_valid_embedding(response.json(), expected_dim)


# ── Determinism ───────────────────────────────────────────────────────────────

@pytest.mark.parametrize("model_name", [
    "efficientnet_b0",
    "fashion_clip",
])
def test_embedding_is_deterministic(authed_client, model_name):
    """
    The same image sent twice must produce the exact same vector.
    Catches any accidental stochasticity (e.g. dropout left in train mode).
    """
    payload = {"image_url": TEST_IMAGE_URL, "model": model_name}
    r1 = authed_client.post(EMBED_URL, json=payload).json()["value"]["vector"]
    r2 = authed_client.post(EMBED_URL, json=payload).json()["value"]["vector"]
    assert r1 == pytest.approx(r2, rel=1e-5), (
        f"[{model_name}] Vector not deterministic across two identical requests"
    )


# ── Discriminability ──────────────────────────────────────────────────────────

@pytest.mark.parametrize("model_name", [
    "efficientnet_b0",
    "fashion_clip",
])
def test_different_images_produce_different_vectors(authed_client, model_name):
    """
    Two distinct images must not produce identical vectors.
    Also verifies cosine similarity is < 1.0 (not a collapsed embedding space).
    """
    r1 = authed_client.post(
        EMBED_URL, json={"image_url": TEST_IMAGE_URL, "model": model_name}
    ).json()["value"]["vector"]

    r2 = authed_client.post(
        EMBED_URL, json={"image_url": TEST_IMAGE_URL_2, "model": model_name}
    ).json()["value"]["vector"]

    assert r1 != r2, f"[{model_name}] Two different images produced identical vectors"
    sim = cosine_similarity(r1, r2)
    assert sim < 1.0 - 1e-4, f"[{model_name}] Cosine similarity is {sim}, expected < 1.0"


# ── Registry Discovery ────────────────────────────────────────────────────────

def test_list_models_returns_all_available_options(authed_client):
    """
    Verifies that /models discovers both registered skills
    and ONNX models on disk.
    """
    response = authed_client.get("/models")
    assert response.status_code == 200

    data = response.json()
    assert data["isSuccess"] is True

    models = data["value"]
    model_ids = [m["id"] for m in models]

    # Check: Basic skills are present
    assert "efficientnet_b0" in model_ids
    assert "clip_vit_b16" in model_ids

    # Check: ONNX models are discovered if they exist on disk
    onnx_models = [m for m in models if m["is_onnx"]]
    if _ONNX_COUNT > 0:
        assert len(onnx_models) > 0, "No ONNX models were discovered on disk but some were expected"
        assert any(m["id"] == "onnx/efficientnet_b0" for m in onnx_models)

    # Check: All models have valid dimensions
    for m in models:
        assert m["dimension"] > 0
        assert m["name"] is not None
