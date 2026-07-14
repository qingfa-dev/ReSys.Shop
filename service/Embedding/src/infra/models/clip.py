"""OpenAI CLIP-generic model implementation.

CLIP-generic tests whether general-purpose text-image pretraining suffices
for fashion CBIR without domain-specific fine-tuning.

Library: transformers (HuggingFace)
Vector dimension: 512
Pretraining: OpenAI WIT-400M

Hypothesis: Underperforms Fashion-CLIP but outperforms pure CNNs (ResNet-50).
See thesis §11.5.5, H4.
"""

import numpy as np
import torch
from PIL import Image

from embedding.infra.models.base import BaseEmbeddingModel


class CLIPGenericModel(BaseEmbeddingModel):
    """Generic CLIP embedder via HuggingFace transformers."""

    model_name = "clip"
    vector_dim = 512

    def _load(self) -> None:
        from transformers import CLIPModel, CLIPProcessor

        self._device = "cuda" if torch.cuda.is_available() else "cpu"
        self._model = CLIPModel.from_pretrained("openai/clip-vit-base-patch32").to(self._device).eval()
        self._preprocess = CLIPProcessor.from_pretrained("openai/clip-vit-base-patch32")

    def encode_image(self, image: Image.Image) -> np.ndarray:
        self._ensure_loaded()

        inputs = self._preprocess(images=image, return_tensors="pt").to(self._device)
        with torch.no_grad():
            features = self._model.get_image_features(**inputs)
            features = features / features.norm(dim=-1, keepdim=True)
        vector = self._to_numpy(features).flatten()
        return self._l2_normalize(vector)
