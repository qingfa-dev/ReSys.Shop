# Thesis Benchmark Mode Design

**Date:** 2026-07-15  
**Scope:** Add a `benchmark thesis` subcommand to the existing `benchmarks/` package that runs the §11.5 ML evaluation protocol from the CTU thesis.  
**Status:** Approved — ready for implementation plan.

---

## 1. Goal

Provide a single CLI invocation that produces all quantitative evidence required for thesis Chapter 11 (§11.5 — ML Comparative Study):

- 4 models × 3-fold cross-validation on the 5k Fashion Product Images Small dataset
- Retrieval effectiveness: Precision@K, Recall@K, mAP (mean ± SD)
- Operational performance: embedding time, load time, storage/1K, query latency, RAM
- Statistical analysis: paired t-tests (Bonferroni corrected), Cohen's d, bootstrap 95% CI
- Output: Typst tables, Pareto frontier chart, JSON with raw fold data + stats

The general `benchmark benchmark` command remains unchanged.

---

## 2. Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│  CLI: benchmark thesis                                          │
│  ─────────────────────────────────────────────────────────────  │
│  Entry: src/benchmark/cli/thesis.py                             │
│                                                                 │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐  │
│  │ GroundTruth  │  │ ThesisRunner │  │ ThesisReporter       │  │
│  │ Builder      │──│ (3-fold CV)  │──│ (tables + charts)    │  │
│  └──────────────┘  └──────────────┘  └──────────────────────┘  │
│         │                 │                      │              │
│         ▼                 ▼                      ▼              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐  │
│  │ Split JSON   │  │ Evaluator    │  │ Typst / PNG / JSON   │  │
│  │ files        │  │ (per fold)   │  │ outputs              │  │
│  └──────────────┘  └──────────────┘  └──────────────────────┘  │
│                                                                 │
│  New adapters: ResNet-50, CLIP-generic                          │
│  New stats: paired t-test, Cohen's d, bootstrap CI              │
└─────────────────────────────────────────────────────────────────┘
```

---

## 3. Components

### 3.1 New Modules

| Module | File | Purpose |
|--------|------|---------|
| `cli/thesis.py` | `src/benchmark/cli/thesis.py` | Typer subcommand, argument parsing, config display |
| `evaluation/thesis.py` | `src/benchmark/evaluation/thesis.py` | Orchestrates 4-model × 3-fold protocol, collects operational metrics |
| `datasets/ground_truth.py` | `src/benchmark/datasets/ground_truth.py` | Parses `styles.csv`, builds relevance sets, generates stratified splits |
| `evaluation/stats.py` | `src/benchmark/evaluation/stats.py` | Paired t-tests, Cohen's d, bootstrap 95% CI |
| `reporting/thesis.py` | `src/benchmark/reporting/thesis.py` | Thesis-specific Typst tables, Pareto chart, summary JSON |
| `models/resnet50.py` | `src/benchmark/models/resnet50.py` | ResNet-50 adapter (torchvision, 2048-D, ImageNet-1K) |
| `models/clip_generic.py` | `src/benchmark/models/clip_generic.py` | Generic CLIP adapter (OpenAI `clip-vit-base-patch32`, 512-D) |

### 3.2 Reused Modules (unchanged)

| Module | Usage |
|--------|-------|
| `FashionDataset` | Loads JSON split files |
| `EmbeddingGenerator` | Batch embedding generation + cache |
| `Evaluator.evaluate_split()` | Query/gallery retrieval + metrics |
| `precision.py`, `recall.py`, `map.py`, `ndcg.py` | Effectiveness metrics |
| `latency.py`, `throughput.py` | Efficiency metrics. `LatencyStats` extended with `mean_ms` and `std_ms` fields for thesis reporting. |
| `cosine.py` | In-memory retrieval for speed |
| `pgvector.py` | Query latency measurement only |
| `reporting/typst.py` | General table generation (extended by thesis reporter) |

### 3.3 Model Registry Changes

Add to `src/benchmark/models/__init__.py`:
- `"resnet-50"`: `ResNet50Model(device=device)`
- `"clip-generic"`: `ClipGenericModel(device=device)`

These models are included in the general registry and available via `benchmark benchmark --models all`. The `benchmark thesis` command uses a hardcoded subset of 4 keys regardless of registry contents.

---

## 4. Data Flow

### 4.1 Input

- `data/raw/fashion-product-images-small/images/` — product images
- `data/raw/fashion-product-images-small/styles.csv` — metadata with `id`, `masterCategory`, `subCategory`

### 4.2 Protocol (per invocation)

1. **Load metadata** — `styles.csv` → DataFrame with `id`, `masterCategory`, `subCategory`
2. **Build ground truth** — relevance set per product = all products with same `masterCategory` + `subCategory` (excluding self). If `subCategory` is missing/NaN, fall back to `masterCategory` only.
3. **Generate 3-fold splits** — stratified by `masterCategory`:
   - Fold 0: train=fold_0_train.json (3333 samples), test=fold_0_test.json (1667 samples)
   - Fold 1: train=fold_1_train.json, test=fold_1_test.json
   - Fold 2: train=fold_2_train.json, test=fold_2_test.json
   - Deterministic via `--seed` (default 42)
4. **Per fold, per model:**
   a. `model.load()` → record `load_time_ms`
   b. `EmbeddingGenerator.generate()` for train + test → cache as `(model.slug, fold_N_train/test)`
   c. `Evaluator.evaluate_split(query, gallery)` → per-query P@K, R@K, mAP, nDCG
   d. `measure_latency()` on 200 samples → `LatencyStats` now includes `mean_ms` and `std_ms` fields. The thesis runner uses these values (not percentiles).
   e. `measure_throughput()` → images/sec
   f. Record operational metrics:
      - `load_time_ms`: timer around `model.load()`
       - `index_storage_mb`: total storage of all embeddings (`embeddings.nbytes / 1024 / 1024`). The Typst table computes "per 1K" by dividing by `(N / 1000)`.
       - `ram_mb`: `psutil.Process().memory_info().rss / 1024 / 1024` after inference
      - `query_latency_ms`: timed `PgvectorRetriever.query()` (optional, if PG available)
5. **Aggregate** — per model, per metric: mean ± SD across 3 folds
6. **Statistical tests** —
   - Paired t-test: Fashion-CLIP vs each competitor on fold-level mAP scores
   - Bonferroni correction: α = 0.05 / 3 ≈ 0.017
   - Cohen's d for effect size
   - Bootstrap 95% CI for mean mAP (10,000 resamples)
7. **Generate outputs** —
   - `outputs/thesis/tables/thesis_results.typ` — all tables with mean ± SD
   - `outputs/thesis/figures/pareto_frontier.png` — mAP vs mean latency
   - `outputs/thesis/thesis_stats.json` — raw fold-level arrays + statistical test results

### 4.3 Output Schema (`thesis_stats.json`)

```json
{
  "config": {
    "dataset": "fashion-product-images-small",
    "n_samples": 5000,
    "folds": 3,
    "k_values": [5, 10, 20],
    "seed": 42
  },
  "models": {
    "fashion-clip": {
      "folds": [
        {
          "fold": 0,
          "map": 0.8234,
          "precision@5": 0.7123,
          "recall@5": 0.1567,
          "load_time_ms": 3456,
          "latency_mean_ms": 12.3,
          "latency_std_ms": 2.1,
          "throughput_per_sec": 78.5,
          "index_storage_mb": 2.0,
          "ram_mb": 2048
        }
      ],
      "aggregate": {
        "map": {"mean": 0.8234, "std": 0.0123, "ci_95": [0.8001, 0.8467]},
        "precision@5": {"mean": 0.7123, "std": 0.0089},
        "latency_mean_ms": {"mean": 12.3, "std": 1.2}
      }
    }
  },
  "statistical_tests": {
    "fashion-clip vs resnet-50": {
      "metric": "map",
      "paired_t_stat": 3.45,
      "p_value": 0.012,
      "p_value_bonferroni": 0.036,
      "cohens_d": 1.23,
      "significant": false
    }
  }
}
```

---

## 5. Error Handling

| Error | Handling |
|-------|----------|
| Missing `styles.csv` | Fatal — print path and exit with code 1 |
| Missing images | Log warning, skip sample (existing behavior in `generator.py`) |
| Model load failure | Fatal for that model — log error, continue with remaining models |
| Cache read failure | Fallback to recompute (existing behavior) |
| PGVector not available for query latency | Skip metric, log warning, continue |
| Fold with zero relevant items | Return 0.0 for AP (existing `map.py` behavior) |
| Statistical test with < 2 folds | Skip test, log warning |

---

## 6. Testing Strategy

| Test | Location | Coverage |
|------|----------|----------|
| Unit: Ground truth builder | `tests/datasets/test_ground_truth.py` | Parse CSV, relevance sets, stratified splits |
| Unit: Statistical functions | `tests/evaluation/test_stats.py` | t-test, Cohen's d, bootstrap CI on synthetic data |
| Unit: ResNet-50 adapter | `tests/models/test_resnet50.py` | Load, embed, output shape, normalisation |
| Unit: CLIP-generic adapter | `tests/models/test_clip_generic.py` | Load, embed, output shape, normalisation |
| Integration: End-to-end thesis run | `tests/integration/test_thesis.py` | Run on 10-image subset, verify JSON output schema |
| Integration: Typst output | `tests/reporting/test_thesis_typst.py` | Verify generated `.typ` compiles with `typst compile` |

---

## 7. CLI Reference

```
uv run benchmark thesis [OPTIONS]

