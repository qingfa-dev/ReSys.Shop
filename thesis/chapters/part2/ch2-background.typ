= Background and Related Work

This chapter establishes the technical foundations for the thesis. It introduces vector embeddings as the mathematical basis of visual search, surveys the neural architectures used to generate them, examines vector database technologies including pgvector and HNSW indexing, evaluates architectural patterns for integrating machine learning into web applications, reviews prior work in fashion image retrieval, and summarises the technology stack chosen for implementation.

== Vector Embeddings: The Mathematical Foundation

At the heart of visual search is a simple idea: turning images into lists of numbers that a computer can compare. These lists are called *vector embeddings*, sometimes *feature vectors*.

When an AI model processes an image, it outputs a fixed-length sequence of numbers representing the visual content:

```
[0.23, -0.15, 0.87, 0.42, ..., -0.31]  (512 numbers)
```

Embeddings capture the essence of an image in compressed form. Visually similar products produce similar number sequences; dissimilar products produce different ones. This transforms visual comparison into a mathematical operation.

=== The Latent Space

Embeddings can be understood as points in a high-dimensional space. A 512-dimensional vector occupies a space with 512 axes, far beyond the three spatial dimensions we can visualise. In this *latent space*:

- Similar images cluster close together
- Different images are far apart
- The word "latent" signifies that the dimensions do not correspond to obvious visual concepts like "redness" or "stripiness." They represent abstract features the model learned during training.

=== Measuring Similarity: Cosine Distance

To compare images, the system measures the angle between their embedding vectors using *cosine similarity*:

$ "cosine similarity" = (A dot B) / (||A|| times ||B||) $

Where $A$ and $B$ are the embedding vectors being compared, $A dot B$ is their dot product, and $||A||$ and $||B||$ are their Euclidean norms.

Cosine similarity ranges from +1.0 (vectors point in the same direction, very similar) to 0.0 (perpendicular, unrelated) to -1.0 (opposite directions, very dissimilar). For fashion images, values above 0.7 typically indicate visual similarity a user would recognise. The key advantage is that the same mathematical operation works for any image, any category, without the system needing to know what makes a "dress" or a "shoe."

// Diagram placeholder: Visualisation of cosine similarity in 2D vector space
// #figure(image("images/diagrams/cosine-similarity.png", width: 70%), caption: [...])

=== The CBIR Pipeline

Content-Based Image Retrieval (CBIR) replaces text queries with image queries through the following pipeline:

1. The user uploads or selects a query image
2. The system generates an embedding for that image using a deep learning model
3. The system compares the query embedding against all product embeddings in the database using cosine distance
4. Results are ranked by similarity and displayed to the user

This approach bypasses the need for consistent, complete textual labels. A photograph of a dress with a distinctive neckline retrieves visually similar products regardless of how the catalog describes them. The embedding becomes a universal description that captures shape, texture, colour, and pattern automatically.

// Diagram placeholder: CBIR pipeline overview (Mermaid flowchart)
// #figure(image("images/diagrams/01-cbir-pipeline.png", width: 90%), caption: [...])

== Deep Learning Architectures for Embedding Generation

Generating useful embeddings requires models that extract features at multiple levels, from low-level textures to high-level garment structure. Three families of architectures have emerged over the past decade.

=== Convolutional Neural Networks

Convolutional neural networks (CNNs) process images through a hierarchy of learned filters. Early layers detect simple patterns (edges, colour transitions, texture directions), middle layers compose these into shapes and parts (lapels, button rows, sleeve boundaries), and deep layers recognise complete structures (dress vs. jacket, formal vs. casual). This layered organisation mirrors aspects of biological vision and has proven remarkably effective for visual recognition @he2016deep.

*ResNet* (Residual Network) introduced skip connections that allow information to bypass layers, solving the degradation problem that plagued deeper networks. ResNet-50, the 50-layer variant used in this thesis, remains a strong baseline for image retrieval with 25.6 million parameters and 2,048-dimensional embeddings.

*EfficientNet* uses compound scaling to simultaneously balance network depth, width, and input resolution @tan2019efficientnet. EfficientNet-B0, the smallest variant evaluated here, achieves competitive accuracy with 5.3 million parameters and produces 1,280-dimensional embeddings. Its compact design makes it well-suited to CPU-only or memory-constrained deployments.

