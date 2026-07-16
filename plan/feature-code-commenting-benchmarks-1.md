---
goal: Apply Code Commenting Standard v3.0 to the benchmarks Python codebase
version: 1.0
date_created: 2026-07-15
owner: Engineering Standards
status: 'Planned'
tags: feature, commenting, benchmarks, python, standards
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Apply the structured Code Commenting Standard v3.0 (`guide/code-commenting/CommentingRules.xml`) to all 55 Python source files in `benchmarks/src/benchmark/`. The standard defines 10 label categories (CAT-1 through CAT-10) plus Temporal Markers and Google Docstring standards for Python. Each comment must explain WHY, never WHAT; use imperative verbs; follow the Semantic Density Principle (every token earns its place).

## 1. Requirements & Constraints

- **REQ-001**: All 55 source files under `benchmarks/src/benchmark/` must receive structured label comments where naming/structure alone cannot carry intent
- **REQ-002**: All public functions must have Google Docstring-style docstrings (`Args:`, `Returns:`, `Raises:`)
- **REQ-003**: CAT-10 agent annotations must use `KEY=VALUE` form for machine parsing
- **REQ-004**: Ruff lint (`uv run ruff check src/`) must pass after all changes — rules E, F, I, UP, B, SIM
- **REQ-005**: All existing tests (`uv run pytest --ignore=src/tests/integration/`) must pass
- **REQ-006**: Max line length 100 characters per F3 rule
- **REQ-007**: One label, one action — never join two actions with "and" (F8)
- **REQ-008**: Comments on their own line — never trailing a code statement (F1 exception for inline data literals)
- **CON-001**: Do NOT modify `src/tests/` test files — commenting standard applies to production code only
- **CON-002**: Do NOT modify `docs/`, `outputs/`, `data/`, `experiments/`, `configs/` — source code only
- **CON-003**: Do NOT modify `old/` directory
- **PAT-001**: Follow existing Google Docstring patterns in the codebase (see `benchmark/models/base.py`, `benchmark/retrieval/cosine.py`)
- **GUD-001**: Use `# Label: Imperative sentence.` format for all inline labels
- **GUD-002**: Write docstrings in `Args:` / `Returns:` / `Raises:` sections per Google style

## 2. Implementation Steps

### Implementation Phase 1: Metrics & Models — Core Academic Domain

- GOAL-001: Annotate the 7 metric files and 13 model adapter files with CAT-1 (Validate), CAT-3 (Compute/Transform/Explain), CAT-4 (Enforce), CAT-10 (Invariant/Contract/Assume), plus docstrings

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | `metrics/precision.py` — add CAT-1 `Validate:` for k==0 guard; `Compute:` on `precision_at_k` and `mean_precision_at_k`; Google docstrings on both functions | | |
| TASK-002 | `metrics/recall.py` — add `Compute:` on `recall_at_k` and `mean_recall_at_k`; docstrings; `Explain:` on relevant_count fallback | | |
| TASK-003 | `metrics/map.py` — add `Compute:` on `average_precision` and `mean_average_precision`; `Explain:` on denominator capping logic and rank iteration | | |
| TASK-004 | `metrics/ndcg.py` — add `Compute:` on `dcg_at_k`, `ideal_dcg_at_k`, `ndcg_at_k`; `Explain:` on logarithmic discount rationale; docstrings | | |
| TASK-005 | `metrics/latency.py` — add `Validate:` warmup/batch params; `Profile:` on timed runs; `Compute:` on per-image latency | | |
| TASK-006 | `metrics/throughput.py` — add `Compute:` on throughput formula; `Profile:` on batch timing | | |
| TASK-007 | `metrics/recall_comparison.py` — add `Compute:` on recall overlap; `Validate:` on shape mismatch; `Explain:` on set-intersection formula | | |
| TASK-008 | `models/base.py` — add `Invariant:` on L2-norm guarantee; `Contract:` on `embed`/`embed_batch`; `Boundary:` marking strategy interface; `AgentHint:` for subclass contract | | |
| TASK-009 | `models/fashion_clip.py` — add `Call:` on HF model load; `Create:` on model init; `Compute:` on embed; `Context:` referencing SIGIR 2022 paper | | |
| TASK-010 | `models/clip_b32.py` — add `Call:` on open_clip load; `Compute:` on embed; `Context:` referencing ICML 2021 paper | | |
| TASK-011 | `models/clip_generic.py` — add `Call:` on HF CLIP load; `Compute:` on embed; `Context:` on purpose of generic vs fashion-tuned | | |
| TASK-012 | `models/clip_l14.py`, `models/clip_vit_b16.py`, `models/siglip.py`, `models/eva_clip.py`, `models/convnext_tiny.py`, `models/dinov2_vits14.py` — each: `Call:` on model load; `Compute:` on embed; `Context:` referencing respective papers | | |
| TASK-013 | `models/resnet50.py` — add `Transform:` on classifier removal (`fc = Identity`); `Compute:` on embed; `Context:` referencing He et al. 2016 | | |
| TASK-014 | `models/efficientnet_b0.py` — add `Transform:` on classifier removal; `Create:` on transform pipeline; `Compute:` on embed | | |
| TASK-015 | `models/registry.py` — add `Boundary:` on module entry point; `Context:` explaining how to add new models; docstrings on `get_model`/`get_models` | | |
| TASK-016 | `models/__init__.py` — add `Context:` on lazy import strategy; `AgentHint:` for add-model steps; docstrings | | |

