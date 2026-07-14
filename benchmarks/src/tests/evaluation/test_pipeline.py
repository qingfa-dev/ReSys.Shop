import json
from pathlib import Path
from unittest.mock import MagicMock, patch

import numpy as np
import pytest

from benchmark.evaluation.pipeline import PipelineRunner


def test_pipeline_runner_runs_all_folds(tmp_path: Path):
    """Integration-style test with mocked DB to avoid heavy torch + postgres."""
    dataset_root = tmp_path / "dataset"
    images = dataset_root / "images"
    images.mkdir(parents=True)
    from PIL import Image

    for i in range(20):
        img = Image.new("RGB", (32, 32), color=(i, i, i))
        img.save(images / f"{i}.jpg")
    import pandas as pd

    df = pd.DataFrame(
        {
            "id": [str(i) for i in range(20)],
            "masterCategory": ["A"] * 10 + ["B"] * 10,
            "subCategory": ["X"] * 20,
            "articleType": ["Shirt"] * 20,
            "image": [f"{i}.jpg" for i in range(20)],
        }
    )
    df.to_csv(dataset_root / "styles.csv", index=False)

    fake_model = MagicMock()
    fake_model.name = "FakeModel"
    fake_model.slug = "fake-model"
    fake_model.load = MagicMock()
    fake_model.embed_batch = MagicMock(return_value=np.random.rand(2, 64).astype(np.float32))
    fake_model.embed = MagicMock(return_value=np.random.rand(64).astype(np.float32))
    fake_model.embedding_dim = 64

    with patch("benchmark.evaluation.pipeline.get_registry") as mock_registry, patch(
        "benchmark.evaluation.pipeline.PgvectorRetriever"
    ) as mock_pg:
        mock_registry.return_value = {"fake-model": fake_model}
        mock_pg_instance = MagicMock()
        mock_pg_instance.connect = MagicMock()
        mock_pg_instance.close = MagicMock()
        mock_pg_instance.upsert_batch = MagicMock()
        mock_pg_instance.clear_table = MagicMock()
        mock_pg_instance.build_index = MagicMock(return_value=0.5)
        mock_pg_instance.query = MagicMock(
            return_value=[
                {"id": "1", "label": "Shirt", "score": 0.9},
                {"id": "2", "label": "Shirt", "score": 0.8},
            ]
        )
        mock_pg.return_value = mock_pg_instance

        runner = PipelineRunner(
            dataset_root=dataset_root,
            output_dir=tmp_path / "outputs",
            folds=3,
            seed=42,
            device="cpu",
            use_cache=False,
            batch_size=2,
            conn_string="postgresql://fake",
        )
        results = runner.run(model_keys=["fake-model"])

    assert len(results) == 1
    result = results[0]
    assert result["model_name"] == "FakeModel"
    assert len(result["folds"]) == 3
    assert "production_metrics" in result
    assert "index_build_time_s" in result["production_metrics"]
