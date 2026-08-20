This chapter closes the thesis: summary, research questions, contributions, limitations, future work, and requirements traceability.

== Summary of Work

This thesis built a fashion e-commerce platform integrating a Vue 3 storefront, .NET 10 modular monolith, and Python ML sidecar. The visual search pipeline was evaluated through systematic benchmark of four models under 3-fold cross-validation on 5,000 fashion images. Three principal findings: domain-specific pre-training provides measurable retrieval advantages; the accuracy-efficiency trade-off is navigable via architecture choice; and the polyglot sidecar architecture is viable for .NET enterprise stacks, achieving interactive response times on commodity hardware.

=== Answering the Research Questions

*RQ1: How do fashion-specific embedding models compare with general-purpose models spanning CNN and ViT architectures?*

Fashion-CLIP outperformed all three general-purpose models: mAP 0.9309 vs CLIP-generic 0.9115 (+2.13%), EfficientNet-B0 0.8895 (+4.65%), ResNet-50 0.8857 (+5.10%). The advantage holds at shallow (P\@5: 0.9582 vs 0.9440) and deep (P\@20: 0.9374 vs 0.9239) retrieval depths with lowest cross-fold variability (±0.0068).

*RQ2: What trade-offs exist between search accuracy and processing speed?*

The trade-off is substantial. Fashion-CLIP (mAP 0.9309, 96.8 ms) represents the quality ceiling; EfficientNet-B0 (37.8 ms) achieves 95.55% of that accuracy at 39.1% of the latency. Domain fine-tuning provides accuracy without speed penalty (Fashion-CLIP vs CLIP-generic: +2.13% mAP at identical latency). For latency-sensitive deployments, EfficientNet-B0 is recommended; for quality-critical, Fashion-CLIP.

*RQ3: Can a service-oriented architecture with a dedicated AI sidecar effectively separate image inference from the main web application while maintaining acceptable response times?*

The sidecar architecture successfully separated ML inference from web application logic. End-to-end search latency remained under one second on CPU. Independent scaling and fault isolation were achieved without distributed infrastructure overhead, confirming viability for real-time interactive search on consumer-grade hardware.

=== Achievement of Technical Objectives

All four technical objectives were met. Model integration was demonstrated through the operational search pipeline. Polyglot architecture delivered clean separation via the sidecar pattern. pgvector feasibility was confirmed: IVFFlat queries execute under 10 ms (2.7-6.5 ms). Benchmark evaluation produced empirical accuracy and efficiency metrics across four models and six supported architectures.

== Contributions

This thesis makes five concrete contributions:

- *A four-model benchmark for fashion image retrieval.* Systematic evaluation with seven reported accuracy columns (three metric families at three depths) and five efficiency metrics across four architecture families, six models supported, 3-fold cross-validation protocol.
- *A reference CBIR implementation integrated into a production-style e-commerce platform.* Demonstrates that open-source tools (PyTorch, FastAPI, pgvector, .NET 10) deliver competitive visual search.
- *A pluggable model architecture enabling runtime model switching.* Strategy-pattern Model Manager controlled via environment variable decouples model selection from application code.
- *Demonstration of pgvector's ACID-compliant vector storage.* Embeddings in the same PostgreSQL database as product data eliminate stale-index bugs.
- *A validated polyglot architecture pattern for .NET and Python AI.* The sidecar integration provides a blueprint for teams incorporating Python ML into .NET applications.

== Limitations

Several limitations constrain the generalisability of the findings. The benchmark uses 5,000 product images from a single dataset; results may not generalise to other markets. All figures were measured on a single laptop with CPU-only inference. The binary category-label ground truth is a coarse proxy for visual similarity. No formal user study was conducted. All models were used as published without fine-tuning. CLIP-based models' text-to-image capability was not evaluated. The enriched-label evaluation reduces P\@20 substantially (from \~0.90 under category-only labels to \~0.30 under category+colour+pattern labels, see Appendix A.2 and A.3) due to the finer-grained relevance criterion. RAM measurement via process-level tools proved unreliable; actual consumption ranges from 100 MB to over 600 MB per model.

== Future Work

Seven directions for future work are motivated by the limitations above and by insights from design and implementation.

