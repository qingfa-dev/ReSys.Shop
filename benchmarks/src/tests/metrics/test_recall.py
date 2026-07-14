"""Unit tests for Recall@K metric."""
from __future__ import annotations

import pytest

from benchmark.metrics.recall import mean_recall_at_k, recall_at_k


def test_perfect_recall_single_relevant() -> None:
    # relevant = {"tops"} — size 1. One hit in top-1.
    retrieved = ["tops", "shoes", "bottoms"]
    relevant = {"tops"}
    assert recall_at_k(retrieved, relevant, k=1) == pytest.approx(1.0)


def test_perfect_recall_two_relevant() -> None:
    # relevant = {"tops", "dresses"} — size 2. Both appear in top-2.
    retrieved = ["tops", "dresses", "shoes"]
    relevant = {"tops", "dresses"}
    assert recall_at_k(retrieved, relevant, k=2) == pytest.approx(1.0)


def test_zero_recall() -> None:
    retrieved = ["shoes", "bottoms", "dresses"]
    relevant = {"tops"}
    assert recall_at_k(retrieved, relevant, k=3) == pytest.approx(0.0)


def test_empty_relevant_returns_zero() -> None:
    retrieved = ["tops", "bottoms"]
    relevant: set[str] = set()
    assert recall_at_k(retrieved, relevant, k=2) == pytest.approx(0.0)


def test_partial_recall() -> None:
    # relevant = {"tops", "dresses"} — 2 unique labels
    # Only "tops" appears in top-3
    retrieved = ["tops", "shoes", "shoes", "dresses"]
    relevant = {"tops", "dresses"}
    assert recall_at_k(retrieved, relevant, k=3) == pytest.approx(0.5)


def test_recall_increases_with_k() -> None:
    retrieved = ["tops", "shoes", "dresses"]
    relevant = {"tops", "dresses"}
    r1 = recall_at_k(retrieved, relevant, k=1)
    r3 = recall_at_k(retrieved, relevant, k=3)
    assert r3 >= r1


def test_mean_recall_perfect() -> None:
    # Each relevant label appears exactly once in the top-K, giving recall = 1.0 per query
    all_retrieved = [
        ["tops", "bottoms", "dresses"],    # "tops" ∈ relevant → 1 hit / 1 relevant = 1.0
        ["shoes", "bottoms", "dresses"],   # "shoes" ∈ relevant → 1 hit / 1 relevant = 1.0
    ]
    all_relevant = [{"tops"}, {"shoes"}]
    mr = mean_recall_at_k(all_retrieved, all_relevant, k=3)
    assert mr == pytest.approx(1.0)


def test_mean_recall_empty_inputs() -> None:
    assert mean_recall_at_k([], [], k=5) == pytest.approx(0.0)
