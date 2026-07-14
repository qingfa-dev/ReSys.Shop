"""
Main API router aggregator.
"""
from fastapi import APIRouter
from embedding.api.routers.system import router as system_router
from embedding.api.routers.inference import router as inference_router

api_router = APIRouter()

# Include sub-routers with logical grouping
api_router.include_router(system_router)
api_router.include_router(inference_router)
