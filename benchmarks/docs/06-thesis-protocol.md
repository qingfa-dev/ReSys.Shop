# 06 — Thesis Protocol

The exact protocol used for the thesis evaluation (Chapter 11, §11.5), explained step by step.

## Why This Protocol Exists

The thesis makes a claim: **Fashion-CLIP outperforms generic models for fashion retrieval.** To support this claim with evidence, we need a rigorous, reproducible experiment. This protocol ensures:

1. **Fair comparison** — All models see the same data
2. **Statistical validity** — Results are averaged across multiple runs
3. **Reproducibility** — Anyone can run the same command and get the same results
4. **Academic standards** — Follows conventions from CBIR literature

## The Core Question

> Which pre-trained embedding model provides the best balance of retrieval effectiveness and operational performance for fashion product images?

## The 4 Models

| Model | Hypothesis | Expected Result |
|-------|-----------|-----------------|
| **Fashion-CLIP** | H1: Highest mAP | Fashion-specific fine-tuning aligns with human judgments |
| **EfficientNet-B0** | H2: Best efficiency (mAP/ms) | Compound scaling optimizes FLOPs-to-accuracy |
| **ResNet-50** | H3: Highest storage cost | 2048-d vectors consume 4× storage vs 512-d |
| **CLIP-generic** | H4: Underperforms Fashion-CLIP, outperforms CNNs | Text-image pretraining > pure visual features |

## Protocol Steps

### Step 1: Dataset Preparation

**Dataset:** Fashion Product Images Small (~5,000 images)

**Ground truth rule:** Two images are relevant if they share `masterCategory` + `subCategory`.

**Example:**
- Black T-shirt (`Apparel`/`Topwear`) and blue T-shirt (`Apparel`/`Topwear`) → **relevant**
- T-shirt (`Apparel`/`Topwear`) and sneaker (`Footwear`/`Shoes`) → **not relevant**

### Step 2: 3-Fold Cross-Validation

**Why cross-validation?**
- Every image serves as a query exactly once
- Every image serves in the gallery exactly twice
- Averaging across folds reduces random variation

**How it works:**

```
All 5,000 images
├─ Fold 0: Test = 1,667 images, Train (gallery) = 3,333 images
├─ Fold 1: Test = 1,667 images, Train (gallery) = 3,333 images
└─ Fold 2: Test = 1,666 images, Train (gallery) = 3,334 images
```

**Stratification:** Each fold maintains the same category proportions. If 50% of all images are `Apparel`, then each fold also has ~50% `Apparel`.

**Minimum frequency:** Categories with <10 images are grouped into `"Other"` before splitting.

### Step 3: Per-Fold, Per-Model Evaluation

For each fold, for each model:

1. **Load model** → record `load_time_ms`
2. **Generate embeddings** for train + test sets
3. **Retrieve top-K** for each query image (test → train gallery)
4. **Compute metrics** (P@K, R@K, mAP, nDCG@K)
5. **Measure efficiency** (latency, throughput, RAM, storage)

**Cache key:** `(model_slug, fold_N_train)` and `(model_slug, fold_N_test)`

### Step 4: Aggregation

For each model, compute **mean ± SD** across the 3 folds.

**Example output:**

| Metric | Fold 0 | Fold 1 | Fold 2 | Mean ± SD |
|--------|--------|--------|--------|-----------|
| mAP | 0.824 | 0.818 | 0.827 | **0.823 ± 0.005** |
| P@5 | 0.715 | 0.708 | 0.720 | **0.714 ± 0.006** |

### Step 5: Statistical Analysis

**What we compute:**

1. **Descriptive statistics** — mean ± SD (primary reporting)
2. **Cohen's d** — effect size between Fashion-CLIP and each competitor
3. **Bootstrap 95% CI** — approximate confidence interval for mean mAP

**What we OMIT:**

- **Paired t-tests** — With only 3 folds (n=3), the test is underpowered. It cannot reliably detect true differences. This is documented as a known limitation.

**Effect size interpretation:**

| Cohen's d | Interpretation |
|-----------|---------------|
| 0.0 – 0.2 | Negligible |
| 0.2 – 0.5 | Small |
| 0.5 – 0.8 | Medium |
| 0.8+ | Large |

**Example:** Cohen's d = 1.2 between Fashion-CLIP and ResNet-50 → **large effect**, Fashion-CLIP is substantially better.

### Step 6: Report Generation

**Typst tables** (included directly in thesis):
- Retrieval effectiveness table (P@K, R@K, mAP with mean ± SD)
- Operational performance table (latency, throughput, storage, RAM)
- Model ranking by mAP

**Pareto frontier chart:**
- X-axis: mean latency (ms)
- Y-axis: mAP
- Top-left models are best (fast AND accurate)

**JSON summary:**
- Raw fold-level data for verification
- Statistical analysis results
- Configuration metadata

## Running the Protocol

```bash
uv run benchmark thesis \
  --dataset-root data/raw/fashion-product-images-small \
  --output outputs/thesis \
  --k 5,10,20 \
  --folds 3 \
  --seed 42 \
  --device auto
```

