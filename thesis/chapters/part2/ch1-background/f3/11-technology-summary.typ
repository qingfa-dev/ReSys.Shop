=== Technology Stack Summary

The preceding sections introduced the principal technologies that compose the ReSys.Shop platform. @tbl-tech-stack consolidates the complete stack.

#figure(
  table(
    columns: (auto, 1fr, 2fr),
    align: (start, start, start),
    table.header([*Layer*], [*Technology*], [*Role*]),
    [Frontend], [Vue 3, TypeScript, Vite], [Customer storefront and administration interface],
    [Backend API], [.NET 10, Carter, MediatR], [REST endpoints via minimal APIs with CQRS across business modules],
    [Database], [PostgreSQL, pgvector], [Relational data and vector embeddings with HNSW-indexed similarity search],
    [Caching], [Redis, HybridCache], [Two-tier cache (L1 in-memory, L2 Redis); Hangfire job queue backing store],
    [ML Sidecar], [Python 3.12, FastAPI, PyTorch], [Embedding generation with lazy model loading and GPU acceleration],
    [Background Jobs], [Hangfire], [Persistent job processing for cart expiry, embedding queue, and maintenance],
    [Auth and Identity], [JWT, ASP.NET Identity], [Access tokens, refresh rotation, permission-based authorisation],
    [Benchmarking], [Python 3.12, PyTorch], [Systematic 11-model comparison across retrieval accuracy and efficiency],
  ),
    kind: table,
  caption: [Technology stack of the ReSys.Shop platform],
) <tbl-tech-stack>
