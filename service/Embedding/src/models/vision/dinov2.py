"""
DINOv2 implementation for visual structure features.
"""
import logging

import torch
from embedding.core.constants import Constants
from embedding.models.base import BaseEmbedder
from embedding.models.registry import ModelRegistry
from torchvision import transforms

logger = logging.getLogger(__name__)

@ModelRegistry.register(
    "dinov2_vits14",
    metadata={
        "name": "DINOv2 ViT-S/14",
        "dimension": Constants.Dimensions.DINOV2_VITS14,
        "description": "Self-supervised structural feature extraction.",
        "tags": ["vision", "structural", "dinov2"]
    }
)
class DINOEmbedder(BaseEmbedder):
    """
    DINOv2 ViT-S/14 self-supervised feature extractor.
    """

    def __init__(self):
        """Initialize: Metadata with 384 dimensions, load DINOv2 via PyTorch Hub.

        Sets up bicubic-resize preprocessing for self-supervised feature extraction.
        """
        super().__init__("dinov2_vits14", Constants.Dimensions.DINOV2_VITS14)

        # Call: Load self-supervised weights from facebookresearch/dinov2 via PyTorch Hub
        self.model = torch.hub.load(
            "facebookresearch/dinov2",
            "dinov2_vits14",
            pretrained=True
        ).to(self.device)
        self.model.eval()

        # Initialize: High-precision preprocessing with bicubic interpolation
        self.preprocess = transforms.Compose([
            transforms.Resize(
                Constants.Image.RESIZE_SIZE,
                interpolation=transforms.InterpolationMode.BICUBIC,
            ),
            transforms.CenterCrop(Constants.Image.DEFAULT_SIZE),
            transforms.ToTensor(),
            transforms.Normalize(mean=Constants.Image.MEAN, std=Constants.Image.STD),
        ])

    def _forward(self, image):
        """Executes the DINOv2 forward pass.

        Args:
            image: Preprocessed PIL Image ready for inference.

        Returns:
            Raw self-supervised feature tensor from DINOv2.
        """
        # Transform: Preprocess input with bicubic resize, add batch dimension, move to device
        tensor = self.preprocess(image).unsqueeze(0).to(self.device)
        with torch.no_grad():
            # Trigger: Self-supervised feature extraction via DINOv2 backbone
            return self.model(tensor)
