=== Mathematical Foundations of Embeddings

An embedding model processes an input image and transforms its visual content into a fixed-length numerical array:

$ bold(e) = [e_1, e_2, e_3, ..., e_d]^T in RR^d $

For a model with a $512$-dimensional output, this vector $bold(e)$ contains $512$ continuous numbers. These arrays are called *vector embeddings*. Visually similar items yield nearby numerical values across these dimensions, converting visual comparison into a standard mathematical distance calculation.

==== The Latent Space

Embeddings act as coordinates within a high-dimensional continuous space known as the *latent space*. A $512$-dimensional vector occupies a coordinate system with $512$ orthogonal axes.

Within this space:
- Visually and semantically similar items sit close to one another.
- Dissimilar items lie further apart.
- The term "latent" indicates that individual axes rarely map directly to single human visual labels like "stripes" or "red." Instead, each dimension captures abstract, non-linear feature combinations learned by the model during training.

==== Measuring Similarity: Cosine Similarity

To evaluate how closely two images match, the system measures the directional angle between their corresponding embedding vectors $bold(A)$ and $bold(B)$ using *cosine similarity*:

$ "Cosine Similarity"(bold(A), bold(B)) = (bold(A) dot bold(B)) / (||bold(A)||_2 times ||bold(B)||_2) $

Where $bold(A) dot bold(B)$ represents the vector dot product, and $||bold(A)||_2$ and $||bold(B)||_2$ denote the Euclidean norms ($L_2$ norms) of each vector.

Cosine similarity produces values ranging from $+1.0$ (identical vector orientation) to $0.0$ (orthogonal vectors) down to $-1.0$ (opposite orientations). For normalized fashion embeddings, scores above $0.70$ generally correspond to strong visual similarity perceptible to human shoppers.

// Diagram placeholder: Visualisation of cosine similarity in 2D vector space
// #figure(image("figures/chapters/cosine-similarity.png", width: 70%), caption: [Geometric interpretation of cosine similarity between embedding vectors in vector space.])

==== The CBIR Pipeline

A standard Content-Based Image Retrieval system operates through four core sequential stages:

1. *Image Input:* The user uploads or selects a target query photograph.
2. *Embedding Generation:* A deep learning model processes the image to output its feature vector.
3. *Vector Comparison:* The database compares the query vector against pre-indexed catalog vectors using cosine distance or cosine similarity.
4. *Ranking & Retrieval:* The system orders products by their similarity scores and renders the top nearest neighbors to the user.

// Diagram placeholder: CBIR pipeline overview
// #figure(image("figures/chapters/cbir-pipeline.png", width: 90%), caption: [End-to-end architecture of the CBIR pipeline from query upload to product ranking.])
