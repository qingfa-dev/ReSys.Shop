"""
Main entry point for the inference service.
Initializes the FastAPI application and launches listeners (HTTP and optional HTTPS).
"""
import os
import argparse
import multiprocessing
from typing import Optional

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from opentelemetry.instrumentation.fastapi import FastAPIInstrumentor
from slowapi import _rate_limit_exceeded_handler
from slowapi.errors import RateLimitExceeded
from scalar_fastapi import get_scalar_api_reference

from fastapi.exceptions import RequestValidationError
from starlette.exceptions import HTTPException as StarletteHTTPException

from src.core.config import settings
from src.core.telemetry import setup_telemetry
from src.core.rate_limit import limiter
from src.core.security import resolve_ssl_paths
from src.api.router import api_router
from src.api.middleware.exception_handlers import (
    global_exception_handler, 
    http_exception_handler, 
    validation_exception_handler
)


def create_app() -> FastAPI:
    """Application factory pattern."""
    setup_telemetry()

    app = FastAPI(
        title=settings.PROJECT_NAME,
        version="1.0.0",
        description="High-performance Inference Service",
        docs_url="/docs",
        redoc_url="/redoc",
    )

    app.state.limiter = limiter
    app.add_exception_handler(RateLimitExceeded, _rate_limit_exceeded_handler)
    app.add_exception_handler(RequestValidationError, validation_exception_handler)
    app.add_exception_handler(StarletteHTTPException, http_exception_handler)
    app.add_exception_handler(Exception, global_exception_handler)
    
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

    FastAPIInstrumentor.instrument_app(app)

    # Register the main router aggregator
    app.include_router(api_router)

    return app


# Create singleton instance
app = create_app()


def run_instance(port: int, cert_path: Optional[str] = None, key_path: Optional[str] = None):
    """Launches a single Uvicorn instance."""
    import uvicorn
    
    mode = "HTTPS" if cert_path and key_path else "HTTP"
    print(f"🚀 Starting {mode} listener on port {port}")

    uvicorn_kwargs = {
        "app": "src.main:app",
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
    """CLI entry point handling port resolution and dual-protocol orchestration."""
    parser = argparse.ArgumentParser(description="Inference Service Runner")
    parser.add_argument("--port", type=int, help="HTTP Port")
    parser.add_argument("--https-port", type=int, help="HTTPS Port")
    parser.add_argument("--ssl-cert", type=str, help="Path to SSL Certificate")
    parser.add_argument("--ssl-key", type=str, help="Path to SSL Private Key")
    args = parser.parse_args()

    # --- 1. Resolve Ports ---
    h_port = args.port or int(os.getenv("PORT", settings.PORT))
    s_port = args.https_port or int(os.getenv("HTTPS_PORT", settings.HTTPS_PORT))

    # --- 2. Resolve SSL Artifacts ---
    cert_path, key_path = resolve_ssl_paths(args.ssl_cert, args.ssl_key)
    
    ssl_available = (
        cert_path and key_path 
        and os.path.exists(cert_path) 
        and os.path.exists(key_path)
    )

    # --- 3. Orchestration ---
    if ssl_available:
        print(f"🔐 SSL detected. Spawning dual listeners (HTTP:{h_port}, HTTPS:{s_port})")
        
        p_http = multiprocessing.Process(target=run_instance, args=(h_port,))
        p_http.start()

        try:
            run_instance(s_port, cert_path, key_path)
        finally:
            p_http.terminate()
            p_http.join()
    else:
        if cert_path or key_path:
            print("⚠️  SSL configuration incomplete (missing file). Falling back to HTTP.")
        run_instance(h_port)


if __name__ == "__main__":
    main()
