== FUTURE WORK

While the current specific implementation serves as a functional proof-of-concept, several avenues exist to elevate the system to a production-grade platform, specifically addressing the scope exclusions and limitations identified in Chapter 1.

=== Addressing Immediate Limitations

1. *User Experience Validation:*
  As noted in the project limitations, this thesis focused on technical metrics. A critical next step is to conduct *A/B Testing* with real users to measure the actual engagement lift provided by Visual Search compared to traditional text search.

2. *Mobile Application Integration:*
  The current "Excluded Scope" involved a mobile frontend. Future work should involve developing a *Flutter or React Native* mobile application that consumes the existing .NET 10 APIs, leveraging the device's native camera for a seamless "Snap-and-Search" experience.

3. *Production Payment Gateways:*
  The simulation of payment processing should be replaced with robust integrations for *Stripe* or *PayPal*, implementing the full webhook lifecycle for refund handling and fraud detection.

=== Medium-Term Scaling

1. *ONNX Optimization:*
  Currently, the system uses PyTorch in "Eager Mode". Converting the Fashion-CLIP weights to *ONNX Runtime* could potentially reduce the inference latency on the MX330 from 280ms to $< 150$ms by leveraging hardware-specific operator fusion.

2. *Kubernetes Orchestration:*
  The current Docker Compose setup is limited to a single node. Migrating to *Kubernetes (K8s)* would allow the Python "Inference Service" to scale independently (e.g., 5 Replicas) from the .NET API (2 Replicas), optimizing cost based on traffic patterns.

=== Long-Term Research

1. *Personalized Embeddings:*
  The current vectors are static. Future work could investigate "User Embeddings" that evolve based on clickstream data, allowing the search results to shift based on a user's personal style preferences (e.g., "Prioritize Minimalist styles").

2. *Hybrid Search (Reciprocal Rank Fusion):*
  Visual search sometimes misses specific textual attributes. Implementing a Hybrid Search (Vector + Keywoard BM25) using *Reciprocal Rank Fusion (RRF)* would combine the semantic power of vision with the precision of text.

=== Closing

The roadmap presented here moves the system from a "Static Prototype" to a "Dynamic, Scalable Engine", reflecting the continuous evolution required in modern software engineering.


