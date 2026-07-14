"""Abstract base class for embedder models implementing the Strategy pattern.

All embedding models in ReSys.Shop inherit from BaseEmbeddingModel.
This enables runtime model swapping without code changes — the backbone
of the comparative evaluation in Chapter 11.
"""

from abc import ABC, abstractmethod
from typing import Any

import numpy as np
from PIL import Image


class BaseEmbeddingModel(ABC):
    """Abstract base for all vision embedding models.

    Attributes:
        model_name: Unique identifier used in the model registry and DB metadata.
        vector_dim: Output dimensionality of the embedding vector.
    """

    model_name: str = ""
    vector_dim: int = 0

    def __init__(self) -> None:
        self._model: Any | None = None
        self._preprocess: Any | None = None
        self._device: str = "cpu"

    @abstractmethod
    def _load(self) -> None:
        """Lazy-load the pretrained model into memory.

        Concrete implementations should:
        1. Detect GPU availability (cuda > mps > cpu).
        2. Download/load weights from the appropriate hub.
        3. Set self._model and self._preprocess.
        4. Move model to self._device.
        """

    def warmup(self) -> None:
        """Force model loading ahead of first request.

        Call this at sidecar startup to avoid cold-start latency on the
        first embedding request.
        """
        if self._model is None:
            self._load()

    def is_loaded(self) -> bool:
        """Return True if the model has been loaded into memory."""
        return self._model is not None

    @abstractmethod
    def encode_image(self, image: Image.Image) -> np.ndarray:
        """Generate an L2-normalized embedding vector from a PIL image.

        Args:
            image: A PIL.Image.Image in RGB mode.

        Returns:
            A 1-D float32 ndarray of length self.vector_dim.
            The vector is L2-normalized so cosine similarity equals dot product.
        """

    def _ensure_loaded(self) -> None:
        if self._model is None:
            self._load()

    def _to_numpy(self, tensor: Any) -> np.ndarray:
        """Detach, move to CPU, and convert a torch tensor to numpy."""
        import torch

        return tensor.detach().cpu().numpy().astype(np.float32)

    def _l2_normalize(self, vector: np.ndarray) -> np.ndarray:
        """L2-normalize a vector for cosine-similarity search."""
        norm = np.linalg.norm(vector)
        if norm == 0:
            return vector
        return vector / norm
