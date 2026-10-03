"""High-resolution timing utilities for latency benchmarking.

Provides ``Timer`` (accumulates multiple measurements) and ``timed`` (one-shot
context manager) for measuring inference latency. ``LatencyStats`` computes
percentiles, mean, and standard deviation.

Edge cases:
- ``LatencyStats`` raises ``ValueError`` on empty sample lists.
- Standard deviation is 0.0 for single-sample lists.
- ``__len__`` returns 0 on a fresh ``Timer`` before any measurement.
"""
from __future__ import annotations

import time
from collections.abc import Generator
from contextlib import contextmanager
from dataclasses import dataclass, field
from statistics import mean, quantiles, stdev

from benchmark._constants import MAGIC, STR


@dataclass
class LatencyStats:
    """Summary statistics for a series of latency measurements (milliseconds).

    Attributes:
        samples: Raw latency values in milliseconds.
        p50: 50th percentile (median).
        p95: 95th percentile.
        p99: 99th percentile.
        mean: Arithmetic mean.
        std: Standard deviation (0.0 for single-sample lists).
        min: Minimum observed latency.
        max: Maximum observed latency.
    """

    samples: list[float]
    p50: float = field(init=False)
    p95: float = field(init=False)
    p99: float = field(init=False)
    mean: float = field(init=False)
    std: float = field(init=False)
    min: float = field(init=False)
    max: float = field(init=False)

    def __post_init__(self) -> None:
        if not self.samples:
            raise ValueError("Cannot compute stats on empty sample list")
        qs = quantiles(self.samples, n=MAGIC.N_QUANTILES)
        self.p50 = qs[MAGIC.P50_INDEX]
        self.p95 = qs[MAGIC.P95_INDEX]
        self.p99 = qs[MAGIC.P99_INDEX]
        self.mean = mean(self.samples)
        self.std = stdev(self.samples) if len(self.samples) > 1 else 0.0
        self.min = min(self.samples)
        self.max = max(self.samples)

    def to_dict(self) -> dict[str, float]:
        """Convert stats to a flat dict for serialisation.

        Returns:
            Dict with keys ``mean_ms``, ``std_ms``, ``p50_ms``, ``p95_ms``,
            ``p99_ms``, ``min_ms``, ``max_ms``, ``n_samples``.
        """
        return {
            STR.MEAN_MS: round(self.mean, MAGIC.LATENCY_DECIMALS),
            STR.STD_MS: round(self.std, MAGIC.LATENCY_DECIMALS),
            STR.P50_MS: round(self.p50, MAGIC.LATENCY_DECIMALS),
            STR.P95_MS: round(self.p95, MAGIC.LATENCY_DECIMALS),
            STR.P99_MS: round(self.p99, MAGIC.LATENCY_DECIMALS),
            STR.MIN_MS: round(self.min, MAGIC.LATENCY_DECIMALS),
            STR.MAX_MS: round(self.max, MAGIC.LATENCY_DECIMALS),
            STR.N_SAMPLES: len(self.samples),
        }


class Timer:
    """Accumulator for multiple latency measurements.

    Records elapsed times via a context manager or manual ``record()`` call.
    Returns a ``LatencyStats`` summary on demand.
    """

    def __init__(self) -> None:
        self._samples: list[float] = []

    @contextmanager
    def measure(self) -> Generator[None, None, None]:
        """Context manager that records a single measurement.

        Measures wall-clock time via ``time.perf_counter()``, converts to
        milliseconds, and appends to the internal sample list.
        """
        start = time.perf_counter()
        try:
            yield
        finally:
            elapsed_ms = (time.perf_counter() - start) * MAGIC.MS_CONVERSION
            self._samples.append(elapsed_ms)

    def record(self, elapsed_ms: float) -> None:
        """Manually record a pre-computed elapsed time in milliseconds.

        Args:
            elapsed_ms: Duration in milliseconds.
        """
        self._samples.append(elapsed_ms)

    @property
    def stats(self) -> LatencyStats:
        """Return summary statistics for all recorded measurements."""
        return LatencyStats(samples=list(self._samples))

    def reset(self) -> None:
        """Clear all recorded measurements."""
        self._samples.clear()

    def __len__(self) -> int:
        return len(self._samples)


@contextmanager
def timed(label: str = "") -> Generator[dict[str, float], None, None]:
    """One-shot context manager that yields a result dict.

    The result dict is filled with ``elapsed_ms`` and ``label`` after the
    wrapped block completes.

    Args:
        label: Optional label stored in the result dict.

    Yields:
        A mutable dict that will contain ``elapsed_ms`` (float) and
        ``label`` (str) after the ``with`` block exits.
    """
    result: dict[str, float] = {}
    start = time.perf_counter()
    try:
        yield result
    finally:
        result[STR.ELAPSED_MS] = (time.perf_counter() - start) * MAGIC.MS_CONVERSION
        result[STR.LABEL] = label  # type: ignore[assignment]
