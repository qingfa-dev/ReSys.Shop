import pytest

from benchmark.utils.timing import LatencyStats


def test_latency_stats_std():
    stats = LatencyStats(samples=[10.0, 20.0, 30.0])
    assert hasattr(stats, "std")
    assert stats.std > 0
    d = stats.to_dict()
    assert "std_ms" in d
    assert d["std_ms"] > 0


def test_latency_stats_std_single_sample():
    # quantiles() requires ≥2 samples — this is a pre-existing limitation
    with pytest.raises(Exception):
        LatencyStats(samples=[42.0])


def test_latency_stats_std_two_samples():
    stats = LatencyStats(samples=[40.0, 44.0])
    assert stats.std > 0
    assert stats.to_dict()["std_ms"] > 0


def test_latency_stats_empty_raises():
    with pytest.raises(ValueError):
        LatencyStats(samples=[])
