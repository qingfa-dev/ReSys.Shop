== VECTOR DATABASES

Once images are converted to vector embeddings, those vectors need to be stored and searched efficiently. This section explains why regular databases are not sufficient and how pgvector solves the problem.

=== The Challenge

Consider a catalog of 10,000 fashion products, each represented by a 512-dimensional vector. When a user uploads a query image:

1. The system converts the query to a vector
2. It must compare this vector to all 10,000 product vectors
3. It returns the most similar ones

A naive approach would compare the query to every vector (10,000 comparisons). This works for small catalogs but becomes slow as the catalog grows:
- 100,000 products = 100,000 comparisons per search
- 1,000,000 products = 1,000,000 comparisons per search

For real-time search (responding in under 1 second), this is too slow.

=== Approximate Nearest Neighbor (ANN) Search

The solution is *approximate nearest neighbor* search. Instead of checking every vector, ANN algorithms use clever data structures to find "good enough" matches quickly.

The key insight: we do not need the *absolute best* match, we need *good matches*. If the true best match has a similarity of 0.95 and we return a result with similarity 0.93, that is usually acceptable for product search.

=== HNSW: The Algorithm

This project uses *HNSW (Hierarchical Navigable Small World)*, one of the most effective ANN algorithms @malkov2018efficient.

HNSW works by building a graph structure where:
- Each vector is a node in the graph
- Nodes are connected to their "neighbors" (similar vectors)
- The graph has multiple layers (like a skip list)

#figure(
  image("/images/diagrams/03-data-architecture/data-02-pgvector-hnsw.png", width: 90%),
  caption: [HNSW index structure with multiple layers for efficient navigation],
) <fig-hnsw-design>

To search:
1. Start at a random node in the top (sparse) layer
2. Navigate to a node close to the query
3. Move to the next layer and refine
4. Repeat until reaching the bottom (dense) layer
5. The final neighbors are the search results

This approach reduces comparisons from O(n) to approximately O(log n), making search much faster for large catalogs.

=== pgvector: Vector Search in PostgreSQL

pgvector is an open-source extension that adds vector operations to PostgreSQL @pgvector2023. It allows storing vectors alongside regular data (product names, prices, images) in the same database.

Key features:
- *Vector column type:* `VECTOR(512)` stores 512-dimensional vectors
- *Similarity operators:* `<=>` for cosine distance, `<->` for Euclidean distance
- *Index support:* HNSW and IVFFlat indexing for fast search

=== Example Usage

A simplified example of how vectors are stored and searched:

```sql
-- Create a table with vector column
CREATE TABLE products (
    id SERIAL PRIMARY KEY,
    name TEXT,
    price DECIMAL,
    image_embedding VECTOR(512)
);

-- Create HNSW index for fast search
CREATE INDEX ON products
USING hnsw (image_embedding vector_cosine_ops);

-- Search for similar products
SELECT id, name, price,
       1 - (image_embedding <=> query_vector) AS similarity
FROM products
ORDER BY image_embedding <=> query_vector
LIMIT 10;
```

=== Configuration Choices

HNSW has two main parameters:

- *m:* Number of connections per node (higher = more accurate but more memory)
- *ef_construction:* How many candidates to consider when building (higher = better index but slower to build)

For this project:
- `m = 16` (default, good balance)
- `ef_construction = 64` (default)

These defaults work well for catalogs up to ~100,000 items. Larger catalogs might need tuning.

=== Architectural Decision: pgvector vs Specialized Vector Databases

Several specialized vector databases exist (Pinecone, Milvus, Weaviate), but pgvector was chosen for practical reasons:

#figure(
  table(
    columns: (auto, 1fr, 1fr),
    stroke: 0.5pt,
    align: (left, center, center),
    [*Feature*], [*Specialized Vector DB*], [*pgvector*],
    [Setup complexity], [Moderate-High], [Low (just an extension)],
    [Data consistency], [Separate from main DB], [Same transaction as product data],
    [Learning curve], [New query language], [Standard SQL],
    [Cost], [Often paid (SaaS)], [Free, open source],
    [Scale limit], [Billions of vectors], [Millions of vectors],
  ),
  caption: [Comparison of vector database options],
)

For a prototype with thousands of products, pgvector's simplicity outweighs the scaling advantages of specialized databases. If this system needed to scale to millions of products, migrating to a dedicated vector database would be considered.

=== Trade-offs Acknowledged

Using pgvector has limitations:
- *Not optimized for massive scale:* May struggle with millions of vectors
- *Less mature:* Fewer features than dedicated solutions
- *Single-node:* Does not distribute across multiple servers

For this project's scope (5,000 products in evaluation), these limitations are acceptable. The primary focus was on architectural integration and functional correctness rather than massive scale.

