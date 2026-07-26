=== Mathematical Foundations of Embeddings

When an AI model processes an image, it outputs a fixed-length sequence of numbers representing the visual content:

```
[0.23, -0.15, 0.87, 0.42, ..., -0.31]  (512 numbers)
```

These lists are called *vector embeddings*. Visually similar products produce similar number sequences; dissimilar products produce different ones. This transforms visual comparison into a mathematical operation.

==== The Latent Space

Embeddings can be understood as points in a high-dimensional space. A 512-dimensional vector occupies a space with 512 axes, far beyond the three spatial dimensions we can visualise. In this *latent space*:

- Similar images cluster close together
- Different images are far apart
- The word "latent" signifies that the dimensions do not correspond to obvious visual concepts like "redness" or "stripiness." They represent abstract features the model learned during training.

==== Measuring Similarity: Cosine Distance

To compare images, the system measures the angle between their embedding vectors using *cosine similarity*:

$ "cosine similarity" = (A dot B) / (||A|| times ||B||) $

Where $A$ and $B$ are the embedding vectors being compared, $A dot B$ is their dot product, and $||A||$ and $||B||$ are their Euclidean norms.

Cosine similarity ranges from +1.0 (vectors point in the same direction, very similar) to 0.0 (perpendicular, unrelated) to -1.0 (opposite directions, very dissimilar). For fashion images, values above *0.7* typically indicate visual similarity a user would recognise. The same mathematical operation works for any image, any category, without the system needing to know what makes a "dress" or a "shoe."

// Diagram placeholder: Visualisation of cosine similarity in 2D vector space
// #figure(image("images/diagrams/cosine-similarity.png", width: 70%), caption: [...])

==== The CBIR Pipeline

Content-Based Image Retrieval (CBIR) replaces text queries with image queries through the following pipeline:

1. The user uploads or selects a query image
2. The system generates an embedding for that image using a deep learning model
3. The system compares the query embedding against all product embeddings in the database using cosine distance
4. Results are ranked by similarity and displayed to the user

A photograph of a dress with a distinctive neckline retrieves visually similar products regardless of how the catalog describes them. The embedding becomes a universal description that captures shape, texture, colour, and pattern automatically.

// Diagram placeholder: CBIR pipeline overview (Mermaid flowchart)
// #figure(image("images/diagrams/01-cbir-pipeline.png", width: 90%), caption: [...])
