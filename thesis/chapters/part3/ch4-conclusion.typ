= Conclusion & Future Work

This chapter brings the thesis to a close. Section 4.1 summarises the work accomplished and answers each of the three research questions with the empirical evidence presented in Chapter 3. Section 4.2 enumerates the concrete contributions of this project. Section 4.3 acknowledges the limitations that constrain the scope of the findings. Section 4.4 proposes actionable directions for future work. Section 4.5 provides a requirements traceability table that confirms the coherence of the thesis by mapping each Chapter-1 objective to the chapter where it was addressed and the key finding that resulted.

== Summary of Work

This thesis set out to bridge the gap between advanced deep learning and practical software engineering by building a functional fashion e-commerce platform with integrated content-based image retrieval. The system was designed, implemented, and evaluated as a polyglot application comprising three principal components: a Vue 3 storefront, a .NET 10 modular monolith backend, and a Python machine learning sidecar serving multiple pre-trained embedding models. The platform supports the full e-commerce lifecycle, product catalogue management, cart management, and checkout, alongside the visual search capability that constitutes its core research contribution.

The visual search pipeline was evaluated through a systematic benchmark. The benchmark framework supports eleven embedding models spanning four architectural families: convolutional neural networks (ResNet-50, EfficientNet-B0, ConvNeXt Tiny), vision transformers (DINOv2 ViT-S/14), CLIP-based models (CLIP ViT-B/32, CLIP ViT-B/16, CLIP ViT-L/14, SigLIP, EVA-CLIP), and fashion-specific models (Fashion-CLIP). Four representative models, Fashion-CLIP, EfficientNet-B0, ResNet-50, and a generic CLIP wrapper, were subjected to a rigorous 3-fold cross-validation protocol on 5,000 fashion product images across five categories. Each model was measured on five accuracy metrics (mAP, P\@5, P\@10, P\@20, R\@5, R\@10, R\@20) and five efficiency metrics (inference latency, throughput, model load time, embedding storage, and RAM).

The evaluation produced three principal findings. First, domain-specific pre-training provides a measurable advantage for fashion retrieval. Second, the accuracy-efficiency trade-off is both substantial and navigable, with architecture choice, CNN versus transformer, dominating the trade-off space. Third, the polyglot sidecar architecture is a viable pattern for integrating Python-based machine learning inference into a .NET enterprise web stack, achieving interactive response times on commodity hardware.

=== Answering the Research Questions

*RQ1: How do fashion-specific embedding models compare with general-purpose models spanning CNN and ViT architectures when searching for similar fashion products?*

Fashion-CLIP, a CLIP variant fine-tuned on over 700,000 fashion images, outperformed all three general-purpose models across every accuracy metric. Its mAP of 0.8788 is 5.4 percent above the generic CLIP ViT-B/32 (0.8341), 7.7 percent above EfficientNet-B0 (0.8158), and 8.2 percent above ResNet-50 (0.8120). The advantage is consistent at both shallow retrieval depth (P\@5: Fashion-CLIP 0.9304 vs CLIP-generic 0.9025, a 3.1 percent gap) and deeper retrieval depth (P\@20: Fashion-CLIP 0.8982 vs CLIP-generic 0.8640, a 4.0 percent gap). Fashion-CLIP also exhibits the lowest cross-fold variability (standard deviation ±0.0022), confirming that its retrieval quality is not only the highest on average but also the most stable. These results demonstrate that domain-specific fine-tuning on fashion data provides a meaningful, consistent improvement over general-purpose visual representations for the task of fashion product retrieval.

*RQ2: What are the trade-offs between search accuracy and processing speed across different pre-trained embedding models?*

The trade-off is substantial and non-linear. The most accurate model, Fashion-CLIP (mAP 0.8788), operates at 92.0 milliseconds per inference. The fastest model, EfficientNet-B0 (23.9 milliseconds per inference), achieves 92.8 percent of Fashion-CLIP's mAP (0.8158) at 26.0 percent of the latency, a 3.8-times speed advantage for a 7.7 percent accuracy cost. ResNet-50 (mAP 0.8120, 64.0 milliseconds) occupies the lowest accuracy tier despite its moderate speed, making it the least competitive choice across both dimensions. The two CLIP-based models have nearly identical latency (92.0 ms vs 92.9 ms), yet Fashion-CLIP's mAP is 5.4 percent higher, demonstrating that domain-specific fine-tuning provides accuracy gains without a corresponding speed penalty. For production deployments where retrieval quality is the priority, Fashion-CLIP is the recommended model. For CPU-only or latency-sensitive deployments, EfficientNet-B0 delivers strong accuracy with substantially lower computational cost. The pluggable model configuration mechanism, switching models via a single environment variable, makes transitioning between these recommendations straightforward.

