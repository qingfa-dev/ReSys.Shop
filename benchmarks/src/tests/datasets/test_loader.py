"""Tests for dataset loading utilities."""

from __future__ import annotations

import json
import tempfile
from pathlib import Path

import pytest
from PIL import Image

from benchmark.datasets.loader import FashionDataset, Sample


@pytest.fixture
def fake_dataset(tmp_path: Path) -> tuple[Path, Path]:
    """Create a minimal fake dataset with real image files."""
    images_dir = tmp_path / "images"
    images_dir.mkdir()

    samples = []
    for i in range(5):
        img_path = images_dir / f"img_{i:04d}.jpg"
        Image.new("RGB", (64, 64), color=(i * 40, 0, 0)).save(img_path)
        samples.append({
            "image_path": f"images/img_{i:04d}.jpg",
            "label": "tops" if i < 3 else "bottoms",
            "product_id": f"pid_{i:04d}",
        })

    split_file = tmp_path / "test.json"
    split_file.write_text(json.dumps(samples), encoding="utf-8")
    return tmp_path, split_file


def test_load_samples(fake_dataset: tuple[Path, Path]) -> None:
    root, split_file = fake_dataset
    ds = FashionDataset(dataset_root=root, split_file=split_file, split="test")
    ds.load()
    assert len(ds) == 5


def test_sample_fields(fake_dataset: tuple[Path, Path]) -> None:
    root, split_file = fake_dataset
    ds = FashionDataset(dataset_root=root, split_file=split_file)
    ds.load()
    s = ds.samples[0]
    assert isinstance(s, Sample)
    assert s.label in ("tops", "bottoms")
    assert s.product_id.startswith("pid_")


def test_iter_images(fake_dataset: tuple[Path, Path]) -> None:
    root, split_file = fake_dataset
    ds = FashionDataset(dataset_root=root, split_file=split_file)
    ds.load()
    pairs = list(ds.iter_images())
    assert len(pairs) == 5
    for sample, image in pairs:
        assert isinstance(image, Image.Image)
