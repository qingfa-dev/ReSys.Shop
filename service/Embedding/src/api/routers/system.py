"""
System-level API endpoints for health monitoring and lifecycle.
"""
import asyncio
import logging

from embedding.core.config import settings
from fastapi import APIRouter, Response, status

router = APIRouter(tags=["system"])
logger = logging.getLogger(__name__)


def _check_model_sync():
    """Synchronous model check, runs in thread pool to avoid blocking event loop."""
    from embedding.services.inference_engine import InferenceEngine
    engine = InferenceEngine()
    return engine.get_embedder(settings.EMBEDDING_MODEL)


@router.get("/health")
async def health_check(response: Response):
    """Standardized health status for orchestration (Readiness).

    Validates that the service can load and execute inference with the default model.
    Returns degraded status if model loading fails, allowing orchestrators to restart.

    Returns:
        Dict with status, service name, environment, version, and optional error.
    """
    try:
        test_result = await asyncio.to_thread(_check_model_sync)

        if not test_result.is_success:
            error_msg = f"Model '{settings.EMBEDDING_MODEL}' failed to load: {test_result.error}"
            logger.warning(error_msg)
            response.status_code = status.HTTP_503_SERVICE_UNAVAILABLE
            return {
                "status": "degraded",
                "service": settings.PROJECT_NAME,
                "environment": settings.ENVIRONMENT,
                "version": "1.0.0",
                "error": error_msg
            }

        return {
            "status": "ok",
            "service": settings.PROJECT_NAME,
            "environment": settings.ENVIRONMENT,
            "version": "1.0.0"
        }
    except Exception as e:
        error_msg = f"Health check failed: {type(e).__name__}: {e}"
        logger.error(error_msg, exc_info=True)
        response.status_code = status.HTTP_503_SERVICE_UNAVAILABLE
        return {
            "status": "error",
            "service": settings.PROJECT_NAME,
            "environment": settings.ENVIRONMENT,
            "version": "1.0.0",
            "error": error_msg
        }


@router.get("/alive")
async def liveness_probe():
    """Liveness probe for orchestration (Alive).

    Returns:
        Dict with a status field set to 'alive'.
    """
    return {"status": "alive"}
