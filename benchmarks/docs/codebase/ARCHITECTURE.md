# Architecture

## Core Sections (Required)

### 1) Architectural Style

- Primary style: Layered / pipeline architecture
- Why this classification: The system is organized as a sequential pipeline (dataset → model → embedding → retrieval → metrics → report) with distinct layers, each in a separate package. CLI commands orchestrate the pipeline; individual layers are decoupled and independently testable.
- Primary constraints: (1) Must support adding new models with a single new file. (2) Embedding cache must be usable offline (no GPU required for reporting). (3) All embeddings must be L2-normalized float32.

### 2) System Flow

```text
[CLI: main.py] → [Dataset: loader.py] → [Model: base.py adapter] → [Embedding: generator.py (+ cache)] → [Retrieval: cosine.py] → [Metrics: map/ndcg/precision/recall] → [Reporting: json/csv/md/typst/charts]
```

Step-by-step flow (from `src/benchmark/cli/benchmark.py`):

1. **CLI parses args** — `benchmark()` command resolves model keys from `REGISTRY`, validates dataset existence, creates `BenchmarkRunner`
2. **Dataset loads** — `FashionDataset.load()` parses a JSON split file into `Sample` dataclasses (image_path, label, product_id)
3. **Model registry** — `get_registry(device)` creates fresh model instances with device-aware constructors; models are lazy-loaded (weights only fetched on first `embed_batch` call)
4. **Embedding generation** — `EmbeddingGenerator.generate()` checks cache first (`.npz` file), otherwise loads images in batches, calls `model.embed_batch()`, L2-normalizes, saves to cache
5. **Retrieval** — `retrieve_batch()` computes pairwise cosine similarity (dot product on unit vectors), returns top-K indices excluding self-match
6. **Metrics** — `Evaluator.evaluate()` computes P@K, R@K, nDCG@K, mAP from retrieved labels vs. ground-truth labels; optionally measures latency/throughput
7. **Reporting** — Results saved as per-model JSON, then written as comparison JSON, CSV, Markdown, Typst tables, and matplotlib charts

### 3) Layer/Module Responsibilities

| Layer or module | Owns | Must not own | Evidence |
|-----------------|------|--------------|----------|
| `cli/` | CLI argument parsing, wiring pipeline components, rich output | Model inference, metric math | `src/benchmark/cli/benchmark.py` |
| `models/` | Abstract `EmbeddingModel` interface, concrete adapters with `load()` and `embed()` | Dataset I/O, retrieval logic | `src/benchmark/models/base.py` |
| `datasets/` | JSON split parsing, `Sample` representation, dataset validation | Embedding generation | `src/benchmark/datasets/loader.py` |
| `embeddings/` | Batch inference loop, npz cache, durable storage to outputs/ | Retrieval, metrics, reporting | `src/benchmark/embeddings/generator.py` |
| `evaluation/` | Orchestration (`BenchmarkRunner`), retrieval+metrics pipeline (`Evaluator`) | Model loading (uses registry), report formatting (delegates) | `src/benchmark/evaluation/benchmark.py` |
| `metrics/` | Pure functions for P@K, R@K, mAP, nDCG, latency, throughput | I/O, model/dataset dependencies | `src/benchmark/metrics/__init__.py` |
| `retrieval/` | Cosine similarity, top-K selection, FAISS/PGVector alternatives | Metric computation | `src/benchmark/retrieval/cosine.py` |
| `reporting/` | JSON, CSV, Markdown, Typst, matplotlib chart generation | Metric computation, model inference | `src/benchmark/reporting/__init__.py` |
| `research/` | Feature extraction, PGVector benchmarks, split-aware evaluation | Core benchmark pipeline (independent) | `src/benchmark/research/feature_extraction.py` |
| `utils/` | Logging (rich handler), torch device resolution, timing context manager, random seed | Any domain logic | `src/benchmark/utils/` |

### 4) Reused Patterns

| Pattern | Where found | Why it exists |
|---------|-------------|---------------|
| Strategy / Adapter | `src/benchmark/models/base.py` — `EmbeddingModel` ABC with `embed()` / `embed_batch()` | Allows adding new models without changing pipeline code; each model wraps its own library (transformers, open-clip-torch, torchvision) |
| Lazy initialization / Registry | `src/benchmark/models/__init__.py` — `_LazyRegistry` dict that builds on first access | Prevents importing torch/transformers at module load time; allows reporting module to work in torch-free environments |
| Dataclass DTOs | `ModelMetrics`, `EmbeddingResult`, `Sample` | Immutable, typed data carriers between pipeline stages |
| Context Manager for timing | `src/benchmark/utils/timing.py` — `timed()` | Standardized latency measurement across the pipeline |
| Cache-aside | `src/benchmark/embeddings/cache.py` — `exists()` → if miss, generate → `save()` | Avoids recomputing embeddings across runs |
| Facade (barrel exports) | Each package's `__init__.py` re-exports its public API | Clean public interface; callers import from `benchmark.metrics` not `benchmark.metrics.map` |

### 5) Known Architectural Risks

- **Research module duplication**: `src/benchmark/research/` contains `ResearchDataset` and `Evaluator` that partially duplicate `datasets/loader.py` and `evaluation/evaluator.py`. If core APIs change, research code may silently diverge. Impact: maintenance burden.
- **Lazy registry creates new instances per BenchmarkRunner**: `get_registry(device)` is called in `BenchmarkRunner.__init__`, returning fresh instances. If called multiple times in same process, models are re-created. Impact: wasted memory if run() is called repeatedly without proper cleanup.
- **No model unloading**: Models load into GPU memory but are never explicitly unloaded. If running many models sequentially, GPU OOM is possible. Impact: potential GPU memory exhaustion on smaller cards.
- **Single-threaded per model**: `BenchmarkRunner.run()` evaluates models sequentially. FAISS/PGVector retrieval modules exist but are only used in the research subcommand. Impact: no multi-model parallelism.

### 6) Evidence

- `src/benchmark/cli/benchmark.py:L115-125` — BenchmarkRunner instantiation and pipeline
- `src/benchmark/evaluation/benchmark.py:L39-109` — BenchmarkRunner and _run_single flow
- `src/benchmark/evaluation/evaluator.py:L81-151` — Evaluator flow
- `src/benchmark/embeddings/generator.py:L53-122` — Embedding generation with cache
- `src/benchmark/models/base.py` — Adapter interface
- `src/benchmark/models/__init__.py:L68-103` — Lazy registry