*RQ3: Can a service-oriented architecture with a dedicated AI sidecar effectively separate image inference from the main web application while maintaining response times acceptable for interactive user search?*

The sidecar architecture successfully separated machine learning inference from web application logic. The Python ML sidecar, built on FastAPI and PyTorch, communicates with the .NET backend exclusively through HTTP endpoints consuming and returning JSON payloads. The ML service is containerised independently and orchestrated by .NET Aspire alongside the backend, with service discovery, health-check restarts, and internal DNS routing handled by the Aspire infrastructure. The separation is effective on two dimensions. Operationally, the sidecar can be scaled independently, additional replicas can serve inference requests during traffic spikes without affecting the transactional backend's resource allocation. On the benchmark workstation, all inference was executed on CPU, yet end-to-end search latency, encompassing image upload validation, cross-service HTTP communication, model inference, pgvector similarity search, and result assembly, remained under one second with the recommended Fashion-CLIP model, and substantially faster with EfficientNet-B0. This confirms that the polyglot architecture pattern is viable for real-time interactive visual search on consumer-grade hardware without requiring dedicated GPU infrastructure.

=== Achievement of Technical Objectives

The four technical objectives established in Chapter 1 were met. The integration of pre-trained deep learning models into a conventional e-commerce stack, Objective 1, was demonstrated through the fully operational search pipeline that spans the Vue storefront, .NET backend, Python sidecar, and PostgreSQL with pgvector. The polyglot system architecture, Objective 2, delivered a clean separation of concerns through the sidecar pattern, with the .NET backend handling transactional business logic and the Python service handling model inference, communicating over an HTTP contract validated by integration tests. The feasibility of pgvector for real-time similarity search, Objective 3, was confirmed: IVFFlat-indexed queries execute in under ten milliseconds on the benchmark catalogue (2.7--6.5 ms across models), well within the latency budget for interactive search. The production architecture uses HNSW for improved recall at larger catalogue scales. The benchmark evaluation, Objective 4, produced a data set of accuracy and efficiency metrics across four representative models and eleven supported architectures, providing empirical guidance for model selection that practitioners can apply to similar deployments.

== Contributions

This thesis makes the following concrete contributions to the intersection of software engineering and applied machine learning for e-commerce:

- *A four-model benchmark for fashion image retrieval.* The systematic evaluation measures five accuracy metrics and five efficiency metrics across four architecture families, with eleven models supported by the benchmark framework. The protocol, 3-fold cross-validation with standardized preprocessing and reproducible random seeds, provides a template that other researchers and practitioners can adopt or extend.

- *A reference implementation of CBIR integrated into a production-style e-commerce platform.* The ReSys.Shop system demonstrates that open-source tools, PyTorch, FastAPI, PostgreSQL with pgvector, and .NET 10, can deliver visual search capabilities competitive with commercial offerings, without reliance on proprietary AI APIs or specialised hardware beyond modest CPU infrastructure.

- *A pluggable model architecture that enables runtime model switching.* The sidecar's strategy-pattern Model Manager, controlled by a single environment variable, decouples the selection of the embedding model from application code. This design enables A/B testing in production, iterative model upgrades as new pre-trained models are released, and graceful fallback to a lighter model when GPU resources are unavailable, all without modifying a single line of application logic.

- *Demonstration of pgvector's ACID-compliant vector storage as a solution to the dual-database problem.* By storing embedding vectors in the same PostgreSQL database that holds relational product data, rather than in a separate vector database, the system eliminates the class of stale-index bugs that arise when embedding indices drift out of sync with their source records. Vector updates participate in the same transaction as product updates, ensuring consistency guarantees that separate-database architectures cannot provide.

- *A validated polyglot architecture pattern for .NET plus Python AI.* The sidecar integration, FastAPI service communicating with a .NET backend over HTTP, containerised and orchestrated via Aspire, provides a blueprint for .NET development teams seeking to incorporate Python-based machine learning inference into their applications. The pattern balances the strengths of each ecosystem: .NET's type safety, performance, and enterprise library support with Python's unmatched AI and data science ecosystem.

