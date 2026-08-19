=== pgvector: Vector Search in PostgreSQL

pgvector is an open-source PostgreSQL extension adding vector operations to standard SQL @pgvector2023, storing vectors alongside regular product data.

Key features:
- *Vector column.* `VECTOR(512)` stores embeddings, supporting varying dimensions.
- *Similarity operators.* `<=>` for cosine distance, `<->` for Euclidean.
- *Indexing.* HNSW and IVFFlat for fast approximate search.

The critical advantage is transactional consistency: vectors and product metadata share the same ACID boundary, eliminating dual-database drift. Combined queries can search for visually similar products filtered by category and price range in one query plan.

```sql
CREATE INDEX ON products USING hnsw (image_embedding vector_cosine_ops);
SELECT id, name, 1 - (image_embedding <=> query_vec) AS similarity
FROM products ORDER BY image_embedding <=> query_vec LIMIT 10;
```
