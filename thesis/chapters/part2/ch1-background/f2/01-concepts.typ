=== Visual Search Concepts

Content-Based Image Retrieval (CBIR) replaces text queries with image queries. Rather than requiring users to describe what they want in words, CBIR lets them search by example.

The system encodes a query image into a *dense vector embedding*: a fixed-length sequence of numbers that captures shape, texture, colour, and pattern. It then retrieves catalog items whose embeddings are nearest in vector space. Visually similar products produce similar vectors; dissimilar products produce distant ones.

This approach bypasses the need for consistent textual labels. A photograph of a dress with a distinctive neckline retrieves visually similar products regardless of how the catalog describes them.

=== The Semantic Gap

The semantic gap, introduced in Section 1.1, is the discrepancy between the visual richness of a garment and a user's ability to express that richness in keywords. CBIR bridges this gap by operating directly on visual content rather than on human-authored metadata. The embedding serves as a universal descriptor that captures attributes such as fabric texture, silhouette proportion, and colour gradients automatically, without any keyword translation step.
