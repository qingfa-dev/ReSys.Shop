# Coding Conventions

## Core Sections (Required)

### 1) Naming Rules

| Item | Rule | Example | Evidence |
|------|------|---------|----------|
| Files | `snake_case.py` | `fashion_clip.py`, `feature_extraction.py`, `test_map.py` | `src/benchmark/models/fashion_clip.py`, `src/benchmark/research/feature_extraction.py` |
| Functions/methods | `snake_case` | `embed_batch()`, `set_seed()`, `get_registry()`, `resolve_device()` | `src/benchmark/models/base.py:L64`, `src/benchmark/utils/random_seed.py`, `src/benchmark/models/__init__.py:L45` |
| Types/interfaces | PascalCase | `EmbeddingModel`, `FashionDataset`, `ModelMetrics`, `EmbeddingResult`, `Sample` | `src/benchmark/models/base.py:L20`, `src/benchmark/datasets/loader.py:L27`, `src/benchmark/evaluation/evaluator.py:L28` |
| Constants/env vars | UPPER_SNAKE_CASE for module-level constants; env vars in Makefile use UPPER_SNAKE_CASE | `CACHE_DIR`, `_HF_MODEL_ID`, `DEVICE`, `BATCH_SIZE` | `src/benchmark/embeddings/cache.py:L24`, `src/benchmark/models/fashion_clip.py:L26`, `Makefile:L12-14` |
| Private methods/attributes | Single underscore prefix `_` | `_loaded`, `_samples`, `_model`, `_run_single()`, `_build()` | `src/benchmark/models/base.py:L30`, `src/benchmark/datasets/loader.py:L46`, `src/benchmark/evaluation/benchmark.py:L92` |
| Module-level internals | Single underscore prefix | `_FACTORIES`, `_register()`, `_npz_path()`, `_LazyRegistry` | `src/benchmark/models/__init__.py:L19-22`, `src/benchmark/embeddings/cache.py:L27` |

### 2) Formatting and Linting

- Formatter: Ruff (implicit formatter via lint rules; no separate formatter config detected)
- Linter: Ruff configured in `pyproject.toml` `[tool.ruff]` and `[tool.ruff.lint]`
- Most relevant enforced rules:
  - `E` — pycodestyle errors
  - `F` — pyflakes (unused imports, undefined names)
  - `I` — isort (import ordering)
  - `UP` — pyupgrade (modern Python syntax)
  - `B` — flake8-bugbear (common bug patterns)
  - `SIM` — flake8-simplify (simplification suggestions)
  - Line length: 100 (`E501` ignored/suppressed)
- Run commands:
  ```bash
  uv run ruff check src/
  ```

### 3) Import and Module Conventions

- Import grouping/order: Ruff `isort` (I) rule enforces: `__future__` → standard library → third-party → local (`benchmark.`). Observed consistently across all modules.
- Alias vs relative import policy: Always use absolute imports with `benchmark.` prefix (e.g., `from benchmark.models.base import EmbeddingModel`). No relative imports (`from .base import ...`) observed in any file.
- Public exports/barrel policy: Each package has an `__init__.py` that re-exports its public API via `__all__`. Callers import from the package (e.g., `from benchmark.metrics import mean_average_precision`) not from submodules.

### 4) Error and Logging Conventions

- Error strategy by layer:
  - **Datasets/Embeddings**: Log warnings for missing/corrupt images, skip them gracefully (don't fail entire run). `OSError` caught in `iter_images()` and `generate()` loops.
  - **Utils**: `resolve_device()` raises `RuntimeError` if requested device is unavailable.
  - **CLI**: Raises `typer.Exit(code=1)` for user-facing errors (unknown model keys, missing data dirs).
  - **Evaluation**: No custom exception handling — lets exceptions propagate.
- Logging style and required context fields:
  - Uses Python stdlib `logging` with Rich handler for console output and optional file handler.
  - Format: `%(asctime)s %(levelname)-8s %(name)s — %(message)s` for file logs; Rich markup for console.
  - All loggers are children of `benchmark.` namespace via `get_logger()`.
  - Common patterns: `logger.info("Loading %s from %s …", ...)`, `logger.warning("Skipping %s: %s", ...)`
- Sensitive-data redaction rules: No sensitive data handling detected. Database connection strings appear in CLI defaults (research subcommands) but are not committed as secrets.

### 5) Testing Conventions

- Test file naming/location rule: Tests in `src/tests/`, mirrored structure (e.g., `src/tests/models/` for `src/benchmark/models/`). Files named `test_*.py`.
- Mocking strategy norm: No mocks observed; tests use in-memory dummy models (`DummyModel`) and synthetic PIL images. Metrics are pure functions tested with list/set inputs.
- Coverage expectation: `pytest-cov` configured but no coverage threshold enforced in config. Current coverage: not measured (no CI pipeline to enforce it).

### 6) Evidence

- `pyproject.toml:L55-62` — ruff configuration
- `src/benchmark/models/base.py` — interface contract, naming
- `src/benchmark/cli/benchmark.py` — import organization, CLI error handling
- `src/benchmark/utils/logging.py` — logging setup
- `src/benchmark/datasets/loader.py:L79-80` — error handling in iter_images
- `src/tests/models/test_base.py` — test patterns
- `src/tests/metrics/test_map.py` — metric test patterns
