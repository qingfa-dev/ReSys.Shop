"""
Centralized architectural and mathematical constants for inference.
Organized into immutable dataclasses with rich metadata for each field.
"""
from dataclasses import dataclass, field, fields
from typing import Any, Dict, List


@dataclass(frozen=True)
class ImageConstants:
    """Constants related to image preprocessing and dataset standards.

    Invariant: All instances are frozen — compile-time safe for concurrent access.
    """

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
        metadata={
            "description": "Standard input size for most vision models (pixels).",
            "unit": "px",
        }
    )

    RESIZE_SIZE: int = field(
        default=256,
        metadata={"description": "Initial resize dimensions before center cropping.", "unit": "px"}
    )


@dataclass(frozen=True)
class DimensionConstants:
    """Fixed output vector dimensions for supported machine learning models.

    Invariant: Values match the output dimension of each model's embedding layer.
    """

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
        metadata={
            "model": "Fashion-CLIP",
            "type": "domain_specific_embedding",
            "source": "patrickjohncyh",
        }
    )

    DINOV2_VITS14: int = field(
        default=384,
        metadata={
            "model": "DINOv2 ViT-S/14",
            "type": "structural_embedding",
            "source": "facebookresearch",
        }
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
    """Engineering constants for ONNX Runtime integration.

    Invariant: Values are frozen and match the ONNX opset version used during export.
    """

    OPSET_VERSION: int = field(
        default=17,
        metadata={
            "description": (
                "Target ONNX operator set version (17+ supports Transformer layers)."
            )
        }
    )


@dataclass(frozen=True)
class DefaultConstants:
    """Application configuration defaults.

    These match the pydantic-settings defaults in ``config.py``.
    """

    PROJECT_NAME: str = field(default="Embedding Service")
    PORT: int = field(default=8000, metadata={"ge": 1, "le": 65535})
    HTTPS_PORT: int = field(default=8001, metadata={"ge": 1, "le": 65535})
    RATE_LIMIT: str = field(default="50/minute")
    EMBEDDING_MODEL: str = field(default="fashion_clip")
    OMP_NUM_THREADS: int = field(default=4, metadata={"ge": 1, "le": 128})
    MKL_NUM_THREADS: int = field(default=4, metadata={"ge": 1, "le": 128})
    NUMEXPR_NUM_THREADS: int = field(default=4, metadata={"ge": 1, "le": 128})
    CORS_ORIGINS: list[str] = field(
        default_factory=lambda: ["http://localhost:3000", "http://localhost:5173"],
    )
    OTLP_ENDPOINT: str = field(default="http://localhost:4317")
    LOG_LEVEL: str = field(default="INFO")


@dataclass(frozen=True)
class ConstraintConstants:
    """Validation bounds and limits."""

    PORT_MIN: int = 1
    PORT_MAX: int = 65535
    API_KEY_MIN_LENGTH: int = 16
    THREAD_COUNT_MIN: int = 1
    THREAD_COUNT_MAX: int = 128
    L2_EPSILON: float = 1e-9
    HTTP_TIMEOUT: int = 10
    ONNX_MIN_FILE_SIZE: int = 1_048_576


@dataclass(frozen=True)
class StringConstants:
    """Shared string literals used across the service."""

    X_API_KEY_HEADER: str = "X-API-Key"
    VERSION: str = "1.0.0"
    ONNX_FILENAME: str = "model.onnx"
    ONNX_PREFIX: str = "onnx/"
    USER_AGENT: str = "Mozilla/5.0 inference/1.0"


@dataclass(frozen=True)
class ErrorCodeConstants:
    """Domain error code strings used in API responses."""

    MODEL_NOT_FOUND: str = "Model.NotFound"
    INFERENCE_ERROR: str = "Inference.Error"
    INFERENCE_DEVICE_ERROR: str = "Inference.DeviceError"
    MODEL_LOAD_ERROR: str = "Model.LoadError"
    SERVER_ERROR: str = "Server.Error"
    ROUTE_NOT_FOUND: str = "Route.NotFound"
    AUTH_UNAUTHORIZED: str = "Auth.Unauthorized"
    AUTH_FORBIDDEN: str = "Auth.Forbidden"
    HTTP_ERROR: str = "Http.Error"
    IMAGE_LOAD_ERROR: str = "Image.LoadError"
    IMAGE_INPUT_ERROR: str = "Image.InputError"
    REGISTRY_ERROR: str = "Registry.Error"
    REQUEST_VALIDATION_ERROR: str = "Request.ValidationError"


@dataclass(frozen=True)
class HttpStatusConstants:
    """HTTP status code constants mapped from ErrorType."""

    OK: int = 200
    BAD_REQUEST: int = 400
    UNAUTHORIZED: int = 401
    FORBIDDEN: int = 403
    NOT_FOUND: int = 404
    CONFLICT: int = 409
    INTERNAL_ERROR: int = 500


class Constants:
    """
    Static container for application-wide constants.
    Preserves the nested access pattern: Constants.Image.MEAN
    """
    Image = ImageConstants()
    Dimensions = DimensionConstants()
    Onnx = OnnxConstants()
    Defaults = DefaultConstants()
    Constraints = ConstraintConstants()
    Strings = StringConstants()
    Errors = ErrorCodeConstants()
    HttpStatus = HttpStatusConstants()

    @classmethod
    def get_metadata(cls, group: str, field_name: str) -> Dict[str, Any]:
        """Retrieves metadata for a specific constant field.

        Args:
            group: The attribute name on Constants (e.g. 'Image', 'Dimensions').
            field_name: The field name within the target dataclass (e.g. 'MEAN').

        Returns:
            Dict of metadata for the matched field, or empty dict if not found.
        """
        target_group = getattr(cls, group, None)
        if not target_group:
            return {}

        for f in fields(target_group):
            if f.name == field_name:
                return dict(f.metadata)
        return {}
