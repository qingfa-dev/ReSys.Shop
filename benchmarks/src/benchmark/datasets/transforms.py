"""Image pre-processing transforms (not model-specific)."""

from __future__ import annotations

from PIL import Image


def resize_pad(image: Image.Image, size: int = 224) -> Image.Image:
    """Resize image to ``size x size`` preserving aspect ratio, pad with white."""
    image = image.convert("RGB")
    image.thumbnail((size, size), Image.LANCZOS)
    canvas = Image.new("RGB", (size, size), (255, 255, 255))
    offset = ((size - image.width) // 2, (size - image.height) // 2)
    canvas.paste(image, offset)
    return canvas


def center_crop(image: Image.Image, size: int = 224) -> Image.Image:
    """Resize short edge to size then center-crop to size x size."""
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
