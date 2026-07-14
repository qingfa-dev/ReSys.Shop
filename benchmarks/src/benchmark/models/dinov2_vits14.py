"""DINOv2 ViT-S/14 adapter for benchmark evaluation.

This adapter uses the DINOv2 vision transformer and returns normalized
image embeddings for retrieval.
"""
from __future__ import annotations

import numpy as np
import torch
from PIL import Image
from torchvision import transforms

from benchmark.models.base import EmbeddingModel
from benchmark.utils.device import resolve_device
from benchmark.utils.logging import get_logger

logger = get_logger("models.dinov2_vits14")


class DinoV2ViTS14Model(EmbeddingModel):
    """DINOv2 ViT-S/14 model."""

    def __init__(self, device: str = "auto") -> None:
        self._device_pref = device
        self._model: torch.nn.Module | None = None
        self._transform: transforms.Compose | None = None
        self._device: torch.device | None = None

    @property
    def name(self) -> str:
        return "DINOv2 ViT-S/14"

    @property
    def embedding_dim(self) -> int:
        return 384

    def load(self) -> None:
        logger.info("Loading %s …", self.name)
        self._device = resolve_device(self._device_pref)
        self._model = torch.hub.load("facebookresearch/dinov2", "dinov2_vits14", pretrained=True)
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
        features = self._model.forward(tensors)
        if hasattr(features, "image_embeds"):
            features = features.image_embeds
        elif hasattr(features, "pooler_output"):
            features = features.pooler_output
        features = features / features.norm(dim=-1, keepdim=True)
        return features.cpu().float().numpy()
