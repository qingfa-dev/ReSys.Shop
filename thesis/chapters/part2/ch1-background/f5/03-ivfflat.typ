=== IVFFlat: Inverted File with Flat Compression

IVFFlat is a simpler ANN algorithm used during evaluation. It partitions the embedding space into clusters using *k-means* and stores each vector in the cluster whose centroid it is nearest to. A query computes the distance to all cluster centroids, selects the nearest few clusters, and searches exhaustively only within those selected clusters.

IVFFlat has two configuration parameters:

- *lists.* The number of clusters. More lists mean smaller clusters and faster search, but risk missing relevant vectors in unexamined clusters.
- *probes.* The number of clusters searched per query. More probes improve recall at the cost of query latency.

Compared to HNSW, IVFFlat's build cost is low: clustering completes in under one second for 5,000 vectors. However, recall is moderate: 65 to 72% at sub-10 ms latency for catalog-scale data. Recall degrades as the catalog grows because each cluster contains more candidates.

IVFFlat is used for the model comparison benchmarks in Chapter 3, where the objective is ranking embedding models rather than optimising index performance. Its fast build time and simple configuration suit rapid evaluation. For production deployment, HNSW is the designated long-term index.
