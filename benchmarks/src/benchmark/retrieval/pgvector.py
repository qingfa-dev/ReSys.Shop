"""pgvector retrieval backend.

Production-grade retriever against the ReSys.Shop schema:
- ``embedding_models`` — model registry (slug → id + dim)
- ``product_embeddings_{512,768}`` — per-dimension product vectors
- ``benchmark_runs`` — audit log of evaluation runs
- ``search_products_{512,768}`` — convenience SQL functions

Requires PostgreSQL 16+ with pgvector extension enabled.
"""
from __future__ import annotations

import json
import time
from typing import Any

import numpy as np

from benchmark._constants import FAISS_PARAMS
from benchmark.utils.logging import get_logger

logger = get_logger("retrieval.pgvector")

_TABLE_BY_DIM = {512: "product_embeddings_512", 768: "product_embeddings_768"}
_FN_BY_DIM = {512: "search_products_512", 768: "search_products_768"}


class PgvectorRetriever:
    """pgvector-backed retriever for fashion product embeddings.

    Connects to a PostgreSQL database with the pgvector extension and the
    ReSys.Shop benchmark schema (model registry + per-dimension tables).
    """

    def __init__(self, conn_string: str) -> None:
        self._conn_string = conn_string
        self._conn = None
        self._model_id_cache: dict[str, int] = {}

    # ── connection ──────────────────────────────────────────────────────────

    def connect(self) -> None:
        """Open a database connection and register the vector type."""
        try:
            import psycopg
            from pgvector.psycopg import register_vector
        except ImportError as exc:
            raise ImportError(
                "pgvector backend requires 'psycopg[binary]' and 'pgvector'. "
                "Install with: pip install psycopg[binary] pgvector"
            ) from exc

        self._conn = psycopg.connect(self._conn_string, autocommit=True)
        register_vector(self._conn)
        logger.info("Connected to pgvector at %s", self._conn_string.split("@")[-1])

    def close(self) -> None:
        """Close the database connection."""
        if self._conn is not None:
            self._conn.close()
            self._conn = None

    def __enter__(self) -> PgvectorRetriever:
        self.connect()
        return self

    def __exit__(self, *_: Any) -> None:
        self.close()

    def _require_conn(self) -> None:
        if self._conn is None:
            raise RuntimeError("Not connected — call connect() first")

    # ── health ──────────────────────────────────────────────────────────────

    def ping(self) -> bool:
        """Check connectivity and that the pgvector extension is installed.

        Returns:
            True if the database responds and ``vector`` extension exists.
        """
        self._require_conn()
        with self._conn.cursor() as cur:
            cur.execute("SELECT 1")
            cur.fetchone()
            cur.execute(
                "SELECT 1 FROM pg_extension WHERE extname = 'vector'"
            )
            return cur.fetchone() is not None

    # ── model registry ──────────────────────────────────────────────────────

    def get_model_id(self, slug: str) -> int:
        """Look up the numeric id for a model slug.

        Args:
            slug: Model slug (e.g. ``"fashion_clip"``).

        Returns:
            The model id from ``embedding_models``.

        Raises:
            ValueError: If the slug is not registered.
        """
        if slug in self._model_id_cache:
            return self._model_id_cache[slug]
        self._require_conn()
        with self._conn.cursor() as cur:
            cur.execute(
                "SELECT id FROM embedding_models WHERE slug = %s",
                (slug,),
            )
            row = cur.fetchone()
        if row is None:
            raise ValueError(f"Model '{slug}' is not registered in embedding_models")
        self._model_id_cache[slug] = row[0]
        return row[0]

    def get_embedding_dim(self, slug: str) -> int:
        """Return the embedding dimension for a registered model.

        Args:
            slug: Model slug.

        Returns:
            Embedding dimension (512 or 768).
        """
        self._require_conn()
        with self._conn.cursor() as cur:
            cur.execute(
                "SELECT embedding_dim FROM embedding_models WHERE slug = %s",
                (slug,),
            )
            row = cur.fetchone()
        if row is None:
            raise ValueError(f"Model '{slug}' is not registered in embedding_models")
        return row[0]

    def _table_for_model(self, slug: str) -> str:
        dim = self.get_embedding_dim(slug)
        table = _TABLE_BY_DIM.get(dim)
        if table is None:
            raise ValueError(f"Unsupported embedding dimension {dim} for model '{slug}'")
        return table

    # ── upsert (single) ─────────────────────────────────────────────────────

    def upsert(
        self,
        product_id: str,
        label: str,
        embedding: np.ndarray,
        model_slug: str,
    ) -> None:
        """Insert or update a single product embedding.

        Args:
            product_id: Product identifier.
            label:      Category or attribute label.
            embedding:  L2-normalised float32 vector.
            model_slug: Registered model slug.
        """
        self._require_conn()
        table = self._table_for_model(model_slug)
        model_id = self.get_model_id(model_slug)
        sql = f"""
            INSERT INTO {table} (product_id, model_id, label, embedding)
            VALUES (%s, %s, %s, %s::vector)
            ON CONFLICT (product_id, model_id) DO UPDATE
                SET label = EXCLUDED.label,
                    embedding = EXCLUDED.embedding,
                    indexed_at = now()
        """
        with self._conn.cursor() as cur:
            cur.execute(sql, (product_id, model_id, label, embedding.tolist()))

    # ── upsert (batch) ──────────────────────────────────────────────────────

    def upsert_batch(
        self,
        product_ids: list[str],
        labels: list[str],
        embeddings: np.ndarray,
        model_slug: str,
        batch_size: int = 100,
    ) -> int:
        """Batch insert or update product embeddings.

        Args:
            product_ids: List of product IDs.
            labels:      List of labels (same length as product_ids).
            embeddings:  Float32 array of shape ``(N, D)``.
            model_slug:  Registered model slug.
            batch_size:  Number of rows per psycopg executemany batch.

        Returns:
            Number of rows inserted/updated.

        Raises:
            ValueError: If lengths mismatch or dimension is unsupported.
        """
        self._require_conn()
        if len(product_ids) != len(labels) or len(product_ids) != len(embeddings):
            raise ValueError("product_ids, labels, and embeddings must have the same length")
        if embeddings.ndim != 2:
            raise ValueError("embeddings must be a 2D array of shape (N, D)")
        if not product_ids:
            return 0

        dim = embeddings.shape[1]
        if dim not in _TABLE_BY_DIM:
            raise ValueError(
                f"Unsupported embedding dimension {dim}. "
                f"Supported: {list(_TABLE_BY_DIM.keys())}"
            )

        table = _TABLE_BY_DIM[dim]
        model_id = self.get_model_id(model_slug)
        sql = f"""
            INSERT INTO {table} (product_id, model_id, label, embedding)
            VALUES (%s, %s, %s, %s::vector)
            ON CONFLICT (product_id, model_id) DO UPDATE
                SET label = EXCLUDED.label,
                    embedding = EXCLUDED.embedding,
                    indexed_at = now()
        """
        values = [
            (pid, model_id, label, emb.tolist())
            for pid, label, emb in zip(product_ids, labels, embeddings, strict=True)
        ]
        with self._conn.cursor() as cur:
            cur.executemany(sql, values)
        return len(product_ids)

    # ── search ──────────────────────────────────────────────────────────────

    def search(
        self,
        embedding: np.ndarray,
        model_slug: str,
        top_k: int = 20,
    ) -> list[dict[str, Any]]:
        """Find the top-K nearest products using cosine similarity.

        Args:
            embedding: L2-normalised float32 query vector.
            model_slug: Registered model slug.
            top_k: Number of results.

        Returns:
            List of dicts with ``product_id``, ``label``, ``score`` (cosine similarity).
        """
        self._require_conn()
        dim = self.get_embedding_dim(model_slug)
        model_id = self.get_model_id(model_slug)
        fn = _FN_BY_DIM.get(dim)
        if fn:
            sql = f"SELECT product_id, label, score FROM {fn}(%s::vector, %s, %s)"
            params = (embedding.tolist(), model_id, top_k)
        else:
            table = _TABLE_BY_DIM[dim]
            sql = f"""
                SELECT product_id, label,
                       1 - (embedding <=> %s::vector) AS score
                FROM {table}
                WHERE model_id = %s
                ORDER BY embedding <=> %s::vector
                LIMIT %s
            """
            params = (embedding.tolist(), model_id, embedding.tolist(), top_k)

        with self._conn.cursor() as cur:
            cur.execute(sql, params)
            rows = cur.fetchall()

        return [
            {"product_id": row[0], "label": row[1], "score": float(row[2])}
            for row in rows
        ]

    def search_batch(
        self,
        queries: np.ndarray,
        model_slug: str,
        top_k: int = 20,
    ) -> list[list[dict[str, Any]]]:
        """Run multiple search queries.

        Args:
            queries: Float32 array of shape ``(N, D)``.
            model_slug: Registered model slug.
            top_k: Number of results per query.

        Returns:
            List of result lists, one per query.
        """
        return [self.search(q, model_slug=model_slug, top_k=top_k) for q in queries]

    # ── counting ────────────────────────────────────────────────────────────

    def count_embeddings(self, model_slug: str) -> int:
        """Count stored embeddings for a given model.

        Args:
            model_slug: Registered model slug.

        Returns:
            Row count.
        """
        self._require_conn()
        table = self._table_for_model(model_slug)
        model_id = self.get_model_id(model_slug)
        with self._conn.cursor() as cur:
            cur.execute(f"SELECT COUNT(*) FROM {table} WHERE model_id = %s", (model_id,))
            row = cur.fetchone()
        return row[0] if row else 0

    # ── audit ───────────────────────────────────────────────────────────────

    def record_run(
        self,
        model_slug: str,
        dataset_name: str,
        metrics: dict[str, Any],
        notes: str = "",
    ) -> int:
        """Record a benchmark run in the audit log.

        Args:
            model_slug:   Registered model slug.
            dataset_name: Name of the dataset used.
            metrics:      Dict with keys like ``map``, ``precision``, etc.
            notes:        Optional human-readable note.

        Returns:
            The new ``benchmark_runs.id``.
        """
        self._require_conn()
        with self._conn.cursor() as cur:
            cur.execute(
                """
                INSERT INTO benchmark_runs
                    (model_slug, dataset_name,
                     n_gallery, n_query,
                     map_score, precision_at, recall_at, ndcg_at,
                     latency_ms, throughput_per_sec,
                     notes)
                VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s)
                RETURNING id
                """,
                (
                    model_slug,
                    dataset_name,
                    metrics.get("n_gallery"),
                    metrics.get("n_query"),
                    metrics.get("map"),
                    json.dumps(metrics.get("precision", {})),
                    json.dumps(metrics.get("recall", {})),
                    json.dumps(metrics.get("ndcg", {})),
                    json.dumps(metrics.get("latency_ms", {})),
                    metrics.get("throughput_per_sec"),
                    notes,
                ),
            )
            row = cur.fetchone()
        return row[0] if row else 0

    # ── table / index management ────────────────────────────────────────────

    def clear_table(self, model_slug: str | None = None) -> None:
        """Delete all rows from product embedding tables.

        Args:
            model_slug: If given, only deletes rows for that model.
                        If None, clears all product embedding tables.
        """
        self._require_conn()
        if model_slug:
            tables = [self._table_for_model(model_slug)]
            model_id = self.get_model_id(model_slug)
        else:
            tables = list(_TABLE_BY_DIM.values())
            model_id = None

        for table in tables:
            with self._conn.cursor() as cur:
                if model_id is not None:
                    cur.execute(f"DELETE FROM {table} WHERE model_id = %s", (model_id,))
                else:
                    cur.execute(f"DELETE FROM {table}")
            logger.info("Cleared table %s%s", table,
                         f" for model_id={model_id}" if model_id else "")

    def build_index(self, dim: int, lists: int = FAISS_PARAMS.N_LISTS) -> float:
        """Build an IVFFlat index on the embedding table for the given dim.

        Args:
            dim:   Embedding dimension (512 or 768).
            lists: Number of IVF lists.

        Returns:
            Index build time in seconds.

        Raises:
            ValueError: If dim is not supported or lists is invalid.
        """
        self._require_conn()
        if dim not in _TABLE_BY_DIM:
            raise ValueError(f"Unsupported dimension {dim}. Supported: {list(_TABLE_BY_DIM.keys())}")
        if not isinstance(lists, int) or lists <= 0:
            raise ValueError(f"lists must be a positive integer, got {lists!r}")

        table = _TABLE_BY_DIM[dim]
        index_name = f"idx_{table}_{dim}_{lists}"
        with self._conn.cursor() as cur:
            cur.execute(
                f"""
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM pg_indexes
                        WHERE indexname = '{index_name}'
                    ) THEN
                        DROP INDEX {index_name};
                    END IF;
                END $$;
                """
            )

        t0 = time.perf_counter()
        with self._conn.cursor() as cur:
            cur.execute(
                f"""
                CREATE INDEX {index_name}
                ON {table}
                USING ivfflat (embedding vector_cosine_ops)
                WITH (lists = {lists});
                """
            )
        elapsed = time.perf_counter() - t0
        logger.info("Built IVFFlat index %s in %.2f s", index_name, elapsed)
        return elapsed
