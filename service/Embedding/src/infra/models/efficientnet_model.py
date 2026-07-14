"""EfficientNet-B0 model implementation.

EfficientNet-B0 represents the state-of-the-art efficiency-accuracy trade-off,
using compound scaling (depth, width, resolution) discovered via AutoML.

Library: timm (PyTorch Image Models)
Vector dimension: 1280
Pretraining: ImageNet-1K (AutoML-optimized)

Hypothesis: Best efficiency metric (mAP / ms) among the 4 models.
See thesis §11.5.5, H2.
"""

import numpy as np
import torch
from PIL import Image

from embedding.infra.models.base import BaseEmbeddingModel


class EfficientNetB0Model(BaseEmbeddingModel):
    """EfficientNet-B0 embedder via timm."""

    model_name = "efficientnet_b0"
    vector_dim = 1280

    def _load(self) -> None:
        import timm
        import torchvision.transforms as T

        self._device = "cuda" if torch.cuda.is_available() else "cpu"
        # num_classes=0 returns features instead of classification logits
        model = timm.create_model(
            "efficientnet_b0",
            pretrained=True,
            num_classes=0,
        )
        self._model = model.to(self._device).eval()
        # timm models expose default transforms via model.pretrained_cfg
        data_config = timm.data.resolve_model_data_config(model)
        self._preprocess = timm.data.create_transform(**data_config, is_training=False)

    def encode_image(self, image: Image.Image) -> np.ndarray:
        self._ensure_loaded()

        tensor = self._preprocess(image).unsqueeze(0).to(self._device)
        with torch.no_grad():
            features = self._model(tensor)
        vector = self._to_numpy(features).flatten()
        return self._l2_normalize(vector)
