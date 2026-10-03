"""Auto-managed pgvector container for integration tests.

Spins up a ``pgvector/pgvector:pg16`` container via Testcontainers,
mounts ``infra/postgres/init.sql`` into ``/docker-entrypoint-initdb.d/``
so the schema is auto-applied on first start, and yields the connection DSN.
Container is torn down when the test module finishes.
"""
from __future__ import annotations

from pathlib import Path

import pytest
from testcontainers.postgres import PostgresContainer

_INIT_SQL = Path(__file__).resolve().parent.parent.parent.parent / "infra" / "postgres" / "init.sql"


@pytest.fixture(scope="module")
def pg_dsn():
    """Start a pgvector container, apply init.sql, yield DSN, teardown."""
    pg = PostgresContainer(image="pgvector/pgvector:pg16")
    pg.with_volume_mapping(str(_INIT_SQL), "/docker-entrypoint-initdb.d/00-init.sql", "ro")
    pg.start()
    # get_connection_url() returns "postgresql+psycopg2://…" — strip
    # the driver suffix since we use psycopg3 (not psycopg2).
    yield pg.get_connection_url().replace("+psycopg2", "")
    pg.stop()
