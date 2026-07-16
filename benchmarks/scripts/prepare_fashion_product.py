#!/usr/bin/env python
"""Prepare the Kaggle Fashion Product Images Small dataset for benchmark use.

This converts the Kaggle CSV + image folder into the benchmark's JSON split format:
    [ {"image_path": "images/12345.jpg", "label": "tops", "product_id": "12345" }, ... ]

Usage:
    python scripts/prepare_fashion_product.py \
        --csv data/raw/fashion-product-images-small/styles.csv \
        --images-root data/raw/fashion-product-images-small/images \
        --output-root data/raw/fashion-product-images-small
"""
from __future__ import annotations

import argparse
import csv
import json
from pathlib import Path
from random import Random
from typing import Sequence


def load_styles(csv_path: Path, label_field: str) -> list[dict[str, str]]:
    if not csv_path.exists():
        raise FileNotFoundError(f"Metadata CSV not found: {csv_path}")

    rows: list[dict[str, str]] = []
    with csv_path.open("r", encoding="utf-8") as f:
        reader = csv.DictReader(f)
        if label_field not in reader.fieldnames:  # type: ignore[assignment]
            raise ValueError(
                f"Label field '{label_field}' not found in CSV columns: {reader.fieldnames}"
            )
        for row in reader:
            rows.append(row)

    return rows


def build_samples(
    rows: Sequence[dict[str, str]],
    images_root: Path,
    label_field: str,
    image_prefix: str = "images",
) -> list[dict[str, str]]:
    samples: list[dict[str, str]] = []
    for row in rows:
        image_id = row.get("id") or row.get("image_id") or row.get("image")
        if not image_id:
            continue
        image_name = f"{image_id}.jpg"
        image_path = images_root / image_name
        if not image_path.exists():
            continue

        label = row.get(label_field, "unknown")
        samples.append(
            {
                "image_path": f"{image_prefix}/{image_name}",
                "label": label,
                "product_id": str(image_id),
            }
        )
    return samples


def write_split_json(split_path: Path, samples: list[dict[str, str]]) -> None:
    split_path.parent.mkdir(parents=True, exist_ok=True)
    split_path.write_text(json.dumps(samples, indent=2), encoding="utf-8")


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Prepare Kaggle Fashion Product Images Small for benchmark evaluation"
    )
    parser.add_argument("--csv", type=Path, required=True, help="Path to styles.csv")
    parser.add_argument(
        "--images-root",
        type=Path,
        required=True,
        help="Path to the extracted image folder",
    )
    parser.add_argument(
        "--output-root",
        type=Path,
        default=Path("data/raw/fashion-product-images-small"),
        help="Root directory for the prepared raw dataset",
    )
    parser.add_argument(
        "--label-field",
        type=str,
        default="subCategory",
        choices=["gender", "masterCategory", "subCategory", "articleType"],
        help="Metadata field to use as the retrieval label",
    )
    parser.add_argument(
        "--train-ratio",
        type=float,
        default=0.8,
        help="Fraction of examples to allocate to the train split",
    )
    parser.add_argument(
        "--seed",
        type=int,
        default=42,
        help="Random seed for split generation",
    )
    args = parser.parse_args()

    rows = load_styles(args.csv, args.label_field)
    samples = build_samples(rows, args.images_root, args.label_field)
    if not samples:
        raise SystemExit("No samples were found. Check the CSV and image paths.")

    rand = Random(args.seed)
    rand.shuffle(samples)
    split_index = int(len(samples) * args.train_ratio)
    train_samples = samples[:split_index]
    test_samples = samples[split_index:]

    output_root = args.output_root
    write_split_json(output_root / "splits" / "train.json", train_samples)
    write_split_json(output_root / "splits" / "test.json", test_samples)

    print(f"Prepared dataset at {output_root}")
    print(f"  {len(train_samples)} train samples")
    print(f"  {len(test_samples)} test samples")
    print(f"Split files written to {output_root / 'splits'}")


if __name__ == "__main__":
    main()
