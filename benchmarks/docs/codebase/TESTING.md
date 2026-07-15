# Testing Patterns

## Core Sections (Required)

### 1) Test Stack and Commands

- Primary test framework: pytest >=8.2
- Assertion/mocking tools: pytest assert (built-in), `unittest.mock.MagicMock` and `unittest.mock.patch` for pgvector/DB tests
- Coverage tool: pytest-cov (configured, no threshold)
- Commands:

```bash
# Run all tests (skip integration tests needing pgvector)
uv run pytest --ignore=src/tests/integration/test_pgvector.py

# Run all tests including integration (requires Podman/pgvector)
uv run pytest

# Run specific module
uv run pytest src/tests/evaluation/test_pipeline.py -v

# Run coverage
uv run pytest --cov=benchmark --cov-report=term
```

### 2) Test Layout

- Pattern: `src/tests/` directory mirrors `src/benchmark/` structure
- Naming: `test_*.py` files with `test_*` function names
- Setup: `src/tests/__init__.py` package marker. No shared `conftest.py`. Fixtures defined inline.
- Test directories:

| Directory | What it tests | Example files |
|---|---|---|
| `datasets/` | Dataset loader, ground-truth builder | `test_loader.py`, `test_ground_truth.py` |
| `evaluation/` | ThesisRunner, PipelineRunner, stats | `test_thesis.py`, `test_pipeline.py`, `test_stats.py` |
| `integration/` | PGVector end-to-end (requires Podman) | `test_pgvector.py` |
| `metrics/` | P@K, R@K, mAP, nDCG, recall comparison | `test_map.py`, `test_recall_comparison.py` |
| `models/` | Registry, base contract, per-model adapters | `test_registry.py`, `test_resnet50.py`, `test_clip_generic.py` |
| `reporting/` | JSON, CSV, Markdown, Typst, pipeline Typst | `test_reporting.py`, `test_typst.py`, `test_pipeline_reporting.py` |
| `retrieval/` | PGVector batch operations (mocked) | `test_pgvector_extended.py` |
| `cli/` | CLI command registration | `test_pipeline_command.py` |
| `utils/` | Timing, LatencyStats | `test_timing.py` |

### 3) Test Scope Matrix

| Scope | Covered? | Typical target | Notes |
|---|---|---|---|
| Unit | Yes — 15 files | `metrics/`, `models/` (base contract), `recall_comparison` | Pure functions, dummy models, synthetic data |
| Integration | Partial — 1 file | `integration/test_pgvector.py` (requires Podman/pgvector) | Mostly skipped; `ping()` attribute check on PgvectorRetriever |
| Pipeline | Yes — 2 files | `evaluation/test_thesis.py`, `evaluation/test_pipeline.py` | Mocked model + mocked pgvector; tests 3-fold flow end-to-end |
| CLI | Yes — 1 file | `cli/test_pipeline_command.py` | Typer CliRunner with `--help` assertions |
| E2E | No | N/A | No end-to-end tests across all 4 models on real hardware |

### 4) Mocking and Isolation Strategy

- **Pure function tests** (`metrics/`): Use synthetic data (lists, numpy arrays). No mocks needed. Assert on exact values.
- **Model adapter tests** (`models/`): Use real model loading when ML deps available (test `embed()` output shape and dtype). Dummy `EmbeddingModel` subclass for interface contract.
- **Pipeline integration tests** (`evaluation/`): Mock model (`MagicMock` with `embed_batch`, `embed` returning random arrays), mock pgvector (`MagicMock` with `__enter__/__exit__` for context manager, `query()` returning synthetic results). Test end-to-end flow produces correct output shape.
- **CLI tests** (`cli/`): `typer.testing.CliRunner` with `--help` assertions.
- **PGVector unit tests** (`retrieval/`): Mock `MagicMock` database connection. Assert SQL strings contain expected patterns (INSERT, DELETE, etc.). Test parameter validation.

### 5) Coverage and Quality Signals

- Coverage tool: `pytest-cov` configured but no threshold enforced.
- Current test count: 125+ tests (excluding skipped integration tests).
- Known gaps:
  - `embeddings/` (generator, cache) — no unit tests (relies on integration tests via pipeline)
  - `retrieval/cosine.py` — no tests for `retrieve_batch` or `top_k_indices`
  - `retrieval/faiss.py` — no tests (requires FAISS)
  - Real model adapters — tests exist for `resnet50`, `clip_generic`, and `fashion_clip` (via base test) but depend on network for weight download on first run
  - `reporting/` CSV, Markdown, JSON generators — tested only through `test_reporting.py` fixture
  - No coverage enforcement in CI

### 6) Evidence

- `pyproject.toml:L52-54` — pytest configuration
- `src/tests/evaluation/test_pipeline.py` — mocked pipeline integration test
- `src/tests/retrieval/test_pgvector_extended.py` — mocked pgvector unit tests
- `src/tests/metrics/test_recall_comparison.py` — pure function recall tests
- `src/tests/models/test_resnet50.py` — real model adapter test
- `src/tests/cli/test_pipeline_command.py` — CLI test
