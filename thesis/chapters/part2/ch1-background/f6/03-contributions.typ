=== Contribution Differentiators

This project differs from prior work by addressing the *engineering gap* between model research and production systems. Four contributions define this gap:

*1. Polyglot architecture.* Python's machine learning ecosystem (PyTorch, HuggingFace) does not natively interoperate with the .NET stack common in enterprise e-commerce. This thesis presents a modular monolith with a dedicated AI sidecar. It combines .NET's type safety and transactional integrity with Python's access to state-of-the-art vision models, without the operational overhead of a full microservices deployment.

*2. Vector-native consistency.* By using pgvector within PostgreSQL, embeddings and product metadata share the same transactional boundary. Product updates, image replacements, and index maintenance occur atomically, eliminating stale-index bugs that arise when a vector store and relational database have independent consistency guarantees.

*3. Commodity hardware benchmarking.* Commercial visual search runs on cloud TPU clusters. This thesis benchmarks six models on consumer-grade hardware, establishing that production-quality visual search is achievable without specialised infrastructure, lowering the barrier for small to medium e-commerce platforms.

*4. Applied model comparison.* Instead of chasing the highest possible benchmark scores, this thesis compares models under realistic deployment constraints (inference latency budget, memory limits, storage cost). The resulting accuracy-efficiency trade-off data, presented in Chapter 3, provides a pragmatic guide for practitioners selecting embedding models.
