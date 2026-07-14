# Project Comparison: `benchmarks` vs `ReSys.Research`

This document compares the root `benchmarks` project with the nested `ReSys.Research` project inside the same workspace. It is designed as a full, side-by-side comparison of scope, architecture, implementation, workflows, and use cases.

---

## 1. Summary

`benchmarks` is a benchmark package built to compare fashion image retrieval models in a repeatable, configurable, and reportable way. `ReSys.Research` is a research lab repository for thesis-driven experimentation, exploratory analysis, feature extraction, and validation scripts.

### At a glance

| Dimension | `benchmarks` | `ReSys.Research` |
|---|---|---|
| Primary goal | Repeatable benchmarking and report generation | Research experimentation and academic validation |
| Main interface | `typer` CLI + package API | Python scripts + notebooks |
| Core deliverables | structured metrics, charts, tables, JSON/Markdown reports | experiment results, feature exports, notebooks, DB ingestion, academic checks |
| Best fit | Model comparison, metric-driven evaluation | Hypothesis testing, thesis proof-of-concept, research exploration |

---

## 2. Project contents and layout

### `benchmarks`

- Root:
  - `README.md`
  - `pyproject.toml`
  - `.python-version`
- Source package: `src/benchmark`
- Configs: `configs/*.yaml`
- Experiment manifests: `experiments/*`
- Data: `data/raw`, `data/processed`, `data/cache`, `data/splits`
- Outputs: `outputs/metrics`, `outputs/reports`, `outputs/tables`, `outputs/figures`, `outputs/logs`
- Scripts: `scripts/*.py`

### `ReSys.Research`

- Root:
  - `README.md`
  - `pyproject.toml`
  - `docker-compose.yml`
  - `.gitignore`
- Research package: `ReSys.Research/src`
- External notebooks: `ReSys.Research/notebooks`
- Research workflows: `ReSys.Research/scripts/*.py`
- Data layers: `ReSys.Research/data/external`, `ReSys.Research/data/interim`, `ReSys.Research/data/processed`
- Reports and artifacts: `ReSys.Research/reports`
- DB: `ReSys.Research/db/schema.sql`

---

## 3. Architectures and pipelines

### `benchmarks` pipeline

1. `src/benchmark/datasets/loader.py` loads a JSON split file into `FashionDataset` and `Sample` objects.
2. `src/benchmark/cli/benchmark.py` parses command-line options and configures the run.
3. `src/benchmark/evaluation/benchmark.py` orchestrates each model through `EmbeddingGenerator` and `Evaluator`.
4. `src/benchmark/embeddings/generator.py` generates or loads cached embeddings.
5. `src/benchmark/retrieval/cosine.py` computes cosine nearest neighbours and returns top-K indices.
6. `src/benchmark/evaluation/evaluator.py` computes metrics and optionally efficiency measurements.
7. `src/benchmark/reporting/*.py` creates JSON, CSV, Markdown, Typst, and chart outputs.

### `ReSys.Research` pipeline

1. `ReSys.Research/scripts/step1_extract_features.py` extracts embeddings from models and writes feature files.
2. `ReSys.Research/scripts/step2_evaluate_splits.py` loads extracted features, applies dataset split logic, and calculates metrics.
3. `ReSys.Research/scripts/step3_generate_reports.py` generates final summary visualizations and report artifacts.
4. `ReSys.Research/scripts/step4_benchmark_pgvector.py` benchmarks database-backed vector search with PGVector.
5. `ReSys.Research/src/search/index.py` provides an in-memory research index for exact cosine retrieval, with placeholders for approximate methods.
6. `ReSys.Research/src/evaluation/metrics.py` computes precision, recall, mAP, and evaluation metrics for research experiments.

---

## 4. CLI and actual usage

### `benchmarks`

Primary CLI entrypoint: `src/benchmark/cli/main.py`

Example commands:

```bash
uv run benchmark benchmark --dataset-root data/raw/deepfashion \
  --split-file data/splits/deepfashion/test.json \
  --models fashion-clip,clip-b32 \
  --k 1,5,10,20

uv run benchmark report --format all --k 1,5,10,20
uv run benchmark cache clear
```

The `benchmark` command is productionized with:
- structured CLI options
- dataset validation via `src/benchmark/datasets/validators.py`
- automatic caching and output writing
- report regeneration without re-running inference

