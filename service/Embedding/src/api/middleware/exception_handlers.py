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
    """Convert: Transform a Result failure into a FastAPI JSON response.

    Args:
        result: A failed Result object containing errors.

    Returns:
        JSONResponse with the status code and serialized content.
    """
    return JSONResponse(
        status_code=result.status_code,
        content=result.model_dump(by_alias=True)
    )


async def global_exception_handler(request: Request, exc: Exception):
    """Catch: All unhandled exceptions — returns standardized 500 Internal Error Result.

    Args:
        request: The incoming HTTP request (unused but required by FastAPI).
        exc: The unhandled exception instance.

    Returns:
        JSONResponse with status 500 and a Server.Error failure body.
    """
    logger.error("Unhandled exception: %s", str(exc), exc_info=True)

    error = Error.internal_error(
        "Server.Error",
        "An unexpected error occurred while processing your request."
    )
    return create_error_response(Result.failure(error))


async def http_exception_handler(request: Request, exc: StarletteHTTPException):
    """Catch: FastAPI/Starlette HTTPExceptions (e.g. 404, 405, 401, 403).

    Maps the HTTP status code to the appropriate ErrorType and returns a
    standardized Result failure body.

    Args:
        request: The incoming HTTP request (unused but required by FastAPI).
        exc: The HTTPException with status code and detail.

    Returns:
        JSONResponse with the mapped status code and structured error body.
    """
    # Map: Translate HTTP status codes to domain ErrorType values
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
    """Catch: Pydantic validation errors (422 Unprocessable Entity).

    Iterates through each validation error, constructs a structured Error,
    and returns a Result failure with all errors aggregated.

    Args:
        request: The incoming HTTP request (unused but required by FastAPI).
        exc: The RequestValidationError with error details.

    Returns:
        JSONResponse with status 422 and a list of validation errors.
    """
    errors = []
    for err in exc.errors():
        # Format: Join error location path into a dot-notation string
        loc = ".".join(str(x) for x in err["loc"])
        msg = err["msg"]
        errors.append(Error.validation(
            code="Request.ValidationError",
            description=f"Validation failed at {loc}: {msg}"
        ))

    return create_error_response(Result.failure(errors))
