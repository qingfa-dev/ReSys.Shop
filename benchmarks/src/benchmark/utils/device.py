"""Torch device resolution with human-readable reporting.

Resolves the best available device (CUDA > MPS > CPU) and provides a
single function used by all model adapters.

Edge cases:
- Preference ``"cuda"`` raises RuntimeError if no GPU is available.
- Preference ``"mps"`` raises RuntimeError on non-Apple hardware.
- ``"auto"`` silently degrades to CPU when no accelerator is found.
"""

from __future__ import annotations

from benchmark.utils.logging import get_logger

logger = get_logger("utils.device")


def resolve_device(preference: str = "auto") -> "torch.device":  # type: ignore[name-defined]
    """Return the best available torch.device.

    Args:
        preference: ``"auto"`` | ``"cpu"`` | ``"cuda"`` | ``"mps"``.

    Returns:
        A ``torch.device`` instance.

    Raises:
        RuntimeError: If the requested device is not available.
    """
    import torch

    if preference == "cpu":
        logger.info("Device: CPU (forced)")
        return torch.device("cpu")

    if preference == "cuda":
        if not torch.cuda.is_available():
            raise RuntimeError("CUDA requested but not available")
        dev = torch.device("cuda")
        logger.info("Device: %s (%s)", dev, torch.cuda.get_device_name(0))
        return dev

    if preference == "mps":
        if not (hasattr(torch.backends, "mps") and torch.backends.mps.is_available()):
            raise RuntimeError("MPS requested but not available")
        logger.info("Device: MPS (Apple Silicon)")
        return torch.device("mps")

    # auto
    if torch.cuda.is_available():
        dev = torch.device("cuda")
        logger.info("Device: CUDA — %s", torch.cuda.get_device_name(0))
        return dev
    if hasattr(torch.backends, "mps") and torch.backends.mps.is_available():
        logger.info("Device: MPS (Apple Silicon)")
        return torch.device("mps")

    logger.info("Device: CPU (no GPU found)")
    return torch.device("cpu")
