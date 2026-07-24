== Objectives

=== Primary Objectives

+ *Design and implement a modular monolith* with 8 self-contained business modules (Catalog, Identity, Inventory, Location, Ordering, Payment, Profile, Shipping) that communicate exclusively via in-process message dispatch, enforcing zero direct cross-module references.

+ *Apply a vertical-slice architecture* where each feature action (e.g., "Create Product", "Checkout Cart") is cohesively implemented in a single folder containing its handler, endpoint, request, response, and validator.

+ *Integrate Content-Based Image Retrieval (CBIR)* via a dedicated Python sidecar supporting multiple pretrained embedding models (Fashion-CLIP, ResNet-50, EfficientNet-B0, CLIP-generic), storing embeddings in PostgreSQL pgvector with a pluggable model interface.

+ *Enforce explicit error handling* through a `Result<T>` / `Error` type system that eliminates exceptions for control flow, making all failure paths explicit and traceable.

+ *Conduct comparative ML evaluation* to measure retrieval effectiveness (Precision\@K, Recall\@K, mAP) and operational performance (embedding generation time, query latency, storage footprint) across 4 embedding models on a fashion ground-truth dataset.

=== Secondary Objectives

+ Provide dual-channel frontends (Admin SPA and Storefront SPA) with distinct UI libraries optimized for their respective user roles.

+ Implement multi-provider abstractions for storage (Local/S3), notifications (SendGrid/SMTP/Sinch), payment gateways (Stripe/Bogus), and *embedding models* to demonstrate the Strategy pattern across both infrastructure and ML domains.

+ Achieve >70% unit-test coverage for domain logic and integration tests for all critical paths (checkout, payment webhooks, auth, CBIR search).
