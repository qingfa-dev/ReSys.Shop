from pathlib import Path
from unittest.mock import MagicMock, patch

import numpy as np
import pytest

from benchmark.evaluation.thesis import ThesisRunner


@pytest.fixture
def mock_dataset_root(tmp_path: Path) -> Path:
    root = tmp_path / "dataset"
    images = root / "images"
    images.mkdir(parents=True)
    # Create a few dummy images
    from PIL import Image
    for i in range(20):
        img = Image.new("RGB", (32, 32), color=(i, i, i))
        img.save(images / f"{i}.jpg")
    # Create styles.csv
    import pandas as pd
    df = pd.DataFrame({
        "id": [str(i) for i in range(20)],
        "masterCategory": ["A"] * 10 + ["B"] * 10,
        "subCategory": ["X"] * 20,
        "baseColour": ["Black"] * 20,
        "image": [f"{i}.jpg" for i in range(20)],
    })
    df.to_csv(root / "styles.csv", index=False)
    return root


def test_thesis_runner_runs_all_folds(mock_dataset_root: Path, tmp_path: Path):
    """Integration-style test with mocked model to avoid heavy torch loading."""
    output_dir = tmp_path / "outputs"

    # Mock the registry to return a lightweight fake model
    fake_model = MagicMock()
    fake_model.name = "FakeModel"
    fake_model.slug = "fake-model"
    fake_model.load = MagicMock()
    fake_model.embed_batch = MagicMock(return_value=np.random.rand(2, 64).astype(np.float32))
    fake_model.embed = MagicMock(return_value=np.random.rand(64).astype(np.float32))

    with patch("benchmark.evaluation.thesis.get_registry") as mock_registry:
        mock_registry.return_value = {"fake-model": fake_model}
        runner = ThesisRunner(
            dataset_root=mock_dataset_root,
            output_dir=output_dir,
            folds=3,
            seed=42,
            device="cpu",
            use_cache=False,
            batch_size=2,
        )
        results = runner.run(model_keys=["fake-model"])

    assert len(results) == 1
    result = results[0]
    assert result["model_name"] == "FakeModel"
    assert result["model_slug"] == "fake-model"
    assert len(result["folds"]) == 3
    assert "aggregate" in result
    assert "map" in result["aggregate"]


def test_thesis_runner_missing_model_skipped(mock_dataset_root: Path, tmp_path: Path):
    runner = ThesisRunner(
        dataset_root=mock_dataset_root,
        output_dir=tmp_path / "outputs",
        folds=3,
        seed=42,
        device="cpu",
        use_cache=False,
    )
    with patch("benchmark.evaluation.thesis.get_registry") as mock_registry:
        mock_registry.return_value = {}
        results = runner.run(model_keys=["nonexistent"])
    assert results == []


def test_thesis_runner_missing_styles_csv_raises(tmp_path: Path):
    runner = ThesisRunner(
        dataset_root=tmp_path,
        output_dir=tmp_path / "outputs",
        folds=3,
    )
    with pytest.raises(FileNotFoundError, match="styles.csv not found"):
        runner.run(model_keys=["fake-model"])
