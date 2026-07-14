"""Markdown report writer — human-readable summary with tables."""
from __future__ import annotations

from datetime import datetime, timezone
from pathlib import Path

from benchmark.evaluation.comparison import comparison_table, rank_models
from benchmark.evaluation.evaluator import ModelMetrics
from benchmark.utils.logging import get_logger

logger = get_logger("reporting.markdown")


def _fmt(val: float | None, decimals: int = 4) -> str:
    if val is None:
        return "—"
    return f"{val:.{decimals}f}"


def write_markdown(
    all_metrics: list[ModelMetrics],
    k_values: list[int] | None = None,
    output_dir: Path = Path("outputs/reports"),
    dataset_name: str = "Fashion Dataset",
) -> Path:
    """Write a Markdown benchmark summary.

    Args:
        all_metrics: Results from ``BenchmarkRunner.run()``.
        k_values:    K cut-offs to include in tables.
        output_dir:  Destination directory.
        dataset_name: Label printed in the report header.

    Returns:
        Path to the written Markdown file.
    """
    output_dir.mkdir(parents=True, exist_ok=True)
    k_values = k_values or sorted({k for m in all_metrics for k in m.precision})
    ranked = rank_models(all_metrics, by="map")
    rows = comparison_table(ranked, k_values)
    best = ranked[0].model_name if ranked else "N/A"
    ts = datetime.now(timezone.utc).strftime("%Y-%m-%d %H:%M UTC")

    lines: list[str] = [
        f"# Fashion Retrieval Benchmark — {dataset_name}",
        f"",
        f"Generated: {ts}  ",
        f"Best model by mAP: **{best}**",
        f"",
        f"## Retrieval Metrics",
        f"",
    ]

    # Precision table
    p_headers = ["Model", "mAP"] + [f"P@{k}" for k in k_values] + [f"R@{k}" for k in k_values]
    lines.append("| " + " | ".join(p_headers) + " |")
    lines.append("| " + " | ".join(["---"] * len(p_headers)) + " |")
    for row in rows:
        cells = [row["model"], _fmt(row["map"])]
        cells += [_fmt(row.get(f"p@{k}")) for k in k_values]
        cells += [_fmt(row.get(f"r@{k}")) for k in k_values]
        lines.append("| " + " | ".join(cells) + " |")

    lines += ["", "## nDCG", ""]
    ndcg_headers = ["Model"] + [f"nDCG@{k}" for k in k_values]
    lines.append("| " + " | ".join(ndcg_headers) + " |")
    lines.append("| " + " | ".join(["---"] * len(ndcg_headers)) + " |")
    for row in rows:
        cells = [row["model"]] + [_fmt(row.get(f"ndcg@{k}")) for k in k_values]
        lines.append("| " + " | ".join(cells) + " |")

    lines += ["", "## Latency & Throughput", ""]
    lat_headers = ["Model", "p50 (ms)", "p95 (ms)", "p99 (ms)", "Throughput (img/s)"]
    lines.append("| " + " | ".join(lat_headers) + " |")
    lines.append("| " + " | ".join(["---"] * len(lat_headers)) + " |")
    for m in ranked:
        lat = m.latency
        cells = [
            m.model_name,
            _fmt(lat.get("p50_ms"), 1),
            _fmt(lat.get("p95_ms"), 1),
            _fmt(lat.get("p99_ms"), 1),
            _fmt(m.throughput_per_sec, 1),
        ]
        lines.append("| " + " | ".join(cells) + " |")

    lines.append("")

    path = output_dir / "summary.md"
    path.write_text("\n".join(lines), encoding="utf-8")
    logger.info("Markdown → %s", path)
    return path
