"""
Integration tests for POST /models/onnx/export endpoint.
"""
import pytest
from embedding.models.onnx.export_state import ExportState


@pytest.fixture(autouse=True)
def reset_export_state():
    """Reset singleton between integration tests."""
    ExportState._instance = None
    yield
    ExportState._instance = None


class TestOnnxExportEndpoint:
    """Tests for POST /models/onnx/export."""

    def test_requires_api_key(self, client):
        """Missing API key returns 403."""
        response = client.post("/models/onnx/export")
        assert response.status_code == 403

    def test_wrong_api_key_returns_403(self, client):
        """Wrong API key returns 403."""
        response = client.post(
            "/models/onnx/export",
            headers={"X-API-Key": "wrong-key-1234567890"},
        )
        assert response.status_code == 403

    def test_returns_value_result_envelope(self, authed_client):
        """Response is wrapped in ValueResult envelope."""
        response = authed_client.post("/models/onnx/export")
        assert response.status_code in (200, 202)
        body = response.json()
        assert body["isSuccess"] is True
        assert "value" in body
        assert body["value"] is not None

    def test_first_call_returns_202(self, authed_client):
        """First call starts export and returns 202 Accepted."""
        response = authed_client.post("/models/onnx/export")
        assert response.status_code == 202
        body = response.json()
        assert body["value"]["overallStatus"] == "running"
        assert len(body["value"]["models"]) == 4

    def test_subsequent_call_returns_200(self, authed_client):
        """Second call while running returns 200 with current status."""
        # Start export
        authed_client.post("/models/onnx/export")
        # Second call
        response = authed_client.post("/models/onnx/export")
        assert response.status_code == 200
        body = response.json()
        assert body["isSuccess"] is True
        assert body["value"]["overallStatus"] == "running"

    def test_response_has_all_models(self, authed_client):
        """Response includes all 4 model entries."""
        response = authed_client.post("/models/onnx/export")
        body = response.json()
        model_names = [m["modelName"] for m in body["value"]["models"]]
        assert "efficientnet_b0" in model_names
        assert "clip_vit_b16" in model_names
        assert "fashion_clip" in model_names
        assert "dinov2_vits14" in model_names

    def test_response_has_start_time(self, authed_client):
        """Response includes start_time when export is running."""
        response = authed_client.post("/models/onnx/export")
        body = response.json()
        assert body["value"]["startTime"] is not None

    def test_health_endpoint_unaffected(self, client):
        """Health endpoint still works (no route conflict)."""
        response = client.get("/health")
        assert response.status_code == 200
        assert response.json()["status"] == "ok"
