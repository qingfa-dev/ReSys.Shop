"""
Base utilities for model export.
"""
import warnings
from pathlib import Path

import torch

# Suppress noisy warnings
warnings.filterwarnings("ignore", category=torch.jit.TracerWarning)
warnings.filterwarnings("ignore", category=DeprecationWarning)
warnings.filterwarnings("ignore", message=".*xFormers is not available.*")

# sidecar/scripts/export/base.py -> sidecar/scripts -> sidecar
ROOT_DIR = Path(__file__).resolve().parent.parent.parent
# Local models folder within the sidecar
EXPORT_ROOT = ROOT_DIR / "models"



def verify_export(path: Path):
    """Checks if the exported model exists and has a reasonable size."""
    if path.exists() and path.stat().st_size > 1024 * 1024:
        print(f"✅ Verified: {path} ({path.stat().st_size / 1024 / 1024:.2f} MB)")
    else:
        raise FileNotFoundError(f"❌ Verification failed for {path}")


def get_model_path(model_name: str) -> Path:
    """Helper to create and return the model directory within artifacts."""
    path = EXPORT_ROOT / model_name
    path.mkdir(parents=True, exist_ok=True)
    return path / "model.onnx"
