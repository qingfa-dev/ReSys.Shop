=== HNSW: Hierarchical Navigable Small World

HNSW is one of the most effective ANN algorithms and the preferred index for production-scale vector search in this project @malkov2018efficient. It builds a multi-layered graph structure where each vector is a node connected to its nearest neighbours.

The graph has multiple layers. The top layers are sparse and connect distant regions of the embedding space, enabling long-range jumps. The bottom layers are dense and refine the search locally. To search, the algorithm starts at a node in the top layer, greedily traverses edges toward the nearest neighbour, descends to the next layer, and repeats. The process converges on the query neighbourhood in logarithmic time: search cost scales with the logarithm of the catalog size rather than the size itself.

#figure(
  image("../../../../figures/chapters/part2/ch1-background/data-02-pgvector-hnsw.png", width: 90%),
  caption: [HNSW index structure with multiple layers for efficient navigation through the embedding space],
) <fig-hnsw-design>

HNSW has two main configuration parameters:

- *M.* The number of connections per node. Higher values improve recall but increase memory usage and build time. The default value of 16 provides a good balance for most use cases.
- *ef_construction.* How many candidates to consider during index construction. Higher values produce a better index at the cost of longer build time.

HNSW consistently exceeds 95% recall at query latencies under 10 milliseconds, sustained across catalog scales of up to 10 million vectors. Its logarithmic query cost makes it suitable for interactive fashion retrieval at millions of catalog items, where sub-100 ms response time is required.
