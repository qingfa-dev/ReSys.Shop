# 08 — Visual Similarity Pipeline

How the benchmark and the production embedding service solve the same problem:
finding visually similar fashion products.

---

## 1. The Core Idea: Visual Similarity as Geometry

Every model in this project converts an image into a list of numbers called an
**embedding vector** — a point in a high-dimensional space.

The fundamental assumption of visual similarity is:

> **If two images look similar, their embedding vectors will be close together
> in this space. If they look different, their vectors will be far apart.**

This reduces the visual similarity problem to a geometry problem: given a query
image's vector, find the nearest neighbours in the vector space.

### Why This Works

Neural networks trained on massive image datasets learn hierarchical features:
- Early layers detect edges, textures, colours
- Middle layers detect shapes, patterns, parts
- Late layers detect concepts: "this looks like a sleeve", "this looks like a collar"

The embedding vector is the activation of the final hidden layer — a compressed
summary of everything the model "knows" about the image. Models trained to
distinguish millions of images naturally learn that similar-looking images have
similar final-layer activations.

### The Cosine Similarity Invariant

All embeddings in this project are **L2-normalised** to unit length (`||v|| = 1`).
This means cosine similarity equals dot product:

```
sim(q, g) = (q · g) / (||q|| * ||g||) = q · g    (since ||q|| = ||g|| = 1)
```

The similarity score ranges from -1 (opposite) to +1 (identical). In practice,
fashion embedding similarities cluster around 0.3–0.9.

---

## 2. The Embedding Pipeline (Conceptual)

Every embedding pipeline — whether in the benchmark or production — follows the
same three steps:

```
Raw Image → Preprocess → Model Forward → Normalise → Embedding Vector
```

### Step 1: Preprocess

Convert the image into the tensor format the model expects:

| Operation | Detail |
|-----------|--------|
| Resize | Typically 224×224 or 256×256 pixels |
| Centre crop | Square crop to model's expected input size |
| Convert to tensor | PIL Image → float32 tensor, values in [0, 1] |
| Normalise | Subtract mean, divide by std (ImageNet stats: `[0.485, 0.456, 0.406]`) |
| Add batch dim | Shape `(1, 3, H, W)` for single-image inference |

### Step 2: Model Forward

Run the preprocessed tensor through the neural network under `torch.no_grad()`
(no gradient computation — inference only). The output is a raw feature vector
from the model's final layer.

Different model families produce different raw outputs:
- **CLIP / FashionCLIP**: `model.get_image_features(pixel_values)` or `outputs.image_embeds`
- **EfficientNet / ResNet**: `model(image_tensor)` → final pooling layer
- **DINOv2**: `model.forward_features(pixel_values)` → `x_norm_poolt`

### Step 3: Normalise

Apply L2 normalisation so the output vector has unit length:

```python
norm = np.linalg.norm(raw_vector)
embedding = raw_vector / (norm + 1e-9)  # epsilon prevents div-by-zero
```

This ensures cosine similarity is a simple dot product and that all embeddings
from different models live on the same unit hypersphere (though at different
dimensionalities).

### The Result

A single 1-D array of float32 numbers:

| Model | Dimensions | Size per image |
|-------|-----------|----------------|
| FashionCLIP | 512 | 2 KB |
| CLIP ViT-B/32 | 512 | 2 KB |
| CLIP ViT-B/16 | 512 | 2 KB |
| CLIP ViT-L/14 | 768 | 3 KB |
| SigLIP | 768 | 3 KB |
| EVA-CLIP | 768 | 3 KB |
| DINOv2 ViT-S/14 | 384 | 1.5 KB |
| ConvNeXt-Tiny | 768 | 3 KB |
| EfficientNet-B0 | 1280 | 5 KB |
| ResNet-50 | 2048 | 8 KB |

---

## 3. The Retrieval Pipeline (Conceptual)

Once every image in the gallery has an embedding vector, retrieval proceeds as:

