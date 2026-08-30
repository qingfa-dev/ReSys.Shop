This chapter summarises the thesis, answers the research questions, states the contributions, discusses limitations and future work, and traces requirements to findings.

== Summary of Work

This thesis built a fashion e-commerce platform integrating a Vue 3 storefront, .NET 10 modular monolith, and Python ML sidecar. The visual search pipeline was assessed through systematic benchmarking of six models under 3-fold cross-validation on 5,000 fashion images. Three principal findings emerged. Domain-specific pre-training provides measurable retrieval advantages. The accuracy-efficiency trade-off can be managed through architecture choice. The polyglot sidecar architecture is viable for .NET enterprise stacks, achieving interactive response times on commodity hardware.

=== Answering the Research Questions

#figure(
  caption: [Research Question Answers],
  table(
    columns: (auto, 3fr, 2fr),
    align: (left, left, left),
    stroke: 0.5pt,
    table.header([*RQ*], [*Answer*], [*Evidence*]),
    [*RQ1*], [Fashion-specific vs general-purpose models: Fashion-CLIP achieved highest mAP (0.9336); advantage over DINOv2 ViT-S/14 (0.40\%) within measurement uncertainty at 3-fold CV; CLIP family more stable than DINOv2 under fine-grained ground truth], [@tbl-aggregate, @tbl-groundtruth, F3.6.1, F3.6.2],
    [*RQ2*], [Accuracy-speed trade-off spans 2.86\% mAP across 2.7x latency range (42.6--113.6 ms); EfficientNet-B0 achieves 97.2\% of Fashion-CLIP's accuracy at 37.5\% of latency], [@tbl-efficiency, @tbl-comparison],
    [*RQ3*], [Sidecar architecture separates ML inference from web logic; end-to-end search latency under one second on CPU; model switching via environment variable], [Section 2.3, @tbl-comparison],
  ),
  kind: table,
) <tbl-rq-answers>

*Achievement of Technical Objectives.* All four technical objectives were addressed within the scope of the 5,000-image benchmark and commodity hardware. Model integration was demonstrated through the operational search pipeline. Polyglot architecture achieved clean separation using the sidecar pattern. pgvector proved feasible: IVFFlat queries execute under 10 ms (2.7-6.5 ms). Benchmark analysis yielded accuracy and efficiency metrics across six benchmarked models spanning four architectural families.

== Contributions

This thesis makes five concrete contributions:

- *A six-model benchmark for fashion image retrieval.* Systematic benchmarking with seven reported accuracy columns (three metric families at three depths) and five efficiency metrics across four architecture families, six models supported, 3-fold cross-validation protocol, plus a three-way ground-truth sensitivity analysis.
- *A reference CBIR implementation integrated into a production-style e-commerce platform.* The pipeline shows that open-source tools (PyTorch, FastAPI, pgvector, .NET 10) can support competitive visual search.
- *A pluggable model architecture enabling runtime model switching.* Strategy-pattern Model Manager controlled via environment variable decouples model selection from application code.
- *Demonstration of pgvector's ACID-compliant vector storage.* Embeddings in the same PostgreSQL database as product data eliminate stale-index bugs.
- *A validated polyglot architecture pattern for .NET and Python AI.* The sidecar integration gives other teams a working example for incorporating Python ML into .NET applications.

As referenced by the title's *Recommendation*, product recommendation in this work is delivered through the *Similar Products* visual-similarity feature (Section 2.3): given a target product, its primary image embedding is queried against the catalogue to surface visually similar items. This is a query-driven, content-based recommendation mechanism; collaborative-filtering or session-based personalisation is not implemented and is left to future work.

== Limitations

Several limitations affect how well these findings apply more broadly:

+ The benchmark uses 5,000 product images from a single dataset; results may not generalise to other markets.
+ All figures were measured on a single laptop with CPU-only inference.
+ The binary category-label ground truth is an imperfect stand-in for visual similarity.
+ No formal user study was conducted.
+ All models were used as published without fine-tuning.
+ CLIP-based models' text-to-image capability was not assessed.
+ The enriched-label scheme reduces P\@20 substantially (from \~0.90 under category-only labels to \~0.30 under category+colour+pattern labels, see Appendix A.2 and A.3) due to the finer-grained relevance criterion.
+ RAM figures are approximate, estimated from each model's parameter count plus PyTorch runtime overhead (ranging from ~100 MB for EfficientNet-B0 to ~600 MB for the CLIP-family models) because direct process-level measurement proved unreliable.
+ The 5,000-image benchmark establishes retrieval quality and relative model ranking, but does not characterise behaviour at production catalogue scale (millions of items), where index build time, query throughput under concurrent load, and embedding-storage growth become the dominant operational concerns.
+ Fashion-CLIP's retrieval advantage over general-purpose CLIP may partly reflect differences in its 700K-image fashion pre-training corpus rather than architecture or fine-tuning alone; isolating each factor's contribution is outside this thesis's scope.

=== Ethical Considerations

Visual search also raises ethical considerations that this engineering thesis does not resolve:

+ Image-based recommendation may reinforce homogeneity, repeatedly surfacing visually similar items and narrowing catalogue diversity rather than broadening discovery.
+ Where a deployment learns from user behaviour, the underlying embeddings can inherit and amplify bias present in the training data (e.g., skewed representation across body types, skin tones, or styles).
+ Uploading personal images for search raises privacy expectations around storage, retention, and deletion, which a production deployment must address through explicit consent and retention policies.
+ Transformer inference carries an energy cost that scales with catalogue and request volume.

