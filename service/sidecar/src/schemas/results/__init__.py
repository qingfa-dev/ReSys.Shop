"""
Core Result and Failure logic.
"""
from src.schemas.results.failure import Failure, FailureType
from src.schemas.results.result import Result, ValueResult

__all__ = [
    "Failure",
    "FailureType",
    "Result",
    "ValueResult",
]
