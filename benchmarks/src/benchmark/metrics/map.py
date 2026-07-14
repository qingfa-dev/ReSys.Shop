"""Mean Average Precision (mAP) — standard IR metric for ranked retrieval."""
from __future__ import annotations


def average_precision(
    retrieved_labels: list[str],
    relevant_labels: set[str],
    relevant_count: int | None = None,
    k_cap: int | None = None,
) -> float:
    """Compute Average Precision (AP) for a single query.

    AP is the area under the precision-recall curve.  For a ranked list it
    equals the mean of precision values at each rank where a relevant item
    appears, divided by the total number of relevant items in the gallery.

    When ``k_cap`` is provided, the denominator is capped at ``k_cap``,
    computing AP@K (the standard for retrieval benchmarks where the gallery
    is larger than the retrieval window).

    Args:
        retrieved_labels: Full ranked list of predicted labels.
        relevant_labels:  Set of ground-truth labels for membership testing.
        relevant_count:   Total number of relevant items in the gallery.
                          Defaults to ``len(relevant_labels)`` if not provided.
        k_cap:            If set, cap the denominator at this value (AP@k_cap).

    Returns:
        AP in [0, 1]. Returns 0 if no relevant items exist.
    """
    if relevant_count is None:
        relevant_count = len(relevant_labels)

    if relevant_count == 0:
        return 0.0

    denominator = relevant_count
    if k_cap is not None and k_cap < denominator:
        denominator = k_cap

    hits = 0
    running_sum = 0.0
    # When k_cap is set, only consider the top-k_cap results
    search_window = retrieved_labels[:k_cap] if k_cap is not None else retrieved_labels
    for rank, label in enumerate(search_window, start=1):
        if label in relevant_labels:
            hits += 1
            running_sum += hits / rank

    return running_sum / denominator


def mean_average_precision(
    all_retrieved: list[list[str]],
    all_relevant: list[set[str]],
    all_counts: list[int] | None = None,
    k_cap: int | None = None,
) -> float:
    """Compute mAP over all queries.

    Args:
        all_retrieved: One ranked label list per query.
        all_relevant:  One ground-truth set per query (for membership).
        all_counts:    One relevant-count per query. Must match in length.
                       Defaults to ``len(set)`` for each query if not provided.
        k_cap:         If set, compute mAP@k_cap.

    Returns:
        mAP in [0, 1].
    """
    if not all_retrieved:
        return 0.0
    if all_counts is None:
        all_counts = [len(s) for s in all_relevant]

    aps = [
        average_precision(ret, rel, cnt, k_cap)
        for ret, rel, cnt in zip(all_retrieved, all_relevant, all_counts)
    ]
    return sum(aps) / len(aps)
