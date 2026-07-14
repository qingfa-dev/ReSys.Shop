"""
System-level API endpoints for health monitoring and lifecycle.
"""
from fastapi import APIRouter
from embedding.core.config import settings

router = APIRouter(tags=["system"])


@router.get("/health")
async def health_check():
    """Standardized health status for orchestration (Readiness)."""
    return {
        "status": "ok", 
        "service": settings.PROJECT_NAME, 
        "environment": settings.ENVIRONMENT,
        "version": "1.0.0"
    }


@router.get("/alive")
async def liveness_probe():
    """Liveness probe for orchestration (Alive)."""
    return {"status": "alive"}
