"""Tests for report writers using synthetic ModelMetrics."""
from __future__ import annotations

import json
from pathlib import Path

import pytest

from benchmark.evaluation.evaluator import ModelMetrics
from benchmark.reporting.csv import write_csv
from benchmark.reporting.json import write_comparison_json, write_model_json
from benchmark.reporting.markdown import write_markdown
from benchmark.reporting.typst import write_all_tables


def _make_metrics(name: str, map_score: float = 0.75) -> ModelMetrics:
    m = ModelMetrics(model_name=name, dataset="test", k_values=[1, 5, 10])
    m.map_score = map_score
    m.precision = {1: 0.90, 5: 0.75, 10: 0.60}
    m.recall    = {1: 0.10, 5: 0.40, 10: 0.60}
    m.ndcg      = {1: 0.90, 5: 0.80, 10: 0.72}
    m.latency   = {"p50_ms": 12.3, "p95_ms": 18.5, "p99_ms": 22.1}
    m.throughput_per_sec = 82.4
    return m


@pytest.fixture()
def sample_metrics():
    return [
        _make_metrics("FashionCLIP",      map_score=0.82),
        _make_metrics("CLIP ViT-B/32",    map_score=0.74),
        _make_metrics("SigLIP ViT-B/16",  map_score=0.79),
    ]


def test_write_model_json(tmp_path, sample_metrics):
    path = write_model_json(sample_metrics[0], output_dir=tmp_path)
    assert path.exists()
    data = json.loads(path.read_text())
    assert data["model"] == "FashionCLIP"
    assert "map" in data
    assert "precision" in data
    assert "recall" in data


def test_write_comparison_json(tmp_path, sample_metrics):
    path = write_comparison_json(sample_metrics, output_dir=tmp_path)
    assert path.exists()
    data = json.loads(path.read_text())
    assert len(data) == 3
    assert all("model" in row for row in data)


def test_write_csv(tmp_path, sample_metrics):
    path = write_csv(sample_metrics, k_values=[1, 5, 10], output_dir=tmp_path)
    assert path.exists()
    text = path.read_text()
    assert "FashionCLIP" in text
    assert "map" in text


def test_write_markdown(tmp_path, sample_metrics):
    path = write_markdown(sample_metrics, k_values=[1, 5, 10], output_dir=tmp_path)
    assert path.exists()
    text = path.read_text()
    assert "FashionCLIP" in text
    assert "mAP" in text
    assert "Latency" in text


def test_write_all_typst_tables(tmp_path, sample_metrics):
    paths = write_all_tables(sample_metrics, k_values=[1, 5, 10], output_dir=tmp_path)
    assert len(paths) == 5
    names = {p.name for p in paths}
    assert "precision.typ" in names
    assert "recall.typ" in names
    assert "ndcg.typ" in names
    assert "latency.typ" in names
    assert "map_summary.typ" in names


def test_typst_tables_not_empty(tmp_path, sample_metrics):
    paths = write_all_tables(sample_metrics, k_values=[1, 5, 10], output_dir=tmp_path)
    for p in paths:
        text = p.read_text()
        assert "#figure(" in text
        assert "FashionCLIP" in text


def test_typst_has_auto_gen_comment(tmp_path, sample_metrics):
    paths = write_all_tables(sample_metrics, k_values=[1, 5, 10], output_dir=tmp_path)
    for p in paths:
        assert "Auto-generated" in p.read_text()
