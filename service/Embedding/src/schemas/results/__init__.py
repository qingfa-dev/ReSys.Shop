"""
Core Result and Failure logic.
"""
from embedding.schemas.results.failure import Failure, FailureType
from embedding.schemas.results.result import Result, ValueResult

__all__ = [
    "Failure",
    "FailureType",
    "Result",
    "ValueResult",
]
