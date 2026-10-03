"""Recall@K comparison between approximate and exact retrieval."""
from __future__ import annotations

import numpy as np


def approximate_recall_at_k(
    approx_indices: np.ndarray,
    exact_indices: np.ndarray,
    k_values: list[int],
) -> dict[int, float]:
    """Compute recall@K of approximate vs exact nearest-neighbour search.

    Recall@K = |approx_top_K ∩ exact_top_K| / K

    Measures how well an approximate retrieval backend (e.g. FAISS, pgvector)
    preserves the top-K results from exact cosine search.  1.0 = perfect
    agreement with the exact ranking.

    Args:
        approx_indices: 2-D int array of shape ``(Q, K)`` from approximate search.
        exact_indices:  2-D int array of shape ``(Q, K)`` from exact search.
        k_values:       List of K values to evaluate.

    Returns:
        Dict mapping K -> mean recall across all queries.
    """
    # Validate: Approximate and exact index matrices must be the same shape
    if approx_indices.shape != exact_indices.shape:
        raise ValueError(
            f"Shape mismatch: approx {approx_indices.shape} vs exact {exact_indices.shape}"
        )

    q = len(approx_indices)
    results: dict[int, float] = {}
    for k in k_values:
        # Validate: Skip non-positive K values
        if k <= 0:
            results[k] = 0.0
            continue
        # Compute: Jaccard-like overlap between approximate and exact top-K sets
        k_cap = min(k, approx_indices.shape[1])
        recalls = []
        for i in range(q):
            approx_set = set(approx_indices[i, :k_cap])
            exact_set = set(exact_indices[i, :k_cap])
            # Explain: When exact set is empty (should not happen), treat as perfect match
            if exact_set:
                recalls.append(len(approx_set & exact_set) / k_cap)
            else:
                recalls.append(1.0)
        # Aggregate: Mean recall across all queries for this K
        results[k] = float(np.mean(recalls)) if recalls else 0.0
    return results
