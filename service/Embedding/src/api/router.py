"""
Main API router aggregator.
"""
from embedding.api.routers.inference import router as inference_router
from embedding.api.routers.models import router as models_router
from embedding.api.routers.system import router as system_router
from fastapi import APIRouter

api_router = APIRouter()

api_router.include_router(system_router)
api_router.include_router(inference_router)
api_router.include_router(models_router)
