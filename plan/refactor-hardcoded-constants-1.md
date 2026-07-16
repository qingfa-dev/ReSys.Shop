---
goal: Extract all hardcoded values in Python codebases (benchmarks + Embedding service) into named constants defined before use
version: 1.0
date_created: 2026-07-15
owner: Engineering
status: Planned
tags: refactor, constants, python, benchmarks, embedding
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Extract every hardcoded magic number, string literal, configuration default, pattern template, constraint bound, and status/result code from the two Python codebases into named constants defined at module top-level or in shared constants modules. The rule is: **declare the constant before use; never inline**.

Survey results identified ~220 distinct hardcoded values across the two codebases, with many duplicated across 3–15 files (seed=42 appears in 6+ files, batch_size=64 in 10+ files, device="auto" in 15+ files).

## 1. Requirements & Constraints

- **REQ-001**: Every hardcoded value must be extracted to a named constant defined before its first use
- **REQ-002**: Constants must be grouped by category (magic_number, string, default, pattern, constraint, result_code, error_code)
- **REQ-003**: No inline numeric/string literals for business logic values — only structural syntax (imports, decorators, type annotations) may remain inline
- **REQ-004**: The benchmark constants module must live at `benchmarks/src/benchmark/_constants.py`
- **REQ-005**: The Embedding service must extend `service/Embedding/src/core/constants.py` — not create a new file
- **REQ-006**: Module-level constants at the top of individual files are acceptable when the value is used only in that single file and not duplicated elsewhere
- **REQ-007**: Constants shared across 2+ files must move to the shared constants module
- **REQ-008**: Ruff lint (`uv run ruff check src/`) must pass after each phase — rules E, F, I, UP, B, SIM, line-length=100
- **REQ-009**: All unit tests must pass: `uv run pytest --ignore=src/tests/integration/` (benchmarks) and `uv run pytest` (Embedding)
- **CON-001**: Do NOT modify test files (`src/tests/` in benchmarks, `tests/` in Embedding) except to update import references when constants modules change
- **CON-002**: Do NOT modify integration test files or files outside `src/` in either project
- **GUD-001**: Follow existing naming conventions — UPPER_SNAKE_CASE for module-level constants, PascalCase for grouped constants classes
- **PAT-001**: Use `dataclass` or `class` with `@dataclass` for grouped constants (matching the existing `Constants` pattern in the Embedding service)
- **PAT-002**: Use `IntEnum` / `StrEnum` for result codes and error codes
- **PAT-003**: Benchmark model dimension constants belong in the model file itself (each model knows its own dim), NOT in the shared constants module

## 2. Implementation Steps

### Implementation Phase 1

- GOAL-001: Create the shared constants module for the benchmark project and the extended constants module for the Embedding service

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `benchmarks/src/benchmark/_constants.py` with category classes: `MagicNumbers`, `Strings`, `Defaults`, `Patterns`, `Constraints`, `ResultCodes` — all empty shells with docstrings defining the category | | |
| TASK-002 | Extend `service/Embedding/src/core/constants.py` — add new category classes `Defaults`, `Patterns`, `Constraints`, `ResultCodes` matching the existing `Constants.Image` and `Constants.Model` structure; align with `src/core/config.py` `Settings` defaults | | |
| TASK-003 | Add `__all__` exports to both constants modules for clean import syntax | | |
| TASK-004 | Run ruff check on both projects to verify no lint errors from new files | | |

### Implementation Phase 2

