# Production Pipeline Benchmark Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Extend the thesis benchmark with a production-pipeline mode that simulates the full workflow from embedding generation through pgvector ingestion, index building, approximate querying, and evaluation — producing all metrics required for the thesis report.

**Architecture:** Create a `PipelineRunner` class that reuses the thesis fold generation and embedding generation, then adds a production phase: batch-insert embeddings into PostgreSQL/pgvector, build IVFFlat/HNSW indexes, measure index build time, run queries via pgvector, compare approximate vs exact recall, and record query latency distributions. All phases emit structured logs; results feed into Typst tables and a JSON trace.

**Tech Stack:** Python 3.12, PostgreSQL 16 + pgvector, psycopg, numpy, pandas, typer. Reuses existing `ThesisRunner`, `EmbeddingGenerator`, `PgvectorRetriever`, `FashionDataset`, metrics modules.

## Global Constraints

- Python ≥ 3.12
- PostgreSQL 16 with pgvector extension
- `psycopg[binary]>=3.1` and `pgvector>=0.2` already in `pyproject.toml`
- Warnings-as-errors enabled (`TreatWarningsAsErrors=true` in parent repo)
- All new code needs tests
- Results must be JSON-serializable for trace output
- Typst tables must compile without errors
- No placeholders — every step has exact code or commands

---

## File Map

| file | responsibility |
|---|---|
| `src/benchmark/retrieval/pgvector.py` | Extend with batch ingestion `upsert_batch()`, index build helpers, timing wrappers |
| `src/benchmark/evaluation/pipeline.py` | `PipelineRunner` — orchestrates full production workflow per fold |
| `src/benchmark/metrics/recall_comparison.py` | Compute recall@K of approximate vs exact search |
| `src/benchmark/reporting/pipeline.py` | `write_pipeline_typst()` and `write_pipeline_json()` — production metrics tables |
| `src/benchmark/cli/benchmark.py` | Add `pipeline` subcommand |
| `src/tests/evaluation/test_pipeline.py` | Integration tests for PipelineRunner (mocked DB) |
| `src/tests/metrics/test_recall_comparison.py` | Unit tests for recall comparison |
| `src/tests/reporting/test_pipeline_reporting.py` | Tests for pipeline report generation |

---

### Task 1: Extend PgvectorRetriever with Batch Ingestion & Indexing

**Files:**
- Modify: `src/benchmark/retrieval/pgvector.py`
- Test: `src/tests/retrieval/test_pgvector_extended.py`

**Interfaces:**
- Consumes: `numpy.ndarray` embeddings, `list[str]` product_ids/labels
- Produces: `upsert_batch(product_ids, labels, embeddings) → None`, `build_index(dim, lists) → float` (returns build time seconds), `clear_table() → None`

- [ ] **Step 1: Write failing test for batch ingestion**

```python
import numpy as np
import pytest
from unittest.mock import MagicMock, patch

from benchmark.retrieval.pgvector import PgvectorRetriever


def test_upsert_batch():
    retriever = PgvectorRetriever(conn_string="postgresql://test@test/test")
    retriever._conn = MagicMock()
    retriever._conn.cursor.return_value.__enter__ = lambda s: s
    retriever._conn.cursor.return_value.__exit__ = lambda *a: None
    cur = retriever._conn.cursor.return_value

    ids = ["1", "2", "3"]
    labels = ["shirt", "jeans", "shoes"]
    embeddings = np.random.rand(3, 512).astype(np.float32)

    retriever.upsert_batch(ids, labels, embeddings)
    assert cur.execute.call_count == 1
    sql = cur.execute.call_args[0][0]
    assert "INSERT INTO" in sql
    assert "ON CONFLICT" in sql
```

- [ ] **Step 2: Run test to verify it fails**

```bash
cd /home/qingfa/Repos/ReSys.Shop/benchmarks
uv run pytest src/tests/retrieval/test_pgvector_extended.py::test_upsert_batch -v
```
Expected: FAIL — `AttributeError: 'PgvectorRetriever' object has no attribute 'upsert_batch'`

- [ ] **Step 3: Implement batch ingestion and index build**

Add to `src/benchmark/retrieval/pgvector.py`, after `upsert_embedding`:

