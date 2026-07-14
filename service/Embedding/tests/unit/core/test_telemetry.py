"""
Unit tests for the Telemetry module.
"""
import os
import pytest
from unittest.mock import patch, MagicMock
from opentelemetry import trace, metrics
from opentelemetry._logs import get_logger_provider
from opentelemetry.sdk.resources import SERVICE_NAME, DEPLOYMENT_ENVIRONMENT
from embedding.core.telemetry import setup_telemetry, get_tracer, get_meter


def test_telemetry_initialization_standard():
    """Verify that setup_telemetry registers global providers even without OTLP."""
    # Ensure any env var is cleared for standard test
    with patch.dict(os.environ, {"OTEL_EXPORTER_OTLP_ENDPOINT": ""}):
        # We also need to patch settings to ensure it doesn't have a default that triggers OTLP
        with patch("src.core.config.settings.OTEL_EXPORTER_OTLP_ENDPOINT", ""):
            setup_telemetry()
            
            # Verify Trace Provider
            tracer_provider = trace.get_tracer_provider()
            assert tracer_provider is not None
            
            # Verify Meter Provider
            meter_provider = metrics.get_meter_provider()
            assert meter_provider is not None
            
            # Verify Log Provider
            log_provider = get_logger_provider()
            assert log_provider is not None


def test_get_tracer_helper():
    """Verify that get_tracer returns a valid OTel tracer."""
    tracer = get_tracer("test-tracer")
    assert tracer is not None
    assert hasattr(tracer, "start_as_current_span")


def test_get_meter_helper():
    """Verify that get_meter returns a valid OTel meter."""
    meter = get_meter("test-meter")
    assert meter is not None
    assert hasattr(meter, "create_counter")


def test_resource_attributes():
    """Verify that resource metadata is correctly configured."""
    setup_telemetry()
    provider = trace.get_tracer_provider()
    if hasattr(provider, "resource"):
        attrs = provider.resource.attributes
        assert attrs[SERVICE_NAME] == "inference"
        assert attrs[DEPLOYMENT_ENVIRONMENT] == "test"


@patch("src.core.telemetry.OTLPSpanExporter")
@patch("src.core.telemetry.OTLPMetricExporter")
@patch("src.core.telemetry.OTLPLogExporter")
@patch("src.core.telemetry.is_otlp_available", return_value=True)
def test_telemetry_with_otlp_endpoint(mock_avail, mock_log, mock_metric, mock_trace):
    """Verify that OTLP exporters are initialized when endpoint is present."""
    # Ensure ENVIRONMENT is dev to trigger processors/readers
    with patch("src.core.config.settings.ENVIRONMENT", "dev"):
        with patch.dict(os.environ, {"OTEL_EXPORTER_OTLP_ENDPOINT": "http://localhost:4317"}):
            # Also patch settings to match
            with patch("src.core.config.settings.OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:4317"):
                setup_telemetry()
                mock_trace.assert_called()
                mock_metric.assert_called()
                mock_log.assert_called()
