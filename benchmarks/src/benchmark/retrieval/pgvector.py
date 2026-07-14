"""pgvector retrieval backend.

Runs cosine nearest-neighbour queries against a PostgreSQL table that has a
``pgvector`` column. Used for end-to-end integration tests against the actual
ReSys.Shop database, not for the offline benchmark (use cosine.py for that).

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

import numpy as np

from benchmark.utils.logging import get_logger

logger = get_logger("retrieval.pgvector")


class PgvectorRetriever:
    """Cosine nearest-neighbour retrieval via pgvector."""

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

    def close(self) -> None:
        """Close the database connection."""
        if self._conn is not None:
            self._conn.close()
            self._conn = None

    def __enter__(self) -> "PgvectorRetriever":
        self.connect()
        return self

    def __exit__(self, *_) -> None:
        self.close()
