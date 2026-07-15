"""Integration tests for the pgvector retrieval backend.

Requirements
------------
- A PostgreSQL instance with pgvector and the schema from init.sql applied.
- ``BENCHMARK_PG_DSN`` environment variable set.
- ``psycopg[binary]`` and ``pgvector`` Python packages installed.

All tests are skipped automatically when these prerequisites are absent.
"""
from __future__ import annotations

import os

import numpy as np
import pytest

# ── skip guard ────────────────────────────────────────────────────────────────

def _pg_dsn() -> str | None:
    return os.environ.get("BENCHMARK_PG_DSN")


def _psycopg_available() -> bool:
    try:
        import pgvector  # noqa: F401
        import psycopg  # noqa: F401
        return True
    except ImportError:
        return False


pytestmark = pytest.mark.integration


# ── fixtures ──────────────────────────────────────────────────────────────────

@pytest.fixture(scope="module")
def retriever():
    """Return a connected PgvectorRetriever, or skip if prereqs are missing."""
    dsn = _pg_dsn()
    if not dsn:
        pytest.skip("BENCHMARK_PG_DSN not set — skipping pgvector integration tests")
    if not _psycopg_available():
        pytest.skip("psycopg or pgvector not installed")

    from benchmark.retrieval.pgvector import PgvectorRetriever
    r = PgvectorRetriever(conn_string=dsn)
    r.connect()
    yield r
    r.close()


@pytest.fixture()
def random_512() -> np.ndarray:
    """Return a random L2-normalised 512-D float32 vector."""
    rng = np.random.default_rng(42)
    v = rng.standard_normal(512).astype(np.float32)
    return v / np.linalg.norm(v)


@pytest.fixture()
def random_768() -> np.ndarray:
    rng = np.random.default_rng(99)
    v = rng.standard_normal(768).astype(np.float32)
    return v / np.linalg.norm(v)


# ── connection and health ─────────────────────────────────────────────────────

def test_ping(retriever) -> None:
    """pgvector extension must be enabled."""
    assert retriever.ping() is True


def test_ping_verifies_vector_extension(retriever) -> None:
    """ping() specifically checks for the 'vector' extension."""
    result = retriever.ping()
    assert isinstance(result, bool)
    assert result  # would be False if extension were missing


# ── model registry ────────────────────────────────────────────────────────────

def test_get_model_id_fashion_clip(retriever) -> None:
    model_id = retriever.get_model_id("fashion-clip")
    assert isinstance(model_id, int)
    assert model_id > 0


def test_get_model_id_all_registered_models(retriever) -> None:
    slugs = ["fashion-clip", "clip-b32", "clip-l14", "siglip", "eva-clip"]
    ids = [retriever.get_model_id(s) for s in slugs]
    assert len(set(ids)) == 5, "Model IDs must be unique"


def test_get_model_id_unknown_slug_raises(retriever) -> None:
    with pytest.raises(ValueError, match="not registered"):
        retriever.get_model_id("unknown-model-xyz")


def test_get_embedding_dim_512_models(retriever) -> None:
    for slug in ("fashion-clip", "clip-b32", "eva-clip"):
        assert retriever.get_embedding_dim(slug) == 512


def test_get_embedding_dim_768_models(retriever) -> None:
    for slug in ("clip-l14", "siglip"):
        assert retriever.get_embedding_dim(slug) == 768


def test_model_id_is_cached(retriever) -> None:
    """Second call must use the in-memory cache (no extra DB round-trip)."""
    retriever._model_id_cache.clear()
    id1 = retriever.get_model_id("fashion-clip")
    id2 = retriever.get_model_id("fashion-clip")
    assert id1 == id2


# ── upsert (single) ───────────────────────────────────────────────────────────

def test_upsert_512(retriever, random_512) -> None:
    retriever.upsert(
        product_id="test_upsert_512",
        label="Tshirts",
        embedding=random_512,
        model_slug="fashion-clip",
    )
    count = retriever.count_embeddings("fashion-clip")
    assert count >= 1


