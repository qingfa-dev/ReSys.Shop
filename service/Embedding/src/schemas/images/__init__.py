"""
Specialized results and errors for Image operations.
"""
from typing import Any

from embedding.schemas.results.error import Error
from embedding.schemas.results.result import ValueResult


class ImageResults:
    """Namespace for image success and error results."""

    class Success:
        """Success result factories for images."""

        @staticmethod
        def Ok(value: Any) -> ValueResult[Any]:
            """Creates a generic success result for image operations.

            Args:
                value: The data payload (e.g. PIL Image).
            """
            return ValueResult.ok_value(value)

    class Errors:
        """Error result factories for images."""

        @staticmethod
        def LoadError(detail: str) -> Error:
            """Creates a bad-request error for image loading failures.

            Args:
                detail: Description of what went wrong during loading.
            """
            return Error.bad_request("Image.LoadError", detail)

        @staticmethod
        def UnsupportedType(type_name: str) -> Error:
            """Creates a bad-request error for unsupported image input types.

            Args:
                type_name: The Python type name that is not supported.
            """
            return Error.bad_request(
                "Image.InputError",
                f"Unsupported input type: {type_name}"
            )
