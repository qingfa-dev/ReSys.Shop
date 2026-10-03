"""Unit tests for nDCG@K metric."""
from __future__ import annotations

import math

import pytest

from benchmark.metrics.ndcg import dcg_at_k, ideal_dcg_at_k, ndcg_at_k


def test_perfect_ndcg_single_relevant() -> None:
    # Only "tops" is relevant, and it appears at rank 1 — perfect
    retrieved = ["tops", "shoes", "bottoms"]
    relevant = {"tops"}
    assert ndcg_at_k(retrieved, relevant, k=3) == pytest.approx(1.0)


def test_perfect_ndcg_multiple_relevant() -> None:
    # Both relevant labels at top-2 positions — ideal ordering
    retrieved = ["tops", "dresses", "shoes"]
    relevant = {"tops", "dresses"}
    assert ndcg_at_k(retrieved, relevant, k=2) == pytest.approx(1.0)


def test_zero_ndcg() -> None:
    retrieved = ["shoes", "shoes", "shoes"]
    relevant = {"tops"}
    assert ndcg_at_k(retrieved, relevant, k=3) == pytest.approx(0.0)


def test_ndcg_bounded_between_0_and_1() -> None:
    retrieved = ["tops", "shoes", "dresses", "bottoms"]
    relevant = {"tops", "dresses"}
    score = ndcg_at_k(retrieved, relevant, k=4)
    assert 0.0 <= score <= 1.0


def test_ndcg_hit_at_rank_2_vs_rank_1() -> None:
    # Hit at rank 2 is worse than hit at rank 1
    retrieved_good = ["tops", "shoes"]
    retrieved_bad  = ["shoes", "tops"]
    relevant = {"tops"}
    good = ndcg_at_k(retrieved_good, relevant, k=2)
    bad  = ndcg_at_k(retrieved_bad,  relevant, k=2)
    assert good > bad


def test_ndcg_empty_relevant_returns_zero() -> None:
    assert ndcg_at_k(["tops", "dresses"], set(), k=2) == pytest.approx(0.0)


def test_ideal_dcg_single_item() -> None:
    # Ideal: 1 relevant item at rank 1 → DCG = 1/log2(2) = 1.0
    assert ideal_dcg_at_k(1, k=3) == pytest.approx(1.0)


def test_ideal_dcg_two_items() -> None:
    # Ideal: 2 relevant items at ranks 1 and 2
    expected = 1 / math.log2(2) + 1 / math.log2(3)
    assert ideal_dcg_at_k(2, k=3) == pytest.approx(expected, rel=1e-6)


def test_dcg_at_k_hit_at_rank1() -> None:
    score = dcg_at_k(["tops", "shoes"], {"tops"}, k=2)
    assert score == pytest.approx(1 / math.log2(2))
