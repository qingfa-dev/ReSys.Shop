"""
Standardized Result models for unified response shapes.
"""
from typing import Generic, List, Optional, TypeVar, Union

from embedding.schemas.results.error import Error
from pydantic import BaseModel, ConfigDict, Field

T = TypeVar("T")


class Result(BaseModel):
    """
    Base class for operation results, ensuring consistent response shapes across
    Python and .NET stacks. On failure, contains one or more Error objects.

    Invariant: success implies errors empty; failure implies at least one error entry.
    """
    # Configure: Serialization by name, allow arbitrary types, enforce immutability
    model_config = ConfigDict(populate_by_name=True, arbitrary_types_allowed=True, frozen=True)

    # Assign: Core status fields
    is_success: bool = Field(
        alias="isSuccess", description="Whether the operation was successful."
    )
    status_code: int = Field(
        alias="statusCode",
        description="The HTTP status code representing the result.",
        json_schema_extra={"example": 200},
    )
    message: Optional[str] = Field(
        default=None, description="An optional summary message."
    )
    errors: List[Error] = Field(
        default=[],
        description="A list of errors, populated if isSuccess is False.",
    )

    @classmethod
    def ok(cls, status_code: int = 200, message: Optional[str] = None) -> "Result":
        """Creates a successful result without a value.

        Args:
            status_code: HTTP status code (default 200).
            message: Optional human-readable summary.

        Returns:
            A new Result with is_success=True.
        """
        return cls(isSuccess=True, statusCode=status_code, message=message, errors=[])

    @classmethod
    def failure(
        cls,
        error: Union[Error, List[Error]],
        message: Optional[str] = None,
    ) -> "Result":
        """Creates a failed result from one or more Error objects.

        Args:
            error: A single Error or list of errors describing the failure.
            message: Optional human-readable summary.

        Returns:
            A new Result with is_success=False and the given errors.
        """
        errors = [error] if isinstance(error, Error) else error
        sc = errors[0].status_code if errors else 400
        return cls(isSuccess=False, statusCode=sc, message=message, errors=errors)


class ValueResult(Result, Generic[T]):
    """
    Represents a result that contains a data value of type T on success.

    Invariant: value is None when is_success is False; value is non-None when is_success is True.
    """
    # Assign: Success payload
    value: Optional[T] = Field(
        default=None,
        description="The data payload returned by the operation on success.",
    )

    @classmethod
    def ok_value(
        cls,
        value: T,
        status_code: int = 200,
        message: Optional[str] = None,
    ) -> "ValueResult[T]":
        """Creates a successful result containing the specified value.

        Args:
            value: The data payload to include.
            status_code: HTTP status code (default 200).
            message: Optional human-readable summary.

        Returns:
            A new ValueResult with is_success=True and the given value.
        """
        return cls(
            isSuccess=True,
            statusCode=status_code,
            message=message,
            errors=[],
            value=value,
        )

    @classmethod
    def failure_value(
        cls,
        error: Union[Error, List[Error]],
        message: Optional[str] = None,
    ) -> "ValueResult[T]":
        """Creates a failed result, ensuring the value is set to None.

        Args:
            error: A single Error or list of errors describing the failure.
            message: Optional human-readable summary.

        Returns:
            A new ValueResult with is_success=False, value=None, and the given errors.
        """
        errors = [error] if isinstance(error, Error) else error
        sc = errors[0].status_code if errors else 400
        return cls(isSuccess=False, statusCode=sc, message=message, errors=errors, value=None)
