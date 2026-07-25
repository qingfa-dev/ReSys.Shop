=== Vector Databases and Approximate Search

Once images are converted to embeddings, those vectors must be stored and queried efficiently. For a catalog of $n$ products, a naive brute-force search requires $n$ distance computations per query. At 10,000 items this is manageable on modern hardware; at millions it becomes impractical.

==== Approximate Nearest Neighbour Search

The solution is *Approximate Nearest Neighbour* (ANN) search. Rather than exhaustively computing the distance to every stored vector, ANN algorithms use index structures that organise vectors into navigable graphs or trees. A query navigates directly toward the neighbourhood of likely matches, skipping the vast majority of irrelevant vectors. The accuracy trade-off is modest: typically 97 to 99% recall of the true top matches, for a speed improvement of several orders of magnitude. For product search, where returning 20 visually similar items matters far more than guaranteeing the absolute 21st best match, this trade-off is entirely acceptable.

==== HNSW: Hierarchical Navigable Small World

The HNSW index is among the most widely adopted ANN algorithms. It constructs a multi-layered graph where each layer contains a subset of vectors connected by edges to their nearest neighbours. Top layers are sparse and enable long-range jumps across the embedding space; bottom layers are dense and refine the search locally. A query begins at the top layer, descends through progressively finer graphs, and converges on the neighbourhood of the query vector. The search complexity scales logarithmically with the number of vectors, making HNSW suitable for interactive applications where query latency must remain under tens of milliseconds.

==== pgvector: PostgreSQL with Vector Search

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
