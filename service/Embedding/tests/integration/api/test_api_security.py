"""
Integration tests — API security and authentication.
"""
import pytest
from embedding.core.config import settings

pytestmark = pytest.mark.integration


class TestApiKeyAuthentication:
    def test_missing_api_key_returns_403(self, client):
        payload = {"image_url": "http://example.com/img.jpg", "model": "efficientnet_b0"}
        response = client.post("/inference/embeddings", json=payload)
        assert response.status_code == 403

    def test_wrong_api_key_returns_403(self, client):
        payload = {"image_url": "http://example.com/img.jpg", "model": "efficientnet_b0"}
        headers = {"X-API-Key": "wrong-key"}
        response = client.post("/inference/embeddings", json=payload, headers=headers)
        assert response.status_code == 403

    def test_empty_api_key_returns_403(self, client):
        payload = {"image_url": "http://example.com/img.jpg", "model": "efficientnet_b0"}
        headers = {"X-API-Key": ""}
        response = client.post("/inference/embeddings", json=payload, headers=headers)
        assert response.status_code == 403

    def test_key_in_wrong_header_returns_403(self, client):
        payload = {"image_url": "http://example.com/img.jpg", "model": "efficientnet_b0"}
        headers = {"Authorization": f"Bearer {settings.API_KEY}"}
        response = client.post("/inference/embeddings", json=payload, headers=headers)
        assert response.status_code == 403

    def test_valid_api_key_is_accepted(self, authed_client, mock_engine):
        """
        Using the authed_client fixture (which has the correct X-API-Key).
        We mock the engine to return a dummy success so we don't hit real ML.
        """
        payload = {"image_url": "http://example.com/img.jpg", "model": "efficientnet_b0"}
        response = authed_client.post("/inference/embeddings", json=payload)
        # Should NOT be 403. Success or 404/500 depending on mock.
        assert response.status_code != 403

    def test_health_endpoint_is_public(self, client):
        """Health check shouldn't require an API key."""
        response = client.get("/health")
        assert response.status_code == 200

    def test_403_body_contains_detail(self, client):
        """Verify that security failures still return our standardized Result body."""
        payload = {"image_url": "http://example.com/img.jpg", "model": "efficientnet_b0"}
        response = client.post("/inference/embeddings", json=payload)
        
        body = response.json()
        assert body["isSuccess"] is False
        assert body["statusCode"] == 403
        # In our global handler, 403 from FastAPI/Security returns a failure
        assert len(body["failures"]) > 0
        assert any("Forbidden" in f["code"] or "Forbidden" in f["description"] for f in body["failures"])


class TestRequestValidation:
    def test_missing_image_url_returns_400(self, authed_client):
        """FastAPI triggers a 422 which our handler converts to a 400 Result."""
        payload = {"model": "efficientnet_b0"}
        response = authed_client.post("/inference/embeddings", json=payload)
        assert response.status_code == 400
        body = response.json()
        assert body["isSuccess"] is False
        assert "Request.ValidationError" in body["failures"][0]["code"]

    def test_empty_body_returns_400(self, authed_client):
        response = authed_client.post("/inference/embeddings", json={})
        assert response.status_code == 400

    def test_invalid_json_returns_400(self, authed_client):
        headers = {"Content-Type": "application/json"}
        response = authed_client.post("/inference/embeddings", data="not-json", headers=headers)
        # Invalid JSON is a parse error, usually 400
        assert response.status_code == 400

    def test_invalid_content_type_returns_400(self, authed_client):
        payload = {"image_url": "http://x.com/i.jpg"}
        headers = {"Content-Type": "text/plain"}
        response = authed_client.post("/inference/embeddings", data=str(payload), headers=headers)
        # FastAPI returns 422 if body doesn't match, our handler makes it 400
        assert response.status_code == 400
