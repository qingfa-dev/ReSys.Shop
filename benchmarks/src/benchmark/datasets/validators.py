"""Dataset integrity validation.

Checks that all image paths referenced in a dataset's split file exist
on disk. Used as a pre-flight check before running benchmarks.

Edge cases:
- Missing images produce warning-level error messages but do not
  interrupt validation; the caller decides how to handle failures.
- An empty dataset passes validation (zero errors).
"""
from __future__ import annotations

from benchmark.datasets.loader import FashionDataset
from benchmark.utils.logging import get_logger

logger = get_logger("datasets.validators")


def validate_dataset(dataset: FashionDataset) -> list[str]:
    """Verify that all image paths in the dataset exist on disk.

    Args:
        dataset: A loaded ``FashionDataset`` instance.

    Returns:
        List of error messages for missing image files. Empty list means
        all images are present.
    """
    errors: list[str] = []
    for sample in dataset.samples:
        if not sample.image_path.exists():
            errors.append(f"Missing image: {sample.image_path}")
    if errors:
        logger.warning("%d validation errors found", len(errors))
    else:
        logger.info("Dataset validation passed (%d samples)", len(dataset))
    return errors
