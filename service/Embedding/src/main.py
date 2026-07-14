from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from embedding.config.settings import Settings
from embedding.middleware.exception_handler import register_exception_handlers
from embedding.routers.cache_router import router as cache_router
from embedding.routers.embedding_router import router as embedding_router
from embedding.routers.health_router import router as health_router
from embedding.routers.model_router import router as model_router
from embedding.services.embedding_service import warmup_default_model

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
app.include_router(cache_router, prefix="/cache", tags=["cache"])


@app.on_event("startup")
async def startup_event() -> None:
    """Preload the default embedding model to avoid cold-start latency."""
    try:
        warmup_default_model()
    except Exception as exc:
        # Log but do not crash — the sidecar can still serve health checks
        # and load the model lazily on first request.
        import logging

        logging.getLogger(__name__).warning(
            "Failed to warmup default model '%s': %s",
            settings.embedding_model,
            exc,
        )
