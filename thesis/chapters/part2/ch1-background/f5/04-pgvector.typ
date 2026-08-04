=== pgvector: Vector Search in PostgreSQL

pgvector is an open-source PostgreSQL extension that adds vector operations to standard SQL @pgvector2023. It stores vectors alongside regular product data (names, prices, images) in the same database.

*Key features:*

- *Vector column.* `VECTOR(512)` stores 512-dimensional embeddings. Supports varying dimensions for different models.
- *Similarity operators.* `<=>` for cosine distance (range [0, 2]), `<->` for Euclidean distance.
- *Indexing.* Supports both HNSW and IVFFlat for fast approximate search.

==== Why pgvector

The critical advantage is *transactional consistency*. Vectors and product metadata share the same ACID boundary. An image change triggers a new embedding, and both are committed atomically. No dual-database drift between a vector store and the relational source of truth.

Combined queries are possible in a single SQL statement. A search can find visually similar products filtered by category and price range using one query plan.

==== Example

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
