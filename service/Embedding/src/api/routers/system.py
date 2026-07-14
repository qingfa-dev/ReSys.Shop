"""
System-level API endpoints for health monitoring and lifecycle.
"""
from embedding.core.config import settings
from fastapi import APIRouter

router = APIRouter(tags=["system"])


@router.get("/health")
async def health_check():
    """Standardized health status for orchestration (Readiness).

    Returns:
        Dict with status, service name, environment, and version.
    """
    return {
        "status": "ok",
        "service": settings.PROJECT_NAME,
        "environment": settings.ENVIRONMENT,
        "version": "1.0.0"
    }


@router.get("/alive")
async def liveness_probe():
    """Liveness probe for orchestration (Alive).

    Returns:
        Dict with a status field set to 'alive'.
    """
    return {"status": "alive"}
