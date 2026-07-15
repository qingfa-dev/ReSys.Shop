"""Chart generator — produces publication-quality figures from benchmark results.

Outputs land in ``outputs/figures/`` as ``.pdf`` (for Typst / LaTeX inclusion)
and ``.png`` (for quick inspection and Markdown reports).

Generates four chart types: Precision@K line chart, Recall@K line chart,
latency grouped bar chart, and mAP horizontal bar chart.

Edge cases:
- Missing metrics (e.g. precision for a specific K) default to 0.0 in charts.
- Empty results produce empty charts.
- matplotlib and seaborn are imported lazily; ImportError gives a clear
  install instruction.

Requires: matplotlib, seaborn
"""
from __future__ import annotations

from pathlib import Path

from benchmark.evaluation.comparison import comparison_table, rank_models
from benchmark.evaluation.evaluator import ModelMetrics
from benchmark.utils.logging import get_logger

logger = get_logger("reporting.charts")

# Shared palette — one colour per model in registry order
_PALETTE = [
    "#E63946",  # fashion-clip  (red)
    "#457B9D",  # clip-b32      (steel blue)
    "#1D3557",  # clip-l14      (navy)
    "#2A9D8F",  # siglip        (teal)
    "#E9C46A",  # eva-clip      (gold)
]


def _setup_matplotlib() -> tuple:
    """Import and configure matplotlib / seaborn lazily.

    Sets the non-interactive ``Agg`` backend safe for headless servers.

    Returns:
        Tuple of ``(plt, sns)`` modules.

    Raises:
        ImportError: If matplotlib or seaborn is not installed.
    """
    try:
        import matplotlib
        matplotlib.use("Agg")
        import matplotlib.pyplot as plt
        import seaborn as sns
    except ImportError as exc:
        raise ImportError(
            "Chart generation requires matplotlib and seaborn. "
            "Install with: pip install matplotlib seaborn"
        ) from exc

    sns.set_theme(style="whitegrid", context="paper", font_scale=1.2)
    return plt, sns


def generate_precision_chart(
    all_metrics: list[ModelMetrics],
    k_values: list[int] | None = None,
    output_dir: Path = Path("outputs/figures"),
) -> list[Path]:
    """Line chart of Precision@K across K values for all models.

    Args:
        all_metrics: Results from ``BenchmarkRunner.run()``.
        k_values: K cut-offs to plot. Defaults to all available.
        output_dir: Destination for PDF and PNG output files.

    Returns:
        List of written file paths (PDF and PNG).
    """
    plt, _ = _setup_matplotlib()
    output_dir.mkdir(parents=True, exist_ok=True)
    k_values = k_values or sorted({k for m in all_metrics for k in m.precision})
    ranked = rank_models(all_metrics, by="map")

    fig, ax = plt.subplots(figsize=(7, 4.5))
    for i, m in enumerate(ranked):
        ys = [m.precision.get(k, 0.0) for k in k_values]
        ax.plot(k_values, ys, marker="o", label=m.model_name,
                color=_PALETTE[i % len(_PALETTE)], linewidth=2)

    ax.set_xlabel("K")
    ax.set_ylabel("Precision@K")
    ax.set_title("Precision@K — Fashion Retrieval Benchmark")
    ax.legend(loc="upper right", framealpha=0.9)
    ax.set_xticks(k_values)
    fig.tight_layout()

    paths = []
    for ext in ("pdf", "png"):
        p = output_dir / f"precision.{ext}"
        fig.savefig(p, dpi=150 if ext == "png" else None)
        paths.append(p)
    plt.close(fig)
    logger.info("Precision chart → %s", output_dir)
    return paths


def generate_recall_chart(
    all_metrics: list[ModelMetrics],
    k_values: list[int] | None = None,
    output_dir: Path = Path("outputs/figures"),
) -> list[Path]:
    """Line chart of Recall@K across K values.

    Args:
        all_metrics: Results from ``BenchmarkRunner.run()``.
        k_values: K cut-offs to plot. Defaults to all available.
        output_dir: Destination for PDF and PNG output files.

    Returns:
        List of written file paths (PDF and PNG).
    """
    plt, _ = _setup_matplotlib()
    output_dir.mkdir(parents=True, exist_ok=True)
    k_values = k_values or sorted({k for m in all_metrics for k in m.recall})
    ranked = rank_models(all_metrics, by="map")

    fig, ax = plt.subplots(figsize=(7, 4.5))
    for i, m in enumerate(ranked):
        ys = [m.recall.get(k, 0.0) for k in k_values]
        ax.plot(k_values, ys, marker="s", label=m.model_name,
                color=_PALETTE[i % len(_PALETTE)], linewidth=2)

    ax.set_xlabel("K")
    ax.set_ylabel("Recall@K")
    ax.set_title("Recall@K — Fashion Retrieval Benchmark")
    ax.legend(loc="lower right", framealpha=0.9)
    ax.set_xticks(k_values)
    fig.tight_layout()

    paths = []
    for ext in ("pdf", "png"):
        p = output_dir / f"recall.{ext}"
        fig.savefig(p, dpi=150 if ext == "png" else None)
        paths.append(p)
    plt.close(fig)
    logger.info("Recall chart → %s", output_dir)
    return paths


