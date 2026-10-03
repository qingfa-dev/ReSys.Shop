#!/usr/bin/env python
"""Merge CSV product metadata with per-product JSON articleAttributes.

Produces an enriched dataset with two label schemes per sample:
  label         — subCategory/normalizedColour  (primary)
  label_pattern — subCategory/normalizedColour/Pattern  (secondary)

Usage:
    uv run python scripts/_05_enrich_dataset.py \
        --json-styles data/raw/fashion-product-images/styles/ \
        --csv data/raw/fashion-product-images-small/styles.csv \
        --output data/raw/fashion-enriched-5k \
        --subset 5000
"""
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any


def parse_args() -> argparse.Namespace:
    p = argparse.ArgumentParser(description="Enrich dataset with JSON articleAttributes")
    p.add_argument("--json-styles", required=True, type=Path,
                   help="Directory of per-product JSON files")
    p.add_argument("--csv", required=True, type=Path,
                   help="CSV with id, subCategory, baseColour columns")
    p.add_argument("--output", required=True, type=Path,
                   help="Output directory for enriched dataset")
    p.add_argument("--subset", type=int, default=0,
                   help="Limit to first N products (0 = all)")
    p.add_argument("--folds", type=int, default=3,
                   help="Number of CV folds")
    p.add_argument("--seed", type=int, default=42,
                   help="Random seed for split generation")
    return p.parse_args()


def extract_pattern(data: dict[str, Any]) -> str:
    aa = data.get("articleAttributes", {})
    if not isinstance(aa, dict):
        return "Unknown"
    return str(aa.get("Pattern", "Unknown")).strip() or "Unknown"


def main() -> None:
    args = parse_args()

    # When running as `python scripts/_05_enrich_dataset.py`, Python adds the
    # script's directory to sys.path[0].  scripts/06_benchmark.py then shadows
    # the benchmark *package*.  Remove scripts/ and ensure src/ comes first.
    _script_dir = str(Path(__file__).resolve().parent)
    sys.path = [p for p in sys.path if p != _script_dir]
    _src = str(Path(__file__).resolve().parent.parent / "src")
    if _src not in sys.path:
        sys.path.insert(0, _src)

    import numpy as np
    import pandas as pd

    from benchmark.datasets.ground_truth import normalize_colour

    print(f"Reading CSV: {args.csv}")
    csv_df = pd.read_csv(args.csv, on_bad_lines="warn")
    csv_rows = {}
    for _, row in csv_df.iterrows():
        pid = str(row["id"])
        csv_rows[pid] = {
            "masterCategory": str(row.get("masterCategory", "")),
            "subCategory": str(row.get("subCategory", "")),
            "articleType": str(row.get("articleType", "")),
            "baseColour": str(row.get("baseColour", "")),
            "season": str(row.get("season", "")),
            "year": str(row.get("year", "")),
            "usage": str(row.get("usage", "")),
            "gender": str(row.get("gender", "")),
            "productDisplayName": str(row.get("productDisplayName", "")),
        }

    json_dir: Path = args.json_styles
    json_files = sorted(json_dir.glob("*.json"))
    if args.subset > 0:
        json_files = json_files[:args.subset]

    enriched: list[dict[str, str]] = []
    for jf in json_files:
        try:
            d = json.loads(jf.read_text(encoding="utf-8"))
        except (json.JSONDecodeError, OSError):
            continue
        data = d.get("data", {})
        pid = str(data.get("id", ""))
        if not pid or pid not in csv_rows:
            continue
        row = csv_rows[pid]
        pattern = extract_pattern(data)
        enriched.append({
            "id": pid,
            "masterCategory": row["masterCategory"],
            "subCategory": row["subCategory"],
            "articleType": row["articleType"],
            "baseColour": row["baseColour"],
            "season": row["season"],
            "year": row["year"],
            "usage": row["usage"],
            "gender": row["gender"],
            "productDisplayName": row["productDisplayName"],
            "pattern": pattern,
        })

    if not enriched:
        print("ERROR: No enriched rows produced. Check --json-styles and --csv paths.")
        sys.exit(1)

    enriched_df = pd.DataFrame(enriched)
    print(f"Enriched {len(enriched_df)} rows")

    args.output.mkdir(parents=True, exist_ok=True)
    csv_out = args.output / "styles.csv"
    enriched_df.to_csv(csv_out, index=False)
    print(f"Wrote enriched CSV: {csv_out}")

    rng = np.random.default_rng(args.seed)
    categories = enriched_df["masterCategory"].unique()
    fold_indices: list[list[int]] = [[] for _ in range(args.folds)]
    for cat in categories:
        cat_df = enriched_df[enriched_df["masterCategory"] == cat].reset_index(drop=True)
        indices = cat_df.index.to_numpy().copy()
        rng.shuffle(indices)
        splits_arr = np.array_split(indices, args.folds)
        for fi, s in enumerate(splits_arr):
            fold_indices[fi].extend(cat_df.iloc[s]["id"].tolist())

    enriched_df["_nc"] = enriched_df["baseColour"].apply(normalize_colour)
    meta_by_id = {}
    for _, row in enriched_df.iterrows():
        pid = row["id"]
        sc = str(row["subCategory"])
        nc = str(row["_nc"])
        pat = str(row.get("pattern", "Unknown"))
        primary = f"{sc}/{nc}"
        secondary = f"{sc}/{nc}/{pat}" if pat != "Unknown" else primary
        meta_by_id[pid] = {
            "image_path": f"images/{pid}.jpg",
            "label": primary,
            "label_pattern": secondary,
            "product_id": str(pid),
        }

    all_ids = set(enriched_df["id"].tolist())
    splits_dir = args.output / "splits"
    splits_dir.mkdir(parents=True, exist_ok=True)
    for fi in range(args.folds):
        test_ids = set(str(i) for i in fold_indices[fi])
        train_ids = all_ids - test_ids
        train = [meta_by_id[pid] for pid in sorted(train_ids) if pid in meta_by_id]
        test = [meta_by_id[pid] for pid in sorted(test_ids) if pid in meta_by_id]
        (splits_dir / f"fold_{fi}_train.json").write_text(json.dumps(train, indent=2), encoding="utf-8")
        (splits_dir / f"fold_{fi}_test.json").write_text(json.dumps(test, indent=2), encoding="utf-8")
        print(f"Fold {fi}: train={len(train)}, test={len(test)}")

    print(f"Done. Output: {args.output}")


if __name__ == "__main__":
    main()
