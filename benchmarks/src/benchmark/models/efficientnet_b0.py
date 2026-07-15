"""EfficientNet-B0 adapter for benchmark evaluation.

EfficientNet-B0 provides a CNN baseline with 1280-D feature vectors.
This adapter removes the final classification head and returns L2-normalized
features suitable for retrieval evaluation.
"""
from __future__ import annotations

import numpy as np
import torch
from PIL import Image
from torchvision import models, transforms

from benchmark._constants import CLI_STR
from benchmark.models.base import EmbeddingModel
from benchmark.utils.device import resolve_device
from benchmark.utils.logging import get_logger

logger = get_logger("models.efficientnet_b0")


class EfficientNetB0Model(EmbeddingModel):
    """EfficientNet-B0 baseline model."""

    def __init__(self, device: str = CLI_STR.AUTO) -> None:
        self._device_pref = device
        self._model: torch.nn.Module | None = None
        self._transform: transforms.Compose | None = None
        self._device: torch.device | None = None

    @property
    def name(self) -> str:
        return "EfficientNet-B0"

    @property
    def embedding_dim(self) -> int:
        return 1280

    def load(self) -> None:
        # Call: Download EfficientNet-B0 ImageNet weights from torchvision
        logger.info("Loading %s ...", self.name)
        self._device = resolve_device(self._device_pref)
        self._model = models.efficientnet_b0(weights=models.EfficientNet_B0_Weights.IMAGENET1K_V1)
        # Transform: Remove classification head — output raw features instead of logits
        self._model.classifier = torch.nn.Identity()
        self._model = self._model.to(self._device)
        self._model.eval()
        # Create: ImageNet normalisation pipeline (resize, centre crop, normalise)
        self._transform = transforms.Compose([
            transforms.Resize(256),
            transforms.CenterCrop(224),
            transforms.ToTensor(),
            transforms.ConvertImageDtype(torch.float32),
            transforms.Normalize(mean=[0.485, 0.456, 0.406], std=[0.229, 0.224, 0.225]),
        ])
        logger.info("%s ready on %s", self.name, self._device)

    def embed(self, image: Image.Image) -> np.ndarray:
        return self.embed_batch([image])[0]

    @torch.inference_mode()
    def embed_batch(self, images: list[Image.Image]) -> np.ndarray:
        # Compute: Extract features through EfficientNet-B0 backbone with L2 normalisation
        self.ensure_loaded()
        assert self._transform is not None and self._model is not None
        tensors = torch.stack([self._transform(img) for img in images]).to(self._device)
        features = self._model(tensors)
        # Normalise: L2-normalise to unit length for cosine similarity
        features = features / features.norm(dim=-1, keepdim=True)
        return features.cpu().float().numpy()
