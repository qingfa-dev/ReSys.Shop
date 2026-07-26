=== IVFFlat: Inverted File with Flat Compression

IVFFlat is a simpler ANN algorithm used during model evaluation. It partitions the embedding space into clusters via *k-means* and stores each vector in its nearest cluster @pgvector2023.

- *Search.* Compute distance to all cluster centroids, select the nearest few clusters, and search exhaustively within only those clusters.
- *Recall.* Moderate: 65 to 72% at sub-10 ms for catalog-scale data, per pgvector documentation @pgvector2023. Recall degrades as the catalog grows because each cluster contains more candidates.
- *Build cost.* Low. Clustering completes in under one second for 5,000 vectors with minimal memory overhead.

*Configuration parameters:*

- *lists.* Number of clusters. More lists means smaller clusters and faster search, but risks missing relevant vectors.
- *probes.* Number of clusters searched per query. More probes improve recall at linear cost to query latency.

IVFFlat is used for the model comparison benchmarks in Chapter 3, where the objective is ranking embedding models rather than optimising index performance. Its fast build time and simple configuration suit rapid evaluation. For production, HNSW is the designated long-term index.
