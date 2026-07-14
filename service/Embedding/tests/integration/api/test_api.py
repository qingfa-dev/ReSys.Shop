import pytest
from embedding.core.config import settings
from embedding.main import app
from fastapi.testclient import TestClient

client = TestClient(app)

def test_health_check():
    """Verify the service is up and telemetry is initialized."""
    response = client.get("/health")
    assert response.status_code == 200
    assert response.json()["service"] == "Inference"

@pytest.mark.parametrize("model_name, expected_dim", [
    ("efficientnet_b0", 1280),
    ("fashion_clip", 512)
])
def test_embeddings_real_models(model_name, expected_dim):
    """
    Verify that real models can be loaded and produce valid embeddings.
    """
    payload = {
        "image_url": "https://images.unsplash.com/photo-1517841905240-472988babdf9?auto=format&fit=crop&q=80&w=1000",
        "model": model_name
    }
    headers = {"X-API-Key": settings.API_KEY}

    response = client.post("/embeddings", json=payload, headers=headers)

    assert response.status_code == 200
    data = response.json()

    # Verify Result Pattern
    assert data["isSuccess"] is True
    assert data["statusCode"] == 200

    # Verify Data
    val = data["value"]
    assert val["dimension"] == expected_dim
    assert len(val["vector"]) == expected_dim

    # Verify Normalization (Sum of squares should be approx 1.0)
    l2_norm = sum(x*x for x in val["vector"])
    assert l2_norm == pytest.approx(1.0, rel=1e-3)

def test_embeddings_unauthorized():
    """Verify that requests without API keys are rejected."""
    payload = {"image_url": "http://test.com/img.jpg", "model": "efficientnet_b0"}
    response = client.post("/embeddings", json=payload)
    assert response.status_code == 403

def test_invalid_model_returns_failure_result():
    """Verify that requesting an invalid model returns a 404."""
    payload = {"image_url": "http://test.com/img.jpg", "model": "invalid_model_name"}
    headers = {"X-API-Key": settings.API_KEY}

    response = client.post("/embeddings", json=payload, headers=headers)

    assert response.status_code == 404
    data = response.json()
    # If the response is a ValueResult (isSuccess=False), it will have 'failures'
    assert data["isSuccess"] is False
    assert data["failures"][0]["code"] == "Model.NotFound"
