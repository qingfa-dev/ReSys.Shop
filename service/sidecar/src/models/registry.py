"""
Centralized registry for inference model "skills".
Allows dynamic registration and discovery of embedder implementations.
"""
from typing import Dict, Type, List, Any, Optional
import logging
from src.schemas import ValueResult, RegistryResults

logger = logging.getLogger(__name__)

class ModelRegistry:
    """
    Singleton-like registry to store and resolve model implementations.
    """
    _models: Dict[str, Type] = {}
    _metadata: Dict[str, Dict[str, Any]] = {}

    @classmethod
    def register(cls, name: str, metadata: Optional[Dict[str, Any]] = None):
        """
        Decorator to register a model class with a specific identifier and optional metadata.
        """
        def wrapper(model_cls: Type):
            cls._models[name] = model_cls
            if metadata:
                cls._metadata[name] = metadata
            logger.debug(f"Registered model skill: {name} -> {model_cls.__name__}")
            return model_cls
        return wrapper

    @classmethod
    def get_model_class(cls, name: str) -> ValueResult[Type]:
        """
        Retrieves the model class for a given identifier.
        """
        model_cls = cls._models.get(name)
        if not model_cls:
            return ValueResult.failure_value(RegistryResults.Errors.NotRegistered(name))
        
        return RegistryResults.Success.Ok(model_cls)

    @classmethod
    def list_models(cls) -> List[str]:
        """
        Returns a list of all registered model identifiers.
        """
        return list(cls._models.keys())

    @classmethod
    def get_all_metadata(cls) -> Dict[str, Dict[str, Any]]:
        """
        Returns metadata for all registered models.
        """
        return cls._metadata
