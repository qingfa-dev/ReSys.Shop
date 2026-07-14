"""
Unit tests for BaseEmbedder.
Uses a MockEmbedder so no real model weights are downloaded.
"""
import math
import pytest
import numpy as np
import torch
from PIL import Image
from unittest.mock import patch, MagicMock

from embedding.models.base import BaseEmbedder
from embedding.schemas import ValueResult


# ── Test double ───────────────────────────────────────────────────────────────

class MockEmbedder(BaseEmbedder):
    """Minimal concrete subclass — returns a constant tensor of ones."""

    def _forward(self, image: Image.Image) -> torch.Tensor:
        return torch.ones((1, self.dim))


class BrokenEmbedder(BaseEmbedder):
    """_forward raises an exception to simulate inference failure."""

    def _forward(self, image: Image.Image):
        raise RuntimeError("GPU out of memory")


# ── Initialisation ────────────────────────────────────────────────────────────

class TestBaseEmbedderInit:
    def test_name_and_dim_stored(self):
        e = MockEmbedder("my_model", 128)
        assert e.name == "my_model"
        assert e.dim == 128

    def test_device_is_lazy(self):
        """Device should not be resolved until first access."""
        e = MockEmbedder("m", 8)
        assert e._device is None
        # Accessing the property triggers resolution
        _ = e.device
        assert e._device is not None

    def test_device_is_cpu_or_cuda(self):
        e = MockEmbedder("m", 8)
        assert str(e.device) in ("cpu", "cuda:0", "cuda")


# ── Image loading ─────────────────────────────────────────────────────────────

class TestLoadImage:
    def test_load_pil_image_directly(self):
        e = MockEmbedder("m", 8)
        img = Image.new("RGB", (20, 20), color="blue")
        result = e._load_image(img)
        assert result.is_success is True
        assert isinstance(result.value, Image.Image)

    def test_load_converts_to_rgb(self):
        """RGBA images must be converted to RGB."""
        e = MockEmbedder("m", 8)
        rgba = Image.new("RGBA", (10, 10))
        result = e._load_image(rgba)
        assert result.is_success is True
        assert result.value.mode == "RGB"

    def test_load_bytes(self):
        import io
        e = MockEmbedder("m", 8)
        buf = io.BytesIO()
        Image.new("RGB", (10, 10)).save(buf, format="PNG")
        result = e._load_image(buf.getvalue())
        assert result.is_success is True
        assert isinstance(result.value, Image.Image)

    def test_load_invalid_path_returns_failure(self):
        e = MockEmbedder("m", 8)
        result = e._load_image("non_existent_file.jpg")
        assert result.is_success is False
        assert result.failures[0].code == "Image.LoadError"

    def test_load_unsupported_type_returns_failure(self):
        e = MockEmbedder("m", 8)
        result = e._load_image(12345)  # type: ignore
        assert result.is_success is False
        assert result.failures[0].code == "Image.InputError"

    def test_load_url_success(self):
        """Mock HTTP so the test is fast and offline-safe."""
        import io
        e = MockEmbedder("m", 8)
        fake_img = Image.new("RGB", (10, 10))
        buf = io.BytesIO()
        fake_img.save(buf, format="JPEG")
        buf.seek(0)

        mock_response = MagicMock()
        mock_response.content = buf.read()
        mock_response.raise_for_status = MagicMock()

        with patch("httpx.Client.get", return_value=mock_response):
            result = e._load_image("http://example.com/img.jpg")

        assert result.is_success is True

    def test_load_url_network_error_returns_failure(self):
        import httpx
        e = MockEmbedder("m", 8)
        with patch("httpx.Client.get", side_effect=httpx.ConnectError("timeout")):
            result = e._load_image("http://unreachable.invalid/img.jpg")
        assert result.is_success is False
        assert result.failures[0].code == "Image.LoadError"


# ── Normalisation ─────────────────────────────────────────────────────────────

class TestNormalize:
    def test_output_is_unit_vector(self):
        e = MockEmbedder("m", 5)
        tensor = torch.ones((1, 5))
        vector = e._normalize(tensor)
        l2 = sum(x * x for x in vector)
        assert l2 == pytest.approx(1.0, rel=1e-5)

    def test_output_is_python_list_of_floats(self):
        e = MockEmbedder("m", 4)
        vector = e._normalize(torch.tensor([[1.0, 2.0, 3.0, 4.0]]))
        assert isinstance(vector, list)
        assert all(isinstance(v, float) for v in vector)

    def test_normalize_numpy_array(self):
        e = MockEmbedder("m", 3)
        arr = np.array([3.0, 4.0, 0.0])
        vector = e._normalize(arr)
        assert len(vector) == 3
        assert sum(x * x for x in vector) == pytest.approx(1.0, rel=1e-5)

    def test_normalize_near_zero_vector_does_not_divide_by_zero(self):
        """The 1e-9 epsilon must prevent NaN on a zero vector."""
        e = MockEmbedder("m", 3)
        vector = e._normalize(torch.zeros((1, 3)))
        assert all(math.isfinite(v) for v in vector)

    def test_normalize_flattens_2d_tensor(self):
        e = MockEmbedder("m", 4)
        tensor = torch.ones((2, 2))  # 2D, should flatten to length 4
        vector = e._normalize(tensor)
        assert len(vector) == 4


# ── End-to-end extract() ──────────────────────────────────────────────────────

class TestExtract:
    def test_extract_from_pil_image_returns_normalised_vector(self):
        e = MockEmbedder("m", 5)
        img = Image.new("RGB", (10, 10), color="red")
        result = e.extract(img)
        assert result.is_success is True
        assert len(result.value) == 5
        assert sum(x * x for x in result.value) == pytest.approx(1.0, rel=1e-4)

    def test_extract_propagates_load_failure(self):
        e = MockEmbedder("m", 128)
        result = e.extract("definitely_not_a_real_file.jpg")
        assert result.is_success is False
        assert "Image.LoadError" in result.failures[0].code

    def test_extract_wraps_forward_exception_in_failure(self):
        e = BrokenEmbedder("broken", 8)
        img = Image.new("RGB", (10, 10))
        result = e.extract(img)
        assert result.is_success is False
        assert result.failures[0].code == "Inference.Error"
        assert "GPU out of memory" in result.failures[0].description

    def test_forward_not_implemented_raises(self):
        """Calling _forward directly on the abstract base must raise NotImplementedError."""
        class Incomplete(BaseEmbedder):
            pass

        e = Incomplete("incomplete", 10)
        img = Image.new("RGB", (10, 10))
        with pytest.raises(NotImplementedError):
            e._forward(img)

    def test_extract_deterministic_same_image(self):
        """Same image → same vector on every call (no stochastic layers)."""
        e = MockEmbedder("m", 16)
        img = Image.new("RGB", (10, 10), color=(100, 150, 200))
        r1 = e.extract(img)
        r2 = e.extract(img)
        assert r1.value == r2.value
