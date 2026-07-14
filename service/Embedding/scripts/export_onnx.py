"""
Model export orchestrator for inference.
Converts PyTorch models to optimized ONNX artifacts.
"""
import os
import sys
from pathlib import Path

# Ensure the project root is in sys.path for internal imports
sys.path.append(str(Path(__file__).parent.parent))

from embedding.core.config import settings
from scripts.export.vision import (
    export_efficientnet, 
    export_clip, 
    export_fashion_clip, 
    export_dinov2
)

# Propagate Hugging Face token from settings to environment for transformers/huggingface_hub
if settings.HUGGING_FACE_TOKEN:
    os.environ["HF_TOKEN"] = settings.HUGGING_FACE_TOKEN
    os.environ["HUGGING_FACE_HUB_TOKEN"] = settings.HUGGING_FACE_TOKEN


def main():
    """Main export orchestration flow."""
    print(f"🚀 Starting model export. Destination: models/")
    try:
        export_efficientnet()
        export_clip()
        export_fashion_clip()
        export_dinov2()
        print("\n✨ All models exported successfully to models!")
    except Exception as e:
        print(f"\n❌ Export failed: {e}")
        import traceback
        traceback.print_exc()
        sys.exit(1)


if __name__ == "__main__":
    main()
