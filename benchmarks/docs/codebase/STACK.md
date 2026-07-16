# Technology Stack

## Core Sections (Required)

### 1) Runtime Summary

| Area | Value | Evidence |
|------|-------|----------|
| Primary language | Python | `pyproject.toml` |
| Runtime + version | Python >=3.12 (`.python-version`: 3.12) | `.python-version`, `pyproject.toml:L5` |
| Package manager | `uv` (lockfile: `uv.lock`) | `uv.lock` |
| Module/build system | Hatchling (`hatchling.build`) | `pyproject.toml:L45-47` |

### 2) Production Frameworks and Dependencies

| Dependency | Version | Role in system | Evidence |
|------------|---------|----------------|----------|
| torch | >=2.3 | Deep learning runtime for vision models | `pyproject.toml:L8` |
| torchvision | >=0.18 | Image transforms / ResNet-50 model | `pyproject.toml:L9` |
| transformers | >=4.41 | HuggingFace model loading (CLIP, SigLIP, FashionCLIP) | `pyproject.toml:L10` |
| open-clip-torch | >=2.24 | OpenCLIP model loading (EVA-CLIP) | `pyproject.toml:L11` |
| fashion-clip | >=0.2 | FashionCLIP model (Coveo fine-tuned) | `pyproject.toml:L12` |
| numpy | >=1.26 | Numerical arrays for embeddings | `pyproject.toml:L14` |
| Pillow | >=10.3 | Image loading / manipulation | `pyproject.toml:L15` |
| faiss-cpu | >=1.8 | Approximate nearest-neighbour search (optional) | `pyproject.toml:L17` |
| psycopg[binary] | >=3.1 | PostgreSQL driver for pgvector pipeline | `pyproject.toml:L18` |
| pgvector | >=0.2 | PGVector extension client | `pyproject.toml:L19` |
| httpx | >=0.24 | HTTP client (dependency of open-clip, not used directly) | `pyproject.toml:L24` |
| pydantic | >=2.7 | Data validation / settings | `pyproject.toml:L21` |
| pydantic-settings | >=2.3 | Environment-based settings | `pyproject.toml:L22` |
| PyYAML | >=6.0 | YAML config file parsing | `pyproject.toml:L23` |
| rich | >=13.7 | Rich CLI output (tables, progress) | `pyproject.toml:L25` |
| typer | >=0.12 | CLI framework | `pyproject.toml:L26` |
| tqdm | >=4.66 | Progress bars | `pyproject.toml:L27` |
| psutil | >=6.0 | RAM measurement during inference | `pyproject.toml:L28` |
| matplotlib | >=3.9 | Chart generation (reports) | `pyproject.toml:L30` |
| seaborn | >=0.13 | Statistical chart styling | `pyproject.toml:L31` |
| pandas | >=2.2 | Data manipulation for CSV styles, ground truth, reporting | `pyproject.toml:L32` |

### 3) Development Toolchain

| Tool | Purpose | Evidence |
|------|---------|----------|
| pytest | Test runner | `pyproject.toml:L36` |
| pytest-cov | Code coverage | `pyproject.toml:L37` |
| ruff | Linter (E, F, I, UP, B, SIM rules) | `pyproject.toml:L56-63` |
| hatchling | Build system / wheel builder | `pyproject.toml:L45-47` |
| uv | Package manager / lockfile | `uv.lock` |
| podman | Container runtime for pgvector PostgreSQL | `infra/postgres/init.sql` |

### 4) Key Commands

```bash
# Install dependencies
uv sync --extra dev

# Run enrich pipeline (JSON metadata → enriched CSV)
uv run benchmark enrich --dataset-root data/raw/fashion-product-images --output data/raw/fashion-enriched-5k --n-samples 5000

# Run thesis benchmark (in-memory, 4 models × 3-fold CV)
uv run benchmark thesis --dataset-root /tmp/thesis_5k --folds 3 --k 5,10,20

# Run production pipeline (with pgvector)
uv run benchmark pipeline --dataset-root /tmp/thesis_5k --folds 3 --k 5,10,20

# Run one-shot comparison (all models)
uv run benchmark run --dataset-root data/raw/fashion-product-images-small --models all

# Regenerate reports from stored results
uv run benchmark report --format typst

# Manage cache
uv run benchmark cache list

# Create enriched dataset
uv run benchmark enrich --dataset-root data/raw/fashion-product-images --n-samples 5000

# Run tests
uv run pytest

# Run lint
uv run ruff check src/
```

### 5) Environment and Config

- Config sources: `pyproject.toml`, `configs/benchmark.yaml`, `configs/datasets.yaml`, `configs/hardware.yaml`, `configs/metrics.yaml`, `configs/models/*.yaml`
- Required env vars: None required (all configurable via CLI). pgvector connection string has default `postgresql://benchmark:benchmark@localhost:5432/benchmark`
- Deployment/runtime constraints: Python 3.12+. ML models require internet on first run for weight download (~5 GB). GPU recommended for realistic latency benchmarks (NVIDIA sm_75+, 8 GB+ VRAM; Turing or newer). pgvector requires PostgreSQL 16 + pgvector extension via Podman (or Docker).

### 6) Evidence

- `pyproject.toml` — project metadata, dependencies, build system, CLI entry point, pytest config, ruff config
- `.python-version` — Python version pin (3.12)
- `uv.lock` — locked dependency versions
- `configs/benchmark.yaml` — experiment-level configuration
- `configs/datasets.yaml` — dataset configuration
- `infra/postgres/init.sql` — PostgreSQL + pgvector schema
- `infra/postgres/wait-for-pg.sh` — PostgreSQL readiness check
