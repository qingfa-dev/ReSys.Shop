"""
Centralized architectural and mathematical constants for inference.
Organized into immutable dataclasses with rich metadata for each field.
"""
from dataclasses import dataclass, field, fields
from typing import Any, Dict, List


@dataclass(frozen=True)
class ImageConstants:
    """Constants related to image preprocessing and dataset standards."""
    
    MEAN: List[float] = field(
        default_factory=lambda: [0.485, 0.456, 0.406],
        metadata={"description": "Standard ImageNet normalization mean values (RGB).", "ref": "https://pytorch.org/vision/main/models/generated/torchvision.models.resnet50.html"}
    )
    
    STD: List[float] = field(
        default_factory=lambda: [0.229, 0.224, 0.225],
        metadata={"description": "Standard ImageNet normalization standard deviation values (RGB)."}
    )

    DEFAULT_SIZE: int = field(
        default=224,
        metadata={"description": "Standard input size for most vision models (pixels).", "unit": "px"}
    )
    
    RESIZE_SIZE: int = field(
        default=256,
        metadata={"description": "Initial resize dimensions before center cropping.", "unit": "px"}
    )


@dataclass(frozen=True)
class DimensionConstants:
    """Fixed output vector dimensions for supported machine learning models."""
    
    EFFICIENTNET_B0: int = field(
        default=1280,
        metadata={"model": "EfficientNet-B0", "type": "embedding", "source": "torchvision"}
    )
    
    CLIP_VIT_B16: int = field(
        default=512,
        metadata={"model": "CLIP ViT-B/16", "type": "semantic_embedding", "source": "openai/clip"}
    )
    
    FASHION_CLIP: int = field(
        default=512,
        metadata={"model": "Fashion-CLIP", "type": "domain_specific_embedding", "source": "patrickjohncyh"}
    )
    
    DINOV2_VITS14: int = field(
        default=384,
        metadata={"model": "DINOv2 ViT-S/14", "type": "structural_embedding", "source": "facebookresearch"}
    )
    
    ONNX_FASHION_CLIP: int = field(
        default=768,
        metadata={"model": "Fashion-CLIP (ONNX)", "type": "optimized_embedding", "opset": 17}
    )
    
    RESNET50: int = field(
        default=2048,
        metadata={"model": "ResNet-50", "type": "cnn_baseline", "source": "torchvision"}
    )


@dataclass(frozen=True)
class OnnxConstants:
    """Engineering constants for ONNX Runtime integration."""
    
    OPSET_VERSION: int = field(
        default=17,
        metadata={"description": "Target ONNX operator set version (17+ supports Transformer layers)."}
    )


class Constants:
    """
    Static container for application-wide constants.
    Preserves the nested access pattern: Constants.Image.MEAN
    """
    Image = ImageConstants()
    Dimensions = DimensionConstants()
    Onnx = OnnxConstants()

    @classmethod
    def get_metadata(cls, group: str, field_name: str) -> Dict[str, Any]:
        """
        Retrieves metadata for a specific constant.
        Example: Constants.get_metadata("Image", "MEAN")
        """
        target_group = getattr(cls, group, None)
        if not target_group:
            return {}
        
        for f in fields(target_group):
            if f.name == field_name:
                return dict(f.metadata)
        return {}
