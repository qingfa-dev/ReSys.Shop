#!/usr/bin/env python
"""Scale product images to medium (512px) and search (224px) sizes."""
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

from PIL import Image
from tqdm import tqdm

sys.path.insert(0, str(Path(__file__).resolve().parent))
from json_io import write_json  # noqa: E402
from shared import MODEL_INPUT_SIZES, SCRIPTS_DIR  # noqa: E402


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


def parse_size(value: str) -> int:
    """Parse '512' or 'model:fashion_clip' into pixel size."""
    if value.startswith("model:"):
        model_id = value.split(":", 1)[1]
        if model_id not in MODEL_INPUT_SIZES:
            print(f"ERROR: Unknown model '{model_id}'. Known: {list(MODEL_INPUT_SIZES.keys())}")
            sys.exit(1)
        return MODEL_INPUT_SIZES[model_id]
    try:
        return int(value)
    except ValueError:
        print(f"ERROR: Invalid size '{value}'. Use integer pixels or 'model:<id>'.")
        sys.exit(1)


def main() -> None:
    parser = argparse.ArgumentParser(description="Scale product images for demo")
    parser.add_argument("--dataset", type=Path, required=True, help="Path to fashion-product-images directory")
    parser.add_argument("--output", type=Path, default=SCRIPTS_DIR / "output", help="Output directory")
    parser.add_argument("--display-size", default="512", help="Display image size (px) or model:<id>")
    parser.add_argument("--search-size", default="224", help="Search image size (px) or model:<id>")
    args = parser.parse_args()

    display_size = parse_size(args.display_size)
    search_size = parse_size(args.search_size)
    print(f"Display size: {display_size}px | Search size: {search_size}px")

    images_json = args.output / "007_demo_variant_images.json"
    if not images_json.exists():
        print(f"ERROR: {images_json} not found; run 07_extract_variant_images.py first")
        sys.exit(1)

    image_records = json.loads(images_json.read_text())
    seen: dict[str, str] = {}
    unique: list[dict] = []
    for rec in image_records:
        sp = rec["storage_path"]
        if sp not in seen:
            seen[sp] = rec["file_name"]
            unique.append(rec)
        elif seen[sp] != rec["file_name"]:
            print(f"  WARN: {sp} has conflicting file_names: {seen[sp]} vs {rec['file_name']}")

    source_dir = args.dataset / "images"
    ok = fail = 0

    medium_imgs = [r for r in unique if "images/medium/" in r["storage_path"]]
    print(f"\n--- Processing {len(medium_imgs)} display images at {display_size}px ---")
    dims_by_storage: dict[str, tuple[int, int, int]] = {}
    for rec in tqdm(medium_imgs, desc="Display"):
        src = source_dir / rec["file_name"]
        if not src.exists():
            print(f"  WARN: {src} not found, skipping")
            fail += 1
            continue
        dst = args.output / rec["storage_path"]
        if resize_image(src, dst, display_size):
            ok += 1
            with Image.open(dst) as img:
                dims_by_storage[rec["storage_path"]] = (img.width, img.height, dst.stat().st_size)
        else:
            fail += 1

    search_imgs = [r for r in unique if "images/search/" in r["storage_path"]]
    print(f"\n--- Processing {len(search_imgs)} search images at {search_size}px ---")
    for rec in tqdm(search_imgs, desc="Search"):
        src = source_dir / rec["file_name"]
        if not src.exists():
            print(f"  WARN: {src} not found, skipping")
            fail += 1
            continue
        dst = args.output / rec["storage_path"]
        if resize_image(src, dst, search_size):
            ok += 1
            with Image.open(dst) as img:
                dims_by_storage[rec["storage_path"]] = (img.width, img.height, dst.stat().st_size)
        else:
            fail += 1

    if dims_by_storage:
        for rec in image_records:
            dims = dims_by_storage.get(rec["storage_path"])
            if dims is not None:
                rec["width"], rec["height"], rec["file_size"] = dims
        write_json(images_json, image_records)
        print(f"Backfilled width/height/file_size into {len(dims_by_storage)} unique images in {images_json}")


if __name__ == "__main__":
    main()
