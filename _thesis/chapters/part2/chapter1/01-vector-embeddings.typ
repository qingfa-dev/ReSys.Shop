== VECTOR EMBEDDINGS

At the heart of visual search is a simple idea: turning images into lists of numbers that a computer can compare. These lists are called *vector embeddings* or *feature vectors*.

=== Definition and Mathematical Representation

When we look at an image, we see colors, shapes, and patterns. Computers cannot "see" in the same way; they require numerical data to process information. A vector embedding is a way to represent the visual content of an image as a sequence of numbers.

For example, when an AI model processes an image of a red dress, it might output a list like:

```
[0.23, -0.15, 0.87, 0.42, ..., -0.31]  (512 numbers total)
```

This list captures the "essence" of that image in a compressed form. Similar images will produce similar lists of numbers.

=== The Latent Space

When we talk about vectors, we can think of them as points in a high-dimensional space. For a 512-dimensional vector, imagine a space with 512 axes (instead of just the 3 axes we can visualize: x, y, z).

In this space:
- Similar images are located close together
- Different images are far apart

This space where embeddings live is called the *latent space*. The word "latent" means hidden, signifying that these dimensions do not correspond to obvious things like "redness" or "stripiness," but rather to abstract features the model learned during training.

=== Measuring Similarity

To find similar products, the system needs to measure how close two vectors are. This project uses *cosine similarity*, which measures the angle between two vectors:

$ "similarity" = cos(theta) = (A dot B) / (||A|| ||B||) $

Where:
- $A$ and $B$ are the two vectors being compared
- $A dot B$ is the dot product (multiply corresponding elements and sum)
- $||A||$ and $||B||$ are the lengths of each vector

The cosine similarity ranges from:
- *1.0* = vectors point in the same direction (very similar)
- *0.0* = vectors are perpendicular (unrelated)
- *-1.0* = vectors point in opposite directions (very different)

For fashion images, a cosine similarity above 0.7 typically indicates visual similarity that users would recognize.

=== Mathematical Similarity: From Visual Comparison to Cosine Distance

The power of embeddings is that complex visual comparisons become simple math. Instead of trying to write rules like "match items with similar colors and patterns," the system:

1. Converts the query image to a vector
2. Compares that vector to all product vectors in the database
3. Returns products with the highest similarity scores

This approach is much more flexible than rule-based matching because the AI model learns what features are important from examples, rather than having those features hand-coded.


Vector embeddings are the mathematical foundation that enables visual search. The key question becomes: *How do we generate these embeddings?* This requires specialized neural network architectures that can extract meaningful features from images. The following sections explore different approaches to embedding generation, starting with Convolutional Neural Networks.
