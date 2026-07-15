"""pgvector retrieval backend.

Runs cosine nearest-neighbour queries against a PostgreSQL table that has a
``pgvector`` column. Used for end-to-end integration tests against the actual
ReSys.Shop database, not for the offline benchmark (use cosine.py for that).

Edge cases:
- Requires PostgreSQL 16+ with pgvector extension enabled at the database level.
- Raises ImportError with install instructions when psycopg or pgvector is
  not installed.
- All public methods raise RuntimeError if connect() has not been called.
- batch upsert validates that input arrays have matching lengths and that
  embeddings is a 2D array; empty product_ids is a no-op.
- build_index drops an existing index before recreating; passes back elapsed
  time in seconds.

Prerequisites
-------------
- PostgreSQL 16 with pgvector extension enabled
- ``pip install psycopg[binary] pgvector``
- A table with schema::

    CREATE TABLE products (
        id          TEXT PRIMARY KEY,
        embedding   VECTOR(512)          -- or 768 for L/14 / SigLIP
    );
    CREATE INDEX ON products USING ivfflat (embedding vector_cosine_ops)
        WITH (lists = 100);

Usage
-----
    retriever = PgvectorRetriever(conn_string="postgresql://...")
    retriever.connect()
    results = retriever.query(embedding, top_k=20)
    retriever.close()
"""
from __future__ import annotations

import time

import numpy as np

from benchmark._constants import FAISS_PARAMS
from benchmark.utils.logging import get_logger

logger = get_logger("retrieval.pgvector")


class PgvectorRetriever:
    """Cosine nearest-neighbour retrieval via pgvector.

    Supports single and batch upsert, IVFFlat index building, and context
    manager protocol. All public database operations raise RuntimeError if
    connect() has not been called.
    """

    def __init__(
        self,
        conn_string: str,
        table: str = "products",
        embedding_col: str = "embedding",
        id_col: str = "id",
        label_col: str = "label",
    ) -> None:
        self._conn_string = conn_string
        self._table = table
        self._embedding_col = embedding_col
        self._id_col = id_col
        self._label_col = label_col
        self._conn = None

    def connect(self) -> None:
        """Open a database connection."""
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

    def ping(self) -> None:
        """Check connectivity by executing a lightweight query.

        Raises:
            RuntimeError: If ``connect()`` has not been called.
        """
        if self._conn is None:
            raise RuntimeError("Not connected — call connect() first")
        with self._conn.cursor() as cur:
            cur.execute("SELECT 1")
            cur.fetchone()

    def query(self, embedding: np.ndarray, top_k: int = 20) -> list[dict]:
        """Find the top-K nearest products.

        Args:
            embedding: L2-normalised float32 vector.
            top_k: Number of results.

        Returns:
            List of dicts with ``id``, ``label``, ``score`` (cosine similarity).
        """
        if self._conn is None:
            raise RuntimeError("Call connect() before querying")

        sql = f"""
            SELECT
                {self._id_col},
                {self._label_col},
                1 - ({self._embedding_col} <=> %s::vector) AS score
            FROM {self._table}
            ORDER BY {self._embedding_col} <=> %s::vector
            LIMIT %s
        """
        with self._conn.cursor() as cur:
            cur.execute(sql, (embedding.tolist(), embedding.tolist(), top_k))
            rows = cur.fetchall()

        return [
            {"id": row[0], "label": row[1], "score": float(row[2])}
            for row in rows
        ]

    def upsert_embedding(self, product_id: str, label: str, embedding: np.ndarray) -> None:
        """Insert or update a single product embedding.

        Useful for integration tests and admin scripts.
        """
        if self._conn is None:
            raise RuntimeError("Call connect() first")

        sql = f"""
            INSERT INTO {self._table} ({self._id_col}, {self._label_col}, {self._embedding_col})
            VALUES (%s, %s, %s::vector)
            ON CONFLICT ({self._id_col}) DO UPDATE
                SET {self._embedding_col} = EXCLUDED.{self._embedding_col},
                    {self._label_col}     = EXCLUDED.{self._label_col}
        """
        with self._conn.cursor() as cur:
            cur.execute(sql, (product_id, label, embedding.tolist()))

    def upsert_batch(
        self,
        product_ids: list[str],
        labels: list[str],
        embeddings: np.ndarray,
    ) -> None:
        """Batch insert or update product embeddings.

        Args:
            product_ids: List of product IDs.
            labels:      List of labels (same length).
            embeddings:  Float32 array of shape ``(N, D)``.
        """
        if self._conn is None:
            raise RuntimeError("Call connect() first")
        if len(product_ids) != len(labels) or len(product_ids) != len(embeddings):
            raise ValueError("product_ids, labels, and embeddings must have the same length")
        if embeddings.ndim != 2:
            raise ValueError("embeddings must be a 2D array of shape (N, D)")
        if not product_ids:
            return

        sql = f"""
            INSERT INTO {self._table} ({self._id_col}, {self._label_col}, {self._embedding_col})
            VALUES (%s, %s, %s::vector)
            ON CONFLICT ({self._id_col}) DO UPDATE
                SET {self._embedding_col} = EXCLUDED.{self._embedding_col},
                    {self._label_col}     = EXCLUDED.{self._label_col}
        """
        values = [
            (pid, label, emb.tolist())
            for pid, label, emb in zip(product_ids, labels, embeddings, strict=True)
        ]
        with self._conn.cursor() as cur:
            cur.executemany(sql, values)

    def clear_table(self) -> None:
        """Delete all rows from the target table."""
        if self._conn is None:
            raise RuntimeError("Call connect() first")
        with self._conn.cursor() as cur:
            cur.execute(f"DELETE FROM {self._table}")
        logger.info("Cleared table %s", self._table)

    def build_index(self, dim: int, lists: int = FAISS_PARAMS.N_LISTS) -> float:
        """Build an IVFFlat index and return elapsed time in seconds.

        Args:
            dim:    Embedding dimension (512 or 768).
            lists:  Number of IVF lists.

        Returns:
            Index build time in seconds.
        """
        if self._conn is None:
            raise RuntimeError("Call connect() first")
        if not isinstance(lists, int) or lists <= 0:
            raise ValueError(f"lists must be a positive integer, got {lists!r}")

        index_name = f"idx_{self._table}_{dim}_{lists}"
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
                ON {self._table}
                USING ivfflat ({self._embedding_col} vector_cosine_ops)
                WITH (lists = {lists});
                """
            )
        elapsed = time.perf_counter() - t0
        logger.info("Built IVFFlat index %s in %.2f s", index_name, elapsed)
        return elapsed

    def close(self) -> None:
        """Close the database connection."""
        if self._conn is not None:
            self._conn.close()
            self._conn = None

    def __enter__(self) -> PgvectorRetriever:
        self.connect()
        return self

    def __exit__(self, *_) -> None:
        self.close()
