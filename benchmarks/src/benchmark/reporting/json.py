"""JSON report writer."""

from __future__ import annotations

import json
from pathlib import Path

from benchmark.evaluation.evaluator import ModelMetrics
from benchmark.utils.logging import get_logger

logger = get_logger("reporting.json")


def write_model_json(metrics: ModelMetrics, output_dir: Path = Path("outputs/metrics")) -> Path:
    """Write per-model JSON result file."""
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
    """Write multi-model comparison JSON."""
    output_dir.mkdir(parents=True, exist_ok=True)
    path = output_dir / "benchmark.json"
    data = [m.to_dict() for m in all_metrics]
    path.write_text(json.dumps(data, indent=2, ensure_ascii=False), encoding="utf-8")
    logger.info("Comparison JSON → %s", path)
    return path
