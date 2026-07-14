"""High-resolution timing utilities for latency benchmarking."""

from __future__ import annotations

import time
from contextlib import contextmanager
from dataclasses import dataclass, field
from statistics import mean, median, quantiles, stdev
from typing import Generator


@dataclass
class LatencyStats:
    """Summary statistics for a series of latency measurements (milliseconds)."""

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
        qs = quantiles(self.samples, n=100)
        self.p50 = qs[49]
        self.p95 = qs[94]
        self.p99 = qs[98]
        self.mean = mean(self.samples)
        self.std = stdev(self.samples) if len(self.samples) > 1 else 0.0
        self.min = min(self.samples)
        self.max = max(self.samples)

    def to_dict(self) -> dict[str, float]:
        return {
            "mean_ms": round(self.mean, 3),
            "std_ms": round(self.std, 3),
            "p50_ms": round(self.p50, 3),
            "p95_ms": round(self.p95, 3),
            "p99_ms": round(self.p99, 3),
            "min_ms": round(self.min, 3),
            "max_ms": round(self.max, 3),
            "n_samples": len(self.samples),
        }


class Timer:
    """Accumulator for latency measurements."""

    def __init__(self) -> None:
        self._samples: list[float] = []

    @contextmanager
    def measure(self) -> Generator[None, None, None]:
        """Context manager that records a single measurement."""
        start = time.perf_counter()
        try:
            yield
        finally:
            elapsed_ms = (time.perf_counter() - start) * 1000.0
            self._samples.append(elapsed_ms)

    def record(self, elapsed_ms: float) -> None:
        """Manually record an elapsed time in milliseconds."""
        self._samples.append(elapsed_ms)

    @property
    def stats(self) -> LatencyStats:
        return LatencyStats(samples=list(self._samples))

    def reset(self) -> None:
        self._samples.clear()

    def __len__(self) -> int:
        return len(self._samples)


@contextmanager
def timed(label: str = "") -> Generator[dict[str, float], None, None]:
    """One-shot context manager; yields a result dict filled after the block."""
    result: dict[str, float] = {}
    start = time.perf_counter()
    try:
        yield result
    finally:
        result["elapsed_ms"] = (time.perf_counter() - start) * 1000.0
        result["label"] = label  # type: ignore[assignment]
