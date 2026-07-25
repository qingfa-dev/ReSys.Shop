= Background and Related Work

== E-commerce Platform Architectures

Modern web applications organise code along a spectrum from tightly coupled monoliths to fully distributed microservices. At one end, a *monolithic* architecture bundles all components into a single deployable unit: simple to develop initially, but prone to accumulating subsystem coupling as the codebase grows. At the other end, *microservices* decompose the application into independently deployable services, each owning a discrete business function. This enables parallel team workflows and heterogeneous technology stacks, at the cost of distributed data management, network latency, and complex deployment pipelines.

The *modular monolith* occupies the middle ground. Code is organised into logically isolated business modules within a single process, sharing a common data store while enforcing compile-time boundaries that prevent cross-module coupling. This thesis adopts the modular monolith pattern for ReSys.Shop because a single-process architecture eliminates service discovery and distributed transaction overhead (disproportionate for a system whose research contribution lies in machine learning integration), a shared PostgreSQL database keeps relational product data and vector embeddings within the same transactional context, and business modules remain independently testable through namespace-level isolation.

The one component that runs as a separate process is the *Python machine learning sidecar*, isolated due to its distinct technology stack (PyTorch, FastAPI) and resource profile (GPU memory, model loading). This sidecar pattern preserves the operational simplicity of a single deployable for the core application while giving the ML workload the runtime environment it requires.

// DIAGRAM: System architecture overview — modular monolith with Python sidecar (PlantUML, to be added)

== Visual Search and Content-Based Image Retrieval

*Content-Based Image Retrieval* (CBIR) replaces text queries with direct visual comparison. Instead of matching keywords to product labels, CBIR encodes images into dense numerical vectors, called *embeddings*, and measures similarity between them mathematically.

An embedding is a fixed-length sequence of numbers that captures the visual essence of an image. When a convolutional or transformer-based model processes an image of a red dress, it outputs a vector such as `[0.23, -0.15, 0.87, ..., -0.31]` (typically 512 numbers). Images with similar visual content produce vectors that are close together in the embedding space; dissimilar images produce vectors far apart.

*Cosine similarity* is the primary distance metric used in this work. It measures the angle between two vectors, producing a score from -1 (opposite) to +1 (identical), with values above 0.7 typically indicating visual similarity recognisable to users. The formula is:

$ "similarity" = cos(theta) = (A dot B) / (||A|| times ||B||) $

The key advantage of embedding-based search over keyword search is flexibility. A rule-based system must explicitly encode every relevant visual attribute (colour, pattern, silhouette). An embedding model learns which features matter from training data, and mathematical comparison handles the rest. This approach also enables *multimodal queries*: if a model maps both images and text into the same embedding space, a user can search using a photo, a text description, or both simultaneously.

// DIAGRAM: CBIR pipeline overview — image upload to ranked results (Mermaid, to be added)

== Deep Learning Models for Fashion Image Retrieval

Generating useful embeddings from fashion product images requires models capable of extracting features at multiple levels of abstraction: from low-level textures and edges to high-level concepts like garment category, silhouette, and style. Three families of neural architectures have shaped this capability, appearing in roughly chronological order.

=== Convolutional Neural Networks (2012–2019)

CNNs process images through a hierarchy of learned filters. Early layers detect edges, colour gradients, and texture orientation; deeper layers compose these primitives into complex structures such as fabric patterns and garment silhouettes. Two CNN architectures are evaluated in this thesis:

- *ResNet* (He et al., 2016) @he2016deep introduced residual connections: shortcut paths that allow gradients to flow directly through very deep networks. This solved the vanishing gradient problem and enabled training of networks with 50, 101, or 152 layers. ResNet-50, pre-trained on the ImageNet dataset (1.2 million images across 1,000 categories), serves as a strong general-purpose visual feature extractor.
- *EfficientNet* (Tan and Le, 2019) @tan2019efficientnet introduced compound scaling: simultaneously adjusting network depth, width, and input resolution using a principled scaling coefficient. This produces a family of models (B0 through B7) that achieve state-of-the-art accuracy with an order of magnitude fewer parameters than comparably accurate ResNet variants. EfficientNet-B0, the smallest member of the family, is evaluated as the lightweight CNN baseline.

