from fastapi.testclient import TestClient


class TestEmbedEndpoint:
    def test_embed_returns_expected_shape(self, client: TestClient) -> None:
        response = client.post(
            "/embeddings",
            json={"image_url": "http://example.com/img.jpg", "model": "openclip-vit-b-32"},
        )
        assert response.status_code == 200
        data = response.json()
        assert data["isSuccess"] is True
        assert data["value"] is not None
        assert "vector" in data["value"]
        assert data["value"]["dimension"] == 512

    def test_embed_from_bytes_returns_expected_shape(self, client: TestClient) -> None:
        response = client.post(
            "/embeddings/bytes",
            files={"image": ("test.jpg", b"fake-image-bytes", "image/jpeg")},
            data={"model": "openclip-vit-b-32"},
        )
        assert response.status_code == 200
        data = response.json()
        assert data["isSuccess"] is True
        assert data["value"] is not None
        assert "vector" in data["value"]
        assert data["value"]["dimension"] == 512


class TestModelEndpoint:
    def test_list_models_returns_expected_shape(self, client: TestClient) -> None:
        response = client.get("/models")
        assert response.status_code == 200
        data = response.json()
        assert data["isSuccess"] is True
        assert data["value"] is not None
        assert isinstance(data["value"], list)
        assert len(data["value"]) > 0
        for model in data["value"]:
            assert "id" in model
            assert "name" in model
            assert "dimension" in model
