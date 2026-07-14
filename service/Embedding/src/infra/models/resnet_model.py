"""ResNet-50 model implementation.

ResNet-50 serves as the CNN baseline in the comparative study.
It is a mature architecture widely used in fashion retrieval literature.

Library: torchvision
Vector dimension: 2048
Pretraining: ImageNet-1K (ResNet50_Weights.DEFAULT)

Note: The high dimensionality (2048-d) implies 4× storage cost vs 512-d models.
See thesis §11.5.5, H3.
"""

import numpy as np
import torch
from PIL import Image

from embedding.infra.models.base import BaseEmbeddingModel


class ResNet50Model(BaseEmbeddingModel):
    """ResNet-50 embedder via torchvision."""

    model_name = "resnet50"
    vector_dim = 2048

    def _load(self) -> None:
        import torchvision.models as models
        import torchvision.transforms as T

        self._device = "cuda" if torch.cuda.is_available() else "cpu"
        weights = models.ResNet50_Weights.DEFAULT
        model = models.resnet50(weights=weights)
        # Remove the final FC layer; avgpool output is 2048-d
        model.fc = torch.nn.Identity()
        self._model = model.to(self._device).eval()
        self._preprocess = weights.transforms()

    def encode_image(self, image: Image.Image) -> np.ndarray:
        self._ensure_loaded()

        tensor = self._preprocess(image).unsqueeze(0).to(self._device)
        with torch.no_grad():
            features = self._model(tensor)
        vector = self._to_numpy(features).flatten()
        return self._l2_normalize(vector)
