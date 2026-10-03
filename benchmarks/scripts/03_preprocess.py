#!/usr/bin/env python
"""Pre-process raw dataset images (resize, validate, deduplicate).

Usage::

    uv run python scripts/03_preprocess.py --dataset data/raw/deepfashion
"""
from __future__ import annotations

import argparse
from pathlib import Path

from PIL import Image
from tqdm import tqdm


def resize_images(dataset_root: Path, target_size: int = 256) -> None:
    """Resize all JPEG images in dataset_root to target_size × target_size."""
    images = list(dataset_root.rglob("*.jpg")) + list(dataset_root.rglob("*.jpeg"))
    print(f"Resizing {len(images)} images to {target_size}px …")

    errors: list[str] = []
    for path in tqdm(images):
        try:
            with Image.open(path) as img:
                img = img.convert("RGB")
                img = img.resize((target_size, target_size), Image.LANCZOS)
                img.save(path, "JPEG", quality=95)
        except Exception as exc:
            errors.append(f"{path}: {exc}")

    if errors:
        print(f"\n⚠ {len(errors)} errors:")
        for e in errors[:10]:
            print(f"  {e}")
    else:
        print("Done.")


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--dataset", type=Path, required=True)
    parser.add_argument("--size", type=int, default=256)
    args = parser.parse_args()
    resize_images(args.dataset, target_size=args.size)


if __name__ == "__main__":
    main()
