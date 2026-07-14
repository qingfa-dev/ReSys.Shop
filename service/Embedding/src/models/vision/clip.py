"""
CLIP (Contrastive Language-Image Pre-Training) model implementations.
"""
import logging

import torch
from embedding.core.constants import Constants
from embedding.models.base import BaseEmbedder
from embedding.models.registry import ModelRegistry

logger = logging.getLogger(__name__)

@ModelRegistry.register(
    "clip_vit_b16",
    metadata={
        "name": "OpenAI CLIP ViT-B/16",
        "dimension": Constants.Dimensions.CLIP_VIT_B16,
        "description": "General semantic visual-text features.",
        "tags": ["vision", "semantic", "clip"]
    }
)
class CLIPEmbedder(BaseEmbedder):
    """
    OpenAI CLIP ViT-B/16 implementation for general semantic features.
    """

    def __init__(self, variant: str = "ViT-B/16"):
        # Initialize: Metadata with 512 dimensions
        super().__init__("clip_vit_b16", Constants.Dimensions.CLIP_VIT_B16)
        try:
            import clip
            # Call: Load OpenAI's implementation if available
            self.model, self.preprocess = clip.load(variant, device=self.device)
            self.model.eval()
            self._use_openai_clip = True
        except ImportError:
            # Fallback: Use HuggingFace Transformers
            from transformers import CLIPModel, CLIPProcessor
            model_id = "openai/clip-vit-base-patch32"
            self.processor = CLIPProcessor.from_pretrained(model_id)
            self.model = CLIPModel.from_pretrained(model_id).to(self.device)
            self.model.eval()
            self._use_openai_clip = False

    def _forward(self, image):
        """Executes the CLIP vision forward pass."""
        # Check: Which implementation is active
        if self._use_openai_clip:
            # Transform: OpenAI preprocessing
            tensor = self.preprocess(image).unsqueeze(0).to(self.device)
            with torch.no_grad():
                # Call: CLIP-specific vision encoding
                return self.model.encode_image(tensor)

        # Call: HuggingFace processor and model
        inputs = self.processor(images=image, return_tensors="pt").to(self.device)
        with torch.no_grad():
            outputs = self.model.get_image_features(**inputs)
            # Transform: Extract embeds if available, otherwise return raw output
            if hasattr(outputs, "image_embeds"):
                return outputs.image_embeds
            return outputs


@ModelRegistry.register(
    "fashion_clip",
    metadata={
        "name": "Fashion-CLIP",
        "dimension": Constants.Dimensions.FASHION_CLIP,
        "description": "Domain-specific visual search for fashion products.",
        "tags": ["vision", "semantic", "fashion"]
    }
)
class FashionCLIPEmbedder(BaseEmbedder):
    """
    Fashion-CLIP implementation for domain-specific visual search.
    """

    def __init__(self):
        # Initialize: Metadata with 512 dimensions
        super().__init__("fashion_clip", Constants.Dimensions.FASHION_CLIP)
        try:
            from transformers import CLIPModel, CLIPProcessor
            model_id = "patrickjohncyh/fashion-clip"
            # Call: Load domain-specific weights from HuggingFace
            self.processor = CLIPProcessor.from_pretrained(model_id)
            self.model = CLIPModel.from_pretrained(model_id).to(self.device)
            self.model.eval()
        except Exception as e:
            # Log: Critical loading error
            logger.error(f"Failed to load Fashion-CLIP: {e}")
            raise RuntimeError(f"Failed to load Fashion-CLIP: {e}")

    def _forward(self, image):
        """Executes the Fashion-CLIP vision forward pass."""
        # Call: Transformers preprocessing
        inputs = self.processor(images=image, return_tensors="pt").to(self.device)
        with torch.no_grad():
            # Trigger: Vision feature extraction
            outputs = self.model.get_image_features(**inputs)
            if hasattr(outputs, "image_embeds"):
                return outputs.image_embeds
            return outputs
