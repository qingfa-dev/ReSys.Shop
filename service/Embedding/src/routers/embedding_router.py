from fastapi import APIRouter, Request
from fastapi import File as FastAPIFile
from fastapi import UploadFile
import os

from embedding.schemas.requests import EmbeddingRequest, EmbeddingFromBytesRequest
from embedding.schemas.responses import ModelListResult

_MODEL_VERSION = os.getenv("EMBEDDING_MODEL_VERSION", "v1.0")

router = APIRouter()


@router.post("")
async def create_embedding(body: EmbeddingRequest):
    vector = _generate_dummy_vector(512)
    return {
        "value": {
            "vector": vector,
            "model_version": _MODEL_VERSION,
            "dimension": 512,
            "metadata": {"model": body.model},
        },
        "statusCode": 200,
        "isSuccess": True,
        "errors": [],
        "successMessage": None,
        "metadata": None,
    }


@router.post("/bytes")
async def create_embedding_from_bytes(
    image: UploadFile = FastAPIFile(...),
    model: str = "efficientnet_b0",
):
    _ = await image.read()
    vector = _generate_dummy_vector(512)
    return {
        "value": {
            "vector": vector,
            "model_version": _MODEL_VERSION,
            "dimension": 512,
            "metadata": {"model": model},
        },
        "statusCode": 200,
        "isSuccess": True,
        "errors": [],
        "successMessage": None,
        "metadata": None,
    }


def _generate_dummy_vector(dim: int) -> list[float]:
    import hashlib
    import math

    seed = "dummy-embedding-for-image-search"
    digest = hashlib.sha256(seed.encode()).digest()
    return [
        (digest[i % len(digest)] / 255.0) * 2.0 - 1.0
        for i in range(dim)
    ]
