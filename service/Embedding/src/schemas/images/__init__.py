"""
Specialized results and errors for Image operations.
"""
from typing import Any
from embedding.schemas.results.result import ValueResult
from embedding.schemas.results.failure import Failure


class ImageResults:
    """Namespace for image success and error results."""

    class Success:
        """Success result factories for images."""
        
        @staticmethod
        def Ok(value: Any) -> ValueResult[Any]:
            return ValueResult.ok_value(value)

    class Errors:
        """Error result factories for images."""

        @staticmethod
        def LoadError(detail: str) -> Failure:
            return Failure.bad_request("Image.LoadError", detail)

        @staticmethod
        def UnsupportedType(type_name: str) -> Failure:
            return Failure.bad_request(
                "Image.InputError", 
                f"Unsupported input type: {type_name}"
            )
