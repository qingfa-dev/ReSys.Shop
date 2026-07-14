"""ThesisRunner — orchestrates the 4-model × 3-fold evaluation protocol."""
from __future__ import annotations

import json
import time
from dataclasses import dataclass, field
from pathlib import Path
from typing import Any

import numpy as np
import pandas as pd
import psutil
from PIL import Image

from benchmark.datasets.ground_truth import GroundTruth
from benchmark.datasets.loader import FashionDataset
from benchmark.embeddings.generator import EmbeddingGenerator
from benchmark.evaluation.evaluator import Evaluator, ModelMetrics
from benchmark.evaluation.stats import aggregate_mean_std, bootstrap_ci, cohens_d
from benchmark.metrics.latency import measure_latency
from benchmark.metrics.throughput import measure_throughput
from benchmark.models import get_registry
from benchmark.utils.logging import get_logger
from benchmark.utils.timing import timed

logger = get_logger("evaluation.thesis")


THESIS_MODEL_KEYS = ["fashion-clip", "resnet-50", "efficientnet-b0", "clip-generic"]


@dataclass
class ThesisResult:
    """Complete results for one model across all folds."""

    model_name: str
    model_slug: str
    folds: list[dict[str, Any]] = field(default_factory=list)
    aggregate: dict[str, dict[str, float]] = field(default_factory=dict)


class ThesisRunner:
    """Evaluates thesis models with k-fold cross-validation."""

    def __init__(
        self,
        dataset_root: Path,
        output_dir: Path = Path("outputs/thesis"),
        k_values: list[int] | None = None,
        folds: int = 3,
        seed: int = 42,
        device: str = "auto",
        use_cache: bool = True,
        batch_size: int = 64,
    ) -> None:
        self.dataset_root = dataset_root
        self.output_dir = output_dir
        self.k_values = k_values or [5, 10, 20]
        self.folds = folds
        self.seed = seed
        self.device = device
        self.use_cache = use_cache
        self.batch_size = batch_size
        self._registry = get_registry(device=device)

    def run(
        self,
        model_keys: list[str] | None = None,
    ) -> list[dict[str, Any]]:
        """Run the full thesis protocol.

        Args:
            model_keys: Subset of models to evaluate. Defaults to THESIS_MODEL_KEYS.

        Returns:
            List of result dicts (one per model), JSON-serializable.
        """
        keys = model_keys or THESIS_MODEL_KEYS
        logger.info("Starting thesis benchmark: %d models, %d folds", len(keys), self.folds)

        # 1. Load metadata and build splits
        styles_csv = self.dataset_root / "styles.csv"
        if not styles_csv.exists():
            logger.error("styles.csv not found at %s", styles_csv)
            raise FileNotFoundError(f"styles.csv not found: {styles_csv}")

        df = pd.read_csv(styles_csv, on_bad_lines="warn")
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

    def _evaluate_model(
        self,
        model,
        splits: list[tuple[Path, Path]],
    ) -> dict[str, Any]:
        """Evaluate one model across all folds."""
        logger.info("Evaluating %s …", model.name)

        fold_results: list[dict[str, Any]] = []
        fold_map_scores: list[float] = []

        # Load model once, time it
        t0 = time.perf_counter()
        model.load()
        load_time_ms = (time.perf_counter() - t0) * 1000.0

        for fold_idx, (train_path, test_path) in enumerate(splits):
            logger.info("  Fold %d …", fold_idx)
            fold_result = self._evaluate_fold(model, train_path, test_path, fold_idx, load_time_ms)
            fold_results.append(fold_result)
            fold_map_scores.append(fold_result["map"])

        # Aggregate
        aggregate: dict[str, dict[str, float]] = {}
        metric_keys = ["map", "precision@5", "precision@10", "precision@20",
                       "recall@5", "recall@10", "recall@20",
                       "latency_mean_ms", "throughput_per_sec",
                       "load_time_ms", "index_storage_mb", "ram_mb"]
        for mk in metric_keys:
            vals = [f[mk] for f in fold_results if mk in f]
            if vals:
                aggregate[mk] = aggregate_mean_std(vals)

        # Bootstrap CI for mAP
        if len(fold_map_scores) >= 3:
            ci_lower, ci_upper = bootstrap_ci(fold_map_scores, seed=self.seed)
            aggregate["map"]["ci_95"] = [ci_lower, ci_upper]

        return {
            "model_name": model.name,
            "model_slug": model.slug,
            "folds": fold_results,
            "aggregate": aggregate,
        }

    def _evaluate_fold(
        self,
        model,
        train_path: Path,
        test_path: Path,
        fold_idx: int,
        load_time_ms: float,
    ) -> dict[str, Any]:
        """Evaluate one model on one fold."""
        query_ds = FashionDataset(
            dataset_root=self.dataset_root,
            split_file=test_path,
            split="test",
        )
        query_ds.load()
        gallery_ds = FashionDataset(
            dataset_root=self.dataset_root,
            split_file=train_path,
            split="train",
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

        # Evaluate
        evaluator = Evaluator(
            dataset=query_ds,
            k_values=self.k_values,
            measure_efficiency=False,  # we measure manually below
        )
        metrics = evaluator.evaluate_split(
            query_result=query_result,
            gallery_result=gallery_result,
            dataset_name=f"fold_{fold_idx}",
        )

        # Efficiency metrics
        sample_images = self._load_sample_images(query_ds.samples, max_n=200)
        latency_stats = measure_latency(model, sample_images, warmup_runs=10, benchmark_runs=100)
        throughput = measure_throughput(model, sample_images[:64], batch_size=64, num_batches=10)

        # RAM (peak during batch inference)
        ram_mb = self._measure_peak_ram(model, sample_images[:64])

        # Storage
        total_storage_mb = query_result.embeddings.nbytes / (1024 * 1024)

        return {
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

    def _load_sample_images(self, samples, max_n: int = 200) -> list[Image.Image]:
        """Load up to max_n sample images for latency measurement."""
        images: list[Image.Image] = []
        for s in samples[:max_n]:
            try:
                images.append(Image.open(s.image_path).convert("RGB"))
            except OSError:
                pass
        return images

    def _measure_peak_ram(self, model, sample_images: list[Image.Image]) -> float:
        """Measure peak RSS during a batch inference."""
        process = psutil.Process()
        # Force garbage collection to get clean baseline
        import gc
        gc.collect()
        baseline = process.memory_info().rss
        # Run batch inference
        model.embed_batch(sample_images)
        peak = process.memory_info().rss
        return (peak - baseline) / (1024 * 1024)
