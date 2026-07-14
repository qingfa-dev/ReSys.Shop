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

### 1. General Benchmark Mode
```bash
uv run benchmark benchmark --models all --dataset deepfashion
```
Compares all registered models on any dataset. Flexible, fast, good for exploration.

### 2. Thesis Mode
```bash
uv run benchmark thesis
```
Runs the strict academic protocol from thesis Chapter 11 (§11.5):
- Exactly 4 models
- 3-fold cross-validation
- Statistical analysis
- Generates Typst tables for the thesis

## The Core Idea (In One Paragraph)

An AI model turns an image into a list of numbers (an **embedding**). Similar images get similar numbers. To test a model, we pick an image, ask "which other images have the most similar numbers?", and check if those other images are actually similar (same category). We repeat this for thousands of images and compute statistics.

## Key Principles

1. **No training required** — We only use pre-trained models downloaded from the internet.
2. **Reproducible** — Same seed → same splits → same results.
3. **Fair comparison** — All models see the same images, same preprocessing, same evaluation code.
4. **Cache-friendly** — Embeddings are cached so re-running is fast.

## Architecture at a Glance

```
Raw Images → Model Adapter → Embedding Generator → Cache
                                              ↓
Dataset Loader → Ground Truth → Evaluator → Metrics
                                              ↓
Reporter → Typst Tables / Charts / JSON
```

Each component is isolated. You can swap the model without touching the evaluator. You can swap the dataset without touching the model.

## Who Is This For?

| Role | What You'll Use |
|------|----------------|
| **Thesis writer** | Thesis mode, Typst output, statistical analysis |
| **ML engineer** | General benchmark, model comparison, latency testing |
| **New developer** | This documentation, glossary, small test runs |
| **DevOps** | Operational metrics, RAM/storage benchmarks |

## Next Steps

- Read [02 — Models](02-models.md) to understand what each model does
- Read [04 — Pipeline](04-pipeline.md) to understand the full workflow
- Run `uv run benchmark benchmark --help` to see CLI options
