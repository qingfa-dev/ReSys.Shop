from pathlib import Path

from benchmark.reporting.pipeline import write_pipeline_typst


def test_write_pipeline_typst(tmp_path: Path):
    results = [
        {
            "model_name": "FakeModel",
            "model_slug": "fake-model",
            "folds": [],
            "aggregate": {"map": {"mean": 0.8, "std": 0.01}},
            "production_metrics": {
                "index_build_time_s": {"mean": 0.5, "std": 0.1},
                "pgvector_query_latency_ms": {"mean": 12.3, "std": 1.2},
                "pgvector_recall@10": {"mean": 0.95, "std": 0.02},
                "ingestion_time_s": {"mean": 2.0, "std": 0.3},
            },
        }
    ]
    paths = write_pipeline_typst(results, output_dir=tmp_path)
    assert len(paths) == 1
    assert paths[0].exists()
    content = paths[0].read_text()
    assert "FakeModel" in content
    assert "0.95" in content
    assert "12.3" in content
