"""Core evaluator — runs the full retrieval + metrics pipeline for one model."""
from __future__ import annotations

from dataclasses import dataclass, field

from benchmark._constants import MAGIC, STR
from benchmark.datasets.loader import FashionDataset
from benchmark.embeddings.generator import EmbeddingResult
from benchmark.metrics import (
    mean_average_precision,
    mean_ndcg_at_k,
    mean_precision_at_k,
    mean_recall_at_k,
    measure_latency,
    measure_throughput,
)
from benchmark.models.base import EmbeddingModel
from benchmark.retrieval.cosine import retrieve_batch
from benchmark.utils.logging import get_logger

logger = get_logger("evaluation.evaluator")


@dataclass
class ModelMetrics:
    """Complete metric results for a single model run."""

    model_name: str
    dataset: str
    k_values: list[int]

    # Retrieval metrics keyed by k
    precision: dict[int, float] = field(default_factory=dict)
    recall: dict[int, float] = field(default_factory=dict)
    ndcg: dict[int, float] = field(default_factory=dict)
    map_score: float = 0.0

    # Efficiency
    latency: dict[str, float] = field(default_factory=dict)
    throughput_per_sec: float = 0.0

    def to_dict(self) -> dict:
        # Transform: Serialise metrics to flat dict for JSON output
        return {
            STR.MODEL: self.model_name,
            STR.DATASET: self.dataset,
            STR.MAP: round(self.map_score, MAGIC.METRIC_DECIMALS),
            STR.PRECISION: {f"@{k}": round(v, MAGIC.METRIC_DECIMALS) for k, v in self.precision.items()},
            STR.RECALL: {f"@{k}": round(v, MAGIC.METRIC_DECIMALS) for k, v in self.recall.items()},
            STR.NDCG: {f"@{k}": round(v, MAGIC.METRIC_DECIMALS) for k, v in self.ndcg.items()},
            STR.LATENCY_MS: self.latency,
            STR.THROUGHPUT: round(self.throughput_per_sec, 2),
        }


