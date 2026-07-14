"""Model backend implementations.

Exports the Strategy-pattern model hierarchy for embedding generation.
"""

from embedding.infra.models.base import BaseEmbeddingModel
from embedding.infra.models.clip import CLIPGenericModel
from embedding.infra.models.efficientnet_model import EfficientNetB0Model
from embedding.infra.models.fashion_clip import FashionCLIPModel
from embedding.infra.models.resnet_model import ResNet50Model

__all__ = [
    "BaseEmbeddingModel",
    "CLIPGenericModel",
    "EfficientNetB0Model",
    "FashionCLIPModel",
    "ResNet50Model",
]
