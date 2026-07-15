"""Embedding cache — speed-up layer that avoids re-running inference.

Cache files live in ``data/cache/`` (transient; gitignored).
Durable, human-inspectable outputs go to ``outputs/embeddings/`` via
``storage.py`` instead.

Public API used by ``generator.py``
------------------------------------
- ``exists(model_slug, dataset_name)``  → bool
- ``save(model_slug, dataset_name, ids, embeddings)``  → Path
- ``load(model_slug, dataset_name)``  → (ids: list[str], embeddings: ndarray)
- ``CACHE_DIR``  exposed for the CLI ``cache`` command

Edge cases:
- ``load()`` raises FileNotFoundError when no cache entry exists.
- ``exists()`` returns False for non-existent cache entries; no exception.
- Legacy wrapper functions maintain backward compatibility for existing code.
"""
from __future__ import annotations

from pathlib import Path

import numpy as np

from benchmark.utils.logging import get_logger

logger = get_logger("embeddings.cache")

CACHE_DIR = Path("data/cache")


def _npz_path(model_slug: str, dataset_name: str, cache_dir: Path = CACHE_DIR) -> Path:
    return cache_dir / f"{model_slug}__{dataset_name}.npz"


def exists(model_slug: str, dataset_name: str, cache_dir: Path = CACHE_DIR) -> bool:
    """Return True if a cache entry exists for this model + dataset pair."""
    return _npz_path(model_slug, dataset_name, cache_dir).exists()


def save(
    model_slug: str,
    dataset_name: str,
    ids: list[str],
    embeddings: np.ndarray,
    cache_dir: Path = CACHE_DIR,
) -> Path:
    """Persist embeddings to the cache.

    Args:
        model_slug:   ``model.slug`` — filesystem-safe model name.
        dataset_name: Logical dataset key (e.g. ``"deepfashion"``).
        ids:          Product-ID strings aligned 1-to-1 with embedding rows.
        embeddings:   Float32 array of shape ``(N, D)``.
        cache_dir:    Cache directory (default ``data/cache/``).

    Returns:
        Path to the written ``.npz`` file.
    """
    cache_dir.mkdir(parents=True, exist_ok=True)
    path = _npz_path(model_slug, dataset_name, cache_dir)
    np.savez_compressed(path, embeddings=embeddings, ids=np.array(ids, dtype=str))
    logger.debug("Cache write: %s  (%d vectors, dim=%d)", path.name, len(ids), embeddings.shape[-1])
    return path


def load(
    model_slug: str,
    dataset_name: str,
    cache_dir: Path = CACHE_DIR,
) -> tuple[list[str], np.ndarray]:
    """Load cached embeddings.

    Args:
        model_slug:   ``model.slug``.
        dataset_name: Logical dataset key.
        cache_dir:    Cache directory.

    Returns:
        Tuple ``(ids, embeddings)`` where ids is a list of product-ID strings
        and embeddings is a float32 array of shape ``(N, D)``.

    Raises:
        FileNotFoundError: If no cache entry exists.
    """
    path = _npz_path(model_slug, dataset_name, cache_dir)
    if not path.exists():
        raise FileNotFoundError(
            f"No cache entry for model='{model_slug}' dataset='{dataset_name}' at {path}"
        )
    data = np.load(path, allow_pickle=False)
    ids: list[str] = data["ids"].tolist()
    embeddings: np.ndarray = data["embeddings"]
    logger.debug("Cache read : %s  (%d vectors)", path.name, len(ids))
    return ids, embeddings


# ── legacy aliases kept for backward-compat with existing code ────────────────
def save_embeddings(
    embeddings: np.ndarray,
    samples_meta: list[dict],
    model_id: str,
    dataset: str,
    cache_dir: Path = CACHE_DIR,
) -> None:
    """Legacy wrapper — use ``save()`` in new code."""
    ids = [m.get("product_id", str(i)) for i, m in enumerate(samples_meta)]
    save(model_id, dataset, ids, embeddings, cache_dir=cache_dir)


def load_embeddings(
    model_id: str,
    dataset: str,
    cache_dir: Path = CACHE_DIR,
) -> tuple[np.ndarray, list[dict]] | None:
    """Legacy wrapper — use ``load()`` in new code. Returns None if not found."""
    if not exists(model_id, dataset, cache_dir):
        return None
    ids, embeddings = load(model_id, dataset, cache_dir)
    return embeddings, [{"product_id": pid} for pid in ids]


def is_cached(model_id: str, dataset: str, cache_dir: Path = CACHE_DIR) -> bool:
    """Legacy alias for ``exists()``."""
    return exists(model_id, dataset, cache_dir)
