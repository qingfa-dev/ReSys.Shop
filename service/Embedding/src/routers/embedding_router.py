from fastapi import APIRouter, Request
from fastapi import File as FastAPIFile
from fastapi import UploadFile

from embedding.schemas.requests import (
    EmbeddingFromBytesRequest,
    EmbeddingRequest,
)
from embedding.schemas.responses import EmbeddingResult, EmbeddingResponse
from embedding.services.embedding_service import encode_image, get_model

router = APIRouter()


@router.post("")
async def create_embedding(body: EmbeddingRequest):
    """Generate embedding from an image URL (not yet implemented).

    Returns a clear error until image download + caching is added.
    """
    # TODO: Implement image URL fetch + embedding
    return EmbeddingResult(
        value=None,
        status_code=501,
        is_success=False,
        errors=["Image URL embedding is not yet implemented. Use POST /embeddings/bytes instead."],
    )


@router.post("/bytes")
async def create_embedding_from_bytes(
    image: UploadFile = FastAPIFile(...),
    model: str | None = None,
):
    """Generate embedding from raw image bytes.

    Query param `model` overrides the EMBEDDING_MODEL env var.
    Supported values: fashion-clip, resnet50, efficientnet_b0, clip.
    """
    from PIL import Image
    import io

    image_bytes = await image.read()
    pil_image = Image.open(io.BytesIO(image_bytes)).convert("RGB")

    result = encode_image(pil_image, model_name=model)

    return EmbeddingResult(
        value=EmbeddingResponse(
            vector=result["embedding"],
            model_version=result["model_name"],
            dimension=result["vector_dim"],
            metadata={
                "model": result["model_name"],
                "elapsed_ms": result["elapsed_ms"],
            },
        ),
        status_code=200,
        is_success=True,
    )
