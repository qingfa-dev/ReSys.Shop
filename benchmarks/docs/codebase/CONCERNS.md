# Codebase Concerns

## Core Sections (Required)

### 1) Top Risks (Prioritized)

| Severity | Concern | Evidence | Impact | Suggested action |
|---|---|---|---|---|
| High | pgvector IVFFlat dimension limit prevents ResNet-50 indexing | `src/benchmark/retrieval/pgvector.py:L170` — `build_index()` called with 2048-d vectors; pgvector caps at 2000 | ResNet-50 production metrics unavailable (0.0 for all pgvector fields). Pipeline gracefully degrades. | Use HNSW index for >2000-dim models, or document as known limitation |
| Medium | No cache invalidation mechanism | `src/benchmark/embeddings/cache.py` — cache key is `model_slug + dataset_name`, no content hash | If dataset content changes but name stays same, stale embeddings silently used | Add dataset checksum to cache key or document `--no-cache` as required after data changes |
| Medium | Models never unloaded from memory | `src/benchmark/models/base.py` — no `unload()` method; runners process models sequentially | Memory pressure on large runs; potential GPU OOM on smaller cards | Add explicit `unload()` to EmbeddingModel or `torch.cuda.empty_cache()` between models |
| Medium | RAM measurement unreliable | `PipelineRunner._measure_peak_ram()` uses `psutil` RSS delta; reports 0.0 or negative on some systems | RAM column in thesis tables unreliable. Cannot verify H3 weight claims without trustworthy RAM data. | Measure RAM externally (e.g., `nvidia-smi` for GPU, `/proc/pid/status` for CPU) or remove from claims |
| Low | No production-grade observability | No APM, Prometheus, health checks | Hard to monitor pipeline health in production | Add structured metrics export if benchmark is deployed as service |

### 2) Technical Debt

| Debt item | Why it exists | Where | Risk if ignored | Suggested fix |
|---|---|---|---|---|
| `retrieve_batch` capping `k` at gallery size | Edge case fix for small galleries | `src/benchmark/retrieval/cosine.py:L67-69` | Minimal — only affects small test datasets | Documented as intentional behavior |
| `Executemany` for pgvector batch insert | Migration from psycopg2 `mogrify` to psycopg3 `executemany` | `src/benchmark/retrieval/pgvector.py:L155` | Slightly slower than `execute_values` for large batches. Acceptable for 5K scale. | Revisit if scaling beyond 100K vectors |
| `_LazyRegistry` and `get_registry()` dual paths | Historical — `get_registry(device)` added later for device-aware creation | `src/benchmark/models/__init__.py:L68-103` | Two code paths to maintain; device-aware vs non-device-aware | Unify into single factory function |
| Output files committed to repo | Pipeline results and splits committed for thesis record | `outputs/pipeline/`, `outputs/thesis/` | Binary files in git history; large diffs on re-run | Move to data store or git-LFS if outputs are final thesis artifacts |
| Old experiments in `experiments/` and `old/` | Historical notebooks and previous versions | `experiments/`, `old/` | Confusion about canonical code location | Archive or remove unused directories |

### 3) Security Concerns

| Risk | Evidence | Current mitigation | Gap |
|---|---|---|---|
| Pgvector connection string in CLI defaults | `src/benchmark/cli/benchmark.py` — `--conn-string` defaults to `postgresql://benchmark:benchmark@localhost:5432/benchmark` | Local dev credentials only; PostgreSQL bound to `localhost` | Acceptable for local dev; should use env vars for any shared/CI environment |
| No input validation on file paths from CLI | User-supplied `dataset_root` and `split_file` paths used directly | Python `Path` provides some protection; `PIL.Image.open()` validates images | Low risk — restricted to local filesystem |
| Model weight download integrity | HuggingFace/OpenCLIP downloads are anonymous public models | Trust in model integrity | [ASK USER] is model checksum verification needed? |

