# Architecture

## Core Sections (Required)

### 1) Architectural Style

- Primary style: Layered / pipeline architecture
- Why this classification: The system is organized as a sequential pipeline (dataset → model → embedding → retrieval → metrics → report) with distinct layers, each in a separate package. Three distinct benchmark modes share the same core layers but orchestrate differently.
- Primary constraints: (1) Must support adding new models with a single new file. (2) Embedding cache must be usable offline. (3) All embeddings must be L2-normalized float32.

### 2) System Flow

**Enrich mode** (`benchmark enrich` — JSON metadata → enriched CSV):
```text
[CLI: benchmark.py enrich cmd] → [enrich_dataset.py: parse 44K JSON files]
  → [extract articleAttributes.Pattern] → [build enriched styles.csv]
  → [GroundTruth: generate_splits with dual-label (label + label_pattern)]
```

**Thesis mode** (`benchmark thesis` — 3-fold CV, in-memory exact search):
```text
[CLI: benchmark.py thesis cmd] → [GroundTruth: ground_truth.py → stratified splits]
  → [Model: load()] → [EmbeddingGenerator: fold_N_test/train] → [Evaluator: evaluate_split()]
  → [Metrics: P@K, R@K, mAP] → [Efficiency: latency, throughput, RAM, storage]
  → [Stats: aggregate_mean_std, bootstrap_ci, cohens_d] → [Thesis typst tables + JSON]
```

**Pipeline mode** (`benchmark pipeline` — 3-fold CV + pgvector production):
```text
[CLI: benchmark.py pipeline cmd] → [same thesis splits + embeddings]
  → [Exact cosine: retrieve_batch() → Evaluator → metrics]   ← baseline
  → [PGVector: upsert_batch → build_index → query → recall_comparison]  ← production
  → [Stats: aggregate both] → [Pipeline typst + JSON]
```

**One-shot mode** (`benchmark run` — single split, in-memory):
```text
[CLI: benchmark.py run cmd] → [FashionDataset: test split] → [Model: load()]
  → [EmbeddingGenerator] → [Evaluator: evaluate_split()] → [Reporting: all formats]
```

### 3) Layer/Module Responsibilities

| Layer or module | Owns | Must not own | Evidence |
|-----------------|------|--------------|----------|
| `cli/` | CLI argument parsing, wiring pipeline components, rich output | Model inference, metric math | `src/benchmark/cli/benchmark.py` |
| `models/` | Abstract `EmbeddingModel` interface, concrete adapters (11 models) with `load()`, `embed()`, `embed_batch()` | Dataset I/O, retrieval logic | `src/benchmark/models/base.py` |
| `datasets/` | JSON split parsing, CSV styles parsing, ground-truth building (`GroundTruth`), stratified fold generation, validation | Embedding generation | `src/benchmark/datasets/loader.py`, `src/benchmark/datasets/ground_truth.py` |
| `embeddings/` | Batch inference loop, npz cache (per model × fold × split), durable storage | Retrieval, metrics, reporting | `src/benchmark/embeddings/generator.py` |
| `evaluation/` | `BenchmarkRunner` (one-shot), `ThesisRunner` (3-fold CV), `PipelineRunner` (pgvector), `Evaluator`, stats | Model loading (uses registry), report formatting (delegates) | `src/benchmark/evaluation/benchmark.py`, `src/benchmark/evaluation/thesis.py`, `src/benchmark/evaluation/pipeline.py` |
| `metrics/` | Pure functions: P@K, R@K, mAP, nDCG, latency, throughput, `approximate_recall_at_k()` | I/O, model/dataset dependencies | `src/benchmark/metrics/__init__.py` |
| `retrieval/` | Cosine similarity, FAISS, PGVector client (`upsert_batch`, `build_index`, `query`) | Metric computation | `src/benchmark/retrieval/cosine.py`, `src/benchmark/retrieval/pgvector.py` |
| `reporting/` | JSON, CSV, Markdown, Typst (thesis + pipeline), matplotlib charts | Metric computation, model inference | `src/benchmark/reporting/__init__.py` |
| `utils/` | Logging (rich handler), torch device resolution, timing context manager, random seed | Domain logic | `src/benchmark/utils/` |

### 4) Reused Patterns

| Pattern | Where found | Why it exists |
|---------|-------------|---------------|
| Strategy / Adapter | `src/benchmark/models/base.py` — `EmbeddingModel` ABC | Add new models without changing pipeline code |
| Lazy Registry | `src/benchmark/models/__init__.py` — `get_registry(device)` builds on first access | Prevents importing torch at module load |
| Cache-aside | `src/benchmark/embeddings/generator.py` — check `.npz` → miss → generate → save | Avoids recomputing embeddings across runs and folds |
| Template Method | `ThesisRunner` and `PipelineRunner` share fold generation and evaluation pattern; `PipelineRunner` extends with pgvector phase | Code reuse across benchmark modes |
| Context Manager | `PgvectorRetriever.__enter__/__exit__` | Safe connection lifecycle in pgvector pipeline |
| Facade (barrel exports) | Each package's `__init__.py` re-exports public API | Clean public interface |
| Graceful Degradation | `PipelineRunner._run_pgvector_pipeline()` catches exceptions, returns zeros, logs warning | Pipeline works without PostgreSQL available |

### 5) Known Architectural Risks

- **Model weight downloads on first run**: Each model downloads ~1 GB from HuggingFace on first use. No progress tracking or retry logic. Impact: slow first run, network-dependent.
- **No model unloading**: Models load into memory and are never explicitly unloaded. `PipelineRunner` and `ThesisRunner` process models sequentially. Impact: memory pressure on large runs.
- **pgvector dimension limit**: IVFFlat index capped at 2000 dimensions. ResNet-50 (2048-d) cannot use IVFFlat; pipeline gracefully degrades. Impact: production metrics unavailable for 2048-d models.
- **Single-threaded per model**: All runners evaluate models sequentially. Impact: no multi-model parallelism; GPU idle when models are CPU-bound.
- **Cache key fragility**: Cache key is `model_slug + dataset_name`. If dataset content changes but name stays the same, stale cache used. Impact: silent incorrect results. Mitigation: `--no-cache` flag.

### 6) Evidence

- `src/benchmark/evaluation/thesis.py:L42-245` — ThesisRunner (3-fold CV)
- `src/benchmark/evaluation/pipeline.py:L42-333` — PipelineRunner (pgvector)
- `src/benchmark/evaluation/benchmark.py:L39-109` — BenchmarkRunner (one-shot)
- `src/benchmark/datasets/ground_truth.py` — Ground truth builder with stratified splits
- `src/benchmark/retrieval/pgvector.py` — PgvectorRetriever with batch ingestion and indexing
- `src/benchmark/cli/benchmark.py` — Typer app with 6 commands
- `src/benchmark/cli/benchmark.py` — CLI entry point
- `src/benchmark/retrieval/cosine.py` — In-memory cosine retrieval
- `src/benchmark/models/__init__.py:L68-103` — Lazy registry
- `scripts/05_enrich_dataset.py` — Enriched dataset builder (JSON → CSV)
