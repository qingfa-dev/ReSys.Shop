# Codebase Concerns

## Core Sections (Required)

### 1) Top Risks (Prioritized)

| Severity | Concern | Evidence | Impact | Suggested action |
|----------|---------|----------|--------|------------------|
| High | Hardcoded database credentials in CLI defaults | `src/benchmark/cli/research.py:L74` — `postgresql://research_user:research_password@localhost:5433/research_sandbox` | Credential leak if code is shared/public. Violates security best practices. | Move to environment variables or `.env` file; add `.env.example` |
| Medium | No cache invalidation mechanism | `src/benchmark/embeddings/cache.py` — cache key is `model_slug + dataset_name`, no content hash or timestamp check | If dataset changes but name stays same, stale embeddings are silently used | Add dataset checksum/hash to cache key or `--no-cache` workflow awareness |
| Medium | Models never unloaded from GPU memory | `src/benchmark/models/base.py` — no `unload()` or cleanup method; `BenchmarkRunner.run()` iterates models sequentially | GPU OOM when running many models in single process | Add explicit `unload()` to EmbeddingModel interface or use `torch.cuda.empty_cache()` between models |
| Medium | No production-grade observability | No APM, Prometheus, or health checks. Latency measured only in-process. | Hard to monitor pipeline health in production deployment | Add structured metrics export; consider OpenTelemetry or Prometheus client |
| Low | README describes `uv run benchmark benchmark --dataset deepfashion` but this dataset path may not be prepared by default | `README.md:L29-41` vs `Makefile:L17` (default `DATASET_NAME=fashion-product-small`) | User confusion; README and Makefile defaults diverge | Align README examples with Makefile defaults or add dataset preparation docs |

### 2) Technical Debt

| Debt item | Why it exists | Where | Risk if ignored | Suggested fix |
|-----------|---------------|-------|-----------------|---------------|
| Legacy API wrappers in cache module | Backward compatibility during refactor | `src/benchmark/embeddings/cache.py:L94-120` | Confusion about which API to use; duplicate code paths | Deprecate `save_embeddings`/`load_embeddings`/`is_cached` after migrating all callers |
| Research module duplicates core logic | Rapid prototyping of research features without refactoring shared code | `src/benchmark/research/datasets.py` (duplicates `loader.py`), `src/benchmark/research/evaluation.py` (duplicates evaluator logic) | Divergent bug fixes; core changes don't propagate to research code | Extract shared dataset/evaluation logic into core packages |
| `_LazyRegistry` and `get_registry()` both build registry from scratch | Quickfix to add device-aware creation without breaking lazy import pattern | `src/benchmark/models/__init__.py:L22-66` | Inconsistent — `REGISTRY` uses `_register()` (no device arg), `get_registry(device)` creates new instances. Two code paths to maintain. | Unify: make `REGISTRY` also device-aware or deprecate one path |
| `benchmarks.v001/` duplicates entire project structure | Likely a version snapshot or experimental workspace | `benchmarks.v001/` (complete copy of project with experiments) | Confusion about canonical code location; divergent changes | Clarify purpose (archive? workspace?) and add README note; consider removing |
| Lossy model identity tuple unpacking | FashionCLIP handles `features` being a tuple — model-specific workaround | `src/benchmark/models/fashion_clip.py:L63-66` | Fragile; if HuggingFace API changes return type, extraction may silently fail | Add type assertion or canonical extraction helper shared across adapters |

### 3) Security Concerns

| Risk | OWASP category (if applicable) | Evidence | Current mitigation | Gap |
|------|--------------------------------|----------|--------------------|-----|
| Hardcoded credentials (postgres connection string) | A07:2021 — Identification and Authentication Failures | `src/benchmark/cli/research.py:L74` | None | No credential separation; no `.env` support for research commands |
| No input validation on file paths from CLI | A03:2021 — Injection | `src/benchmark/cli/benchmark.py` — user-supplied `dataset_root`, `split_file` used directly as `Path` objects | Python `Path` prevents some injection; image loading via `PIL.Image.open()` is relatively safe | Add path existence checks and restrict to allowed directories |
| No authentication on external model downloads | N/A | HuggingFace/OpenCLIP downloads are anonymous public models | Trust in model integrity from HuggingFace | [ASK USER] Is model integrity verification needed? (e.g., checksum validation) |

