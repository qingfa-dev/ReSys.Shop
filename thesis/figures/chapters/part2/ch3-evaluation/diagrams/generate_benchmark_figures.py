"""Regenerate the 5 thesis benchmark PNG charts from authoritative JSON.

Mirrors the convention used by
figures/chapters/part2/ch2-design/04-implementations/diagram: the generator
lives alongside the diagrams it produces.

Reads the 6-model category-only sweep results
(benchmarks/outputs/thesis_catonly/results/thesis_results.json) and writes 5
PNG charts into this folder:

    P2S3.5_benchmark_map.png
    P2S3.5_benchmark_precision.png
    P2S3.5_benchmark_recall.png
    P2S3.6_benchmark_latency.png
    P2S3.6_benchmark_throughput.png

Run:
    python3 figures/chapters/part2/ch3-evaluation/diagrams/generate_benchmark_figures.py
or via the Makefile:
    make charts
"""
import argparse
import json
from pathlib import Path

import matplotlib

matplotlib.use("Agg")
import matplotlib.pyplot as plt

plt.rcParams.update(
    {
        "axes.grid": True,
        "grid.alpha": 0.3,
        "axes.axisbelow": True,
        "figure.dpi": 100,
        "font.size": 11,
    }
)
PALETTE = ["#1f77b4", "#ff7f0e", "#2ca02c", "#d62728", "#9467bd", "#8c564b"]

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[5]
DEFAULT_RESULTS = (
    ROOT / "benchmarks" / "outputs" / "thesis_catonly" / "results" / "thesis_results.json"
)


def load_rows(results: Path) -> list[dict]:
    rows = json.loads(results.read_text())
    return sorted(rows, key=lambda r: r["aggregate"]["map"]["mean"], reverse=True)


def main() -> None:
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument(
        "--results",
        type=Path,
        default=DEFAULT_RESULTS,
        help="Path to the 6-model benchmark results JSON (category-only sweep).",
    )
    ap.add_argument(
        "--out",
        type=Path,
        default=HERE,
        help="Output directory for the PNG charts (defaults to this folder).",
    )
    args = ap.parse_args()

    results = args.results.resolve()
    out = args.out.resolve()
    out.mkdir(parents=True, exist_ok=True)

    data = load_rows(results)
    names = [r["model_name"] for r in data]
    maps = [r["aggregate"]["map"]["mean"] for r in data]
    p5 = [r["aggregate"]["precision@5"]["mean"] for r in data]
    p10 = [r["aggregate"]["precision@10"]["mean"] for r in data]
    p20 = [r["aggregate"]["precision@20"]["mean"] for r in data]
    r5 = [r["aggregate"]["recall@5"]["mean"] for r in data]
    r10 = [r["aggregate"]["recall@10"]["mean"] for r in data]
    r20 = [r["aggregate"]["recall@20"]["mean"] for r in data]
    lat = [r["aggregate"]["latency_mean_ms"]["mean"] for r in data]
    thr = [r["aggregate"]["throughput_per_sec"]["mean"] for r in data]

    # mAP horizontal bar
    fig, ax = plt.subplots(figsize=(7, 4))
    order = list(range(len(names)))[::-1]
    bars = ax.barh(
        [names[i] for i in order],
        [maps[i] for i in order],
        color=[PALETTE[i % len(PALETTE)] for i in order],
        edgecolor="white",
        linewidth=0.5,
    )
    ax.bar_label(bars, fmt="%.4f", padding=3, fontsize=9)
    ax.set_xlabel("mAP")
    ax.set_title("Mean Average Precision: Fashion Retrieval Benchmark (6 Models)")
    ax.set_xlim(0, 1.0)
    fig.tight_layout()
    fig.savefig(out / "P2S3.5_benchmark_map.png", dpi=150)
    plt.close(fig)

    # Precision@K line
    fig, ax = plt.subplots(figsize=(7, 4))
    ks = [5, 10, 20]
    for i, r in enumerate(data):
        ys = [
            r["aggregate"]["precision@5"]["mean"],
            r["aggregate"]["precision@10"]["mean"],
            r["aggregate"]["precision@20"]["mean"],
        ]
        ax.plot(ks, ys, marker="o", label=r["model_name"], color=PALETTE[i % len(PALETTE)], linewidth=2)
    ax.set_xlabel("K")
    ax.set_ylabel("Precision@K")
    ax.set_title("Precision@K: Fashion Retrieval Benchmark (6 Models)")
    ax.legend(loc="lower left", framealpha=0.9)
    ax.set_xticks(ks)
    fig.tight_layout()
    fig.savefig(out / "P2S3.5_benchmark_precision.png", dpi=150)
    plt.close(fig)

    # Recall@K line
    fig, ax = plt.subplots(figsize=(7, 4))
    for i, r in enumerate(data):
        ys = [
            r["aggregate"]["recall@5"]["mean"],
            r["aggregate"]["recall@10"]["mean"],
            r["aggregate"]["recall@20"]["mean"],
        ]
        ax.plot(ks, ys, marker="s", label=r["model_name"], color=PALETTE[i % len(PALETTE)], linewidth=2)
    ax.set_xlabel("K")
    ax.set_ylabel("Recall@K")
    ax.set_title("Recall@K: Fashion Retrieval Benchmark (6 Models)")
    ax.legend(loc="upper left", framealpha=0.9)
    ax.set_xticks(ks)
    fig.tight_layout()
    fig.savefig(out / "P2S3.5_benchmark_recall.png", dpi=150)
    plt.close(fig)

    # Latency bar (ascending)
    lat_order = sorted(range(len(names)), key=lambda i: lat[i])
    fig, ax = plt.subplots(figsize=(7, 4))
    bars = ax.bar(
        range(len(lat_order)),
        [lat[i] for i in lat_order],
        color=[PALETTE[i % len(PALETTE)] for i in lat_order],
        edgecolor="white",
    )
    ax.bar_label(bars, fmt="%.1f", padding=3, fontsize=9)
    ax.set_ylabel("Latency (ms)")
    ax.set_title("Embedding Latency: Fashion Retrieval Benchmark (6 Models)")
    ax.set_xticks(range(len(lat_order)))
    ax.set_xticklabels([names[i] for i in lat_order], rotation=15, ha="right")
    fig.tight_layout()
    fig.savefig(out / "P2S3.6_benchmark_latency.png", dpi=150)
    plt.close(fig)

    # Throughput bar (descending)
    thr_order = sorted(range(len(names)), key=lambda i: -thr[i])
    fig, ax = plt.subplots(figsize=(7, 4))
    bars = ax.bar(
        range(len(thr_order)),
        [thr[i] for i in thr_order],
        color=[PALETTE[i % len(PALETTE)] for i in thr_order],
        edgecolor="white",
    )
    ax.bar_label(bars, fmt="%.1f", padding=3, fontsize=9)
    ax.set_ylabel("Throughput (img/s)")
    ax.set_title("Embedding Throughput: Fashion Retrieval Benchmark (6 Models)")
    ax.set_xticks(range(len(thr_order)))
    ax.set_xticklabels([names[i] for i in thr_order], rotation=15, ha="right")
    fig.tight_layout()
    fig.savefig(out / "P2S3.6_benchmark_throughput.png", dpi=150)
    plt.close(fig)

    print(f"Figures written to {out}")
    for n, m in zip(names, maps):
        idx = names.index(n)
        print(f"  {n}: mAP={m:.4f} lat={lat[idx]:.1f} thr={thr[idx]:.1f}")


if __name__ == "__main__":
    main()