### Implementation Phase 2: Evaluation & Retrieval — Pipeline Core

- GOAL-002: Annotate the 4 retrieval files and 6 evaluation files with CAT-5 (Await/Retry/Fallback), CAT-6 (Acquire/Release/Cache), CAT-7 (Catch/Compensate/Degrade), CAT-8 (Call/Send), CAT-9 (Log/Monitor/Profile), plus docstrings

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-017 | `evaluation/evaluator.py` — add `Contract:` on `evaluate`/`evaluate_split`; `Compute:` on metric aggregation; `Filter:` on sample loading; `Explain:` on split-aware vs self-retrieval difference; `Log:` on result summary | | |
| TASK-018 | `evaluation/benchmark.py` — add `Transform:` on embedding generation; `Cache:` on npz persistence; `Call:` on registry lookup; `Batch:` on model iteration; `Profile:` via `timed` context manager; `Log:` on start/finish | | |
| TASK-019 | `evaluation/thesis.py` — add `Enforce:` on styles.csv existence; `Aggregate:` on fold-level metrics; `Compute:` on bootstrap CI; `Log:` on fold progress; `Profile:` on model load time; `Explain:` on RAM measurement | | |
| TASK-020 | `evaluation/pipeline.py` — add `Call:` on pgvector ingestion; `Retry:` on DB connection; `Catch:` on pgvector unavailability; `Aggregate:` on fold results; `Explain:` on dim-to-table mapping | | |
| TASK-021 | `evaluation/stats.py` — add `Compute:` on aggregate_mean_std, cohens_d, bootstrap_ci; `Explain:` on manual bootstrap (no scipy dep); `Validate:` on group length match for Cohen's d | | |
| TASK-022 | `evaluation/comparison.py` — add `Sort:` on rank_models; `Transform:` on comparison_table; `Explain:` on latency sort direction (ascending); docstrings | | |
| TASK-023 | `retrieval/cosine.py` — add `Compute:` on cosine_similarity; `Explain:` on argpartition + sort O(N + k log k) vs argsort O(N log N); `Filter:` on self-exclusion mask; docstrings on all 3 functions | | |
| TASK-024 | `retrieval/faiss.py` — add `Create:` on IVFFlat index; `Call:` on faiss import (try/except); `Fallback:` on FlatIP for small galleries; `Acquire:` on index build; `Explain:` on IVFFlat Voronoi cells | | |
| TASK-025 | `retrieval/pgvector.py` — add `Acquire:` on DB connection; `Call:` on pgvector queries; `Create:` on index; `Release:` via `close`/`__exit__`; `Purge:` on `clear_table`; `Catch:` on import errors; `Validate:` on table params | | |
| TASK-026 | `retrieval/__init__.py` — add docstring with backend descriptions | | |

### Implementation Phase 3: Datasets, Embeddings & Utils — Foundation Layer

