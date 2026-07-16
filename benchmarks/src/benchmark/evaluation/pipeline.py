"""PipelineRunner — full production pipeline: embedding -> pgvector -> query -> evaluate."""
from __future__ import annotations

import time
from dataclasses import dataclass, field
from pathlib import Path
from typing import Any

import numpy as np
import pandas as pd

from benchmark._constants import CONST, FAISS_PARAMS, MAGIC, SPLIT, THESIS_MODEL_KEYS
from benchmark.datasets.ground_truth import GroundTruth
from benchmark.datasets.loader import FashionDataset
from benchmark.embeddings.generator import EmbeddingGenerator
from benchmark.evaluation.evaluator import Evaluator
from benchmark.evaluation.stats import aggregate_mean_std
from benchmark.metrics.latency import measure_latency
from benchmark.metrics.recall_comparison import approximate_recall_at_k
from benchmark.metrics.throughput import measure_throughput
from benchmark.models import get_registry
from benchmark.retrieval.cosine import retrieve_batch
from benchmark.retrieval.pgvector import PgvectorRetriever
from benchmark.utils.logging import get_logger

logger = get_logger("evaluation.pipeline")


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
        folds: int = MAGIC.N_FOLDS_DEFAULT,
        seed: int = MAGIC.SEED,
        device: str = "auto",
        use_cache: bool = True,
        batch_size: int = MAGIC.BATCH_SIZE,
        conn_string: str = "postgresql://benchmark:benchmark@localhost:5432/benchmark",
        pg_lists: int = FAISS_PARAMS.N_LISTS,
    ) -> None:
        self.dataset_root = dataset_root
        self.output_dir = output_dir
        self.k_values = k_values or list(MAGIC.DEFAULT_THESIS_K_VALUES)
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

        # Enforce: styles.csv must exist
        styles_csv = self.dataset_root / "styles.csv"
        if not styles_csv.exists():
            raise FileNotFoundError(f"styles.csv not found: {styles_csv}")

        # Parse: Load metadata CSV and build stratified k-fold splits
        df = pd.read_csv(styles_csv)
        gt = GroundTruth(df, min_category_freq=MAGIC.MIN_CATEGORY_FREQ)
        splits = gt.generate_splits(
            n_splits=self.folds,
            seed=self.seed,
            output_dir=self.output_dir / "splits",
        )

        # Batch: Evaluate each model independently
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
        logger.info("Evaluating %s ...", model.name)

        fold_results: list[dict[str, Any]] = []
        prod_metrics_per_fold: list[dict[str, Any]] = []

        # Profile: Time model weight loading
        t0 = time.perf_counter()
        model.load()
        load_time_ms = (time.perf_counter() - t0) * MAGIC.MS_CONVERSION

        # Batch: Evaluate each fold independently
        for fold_idx, (train_path, test_path) in enumerate(splits):
            logger.info("  Fold %d ...", fold_idx)
            fold_result, prod_metrics = self._evaluate_fold(
                model, train_path, test_path, fold_idx, load_time_ms
            )
            fold_results.append(fold_result)
            prod_metrics_per_fold.append(prod_metrics)

        # Aggregate: Compute mean +/- SD across folds for retrieval metrics
        aggregate: dict[str, dict[str, float]] = {}
        metric_keys = [
            "map",
            "precision@5",
            "precision@10",
            "precision@20",
            "recall@5",
            "recall@10",
            "recall@20",
            "latency_mean_ms",
            "throughput_per_sec",
            "load_time_ms",
            "index_storage_mb",
            "ram_mb",
        ]
        for mk in metric_keys:
            vals = [f[mk] for f in fold_results if mk in f]
            if vals:
                aggregate[mk] = aggregate_mean_std(vals)

        # Aggregate: Compute mean +/- SD across folds for production metrics
        prod_aggregate: dict[str, dict[str, float]] = {}
        prod_keys = [
            "index_build_time_s",
            "pgvector_query_latency_ms",
            "pgvector_recall@5",
            "pgvector_recall@10",
            "pgvector_recall@20",
            "ingestion_time_s",
        ]
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
        # Create: Datasets from fold split files
        query_ds = FashionDataset(
            dataset_root=self.dataset_root, split_file=test_path, split=SPLIT.TEST
        )
        query_ds.load()
        gallery_ds = FashionDataset(
            dataset_root=self.dataset_root, split_file=train_path, split=SPLIT.TRAIN
        )
        gallery_ds.load()

        # Transform: Generate embeddings for query and gallery
        query_gen = EmbeddingGenerator(
            model=model,
            dataset=query_ds,
            batch_size=self.batch_size,
            use_cache=self.use_cache,
        )
        gallery_gen = EmbeddingGenerator(
            model=model,
            dataset=gallery_ds,
            batch_size=self.batch_size,
            use_cache=self.use_cache,
        )
        query_result = query_gen.generate(dataset_name=f"fold_{fold_idx}_test")
        gallery_result = gallery_gen.generate(dataset_name=f"fold_{fold_idx}_train")

        # Compute: Split-aware retrieval metrics (exact cosine)
        evaluator = Evaluator(
            dataset=query_ds, k_values=self.k_values, measure_efficiency=False
        )
        metrics = evaluator.evaluate_split(
            query_result=query_result,
            gallery_result=gallery_result,
            dataset_name=f"fold_{fold_idx}",
        )

        # Call: pgvector pipeline — ingest, index, query, measure recall
        prod_metrics = self._run_pgvector_pipeline(
            model, gallery_result, query_result, gallery_ds
        )

        # Profile: Manual efficiency metrics
        from contextlib import suppress

        from PIL import Image

        sample_images = []
        for s in query_ds.samples[:MAGIC.MAX_LATENCY_SAMPLES]:
            with suppress(OSError):
                sample_images.append(Image.open(s.image_path).convert("RGB"))

        latency_stats = measure_latency(model, sample_images, warmup_runs=MAGIC.WARMUP_RUNS, benchmark_runs=MAGIC.BENCHMARK_RUNS)
        throughput = measure_throughput(
            model, sample_images[:MAGIC.BATCH_SIZE], batch_size=MAGIC.BATCH_SIZE, num_batches=10
        )

        # Profile: Peak RAM during batch inference
        import gc

        import psutil

        process = psutil.Process()
        gc.collect()
        baseline = process.memory_info().rss - process.memory_info().shared
        model.embed_batch(sample_images[:MAGIC.BATCH_SIZE])
        peak = process.memory_info().rss - process.memory_info().shared
        ram_mb = (peak - baseline) / CONST.BYTES_TO_MB

        # Compute: Storage footprint
        total_storage_mb = query_result.embeddings.nbytes / CONST.BYTES_TO_MB

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
        self, model, gallery_result, query_result, gallery_ds
    ) -> dict[str, Any]:
        """Ingest into pgvector, build index, query, measure recall + latency.

        Args:
            model: Embedding model (used for dimension).
            gallery_result: EmbeddingResult for the gallery/train set.
            query_result: EmbeddingResult for the query/test set.
            gallery_ds: Gallery FashionDataset (for product IDs and labels).

        Returns:
            Dict of production metrics (index time, query latency, recall@K).
        """
        # Explain: Map embedding dimension to pgvector table name
        dim = model.embedding_dim
        if dim == 512:
            table = "products_512"
        elif dim == 768:
            table = "product_embeddings_768"
        elif dim == 1280:
            table = "products_1280"
        elif dim == 2048:
            table = "products_2048"
        else:
            table = f"products_{dim}"

        try:
            # Call: Open pgvector connection with context manager for auto-close
            with PgvectorRetriever(
                conn_string=self.conn_string,
                table=table,
                embedding_col="embedding",
                id_col="id",
                label_col="label",
            ) as retriever:
                # Purge: Clear existing data from the table
                retriever.clear_table()

                # Call: Batch ingest gallery embeddings into pgvector
                gallery_ids = [s.product_id for s in gallery_ds.samples]
                gallery_labels = [getattr(s, "label", "unknown") for s in gallery_ds.samples]
                t0 = time.perf_counter()
                retriever.upsert_batch(gallery_ids, gallery_labels, gallery_result.embeddings)
                ingestion_time = time.perf_counter() - t0

                # Build: IVFFlat index for approximate search
                index_time = retriever.build_index(dim=model.embedding_dim, lists=self.pg_lists)

                # Query: Run pgvector approximate search for each query embedding
                pgvector_results = []
                query_latencies = []
                for emb in query_result.embeddings:
                    t0 = time.perf_counter()
                    results = retriever.query(emb, top_k=max(self.k_values))
                    query_latencies.append((time.perf_counter() - t0) * 1000.0)
                    pgvector_results.append([r["id"] for r in results])

                # Compute: Exact cosine retrieval for recall comparison
                exact_indices = retrieve_batch(
                    query_result.embeddings,
                    gallery_result.embeddings,
                    k=max(self.k_values),
                    exclude_self=False,
                )

                # Transform: Map pgvector string IDs back to gallery index positions
                id_to_idx = {str(pid): i for i, pid in enumerate(gallery_ids)}
                pgvector_mapped = np.full_like(exact_indices, -1)
                for i, pgv_ids in enumerate(pgvector_results):
                    for j, pgv_id in enumerate(pgv_ids):
                        if j < pgvector_mapped.shape[1] and str(pgv_id) in id_to_idx:
                            pgvector_mapped[i, j] = id_to_idx[str(pgv_id)]

                # Compute: Approximate recall@K comparing pgvector vs exact cosine
                recall = approximate_recall_at_k(pgvector_mapped, exact_indices, self.k_values)

                return {
                    "index_build_time_s": round(index_time, 2),
                    "pgvector_query_latency_ms": round(float(np.mean(query_latencies)), 2),
                    "pgvector_recall@5": round(recall.get(5, 0.0), 4),
                    "pgvector_recall@10": round(recall.get(10, 0.0), 4),
                    "pgvector_recall@20": round(recall.get(20, 0.0), 4),
                    "ingestion_time_s": round(ingestion_time, 2),
                }
        except Exception as exc:
            # Degrade: pgvector unavailable — return zero metrics and log warning
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
