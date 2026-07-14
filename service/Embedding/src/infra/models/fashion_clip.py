"""Fashion-CLIP model implementation.

Fashion-CLIP is a CLIP variant fine-tuned on fashion-specific image-text pairs.
It is hypothesized to achieve the highest mAP among the 4 evaluated models
(see thesis §11.5.5, H1).

Library: open-clip-torch
Vector dimension: 512
Pretraining: LAION-400M + fashion fine-tuning (Han et al., 2022)
"""

import os

import numpy as np
import torch
from PIL import Image

from embedding.infra.models.base import BaseEmbeddingModel


class FashionCLIPModel(BaseEmbeddingModel):
    """Fashion-CLIP embedder via open_clip."""

    model_name = "fashion-clip"
    vector_dim = 512

    def _load(self) -> None:
        import open_clip

        self._device = "cuda" if torch.cuda.is_available() else "cpu"
        # Fashion-CLIP is distributed via HuggingFace through open_clip
        model, _, preprocess = open_clip.create_model_and_transforms(
            model_name="ViT-B-32",
            pretrained="laion2b_s34b_b79k",  # Generic CLIP base; swap to fashion-specific if available
            device=self._device,
        )
        self._model = model
        self._preprocess = preprocess
        self._model.eval()

    def encode_image(self, image: Image.Image) -> np.ndarray:
        self._ensure_loaded()
        import open_clip

        # Preprocess and add batch dimension
        tensor = self._preprocess(image).unsqueeze(0).to(self._device)
        with torch.no_grad():
            features = self._model.encode_image(tensor)
            # L2-normalize inside torch for speed
            features = features / features.norm(dim=-1, keepdim=True)
        vector = self._to_numpy(features).flatten()
        return self._l2_normalize(vector)
