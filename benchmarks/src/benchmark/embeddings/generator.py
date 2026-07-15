"""Generates and optionally caches embeddings for a full dataset split.

Workflow
--------
1. Check the cache — if ``(model.slug, dataset_name)`` exists, load and return.
2. Otherwise run batched inference, normalise, write to cache, return.

The generator is the only place in the pipeline that touches raw images,
keeping the evaluator and reporters free of I/O concerns.

Edge cases:
- Corrupt or missing images are logged and skipped within each batch.
- An all-skipped batch produces no embedding rows (empty batch skipped).
- Cache alignment warns when cached IDs do not fully match dataset samples.
"""
from __future__ import annotations

from dataclasses import dataclass
from pathlib import Path

import numpy as np
from tqdm import tqdm

from benchmark.datasets.loader import FashionDataset, Sample
from benchmark.embeddings import cache as embedding_cache
from benchmark.models.base import EmbeddingModel
from benchmark.utils.logging import get_logger

logger = get_logger("embeddings.generator")


@dataclass
class EmbeddingResult:
    """Output of ``EmbeddingGenerator.generate``.

    Attributes:
        model_name: Human-readable model name.
        model_slug: Filesystem-safe model identifier.
        dataset_name: Logical name of the source dataset.
        embeddings: Float32 array of shape ``(N, D)``, L2-normalised.
        samples: Sample list aligned 1-to-1 with embedding rows.
    """

    model_name: str
    model_slug: str
    dataset_name: str
    embeddings: np.ndarray
    samples: list[Sample]


class EmbeddingGenerator:
    """Generates (or loads from cache) embeddings for a FashionDataset.

    Batches image loading and model inference, with optional disk cache
    keyed by model slug + dataset name.

    Args:
        model: The embedding model adapter to use.
        dataset: The dataset to generate embeddings for.
        batch_size: Number of images per inference batch (default 64).
        use_cache: Whether to read/write the disk cache.
    """

    def __init__(
        self,
        model: EmbeddingModel,
        dataset: FashionDataset,
        batch_size: int = 64,
        use_cache: bool = True,
    ) -> None:
        self.model = model
        self.dataset = dataset
        self.batch_size = batch_size
        self.use_cache = use_cache

    def generate(self, dataset_name: str = "dataset") -> EmbeddingResult:
        """Return embeddings for all samples, loading from cache if available.

        Args:
            dataset_name: Logical name used for the cache key and result label.

        Returns:
            ``EmbeddingResult`` with embeddings aligned to ``dataset.samples``.
        """
        slug = self.model.slug

        if self.use_cache and embedding_cache.exists(slug, dataset_name):
            logger.info("Cache hit — %s / %s", slug, dataset_name)
            ids, embeddings = embedding_cache.load(slug, dataset_name)
            # Reconstruct sample alignment from cached IDs
            id_to_sample = {s.product_id: s for s in self.dataset.samples}
            aligned_samples: list[Sample] = []
            aligned_indices: list[int] = []
            for idx, pid in enumerate(ids):
                if pid in id_to_sample:
                    aligned_samples.append(id_to_sample[pid])
                    aligned_indices.append(idx)
            if len(aligned_samples) != len(ids):
                n_skipped = len(ids) - len(aligned_samples)
                logger.warning(
                    "Cache alignment: skipped %d missing IDs for %s / %s",
                    n_skipped, slug, dataset_name,
                )
            return EmbeddingResult(
                model_name=self.model.name,
                model_slug=slug,
                dataset_name=dataset_name,
                embeddings=embeddings[aligned_indices],
                samples=aligned_samples,
            )

        logger.info("Generating embeddings — %s on %s …", self.model.name, dataset_name)
        self.model.ensure_loaded()

        all_samples: list[Sample] = []
        all_embeddings: list[np.ndarray] = []
        samples = self.dataset.samples

        for start in tqdm(
            range(0, len(samples), self.batch_size),
            desc=f"{self.model.name}",
            unit="batch",
        ):
            batch_samples = samples[start : start + self.batch_size]
            images = []
            valid_samples = []
            for s in batch_samples:
                try:
                    from PIL import Image
                    img = Image.open(s.image_path).convert("RGB")
                    images.append(img)
                    valid_samples.append(s)
                except OSError as exc:
                    logger.warning("Skipping %s: %s", s.image_path, exc)

            if not images:
                continue

            batch_emb = self.model.embed_batch(images)
            all_samples.extend(valid_samples)
            all_embeddings.append(batch_emb)

        embeddings = np.concatenate(all_embeddings, axis=0)

        if self.use_cache:
            ids = [s.product_id for s in all_samples]
            path = embedding_cache.save(slug, dataset_name, ids, embeddings)
            logger.info("Cached %d embeddings → %s", len(all_samples), path)

        return EmbeddingResult(
            model_name=self.model.name,
            model_slug=slug,
            dataset_name=dataset_name,
            embeddings=embeddings,
            samples=all_samples,
        )