```
Query Image → [Embedding Pipeline] → Query Vector (1 × D)
                                          ↓
                              ┌──────────────────────┐
                              │  Compute Similarity  │
                              │  query · gallery^T   │
                              │  → similarity scores │
                              │     (1 × N_gallery)   │
                              └──────────────────────┘
                                          ↓
                              ┌──────────────────────┐
                              │  Sort by score        │
                              │  Take top-K indices   │
                              └──────────────────────┘
                                          ↓
                              Ranked list of similar product IDs
```

### In-Memory (Benchmark)

The benchmark loads all gallery embeddings into a NumPy matrix of shape
`(N_gallery, D)` and computes:

```python
scores = gallery_matrix @ query_vector  # dot product, shape (N_gallery,)
top_k = np.argpartition(scores, -K)[-K:]
top_k = top_k[np.argsort(scores[top_k])[::-1]]
```

`argpartition` is O(N) instead of O(N log N), making this fast even for
100K+ image galleries.

### In-Database (Production)

The production service stores embedding vectors in a PostgreSQL column with
the **pgvector** extension:

```sql
CREATE TABLE product_image_embeddings (
    product_id  TEXT PRIMARY KEY,
    embedding   vector(512)     -- or vector(2048) for ResNet, etc.
);

-- Similarity search
SELECT product_id
FROM product_image_embeddings
ORDER BY embedding <=> :query_vec     -- cosine distance operator
LIMIT 20;
```

The `<=>` operator computes `1 - cosine_similarity`, so lower = more similar.
pgvector uses IVFFlat or HNSW indexing to avoid a full table scan.

---

## 4. How the Benchmark Pipeline Evaluates Models

The benchmark pipeline lives in `src/benchmark/` and is organised as a
sequential data flow:

```
                                 ┌─ Fold 0 ─┐
Dataset ──▶ Split ──▶ 3 folds ──┼─ Fold 1 ─┼──▶ For each fold:
                                 └─ Fold 2 ─┘    For each model:
                                                    │
                                  ┌─────────────────┘
                                  ▼
                           BenchmarkRunner
                                  │
                    ┌─────────────┴─────────────┐
                    ▼                           ▼
             Query Embeddings            Gallery Embeddings
             (test split)                (train split)
                    ▼                           ▼
                    └──────▶ Retrieval ◀────────┘
                                    │
                          Cosine similarity, top-K
                                    │
                                    ▼
                              Evaluator
                                    │
                         ┌──────────┼──────────┐
                         ▼          ▼          ▼
                    Precision@K  Recall@K     mAP
                                    │
                                    ▼
                              Aggregator
                          (mean ± SD, Cohen's d,
                           bootstrap CI)
                                    │
                                    ▼
                              Reporter
                          (Typst, JSON, CSV, PNG)
```

### The Ground Truth Problem

To measure accuracy, we need to define what "similar" means for the dataset.
The benchmark uses a **category-based relevance rule**:

> Two fashion products are similar if they share the same
> `masterCategory` + `subCategory`.

For example, in the Fashion Product Images Small dataset:
- Product 1163: `Apparel/Topwear/T-shirt/Black`
- Product 1165: `Apparel/Topwear/T-shirt/Blue`

Since both share `Apparel/Topwear`, they are treated as relevant to each other.
A good embedding model should place them close together.

This is an imperfect ground truth — a black T-shirt and a blue T-shirt are indeed
similar, but so are a blue T-shirt and blue sneakers (both blue, but different
categories). The thesis explicitly acknowledges this limitation.

### How Each Metric Answers a Specific Question

| Metric | Definition | What It Tells You |
|--------|-----------|-------------------|
| **Precision@K** | Of the top-K results, how many are relevant? | How trustworthy are the top results? |
| **Recall@K** | Of all relevant items, how many are in the top-K? | Does the model find everything it should? |
| **mAP** | Average precision across all recall levels | Overall ranking quality — is the order correct? |
| **nDCG@K** | Discounted cumulative gain with ideal ranking | Are the most relevant items at the very top? |

