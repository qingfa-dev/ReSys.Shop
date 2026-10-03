"""Integration tests — require a running PostgreSQL + pgvector instance.

Run with Docker Compose:

    docker compose up postgres --wait
    uv run pytest src/tests/integration/ -m integration -v

Or against any reachable PostgreSQL with pgvector:

    BENCHMARK_PG_DSN="postgresql://benchmark:benchmark@localhost:5432/benchmark" \\
        uv run pytest src/tests/integration/ -m integration -v

These tests are skipped automatically when BENCHMARK_PG_DSN is not set or
when psycopg / pgvector packages are not installed.
"""
