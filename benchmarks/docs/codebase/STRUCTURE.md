# Codebase Structure

## Core Sections (Required)

### 1) Top-Level Map

| Path | Purpose | Evidence |
|------|---------|----------|
| `src/benchmark/` | Main benchmark package (source code) | `pyproject.toml:L50` |
| `src/benchmark/cli/` | Typer CLI commands (`run`, `thesis`, `pipeline`, `report`, `cache`) | `src/benchmark/cli/benchmark.py` |
| `src/benchmark/models/` | Model adapters (11 models: FashionCLIP, CLIP variants, SigLIP, EVA-CLIP, ResNet-50, EfficientNet-B0, CLIP-generic, ConvNeXt, DINOv2) | `src/benchmark/models/__init__.py` |
| `src/benchmark/datasets/` | Dataset loading (`FashionDataset`), ground-truth builder (`GroundTruth`), image transforms, validation | `src/benchmark/datasets/loader.py` |
| `src/benchmark/embeddings/` | Embedding generation (`EmbeddingGenerator`), `.npz` cache, durable storage | `src/benchmark/embeddings/generator.py` |
| `src/benchmark/evaluation/` | `BenchmarkRunner` (one-shot), `ThesisRunner` (3-fold CV), `PipelineRunner` (pgvector), `Evaluator`, stats (`aggregate_mean_std`, `cohens_d`, `bootstrap_ci`) | `src/benchmark/evaluation/benchmark.py` |
| `src/benchmark/metrics/` | Pure metric functions: P@K, R@K, mAP, nDCG, latency, throughput, recall_comparison | `src/benchmark/metrics/__init__.py` |
| `src/benchmark/retrieval/` | Cosine similarity, FAISS index, PGVector retriever (batch ingestion, index build, query) | `src/benchmark/retrieval/cosine.py` |
| `src/benchmark/reporting/` | Report generators: JSON, CSV, Markdown, Typst, pipeline Typst, matplotlib charts | `src/benchmark/reporting/__init__.py` |
| `src/benchmark/utils/` | Shared utilities: logging, device resolution, timing, random seed | `src/benchmark/utils/logging.py` |
| `src/tests/` | Test suite (datasets, evaluation, metrics, models, reporting, retrieval, cli, integration, utils) | `pyproject.toml:L52` |
| `scripts/` | Standalone scripts: dataset download, preprocessing, benchmark, report, clean | `scripts/download_dataset.py` |
| `configs/` | YAML configuration files (benchmark, datasets, hardware, metrics, per-model) | `configs/benchmark.yaml` |
| `data/` | Datasets, embedding cache, splits (gitignored except `.gitkeep`) | `data/cache/.gitkeep` |
| `infra/` | PostgreSQL Docker/Podman configs and init scripts | `infra/postgres/init.sql` |
| `docs/` | Project documentation (9 numbered guides + codebase docs) | `docs/README.md` |
| `outputs/` | Runtime outputs: metrics, reports, tables, figures, embeddings, logs (gitignored) | Created at runtime |
| `experiments/` | Per-model ad-hoc experiment notebooks | `experiments/fashion_clip/` |
| `old/` | Legacy code: previous benchmark versions, old thesis code, old research module | `old/` |

### 2) Entry Points

- Main runtime entry: `src/benchmark/cli/benchmark.py` — Typer app registered as `benchmark` CLI via `pyproject.toml:L42` (`benchmark = "benchmark.cli.benchmark:app"`)
- CLI commands: `run` (one-shot), `thesis` (3-fold CV), `pipeline` (pgvector), `report` (regenerate), `cache` (manage)
- How entry is selected: `pyproject.toml` `[project.scripts]` maps `benchmark` to `benchmark.cli.benchmark:app`. Invoked via `uv run benchmark <command>`

### 3) Module Boundaries

| Boundary | What belongs here | What must not be here |
|----------|-------------------|------------------------|
| `cli/` | CLI argument definition, pipeline orchestration, user I/O via rich | Model inference, metric computation, embedding generation |
| `models/` | Model adapters implementing `EmbeddingModel` interface (11 models) | Dataset loading, retrieval, metrics, reporting |
| `datasets/` | Dataset loading from JSON splits and CSV, ground-truth building, stratified folds, validation | Embedding generation, model inference |
| `embeddings/` | Batch embedding generation, npz cache read/write, durable storage | Metric computation, retrieval, reporting |
| `evaluation/` | `BenchmarkRunner` (one-shot), `ThesisRunner` (CV), `PipelineRunner` (pgvector), `Evaluator`, stats | Model loading (uses registry), report formatting (uses reporting) |
| `metrics/` | Pure metric functions (P@K, R@K, mAP, nDCG, latency, throughput, recall_comparison) | I/O, model inference, dataset loading |
| `retrieval/` | Nearest-neighbour search (cosine, FAISS), pgvector client (batch ingestion, index, query) | Metric computation, dataset loading |
| `reporting/` | Output formatting (JSON, CSV, Markdown, Typst, pipeline Typst, charts) | Metric computation, model inference |
| `utils/` | Logging, device resolution, timing, random seed | Domain logic, model inference |

### 4) Naming and Organization Rules

- File naming pattern: `snake_case` for modules (e.g., `fashion_clip.py`, `recall_comparison.py`, `test_pipeline.py`)
- Directory organization pattern: layer-based (cli, models, datasets, embeddings, evaluation, metrics, retrieval, reporting, utils)
- Import convention: All imports use `benchmark.` prefix (absolute imports). Package is installed editable via `uv sync`.
- Model keys: canonical dash-separated lowercase strings — `fashion-clip`, `clip-b32`, `clip-l14`, `clip-vit-b16`, `clip-generic`, `siglip`, `eva-clip`, `efficientnet-b0`, `resnet-50`, `convnext-tiny`, `dinov2-vits14`
- Test files: `src/tests/` mirrors `src/benchmark/` structure. Named `test_*.py`.

### 5) Evidence

- `src/benchmark/` — package tree
- `pyproject.toml:L42` — CLI entry point
- `pyproject.toml:L50` — wheel package source
- `src/benchmark/cli/benchmark.py` — Typer app with 5 commands
- `src/benchmark/models/__init__.py` — 11-model registry
- `src/tests/` — mirrored test structure