### Why 3-Fold Cross-Validation

The thesis protocol uses 3-fold stratified CV:

```
Fold 0: Test = 1/3 of data, Gallery = 2/3
Fold 1: Test = a different 1/3, Gallery = rest
Fold 2: Test = remaining 1/3, Gallery = rest
```

This means:
- Every image serves as a query exactly once (test)
- Every image serves in the gallery exactly twice (train)
- Results are averaged across all 3 folds

Stratification ensures each fold has the same category proportions as the full
dataset — if 50% of images are `Apparel`, each fold's test set is ~50% `Apparel`.

### Why 4 Models

The thesis compares exactly 4 models, each testing a specific hypothesis:

| Model | Hypothesis | Why It Matters |
|-------|-----------|----------------|
| **FashionCLIP** | Fashion-tuned CLIP should win on mAP | Tests if domain-specific fine-tuning helps |
| **CLIP-generic** | Text-image pretraining beats pure vision | Tests the CLIP paradigm vs. CNNs |
| **EfficientNet-B0** | Best efficiency/accuracy trade-off | Tests if Pareto-optimal model exists |
| **ResNet-50** | High-dim embeddings (2048-d) help recall | Tests if more dimensions = better retrieval |

### What a "Good" Result Looks Like

A hypothetical result from the thesis pipeline:

```
FashionCLIP     — mAP = 0.823 ± 0.005,  P@5 = 0.714 ± 0.006
CLIP-generic    — mAP = 0.751 ± 0.007,  P@5 = 0.642 ± 0.008
EfficientNet-B0 — mAP = 0.689 ± 0.009,  P@5 = 0.573 ± 0.011
ResNet-50       — mAP = 0.702 ± 0.008,  P@5 = 0.591 ± 0.010

Cohen's d (FashionCLIP vs ResNet-50) = 1.23 → "Large effect"
```

This would mean:
- **FashionCLIP is substantially better** — 0.12 mAP points over the next best
- **ResNet-50's 2048-d vectors don't help** — CNNs underperform CLIP models
- **Effect is large** — Cohen's d > 0.8 confirms meaningful difference

---

## 5. How the Production Embedding Service Mirrors the Same Pipeline

The production embedding service (`service/Embedding/`) solves the same visual
similarity problem but as a **live HTTP sidecar** called by the .NET API.

### Architecture

```
Client (.NET API)
    │
    │  POST /embeddings  { image_url, model }
    │       or
    │  POST /embeddings/bytes  (multipart file upload)
    │
    ▼
InferenceEngine (singleton, in-memory LRU cache of loaded models)
    │
    ├── get_embedder("fashion_clip")
    │       │
    │       └── FashionCLIPEmbedder (loaded once, cached forever)
    │               │
    │               └── BaseEmbedder.extract(image)
    │                       │
    │                       ├── _load_image(image_input)
    │                       │       URL → httpx download → PIL Image
    │                       │       or bytes → PIL Image
    │                       │
    │                       ├── _forward(image)
    │                       │       CLIPProcessor(image) → pixel_values
    │                       │       model.get_image_features(pixel_values)
    │                       │       → raw tensor
    │                       │
    │                       └── _normalize(raw_tensor)
    │                               detach().cpu().numpy()
    │                               L2 normalize
    │                               → List[float]
    │
    ▼
Response: { vector: [0.012, -0.034, ...], model: "fashion_clip", duration_ms: 42 }
```

### Same Three-Step Pipeline

Compare the benchmark's `FashionClipModel.embed(image)` with the service's
`FashionCLIPEmbedder._forward(image)` + `_normalize()`:

| Step | Benchmark (`src/benchmark/models/fashion_clip.py`) | Service (`service/Embedding/src/models/vision/clip.py`) |
|------|---------------------------------------------------|------------------------------------------------------|
| Load model | `CLIPModel.from_pretrained("patrickjohncyh/fashion-clip")` | `CLIPModel.from_pretrained("patrickjohncyh/fashion-clip")` |
| Preprocess | `processor(images=img, return_tensors="pt")` | `processor(images=image, return_tensors="pt")` |
| Forward | `model.get_image_features(**inputs)` | `model.get_image_features(**inputs)` |
| Normalise | `F.normalize(emb, dim=-1)` | `np.linalg.norm(features)` + divide |
| Output | `np.ndarray` (float32, 512-d) | `List[float]` (512 elements) |
| no_grad | `@torch.inference_mode()` | `with torch.no_grad():` + `@torch.inference_mode()` |

The core logic is **nearly identical**. The differences are purely about where
the code runs:
- Benchmark: offline CLI, evaluates on many models at once
- Service: HTTP server, serves one request per invocation, instrumented with
  OpenTelemetry, rate-limited, authenticated

### How Production Retrieval Works

The retrieval side in production is handled by PostgreSQL + pgvector, not by
a Python retrieval module:

```
1. Users uploads an image in the storefront ("find similar items")
2. .NET API uploads image bytes → POST /embeddings/bytes
3. Embedding service returns a 512-d vector
4. .NET runs: SELECT product_id FROM embeddings ORDER BY embedding <=> :q LIMIT 20
5. Top-20 product IDs are returned to the storefront
```

This mirrors `retrieval/cosine.py`'s `retrieve_batch()` but at the database
level — the `<=>` operator computes the same cosine distance that
`cosine_similarity()` computes in NumPy.

### Background Embedding Pipeline

There is also an async path: when an admin uploads new product images, a
**Hangfire background job** is queued:

```
UploadVariantImage handler
    → enqueue IEmbeddingOrchestrator.GenerateAndPersistAsync(image_bytes)
    → Hangfire worker calls POST /embeddings/bytes
    → stores result in product_image_embeddings table
```

This keeps the product catalogue always up-to-date — every new product image
automatically gets an embedding vector for future similarity searches.

---

## 6. End-to-End: From Benchmark to Production

The two systems form a complete feedback loop:

```
┌──────────────────────────────────────────────────────────────────────┐
│                          THESIS / RESEARCH                           │
│                                                                      │
│  Benchmark Pipeline (CLI)                                            │
│  ┌─────────────┐    ┌──────────────┐    ┌──────────────────────┐    │
│  │ Evaluate 11  │───▶│ Find best    │───▶│ Produce thesis       │    │
│  │ models on    │    │ model for    │    │ tables + charts      │    │
│  │ DeepFashion  │    │ fashion      │    │ (Typst, PNG, JSON)   │    │
│  └─────────────┘    └──────────────┘    └──────────────────────┘    │
│         │                                                            │
│         │ Answer: "Which embedding space best aligns                │
│         │          with fashion category similarity?"                 │
└─────────┼────────────────────────────────────────────────────────────┘
          │
          ▼  Informs model choice
┌──────────────────────────────────────────────────────────────────────┐
│                      PRODUCTION (ReSys.Shop)                        │
│                                                                      │
│  ┌──────────────────┐     ┌────────────────┐     ┌───────────────┐  │
│  │ Embedding Service │────▶│ pgvector DB    │────▶│ Storefront   │  │
│  │ (FastAPI sidecar) │     │ (stored         │     │ Search-By-   │  │
│  │ POST /embeddings  │     │  vectors)      │     │ Image        │  │
│  └──────────────────┘     └────────────────┘     └───────────────┘  │
│         │                      │                                       │
│         │ Background job       │ SQL query                           │
│         ▼                      ▼                                       │
│  Admin uploads           "Show me similar                           │
│  new product             items" → top-20                             │
│  → auto-embed            nearest neighbours                          │
└──────────────────────────────────────────────────────────────────────┘
```

### Summary

