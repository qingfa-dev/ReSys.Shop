# Benchmarks — Agent Guide

Fashion image retrieval benchmark for the CTU thesis. Python 3.12, 11 embedding models, 5 CLI commands. See `docs/` for deep dives and `.harness/` for machine-readable boundaries.

## Non-Negotiable Rules

1. **Absolute imports only** — all imports use `benchmark.` prefix. No relative imports.
2. **Pure metrics** — files in `metrics/` must contain zero I/O. Functions accept arrays, return scalars.
3. **TDD required** — new features need a test written first (RED), then implementation (GREEN), then commit.
4. **Ruff lint before commit** — `uv run ruff check src/` must pass. Rules: E, F, I, UP, B, SIM.
5. **Single model = one file** — add new models under `models/` with one adapter file each.

## Repository Map

- `docs/08-replication-guide.md` — step-by-step to reproduce all results (start here)
- `docs/09-benchmark-results.md` — consolidated 5K pipeline + thesis results
- `docs/06-thesis-protocol.md` — §11.5 academic evaluation protocol
- `docs/codebase/ARCHITECTURE.md` — 3 benchmark modes, layer flow, patterns
- `docs/codebase/STRUCTURE.md` — directory map, entry points, module boundaries
- `docs/codebase/STACK.md` — full dependency inventory + key commands
- `docs/codebase/CONVENTIONS.md` — naming, imports, logging, testing
- `docs/codebase/TESTING.md` — test layout, mocking strategy, known gaps
- `docs/codebase/CONCERNS.md` — risks, tech debt, thesis caveats
- `docs/codebase/DIRECTORY_MAP.md` — full folder map with priorities

## Verification

```bash
uv run ruff check src/              # Lint
uv run pytest --ignore=src/tests/integration/  # Unit tests (125+, fast)
uv run pytest                        # All tests (inc. integration — requires Docker/pgvector)
uv run pytest --cov=benchmark        # Coverage report
uv run benchmark --help              # CLI sanity
```

## CLI Commands

```bash
uv run benchmark run     --dataset-root PATH --models MODEL [OPTIONS]  # One-shot comparison
uv run benchmark thesis  --dataset-root PATH [OPTIONS]                 # 3-fold CV, in-memory
uv run benchmark pipeline --dataset-root PATH [OPTIONS]               # CV + pgvector production
uv run benchmark report  --format typst [OPTIONS]                      # Regenerate reports
uv run benchmark cache   list|stats|clear                               # Embedding cache mgmt
```

## Code Organization

- `src/benchmark/models/` — 11 model adapters (FashionCLIP, ResNet-50, CLIP variants, etc.)
- `src/benchmark/evaluation/` — BenchmarkRunner (one-shot), ThesisRunner (CV), PipelineRunner (pgvector)
- `src/benchmark/datasets/` — FashionDataset, GroundTruth (stratified folds)
- `src/benchmark/embeddings/` — EmbeddingGenerator + npz cache
- `src/benchmark/metrics/` — P@K, R@K, mAP, nDCG, latency, recall_comparison
- `src/benchmark/retrieval/` — Cosine (exact), FAISS, PGVector (batch ingestion, index, query)
- `src/benchmark/reporting/` — JSON, CSV, Markdown, Typst (thesis + pipeline), charts
- `src/benchmark/cli/` — Typer app with 5 commands
- `src/tests/` — mirrored test structure (~125 tests)
- `infra/` — PostgreSQL/pgvector init scripts

## Known Issues

- pgvector IVFFlat capped at 2000 dims — ResNet-50 (2048-d) cannot use IVFFlat index
- RAM measurement via psutil unreliable on some systems (reports 0.0 or negative)
- No model unload mechanism — GPU memory can accumulate across sequential model runs
- Cache key is `model_slug + dataset_name` — no content hash; use `--no-cache` after data changes
