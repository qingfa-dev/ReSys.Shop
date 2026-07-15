# 01 — Overview

## What Is This Benchmark?

This is an **academic benchmark** that answers one question:

> **Which pre-trained AI model is best at finding visually similar fashion products?**

We take thousands of fashion product images, run them through different AI models, and measure how well each model finds similar items.

## Why Does This Exist?

The ReSys.Shop platform needs image-based product search ("show me items like this dress"). There are dozens of AI models that claim to do this. We need to know:

1. **Which model finds the most relevant similar items?** (accuracy)
2. **Which model is fastest?** (latency)
3. **Which model uses the least memory?** (efficiency)
4. **Is a fashion-specific model actually better than a general one?** (domain relevance)

## What We Measure

### Retrieval Effectiveness
How good is the model at finding similar items?

- **Precision@K** — Of the top-K results, how many are actually similar?
- **Recall@K** — Of all similar items, how many did the model find?
- **mAP** — Overall quality of the ranking

### Operational Performance
How practical is the model for production use?

- **Latency** — ms per image
- **Throughput** — images per second
- **Load time** — seconds to start the model
- **RAM** — memory usage
- **Storage** — MB per 1,000 embeddings

## Two Modes

### 1. One-Shot Benchmark Mode
```bash
uv run benchmark run --dataset-root data/raw/fashion-product-images-small --models all --k 10
```
Compares all registered models on any dataset. Fast, good for exploration.

### 2. Thesis Mode
```bash
uv run benchmark thesis --dataset-root data/raw/fashion-5k --folds 3
```
Runs the strict academic protocol from thesis Chapter 11 (§11.5):
- Exactly 4 models (FashionCLIP, CLIP-generic, EfficientNet-B0, ResNet-50)
- 3-fold cross-validation
- Statistical analysis (Cohen's d, bootstrap CI)
- Generates Typst tables for the thesis

### 3. Enrich Mode (Data Preparation)
```bash
uv run benchmark enrich --dataset-root data/raw/fashion-product-images --n-samples 5000
```
Extracts visual attributes (Pattern, Fabric, etc.) from per-product JSON metadata and builds an enriched CSV with dual-label splits for colour+pattern evaluation.

## The Core Idea (In One Paragraph)

An AI model turns an image into a list of numbers (an **embedding**). Similar images get similar numbers. To test a model, we pick an image, ask "which other images have the most similar numbers?", and check if those other images are actually similar (same category + colour + pattern). We repeat this for thousands of images and compute statistics.

## Glossary

| Term | Definition |
|------|-----------|
| **Embedding** | A fixed-length list of floats (vector) representing an image in a high-dimensional space. Similar images → nearby vectors. |
| **mAP (mean Average Precision)** | Primary accuracy metric. Average of precision values at each rank where a relevant item appears, averaged across all queries. 1.0 = perfect ranking. |
| **Precision@K** | Fraction of top-K results that are relevant. High precision = few wrong results in top-K. |
| **Recall@K** | Fraction of all relevant items found in top-K. High recall = most relevant items recovered. |
| **nDCG (normalized Discounted Cumulative Gain)** | Ranking quality metric that penalizes relevant items appearing late in the list. 1.0 = optimal ordering. |
| **Stratified k-fold CV** | Dataset split into k equal parts, preserving category proportions. Each part serves as test set once. |
| **Ground truth** | The "correct answer" — which items should be relevant to which query. Built from product metadata (category + normalised colour + pattern). |
| **Colour normalisation** | Merging 46 raw colour labels (e.g. "Navy Blue", "Turquoise Blue") into 11 perceptual groups (e.g. "Blue"). Makes ground truth reflect human visual perception. |
| **Cohen's d** | Effect size — how many standard deviations apart two models' means are. `|d| > 0.8` = large effect. |
| **Bootstrap CI** | Confidence interval estimated by resampling observed data 10,000×. Non-parametric, no normality assumption. |
| **L2 normalisation** | Scaling each embedding vector to unit length (Euclidean norm = 1.0). Makes dot product = cosine similarity. |

## Key Principles

1. **No training required** — We only use pre-trained models downloaded from the internet.
2. **Reproducible** — Same seed → same splits → same results.
3. **Fair comparison** — All models see the same images, same preprocessing, same evaluation code.
4. **Cache-friendly** — Embeddings are cached so re-running is fast.

## Architecture at a Glance

```
Raw JSON → Enrich (pattern) → CSV → GroundTruth (splits) → Dataset → Model
                                                                  ↓
                                                         Embedding → Cache
                                                                  ↓
                                              Evaluator → Metrics (P@K, R@K, mAP, nDCG)
                                                                  ↓
                                              Reporter → Typst Tables / Charts / JSON
```

Each component is isolated. You can swap the model without touching the evaluator. You can swap the dataset without touching the model. The enrich step is optional — for cat+colour+pattern evaluation only.

## Who Is This For?

| Role | What You'll Use |
|------|----------------|
| **Thesis writer** | Thesis mode, Typst output, statistical analysis |
| **ML engineer** | General benchmark, model comparison, latency testing |
| **New developer** | This documentation, glossary, small test runs |
| **DevOps** | Operational metrics, RAM/storage benchmarks |

## Next Steps

- Read [02 — Models](02-models.md) to understand each model
- Read [03 — Metrics](03-metrics.md) to understand what we measure
- Read [10 — Benchmark Comparison](10-benchmark-comparison.md) for the 3-way results
- Run `uv run benchmark --help` to see CLI options
