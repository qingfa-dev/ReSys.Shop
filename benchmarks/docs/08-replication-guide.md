# 08 — Replication Guide

Complete, step-by-step guide to replicate all benchmark results from scratch.

---

## 1. Prerequisites

### Hardware

| minimum | recommended |
|---|---|
| 8 GB RAM | 16 GB RAM |
| 4 CPU cores | 8 CPU cores |
| 20 GB free disk | 50 GB free disk (model weights ~5 GB cached) |
| CPU-only works | NVIDIA GPU 8 GB+ VRAM (10× faster) |

### Software

```bash
# Python 3.12+
python --version  # ≥ 3.12

# uv package manager
curl -LsSf https://astral.sh/uv/install.sh | sh

# Podman or Docker (for pgvector)
podman --version  # or: docker --version

# Git
git --version
```

---

## 2. Repository Setup

```bash
git clone <repo-url> ReSys.Shop
cd ReSys.Shop/benchmarks
uv sync --extra dev
```

Dependencies installed:
- ML: `torch`, `torchvision`, `transformers`, `open-clip-torch`, `fashion-clip`
- Data: `numpy`, `Pillow`, `pandas`
- Retrieval: `faiss-cpu`, `psycopg[binary]`, `pgvector`
- CLI: `rich`, `typer`, `tqdm`
- Reporting: `matplotlib`, `seaborn`
- System: `psutil`

---

## 3. Data Preparation

### 3.1 Download Dataset

```bash
# Download Fashion Product Images Small (~44K images, ~4 GB)
uv run scripts/download_dataset.py --dataset fashion-product-images-small
```

Expected: `data/raw/fashion-product-images-small/` with:
```
images/          # 44,441 JPEG files
styles.csv       # 44,441 rows × 10 columns
splits/
    train.json   # 35,552 samples (80%)
    test.json    # 8,889 samples (20%)
```

### 3.2 Create 5K Subset (Thesis Protocol)

The thesis protocol uses 5,000 images. To create the subset:

```bash
mkdir -p /tmp/thesis_5k/images

# Take first 5,000 rows + header
head -5001 data/raw/fashion-product-images-small/styles.csv > /tmp/thesis_5k/styles.csv

# Copy images
tail -n +2 /tmp/thesis_5k/styles.csv | head -5000 | while IFS=, read -r id rest; do
  src="data/raw/fashion-product-images-small/images/$id.jpg"
  if [ -f "$src" ]; then cp "$src" /tmp/thesis_5k/images/; fi
done
```

Verify:
```bash
ls /tmp/thesis_5k/images/ | wc -l    # → 5000
wc -l /tmp/thesis_5k/styles.csv       # → 5001 (1 header + 5000 data)
```

---

## 4. Running Thesis Benchmark (In-Memory, No PGVector)

This mode uses in-memory cosine similarity. **No PostgreSQL required.** Produces all retrieval and efficiency metrics.

### Command

```bash
uv run benchmark thesis \
  --dataset-root /tmp/thesis_5k \
  --output outputs/thesis \
  --folds 3 \
  --k 5,10,20 \
  --device auto \
  --seed 42
```

### Options

| flag | default | description |
|---|---|---|
| `--dataset-root` | `data/raw/deepfashion` | Path to directory with `styles.csv` and `images/` |
| `--output` | `outputs/thesis` | Where results go |
| `--folds` | 3 | Number of CV folds |
| `--k` | `5,10,20` | K values for P@K / R@K |
| `--device` | `auto` | `cpu`, `cuda`, `mps`, or `auto` |
| `--models` | `all` | Comma-separated: `fashion-clip,resnet-50` or `all` |
| `--batch-size` | 64 | Images per forward pass |
| `--no-cache` | false | Skip embedding cache (recompute all) |
| `--seed` | 42 | Random seed for reproducibility |

### Expected Runtime

| device | runtime |
|---|---|
| NVIDIA GPU (8 GB) | 30–60 min |
| CPU (8 cores) | 2–4 hours |
| CPU (4 cores) | 4–6 hours |

### How Caching Works