def generate_latency_chart(
    all_metrics: list[ModelMetrics],
    output_dir: Path = Path("outputs/figures"),
) -> list[Path]:
    """Grouped bar chart of p50 / p95 / p99 latency per model.

    Args:
        all_metrics: Results from ``BenchmarkRunner.run()``.
        output_dir: Destination for PDF and PNG output files.

    Returns:
        List of written file paths (PDF and PNG).
    """
    plt, _ = _setup_matplotlib()
    import numpy as np

    output_dir.mkdir(parents=True, exist_ok=True)
    ranked = rank_models(all_metrics, by="latency")
    models = [m.model_name for m in ranked]
    p50 = [m.latency.get("p50_ms", 0.0) for m in ranked]
    p95 = [m.latency.get("p95_ms", 0.0) for m in ranked]
    p99 = [m.latency.get("p99_ms", 0.0) for m in ranked]

    x = np.arange(len(models))
    width = 0.25

    fig, ax = plt.subplots(figsize=(8, 4.5))
    ax.bar(x - width, p50, width, label="p50", color="#457B9D")
    ax.bar(x,         p95, width, label="p95", color="#E9C46A")
    ax.bar(x + width, p99, width, label="p99", color="#E63946")

    ax.set_ylabel("Latency (ms)")
    ax.set_title("Embedding Latency — Fashion Retrieval Benchmark")
    ax.set_xticks(x)
    ax.set_xticklabels(models, rotation=15, ha="right")
    ax.legend()
    fig.tight_layout()

    paths = []
    for ext in ("pdf", "png"):
        p = output_dir / f"latency.{ext}"
        fig.savefig(p, dpi=150 if ext == "png" else None)
        paths.append(p)
    plt.close(fig)
    logger.info("Latency chart → %s", output_dir)
    return paths


def generate_map_bar_chart(
    all_metrics: list[ModelMetrics],
    output_dir: Path = Path("outputs/figures"),
) -> list[Path]:
    """Horizontal bar chart of mAP scores, sorted best to worst.

    Args:
        all_metrics: Results from ``BenchmarkRunner.run()``.
        output_dir: Destination for PDF and PNG output files.

    Returns:
        List of written file paths (PDF and PNG).
    """
    plt, _ = _setup_matplotlib()

    output_dir.mkdir(parents=True, exist_ok=True)
    ranked = rank_models(all_metrics, by="map")
    models = [m.model_name for m in ranked]
    scores = [m.map_score for m in ranked]

    fig, ax = plt.subplots(figsize=(7, max(3, len(models) * 0.7)))
    bars = ax.barh(
        models[::-1], scores[::-1],
        color=[_PALETTE[i % len(_PALETTE)] for i in range(len(models) - 1, -1, -1)],
        edgecolor="white", linewidth=0.5,
    )
    ax.bar_label(bars, fmt="%.4f", padding=3, fontsize=9)
    ax.set_xlabel("mAP")
    ax.set_title("Mean Average Precision — Fashion Retrieval Benchmark")
    ax.set_xlim(0, min(1.05, max(scores) * 1.15) if scores else 1)
    fig.tight_layout()

    paths = []
    for ext in ("pdf", "png"):
        p = output_dir / f"map.{ext}"
        fig.savefig(p, dpi=150 if ext == "png" else None)
        paths.append(p)
    plt.close(fig)
    logger.info("mAP chart → %s", output_dir)
    return paths


def generate_all_charts(
    all_metrics: list[ModelMetrics],
    k_values: list[int] | None = None,
    output_dir: Path = Path("outputs/figures"),
) -> list[Path]:
    """Generate precision, recall, latency, and mAP charts.

    Convenience wrapper that runs all four chart generators in sequence.

    Args:
        all_metrics: Results from ``BenchmarkRunner.run()``.
        k_values: K cut-offs to include in precision/recall charts.
        output_dir: Destination directory.

    Returns:
        Flat list of all written file paths.
    """
    all_paths: list[Path] = []
    all_paths += generate_precision_chart(all_metrics, k_values, output_dir)
    all_paths += generate_recall_chart(all_metrics, k_values, output_dir)
    all_paths += generate_latency_chart(all_metrics, output_dir)
    all_paths += generate_map_bar_chart(all_metrics, output_dir)
    return all_paths
