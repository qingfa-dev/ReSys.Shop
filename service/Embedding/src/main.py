from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from embedding.config.settings import Settings
from embedding.middleware.exception_handler import register_exception_handlers
from embedding.routers.embedding_router import router as embedding_router
from embedding.routers.health_router import router as health_router
from embedding.routers.model_router import router as model_router

settings = Settings()

app = FastAPI(
    title=settings.app_name,
    version=settings.app_version,
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=settings.cors_origins,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

register_exception_handlers(app)

app.include_router(health_router, prefix="", tags=["health"])
app.include_router(embedding_router, prefix="/embeddings", tags=["embeddings"])
app.include_router(model_router, prefix="", tags=["models"])