1. Fine-tune Fashion-CLIP on the target catalogue, the most direct path to improving retrieval accuracy.
2. Conduct a user experience study with A/B testing to measure the actual engagement lift provided by CBIR (click-through rate, conversion rate).
3. Implement multi-modal search combining text and image queries, exploiting CLIP-family models' shared latent space.
4. Scale the benchmark to production-size catalogues (100,000 to 1,000,000 images) to validate pgvector HNSW scalability and model ranking stability.
5. Investigate ONNX Runtime optimisation to reduce transformer inference latency by 30 to 50 percent through operator fusion and hardware-specific kernels.
6. Add personalised re-ranking using user-level signals (past purchases, browsing history, wishlists).
7. Develop a mobile application with on-device inference using quantised EfficientNet-B0 for offline visual search.

These directions define a roadmap from research demonstration to production-grade visual commerce engine, each grounded in empirical findings and architectural decisions documented in preceding chapters.

== Requirements Traceability

@tbl-traceability confirms that every objective and research question from Chapter 1 is addressed in a specific chapter section and produces a verifiable finding.

#figure(
  table(
    columns: (2fr, 2fr, 4fr),
    stroke: 0.5pt,
    align: (left, left, left),
    table.header([*Objective / RQ*], [*Addressed In*], [*Key Finding*]),
    [Integrate pre-trained deep learning models into a conventional e-commerce stack],
    [Chapter 2, Sections 2.2.3--2.2.4, 2.3.2--2.3.3],
    [Functional visual search pipeline: Vue storefront, .NET backend, Python ML sidecar, PostgreSQL pgvector, sub-second latency.],
    [Architect a polyglot system bridging .NET and Python ML],
    [Chapter 2, Sections 2.2.3--2.2.4, 2.3.1--2.3.3],
    [Sidecar pattern isolates ML inference from transactional logic; integration tests validate cross-service HTTP contract.],
    [Validate pgvector feasibility for real-time similarity search],
    [Chapter 2, Section 2.3.4, Section 2.4.3],
    [IVFFlat cosine similarity queries under 10 ms (2.7--6.5 ms); vectors share PostgreSQL database with relational data.],
    [Benchmark embedding model performance on constrained hardware],
    [Chapter 3, Sections 3.2--3.7],
    [Four models evaluated on 5,000 images. Fashion-CLIP: mAP 0.9309; EfficientNet-B0: 37.8 ms.],
    [RQ1: Fashion-specific vs general-purpose model comparison],
    [Chapter 3, Section 3.3; Chapter 3, Section 3.5],
    [Fashion-CLIP outperforms all general-purpose models: mAP 0.9309 vs 0.8857--0.9115 (+2.13--5.10%).],
    [RQ2: Accuracy vs speed trade-offs],
    [Chapter 3, Sections 3.3--3.6],
    [Fashion-CLIP: mAP 0.9309 at 96.8 ms; EfficientNet-B0: 95.55% of accuracy at 39.1% of latency (37.8 ms).],
    [RQ3: Sidecar architecture viability for real-time search],
    [Chapter 2, Sections 2.3.2--2.3.3; Chapter 3, Section 3.7],
    [End-to-end latency under one second; independent scaling and fault isolation without distributed overhead.],
    [Build AI service],
    [Chapter 2, Section 2.3.2],
    [Python FastAPI service with three-layer architecture, lazy-loading, containerised via Docker.],
    [Set up vector search],
    [Chapter 2, Section 2.3.4, Section 2.4.3],
    [pgvector with cosine similarity; IVFFlat queries under 10 ms; vector storage coexists with relational data.],
    [Connect the services],
    [Chapter 2, Sections 2.3.2--2.3.3],
    [.NET CBIR handler: client validation, magic-byte verification, HTTP to ML sidecar, pgvector query, result deduplication.],
    [Create the user interface],
    [Chapter 2, Sections 2.3.3 and 2.3.5],
    [Vue 3 storefront: drag-and-drop upload, similarity badges, product-card display, client-side validation.],
    [Evaluate the results],
    [Chapter 3, Sections 3.2--3.7],
    [Four-model benchmark, 3-fold cross-validation on 5,000 images, seven reported accuracy columns (three metric families at three depths) and five efficiency metrics.],
  ),
  caption: [Requirements traceability: mapping from Chapter 1 objectives and research questions to the chapters where they are addressed, with key findings confirming each was met.],
  kind: table,
) <tbl-traceability>

The traceability table confirms thesis completeness: every objective finds resolution in the architecture, implementation, and evaluation chapters.

This thesis demonstrated that integrating deep learning-based visual search into a conventional e-commerce platform is technically feasible with open-source tools on modest hardware. The contributions are offered as building blocks for practitioners enabling customers to search by showing, not describing.
