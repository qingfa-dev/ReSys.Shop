"""
Utility functions for ONNX model processing.
"""
import onnxruntime as ort


def infer_onnx_dim(model_path: str) -> int:
    """
    Reads the output dimension directly from the ONNX graph metadata.
    Avoids the need for manual dimension configuration.
    """
    # Create session on CPU just to read metadata
    session = ort.InferenceSession(model_path, providers=["CPUExecutionProvider"])

    # Inspect the output node shape: e.g. [1, 512] or [None, 512]
    output_shape = session.get_outputs()[0].shape
    dim = output_shape[-1]

    # Guard: Ensure the dimension is a concrete integer
    if not isinstance(dim, int):
        raise ValueError(
            f"Cannot infer dim from ONNX output shape {output_shape}. "
            "Please ensure the model has a fixed output dimension."
        )
    return dim
