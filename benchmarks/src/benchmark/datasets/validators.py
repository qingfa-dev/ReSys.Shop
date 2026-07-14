"""Dataset integrity validation."""

from __future__ import annotations

from pathlib import Path

from benchmark.datasets.loader import FashionDataset
from benchmark.utils.logging import get_logger

logger = get_logger("datasets.validators")


def validate_dataset(dataset: FashionDataset) -> list[str]:
    """Check all image paths exist and return a list of error messages."""
    errors: list[str] = []
    for sample in dataset.samples:
        if not sample.image_path.exists():
            errors.append(f"Missing image: {sample.image_path}")
    if errors:
        logger.warning("%d validation errors found", len(errors))
    else:
        logger.info("Dataset validation passed (%d samples)", len(dataset))
    return errors
