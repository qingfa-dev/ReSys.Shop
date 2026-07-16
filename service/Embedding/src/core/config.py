"""
Configuration module for inference.

This module defines the global settings for the FastAPI application using Pydantic Settings.
It handles environment variable loading from .env and .env.{environment} files,
providing type-safe access and robust validation.
"""
import logging
import os
from enum import Enum
from pathlib import Path
from typing import List, Optional

from embedding.core.constants import Constants
from pydantic import Field, field_validator, model_validator
from pydantic_settings import BaseSettings, SettingsConfigDict

logger = logging.getLogger(__name__)

def detect_project_root() -> Path:
    """
    Detects the project root directory by traversing up from the current file
    until a marker file (like pyproject.toml) is found.

    Returns:
        Path to the project root directory.
    """
    current = Path(__file__).resolve().parent
    for parent in [current] + list(current.parents):
        if (parent / "pyproject.toml").exists() or (parent / "uv.lock").exists():
            return parent
    # Fallback: Return CWD when no marker file found upstream
    return Path.cwd()

# sidecar root directory for relative path resolution
SERVICE_ROOT = detect_project_root()


class LogLevel(str, Enum):
    """Supported logging levels for the application."""
    DEBUG = "DEBUG"
    INFO = "INFO"
    WARNING = "WARNING"
    ERROR = "ERROR"
    CRITICAL = "CRITICAL"


class Environment(str, Enum):
    """Supported runtime environments."""
    DEV = "dev"
    TEST = "test"
    PRODUCTION = "production"