def test_upsert_is_idempotent(retriever, random_512) -> None:
    for _ in range(3):
        retriever.upsert(
            product_id="test_idempotent",
            label="Jeans",
            embedding=random_512,
            model_slug="fashion-clip",
        )
    # Should not raise or create duplicate rows
    count_before = retriever.count_embeddings("fashion-clip")
    retriever.upsert(
        product_id="test_idempotent",
        label="Jeans",
        embedding=random_512,
        model_slug="fashion-clip",
    )
    count_after = retriever.count_embeddings("fashion-clip")
    assert count_after == count_before  # ON CONFLICT DO UPDATE → no new row


def test_upsert_768(retriever, random_768) -> None:
    retriever.upsert(
        product_id="test_upsert_768",
        label="Watches",
        embedding=random_768,
        model_slug="clip-l14",
    )
    count = retriever.count_embeddings("clip-l14")
    assert count >= 1


# ── upsert (batch) ────────────────────────────────────────────────────────────

def test_upsert_batch_512(retriever) -> None:
    rng = np.random.default_rng(123)
    n = 20
    embeddings = rng.standard_normal((n, 512)).astype(np.float32)
    norms = np.linalg.norm(embeddings, axis=1, keepdims=True)
    embeddings = embeddings / norms

    product_ids = [f"batch_test_{i:04d}" for i in range(n)]
    labels = ["Tshirts"] * 10 + ["Jeans"] * 10

    count = retriever.upsert_batch(
        product_ids=product_ids,
        labels=labels,
        embeddings=embeddings,
        model_slug="fashion-clip",
        batch_size=8,
    )
    assert count == n


def test_upsert_batch_length_mismatch_raises(retriever) -> None:
    rng = np.random.default_rng(0)
    embeddings = rng.standard_normal((5, 512)).astype(np.float32)
    with pytest.raises(ValueError, match="same length"):
        retriever.upsert_batch(
            product_ids=["a", "b"],        # length 2
            labels=["x"] * 5,             # length 5
            embeddings=embeddings,         # length 5
            model_slug="fashion-clip",
        )


def test_upsert_batch_unsupported_dim_raises(retriever) -> None:
    rng = np.random.default_rng(0)
    embeddings = rng.standard_normal((2, 256)).astype(np.float32)
    with pytest.raises(ValueError, match="Unsupported embedding dimension"):
        retriever.upsert_batch(
            product_ids=["a", "b"],
            labels=["x", "y"],
            embeddings=embeddings,
            model_slug="fashion-clip",
        )


# ── search ────────────────────────────────────────────────────────────────────

def test_search_returns_results(retriever, random_512) -> None:
    # Ensure at least one row exists
    retriever.upsert("search_test_001", "Tshirts", random_512, "fashion-clip")
    results = retriever.search(random_512, model_slug="fashion-clip", top_k=5)
    assert isinstance(results, list)
    assert len(results) >= 1


def test_search_result_structure(retriever, random_512) -> None:
    retriever.upsert("search_test_002", "Jeans", random_512, "fashion-clip")
    results = retriever.search(random_512, model_slug="fashion-clip", top_k=3)
    for r in results:
        assert "product_id" in r
        assert "label" in r
        assert "score" in r
        assert isinstance(r["score"], float)
        assert -1.0 <= r["score"] <= 1.0


def test_search_self_is_top_result(retriever) -> None:
    """Searching for an embedded vector should return itself as the top hit."""
    rng = np.random.default_rng(77)
    v = rng.standard_normal(512).astype(np.float32)
    v = v / np.linalg.norm(v)

    retriever.upsert("search_self_test", "Shirts", v, "fashion-clip")
    results = retriever.search(v, model_slug="fashion-clip", top_k=1)
    assert len(results) >= 1
    assert results[0]["product_id"] == "search_self_test"
    assert results[0]["score"] > 0.99  # near-perfect cosine similarity


def test_search_top_k_respected(retriever, random_512) -> None:
    results = retriever.search(random_512, model_slug="fashion-clip", top_k=3)
    assert len(results) <= 3


