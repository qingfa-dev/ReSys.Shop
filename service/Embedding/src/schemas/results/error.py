"""
Failure models for standardized error reporting.
"""
from enum import IntEnum

from pydantic import BaseModel, ConfigDict, Field


class ErrorType(IntEnum):
    """
    Categorizes the type of failure for consistent handling across the system.
    Matches the BuildingBlocks.Models.FailureType enum in .NET.
    """
    None_ = 0
    Validation = 1
    Conflict = 2
    NotFound = 3
    BadRequest = 4
    InternalError = 5
    Unauthorized = 6
    Forbidden = 7
    Unexpected = 8


class Error(BaseModel):
    """
    Represents a specific error or failure that occurred during processing.
    Includes a type, machine-readable code, human-readable description, and status code.
    """
    # Configure: Immutable model to prevent accidental modification
    model_config = ConfigDict(frozen=True)

    # Assign: Metadata properties
    type: ErrorType = Field(
        ...,
        description="The high-level category of failure (e.g. NotFound, Conflict).",
    )
    code: str = Field(
        ...,
        description="A stable, machine-readable error code.",
        json_schema_extra={"example": "Model.NotFound"},
    )
    description: str = Field(
        ...,
        description=(
            "A detailed, human-readable description of what went wrong."
        ),
        json_schema_extra={"example": "Model 'abc' could not be found."},
    )
    status_code: int = Field(
        default=400,
        description=(
            "The corresponding HTTP status code for this failure."
        ),
        json_schema_extra={"example": 404},
    )

    @classmethod
    def validation(cls, code: str, description: str) -> "Error":
        """Creates a validation failure (HTTP 400)."""
        return cls(type=ErrorType.Validation, code=code, description=description, status_code=400)

    @classmethod
    def conflict(cls, code: str, description: str) -> "Error":
        """Creates a conflict failure (HTTP 409)."""
        return cls(type=ErrorType.Conflict, code=code, description=description, status_code=409)

    @classmethod
    def not_found(cls, code: str, description: str) -> "Error":
        """Creates a not found failure (HTTP 404)."""
        return cls(type=ErrorType.NotFound, code=code, description=description, status_code=404)

    @classmethod
    def bad_request(cls, code: str, description: str) -> "Error":
        """Creates a bad request failure (HTTP 400)."""
        return cls(type=ErrorType.BadRequest, code=code, description=description, status_code=400)

    @classmethod
    def internal_error(cls, code: str, description: str) -> "Error":
        """Creates an internal error failure (HTTP 500)."""
        return cls(
            type=ErrorType.InternalError,
            code=code,
            description=description,
            status_code=500,
        )

    @classmethod
    def unauthorized(cls, code: str, description: str) -> "Error":
        """Creates an unauthorized failure (HTTP 401)."""
        return cls(
            type=ErrorType.Unauthorized,
            code=code,
            description=description,
            status_code=401,
        )

    @classmethod
    def forbidden(cls, code: str, description: str) -> "Error":
        """Creates a forbidden failure (HTTP 403)."""
        return cls(type=ErrorType.Forbidden, code=code, description=description, status_code=403)
