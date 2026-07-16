import json
from pathlib import Path
from unittest.mock import MagicMock, patch

import numpy as np
import pytest

from benchmark.evaluation.thesis import ThesisRunner


@pytest.fixture
def dual_label_dataset(tmp_path: Path) -> Path:
    root = tmp_path / "ds"
    root.mkdir()
    images_dir = root / "images"
    images_dir.mkdir(parents=True)
    from PIL import Image
    for i in range(20):
        img = Image.new("RGB", (32, 32), color=(i, i, i))
        img.save(images_dir / f"{i}.jpg")
    import pandas as pd
    df = pd.DataFrame({
        "id": [str(i) for i in range(20)],
        "masterCategory": ["A"] * 10 + ["B"] * 10,
        "subCategory": ["X"] * 20,
        "baseColour": ["Black"] * 20,
    })
    df.to_csv(root / "styles.csv", index=False)

    splits_dir = root / "splits"
    splits_dir.mkdir()
    split_paths: list[tuple[Path, Path]] = []
    for fold in range(3):
        train_path = splits_dir / f"fold_{fold}_train.json"
        test_path = splits_dir / f"fold_{fold}_test.json"
        for split_name, ids in [("train", [str(i) for i in range(14)]),
                                  ("test", [str(i) for i in range(14, 20)])]:
            samples = [{
                "image_path": f"images/{pid}.jpg",
                "label": "X/Black",
                "label_pattern": "X/Black/Solid",
                "product_id": pid,
            } for pid in ids]
            path = splits_dir / f"fold_{fold}_{split_name}.json"
            path.write_text(json.dumps(samples))
        split_paths.append((train_path, test_path))
    return root, split_paths


def test_thesis_runner_secondary_label_writes_pattern_file(dual_label_dataset, tmp_path: Path):
    root, split_paths = dual_label_dataset
    fake_model = MagicMock()
    fake_model.name = "Fake"
    fake_model.slug = "fake"
    fake_model.load = MagicMock()
    fake_model.embed_batch = MagicMock(return_value=np.random.rand(2, 64).astype(np.float32))
    fake_model.embed = MagicMock(return_value=np.random.rand(64).astype(np.float32))

    output_dir = tmp_path / "output"
    with patch("benchmark.evaluation.thesis.get_registry") as mock_reg, \
         patch("benchmark.evaluation.thesis.GroundTruth") as mock_gt_class:
        mock_reg.return_value = {"fake": fake_model}
        mock_gt = MagicMock()
        mock_gt.generate_splits.return_value = split_paths
        mock_gt_class.return_value = mock_gt
        runner = ThesisRunner(
            dataset_root=root,
            output_dir=output_dir,
            folds=3, seed=42, device="cpu",
            use_cache=False, batch_size=2,
            secondary_label="label_pattern",
        )
        results = runner.run(model_keys=["fake"])

    assert len(results) == 1
    secondary = output_dir / "results" / "thesis_results_pattern.json"
    assert secondary.exists()
    secondary_data = json.loads(secondary.read_text())
    assert len(secondary_data) == 1
    assert secondary_data[0]["model_name"] == "Fake"
