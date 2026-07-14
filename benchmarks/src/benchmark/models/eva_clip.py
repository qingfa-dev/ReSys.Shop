"""EVA-CLIP adapter (BAAI, via open_clip).

EVA-CLIP scales the ViT encoder to 18B parameters (largest public CLIP).
The ViT-B/16 variant used here is a practical mid-range choice producing
512-D embeddings available through open_clip as ``EVA02-B-16``.

References
----------
- https://github.com/baaivision/EVA/tree/master/EVA-CLIP
- Sun et al., "EVA-CLIP-18B: Scaling CLIP to 18 Billion Parameters", 2024.
"""
from __future__ import annotations

import numpy as np
import open_clip
import torch
from PIL import Image

from benchmark.models.base import EmbeddingModel
from benchmark.utils.device import resolve_device
from benchmark.utils.logging import get_logger

logger = get_logger("models.eva_clip")

# EVA02-B-16 is available directly in open_clip with merged-2b weights
_MODEL_NAME = "EVA02-B-16"
_PRETRAINED = "merged2b_s8b_b131k"


class EvaClipModel(EmbeddingModel):
    """EVA-CLIP EVA02-B/16 (BAAI, 512-D embeddings)."""

    def __init__(self, device: str = "auto") -> None:
        self._device_pref = device
        self._model: open_clip.CLIP | None = None
        self._transform = None
        self._device: torch.device | None = None

    @property
    def name(self) -> str:
        return "EVA-CLIP EVA02-B/16"

    @property
    def embedding_dim(self) -> int:
        return 512

    def load(self) -> None:
        logger.info("Loading %s …", self.name)
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
        self.ensure_loaded()
        tensors = torch.stack([self._transform(img) for img in images]).to(self._device)
        features = self._model.encode_image(tensors)
        features = features / features.norm(dim=-1, keepdim=True)
        return features.cpu().float().numpy()
