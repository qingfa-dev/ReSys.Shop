=== The Search Challenge and Approximate Nearest Neighbours

Consider a catalog of 10,000 fashion products, each represented by a 512-dimensional embedding vector. When a user uploads a query image, the system must compare that query vector to all 10,000 product vectors to find the most similar ones. This is called *nearest neighbour search*.

A naive approach computes the distance to every vector: 10,000 comparisons per search. For a catalog of 100,000 products, that becomes 100,000 comparisons. For one million products, one million comparisons. For real-time search (responding under one second), this brute-force approach is too slow.

The solution is *Approximate Nearest Neighbour* (ANN) search. Rather than checking every vector, ANN algorithms use index structures that organise vectors into navigable graphs or clusters. A query navigates directly toward the neighbourhood of likely matches, skipping the vast majority of irrelevant vectors.

The key insight is simple: we do not need the absolute best match. If the true best match has a similarity of 0.95 and the returned result has 0.93, that is acceptable for product search. The accuracy trade-off is modest: ANN algorithms typically achieve *97 to 99% recall* of the true top matches, while reducing search time by several orders of magnitude. For product search, where returning 20 visually similar items matters far more than guaranteeing the absolute 21st best match, this trade-off is entirely acceptable.

Two ANN indexing algorithms are used in this project: HNSW for production-scale search and IVFFlat for rapid evaluation.
