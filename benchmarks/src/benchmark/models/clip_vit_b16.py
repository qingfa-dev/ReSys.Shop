"""CLIP ViT-B/16 adapter for benchmark evaluation.

This model uses the standard CLIP ViT-B/16 image tower for general image
retrieval embeddings.
"""
from __future__ import annotations

import numpy as np
import torch
from PIL import Image
from transformers import CLIPModel, CLIPProcessor

from benchmark.models.base import EmbeddingModel
from benchmark.utils.device import resolve_device
from benchmark.utils.logging import get_logger

logger = get_logger("models.clip_vit_b16")

_HF_MODEL_ID = "openai/clip-vit-base-patch16"


class ClipViTB16Model(EmbeddingModel):
    """CLIP ViT-B/16 adapter."""

    def __init__(self, device: str = "auto") -> None:
        self._device_pref = device
        self._model: CLIPModel | None = None
        self._processor: CLIPProcessor | None = None
        self._device: torch.device | None = None

    @property
    def name(self) -> str:
        return "CLIP ViT-B/16"

    @property
    def embedding_dim(self) -> int:
        return 512

    def load(self) -> None:
        # Call: Download OpenAI CLIP ViT-B/16 weights from HuggingFace hub
        logger.info("Loading %s from %s ...", self.name, _HF_MODEL_ID)
        self._device = resolve_device(self._device_pref)
        self._processor = CLIPProcessor.from_pretrained(_HF_MODEL_ID)
        self._model = CLIPModel.from_pretrained(_HF_MODEL_ID).to(self._device)
        self._model.eval()
        logger.info("%s ready on %s", self.name, self._device)

    def embed(self, image: Image.Image) -> np.ndarray:
        return self.embed_batch([image])[0]

    @torch.inference_mode()
    def embed_batch(self, images: list[Image.Image]) -> np.ndarray:
        # Compute: Encode images through CLIP vision encoder with L2 normalisation
        self.ensure_loaded()
        assert self._processor is not None and self._model is not None
        inputs = self._processor(images=images, return_tensors="pt", padding=True)
        inputs = {k: v.to(self._device) for k, v in inputs.items()}
        features = self._model.get_image_features(**inputs)
        # Normalise: Handle tuple vs pooler_output depending on transformers version
        if isinstance(features, tuple):
            features = features[0]
        elif hasattr(features, "pooler_output"):
            features = features.pooler_output
        # Normalise: L2-normalise to unit length for cosine similarity
        features = features / features.norm(dim=-1, keepdim=True)
        return features.cpu().float().numpy()
