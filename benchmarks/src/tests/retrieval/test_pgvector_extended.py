import numpy as np
import pytest
from unittest.mock import MagicMock, patch

from benchmark.retrieval.pgvector import PgvectorRetriever


def test_upsert_batch():
    retriever = PgvectorRetriever(conn_string="postgresql://test@test/test")
    retriever._conn = MagicMock()
    retriever._conn.cursor.return_value.__enter__ = lambda s: s
    retriever._conn.cursor.return_value.__exit__ = lambda *a: None
    cur = retriever._conn.cursor.return_value
    cur.mogrify.return_value = b"(%s, %s, %s::vector)"

    ids = ["1", "2", "3"]
    labels = ["shirt", "jeans", "shoes"]
    embeddings = np.random.rand(3, 512).astype(np.float32)

    retriever.upsert_batch(ids, labels, embeddings)
    assert cur.execute.call_count == 1
    sql = cur.execute.call_args[0][0]
    assert "INSERT INTO" in sql
    assert "ON CONFLICT" in sql