Embeddings are cached as `.npz` files in `data/cache/`. Cache keys include model slug and dataset name (e.g., `fold_0_test`, `fold_1_train`). Re-running uses cached embeddings — only evaluation runs again.

**Clear cache:** `rm data/cache/*.npz`

**Reuse cache across runs:**
```bash
# First run — computes embeddings
uv run benchmark thesis --dataset-root /tmp/thesis_5k --k 5,10,20

# Second run with different K values — reuses cached embeddings
uv run benchmark thesis --dataset-root /tmp/thesis_5k --k 1,5,10,20
```

### Outputs

```
outputs/thesis/
├── splits/
│   ├── fold_0_test.json      # Query samples for fold 0
│   ├── fold_0_train.json     # Gallery samples for fold 0
│   ├── fold_1_test.json
│   ├── fold_1_train.json
│   ├── fold_2_test.json
│   └── fold_2_train.json
├── tables/
│   ├── thesis_aggregate.typ    # Retrieval effectiveness (P@K, R@K, mAP)
│   └── thesis_efficiency.typ   # Operational performance (latency, throughput, storage)
├── results/
│   └── thesis_results.json     # Complete fold-level data + stats
└── logs/
    └── thesis.log
```

### Include in Thesis

```typst
// In your .typ file:
#include "benchmarks/outputs/thesis/tables/thesis_aggregate.typ"
#include "benchmarks/outputs/thesis/tables/thesis_efficiency.typ"
```

---

## 5. Running Pipeline Benchmark (With PGVector)

This mode extends the thesis protocol with production-database metrics: vector ingestion, index building, approximate query latency, and recall comparison.

### 5.1 Start PostgreSQL + pgvector

#### Option A: Podman (preferred on most Linux)

```bash
podman run -d \
  --name pgvector-benchmark \
  -e POSTGRES_USER=benchmark \
  -e POSTGRES_PASSWORD=benchmark \
  -e POSTGRES_DB=benchmark \
  -p 5432:5432 \
  -v $(pwd)/infra/postgres/init.sql:/docker-entrypoint-initdb.d/init.sql:Z \
  docker.io/pgvector/pgvector:pg16
```

#### Option B: Docker

```bash
docker run -d \
  --name pgvector-benchmark \
  -e POSTGRES_USER=benchmark \
  -e POSTGRES_PASSWORD=benchmark \
  -e POSTGRES_DB=benchmark \
  -p 5432:5432 \
  -v $(pwd)/infra/postgres/init.sql:/docker-entrypoint-initdb.d/init.sql \
  pgvector/pgvector:pg16
```

#### Wait for PostgreSQL

```bash
# Using wait script:
./infra/postgres/wait-for-pg.sh

# Or manually:
podman exec pgvector-benchmark pg_isready -U benchmark -d benchmark
# Expected: "/var/run/postgresql:5432 - accepting connections"
```

#### Verify pgvector Extension

```bash
podman exec pgvector-benchmark psql -U benchmark -d benchmark \
  -c "SELECT extname, extversion FROM pg_extension WHERE extname='vector';"
```

### 5.2 Create Pipeline Tables

The `init.sql` creates schema tables (`product_embeddings_512`, `product_embeddings_768`, `benchmark_runs`, etc.), but the pipeline needs per-dimension tables:

```bash
podman exec pgvector-benchmark psql -U benchmark -d benchmark <<SQL
CREATE TABLE IF NOT EXISTS products_512  (id TEXT PRIMARY KEY, label TEXT NOT NULL, embedding vector(512));
CREATE TABLE IF NOT EXISTS products_1280 (id TEXT PRIMARY KEY, label TEXT NOT NULL, embedding vector(1280));
CREATE TABLE IF NOT EXISTS products_2048 (id TEXT PRIMARY KEY, label TEXT NOT NULL, embedding vector(2048));
SQL
```

### 5.3 Run Pipeline Benchmark

```bash
uv run benchmark pipeline \
  --dataset-root /tmp/thesis_5k \
  --output outputs/pipeline \
  --folds 3 \
  --k 5,10,20 \
  --device auto \
  --seed 42 \
  --conn-string "postgresql://benchmark:benchmark@localhost:5432/benchmark" \
  --pg-lists 100 \
  --models all
```

