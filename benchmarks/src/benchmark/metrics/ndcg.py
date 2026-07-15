"""Normalised Discounted Cumulative Gain (nDCG@K).

nDCG penalises relevant items that appear late in the ranking using a
logarithmic discount. Widely used in academic IR benchmarks.
"""
from __future__ import annotations

import math


def dcg_at_k(retrieved_labels: list[str], relevant_labels: set[str], k: int) -> float:
    """Discounted Cumulative Gain at rank K.

    Args:
        retrieved_labels: Ordered list of predicted labels (length >= k).
        relevant_labels:  Set of ground-truth labels for membership testing.
        k:                Cut-off rank.

    Returns:
        DCG in [0, log2(K+1)] (unbounded upper).
    """
    # Compute: Logarithmic discount — relevant items at lower ranks contribute less
    total = 0.0
    for rank, label in enumerate(retrieved_labels[:k], start=1):
        if label in relevant_labels:
            total += 1.0 / math.log2(rank + 1)
    return total


def ideal_dcg_at_k(relevant_count: int, k: int) -> float:
    """Ideal DCG — all relevant items at the top ranks.

    This is the normalisation denominator for nDCG.  It assumes the
    optimal ranking where every relevant item appears before any
    non-relevant item.

    Args:
        relevant_count: Total number of relevant items in the gallery.
        k:              Cut-off rank.

    Returns:
        Ideal DCG value.
    """
    # Compute: Sum of discounts for the first min(K, |relevant|) ranks
    n_relevant = min(relevant_count, k)
    return sum(1.0 / math.log2(rank + 1) for rank in range(1, n_relevant + 1))


def ndcg_at_k(
    retrieved_labels: list[str],
    relevant_labels: set[str],
    k: int,
    relevant_count: int | None = None,
) -> float:
    """Compute nDCG@K for a single query.

    nDCG = DCG / IDCG — the ratio of achieved to ideal gain.  Values
    near 1.0 indicate the ranking puts relevant items near the top.

    Args:
        retrieved_labels: Ordered list of predicted labels.
        relevant_labels:  Set of ground-truth labels for membership testing.
        k:                Cut-off rank.
        relevant_count:   Total number of relevant items in the gallery.
                          Defaults to ``len(relevant_labels)`` if not provided.

    Returns:
        nDCG@K in [0, 1]. Returns 0 if no relevant items exist.
    """
    # Assume: relevant_count defaults to set size when caller has no gallery info
    if relevant_count is None:
        relevant_count = len(relevant_labels)

    # Compute: Normalise DCG by ideal DCG
    idcg = ideal_dcg_at_k(relevant_count, k)
    # Validate: No relevant items means zero nDCG
    if idcg == 0:
        return 0.0
    return dcg_at_k(retrieved_labels, relevant_labels, k) / idcg


def mean_ndcg_at_k(
    all_retrieved: list[list[str]],
    all_relevant: list[set[str]],
    k: int,
    all_counts: list[int] | None = None,
) -> float:
    """Mean nDCG@K across all queries.

    Args:
        all_retrieved: One ranked label list per query (Q lists).
        all_relevant:  One ground-truth set per query (Q sets).
        k:             Cut-off rank.
        all_counts:    One relevant-count per query.  Defaults to
                       ``len(relevant_set)`` per query if not provided.

    Returns:
        Mean nDCG@K in [0, 1]. Returns 0 if no queries exist.
    """
    # Validate: Empty retrieval returns zero
    if not all_retrieved:
        return 0.0
    # Assume: Default counts from set sizes
    if all_counts is None:
        all_counts = [len(s) for s in all_relevant]
    # Compute: Average per-query nDCG@K
    scores = [
        ndcg_at_k(ret, rel, k, cnt)
        for ret, rel, cnt in zip(all_retrieved, all_relevant, all_counts)
    ]
    return sum(scores) / len(scores)