- GOAL-003: Annotate the 5 dataset files, 4 embedding files, and 5 util files with CAT-1 (Validate/Guard), CAT-2 (Create/Initialize), CAT-3 (Parse/Transform/Filter), CAT-6 (Cache/Purge), CAT-8 (Serialize/Deserialize), plus docstrings

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-027 | `datasets/loader.py` — add `Create:` on Sample/FashionDataset; `Parse:` on JSON split file; `Filter:` on image iteration with OSError skip; `Validate:` on sample access guard; `Generate:` on product IDs | | |
| TASK-028 | `datasets/ground_truth.py` — add `Compute:` on relevance key construction; `Enforce:` on min_category_freq; `Aggregate:` on stratified fold splitting; `Explain:` on 3-part relevance key logic; `Serialize:` on fold JSON output | | |
| TASK-029 | `datasets/validators.py` — add `Validate:` on image path existence; `Log:` on validation results | | |
| TASK-030 | `datasets/transforms.py` — add `Transform:` on resize_pad and center_crop; `Explain:` on aspect-ratio preservation | | |
| TASK-031 | `datasets/__init__.py` — add docstring | | |
| TASK-032 | `embeddings/generator.py` — add `Cache:` on npz load/save; `Batch:` on batched inference; `Fallback:` on cache miss; `Log:` on cache hit/miss; `Explain:` on sample-ID alignment reconstruction; `Contract:` on `generate` | | |
| TASK-033 | `embeddings/cache.py` — add `Cache:` on save/load/exists; `Deserialize:` on npz load; `Serialize:` on npz save; `Deprecated` markers on legacy aliases; docstrings on all public API | | |
| TASK-034 | `embeddings/storage.py` — add `Cache:` on durable persist; `Serialize:` on npz save; `Deserialize:` on npz load; `Context:` distinguishing from cache.py | | |
| TASK-035 | `embeddings/__init__.py` — add docstring | | |
| TASK-036 | `utils/logging.py` — add `Create:` on handler setup; `Log:` on formatter config; docstrings with `Args:`/`Returns:` | | |
| TASK-037 | `utils/timing.py` — add `Compute:` on LatencyStats percentiles; `Profile:` on Timer.measure; `Collect:` on sample accumulation; `Create:` on Timer; docstrings on all public classes/functions | | |
| TASK-038 | `utils/device.py` — add `Acquire:` on device resolution; `Fallback:` on CPU fallback; `Log:` on device choice; `Explain:` on auto detection order | | |
| TASK-039 | `utils/random_seed.py` — add `Reset:` on seed; `Call:` on torch seed; docstring | | |
| TASK-040 | `utils/__init__.py` — add docstring | | |

### Implementation Phase 4: Reporting & CLI — Output Layer

- GOAL-004: Annotate the 7 reporting files and 3 CLI files with CAT-2 (Create/Assign), CAT-3 (Format/Transform/Generate), CAT-8 (Serialize), CAT-9 (Log/Trace/Monitor), plus docstrings

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-041 | `reporting/json.py` — add `Serialize:` on JSON output; `Create:` on output dir; `Log:` on write paths; docstrings | | |
| TASK-042 | `reporting/csv.py` — add `Serialize:` on CSV output; `Transform:` on metric flattening via comparison_table; `Sort:` via rank_models; `Log:` | | |
| TASK-043 | `reporting/markdown.py` — add `Format:` on Markdown tables; `Generate:` on report header with timestamp; `Log:` | | |
| TASK-044 | `reporting/charts.py` — add `Create:` on matplotlib figures; `Generate:` on precision/recall/latency/mAP charts; `Explain:` on palette choice; `Log:` on file paths | | |
| TASK-045 | `reporting/typst.py` — add `Format:` on Typst table generation; `Generate:` on auto-gen comment; `Create:` on figure blocks; `Explain:` on table structure; docstrings on all writers | | |
| TASK-046 | `reporting/pipeline.py` — add `Format:` on pipeline Typst tables; `Serialize:` on JSON output; `Log:` | | |
| TASK-047 | `reporting/__init__.py` — add docstring | | |
| TASK-048 | `cli/benchmark.py` — add `Enforce:` on model key validation; `Call:` on registry/runner/reporters; `Batch:` on model iteration; `Log:` on config table; `Cache:` on cache management; `Parse:` on CLI options; `Explain:` on split-aware mode | | |
| TASK-049 | `cli/main.py` — add docstring with CLI usage examples | | |
| TASK-050 | `cli/__init__.py` — add docstring | | |
| TASK-051 | `__init__.py` — add docstring with package overview and quickstart | | |

### Implementation Phase 5: Verification & Quality Gate

- GOAL-005: Validate all changes pass lint, tests, and conform to the Commenting Standard

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-052 | Run `uv run ruff check src/` and fix any violations | | |
| TASK-053 | Run `uv run pytest --ignore=src/tests/integration/` and confirm all tests pass | | |
| TASK-054 | Run `uv run pytest` (full suite) if Docker/pgvector available | | |
| TASK-055 | Manual audit: verify no AP-1 (redundancy), AP-2 (vagueness), AP-3 (over-commenting), AP-4 (inconsistent style), AP-6 (commented-out code) | | |
| TASK-056 | Manual audit: verify all CAT-10 annotations use KEY=VALUE format, no AP-7 (verbose agent context) | | |
| TASK-057 | Update benchmarks/AGENTS.md to reference the commenting standard in conventions | | |

