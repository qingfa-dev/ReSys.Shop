"""Model registry — single source of truth for all supported adapters.

Imports are lazy so that the metrics, reporting, and evaluation packages
can be imported in environments where torch / transformers are not installed
(e.g. during report generation from cached JSON results).

To add a new model:
  1. Create ``models/<name>.py`` subclassing ``EmbeddingModel``
  2. Add a factory function to ``_FACTORIES`` below

Nothing else in the pipeline needs to change.
"""
from __future__ import annotations

from benchmark.models.base import EmbeddingModel

# Registry factories — each returns a fresh instance on first access.
# Lazy so torch/transformers are not imported at module load time.
_FACTORIES: dict[str, type] = {}  # populated by _register()


def _register() -> dict[str, EmbeddingModel]:
    """Import and instantiate all adapters (deferred until first use)."""
    from benchmark.models.clip_b32 import ClipB32Model
    from benchmark.models.clip_generic import ClipGenericModel
    from benchmark.models.clip_l14 import ClipL14Model
    from benchmark.models.clip_vit_b16 import ClipViTB16Model
    from benchmark.models.eva_clip import EvaClipModel
    from benchmark.models.convnext_tiny import ConvNeXtTinyModel
    from benchmark.models.efficientnet_b0 import EfficientNetB0Model
    from benchmark.models.fashion_clip import FashionClipModel
    from benchmark.models.dinov2_vits14 import DinoV2ViTS14Model
    from benchmark.models.resnet50 import ResNet50Model
    from benchmark.models.siglip import SigLipModel

    return {
        "efficientnet-b0": EfficientNetB0Model(),
        "convnext-tiny":   ConvNeXtTinyModel(),
        "dinov2-vits14":   DinoV2ViTS14Model(),
        "fashion-clip":    FashionClipModel(),
        "clip-b32":        ClipB32Model(),
        "clip-generic":    ClipGenericModel(),
        "clip-l14":        ClipL14Model(),
        "clip-vit-b16":    ClipViTB16Model(),
        "siglip":          SigLipModel(),
        "resnet-50":       ResNet50Model(),
        "eva-clip":        EvaClipModel(),
    }


def get_registry(device: str = "auto") -> dict[str, EmbeddingModel]:
    """Return a fresh registry of model instances for the requested device."""
    from benchmark.models.clip_b32 import ClipB32Model
    from benchmark.models.clip_generic import ClipGenericModel
    from benchmark.models.clip_l14 import ClipL14Model
    from benchmark.models.clip_vit_b16 import ClipViTB16Model
    from benchmark.models.convnext_tiny import ConvNeXtTinyModel
    from benchmark.models.eva_clip import EvaClipModel
    from benchmark.models.efficientnet_b0 import EfficientNetB0Model
    from benchmark.models.fashion_clip import FashionClipModel
    from benchmark.models.dinov2_vits14 import DinoV2ViTS14Model
    from benchmark.models.resnet50 import ResNet50Model
    from benchmark.models.siglip import SigLipModel

    return {
        "efficientnet-b0": EfficientNetB0Model(device=device),
        "convnext-tiny":   ConvNeXtTinyModel(device=device),
        "dinov2-vits14":   DinoV2ViTS14Model(device=device),
        "fashion-clip":    FashionClipModel(device=device),
        "clip-b32":        ClipB32Model(device=device),
        "clip-generic":    ClipGenericModel(device=device),
        "clip-l14":        ClipL14Model(device=device),
        "clip-vit-b16":    ClipViTB16Model(device=device),
        "siglip":          SigLipModel(device=device),
        "resnet-50":       ResNet50Model(device=device),
        "eva-clip":        EvaClipModel(device=device),
    }


class _LazyRegistry(dict):
    """Dict that builds itself on first access."""

    _built: bool = False

    def _build(self) -> None:
        if not self._built:
            self.update(_register())
            self._built = True

    def __getitem__(self, key):
        self._build()
        return super().__getitem__(key)

    def __iter__(self):
        self._build()
        return super().__iter__()

    def keys(self):
        self._build()
        return super().keys()

    def values(self):
        self._build()
        return super().values()

    def items(self):
        self._build()
        return super().items()

    def __len__(self):
        self._build()
        return super().__len__()


REGISTRY: dict[str, EmbeddingModel] = _LazyRegistry()

__all__ = ["EmbeddingModel", "REGISTRY", "get_registry"]