### 4) Performance and Scaling Concerns

| Concern | Evidence | Current symptom | Scaling risk | Suggested improvement |
|---|---|---|---|---|
| Sequential model evaluation | All runners use `for key in keys:` loop | Models run one at a time; GPU idle between models | On multi-GPU systems, significant idle GPU time | Add multiprocessing or per-GPU process per model |
| Sequential per-query retrieval | `src/benchmark/retrieval/cosine.py:L68-70` — per-query loop | O(Q × N × D) for Q queries, N gallery, D dim | Slow for >100K images | Use FAISS or batched matrix multiply |
| NPZ cache grows without bound | `src/benchmark/embeddings/cache.py:L36-59` — no size limit | Disk usage = (models × folds × splits × dim × N × 4 bytes) | For 11 models × 3 folds × 2 splits × 512d × 5K → ~1.7 GB. Acceptable for thesis scale. | Monitor if expanding to larger datasets or more models |
| pgvector `executemany` per-row insert | `src/benchmark/retrieval/pgvector.py:L155` — one INSERT per row | Per-row overhead for 3,300 gallery items | Slow for >100K vectors | Use `COPY` or batched `VALUES` clause |

### 5) Fragile/High-Churn Areas

| Area | Why fragile | Safe change strategy |
|---|---|---|
| `src/benchmark/models/__init__.py` | Every model addition requires editing both `_register()` and `get_registry()` | Single-source model list with dual generation |
| `src/benchmark/evaluation/pipeline.py` | 333 lines — ties embedding, evaluation, and pgvector; hard to test individual phases | Extract pgvector phase to separate method (already done: `_run_pgvector_pipeline`) |
| `src/benchmark/cli/benchmark.py` | 5 CLI commands in one file; shared imports and setup | Consider splitting per-command CLI files when >8 commands |

### 6) Caveats for Thesis Claims

- **R@K values**: Category-based ground truth produces ~30 relevant items per query in ~3,300 gallery. R@10 ≈ 0.06 is expected; maximum R@10 = 10/30 ≈ 0.33. Low R@K reflects coarse-grained relevance proxy, not model weakness.
- **pgvector recall**: IVFFlat is approximate. Recall@10 ≈ 0.65–0.72 means approximate search finds 65–72% of exact top-10. Expected for 100 lists on 3,300 vectors.
- **RAM measurement**: psutil-based RSS delta may report 0.0 or negative on some systems. Values not reliable for thesis claims without external verification.
- **Statistical power**: n=3 folds. Paired t-tests omitted. Descriptive statistics only.
- **Hardware dependency**: Latency and throughput are hardware-specific. Report exact hardware specs alongside results.

### 7) `[ASK USER]` Questions

1. [ASK USER] Should the `old/` directory (legacy benchmarks, old thesis code) be archived or removed entirely?
2. [ASK USER] Should `outputs/pipeline/` and `outputs/thesis/` committed results be moved to git-LFS or kept as-is for thesis record?
3. [ASK USER] Is model integrity verification (checksum/hash) needed for HuggingFace weight downloads?
4. [ASK USER] Should RAM measurement be verified externally before citing in thesis, given psutil unreliability on some systems?
5. [ASK USER] Is GPU memory management (model unloading between models) a current concern for your hardware?

### 8) Evidence

- `src/benchmark/retrieval/pgvector.py:L206` — IVFFlat dimension limit issue
- `src/benchmark/evaluation/pipeline.py:L235-245` — RAM measurement
- `src/benchmark/embeddings/cache.py:L27` — cache key formula
- `src/benchmark/evaluation/pipeline.py:L80` — sequential model loop
- `src/benchmark/retrieval/cosine.py:L68-70` — per-query retrieval loop
- `src/benchmark/evaluation/pipeline.py:L248-333` — graceful pgvector degradation
- `src/benchmark/datasets/ground_truth.py` — category-based ground truth