### 4) Performance and Scaling Concerns

| Concern | Evidence | Current symptom | Scaling risk | Suggested improvement |
|---------|----------|-----------------|-------------|-----------------------|
| Sequential model evaluation | `src/benchmark/evaluation/benchmark.py:L80` — `for key in keys:` loop | Models run one at a time | On multi-GPU systems, significant idle GPU time | Add parallel model execution (multiprocessing or per-GPU process) |
| Sequential image loading + embedding per batch | `src/benchmark/embeddings/generator.py:L85-107` — images loaded one-by-one per batch in main thread | Bottleneck when I/O latency > GPU time | With fast GPU, CPU becomes bottleneck | Add `DataLoader`-style prefetching with multi-worker image loading |
| Sequential per-query retrieval | `src/benchmark/retrieval/cosine.py:L68-70` — `for i, q in enumerate(queries)` loop | O(Q × N × D) for Q queries, N gallery, D dim | Slow for large datasets (>100k images) | Use FAISS index (already in dependencies but not used in main pipeline) or batched matrix multiplication |
| NPZ cache grows unbounded | `src/benchmark/embeddings/cache.py:L36-59` — no size limit or eviction policy | Disk usage grows linearly with (models × datasets) | Disk exhaustion on long-running systems | Add cache size limit, age-based eviction, or configurable cache location |

### 5) Fragile/High-Churn Areas

| Area | Why fragile | Churn signal | Safe change strategy |
|------|-------------|-------------|----------------------|
| `src/benchmark/models/__init__.py` | Every model addition requires editing both `_register()` and `get_registry()` — easy to forget one | 2 commits in last 90 days (highest churn) | Consider single-source model list, generate both registries from it |
| `src/benchmark/cli/benchmark.py` | 289 lines mixing CLI definition, pipeline orchestration, and report generation; single responsibility principle violated | 2 commits in last 90 days | Extract pipeline orchestration to separate function; keep CLI to arg parsing + calling |
| `src/benchmark/evaluation/benchmark.py` | `BenchmarkRunner._run_single` ties embedding generation, storage, and evaluation — hard to test independently | 2 commits in last 90 days | Extract storage decision to caller; make `_run_single` pure(embedding_result → metrics) |
| `src/benchmark/evaluation/evaluator.py` | `evaluate()` loads images for latency testing (I/O inside evaluator) — breaks separation of concerns | 1 commit in last 90 days | Move image loading for latency to a separate component; evaluate() should only do metric math |

### 6) `[ASK USER]` Questions

1. [ASK USER] Is model integrity verification (checksum/hash validation) needed for HuggingFace downloads?
2. [ASK USER] What is the intended purpose of `benchmarks.v001/`? Is it an archive, a parallel workspace, or should it be removed?
3. [ASK USER] Should the `research` CLI subcommands be merged into the main benchmark workflow, or kept separate long-term?
4. [ASK USER] Is GPU memory management (model unloading between runs) a current concern, or is the single-GPU sequential pattern adequate?
5. [ASK USER] What is the target coverage threshold for test coverage? Should it be enforced in CI?
6. [ASK USER] Should the hardcoded PGVector connection string be moved to environment variables, and should a `.env.example` be created?

### 7) Evidence

- `src/benchmark/cli/research.py:L74` — hardcoded credentials
- `src/benchmark/embeddings/cache.py:L27` — cache key formula
- `src/benchmark/evaluation/benchmark.py:L80` — sequential model loop
- `src/benchmark/retrieval/cosine.py:L68-70` — per-query retrieval loop
- `.codebase-scan.txt:L442-461` — high-churn files
- `src/benchmark/embeddings/cache.py:L94-120` — legacy wrappers
- `src/benchmark/models/__init__.py:L22-66` — dual registry paths