```python
    def upsert_batch(
        self,
        product_ids: list[str],
        labels: list[str],
        embeddings: np.ndarray,
    ) -> None:
        """Batch insert or update product embeddings.

        Args:
            product_ids: List of product IDs.
            labels:      List of labels (same length).
            embeddings:  Float32 array of shape ``(N, D)``.
        """
        if self._conn is None:
            raise RuntimeError("Call connect() first")
        if len(product_ids) != len(labels) or len(product_ids) != len(embeddings):
            raise ValueError("product_ids, labels, and embeddings must have the same length")

        sql = f"""
            INSERT INTO {self._table} ({self._id_col}, {self._label_col}, {self._embedding_col})
            VALUES %s
            ON CONFLICT ({self._id_col}) DO UPDATE
                SET {self._embedding_col} = EXCLUDED.{self._embedding_col},
                    {self._label_col}     = EXCLUDED.{self._label_col}
        """
        values = [
            (pid, label, emb.tolist())
            for pid, label, emb in zip(product_ids, labels, embeddings)
        ]
        with self._conn.cursor() as cur:
            from psycopg.sql import Literal
            # Use execute_values for batch insert
            args_str = ",".join(
                cur.mogrify("(%s, %s, %s::vector)", v).decode("utf-8")
                for v in values
            )
            full_sql = sql.replace("VALUES %s", f"VALUES {args_str}")
            cur.execute(full_sql)

    def clear_table(self) -> None:
        """Delete all rows from the target table."""
        if self._conn is None:
            raise RuntimeError("Call connect() first")
        with self._conn.cursor() as cur:
            cur.execute(f"DELETE FROM {self._table}")
        logger.info("Cleared table %s", self._table)

    def build_index(self, dim: int, lists: int = 100) -> float:
        """Build an IVFFlat index and return elapsed time in seconds.

        Args:
            dim:    Embedding dimension (512 or 768).
            lists:  Number of IVF lists.

        Returns:
            Index build time in seconds.
        """
        if self._conn is None:
            raise RuntimeError("Call connect() first")

        index_name = f"idx_{self._table}_{dim}_{lists}"
        with self._conn.cursor() as cur:
            cur.execute(
                f"""
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM pg_indexes
                        WHERE indexname = '{index_name}'
                    ) THEN
                        DROP INDEX {index_name};
                    END IF;
                END $$;
                """
            )

        t0 = time.perf_counter()
        with self._conn.cursor() as cur:
            cur.execute(
                f"""
                CREATE INDEX {index_name}
                ON {self._table}
                USING ivfflat ({self._embedding_col} vector_cosine_ops)
                WITH (lists = {lists});
                """
            )
        elapsed = time.perf_counter() - t0
        logger.info("Built IVFFlat index %s in %.2f s", index_name, elapsed)
        return elapsed
```

Also add `import time` at the top of the file.

- [ ] **Step 4: Run test to verify it passes**

```bash
uv run pytest src/tests/retrieval/test_pgvector_extended.py::test_upsert_batch -v
```
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add src/benchmark/retrieval/pgvector.py src/tests/retrieval/test_pgvector_extended.py
git commit -m "feat(pgvector): add batch ingestion, table clear, and index build timing"
```

---

### Task 2: Create Recall Comparison Metric

**Files:**
- Create: `src/benchmark/metrics/recall_comparison.py`
- Test: `src/tests/metrics/test_recall_comparison.py`

**Interfaces:**
- Consumes: `approx_indices: np.ndarray (Q, K)`, `exact_indices: np.ndarray (Q, K)`, `k_values: list[int]`
- Produces: `dict[int, float]` — recall@K for each k

- [ ] **Step 1: Write failing test**

```python
import numpy as np

from benchmark.metrics.recall_comparison import approximate_recall_at_k


def test_perfect_recall():
    exact = np.array([[0, 1, 2], [3, 4, 5]])
    approx = np.array([[0, 1, 2], [3, 4, 5]])
    result = approximate_recall_at_k(approx, exact, k_values=[1, 2, 3])
    assert result[1] == 1.0
    assert result[2] == 1.0
    assert result[3] == 1.0


def test_zero_recall():
    exact = np.array([[0, 1, 2], [3, 4, 5]])
    approx = np.array([[6, 7, 8], [9, 10, 11]])
    result = approximate_recall_at_k(approx, exact, k_values=[1, 2, 3])
    assert result[1] == 0.0
    assert result[2] == 0.0
    assert result[3] == 0.0


def test_partial_recall():
    exact = np.array([[0, 1, 2], [3, 4, 5]])
    approx = np.array([[0, 7, 8], [9, 4, 5]])
    result = approximate_recall_at_k(approx, exact, k_values=[1, 2, 3])
    assert result[1] == 0.5  # one out of two correct @1
    assert result[2] == 0.5  # still one correct @2
    assert result[3] == 1.0  # both correct @3
```

- [ ] **Step 2: Run to verify failure**

```bash
uv run pytest src/tests/metrics/test_recall_comparison.py -v
```
Expected: FAIL — `ModuleNotFoundError: No module named 'benchmark.metrics.recall_comparison'`

- [ ] **Step 3: Implement recall comparison**

```python
"""Recall@K comparison between approximate and exact retrieval."""
from __future__ import annotations

import numpy as np


def approximate_recall_at_k(
    approx_indices: np.ndarray,
    exact_indices: np.ndarray,
    k_values: list[int],
) -> dict[int, float]:
    """Compute recall@K of approximate vs exact nearest-neighbour search.

    Recall@K = |approx_top_K ∩ exact_top_K| / K

    Args:
        approx_indices: 2-D int array of shape ``(Q, K)`` from approximate search.
        exact_indices:  2-D int array of shape ``(Q, K)`` from exact search.
        k_values:       List of K values to evaluate.

    Returns:
        Dict mapping K → mean recall across all queries.
    """
    if approx_indices.shape != exact_indices.shape:
        raise ValueError(
            f"Shape mismatch: approx {approx_indices.shape} vs exact {exact_indices.shape}"
        )

    q = len(approx_indices)
    results: dict[int, float] = {}
    for k in k_values:
        if k <= 0:
            results[k] = 0.0
            continue
        k_cap = min(k, approx_indices.shape[1])
        recalls = []
        for i in range(q):
            approx_set = set(approx_indices[i, :k_cap])
            exact_set = set(exact_indices[i, :k_cap])
            if exact_set:
                recalls.append(len(approx_set & exact_set) / k_cap)
            else:
                recalls.append(1.0)
        results[k] = float(np.mean(recalls)) if recalls else 0.0
    return results