**What this does:**
1. Loads `styles.csv` from the dataset root
2. Builds ground truth and generates 3 stratified splits
3. Runs all 4 models on all 3 folds
4. Computes metrics, efficiency, and statistics
5. Writes outputs to `outputs/thesis/`

**Expected runtime:**
- With GPU: ~30-60 minutes (4 models × 3 folds)
- With CPU: ~2-4 hours

## Output Structure

```
outputs/thesis/
├── splits/
│   ├── fold_0_train.json
│   ├── fold_0_test.json
│   ├── fold_1_train.json
│   ├── fold_1_test.json
│   ├── fold_2_train.json
│   └── fold_2_test.json
├── tables/
│   ├── thesis_aggregate.typ     # Retrieval effectiveness table
│   └── thesis_efficiency.typ    # Operational performance table
├── results/
│   └── thesis_results.json      # Complete statistical summary
└── logs/
    └── thesis.log               # Detailed execution log
```

## Deviation from Original Thesis Plan

The thesis §11.5.4 originally described:
- **100-image dataset** with manual annotation
- **3 repeated runs** (not cross-validation)
- **Paired t-tests** on 100 query-level observations

**What we changed and why:**

| Aspect | Original Plan | Our Protocol | Rationale |
|--------|--------------|--------------|-----------|
| Dataset size | 100 images | 5,000 images | Category-based ground truth scales without manual annotation cost |
| Validation | 3 repeated runs | 3-fold CV | CV is methodologically stronger; every sample serves as query once |
| Significance testing | Paired t-tests | Omitted (underpowered) | n=3 folds cannot achieve statistical power; descriptive stats are honest |

**This deviation is explicitly documented in the thesis text.**

## Results Template

The thesis includes tables in this format (to be populated with actual numbers):

### Retrieval Effectiveness (mean ± SD)

| Model | Precision@5 | Recall@5 | Precision@10 | Recall@10 | Precision@20 | Recall@20 | mAP |
|-------|-------------|----------|--------------|-----------|--------------|-----------|-----|
| Fashion-CLIP | `X.XX ± X.XX` | `X.XX ± X.XX` | `X.XX ± X.XX` | `X.XX ± X.XX` | `X.XX ± X.XX` | `X.XX ± X.XX` | `X.XX ± X.XX` |
| CLIP-generic | `X.XX ± X.XX` | `X.XX ± X.XX` | `X.XX ± X.XX` | `X.XX ± X.XX` | `X.XX ± X.XX` | `X.XX ± X.XX` | `X.XX ± X.XX` |
| EfficientNet-B0 | `X.XX ± X.XX` | `X.XX ± X.XX` | `X.XX ± X.XX` | `X.XX ± X.XX` | `X.XX ± X.XX` | `X.XX ± X.XX` | `X.XX ± X.XX` |
| ResNet-50 | `X.XX ± X.XX` | `X.XX ± X.XX` | `X.XX ± X.XX` | `X.XX ± X.XX` | `X.XX ± X.XX` | `X.XX ± X.XX` | `X.XX ± X.XX` |

### Operational Performance

| Model | Embed Time (ms) | Load Time (s) | Storage/1K (MB) | RAM (MB) |
|-------|-----------------|---------------|-----------------|----------|
| Fashion-CLIP | `XXX ± XX` | `X.XX` | `X.XX` | `XXXX` |
| CLIP-generic | `XXX ± XX` | `X.XX` | `X.XX` | `XXXX` |
| EfficientNet-B0 | `XXX ± XX` | `X.XX` | `X.XX` | `XXXX` |
| ResNet-50 | `XXX ± XX` | `X.XX` | `X.XX` | `XXXX` |

### Analysis Dimensions

1. **Retrieval effectiveness:** Which model maximizes mAP?
2. **Efficiency-accuracy trade-off:** Which model dominates the Pareto frontier?
3. **Storage cost:** Is ResNet-50's 4× storage justified by retrieval gains?
4. **Business impact:** Which model meets the ≥0.70 Recall@20 target while minimizing operational cost?

## Threats to Validity

| Threat | Mitigation |
|--------|-----------|
| **Category-based relevance** may not match human judgment | Defensible at scale; thesis acknowledges this limitation |
| **Small fold count (n=3)** limits statistical power | Descriptive statistics primary; no overclaiming significance |
| **Dataset imbalance** (some categories overrepresented) | Stratified splitting ensures proportional representation |
| **Hardware-specific results** | Full hardware spec reported; results are relative comparisons |
| **Model version drift** | Exact package versions pinned in `pyproject.toml` |
| **PGVector query latency** may not reflect production | Optional metric; reported as supplementary only |

## Reproducibility Checklist

Before claiming results are final:

- [ ] Run with `--seed 42` (deterministic)
- [ ] Document hardware specs (CPU, GPU, RAM)
- [ ] Pin all dependency versions (`uv lock` → `uv.lock`)
- [ ] Save complete `thesis_stats.json` with all fold data
- [ ] Verify Typst tables compile without errors
- [ ] Include Pareto frontier chart in thesis
- [ ] Document any deviations from original protocol in thesis text
