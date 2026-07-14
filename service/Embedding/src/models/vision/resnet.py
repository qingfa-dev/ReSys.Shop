"""ResNet-50 model implementation for visual features (CNN baseline)."""
import logging

import torch
from embedding.core.constants import Constants
from embedding.models.base import BaseEmbedder
from embedding.models.registry import ModelRegistry
from torchvision import models as tv_models

logger = logging.getLogger(__name__)


@ModelRegistry.register(
    "resnet50",
    metadata={
        "name": "ResNet-50",
        "dimension": Constants.Dimensions.RESNET50,
        "description": "ImageNet-pretrained ResNet-50 CNN baseline for comparative evaluation.",
        "tags": ["vision", "cnn", "baseline", "imagenet"]
    }
)
class ResNet50Embedder(BaseEmbedder):
    """ResNet-50 feature extractor via torchvision."""

    def __init__(self):
        super().__init__("resnet50", Constants.Dimensions.RESNET50)

        weights = tv_models.ResNet50_Weights.DEFAULT
        self.model = tv_models.resnet50(weights=weights)
        self.model.fc = torch.nn.Identity()
        self.model = self.model.to(self.device).eval()
        self.preprocess = weights.transforms()

    def _forward(self, image):
        tensor = self.preprocess(image).unsqueeze(0).to(self.device)
        with torch.no_grad():
            return self.model(tensor)