```

- [ ] **Step 4: Run test to verify pass**

```bash
uv run pytest src/tests/metrics/test_recall_comparison.py -v
```
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add src/benchmark/metrics/recall_comparison.py src/tests/metrics/test_recall_comparison.py
git commit -m "feat(metrics): add approximate recall@K vs exact search"
```

---

### Task 3: Create PipelineRunner

**Files:**
- Create: `src/benchmark/evaluation/pipeline.py`
- Test: `src/tests/evaluation/test_pipeline.py`

**Interfaces:**
- Consumes: `ThesisRunner` splits, `EmbeddingResult`, `PgvectorRetriever`, `recall_comparison.approximate_recall_at_k`
- Produces: `PipelineResult` dataclass → JSON-serializable dict with keys: `model_name`, `model_slug`, `folds`, `aggregate`, `production_metrics`

- [ ] **Step 1: Write failing test**

```python
import json
from pathlib import Path
from unittest.mock import MagicMock, patch

import numpy as np
import pytest

from benchmark.evaluation.pipeline import PipelineRunner


def test_pipeline_runner_runs_all_folds(tmp_path: Path):
    """Integration-style test with mocked DB to avoid heavy torch + postgres."""
    # Create dummy dataset root with styles.csv
    dataset_root = tmp_path / "dataset"
    images = dataset_root / "images"
    images.mkdir(parents=True)
    from PIL import Image
    for i in range(20):
        img = Image.new("RGB", (32, 32), color=(i, i, i))
        img.save(images / f"{i}.jpg")
    import pandas as pd
    df = pd.DataFrame({
        "id": [str(i) for i in range(20)],
        "masterCategory": ["A"] * 10 + ["B"] * 10,
        "subCategory": ["X"] * 20,
        "articleType": ["Shirt"] * 20,
        "image": [f"{i}.jpg" for i in range(20)],
    })
    df.to_csv(dataset_root / "styles.csv", index=False)

    fake_model = MagicMock()
    fake_model.name = "FakeModel"
    fake_model.slug = "fake-model"
    fake_model.load = MagicMock()
    fake_model.embed_batch = MagicMock(return_value=np.random.rand(2, 64).astype(np.float32))
    fake_model.embed = MagicMock(return_value=np.random.rand(64).astype(np.float32))
    fake_model.embedding_dim = 64

    with patch("benchmark.evaluation.pipeline.get_registry") as mock_registry, \
         patch("benchmark.evaluation.pipeline.PgvectorRetriever") as mock_pg:
        mock_registry.return_value = {"fake-model": fake_model}
        mock_pg_instance = MagicMock()
        mock_pg_instance.connect = MagicMock()
        mock_pg_instance.close = MagicMock()
        mock_pg_instance.upsert_batch = MagicMock()
        mock_pg_instance.clear_table = MagicMock()
        mock_pg_instance.build_index = MagicMock(return_value=0.5)
        mock_pg_instance.query = MagicMock(return_value=[
            {"id": "1", "label": "Shirt", "score": 0.9},
            {"id": "2", "label": "Shirt", "score": 0.8},
        ])
        mock_pg.return_value = mock_pg_instance

        runner = PipelineRunner(
            dataset_root=dataset_root,
            output_dir=tmp_path / "outputs",
            folds=3,
            seed=42,
            device="cpu",
            use_cache=False,
            batch_size=2,
            conn_string="postgresql://fake",
        )
        results = runner.run(model_keys=["fake-model"])

    assert len(results) == 1
    result = results[0]
    assert result["model_name"] == "FakeModel"
    assert len(result["folds"]) == 3
    assert "production_metrics" in result
    assert "index_build_time_s" in result["production_metrics"]
```

- [ ] **Step 2: Run to verify failure**

```bash
uv run pytest src/tests/evaluation/test_pipeline.py::test_pipeline_runner_runs_all_folds -v
```
Expected: FAIL — `ModuleNotFoundError: No module named 'benchmark.evaluation.pipeline'`

- [ ] **Step 3: Implement PipelineRunner**

