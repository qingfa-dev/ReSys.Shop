=== Database Scalability Metrics

To validate the scalability of the storage layer, we benchmarked the PostgreSQL (pgvector) database with HNSW indexing enabled.

#figure(
  table(
    columns: (auto, 1fr, 1fr, 1fr, 1fr),
    stroke: 0.5pt,
    align: center,
    [*Model*], [*Avg Latency*], [*P95 Latency*], [*QPS*], [*HNSW Recall*],
    [EfficientNet-B0], [62.37ms], [74.42ms], [16.02], [1.00],
    [DINOv2], [19.65ms], [30.19ms], [50.85], [1.00],
    [Standard CLIP], [27.16ms], [36.05ms], [36.79], [1.00],
    [Fashion-CLIP], [21.19ms], [25.85ms], [47.15], [1.00],
  ),
  caption: [PGVector Query Performance. HNSW indexing maintains perfect recall (1.0) while delivering sub-30ms latency for Transformer models.],
  kind: table,
)

- *Database Scalability:* The "Order Summary" queries consistently returned in under *20ms*, verifying that the read-optimized database design scales effectively even with thousands of records.
