"""
Unified models package. 
Exports the ModelRegistry and triggers registration of all model skills.
"""
from embedding.models.registry import ModelRegistry
from embedding.models.base import BaseEmbedder

# Trigger: Registration of all vision skills
import embedding.models.vision.efficientnet
import embedding.models.vision.clip
import embedding.models.vision.dinov2

# Trigger: Registration of ONNX skills
import embedding.models.onnx.onnx_embedder

__all__ = ["ModelRegistry", "BaseEmbedder"]
