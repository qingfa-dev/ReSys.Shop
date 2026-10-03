"""Utility helpers: logging, timing, device, and reproducibility.

Convenience re-exports for the most commonly used utility functions and
types across the benchmark codebase.
"""

from benchmark.utils.device import resolve_device
from benchmark.utils.logging import get_logger, setup_logging
from benchmark.utils.random_seed import set_seed
from benchmark.utils.timing import LatencyStats, Timer, timed

__all__ = [
    "resolve_device",
    "get_logger",
    "setup_logging",
    "set_seed",
    "LatencyStats",
    "Timer",
    "timed",
]
