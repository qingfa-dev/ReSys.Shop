# 05 — Datasets

Everything about the datasets used in the benchmark.

## Overview

We use two variants of the **Fashion Product Images** dataset from Kaggle:

| Dataset | Images | Size | Purpose |
|---------|--------|------|---------|
| **Fashion Product Images Small** | ~5,000 | ~600 MB | Benchmark experiments |
| **Fashion Product Images (Full)** | ~44,000 | ~20 GB | Demo seeding, production data |

Both come from the same source: [Kaggle — Fashion Product Images Dataset](https://www.kaggle.com/datasets/paramaggarwal/fashion-product-images-dataset)

---

## Fashion Product Images Small (~5,000 images)

### What It Is
A curated subset of the full dataset with smaller image files (lower resolution). Perfect for experimentation and the thesis benchmark.

### Directory Structure
```
fashion-product-images-small/
├── images/                    # Product photos (JPEG)
│   ├── 1163.jpg
│   ├── 1165.jpg
│   └── ... (~5,000 files)
└── styles.csv                 # Metadata for all products
```

### The `styles.csv` File

This is the most important file. It contains metadata for every image.

**Key columns:**

| Column | Example | Description |
|--------|---------|-------------|
| `id` | `1163` | Unique product ID (matches image filename) |
| `gender` | `Men` | Target gender |
| `masterCategory` | `Apparel` | Broad category |
| `subCategory` | `Topwear` | Medium category |
| `articleType` | `T-shirts` | Specific item type |
| `baseColour` | `Black` | Dominant color |
| `season` | `Summer` | Fashion season |
| `year` | `2012` | Release year |
| `usage` | `Casual` | Occasion/style |
| `productDisplayName` | `Black T-shirt` | Human-readable name |

**Sample rows:**

```csv
id,gender,masterCategory,subCategory,articleType,baseColour,season,year,usage,productDisplayName
1163,Men,Apparel,Topwear,T-shirts,Black,Summer,2011,Casual,Turtle Check Men Navy Blue Shirt
1165,Men,Apparel,Topwear,T-shirts,Blue,Summer,2011,Casual,Peter England Men Party Blue Shirt
1404,Men,Footwear,Shoes,Casual Shoes,Black,Summer,2013,Casual,Gas Men Athleisure Black Shoe
```

### How We Use It for Ground Truth

Two images are **relevant** (visually similar) if they share:
- Same `masterCategory` AND
- Same `subCategory` AND
- Same `baseColour`

This ensures the benchmark measures visual similarity, not just taxonomic category membership. Two T-shirts of the same colour are relevant; a black T-shirt and a blue T-shirt are not.

**Example:**
- Image 1163: `Apparel` + `Topwear` + `Black` → Black T-shirt
- Image 1165: `Apparel` + `Topwear` + `Black` → Black T-shirt
- **Result:** Relevant to each other

- Image 1400: `Apparel` + `Topwear` + `Blue` → Blue T-shirt
- **Result:** NOT relevant to 1163 (different colour)

- Image 1404: `Footwear` + `Shoes` + `Black` → Black Shoe
- **Result:** NOT relevant to 1163 (different category)

**Fallback:** If `subCategory` or `baseColour` is missing, fall back to the coarser grouping.

### Category Distribution

The dataset is imbalanced. Some categories have thousands of images, others have only a few:

| masterCategory | Count | % |
|---------------|-------|---|
| Apparel | ~2,500 | ~50% |
| Footwear | ~800 | ~16% |
| Accessories | ~1,000 | ~20% |
| Personal Care | ~400 | ~8% |
| ... | ... | ... |

**Why this matters:** When splitting into folds, we use **stratification** to ensure each fold has the same category proportions.

### Download

```bash
# Via Kaggle CLI (requires kaggle.json API key)
kaggle datasets download -d paramaggarwal/fashion-product-images-small

# Or download manually from Kaggle website
# Extract to: data/raw/fashion-product-images-small/
```

---

## Fashion Product Images (Full ~44,000 images)

### What It Is
The complete dataset with full-resolution images. Used for:
- Populating the demo shop with realistic products
- Stress-testing the embedding service at scale
- Future production data

### Directory Structure
```
fashion-product-images/
├── images/                    # Full-resolution product photos
│   └── ... (~44,000 files)
└── styles.csv                 # Metadata (same format as small)
```

### Differences from Small

| Aspect | Small | Full |
|--------|-------|------|
| Image count | ~5,000 | ~44,000 |
| Image resolution | Lower (~300×400) | Higher (~800×1200) |
| Total size | ~600 MB | ~20 GB |
| Use case | Benchmark | Demo / Production |

### Preparing for Demo Seeding

To use the full dataset for the shop demo, you need to:

1. **Resize images** to web-friendly sizes (e.g., 400×600)
2. **Generate a seeder CSV** with columns the shop expects
3. **Upload to the shop's database**

**Example seeder script logic:**
```python
import pandas as pd
from pathlib import Path
from PIL import Image

styles = pd.read_csv("fashion-product-images/styles.csv")

# Select a subset (e.g., first 5,000 for demo)
subset = styles.head(5000)

# Resize images and save to shop's storage
for _, row in subset.iterrows():
    img_path = Path(f"fashion-product-images/images/{row['id']}.jpg")
    if img_path.exists():
        img = Image.open(img_path)
        img.thumbnail((400, 600))
        img.save(f"shop-seed/images/{row['id']}.jpg")

# Generate seeder CSV
subset.to_csv("shop-seed/products.csv", index=False)
```

This is **not part of the benchmark** — it's a separate step for the demo.

---

## Dataset Format for the Benchmark

The benchmark expects a JSON split file:

```json
[
  {
    "image_path": "images/1163.jpg",
    "label": "Apparel/Topwear",
    "product_id": "1163"
  },
  {
    "image_path": "images/1165.jpg",
    "label": "Apparel/Topwear",
    "product_id": "1165"
  }
]
```

**Fields:**
- `image_path` — relative path from dataset root to the image
- `label` — ground-truth category label (used for relevance)
- `product_id` — unique identifier (matches `styles.csv` id)

**Generated automatically** by the benchmark from `styles.csv`.

---

## Dataset Preparation Checklist

Before running the benchmark:

- [ ] Download Fashion Product Images Small from Kaggle
- [ ] Extract to `data/raw/fashion-product-images-small/`
- [ ] Verify `styles.csv` exists in that directory
- [ ] Verify `images/` subdirectory contains ~5,000 `.jpg` files
- [ ] Run validation: `uv run benchmark benchmark --dataset-root ...` (it will check for missing images)

---

## Common Issues

### Missing Images
Some rows in `styles.csv` may reference images that don't exist. The benchmark skips these with a warning.

### Corrupted Images
Occasional JPEG files may be corrupted. The benchmark catches `OSError` and skips them.

### Wrong Path
Ensure `--dataset-root` points to the folder **containing** `images/` and `styles.csv`, not the `images/` folder itself.

### Encoding Issues
`styles.csv` may have encoding issues. The benchmark reads it with `encoding="utf-8"` and handles errors gracefully.

---

### Enriched Dataset (JSON Attributes)

To unlock visual attributes beyond the CSV (Pattern, Sleeve Length, Fabric), use:

```bash
uv run benchmark enrich \
    --json-styles data/raw/fashion-product-images/styles/ \
    --csv data/raw/fashion-product-images-small/styles.csv \
    --output data/raw/fashion-enriched-5k \
    --subset 5000
```

This produces `fashion-enriched-5k/` with `styles.csv` (enriched), `splits/`
(dual-label JSON), and an `images/` symlink.  See [11 — Enriched Dataset](11-enriched-dataset.md).

## Alternative Datasets

The benchmark can theoretically work with any dataset that provides:
1. Images in a folder
2. A metadata CSV with category labels
3. A way to define "relevance" (same category = similar)

Other fashion datasets you could try:
- **DeepFashion** (Liu et al., 2016) — 800,000 images, more complex
- **Fashion-MNIST** — 70,000 tiny 28×28 grayscale images (too small for realistic evaluation)
- **Street2Shop** — street photos matched to shop products (cross-domain retrieval)

For the thesis, stick with Fashion Product Images Small — it's the right size, well-documented, and category-based relevance is defensible.
