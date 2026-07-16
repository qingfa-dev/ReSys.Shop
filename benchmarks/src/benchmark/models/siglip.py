"""SigLIP adapter (Google, via transformers).

SigLIP replaces CLIP's softmax loss with a sigmoid pairwise loss that scales
better to large batch sizes. The ViT-B/16 variant produces 768-D embeddings.

References
----------
- https://huggingface.co/google/siglip-base-patch16-224
- Zhai et al., "Sigmoid Loss for Language Image Pre-Training", ICCV 2023.
"""
from __future__ import annotations

import numpy as np
import torch
from PIL import Image
from transformers import AutoModel, AutoProcessor

from benchmark._constants import CLI_STR
from benchmark.models.base import EmbeddingModel
from benchmark.utils.device import resolve_device
from benchmark.utils.logging import get_logger

logger = get_logger("models.siglip")

_HF_MODEL_ID = "google/siglip-base-patch16-224"


class SigLipModel(EmbeddingModel):
    """SigLIP ViT-B/16 (Google, 768-D embeddings)."""

    def __init__(self, device: str = CLI_STR.AUTO) -> None:
        self._device_pref = device
        self._model: AutoModel | None = None
        self._processor: AutoProcessor | None = None
        self._device: torch.device | None = None

    @property
    def name(self) -> str:
        return "SigLIP ViT-B/16"

    @property
    def embedding_dim(self) -> int:
        return 768

    def load(self) -> None:
        # Call: Download Google SigLIP weights from HuggingFace hub
        logger.info("Loading %s from %s ...", self.name, _HF_MODEL_ID)
        self._device = resolve_device(self._device_pref)
        self._processor = AutoProcessor.from_pretrained(_HF_MODEL_ID)
        self._model = AutoModel.from_pretrained(_HF_MODEL_ID).to(self._device)
        self._model.eval()
        logger.info("%s ready on %s", self.name, self._device)

    def embed(self, image: Image.Image) -> np.ndarray:
        return self.embed_batch([image])[0]

    @torch.inference_mode()
    def embed_batch(self, images: list[Image.Image]) -> np.ndarray:
        # Compute: Encode through SigLIP vision model with L2 normalisation
        self.ensure_loaded()
        inputs = self._processor(images=images, return_tensors="pt", padding=True)
        inputs = {k: v.to(self._device) for k, v in inputs.items()}
        # Explain: SigLIP exposes image features via vision_model.pooler_output (CLS token)
        vision_out = self._model.vision_model(**inputs)
        features = vision_out.pooler_output
        # Normalise: L2-normalise to unit length for cosine similarity
        features = features / features.norm(dim=-1, keepdim=True)
        return features.cpu().float().numpy()
