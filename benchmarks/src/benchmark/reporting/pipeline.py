"""Pipeline-specific reporting for production benchmark results."""
from __future__ import annotations

import json
from datetime import datetime, timezone
from pathlib import Path

from benchmark.reporting.typst import _AUTO_GEN_COMMENT, _fmt, _table_block
from benchmark.utils.logging import get_logger

logger = get_logger("reporting.pipeline")


def write_pipeline_typst(
    results: list[dict],
    output_dir: Path = Path("outputs/pipeline/tables"),
) -> list[Path]:
    """Generate Typst tables for production pipeline metrics.

    Args:
        results: Output of ``PipelineRunner.run()``.
        output_dir: Where to write ``.typ`` files.

    Returns:
        List of written file paths.
    """
    output_dir.mkdir(parents=True, exist_ok=True)
    paths: list[Path] = []

    col_headers = [
        "Model", "Index Build (s)", "Ingestion (s)", "Query Latency (ms)",
        "Recall@5", "Recall@10", "Recall@20",
    ]
    data_rows = []
    for r in results:
        pm = r.get("production_metrics", {})
        data_rows.append([
            r["model_name"],
            _fmt(pm.get("index_build_time_s", {}).get("mean")),
            _fmt(pm.get("ingestion_time_s", {}).get("mean")),
            _fmt(pm.get("pgvector_query_latency_ms", {}).get("mean")),
            _fmt(pm.get("pgvector_recall@5", {}).get("mean")),
            _fmt(pm.get("pgvector_recall@10", {}).get("mean")),
            _fmt(pm.get("pgvector_recall@20", {}).get("mean")),
        ])

    content = _AUTO_GEN_COMMENT + "\n" + _table_block(
        caption="Production Pipeline — pgvector Metrics (3-Fold CV)",
        label="tab:pipeline-production",
        col_headers=col_headers,
        data_rows=data_rows,
    )
    path = output_dir / "pipeline_production.typ"
    path.write_text(content, encoding="utf-8")
    paths.append(path)

    logger.info("Pipeline Typst tables → %s", output_dir)
    return paths


def write_pipeline_json(
    results: list[dict],
    output_dir: Path = Path("outputs/pipeline/results"),
) -> Path:
    """Write complete pipeline results as JSON.

    Args:
        results: Output of ``PipelineRunner.run()``.
        output_dir: Where to write JSON file.

    Returns:
        Path to written JSON file.
    """
    output_dir.mkdir(parents=True, exist_ok=True)
    path = output_dir / "pipeline_results.json"
    path.write_text(
        json.dumps(results, indent=2),
        encoding="utf-8",
    )
    logger.info("Pipeline JSON results → %s", path)
    return path
