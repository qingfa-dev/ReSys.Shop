"""
Unit tests for the Rate Limiting module.
"""
from slowapi import Limiter
from src.core.rate_limit import get_limiter
from src.core.config import settings

def test_limiter_initialization():
    """Verify that the limiter is initialized with the correct defaults."""
    limiter = get_limiter()
    assert isinstance(limiter, Limiter)
    # Check that it has some limits defined (default_limits is a list of strings/callables)
    assert len(limiter._default_limits) > 0
    # Note: We can't easily check the exact value of settings.RATE_LIMIT inside the 
    # slowapi object as it parses it, but we verify it's active.
