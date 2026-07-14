import numpy as np
import pytest

from benchmark.evaluation.stats import aggregate_mean_std, bootstrap_ci, cohens_d


def test_aggregate_mean_std():
    values = [0.80, 0.82, 0.81]
    result = aggregate_mean_std(values)
    assert result["mean"] == pytest.approx(0.81, abs=0.01)
    assert result["std"] > 0


def test_cohens_d():
    # Fashion-CLIP clearly better
    a = [0.82, 0.83, 0.81]
    b = [0.70, 0.71, 0.69]
    d = cohens_d(a, b)
    assert d > 1.0  # large effect


def test_bootstrap_ci():
    np.random.seed(42)
    samples = [0.80, 0.82, 0.81]
    ci = bootstrap_ci(samples, n_resamples=1000)
    assert ci[0] <= ci[1]  # lower <= upper
    assert ci[0] <= 0.82 <= ci[1]  # mean inside or on boundary


def test_cohens_d_unequal_length_raises():
    with pytest.raises(ValueError, match="same length"):
        cohens_d([0.8, 0.9], [0.7])
