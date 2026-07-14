# benchmarks/ Directory Map

Complete guide to every folder, subfolder, and file in the benchmarks package.

---

## Top-Level Files

| file | purpose | priority |
|---|---|---|
| `pyproject.toml` | uv project config, dependencies, scripts, pytest/ruff settings | P0 — entry point |
| `README.md` | quickstart, model list, CLI commands | P0 — onboarding |
| `uv.lock` | locked dependency versions for reproducibility | P0 — reproducibility |
| `.python-version` | pins Python 3.12 | P1 — version control |
| `.gitignore` | excludes `.venv/`, `data/cache/`, `outputs*/`, `__pycache__/` | P1 — hygiene |
| `PROJECT_COMPARISON.md` | compares this benchmark to academic baselines | P2 — reference |

---

## Top-Level Folders

### `src/` — Source Code (P0)

All Python source code. Two sub-packages:

#### `src/benchmark/` — Main Package (P0)

| subfolder | files | purpose |
|---|---|---|
| `cli/` | `main.py`, `benchmark.py` | Typer CLI entry points. `main.py` composes sub-commands. `benchmark.py` has `run`, `thesis`, `report`, `cache`. |
| `datasets/` | `loader.py`, `ground_truth.py`, `transforms.py`, `validators.py` | Dataset loading, ground-truth building, image transforms, dataset validation. |
| `embeddings/` | `generator.py`, `cache.py`, `storage.py` | Embedding generation, `.npz` cache, storage helpers. |
| `evaluation/` | `evaluator.py`, `benchmark.py`, `thesis.py`, `stats.py`, `comparison.py` | Evaluation orchestration: one-shot (`benchmark.py`), thesis CV (`thesis.py`), metrics computation (`evaluator.py`), statistical analysis (`stats.py`), model ranking (`comparison.py`). |
| `metrics/` | `precision.py`, `recall.py`, `map.py`, `ndcg.py`, `latency.py`, `throughput.py` | Pure metric implementations. All accept numpy arrays, return floats. |
| `models/` | `base.py`, `registry.py`, `fashion_clip.py`, `clip_b32.py`, `clip_l14.py`, `clip_vit_b16.py`, `clip_generic.py`, `siglip.py`, `eva_clip.py`, `efficientnet_b0.py`, `resnet50.py`, `convnext_tiny.py`, `dinov2_vits14.py` | Model adapters. `base.py` = abstract class. `registry.py` = lazy registry. Each other file = one model adapter. |
| `reporting/` | `json.py`, `csv.py`, `markdown.py`, `typst.py`, `charts.py` | Output generators. `typst.py` writes `.typ` files auto-included in thesis. |
| `retrieval/` | `cosine.py`, `faiss.py`, `pgvector.py` | Retrieval backends: cosine similarity (numpy), FAISS (ANN), PGVector (postgres). |
| `utils/` | `logging.py`, `timing.py`, `device.py`, `random_seed.py` | Cross-cutting: structured logging, latency measurement, device detection, seed setting. |

#### `src/tests/` — Test Suite (P0)

Mirrors `src/benchmark/` structure:

| subfolder | what it tests |
|---|---|
| `datasets/` | `loader.py`, `ground_truth.py` |
| `evaluation/` | `stats.py`, `thesis.py` |
| `integration/` | `pgvector.py` (requires Docker) |
| `metrics/` | `precision.py`, `recall.py`, `map.py`, `ndcg.py` |
| `models/` | `base.py`, `registry.py`, all model adapters |
| `reporting/` | `json.py`, `csv.py`, `markdown.py`, `typst.py` |
| `utils/` | `timing.py` |

---

### `docs/` — Documentation (P1)

