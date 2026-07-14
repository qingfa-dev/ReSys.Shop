"""Embedding storage helpers — bulk save/load for full experiment outputs.

Differs from ``cache.py`` (which is a transient speed-up cache keyed by model
slug) in that ``storage.py`` is for durable, human-inspectable outputs written
to ``outputs/embeddings/`` as part of the published benchmark record.
"""
from __future__ import annotations

from pathlib import Path

import numpy as np

from benchmark.utils.logging import get_logger

logger = get_logger("embeddings.storage")

OUTPUT_DIR = Path("outputs/embeddings")


def save_embeddings(
    embeddings: np.ndarray,
    ids: list[str],
    model_slug: str,
    output_dir: Path = OUTPUT_DIR,
) -> Path:
    """Persist embeddings to ``outputs/embeddings/<model_slug>.npz``.

    Args:
        embeddings: Float32 array of shape ``(N, D)``.
        ids:        Product-ID strings aligned to embedding rows.
        model_slug: Filesystem-safe model name (``model.slug``).
        output_dir: Destination directory.

    Returns:
        Path to the written ``.npz`` file.
    """
    output_dir.mkdir(parents=True, exist_ok=True)
    path = output_dir / f"{model_slug}.npz"
    np.savez_compressed(path, embeddings=embeddings, ids=np.array(ids, dtype=str))
    logger.info("Saved %d embeddings (%s) → %s", len(ids), model_slug, path)
    return path


def load_embeddings(model_slug: str, output_dir: Path = OUTPUT_DIR) -> tuple[np.ndarray, list[str]]:
    """Load embeddings from ``outputs/embeddings/<model_slug>.npz``.

    Returns:
        Tuple of ``(embeddings, ids)`` with shapes ``(N, D)`` and ``(N,)``.

    Raises:
        FileNotFoundError: If the file does not exist.
    """
    path = output_dir / f"{model_slug}.npz"
    if not path.exists():
        raise FileNotFoundError(f"No stored embeddings for '{model_slug}' at {path}")
    data = np.load(path, allow_pickle=False)
    return data["embeddings"], data["ids"].tolist()


def list_stored(output_dir: Path = OUTPUT_DIR) -> list[str]:
    """Return model slugs for which stored embeddings exist."""
    if not output_dir.exists():
        return []
    return sorted(p.stem for p in output_dir.glob("*.npz"))
