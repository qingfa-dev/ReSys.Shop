"""
Unified models package. 
Exports the ModelRegistry and triggers registration of all model skills.
"""
from src.models.registry import ModelRegistry
from src.models.base import BaseEmbedder

# Trigger: Registration of all vision skills
import src.models.vision.efficientnet
import src.models.vision.clip
import src.models.vision.dinov2

# Trigger: Registration of ONNX skills
import src.models.onnx.onnx_embedder

__all__ = ["ModelRegistry", "BaseEmbedder"]