| | Benchmark (`benchmarks/`) | Service (`service/Embedding/`) |
|---|---|---|
| **Role** | Offline academic evaluation | Live production inference |
| **Pipeline** | Dataset → Model → Embed → Cache → Retrieval → Metrics → Report | Image → Preprocess → Model → Normalise → Response |
| **Model** | 11 adapters, evaluated individually | 5 registered models, configurable default (`fashion_clip`) |
| **Normalisation** | Each adapter normalises its own output | Shared `BaseEmbedder._normalize()` |
| **Retrieval** | In-memory NumPy (`cosine.py`) | PostgreSQL pgvector `<=>` operator |
| **Output** | Precision/Recall/mAP reports across 3-folds | 512-d vector for one image |
| **When used** | Thesis writing, model selection | Every product upload, every search-by-image request |

Both are the same pipeline at different abstraction levels — the benchmark
proves which model works best, and the production service runs that model
for every user query.

---

## 7. How the Code Maps to Each Pipeline Stage

### Benchmark Pipeline Code Map

| Stage | Package | Key Classes/Functions |
|-------|---------|-----------------------|
| CLI orchestration | `cli/main.py`, `cli/benchmark.py` | `benchmark()` command, `BenchmarkRunner` |
| Dataset loading | `datasets/loader.py` | `FashionDataset`, `Sample` dataclass |
| Ground truth | `datasets/ground_truth.py` | `GroundTruth`, `build_relevance_sets()` |
| Split generation | `datasets/ground_truth.py` | `GroundTruth.generate_splits()` |
| Model loading | `models/*.py` + `models/__init__.py` | `EmbeddingModel` ABC, `get_registry()` |
| Embedding generation | `embeddings/generator.py` | `EmbeddingGenerator`, `EmbeddingResult` |
| Cache | `embeddings/cache.py` | `exists()`, `load()`, `save()` as `.npz` |
| Retrieval | `retrieval/cosine.py` | `cosine_similarity()`, `top_k_indices()`, `retrieve_batch()` |
| Metrics | `metrics/*.py` | `mean_precision_at_k()`, `mean_recall_at_k()`, `mean_average_precision()` |
| Evaluation | `evaluation/evaluator.py` | `Evaluator`, `ModelMetrics` |
| Statistical analysis | `evaluation/stats.py` | `aggregate_mean_std()`, `bootstrap_ci()`, `cohens_d()` |
| Reporting | `reporting/*.py` | Typst tables, CSV, JSON, matplotlib charts |

### Production Service Code Map

| Stage | File | Key Classes/Functions |
|-------|------|-----------------------|
| HTTP API | `src/api/routers/embeddings.py` | `create_embedding()`, `create_embedding_from_bytes()` |
| Engine | `src/services/inference_engine.py` | `InferenceEngine.get_embedder()`, `embed()` |
| Base embedder | `src/models/base.py` | `BaseEmbedder.extract()`, `_load_image()`, `_normalize()` |
| Model registry | `src/models/registry.py` | `ModelRegistry.register()`, `get_model_class()` |
| FashionCLIP | `src/models/vision/clip.py` | `FashionCLIPEmbedder._forward()` |
| .NET client | `Module.Catalog.Features/.../Clients/` | `IInferenceClient`, `EmbeddingOrchestrator` |
| DB retrieval | SQL (in .NET) | `ORDER BY embedding <=> :query_vec LIMIT 20` |

---

## 8. Key Design Decisions

### Why Normalise Embeddings?

- **Fair comparison**: Cosine similarity is scale-invariant — normalisation removes
  the effect of one model naturally producing larger-magnitude vectors.
- **Efficient retrieval**: Dot product on unit vectors is simpler to implement and
  faster to compute than true cosine similarity (no division).
- **pgvector compatibility**: The `<=>` operator computes cosine distance, which
  requires unit-length vectors for correct results.

### Why No Training?

