"""
Specialized results and errors for Registry operations.
"""
from typing import Any
from embedding.schemas.results.result import ValueResult
from embedding.schemas.results.failure import Failure


class RegistryResults:
    """Namespace for registry success and error results."""

    class Success:
        """Success result factories for registries."""
        
        @staticmethod
        def Ok(value: Any) -> ValueResult[Any]:
            return ValueResult.ok_value(value)

    class Errors:
        """Error result factories for registries."""

        @staticmethod
        def NotRegistered(skill_name: str) -> Failure:
            return Failure.internal_error(
                "Registry.Error", 
                f"Skill implementation '{skill_name}' not registered."
            )
