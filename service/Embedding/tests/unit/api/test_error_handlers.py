"""
Unit tests for Global Error Handlers.
"""
import pytest
from unittest.mock import MagicMock
from fastapi import status
from fastapi.exceptions import RequestValidationError
from starlette.exceptions import HTTPException as StarletteHTTPException

from embedding.api.middleware.exception_handlers import (
    http_exception_handler, 
    validation_exception_handler, 
    global_exception_handler
)


@pytest.mark.asyncio
async def test_http_exception_handler_404():
    request = MagicMock()
    exc = StarletteHTTPException(status_code=404, detail="Not Found")
    
    response = await http_exception_handler(request, exc)
    
    assert response.status_code == 404
    import json
    data = json.loads(response.body.decode())
    assert data["isSuccess"] is False
    assert data["failures"][0]["code"] == "Route.NotFound"


@pytest.mark.asyncio
async def test_validation_exception_handler():
    request = MagicMock()
    # Mock a Pydantic-style error list
    errors = [
        {"loc": ("body", "image_url"), "msg": "field required", "type": "value_error.missing"}
    ]
    exc = RequestValidationError(errors)
    
    response = await validation_exception_handler(request, exc)
    
    # Result.failure(failures) uses status_code of first failure (which is 400 for validation)
    assert response.status_code == 400 
    
    import json
    data = json.loads(response.body.decode())
    assert data["failures"][0]["code"] == "Request.ValidationError"
    assert "body.image_url" in data["failures"][0]["description"]


@pytest.mark.asyncio
async def test_global_exception_handler():
    request = MagicMock()
    exc = RuntimeError("Something went wrong")
    
    response = await global_exception_handler(request, exc)
    
    assert response.status_code == 500
    import json
    data = json.loads(response.body.decode())
    assert data["isSuccess"] is False
    assert data["failures"][0]["code"] == "Server.Error"
