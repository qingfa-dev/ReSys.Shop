=== pgvector: PostgreSQL with Vector Search

pgvector is an open-source PostgreSQL extension that implements vector storage and similarity search within the standard relational database. This project uses pgvector 0.8+.

- *Transactional consistency.* Vectors and product metadata live in the same database. A product update and its embedding update occur within a single ACID transaction, eliminating the dual-database problem where a separate vector store drifts out of sync with the relational source of truth.
- *Combined SQL queries.* Vector similarity and relational filtering combine in a single query plan: find products visually similar to a query image, restricted to a specific category and price range, using a single SQL statement.
- *Index support.* pgvector 0.8+ supports both IVFFlat (Inverted File with Flat Compression) and HNSW (Hierarchical Navigable Small World). This project uses each index type for a distinct purpose.

*Index assignment in this project.* The benchmark evaluation (Chapter 3) uses IVFFlat with 100 lists at the 5,000-vector catalogue scale. The production architecture designates HNSW as the long-term index target. The project migration strategy progresses through three phases:

1. *Exact search* (current state). At the current catalogue scale, the pgvector `<=>` operator without an index provides adequate query latency.
2. *IVFFlat* (intermediate scale). As the catalogue grows beyond 10,000 items, IVFFlat with tuned `lists` and `probes` provides approximate search with minimal build overhead.
3. *HNSW* (production scale). At millions of catalogue items, HNSW's superior recall-speed trade-off and sustained sub-100 ms latency become decisive.

- *Variable-length vectors.* pgvector accommodates the different embedding dimensions produced by different models: 384 (DINOv2-S), 512 (Fashion-CLIP), 768 (DINOv2-B), 1280 (EfficientNet-B0), and 2048 (ResNet-50).
- *Distance metric.* Cosine distance, using the `<=>` operator. Cosine is bounded in $[0, 2]$ and produces interpretable similarity thresholds for fashion retrieval.

#figure(
  table(
    columns: (auto, 2fr, 2fr, 3fr),
    align: (start, start, start, start),
    table.header([*Property*], [*Benchmark Value*], [*Production Target*], [*Rationale*]),
    [Extension], [pgvector 0.8+], [pgvector 0.8+], [Open-source, zero additional infrastructure],
    [Index type], [IVFFlat (100 lists)], [HNSW], [IVFFlat for fast build and simple tuning during evaluation; HNSW for optimal recall at scale],
    [Distance metric], [Cosine], [Cosine], [Bounded range, interpretable thresholds for fashion similarity],
    [Model metadata], [model_name, model_version columns], [model_name, model_version columns], [Enables per-model filtering and A/B testing],
  ),
    kind: table,
  caption: [pgvector configuration: benchmark evaluation and production target],
) <tbl-pgvector-config>