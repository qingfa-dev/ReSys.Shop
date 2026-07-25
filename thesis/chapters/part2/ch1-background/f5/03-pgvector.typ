=== pgvector: PostgreSQL with Vector Search

This project uses *pgvector*, an open-source PostgreSQL extension that implements HNSW-indexed vector storage and similarity search within the standard relational database. The key practical advantage is transactional consistency: because vectors and product metadata live in the same database, a product update and its embedding update occur within a single ACID transaction. This eliminates the *dual-database problem*, where a separate vector store can drift out of sync with the relational source of truth, producing stale search results.

Queries combine vector similarity with relational filtering in standard SQL: find products visually similar to a query image, but restrict results to a specific category and price range, using a single query plan. The extension supports variable-length vectors, accommodating the different embedding dimensions produced by different models (384 for DINOv2-S, 512 for Fashion-CLIP, 768 for DINOv2-B, 1280 for EfficientNet-B0, 2048 for ResNet-50).

#figure(
  table(
    columns: (auto, auto, 1fr),
    align: (start, start, start),
    table.header([*Property*], [*Value*], [*Rationale*]),
    [Extension], [pgvector 0.8+], [Open-source, zero additional infrastructure],
    [Index type], [HNSW], [Logarithmic search complexity, good recall-speed balance],
    [Distance metric], [Cosine], [Bounded range, interpretable thresholds for fashion similarity],
    [Model metadata], [model_name, model_version columns], [Enables per-model filtering and A/B testing],
  ),
  caption: [pgvector configuration used in this thesis],
) <tbl-pgvector-config>