- GOAL-002: Extract the 10 most-duplicated numeric constants across both codebases

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | Extract `SEED = 42` → `_constants.MagicNumbers.SEED`; replace 6+ usages in `cli/benchmark.py`, `evaluation/thesis.py`, `evaluation/pipeline.py`, `datasets/ground_truth.py` | | |
| TASK-006 | Extract `BATCH_SIZE = 64` → `_constants.MagicNumbers.BATCH_SIZE`; replace 10+ usages in `embeddings/generator.py`, `metrics/throughput.py`, `evaluation/benchmark.py`, `evaluation/thesis.py`, `evaluation/pipeline.py`, `cli/benchmark.py` | | |
| TASK-007 | Extract `DEFAULT_K_VALUES = [1, 5, 10, 20]` / `DEFAULT_THESIS_K = [5, 10, 20]` → `_constants.MagicNumbers.DEFAULT_K_VALUES` / `DEFAULT_THESIS_K_VALUES`; replace in `evaluation/benchmark.py`, `evaluation/thesis.py`, `evaluation/pipeline.py`, `evaluation/evaluator.py`, `cli/benchmark.py` | | |
| TASK-008 | Extract `WARMUP_RUNS = 10` and `BENCHMARK_RUNS = 100` → `_constants.MagicNumbers.WARMUP_RUNS` / `BENCHMARK_RUNS`; replace in `metrics/latency.py`, `evaluation/evaluator.py`, `evaluation/thesis.py`, `evaluation/pipeline.py` | | |
| TASK-009 | Extract `MAX_LATENCY_SAMPLES = 200` → `_constants.Constraints.MAX_LATENCY_SAMPLES`; replace in `evaluation/evaluator.py:144,244`, `evaluation/thesis.py:204,230`, `evaluation/pipeline.py:218` | | |
| TASK-010 | Extract `MS_CONVERSION = 1000.0` → `_constants.MagicNumbers.MS_CONVERSION`; replace 3 usages in `utils/timing.py:97,140` and `metrics/latency.py:42` | | |
| TASK-011 | Extract `MIN_CATEGORY_FREQ = 10` → `_constants.Constraints.MIN_CATEGORY_FREQ`; replace in `datasets/ground_truth.py:82`, `evaluation/thesis.py:90`, `evaluation/pipeline.py:85` | | |
| TASK-012 | Extract `BOOTSTRAP_CONFIDENCE = 0.95` and `BOOTSTRAP_RESAMPLES = 10_000` → `_constants.MagicNumbers`; replace in `evaluation/stats.py:68,69` | | |
| TASK-013 | Extract `N_FOLDS_DEFAULT = 3` → `_constants.MagicNumbers.N_FOLDS_DEFAULT`; replace in `datasets/ground_truth.py:102`, `evaluation/thesis.py:50`, `evaluation/pipeline.py:49`, `cli/benchmark.py` | | |
| TASK-014 | Extract Embedding service port defaults — `PORT = 8000`, `HTTPS_PORT = 8001` → `Constants.Defaults` in `src/core/constants.py`; replace in `src/core/config.py:69,76` and align with `Dockerfile` | | |
| TASK-015 | Extract Embedding service `MIN_API_KEY_LENGTH = 16` → `Constants.Constraints`; replace in `src/core/config.py:86` | | |
| TASK-016 | Extract Embedding service `THREAD_COUNT_DEFAULT = 4`, `THREAD_COUNT_MIN = 1`, `THREAD_COUNT_MAX = 128` → `Constants.Constraints`; replace in `src/core/config.py:154-165` | | |
| TASK-017 | Ruff check + pytest on both projects | | |

### Implementation Phase 3

- GOAL-003: Extract frequently used string constants across both codebases

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-018 | Extract metric dict keys → `_constants.Strings.METRIC_KEYS` as a class with attributes: `MAP = "map"`, `PRECISION = "precision"`, `RECALL = "recall"`, `NDCG = "ndcg"`, `LATENCY = "latency"`, `P50_MS = "p50_ms"`, `P95_MS = "p95_ms"`, `P99_MS = "p99_ms"`, `MEAN_MS = "mean_ms"`, `STD_MS = "std_ms"`, `THROUGHPUT = "throughput_per_sec"`; replace in `evaluation/comparison.py`, `evaluation/evaluator.py`, `reporting/markdown.py`, `reporting/typst.py`, `cli/benchmark.py`, `utils/timing.py` | | |
| TASK-019 | Extract split/status strings → `_constants.Strings.SPLITS` with: `TRAIN = "train"`, `TEST = "test"`, `VAL = "val"`, `OTHER = "Other"`; replace in `datasets/loader.py:39,61`, `cli/benchmark.py:127,135`, `evaluation/thesis.py:168,174`, `evaluation/pipeline.py:175,179`, `datasets/ground_truth.py:97` | | |
| TASK-020 | Extract CLI sentinel strings → `_constants.Strings.CLI` with: `ALL = "all"`, `AUTO = "auto"`, `LIST = "list"`, `STATS = "stats"`, `CLEAR = "clear"`; replace in `cli/benchmark.py` (8+ occurrences of "all", device "auto" across all model adapters and CLI) | | |
| TASK-021 | Extract `DEFAULT_DATASET_NAME = "deepfashion"` / `DEFAULT_DATASET_ROOT` → `_constants.Defaults`; replace in `cli/benchmark.py:44,46,64,199,300` | | |
| TASK-022 | Extract `DEFAULT_LOG_LEVEL = "INFO"` → `_constants.Defaults.DEFAULT_LOG_LEVEL`; replace in `utils/logging.py:23`, `cli/benchmark.py:69,217,321,410` | | |
| TASK-023 | Extract Embedding service `X_API_KEY_HEADER = "X-API-Key"` → `Constants.Strings`; replace in `src/api/routers/inference.py:27` | | |
| TASK-024 | Extract Embedding service `VERSION = "1.0.0"` → `Constants.Strings.VERSION`; replace in `src/core/telemetry.py:84`, `src/main.py:45` | | |
| TASK-025 | Extract Embedding service `ONNX_FILENAME = "model.onnx"` → `Constants.Strings`; replace in `src/services/inference_engine.py:112,114`, `src/api/routers/inference.py:200`, `scripts/export/base.py:33` | | |
| TASK-026 | Ruff check + pytest on both projects | | |