Both the benchmark and the production service use **pre-trained models only**.
No model is fine-tuned on the Fashion Product Images dataset. This is deliberate:
- Fine-tuning would measure the benchmark's chosen model, not the model itself
- The thesis claims are about **transfer learning** — how well a model trained
  on generic data (ImageNet, LAION-400M) generalises to fashion
- Realistically, production uses pre-trained models to avoid ongoing training costs

### Why Batch Inference in Benchmark but Single-Shot in Production?

| | Benchmark | Production |
|---|---|---|
| **Typical input** | 5,000 images (all at once) | 1 image (one user request) |
| **Batching** | Yes, batch of 64 | No, single-image endpoint |
| **Why** | Efficiency: GPU utilises parallelism on batches | Latency: users wait for one result; batching across requests adds complexity |

The production service could batch, but the added latency and complexity
outweigh the throughput gain for a single-product search.

### Why In-Memory Retrieval for Benchmark but Database Retrieval for Production?

| | Benchmark | Production |
|---|---|---|
| **Gallery size** | ~5,000–50,000 images | Potentially 100K+ products |
| **Retrieval engine** | NumPy `argpartition` | pgvector with IVFFlat/HNSW index |
| **Persistence** | None (embeddings rebuilt each run) | Permanent (stored alongside product data) |
| **Why** | Simplicity + reproducibility for academic use | Scalability + transactional integrity for production |

The benchmark could use pgvector, but rebuilding the index from scratch each
run is simpler and avoids a Docker dependency for users who just want to
run a quick comparison.

---

## 9. Running the Full Pipeline

### Benchmark (Evaluate Model Quality)

```bash
# Quick: run all 11 models on a single split
uv run benchmark run --dataset-root data/raw/deepfashion --models all

# Thesis: 4 models × 3-fold CV with statistical analysis
uv run benchmark thesis --dataset-root data/raw/deepfashion --folds 3

# Generate charts and Typst tables from cached results
uv run benchmark report --format all
```

### Production (Serve Embeddings Live)

```bash
# Start the embedding service (via Aspire or standalone)
cd service/Embedding && uv run uvicorn embedding.main:app

# Call the API
curl -X POST http://localhost:8000/embeddings \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-key" \
  -d '{"image_url": "https://example.com/product.jpg", "model": "fashion_clip"}'

# Response:
# {
#   "vector": [0.012, -0.034, ...],  # 512 elements
#   "model": "fashion_clip",
#   "duration_ms": 42
# }
```

---

## Appendix: Glossary

| Term | Definition |
|------|-----------|
| **Embedding** | A fixed-length vector (list of numbers) that represents an image in a high-dimensional space. Produced by the final layer of a neural network. |
| **L2 normalisation** | Scaling a vector so its length (Euclidean norm) equals 1. Makes cosine similarity equal to dot product. |
| **Cosine similarity** | A measure of how similar two vectors are, computed as the cosine of the angle between them. Range: [-1, 1]. For unit vectors: `dot_product(a, b)`. |
| **Cosine distance** | `1 - cosine_similarity`. Used by pgvector's `<=>` operator. |
| **Gallery** | The set of all images that can be retrieved (the "search space"). |
| **Query** | A single image used to search the gallery. |
| **Top-K** | The K most similar gallery items returned for a query. |
| **Precision@K** | Of the top-K results, the fraction that are relevant. |
| **Recall@K** | Of all relevant items in the gallery, the fraction found in the top-K. |
| **mAP** | Mean Average Precision — the average of precision scores at every rank where a relevant item appears. A single number summarising ranking quality. |
| **Stratified split** | A train/test split that preserves the original dataset's category proportions. |
| **K-fold CV** | Cross-validation where the dataset is split into K folds; each fold serves as test set exactly once. |
| **Cohen's d** | A measure of effect size — how many standard deviations apart two group means are. |
| **pgvector** | A PostgreSQL extension that adds vector similarity search with IVFFlat and HNSW indexing. |
| **Sidecar** | An independent microservice that runs alongside the main application (here, the .NET API). |
