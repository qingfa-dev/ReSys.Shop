#!/usr/bin/env python
"""Download the Kaggle Fashion Product Images datasets for benchmarking.

Uses ``kagglehub`` to download from Kaggle (requires API token in
``~/.kaggle/access_token`` or the ``KAGGLE_API_TOKEN`` env var).

Datasets
--------
- small  — paramaggarwal/fashion-product-images-small (44K images + styles.csv)
- full   — paramaggarwal/fashion-product-images-dataset (44K images + per-product JSONs)
- synthetic — locally-generated mini-dataset (no download, for CI/smoke tests)

Usage::

    # Default: small dataset
    uv run python scripts/01_download_dataset.py

    # Full dataset (~23 GB)
    uv run python scripts/01_download_dataset.py --dataset full

    # Synthetic mini-dataset
    uv run python scripts/01_download_dataset.py --dataset synthetic --n 500
"""
from __future__ import annotations

import argparse
import csv
import random
import sys
from pathlib import Path

from PIL import Image, ImageDraw


_KAGGLE_SLUGS = {
    "small": "paramaggarwal/fashion-product-images-small",
    "full": "paramaggarwal/fashion-product-images-dataset",
}


def _download_kagglehub(slug: str) -> Path:
    """Download a Kaggle dataset via kagglehub and return its cache path."""
    try:
        import kagglehub
    except ImportError:
        print("kagglehub not installed. Run: uv add kagglehub")
        raise SystemExit(1)

    print(f"Downloading {slug} via kagglehub …")
    path = Path(kagglehub.dataset_download(slug))
    print(f"  cached at {path}")
    return path


def _link_or_copy(src: Path, dst: Path) -> None:
    """Link src to dst. Fall back to copy if cross-device."""
    dst.unlink(missing_ok=True)
    try:
        dst.symlink_to(src, target_is_directory=True)
        print(f"  symlinked {dst} → {src}")
    except OSError:
        import shutil
        shutil.copytree(src, dst, dirs_exist_ok=True)
        print(f"  copied {src} → {dst}")


def _resolve_cache_dir(cache_path: Path) -> Path:
    """Resolve the actual data directory inside a kagglehub cache path.

    The small dataset stores files directly in ``versions/1/``, while the
    full dataset wraps them in a ``fashion-dataset/`` subdirectory.
    """
    candidate = cache_path / "fashion-dataset"
    if candidate.is_dir():
        return candidate
    return cache_path


def create_symlink_output(cache_path: Path, output_root: Path, name: str) -> Path:
    """Create a symlink from cache to output_root/name, returns output path."""
    src = _resolve_cache_dir(cache_path)
    dest = output_root / name
    dest.parent.mkdir(parents=True, exist_ok=True)
    _link_or_copy(src, dest)
    return dest


def validate_layout(dataset_root: Path) -> bool:
    """Check that styles.csv and images/ are present and plausible."""
    ok = True
    csv_path = dataset_root / "styles.csv"
    img_dir  = dataset_root / "images"

    # The full dataset also provides styles/ (per-product JSONs) — optional
    styles_dir = dataset_root / "styles"
    if styles_dir.is_dir():
        n_jsons = len(list(styles_dir.glob("*.json")))
        print(f"✓  styles/ — {n_jsons:,} JSON files")
    else:
        print(f"–  styles/ not found (expected only for full dataset)")

    if not csv_path.exists():
        print(f"✗  styles.csv not found at {csv_path}")
        ok = False
    else:
        with csv_path.open(encoding="utf-8", errors="replace") as fh:
            reader = csv.DictReader(fh)
            rows = sum(1 for _ in reader)
        print(f"✓  styles.csv — {rows:,} products")

    if not img_dir.exists():
        print(f"✗  images/ directory not found at {img_dir}")
        ok = False
    else:
        n_imgs = len(list(img_dir.glob("*.jpg")))
        print(f"✓  images/ — {n_imgs:,} JPEG files")

    return ok


# ── Synthetic dataset (CI smoke tests) ───────────────────────────────────────
_ARTICLE_TYPES = [
    "Tshirts", "Shirts", "Jeans", "Trousers", "Casual Shoes",
    "Watches", "Sports Shoes", "Kurtas", "Tops", "Handbags",
    "Heels", "Sunglasses", "Flats", "Sandals", "Dresses",
]
_MASTER_CATEGORIES = {
    "Tshirts": "Apparel",  "Shirts": "Apparel",  "Jeans": "Apparel",
    "Trousers": "Apparel", "Kurtas": "Apparel",  "Tops": "Apparel",
    "Dresses": "Apparel",  "Casual Shoes": "Footwear",
    "Sports Shoes": "Footwear", "Heels": "Footwear",
    "Flats": "Footwear",   "Sandals": "Footwear",
    "Watches": "Accessories", "Handbags": "Accessories",
    "Sunglasses": "Accessories",
}
_SUB_CATEGORIES = {
    "Tshirts": "Topwear",  "Shirts": "Topwear",  "Tops": "Topwear",
    "Kurtas": "Topwear",   "Jeans": "Bottomwear", "Trousers": "Bottomwear",
    "Dresses": "Dress",    "Casual Shoes": "Shoes",
    "Sports Shoes": "Shoes", "Heels": "Shoes",
    "Flats": "Shoes",      "Sandals": "Shoes",
    "Watches": "Watches",  "Handbags": "Bags",
    "Sunglasses": "Eyewear",
}
_GENDERS   = ["Men", "Women", "Boys", "Girls", "Unisex"]
_COLOURS   = ["Black", "White", "Blue", "Red", "Green", "Yellow", "Grey", "Brown"]
_SEASONS   = ["Summer", "Winter", "Fall", "Spring"]
_USAGES    = ["Casual", "Formal", "Sports", "Ethnic"]


