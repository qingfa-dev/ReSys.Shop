# 11 — Enriched Dataset

Complete guide for enriching the benchmark dataset with visual attributes
from per-product JSON metadata.

## Prerequisites

Download the **full** Fashion Product Images dataset from Kaggle
([paramaggarwal/fashion-product-images-dataset](https://www.kaggle.com/datasets/paramaggarwal/fashion-product-images-dataset)).
Extract it so you have:

```
data/raw/fashion-product-images/
├── images/          # 44K+ JPEG product photos
└── styles/          # 44K+ per-product JSON files
    ├── 10000.json
    ├── 10001.json
    └── ...
```

You also need the small CSV dataset (`fashion-product-images-small/styles.csv`).

## Enrichment

```bash
uv run benchmark enrich \
    --json-styles data/raw/fashion-product-images/styles/ \
    --csv data/raw/fashion-product-images-small/styles.csv \
    --output data/raw/fashion-enriched-5k \
    --subset 5000
```

## Output Structure

```
fashion-enriched-5k/
├── styles.csv              # Enriched: id, masterCategory, …, pattern
├── images/                 # → symlink to original images
└── splits/
    ├── fold_0_test.json     # { "label": "Topwear/Blue",
    │                          "label_pattern": "Topwear/Blue/Checked" }
    ├── fold_0_train.json
    ├── ...
```

## Running Dual-Label Evaluation

```bash
uv run benchmark thesis \
    --dataset-root data/raw/fashion-enriched-5k \
    --secondary-label label_pattern \
    --folds 3 --seed 42 --device cpu
```

Produces:
- `outputs/thesis/results/thesis_results.json` — primary (category + colour)
- `outputs/thesis/results/thesis_results_pattern.json` — secondary (category + colour + pattern)

## Results (5K Subset, Same Folds, Same Embeddings)

| Model | 1. Cat-only | 2. + Colour | 3. + Colour+Pattern |
|-------|------------|-------------|---------------------|
| FashionCLIP | 0.931 ± 0.007 | 0.245 ± 0.004 | 0.215 ± 0.008 |
| CLIP-generic | 0.912 ± 0.008 | 0.231 ± 0.006 | 0.201 ± 0.007 |
| EfficientNet-B0 | 0.890 ± 0.006 | 0.220 ± 0.006 | 0.192 ± 0.004 |
| ResNet-50 | 0.886 ± 0.011 | 0.209 ± 0.004 | 0.186 ± 0.007 |

**Rankings are stable across all three schemes.** The 3.7× mAP drop from
category-only to category+colour confirms the original benchmark measured
category classification, not visual similarity.

Result files:
- `outputs/thesis/results/thesis_results_category_only.json`
- `outputs/thesis/results/thesis_results.json`
- `outputs/thesis/results/thesis_results_pattern.json`

## Extracting Other Attributes

The enrichment script extracts `articleAttributes.Pattern` by default.
To add Sleeve Length, Fabric, or Fit in the future, modify
`scripts/enrich_dataset.py::extract_pattern()` to also extract those keys.
Each new attribute adds a new `label_<attr>` field to the split JSON.
