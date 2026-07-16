#!/usr/bin/env python
"""Generate image embeddings using the benchmark model registry."""
from __future__ import annotations

import argparse
import json
from pathlib import Path

import numpy as np
import torch
from PIL import Image
from tqdm import tqdm

from benchmark.models import get_registry

SCRIPTS_DIR = Path(__file__).resolve().parent

# script ID → benchmark registry slug
DEFAULT_MODEL_SLUGS = ["fashion-clip", "efficientnet-b0", "dinov2-vits14", "clip-vit-b16"]


def main() -> None:
    parser = argparse.ArgumentParser(description="Generate image embeddings via benchmark.models.REGISTRY")
    parser.add_argument("--output", type=Path, default=SCRIPTS_DIR / "output")
    parser.add_argument("--models", nargs="+", default=DEFAULT_MODEL_SLUGS)
    args = parser.parse_args()

    images_json = args.output / "demo_variant_images.json"
    if not images_json.exists():
        print(f"ERROR: {images_json} not found; run extract_products.py first"); return

    records = json.loads(images_json.read_text())
    search_records = [r for r in records if r.get("type") == "Search"]
    if not search_records:
        print("No search images found. Skipping."); return

    print(f"Generating embeddings for {len(search_records)} search images")
    print(f"Models: {args.models}")

    all_embeddings: list[dict] = []
    registry = get_registry(device="cpu")

    for slug in args.models:
        print(f"\n--- Loading model: {slug} ---")
        try:
            model = registry[slug]
            model.ensure_loaded()
        except Exception as e:
            print(f"  WARN: Cannot load {slug}: {e}. Skipping."); continue

        dim = model.embedding_dim
        print(f"  dim={dim}, name={model.name}")

        for rec in tqdm(search_records, desc=f"  {slug}"):
            img_path = args.output / rec["storage_path"]
            if not img_path.exists():
                continue
            try:
                img = Image.open(img_path).convert("RGB")
                vec = model.embed(img)
                all_embeddings.append({
                    "variant_image_id": rec["id"],
                    "model_name": slug,
                    "model_version": slug,
                    "vector": vec.tolist(),
                    "dimensions": dim,
                })
            except Exception as e:
                print(f"  WARN: {rec['storage_path']}: {e}"); continue

        if torch.cuda.is_available():
            torch.cuda.empty_cache()

    (args.output / "demo_embeddings.json").write_text(json.dumps(all_embeddings, indent=2))
    print(f"\nWritten {len(all_embeddings)} embeddings for {len(search_records)} images x {args.models}")


if __name__ == "__main__":
    main()
