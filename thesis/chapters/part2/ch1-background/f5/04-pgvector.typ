=== pgvector: Vector Search in PostgreSQL

pgvector is an open-source extension that adds vector operations to PostgreSQL @pgvector2023. It allows storing vectors alongside regular product data (names, prices, images) in the same database, using standard SQL.

Key features of pgvector:

- *Vector column type.* `VECTOR(512)` stores 512-dimensional embeddings. The extension accommodates the different embedding dimensions produced by different models.
- *Similarity operators.* The `<=>` operator computes cosine distance (values in the range [0, 2], where 0 means identical and 2 means maximally dissimilar). The `<->` operator computes Euclidean distance.
- *Index support.* Both HNSW and IVFFlat indexing are supported for fast approximate search.

==== Why pgvector Over a Separate Vector Database

The critical advantage of pgvector is *transactional consistency*. Vectors and product metadata live in the same database. A product update and its embedding update occur within a single ACID transaction. If an admin changes a product image, the new embedding is committed atomically with the catalog update. This eliminates the dual-database problem: a separate vector store that can drift out of sync with the relational source of truth.

Combined queries present a second advantage. Vector similarity and relational filtering can be combined in a single SQL statement: find products visually similar to a query image, restricted to a specific category and within a price range, using one query plan. With separate databases, this requires querying the vector store, collecting result IDs, and querying the relational store in a second pass.

==== Example Usage

A simplified example of how vectors are stored and searched:

```sql
CREATE TABLE products (
    id SERIAL PRIMARY KEY,
    name TEXT,
    price DECIMAL,
    image_embedding VECTOR(512)
);

CREATE INDEX ON products
USING hnsw (image_embedding vector_cosine_ops);

SELECT id, name, price,
       1 - (image_embedding <=> query_vector) AS similarity
FROM products
ORDER BY image_embedding <=> query_vector
LIMIT 10;
```

The HNSW index enables the `ORDER BY ... <=>` clause to execute in logarithmic time rather than scanning the entire table.
