"""Verification oracle for thesis review remediation.

Reads the authoritative benchmark JSON and prints the values the thesis
must match. Later tasks run this to confirm edits. Exits non-zero if a
JSON file is missing or unreadable.
"""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
BENCH = ROOT / "benchmarks/outputs/thesis/results"
CAT_ONLY = BENCH / "thesis_results_category_only.json"
EFFICIENCY = BENCH / "thesis_results.json"


def load(path: Path) -> list:
    if not path.exists():
        raise SystemExit(f"MISSING authoritative source: {path}")
    return json.loads(path.read_text())


def fmt(v: dict) -> str:
    return f"{round(v['mean'], 4)} ± {round(v['std'], 4)}"


def main() -> None:
    cat = load(CAT_ONLY)
    eff = load(EFFICIENCY)

    print("=== CATEGORY-ONLY (Appendix A.1 authoritative) ===")
    models = {m["model_name"]: m for m in cat}
    for name, m in models.items():
        a = m["aggregate"]
        print(f"{name}: mAP {fmt(a['map'])}")
        for k in ("precision@5", "precision@10", "precision@20",
                  "recall@5", "recall@10", "recall@20"):
            print(f"    {k}: {fmt(a[k])}")

    print("\n=== EFFICIENCY (Appendix A.4 authoritative) ===")
    emodels = {m["model_name"]: m for m in eff}
    for name, m in emodels.items():
        a = m["aggregate"]
        print(f"{name}:")
        for k in ("latency_mean_ms", "throughput_per_sec",
                  "load_time_ms", "index_storage_mb"):
            if k in a:
                print(f"    {k}: {fmt(a[k])}")

    print("\n=== RECOMPUTED DERIVED PERCENTAGES ===")
    fclip = models["FashionCLIP"]["aggregate"]["map"]["mean"]
    clip_g = models["CLIP-generic"]["aggregate"]["map"]["mean"]
    effnet = models["EfficientNet-B0"]["aggregate"]["map"]["mean"]
    resnet = models["ResNet-50"]["aggregate"]["map"]["mean"]

    def pct(numerator_gain: float, base: float) -> float:
        return round((numerator_gain - base) / base * 100, 2)

    print(f"Fashion-CLIP vs CLIP-generic mAP: {pct(fclip, clip_g)}% (was 5.4%)")
    print(f"Fashion-CLIP vs EfficientNet-B0 mAP: {pct(fclip, effnet)}% (was 7.7%)")
    print(f"Fashion-CLIP vs ResNet-50 mAP: {pct(fclip, resnet)}% (was 8.2%)")
    print(f"EfficientNet-B0 as % of Fashion-CLIP mAP: {round(effnet/fclip*100, 2)}% (was 92.8%)")
    print(f"CLIP-generic vs EfficientNet-B0 mAP: {pct(clip_g, effnet)}% (was 2.2%)")
    print(f"CLIP-generic vs ResNet-50 mAP: {pct(clip_g, resnet)}% (was 2.7%)")

    ef = emodels["FashionCLIP"]["aggregate"]
    ee = emodels["EfficientNet-B0"]["aggregate"]
    print(f"EfficientNet-B0 latency as % of Fashion-CLIP: "
          f"{round(ee['latency_mean_ms']['mean']/ef['latency_mean_ms']['mean']*100, 2)}% (was 26.0%)")
    print(f"Fashion-CLIP latency / EfficientNet-B0 latency: "
          f"{round(ef['latency_mean_ms']['mean']/ee['latency_mean_ms']['mean'], 2)}x (was 3.8x)")

    print("\n=== CONFIDENCE-INTERVAL BOUNDS (mean ± 2SD) ===")
    for name, m in models.items():
        mv = m["aggregate"]["map"]
        lo = round(mv["mean"] - 2 * mv["std"], 4)
        hi = round(mv["mean"] + 2 * mv["std"], 4)
        print(f"{name}: mAP lower {lo}, upper {hi}")


if __name__ == "__main__":
    main()
