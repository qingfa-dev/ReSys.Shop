"""Integration tests for the image upload and serve endpoints."""
import io
import pytest
from PIL import Image
from fastapi.testclient import TestClient


@pytest.fixture
def png_bytes() -> bytes:
    img = Image.new("RGB", (10, 10), color="red")
    buf = io.BytesIO()
    img.save(buf, format="PNG")
    return buf.getvalue()


@pytest.fixture
def jpeg_bytes() -> bytes:
    img = Image.new("RGB", (10, 10), color="blue")
    buf = io.BytesIO()
    img.save(buf, format="JPEG")
    return buf.getvalue()


class TestImageUpload:
    def test_upload_png_success(self, authed_client: TestClient, png_bytes: bytes):
        res = authed_client.post(
            "/images/upload",
            files={"image": ("test.png", png_bytes, "image/png")},
        )
        assert res.status_code == 200
        data = res.json()
        assert data["isSuccess"] is True
        assert data["value"]["url"].startswith("/images/")

    def test_upload_jpeg_success(self, authed_client: TestClient, jpeg_bytes: bytes):
        res = authed_client.post(
            "/images/upload",
            files={"image": ("photo.jpg", jpeg_bytes, "image/jpeg")},
        )
        assert res.status_code == 200
        assert res.json()["isSuccess"] is True

    def test_upload_rejects_unauthorized(self, client: TestClient, png_bytes: bytes):
        res = client.post(
            "/images/upload",
            files={"image": ("test.png", png_bytes, "image/png")},
        )
        assert res.status_code == 403

    def test_upload_rejects_non_image(self, authed_client: TestClient):
        res = authed_client.post(
            "/images/upload",
            files={"image": ("test.txt", b"hello", "text/plain")},
        )
        assert res.status_code == 400
        data = res.json()
        assert data["isSuccess"] is False

    def test_upload_rejects_large_file(self, authed_client: TestClient):
        big = b"x" * (11 * 1024 * 1024)  # 11 MB
        res = authed_client.post(
            "/images/upload",
            files={"image": ("big.png", big, "image/png")},
        )
        assert res.status_code == 413

    def test_serve_uploaded_image(self, authed_client: TestClient, png_bytes: bytes):
        upload = authed_client.post(
            "/images/upload",
            files={"image": ("serve_test.png", png_bytes, "image/png")},
        )
        url = upload.json()["value"]["url"]

        res = authed_client.get(url)
        assert res.status_code == 200
        assert res.content == png_bytes

    def test_serve_nonexistent_returns_404(self, authed_client: TestClient):
        res = authed_client.get("/images/nonexistent.png")
        assert res.status_code == 404
        data = res.json()
        assert data["isSuccess"] is False
