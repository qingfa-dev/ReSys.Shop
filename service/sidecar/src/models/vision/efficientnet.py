"""
EfficientNet-B0 implementation for general visual feature extraction.
"""
import logging
import torch
from torchvision import models, transforms
from src.models.base import BaseEmbedder
from src.core.constants import Constants
from src.models.registry import ModelRegistry

logger = logging.getLogger(__name__)

@ModelRegistry.register(
    "efficientnet_b0", 
    metadata={
        "name": "EfficientNet-B0", 
        "dimension": Constants.Dimensions.EFFICIENTNET_B0,
        "description": "General purpose visual feature extractor.",
        "tags": ["vision", "efficientnet"]
    }
)
class EfficientNetEmbedder(BaseEmbedder):
    """
    Standard EfficientNet-B0 feature extractor.
    """

    def __init__(self):
        # Initialize: Metadata with 1280 dimensions
        super().__init__("efficientnet_b0", Constants.Dimensions.EFFICIENTNET_B0)

        # Create: Load weights from ImageNet-1K V1
        self.model = models.efficientnet_b0(weights=models.EfficientNet_B0_Weights.IMAGENET1K_V1)
        # Update: Remove the classification head
        self.model.classifier = torch.nn.Identity()
        # Map: Move model to GPU if available
        self.model.to(self.device)
        # Trigger: Set to evaluation mode
        self.model.eval()

        # Initialize: Image preprocessing pipeline
        self.preprocess = transforms.Compose([
            transforms.Resize(Constants.Image.RESIZE_SIZE),
            transforms.CenterCrop(Constants.Image.DEFAULT_SIZE),
            transforms.ToTensor(),
            transforms.Normalize(mean=Constants.Image.MEAN, std=Constants.Image.STD),
        ])

    def _forward(self, image):
        """Executes the EfficientNet forward pass."""
        # Transform: Preprocess image and add batch dimension
        tensor = self.preprocess(image).unsqueeze(0).to(self.device)
        # Await: Run inference without gradient tracking
        with torch.no_grad():
            return self.model(tensor)