```python
"""PipelineRunner — full production pipeline: embedding → pgvector → query → evaluate."""
from __future__ import annotations

import json
import time
from dataclasses import dataclass, field
from pathlib import Path
from typing import Any

import numpy as np
import pandas as pd

from benchmark.datasets.ground_truth import GroundTruth
from benchmark.datasets.loader import FashionDataset
from benchmark.embeddings.generator import EmbeddingGenerator
from benchmark.evaluation.evaluator import Evaluator
from benchmark.evaluation.stats import aggregate_mean_std, bootstrap_ci
from benchmark.metrics.latency import measure_latency
from benchmark.metrics.recall_comparison import approximate_recall_at_k
from benchmark.metrics.throughput import measure_throughput
from benchmark.models import get_registry
from benchmark.retrieval.cosine import retrieve_batch
from benchmark.retrieval.pgvector import PgvectorRetriever
from benchmark.utils.logging import get_logger

logger = get_logger("evaluation.pipeline")

THESIS_MODEL_KEYS = ["fashion-clip", "resnet-50", "efficientnet-b0", "clip-generic"]


@dataclass
class PipelineResult:
    """Complete production pipeline results for one model."""

    model_name: str
    model_slug: str
    folds: list[dict[str, Any]] = field(default_factory=list)
    aggregate: dict[str, dict[str, float]] = field(default_factory=dict)
    production_metrics: dict[str, Any] = field(default_factory=dict)


class PipelineRunner:
    """Run the full production pipeline: thesis CV + pgvector benchmarking."""

    def __init__(
        self,
        dataset_root: Path,
        output_dir: Path = Path("outputs/pipeline"),
        k_values: list[int] | None = None,
        folds: int = 3,
        seed: int = 42,
        device: str = "auto",
        use_cache: bool = True,
        batch_size: int = 64,
        conn_string: str = "postgresql://benchmark:benchmark@localhost:5432/benchmark",
        pg_lists: int = 100,
    ) -> None:
        self.dataset_root = dataset_root
        self.output_dir = output_dir
        self.k_values = k_values or [5, 10, 20]
        self.folds = folds
        self.seed = seed
        self.device = device
        self.use_cache = use_cache
        self.batch_size = batch_size
        self.conn_string = conn_string
        self.pg_lists = pg_lists
        self._registry = get_registry(device=device)

    def run(self, model_keys: list[str] | None = None) -> list[dict[str, Any]]:
        """Run the full production pipeline.

        Returns:
            List of result dicts (one per model), JSON-serializable.
        """
        keys = model_keys or THESIS_MODEL_KEYS
        logger.info("Starting pipeline benchmark: %d models, %d folds", len(keys), self.folds)

        styles_csv = self.dataset_root / "styles.csv"
        if not styles_csv.exists():
            raise FileNotFoundError(f"styles.csv not found: {styles_csv}")

        df = pd.read_csv(styles_csv)
        gt = GroundTruth(df, min_category_freq=10)
        splits = gt.generate_splits(
            n_splits=self.folds,
            seed=self.seed,
            output_dir=self.output_dir / "splits",
        )

        results: list[dict[str, Any]] = []
        for key in keys:
            if key not in self._registry:
                logger.error("Model %s not in registry, skipping", key)
                continue
            model = self._registry[key]
            model_result = self._evaluate_model(model, splits)
            results.append(model_result)

        return results

    def _evaluate_model(self, model, splits: list[tuple[Path, Path]]) -> dict[str, Any]:
        """Evaluate one model across all folds with production pipeline."""
        logger.info("Evaluating %s …", model.name)

        fold_results: list[dict[str, Any]] = []
        prod_metrics_per_fold: list[dict[str, Any]] = []

        t0 = time.perf_counter()
        model.load()
        load_time_ms = (time.perf_counter() - t0) * 1000.0

        for fold_idx, (train_path, test_path) in enumerate(splits):
            logger.info("  Fold %d …", fold_idx)
            fold_result, prod_metrics = self._evaluate_fold(
                model, train_path, test_path, fold_idx, load_time_ms
            )
            fold_results.append(fold_result)
            prod_metrics_per_fold.append(prod_metrics)

        # Aggregate thesis metrics
        aggregate: dict[str, dict[str, float]] = {}
        metric_keys = ["map", "precision@5", "precision@10", "precision@20",
                       "recall@5", "recall@10", "recall@20",
                       "latency_mean_ms", "throughput_per_sec",
                       "load_time_ms", "index_storage_mb", "ram_mb"]
        for mk in metric_keys:
            vals = [f[mk] for f in fold_results if mk in f]
            if vals:
                aggregate[mk] = aggregate_mean_std(vals)

        # Aggregate production metrics
        prod_aggregate: dict[str, dict[str, float]] = {}
        prod_keys = ["index_build_time_s", "pgvector_query_latency_ms",
                     "pgvector_recall@5", "pgvector_recall@10", "pgvector_recall@20",
                     "ingestion_time_s"]
        for pk in prod_keys:
            vals = [p[pk] for p in prod_metrics_per_fold if pk in p]
            if vals:
                prod_aggregate[pk] = aggregate_mean_std(vals)

        return {
            "model_name": model.name,
            "model_slug": model.slug,
            "folds": fold_results,
            "aggregate": aggregate,
            "production_metrics": prod_aggregate,
        }

    def _evaluate_fold(
        self, model, train_path: Path, test_path: Path, fold_idx: int, load_time_ms: float
    ) -> tuple[dict[str, Any], dict[str, Any]]:
        """Evaluate one fold: exact cosine + pgvector pipeline."""
        query_ds = FashionDataset(
            dataset_root=self.dataset_root, split_file=test_path, split="test"
        )
        query_ds.load()
        gallery_ds = FashionDataset(
            dataset_root=self.dataset_root, split_file=train_path, split="train"
        )
        gallery_ds.load()

        # Generate embeddings
        query_gen = EmbeddingGenerator(
            model=model, dataset=query_ds,
            batch_size=self.batch_size, use_cache=self.use_cache,
        )
        gallery_gen = EmbeddingGenerator(
            model=model, dataset=gallery_ds,
            batch_size=self.batch_size, use_cache=self.use_cache,
        )
        query_result = query_gen.generate(dataset_name=f"fold_{fold_idx}_test")
        gallery_result = gallery_gen.generate(dataset_name=f"fold_{fold_idx}_train")

        # Exact cosine evaluation (baseline)
        evaluator = Evaluator(
            dataset=query_ds, k_values=self.k_values, measure_efficiency=False
        )
        metrics = evaluator.evaluate_split(
            query_result=query_result,
            gallery_result=gallery_result,
            dataset_name=f"fold_{fold_idx}",
        )

        # Production pipeline: pgvector
        prod_metrics = self._run_pgvector_pipeline(
            model, gallery_result, query_result, gallery_ds, query_ds
        )

        # Efficiency metrics
        from PIL import Image
        sample_images = []
        for s in query_ds.samples[:200]:
            try:
                sample_images.append(Image.open(s.image_path).convert("RGB"))
            except OSError:
                pass

        latency_stats = measure_latency(model, sample_images, warmup_runs=10, benchmark_runs=100)
        throughput = measure_throughput(model, sample_images[:64], batch_size=64, num_batches=10)

        import psutil, gc
        process = psutil.Process()
        gc.collect()
        baseline = process.memory_info().rss
        model.embed_batch(sample_images[:64])
        peak = process.memory_info().rss
        ram_mb = (peak - baseline) / (1024 * 1024)

        total_storage_mb = query_result.embeddings.nbytes / (1024 * 1024)

        fold_result = {
            "fold": fold_idx,
            "map": round(metrics.map_score, 4),
            "precision@5": round(metrics.precision.get(5, 0.0), 4),
            "precision@10": round(metrics.precision.get(10, 0.0), 4),
            "precision@20": round(metrics.precision.get(20, 0.0), 4),
            "recall@5": round(metrics.recall.get(5, 0.0), 4),
            "recall@10": round(metrics.recall.get(10, 0.0), 4),
            "recall@20": round(metrics.recall.get(20, 0.0), 4),
            "latency_mean_ms": round(latency_stats.mean, 2),
            "latency_std_ms": round(latency_stats.std, 2),
            "throughput_per_sec": round(throughput, 2),
            "load_time_ms": round(load_time_ms, 2),
            "index_storage_mb": round(total_storage_mb, 2),
            "ram_mb": round(ram_mb, 2),
        }
        return fold_result, prod_metrics

    def _run_pgvector_pipeline(
        self, model, gallery_result, query_result, gallery_ds, query_ds
    ) -> dict[str, Any]:
        """Ingest into pgvector, build index, query, measure recall + latency."""
        retriever = PgvectorRetriever(
            conn_string=self.conn_string,
            table="products",
            embedding_col="embedding",
            id_col="id",
            label_col="label",
        )
        try:
            retriever.connect()
        except Exception as exc:
            logger.warning("PGVector not available: %s", exc)
            return {
                "index_build_time_s": 0.0,
                "pgvector_query_latency_ms": 0.0,
                "pgvector_recall@5": 0.0,
                "pgvector_recall@10": 0.0,
                "pgvector_recall@20": 0.0,
                "ingestion_time_s": 0.0,
                "error": str(exc),
            }

        retriever.clear_table()

        # Batch ingestion
        gallery_ids = [s.product_id for s in gallery_ds.samples]
        gallery_labels = [getattr(s, "label", "unknown") for s in gallery_ds.samples]
        t0 = time.perf_counter()
        retriever.upsert_batch(gallery_ids, gallery_labels, gallery_result.embeddings)
        ingestion_time = time.perf_counter() - t0

        # Build index
        index_time = retriever.build_index(dim=model.embedding_dim, lists=self.pg_lists)

        # Query via pgvector
        pgvector_results = []
        query_latencies = []
        for emb in query_result.embeddings:
            t0 = time.perf_counter()
            results = retriever.query(emb, top_k=max(self.k_values))
            query_latencies.append((time.perf_counter() - t0) * 1000.0)
            pgvector_results.append([r["id"] for r in results])

        pgvector_indices = np.array(pgvector_results, dtype=np.int64)

        # Exact cosine search for comparison
        exact_indices = retrieve_batch(
            query_result.embeddings, gallery_result.embeddings,
            k=max(self.k_values), exclude_self=False
        )

        # Map gallery IDs to indices for recall comparison
        id_to_idx = {pid: i for i, pid in enumerate(gallery_ids)}
        pgvector_mapped = np.full_like(exact_indices, -1)
        for i, row in enumerate(pgvector_indices):
            for j, pid in enumerate(row):
                if j < pgvector_mapped.shape[1]:
                    pgvector_mapped[i, j] = id_to_idx.get(str(pid), -1)

        # Only compare valid entries
        valid_mask = pgvector_mapped >= 0
        if not valid_mask.any():
            recall = {k: 0.0 for k in self.k_values}
        else:
            # For simplicity, compute recall on full arrays (mapped entries vs exact)
            recall = approximate_recall_at_k(pgvector_mapped, exact_indices, self.k_values)

        retriever.close()

        return {
            "index_build_time_s": round(index_time, 2),
            "pgvector_query_latency_ms": round(float(np.mean(query_latencies)), 2),
            "pgvector_recall@5": round(recall.get(5, 0.0), 4),
            "pgvector_recall@10": round(recall.get(10, 0.0), 4),
            "pgvector_recall@20": round(recall.get(20, 0.0), 4),
            "ingestion_time_s": round(ingestion_time, 2),
        }
```

