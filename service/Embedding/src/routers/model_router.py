from fastapi import APIRouter

from embedding.schemas.responses import ModelItem, ModelListResult
from embedding.services.embedding_service import list_available_models

router = APIRouter()


@router.get("/models")
async def list_models():
    """List all registered embedding models with their metadata.

    These 4 models are the subjects of the comparative evaluation
    described in thesis Chapter 11 (§11.5).
    """
    models = list_available_models()
    return ModelListResult(
        value=[
            ModelItem(
                id=m["id"],
                name=m["name"],
                dimension=m["dimension"],
                description=m["description"],
                is_onnx=m["is_onnx"],
                tags=m["tags"],
            )
            for m in models
        ],
        status_code=200,
        is_success=True,
    )
