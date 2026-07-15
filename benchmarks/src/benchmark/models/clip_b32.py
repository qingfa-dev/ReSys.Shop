"""CLIP ViT-B/32 adapter using open_clip.

Open-source CLIP trained by OpenAI on WIT-400M. ViT-B/32 is the smaller,
faster variant — 512-D embeddings.

References
----------
- https://github.com/mlfoundations/open_clip
- Radford et al., "Learning Transferable Visual Models From Natural Language
  Supervision", ICML 2021.
"""
from __future__ import annotations

import numpy as np
import open_clip
import torch
from PIL import Image

from benchmark.models.base import EmbeddingModel
from benchmark.utils.device import resolve_device
from benchmark.utils.logging import get_logger

logger = get_logger("models.clip_b32")

_MODEL_NAME = "ViT-B-32"
_PRETRAINED = "openai"


class ClipB32Model(EmbeddingModel):
    """CLIP ViT-B/32 (OpenAI weights, 512-D embeddings)."""

    def __init__(self, device: str = "auto") -> None:
        self._device_pref = device
        self._model: open_clip.CLIP | None = None
        self._transform = None
        self._device: torch.device | None = None

    @property
    def name(self) -> str:
        return "CLIP ViT-B/32"

    @property
    def embedding_dim(self) -> int:
        return 512

    def load(self) -> None:
        # Call: Download OpenAI CLIP weights from open_clip hub
        logger.info("Loading %s ...", self.name)
        self._device = resolve_device(self._device_pref)
        self._model, _, self._transform = open_clip.create_model_and_transforms(
            _MODEL_NAME, pretrained=_PRETRAINED, device=self._device
        )
        self._model.eval()
        logger.info("%s ready on %s", self.name, self._device)

    def embed(self, image: Image.Image) -> np.ndarray:
        return self.embed_batch([image])[0]

    @torch.inference_mode()
    def embed_batch(self, images: list[Image.Image]) -> np.ndarray:
        # Compute: Encode images through CLIP vision encoder with L2 normalisation
        self.ensure_loaded()
        tensors = torch.stack([self._transform(img) for img in images]).to(self._device)
        features = self._model.encode_image(tensors)
        # Normalise: L2-normalise to unit length for cosine similarity
        features = features / features.norm(dim=-1, keepdim=True)
        return features.cpu().float().numpy()
