import json
from pathlib import Path

from benchmark.datasets.loader import FashionDataset


def test_load_with_custom_label_field(tmp_path: Path):
    split_file = tmp_path / "split.json"
    split_file.write_text(json.dumps([
        {"image_path": "images/1.jpg", "label": "Topwear/Blue",
         "label_pattern": "Topwear/Blue/Checked", "product_id": "1"},
        {"image_path": "images/2.jpg", "label": "Topwear/Blue",
         "label_pattern": "Topwear/Blue/Solid", "product_id": "2"},
    ]))

    dataset_root = tmp_path / "images"
    dataset_root.mkdir()

    ds1 = FashionDataset(dataset_root=tmp_path, split_file=split_file, split="test")
    ds1.load()
    assert ds1.samples[0].label == "Topwear/Blue"

    ds2 = FashionDataset(dataset_root=tmp_path, split_file=split_file, split="test")
    ds2.load(label_field="label_pattern")
    assert ds2.samples[0].label == "Topwear/Blue/Checked"
    assert ds2.samples[1].label == "Topwear/Blue/Solid"
