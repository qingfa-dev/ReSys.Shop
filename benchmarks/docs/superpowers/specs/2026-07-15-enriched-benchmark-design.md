# Design Spec: Enriched Dataset with Dual-Label Benchmark

**Status**: Draft (awaiting user review)
**Date**: 2026-07-15
**Context**: Extends the fashion image retrieval benchmark to use visual attributes
from per-product JSON metadata (`articleAttributes.Pattern`), enabling side-by-side
primary (category+colour) and secondary (category+colour+pattern) evaluation.

---

## 1. Problem Statement

The current benchmark uses a flat CSV (`styles.csv`) with 10 columns as its only
data source. The full Fashion Product Images dataset also contains 44,446 per-product
JSON files (`styles/{product_id}.json`) with 30+ fields including a nested
`articleAttributes` object that holds **visual attributes absent from the CSV**:

| Attribute | Coverage | Values | Visual Impact |
|-----------|----------|--------|---------------|
| Pattern | 48% | Solid, Printed, Checked, Striped, Self Design, … | High (printed vs solid is immediate) |
| Sleeve Length | 39% | Long Sleeves, Short Sleeves, Sleeveless, … | High (silhouette) |
| Fabric | 42% | Cotton, Polyester, Blended, … | Medium (texture) |
| Fit | 33% | Regular Fit, Slim Fit, Skinny Fit | Medium (silhouette) |
| Neck | 22% | Round Neck, V-Neck, Henley, … | Medium |
| Collar | 14% | Spread Collar, Mandarin, Polo, … | Medium |

Currently these attributes are unused. Integrating **Pattern** (highest coverage,
highest visual impact) adds a **secondary relevance scheme** that operates
side-by-side with the primary `subCategory/norm_colour` scheme:

- **Primary**: `subCategory/norm_colour` → "Blue T-shirt" vs "Blue polo" = similar
- **Secondary**: `subCategory/norm_colour/pattern` → "Checked Blue Shirt" vs
  "Solid Blue Shirt" = NOT similar (different pattern)

The two schemes share the same images, splits, and model embeddings. Running both
produces two independent sets of retrieval metrics, enabling model comparison on
different levels of visual precision.

---

## 2. Design Overview

```
                    ┌─────────────────────────┐
                    │  bench enrich  (new CLI) │
                    │  scripts/enrich_dataset.py│
                    │                           │
  styles/*.json ──▶ │  1. Read 44K JSON files  │
  (44K files)       │  2. Extract Pattern      │────▶ fashion-enriched/
  styles.csv   ──▶  │  3. Merge with CSV       │        styles.csv (enriched)
                    │  4. Normalize colours     │        splits/fold_*_*.json
                    │  5. Write dual-label JSON │        (label + label_pattern)
                    └─────────────────────────┘
                              │
                              ▼
                    ┌─────────────────────────┐
                    │  benchmark thesis        │
                    │  --dataset-csv FASHION   │
                    │  --secondary-label       │
                    │                           │
  fashion-enriched  │  1. Run primary eval     │────▶ thesis_results.json
  (splits)         │     on label field        │
                    │  2. Run secondary eval    │────▶ thesis_results_pattern.json
                    │     on label_pattern      │
                    │  3. Print comparison      │
                    └─────────────────────────┘
```

**Key principle**: The secondary evaluation uses the **same model embeddings** as
the primary. No re-embedding — we swap the label field and recompute metrics
from the cached embeddings. This makes the secondary evaluation extremely fast
(a few seconds per model after initial embedding generation).

---

## 3. File Changes

### 3.1 New File: `scripts/enrich_dataset.py`

Standalone script. No dependency on the benchmark package (so it can run before
the benchmark). Exposed via `benchmark enrich` CLI command.