#figure(
  table(
    columns: (auto, auto, auto, auto, auto),
    align: center + horizon,
    table.header([*Family*], [*Model*], [*Parameters*], [*Embedding Dim*], [*Training Data*]),
    [CNN], [ResNet-50], [25.6M], [2048], [ImageNet (1.2M images)],
    [CNN], [ResNet-101], [44.5M], [2048], [ImageNet (1.2M images)],
    [CNN], [EfficientNet-B0], [5.3M], [1280], [ImageNet (1.2M images)],
    [CNN], [EfficientNet-B4], [19.3M], [1792], [ImageNet (1.2M images)],
  ),
  caption: [CNN-based models evaluated in this thesis],
) <tbl-cnn-models>

=== Vision Transformers

While CNNs capture local patterns through their layered filter design, their reliance on small receptive fields (typically 3×3 pixel windows) limits their ability to model relationships between distant image regions. Vision Transformers (ViTs) address this by applying the *self-attention* mechanism that was originally developed for natural language processing to image data @dosovitskiy2020vit.

A ViT divides an image into a grid of fixed-size patches (typically 16×16 pixels), treats each patch as a "token" analogous to a word in a sentence, and passes the sequence through transformer layers. Self-attention computes pairwise relationships between all patches simultaneously. For fashion, this means a ViT can relate a collar detail in one corner to a hemline pattern at the opposite edge without needing many intermediate layers, a capability especially valuable for garment retrieval where global silhouette matters as much as local texture.

*DINOv2*, developed by Meta AI, takes a different approach: rather than training on human-labelled images, it uses self-supervised learning on a large, uncurated collection of images @oquab2023dinov2. The model learns visual features by solving a prediction task on its own representations, without ever seeing a category label. DINOv2 produces 384-dimensional embeddings (ViT-S variant) or 768-dimensional embeddings (ViT-B variant). Its self-supervised training makes it adaptable to domains where curated labels are scarce.

#figure(
  table(
    columns: (auto, auto, auto, auto, auto),
    align: center + horizon,
    table.header([*Family*], [*Model*], [*Parameters*], [*Embedding Dim*], [*Training Method*]),
    [ViT], [DINOv2 ViT-S/14], [21M], [384], [Self-supervised (142M images)],
    [ViT], [DINOv2 ViT-B/14], [86M], [768], [Self-supervised (142M images)],
    [ViT], [CLIP ViT-B/32], [151M], [512], [Contrastive (400M pairs)],
    [ViT], [CLIP ViT-B/16], [150M], [512], [Contrastive (400M pairs)],
    [ViT], [CLIP ViT-L/14], [428M], [768], [Contrastive (400M pairs)],
  ),
  caption: [Vision Transformer models evaluated in this thesis],
) <tbl-vit-models>

=== CLIP and Fashion-CLIP: Bridging Vision and Language

CNNs and ViTs operate purely in the visual domain, mapping images to embedding spaces that have no connection to human language. *CLIP* (Contrastive Language-Image Pre-training) bridges this gap through a dual-tower architecture: one tower encodes images, a parallel text encoder processes natural language descriptions, and both are trained jointly on 400 million (image, caption) pairs from the public web @radford2021learning. A contrastive objective pulls matching image-text pairs together in a shared embedding space while pushing non-matching pairs apart. The result is a model that can both see and read.

*Fashion-CLIP* extends CLIP by fine-tuning it on over 700,000 fashion images paired with domain-specific text descriptions @chia2022fashionclip. The fine-tuning adjusts model weights to emphasise fashion-relevant attributes (garment categories, fabric textures, style descriptors, occasion labels) while retaining general visual understanding. Fashion-CLIP uses the ViT-B/16 architecture inherited from CLIP, producing 512-dimensional embeddings. The original paper reports a 15 to 20% improvement on fashion retrieval over general CLIP, a result confirmed in the benchmark evaluation presented in Chapter 5 of this thesis.

The dual-tower design also enables *multimodal queries* unavailable in pure vision models like DINOv2 or EfficientNet. A user searching for "red floral summer dress" does not need a reference image; the text encoder maps the description directly into the same embedding space as catalog images. A hybrid query combining an uploaded photo with a textual refinement, "like this, but in blue," becomes possible by encoding both modalities and merging the results.

