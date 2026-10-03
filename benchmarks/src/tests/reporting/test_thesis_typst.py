from pathlib import Path

from benchmark.reporting.typst import write_thesis_tables


def test_write_thesis_tables(tmp_path: Path):
    results = [
        {
            "model_name": "FakeModel",
            "model_slug": "fake-model",
            "folds": [
                {"fold": 0, "map": 0.8, "precision@5": 0.7, "precision@10": 0.6,
                 "recall@5": 0.5, "recall@10": 0.4, "latency_mean_ms": 10.0,
                 "throughput_per_sec": 100.0, "load_time_ms": 50.0,
                 "index_storage_mb": 5.0, "ram_mb": 200.0},
                {"fold": 1, "map": 0.82, "precision@5": 0.72, "precision@10": 0.62,
                 "recall@5": 0.52, "recall@10": 0.42, "latency_mean_ms": 11.0,
                 "throughput_per_sec": 95.0, "load_time_ms": 52.0,
                 "index_storage_mb": 5.1, "ram_mb": 205.0},
                {"fold": 2, "map": 0.81, "precision@5": 0.71, "precision@10": 0.61,
                 "recall@5": 0.51, "recall@10": 0.41, "latency_mean_ms": 10.5,
                 "throughput_per_sec": 98.0, "load_time_ms": 51.0,
                 "index_storage_mb": 5.05, "ram_mb": 202.0},
            ],
            "aggregate": {
                "map": {"mean": 0.81, "std": 0.01},
                "precision@5": {"mean": 0.71, "std": 0.01},
                "precision@10": {"mean": 0.61, "std": 0.01},
                "precision@20": {"mean": 0.51, "std": 0.01},
                "recall@5": {"mean": 0.51, "std": 0.01},
                "recall@10": {"mean": 0.41, "std": 0.01},
                "recall@20": {"mean": 0.31, "std": 0.01},
                "latency_mean_ms": {"mean": 10.5, "std": 0.5},
                "throughput_per_sec": {"mean": 97.7, "std": 2.5},
                "load_time_ms": {"mean": 51.0, "std": 1.0},
                "index_storage_mb": {"mean": 5.05, "std": 0.05},
                "ram_mb": {"mean": 202.3, "std": 2.5},
            },
        }
    ]
    paths = write_thesis_tables(results, output_dir=tmp_path)
    assert len(paths) == 2
    aggregate_path = [p for p in paths if "aggregate" in p.name][0]
    efficiency_path = [p for p in paths if "efficiency" in p.name][0]
    for p in paths:
        assert p.exists()
        content = p.read_text()
        assert "Auto-generated" in content
        assert "FakeModel" in content
    assert "0.81" in aggregate_path.read_text()
    assert "10.5" in efficiency_path.read_text()