**Inputs**:
- `--json-styles`: path to `fashion-product-images/styles/` directory (44K JSON files)
- `--csv`: path to `fashion-product-images-small/styles.csv` (or any CSV with id, subCategory, baseColour)
- `--output`: output directory for enriched dataset
- `--subset`: optional, number of rows to keep (default: all)

**Processing**:
1. Read CSV → `{id: {subCategory, baseColour, masterCategory, …}}` lookup map
2. For each JSON file in `styles/`:
   - Parse `data.articleAttributes.Pattern` → `"Solid"` / `"Printed"` / … / `"Unknown"`
   - Look up CSV row by product ID
   - Join: JSON attributes + CSV fields
3. Produce enriched CSV with columns: `id`, `masterCategory`, `subCategory`,
   `articleType`, `baseColour`, `season`, `year`, `usage`, `gender`,
   `productDisplayName`, `pattern`, `sleeve_length`, `fabric`
4. Run `GroundTruth.generate_splits()` to produce split JSON files with dual labels

**Output format** (split JSON):
```json
[
  {
    "image_path": "images/15970.jpg",
    "label": "Topwear/Blue",
    "label_pattern": "Topwear/Blue/Checked",
    "product_id": "15970"
  },
  {
    "image_path": "images/39386.jpg",
    "label": "Bottomwear/Blue",
    "label_pattern": "Bottomwear/Blue",
    "product_id": "39386"
  }
]
```

When `pattern` is `"Unknown"`, `label_pattern` falls back to `label` (no
penalty for missing data).

**Output format** (split JSON):
```json
[
  {
    "image_path": "images/15970.jpg",
    "label": "Topwear/Blue",
    "label_pattern": "Topwear/Blue/Checked",
    "product_id": "15970"
  },
  {
    "image_path": "images/39386.jpg",
    "label": "Bottomwear/Blue",
    "label_pattern": "Bottomwear/Blue",
    "product_id": "39386"
  }
]
```

When `pattern` is `"Unknown"`, `label_pattern` falls back to `label` (no
penalty for missing data).

**Output directory structure**:
```
fashion-enriched-5k/
├── styles.csv              # Enriched CSV: id, masterCategory, …, pattern, sleeve, fabric
├── images/                 # Symlink → fashion-product-images-small/images/
└── splits/
    ├── fold_0_test.json     # { …, "label": "Topwear/Blue", "label_pattern": "Topwear/Blue/Checked" }
    ├── fold_0_train.json
    ├── fold_1_test.json
    ├── fold_1_train.json
    ├── fold_2_test.json
    └── fold_2_train.json
```

### 3.2 Modified: `src/benchmark/datasets/ground_truth.py`

- Export `normalize_colour()` (currently `_normalize_colour`) so the enrichment
  script can import and reuse it. No other changes needed — the script produces
  its own dual-label JSON splits, not via `generate_splits()`.

### 3.3 Modified: `src/benchmark/evaluation/thesis.py`

Add `secondary_label_field` parameter to `ThesisRunner.__init__()`:

```python
def __init__(self, ..., secondary_label_field: str | None = None):
    self._secondary_label = secondary_label_field
```

In `run()`, when `secondary_label_field` is set, after the primary evaluation:
1. Re-read split files
2. Swap the `label` field → `label_pattern` for evaluation
3. Run evaluation again (embeddings loaded from cache)
4. Write `thesis_results_pattern.json`

### 3.4 Modified: `src/benchmark/cli/benchmark.py`

- Add `--secondary-label TEXT` flag to `thesis` command (optional, triggers
  second evaluation pass using `label_pattern` field)
- Add new `enrich` command group under the Typer app
- **No `--dataset-csv` flag needed** — the enriched dataset follows the same
  `dataset_root/styles.csv` convention. Pass `--dataset-root data/raw/fashion-enriched-5k`
  directly.

### 3.5 Modified: `src/benchmark/datasets/loader.py`

The `FashionDataset.load()` method reads `label` from split JSON.
Add `label_field` parameter (default `"label"`) to support loading either
`"label"` or `"label_pattern"`.