== Limitations

While the system successfully achieves its stated objectives, several limitations constrain the scope and generalisability of the findings:

- *Dataset representativeness.* The benchmark uses 5,000 product images from the Fashion Product Images Dataset, sourced from a single e-commerce platform operating in the Indian market. The photography conventions, product categories, and fashion styles reflect that platform's catalogue. Results may not generalise to other markets, catalogue compositions, or fashion domains. Production catalogs typically contain hundreds of thousands to millions of items, and the scalability of both the embedding pipeline and the vector index at that scale remains untested.

- *Hardware specificity.* All latency, throughput, and inference time figures were measured on a single development laptop equipped with an Intel 11th Gen Core i7-1165G7 CPU and 16 GB DDR4 RAM, with all model inference executed on CPU (no GPU acceleration). This represents a standard consumer laptop configuration. Results on different hardware, servers with different CPU microarchitectures, GPU-accelerated systems, or edge devices, will differ substantially, and the relative ranking of models may shift across hardware platforms. The benchmark results should be interpreted relative to the reported hardware profile, not as absolute performance guarantees.

- *Category-level relevance criterion.* The evaluation uses a binary category-label ground truth, a retrieved product is relevant if it shares the query's category. This is a coarse proxy for visual similarity. Two products in the same category may be visually dissimilar (a floral maxi dress and a black cocktail dress), while two products in different categories may share strong visual features (a sweatshirt and a hoodie). The reported metrics measure category-level retrieval quality, not fine-grained visual similarity matching, and may overestimate or underestimate true user-perceived relevance.

- *No user experience evaluation.* Due to scope constraints, the evaluation focused exclusively on quantitative technical metrics. Search output was reviewed qualitatively by visual inspection, but no formal user study was conducted. The relationship between measured accuracy improvements, a 4.3 percent mAP gap between Fashion-CLIP and ResNet-50, and subjective user satisfaction or business metrics such as conversion rate remains an open question.

- *Pre-trained models only.* All embedding models were used as published by their original authors, without fine-tuning on the evaluation dataset. Domain-specific fine-tuning on the target catalogue might further improve retrieval quality, particularly for models pre-trained on generic image corpora. This work evaluated the out-of-the-box performance of pre-trained models; custom training was deliberately excluded from scope.

- *Image-only modality.* The evaluation is confined to image-to-image retrieval. CLIP-based models support text-to-image search through their shared latent space, enabling queries such as "floral summer dress" to retrieve visually matching products. This multimodal capability was not evaluated. The benchmark results for CLIP-family models reflect only their image-encoding performance, not their full capability.

- *K-value selection and labelling scheme.* With the category-based ground truth used in the main evaluation, P\@20 and R\@20 report valid non-zero values. However, the enriched-label evaluation in Appendix A.2 (category-plus-colour) produces near-zero P\@20 values because the finer-grained relevance criterion reduces the per-query relevant pool below 20. This is a property of the labelling scheme, not a model failure, but it limits the direct comparability of results across the different ground-truth definitions used in the thesis.

- *Memory measurement limitations.* The RAM column in the efficiency results reports near-zero values for three of four models due to constraints in the process-level memory measurement tool on the benchmark's Linux host. Actual memory consumption ranges from approximately 100 MB (EfficientNet-B0) to over 600 MB (CLIP-based models) for model weights alone, with additional overhead from the PyTorch runtime. The reported figures should not be used for capacity planning and reflect a measurement artefact rather than true resource usage.

== Future Work

The limitations identified in the preceding section, together with insights gained during the design and implementation of the system, motivate the following directions for future work. The items are prioritised from most actionable to most ambitious.

*1. Fine-tune Fashion-CLIP on the target catalogue.* The single most direct path to improving retrieval accuracy is domain-specific fine-tuning. Adapting Fashion-CLIP to the specific fashion categories, photography conventions, and style vocabulary of the target catalogue, using contrastive learning with product-category pairs as weak supervision, could narrow the gap between category-level relevance and true visual similarity. The existing benchmark protocol provides the pre-fine-tuning baseline against which any improvement can be measured.

