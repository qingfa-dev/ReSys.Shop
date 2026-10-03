"""Image pre-processing transforms (not model-specific).

Provides two common resizing strategies used across all model adapters:
``resize_pad`` (preserve aspect ratio, pad shorter side) and ``center_crop``
(resize short edge to target, then centre-crop). Neither applies model-specific
normalisation — that is handled by each adapter's ``preprocess``.

Edge cases:
- Input images may be any size or aspect ratio; both transforms always produce
  exactly ``size x size`` RGB output.
- Very small inputs (< size on both dimensions) are upscaled via LANCZOS.
"""
from __future__ import annotations

from PIL import Image


def resize_pad(image: Image.Image, size: int = 224) -> Image.Image:
    """Resize image to ``size x size`` preserving aspect ratio, pad with white.

    Args:
        image: Input PIL image in any mode.
        size: Target square dimension (default 224).

    Returns:
        RGB image of exactly ``(size, size)`` with white padding on the
        shorter side.
    """
    image = image.convert("RGB")
    image.thumbnail((size, size), Image.LANCZOS)
    canvas = Image.new("RGB", (size, size), (255, 255, 255))
    offset = ((size - image.width) // 2, (size - image.height) // 2)
    canvas.paste(image, offset)
    return canvas


def center_crop(image: Image.Image, size: int = 224) -> Image.Image:
    """Resize short edge to size then centre-crop to size x size.

    Args:
        image: Input PIL image in any mode.
        size: Target square dimension (default 224).

    Returns:
        RGB image of exactly ``(size, size)`` cropped from the centre of
        the resized image.
    """
    image = image.convert("RGB")
    w, h = image.size
    if w < h:
        new_w, new_h = size, int(h * size / w)
    else:
        new_w, new_h = int(w * size / h), size
    image = image.resize((new_w, new_h), Image.LANCZOS)
    left = (new_w - size) // 2
    top = (new_h - size) // 2
    return image.crop((left, top, left + size, top + size))
