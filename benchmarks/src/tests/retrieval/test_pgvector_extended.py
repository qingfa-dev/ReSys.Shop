from unittest.mock import MagicMock

import numpy as np
import pytest

from benchmark.retrieval.pgvector import PgvectorRetriever


def test_upsert_batch():
    retriever = PgvectorRetriever(conn_string="postgresql://test@test/test")
    retriever._conn = MagicMock()
    retriever._conn.cursor.return_value.__enter__ = lambda s: s
    retriever._conn.cursor.return_value.__exit__ = lambda *a: None
    cur = retriever._conn.cursor.return_value

    ids = ["1", "2", "3"]
    labels = ["shirt", "jeans", "shoes"]
    embeddings = np.random.rand(3, 512).astype(np.float32)

    retriever.upsert_batch(ids, labels, embeddings, model_slug="fashion_clip")
    assert cur.executemany.call_count == 1
    sql = cur.executemany.call_args[0][0]
    assert "INSERT INTO" in sql
    assert "ON CONFLICT" in sql


def test_clear_table():
    retriever = PgvectorRetriever(conn_string="postgresql://test@test/test")
    retriever._conn = MagicMock()
    retriever._conn.cursor.return_value.__enter__ = lambda s: s
    retriever._conn.cursor.return_value.__exit__ = lambda *a: None
    cur = retriever._conn.cursor.return_value

    retriever.clear_table()
    assert cur.execute.call_count == 2
    for call_args in cur.execute.call_args_list:
        sql = call_args[0][0]
        assert "DELETE FROM" in sql


def test_build_index():
    retriever = PgvectorRetriever(conn_string="postgresql://test@test/test")
    retriever._conn = MagicMock()
    retriever._conn.cursor.return_value.__enter__ = lambda s: s
    retriever._conn.cursor.return_value.__exit__ = lambda *a: None
    cur = retriever._conn.cursor.return_value

    elapsed = retriever.build_index(512, 100)
    assert cur.execute.call_count == 2
    assert isinstance(elapsed, float)
    assert elapsed >= 0


def test_build_index_invalid_lists():
    retriever = PgvectorRetriever(conn_string="postgresql://test@test/test")
    retriever._conn = MagicMock()

    with pytest.raises(ValueError, match="lists must be a positive integer"):
        retriever.build_index(512, -1)
    with pytest.raises(ValueError, match="lists must be a positive integer"):
        retriever.build_index(512, "100")
