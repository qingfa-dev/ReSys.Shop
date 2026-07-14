"""Research reporting helpers for generating summary artifacts."""
from __future__ import annotations

from pathlib import Path

import matplotlib.pyplot as plt
import pandas as pd
import seaborn as sns

from benchmark.utils.logging import get_logger

logger = get_logger("research.reports")


def generate_research_report(
    metrics: list[dict],
    output_dir: Path = Path("outputs/research/reports"),
) -> dict[str, Path]:
    output_dir.mkdir(parents=True, exist_ok=True)
    df = pd.DataFrame(metrics)
    df = df.sort_values(by=[col for col in ["mAP@10"] if col in df.columns], ascending=False)

    csv_path = output_dir / "final_results.csv"
    df.to_csv(csv_path, index=False)

    md_path = output_dir / "research_summary.md"
    md_lines = ["# Research Evaluation Summary", ""]
    md_lines.append(df.to_markdown(index=False))
    md_lines.append("")
    md_path.write_text("\n".join(md_lines), encoding="utf-8")

    tex_path = output_dir / "final_results.tex"
    try:
        df.to_latex(
            tex_path,
            index=False,
            caption="Research Model Performance Summary",
            label="tab:research_results",
            float_format="%.4f",
        )
    except Exception:
        logger.warning("Failed to generate LaTeX output; pandas may not support it in this environment")

    chart_paths = _generate_research_charts(df, output_dir)
    return {
        "csv": csv_path,
        "markdown": md_path,
        "latex": tex_path,
        **chart_paths,
    }


def _generate_research_charts(df: pd.DataFrame, output_dir: Path) -> dict[str, Path]:
    chart_paths: dict[str, Path] = {}
    if "mAP@10" in df.columns:
        chart_paths["accuracy"] = _generate_bar_chart(
            df,
            x="Model",
            y="mAP@10",
            title="Model mAP@10 Comparison",
            filename="accuracy-comparison.png",
            output_dir=output_dir,
        )
    if "Avg Latency (ms)" in df.columns:
        chart_paths["latency"] = _generate_bar_chart(
            df,
            x="Model",
            y="Avg Latency (ms)",
            title="Average Latency per Model",
            filename="latency-comparison.png",
            output_dir=output_dir,
        )
    return chart_paths


def _generate_bar_chart(
    df: pd.DataFrame,
    x: str,
    y: str,
    title: str,
    filename: str,
    output_dir: Path,
) -> Path:
    sns.set_theme(style="whitegrid")
    plt.figure(figsize=(9, 5))
    ax = sns.barplot(data=df, x=x, y=y, palette="crest")
    ax.set_title(title)
    ax.set_xlabel(x)
    ax.set_ylabel(y)
    plt.xticks(rotation=15)
    plt.tight_layout()
    path = output_dir / filename
    plt.savefig(path, dpi=200)
    plt.close()
    logger.info("Research chart generated: %s", path)
    return path
