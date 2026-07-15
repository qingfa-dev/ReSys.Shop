"""FAISS retrieval backend (optional, CPU or GPU).

Use this when the gallery is large (>100 k items) and cosine.py becomes slow.
FAISS IVFFlat gives sub-linear search time with minimal accuracy loss.

Edge cases:
- Falls back to exact FlatIP when the gallery is too small for IVFFlat.
- Requires float32 input; raises a runtime error if build() is not called
  before querying.
- ImportError is raised with a clear install instruction when faiss is
  not available.

Install: ``pip install faiss-cpu``  (or ``faiss-gpu`` for CUDA)
"""
from __future__ import annotations

import numpy as np

from benchmark.utils.logging import get_logger

logger = get_logger("retrieval.faiss")


class FaissRetriever:
    """Approximate nearest-neighbour retrieval using FAISS IVFFlat.

    Args:
        dim:         Embedding dimension (must match the model).
        n_lists:     Number of Voronoi cells for IVFFlat (default 100).
        n_probe:     Number of cells to search at query time (higher = slower
                     but more accurate; default 10).
    """

    def __init__(self, dim: int, n_lists: int = 100, n_probe: int = 10) -> None:
        self.dim = dim
        self.n_lists = n_lists
        self.n_probe = n_probe
        self._index = None

    def build(self, gallery_embeddings: np.ndarray) -> None:
        """Build the FAISS index from gallery embeddings.

        Args:
            gallery_embeddings: Float32 array of shape ``(N, D)``, L2-normalised.
        """
        try:
            import faiss
        except ImportError as exc:
            raise ImportError(
                "FAISS backend requires 'faiss-cpu'. "
                "Install with: pip install faiss-cpu"
            ) from exc

        assert gallery_embeddings.dtype == np.float32, "FAISS requires float32"
        n = len(gallery_embeddings)

        # Fallback: IVFFlat requires at least 39 * n_lists vectors; otherwise use FlatIP
        if n < self.n_lists * 39:
            logger.warning(
                "Gallery too small for IVFFlat (n=%d, lists=%d) — using FlatIP",
                n, self.n_lists,
            )
            self._index = faiss.IndexFlatIP(self.dim)
        else:
            quantiser = faiss.IndexFlatIP(self.dim)
            self._index = faiss.IndexIVFFlat(quantiser, self.dim, self.n_lists, faiss.METRIC_INNER_PRODUCT)
            self._index.train(gallery_embeddings)

        self._index.nprobe = self.n_probe
        self._index.add(gallery_embeddings)
        logger.info("FAISS index built: %d vectors (dim=%d)", n, self.dim)

    def query(self, embedding: np.ndarray, top_k: int) -> np.ndarray:
        """Return indices of the top-K nearest gallery items.

        Args:
            embedding: 1-D float32 query, L2-normalised.
            top_k:     Number of results.

        Returns:
            1-D int array of gallery indices, sorted by descending similarity.
        """
        if self._index is None:
            raise RuntimeError("Call build() before querying")
        q = embedding.reshape(1, -1).astype(np.float32)
        _, indices = self._index.search(q, top_k)
        return indices[0]

    def retrieve_batch(self, queries: np.ndarray, top_k: int) -> np.ndarray:
        """Retrieve top-K for every query in the batch.

        Args:
            queries: Float32 array of shape ``(Q, D)``.
            top_k:   Number of neighbours per query.

        Returns:
            Int array of shape ``(Q, top_k)``.
        """
        if self._index is None:
            raise RuntimeError("Call build() before querying")
        queries = queries.astype(np.float32)
        _, indices = self._index.search(queries, top_k)
        return indices
