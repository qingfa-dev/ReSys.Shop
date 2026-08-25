=== pgvector: Vector Search in PostgreSQL

pgvector is an open-source PostgreSQL extension adding vector operations to standard SQL @pgvector2023, storing vectors alongside regular product data.

Key features:
- *Vector column.* `vector` columns store embeddings and support arbitrary dimensionality; the platform stores an untyped `vector` column whose dimension is fixed per model (e.g., 512 for Fashion-CLIP).
- *Similarity operators.* `<=>` for cosine distance, `<->` for Euclidean.
- *Indexing.* HNSW and IVFFlat for fast approximate search.

The critical advantage is transactional consistency: vectors and product metadata share the same ACID boundary, avoiding the inconsistency problems that occur when a vector store and a relational database are kept separate. Combined queries can search for visually similar products filtered by category and price range in one query plan.

```sql
CREATE INDEX ON variant_image_embeddings USING hnsw ((vector::vector(512)) vector_cosine_ops)
  WHERE model_name = 'fashion_clip';
SELECT id, name, 1 - (image_embedding <=> query_vec) AS similarity
FROM products ORDER BY image_embedding <=> query_vec LIMIT 10;
```
