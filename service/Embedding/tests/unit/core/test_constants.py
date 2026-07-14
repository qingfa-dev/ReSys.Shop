"""
Unit tests for the Constants module.
Verifies access patterns and metadata retrieval.
"""
from embedding.core.constants import Constants


def test_constants_access_pattern():
    """Verify that constants are still accessible via dot notation."""
    assert Constants.Image.DEFAULT_SIZE == 224
    assert Constants.Dimensions.EFFICIENTNET_B0 == 1280
    assert Constants.Onnx.OPSET_VERSION == 17


def test_constants_metadata_retrieval():
    """Verify that metadata can be retrieved for specific fields."""
    meta = Constants.get_metadata("Image", "DEFAULT_SIZE")
    assert meta["unit"] == "px"
    assert "Standard input size" in meta["description"]

    dim_meta = Constants.get_metadata("Dimensions", "EFFICIENTNET_B0")
    assert dim_meta["model"] == "EfficientNet-B0"
    assert dim_meta["source"] == "torchvision"


def test_constants_invalid_metadata_retrieval():
    """Verify that invalid group or field returns empty dict."""
    assert Constants.get_metadata("NonExistent", "FIELD") == {}
    assert Constants.get_metadata("Image", "NonExistentField") == {}


def test_constants_are_frozen():
    """Verify that dataclasses are frozen (immutable)."""
    from dataclasses import FrozenInstanceError

    import pytest

    with pytest.raises(FrozenInstanceError):
        Constants.Image.DEFAULT_SIZE = 512
