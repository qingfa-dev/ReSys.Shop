"""
Unit tests for the Configuration and Settings module.
Validates environment loading, Pydantic type-safety, and custom constraints.
"""
import os
from unittest.mock import patch

import pytest
from embedding.core.config import Environment, LogLevel, Settings
from pydantic import ValidationError


def test_settings_default_values():
    """Verify that default values are correctly populated (including .env.test overrides)."""
    s = Settings()
    assert s.PROJECT_NAME == "inference"
    assert s.PORT == 5002
    assert s.ENVIRONMENT == Environment.TEST
    assert s.RATE_LIMIT == "50/minute"
    # Note: .env.test sets this to empty
    assert s.OTEL_EXPORTER_OTLP_ENDPOINT == ""
    assert "sidecar/models" in s.ONNX_MODEL_DIR.replace("\\", "/")


def test_port_validation_range():
    """Verify that the PORT must be between 1 and 65535."""
    # Valid low
    assert Settings(PORT=1).PORT == 1
    # Valid high
    assert Settings(PORT=65535).PORT == 65535

    # Invalid low
    with pytest.raises(ValidationError) as excinfo:
        Settings(PORT=0)
    assert "Input should be greater than or equal to 1" in str(excinfo.value)

    # Invalid high
    with pytest.raises(ValidationError) as excinfo:
        Settings(PORT=70000)
    assert "Input should be less than or equal to 65535" in str(excinfo.value)


def test_api_key_min_length():
    """Verify that the API_KEY has a minimum length constraint."""
    # Valid length (>= 16)
    valid_key = "secure-key-long-enough-12345"
    assert Settings(API_KEY=valid_key).API_KEY == valid_key

    # Invalid length (too short)
    with pytest.raises(ValidationError) as excinfo:
        Settings(API_KEY="too-short")
    assert "String should have at least 16 characters" in str(excinfo.value)


def test_otlp_endpoint_url_validation():
    """Verify that the OTEL_EXPORTER_OTLP_ENDPOINT requires a valid URL schema."""
    # Valid http
    assert (
        Settings(
            OTEL_EXPORTER_OTLP_ENDPOINT="http://collector:4317"
        ).OTEL_EXPORTER_OTLP_ENDPOINT
        == "http://collector:4317"
    )
    # Valid https
    assert (
        Settings(
            OTEL_EXPORTER_OTLP_ENDPOINT="https://secure:4317"
        ).OTEL_EXPORTER_OTLP_ENDPOINT
        == "https://secure:4317"
    )

    # Invalid schema
    with pytest.raises(ValidationError) as excinfo:
        Settings(OTEL_EXPORTER_OTLP_ENDPOINT="ftp://server")
    assert "OTEL_EXPORTER_OTLP_ENDPOINT must start with http:// or https://" in str(excinfo.value)


def test_environment_normalization():
    """Verify that ENVIRONMENT is normalized and validated against Environment enum."""
    assert Settings(ENVIRONMENT="PRODUCTION").ENVIRONMENT == Environment.PRODUCTION
    assert Settings(ENVIRONMENT="prod").ENVIRONMENT == Environment.PRODUCTION
    assert Settings(ENVIRONMENT="DEV").ENVIRONMENT == Environment.DEV

    # Invalid value
    with pytest.raises(ValidationError) as excinfo:
        Settings(ENVIRONMENT="staging")
    assert "ENVIRONMENT must be one of ['dev', 'test', 'production', 'prod']" in str(excinfo.value)


def test_cors_origins_list_loading():
    """Verify that CORS_ORIGINS can be loaded from a list."""
    origins = ["http://app1.local", "http://app2.local"]
    s = Settings(CORS_ORIGINS=origins)
    assert s.CORS_ORIGINS == origins
    assert len(s.CORS_ORIGINS) == 2


def test_hf_token_validation():
    """Verify that HUGGING_FACE_TOKEN must start with hf_."""
    # Valid
    assert Settings(HUGGING_FACE_TOKEN="hf_123").HUGGING_FACE_TOKEN == "hf_123"
    # None is valid (optional)
    assert Settings(HUGGING_FACE_TOKEN=None).HUGGING_FACE_TOKEN is None

    # Invalid
    with pytest.raises(ValidationError) as excinfo:
        Settings(HUGGING_FACE_TOKEN="invalid_token")
    assert "HUGGING_FACE_TOKEN must start with 'hf_'" in str(excinfo.value)


def test_log_level_validation():
    """Verify that LOG_LEVEL is normalized and validated against LogLevel enum."""
    # Valid string
    assert Settings(LOG_LEVEL="debug").LOG_LEVEL == LogLevel.DEBUG
    # Valid Enum
    assert Settings(LOG_LEVEL=LogLevel.WARNING).LOG_LEVEL == LogLevel.WARNING

    # Invalid value
    with pytest.raises(ValidationError) as excinfo:
        Settings(LOG_LEVEL="VERBOSE")
    assert (
        "LOG_LEVEL must be one of "
        "['DEBUG', 'INFO', 'WARNING', 'ERROR', 'CRITICAL']"
        in str(excinfo.value)
    )


def test_verify_onnx_dir_helper():
    """Verify the helper method for directory checking."""
    s = Settings()
    with patch("pathlib.Path.exists", return_value=True):
        with patch("pathlib.Path.is_dir", return_value=True):
            assert s.verify_onnx_dir() is True

    with patch("pathlib.Path.exists", return_value=False):
        assert s.verify_onnx_dir() is False


@pytest.mark.parametrize("env_var, value, attribute", [
    ("PORT", "8080", "PORT"),
    ("PROJECT_NAME", "my-service", "PROJECT_NAME"),
    ("ENVIRONMENT", "production", "ENVIRONMENT"),
    ("OTEL_EXPORTER_OTLP_ENDPOINT", "http://remote:4317", "OTEL_EXPORTER_OTLP_ENDPOINT"),
    ("RATE_LIMIT", "100/minute", "RATE_LIMIT"),
    ("LOG_LEVEL", "DEBUG", "LOG_LEVEL"),
])
def test_settings_load_from_env_vars(env_var, value, attribute):
    """Verify that environment variables take precedence over defaults."""
    os.environ[env_var] = value
    try:
        s = Settings()
        attr_val = getattr(s, attribute)
        # Handle type conversion for PORT
        if attribute == "PORT":
            assert attr_val == int(value)
        elif attribute == "LOG_LEVEL":
            assert attr_val == LogLevel.DEBUG
        elif attribute == "ENVIRONMENT":
            assert attr_val == Environment.PRODUCTION
        else:
            assert attr_val == value.lower() if attribute == "ENVIRONMENT" else value
    finally:
        # Cleanup to avoid leaking to other tests
        del os.environ[env_var]