*2. Conduct a user experience study with A/B testing.* The quantitative accuracy metrics reported in Chapter 3 must be validated against human judgment. A controlled A/B test, assigning half of storefront visitors to text-only search and half to visual search, would measure the actual engagement lift provided by CBIR. Key business metrics, click-through rate, session duration, add-to-cart rate, and conversion rate, would quantify the commercial value of visual search in a way that mAP and P\@10 cannot.

*3. Implement multi-modal search combining text and image queries.* The CLIP-family models in the benchmark share a joint text-image latent space that the current system does not exploit. Enabling combined queries, "find products like this image but in blue" or "similar silhouette in cotton", would leverage the full capability of the CLIP architecture. This requires extending the search endpoint to accept an optional text prompt alongside the query image and using CLIP's text encoder to refine the similarity computation.

*4. Scale the benchmark to production-size catalogues.* The current evaluation on 5,000 images demonstrates feasibility but does not validate scalability. Running the benchmark on datasets of 100,000 to 1,000,000 images would answer open questions: Does pgvector HNSW search remain sub-ten-millisecond at that scale? Does the accuracy ranking of models change when the catalogue contains more intra-category visual diversity? Do some models degrade gracefully while others collapse? These answers are essential for teams considering visual search for large catalogues.

*5. Investigate ONNX Runtime optimisation.* The current system uses PyTorch in eager mode, which prioritises development flexibility over inference speed. Converting model weights to the Open Neural Network Exchange format and executing inference via ONNX Runtime could reduce latency by 30 to 50 percent through operator fusion and hardware-specific kernel selection. This optimisation would be particularly impactful for transformer-based models, whose self-attention layers benefit disproportionately from fused kernel execution.

*6. Add personalised re-ranking to search results.* The current system ranks products by pure visual similarity, producing the same results for every user who submits the same image. Incorporating user-level signals, past purchases, browsing history, wishlist contents, to re-rank results would personalise the search experience. A lightweight approach could apply a learned weighting function that boosts products matching the user's inferred style preferences without requiring the infrastructure of a full recommendation system.

*7. Develop a mobile application with on-device inference.* The excluded scope of this thesis included a mobile frontend. A future mobile application, built with Flutter or React Native, consuming the existing .NET APIs, could use the device camera for a "snap-and-search" experience. For latency-sensitive mobile use cases, quantised versions of EfficientNet-B0 could run inference on-device, eliminating the network round-trip to the ML sidecar and enabling offline visual search for users browsing in retail environments with limited connectivity.

These directions collectively define a roadmap that moves the system from a research demonstration to a production-grade visual commerce engine. Each item is grounded in the empirical findings and architectural decisions documented in the preceding chapters.

== Requirements Traceability

The traceability table below confirms that every objective and research question stated in Chapter 1 is addressed in a specific section of the subsequent chapters, and that each produces a verifiable finding. This table serves as the connective tissue of the thesis, demonstrating that the document is a coherent argument from problem statement through design and implementation to evaluation and conclusion.

