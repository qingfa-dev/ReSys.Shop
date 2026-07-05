from pydantic import BaseModel


class ModelItem(BaseModel):
    id: str
    name: str
    dimension: int
    description: str | None = None
    is_onnx: bool = False
    tags: list[str] = []


class ModelListResult(BaseModel):
    value: list[ModelItem] | None = None
    status_code: int = 200
    is_success: bool = True
    errors: list[str] = []
    success_message: str | None = None
    metadata: dict[str, object] | None = None