### Pipeline-Specific Options

| flag | default | description |
|---|---|---|
| `--conn-string` | `postgresql://benchmark:benchmark@localhost:5432/benchmark` | PostgreSQL connection |
| `--pg-lists` | 100 | IVFFlat index lists parameter |

### What Happens Per Fold

```
Fold N:
  1. Load model                         → load_time_ms
  2. Generate embeddings (train+test)   → cached to data/cache/
  3. Exact cosine evaluation            → mAP, P@K, R@K (baseline)
  4. Batch insert into pgvector         → ingestion_time_s
  5. Build IVFFlat index                → index_build_time_s
  6. Query pgvector (all test vectors)  → query_latency_ms
  7. Compare pgvector vs exact results  → recall@K
```

### Graceful Degradation

If PostgreSQL is not available, the pipeline:
- Logs a warning: `PGVector not available: ...`
- Returns zeros for all pgvector metrics
- Continues with exact cosine evaluation (mAP, P@K, R@K still valid)

### Expected Runtime

Additional pgvector overhead per fold: 10–30 seconds (ingestion + index build + queries).

| device | runtime (pipeline) |
|---|---|
| GPU | 35–70 min |
| CPU (8 cores) | 2.5–4.5 hours |

### Outputs

```
outputs/pipeline/
├── splits/                            # Stratified 3-fold splits
├── tables/
│   └── pipeline_production.typ        # PGVector metrics for thesis
├── results/
│   └── pipeline_results.json          # Full trace: fold data + aggregates + production metrics
└── logs/
    └── pipeline.log                   # Execution trace
```

### Include in Thesis

```typst
#include "benchmarks/outputs/pipeline/tables/pipeline_production.typ"
```

---

## 6. Interpreting Results

### 6.1 Key Metrics

| metric | meaning | range | good value |
|---|---|---|---|
| **mAP** | Mean Average Precision | 0–1 | ≥ 0.80 |
| **P@K** | Precision at K — % of top-K that are relevant | 0–1 | ≥ 0.85 |
| **R@K** | Recall at K — % of relevant items found in top-K | 0–1 | ≥ 0.10 (low because many relevant items per query) |
| **Latency (ms)** | Mean embedding time per image | ms | ≤ 100ms |
| **Throughput** | Images per second (batch) | img/s | ≥ 10 |
| **Storage** | Embedding size per 5K images | MB | Lower is better |
| **PG Recall@K** | Approximate search accuracy vs exact | 0–1 | ≥ 0.90 (HNSW), ≥ 0.65 (IVFFlat) |
| **PG Query Latency** | Mean pgvector query time | ms | ≤ 10ms |

### 6.2 Why R@K is Low

With category-based ground truth (masterCategory + subCategory), each query has ~30 relevant items in a gallery of ~3,300. R@10 = 10/30 ≈ 0.33 maximum. The observed values (~0.06) suggest most relevant items are not in the top 10 — this is expected for fashion retrieval where category matching is a coarse-grained relevance proxy.

### 6.3 PGVector Recall < 1.0

IVFFlat is approximate — it doesn't scan the full database. Recall@10 ≈ 0.65–0.72 means the approximate index finds 65-72% of the top-10 that exact search would find. This is expected for IVFFlat with only 100 lists on 3,300 vectors. For higher recall:
- Increase `--pg-lists` (e.g., 200 → slower index build, higher recall)
- Use HNSW index instead (not yet supported in pipeline)
- Accept the trade-off: 2.7ms query vs 30+ms for exact search

---

## 7. Complete Run Script

Save as `run_full_benchmark.sh`:

