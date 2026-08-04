=== HNSW: Hierarchical Navigable Small World

HNSW is the preferred index for production-scale vector search @malkov2018efficient. It builds a multi-layered graph where each vector is a node connected to its nearest neighbours.

- *Structure.* Multiple layers form a hierarchy. Top layers are sparse and connect distant regions for long-range jumps. Bottom layers are dense and refine the search locally.
- *Search.* Start at a top-layer node, greedily traverse toward the nearest neighbour, descend one layer, and repeat. Cost scales logarithmically with catalog size.
- *Recall.* Reported at over 95% with query latencies under 10 ms, sustained across catalog scales of up to 10 million vectors @malkov2018efficient.
- *Build cost.* Graph construction is computationally expensive. At tens of thousands of vectors, build time is measured in minutes.

*Configuration parameters:*

- *M.* Connections per node. Default 16. Higher improves recall at cost of memory and build time.
- *ef_construction.* Candidates evaluated during index build. Higher produces better index at cost of build time.

HNSW's logarithmic query cost makes it suitable for interactive fashion retrieval at millions of catalog items @malkov2018efficient, where sub-100 ms latency is required.
