===== Product Images & Vectorization
To support the multi-model AI strategy, the database uses a flexible *One-to-Many* relationship for storing embeddings.

```csharp
public class ProductImage : Aggregate {
    public Guid ProductId { get; set; }
    public string Url { get; set; }
    public ICollection<ImageEmbedding> Embeddings { get; set; }
}

public class ImageEmbedding : Entity {
    public string ModelName { get; set; } // e.g., "fashion_clip"
    public Vector Vector { get; set; } // pgvector column, 512-dim
}
```

To optimize high-dimensional search performance, we implement a *Hierarchical Navigable Small World (HNSW)* index on the `Vector` column (@fig-hnsw-design). The HNSW algorithm allows the system to perform approximate nearest neighbor (ANN) searches with sub-linear time complexity, ensuring that search results are retrieved in milliseconds even as the catalog grows to millions of items.

/*
  Embeddings Pipeline (UC-0016)
*/
*Asynchronous Vector Generation:*
- *Logic Flow (Webhook):* The UI does not wait for vectorization. The upload finishes immediately. Later, the frontend receives a WebSocket notification (or polls an endpoint) to "Light Up" the semantic search capability for that product.
- *Sequence Flow:* @fig:sq-0016-embed shows the decoupling. The `ML Service` processes the image on a GPU-optimized node. Upon completion, it calls back to the Core API (`POST /callbacks/embedding-complete`) to update the `ImageEmbedding` entity.

#figure(
  placement: none,
  image("../../../../../images/diagrams/sequences/system/sq-0016-embeddings.png", width: 85%),
  caption: [Embeddings Generation: The pipeline for converting product images into 512-dimensional vectors (UC-0016).],
) <fig:sq-0016-embed>

/*
  Index Construction (UC-0018)
*/
*Graph Maintenance:*
- *Logic Flow (Admin Trigger):* While primarily a scheduled background task (e.g., 03:00 UTC), this process exposes a manual "Rebuild Index" control in the *System Admin Console* to allow operators to force immediate consistency after bulk imports.
- *Sequence Flow:* @fig:sq-0018-index illustrates the `IVFFlat` (or HNSW) build process. This compute-intensive operation is offloaded to a read-replica or executed during maintenance windows to avoid locking the `vector` table during high-write periods.

#figure(
  placement: none,
  image("../../../../../images/diagrams/sequences/system/sq-0018-vector-index.png", width: 80%),
  caption: [Index Construction: Building the HNSW graph structures for efficient nearest-neighbor lookup (UC-0018).],
) <fig:sq-0018-index>