- [ ] **Step 4: Run test to verify it passes**

```bash
uv run pytest src/tests/evaluation/test_pipeline.py::test_pipeline_runner_runs_all_folds -v
```
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add src/benchmark/evaluation/pipeline.py src/tests/evaluation/test_pipeline.py
git commit -m "feat(pipeline): add PipelineRunner for production-pipeline benchmark"
```

---

### Task 4: Create Pipeline Reporting

**Files:**
- Create: `src/benchmark/reporting/pipeline.py`
- Test: `src/tests/reporting/test_pipeline_reporting.py`
- Modify: `src/benchmark/reporting/__init__.py`

**Interfaces:**
- Consumes: `list[dict]` from `PipelineRunner.run()`
- Produces: `list[Path]` — Typst `.typ` files and JSON report

- [ ] **Step 1: Write failing test**

```python
from pathlib import Path

from benchmark.reporting.pipeline import write_pipeline_typst


def test_write_pipeline_typst(tmp_path: Path):
    results = [
        {
            "model_name": "FakeModel",
            "model_slug": "fake-model",
            "folds": [],
            "aggregate": {"map": {"mean": 0.8, "std": 0.01}},
            "production_metrics": {
                "index_build_time_s": {"mean": 0.5, "std": 0.1},
                "pgvector_query_latency_ms": {"mean": 12.3, "std": 1.2},
                "pgvector_recall@10": {"mean": 0.95, "std": 0.02},
                "ingestion_time_s": {"mean": 2.0, "std": 0.3},
            },
        }
    ]
    paths = write_pipeline_typst(results, output_dir=tmp_path)
    assert len(paths) == 1
    assert paths[0].exists()
    content = paths[0].read_text()
    assert "FakeModel" in content
    assert "0.95" in content
    assert "12.3" in content
