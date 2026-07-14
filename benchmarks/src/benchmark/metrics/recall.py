"""Recall@K — fraction of relevant items appearing in the top-K results."""
from __future__ import annotations


def recall_at_k(
    retrieved_labels: list[str],
    relevant_labels: set[str],
    k: int,
    relevant_count: int | None = None,
) -> float:
    """Compute Recall@K for a single query.

    Args:
        retrieved_labels: Ordered list of predicted labels (length >= k).
        relevant_labels:  Set of ground-truth labels for membership testing.
        k:                Cut-off rank.
        relevant_count:   Total number of relevant items in the gallery.
                          Defaults to ``len(relevant_labels)`` if not provided.

    Returns:
        Recall@K in [0, 1]. Returns 0 if no relevant items exist.
    """
    if relevant_count is None:
        relevant_count = len(relevant_labels)

    if relevant_count == 0:
        return 0.0
    top_k = retrieved_labels[:k]
    hits = sum(1 for label in top_k if label in relevant_labels)
    return hits / relevant_count


def mean_recall_at_k(
    all_retrieved: list[list[str]],
    all_relevant: list[set[str]],
    k: int,
    all_counts: list[int] | None = None,
) -> float:
    """Mean Recall@K across all queries."""
    if not all_retrieved:
        return 0.0
    if all_counts is None:
        all_counts = [len(s) for s in all_relevant]
    scores = [
        recall_at_k(ret, rel, k, cnt)
        for ret, rel, cnt in zip(all_retrieved, all_relevant, all_counts)
    ]
    return sum(scores) / len(scores)
