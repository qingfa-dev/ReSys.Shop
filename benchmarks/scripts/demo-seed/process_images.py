#!/usr/bin/env python
"""Scale product images to medium (512px) and search (224px) sizes."""
from __future__ import annotations

import argparse
import json
from pathlib import Path

from PIL import Image
from tqdm import tqdm


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
    parser.add_argument("--storage", type=Path, required=True, help="Path to infra/Storage/demo")
    parser.add_argument("--json-dir", type=Path, required=True, help="Directory with demo_variant_images.json")
    args = parser.parse_args()

    images_json = args.json_dir / "demo_variant_images.json"
    if not images_json.exists():
        print(f"ERROR: {images_json} not found; run extract_products.py first"); return

    image_records = json.loads(images_json.read_text())
    # Deduplicate by storage path
    seen: set[str] = set()
    unique: list[dict] = []
    for rec in image_records:
        sp = rec["storage_path"]
        if sp not in seen:
            seen.add(sp)
            unique.append(rec)

    source_dir = args.dataset / "images"
    medium_dir = args.storage / "images" / "medium"
    search_dir_224 = args.storage / "images" / "search" / "224"

    ok = fail = 0
    for rec in tqdm(unique, desc="Processing images"):
        fname = rec["file_name"]
        src = source_dir / fname
        if not src.exists():
            print(f"  WARN: {src} not found, skipping")
            fail += 1
            continue

        if "medium" in rec["storage_path"]:
            dst = args.storage / rec["storage_path"]
            if resize_image(src, dst, 512):
                ok += 1
            else:
                fail += 1
        elif "search" in rec["storage_path"]:
            dst = args.storage / rec["storage_path"]
            if resize_image(src, dst, 224):
                ok += 1
            else:
                fail += 1

    print(f"Done: {ok} images processed, {fail} failures")


if __name__ == "__main__":
    main()
