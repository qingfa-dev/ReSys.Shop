"""Statistical analysis for thesis benchmark results.

Implements Cohen's d and bootstrap confidence intervals manually to avoid
a scipy dependency.
"""
from __future__ import annotations

import math
from statistics import mean, stdev

import numpy as np


def aggregate_mean_std(values: list[float]) -> dict[str, float]:
    """Compute mean ± SD for a list of fold-level values."""
    if not values:
        return {"mean": 0.0, "std": 0.0}
    m = mean(values)
    s = stdev(values) if len(values) > 1 else 0.0
    return {"mean": round(m, 4), "std": round(s, 4)}


def cohens_d(group_a: list[float], group_b: list[float]) -> float:
    """Compute Cohen's d for paired samples (effect size).

    Uses the standard deviation of the differences.
    """
    if len(group_a) != len(group_b):
        raise ValueError("Groups must have the same length for paired Cohen's d")
    differences = [a - b for a, b in zip(group_a, group_b)]
    if len(differences) < 2:
        return 0.0
    d_mean = mean(differences)
    d_std = stdev(differences)
    if d_std == 0:
        return 0.0
    return d_mean / d_std


def bootstrap_ci(
    samples: list[float],
    confidence: float = 0.95,
    n_resamples: int = 10_000,
    seed: int | None = None,
) -> tuple[float, float]:
    """Compute bootstrap confidence interval for the mean.

    Args:
        samples: Observed values (e.g., fold-level mAP scores).
        confidence: Confidence level (default 0.95 for 95% CI).
        n_resamples: Number of bootstrap resamples.
        seed: Random seed for reproducibility.

    Returns:
        Tuple (lower_bound, upper_bound).
    """
    if not samples:
        return (0.0, 0.0)
    rng = np.random.default_rng(seed)
    arr = np.array(samples)
    boot_means = np.empty(n_resamples)
    for i in range(n_resamples):
        resample = rng.choice(arr, size=len(arr), replace=True)
        boot_means[i] = resample.mean()
    lower = (1 - confidence) / 2
    upper = 1 - lower
    return (
        round(float(np.percentile(boot_means, lower * 100)), 4),
        round(float(np.percentile(boot_means, upper * 100)), 4),
    )
