"""Normalised Discounted Cumulative Gain (nDCG@K).

nDCG penalises relevant items that appear late in the ranking using a
logarithmic discount. Widely used in academic IR benchmarks.
"""
from __future__ import annotations

import math


def dcg_at_k(retrieved_labels: list[str], relevant_labels: set[str], k: int) -> float:
    """Discounted Cumulative Gain at rank K."""
    total = 0.0
    for rank, label in enumerate(retrieved_labels[:k], start=1):
        if label in relevant_labels:
            total += 1.0 / math.log2(rank + 1)
    return total


def ideal_dcg_at_k(relevant_count: int, k: int) -> float:
    """Ideal DCG (all relevant items at the top).

    Args:
        relevant_count: Total number of relevant items in the gallery.
        k:              Cut-off rank.
    """
    n_relevant = min(relevant_count, k)
    return sum(1.0 / math.log2(rank + 1) for rank in range(1, n_relevant + 1))


def ndcg_at_k(
    retrieved_labels: list[str],
    relevant_labels: set[str],
    k: int,
    relevant_count: int | None = None,
) -> float:
    """Compute nDCG@K for a single query.

    Args:
        retrieved_labels: Ordered list of predicted labels.
        relevant_labels:  Set of ground-truth labels for membership testing.
        k:                Cut-off rank.
        relevant_count:   Total number of relevant items in the gallery.
                          Defaults to ``len(relevant_labels)`` if not provided.

    Returns:
        nDCG@K in [0, 1]. Returns 0 if no relevant items exist.
    """
    if relevant_count is None:
        relevant_count = len(relevant_labels)

    idcg = ideal_dcg_at_k(relevant_count, k)
    if idcg == 0:
        return 0.0
    return dcg_at_k(retrieved_labels, relevant_labels, k) / idcg


def mean_ndcg_at_k(
    all_retrieved: list[list[str]],
    all_relevant: list[set[str]],
    k: int,
    all_counts: list[int] | None = None,
) -> float:
    """Mean nDCG@K across all queries."""
    if not all_retrieved:
        return 0.0
    if all_counts is None:
        all_counts = [len(s) for s in all_relevant]
    scores = [
        ndcg_at_k(ret, rel, k, cnt)
        for ret, rel, cnt in zip(all_retrieved, all_relevant, all_counts)
    ]
    return sum(scores) / len(scores)