```bash
#!/usr/bin/env bash
set -euo pipefail

echo "=== 1. Creating 5K dataset subset ==="
mkdir -p /tmp/thesis_5k/images
head -5001 data/raw/fashion-product-images-small/styles.csv > /tmp/thesis_5k/styles.csv
tail -n +2 /tmp/thesis_5k/styles.csv | head -5000 | while IFS=, read -r id rest; do
  src="data/raw/fashion-product-images-small/images/$id.jpg"
  if [ -f "$src" ]; then cp "$src" /tmp/thesis_5k/images/; fi
done
echo "  Created $(ls /tmp/thesis_5k/images/ | wc -l) images"

echo "=== 2. Starting pgvector ==="
podman run -d --name pgvector-benchmark --replace \
  -e POSTGRES_USER=benchmark -e POSTGRES_PASSWORD=benchmark \
  -e POSTGRES_DB=benchmark -p 5432:5432 \
  -v "$(pwd)/infra/postgres/init.sql:/docker-entrypoint-initdb.d/init.sql:Z" \
  docker.io/pgvector/pgvector:pg16
./infra/postgres/wait-for-pg.sh

echo "=== 3. Creating pipeline tables ==="
podman exec pgvector-benchmark psql -U benchmark -d benchmark <<SQL
CREATE TABLE IF NOT EXISTS products_512  (id TEXT PRIMARY KEY, label TEXT NOT NULL, embedding vector(512));
CREATE TABLE IF NOT EXISTS products_1280 (id TEXT PRIMARY KEY, label TEXT NOT NULL, embedding vector(1280));
CREATE TABLE IF NOT EXISTS products_2048 (id TEXT PRIMARY KEY, label TEXT NOT NULL, embedding vector(2048));
SQL

echo "=== 4. Running pipeline benchmark ==="
uv run benchmark pipeline \
  --dataset-root /tmp/thesis_5k \
  --output outputs/pipeline \
  --folds 3 --k 5,10,20 \
  --device auto --seed 42 \
  --conn-string "postgresql://benchmark:benchmark@localhost:5432/benchmark"

echo "=== Done ==="
echo "Results: outputs/pipeline/results/pipeline_results.json"
echo "Tables:  outputs/pipeline/tables/pipeline_production.typ"
echo "Logs:    outputs/pipeline/logs/pipeline.log"
```

---

## 8. Troubleshooting

### "No module named 'benchmark'"

```bash
uv sync --extra dev
```

### "PGVector not available: relation does not exist"

Run the table creation SQL in §5.2.

### "column cannot have more than 2000 dimensions for ivfflat index"

ResNet-50 (2048-d) exceeds pgvector's IVFFlat dimension limit. The pipeline gracefully degrades — ResNet-50 pgvector metrics show 0.0. Exact cosine metrics are still valid. Workarounds:
1. Use HNSW index instead (no dimension limit) — not yet in pipeline
2. Exclude ResNet-50 from pipeline: `--models fashion-clip,clip-generic,efficientnet-b0`

### "out of memory" during embedding generation

Reduce batch size: `--batch-size 16`

### Cache errors / stale cache

```bash
rm -rf data/cache/*.npz
```

### Port 5432 already in use

```bash
podman stop pgvector-benchmark && podman rm pgvector-benchmark
# Or use a different port: -p 5433:5432 and --conn-string with :5433
```

### Disk space for model weights

First run downloads ~5 GB of model weights to `~/.cache/huggingface/` and `~/.cache/torch/`. Ensure sufficient space.

---

## 9. Reproducibility Checklist

- [ ] Pin Python version: `cat .python-version` → `3.12`
- [ ] Lock dependencies: `uv lock` → committed `uv.lock`
- [ ] Use fixed seed: `--seed 42`
- [ ] Document hardware: `lscpu | grep "Model name"` and `nvidia-smi` (if GPU)
- [ ] Save split files: `outputs/pipeline/splits/fold_*.json`
- [ ] Save full JSON trace: `outputs/pipeline/results/pipeline_results.json`
- [ ] Verify Typst compilation: `typst compile outputs/pipeline/tables/pipeline_production.typ`
- [ ] Document model versions: see dependencies in `uv.lock`
- [ ] Note pgvector version: `podman exec pgvector-benchmark psql -U benchmark -d benchmark -c "SELECT extversion FROM pg_extension WHERE extname='vector';"`
- [ ] Record runtime: check `pipeline.log` timestamps
