"""
Standardized Result models for unified response shapes.
"""
from typing import Generic, List, Optional, TypeVar, Union

from embedding.schemas.results.failure import Failure
from pydantic import BaseModel, ConfigDict, Field

T = TypeVar("T")


class Result(BaseModel):
    """
    Base class for operation results, ensuring consistent response shapes across
    Python and .NET stacks. On failure, contains one or more Failure objects.
    """
    # Configure: Serialization and immutability settings
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
    failures: List[Failure] = Field(
        default=[],
        description="A list of failures, populated if isSuccess is False.",
    )

    @classmethod
    def ok(cls, status_code: int = 200, message: Optional[str] = None) -> "Result":
        """Creates a successful result without a value."""
        return cls(isSuccess=True, statusCode=status_code, message=message, failures=[])

    @classmethod
    def failure(
        cls,
        failure: Union[Failure, List[Failure]],
        message: Optional[str] = None,
    ) -> "Result":
        """Creates a failed result from one or more Failure objects."""
        failures = [failure] if isinstance(failure, Failure) else failure
        sc = failures[0].status_code if failures else 400
        return cls(isSuccess=False, statusCode=sc, message=message, failures=failures)


class ValueResult(Result, Generic[T]):
    """
    Represents a result that contains a data value of type T on success.
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
        """Creates a successful result containing the specified value."""
        return cls(
            isSuccess=True,
            statusCode=status_code,
            message=message,
            failures=[],
            value=value,
        )

    @classmethod
    def failure_value(
        cls,
        failure: Union[Failure, List[Failure]],
        message: Optional[str] = None,
    ) -> "ValueResult[T]":
        """Creates a failed result, ensuring the value is set to None."""
        failures = [failure] if isinstance(failure, Failure) else failure
        sc = failures[0].status_code if failures else 400
        return cls(isSuccess=False, statusCode=sc, message=message, failures=failures, value=None)
