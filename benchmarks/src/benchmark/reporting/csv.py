"""CSV report writer — one row per model, all metrics as flat columns."""
from __future__ import annotations

import csv
from pathlib import Path

from benchmark.evaluation.comparison import comparison_table, rank_models
from benchmark.evaluation.evaluator import ModelMetrics
from benchmark.utils.logging import get_logger

logger = get_logger("reporting.csv")


def write_csv(
    all_metrics: list[ModelMetrics],
    k_values: list[int] | None = None,
    output_dir: Path = Path("outputs/reports"),
) -> Path:
    """Write a flat CSV with one row per model.

    Columns: model, map, p@K... r@K... ndcg@K..., latency_p50_ms,
             latency_p95_ms, latency_p99_ms, throughput_per_sec.

    Args:
        all_metrics: Results from ``BenchmarkRunner.run()``.
        k_values:    K cut-offs to include (defaults to all available).
        output_dir:  Destination directory.

    Returns:
        Path to the written CSV file.
    """
    output_dir.mkdir(parents=True, exist_ok=True)
    k_values = k_values or sorted({k for m in all_metrics for k in m.precision})
    ranked = rank_models(all_metrics, by="map")
    rows = comparison_table(ranked, k_values)

    if not rows:
        logger.warning("No results to write")
        return output_dir / "benchmark.csv"

    path = output_dir / "benchmark.csv"
    fieldnames = list(rows[0].keys())

    with path.open("w", newline="", encoding="utf-8") as fh:
        writer = csv.DictWriter(fh, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(rows)

    logger.info("CSV → %s (%d rows)", path, len(rows))
    return path