=== Vision Transformers (2020–present)

While CNNs capture local patterns through layered filters, their limited receptive field (typically 3×3 or 5×5 pixel windows) constrains their ability to model relationships between distant image regions. *Vision Transformers* (ViTs) address this by applying the self-attention mechanism to image data @dosovitskiy2020vit.

A ViT divides an image into a grid of fixed-size patches (typically 16×16 pixels), treats each patch as a token analogous to a word in a sentence, and processes the sequence through transformer encoder layers. Self-attention computes pairwise relationships between all patches simultaneously, enabling the model to capture long-range dependencies (a sleeve pattern that echoes a collar detail, a texture that repeats across a dress) without requiring information to propagate through many successive layers.

- *DINOv2* (Oquab et al., 2023) represents a self-supervised approach: the model is trained without human-labelled data, learning visual representations by predicting relationships between different augmented views of the same image. This produces strong general-purpose features that transfer well to downstream tasks without fine-tuning.

=== CLIP and Multimodal Models (2021–present)

CNNs and ViTs operate purely in the visual domain: they map images to embedding vectors, but those vectors have no connection to human language. *CLIP* (Contrastive Language-Image Pre-training), introduced by Radford et al. (2021) @radford2021learning, bridges this gap through a dual-tower design. One tower encodes images; a parallel tower encodes text. Both are trained jointly on 400 million (image, caption) pairs using a contrastive objective that pulls matching pairs together in a shared embedding space.

- *Fashion-CLIP* @chia2022fashionclip extends CLIP by fine-tuning on over 700,000 fashion images with domain-specific text descriptions (garment categories, fabric terms, style labels, occasion tags). Fashion-CLIP uses the ViT-B/16 architecture inherited from CLIP and produces 512-dimensional embeddings. The domain-specific training improves retrieval quality on fashion queries by approximately 15–20% relative to general CLIP, and the dual-tower design enables text-to-image search without requiring a reference photo.

=== Model Comparison

The thesis evaluates four representative models spanning these architectural families. Their key characteristics (architecture type, embedding dimension, training method, and domain) are described in the preceding subsections. The evaluation in Chapter 5 measures both retrieval accuracy and operational efficiency, providing empirical data on the accuracy-speed trade-offs that inform practical model selection.

== Vector Search and Databases

Once product images are converted to embeddings, those vectors must be stored and searched efficiently. A naive approach (computing the distance from the query vector to every stored vector) becomes impractical beyond a few thousand items.

*Approximate Nearest Neighbour* (ANN) search addresses this by trading a small amount of accuracy for large speed gains. Rather than exhaustively scanning all vectors, ANN algorithms use index structures that guide the search directly toward the neighbourhood of likely matches, achieving 97–99% recall of the true nearest neighbours with query times that scale logarithmically rather than linearly.

This project uses *HNSW* (Hierarchical Navigable Small World), a graph-based ANN algorithm that organises vectors into a multi-layered graph structure. Each layer is a proximity graph where nodes represent vectors and edges connect near neighbours. Search begins at the top (sparsest) layer and descends through increasingly dense layers, rapidly narrowing toward the target region.

The implementation uses *pgvector*, a PostgreSQL extension that stores vectors alongside relational data within the same database. This approach eliminates the *dual-database problem*: when a vector index lives in a separate system (Pinecone, Milvus, Weaviate), it can drift out of sync with the relational source of truth. With pgvector, product updates and embedding updates occur within a single ACID transaction. Vector search queries use standard SQL with the cosine distance operator (`<=>`), and can combine similarity search with relational filtering in a single execution plan.

// DIAGRAM: HNSW index structure — multi-layer graph with search descent (Mermaid, to be added)

== Related Work

Visual search for fashion has been studied in both academic research and industrial deployment.