#figure(
  table(
    columns: (2fr, 2fr, 4fr),
    stroke: 0.5pt,
    align: (left, left, left),
    table.header([*Objective / RQ*], [*Addressed In*], [*Key Finding*]),
    [Integrate pre-trained deep learning models into a conventional e-commerce stack],
    [Chapter 2, Sections 2.2.3--2.2.4, 2.3.2--2.3.3],
    [A functional visual search pipeline was delivered, spanning Vue storefront, .NET backend, Python ML sidecar, and PostgreSQL pgvector. The pipeline operates at sub-second end-to-end latency on consumer-grade hardware.],
    [Architect a polyglot system bridging .NET and Python ML],
    [Chapter 2, Sections 2.2.3--2.2.4, 2.3.1--2.3.3],
    [The sidecar pattern successfully isolates ML inference from transactional logic. The .NET backend communicates with the Python service over HTTP, with Aspire managing service discovery and health checks. Integration tests validate the cross-service contract.],
    [Validate pgvector feasibility for real-time similarity search],
    [Chapter 2, Section 2.2.4, Section 2.3.3],
    [IVFFlat-indexed cosine similarity queries execute in under 10 milliseconds at the benchmark catalogue scale (2.7--6.5 ms). Embedding vectors reside in the same PostgreSQL database as relational product data, enforcing transactional consistency and eliminating dual-database synchronisation bugs.],
    [Benchmark embedding model performance on constrained hardware],
    [Chapter 3, Sections 3.2--3.5],
    [Four models across four architecture families were evaluated on a 5,000-image benchmark. Fashion-CLIP delivers the highest accuracy (mAP 0.8788); EfficientNet-B0 delivers the best efficiency (23.9 ms per inference). Deployment recommendations are provided for different operational contexts.],
    [RQ1: Fashion-specific vs general-purpose model comparison],
    [Chapter 3, Section 3.3; Chapter 3, Section 3.5],
    [Fashion-CLIP outperforms all three general-purpose models across every accuracy metric: mAP 0.8788 vs 0.8120--0.8341. Domain-specific fine-tuning provides a consistent 5.4--8.2 percent mAP improvement. Fashion-CLIP also exhibits the lowest cross-fold variability (±0.0022).],
    [RQ2: Accuracy vs speed trade-offs],
    [Chapter 3, Sections 3.3--3.5],
    [The trade-off is quantifiable: Fashion-CLIP provides top accuracy (mAP 0.8788) at 92.0 ms; EfficientNet-B0 achieves 92.8 percent of that accuracy at 26.0 percent of the latency (23.9 ms). Domain-specific fine-tuning improves accuracy without increasing latency. ResNet-50 is the least competitive choice on both dimensions.],
    [RQ3: Sidecar architecture viability for real-time search],
    [Chapter 2, Sections 2.3.2--2.3.3; Chapter 3, Section 3.5 (synthesis)],
    [The polyglot architecture is viable. The sidecar handles model inference at 23.9--92.9 ms per image depending on model, while the .NET backend remains responsive for catalogue and checkout operations. End-to-end search latency remains under one second. Independent scaling and fault isolation are achieved without the operational overhead of a full microservices deployment.],
    [Build AI service],
    [Chapter 2, Section 2.3.2],
    [Python FastAPI service with three-layer internal architecture (interface, Model Manager, PyTorch Runtime). Lazy-loading reduces cold-start memory pressure. Exposes POST /embeddings and GET /health endpoints, containerised via Docker and orchestrated by Aspire.],
    [Set up vector search],
    [Chapter 2, Section 2.2.4, Section 2.3.3],
    [pgvector configured with cosine similarity on the embedding column. IVF-Flat queries execute in under 10 milliseconds. Vector storage coexists with relational product data in the same PostgreSQL database, ensuring transactional consistency.],

    [Connect the services],
    [Chapter 2, Sections 2.3.2--2.3.3],
    [The .NET CBIR handler orchestrates the full pipeline: client-side validation, server-side magic-byte verification, cross-service HTTP to the ML sidecar with API-key authentication, pgvector similarity query with model-name filtering, and result deduplication with similarity-score conversion.],
    [Create the user interface],
    [Chapter 2, Sections 2.3.3 (flow) and 2.3.5 (supporting modules)],
    [Vue 3 storefront with drag-and-drop image upload, similarity score badges, and product-card result display. Client-side file-type and size validation reduces server load. Results are rendered as a grid with thumbnail images and navigable product links.],
    [Evaluate the results],
    [Chapter 3, Sections 3.2--3.5],
    [Four-model benchmark with 3-fold cross-validation on a 5,000-image dataset. Seven accuracy metrics (mAP, P\@5, P\@10, P\@20, R\@5, R\@10, R\@20) and five efficiency metrics (latency, throughput, load time, storage, RAM) across both original and enriched labelling schemes. Deployment recommendations provided.],
  ),
  caption: [Requirements traceability: mapping from Chapter 1 objectives and research questions to the chapters where they are addressed, with the key finding that confirms each objective was met.],
  kind: table,
)

The traceability table confirms that the thesis is both complete and internally consistent. Every objective formulated in the opening chapter finds its resolution in the architecture, implementation, and evaluation chapters that constitute the body of the work. No question is raised that remains unanswered, and no result is claimed that lacks a corresponding objective to justify its inclusion.

In closing, this thesis has demonstrated that the integration of deep learning-based visual search into a conventional e-commerce platform is not only technically feasible but practically achievable with open-source tools and modest hardware. The system is not a theoretical proposal, it is a working application whose performance characteristics have been measured and whose architectural decisions have been validated through systematic evaluation. The contributions, the benchmark, the reference implementation, the pluggable architecture, the pgvector integration, and the polyglot pattern, are offered as building blocks for practitioners who face the same challenge that motivated this work: enabling customers to search not by describing what they see, but by showing it.