### Implementation Phase 4

- GOAL-004: Extract default directory paths and output location patterns

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-027 | Extract default paths → `_constants.Defaults.PATHS` dataclass: `CACHE_DIR = Path("data/cache")`, `OUTPUTS_ROOT = Path("outputs")`, `METRICS_DIR = Path("outputs/metrics")`, `REPORTS_DIR = Path("outputs/reports")`, `TABLES_DIR = Path("outputs/tables")`, `FIGURES_DIR = Path("outputs/figures")`, `THESIS_DIR = Path("outputs/thesis")`, `PIPELINE_DIR = Path("outputs/pipeline")`, `EMBEDDINGS_DIR = Path("outputs/embeddings")`; replace across `embeddings/cache.py`, `embeddings/storage.py`, `reporting/json.py`, `reporting/csv.py`, `reporting/markdown.py`, `reporting/typst.py`, `reporting/charts.py`, `reporting/pipeline.py`, `cli/benchmark.py` | | |
| TASK-028 | Extract default data paths → `_constants.Defaults.DATA` dataclass: `DATASET_ROOT = Path("data/raw/deepfashion")`, `SPLIT_FILE = Path("data/splits/deepfashion/test.json")`, `SPLITS_DIR = Path("outputs/thesis/splits")`; replace in `cli/benchmark.py`, `datasets/ground_truth.py` | | |
| TASK-029 | Extract log file name patterns → `_constants.Patterns.LOG_FILES` dataclass: `RUN = "benchmark.log"`, `THESIS = "thesis.log"`, `PIPELINE = "pipeline.log"`; replace in `cli/benchmark.py:86,231,336` | | |
| TASK-030 | Extract output filename patterns → `_constants.Patterns.OUTPUT_FILES` dataclass: `COMPARISON_JSON = "benchmark.json"`, `CSV = "benchmark.csv"`, `MARKDOWN = "summary.md"`, `THESIS_RESULTS = "thesis_results.json"`, `PIPELINE_RESULTS = "pipeline_results.json"`; replace in `reporting/json.py`, `reporting/csv.py`, `reporting/markdown.py`, `cli/benchmark.py` | | |
| TASK-031 | Extract Typst table filename patterns → `_constants.Patterns.TYPST_FILES` dataclass: `PRECISION = "precision.typ"`, `RECALL = "recall.typ"`, `NDCG = "ndcg.typ"`, `LATENCY = "latency.typ"`, `MAP_SUMMARY = "map_summary.typ"`, `THESIS_AGGREGATE = "thesis_aggregate.typ"`, `THESIS_EFFICIENCY = "thesis_efficiency.typ"`, `PIPELINE_PRODUCTION = "pipeline_production.typ"`; replace in `reporting/typst.py`, `reporting/pipeline.py` | | |
| TASK-032 | Extract glossary column/field name strings → `_constants.Strings.DATASET_FIELDS` dataclass: `ID = "id"`, `IMAGE_PATH = "image_path"`, `LABEL = "label"`, `PRODUCT_ID = "product_id"`, `MASTER_CATEGORY = "masterCategory"`, `SUB_CATEGORY = "subCategory"`, `BASE_COLOUR = "baseColour"`, `SPLIT = "split"`; replace in `datasets/loader.py:77-79`, `datasets/ground_truth.py:39-44,48-54,61,90-93,126-127,136-144,150` | | |
| TASK-033 | Extract Embedding service `CORS_ORIGINS_DEFAULT` → `Constants.Defaults`; replace `["http://localhost:3000", "http://localhost:5173"]` in `src/core/config.py:128` | | |
| TASK-034 | Extract Embedding service `RATE_LIMIT_DEFAULT = "50/minute"` → `Constants.Defaults`; replace in `src/core/config.py:91` and doc-align with `.env.template` value | | |
| TASK-035 | Ruff check + pytest on both projects | | |

