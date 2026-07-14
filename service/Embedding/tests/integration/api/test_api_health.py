"""
Integration tests — health and error-handling endpoints.

These tests are fast (no model weights) and verify the service contract
for ops tooling: load balancers, k8s probes, and Aspire dashboards.
"""
from unittest.mock import MagicMock

import pytest
from embedding.api.routers.inference import get_engine
from embedding.schemas import Failure, ValueResult

pytestmark = pytest.mark.integration

EMBED_URL = "/embeddings"


class TestHealthEndpoint:
    def test_returns_200(self, client):
        response = client.get("/health")
        assert response.status_code == 200

    def test_body_contains_status_ok(self, client):
        body = client.get("/health").json()
        assert body["status"] == "ok"

    def test_body_contains_service_name(self, client):
        body = client.get("/health").json()
        assert body["service"] == "Embedding Service"

    def test_health_is_idempotent(self, client):
        """Multiple consecutive calls must all return 200."""
        for _ in range(3):
            assert client.get("/health").status_code == 200


class TestErrorPropagation:
    def test_invalid_model_returns_404_not_200(self, authed_client):
        """
        The corrected endpoint must propagate the status code
        from the ValueResult to the HTTP layer.
        """
        response = authed_client.post(
            EMBED_URL,
            json={"image_url": "http://example.com/img.jpg", "model": "invalid_model_xyz"},
        )
        assert response.status_code == 404

    def test_invalid_model_detail_contains_failure_info(self, authed_client):
        response = authed_client.post(
            EMBED_URL,
            json={"image_url": "http://example.com/img.jpg", "model": "invalid_model_xyz"},
        )
        body = response.json()
        assert body["isSuccess"] is False
        assert body["failures"][0]["code"] == "Model.NotFound"

    def test_bad_image_url_returns_4xx(self, app, authed_client):
        """
        An image that cannot be fetched should surface as a 4xx, not a 500.
        """
        mock_engine = MagicMock()
        mock_engine.embed.return_value = ValueResult.failure_value(
            Failure.bad_request("Image.LoadError", "Connection refused")
        )

        # Use FastAPI dependency overrides for reliable patching
        app.dependency_overrides[get_engine] = lambda: mock_engine
        try:
            response = authed_client.post(
                EMBED_URL,
                json={"image_url": "http://invalid.invalid/img.jpg", "model": "efficientnet_b0"},
            )
            assert response.status_code == 400
            assert response.json()["failures"][0]["code"] == "Image.LoadError"
        finally:
            app.dependency_overrides.clear()

    def test_internal_engine_error_returns_500(self, app, authed_client):
        mock_engine = MagicMock()
        mock_engine.embed.return_value = ValueResult.failure_value(
            Failure.internal_error("Inference.Error", "CUDA OOM")
        )

        app.dependency_overrides[get_engine] = lambda: mock_engine
        try:
            response = authed_client.post(
                EMBED_URL,
                json={"image_url": "http://example.com/img.jpg", "model": "efficientnet_b0"},
            )
            assert response.status_code == 500
            assert response.json()["isSuccess"] is False
        finally:
            app.dependency_overrides.clear()

    def test_unknown_route_returns_404(self, client):
        response = client.get("/api/does_not_exist")
        assert response.status_code == 404

    def test_get_on_post_only_route_returns_405(self, authed_client):
        response = authed_client.get(EMBED_URL)
        assert response.status_code == 405
