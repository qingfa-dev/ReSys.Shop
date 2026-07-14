# Codebase Structure

## Core Sections (Required)

### 1) Top-Level Map

| Path | Purpose | Evidence |
|------|---------|----------|
| `src/benchmark/` | Main benchmark package (source code) | `pyproject.toml:L48` |
| `src/benchmark/cli/` | Typer CLI commands (benchmark, report, cache, research) | `src/benchmark/cli/main.py` |
| `src/benchmark/models/` | Model adapters (FashionCLIP, CLIP, SigLIP, EVA-CLIP, etc.) | `src/benchmark/models/__init__.py` |
| `src/benchmark/datasets/` | Dataset loading, validation | `src/benchmark/datasets/loader.py` |
| `src/benchmark/embeddings/` | Embedding generation, cache, durable storage | `src/benchmark/embeddings/generator.py` |
| `src/benchmark/evaluation/` | Benchmark runner, evaluator, metric computation | `src/benchmark/evaluation/benchmark.py` |
| `src/benchmark/metrics/` | Metric implementations (P@K, R@K, mAP, nDCG, latency, throughput) | `src/benchmark/metrics/__init__.py` |
| `src/benchmark/retrieval/` | Cosine similarity retrieval, FAISS, PGVector | `src/benchmark/retrieval/cosine.py` |
| `src/benchmark/reporting/` | Report generators (JSON, CSV, Markdown, Typst, charts) | `src/benchmark/reporting/__init__.py` |
| `src/benchmark/research/` | Research extension (feature extraction, PGVector benchmarks, evaluation) | `src/benchmark/research/` |
| `src/benchmark/utils/` | Shared utilities (logging, device resolution, timing, seed) | `src/benchmark/utils/logging.py` |
| `src/tests/` | Test suite | `pyproject.toml:L51` |
| `scripts/` | Standalone scripts (dataset preparation) | `scripts/prepare_fashion_product.py` |
| `configs/` | YAML configuration files | `configs/benchmark.yaml` |
| `data/` | Datasets, cache, splits (gitignored except .gitkeep) | `data/cache/.gitkeep` |
| `outputs/` | Runtime outputs (metrics, reports, tables, figures, embeddings, logs) | `Makefile:L15` |
| `ReSys.Research/` | Nested research project (thesis experiments, PGVector, Docker) | `ReSys.Research/` |
| `benchmarks.v001/` | Alternative version / workspace with experiments and docker infra | `benchmarks.v001/` |
| `docs/` | Project documentation | `docs/datasets.md` |

### 2) Entry Points

- Main runtime entry: `src/benchmark/cli/main.py` — Typer app registered as `benchmark` CLI via `pyproject.toml:L41`
- Secondary entry points: `scripts/prepare_fashion_product.py` (dataset preparation), `ReSys.Research/scripts/*.py` (research experiments)
- How entry is selected: `pyproject.toml` `[project.scripts]` maps `benchmark = "benchmark.cli.main:app"`; invoked via `uv run benchmark` or `./.venv/bin/benchmark`

### 3) Module Boundaries

| Boundary | What belongs here | What must not be here |
|----------|-------------------|------------------------|
| `cli/` | CLI argument definition, pipeline orchestration, user I/O via rich | Model inference, metric computation, embedding generation |
| `models/` | Model adapters implementing `EmbeddingModel` interface | Dataset loading, retrieval, metrics, reporting |
| `datasets/` | Dataset loading from JSON splits, sample representation, validation | Embedding generation, model inference |
| `embeddings/` | Batch embedding generation, npz cache read/write, durable storage | Metric computation, retrieval, reporting |
| `evaluation/` | BenchmarkRunner orchestration, Evaluator (retrieval + metrics) | Model loading (delegates to models), report formatting (delegates to reporting) |
| `metrics/` | Pure metric functions (P@K, R@K, mAP, nDCG, latency, throughput) | I/O, model inference, dataset loading |
| `retrieval/` | Nearest-neighbour search (cosine, FAISS, PGVector) | Metric computation, dataset loading |
| `reporting/` | Output formatting (JSON, CSV, Markdown, Typst, matplotlib charts) | Metric computation, model inference |
| `research/` | Research-specific workflows (feature extraction, PGVector benchmarks, split-aware evaluation) | Core benchmark pipeline (should not depend on it) |
| `utils/` | Logging, device resolution, timing, random seed | Domain logic, model inference |

### 4) Naming and Organization Rules

- File naming pattern: `snake_case` for modules (e.g., `fashion_clip.py`, `feature_extraction.py`)
- Directory organization pattern: layer-based (cli, models, datasets, embeddings, evaluation, metrics, retrieval, reporting, research, utils)
- Import aliasing or path conventions: No path aliases. All imports use `benchmark.` prefix (package name). The package is installed editable via `pip install -e .` so `from benchmark.models import ...` works.
- Model keys are canonical dash-separated lowercase strings: `fashion-clip`, `clip-b32`, `clip-l14`, `clip-vit-b16`, `siglip`, `eva-clip`, `efficientnet-b0`, `dinov2-vits14`

### 5) Evidence

- `src/benchmark/` — package tree
- `pyproject.toml:L41` — CLI entry point
- `pyproject.toml:L48` — wheel package source
- `src/benchmark/cli/main.py` — Typer app structure
- `AGENTS.md:L22-29` — important code locations
- `Makefile:L57-67` — benchmark invocation
