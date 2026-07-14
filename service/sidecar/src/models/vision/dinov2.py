"""
DINOv2 implementation for visual structure features.
"""
import logging
import torch
from torchvision import transforms
from src.models.base import BaseEmbedder
from src.core.constants import Constants
from src.models.registry import ModelRegistry

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
        # Initialize: Metadata with 384 dimensions
        super().__init__("dinov2_vits14", Constants.Dimensions.DINOV2_VITS14)

        # Call: Load self-supervised weights via PyTorch Hub
        self.model = torch.hub.load(
            "facebookresearch/dinov2",
            "dinov2_vits14",
            pretrained=True
        ).to(self.device)
        self.model.eval()

        # Initialize: High-precision preprocessing
        self.preprocess = transforms.Compose([
            transforms.Resize(Constants.Image.RESIZE_SIZE, interpolation=transforms.InterpolationMode.BICUBIC),
            transforms.CenterCrop(Constants.Image.DEFAULT_SIZE),
            transforms.ToTensor(),
            transforms.Normalize(mean=Constants.Image.MEAN, std=Constants.Image.STD),
        ])

    def _forward(self, image):
        """Executes the DINOv2 forward pass."""
        # Transform: Preprocess input
        tensor = self.preprocess(image).unsqueeze(0).to(self.device)
        with torch.no_grad():
            # Trigger: Self-supervised feature extraction
            return self.model(tensor)