#figure(
  table(
    columns: (auto, auto, auto, auto),
    align: center + horizon,
    table.header([*Model*], [*Architecture*], [*Training*], [*Domain*]),
    [CLIP ViT-B/32], [Dual-tower (ViT + text transformer)], [Contrastive (400M image-text pairs)], [General],
    [CLIP ViT-B/16], [Dual-tower (ViT + text transformer)], [Contrastive (400M image-text pairs)], [General],
    [CLIP ViT-L/14], [Dual-tower (ViT + text transformer)], [Contrastive (400M image-text pairs)], [General],
    [Fashion-CLIP], [Dual-tower (ViT + text transformer)], [Contrastive, fine-tuned on 700K fashion images], [Fashion-specific],
  ),
  caption: [CLIP variants evaluated in this thesis],
) <tbl-clip-models>

== Vector Databases and Approximate Search

Once images are converted to embeddings, those vectors must be stored and queried efficiently. For a catalog of $n$ products, a naive brute-force search requires $n$ distance computations per query. At 10,000 items this is manageable on modern hardware; at millions it becomes impractical.

=== Approximate Nearest Neighbour Search

The solution is *Approximate Nearest Neighbour* (ANN) search. Rather than exhaustively computing the distance to every stored vector, ANN algorithms use index structures that organise vectors into navigable graphs or trees. A query navigates directly toward the neighbourhood of likely matches, skipping the vast majority of irrelevant vectors. The accuracy trade-off is modest: typically 97 to 99% recall of the true top matches, for a speed improvement of several orders of magnitude. For product search, where returning 20 visually similar items matters far more than guaranteeing the absolute 21st best match, this trade-off is entirely acceptable.

=== HNSW: Hierarchical Navigable Small World

The HNSW index is among the most widely adopted ANN algorithms. It constructs a multi-layered graph where each layer contains a subset of vectors connected by edges to their nearest neighbours. Top layers are sparse and enable long-range jumps across the embedding space; bottom layers are dense and refine the search locally. A query begins at the top layer, descends through progressively finer graphs, and converges on the neighbourhood of the query vector. The search complexity scales logarithmically with the number of vectors, making HNSW suitable for interactive applications where query latency must remain under tens of milliseconds.

=== pgvector: PostgreSQL with Vector Search

This project uses *pgvector*, an open-source PostgreSQL extension that implements HNSW-indexed vector storage and similarity search within the standard relational database. The key practical advantage is transactional consistency: because vectors and product metadata live in the same database, a product update and its embedding update occur within a single ACID transaction. This eliminates the *dual-database problem*, where a separate vector store can drift out of sync with the relational source of truth, producing stale search results.

Queries combine vector similarity with relational filtering in standard SQL: find products visually similar to a query image, but restrict results to a specific category and price range, using a single query plan. The extension supports variable-length vectors, accommodating the different embedding dimensions produced by different models (384 for DINOv2-S, 512 for Fashion-CLIP, 768 for DINOv2-B, 1280 for EfficientNet-B0, 2048 for ResNet-50).

#figure(
  table(
    columns: (auto, auto, 1fr),
    align: (start, start, start),
    table.header([*Property*], [*Value*], [*Rationale*]),
    [Extension], [pgvector 0.8+], [Open-source, zero additional infrastructure],
    [Index type], [HNSW], [Logarithmic search complexity, good recall-speed balance],
    [Distance metric], [Cosine], [Bounded range, interpretable thresholds for fashion similarity],
    [Model metadata], [model_name, model_version columns], [Enables per-model filtering and A/B testing],
  ),
  caption: [pgvector configuration used in this thesis],
) <tbl-pgvector-config>

== E-commerce Platform Architectures

Having covered how embeddings are generated and stored, this section examines the architectural patterns that organise the ReSys.Shop platform around these capabilities. Modern web applications are shaped by three architectural patterns, each trading off simplicity, scalability, and operational cost.

=== Monolith

A monolith packages the user interface, business logic, and data access into a single deployable unit. Development is straightforward: one codebase, one build pipeline, one deployment. At small scale this works well. As the system grows, subsystems accumulate coupling. Changing the checkout flow requires understanding the catalog module; deploying a payment fix means redeploying the entire application. The monolith does not scale with team size or codebase age.

=== Microservices

Microservices decompose an application into independently deployable services, each owning a discrete business capability. Teams can work in parallel using different technology stacks per service. The trade-off is operational complexity: service discovery, inter-service authentication, network latency, partial failure modes, and distributed transaction management. For a system where the primary research contribution lies in machine learning integration rather than infrastructure engineering, this overhead is disproportionate.

