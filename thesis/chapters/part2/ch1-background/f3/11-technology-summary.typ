=== Technology Stack Summary

The preceding sections introduced the principal technologies that compose the ReSys.Shop platform. Table @tbl-tech-stack consolidates the complete stack.

#figure(
  table(
    columns: (auto, 1fr, 2fr),
    align: (start, start, start),
    table.header([*Layer*], [*Technology*], [*Role*]),
    [Frontend], [Vue 3, TypeScript, Vite], [Customer storefront and administration interface; reactive UI with Pinia state management],
    [Backend API], [.NET 10, Carter, MediatR], [REST endpoints via minimal APIs; CQRS command-query separation across business modules],
    [Database], [PostgreSQL, pgvector], [Relational data and vector embeddings in a single ACID database with HNSW-indexed similarity search],
    [Caching], [Redis, HybridCache], [Two-tier cache (in-memory L1 and Redis L2); Hangfire job queue and session state backing store],
    [ML Sidecar], [Python 3.12, FastAPI, PyTorch], [Dedicated embedding generation service with lazy model loading and GPU acceleration],
    [Orchestration], [.NET Aspire], [Container lifecycle management, service discovery, and reproducible local development environment],
    [Background Jobs], [Hangfire], [Persistent job processing for cart expiry, embedding queue, and maintenance tasks],
    [Auth and Identity], [JWT, ASP.NET Identity], [Access tokens, refresh rotation, permission-based authorisation, guest sessions],
    [Benchmarking], [Python 3.12, PyTorch], [Systematic 11-model comparison with cross-validation, accuracy and efficiency metrics],
  ),
    kind: table,
  caption: [Technology stack of the ReSys.Shop platform],
) <tbl-tech-stack>

Together these technologies form a polyglot stack spanning three languages (C\#, TypeScript, Python) orchestrated through a unified containerised environment. The .NET backend hosts transactional e-commerce logic; Vue 3 delivers the customer-facing interface; PostgreSQL serves as the single source of truth for both business data and embeddings; and the Python sidecar provides the bridge to GPU-accelerated model inference. A dedicated benchmarking framework, separate from the application codebase, provides the systematic evaluation infrastructure that produces the experimental results in Chapter 3.
