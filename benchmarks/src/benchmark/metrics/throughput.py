"""Batch throughput measurement (images / second)."""

from __future__ import annotations

import time

from PIL import Image

from benchmark.models.base import EmbeddingModel


def measure_throughput(
    model: EmbeddingModel,
    sample_images: list[Image.Image],
    batch_size: int = 64,
    num_batches: int = 10,
) -> float:
    """Measure batch throughput in images/second.

    Args:
        model: Loaded embedding model.
        sample_images: Pool of images (must be >= batch_size).
        batch_size: Number of images per forward pass.
        num_batches: Number of batches to average over.

    Returns:
        Throughput as images per second (float).
    """
    n = len(sample_images)
    total_images = 0
    t0 = time.perf_counter()

    for b in range(num_batches):
        batch = [sample_images[(b * batch_size + i) % n] for i in range(batch_size)]
        model.embed_batch(batch)
        total_images += batch_size

    elapsed = time.perf_counter() - t0
    return total_images / elapsed if elapsed > 0 else 0.0