### `ReSys.Research`

Primary interaction is via scripts and notebooks rather than a packaged CLI.
Common usage patterns:

```bash
python ReSys.Research/scripts/step1_extract_features.py --model clip --limit 500
python ReSys.Research/scripts/step2_evaluate_splits.py --input output/features/clip_features.npz
python ReSys.Research/scripts/step3_generate_reports.py --input output/features/clip_features.npz
```

The research folder also includes `run_thesis_experiments.py` and `check_academic_alignment.py` for thesis validation.

---

## 5. Data ingestion and sample handling

### `benchmarks`

- Uses `FashionDataset` in `src/benchmark/datasets/loader.py`.
- Expected split JSON format: image path, label, product ID.
- `Sample` contains `image_path`, `label`, `product_id`, `split`.
- Supports `iter_images` to yield PIL images safely.

### `ReSys.Research`

- Uses `src/data/loader.py` with dataloaders, transforms, and CSV metadata.
- Supports different transform pipelines including CLIP-specific preprocessing.
- Data is frequently loaded from `.csv` files and feature `.npz` dumps.

---

## 6. Model registry, loading, and embedding extraction

### `benchmarks`

- Concrete model classes:
  - `src/benchmark/models/clip_b32.py`
  - `src/benchmark/models/clip_l14.py`
  - `src/benchmark/models/eva_clip.py`
  - `src/benchmark/models/fashion_clip.py`
  - `src/benchmark/models/siglip.py`
- Registry in `src/benchmark/models/registry.py` exposes keys like `fashion-clip`, `clip-b32`, `clip-l14`, `siglip`, `eva-clip`.
- `EmbeddingGenerator` in `src/benchmark/embeddings/generator.py` handles batched inference, caching, and alignment of embeddings to samples.

### `ReSys.Research`

- Embedding extraction is performed in `ReSys.Research/scripts/step1_extract_features.py`.
- Supports multiple model types, including EfficientNet and CLIP variants.
- `CLIPImageWrapper` accommodates different CLIP output shapes.
- Exports features into `.npz` files for later evaluation.

---

## 7. Retrieval and indexing

### `benchmarks`

- In-memory cosine search in `src/benchmark/retrieval/cosine.py`.
- `retrieve_batch` returns top-K indices for every query, with optional self-exclusion.
- Also includes `src/benchmark/retrieval/faiss.py` and `src/benchmark/retrieval/pgvector.py` for alternate backends.

### `ReSys.Research`

- `ReSys.Research/src/search/index.py` implements `ResearchIndex`.
- Uses normalized cosine similarity via dot product on numpy arrays.
- Provides `search(query, k)` and `filter_split(split_name)`.
- Designed for research experiments rather than a general retrieval service.

---

## 8. Evaluation metrics

### `benchmarks`

Metrics are modular and explicit:
- `src/benchmark/metrics/precision.py`: `precision_at_k`, `mean_precision_at_k`
- `src/benchmark/metrics/recall.py`: `recall_at_k`, `mean_recall_at_k`
- `src/benchmark/metrics/ndcg.py`: `ndcg_at_k`, `mean_ndcg_at_k`
- `src/benchmark/metrics/map.py`: `average_precision`, `mean_average_precision`
- `src/benchmark/metrics/latency.py`: latency measurement wrapper
- `src/benchmark/metrics/throughput.py`: throughput calculation

Evaluator behavior in `src/benchmark/evaluation/evaluator.py`:
- computes retrieval metrics for every K in `k_values`
- computes global `mAP` over full ranked lists
- optionally measures latency and throughput using real images
- writes per-model results into JSON via `ModelMetrics.to_dict`

### `ReSys.Research`

- Core evaluation is in `ReSys.Research/src/evaluation/metrics.py`.
- Computes:
  - `P@1`, `P@5`, `P@10`
  - `R@10`
  - `mAP@10`
- Uses query/gallery splits defined in metadata or a default 20/80 split.
- Uses a simplified evaluation flow tailored for the research dataset and thesis metrics.

---

## 9. Reporting and artifact generation

### `benchmarks`

