# 04 — Pipeline

A step-by-step walkthrough of how the benchmark turns raw images into final reports.

## The Pipeline at a Glance

```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│   Dataset   │───▶│  Embedding  │───▶│  Retrieval  │───▶│   Metrics   │
│   Loader    │    │  Generator  │    │   Engine    │    │  Computer   │
└─────────────┘    └─────────────┘    └─────────────┘    └─────────────┘
       │                  │                  │                  │
       ▼                  ▼                  ▼                  ▼
  styles.csv         Model Adapter      Cosine Similarity   Precision@K
  images/            Cache (.npz)       Top-K Search        Recall@K
                                                           mAP
                                                           ↓
                                                    ┌─────────────┐
                                                    │   Reporter  │
                                                    │  (Typst/PNG/JSON)
                                                    └─────────────┘
```

---

## Step 1: Dataset Loading

**What happens:** Parse `styles.csv` and build a list of all images with their metadata.

**Input:**
```
data/raw/fashion-product-images-small/
├── images/
│   ├── 1163.jpg
│   ├── 1165.jpg
│   └── ... (5,000 images)
└── styles.csv
```

**Output:** A list of `Sample` objects:
```python
Sample(
    image_path=Path("images/1163.jpg"),
    label="Apparel/Topwear",      # masterCategory/subCategory
    product_id="1163",
    split="test"
)
```

**Key code:** `src/benchmark/datasets/loader.py`

---

## Step 2: Ground Truth Building

**What happens:** Decide which images are "similar" to each other.

**Rule:** Two images are relevant if they share the same `masterCategory` + `subCategory` + `baseColour`. Fallback: if `subCategory` or `baseColour` is missing, fall back to the coarser grouping.

**Example:**
- Image 1163: `Apparel` / `Topwear` / `T-shirt` / `Black`
- Image 1165: `Apparel` / `Topwear` / `T-shirt` / `Black`
- **Result:** 1163 and 1165 are relevant to each other (same category + colour)

- Image 1400: `Apparel` / `Topwear` / `T-shirt` / `Blue`
- **Result:** NOT relevant to 1163 (different colour)

- Image 2000: `Footwear` / `Shoes` / `Sneakers` / `White`
- **Result:** NOT relevant to 1163 (different category AND colour)

**Output:** A dictionary mapping each image ID to a set of relevant image IDs.

**Key code:** `src/benchmark/datasets/ground_truth.py`

---

## Step 3: Split Generation

**What happens:** Divide the dataset into train (gallery) and test (query) sets.

**For general benchmark:**
- Single 80/20 split
- Or use provided JSON split files

**For thesis mode (3-fold CV):**
```
Fold 0:  Test = images 0-1666,     Train = images 1667-4999
Fold 1:  Test = images 1667-3333,  Train = images 0-1666 + 3334-4999
Fold 2:  Test = images 3334-4999,  Train = images 0-3333
```

**Stratification:** Each fold maintains the same category proportions as the full dataset.

**Output:** JSON split files:
```
outputs/thesis/splits/
├── fold_0_train.json
├── fold_0_test.json
├── fold_1_train.json
├── fold_1_test.json
├── fold_2_train.json
└── fold_2_test.json
```

**Key code:** `src/benchmark/datasets/ground_truth.py`

---

## Step 4: Model Loading

**What happens:** Download model weights and initialize the neural network.

**Process:**
1. Check if weights are cached locally (~/.cache/huggingface/)
2. If not, download from HuggingFace Hub or PyTorch Hub
3. Load model into memory (RAM/GPU)
4. Set to evaluation mode (disables dropout, freezes batch norm)
5. Record load time

**Example (Fashion-CLIP):**
```python
from transformers import CLIPModel, CLIPProcessor
processor = CLIPProcessor.from_pretrained("patrickjohncyh/fashion-clip")
model = CLIPModel.from_pretrained("patrickjohncyh/fashion-clip").to("cuda")
model.eval()
```

**Output:** A loaded model ready for inference.