```

- [ ] **Step 2: Run to verify failure**

```bash
uv run pytest src/tests/reporting/test_pipeline_reporting.py::test_write_pipeline_typst -v
```
Expected: FAIL — `ModuleNotFoundError: No module named 'benchmark.reporting.pipeline'`

- [ ] **Step 3: Implement pipeline reporting**

```python
"""Pipeline-specific reporting for production benchmark results."""
from __future__ import annotations

from datetime import datetime, timezone
from pathlib import Path
from typing import Any

from benchmark.reporting.typst import _AUTO_GEN_COMMENT, _fmt, _table_block
from benchmark.utils.logging import get_logger

logger = get_logger("reporting.pipeline")


def write_pipeline_typst(
    results: list[dict],
    output_dir: Path = Path("outputs/pipeline/tables"),
) -> list[Path]:
    """Generate Typst tables for production pipeline metrics.

    Args:
        results: Output of ``PipelineRunner.run()``.
        output_dir: Where to write ``.typ`` files.

    Returns:
        List of written file paths.
    """
    output_dir.mkdir(parents=True, exist_ok=True)
    paths: list[Path] = []

    # Production metrics table
    col_headers = [
        "Model", "Index Build (s)", "Ingestion (s)", "Query Latency (ms)",
        "Recall@5", "Recall@10", "Recall@20",
    ]
    data_rows = []
    for r in results:
        pm = r.get("production_metrics", {})
        data_rows.append([
            r["model_name"],
            _fmt(pm.get("index_build_time_s", {}).get("mean")),
            _fmt(pm.get("ingestion_time_s", {}).get("mean")),
            _fmt(pm.get("pgvector_query_latency_ms", {}).get("mean")),
            _fmt(pm.get("pgvector_recall@5", {}).get("mean")),
            _fmt(pm.get("pgvector_recall@10", {}).get("mean")),
            _fmt(pm.get("pgvector_recall@20", {}).get("mean")),
        ])

    content = _AUTO_GEN_COMMENT + "\n" + _table_block(
        caption="Production Pipeline — pgvector Metrics (3-Fold CV)",
        label="tab:pipeline-production",
        col_headers=col_headers,
        data_rows=data_rows,
    )
    path = output_dir / "pipeline_production.typ"
    path.write_text(content, encoding="utf-8")
    paths.append(path)

    logger.info("Pipeline Typst tables → %s", output_dir)
    return paths


def write_pipeline_json(
    results: list[dict],
    output_dir: Path = Path("outputs/pipeline/results"),
) -> Path:
    """Write complete pipeline results as JSON.

    Args:
        results: Output of ``PipelineRunner.run()``.
        output_dir: Where to write JSON file.

    Returns:
        Path to written JSON file.
    """
    output_dir.mkdir(parents=True, exist_ok=True)
    path = output_dir / "pipeline_results.json"
    path.write_text(
        json.dumps(results, indent=2),
        encoding="utf-8",
    )
    logger.info("Pipeline JSON results → %s", path)
    return path
```

Add `import json` at the top of the file.

- [ ] **Step 4: Export from `reporting/__init__.py`**

```python
from benchmark.reporting.pipeline import write_pipeline_json, write_pipeline_typst

