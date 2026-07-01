from fastapi import FastAPI
from fastapi.testclient import TestClient


def test_unhandled_exception_returns_500() -> None:
    """Verify the global exception handler catches unhandled errors."""
    app = FastAPI()

    @app.get("/crash")
    async def crash():
        raise ValueError("boom")

    from embedding.middleware.exception_handler import register_exception_handlers
    register_exception_handlers(app)

    client = TestClient(app, raise_server_exceptions=False)
    response = client.get("/crash")
    assert response.status_code == 500
    assert response.json() == {"detail": "Internal server error"}
