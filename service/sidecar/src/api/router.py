"""
Main API router aggregator.
"""
from fastapi import APIRouter
from src.api.routers.system import router as system_router
from src.api.routers.inference import router as inference_router

api_router = APIRouter()

# Include sub-routers with logical grouping
api_router.include_router(system_router)
api_router.include_router(inference_router, prefix="/inference")