These concerns are relevant to any responsible deployment of the reference implementation and are acknowledged here as open issues rather than resolved claims.

== Future Work

These directions address the limitations identified above. They are grouped roughly by expected effort: near-term, low-risk extensions (1-3) build directly on the existing pipeline; medium-term enhancements (4-6) require additional data or components; and the longest-lead item (7) targets a separate deployment target.

1. Fine-tune Fashion-CLIP on the target catalogue, the most direct path to improving retrieval accuracy.
2. Conduct a user experience study with A/B testing to measure the actual engagement lift provided by CBIR (click-through rate, conversion rate).
3. Implement multi-modal search combining text and image queries, exploiting CLIP-family models' shared latent space.
4. Scale the benchmark to production-size catalogues (100,000 to 1,000,000 images) to validate pgvector HNSW scalability and model ranking stability.
5. Investigate ONNX Runtime optimisation to reduce transformer inference latency by 30 to 50 percent through operator fusion and hardware-specific kernels.
6. Add personalised re-ranking using user-level signals (past purchases, browsing history, wishlists).
7. Develop a mobile application with on-device inference using quantised EfficientNet-B0 for offline visual search.

These directions outline next steps from research demonstration toward a production-grade visual commerce engine, each based on empirical findings and architectural decisions documented in preceding chapters.

== Requirements Traceability

@tbl-traceability shows that every objective and research question from Chapter 1 is addressed in a specific chapter section and produces a verifiable finding.

#figure(
  table(
    columns: (2fr, 2fr, 4fr),
    stroke: 0.5pt,
    align: (left, left, left),
    table.header([*Objective / RQ*], [*Addressed In*], [*Key Finding*]),
    [Integrate pre-trained deep learning models into a conventional e-commerce stack],
    [Chapter 2, Sections 2.2.3--2.2.4, 2.3.2--2.3.3],
    [Functional visual search pipeline: Vue storefront, .NET backend, Python ML sidecar, PostgreSQL pgvector, sub-second latency.],
    [Architect a polyglot system connecting .NET and Python ML],
    [Chapter 2, Sections 2.2.3--2.2.4, 2.3.1--2.3.3],
    [Sidecar pattern isolates ML inference from transactional logic; integration tests validate cross-service HTTP contract.],
    [Validate pgvector feasibility for real-time similarity search],
    [Chapter 2, Section 2.3.4, Section 2.4.3],
    [IVFFlat cosine similarity queries under 10 ms (2.7--6.5 ms); vectors share PostgreSQL database with relational data.],
    [Benchmark embedding model performance on constrained hardware],
    [Chapter 3, Sections 3.3--3.8],
    [Six models benchmarked on 5,000 images. Fashion-CLIP: mAP 0.9336; EfficientNet-B0: 42.6 ms. F3.6.1, F3.6.2],
    [RQ1: Fashion-specific vs general-purpose model comparison],
    [Chapter 3, Section 3.4; Chapter 3, Section 3.6],
    [Fashion-CLIP highest mAP (0.9336); DINOv2 ViT-S/14 within 0.40\% on category-only retrieval. F3.6.1, F3.6.2, C1],
    [RQ2: Accuracy vs speed trade-offs],
    [Chapter 3, Sections 3.4--3.7],
    [Fashion-CLIP: mAP 0.9336 at 113.6 ms; EfficientNet-B0: 97.2\% of accuracy at 37.5\% of latency (42.6 ms). @tbl-efficiency],
    [RQ3: Sidecar architecture viability for real-time search],
    [Chapter 2, Sections 2.3.2--2.3.3; Chapter 3, Section 3.8.4],
    [End-to-end latency under one second; model switching via environment variable. @tbl-comparison],
    [Build AI service],
    [Chapter 2, Section 2.3.2],
    [Python FastAPI service with three-layer architecture, lazy-loading, containerised via Docker.],
    [Set up vector search],
    [Chapter 2, Section 2.3.4, Section 2.4.4],
    [pgvector with cosine similarity; IVFFlat queries under 10 ms; vector storage coexists with relational data.],
    [Connect the services],
    [Chapter 2, Sections 2.3.2--2.3.3],
    [.NET CBIR handler: client validation, magic-byte verification, HTTP to ML sidecar, pgvector query, result deduplication.],
    [Create the user interface],
    [Chapter 2, Sections 2.3.3 and 2.3.5],
    [Vue 3 storefront: drag-and-drop upload, similarity badges, product-card display, client-side validation.],
    [Assess the results],
    [Chapter 3, Sections 3.3--3.8],
    [Six-model benchmark, 3-fold cross-validation on 5,000 images, seven reported accuracy columns (three metric families at three depths) and five efficiency metrics, plus three-way ground-truth sensitivity.],
  ),
  caption: [Requirements traceability mapping objectives and research questions to chapters.],
  kind: table,
) <tbl-traceability>

All objectives and research questions are addressed; no gaps remain.

This thesis demonstrated that integrating deep learning-based visual search into a conventional e-commerce platform is technically feasible with open-source tools on modest hardware. The contributions offer reference patterns for practitioners pursuing visual search in e-commerce.
