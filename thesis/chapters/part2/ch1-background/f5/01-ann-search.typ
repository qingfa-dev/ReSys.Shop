=== The Search Challenge

When a user uploads a query image, the system must compare its embedding vector against every product vector in the catalog. This is *nearest neighbour search*.

A naive brute-force approach scales linearly with catalog size:

- 10,000 products = 10,000 comparisons per search.
- 100,000 products = 100,000 comparisons.
- 1,000,000 products = 1,000,000 comparisons.

For real-time search (under one second), this is too slow.

=== Approximate Nearest Neighbour Search

The solution is *Approximate Nearest Neighbour* (ANN) search. Instead of checking every vector, ANN algorithms build index structures (graphs or clusters) that let a query navigate directly to the neighbourhood of likely matches, skipping irrelevant vectors.

The key trade-off: we trade perfect accuracy for speed. If the true top match has similarity 0.95 and the returned result has 0.93, that is acceptable for product search. ANN algorithms typically achieve 97 to 99% recall of the true top matches while reducing search time by orders of magnitude @malkov2018efficient. Two algorithms are used in this project: *HNSW* for production search and *IVFFlat* for rapid evaluation @pgvector2023.
