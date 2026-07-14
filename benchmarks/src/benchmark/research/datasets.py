"""Research dataset helpers for combined train/test split workflows."""
from __future__ import annotations

import json
from pathlib import Path
from typing import Iterable

from benchmark.datasets.loader import Sample
from benchmark.utils.logging import get_logger

logger = get_logger("research.datasets")


class ResearchDataset:
    """Load one or both train/test split files for research evaluation."""

    def __init__(
        self,
        dataset_root: Path,
        train_split_file: Path | None = None,
        test_split_file: Path | None = None,
        combined_split_file: Path | None = None,
        default_split: str = "test",
    ) -> None:
        self.dataset_root = dataset_root
        self.train_split_file = train_split_file
        self.test_split_file = test_split_file
        self.combined_split_file = combined_split_file
        self.default_split = default_split
        self._samples: list[Sample] = []

    def load(self) -> None:
        """Load split JSON files and preserve sample split metadata."""
        if self.train_split_file and self.test_split_file:
            logger.info(
                "Loading research dataset from train=%s and test=%s",
                self.train_split_file,
                self.test_split_file,
            )
            self._samples = [
                *self._load_json(self.train_split_file, "train"),
                *self._load_json(self.test_split_file, "test"),
            ]
        elif self.combined_split_file:
            logger.info("Loading research dataset from %s", self.combined_split_file)
            self._samples = self._load_json(self.combined_split_file, None)
        else:
            raise ValueError(
                "Provide either train_split_file/test_split_file or combined_split_file."
            )

        logger.info("Loaded %d research samples", len(self._samples))

    def _load_json(self, path: Path, default_split: str | None) -> list[Sample]:
        raw = json.loads(path.read_text(encoding="utf-8"))
        samples: list[Sample] = []
        for item in raw:
            split = item.get("split")
            if split is None:
                split = default_split or self.default_split
            samples.append(self._create_sample(item, split))
        return samples

    def _create_sample(self, item: dict, split: str) -> Sample:
        return Sample(
            image_path=self.dataset_root / item["image_path"],
            label=item.get("label", "unknown"),
            product_id=str(item.get("product_id", item.get("id", ""))),
            split=split,
        )

    @property
    def samples(self) -> list[Sample]:
        if not self._samples:
            raise RuntimeError("Call load() before accessing samples")
        return self._samples

    def query_gallery_split(self) -> tuple[list[Sample], list[Sample]]:
        """Return query (test) and gallery (train/val) sample lists."""
        if not self._samples:
            raise RuntimeError("Call load() before accessing splits")

        query = [s for s in self._samples if s.split == "test"]
        gallery = [s for s in self._samples if s.split != "test"]

        if not query or not gallery:
            raise ValueError("Research dataset requires both test and gallery samples")

        return query, gallery

    def sample_metadata(self) -> list[dict[str, str]]:
        return [
            {
                "image_path": str(sample.image_path),
                "label": sample.label,
                "product_id": sample.product_id,
                "split": sample.split,
            }
            for sample in self.samples
        ]
