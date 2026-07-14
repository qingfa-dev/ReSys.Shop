"""
Specialized results and errors for Registry operations.
"""
from typing import Any

from embedding.schemas.results.error import Error
from embedding.schemas.results.result import ValueResult


class RegistryResults:
    """Namespace for registry success and error results."""

    class Success:
        """Success result factories for registries."""

        @staticmethod
        def Ok(value: Any) -> ValueResult[Any]:
            """Creates a generic success result for registry lookups.

            Args:
                value: The data payload (e.g. a model class).
            """
            return ValueResult.ok_value(value)

    class Errors:
        """Error result factories for registries."""

        @staticmethod
        def NotRegistered(skill_name: str) -> Error:
            """Creates an internal error for an unregistered model skill.

            Args:
                skill_name: The model identifier that was not found in the registry.
            """
            return Error.internal_error(
                "Registry.Error",
                f"Skill implementation '{skill_name}' not registered."
            )
