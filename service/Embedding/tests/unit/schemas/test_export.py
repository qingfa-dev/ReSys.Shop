"""
Unit tests for ONNX export response schemas.
"""
import pytest
from embedding.schemas.inferences.export import ModelExportReport, OnnxExportResponse


class TestModelExportReport:
    def test_serialization_camel_case(self):
        report = ModelExportReport(
            modelName="fashion_clip",
            status="completed",
            durationMs=1234.5,
            error=None,
        )
        data = report.model_dump(by_alias=True)
        assert data["modelName"] == "fashion_clip"
        assert data["status"] == "completed"
        assert data["durationMs"] == 1234.5
        assert data["error"] is None

    def test_serialization_snake_case(self):
        report = ModelExportReport(
            model_name="fashion_clip",
            status="failed",
            duration_ms=500.0,
            error="Export failed",
        )
        data = report.model_dump(by_alias=False)
        assert data["model_name"] == "fashion_clip"
        assert data["status"] == "failed"
        assert data["duration_ms"] == 500.0
        assert data["error"] == "Export failed"

    def test_frozen_model(self):
        report = ModelExportReport(modelName="test", status="pending")
        with pytest.raises(Exception):
            report.status = "completed"


class TestOnnxExportResponse:
    def test_serialization_camel_case(self):
        resp = OnnxExportResponse(
            overallStatus="completed",
            models=[
                {
                    "modelName": "efficientnet_b0",
                    "status": "completed",
                    "durationMs": 1000.0,
                }
            ],
            startTime="2026-07-27T01:30:00",
            endTime="2026-07-27T01:35:00",
            totalDurationMs=300000.0,
        )
        data = resp.model_dump(by_alias=True)
        assert data["overallStatus"] == "completed"
        assert data["startTime"] == "2026-07-27T01:30:00"
        assert data["endTime"] == "2026-07-27T01:35:00"
        assert data["totalDurationMs"] == 300000.0
        assert len(data["models"]) == 1
        assert data["models"][0]["modelName"] == "efficientnet_b0"

    def test_empty_response(self):
        resp = OnnxExportResponse(overallStatus="idle")
        data = resp.model_dump(by_alias=True)
        assert data["overallStatus"] == "idle"
        assert data["models"] == []
        assert data["startTime"] is None
        assert data["endTime"] is None
        assert data["totalDurationMs"] is None

    def test_frozen_model(self):
        resp = OnnxExportResponse(overallStatus="running")
        with pytest.raises(Exception):
            resp.overallStatus = "completed"
