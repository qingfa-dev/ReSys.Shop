"""Research extension helpers for the benchmark package."""

from benchmark.research.datasets import ResearchDataset
from benchmark.research.db import PgvectorBenchmark
from benchmark.research.evaluation import (
    evaluate_features,
    evaluate_retrieval,
)
from benchmark.research.feature_extraction import (
    extract_and_save_features,
    load_research_features,
)
from benchmark.research.reports import generate_research_report

__all__ = [
    "ResearchDataset",
    "PgvectorBenchmark",
    "evaluate_features",
    "evaluate_retrieval",
    "extract_and_save_features",
    "load_research_features",
    "generate_research_report",
]
