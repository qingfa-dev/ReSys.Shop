"""Tests for the dataset loader using a temporary JSON split file."""
from __future__ import annotations

import json
from pathlib import Path

import pytest
from PIL import Image

from benchmark.datasets.loader import FashionDataset, Sample
from benchmark.datasets.validators import validate_dataset


@pytest.fixture()
def tmp_dataset(tmp_path):
    """Create a minimal valid dataset on disk."""
    img_dir = tmp_path / "images"
    img_dir.mkdir()
    # Create 4 tiny images
    for i in range(4):
        img = Image.new("RGB", (32, 32), color=(i * 60, 120, 200))
        img.save(img_dir / f"img_{i:04d}.jpg")

    split = [
        {"image_path": f"images/img_{i:04d}.jpg", "label": f"class_{i % 2}", "product_id": f"pid_{i}"}
        for i in range(4)
    ]
    split_file = tmp_path / "test.json"
    split_file.write_text(json.dumps(split), encoding="utf-8")

    return tmp_path, split_file


def test_load_dataset(tmp_dataset):
    root, split_file = tmp_dataset
    ds = FashionDataset(dataset_root=root, split_file=split_file, split="test")
    ds.load()
    assert len(ds) == 4


def test_sample_fields(tmp_dataset):
    root, split_file = tmp_dataset
    ds = FashionDataset(dataset_root=root, split_file=split_file, split="test")
    ds.load()
    for s in ds.samples:
        assert isinstance(s, Sample)
        assert s.image_path.exists()
        assert s.label.startswith("class_")
        assert s.product_id.startswith("pid_")
        assert s.split == "test"


def test_iter_images(tmp_dataset):
    root, split_file = tmp_dataset
    ds = FashionDataset(dataset_root=root, split_file=split_file, split="test")
    ds.load()
    images = list(ds.iter_images())
    assert len(images) == 4
    for _sample, img in images:
        assert img.mode == "RGB"


def test_validate_passes(tmp_dataset):
    root, split_file = tmp_dataset
    ds = FashionDataset(dataset_root=root, split_file=split_file, split="test")
    ds.load()
    errors = validate_dataset(ds)
    assert errors == []


def test_validate_missing_images(tmp_dataset):
    root, split_file = tmp_dataset
    split = [{"image_path": "images/does_not_exist.jpg", "label": "x", "product_id": "p0"}]
    split_file.write_text(json.dumps(split), encoding="utf-8")
    ds = FashionDataset(dataset_root=root, split_file=split_file, split="test")
    ds.load()
    errors = validate_dataset(ds)
    assert len(errors) == 1


def test_samples_not_accessible_before_load():
    ds = FashionDataset(dataset_root=Path("."), split_file=Path("nonexistent.json"))
    with pytest.raises(RuntimeError):
        _ = ds.samples
