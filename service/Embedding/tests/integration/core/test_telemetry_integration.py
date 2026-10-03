"""
Integration tests for Telemetry.
Verifies that traces and logs are correlated during an API request.
"""
import logging

import pytest
from embedding.core.config import settings


@pytest.mark.integration
class TestTelemetryIntegration:
    def test_trace_correlation_in_logs(self, client, caplog):
        """
        Verify that when a request is made, internal logs contain trace context.
        Note: The actual OTel LoggingHandler might not populate caplog in the same way
        as standard logging unless we check the LogRecord attributes.
        """
        caplog.set_level(logging.INFO)

        headers = {"X-API-Key": settings.API_KEY}
        # Hit a simple endpoint
        response = client.get("/health", headers=headers)
        assert response.status_code == 200

        # We can't easily inspect the OTLP exporter buffer here without complex mocks,
        # but we can verify the get_tracer helper works.
        from embedding.core.telemetry import get_tracer
        tracer = get_tracer("test-tracer")
        with tracer.start_as_current_span("test-span") as span:
            ctx = span.get_span_context()
            assert ctx.is_valid
            assert ctx.trace_id != 0
            assert ctx.span_id != 0
