"""
Main entry point for the inference service.
Initializes the FastAPI application and launches listeners (HTTP and optional HTTPS).
"""
import argparse
import multiprocessing
import os
from typing import Optional

from embedding.api.middleware.exception_handlers import (
    global_exception_handler,
    http_exception_handler,
    validation_exception_handler,
)
from embedding.api.router import api_router
from embedding.core.config import settings
from embedding.core.constants import Constants
from embedding.core.rate_limit import limiter
from embedding.core.security import resolve_ssl_paths
from embedding.core.telemetry import setup_telemetry
from fastapi import FastAPI
from fastapi.exceptions import RequestValidationError
from fastapi.middleware.cors import CORSMiddleware
from opentelemetry.instrumentation.fastapi import FastAPIInstrumentor
from scalar_fastapi import get_scalar_api_reference
from slowapi import _rate_limit_exceeded_handler
from slowapi.errors import RateLimitExceeded
from starlette.exceptions import HTTPException as StarletteHTTPException


def create_app() -> FastAPI:
    """Application factory: Creates and configures the FastAPI application.

    Sets up telemetry, exception handlers, CORS middleware, OpenAPI docs,
    and route registration.

    Returns:
        A fully configured FastAPI application instance.
    """
    # Initialize: OpenTelemetry telemetry stack (tracing, metrics, logging)
    setup_telemetry()

    # Create: FastAPI application with metadata and interactive docs
    app = FastAPI(
        title=settings.PROJECT_NAME,
        version=Constants.Strings.VERSION,
        description="High-performance Inference Service",
        docs_url="/docs",
        redoc_url="/redoc",
    )

    # Register: Global exception handlers — rate limit, validation, HTTP, unhandled
    app.state.limiter = limiter
    app.add_exception_handler(RateLimitExceeded, _rate_limit_exceeded_handler)
    app.add_exception_handler(RequestValidationError, validation_exception_handler)
    app.add_exception_handler(StarletteHTTPException, http_exception_handler)
    app.add_exception_handler(Exception, global_exception_handler)

    # Add: CORS middleware with allowlisted origins from configuration
    app.add_middleware(
        CORSMiddleware,
        allow_origins=settings.CORS_ORIGINS,
        allow_credentials=False,
        allow_methods=["*"],
        allow_headers=["*"],
    )

    @app.get("/scalar", include_in_schema=False)
    async def scalar_html():
        """Generates Scalar API reference page."""
        return get_scalar_api_reference(
            openapi_url=app.openapi_url,
            title=app.title,
        )

    # Instrument: Attach OpenTelemetry middleware for automatic span creation
    FastAPIInstrumentor.instrument_app(app)

    # Register: Main router aggregator with all endpoint groups
    # Boundary: API → Router — do not add endpoint routes directly in create_app
    app.include_router(api_router)

    return app


# Create singleton instance
app = create_app()


def run_instance(port: int, cert_path: Optional[str] = None, key_path: Optional[str] = None):
    """Launches a single Uvicorn instance for the given port and optional SSL.

    Args:
        port: The port number to bind the HTTP/HTTPS listener to.
        cert_path: Optional path to the SSL certificate file (enables HTTPS).
        key_path: Optional path to the SSL private key file.
    """
    import uvicorn

    mode = "HTTPS" if cert_path and key_path else "HTTP"
    print(f"🚀 Starting {mode} listener on port {port}")

    uvicorn_kwargs = {
        "app": "embedding.main:app",
        "host": "0.0.0.0",
        "port": port,
        "reload": (mode == "HTTP" and settings.ENVIRONMENT == "dev"),
        "log_level": settings.LOG_LEVEL.lower()
    }

    if cert_path and key_path:
        uvicorn_kwargs["ssl_certfile"] = cert_path
        uvicorn_kwargs["ssl_keyfile"] = key_path

    uvicorn.run(**uvicorn_kwargs)


def main():
    """CLI entry point: Resolves ports and SSL artifacts, then orchestration.

    Supports dual-protocol mode (HTTP + HTTPS) using multiprocessing when SSL
    certificates are available. Falls back to HTTP-only if SSL is incomplete.

    Port resolution priority:
    1. CLI --port / --https-port arguments
    2. PORT / HTTPS_PORT environment variables
    3. Settings defaults
    """
    parser = argparse.ArgumentParser(description="Inference Service Runner")
    parser.add_argument("--port", type=int, help="HTTP Port")
    parser.add_argument("--https-port", type=int, help="HTTPS Port")
    parser.add_argument("--ssl-cert", type=str, help="Path to SSL Certificate")
    parser.add_argument("--ssl-key", type=str, help="Path to SSL Private Key")
    args = parser.parse_args()

    # --- 1. Resolve Ports ---
    # Resolve: CLI arg > environment variable > settings default
    h_port = args.port or int(os.getenv("PORT", settings.PORT))
    s_port = args.https_port or int(os.getenv("HTTPS_PORT", settings.HTTPS_PORT))

    # --- 2. Resolve SSL Artifacts ---
    # Resolve: Multi-source SSL resolution (CLI, Aspire, config, auto-discover)
    cert_path, key_path = resolve_ssl_paths(args.ssl_cert, args.ssl_key)

    # Guard: Verify both cert and key files exist on disk
    ssl_available = (
        cert_path and key_path
        and os.path.exists(cert_path)
        and os.path.exists(key_path)
    )

    # --- 3. Orchestration ---
    if ssl_available:
        print(f"🔐 SSL detected. Spawning dual listeners (HTTP:{h_port}, HTTPS:{s_port})")

        # Spawn: Separate process for HTTP listener (non-blocking)
        p_http = multiprocessing.Process(target=run_instance, args=(h_port,))
        p_http.start()

        try:
            # Run: HTTPS listener in the main process (blocks until interrupted)
            run_instance(s_port, cert_path, key_path)
        finally:
            # Cleanup: Terminate the HTTP child process on exit
            p_http.terminate()
            p_http.join()
    else:
        if cert_path or key_path:
            # Warn: SSL config incomplete — fall back to plain HTTP
            print("⚠️  SSL configuration incomplete (missing file). Falling back to HTTP.")
        run_instance(h_port)


if __name__ == "__main__":
    main()
