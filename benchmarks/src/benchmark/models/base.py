"""Abstract base class that every embedding model adapter must implement.

Adding a new model requires exactly three steps:
  1. Create ``src/benchmark/models/<name>.py``
  2. Subclass ``EmbeddingModel``; implement ``name``, ``embedding_dim``,
     ``load``, and ``embed``
  3. Register an instance in ``REGISTRY`` inside ``__init__.py``

The benchmark pipeline (generator -> retriever -> evaluator -> reporter)
never changes when new models are added.
"""
from __future__ import annotations

from abc import ABC, abstractmethod

import numpy as np
from PIL import Image


# Invariant: embed(img) always returns L2-normalised float32 vector
#            of length embedding_dim for every subclass
class EmbeddingModel(ABC):
    """Strategy interface for image embedding models.

    All adapters must be safe to call from a single thread but need not be
    thread-safe themselves (the benchmark runner is sequential per model).

    Invariant: ``embed(img)`` always returns an L2-normalised float32 vector
    of length ``embedding_dim``.
    """

    _loaded: bool = False

    # ── required interface ────────────────────────────────────────────────

    @property
    @abstractmethod
    def name(self) -> str:
        """Human-readable model identifier used in reports and filenames."""

    @property
    @abstractmethod
    def embedding_dim(self) -> int:
        """Dimension of the output embedding vector."""

    # Contract: pre=model not yet loaded, post=model loaded on target device,
    #           throws=RuntimeError if device unavailable
    # AgentHint: Subclasses must implement pre/post conditions to tolerate
    #            missing GPU; do NOT throw unconditionally
    @abstractmethod
    def load(self) -> None:
        """Download weights and initialise the model on the target device.

        Called once by ``ensure_loaded()``. Must be idempotent.
        """

    # Contract: pre=image is RGB PIL, post=return float32 shape (D,) L2-norm=1
    @abstractmethod
    def embed(self, image: Image.Image) -> np.ndarray:
        """Return a normalised float32 embedding for a single PIL image.

        Args:
            image: An RGB PIL image (any size - the adapter handles resizing).

        Returns:
            1-D float32 numpy array of shape ``(embedding_dim,)``, L2-normalised.
        """

    # ── optional override ─────────────────────────────────────────────────

    # Contract: pre=len(images) > 0, post=return float32 shape (N, D) L2-normalised
    def embed_batch(self, images: list[Image.Image]) -> np.ndarray:
        """Embed a batch of images.

        Default: calls ``embed`` in a loop — override for GPU-efficient batching.

        Args:
            images: List of RGB PIL images.

        Returns:
            2-D float32 numpy array of shape ``(len(images), embedding_dim)``.
        """
        return np.stack([self.embed(img) for img in images])

    # ── lifecycle ─────────────────────────────────────────────────────────

    def ensure_loaded(self) -> None:
        """Call ``load()`` at most once."""
        if not self._loaded:
            self.load()
            self._loaded = True

    # ── slug used for cache filenames ─────────────────────────────────────

    @property
    def slug(self) -> str:
        """Filesystem-safe version of ``name`` (lowercase, hyphens)."""
        return self.name.lower().replace(" ", "-").replace("/", "-")

    def __repr__(self) -> str:
        return f"{self.__class__.__name__}(name={self.name!r}, dim={self.embedding_dim})"
