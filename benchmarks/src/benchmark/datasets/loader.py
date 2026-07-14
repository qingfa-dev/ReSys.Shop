"""Dataset loading — produces (image, label) pairs from configured dataset paths."""

from __future__ import annotations

import json
from dataclasses import dataclass
from pathlib import Path
from typing import Iterator

from PIL import Image

from benchmark.utils.logging import get_logger

logger = get_logger("datasets.loader")


@dataclass(frozen=True)
class Sample:
    """A single dataset sample."""

    image_path: Path
    label: str          # category / product ID used for retrieval relevance
    product_id: str
    split: str          # train | val | test


class FashionDataset:
    """Load a fashion retrieval dataset from a JSON split file.

    Expected JSON format::

        [
          {
            "image_path": "tops/img_00000001.jpg",
            "label": "tops",
            "product_id": "id_00000001"
          },
          ...
        ]
    """

    def __init__(self, dataset_root: Path, split_file: Path, split: str = "test") -> None:
        self.dataset_root = dataset_root
        self.split_file = split_file
        self.split = split
        self._samples: list[Sample] = []

    def load(self) -> None:
        """Parse the split JSON and build the sample list."""
        logger.info("Loading %s split from %s", self.split, self.split_file)
        raw = json.loads(self.split_file.read_text(encoding="utf-8"))
        self._samples = [
            Sample(
                image_path=self.dataset_root / item["image_path"],
                label=item["label"],
                product_id=item["product_id"],
                split=self.split,
            )
            for item in raw
        ]
        logger.info("Loaded %d samples", len(self._samples))

    @property
    def samples(self) -> list[Sample]:
        if not self._samples:
            raise RuntimeError("Call load() before accessing samples")
        return self._samples

    def __len__(self) -> int:
        return len(self._samples)

    def iter_images(self, max_samples: int | None = None) -> Iterator[tuple[Sample, Image.Image]]:
        """Yield (sample, PIL Image) pairs for the dataset."""
        samples = self._samples[:max_samples] if max_samples else self._samples
        for sample in samples:
            try:
                image = Image.open(sample.image_path).convert("RGB")
                yield sample, image
            except (FileNotFoundError, OSError) as exc:
                logger.warning("Skipping %s: %s", sample.image_path, exc)
