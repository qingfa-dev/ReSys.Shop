# Coding Conventions

## Core Sections (Required)

### 1) Naming Rules

| Item | Rule | Example | Evidence |
|------|------|---------|----------|
| Files | `snake_case.py` | `fashion_clip.py`, `recall_comparison.py`, `test_map.py`, `test_pgvector_extended.py` | `src/benchmark/models/`, `src/benchmark/metrics/`, `src/tests/` |
| Functions/methods | `snake_case` | `embed_batch()`, `approximate_recall_at_k()`, `upsert_batch()`, `build_index()` | `src/benchmark/models/base.py`, `src/benchmark/retrieval/pgvector.py` |
| Types/interfaces | PascalCase | `EmbeddingModel`, `FashionDataset`, `ModelMetrics`, `EmbeddingResult`, `PipelineRunner`, `ThesisRunner` | `src/benchmark/models/base.py`, `src/benchmark/evaluation/pipeline.py` |
| Constants/env vars | UPPER_SNAKE_CASE | `CACHE_DIR`, `THESIS_MODEL_KEYS`, `_AUTO_GEN_COMMENT`, `_EXPECTED_KEYS` | `src/benchmark/embeddings/cache.py`, `src/benchmark/evaluation/pipeline.py` |
| Private methods | Single underscore prefix `_` | `_evaluate_model()`, `_evaluate_fold()`, `_run_pgvector_pipeline()`, `_measure_peak_ram()` | `src/benchmark/evaluation/pipeline.py`, `src/benchmark/evaluation/thesis.py` |
| Module-level internals | Single underscore prefix | `_LazyRegistry`, `_register()`, `_npz_path()`, `_FACTORIES` | `src/benchmark/models/__init__.py`, `src/benchmark/embeddings/cache.py` |
| Test files | `test_*.py` with `test_*` functions | `test_upsert_batch()`, `test_approximate_recall_at_k()` | `src/tests/retrieval/test_pgvector_extended.py` |

### 2) Formatting and Linting

- Formatter: Ruff (implicit via lint rules)
- Linter: Ruff configured in `pyproject.toml` `[tool.ruff]` and `[tool.ruff.lint]`
- Enforced rules: `E` (pycodestyle), `F` (pyflakes), `I` (isort), `UP` (pyupgrade), `B` (bugbear), `SIM` (simplify)
- Line length: 100 (E501 ignored)
- Run: `uv run ruff check src/`

### 3) Import and Module Conventions

- Import order: `__future__` → standard library → third-party → `benchmark.*`. Enforced by Ruff `I` rule.
- Import style: Always use absolute imports with `benchmark.` prefix (e.g., `from benchmark.models.base import EmbeddingModel`). No relative imports.
- Public exports: Each package's `__init__.py` re-exports public API via `__all__`. Callers import from the package, not submodules.
- Optional dependency imports: Database/ML imports deferred inside methods (e.g., `import psycopg` inside `connect()`, `import torch` inside `load()`).

### 4) Error and Logging Conventions

- Error strategy by layer:
  - **Datasets/Embeddings**: Log warnings for missing/corrupt images, skip gracefully.
  - **Utils**: `resolve_device()` raises `RuntimeError` if requested device unavailable.
  - **CLI**: Raises `typer.Exit(code=1)` for user-facing errors.
  - **PgvectorRetriever**: Methods raise `RuntimeError` if `connect()` not called first. `build_index()` raises `ValueError` for invalid parameters.
  - **PipelineRunner**: `_run_pgvector_pipeline()` catches all pgvector exceptions, returns zero-filled metrics, logs warning. Exact cosine metrics still valid.
- Logging style:
  - Uses Python `logging` with Rich handler (console) and file handler.
  - Format: `%(asctime)s %(levelname)-8s %(name)s — %(message)s` for file logs.
  - All loggers are children of `benchmark.` namespace via `get_logger()`.
  - Common patterns: `logger.info("Evaluating %s …", model.name)`, `logger.warning("PGVector not available: %s", exc)`

### 5) Testing Conventions

- Test file location: `src/tests/` mirrors `src/benchmark/` structure (e.g., `tests/retrieval/` for `benchmark/retrieval/`).
- Mocking: Tests that need DB mocking use `unittest.mock.MagicMock` with `patch` (e.g., `test_pgvector_extended.py`, `test_pipeline.py`). Metric tests use synthetic data. Model tests use real adapters if ML deps are available.
- Test naming: `test_<function_or_class>_<scenario>` (e.g., `test_upsert_batch`, `test_clear_table`, `test_build_index_invalid_lists`)
- Fixtures: Defined inline in test files. No shared `conftest.py`.
- TDD expected: New features follow TDD (write test → run RED → implement → run GREEN → commit).

### 6) Evidence

- `pyproject.toml:L56-63` — ruff configuration
- `src/benchmark/models/base.py` — interface contract, naming
- `src/benchmark/evaluation/pipeline.py` — error handling, graceful degradation
- `src/benchmark/retrieval/pgvector.py` — guard patterns, deferred imports
- `src/benchmark/utils/logging.py` — logging setup
- `src/tests/retrieval/test_pgvector_extended.py` — mock-based tests
- `src/tests/metrics/test_recall_comparison.py` — pure function tests
