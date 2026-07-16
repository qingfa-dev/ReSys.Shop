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
from embedding.core.constants import Constants
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
    """Check: OTLP gRPC endpoint is reachable to avoid noisy connection errors.

    Args:
        endpoint: The OTLP endpoint URL (e.g. http://localhost:4317).

    Returns:
        True if a socket connection succeeds within 1 second.
    """
    if not endpoint:
        return False

    try:
        # Parse: Extract host and port from the endpoint URL
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
    """Orchestrates observability initialization with dual-output support.

    Always enables Console output; optionally adds OTLP if the endpoint is reachable.
    Initializes tracer, metrics, and logging providers.
    """
    # Uses the standardized setting name
    endpoint = settings.OTEL_EXPORTER_OTLP_ENDPOINT
    # Check: Probe OTLP endpoint reachability before attempting connection
    otlp_enabled = is_otlp_available(endpoint)

    # 1. Resource Metadata
    # Create: Shared resource with service identity and version
    resource = Resource.create({
        SERVICE_NAME: settings.PROJECT_NAME,
        DEPLOYMENT_ENVIRONMENT: settings.ENVIRONMENT,
        "service.version": Constants.Strings.VERSION,
    })

    # 2. Protocol Security
    # Check: Determine if OTLP connection uses insecure gRPC
    if otlp_enabled:
        is_insecure = endpoint.startswith("http://")
        _setup_ssl_context(is_insecure)
    else:
        is_insecure = True

    # 3. Initialize Pillars
    # Initialize: Tracer, metrics, and logging providers in order
    _init_tracer(endpoint, resource, is_insecure, otlp_enabled)
    _init_metrics(endpoint, resource, is_insecure, otlp_enabled)
    _init_logging(endpoint, resource, is_insecure, otlp_enabled)

    if otlp_enabled:
        logger.info("Telemetry initialized: [Console + OTLP] -> %s", endpoint)
    else:
        logger.info("Telemetry initialized: [Console Only] (OTLP collector unreachable or missing)")


def _setup_ssl_context(is_insecure: bool):
    """Configure SSL environment variables for secure gRPC when not using insecure mode.

    Args:
        is_insecure: If True, skip SSL configuration entirely.
    """
    if is_insecure:
        return
    # Resolve: OTLP certificate from env var or settings
    otlp_cert = os.getenv("OTEL_EXPORTER_OTLP_CERTIFICATE") or settings.SSL_CERT_FILE
    if otlp_cert:
        os.environ["GRPC_DEFAULT_SSL_ROOTS_FILE_PATH"] = otlp_cert
    if settings.SSL_CERT_FILE:
        os.environ["SSL_CERT_FILE"] = settings.SSL_CERT_FILE
    if settings.SSL_CERT_DIR:
        os.environ["SSL_CERT_DIR"] = settings.SSL_CERT_DIR


def _init_tracer(endpoint: str, resource: Resource, is_insecure: bool, otlp_enabled: bool):
    """Initialize the OpenTelemetry trace provider with console and optional OTLP export.

    Args:
        endpoint: OTLP gRPC endpoint URL.
        resource: Shared resource metadata for all signals.
        is_insecure: Use insecure gRPC if True.
        otlp_enabled: Enable OTLP exporter if True.
    """
    # Create: Tracer provider with service resource metadata
    provider = TracerProvider(resource=resource)
    if settings.ENVIRONMENT == "dev":
        # Simple: Console span export for development visibility
        provider.add_span_processor(SimpleSpanProcessor(ConsoleSpanExporter(out=sys.stdout)))
    if otlp_enabled:
        # Batch: OTLP span export for production telemetry pipeline
        provider.add_span_processor(
            BatchSpanProcessor(
                OTLPSpanExporter(endpoint=endpoint, insecure=is_insecure)
            )
        )
    trace.set_tracer_provider(provider)


def _init_metrics(endpoint: str, resource: Resource, is_insecure: bool, otlp_enabled: bool):
    """Initialize the OpenTelemetry meter provider with periodic console and OTLP export.

    Args:
        endpoint: OTLP gRPC endpoint URL.
        resource: Shared resource metadata for all signals.
        is_insecure: Use insecure gRPC if True.
        otlp_enabled: Enable OTLP exporter if True.
    """
    readers = []
    if settings.ENVIRONMENT == "dev":
        # Periodic: Console metric export every 60 s for development
        readers.append(
            PeriodicExportingMetricReader(
                ConsoleMetricExporter(out=sys.stdout),
                export_interval_millis=60000,
            )
        )
    if otlp_enabled:
        # Periodic: OTLP metric export every 60 s for production
        readers.append(
            PeriodicExportingMetricReader(
                OTLPMetricExporter(endpoint=endpoint, insecure=is_insecure),
                export_interval_millis=60000,
            )
        )
    if readers:
        metrics.set_meter_provider(MeterProvider(resource=resource, metric_readers=readers))


def _init_logging(endpoint: str, resource: Resource, is_insecure: bool, otlp_enabled: bool):
    """Initialize standard logging with OpenTelemetry log correlation.

    Configures Python logging with structured format and attaches OTel log handler.
    Also instruments uvicorn and fastapi loggers for unified output.

    Args:
        endpoint: OTLP gRPC endpoint URL.
        resource: Shared resource metadata for all signals.
        is_insecure: Use insecure gRPC if True.
        otlp_enabled: Enable OTLP log export if True.
    """
    log_format = "%(asctime)s %(levelname)s [%(name)s] %(message)s"
    log_level = getattr(logging, settings.LOG_LEVEL.upper(), logging.INFO)
    logging.basicConfig(
        level=log_level,
        format=log_format,
        handlers=[logging.StreamHandler(sys.stdout)],
        force=True,
    )

    # Create: Logger provider with service resource
    provider = LoggerProvider(resource=resource)
    if settings.ENVIRONMENT == "dev":
        provider.add_log_record_processor(SimpleLogRecordProcessor(ConsoleLogRecordExporter(out=sys.stdout)))
    if otlp_enabled:
        # Batch: OTLP log export for production log aggregation
        provider.add_log_record_processor(
            BatchLogRecordProcessor(
                OTLPLogExporter(endpoint=endpoint, insecure=is_insecure)
            )
        )
    set_logger_provider(provider)

    # Instrument: Attach OTel handler to root and framework loggers
    LoggingInstrumentor().instrument(set_logging_format=False)
    otel_handler = LoggingHandler(level=logging.NOTSET, logger_provider=provider)
    logging.getLogger().addHandler(otel_handler)

    for name in ("uvicorn", "uvicorn.error", "uvicorn.access", "fastapi"):
        logging.getLogger(name).addHandler(otel_handler)
        logging.getLogger(name).setLevel(log_level)


def get_tracer(name: str):
    """Returns a named OpenTelemetry tracer instance for instrumentation."""
    return trace.get_tracer(name)


def get_meter(name: str):
    """Returns a named OpenTelemetry meter instance for metrics."""
    return metrics.get_meter(name)
