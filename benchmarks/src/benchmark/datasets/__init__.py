"""Dataset loading, validation, and transforms.

Exports the primary dataset types (``FashionDataset``, ``Sample``) and the
pre-flight integrity check (``validate_dataset``). Transforms are imported
directly from ``benchmark.datasets.transforms`` when needed.
"""

from benchmark.datasets.loader import FashionDataset, Sample
from benchmark.datasets.validators import validate_dataset

__all__ = ["FashionDataset", "Sample", "validate_dataset"]
