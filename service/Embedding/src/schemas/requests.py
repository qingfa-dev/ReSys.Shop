from pydantic import BaseModel, Field


class EmbeddingRequest(BaseModel):
    image_url: str = Field(default="", serialization_alias="image_url")
    model: str = Field(default="efficientnet_b0")


class EmbeddingFromBytesRequest(BaseModel):
    model: str | None = None


class EmbeddingResponse(BaseModel):
    vector: list[float] = Field(default_factory=list)
    model_version: str = Field(default="", serialization_alias="model_version")
    dimension: int = Field(default=0)
    metadata: dict[str, object] | None = None


class EmbeddingResult(BaseModel):
    value: EmbeddingResponse | None = None
    status_code: int = Field(default=200, serialization_alias="statusCode")
    is_success: bool = Field(default=True, serialization_alias="isSuccess")
    errors: list[str] = Field(default_factory=list)
    success_message: str | None = Field(default=None, serialization_alias="successMessage")
    metadata: dict[str, object] | None = None
