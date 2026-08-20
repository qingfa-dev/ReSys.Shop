"""BenchmarkRunner — orchestrates multi-model evaluation end-to-end.

Supports two evaluation modes:
  1. Self-retrieval (default): query and gallery are the same set.
     Fast for quick tests on unified splits.
  2. Split-aware: separate query/test and gallery/train sets.
     Academically correct protocol for thesis results.

Usage (split-aware)::

    query_ds = FashionDataset(root=..., split_file=test_json)
    query_ds.load()
    gallery_ds = FashionDataset(root=..., split_file=train_json)
    gallery_ds.load()

    runner = BenchmarkRunner(dataset=query_ds, gallery_dataset=gallery_ds,
                             k_values=[1, 5, 10, 20])
    results = runner.run(["fashion_clip", "clip_b32"])
"""
from __future__ import annotations

from benchmark._constants import MAGIC
from benchmark.datasets.loader import FashionDataset
from benchmark.embeddings.generator import EmbeddingGenerator
from benchmark.embeddings.storage import save_embeddings
from benchmark.evaluation.evaluator import Evaluator, ModelMetrics
from benchmark.models import EmbeddingModel, get_registry
from benchmark.utils.logging import get_logger
from benchmark.utils.timing import timed

logger = get_logger("evaluation.benchmark")


class BenchmarkRunner:
    """Evaluates one or more models on a loaded ``FashionDataset``.

    Args:
        dataset:            Query/test dataset (required).
        k_values:           K cut-offs for Precision@K, Recall@K, nDCG@K.
        batch_size:         Forward-pass batch size (tune to GPU memory).
        use_cache:          Load cached embeddings from ``data/cache/`` if available.
        measure_efficiency: Whether to measure per-image latency and throughput.
        save_embeddings:    Whether to persist embeddings to ``outputs/embeddings/``.
        dataset_name:       Label used in output file names and report headers.
        device:             Device for model inference (auto, cpu, cuda, mps).
        gallery_dataset:    If provided, use split-aware evaluation (gallery = separate
                            training set). If None, use self-retrieval on the query set.
    """

    def __init__(
        self,
        dataset: FashionDataset,
        k_values: list[int] | None = None,
        batch_size: int = MAGIC.BATCH_SIZE,
        use_cache: bool = True,
        measure_efficiency: bool = True,
        save_embeddings: bool = True,
        dataset_name: str = "dataset",
        device: str = "auto",
        gallery_dataset: FashionDataset | None = None,
    ) -> None:
        self.dataset = dataset
        self.gallery_dataset = gallery_dataset
        self.k_values = k_values or list(MAGIC.DEFAULT_K_VALUES)
        self.batch_size = batch_size
        self.use_cache = use_cache
        self.measure_efficiency = measure_efficiency
        self._save_embeddings = save_embeddings
        self.dataset_name = dataset_name
        self.device = device
        # Call: Initialise model registry for the target device
        self._registry = get_registry(device=device)
        self._evaluator = Evaluator(
            dataset=dataset,
            k_values=self.k_values,
            measure_efficiency=measure_efficiency,
        )

    @property
    def mode(self) -> str:
        """Evaluation mode: 'split' if gallery_dataset is provided, else 'self'."""
        return "split" if self.gallery_dataset is not None else "self"

    def run(self, model_keys: list[str] | None = None) -> list[ModelMetrics]:
        """Evaluate all (or a subset of) models.

        Args:
            model_keys: Keys into ``REGISTRY``. ``None`` evaluates all models.

        Returns:
            List of ``ModelMetrics`` in the same order as ``model_keys``.
        """
        keys = model_keys or list(self._registry.keys())
        logger.info(
            "Starting benchmark: %d model(s), k=%s, mode=%s",
            len(keys), self.k_values, self.mode,
        )

        # Batch: Iterate through models sequentially (each generates embeddings independently)
        results: list[ModelMetrics] = []
        for key in keys:
            model: EmbeddingModel = self._registry[key]
            # Profile: Time each model's end-to-end evaluation
            with timed(label=key) as t:
                if self.gallery_dataset is not None:
                    metrics = self._run_split(model)
                else:
                    metrics = self._run_self(model)
            logger.info(
                "Finished %s in %.1f s — mAP=%.4f",
                key, t["elapsed_ms"] / 1000, metrics.map_score,
            )
            results.append(metrics)

        return results

    def _run_self(self, model: EmbeddingModel) -> ModelMetrics:
        """Self-retrieval: query and gallery from the same embedding set."""
        # Transform: Generate embeddings for the full dataset
        generator = EmbeddingGenerator(
            model=model,
            dataset=self.dataset,
            batch_size=self.batch_size,
            use_cache=self.use_cache,
        )
        result = generator.generate(dataset_name=self.dataset_name)

        # Cache: Persist embeddings to outputs/embeddings/ for reproducibility
        if self._save_embeddings:
            ids = [s.product_id for s in result.samples]
            save_embeddings(result.embeddings, ids, model_slug=model.slug)

        return self._evaluator.evaluate(
            result=result,
            model=model if self.measure_efficiency else None,
            dataset_name=self.dataset_name,
        )

    def _run_split(self, model: EmbeddingModel) -> ModelMetrics:
        """Split-aware: query from test, gallery from separate train set."""
        assert self.gallery_dataset is not None

        # Transform: Generate embeddings for query and gallery datasets
        query_gen = EmbeddingGenerator(
            model=model,
            dataset=self.dataset,
            batch_size=self.batch_size,
            use_cache=self.use_cache,
        )
        gallery_gen = EmbeddingGenerator(
            model=model,
            dataset=self.gallery_dataset,
            batch_size=self.batch_size,
            use_cache=self.use_cache,
        )

        query_result = query_gen.generate(
            dataset_name=f"{self.dataset_name}__query"
        )
        gallery_result = gallery_gen.generate(
            dataset_name=f"{self.dataset_name}__gallery"
        )

        # Cache: Persist both query and gallery embeddings
        if self._save_embeddings:
            save_embeddings(
                query_result.embeddings,
                [s.product_id for s in query_result.samples],
                model_slug=f"{model.slug}__query",
            )
            save_embeddings(
                gallery_result.embeddings,
                [s.product_id for s in gallery_result.samples],
                model_slug=f"{model.slug}__gallery",
            )

        return self._evaluator.evaluate_split(
            query_result=query_result,
            gallery_result=gallery_result,
            model=model if self.measure_efficiency else None,
            dataset_name=self.dataset_name,
        )