```python
def load(self, label_field: str = "label") -> None:
    # ... existing code, but reads self._label_field instead of hardcoded "label"
```

---

## 4. CLI Commands

### `benchmark enrich`

```bash
# Create 5K enriched subset
uv run benchmark enrich \
    --json-styles data/raw/fashion-product-images/styles/ \
    --csv data/raw/fashion-product-images-small/styles.csv \
    --output data/raw/fashion-enriched-5k \
    --subset 5000

# Create full enriched dataset
uv run benchmark enrich \
    --json-styles data/raw/fashion-product-images/styles/ \
    --csv data/raw/fashion-product-images-small/styles.csv \
    --output data/raw/fashion-enriched
```

### `benchmark thesis` (with secondary label)

```bash
# Run dual-label evaluation
uv run benchmark thesis \
    --dataset-root data/raw/fashion-enriched-5k \
    --secondary-label pattern \
    --folds 3 --seed 42 --device cpu
```

---

## 5. Test Implications

### New Tests

| File | Tests |
|------|-------|
| `tests/datasets/test_enrich.py` | Verify enrichment produces correct dual labels, edge cases for missing Pattern |
| `tests/datasets/test_loader.py` | Add `test_loader_with_label_field` — verify `label_field` parameter |
| `tests/evaluation/test_thesis_secondary.py` | Verify secondary evaluation runs, produces separate results file, handles missing labels |

### Modified Tests

| File | Change |
|------|--------|
| `tests/datasets/test_ground_truth.py` | Update import for renamed `normalize_colour` |
| `tests/evaluation/test_thesis.py` | Add mock test for `secondary_label_field` path |

---

## 6. Docs to Update

| Doc | Change | Priority |
|-----|--------|----------|
| `09-visual-similarity-attributes.md` | §2.8: Pattern attribute from JSON + enrichment explanation | High |
| `06-thesis-protocol.md` | Dual-label evaluation protocol section | High |
| `10-benchmark-comparison.md` | Add pattern-augmented mAP column to comparison table | High |
| `05-datasets.md` | "Enriched Dataset" section | Medium |
| `04-pipeline.md` | Note dual-label evaluation flow | Low |
| `README.md` | `benchmark enrich` command + enriched dataset instructions | High |
| New: `11-enriched-dataset.md` | Full user guide for enrichment script | High |

---

## 7. Non-Goals (Explicitly Deferred)

- **Sleeve Length, Fabric, Fit, Neck, Collar** — only Pattern in v1. These are
  future extensions following the same pattern.
- **Graded/scored relevance** — both evaluations remain binary label matching.
  Weighted relevance is a separate design problem.
- **Auto-download of JSON dataset** — users must have the full Kaggle dataset
  already downloaded. The enrichment script works on what's on disk.
- **Changing the default thesis evaluation** — `--secondary-label` is opt-in.
  The default `benchmark thesis` behavior is unchanged.

---

## 8. Migration Path

1. User downloads Fashion Product Images (full) from Kaggle → gets `styles/` JSON files
2. Run `benchmark enrich --subset 5000` → creates `fashion-enriched-5k/` with dual-label splits
3. Run `benchmark thesis --secondary-label` → produces two result JSON files
4. Read `thesis_results.json` (primary) and `thesis_results_pattern.json` (secondary)
5. Compare model rankings between primary and secondary: does FashionCLIP still lead?

---

## 9. Self-Review Checklist

- [x] No TBD or TODO placeholders
- [x] Architecture matches feature descriptions
- [x] Scope is single-spec (no decomposition needed)
- [x] No ambiguous requirements — two interpretations resolved to one
- [x] All CLI flags have defaults documented
- [x] Test plan covers new and modified code paths
- [x] Docs update list is prioritized and complete
- [x] Non-goals are explicitly stated
- [x] Migration path is step-by-step