__all__ = [
    # ... existing exports ...
    "write_pipeline_typst",
    "write_pipeline_json",
]
```

- [ ] **Step 5: Run test to verify pass**

```bash
uv run pytest src/tests/reporting/test_pipeline_reporting.py::test_write_pipeline_typst -v
```
Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add src/benchmark/reporting/pipeline.py src/tests/reporting/test_pipeline_reporting.py src/benchmark/reporting/__init__.py
git commit -m "feat(reporting): add pipeline production metrics tables"
```

---

### Task 5: Add `pipeline` CLI Subcommand

**Files:**
- Modify: `src/benchmark/cli/benchmark.py`
- Test: `src/tests/cli/test_pipeline_command.py`

**Interfaces:**
- Consumes: `PipelineRunner`
- Produces: CLI command `benchmark pipeline`

- [ ] **Step 1: Write failing test**

```python
from unittest.mock import patch

from typer.testing import CliRunner

from benchmark.cli.main import app

runner = CliRunner()


def test_pipeline_command_exists():
    result = runner.invoke(app, ["pipeline", "--help"])
    assert result.exit_code == 0
    assert "production pipeline" in result.output.lower()
```

- [ ] **Step 2: Run to verify failure**

```bash
uv run pytest src/tests/cli/test_pipeline_command.py::test_pipeline_command_exists -v
```
Expected: FAIL — `UsageError: No such command 'pipeline'`

- [ ] **Step 3: Add CLI command**

In `src/benchmark/cli/benchmark.py`, after the `thesis` command:

```python
# ── pipeline command ─────────────────────────────────────────────────────────

@app.command()
def pipeline(
    dataset_root: Annotated[Path, typer.Option("--dataset-root", "-d",
        help="Path to the raw dataset directory.")] = Path("data/raw/deepfashion"),
    models: Annotated[str, typer.Option("--models", "-m",
        help="Comma-separated model keys, or 'all'.")] = "all",
    folds: Annotated[int, typer.Option("--folds",
        help="Number of cross-validation folds.")] = 3,
    k: Annotated[str, typer.Option("--k",
        help="Comma-separated K values for P@K / R@K.")] = "5,10,20",
    batch_size: Annotated[int, typer.Option("--batch-size",
        help="Images per forward pass.")] = 64,
    no_cache: Annotated[bool, typer.Option("--no-cache",
        help="Disable embedding cache.")] = False,
    output: Annotated[Path, typer.Option("--output", "-o",
        help="Root output directory.", show_default=True)] = Path("outputs/pipeline"),
    device: Annotated[str, typer.Option("--device",
        help="Device (cpu, cuda, mps, auto).", show_default=True)] = "auto",
    seed: Annotated[int, typer.Option("--seed",
        help="Random seed.", show_default=True)] = 42,
    conn_string: Annotated[str, typer.Option("--conn-string",
        help="PostgreSQL connection string.", show_default=True)] = "postgresql://benchmark:benchmark@localhost:5432/benchmark",
    pg_lists: Annotated[int, typer.Option("--pg-lists",
        help="IVFFlat lists parameter.", show_default=True)] = 100,
    log_level: Annotated[str, typer.Option("--log-level", show_default=True)] = "INFO",
) -> None:
    """Run the production pipeline benchmark (thesis + pgvector).

    Example::

        uv run benchmark pipeline \\
            --dataset-root data/raw/deepfashion \\
            --models fashion-clip,resnet-50 \\
            --folds 3
    """
    setup_logging(level=log_level, log_file=output / "logs" / "pipeline.log")

    from benchmark.evaluation.pipeline import THESIS_MODEL_KEYS, PipelineRunner
    from benchmark.reporting.pipeline import write_pipeline_json, write_pipeline_typst

    model_keys = THESIS_MODEL_KEYS if models == "all" else [k.strip() for k in models.split(",")]
    top_k = [int(v) for v in k.split(",")]

    config_table = Table(title="Pipeline Benchmark Configuration", show_header=False)
    config_table.add_column("Key", style="bold")
    config_table.add_column("Value")
    config_table.add_row("Models", ", ".join(model_keys))
    config_table.add_row("Folds", str(folds))
    config_table.add_row("K values", str(top_k))
    config_table.add_row("Dataset root", str(dataset_root))
    config_table.add_row("Batch size", str(batch_size))
    config_table.add_row("Cache", "disabled" if no_cache else "enabled")
    config_table.add_row("PGVector", conn_string.split("@")[-1])
    config_table.add_row("PG lists", str(pg_lists))
    config_table.add_row("Seed", str(seed))
    console.print(config_table)

    runner = PipelineRunner(
        dataset_root=dataset_root,
        output_dir=output,
        k_values=top_k,
        folds=folds,
        seed=seed,
        device=device,
        use_cache=not no_cache,
        batch_size=batch_size,
        conn_string=conn_string,
        pg_lists=pg_lists,
    )
    results = runner.run(model_keys=model_keys)

    # Save results
    results_dir = output / "results"
    results_dir.mkdir(parents=True, exist_ok=True)
    out_path = results_dir / "pipeline_results.json"
    out_path.write_text(json.dumps(results, indent=2))
    console.print(f"\n[green]✓ Results written to {out_path}[/green]")

    # Generate Typst tables
    write_pipeline_typst(results, output_dir=output / "tables")
    console.print(f"[green]✓ Typst tables written to {output / 'tables'}[/green]")

    # Summary table
    summary = Table(title="Pipeline Results", show_header=True, header_style="bold cyan")
    summary.add_column("Model")
    summary.add_column("mAP (mean ± SD)", justify="right")
    summary.add_column("PG Recall@10", justify="right")
    summary.add_column("Query Latency (ms)", justify="right")

    for r in results:
        agg = r.get("aggregate", {})
        pm = r.get("production_metrics", {})
        summary.add_row(
            r["model_name"],
            f"{agg.get('map', {}).get('mean', 0):.4f} ± {agg.get('map', {}).get('std', 0):.4f}",
            f"{pm.get('pgvector_recall@10', {}).get('mean', 0):.4f}",
            f"{pm.get('pgvector_query_latency_ms', {}).get('mean', 0):.1f}",
        )
    console.print(summary)
```

