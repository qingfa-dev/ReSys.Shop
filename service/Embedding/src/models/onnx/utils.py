"""
Utility functions for ONNX model processing.
"""
import onnxruntime as ort


def infer_onnx_dim(model_path: str) -> int:
    """Reads the output dimension directly from the ONNX graph metadata.

    Avoids the need for manual dimension configuration by introspecting the
    model's output shape.

    Args:
        model_path: Absolute path to the .onnx model file.

    Returns:
        The output feature dimension (e.g. 512, 768, 1280).

    Raises:
        ValueError: If the output dimension is a dynamic value (not a fixed integer).
    """
    # Create: CPU-only session just to read metadata (no GPU needed)
    session = ort.InferenceSession(model_path, providers=["CPUExecutionProvider"])

    # Inspect: Read output node shape — expected to be [1, D] or [None, D]
    output_shape = session.get_outputs()[0].shape
    dim = output_shape[-1]

    # Guard: Ensure the dimension is a concrete integer, not a dynamic placeholder
    if not isinstance(dim, int):
        raise ValueError(
            f"Cannot infer dim from ONNX output shape {output_shape}. "
            "Please ensure the model has a fixed output dimension."
        )
    return dim
