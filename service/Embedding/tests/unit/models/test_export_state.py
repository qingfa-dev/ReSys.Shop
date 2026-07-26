"""
Unit tests for the ONNX export state manager.
"""
import time
from unittest.mock import MagicMock, patch

import pytest
from embedding.models.onnx.export_state import (
    ExportReport,
    ExportState,
    ExportStatus,
    ModelExportReport,
    ModelExportStatus,
)


@pytest.fixture(autouse=True)
def reset_export_state():
    """Reset the singleton between tests."""
    ExportState._instance = None
    yield
    ExportState._instance = None


class TestExportStateInit:
    def test_initial_status_is_idle(self):
        state = ExportState()
        assert state._status == ExportStatus.IDLE

    def test_initial_models_empty(self):
        state = ExportState()
        assert state._models == {}

    def test_singleton_returns_same_instance(self):
        a = ExportState()
        b = ExportState()
        assert a is b

    def test_is_running_returns_false_initially(self):
        state = ExportState()
        assert state.is_running() is False


class TestStartExport:
    @patch("embedding.models.onnx.export_state.threading.Thread")
    def test_start_export_sets_running(self, mock_thread_cls):
        mock_thread_cls.return_value = MagicMock()
        state = ExportState()
        report = state.start_export()
        assert report.overall_status == ExportStatus.RUNNING
        assert len(report.models) == 4
        mock_thread_cls.assert_called_once()
        mock_thread_cls.return_value.start.assert_called_once()

    @patch("embedding.models.onnx.export_state.threading.Thread")
    def test_start_export_returns_report(self, mock_thread_cls):
        mock_thread_cls.return_value = MagicMock()
        state = ExportState()
        report = state.start_export()
        assert isinstance(report, ExportReport)
        assert report.start_time is not None
        assert len(report.models) == 4

    @patch("embedding.models.onnx.export_state.threading.Thread")
    def test_concurrent_start_returns_existing(self, mock_thread_cls):
        mock_thread_cls.return_value = MagicMock()
        state = ExportState()
        state.start_export()
        # Second call should return existing status (no new thread)
        report2 = state.start_export()
        assert report2.overall_status == ExportStatus.RUNNING
        # Thread was only created once
        assert mock_thread_cls.call_count == 1


class TestGetReport:
    def test_get_report_initial_structure(self):
        state = ExportState()
        report = state.get_report()
        assert isinstance(report, ExportReport)
        assert report.overall_status == ExportStatus.IDLE
        assert report.models == []
        assert report.start_time is None
        assert report.end_time is None
        assert report.total_duration_ms is None

    @patch("embedding.models.onnx.export_state.threading.Thread")
    def test_get_report_after_start(self, mock_thread_cls):
        mock_thread_cls.return_value = MagicMock()
        state = ExportState()
        state.start_export()
        report = state.get_report()
        assert report.overall_status == ExportStatus.RUNNING
        assert len(report.models) == 4
        assert report.start_time is not None


class TestRunExportInner:
    @patch("embedding.models.onnx.export_state.ExportState._load_export_functions")
    def test_successful_export_sets_completed(self, mock_load):
        mock_load.return_value = {name: lambda: None for name in [
            "efficientnet_b0", "clip_vit_b16", "fashion_clip", "dinov2_vits14"
        ]}
        state = ExportState()
        state._start_time = time.time()
        state._models = {
            name: ModelExportReport(model_name=name)
            for name in ["efficientnet_b0", "clip_vit_b16", "fashion_clip", "dinov2_vits14"]
        }
        state._run_export_inner()

        assert state._status == ExportStatus.COMPLETED
        assert state._end_time is not None
        for m in state._models.values():
            assert m.status == ModelExportStatus.COMPLETED
            assert m.duration_ms is not None

    @patch("embedding.models.onnx.export_state.ExportState._load_export_functions")
    def test_partial_failure_sets_completed(self, mock_load):
        def failing_export():
            raise RuntimeError("Export failed")

        mock_load.return_value = {
            "efficientnet_b0": lambda: None,
            "clip_vit_b16": failing_export,
            "fashion_clip": lambda: None,
            "dinov2_vits14": failing_export,
        }
        state = ExportState()
        state._start_time = time.time()
        state._models = {
            name: ModelExportReport(model_name=name)
            for name in ["efficientnet_b0", "clip_vit_b16", "fashion_clip", "dinov2_vits14"]
        }
        state._run_export_inner()

        assert state._status == ExportStatus.COMPLETED
        assert state._models["efficientnet_b0"].status == ModelExportStatus.COMPLETED
        assert state._models["clip_vit_b16"].status == ModelExportStatus.FAILED
        assert state._models["clip_vit_b16"].error == "Export failed"
        assert state._models["fashion_clip"].status == ModelExportStatus.COMPLETED
        assert state._models["dinov2_vits14"].status == ModelExportStatus.FAILED

    @patch("embedding.models.onnx.export_state.ExportState._load_export_functions")
    def test_all_failure_sets_failed(self, mock_load):
        def failing_export():
            raise RuntimeError("Failed")

        mock_load.return_value = {name: failing_export for name in [
            "efficientnet_b0", "clip_vit_b16", "fashion_clip", "dinov2_vits14"
        ]}
        state = ExportState()
        state._start_time = time.time()
        state._models = {
            name: ModelExportReport(model_name=name)
            for name in ["efficientnet_b0", "clip_vit_b16", "fashion_clip", "dinov2_vits14"]
        }
        state._run_export_inner()

        assert state._status == ExportStatus.FAILED


class TestModelExportReport:
    def test_report_defaults(self):
        report = ModelExportReport(model_name="test_model")
        assert report.model_name == "test_model"
        assert report.status == ModelExportStatus.PENDING
        assert report.duration_ms is None
        assert report.error is None


class TestExportReportDataclass:
    def test_report_defaults(self):
        report = ExportReport()
        assert report.overall_status == ExportStatus.IDLE
        assert report.models == []
        assert report.start_time is None