- [ ] **Step 4: Run test to verify pass**

```bash
uv run pytest src/tests/cli/test_pipeline_command.py::test_pipeline_command_exists -v
```
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add src/benchmark/cli/benchmark.py src/tests/cli/test_pipeline_command.py
git commit -m "feat(cli): add pipeline subcommand for production benchmark"
```

---

### Task 6: Run Full 5K Benchmark

**Prerequisites:** PostgreSQL 16 with pgvector running locally or in Docker.

- [ ] **Step 1: Start PostgreSQL (if not running)**

Using Docker:
```bash
docker run -d \
  --name pgvector-benchmark \
  -e POSTGRES_USER=benchmark \
  -e POSTGRES_PASSWORD=benchmark \
  -e POSTGRES_DB=benchmark \
  -p 5432:5432 \
  -v $(pwd)/infra/postgres/init.sql:/docker-entrypoint-initdb.d/init.sql \
  pgvector/pgvector:pg16
```

Wait for it to be ready:
```bash
./infra/postgres/wait-for-pg.sh
```

- [ ] **Step 2: Run the pipeline benchmark on full 5K dataset**

```bash
cd /home/qingfa/Repos/ReSys.Shop/benchmarks
uv run benchmark pipeline \
  --dataset-root data/raw/fashion-product-images-small \
  --output outputs/pipeline \
  --folds 3 \
  --k 5,10,20 \
  --device auto \
  --seed 42 \
  --conn-string "postgresql://benchmark:benchmark@localhost:5432/benchmark" \
  --pg-lists 100
```

Expected runtime: ~30–60 min with GPU; 2–4 hours with CPU.

- [ ] **Step 3: Verify outputs**

```bash
find outputs/pipeline -type f | sort
```

Expected:
```
outputs/pipeline/logs/pipeline.log
outputs/pipeline/results/pipeline_results.json
outputs/pipeline/splits/fold_0_test.json
outputs/pipeline/splits/fold_0_train.json
outputs/pipeline/splits/fold_1_test.json
outputs/pipeline/splits/fold_1_train.json
outputs/pipeline/splits/fold_2_test.json
outputs/pipeline/splits/fold_2_train.json
outputs/pipeline/tables/pipeline_production.typ
```

- [ ] **Step 4: Inspect JSON trace**

```bash
python3 -m json.tool outputs/pipeline/results/pipeline_results.json | head -80
```

Verify keys present:
- `model_name`, `model_slug`
- `folds[].map`, `folds[].precision@5`, etc.
- `aggregate.map.mean`, `aggregate.map.std`
- `production_metrics.index_build_time_s.mean`
- `production_metrics.pgvector_recall@10.mean`
- `production_metrics.pgvector_query_latency_ms.mean`

- [ ] **Step 5: Inspect Typst table**

```bash
cat outputs/pipeline/tables/pipeline_production.typ
```

Verify it contains all 4 models, index build times, query latencies, and recall values.

- [ ] **Step 6: Commit outputs (optional)**

If you want to version the 5K results:
```bash
git add outputs/pipeline/results/pipeline_results.json outputs/pipeline/tables/
git commit -m "data: add 5K production pipeline benchmark results"
```

---

## Self-Review

### 1. Spec Coverage

| requirement | task |
|---|---|
| Batch ingestion into pgvector | Task 1 |
| Index build time measurement | Task 1 |
| pgvector query latency | Task 3 |
| Recall@K vs exact search | Task 2 + Task 3 |
| Production metrics aggregation | Task 3 |
| Typst table output | Task 4 |
| CLI entry point | Task 5 |
| Full 5K run | Task 6 |
| Log/trace along process | Task 3 (structured logging) + Task 6 (pipeline.log) |

### 2. Placeholder Scan

- No "TBD", "TODO", or "implement later" found.
- All steps have exact code blocks.
- All test commands have expected output.

### 3. Type Consistency

- `PipelineRunner.run()` returns `list[dict[str, Any]]` — matches `ThesisRunner.run()`
- `write_pipeline_typst()` takes `list[dict]` — matches `write_thesis_tables()`
- `approximate_recall_at_k()` returns `dict[int, float]` — used correctly in Task 3

---

## Execution Handoff

**Plan complete and saved to `docs/superpowers/plans/2026-07-15-production-pipeline-benchmark.md`.**

**Two execution options:**

**1. Subagent-Driven (recommended)** — I dispatch a fresh subagent per task, review between tasks, fast iteration

**2. Inline Execution** — Execute tasks in this session using executing-plans, batch execution with checkpoints

**Which approach?**
