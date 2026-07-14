# Benchmarks — Resys Fashion Shop

Academic benchmark comparing one-shot vision-language models for fashion product retrieval.

## Models compared

| Model | Backbone | Embedding Dim |
|---|---|---|
| FashionCLIP | ViT-B/32 (fashion-tuned) | 512 |
| CLIP ViT-B/32 | ViT-B/32 | 512 |
| CLIP ViT-L/14 | ViT-L/14 | 768 |
| CLIP ViT-B/16 | ViT-B/16 | 512 |
| SigLIP | ViT-B/16 | 768 |
| EVA-CLIP | EVA-02-L/14 | 768 |
| EfficientNet-B0 | EfficientNet-B0 | 1280 |
| ResNet-50 | ResNet-50 | 2048 |

## Metrics

- Precision@K (K = 1, 5, 10, 20)
- Recall@K
- mAP (mean Average Precision)
- nDCG@K
- Latency (ms/image, mean ± SD)
- Throughput (images/sec)
- RAM usage (MB)
- Storage per 1K images (MB)

## Quickstart

```bash
# Install dependencies
uv sync

# Download dataset
uv run scripts/download_dataset.py --dataset deepfashion

# Run all models (one-shot comparison)
uv run benchmark run --dataset-root data/raw/deepfashion --models all --k 10

# Run thesis benchmark (4 models × 3-fold CV)
uv run benchmark thesis --dataset-root data/raw/deepfashion --folds 3

# Generate report from stored results
uv run benchmark report --format all

# Single model
uv run benchmark run --models fashion-clip --dataset-root data/raw/deepfashion
```

## Pipeline

```
Dataset → Model → Embedding → Cache → Retrieval → Metrics → Report
```

Adding a new model requires only one new file under `src/benchmark/models/`.

## Research extension

The benchmark package now exposes research-friendly commands under the `research` subcommand.
Use `uv run benchmark research extract-features`, `uv run benchmark research evaluate`, and `uv run benchmark research pgvector-benchmark` to build feature bundles, compute split-aware metrics, and validate PGVector search behavior.

## Outputs

- `outputs/metrics/` — per-model JSON results
- `outputs/reports/` — summary CSV + Markdown
- `outputs/tables/` — Typst table snippets (auto-included in thesis)
- `outputs/figures/` — precision/recall/latency charts
