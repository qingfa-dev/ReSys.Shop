"""
Unified models package.
Exports the ModelRegistry and triggers registration of all model skills.
"""

# Trigger: Registration of all vision skills
# Trigger: Registration of ONNX skills
import embedding.models.onnx.onnx_embedder  # noqa: F401
import embedding.models.vision.clip  # noqa: F401
import embedding.models.vision.dinov2  # noqa: F401
import embedding.models.vision.efficientnet  # noqa: F401
import embedding.models.vision.resnet  # noqa: F401
from embedding.models.base import BaseEmbedder
from embedding.models.registry import ModelRegistry

__all__ = ["ModelRegistry", "BaseEmbedder"]
