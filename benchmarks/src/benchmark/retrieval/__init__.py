"""Retrieval backends: cosine (exact), FAISS (approximate), pgvector (DB)."""

from benchmark.retrieval.cosine import retrieve_batch, top_k_indices

__all__ = ["retrieve_batch", "top_k_indices"]
