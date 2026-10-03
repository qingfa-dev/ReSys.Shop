"""Retrieval backends: cosine (exact), FAISS (approximate), pgvector (DB).

The default (and most used) backend is cosine — exact nearest-neighbour over
in-memory numpy matrices. FAISS provides sub-linear approximate search for
large galleries. pgvector enables integration tests against a real database.

Exports only the cosine functions; FAISS and pgvector are imported directly
for specialised use cases.
"""

from benchmark.retrieval.cosine import retrieve_batch, top_k_indices

__all__ = ["retrieve_batch", "top_k_indices"]
