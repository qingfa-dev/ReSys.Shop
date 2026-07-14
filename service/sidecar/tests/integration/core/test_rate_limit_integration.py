"""
Integration tests for Rate Limiting.
Verifies that the service correctly returns 429 Too Many Requests.
"""
import pytest
from src.core.config import settings
from tests.conftest import TEST_IMAGE_URL

@pytest.mark.integration
class TestRateLimiting:
    def test_rate_limit_exceeded(self, client):
        """
        Hit the embeddings endpoint repeatedly until we exceed the limit.
        """
        headers = {"X-API-Key": settings.API_KEY}
        payload = {
            "image_url": TEST_IMAGE_URL,
            "model": "efficientnet_b0"
        }
        
        # Increase limit count to ensure we hit it even if some requests were already made.
        # Default is 50/minute, so 100 should be plenty.
        limit_count = 100 
        found_429 = False
        
        for _ in range(limit_count):
            response = client.post("/inference/embeddings", json=payload, headers=headers)
            if response.status_code == 429:
                found_429 = True
                break
            # Handled errors or success are fine as long as they hit the limiter
            assert response.status_code in [200, 404, 403, 400]

        assert found_429, f"Rate limit (429) was not triggered after {limit_count} requests."

    def test_health_check_bypasses_limiter(self, client):
        """
        Verify that critical infrastructure endpoints (health) are NOT rate limited.
        """
        for _ in range(10):
            response = client.get("/health")
            assert response.status_code == 200
