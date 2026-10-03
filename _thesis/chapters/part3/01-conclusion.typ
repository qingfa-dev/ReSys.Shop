== CONCLUSION

The primary goal of this project was to build a domain-specific visual search system for fashion e-commerce that bridges the gap between advanced deep learning and practical software engineering. By integrating a *Vertical Slice Architecture* with a lightweight *Fashion-CLIP* inference engine, the thesis successfully demonstrated that semantic search capabilities can be democratized—running efficiently on commodity hardware without relying on expensive SaaS providers.

=== Summary of Achievements

The system successfully met the technical objectives established at the outset of this thesis:

1. *Polyglot System Architecture:* A robust microservices-based integration was established, where a .NET Core transactional backend communicates seamlessly with a Python-based AI service via asynchronous HTTP/REST patterns.
2. *Vector Search Feasibility:* The implementation verified that `pgvector` (within a standard PostgreSQL container) provides sufficient performance (sub-30ms retrieval) for catalogs up to 5,000 items, sustaining real-time user interactivity.
3. *Production-Grade UI:* A complete Vue.js frontend was delivered, featuring "Optimistic UI" patterns, drag-and-drop visual search, and lazy-loaded recommendation carousels.

=== Answering Research Questions

The experimental results obtained from the validation phase provide direct answers to the research questions:

*RQ1: How does a fashion-specific model compare to a general-purpose model?*
*Answer:* *Superior.* The domain-specific *Fashion-CLIP* model achieved an mAP\@10 of *0.725*, significantly outperforming the general-purpose *Standard CLIP* (0.642). This confirms that for specialized domains like fashion, smaller, fine-tuned models offer better semantic relevance than larger, generic foundational models.

*RQ2: What are the trade-offs between search accuracy and processing speed?*
*Answer:* *Acceptable Latency for Higher Accuracy.* While *EfficientNet-B0* was the fastest (~32ms inference), its lower accuracy limits its utility for semantic search. *Fashion-CLIP* presented the optimal trade-off: it incurs a 2x latency cost (~60ms) compared to EfficientNet but delivers a 12% improvement in retrieval quality. This latency remains well within the 100ms budget for backend processing.

*RQ3: Can a microservices architecture effectively separate AI processing?*
*Answer:* *Yes.* The *Vertical Slice Architecture* proved highly effective. By isolating the "Visual Search" slice, CPU-intensive vector generation tasks were decoupled from the main thread. Even during parallel image uploads (Stress Test TC-P03), the "Checkout" and "Browsing" functions remained responsive, validating the architectural capability to isolate load.

=== Final Remarks

This thesis moves beyond theoretical model comparison to provide a deployable blueprint for modern e-commerce. It illustrates that "Intelligence" in software is not just about the model—it is about the *system* that wraps it. The provided codebase serves as a foundational reference for developers aiming to integrate multimodal AI into .NET ecosystems.
