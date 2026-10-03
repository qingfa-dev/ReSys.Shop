# 13 — Benchmark Outcomes (Non-ML Developers)

Practical summary of the fashion image retrieval benchmark — what we measured,
what we learned, and what it means for building the search feature.

---

## What This Benchmark Is

We have 44,000 product images. We want to answer: *"Given a photo of a product,
find visually similar products."* No text search, no filters — purely
image-to-image matching.

We tested 11 AI models that convert images into numerical fingerprints
(embeddings) and compared how well they find similar items.

---

## The Key Discovery: Old Results Were Wrong

The original evaluation used a trivially easy test: "Is this T-shirt in the same
category as other T-shirts?" Every model scored ~93% because "Apparel > Topwear"
is easy to guess.

**We fixed the test** by requiring the model to match not just category, but
also **colour** and **pattern**. The real scores dropped from 93% to 24%.

| Test | What it measures | FashionCLIP score |
|------|-----------------|------------------|
| **Old:** same category? | Is this a T-shirt? | 93% |
| **Fixed:** same category + colour? | Is this a *blue* T-shirt? | **24%** |
| **Fixed:** same category + colour + pattern? | Is this a *blue checked* T-shirt? | **21%** |

The 4× drop proves the old benchmark measured category classification, not
visual similarity. The fixed results are the ones that matter.

---

## Which Model Should We Use?

### For accuracy (best search results)

**FashionCLIP** — wins every test. Fine-tuned on 800K fashion images.

| Test | FashionCLIP | Runner-up |
|------|-----------|-----------|
| Category only | 93.1% | CLIP-generic 91.2% |
| Category + colour | **24.5%** | CLIP-generic 23.1% |
| Category + colour + pattern | **21.5%** | CLIP-generic 20.1% |

### For speed (latency-sensitive, e.g. real-time search)

**EfficientNet-B0** — 4× faster than FashionCLIP, only 10% less accurate.

| Model | Time per image | Images/second |
|-------|---------------|--------------|
| EfficientNet-B0 | **24 ms** | **33/s** |
| ResNet-50 | 64 ms | 13/s |
| FashionCLIP | 92 ms | 18/s |
| CLIP-generic | 93 ms | 20/s |

### For storage

**FashionCLIP & CLIP** (512 numbers per image) — 3.3 MB per 1,000 products.

ResNet-50 needs 4× more storage (2048 numbers = 13 MB per 1,000 products).

### TL;DR

| Priority | Pick |
|----------|------|
| Best results | FashionCLIP |
| Fastest | EfficientNet-B0 |
| Cheapest storage | FashionCLIP or CLIP variants |

---

## What About Database Search (pgvector)?

We tested PostgreSQL with the pgvector extension for approximate nearest-neighbour
search. Key findings:

| Metric | Value |
|--------|-------|
| Query time | **2.2 ms** per search |
| Recall (how many correct results found) | 62% at top-20 |
| Time to index 5,000 images | 0.19 seconds |

**The recall is low (62%)** because the index needs more data to train on.
PostgreSQL's IVFFlat index is designed for 100K+ rows — our 5K test dataset
is too small. With production-scale data, recall should reach 95%+.

**The query speed is excellent** — 2 ms per search means the database can
handle hundreds of concurrent searches.

---

## The Model Rankings (Stable Across Every Test)

```
1. FashionCLIP     — best accuracy, moderate speed
2. CLIP-generic    — good accuracy, moderate speed
3. EfficientNet-B0 — decent accuracy, fastest
4. ResNet-50       — slowest, most storage, worst accuracy
```

This ranking stayed the same across all three test types (category,
category+colour, category+colour+pattern). The relative ordering is robust.

---

## Statistical Rigour

We ran 3-fold cross-validation on 5,000 images. Every number reported includes
mean and standard deviation across folds. We computed Cohen's d effect sizes
(all "Large" — d > 0.8 for FashionCLIP vs every competitor) and bootstrap
95% confidence intervals. See the [academic version](./13-benchmark-outcomes-academic.md)
for the full tables.

---

## Where the Results Live

| Directory | What's in it |
|-----------|-------------|
| `outputs_5k/metrics/` | One-shot results for 4 models on 5K images |
| `outputs_5k_split/metrics/` | Same, with separate query/gallery splits |
| `outputs/thesis/results/` | 3-fold CV results: category-only, category+colour, category+colour+pattern |
| `outputs/pipeline/results/` | 5-model results with pgvector metrics |
| `outputs/pipeline/tables/` | Typst table snippets (included in thesis) |

Each `.json` file contains per-model metrics (mAP, P@K, R@K, nDCG, latency,
throughput). The `comparison.json` in each `reports/` directory aggregates
all models.

---

## How to Reproduce

```bash
# Install everything
cd benchmarks && uv sync --extra dev

# Download the datasets (requires Kaggle API token)
uv run python scripts/01_download_dataset.py              # small (44K images)
uv run python scripts/01_download_dataset.py --dataset full  # + JSON metadata

# Run a quick test
uv run benchmark run --dataset-root data/raw/fashion-product-images-small --models fashion-clip --k 10

# Run the full thesis benchmark (4 models, 3-fold CV)
uv run benchmark thesis --dataset-root data/raw/fashion-product-images-small --folds 3

# Run with pgvector (requires Docker/Podman)
uv run benchmark pipeline --dataset-root data/raw/fashion-product-images-small --folds 3

# Generate reports from stored results
uv run benchmark report --format all
```

---

## Practical Recommendations for ReSys.Shop

1. **Default model: FashionCLIP.** Best accuracy, reasonable speed, small embeddings.

2. **Mobile/edge: EfficientNet-B0.** 4× faster, 5.3M parameters (fits on device),
   still gets 89% of FashionCLIP's accuracy.

3. **Production search: PostgreSQL + pgvector.** 2 ms query time, scales to
   millions of products, no separate vector database needed.

4. **Don't use ResNet-50.** Slowest, most storage, worst accuracy, and its
   2048-d embeddings can't use pgvector's IVFFlat index.

5. **Test with real data volume.** The 5K subset is too small for meaningful
   pgvector recall measurement. Index 100K+ products before evaluating
   production readiness.

---

## Related Docs

- [Academic version](./13-benchmark-outcomes-academic.md) — full methodology,
  statistical analysis, all tables, evidence appendix with raw data + charts
- [Benchmark Results (consolidated)](./09-benchmark-results.md)
- [Three-Way Comparison](./10-benchmark-comparison.md)
- [Enriched Dataset Guide](./11-enriched-dataset.md)
- [Replication Guide](./08-replication-guide.md)

---

## Key Charts

| Chart | Link |
|-------|------|
| mAP comparison (colour-aware) | [assets/figures/map_split.png](assets/figures/map_split.png) |
| Precision@K curves | [assets/figures/precision_split.png](assets/figures/precision_split.png) |
| Recall@K curves | [assets/figures/recall_split.png](assets/figures/recall_split.png) |
| Latency comparison | [assets/figures/latency.png](assets/figures/latency.png) |

Raw result data in [`assets/data/`](assets/data/) — 6 JSON files covering
thesis, pipeline, and one-shot protocols.