### Implementation Phase 5

- GOAL-005: Extract constraint bounds, pattern templates, and FAISS/pgvector parameters

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-036 | Extract FAISS/pgvector parameters → `_constants.MagicNumbers.FAISS`: `N_LISTS = 100`, `N_PROBE = 10`, `IVFFLAT_MIN_FACTOR = 39`; replace in `retrieval/faiss.py:34,58`, `retrieval/pgvector.py:191`, `evaluation/pipeline.py:55`, `cli/benchmark.py:320` | | |
| TASK-037 | Extract chart constants → `_constants.MagicNumbers.CHARTS` dataclass: `FIG_SIZE_PRECISION = (7, 4.5)`, `FIG_SIZE_LATENCY = (8, 4.5)`, `PNG_DPI = 150`, `BAR_WIDTH = 0.25`, `MAP_X_LIMIT_MULTIPLIER = 1.15`, `MAP_X_LIMIT_ABS = 1.05`; replace in `reporting/charts.py` | | |
| TASK-038 | Extract rounding decimals → `_constants.MagicNumbers.ROUNDING` dataclass: `METRIC_DECIMALS = 4`, `LATENCY_DECIMALS = 1`; replace in `reporting/markdown.py:22,97-100`, `reporting/typst.py:38,234-237`, `evaluation/stats.py`, `evaluation/evaluator.py` | | |
| TASK-039 | Extract Embedding service `ONNX_OPSET = 17` → `Constants.MagicNumbers.ONNX_OPSET`; replace in `scripts/export/vision.py:17,43,72,107` | | |
| TASK-040 | Extract Embedding service `L2_EPSILON = 1e-9` → `Constants.Constraints.L2_EPSILON`; replace in `src/models/base.py:165` | | |
| TASK-041 | Extract Embedding service `HTTP_TIMEOUT = 10` (image download client) → `Constants.MagicNumbers`; replace in `src/models/base.py:106` | | |
| TASK-042 | Extract cache file pattern → `_constants.Patterns.CACHE_NPZ = "{model_slug}__{dataset_name}.npz"`; replace in `embeddings/cache.py:33` (used in `_npz_path`) | | |
| TASK-043 | Extract fold split file pattern → `_constants.Patterns.FOLD_TRAIN = "fold_{fold_idx}_train.json"` / `FOLD_TEST = "fold_{fold_idx}_test.json"`; replace in `datasets/ground_truth.py:160-161`, `evaluation/thesis.py:187-188`, `evaluation/pipeline.py:197` | | |
| TASK-044 | Extract image path template → `_constants.Patterns.IMAGE_PATH = "images/{product_id}.jpg"`; replace in `datasets/ground_truth.py:137` | | |
| TASK-045 | Ruff check + pytest on both projects | | |

### Implementation Phase 6

- GOAL-006: Extract result codes, error codes, and status enums; finalize Embedding service constants consolidation

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-046 | Create `_constants.ResultCodes` with `EXIT_FAILURE = 1`; replace `code=1` in `cli/benchmark.py:105,433,529` | | |
| TASK-047 | Create `_constants.Strings.PLACEHOLDERS` dataclass: `MISSING_TYPST = "---"`, `MISSING_MD = "—"`, `BEST_MODEL_FALLBACK = "N/A"`; replace in `reporting/typst.py:49`, `reporting/markdown.py:33,58` | | |
| TASK-048 | Create `_constants.Strings.FILE_ENCODING = "utf-8"`; replace all `encoding="utf-8"` occurrences across all benchmark Python files (~15 occurrences) | | |
| TASK-049 | Verify Embedding service `Constants.Model` dimension values: ensure `EFFICIENTNET_B0 = 1280`, `CLIP_VIT_B16 = 512`, `FASHION_CLIP = 512`, `DINOV2_VITS14 = 384`, `ONNX_FASHION_CLIP = 768`, `RESNET50 = 2048` are used everywhere they are referenced; add missing imports in `test_api_inference.py` | | |
| TASK-050 | Extract Embedding service domain error code strings → `Constants.Errors` dataclass with all error code constants (MODEL_NOT_FOUND, INFERENCE_ERROR, etc. from `src/schemas/`); replace inline string literals across `src/schemas/inferences/__init__.py`, `src/api/middleware/exception_handlers.py` | | |
| TASK-051 | Extract Embedding service error-to-HTTP mapping → `Constants.ResultCodes` with an `IntEnum` for each error type's HTTP status (Validation=400, Conflict=409, NotFound=404, BadRequest=400, InternalError=500, Unauthorized=401, Forbidden=403); replace the inline dict/method in `src/schemas/results/error.py` | | |
| TASK-052 | Consolidate `THESIS_MODEL_KEYS` — currently duplicated identically in `evaluation/thesis.py:29` and `evaluation/pipeline.py:27`; move to `_constants.Strings.THESIS_MODEL_KEYS` or `_constants.Defaults.THESIS_MODEL_KEYS`; update both files to import | | |
| TASK-053 | Consolidate the `_PALETTE` hex color list from `reporting/charts.py:28-34` into `_constants.Strings.PALETTE` | | |
| TASK-054 | Final ruff check + pytest on both projects | | |

