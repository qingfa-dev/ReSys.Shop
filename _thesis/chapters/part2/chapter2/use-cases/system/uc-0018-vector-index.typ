#figure(
  placement: none,
  image("../../../../../images/diagrams/usecases/system/uc-0018-vector-index.png", width: 100%),
  caption: [Use Case Diagram for UC-0018],
)

#figure(
  placement: none,
  table(
    columns: (1fr, 3fr),
    align: left,
    stroke: 0.5pt,
    [*UC-0018*], [*Vector Index Maintenance*],
    [Actor], [System],
    [Description], [Maintain HNSW index for fast similarity search.],
    [Trigger], [Vector Insertion (UC-0016).],
    [Ordinary Sequence],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [1], [System detects new embeddings.],
      [2], [Adds vector to pgvector HNSW index.],
      [3], [Vacuum/Analyze if fragmentation high.],
    ),

    [Related Use Cases], [UC-0016 (Embeddings)],
  ),
  caption: [UC-0018: Vector Index Maintenance],
)

This use case manages the lifecycle of the Hierarchical Navigable Small World (HNSW) index within the PostgreSQL database. As new vectors are generated (UC-0016), this process ensures they are efficiently inserted into the index structure. Periodic maintenance tasks, such as re-indexing or vacuuming, are also handled here to maintain sub-millisecond query performance as the dataset grows.
