"""Research-style retrieval evaluation for split-aware experiments."""
from __future__ import annotations

from pathlib import Path
from typing import Dict, List

import numpy as np


def calculate_precision_at_k(actual: List[int], predicted: List[int], k: int) -> float:
    if len(predicted) > k:
        predicted = predicted[:k]

    num_hits = sum(1 for p in predicted if p in actual)
    return num_hits / k


def calculate_average_precision(actual: List[int], predicted: List[int]) -> float:
    if not actual:
        return 0.0

    score = 0.0
    num_hits = 0.0
    for i, p in enumerate(predicted):
        if p in actual:
            num_hits += 1.0
            score += num_hits / (i + 1.0)

    return score / len(actual)


def calculate_recall_at_k(actual: List[int], predicted: List[int], k: int) -> float:
    if not actual:
        return 0.0

    if len(predicted) > k:
        predicted = predicted[:k]

    num_hits = sum(1 for p in predicted if p in actual)
    return num_hits / len(actual)


def evaluate_retrieval(
    q_features: np.ndarray,
    g_features: np.ndarray,
    q_labels: np.ndarray,
    g_labels: np.ndarray,
    ks: List[int] = [1, 5, 10],
) -> Dict[str, float]:
    q_features = q_features.astype(np.float32)
    g_features = g_features.astype(np.float32)

    if q_features.ndim != 2 or g_features.ndim != 2:
        raise ValueError("Feature arrays must be 2-dimensional")

    sim_matrix = np.matmul(q_features, g_features.T)
    top_k = max(ks)

    ap_scores: list[float] = []
    p_scores: dict[int, list[float]] = {k: [] for k in ks}
    r_scores: dict[int, list[float]] = {k: [] for k in ks}

    for i in range(len(q_features)):
        sorted_indices = np.argsort(sim_matrix[i])[::-1].tolist()
        actual = np.where(g_labels == q_labels[i])[0].tolist()
        if not actual:
            continue

        ap_scores.append(calculate_average_precision(actual, sorted_indices))
        for k in ks:
            p_scores[k].append(calculate_precision_at_k(actual, sorted_indices, k))
            r_scores[k].append(calculate_recall_at_k(actual, sorted_indices, k))

    return {
        **{f"P@{k}": float(np.mean(p_scores[k])) if p_scores[k] else 0.0 for k in ks},
        **{f"R@{k}": float(np.mean(r_scores[k])) if r_scores[k] else 0.0 for k in ks},
        "mAP@10": float(np.mean(ap_scores)) if ap_scores else 0.0,
    }


def evaluate_features(path: Path, ks: List[int] = [1, 5, 10]) -> Dict[str, float]:
    from pathlib import Path

    data = np.load(path, allow_pickle=False)
    embeddings = data["embeddings"]
    labels = data["labels"]
    splits = data["splits"]

    if len(embeddings) != len(labels) or len(labels) != len(splits):
        raise ValueError("Features, labels, and splits must have the same length")

    query_mask = splits == "test"
    gallery_mask = splits != "test"

    if not np.any(query_mask) or not np.any(gallery_mask):
        raise ValueError(
            "Feature bundle must contain both test and gallery splits for research evaluation"
        )

    q_features = embeddings[query_mask]
    g_features = embeddings[gallery_mask]
    q_labels = labels[query_mask]
    g_labels = labels[gallery_mask]

    return evaluate_retrieval(q_features, g_features, q_labels, g_labels, ks=ks)
