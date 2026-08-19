"""Regenerate the 5 thesis PNG charts from authoritative benchmark JSON.

Reads benchmarks/outputs/thesis/results/thesis_results_category_only.json
(accuracy) and benchmarks/outputs/thesis/results/thesis_results.json
(efficiency), then writes 5 PNG charts to
thesis/figures/chapters/part2/ch3-evaluation/diagrams/.

Run: python3 thesis/spec/regenerate_charts.py
"""
import json
from pathlib import Path

import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
import numpy as np

ROOT = Path(__file__).resolve().parents[2]
CAT_ONLY = ROOT / "benchmarks/outputs/thesis/results/thesis_results_category_only.json"
EFFICIENCY = ROOT / "benchmarks/outputs/thesis/results/thesis_results.json"
OUT_DIR = ROOT / "thesis/figures/chapters/part2/ch3-evaluation/diagrams"

PALETTE = ["#457B9D", "#E9C46A", "#E63946", "#2A9D8F"]
MODEL_ORDER = ["FashionCLIP", "CLIP-generic", "EfficientNet-B0", "ResNet-50"]


def load_json(path: Path) -> dict:
    return {m["model_name"]: m for m in json.loads(path.read_text())}


def main() -> None:
    cat = load_json(CAT_ONLY)
    eff = load_json(EFFICIENCY)
    OUT_DIR.mkdir(parents=True, exist_ok=True)

    models = [m for m in MODEL_ORDER if m in cat]
    colors = {m: PALETTE[i % len(PALETTE)] for i, m in enumerate(models)}

    # ── 1. mAP horizontal bar chart ──────────────────────────────────────
    fig, ax = plt.subplots(figsize=(7, max(3, len(models) * 0.7)))
    scores = [cat[m]["aggregate"]["map"]["mean"] for m in models]
    bars = ax.barh(
        models[::-1], scores[::-1],
        color=[colors[m] for m in models[::-1]],
        edgecolor="white", linewidth=0.5,
    )
    ax.bar_label(bars, fmt="%.4f", padding=3, fontsize=9)
    ax.set_xlabel("mAP")
    ax.set_title("Mean Average Precision — Fashion Retrieval Benchmark")
    ax.set_xlim(0, min(1.0, max(scores) * 1.08))
    fig.tight_layout()
    fig.savefig(OUT_DIR / "P2S3.5_benchmark_map.png", dpi=200)
    plt.close(fig)
    print("mAP chart → P2S3.5_benchmark_map.png")

    # ── 2. Precision@K line chart ────────────────────────────────────────
    k_values = sorted({int(k.split("@")[1]) for m in models for k in cat[m]["aggregate"] if k.startswith("precision@")})
    fig, ax = plt.subplots(figsize=(8, 5))
    for i, m in enumerate(models):
        ys = [cat[m]["aggregate"].get(f"precision@{k}", {}).get("mean", 0.0) for k in k_values]
        ax.plot(k_values, ys, marker="o", label=m, color=colors[m], linewidth=2)
    ax.set_xlabel("K")
    ax.set_ylabel("Precision@K")
    ax.set_title("Precision@K — Fashion Retrieval Benchmark")
    ax.legend(loc="upper right", framealpha=0.9)
    ax.set_xticks(k_values)
    fig.tight_layout()
    fig.savefig(OUT_DIR / "P2S3.5_benchmark_precision.png", dpi=200)
    plt.close(fig)
    print("Precision chart → P2S3.5_benchmark_precision.png")

    # ── 3. Recall@K line chart ───────────────────────────────────────────
    fig, ax = plt.subplots(figsize=(8, 5))
    for i, m in enumerate(models):
        ys = [cat[m]["aggregate"].get(f"recall@{k}", {}).get("mean", 0.0) for k in k_values]
        ax.plot(k_values, ys, marker="s", label=m, color=colors[m], linewidth=2)
    ax.set_xlabel("K")
    ax.set_ylabel("Recall@K")
    ax.set_title("Recall@K — Fashion Retrieval Benchmark")
    ax.legend(loc="lower right", framealpha=0.9)
    ax.set_xticks(k_values)
    fig.tight_layout()
    fig.savefig(OUT_DIR / "P2S3.5_benchmark_recall.png", dpi=200)
    plt.close(fig)
    print("Recall chart → P2S3.5_benchmark_recall.png")

    # ── 4. Latency grouped bar chart (p50/p95/p99) ──────────────────────
    models_by_latency = sorted(models, key=lambda m: eff[m]["aggregate"].get("latency_mean_ms", {}).get("mean", 999))
    x = np.arange(len(models_by_latency))
    width = 0.25

    p50, p95, p99 = [], [], []
    for m in models_by_latency:
        lat = eff[m]["aggregate"].get("latency_mean_ms", {})
        mean = lat.get("mean", 0)
        std = lat.get("std", 0)
        p50.append(mean)
        p95.append(mean + 1.645 * std)  # approximate p95 from mean+1.645*SD
        p99.append(mean + 2.326 * std)  # approximate p99 from mean+2.326*SD

    fig, ax = plt.subplots(figsize=(8, 5))
    ax.bar(x - width, p50, width, label="p50 (mean)", color="#457B9D")
    ax.bar(x,         p95, width, label="p95 (approx)", color="#E9C46A")
    ax.bar(x + width, p99, width, label="p99 (approx)", color="#E63946")
    ax.set_ylabel("Latency (ms)")
    ax.set_title("Embedding Latency — Fashion Retrieval Benchmark")
    ax.set_xticks(x)
    ax.set_xticklabels(models_by_latency, rotation=15, ha="right")
    ax.legend()
    fig.tight_layout()
    fig.savefig(OUT_DIR / "P2S3.6_benchmark_latency.png", dpi=200)
    plt.close(fig)
    print("Latency chart → P2S3.6_benchmark_latency.png")

    # ── 5. Throughput bar chart ──────────────────────────────────────────
    models_by_throughput = sorted(models, key=lambda m: eff[m]["aggregate"].get("throughput_per_sec", {}).get("mean", 0), reverse=True)
    throughputs = [eff[m]["aggregate"].get("throughput_per_sec", {}).get("mean", 0) for m in models_by_throughput]
    errors = [eff[m]["aggregate"].get("throughput_per_sec", {}).get("std", 0) for m in models_by_throughput]

    fig, ax = plt.subplots(figsize=(7, 4))
    bars = ax.bar(models_by_throughput, throughputs, yerr=errors,
                  color=[colors[m] for m in models_by_throughput],
                  edgecolor="white", linewidth=0.5, capsize=4)
    ax.bar_label(bars, fmt="%.1f", padding=3, fontsize=9)
    ax.set_ylabel("Throughput (images/sec)")
    ax.set_title("Inference Throughput — Fashion Retrieval Benchmark")
    ax.set_xticks(range(len(models_by_throughput)))
    ax.set_xticklabels(models_by_throughput, rotation=15, ha="right")
    fig.tight_layout()
    fig.savefig(OUT_DIR / "P2S3.6_benchmark_throughput.png", dpi=200)
    plt.close(fig)
    print("Throughput chart → P2S3.6_benchmark_throughput.png")

    print("\nAll 5 charts regenerated from authoritative JSON.")


if __name__ == "__main__":
    main()
