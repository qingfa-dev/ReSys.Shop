"""Tests for Typst table generation."""
from __future__ import annotations

import pytest

from benchmark.evaluation.evaluator import ModelMetrics
from benchmark.reporting.typst import (
    write_all_tables,
    write_latency_table,
    write_map_summary_table,
    write_ndcg_table,
    write_precision_table,
    write_recall_table,
)


def _make_metrics(name: str, map_score: float = 0.75) -> ModelMetrics:
    m = ModelMetrics(model_name=name, dataset="deepfashion", k_values=[1, 5, 10])
    m.map_score = map_score
    m.precision = {1: 0.9, 5: 0.7, 10: 0.6}
    m.recall    = {1: 0.1, 5: 0.35, 10: 0.6}
    m.ndcg      = {1: 0.9, 5: 0.72, 10: 0.65}
    m.latency   = {"p50_ms": 12.3, "p95_ms": 18.4, "p99_ms": 22.1}
    m.throughput_per_sec = 80.0
    return m


@pytest.fixture()
def two_models() -> list[ModelMetrics]:
    return [_make_metrics("FashionCLIP", 0.82), _make_metrics("SigLIP", 0.74)]


def test_write_all_tables_returns_five_files(tmp_path, two_models) -> None:
    paths = write_all_tables(two_models, k_values=[1, 5, 10], output_dir=tmp_path)
    assert len(paths) == 5
    names = {p.name for p in paths}
    assert names == {"precision.typ", "recall.typ", "ndcg.typ", "latency.typ", "map_summary.typ"}


def test_all_typst_files_exist_on_disk(tmp_path, two_models) -> None:
    paths = write_all_tables(two_models, k_values=[1, 5, 10], output_dir=tmp_path)
    for path in paths:
        assert path.exists(), f"{path.name} was not written"


def test_typst_contains_figure_and_table_tags(tmp_path, two_models) -> None:
    paths = write_all_tables(two_models, k_values=[1, 5, 10], output_dir=tmp_path)
    for path in paths:
        content = path.read_text()
        assert "#figure(" in content, f"{path.name} missing #figure"
        assert "table(" in content, f"{path.name} missing table("


def test_typst_contains_model_names(tmp_path, two_models) -> None:
    paths = write_all_tables(two_models, k_values=[1, 5, 10], output_dir=tmp_path)
    for path in paths:
        content = path.read_text()
        assert "FashionCLIP" in content, f"{path.name} missing FashionCLIP"
        assert "SigLIP" in content, f"{path.name} missing SigLIP"


def test_typst_has_auto_generated_comment(tmp_path, two_models) -> None:
    paths = write_all_tables(two_models, k_values=[1, 5, 10], output_dir=tmp_path)
    for path in paths:
        assert "Auto-generated" in path.read_text()


def test_precision_table_has_map_column(tmp_path, two_models) -> None:
    path = write_precision_table(two_models, k_values=[1, 5, 10], output_dir=tmp_path)
    assert "mAP" in path.read_text()


def test_map_summary_table_has_rank_column(tmp_path, two_models) -> None:
    path = write_map_summary_table(two_models, output_dir=tmp_path)
    content = path.read_text()
    assert "Rank" in content
    assert "[1]" in content  # rank 1 cell


def test_latency_table_has_throughput_column(tmp_path, two_models) -> None:
    path = write_latency_table(two_models, output_dir=tmp_path)
    assert "Throughput" in path.read_text()


def test_ndcg_table_file(tmp_path, two_models) -> None:
    path = write_ndcg_table(two_models, k_values=[1, 5, 10], output_dir=tmp_path)
    assert path.name == "ndcg.typ"
    content = path.read_text()
    assert "nDCG" in content


def test_recall_table_file(tmp_path, two_models) -> None:
    path = write_recall_table(two_models, k_values=[1, 5, 10], output_dir=tmp_path)
    assert path.name == "recall.typ"
    assert "Recall" in path.read_text()


def test_single_model_does_not_crash(tmp_path) -> None:
    single = [_make_metrics("OnlyModel")]
    paths = write_all_tables(single, k_values=[5, 10], output_dir=tmp_path)
    assert len(paths) == 5
