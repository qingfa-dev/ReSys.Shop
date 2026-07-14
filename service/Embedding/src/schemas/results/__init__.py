"""
Core Result and Error logic.
"""
from embedding.schemas.results.error import Error, ErrorType
from embedding.schemas.results.result import Result, ValueResult

__all__ = [
    "Error",
    "ErrorType",
    "Result",
    "ValueResult",
]
