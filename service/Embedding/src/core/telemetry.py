"""
Advanced Telemetry module for the Inference service.
Supports Dual Output: Terminal (Console) + Telemetry (OTLP).
Automatically falls back to standard console logging if OTLP is unavailable.
"""
import logging
import os
import socket
import sys

from embedding.core.config import settings
from opentelemetry import metrics, trace

# Internal OTel logging imports
from opentelemetry._logs import set_logger_provider
from opentelemetry.exporter.otlp.proto.grpc._log_exporter import OTLPLogExporter
from opentelemetry.exporter.otlp.proto.grpc.metric_exporter import OTLPMetricExporter
from opentelemetry.exporter.otlp.proto.grpc.trace_exporter import OTLPSpanExporter
from opentelemetry.instrumentation.logging import LoggingInstrumentor
from opentelemetry.instrumentation.logging.handler import LoggingHandler
from opentelemetry.sdk._logs import LoggerProvider
from opentelemetry.sdk._logs.export import (
    BatchLogRecordProcessor,
    ConsoleLogRecordExporter,
    SimpleLogRecordProcessor,
)
from opentelemetry.sdk.metrics import MeterProvider
from opentelemetry.sdk.metrics.export import ConsoleMetricExporter, PeriodicExportingMetricReader
from opentelemetry.sdk.resources import DEPLOYMENT_ENVIRONMENT, SERVICE_NAME, Resource
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import (
    BatchSpanProcessor,
    ConsoleSpanExporter,
    SimpleSpanProcessor,
)

logger = logging.getLogger(__name__)


def is_otlp_available(endpoint: str) -> bool:
    """
    Empirically checks if the OTLP endpoint is reachable to avoid noisy gRPC errors.
    """
    if not endpoint:
        return False

    try:
        # Extract host and port
        host_port = endpoint.split("//")[-1].split("/") [0]
        if ":" in host_port:
            host, port = host_port.split(":")
            port = int(port)
        else:
            host = host_port
            port = 4317

        with socket.create_connection((host, port), timeout=1) as _:
            return True
    except (socket.error, ValueError, IndexError):
        return False


def setup_telemetry():
    """
    Orchestrates observability with dual-output support.
    Always enables Console output; optionally adds OTLP if reachable.
    """
    # Uses the standardized setting name
    endpoint = settings.OTEL_EXPORTER_OTLP_ENDPOINT
    otlp_enabled = is_otlp_available(endpoint)

    # 1. Resource Metadata
    resource = Resource.create({
        SERVICE_NAME: settings.PROJECT_NAME,
        DEPLOYMENT_ENVIRONMENT: settings.ENVIRONMENT,
        "service.version": "1.0.0",
    })

    # 2. Protocol Security
    if otlp_enabled:
        is_insecure = endpoint.startswith("http://")
        _setup_ssl_context(is_insecure)
    else:
        is_insecure = True

    # 3. Initialize Pillars
    _init_tracer(endpoint, resource, is_insecure, otlp_enabled)
    _init_metrics(endpoint, resource, is_insecure, otlp_enabled)
    _init_logging(endpoint, resource, is_insecure, otlp_enabled)

    if otlp_enabled:
        logger.info("Telemetry initialized: [Console + OTLP] -> %s", endpoint)
    else:
        logger.info("Telemetry initialized: [Console Only] (OTLP collector unreachable or missing)")


def _setup_ssl_context(is_insecure: bool):
    """Configures SSL environment variables."""
    if is_insecure:
        return
    otlp_cert = os.getenv("OTEL_EXPORTER_OTLP_CERTIFICATE") or settings.SSL_CERT_FILE
    if otlp_cert:
        os.environ["GRPC_DEFAULT_SSL_ROOTS_FILE_PATH"] = otlp_cert
    if settings.SSL_CERT_FILE:
        os.environ["SSL_CERT_FILE"] = settings.SSL_CERT_FILE
    if settings.SSL_CERT_DIR:
        os.environ["SSL_CERT_DIR"] = settings.SSL_CERT_DIR


def _init_tracer(endpoint: str, resource: Resource, is_insecure: bool, otlp_enabled: bool):
    provider = TracerProvider(resource=resource)
    if settings.ENVIRONMENT == "dev":
        provider.add_span_processor(SimpleSpanProcessor(ConsoleSpanExporter(out=sys.stdout)))
    if otlp_enabled:
        provider.add_span_processor(
            BatchSpanProcessor(
                OTLPSpanExporter(endpoint=endpoint, insecure=is_insecure)
            )
        )
    trace.set_tracer_provider(provider)


def _init_metrics(endpoint: str, resource: Resource, is_insecure: bool, otlp_enabled: bool):
    readers = []
    if settings.ENVIRONMENT == "dev":
        readers.append(
            PeriodicExportingMetricReader(
                ConsoleMetricExporter(out=sys.stdout),
                export_interval_millis=60000,
            )
        )
    if otlp_enabled:
        readers.append(
            PeriodicExportingMetricReader(
                OTLPMetricExporter(endpoint=endpoint, insecure=is_insecure),
                export_interval_millis=60000,
            )
        )
    if readers:
        metrics.set_meter_provider(MeterProvider(resource=resource, metric_readers=readers))


def _init_logging(endpoint: str, resource: Resource, is_insecure: bool, otlp_enabled: bool):
    log_format = "%(asctime)s %(levelname)s [%(name)s] %(message)s"
    log_level = getattr(logging, settings.LOG_LEVEL.upper(), logging.INFO)
    logging.basicConfig(
        level=log_level,
        format=log_format,
        handlers=[logging.StreamHandler(sys.stdout)],
        force=True,
    )

    provider = LoggerProvider(resource=resource)
    if settings.ENVIRONMENT == "dev":
        provider.add_log_record_processor(SimpleLogRecordProcessor(ConsoleLogRecordExporter(out=sys.stdout)))
    if otlp_enabled:
        provider.add_log_record_processor(
            BatchLogRecordProcessor(
                OTLPLogExporter(endpoint=endpoint, insecure=is_insecure)
            )
        )
    set_logger_provider(provider)

    LoggingInstrumentor().instrument(set_logging_format=False)
    otel_handler = LoggingHandler(level=logging.NOTSET, logger_provider=provider)
    logging.getLogger().addHandler(otel_handler)

    for name in ("uvicorn", "uvicorn.error", "uvicorn.access", "fastapi"):
        logging.getLogger(name).addHandler(otel_handler)
        logging.getLogger(name).setLevel(log_level)


def get_tracer(name: str):
    return trace.get_tracer(name)


def get_meter(name: str):
    return metrics.get_meter(name)
