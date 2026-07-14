"""
Global exception handlers for FastAPI.
Converts various exception types into standardized Result/Failure models.
Follows the .NET BuildingBlocks error handling pattern.
"""
import logging

from embedding.schemas import Error, ErrorType, Result
from fastapi import Request
from fastapi.exceptions import RequestValidationError
from fastapi.responses import JSONResponse
from starlette.exceptions import HTTPException as StarletteHTTPException

logger = logging.getLogger(__name__)


def create_error_response(result: Result) -> JSONResponse:
    """Helper to convert a Result failure into a JSONResponse."""
    return JSONResponse(
        status_code=result.status_code,
        content=result.model_dump(by_alias=True)
    )


async def global_exception_handler(request: Request, exc: Exception):
    """
    Catches all unhandled exceptions and returns a standardized 500 Internal Error Result.
    """
    logger.error("Unhandled exception: %s", str(exc), exc_info=True)

    error = Error.internal_error(
        "Server.Error",
        "An unexpected error occurred while processing your request."
    )
    return create_error_response(Result.failure(error))


async def http_exception_handler(request: Request, exc: StarletteHTTPException):
    """
    Handles FastAPI/Starlette HTTPExceptions (e.g. 404, 405).
    """
    # Map status code to failure type
    if exc.status_code == 404:
        ftype = ErrorType.NotFound
        code = "Route.NotFound"
    elif exc.status_code == 401:
        ftype = ErrorType.Unauthorized
        code = "Auth.Unauthorized"
    elif exc.status_code == 403:
        ftype = ErrorType.Forbidden
        code = "Auth.Forbidden"
    else:
        ftype = ErrorType.Unexpected
        code = "Http.Error"

    error = Error(
        type=ftype,
        code=code,
        description=str(exc.detail),
        status_code=exc.status_code
    )
    return create_error_response(Result.failure(error))


async def validation_exception_handler(request: Request, exc: RequestValidationError):
    """
    Handles Pydantic validation errors (422 Unprocessable Entity).
    """
    errors = []
    for err in exc.errors():
        loc = ".".join(str(x) for x in err["loc"])
        msg = err["msg"]
        errors.append(Error.validation(
            code="Request.ValidationError",
            description=f"Validation failed at {loc}: {msg}"
        ))

    return create_error_response(Result.failure(errors))
