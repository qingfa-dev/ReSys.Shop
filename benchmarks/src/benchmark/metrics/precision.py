"""Precision@K — fraction of retrieved items that are relevant."""
from __future__ import annotations


def precision_at_k(retrieved_labels: list[str], relevant_labels: set[str], k: int) -> float:
    """Compute Precision@K for a single query.

    Args:
        retrieved_labels: Ordered list of predicted labels (length >= k).
        relevant_labels:  Set of ground-truth labels for this query.
        k:                Cut-off rank.

    Returns:
        Precision@K in [0, 1].
    """
    if k == 0:
        return 0.0
    top_k = retrieved_labels[:k]
    hits = sum(1 for label in top_k if label in relevant_labels)
    return hits / k


def mean_precision_at_k(
    all_retrieved: list[list[str]],
    all_relevant: list[set[str]],
    k: int,
) -> float:
    """Mean Precision@K across all queries."""
    if not all_retrieved:
        return 0.0
    scores = [
        precision_at_k(ret, rel, k)
        for ret, rel in zip(all_retrieved, all_relevant)
    ]
    return sum(scores) / len(scores)
