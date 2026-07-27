== RELATED WORK

This section compares the ReSys.Shop project with existing academic research and commercial products to understand the current state of fashion visual search and how this project contributes to the field.

=== Academic Research in Fashion Retrieval

Visual search for fashion has been an active research area for over a decade. Key developments include:

==== DeepFashion Dataset

The *DeepFashion* dataset @liu2016deepfashion, with over 800,000 fashion images, established benchmarks for fashion recognition and retrieval. It provided:
- Attribute annotations (color, pattern, category)
- Landmark annotations (collar, sleeve, hemline positions)
- Pairs of matching in-shop and consumer photos

This dataset enabled much of the subsequent research in fashion AI.

==== Conversational Fashion Retrieval

More recent work has explored combining images with text feedback. *FashionIQ* introduced the task of modifying retrieval based on natural language (e.g., "like this dress but shorter"). This requires understanding both images and text modifications.

While interesting, this approach was beyond the scope of this project due to its complexity.

==== Pre-trained Foundation Models

Recent trends favor using pre-trained models like CLIP rather than training from scratch. Fashion-CLIP @chia2022fashionclip showed that domain-specific fine-tuning of CLIP improves fashion retrieval by 15-20% over general CLIP.

This project follows this approach: using pre-trained Fashion-CLIP rather than training new models.

=== Commercial Visual Search Systems

Several companies have deployed visual search at scale:

#figure(
  table(
    columns: (auto, 1fr, 1fr),
    stroke: 0.5pt,
    align: (center, left, left),
    [*Product*], [*Strengths*], [*Limitations for This Project*],
    [Google Lens], [Massive scale, general purpose], [Closed ecosystem, not customizable],
    [Pinterest Lens], [600M+ monthly searches, style-aware], [Proprietary, requires Pinterest integration],
    [ASOS Style Match], [Fashion-specific accuracy], [Only for ASOS catalog],
    [ViSenze], [API available, good accuracy], [Paid service, recurring costs],
  ),
  caption: [Comparison of commercial visual search products],
)

These products are impressive but share common limitations for smaller projects:
- *Proprietary:* Cannot be studied or modified
- *Expensive:* API access costs money per query
- *Vendor lock-in:* Dependent on external service availability

This project demonstrates that similar functionality can be achieved with open-source tools, providing a reference implementation and cost-effective solution for smaller applications.

=== Technical Positioning and Contribution

This project distinguishes itself from existing literature by addressing the *engineering gap* between theoretical AI models and production-grade software. While typical research focuses on optimizing metric scores (e.g. mAP), this thesis contributes a reference architecture for *operationalizing* those models.

==== 1. Polyglot Vertical Slice Architecture
Most open-source implementations force a choice between a monolithic python web stack (Django/Flask) or a complex microservices mesh. This project introduces a *Distributed Vertical Slice* pattern that:
- Leverages *.NET 10* for strict type safety, high-performance concurrency, and domain logic integrity in the transactional core.
- Isolates *Python 3.12* solely for tensor computations (PyTorch), connected via a resilient HTTP/gRPC bridge.
- *Differentiation:* This provides the best-of-both-worlds: enterprise-grade backend reliability with access to the bleeding-edge AI ecosystem.

==== 2. Vector-Native Data Consistency
A common pitfall in visual search is the "dual-database problem," where vector data (Simulated in a Chroma/Pinecone instance) drifts from the relational source of truth (SQL).
- *Contribution:* This implementation utilizes *pgvector* to enforce ACID transactions across both relational entities and vector embeddings.
- *Impact:* Product updates and index re-calculations occur in the same atomic transaction scope, eliminating the class of "stale index" bugs common in distributed systems.

==== 3. Feature Parity on Commodity Hardware
Commercial solutions (Google Lens) rely on massive cloud TPU clusters. This project demonstrates that *Fashion-CLIP* (ViT-B/16) can be effectively served on commodity hardware (NVIDIA MX330 / Standard CPU) with sub-100ms latency.
- *Result:* This effectively lowers the barrier to entry for SME e-commerce platforms to adopt Generative AI and Semantic Search features without recurring cloud API costs.

==== 4. Applied Evaluation of Foundation Models
Moving beyond the "leaderboard" mentality, this thesis compares *EfficientNet*, *DINOv2*, and *Fashion-CLIP* specifically within the constraints of a real-time web application.
- *Key Finding:* We demonstrate that while DINOv2 offers superior raw geometric matching, its heavy structure makes it less viable for CPU-bound environments than the optimized Fashion-CLIP, providing a pragmatic guide for model selection in resource-constrained environments.