=== Modular Monolith

The modular monolith occupies the middle ground. Code is organised into logically isolated business modules within a single process. Compile-time boundaries prevent direct cross-module references, preserving the logical independence of bounded contexts. There is one build, one deployment, and one shared database. The nine business modules in ReSys.Shop (Catalog, Ordering, Payment, Inventory, Identity, Profile, Shipping, Location, Dashboard) communicate through an in-process message bus with no namespace-level dependencies between them. A shared PostgreSQL instance allows relational product data and vector embeddings to coexist within the same transactional boundary, maintaining consistency between catalog updates and search index changes.

The machine learning capability is the one exception to the single-process rule. It runs as a dedicated Python sidecar service, isolated because PyTorch and the broader Python scientific stack have incompatible runtime requirements with .NET. The sidecar communicates with the main application over HTTP, exposing a narrow embedding-generation interface while keeping GPU resource contention isolated from the e-commerce API.

=== Architectural Decision

ReSys.Shop adopts the modular monolith with a machine learning sidecar. The decision is guided by three trade-offs:

- *Deployment.* One process for the core application avoids service discovery, inter-service authentication, and distributed transaction orchestration. The Python sidecar runs as a separate process because PyTorch and .NET have incompatible runtime environments, but the sidecar exposes a narrow HTTP interface restricted to embedding generation. A GPU failure in the sidecar does not affect e-commerce API availability.

- *Data consistency.* A single PostgreSQL instance hosts both relational product data and pgvector embeddings. Catalog updates and embedding index changes share the same transactional boundary, eliminating the class of stale-index bugs that arise when a vector store and relational database drift out of sync.

- *Module boundaries.* Nine business modules (Catalog, Ordering, Payment, Inventory, Identity, Profile, Shipping, Location, Dashboard) are isolated by namespace convention within one assembly. Inter-module communication uses an in-process message bus. There are no direct cross-module references at compile time, preserving bounded-context independence without the operational cost of separate deployment units.

// Diagram placeholder: Three architecture patterns side-by-side (Mermaid)
// #figure(image("images/diagrams/arch-patterns.png", width: 90%), caption: [Monolith, modular monolith, and microservices compared.])

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

== Technology Stack

#figure(
  table(
    columns: (auto, auto, 1fr),
    align: (start, start, start),
    table.header([*Layer*], [*Technology*], [*Role*]),
    [Frontend], [Vue 3, TypeScript, Vite], [Customer storefront and admin panel; reactive UI with Pinia state management],
    [Backend API], [.NET 10, Carter, MediatR], [REST endpoints via minimal APIs; CQRS command-query separation across business modules],
    [Database], [PostgreSQL, pgvector], [Relational data and vector embeddings in a single ACID database with HNSW-indexed similarity search],
    [Caching], [Redis, HybridCache], [Two-tier cache (in-memory L1 + Redis L2); Hangfire job queue and session state backing store],
    [ML Sidecar], [Python 3.12, FastAPI, PyTorch], [Dedicated embedding generation service with lazy model loading and GPU acceleration],
    [Orchestration], [.NET Aspire], [Container lifecycle management, service discovery, and reproducible local development],
    [Background Jobs], [Hangfire], [Persistent job processing for cart expiry, embedding queue, and maintenance tasks],
    [Authentication], [JWT, ASP.NET Identity], [Short-lived access tokens with refresh rotation; role and permission-based authorisation],
  ),
  caption: [Technology stack of the ReSys.Shop platform],
) <tbl-tech-stack>

Each technology addresses a specific architectural requirement. Vue 3 and Vite provide modern frontend tooling with fast development cycles. .NET 10 offers the strong type system and high-throughput web server needed for transactional e-commerce logic. PostgreSQL, via pgvector, consolidates relational and vector data management into one well-understood database, avoiding the complexity of operating a separate vector store. Redis layers in caching and transient state. The Python sidecar exists specifically to host PyTorch, the standard framework for pre-trained deep learning models, isolated so that an inference resource spike cannot affect e-commerce API availability. .NET Aspire orchestrates these components into a reproducible containerised environment. Hangfire persists background jobs in Redis for resilience across restarts. JWT-based authentication with refresh token rotation follows current best practices for securing browser-based single-page applications.
