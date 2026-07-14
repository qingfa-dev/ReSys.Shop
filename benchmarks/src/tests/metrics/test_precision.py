"""Unit tests for Precision@K metric."""
from __future__ import annotations

import pytest

from benchmark.metrics.precision import mean_precision_at_k, precision_at_k


def test_perfect_precision() -> None:
    # All top-3 are the relevant label "tops"
    retrieved = ["tops", "tops", "tops", "bottoms"]
    relevant = {"tops"}
    assert precision_at_k(retrieved, relevant, k=3) == pytest.approx(1.0)


def test_zero_precision() -> None:
    retrieved = ["bottoms", "shoes", "dresses"]
    relevant = {"tops"}
    assert precision_at_k(retrieved, relevant, k=3) == pytest.approx(0.0)


def test_partial_precision() -> None:
    retrieved = ["tops", "bottoms", "tops", "shoes"]
    relevant = {"tops"}
    # 2 hits in top 4 → 2/4 = 0.5
    assert precision_at_k(retrieved, relevant, k=4) == pytest.approx(0.5)


def test_precision_at_1_hit() -> None:
    retrieved = ["tops", "bottoms"]
    relevant = {"tops"}
    assert precision_at_k(retrieved, relevant, k=1) == pytest.approx(1.0)


def test_precision_at_1_miss() -> None:
    retrieved = ["bottoms", "tops"]
    relevant = {"tops"}
    assert precision_at_k(retrieved, relevant, k=1) == pytest.approx(0.0)


def test_precision_k_zero() -> None:
    assert precision_at_k(["tops"], {"tops"}, k=0) == pytest.approx(0.0)


def test_mean_precision_at_k_perfect() -> None:
    # Both queries have all-relevant top-3 results
    all_retrieved = [
        ["tops", "tops", "tops"],
        ["shoes", "shoes", "shoes"],
    ]
    all_relevant = [{"tops"}, {"shoes"}]
    mp = mean_precision_at_k(all_retrieved, all_relevant, k=3)
    assert mp == pytest.approx(1.0)


def test_mean_precision_at_k_mixed() -> None:
    all_retrieved = [
        ["tops", "bottoms", "tops"],  # 2/3
        ["shoes", "tops", "shoes"],   # 2/3
    ]
    all_relevant = [{"tops"}, {"shoes"}]
    mp = mean_precision_at_k(all_retrieved, all_relevant, k=3)
    assert mp == pytest.approx(2 / 3)


def test_mean_precision_empty_inputs() -> None:
    assert mean_precision_at_k([], [], k=5) == pytest.approx(0.0)