class Evaluator:
    """Runs the retrieval + metrics pipeline for an embedding result.

    Usage::

        evaluator = Evaluator(dataset=dataset, k_values=[1, 5, 10])
        metrics = evaluator.evaluate(result, model=model)
    """

    def __init__(
        self,
        dataset: FashionDataset,
        k_values: list[int] | None = None,
        measure_efficiency: bool = True,
        latency_warmup: int = MAGIC.WARMUP_RUNS,
        latency_runs: int = MAGIC.BENCHMARK_RUNS,
    ) -> None:
        self.dataset = dataset
        self.k_values = k_values or list(MAGIC.DEFAULT_K_VALUES)
        self.measure_efficiency = measure_efficiency
        self.latency_warmup = latency_warmup
        self.latency_runs = latency_runs

    # Contract: pre=result.embeddings != None, post=returns ModelMetrics with all fields
    def evaluate(
        self,
        result: EmbeddingResult,
        model: EmbeddingModel | None = None,
        dataset_name: str = "unknown",
    ) -> ModelMetrics:
        """Compute all metrics for ``result``.

        Args:
            result: Pre-generated embeddings with aligned sample list.
            model: Loaded model, required if ``measure_efficiency=True``.
            dataset_name: Label for the dataset (used in output).

        Returns:
            ``ModelMetrics`` with all scores populated.
        """
        logger.info("Evaluating %s on %s ...", result.model_name, dataset_name)

        embeddings = result.embeddings
        samples = result.samples
        # Transform: Build relevance sets — each query has a set of same-label items (excl self)
        labels = [s.label for s in samples]
        label_set_per_query = [
            {labels[j] for j in range(len(labels)) if labels[j] == labels[i] and j != i}
            for i in range(len(labels))
        ]
        label_counts_per_query = [
            sum(1 for j in range(len(labels)) if labels[j] == labels[i] and j != i)
            for i in range(len(labels))
        ]

        # Compute: Retrieve top-K via exact cosine similarity (self-retrieval)
        max_k = max(self.k_values)
        retrieved_indices = retrieve_batch(embeddings, embeddings, k=max_k, exclude_self=True)
        retrieved_labels = [[labels[idx] for idx in row] for row in retrieved_indices]

        metrics = ModelMetrics(
            model_name=result.model_name,
            dataset=dataset_name,
            k_values=self.k_values,
        )

        # Compute: Precision@K, Recall@K, nDCG@K for each K value
        for k in self.k_values:
            metrics.precision[k] = mean_precision_at_k(retrieved_labels, label_set_per_query, k)
            metrics.recall[k] = mean_recall_at_k(retrieved_labels, label_set_per_query, k,
                                                  all_counts=label_counts_per_query)
            metrics.ndcg[k] = mean_ndcg_at_k(retrieved_labels, label_set_per_query, k,
                                              all_counts=label_counts_per_query)

        # Compute: mAP across all queries capped at max_k
        metrics.map_score = mean_average_precision(
            retrieved_labels, label_set_per_query,
            all_counts=label_counts_per_query, k_cap=max_k,
        )

        # Profile: Measure latency and throughput when model is provided
        if self.measure_efficiency and model is not None:
            from contextlib import suppress

            from PIL import Image

            # Filter: Load sample images for latency measurement (skip corrupted files)
            sample_images = []
            for s in samples[:MAGIC.MAX_LATENCY_SAMPLES]:
                with suppress(OSError):
                    sample_images.append(Image.open(s.image_path).convert("RGB"))

            if sample_images:
                latency_stats = measure_latency(
                    model,
                    sample_images,
                    warmup_runs=self.latency_warmup,
                    benchmark_runs=self.latency_runs,
                )
                metrics.latency = latency_stats.to_dict()
                metrics.throughput_per_sec = measure_throughput(model, sample_images)

        logger.info(
            "%s — mAP=%.4f  P@10=%.4f  R@10=%.4f",
            result.model_name,
            metrics.map_score,
            metrics.precision.get(10, 0.0),
            metrics.recall.get(10, 0.0),
        )
        return metrics

    # Contract: pre=query_result.samples disjoint from gallery_result.samples
    def evaluate_split(
        self,
        query_result: EmbeddingResult,
        gallery_result: EmbeddingResult,
        model: EmbeddingModel | None = None,
        dataset_name: str = "unknown",
    ) -> ModelMetrics:
        """Compute retrieval metrics with proper query/gallery split.

        Queries retrieve from a separate gallery set. This is the academically
        correct protocol: no self-exclusion needed because query and gallery
        are disjoint samples.

        Args:
            query_result:   Embeddings for the query split (Q, D).
            gallery_result: Embeddings for the gallery split (G, D).
            model:          Loaded model for latency measurement (optional).
            dataset_name:   Label used in output files.

        Returns:
            ``ModelMetrics`` with all scores in [0, 1].
        """
        logger.info("Evaluating %s (split-aware) ...", query_result.model_name)

        q_embeddings = query_result.embeddings
        g_embeddings = gallery_result.embeddings
        q_samples = query_result.samples
        g_samples = gallery_result.samples

        # Transform: Build relevance from gallery labels for each query
        g_labels = [s.label for s in g_samples]
        q_labels = [s.label for s in q_samples]

        # Explain: Each query has exactly one relevant label in split-aware mode
        relevance = [{lbl} for lbl in q_labels]
        relevant_counts = [
            sum(1 for gl in g_labels if gl == ql)
            for ql in q_labels
        ]

        # Compute: Retrieve top-K from gallery using query embeddings
        max_k = max(self.k_values)
        retrieved_indices = retrieve_batch(
            q_embeddings, g_embeddings, k=max_k, exclude_self=False
        )
        retrieved_labels: list[list[str]] = [
            [g_labels[idx] for idx in row]
            for row in retrieved_indices
        ]

        metrics = ModelMetrics(
            model_name=query_result.model_name,
            dataset=dataset_name,
            k_values=self.k_values,
        )

        # Compute: Precision@K, Recall@K, nDCG@K for each K value
        for k in self.k_values:
            metrics.precision[k] = mean_precision_at_k(retrieved_labels, relevance, k)
            metrics.recall[k] = mean_recall_at_k(retrieved_labels, relevance, k,
                                                  all_counts=relevant_counts)
            metrics.ndcg[k] = mean_ndcg_at_k(retrieved_labels, relevance, k,
                                              all_counts=relevant_counts)

        # Compute: mAP across all queries capped at max_k
        metrics.map_score = mean_average_precision(
            retrieved_labels, relevance,
            all_counts=relevant_counts, k_cap=max_k,
        )

        # Profile: Measure latency and throughput when model is provided
        if self.measure_efficiency and model is not None:
            from contextlib import suppress

            from PIL import Image as PILImage
            imgs: list[PILImage.Image] = []
            for s in q_samples[:MAGIC.MAX_LATENCY_SAMPLES]:
                with suppress(OSError):
                    imgs.append(PILImage.open(s.image_path).convert("RGB"))
            if imgs:
                stats = measure_latency(
                    model, imgs,
                    warmup_runs=self.latency_warmup,
                    benchmark_runs=self.latency_runs,
                )
                metrics.latency = stats.to_dict()
                metrics.throughput_per_sec = measure_throughput(model, imgs)

        logger.info(
            "%s — mAP=%.4f  P@10=%.4f  R@10=%.4f",
            query_result.model_name,
            metrics.map_score,
            metrics.precision.get(10, 0.0),
            metrics.recall.get(10, 0.0),
        )
        return metrics
