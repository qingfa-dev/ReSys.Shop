"""Cosine similarity retrieval over in-memory numpy embedding matrices."""

from __future__ import annotations

import numpy as np


def cosine_similarity(query: np.ndarray, gallery: np.ndarray) -> np.ndarray:
    """Compute cosine similarity between a query and a gallery matrix.

    Args:
        query: 1-D array of shape ``(D,)`` — already L2-normalised.
        gallery: 2-D array of shape ``(N, D)`` — already L2-normalised.

    Returns:
        1-D array of shape ``(N,)`` with similarity scores in ``[-1, 1]``.
    """
    return gallery @ query  # dot product on unit vectors = cosine sim


def top_k_indices(
    query: np.ndarray,
    gallery: np.ndarray,
    k: int,
    exclude_self: bool = True,
    self_idx: int | None = None,
) -> np.ndarray:
    """Return indices of the top-K most similar gallery items for a query.

    Args:
        query: 1-D embedding of shape ``(D,)``.
        gallery: 2-D gallery matrix of shape ``(N, D)``.
        k: Number of results to return.
        exclude_self: If True the gallery item at ``self_idx`` is excluded.
        self_idx: Index of the query in the gallery. Required when
            ``exclude_self=True`` and the gallery may contain duplicates.
            If None, falls back to ``np.argmax(sims)`` (legacy behaviour).

    Returns:
        1-D int array of shape ``(k,)`` with gallery indices sorted by
        descending similarity.
    """
    sims = cosine_similarity(query, gallery)
    if exclude_self:
        # Mask out the self-match using the explicit index when available
        idx = self_idx if self_idx is not None else int(np.argmax(sims))
        sims[idx] = -1.0

    k = min(k, len(sims))
    if k <= 0:
        return np.array([], dtype=np.int64)

    # argpartition then sort is O(N + k log k) vs argsort's O(N log N)
    top_idx = np.argpartition(sims, -k)[-k:]
    return top_idx[np.argsort(sims[top_idx])[::-1]]


def retrieve_batch(
    queries: np.ndarray,
    gallery: np.ndarray,
    k: int,
    exclude_self: bool = True,
) -> np.ndarray:
    """Retrieve top-K for each row in ``queries``.

    Args:
        queries: 2-D array of shape ``(Q, D)``.
        gallery: 2-D array of shape ``(N, D)``.
        k: Number of neighbours.
        exclude_self: Exclude perfect self-match from results.

    Returns:
        2-D int array of shape ``(Q, min(k, N))``.
    """
    k = min(k, len(gallery))
    if k <= 0:
        return np.empty((len(queries), 0), dtype=np.int64)
    results = np.empty((len(queries), k), dtype=np.int64)
    for i, q in enumerate(queries):
        results[i] = top_k_indices(q, gallery, k, exclude_self=exclude_self, self_idx=i)
    return results
