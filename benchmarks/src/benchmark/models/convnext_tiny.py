"""ConvNeXt-Tiny adapter for benchmark evaluation.

ConvNeXt-Tiny provides a modern CNN baseline (768-D) with Transformer-inspired
design. This adapter strips the classification head and returns L2-normalized
features suitable for retrieval evaluation.
"""
from __future__ import annotations

import numpy as np
import torch
from PIL import Image
from torchvision import models, transforms

from benchmark.models.base import EmbeddingModel
from benchmark.utils.device import resolve_device
from benchmark.utils.logging import get_logger

logger = get_logger("models.convnext_tiny")


class ConvNeXtTinyModel(EmbeddingModel):
    """ConvNeXt-Tiny — modern CNN with Transformer-inspired architecture."""

    def __init__(self, device: str = "auto") -> None:
        self._device_pref = device
        self._model: torch.nn.Module | None = None
        self._transform: transforms.Compose | None = None
        self._device: torch.device | None = None

    @property
    def name(self) -> str:
        return "ConvNeXt-Tiny"

    @property
    def embedding_dim(self) -> int:
        return 768

    def load(self) -> None:
        logger.info("Loading %s …", self.name)
        self._device = resolve_device(self._device_pref)
        self._model = models.convnext_tiny(weights=models.ConvNeXt_Tiny_Weights.IMAGENET1K_V1)
        self._model.classifier = torch.nn.Flatten(start_dim=1)
        self._model = self._model.to(self._device)
        self._model.eval()
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
        self.ensure_loaded()
        assert self._transform is not None and self._model is not None
        tensors = torch.stack([self._transform(img) for img in images]).to(self._device)
        features = self._model(tensors)
        features = features / features.norm(dim=-1, keepdim=True)
        return features.cpu().float().numpy()
