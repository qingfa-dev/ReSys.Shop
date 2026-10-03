"""
Generic ONNX Runtime wrapper for serving optimized model artifacts.
"""
import logging
from pathlib import Path

from embedding.core.constants import Constants
from embedding.models.base import BaseEmbedder
from embedding.models.registry import ModelRegistry

logger = logging.getLogger(__name__)

@ModelRegistry.register(
    "onnx",
    metadata={
        "name": "Generic ONNX Wrapper",
        "description": "Optimized inference using ONNX Runtime.",
        "tags": ["onnx", "optimized"]
    }
)
class OnnxEmbedder(BaseEmbedder):
    """
    Generic ONNX Runtime wrapper for optimized models.
    """

    def __init__(self, model_path: str, dim: int, input_size: int = Constants.Image.DEFAULT_SIZE):
        """Initialize: ONNX Runtime session with GPU/CPU provider selection and preprocessing.

        Args:
            model_path: Absolute path to the .onnx model file.
            dim: Output embedding dimension from the ONNX model.
            input_size: Input image size in pixels (default 224).

        Raises:
            FileNotFoundError: If the .onnx file does not exist at model_path.
        """
        name = Path(model_path).stem
        super().__init__(name, dim)

        import time

        import onnxruntime as ort
        from embedding.models.base import model_init_duration
        from torchvision import transforms

        # Guard: Ensure the ONNX model artifact exists on disk
        if not Path(model_path).exists():
            raise FileNotFoundError(f"ONNX model not found: {model_path}")

        start_init = time.perf_counter()
        # Compute: Select best ONNX execution providers — CUDA if GPU available, else CPU
        providers = (
            ["CUDAExecutionProvider", "CPUExecutionProvider"]
            if ort.get_device() == "GPU"
            else ["CPUExecutionProvider"]
        )
        # Create: ONNX Runtime inference session
        self.session = ort.InferenceSession(model_path, providers=providers)
        # Assign: Metadata about the input node name for inference calls
        self.input_name = self.session.get_inputs()[0].name

        duration = (time.perf_counter() - start_init) * 1000
        model_init_duration.record(duration, {"model": self.name, "type": "onnx"})
        logger.info(f"[{self.name}] ONNX Session initialized in {duration:.2f}ms")

        # Initialize: Image preprocessing pipeline matching ONNX input expectations
        self.preprocess = transforms.Compose([
            transforms.Resize(
                input_size
                + (Constants.Image.RESIZE_SIZE - Constants.Image.DEFAULT_SIZE)
            ),
            transforms.CenterCrop(input_size),
            transforms.ToTensor(),
            transforms.Normalize(mean=Constants.Image.MEAN, std=Constants.Image.STD),
        ])

    def _forward(self, image):
        """Executes inference via ONNX Runtime session.

        Args:
            image: Preprocessed PIL Image ready for inference.

        Returns:
            Raw output tensor from the ONNX model graph.
        """
        # Transform: Convert PIL image to [1, 3, H, W] float32 numpy array
        tensor = self.preprocess(image).unsqueeze(0).numpy()
        # Call: Execute the ONNX computation graph
        outputs = self.session.run(None, {self.input_name: tensor})

        # Check: Handle models with multiple output nodes — return the last one
        if len(outputs) > 1:
            return outputs[-1]
        return outputs[0]
