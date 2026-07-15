"""Embedding generation, caching, and durable storage.

Provides the ``EmbeddingGenerator`` (batched inference with optional caching),
``cache`` (transient speed-up layer), and ``storage`` (durable experiment
output persistence).
"""

from benchmark.embeddings import cache, storage
from benchmark.embeddings.generator import EmbeddingGenerator, EmbeddingResult

__all__ = ["EmbeddingGenerator", "EmbeddingResult", "cache", "storage"]
