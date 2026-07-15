"""Reproducibility helpers — set all relevant RNG seeds in one call.

Seeds Python's ``random``, NumPy, and PyTorch (if available) with a single
integer. Ensures deterministic results across runs.

Edge cases:
- PyTorch seeding is optional — silently skipped if torch is not installed.
- CUDA-specific seeding (``manual_seed_all``) is applied only when CUDA is
  available.
"""

from __future__ import annotations

import random

import numpy as np


def set_seed(seed: int = 42) -> None:
    """Seed Python, NumPy, and PyTorch (if available) for reproducibility.

    Args:
        seed: Integer seed value.  Default 42 matches benchmark.yaml.
    """
    random.seed(seed)
    np.random.seed(seed)

    try:
        import torch

        torch.manual_seed(seed)
        if torch.cuda.is_available():
            torch.cuda.manual_seed_all(seed)
            torch.backends.cudnn.deterministic = True
            torch.backends.cudnn.benchmark = False
    except ImportError:
        pass
