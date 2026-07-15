"""Model registry — single source of truth for available adapters.

All runnable models are registered here.
The CLI and runner resolve model names through this dict.

To add a new model:
  1. Implement EmbeddingModel in models/<your_model>.py
  2. Import it below
  3. Add one entry to MODELS

Nothing else needs to change.
"""
from __future__ import annotations

from benchmark.models.base import EmbeddingModel
from benchmark.models.clip_b32 import ClipB32Model
from benchmark.models.clip_l14 import ClipL14Model
from benchmark.models.eva_clip import EvaClipModel
from benchmark.models.fashion_clip import FashionClipModel
from benchmark.models.siglip import SigLipModel

# ------------------------------------------------------------------ #
# Registry                                                             #
# ------------------------------------------------------------------ #

# Context: Backward-compatible simple dict registry (not used by new code;
#          use __init__.py's REGISTRY instead)
MODELS: dict[str, EmbeddingModel] = {
    "fashion-clip": FashionClipModel(),
    "clip-b32":     ClipB32Model(),
    "clip-l14":     ClipL14Model(),
    "siglip":       SigLipModel(),
    "eva-clip":     EvaClipModel(),
}

ALL_MODEL_KEYS: list[str] = list(MODELS.keys())


def get_model(key: str) -> EmbeddingModel:
    """Retrieve a model by its registry key.

    Args:
        key: Model registry key (e.g. "fashion-clip").

    Returns:
        Model instance.

    Raises:
        KeyError: If the key is not in the registry.
    """
    if key not in MODELS:
        available = ", ".join(ALL_MODEL_KEYS)
        raise KeyError(f"Unknown model {key!r}. Available: {available}")
    return MODELS[key]


def get_models(keys: list[str] | None = None) -> list[EmbeddingModel]:
    """Return a list of models.

    Args:
        keys: List of model keys, or ``None`` / ``["all"]`` for all models.

    Returns:
        List of ``EmbeddingModel`` instances.
    """
    if keys is None or keys == ["all"]:
        return list(MODELS.values())
    return [get_model(k) for k in keys]
