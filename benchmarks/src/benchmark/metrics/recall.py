"""Recall@K — fraction of relevant items appearing in the top-K results."""
from __future__ import annotations


def recall_at_k(
    retrieved_labels: list[str],
    relevant_labels: set[str],
    k: int,
    relevant_count: int | None = None,
) -> float:
    """Compute Recall@K for a single query.

    Recall@K measures how many of all relevant items appear in the
    top-K results. The denominator is the total number of relevant
    items in the gallery, not just those in top-K.

    Args:
        retrieved_labels: Ordered list of predicted labels (length >= k).
        relevant_labels:  Set of ground-truth labels for membership testing.
        k:                Cut-off rank.
        relevant_count:   Total number of relevant items in the gallery.
                          Defaults to ``len(relevant_labels)`` if not provided.

    Returns:
        Recall@K in [0, 1]. Returns 0 if no relevant items exist.
    """
    # Assume: relevant_count overrides len(relevant_labels) when caller
    #         has pre-computed gallery-level counts for accuracy
    if relevant_count is None:
        relevant_count = len(relevant_labels)

    # Validate: No relevant items means zero recall
    if relevant_count == 0:
        return 0.0
    # Compute: Hits among top-K divided by total relevant items
    top_k = retrieved_labels[:k]
    hits = sum(1 for label in top_k if label in relevant_labels)
    return hits / relevant_count


def mean_recall_at_k(
    all_retrieved: list[list[str]],
    all_relevant: list[set[str]],
    k: int,
    all_counts: list[int] | None = None,
) -> float:
    """Mean Recall@K across all queries.

    Args:
        all_retrieved: One ranked label list per query (Q lists).
        all_relevant:  One ground-truth set per query (Q sets).
        k:             Cut-off rank.
        all_counts:    One relevant-count per query.  Defaults to
                       ``len(relevant_set)`` per query if not provided.

    Returns:
        Mean Recall@K in [0, 1]. Returns 0 if no queries exist.
    """
    # Validate: Empty retrieval set returns zero
    if not all_retrieved:
        return 0.0
    # Assume: Default all_counts from set sizes when caller has no gallery info
    if all_counts is None:
        all_counts = [len(s) for s in all_relevant]
    # Compute: Average per-query Recall@K
    scores = [
        recall_at_k(ret, rel, k, cnt)
        for ret, rel, cnt in zip(all_retrieved, all_relevant, all_counts, strict=True)
    ]
    return sum(scores) / len(scores)
