"""Dataset loading — produces (image, label) pairs from configured dataset paths.

Provides ``FashionDataset`` for loading JSON-based split files and yielding
PIL image pairs. Supports train / val / test splits.

Edge cases:
- Missing or corrupt image files are logged and skipped during iteration.
- Accessing ``samples`` before calling ``load()`` raises RuntimeError.
- ``max_samples`` in ``iter_images`` limits yielded pairs without pre-loading.
"""
from __future__ import annotations

import json
from collections.abc import Iterator
from dataclasses import dataclass
from pathlib import Path

from PIL import Image

from benchmark._constants import FIELD, FILE_ENCODING, SPLIT
from benchmark.utils.logging import get_logger

logger = get_logger("datasets.loader")


@dataclass(frozen=True)
class Sample:
    """A single dataset sample with metadata for retrieval evaluation.

    Attributes:
        image_path: Absolute path to the source image file.
        label: Category or product ID used as the retrieval relevance key.
        product_id: Unique product identifier.
        split: Dataset partition — ``"train"``, ``"val"``, or ``"test"``.
    """

    image_path: Path
    label: str
    product_id: str
    split: str


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

    Edge cases:
    - Empty JSON produces an empty sample list (zero-length dataset).
    - Call ``load()`` before accessing ``samples`` or ``iter_images``.
    """

    def __init__(self, dataset_root: Path, split_file: Path, split: str = SPLIT.TEST) -> None:
        self.dataset_root = dataset_root
        self.split_file = split_file
        self.split = split
        self._samples: list[Sample] = []

    def load(self, label_field: str = FIELD.LABEL) -> None:
        """Parse the split JSON and build the internal sample list.

        Reads the split file as UTF-8 JSON, then constructs ``Sample``
        instances with paths rooted at ``dataset_root``.

        Args:
            label_field: JSON key for the relevance label. Defaults to
                ``"label"``.  Set to ``"label_pattern"`` for the secondary
                (pattern-aware) evaluation pass.
        """
        logger.info("Loading %s split from %s (label=%s)", self.split, self.split_file, label_field)
        raw = json.loads(self.split_file.read_text(encoding=FILE_ENCODING))
        self._samples = [
            Sample(
                image_path=self.dataset_root / item[FIELD.IMAGE_PATH],
                label=item[label_field],
                product_id=item[FIELD.PRODUCT_ID],
                split=self.split,
            )
            for item in raw
        ]
        logger.info("Loaded %d samples", len(self._samples))

    @property
    def samples(self) -> list[Sample]:
        """Return the loaded sample list.

        Raises:
            RuntimeError: If ``load()`` has not been called yet.
        """
        if not self._samples:
            raise RuntimeError("Call load() before accessing samples")
        return self._samples

    def __len__(self) -> int:
        return len(self._samples)

    def iter_images(self, max_samples: int | None = None) -> Iterator[tuple[Sample, Image.Image]]:
        """Yield (sample, PIL Image) pairs for the dataset.

        Args:
            max_samples: Maximum number of samples to yield. Yields all if None.

        Yields:
            Tuple of (Sample, RGB PIL Image) for each image that loads
            successfully. Missing or corrupt images are logged and skipped.
        """
        samples = self._samples[:max_samples] if max_samples else self._samples
        for sample in samples:
            try:
                image = Image.open(sample.image_path).convert("RGB")
                yield sample, image
            except (FileNotFoundError, OSError) as exc:
                logger.warning("Skipping %s: %s", sample.image_path, exc)
