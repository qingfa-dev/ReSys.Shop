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
    """ResNet-50 feature extractor via torchvision for CNN baseline comparison."""

    def __init__(self):
        """Initialize: Metadata with 2048 dimensions, load ImageNet-pretrained ResNet-50.

        Replaces the fully-connected classification head with identity
        and uses the default weight transform pipeline for preprocessing.
        """
        super().__init__("resnet50", Constants.Dimensions.RESNET50)

        # Create: Load ImageNet-pretrained weights
        weights = tv_models.ResNet50_Weights.DEFAULT
        self.model = tv_models.resnet50(weights=weights)
        # Update: Replace fc layer with identity for feature extraction
        self.model.fc = torch.nn.Identity()
        # Map: Transfer to device and set evaluation mode
        self.model = self.model.to(self.device).eval()
        # Initialize: Use the weights' built-in transform pipeline
        self.preprocess = weights.transforms()

    def _forward(self, image):
        """Executes the ResNet-50 forward pass.

        Args:
            image: Preprocessed PIL Image ready for inference.

        Returns:
            Raw feature tensor from the truncated ResNet-50 model.
        """
        # Transform: Apply weight-specific preprocessing, add batch dimension, move to device
        tensor = self.preprocess(image).unsqueeze(0).to(self.device)
        with torch.no_grad():
            return self.model(tensor)
