"""
Unit tests for the Security and SSL resolution module.
"""
import os
import pytest
from unittest.mock import patch, MagicMock
from src.core.security import resolve_ssl_paths


def test_resolve_ssl_paths_from_args():
    """Verify that arguments take highest priority."""
    cert, key = resolve_ssl_paths(cert_arg="arg.crt", key_arg="arg.key")
    assert cert == "arg.crt"
    assert key == "arg.key"


def test_resolve_ssl_paths_from_aspire_env():
    """Verify that Aspire environment variables are picked up."""
    with patch.dict(os.environ, {
        "ASPIRE_CERTIFICATE_PATH": "env.crt",
        "ASPIRE_CERTIFICATE_KEY_PATH": "env.key"
    }):
        cert, key = resolve_ssl_paths()
        assert cert == "env.crt"
        assert key == "env.key"


def test_resolve_ssl_auto_discovery():
    """Verify that the key is auto-discovered if not explicitly provided."""
    # We must patch multiple os.path helpers to simulate discovery
    with patch("os.path.exists") as mock_exists:
        with patch("os.path.dirname", return_value="/certs"):
            with patch("os.path.basename", return_value="cert.pem"):
                with patch("os.path.splitext", return_value=("cert", ".pem")):
                    # Simulate cert exists, and key.pem exists in that same dir
                    # The code checks key.pem, cert.key, private.key, etc.
                    def side_effect(path):
                        return path in ["/certs/cert.pem", "/certs/key.pem"]
                    
                    mock_exists.side_effect = side_effect
                    
                    # We also need to patch join since it's used to construct the candidate paths
                    with patch("os.path.join", side_effect=lambda a, b: f"{a}/{b}"):
                        with patch("src.core.config.settings.SSL_CERT_FILE", "/certs/cert.pem"):
                            cert, key = resolve_ssl_paths()
                            assert cert == "/certs/cert.pem"
                            assert key == "/certs/key.pem"


def test_resolve_ssl_none_if_missing():
    """Verify that None is returned if no sources are present."""
    with patch.dict(os.environ, {}, clear=True):
        with patch("src.core.config.settings.SSL_CERT_FILE", None):
            with patch("src.core.config.settings.SSL_CERT_DIR", None):
                cert, key = resolve_ssl_paths()
                assert cert is None
                assert key is None
