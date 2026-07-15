"""Unit tests for all retrieval metrics."""
from __future__ import annotations

import pytest

from benchmark.metrics.map import average_precision, mean_average_precision
from benchmark.metrics.ndcg import ndcg_at_k
from benchmark.metrics.precision import mean_precision_at_k, precision_at_k
from benchmark.metrics.recall import mean_recall_at_k, recall_at_k

# ── fixtures ──────────────────────────────────────────────────────────────────

@pytest.fixture()
def perfect_retrieval():
    """All top-K results are relevant."""
    relevant = {"cat", "dog"}
    retrieved = ["cat", "dog", "fish", "bird", "snake"]
    return retrieved, relevant


@pytest.fixture()
def empty_relevant():
    retrieved = ["cat", "dog"]
    relevant: set[str] = set()
    return retrieved, relevant


# ── Precision@K ───────────────────────────────────────────────────────────────

def test_precision_perfect(perfect_retrieval):
    retrieved, relevant = perfect_retrieval
    assert precision_at_k(retrieved, relevant, k=2) == 1.0


def test_precision_zero_when_no_match():
    assert precision_at_k(["fish", "bird"], {"cat"}, k=2) == 0.0


def test_precision_partial():
    # 1 hit in top 4
    assert precision_at_k(["cat", "fish", "bird", "snake"], {"cat"}, k=4) == pytest.approx(0.25)


def test_precision_k_zero():
    assert precision_at_k(["cat"], {"cat"}, k=0) == 0.0


def test_precision_empty_relevant(empty_relevant):
    retrieved, relevant = empty_relevant
    assert precision_at_k(retrieved, relevant, k=2) == 0.0


def test_mean_precision_at_k():
    all_r = [["a", "b", "c"], ["d", "e", "f"]]
    all_rel = [{"a", "b"}, {"d"}]
    # query1: 2/3, query2: 1/3
    result = mean_precision_at_k(all_r, all_rel, k=3)
    assert result == pytest.approx((2 / 3 + 1 / 3) / 2)


# ── Recall@K ──────────────────────────────────────────────────────────────────

def test_recall_perfect(perfect_retrieval):
    retrieved, relevant = perfect_retrieval
    assert recall_at_k(retrieved, relevant, k=2) == 1.0


def test_recall_partial():
    # 1 of 3 relevant items found in top-2
    assert recall_at_k(["cat", "fish", "dog", "bird"], {"cat", "dog", "snake"}, k=2) == pytest.approx(1 / 3)


def test_recall_empty_relevant(empty_relevant):
    retrieved, relevant = empty_relevant
    assert recall_at_k(retrieved, relevant, k=2) == 0.0


def test_mean_recall_at_k():
    all_r = [["a", "b"], ["d", "e"]]
    all_rel = [{"a", "b", "c"}, {"d"}]
    # query1: 2/3, query2: 1/1
    result = mean_recall_at_k(all_r, all_rel, k=2)
    assert result == pytest.approx((2 / 3 + 1.0) / 2)


# ── mAP ───────────────────────────────────────────────────────────────────────

def test_ap_perfect():
    retrieved = ["a", "b", "c"]
    relevant = {"a", "b", "c"}
    # AP = (1/1 + 2/2 + 3/3) / 3 = 1.0
    assert average_precision(retrieved, relevant) == pytest.approx(1.0)


def test_ap_single_hit_at_rank3():
    # Only rank 3 is relevant — precision at that rank is 1/3
    retrieved = ["x", "y", "a"]
    relevant = {"a"}
    assert average_precision(retrieved, relevant) == pytest.approx(1 / 3)


def test_ap_empty_relevant():
    assert average_precision(["a", "b"], set()) == 0.0


def test_map():
    all_r   = [["a", "b", "c"], ["d", "e"]]
    all_rel = [{"a", "b", "c"}, {"d"}]
    # AP1 = 1.0, AP2 = 1.0 → mAP = 1.0
    assert mean_average_precision(all_r, all_rel) == pytest.approx(1.0)


def test_map_empty():
    assert mean_average_precision([], []) == 0.0


# ── nDCG@K ────────────────────────────────────────────────────────────────────

def test_ndcg_perfect():
    retrieved = ["a", "b", "c"]
    relevant = {"a", "b", "c"}
    assert ndcg_at_k(retrieved, relevant, k=3) == pytest.approx(1.0)


def test_ndcg_worst_case():
    # Relevant items at the very end — low nDCG
    retrieved = ["x", "y", "a"]
    relevant = {"a"}
    # DCG  = 1/log2(4) ≈ 0.5
    # IDCG = 1/log2(2) = 1.0
    import math
    expected = (1 / math.log2(4)) / (1 / math.log2(2))
    assert ndcg_at_k(retrieved, relevant, k=3) == pytest.approx(expected, rel=1e-4)


def test_ndcg_empty_relevant():
    assert ndcg_at_k(["a", "b"], set(), k=2) == 0.0
