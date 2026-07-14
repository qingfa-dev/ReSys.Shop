# Testing Patterns

## Core Sections (Required)

### 1) Test Stack and Commands

- Primary test framework: pytest >=8.2
- Assertion/mocking tools: pytest assert (built-in); no external mocking library detected (tests use dummy implementations)
- Commands:

```bash
# Run all tests
uv run pytest

# Run coverage
uv run pytest --cov=benchmark --cov-report=term
```

### 2) Test Layout

- Test file placement pattern: `src/tests/` directory, mirrored structure from `src/benchmark/`. Tests co-located by domain: `datasets/`, `metrics/`, `models/`, `reporting/`, `integration/`
- Naming convention: `test_*.py` files with `test_*` function names
- Setup files and where they run: `src/tests/__init__.py` exists as package marker. No `conftest.py` found; fixtures defined inline in test files

### 3) Test Scope Matrix

| Scope | Covered? | Typical target | Notes |
|-------|----------|----------------|-------|
| Unit | Yes | `metrics/` (P@K, R@K, mAP, nDCG), `models/` (base interface contract), `datasets/` (loader) | Pure functions and dummy models; no real model loading |
| Integration | No | `integration/` directory exists but is empty (no test files) | No integration tests implemented |
| E2E | No | N/A | No end-to-end tests detected |

### 4) Mocking and Isolation Strategy

- Main mocking approach: No mocking framework used. Tests create lightweight dummy implementations (e.g., `DummyModel(EmbeddingModel)` in `test_base.py`). Metric tests use synthetic label lists and relevance sets directly.
- Isolation guarantees: Each test function creates fresh inputs; no shared state between tests.
- Common failure mode in tests: Tests do not exercise GPU paths or real model weights — only interface contracts and pure math functions. Real model adapters are not tested.

### 5) Coverage and Quality Signals

- Coverage tool + threshold: `pytest-cov` configured in `pyproject.toml:L36`. No coverage threshold enforced.
- Current reported coverage: [TODO — run `uv run pytest --cov=benchmark --cov-report=term` to measure]
- Known gaps/flaky areas:
  - No tests for `embeddings/` (generator, cache) — core pipeline component
  - No tests for `retrieval/` (cosine, FAISS, PGVector) — core pipeline component
  - No tests for `evaluation/` (BenchmarkRunner, Evaluator) — orchestration logic
  - No tests for `reporting/` (JSON, CSV, Markdown, Typst, charts) — report generation
  - No tests for `research/` module — PGVector, feature extraction, split-aware evaluation
  - No tests for real model adapters (FashionCLIP, CLIP, SigLIP, EVA-CLIP) — would require network + GPU
  - No tests for `cli/` — CLI argument parsing and pipeline wiring
  - Model registry tests exist (`test_registry.py`) but only verify listing, not runtime behavior

### 6) Evidence

- `pyproject.toml:L50-53` — pytest configuration
- `pyproject.toml:L34-36` — pytest + pytest-cov dependency declaration
- `src/tests/models/test_base.py` — interface contract tests
- `src/tests/metrics/test_map.py` — metric unit tests
- `src/tests/datasets/test_loader.py` — dataset test file (exists)
- `src/tests/metrics/test_precision.py` — precision test file (exists)
- `src/tests/reporting/` — reporting test directory (exists)
- `src/tests/integration/` — integration test directory (exists)
