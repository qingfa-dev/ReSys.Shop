== Related Work

This section positions the ReSys.Shop platform within the broader landscape of fashion image retrieval research and commercial visual search systems.

=== Academic Research

The *DeepFashion* dataset, introduced by Liu et al., established the foundational benchmark for fashion recognition and retrieval with over 800,000 images annotated with attributes, landmarks, and in-shop-to-consumer photo pairs @liu2016deepfashion. This dataset catalysed much of the subsequent work in fashion AI.

*FashionIQ* extended retrieval to the conversational setting, where users modify queries through natural language feedback ("like this dress but shorter") @wu2019fashioniq. While compelling, the interactive dialogue paradigm requires infrastructure beyond the scope of this project, which focuses on single-turn visual and text queries.

The *Fashion-CLIP* work demonstrated that domain-specific fine-tuning of CLIP on 700,000 fashion images improves retrieval by 15 to 20% over the general model @chia2022fashionclip. This thesis follows that approach, using pre-trained models without custom training, and extends the evaluation to additional architectures (ResNet, EfficientNet, DINOv2) for systematic comparison.

=== Commercial Systems

Several platforms have deployed visual search at production scale.

#figure(
  table(
    columns: (auto, 1fr, 1fr),
    stroke: 0.5pt,
    align: (center, left, left),
    [*Product*], [*Key Strength*], [*Limitation*],
    [Google Lens], [Massive scale, general-domain coverage], [Closed ecosystem, not customisable],
    [Pinterest Lens], [Over 600M monthly searches, style-aware], [Proprietary, requires Pinterest integration],
    [ASOS Style Match], [Fashion-specific accuracy], [Restricted to ASOS catalog only],
    [ViSenze], [API-based, good accuracy], [Paid service with recurring per-query costs],
  ),
  caption: [Comparison of commercial visual search products],
) <tbl-commercial-comparison>

These products share common limitations for independent projects: they are proprietary and cannot be studied or modified, API access incurs costs at query volume, and reliance on external services creates vendor lock-in. This thesis demonstrates that comparable functionality is achievable with open-source tools, providing both a reference implementation and a cost-effective alternative for smaller deployments.

=== Contribution Differentiators

This project distinguishes itself from prior work by addressing the *engineering gap* between model research and production systems. Four contributions define this gap:

*1. Polyglot architecture.* Python's machine learning ecosystem (PyTorch, HuggingFace) does not natively interoperate with the .NET stack common in enterprise e-commerce. This thesis presents a modular monolith with a dedicated AI sidecar, combining .NET's type safety and transactional integrity with Python's access to state-of-the-art vision models, without the operational overhead of a full microservices deployment.

*2. Vector-native consistency.* By using pgvector within PostgreSQL, embeddings and product metadata share the same transactional boundary. Product updates, image replacements, and index maintenance occur atomically, eliminating stale-index bugs that arise when a vector store and relational database have independent consistency guarantees.

*3. Commodity hardware benchmarking.* Commercial visual search runs on cloud TPU clusters. This thesis evaluates 11 models on consumer-grade hardware, establishing that production-quality visual search is achievable without specialised infrastructure, lowering the barrier for small to medium e-commerce platforms.

*4. Applied model comparison.* Rather than chasing leaderboard metrics, this thesis compares models within realistic deployment constraints (inference latency budget, memory limits, storage cost). The resulting accuracy-efficiency trade-off data, presented in Chapter 5, provides a pragmatic guide for practitioners selecting embedding models.
