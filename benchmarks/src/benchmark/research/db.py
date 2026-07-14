"""Database-backed research helpers for PGVector benchmark and validation."""
from __future__ import annotations

import time
from pathlib import Path
from typing import Any

import numpy as np

from benchmark.retrieval.pgvector import PgvectorRetriever
from benchmark.utils.logging import get_logger

logger = get_logger("research.db")

MODEL_COLUMN_MAP = {
    "efficientnet-b0": "efficientnet_vector",
    "dinov2-vits14": "dinov2_vector",
    "clip-b32": "clip_vector",
    "clip-l14": "clip_l14_vector",
    "clip-vit-b16": "clip_vit_b16_vector",
    "fashion-clip": "fashion_clip_vector",
    "siglip": "siglip_vector",
    "eva-clip": "eva_clip_vector",
}


class PgvectorBenchmark:
    """Run PGVector performance benchmarks and HNSW validation."""

    def __init__(
        self,
        conn_string: str,
        table: str = "gallery_items",
        id_col: str = "id",
        label_col: str = "label",
    ) -> None:
        self.conn_string = conn_string
        self.table = table
        self.id_col = id_col
        self.label_col = label_col
        self._retriever: PgvectorRetriever | None = None

    def __enter__(self) -> "PgvectorBenchmark":
        self.connect()
        return self

    def __exit__(self, *_) -> None:
        self.close()

    def connect(self) -> None:
        self._retriever = PgvectorRetriever(conn_string=self.conn_string)
        self._retriever.connect()

    def close(self) -> None:
        if self._retriever is not None:
            self._retriever.close()
            self._retriever = None

    def benchmark_model(
        self,
        model_key: str,
        num_queries: int = 100,
        top_k: int = 10,
    ) -> dict[str, Any]:
        if self._retriever is None:
            raise RuntimeError("Call connect() before benchmark_model()")

        column = MODEL_COLUMN_MAP.get(model_key)
        if column is None:
            raise ValueError(f"Unknown model key: {model_key}")

        query_sql = f"SELECT {column} FROM {self.table} WHERE {column} IS NOT NULL ORDER BY RANDOM() LIMIT %s"
        with self._retriever._conn.cursor() as cur:  # type: ignore[attr-defined]
            cur.execute(query_sql, (num_queries,))
            rows = cur.fetchall()

        query_vectors = [np.array(row[0], dtype=np.float32) for row in rows]
        if not query_vectors:
            raise RuntimeError(f"No vectors found for {model_key} in {self.table}")

        latencies: list[float] = []
        scores: list[float] = []
        for vec in query_vectors:
            start = time.perf_counter()
            results = self._retriever.query(vec, top_k=top_k)
            elapsed = (time.perf_counter() - start) * 1000
            latencies.append(elapsed)
            if results:
                scores.append(results[0]["score"])

        return {
            "Model": model_key,
            "Queries": len(query_vectors),
            "TopK": top_k,
            "AvgLatency_ms": float(np.mean(latencies)) if latencies else 0.0,
            "P95Latency_ms": float(np.percentile(latencies, 95)) if latencies else 0.0,
            "P99Latency_ms": float(np.percentile(latencies, 99)) if latencies else 0.0,
            "QPS": float(len(latencies) / (sum(latencies) / 1000)) if latencies else 0.0,
            "AvgTopScore": float(np.mean(scores)) if scores else 0.0,
        }

    def validate_hnsw(
        self,
        model_key: str,
        num_queries: int = 50,
        top_k: int = 10,
    ) -> dict[str, Any]:
        if self._retriever is None:
            raise RuntimeError("Call connect() before validate_hnsw()")

        column = MODEL_COLUMN_MAP.get(model_key)
        if column is None:
            raise ValueError(f"Unknown model key: {model_key}")

        conn = self._retriever._conn  # type: ignore[attr-defined]
        recalls: list[float] = []
        with conn.cursor() as cur:
            cur.execute(
                f"SELECT {column} FROM {self.table} WHERE {column} IS NOT NULL ORDER BY RANDOM() LIMIT %s",
                (num_queries,),
            )
            query_rows = cur.fetchall()

            for row in query_rows:
                query_vec = np.array(row[0], dtype=np.float32)
                query_list = query_vec.tolist()

                cur.execute("SET LOCAL enable_seqscan = OFF; SET LOCAL enable_indexscan = ON;")
                cur.execute(
                    f"SELECT {self.id_col} FROM {self.table} ORDER BY {column} <-> %s LIMIT %s",
                    (query_list, top_k),
                )
                approx = [r[0] for r in cur.fetchall()]

                cur.execute("SET LOCAL enable_seqscan = ON; SET LOCAL enable_indexscan = OFF;")
                cur.execute(
                    f"SELECT {self.id_col} FROM {self.table} ORDER BY {column} <-> %s LIMIT %s",
                    (query_list, top_k),
                )
                exact = [r[0] for r in cur.fetchall()]

                if exact:
                    recalls.append(len(set(approx) & set(exact)) / len(exact))
                else:
                    recalls.append(1.0)

        return {
            "Model": model_key,
            "HNSW_Recall_at_10": float(np.mean(recalls)) if recalls else 0.0,
            "Queries": len(recalls),
        }
