"""Per-image latency measurement for embedding models."""
from __future__ import annotations

import time

import numpy as np
from PIL import Image

from benchmark.models.base import EmbeddingModel
from benchmark.utils.timing import LatencyStats


def measure_latency(
    model: EmbeddingModel,
    sample_images: list[Image.Image],
    warmup_runs: int = 10,
    benchmark_runs: int = 100,
) -> LatencyStats:
    """Measure single-image embedding latency with warmup.

    Args:
        model: Loaded embedding model.
        sample_images: Pool of images to sample from during measurement.
        warmup_runs: Number of forward passes before timing begins.
        benchmark_runs: Number of timed forward passes.

    Returns:
        ``LatencyStats`` with p50/p95/p99 in milliseconds.
    """
    n = len(sample_images)
    # Profile: Warmup forward passes to stabilise GPU compute cache
    for i in range(warmup_runs):
        model.embed(sample_images[i % n])

    # Profile: Timed forward passes for latency distribution
    samples_ms: list[float] = []
    for i in range(benchmark_runs):
        img = sample_images[i % n]
        t0 = time.perf_counter()
        model.embed(img)
        # Compute: Elapsed wall-clock time in milliseconds
        samples_ms.append((time.perf_counter() - t0) * 1000.0)

    # Aggregate: Compute p50/p95/p99/mean/std from raw samples
    return LatencyStats(samples=samples_ms)
