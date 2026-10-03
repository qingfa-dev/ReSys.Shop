"""
Integration tests — API request validation.
"""
import pytest

pytestmark = pytest.mark.integration


class TestRequestValidation:
    def test_missing_image_url_returns_400(self, client):
        """FastAPI triggers a 422 which our handler converts to a 400 Result."""
        payload = {"model_name": "efficientnet_b0"}
        response = client.post("/embeddings", json=payload)
        assert response.status_code == 400
        body = response.json()
        assert body["isSuccess"] is False
        assert "Request.ValidationError" in body["errors"][0]["code"]

    def test_empty_body_returns_400(self, client):
        response = client.post("/embeddings", json={})
        assert response.status_code == 400

    def test_invalid_json_returns_400(self, client):
        headers = {"Content-Type": "application/json"}
        response = client.post("/embeddings", data="not-json", headers=headers)
        # Invalid JSON is a parse error, usually 400
        assert response.status_code == 400

    def test_invalid_content_type_returns_400(self, client):
        payload = {"image_url": "http://x.com/i.jpg"}
        headers = {"Content-Type": "text/plain"}
        response = client.post("/embeddings", data=str(payload), headers=headers)
        # FastAPI returns 422 if body doesn't match, our handler makes it 400
        assert response.status_code == 400

    def test_health_endpoint_is_public(self, client):
        """Health check should be accessible without any auth."""
        response = client.get("/health")
        assert response.status_code == 200
