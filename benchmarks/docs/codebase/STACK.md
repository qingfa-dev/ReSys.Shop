# Technology Stack

## Core Sections (Required)

### 1) Runtime Summary

| Area | Value | Evidence |
|------|-------|----------|
| Primary language | Python | `pyproject.toml` |
| Runtime + version | Python >=3.12 (`.python-version`: 3.12) | `.python-version`, `pyproject.toml:L5` |
| Package manager | `uv` (lockfile: `uv.lock`); `pip` via `Makefile` `.venv` | `uv.lock`, `Makefile:L41-44` |
| Module/build system | Hatchling (`hatchling.build`) | `pyproject.toml:L43-44` |

### 2) Production Frameworks and Dependencies

| Dependency | Version | Role in system | Evidence |
|------------|---------|----------------|----------|
| torch | >=2.3 | Deep learning runtime for vision models | `pyproject.toml:L8` |
| torchvision | >=0.18 | Image transforms / preprocessing | `pyproject.toml:L9` |
| transformers | >=4.41 | HuggingFace model loading (CLIP, SigLIP) | `pyproject.toml:L10` |
| open-clip-torch | >=2.24 | OpenCLIP model loading (EVA-CLIP) | `pyproject.toml:L11` |
| fashion-clip | >=0.2 | FashionCLIP model (Coveo fine-tuned) | `pyproject.toml:L12` |
| numpy | >=1.26 | Numerical arrays for embeddings | `pyproject.toml:L14` |
| Pillow | >=10.3 | Image loading / manipulation | `pyproject.toml:L15` |
| faiss-cpu | >=1.8 | Approximate nearest-neighbour search (optional) | `pyproject.toml:L17` |
| psycopg[binary] | >=3.1 | PostgreSQL driver (research PGVector path) | `pyproject.toml:L18` |
| pgvector | >=0.2 | PGVector extension client (research path) | `pyproject.toml:L19` |
| pydantic | >=2.7 | Data validation / settings | `pyproject.toml:L21` |
| pydantic-settings | >=2.3 | Environment-based settings | `pyproject.toml:L22` |
| PyYAML | >=6.0 | YAML config file parsing | `pyproject.toml:L23` |
| rich | >=13.7 | Rich CLI output (tables, progress) | `pyproject.toml:L25` |
| typer | >=0.12 | CLI framework | `pyproject.toml:L26` |
| tqdm | >=4.66 | Progress bars | `pyproject.toml:L27` |
| matplotlib | >=3.9 | Chart generation (reports) | `pyproject.toml:L29` |
| seaborn | >=0.13 | Statistical chart styling | `pyproject.toml:L30` |
| pandas | >=2.2 | Data manipulation for reporting | `pyproject.toml:L31` |

### 3) Development Toolchain

| Tool | Purpose | Evidence |
|------|---------|----------|
| pytest | Test runner | `pyproject.toml:L35`, `[tool.pytest.ini_options]` |
| pytest-cov | Code coverage | `pyproject.toml:L36` |
| ruff | Linter (E, F, I, UP, B, SIM rules) | `pyproject.toml:L55-62` |
| hatchling | Build system / wheel builder | `pyproject.toml:L43-44` |
| uv | Package manager / lockfile | `uv.lock` |

### 4) Key Commands

```bash
# Install dependencies
uv sync

# Run all models via Makefile
make benchmark

# Run benchmark CLI directly
uv run benchmark benchmark --dataset-root data/raw/deepfashion --split-file data/splits/test.json --models all --device cpu

# Run tests
uv run pytest

# Run lint
uv run ruff check src/

# Prepare fashion product dataset
make prepare-fashion-product

# Clean virtualenv and outputs
make clean
```

### 5) Environment and Config

- Config sources: `pyproject.toml`, `Makefile` (defaults for `DEVICE`, `MODELS`, `BATCH_SIZE`, etc.), `configs/benchmark.yaml`, `configs/datasets.yaml`, `configs/hardware.yaml`, `configs/metrics.yaml`, `configs/models/*.yaml`
- Required env vars: None required (all configurable via CLI). Makefile exposes `DATA_ROOT`, `SPLIT_FILE`, `MODELS`, `DEVICE`, `BATCH_SIZE`, `OUTPUT`, `NO_CACHE`, `NO_LATENCY`
- Deployment/runtime constraints: Requires Python 3.12+. ML models require internet access on first run for weight download. GPU recommended for realistic latency benchmarks.

### 6) Evidence

- `pyproject.toml` — project metadata, dependencies, build system, CLI entry point, pytest config, ruff config
- `.python-version` — Python version pin (3.12)
- `uv.lock` — locked dependency versions
- `Makefile` — venv creation, dataset prep, benchmark execution
- `configs/benchmark.yaml` — experiment-level configuration
- `configs/datasets.yaml` — dataset configuration
