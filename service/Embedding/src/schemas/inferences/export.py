"""
Pydantic schemas for ONNX export status reporting.
"""
from typing import List, Optional

from pydantic import BaseModel, ConfigDict, Field


class ModelExportReport(BaseModel):
    """Status report for a single model's ONNX export."""

    model_config = ConfigDict(populate_by_name=True, frozen=True)

    model_name: str = Field(
        ...,
        alias="modelName",
        description="Identifier of the model being exported.",
    )
    status: str = Field(
        ...,
        description="Export status: pending, exporting, completed, or failed.",
        json_schema_extra={"example": "completed"},
    )
    duration_ms: Optional[float] = Field(
        default=None,
        alias="durationMs",
        description="Export duration in milliseconds (null if not yet finished).",
    )
    error: Optional[str] = Field(
        default=None,
        description="Error message if export failed (null on success).",
    )


class OnnxExportResponse(BaseModel):
    """Response envelope for the ONNX export endpoint."""

    model_config = ConfigDict(populate_by_name=True, frozen=True)

    overall_status: str = Field(
        ...,
        alias="overallStatus",
        description="Overall export status: idle, running, completed, or failed.",
        json_schema_extra={"example": "completed"},
    )
    models: List[ModelExportReport] = Field(
        default_factory=list,
        description="Per-model export status reports.",
    )
    start_time: Optional[str] = Field(
        default=None,
        alias="startTime",
        description="ISO-8601 local timestamp when export started.",
        json_schema_extra={"example": "2026-07-27T01:30:00"},
    )
    end_time: Optional[str] = Field(
        default=None,
        alias="endTime",
        description="ISO-8601 local timestamp when export finished.",
        json_schema_extra={"example": "2026-07-27T01:35:00"},
    )
    total_duration_ms: Optional[float] = Field(
        default=None,
        alias="totalDurationMs",
        description="Total export wall-clock time in milliseconds.",
        json_schema_extra={"example": 298450.5},
    )
