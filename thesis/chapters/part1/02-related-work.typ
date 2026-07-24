== Related Work

=== Content-Based Image Retrieval

Content-Based Image Retrieval (CBIR) systems retrieve images from a database based on visual similarity rather than textual metadata. Early CBIR systems relied on hand-crafted features such as color histograms, texture descriptors (Gabor filters), and shape representations (SIFT, SURF). These approaches suffered from the semantic gap — low-level visual features do not map reliably to high-level human concepts.

Deep learning transformed CBIR by learning visual representations directly from data. Convolutional Neural Networks (CNNs) such as ResNet and EfficientNet extract rich feature vectors that capture semantic content. More recently, Vision Transformers (ViT) and contrastive learning approaches like CLIP have enabled zero-shot visual retrieval by learning joint visual-linguistic representations.

=== Fashion Image Retrieval

Fashion-specific retrieval presents unique challenges: fine-grained visual differences (sleeve length, pattern, neckline), style consistency across views, and the need to match across diverse product categories. Fashion-CLIP extends CLIP with fashion-domain pretraining, achieving superior retrieval on fashion datasets compared to generic models.

Prior work in fashion CBIR typically selects a single embedding model without empirical comparison. This thesis addresses that gap by evaluating four models (Fashion-CLIP, ResNet-50, EfficientNet-B0, CLIP-generic) on both retrieval effectiveness and operational performance.

=== Modular Monolith Architecture

The modular monolith pattern combines the simplicity of a single deployable unit with the modularity benefits of microservices. Each business domain lives in an isolated module with explicit boundaries, communicating via in-process message dispatch rather than network calls.

Vertical slice architecture further refines this by organizing code around feature actions rather than technical layers. Each feature (e.g., "Create Product") is cohesively implemented in a single folder containing handler, endpoint, request, response, and validator — eliminating the cross-cutting concerns of traditional horizontal layering.

=== Vector Database Integration

PostgreSQL with the pgvector extension enables similarity search directly within the relational database, eliminating the need for a separate vector database. This approach leverages existing SQL tooling, transactions, and operational knowledge while providing cosine similarity and nearest-neighbor search on embedding vectors.

=== Summary

This thesis contributes a dual evaluation: (a) architectural patterns for modular e-commerce systems, and (b) empirical comparison of embedding models for fashion CBIR. The work bridges software engineering rigor with machine learning evaluation methodology.
