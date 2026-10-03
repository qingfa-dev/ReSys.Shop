"""Unit tests for approximate recall@K comparison metric."""
from __future__ import annotations

import numpy as np

from benchmark.metrics.recall_comparison import approximate_recall_at_k


def test_perfect_recall():
    exact = np.array([[0, 1, 2], [3, 4, 5]])
    approx = np.array([[0, 1, 2], [3, 4, 5]])
    result = approximate_recall_at_k(approx, exact, k_values=[1, 2, 3])
    assert result[1] == 1.0
    assert result[2] == 1.0
    assert result[3] == 1.0


def test_zero_recall():
    exact = np.array([[0, 1, 2], [3, 4, 5]])
    approx = np.array([[6, 7, 8], [9, 10, 11]])
    result = approximate_recall_at_k(approx, exact, k_values=[1, 2, 3])
    assert result[1] == 0.0
    assert result[2] == 0.0
    assert result[3] == 0.0


def test_partial_recall():
    exact = np.array([[0, 1, 2], [3, 4, 5]])
    approx = np.array([[0, 7, 8], [9, 4, 5]])
    result = approximate_recall_at_k(approx, exact, k_values=[1, 2, 3])
    assert result[1] == 0.5  # one out of two correct @1
    assert result[2] == 0.5  # still one correct @2
    assert result[3] == 0.5  # mean of 1/3 and 2/3
