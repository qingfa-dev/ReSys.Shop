"""Statistical analysis for thesis benchmark results.

Computes summary statistics, effect sizes (Cohen's d), and bootstrap confidence
intervals. Implements these manually to avoid adding a scipy dependency.

Edge cases:
- Empty input lists return safe defaults (mean=0, std=0, CI=(0,0)).
- Single-element samples return std=0 for mean-std aggregation.
- Cohen's d requires paired equal-length groups; fewer than 2 differences
  or zero variance returns 0.0 (no measurable effect).
"""
from __future__ import annotations

from statistics import mean, stdev

import numpy as np

from benchmark._constants import MAGIC


def aggregate_mean_std(values: list[float]) -> dict[str, float]:
    """Compute mean ± standard deviation for a list of fold-level values.

    Args:
        values: Fold-level metric scores. Empty list produces (0.0, 0.0).

    Returns:
        Dict with ``mean`` and ``std`` keys, each rounded to 4 decimal places.
        Standard deviation is 0.0 for single-element lists.
    """
    if not values:
        return {"mean": 0.0, "std": 0.0}
    m = mean(values)
    s = stdev(values) if len(values) > 1 else 0.0
    return {"mean": round(m, MAGIC.METRIC_DECIMALS), "std": round(s, MAGIC.METRIC_DECIMALS)}


def cohens_d(group_a: list[float], group_b: list[float]) -> float:
    """Compute Cohen's d for paired samples (effect size).

    Uses the standard deviation of the paired differences.
    Returns 0.0 when the difference standard deviation is zero
    or when fewer than 2 paired differences are available.

    Args:
        group_a: First group of observations.
        group_b: Second group of observations (paired with group_a).

    Returns:
        Cohen's d effect size. 0.0 if variance is zero or sample is too small.

    Raises:
        ValueError: If groups have different lengths.
    """
    if len(group_a) != len(group_b):
        raise ValueError("Groups must have the same length for paired Cohen's d")
    differences = [a - b for a, b in zip(group_a, group_b, strict=True)]
    if len(differences) < 2:
        return 0.0
    d_mean = mean(differences)
    d_std = stdev(differences)
    if d_std == 0:
        return 0.0
    return d_mean / d_std


def bootstrap_ci(
    samples: list[float],
    confidence: float = MAGIC.BOOTSTRAP_CONFIDENCE,
    n_resamples: int = MAGIC.BOOTSTRAP_RESAMPLES,
    seed: int | None = None,
) -> tuple[float, float]:
    """Compute bootstrap confidence interval for the mean.

    Resamples with replacement from the observed samples to estimate the
    sampling distribution of the mean, then returns percentiles at the
    requested confidence level.

    Args:
        samples: Observed values (e.g., fold-level mAP scores).
        confidence: Confidence level (default 0.95 for 95% CI).
        n_resamples: Number of bootstrap resamples.
        seed: Random seed for reproducibility.

    Returns:
        Tuple (lower_bound, upper_bound), each rounded to 4 decimal places.
        Returns (0.0, 0.0) for empty input.
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
        round(float(np.percentile(boot_means, lower * 100)), MAGIC.METRIC_DECIMALS),
        round(float(np.percentile(boot_means, upper * 100)), MAGIC.METRIC_DECIMALS),
    )