| file | purpose |
|---|---|
| `README.md` | docs index |
| `01-overview.md` | project overview, goals |
| `02-models.md` | model descriptions, embedding dims |
| `03-metrics.md` | metric formulas, definitions |
| `04-pipeline.md` | data flow, pipeline stages |
| `05-datasets.md` | dataset descriptions, formats |
| `06-thesis-protocol.md` | §11.5 thesis evaluation protocol |
| `07-references.md` | academic references |
| `08-replication-guide.md` | step-by-step replication guide (thesis + pipeline + pgvector) |
| `09-benchmark-results.md` | consolidated benchmark results — 5K pipeline + 300-image thesis demo |
| `codebase/ARCHITECTURE.md` | architecture decisions |
| `codebase/CONCERNS.md` | tech debt, risks |
| `codebase/CONVENTIONS.md` | coding conventions |
| `codebase/INTEGRATIONS.md` | external integrations |
| `codebase/STACK.md` | tech stack versions |
| `codebase/STRUCTURE.md` | codebase organization |
| `codebase/TESTING.md` | testing strategy |

---

### `scripts/` — Helper Scripts (P1)

| file | purpose |
|---|---|
| `download_dataset.py` | downloads fashion datasets |
| `prepare_fashion_product.py` | preprocesses fashion-product-images |
| `preprocess.py` | general preprocessing |
| `benchmark.py` | standalone benchmark script |
| `report.py` | standalone report generator |
| `clean.py` | cleans outputs / cache |
| `verify_extraction.py` | verifies feature extraction |

---

### `configs/` — Configuration (P1)

| file | purpose |
|---|---|
| `benchmark.yaml` | benchmark run config |
| `datasets.yaml` | dataset paths, splits |
| `hardware.yaml` | hardware settings |
| `metrics.yaml` | metric parameters |
| `models/*.yaml` | per-model configs (5 files) |

---

### `data/` — Data Directory (P1, gitignored contents)

| subfolder | purpose |
|---|---|
| `raw/` | raw downloaded datasets |
| `processed/` | preprocessed data |
| `splits/` | train/test split JSONs |
| `cache/` | embedding `.npz` cache |
| `metadata/` | dataset metadata |

---

### `outputs/` — Run Outputs (P2, generated)

Default output directory. Created at runtime:

```
outputs/
├── metrics/     # per-model JSON results
├── reports/     # CSV, Markdown summaries
├── tables/      # Typst `.typ` files
├── figures/     # PNG charts
└── logs/        # execution logs
```

Historical outputs (committed for reference):
- `outputs_5k/` — run on 5K images
- `outputs_5k_split/` — run with train/test split

---

### `experiments/` — Ad-Hoc Experiments (P3)

| subfolder | purpose |
|---|---|
| `fashion_clip/` | Fashion-CLIP specific experiments |
| `clip_b32/` | CLIP-B32 experiments |
| `clip_l14/` | CLIP-L14 experiments |
| `eva_clip/` | EVA-CLIP experiments |
| `siglip/` | SigLIP experiments |
| `comparison/` | cross-model comparison notebooks |

---

### `infra/` — Infrastructure (P3)

| subfolder | purpose |
|---|---|
| `docker/` | Docker configs for containerized runs |
| `postgres/` | PostgreSQL / PGVector setup |

---

### `old/` — Legacy Code (P3)

| subfolder | purpose |
|---|---|
| `_thesis/` | old thesis-related code |
| `benchmarks.v001/` | first benchmark iteration |
| `ReSys.ML/` | old ML module |
| `ReSys.Research/` | old research module |

---

## Priority Legend

| level | meaning |
|---|---|
| **P0** | required for benchmark to function. changes here affect core behavior. |
| **P1** | supporting infrastructure. important but replaceable. |
| **P2** | generated outputs / reference material. not source code. |
| **P3** | historical / experimental. read-only or ad-hoc. |

---

## Quick Navigation

| want to... | go to... |
|---|---|
| add a model | `src/benchmark/models/` + `src/tests/models/` |
| add a metric | `src/benchmark/metrics/` + `src/tests/metrics/` |
| change CLI | `src/benchmark/cli/benchmark.py` |
| change thesis protocol | `src/benchmark/evaluation/thesis.py` |
| change output format | `src/benchmark/reporting/` |
| change dataset loading | `src/benchmark/datasets/loader.py` |
| run tests | `uv run pytest src/tests` |
| run benchmark | `uv run benchmark run --models all` |
| run thesis | `uv run benchmark thesis --folds 3` |
