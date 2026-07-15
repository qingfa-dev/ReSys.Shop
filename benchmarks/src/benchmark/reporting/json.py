"""JSON report writer.

Serialises per-model and multi-model comparison results as pretty-printed
JSON files in ``outputs/metrics/`` and ``outputs/reports/``.

Edge cases:
- Empty metrics list produces an empty JSON array in the comparison file.
- Special characters in model names are replaced with underscores for the
  slug used as the filename.
"""
from __future__ import annotations

import json
from pathlib import Path

from benchmark.evaluation.evaluator import ModelMetrics
from benchmark.utils.logging import get_logger

logger = get_logger("reporting.json")


def write_model_json(metrics: ModelMetrics, output_dir: Path = Path("outputs/metrics")) -> Path:
    """Write a per-model JSON result file.

    Args:
        metrics: A single model's benchmark results.
        output_dir: Destination directory for the JSON file.

    Returns:
        Path to the written JSON file.
    """
    output_dir.mkdir(parents=True, exist_ok=True)
    slug = metrics.model_name.lower().replace(" ", "_").replace("/", "_")
    path = output_dir / f"{slug}.json"
    path.write_text(json.dumps(metrics.to_dict(), indent=2, ensure_ascii=False), encoding="utf-8")
    logger.info("JSON → %s", path)
    return path


def write_comparison_json(
    all_metrics: list[ModelMetrics],
    output_dir: Path = Path("outputs/reports"),
) -> Path:
    """Write a multi-model comparison JSON file.

    Args:
        all_metrics: Results from ``BenchmarkRunner.run()`` or ``rank_models()``.
        output_dir: Destination directory.

    Returns:
        Path to the written ``benchmark.json`` file.
    """
    output_dir.mkdir(parents=True, exist_ok=True)
    path = output_dir / "benchmark.json"
    data = [m.to_dict() for m in all_metrics]
    path.write_text(json.dumps(data, indent=2, ensure_ascii=False), encoding="utf-8")
    logger.info("Comparison JSON → %s", path)
    return path
