"""
Rate limiting module for inference.
Uses SlowAPI to provide endpoint-level rate protection.
"""
from embedding.core.config import settings
from slowapi import Limiter
from slowapi.util import get_remote_address

# Initialize: Rate limiter using the remote client's address as the default key.
# Assume: In production behind load balancers, ensure X-Forwarded-For is trusted.
#          Memory-based storage is suitable for single-process sidecar deployment.
limiter = Limiter(
    key_func=get_remote_address,
    default_limits=[settings.RATE_LIMIT]
)


def get_limiter() -> Limiter:
    """Returns the configured limiter singleton instance."""
    return limiter
