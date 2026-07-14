"""
Security and SSL utilities for the inference service.
Handles certificate resolution for secure communication across multiple sources.
"""
import logging
import os
from typing import Optional, Tuple

from embedding.core.config import settings

logger = logging.getLogger(__name__)


def resolve_ssl_paths(
    cert_arg: Optional[str] = None,
    key_arg: Optional[str] = None
) -> Tuple[Optional[str], Optional[str]]:
    """
    Resolves SSL certificate and private key paths.

    Priority:
    1. CLI Arguments (explicit override)
    2. Aspire Environment Variables (managed orchestration)
    3. Configuration Settings (application defaults)
    4. Filesystem Convention (auto-discovery in cert directory)

    Returns:
        (cert_path, key_path) — either value may be None if not found.
    """
    # --- 1. Cert Path Resolution ---
    cert_path: Optional[str] = (
        cert_arg
        or os.getenv("ASPIRE_CERTIFICATE_PATH")
        or settings.SSL_CERT_FILE
        or (
            os.path.join(settings.SSL_CERT_DIR, "cert.pem")
            if settings.SSL_CERT_DIR
            and os.path.exists(
                os.path.join(settings.SSL_CERT_DIR, "cert.pem")
            )
            else None
        )
    )

    # --- 2. Key Path Resolution ---
    key_path: Optional[str] = (
        key_arg
        or os.getenv("ASPIRE_CERTIFICATE_KEY_PATH")
        or os.getenv("DOTNET_CERTIFICATE_KEY_PATH")
    )

    # --- 3. Auto-Discovery Logic ---
    # If cert is found but no explicit key, scan the cert's directory
    if not key_path and cert_path and os.path.exists(cert_path):
        cert_dir = os.path.dirname(cert_path)
        cert_stem = os.path.splitext(os.path.basename(cert_path))[0]

        candidates = [
            "key.pem",
            "cert.key",
            "private.key",
            f"{cert_stem}.key",
            f"{cert_stem}-key.pem",
        ]

        for key_name in candidates:
            potential_key = os.path.join(cert_dir, key_name)
            if os.path.exists(potential_key):
                logger.info("Auto-discovered private key: %s", potential_key)
                key_path = potential_key
                break

    # Also check SSL_CERT_DIR independently if still not found
    if not key_path and settings.SSL_CERT_DIR and os.path.exists(settings.SSL_CERT_DIR):
        for key_name in ("key.pem", "cert.key", "private.key"):
            potential_key = os.path.join(settings.SSL_CERT_DIR, key_name)
            if os.path.exists(potential_key):
                logger.info("Discovered key in SSL_CERT_DIR: %s", potential_key)
                key_path = potential_key
                break

    return cert_path, key_path
