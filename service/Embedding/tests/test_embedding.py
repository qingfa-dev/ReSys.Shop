from fastapi.testclient import TestClient


class TestEmbedEndpoint:
    def test_embed_returns_expected_shape(self, client: TestClient) -> None:
        response = client.post("/api/v1/embeddings/embed")
        assert response.status_code == 200
        data = response.json()
        assert "embedding" in data
        assert data["embedding"] == []
        assert data["dim"] == 512

    def test_embed_batch_returns_expected_shape(self, client: TestClient) -> None:
        response = client.post("/api/v1/embeddings/embed/batch")
        assert response.status_code == 200
        data = response.json()
        assert "embeddings" in data
        assert data["embeddings"] == []
        assert data["dim"] == 512