def test_search_results_are_sorted_descending(retriever, random_512) -> None:
    results = retriever.search(random_512, model_slug="fashion-clip", top_k=10)
    scores = [r["score"] for r in results]
    assert scores == sorted(scores, reverse=True), "Results must be sorted by score DESC"


def test_search_batch(retriever, random_512) -> None:
    rng = np.random.default_rng(55)
    queries = rng.standard_normal((3, 512)).astype(np.float32)
    queries = queries / np.linalg.norm(queries, axis=1, keepdims=True)

    all_results = retriever.search_batch(queries, model_slug="fashion-clip", top_k=5)
    assert len(all_results) == 3
    for res in all_results:
        assert isinstance(res, list)


# ── 768-D search ──────────────────────────────────────────────────────────────

def test_search_768(retriever, random_768) -> None:
    retriever.upsert("search_768_test", "Watches", random_768, "clip-l14")
    results = retriever.search(random_768, model_slug="clip-l14", top_k=5)
    assert len(results) >= 1
    assert results[0]["score"] > 0.99


# ── audit log ─────────────────────────────────────────────────────────────────

def test_record_run(retriever) -> None:
    metrics = {
        "map": 0.7812,
        "precision": {"@10": 0.75},
        "recall": {"@10": 0.60},
        "ndcg": {"@10": 0.80},
        "latency_ms": {"p50_ms": 12.3, "p95_ms": 18.5},
        "throughput_per_sec": 82.0,
        "n_gallery": 35000,
        "n_query": 8700,
    }
    run_id = retriever.record_run(
        model_slug="fashion-clip",
        dataset_name="fashion-product-images-small",
        metrics=metrics,
        notes="integration test run",
    )
    assert isinstance(run_id, int)
    assert run_id > 0


# ── require_conn guard ────────────────────────────────────────────────────────

def test_operations_require_connection() -> None:
    """Methods must raise RuntimeError if connect() was never called."""
    if not _psycopg_available():
        pytest.skip("psycopg not installed")

    from benchmark.retrieval.pgvector import PgvectorRetriever
    r = PgvectorRetriever("postgresql://dummy/dummy")
    with pytest.raises(RuntimeError, match="Not connected"):
        r.ping()


# ── pgvector init.sql schema verification ─────────────────────────────────────

def test_schema_has_required_tables(retriever) -> None:
    """Verify that init.sql created all expected tables."""
    expected_tables = {
        "embedding_models",
        "product_embeddings_512",
        "product_embeddings_768",
        "benchmark_runs",
    }
    with retriever._conn.cursor() as cur:
        cur.execute("""
            SELECT tablename
            FROM   pg_tables
            WHERE  schemaname = 'public'
        """)
        existing = {row[0] for row in cur.fetchall()}
    missing = expected_tables - existing
    assert not missing, f"Missing tables: {missing}"


def test_schema_has_required_views(retriever) -> None:
    expected_views = {"latest_benchmark_runs", "model_ranking"}
    with retriever._conn.cursor() as cur:
        cur.execute("""
            SELECT viewname
            FROM   pg_views
            WHERE  schemaname = 'public'
        """)
        existing = {row[0] for row in cur.fetchall()}
    missing = expected_views - existing
    assert not missing, f"Missing views: {missing}"


def test_schema_has_search_functions(retriever) -> None:
    expected_fns = {"search_products_512", "search_products_768"}
    with retriever._conn.cursor() as cur:
        cur.execute("""
            SELECT routine_name
            FROM   information_schema.routines
            WHERE  routine_type = 'FUNCTION'
            AND    routine_schema = 'public'
        """)
        existing = {row[0] for row in cur.fetchall()}
    missing = expected_fns - existing
    assert not missing, f"Missing functions: {missing}"


def test_embedding_models_seeded(retriever) -> None:
    """init.sql must pre-insert all 5 model slugs."""
    with retriever._conn.cursor() as cur:
        cur.execute("SELECT slug FROM embedding_models ORDER BY slug")
        slugs = {row[0] for row in cur.fetchall()}
    expected = {"fashion-clip", "clip-b32", "clip-l14", "siglip", "eva-clip"}
    assert expected.issubset(slugs), f"Missing model slugs: {expected - slugs}"