## 3. Alternatives

- **ALT-001**: Use YAML/TOML config files instead of Python constants — rejected because Python constants are type-checkable, importable, and follow existing codebase conventions; config files would require a loading layer with no benefit for values that never change at runtime
- **ALT-002**: Use environment variables for all values — rejected because most of these are internal code constants (percentile indices, chart DPI, rounding decimals) that should never be user-configurable; only config defaults belong in env vars
- **ALT-003**: Refactor every value in a single monolithic pass — rejected because the survey found ~220 values across 80+ files; phased extraction reduces risk, allows per-phase verification, and produces atomic commits
- **ALT-004**: Keep constants inline but add explanatory comments — rejected per REQ-001; comments explain WHY but don't eliminate the duplication or maintenance burden of changing a value in 10 places

## 4. Dependencies

- **DEP-001**: `benchmarks/pyproject.toml` — must not require any new external packages (pure Python constants only)
- **DEP-002**: `service/Embedding/pyproject.toml` — must not require any new external packages
- **DEP-003**: The existing `Constants` class in `service/Embedding/src/core/constants.py` — the pattern to follow and extend
- **DEP-004**: No changes to `Directory.Packages.props`, `pyproject.toml`, or any project configuration

## 5. Files

- **FILE-001** (create): `benchmarks/src/benchmark/_constants.py` — master constants module for benchmark project
- **FILE-002** (modify): `service/Embedding/src/core/constants.py` — extend with new category classes
- **FILE-003** to **FILE-040** (modify): All benchmark source files in `src/benchmark/` — replace inline values with constants imports (see per-task file lists above)
- **FILE-041** to **FILE-060** (modify): All Embedding service source files — replace inline values with constants imports (see per-task file lists above)

## 6. Testing

- **TEST-001**: `uv run ruff check src/` must produce zero new errors after each phase in both projects
- **TEST-002**: `uv run pytest --ignore=src/tests/integration/` (benchmarks) must pass all 136 tests after each phase
- **TEST-003**: `uv run pytest` (Embedding service) must pass all tests after each phase
- **TEST-004**: Manual review — verify no inline values remain in touched files by searching for bare numeric/string literals at function scope

## 7. Risks & Assumptions

- **RISK-001**: Phase 6 modifies error code schemas — could break API response contracts if error code strings change; mitigation: verify that domain error code STRING VALUES remain identical, only the declaration location changes
- **RISK-002**: Renaming existing constants already used across the codebase (Embedding service `Constants.Image.SIZE`, `Constants.Model.*`) could cause silent regressions; mitigation: use search-and-replace with explicit import updates, then run full test suite
- **RISK-003**: The Embedding service `scripts/export/` directory is not covered by pytest; manual verification required for TASK-024 and TASK-039
- **ASSUMPTION-001**: All hardcoded values are safe to extract — none depend on dynamic computation at the point of use (validated by the survey)
- **ASSUMPTION-002**: Phase ordering by duplication count minimizes merge conflicts and maximizes early payoff

## 8. Related Specifications / Further Reading

- `benchmarks/AGENTS.md` — ruff rules, test commands, project conventions
- `service/Embedding/AGENTS.md` — test commands, project conventions
- `service/Embedding/src/core/constants.py` — existing constants pattern to extend
- `service/Embedding/src/core/config.py` — pydantic-settings `Settings` class with defaults to align
- `docs/codebase/CONVENTIONS.md` — coding conventions
- `guide/code-commenting/CommentingRules.xml` — commenting standard used throughout