*Academic foundations.* The *DeepFashion* dataset @liu2016deepfashion, containing over 800,000 fashion images with attribute and landmark annotations, established standard benchmarks for fashion recognition and retrieval. *FashionIQ* @wu2019fashioniq introduced conversational retrieval, where users refine search results through natural language feedback ("like this dress but shorter"). This multi-turn interaction paradigm is beyond the scope of this thesis, which focuses on single-turn image-based search. The shift toward *pre-trained foundation models*, exemplified by CLIP @radford2021learning and its fashion-specific variant @chia2022fashionclip, has substantially improved retrieval quality without requiring task-specific training from scratch.

*Commercial systems.* Google Lens, Pinterest Lens, and ASOS Style Match demonstrate that visual search operates at massive scale in production. However, these systems are proprietary, closed to modification, and incur per-query API costs. ViSenze offers API-based visual search as a service but similarly creates vendor lock-in. This thesis demonstrates that comparable functionality is achievable with open-source tools, providing a reference implementation for smaller platforms.

*Positioning.* This project distinguishes itself by addressing the *engineering gap* between theoretical model research and production software. While the academic literature primarily optimises metric scores, this thesis contributes:
- A polyglot architecture (.NET + Python sidecar) that gives production web applications access to the Python AI ecosystem without adopting a full microservices deployment.
- Vector-native data consistency through pgvector, keeping embeddings and product data in the same transactional scope.
- Demonstration that commodity hardware (mid-range GPU, standard CPU) suffices for production-quality fashion CBIR.
- Systematic comparison of CNN, ViT, and CLIP-based architectures within the constraints of a real-time web application, providing practical model selection guidance.

== Technology Stack

The platform spans three programming languages orchestrated through a containerised environment. Table @tbl-tech-stack summarises the principal components.

#figure(
  table(
    columns: (auto, auto, 1fr),
    stroke: 0.5pt,
    align: (left + horizon, left + horizon, left),

    table.header([*Layer*], [*Technology*], [*Role*]),

    [Frontend], [Vue 3, TypeScript, Vite],
    [Customer storefront and admin panel; reactive component model with Pinia state management.],

    [Backend API], [.NET 10, Carter, MediatR],
    [REST endpoints via minimal APIs; CQRS separation of read and write operations enforcing module boundaries.],

    [Database], [PostgreSQL, pgvector],
    [Relational data and vector embeddings in a single ACID database; HNSW-indexed similarity search via SQL.],

    [Caching], [Redis, HybridCache],
    [Multi-tier cache with in-memory L1 and Redis L2; backing store for job queues and session state.],

    [ML Sidecar], [Python, FastAPI, PyTorch],
    [Embedding generation service; GPU-accelerated inference; isolated from main application process.],

    [Orchestration], [.NET Aspire],
    [Container lifecycle management; service discovery; reproducible local development environment.],

    [Background Jobs], [Hangfire],
    [Persistent job processing for cart expiry, embedding generation queue, and periodic maintenance.],

    [Authentication], [JWT, ASP.NET Identity],
    [Short-lived access tokens with refresh token rotation; role-based and permission-based authorisation.],

  ),
  caption: [Technology stack of the ReSys.Shop platform, organised by architectural layer.],
) <tbl-tech-stack>

Each component was selected to satisfy a specific architectural requirement. Vue 3 and Vite provide fast frontend iteration appropriate for a single-developer thesis project. .NET 10 supplies the strong type system, high-throughput web server, and mature ORM that underpin the transactional e-commerce backend. PostgreSQL hosts both the relational schema and, via pgvector, the vector index, consolidating data management into a single database. Redis adds a caching tier for repeated queries and transient state storage. The Python sidecar exists specifically to host PyTorch for model inference, isolated so that GPU resource spikes do not affect API availability. .NET Aspire unifies these components into a reproducible container-based development environment. Hangfire persists background job state in Redis, ensuring operations survive application restarts. JWT-based authentication with refresh token rotation follows current best practices for securing browser-based single-page applications.