**Key code:** `src/benchmark/models/*.py` (each model's `load()` method)

---

## Step 5: Embedding Generation

**What happens:** Convert all images into numerical vectors (embeddings).

**Process:**
1. Load a batch of images (default 64)
2. Preprocess each image (resize to 224×224, normalize)
3. Run through the model
4. L2-normalize the output vectors
5. Store as float32 numpy array

**Shape:** `(N, D)` where:
- `N` = number of images (e.g., 5,000)
- `D` = embedding dimension (e.g., 512 for Fashion-CLIP)

**Cache:** Embeddings are saved to disk:
```
data/cache/
└── fashion-clip__deepfashion.npz
```

If the cache exists and `--use-cache` is enabled, skip generation entirely.

**Output:**
- `embeddings`: numpy array of shape `(N, D)`
- `samples`: list of `Sample` objects aligned with embedding rows

**Key code:** `src/benchmark/embeddings/generator.py`

---

## Step 6: Retrieval

**What happens:** For each query image, find the most similar gallery images.

**Algorithm:**
1. Compute cosine similarity between query embedding and all gallery embeddings
2. Sort by similarity (highest first)
3. Return top-K indices

**Math:**
```python
# For L2-normalized vectors, cosine similarity = dot product
similarities = gallery_embeddings @ query_embedding  # shape: (N_gallery,)
top_k_indices = np.argsort(similarities)[-K:][::-1]
```

**Exclude self:** If the query is in the gallery, skip it (don't return the exact same image).

**Output:** For each query, a list of K gallery indices, ranked by similarity.

**Key code:** `src/benchmark/retrieval/cosine.py`

---

## Step 7: Metric Computation

**What happens:** Compare retrieved results against ground truth.

**For each query:**
1. Get the top-K retrieved image labels
2. Count how many are in the ground-truth relevant set
3. Compute Precision@K, Recall@K, AP

**Example:**

Query: image 1163 (black T-shirt, relevant set = {1165, 1167, 1200, ...})

Retrieved top-5: [1165, 2000, 1167, 3000, 1200]

- Relevant in top-5: 1165 ✅, 1167 ✅, 1200 ✅ → 3 hits
- Precision@5 = 3/5 = 0.60
- Recall@5 = 3/50 = 0.06 (assuming 50 total relevant items)

**Aggregation:**
- Average across all queries → mean Precision@K, mean Recall@K, mAP
- Repeat for each fold
- Compute mean ± SD across folds

**Output:** Per-fold and aggregated metrics.

**Key code:**
- `src/benchmark/metrics/precision.py`
- `src/benchmark/metrics/recall.py`
- `src/benchmark/metrics/map.py`
- `src/benchmark/metrics/ndcg.py`

---

## Step 8: Efficiency Measurement

**What happens:** Measure how fast and resource-intensive the model is.

**Latency:**
```python
for i in range(100):
    t0 = time.perf_counter()
    model.embed(image)
    latency_ms = (time.perf_counter() - t0) * 1000
```

**Throughput:**
```python
t0 = time.perf_counter()
for batch in batches:
    model.embed_batch(batch)
throughput = total_images / (time.perf_counter() - t0)
```

**RAM:** Sample during batch inference.

**Storage:** `embeddings.nbytes / 1024 / 1024` (total MB).

**Output:** Operational metrics dictionary.

**Key code:**
- `src/benchmark/metrics/latency.py`
- `src/benchmark/metrics/throughput.py`

---

## Step 9: Statistical Analysis

**What happens:** Compare models statistically.

**For thesis mode:**
1. Collect fold-level mAP scores for each model
2. Compute mean ± SD
3. Compute Cohen's d (effect size) between Fashion-CLIP and each competitor
4. Compute bootstrap 95% CI for mean mAP

**Example output:**
```json
{
  "fashion-clip vs resnet-50": {
    "cohens_d": 1.23,
    "interpretation": "Large effect — Fashion-CLIP is substantially better"
  }
}
```

**Key code:** `src/benchmark/evaluation/stats.py`

---

## Step 10: Report Generation

**What happens:** Turn numbers into human-readable outputs.

**Typst tables** (`outputs/thesis/tables/thesis_results.typ`):
```typst
#figure(
  caption: [Retrieval Effectiveness — Fashion Retrieval Benchmark],
  table(
    columns: 7,
    table.header([Model], [P@5], [R@5], [P@10], [R@10], [mAP]),
    [Fashion-CLIP], [0.72 ± 0.02], [0.15 ± 0.01], ...
  )
)
```

**Pareto chart** (`outputs/thesis/figures/pareto_frontier.png`):
- X-axis: mean latency (ms)
- Y-axis: mAP
- Each model is a point
- Top-left = best (fast and accurate)

**JSON** (`outputs/thesis/thesis_stats.json`):
- Raw fold-level data
- Statistical test results
- Machine-readable for further analysis

**Key code:** `src/benchmark/reporting/thesis.py`

---

## Full Data Flow Summary

```
styles.csv + images/
    ↓
[Dataset Loader] → List[Sample]
    ↓
[Ground Truth Builder] → Dict[id, Set[relevant_ids]]
    ↓
[Split Generator] → fold_0_train.json, fold_0_test.json, ...
    ↓
For each fold, for each model:
    [Model Loader] → loaded PyTorch model
        ↓
    [Embedding Generator] → (N, D) numpy array + cache
        ↓
    [Retrieval Engine] → top-K indices per query
        ↓
    [Metric Computer] → P@K, R@K, mAP, nDCG
        ↓
    [Efficiency Measurer] → latency, throughput, RAM, storage
        ↓
[Aggregator] → mean ± SD across folds
    ↓
[Statistical Analyzer] → Cohen's d, bootstrap CI
    ↓
[Reporter] → Typst tables, PNG charts, JSON summary
```

---

## Running the Pipeline

### General Benchmark (One Command)
```bash
uv run benchmark benchmark \
  --dataset-root data/raw/fashion-product-images-small \
  --split-file data/splits/test.json \
  --models fashion-clip,efficientnet-b0 \
  --k 5,10,20
```

### Thesis Protocol (One Command)
```bash
uv run benchmark thesis \
  --dataset-root data/raw/fashion-product-images-small \
  --output outputs/thesis \
  --folds 3
```

### Step-by-Step (Manual)
```bash
# 1. Generate embeddings
uv run benchmark benchmark --models fashion-clip --no-cache

# 2. Run evaluation only (if embeddings cached)
# (Not directly supported; use full command)

# 3. Generate reports from cached metrics
uv run benchmark report --format all
```

---

## Pipeline Design Principles

1. **Each step is isolated** — You can replace the model without touching the evaluator.
2. **Caching at every layer** — Embeddings, metrics, and reports are all cached.
3. **Deterministic** — Same seed → same splits → same results.
4. **Composable** — General benchmark and thesis mode share 90% of code.
