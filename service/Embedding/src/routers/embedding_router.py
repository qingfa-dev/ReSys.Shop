from fastapi import APIRouter

router = APIRouter()


@router.post("/embed")
async def embed():
    return {"embedding": [], "dim": 512}


@router.post("/embed/batch")
async def embed_batch():
    return {"embeddings": [], "dim": 512}