Options:
  --dataset-root PATH     Path to fashion-product-images-small/  [default: data/raw/fashion-product-images-small]
  --styles-csv PATH       Path to styles.csv  [default: <dataset-root>/styles.csv]
  --output PATH           Output directory  [default: outputs/thesis]
  --k TEXT                Comma-separated K values  [default: 5,10,20]
  --folds INTEGER         Number of CV folds  [default: 3]
  --seed INTEGER          Random seed  [default: 42]
  --device TEXT           Device (cpu, cuda, mps, auto)  [default: auto]
  --no-cache              Disable embedding cache
  --pgvector-url TEXT     PostgreSQL connection string for query latency measurement
  --log-level TEXT        Logging level  [default: INFO]
```

---

## 8. Relation to Thesis Chapter 11

| Thesis §11.5 Requirement | Design Element |
|-------------------------|----------------|
| 4 models (Fashion-CLIP, ResNet-50, EfficientNet-B0, CLIP-generic) | Model adapters + registry |
| 5k dataset (scaled from 100) | `FashionDataset` on full small dataset |
| Ground-truth relevance | `datasets/ground_truth.py` — category-based |
| 3-fold cross-validation | `ThesisRunner` stratified splits |
| Precision@K, Recall@K, mAP | Existing metrics + aggregation |
| Embedding generation time | `measure_latency()` + mean export |
| Model load time | Timer around `model.load()` |
| Index storage / 1K | `embeddings.nbytes` calculation |
| Query latency | `PgvectorRetriever` timed queries |
| RAM footprint | `psutil.Process().memory_info()` |
| Paired t-tests, Bonferroni | `evaluation/stats.py` |
| Cohen's d | `evaluation/stats.py` |
| Bootstrap 95% CI | `evaluation/stats.py` |
| Typst tables | `reporting/thesis.py` |
| Pareto frontier plot | `reporting/thesis.py` |

---

## 9. Non-Goals

- **Not** replacing the general `benchmark benchmark` command
- **Not** manual annotation or inter-annotator agreement (κ) — category-based relevance is the scalable proxy
- **Not** fine-tuning any model — all adapters use pretrained weights only
- **Not** HNSW index comparison — IVF flat (nlist=100) as specified in thesis
- **Not** user study / SUS evaluation — out of thesis scope per §11.6

---

## 10. Dependencies

New runtime dependencies (add to `pyproject.toml`):
- `psutil` — RAM measurement
- `scipy` — t-tests (or implement manually to avoid heavy dep; preference: manual implementation using `statistics` module)

No new dependencies if statistical tests are implemented manually. `scipy` is acceptable if already in the venv.