def generate_synthetic(output_root: Path, n: int = 500, seed: int = 42) -> Path:
    """Generate a synthetic fashion-dataset with the same layout as the Kaggle data.

    Args:
        output_root: Parent directory; dataset created at output_root/fashion-dataset/
        n:           Number of products to generate.
        seed:        RNG seed for reproducibility.

    Returns:
        Path to the generated dataset root.
    """
    rng = random.Random(seed)
    dest = output_root / "synthetic-dataset"
    img_dir = dest / "images"
    img_dir.mkdir(parents=True, exist_ok=True)

    rows: list[dict] = []
    for i in range(n):
        pid        = 10000 + i
        article    = _ARTICLE_TYPES[i % len(_ARTICLE_TYPES)]
        master     = _MASTER_CATEGORIES[article]
        sub        = _SUB_CATEGORIES[article]
        gender     = rng.choice(_GENDERS)
        colour     = rng.choice(_COLOURS)
        season     = rng.choice(_SEASONS)
        usage      = rng.choice(_USAGES)
        year       = rng.randint(2010, 2020)
        name       = f"{colour} {article} for {gender}"

        hue = (i * 37) % 360
        r = int(128 + 127 * __import__("math").sin(hue * 3.14159 / 180))
        g = int(128 + 127 * __import__("math").sin((hue + 120) * 3.14159 / 180))
        b = int(128 + 127 * __import__("math").sin((hue + 240) * 3.14159 / 180))
        img = Image.new("RGB", (224, 224), color=(r, g, b))
        draw = ImageDraw.Draw(img)
        draw.rectangle([40, 60, 184, 164], fill=(r // 2, g // 2, b // 2))
        draw.text((10, 10), article[:12], fill=(255, 255, 255))
        img.save(img_dir / f"{pid}.jpg", "JPEG", quality=85)

        rows.append({
            "id": pid,
            "gender": gender,
            "masterCategory": master,
            "subCategory": sub,
            "articleType": article,
            "baseColour": colour,
            "season": season,
            "year": year,
            "usage": usage,
            "productDisplayName": name,
        })

    csv_path = dest / "styles.csv"
    with csv_path.open("w", newline="", encoding="utf-8") as fh:
        writer = csv.DictWriter(fh, fieldnames=list(rows[0].keys()))
        writer.writeheader()
        writer.writerows(rows)

    print(f"Synthetic dataset written to {dest}")
    print(f"  {n} products across {len(_ARTICLE_TYPES)} article types")
    print(f"  images/ : {n} JPEG files")
    print(f"  styles.csv : {n} rows")
    return dest


# ── entry point ───────────────────────────────────────────────────────────────

def main() -> None:
    parser = argparse.ArgumentParser(
        description="Download Kaggle Fashion Product Images datasets via kagglehub",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=__doc__,
    )
    parser.add_argument(
        "--dataset", choices=("small", "full", "synthetic"), default="small",
        help="Dataset to download (default: small).",
    )
    parser.add_argument(
        "--output", type=Path, default=Path("data/raw"), metavar="DIR",
        help="Parent directory for the dataset (default: data/raw).",
    )
    parser.add_argument(
        "--n", type=int, default=500,
        help="Number of products to generate (synthetic only, default 500).",
    )
    parser.add_argument(
        "--seed", type=int, default=42,
        help="RNG seed for synthetic generation (default 42).",
    )
    args = parser.parse_args()

    output_root = args.output.resolve()

    if args.dataset == "synthetic":
        dest = generate_synthetic(output_root, n=args.n, seed=args.seed)
        print("\nValidating …")
        validate_layout(dest)
        print(f"\nRun: uv run benchmark run --dataset-root {dest}")
        return

    slug = _KAGGLE_SLUGS[args.dataset]
    name = slug.split("/")[-1]

    cache_path = _download_kagglehub(slug)
    dest = create_symlink_output(cache_path, output_root, name)

    print("\nValidating …")
    ok = validate_layout(dest)
    if not ok:
        print("\nDownload may be incomplete. Re-run the script.")
        raise SystemExit(1)

    print(f"\nDataset ready at {dest}")
    print(f"  Run: uv run benchmark run --dataset-root {dest}")


if __name__ == "__main__":
    main()
