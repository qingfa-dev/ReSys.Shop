"""
Pydantic schemas for image embedding requests and responses.
"""
from typing import Any, Dict, List, Optional

from pydantic import BaseModel, Field


class EmbeddingRequest(BaseModel):
    """Request model for generating a high-dimensional vector embedding from an image.

    Invariant: image_url must be a valid HTTP/HTTPS URL; model defaults to fashion_clip.
    """
    image_url: str = Field(
        ...,
        description="The publicly accessible HTTP/HTTPS URL of the image to be processed.",
        json_schema_extra={"example": "https://images.unsplash.com/photo-1517841905240-472988babdf9?auto=format&fit=crop&q=80&w=1000"}
    )
    model_name: str = Field(
        default="fashion_clip",
        description="The identifier of the ML model to use.",
        json_schema_extra={"example": "fashion_clip"}
    )


class EmbeddingResponse(BaseModel):
    """Response model containing the generated vector embedding.

    Invariant: dimension equals len(vector); model_version is never None.
    """
    vector: List[float] = Field(..., description="The L2-normalized numerical vector.")
    model_version: str = Field(..., description="The specific version of the model.")
    dimension: int = Field(..., description="The number of elements in the vector.")
    metadata: Optional[Dict[str, Any]] = Field(
        default=None, description="Additional technical metadata."
    )


class ModelMetadata(BaseModel):
    """Metadata describing an available machine learning model.

    Invariant: id is unique across all registered and discovered models.
    """
    id: str = Field(..., description="Unique model identifier used in requests.")
    name: str = Field(..., description="Human-readable name of the model.")
    dimension: int = Field(..., description="Output vector dimensionality.")
    description: Optional[str] = Field(
        None, description="Brief explanation of the model's purpose."
    )
    is_onnx: bool = Field(
        default=False, description="Whether this is an optimized ONNX model."
    )
    tags: List[str] = Field(
        default_factory=list,
        description="Categorization tags (e.g. 'vision', 'semantic').",
    )
