"""Multi-model comparison summaries.

Ranks and tabulates results across models. Produces sorted leaderboards
and flat dict rows for downstream CSV / Typst rendering.

Edge cases:
- Unknown sort key raises ValueError.
- Missing metric values default to 0.0 (accuracy) or inf (latency) for
  safe sorting.
"""
from __future__ import annotations

from benchmark.evaluation.evaluator import ModelMetrics


def rank_models(metrics_list: list[ModelMetrics], by: str = "map") -> list[ModelMetrics]:
    """Sort models by a chosen metric.

    Args:
        metrics_list: Results from ``run_benchmark``.
        by: Sort criterion — ``"map"``, ``"precision@K"``, ``"recall@K"``,
            or ``"latency"``.

    Returns:
        Sorted list. Best-first for accuracy metrics; lowest latency first
        when sorting by ``"latency"``.

    Raises:
        ValueError: If ``by`` does not match a recognised sort key.
    """
    if by == "map":
        return sorted(metrics_list, key=lambda m: m.map_score, reverse=True)

    if by.startswith("precision@"):
        # Extract K from the key string, e.g. "precision@5" -> 5
        k = int(by.split("@")[1])
        return sorted(metrics_list, key=lambda m: m.precision.get(k, 0.0), reverse=True)

    if by.startswith("recall@"):
        k = int(by.split("@")[1])
        return sorted(metrics_list, key=lambda m: m.recall.get(k, 0.0), reverse=True)

    if by == "latency":
        return sorted(metrics_list, key=lambda m: m.latency.get("p50_ms", float("inf")))

    raise ValueError(f"Unknown sort key '{by}'")


def comparison_table(metrics_list: list[ModelMetrics], k_values: list[int]) -> list[dict]:
    """Build a flat list of dicts for CSV / Typst table rendering.

    Args:
        metrics_list: Model results to tabulate.
        k_values: List of K values for precision/recall/ndcg columns.

    Returns:
        List of dicts, one per model, with keys ``model``, ``map``,
        ``p@{K}``, ``r@{K}``, ``ndcg@{K}``, ``latency_p50_ms``,
        ``latency_p95_ms``, and ``throughput``.
    """
    rows = []
    for m in metrics_list:
        row: dict = {"model": m.model_name, "map": round(m.map_score, 4)}
        for k in k_values:
            row[f"p@{k}"] = round(m.precision.get(k, 0.0), 4)
            row[f"r@{k}"] = round(m.recall.get(k, 0.0), 4)
            row[f"ndcg@{k}"] = round(m.ndcg.get(k, 0.0), 4)
        row["latency_p50_ms"] = m.latency.get("p50_ms", None)
        row["latency_p95_ms"] = m.latency.get("p95_ms", None)
        row["throughput"] = m.throughput_per_sec
        rows.append(row)
    return rows