class Settings(BaseSettings):
    """
    Global application settings for the Inference service.
    Provides type-safe access to environment variables with validation.
    """
    # Boundary: Config → Environment — do not add business logic here;
    #            this class reads env vars and validates them only

    # ── Service Identity & Network ───────────────────────────────────────────────
    PROJECT_NAME: str = Field(
        default="Embedding Service",
        description="The human-readable name of the service.",
        json_schema_extra={"example": "inference-sidecar"}
    )
    PORT: int = Field(
        default=Constants.Defaults.PORT,
        ge=Constants.Constraints.PORT_MIN,
        le=Constants.Constraints.PORT_MAX,
        description="The local network port the HTTP server binds to.",
        json_schema_extra={"example": 5002}
    )
    HTTPS_PORT: int = Field(
        default=Constants.Defaults.HTTPS_PORT,
        ge=Constants.Constraints.PORT_MIN,
        le=Constants.Constraints.PORT_MAX,
        description="The local network port the HTTPS server binds to.",
        json_schema_extra={"example": 5003}
    )

    # ── Security ──────────────────────────────────────────────────────────────────
    API_KEY: str = Field(
        default="dev-key-must-be-long-enough",
        min_length=Constants.Constraints.API_KEY_MIN_LENGTH,
        description="Shared secret for authenticating internal sidecar calls.",
        json_schema_extra={"example": "a-very-long-and-secure-random-key-12345"}
    )
    RATE_LIMIT: str = Field(
        default="50/minute",
        description="Global rate limit for embedding endpoints (e.g., '50/minute').",
        json_schema_extra={"example": "100/minute"}
    )

    # ── External Integrations ─────────────────────────────────────────────────────
    HUGGING_FACE_TOKEN: Optional[str] = Field(
        default=None,
        description="Hugging Face Hub token for gated models (optional).",
        json_schema_extra={"example": "hf_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"}
    )

    # ── Machine Learning Configuration ────────────────────────────────────────────
    ONNX_MODEL_DIR: str = Field(
        default=str(SERVICE_ROOT / "models"),
        description="Absolute path to the directory containing ONNX model artifacts.",
        json_schema_extra={"example": "/app/models"}
    )
    EMBEDDING_MODEL: str = Field(
        default="fashion_clip",
        description="Default model name used when request does not specify one.",
        json_schema_extra={"example": "fashion_clip"}
    )
    # ── SSL Certificate Configuration ─────────────────────────────────────────────
    SSL_CERT_FILE: Optional[str] = Field(
        default=None,
        description="Path to the SSL certificate file (.pem).",
        json_schema_extra={"example": "/certs/cert.pem"}
    )
    SSL_CERT_DIR: Optional[str] = Field(
        default=None,
        description="Directory containing SSL certificates for auto-discovery.",
        json_schema_extra={"example": "/certs"}
    )

    # ── CORS Settings ─────────────────────────────────────────────────────────────
    CORS_ORIGINS: List[str] = Field(
        default=["http://localhost:3000", "http://localhost:5173"],
        description="Allowed origins for Cross-Origin Resource Sharing.",
        json_schema_extra={"example": ["https://shop.example.com"]}
    )

    # ── Environment ──────────────────────────────────────────────────────────────
    ENVIRONMENT: Environment = Field(
        default=Environment.DEV,
        description="The runtime environment (dev, test, production).",
        json_schema_extra={"example": "production"}
    )

    # ── Observability ─────────────────────────────────────────────────────────────
    LOG_LEVEL: LogLevel = Field(
        default=LogLevel.INFO,
        description="The verbosity level for application logging.",
        json_schema_extra={"example": "DEBUG"}
    )
    OTEL_EXPORTER_OTLP_ENDPOINT: str = Field(
        default="http://localhost:4317",
        description="The OTLP gRPC endpoint for telemetry exports.",
        json_schema_extra={"example": "http://otel-collector:4317"}
    )

    # ── Performance ──────────────────────────────────────────────────────────────
    OMP_NUM_THREADS: int = Field(
        default=Constants.Defaults.OMP_NUM_THREADS,
        ge=Constants.Constraints.THREAD_COUNT_MIN,
        le=Constants.Constraints.THREAD_COUNT_MAX,
        description="Number of threads for OpenMP (CPU-parallelism).",
        json_schema_extra={"example": 8}
    )
    MKL_NUM_THREADS: int = Field(
        default=Constants.Defaults.MKL_NUM_THREADS,
        ge=Constants.Constraints.THREAD_COUNT_MIN,
        le=Constants.Constraints.THREAD_COUNT_MAX,
        description="Number of threads for Intel MKL.",
        json_schema_extra={"example": 8}
    )
    NUMEXPR_NUM_THREADS: int = Field(
        default=Constants.Defaults.NUMEXPR_NUM_THREADS,
        ge=Constants.Constraints.THREAD_COUNT_MIN,
        le=Constants.Constraints.THREAD_COUNT_MAX,
        description="Number of threads for NumExpr.",
        json_schema_extra={"example": 8}
    )

    # ── Validation ────────────────────────────────────────────────────────────────
    @field_validator("ENVIRONMENT", mode="before")
    @classmethod
    def validate_env_name(cls, v: str | Environment) -> Environment:
        """Validate and normalize the environment name, accepting 'prod' as alias."""
        if isinstance(v, Environment):
            return v

        # Normalize: Accept 'prod' as shorthand for 'production'
        v = v.lower()
        if v == "prod":
            return Environment.PRODUCTION

        try:
            return Environment(v)
        except ValueError:
            allowed = [e.value for e in Environment] + ["prod"]
            raise ValueError(f"ENVIRONMENT must be one of {allowed}")

    @field_validator("LOG_LEVEL", mode="before")
    @classmethod
    def validate_log_level(cls, v: str | LogLevel) -> LogLevel:
        """Validate and normalize the log level string."""
        if isinstance(v, LogLevel):
            return v

        v = v.upper()
        try:
            return LogLevel(v)
        except ValueError:
            allowed = [level.value for level in LogLevel]
            raise ValueError(f"LOG_LEVEL must be one of {allowed}")

    @field_validator("RATE_LIMIT")
    @classmethod
    def validate_rate_limit(cls, v: str) -> str:
        """Validate: Rate limit string matches expected format (e.g. '50/minute')."""
        import re
        if not re.match(r"^\d+\s*/\s*(second|minute|hour|day)$", v.lower()):
            raise ValueError("RATE_LIMIT must be in format 'N/minute', 'N/hour', etc.")
        return v

    @field_validator("HUGGING_FACE_TOKEN")
    @classmethod
    def validate_hf_token(cls, v: Optional[str]) -> Optional[str]:
        """Validate: HuggingFace token starts with 'hf_' prefix."""
        if v and not v.startswith("hf_"):
            raise ValueError("HUGGING_FACE_TOKEN must start with 'hf_'")
        return v

    @field_validator("OTEL_EXPORTER_OTLP_ENDPOINT")
    @classmethod
    def validate_otlp_url(cls, v: str) -> str:
        """Validate: OTLP endpoint begins with http:// or https://."""
        if v and not v.startswith(("http://", "https://")):
            raise ValueError("OTEL_EXPORTER_OTLP_ENDPOINT must start with http:// or https://")
        return v

    @model_validator(mode="after")
    def resolve_absolute_paths(self) -> "Settings":
        """Enforce: All directory paths are absolute and numerical library thread vars are set."""
        self.ONNX_MODEL_DIR = str(Path(self.ONNX_MODEL_DIR).resolve())

        # Set environment variables for numerical libraries immediately
        os.environ["OMP_NUM_THREADS"] = str(self.OMP_NUM_THREADS)
        os.environ["MKL_NUM_THREADS"] = str(self.MKL_NUM_THREADS)
        os.environ["NUMEXPR_NUM_THREADS"] = str(self.NUMEXPR_NUM_THREADS)

        return self

    def verify_onnx_dir(self) -> bool:
        """Check: ONNX model directory exists and is a valid directory on disk.

        Returns:
            True if the directory exists and is a directory, False otherwise.
        """
        path = Path(self.ONNX_MODEL_DIR)
        return path.exists() and path.is_dir()

    # ── Configuration Loading ─────────────────────────────────────────────────────
    model_config = SettingsConfigDict(
        env_file=(
            ".env",
            f".env.{os.getenv('ENVIRONMENT', 'dev').lower()}",
        ),
        env_file_encoding="utf-8",
        env_nested_delimiter="__",
        extra="ignore",
    )


# Create: Singleton instance of settings for application-wide use
settings = Settings()
