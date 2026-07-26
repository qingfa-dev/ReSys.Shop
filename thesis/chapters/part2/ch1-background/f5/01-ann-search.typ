=== Approximate Nearest Neighbour Search

For a catalog of $n$ products, a naive brute-force search requires $n$ distance computations per query. At 10,000 items this is manageable on modern hardware; at millions it becomes impractical.

The solution is *Approximate Nearest Neighbour* (ANN) search. Rather than exhaustively computing the distance to every stored vector, ANN algorithms use index structures that organise vectors into navigable graphs or trees. A query navigates directly toward the neighbourhood of likely matches, skipping the vast majority of irrelevant vectors. The accuracy trade-off is modest: typically *97 to 99% recall* of the true top matches, for a speed improvement of several orders of magnitude. For product search, where returning 20 visually similar items matters far more than guaranteeing the absolute 21st best match, this trade-off is entirely acceptable.
