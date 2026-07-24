#!/usr/bin/env python
"""Prepare the Kaggle Fashion Product Images (Small) dataset for benchmarking.

The dataset is ~280 MB and must be downloaded manually from Kaggle
(requires a free Kaggle account):

    https://www.kaggle.com/datasets/paramaggarwal/fashion-product-images-small

After downloading, you will have a zip file. Run this script to extract it
and validate the layout:

    uv run python scripts/01_download_dataset.py --source ~/Downloads/archive.zip

Expected layout after extraction
---------------------------------
data/raw/fashion-dataset/
├── images/           # 44 k JPEG files named {product_id}.jpg
└── styles.csv        # product metadata (id, gender, masterCategory, …)

Columns in styles.csv
---------------------
id, gender, masterCategory, subCategory, articleType,
baseColour, season, year, usage, productDisplayName

Alternatively, use the Kaggle CLI (pip install kaggle):

    kaggle datasets download -d paramaggarwal/fashion-product-images-small
    uv run python scripts/01_download_dataset.py --source fashion-product-images-small.zip

Synthetic dataset (for CI / smoke tests without the real data)
--------------------------------------------------------------
    uv run python scripts/01_download_dataset.py --synthetic --n 500
"""
from __future__ import annotations

import argparse
import csv
import random
import zipfile
from io import StringIO
from pathlib import Path

from PIL import Image, ImageDraw


# ── Kaggle dataset preparation ────────────────────────────────────────────────

def extract_kaggle_zip(source: Path, output_root: Path) -> Path:
    """Extract the Kaggle zip to output_root/fashion-dataset/.

    Returns:
        Path to the extracted dataset root.
    """
    dest = output_root / "fashion-dataset"
    dest.mkdir(parents=True, exist_ok=True)

    print(f"Extracting {source} → {dest} …")
    with zipfile.ZipFile(source, "r") as zf:
        members = zf.namelist()
        total = len(members)
        for i, member in enumerate(members, 1):
            if i % 5000 == 0 or i == total:
                print(f"  {i}/{total} files extracted …", end="\r")
            zf.extract(member, dest)
    print(f"\nDone — {total} files extracted.")
    return dest


def validate_layout(dataset_root: Path) -> bool:
    """Check that styles.csv and images/ are present and plausible."""
    ok = True
    csv_path = dataset_root / "styles.csv"
    img_dir  = dataset_root / "images"

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

# Real article types from the Kaggle dataset — used to make synthetic data realistic
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

    The generated dataset has:
    - ``images/{id}.jpg``  — small coloured rectangles (224×224)
    - ``styles.csv``       — same columns as the real Kaggle CSV

    Args:
        output_root: Parent directory; dataset created at output_root/fashion-dataset/
        n:           Number of products to generate.
        seed:        RNG seed for reproducibility.

    Returns:
        Path to the generated dataset root.
    """
    rng = random.Random(seed)
    dest = output_root / "fashion-dataset"
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

        # Generate a coloured thumbnail
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
        description="Prepare the Kaggle Fashion Product Images (Small) dataset",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=__doc__,
    )
    parser.add_argument(
        "--source", type=Path, metavar="ZIP",
        help="Path to the downloaded archive.zip from Kaggle.",
    )
    parser.add_argument(
        "--output", type=Path, default=Path("data/raw"), metavar="DIR",
        help="Parent directory for the extracted dataset (default: data/raw).",
    )
    parser.add_argument(
        "--synthetic", action="store_true",
        help="Generate a synthetic mini-dataset instead of extracting a real one.",
    )
    parser.add_argument(
        "--n", type=int, default=500,
        help="Number of products to generate (--synthetic only, default 500).",
    )
    parser.add_argument(
        "--seed", type=int, default=42,
        help="RNG seed for synthetic generation (default 42).",
    )
    args = parser.parse_args()

    if args.synthetic:
        dest = generate_synthetic(args.output, n=args.n, seed=args.seed)
        print("\nValidating …")
        validate_layout(dest)
        print("\nRun the benchmark with:")
        print(f"  uv run benchmark benchmark --dataset-root {dest}")
        return

    if args.source is None:
        print(
            "Provide --source to extract a Kaggle zip, or --synthetic to generate test data.\n"
            "Download from:\n"
            "  https://www.kaggle.com/datasets/paramaggarwal/fashion-product-images-small\n"
            "Or with the Kaggle CLI:\n"
            "  kaggle datasets download -d paramaggarwal/fashion-product-images-small"
        )
        raise SystemExit(1)

    dest = extract_kaggle_zip(args.source, args.output)
    print("\nValidating extracted layout …")
    ok = validate_layout(dest)
    if ok:
        print("\nDataset ready. Run the benchmark with:")
        print(f"  uv run benchmark benchmark --dataset-root {dest}")
    else:
        print("\nExtraction may be incomplete. Re-download and try again.")
        raise SystemExit(1)


if __name__ == "__main__":
    main()