## 3. Alternatives

- **ALT-001**: One big-bang comment pass across all files — rejected because it makes review impossible and risks introducing too many simultaneous changes
- **ALT-002**: Using an automated comment generation tool — rejected because the standard requires human-curated, semantically precise annotations (ETH AGENTbench 2026 finding); automated tools produce verbose, low-quality annotations
- **ALT-003**: Commenting only new code (leave existing code uncommented) — rejected because the standard requires consistent application across the codebase for AI agent context

## 4. Dependencies

- **DEP-001**: Python 3.12+ with `uv` installed (`uv sync` must work)
- **DEP-002**: `ruff` installed via `uv sync --extra dev` (lint rules E, F, I, UP, B, SIM)
- **DEP-003**: `pytest` installed via `uv sync --extra dev` (test runner)
- **DEP-004**: Existing test suite must be green before changes (baseline)
- **DEP-005**: `guide/code-commenting/CommentingRules.xml` v3.0 as authoritative standard
- **DEP-006**: `guide/code-commenting/README.md` for human-readable label reference

## 5. Files

- **FILE-001 to FILE-055**: All 55 `.py` files under `benchmarks/src/benchmark/` — each file receives structured label comments and docstring upgrades per the Commenting Standard v3.0
- **FILE-056**: `benchmarks/src/benchmark/__init__.py` — package-level docstring
- **FILE-057**: `benchmarks/AGENTS.md` — reference to commenting standard in conventions section
- **FILE-058**: `guide/code-commenting/CommentingRules.xml` — authoritative source (read-only input)
- **FILE-059**: `guide/code-commenting/README.md` — human-readable reference (read-only input)

## 6. Testing

- **TEST-001**: `uv run ruff check src/` — lint must pass with zero violations; verifies no syntax errors, no unused imports, consistent formatting
- **TEST-002**: `uv run pytest --ignore=src/tests/integration/` — all unit tests must pass; verifies no behavioral changes from comment additions
- **TEST-003**: Manual diff review — verify no commented-out code (AP-6), no stale comments (AP-5), no redundancy (AP-1)
- **TEST-004**: Spot-check: for 5 random files, verify each public function has Google Docstring with `Args:`, `Returns:`, `Raises:` sections
- **TEST-005**: Spot-check: verify CAT-10 labels use `KEY=VALUE` form and no AP-7 verbosity
- **TEST-006**: Verify that `# TEMP Debug:` markers and `# TODO():` markers (if any existing) use the standard format from §5 Temporal Markers

## 7. Risks & Assumptions

- **RISK-001**: Adding comments could introduce line-length violations (>100 chars) — mitigated by ruff `E501` check and manual review
- **RISK-002**: Over-commenting (AP-3) could make code harder to read — mitigated by Semantic Density Principle (every token earns its place) and strict adherence to "WHY not WHAT"
- **RISK-003**: Tests could fail if comments are accidentally placed mid-statement or break block structure — mitigated by keeping all comments on separate lines per F1
- **ASSUMPTION-001**: The existing test suite has adequate coverage to catch behavioral regressions
- **ASSUMPTION-002**: All 55 source files are equally important to annotate; no file should be skipped
- **ASSUMPTION-003**: Ruff lint rules E, F, I, UP, B, SIM are sufficient for quality enforcement
- **ASSUMPTION-004**: AI agents (Claude Code, Copilot) will benefit from CAT-10 annotations at model boundaries and critical decision points

## 8. Related Specifications / Further Reading

- `guide/code-commenting/CommentingRules.xml` — authoritative XML standard v3.0
- `guide/code-commenting/README.md` — human-readable version with examples
- `guide/code-commenting/SKILL.md` — usage workflow for applying the standard
- `guide/code-commenting/references/label-quick-reference.md` — full label table
- `guide/code-commenting/references/anti-patterns.md` — anti-pattern checklist
- `benchmarks/AGENTS.md` — benchmark project agent guide
- `benchmarks/docs/codebase/CONVENTIONS.md` — existing coding conventions
- `benchmarks/pyproject.toml` — ruff config (line-length=100, select=E,F,I,UP,B,SIM)
- https://google.github.io/styleguide/pyguide.html#38-comments-and-docstrings — Google Python Docstring style
