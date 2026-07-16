"""ResNet-50 adapter for benchmark evaluation.

ResNet-50 is a classic CNN baseline (He et al., 2016). This adapter removes
the final classification head and returns L2-normalized features.
"""
from __future__ import annotations

import numpy as np
import torch
from PIL import Image
from torchvision import models as tv_models

from benchmark._constants import CLI_STR
from benchmark.models.base import EmbeddingModel
from benchmark.utils.device import resolve_device
from benchmark.utils.logging import get_logger

logger = get_logger("models.resnet50")


class ResNet50Model(EmbeddingModel):
    """ResNet-50 CNN baseline (2048-D embeddings)."""

    def __init__(self, device: str = CLI_STR.AUTO) -> None:
        self._device_pref = device
        self._model: torch.nn.Module | None = None
        self._preprocess: torch.nn.Module | None = None
        self._device: torch.device | None = None

    @property
    def name(self) -> str:
        return "ResNet-50"

    @property
    def embedding_dim(self) -> int:
        return 2048

    def load(self) -> None:
        # Call: Download ResNet-50 ImageNet weights from torchvision
        logger.info("Loading %s ...", self.name)
        self._device = resolve_device(self._device_pref)
        weights = tv_models.ResNet50_Weights.DEFAULT
        self._model = tv_models.resnet50(weights=weights)
        # Transform: Remove classification head — output raw features instead of logits
        self._model.fc = torch.nn.Identity()
        self._model = self._model.to(self._device)
        self._model.eval()
        self._preprocess = weights.transforms()
        logger.info("%s ready on %s", self.name, self._device)

    def embed(self, image: Image.Image) -> np.ndarray:
        return self.embed_batch([image])[0]

    @torch.inference_mode()
    def embed_batch(self, images: list[Image.Image]) -> np.ndarray:
        # Compute: Extract features through ResNet-50 backbone with L2 normalisation
        self.ensure_loaded()
        assert self._preprocess is not None and self._model is not None
        tensors = torch.stack([self._preprocess(img) for img in images]).to(self._device)
        features = self._model(tensors)
        # Normalise: L2-normalise to unit length for cosine similarity
        features = features / features.norm(dim=-1, keepdim=True)
        return features.cpu().float().numpy()
