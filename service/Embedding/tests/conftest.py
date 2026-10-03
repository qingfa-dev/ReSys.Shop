"""
Root conftest.py — must be the very first thing pytest loads.

Environment variables are injected here, BEFORE any application module
is imported.
"""
import os
from pathlib import Path

import pytest
from fastapi.testclient import TestClient


# embedding/tests/conftest.py
def find_artifacts_dir():
    """
    Locate the local models directory within the embedding service.
    """
    # embedding/tests/conftest.py -> embedding/
    embedding_root = Path(__file__).resolve().parent.parent
    local_models = embedding_root / "models"

    if local_models.exists():
        return local_models

    # Fallback to environment variable if local models aren't found
    env_val = os.getenv("ONNX_MODEL_DIR")
    if env_val:
        return Path(env_val)

    raise RuntimeError(
        f"Could not find local models directory at {local_models} or ONNX_MODEL_DIR env var."
    )

# ── Set env vars before any src.* import ─────────────────────────────────────
os.environ.setdefault("API_KEY", "test-key-for-embedding-integration-tests")
os.environ.setdefault("ENVIRONMENT", "test")
# Ensure ONNX_MODEL_DIR is set for the test session
os.environ.setdefault("ONNX_MODEL_DIR", str(find_artifacts_dir().resolve()))

# Empty string → setup_telemetry() falls back to basicConfig (no OTLP needed)
os.environ.setdefault("OTEL_EXPORTER_OTLP_ENDPOINT", "")
os.environ.setdefault("TELEMETRY__OTLP_ENDPOINT", "")

# ── Shared constants ──────────────────────────────────────────────────────────
VALID_API_KEY = "test-key-for-embedding-integration-tests"
VALID_HEADERS = {"X-API-Key": VALID_API_KEY}

# A small, stable, publicly accessible test image (Dog)
TEST_IMAGE_URL = (
    "https://images.unsplash.com/photo-1517849845537-4d257902454a?w=400&q=80"
)
# A second distinct image for discriminability tests (Cat)
TEST_IMAGE_URL_2 = (
    "https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?w=400&q=80"
)

# ── Fixtures ──────────────────────────────────────────────────────────────────

@pytest.fixture(scope="session")
def app():
    """
    Create the FastAPI application once per test session.
    """
    from embedding.main import app as _app
    return _app


@pytest.fixture
def mock_engine():
    """
    Provides a mock InferenceEngine.
    """
    from unittest.mock import MagicMock
    return MagicMock()


@pytest.fixture(scope="session")
def client(app):
    """TestClient wrapping the session-scoped app."""
    with TestClient(app) as c:
        yield c


@pytest.fixture(scope="session")
def authed_client(app):
    """TestClient with the API key pre-set on every request."""
    with TestClient(app, headers=VALID_HEADERS) as c:
        yield c


# ── pytest marks ─────────────────────────────────────────────────────────────
def pytest_configure(config):
    config.addinivalue_line(
        "markers", "integration: marks end-to-end tests that hit the full FastAPI stack"
    )
