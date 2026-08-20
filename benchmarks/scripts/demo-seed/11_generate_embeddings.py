#!/usr/bin/env python
"""Generate image embeddings using the benchmark model registry.

Writes per-model JSON files to 012_demo_embeddings/{model_name}.json.

Modes:
  --demo       6 thesis models only (fashion_clip, clip_b32, clip_vit_b16,
               dinov2_vits14, efficientnet_b0, resnet50)
  (default)    all 11 models from the benchmark registry
"""
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

import torch
from PIL import Image
from tqdm import tqdm

from benchmark.models import get_registry

sys.path.insert(0, str(Path(__file__).resolve().parent))
from shared import SCRIPTS_DIR  # noqa: E402

# 6 thesis models — matches VariantImageConstant.AIModels
DEMO_MODEL_SLUGS = [
    "fashion_clip",
    "clip_b32",
    "clip_vit_b16",
    "dinov2_vits14",
    "efficientnet_b0",
    "resnet50",
]

# Expected dimensions per model (from architecture output)
EXPECTED_DIMS: dict[str, int] = {
    "fashion_clip": 512,
    "clip_b32": 512,
    "clip_vit_b16": 512,
    "dinov2_vits14": 384,
    "efficientnet_b0": 1280,
    "resnet50": 2048,
}

# script ID → benchmark registry slug
DEFAULT_MODEL_SLUGS = list(get_registry(device="cpu").keys())


def main() -> None:
    parser = argparse.ArgumentParser(description="Generate image embeddings via benchmark.models.REGISTRY")
    parser.add_argument("--output", type=Path, default=SCRIPTS_DIR / "output")
    parser.add_argument("--models", nargs="+", default=None,
                        help="Explicit model slugs. Ignored when --demo is set.")
    parser.add_argument("--demo", action="store_true",
                        help="Use the 6 thesis models only (default: all 11 registry models).")
    args = parser.parse_args()

    # Resolve model list: --demo wins, then --models, then defaults
    if args.demo:
        model_slugs = DEMO_MODEL_SLUGS
    elif args.models is not None:
        model_slugs = args.models
    else:
        model_slugs = DEFAULT_MODEL_SLUGS

    images_json = args.output / "007_demo_variant_images.json"
    if not images_json.exists():
        print(f"ERROR: {images_json} not found; run 07_extract_variant_images.py first")
        return

    records = json.loads(images_json.read_text())
    search_records = [r for r in records if r.get("type") == "Search"]
    if not search_records:
        print("No search images found. Skipping.")
        return

    print(f"Generating embeddings for {len(search_records)} search images")
    print(f"Models: {model_slugs}")

    out_dir = args.output / "012_demo_embeddings"
    out_dir.mkdir(parents=True, exist_ok=True)

    registry = get_registry(device="cpu")

    for slug in model_slugs:
        out_file = out_dir / f"{slug}.json"

        # Resume: skip if per-model file already exists with correct count
        if out_file.exists():
            existing = json.loads(out_file.read_text())
            if len(existing) == len(search_records):
                print(f"\n--- {slug}: already done ({len(existing)} embeddings). Skipping. ---")
                continue
            print(f"\n--- {slug}: partial file ({len(existing)}/{len(search_records)}). Re-generating. ---")

        print(f"\n--- Loading model: {slug} ---")
        try:
            model = registry[slug]
            model.ensure_loaded()
        except Exception as e:
            print(f"  WARN: Cannot load {slug}: {e}. Skipping.")
            continue

        dim = model.embedding_dim
        expected = EXPECTED_DIMS.get(slug)

        # Validate: model's actual output dimension must match expected
        if expected and dim != expected:
            print(f"  ERROR: {slug} model reports dim={dim} but expected {expected}. Skipping.")
            continue

        print(f"  dim={dim}, name={model.name}")

        embeddings: list[dict] = []
        for rec in tqdm(search_records, desc=f"  {slug}"):
            img_path = args.output / rec["storage_path"]
            if not img_path.exists():
                continue
            try:
                img = Image.open(img_path).convert("RGB")
                vec = model.embed(img)

                # Validate: actual vector length must match model's reported dimension
                vec_len = len(vec)
                if vec_len != dim:
                    print(f"  WARN: {rec['storage_path']}: vector length {vec_len} != model dim {dim}. Skipping entry.")
                    continue

                embeddings.append({
                    "variant_image_id": rec["id"],
                    "model_name": slug,
                    "model_version": slug,
                    "vector": vec.tolist(),
                    "dimensions": dim,
                })
            except Exception as e:
                print(f"  WARN: {rec['storage_path']}: {e}")
                continue

        # Write per-model file
        out_file.write_text(json.dumps(embeddings, indent=2))
        print(f"  Written {len(embeddings)} embeddings to {out_file.name}")

        if torch.cuda.is_available():
            torch.cuda.empty_cache()

    # Summary
    print("\n=== Done ===")
    for slug in model_slugs:
        out_file = out_dir / f"{slug}.json"
        if out_file.exists():
            data = json.loads(out_file.read_text())
            dims = set(e["dimensions"] for e in data)
            print(f"  {slug}: {len(data)} embeddings, dim={dims}")
        else:
            print(f"  {slug}: FAILED")


if __name__ == "__main__":
    main()
