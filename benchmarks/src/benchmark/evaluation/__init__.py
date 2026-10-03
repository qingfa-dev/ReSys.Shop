"""Evaluation pipeline: evaluator, benchmark runner, and cross-model comparison.

Exports the primary evaluation interfaces: ``Evaluator`` for per-query scoring,
``BenchmarkRunner`` for one-shot model comparison, ``rank_models`` and
``comparison_table`` for leaderboard construction."""

from benchmark.evaluation.benchmark import BenchmarkRunner
from benchmark.evaluation.comparison import comparison_table, rank_models
from benchmark.evaluation.evaluator import Evaluator, ModelMetrics

__all__ = [
    "Evaluator",
    "ModelMetrics",
    "BenchmarkRunner",
    "rank_models",
    "comparison_table",
]
