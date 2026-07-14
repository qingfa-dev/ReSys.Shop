"""
Rate limiting module for inference.
Uses SlowAPI to provide endpoint-level rate protection.
"""
from embedding.core.config import settings
from slowapi import Limiter
from slowapi.util import get_remote_address

# Initialize: Rate limiter using the remote client's address as the default key.
# Note: In production environments behind load balancers, ensure 'X-Forwarded-For' is trusted.
# We use a memory-based storage by default which is suitable for sidecar deployment.
limiter = Limiter(
    key_func=get_remote_address,
    default_limits=[settings.RATE_LIMIT]
)


def get_limiter() -> Limiter:
    """
    Returns the configured limiter instance.
    """
    return limiter
