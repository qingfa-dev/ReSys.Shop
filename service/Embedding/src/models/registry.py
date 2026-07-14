"""
Centralized registry for inference model "skills".
Allows dynamic registration and discovery of embedder implementations.
"""
import logging
from typing import Any, Dict, List, Optional, Type

from embedding.schemas import RegistryResults, ValueResult

logger = logging.getLogger(__name__)

class ModelRegistry:
    """
    Singleton-like registry to store and resolve model implementations.

    Invariant: _models maps unique name → class; _metadata stores optional metadata per name.
    """
    _models: Dict[str, Type] = {}
    _metadata: Dict[str, Dict[str, Any]] = {}

    @classmethod
    def register(cls, name: str, metadata: Optional[Dict[str, Any]] = None):
        """Decorator to register a model class with a specific identifier.

        Args:
            name: Unique model identifier (e.g. 'efficientnet_b0').
            metadata: Optional dict of meta attributes (name, dimension, description, tags).

        Returns:
            A decorator that registers the model class and returns it unchanged.
        """
        def wrapper(model_cls: Type):
            cls._models[name] = model_cls
            if metadata:
                cls._metadata[name] = metadata
            # Log: Confirm registration for debugging
            logger.debug(f"Registered model skill: {name} -> {model_cls.__name__}")
            return model_cls
        return wrapper

    @classmethod
    def get_model_class(cls, name: str) -> ValueResult[Type]:
        """Retrieves the model class for a given identifier.

        Args:
            name: The registered model identifier to look up.

        Returns:
            ValueResult containing the model class, or a NotRegistered error if not found.
        """
        model_cls = cls._models.get(name)
        if not model_cls:
            return ValueResult.failure_value(RegistryResults.Errors.NotRegistered(name))

        return RegistryResults.Success.Ok(model_cls)

    @classmethod
    def list_models(cls) -> List[str]:
        """Returns a list of all registered model identifiers.

        Returns:
            A list of model name strings.
        """
        return list(cls._models.keys())

    @classmethod
    def get_all_metadata(cls) -> Dict[str, Dict[str, Any]]:
        """Returns metadata for all registered models.

        Returns:
            Dict mapping model identifiers to their metadata dicts.
        """
        return cls._metadata
