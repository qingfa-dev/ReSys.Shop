#!/usr/bin/env python
"""Scale product images to medium (512px) and search (224px) sizes."""
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

from PIL import Image
from tqdm import tqdm

MODEL_INPUT_SIZES: dict[str, int] = {
    "efficientnet_b0": 224, "clip_vit_b16": 224, "fashion_clip": 224,
    "dinov2_vits14": 224,
}

SCRIPTS_DIR = Path(__file__).resolve().parent


def resize_image(src: Path, dst: Path, size: int) -> bool:
    try:
        img = Image.open(src).convert("RGB")
        img.thumbnail((size, size), Image.LANCZOS)
        dst.parent.mkdir(parents=True, exist_ok=True)
        img.save(dst, "JPEG", quality=85, optimize=True)
        return True
    except Exception as e:
        print(f"  WARN: Cannot process {src.name}: {e}")
        return False


def main() -> None:
    parser = argparse.ArgumentParser(description="Scale product images for demo")
    parser.add_argument("--dataset", type=Path, required=True, help="Path to fashion-product-images directory")
    parser.add_argument("--output", type=Path, default=SCRIPTS_DIR / "output", help="Output directory")
    args = parser.parse_args()

    images_json = args.output / "demo_variant_images.json"
    if not images_json.exists():
        print(f"ERROR: {images_json} not found; run extract_products.py first"); sys.exit(1)

    image_records = json.loads(images_json.read_text())
    seen: set[str] = set()
    unique: list[dict] = []
    for rec in image_records:
        sp = rec["storage_path"]
        if sp not in seen:
            seen.add(sp)
            unique.append(rec)

    source_dir = args.dataset / "images"
    medium_dir = args.output / "images" / "medium"

    sizes = set(MODEL_INPUT_SIZES.values())
    for size in sizes:
        search_dir = args.output / "images" / "search" / str(size)
        search_dir.mkdir(parents=True, exist_ok=True)

    ok = fail = 0
    for rec in tqdm(unique, desc="Processing images"):
        fname = rec["file_name"]
        src = source_dir / fname
        if not src.exists():
            print(f"  WARN: {src} not found, skipping")
            fail += 1
            continue

        if "medium" in rec["storage_path"]:
            dst = args.output / rec["storage_path"]
            if resize_image(src, dst, 512):
                ok += 1
            else:
                fail += 1
        elif "search" in rec["storage_path"]:
            for size in sizes:
                dst = args.output / "images" / "search" / str(size) / fname
                if resize_image(src, dst, size):
                    ok += 1
                else:
                    fail += 1
                    break

    print(f"Done: {ok} images processed, {fail} failures")


if __name__ == "__main__":
    main()