Reporting is a first-class feature:
- CSV via `src/benchmark/reporting/csv.py`
- Markdown via `src/benchmark/reporting/markdown.py`
- Typst table generation via `src/benchmark/reporting/typst.py`
- Charts via `src/benchmark/reporting/charts.py`
- `benchmark report` command regenerates outputs from saved metric JSON files.

### `ReSys.Research`

Reporting is script-driven and research-focused:
- `step3_generate_reports.py` produces thesis-ready report artifacts
- `run_thesis_experiments.py` aggregates experimental results for academic summaries
- `check_academic_alignment.py` validates metric targets against thesis sections
- Notebooks are also used for figure generation and data exploration

---

## 10. File-level feature mapping

| `benchmarks` | Equivalent / related `ReSys.Research` |
|---|---|
| `src/benchmark/cli/main.py` | `ReSys.Research/scripts/run_thesis_experiments.py` / notebooks for execution flow |
| `src/benchmark/cli/benchmark.py` | `ReSys.Research/scripts/step1_extract_features.py` + `step2_evaluate_splits.py` |
| `src/benchmark/cli/report.py` | `ReSys.Research/scripts/step3_generate_reports.py` |
| `src/benchmark/datasets/loader.py` | `ReSys.Research/src/data/loader.py` |
| `src/benchmark/embeddings/generator.py` | `ReSys.Research/scripts/step1_extract_features.py` |
| `src/benchmark/evaluation/benchmark.py` | `ReSys.Research/src/evaluation/run_experiment.py` |
| `src/benchmark/retrieval/cosine.py` | `ReSys.Research/src/search/index.py` |
| `src/benchmark/evaluation/evaluator.py` | `ReSys.Research/src/evaluation/metrics.py` |
| `src/benchmark/reporting/*` | `ReSys.Research/scripts/step3_generate_reports.py` + notebooks |
| `configs/benchmark.yaml` | `ReSys.Research/db/schema.sql` and research config patterns |

---

## 11. Dependency and runtime comparison

### `benchmarks`

- Strongly typed package with CLI and reusable benchmark code.
- Uses `pydantic`/`PyYAML` for config and dataset validation.
- Includes retrieval backends for production-like benchmarking: `faiss-cpu`, `pgvector`.
- Embedding caching and output directories are standardized.

### `ReSys.Research`

- Focused on research tooling and prototyping.
- Mixes notebook-driven workflows with scripts.
- Uses ONNX export tooling (`onnx`, `onnxruntime`, `onnxsim`) for model deployment experiments.
- Includes script support for database ingestion and sandbox benchmarking.

---

## 12. Detailed metric comparison

### `benchmarks`
- `P@K` and `R@K` are computed for arbitrary K values in `k_values`.
- `mAP` is computed over the full ranked list and stored as `map_score`.
- `Validator` computes results using `retrieve_batch` with a full gallery search.

### `ReSys.Research`
- `evaluate_retrieval` computes `mAP@10` specifically, plus `P@1`, `P@5`, `P@10`, `R@10`.
- The research evaluation assumes dataset splits and label columns from metadata.
- Its `calculate_average_precision` uses the same AP principle, while `calculate_precision_at_k` and `calculate_recall_at_k` operate on the top K predictions.

---

## 13. Strengths and ideal use cases

### `benchmarks`
- Ideal for benchmark engineers who want a repeatable scoring pipeline.
- Best when you need standardized model comparison across many models and K cutoffs.
- Good for generating polished deliverables: charts, tables, JSON summaries, reports.

### `ReSys.Research`
- Ideal for researchers who are exploring new model behavior or thesis hypotheses.
- Best when you need quick experimental scripts, split handling, and custom academic checks.
- Good for prototyping ONNX export, PGVector benchmarking, and research notebook workflows.

---

## 14. Recommended next steps

- If your goal is **benchmarking**, use the `benchmarks` package and run `uv run benchmark benchmark ...`.
- If your goal is **research exploration**, use `ReSys.Research` scripts and notebooks.
- If you want both, keep `benchmarks` as the stable benchmark runner and use `ReSys.Research` for hypothesis validation and feature extraction.

---

## 15. Location reference

- `benchmarks` root: `/home/qingfa/Downloads/benchmarks`
- `ReSys.Research` root: `/home/qingfa/Downloads/benchmarks/ReSys.Research`
