= Background and Related Work

== E-commerce Platform Architectures

Enterprise web applications have evolved through several architectural patterns, each shaped by the competing demands of development velocity, operational complexity, and runtime performance. The *monolithic* architecture packages all components, user interface, business logic, data access, into a single deployable unit. While simple to develop and test at small scale, monoliths tend to accumulate coupling between subsystems as the codebase grows, making independent feature development and incremental upgrades progressively more difficult. At the opposite pole, the *microservices* pattern decomposes an application into independently deployable services, each owning a discrete business capability. This enables teams to work in parallel and adopt different technology stacks per service, but it introduces substantial operational overhead: distributed data management, network latency, partial failure modes, and complex deployment pipelines. Between these extremes lies the *modular monolith*, an architecture that organises code into logically isolated business modules within a single process, sharing a common data store while enforcing strict compile-time boundaries that prevent direct cross-module references.

This thesis adopts the modular monolith pattern as the structural backbone of the ReSys.Shop platform. The choice is motivated by three practical concerns. First, a single-process architecture eliminates the need for service discovery, inter-service authentication, and distributed transaction orchestration, complexities that would be disproportionate for a system whose primary research contribution lies in its machine learning integration, not in its infrastructure. Second, a shared PostgreSQL database allows relational product data and vector embeddings to coexist within the same transactional context, an important property for maintaining consistency between catalog updates and search index changes. Third, the nine business modules (Catalog, Ordering, Payment, Inventory, Identity, Profile, Shipping, Location, and Dashboard) are separated by namespace convention with no direct cross-references, preserving the logical independence of bounded contexts while avoiding the operational cost of separate deployment units. The machine learning capability, image embedding generation and model inference, is isolated in a dedicated Python sidecar service, the one component that runs as a separate process due to its distinct technology stack and resource profile.

== Visual Search and Content-Based Image Retrieval

At the heart of visual search is a simple idea: turning images into lists of numbers that a computer can compare. These lists are called *vector embeddings* or *feature vectors*.

=== Definition and Mathematical Representation

When we look at an image, we see colours, shapes, and patterns. Computers cannot "see" in the same way; they require numerical data to process information. A vector embedding is a way to represent the visual content of an image as a sequence of numbers.

For example, when an AI model processes an image of a red dress, it might output a 512-dimensional list like `[0.23, -0.15, 0.87, 0.42, ..., -0.31]`.

This list captures the "essence" of that image in a compressed form. Similar images will produce similar lists of numbers.

=== The Latent Space

When we talk about vectors, we can think of them as points in a high-dimensional space. For a 512-dimensional vector, imagine a space with 512 axes, instead of just the three axes we can visualise (x, y, z).

In this space:
- Similar images are located close together
- Different images are far apart

This space where embeddings live is called the *latent space*. The word "latent" means hidden, signifying that these dimensions do not correspond to obvious things like "redness" or "stripiness," but rather to abstract features the model learned during training.

=== Measuring Similarity

To find similar products, the system needs to measure how close two vectors are. This project uses *cosine similarity*, which measures the angle between two vectors:

$ "similarity" = cos(theta) = (A dot B) / (||A|| times ||B||) $

Where:
- $A$ and $B$ are the two vectors being compared
- $A dot B$ is the dot product (multiply corresponding elements and sum)
- $||A||$ and $||B||$ are the lengths of each vector

The cosine similarity ranges from:
- *1.0*, vectors point in the same direction (very similar)
- *0.0*, vectors are perpendicular (unrelated)
- *-1.0*, vectors point in opposite directions (very different)

For fashion images, a cosine similarity above 0.7 typically indicates visual similarity that users would recognise.

=== Mathematical Similarity: From Visual Comparison to Cosine Distance

The power of embeddings is that complex visual comparisons become simple mathematics. Instead of trying to write rules like "match items with similar colours and patterns," the system:

1. Converts the query image to a vector
2. Compares that vector to all product vectors in the database
3. Returns products with the highest similarity scores

This approach is much more flexible than rule-based matching because the AI model learns which features are important from examples, rather than having those features hand-coded by a developer.

=== The CBIR Pipeline

The end-to-end flow from image upload to ranked results is a sequential pipeline connecting preprocessing, feature extraction, vector storage, and similarity search. Figure @fig-cbir-pipeline summarises this pipeline as implemented in the ReSys.Shop platform.

#figure(
  image("../../images/diagrams/01-cbir-pipeline.png", width: 90%),
  caption: [
    High-level CBIR pipeline: from image upload through preprocessing, feature extraction, vector indexing, and similarity search to ranked results.
  ],
) <fig-cbir-pipeline>

Vector embeddings are the mathematical foundation that enables visual search. The key question becomes: *how do we generate these embeddings?* This requires specialised neural network architectures that can extract meaningful features from images. The next section surveys the architectures relevant to this project.

== Deep Learning Models for Fashion Image Retrieval

Generating useful vector embeddings from fashion product images requires models that can extract features at multiple levels of abstraction, from low-level textures and edges to high-level concepts like silhouette, style category, and garment type. Over the past decade, three families of neural architectures have emerged as the dominant approaches to this task, progressing from convolutional feature hierarchies to attention-driven global context models and, most recently, to models that jointly understand both visual and textual descriptions.

=== Convolutional Neural Networks

Convolutional neural networks (CNNs) process images through a hierarchy of learned filters that detect increasingly abstract patterns @he2016deep. Early layers respond to simple features, edges, colour gradients, and texture orientations, while deeper layers compose these primitives into complex structures such as fabric patterns, neckline shapes, and garment silhouettes. This hierarchical organisation mirrors aspects of the mammalian visual cortex and has proven remarkably effective for visual recognition tasks.

Two CNN architectures are particularly relevant to this project. *ResNet* (Residual Network), introduced by He et al. in 2016, addressed the vanishing gradient problem that had historically limited the depth of trainable networks. By adding skip connections that bypass one or more layers, ResNet enables gradients to flow directly through the network during backpropagation, making it feasible to train architectures with 50, 101, or even 152 layers. ResNet-50, the 50-layer variant, serves as a widely adopted feature extractor in the computer vision community, pre-trained on the ImageNet dataset of over one million labelled images across one thousand categories.

*EfficientNet*, proposed by Tan and Le in 2019, introduced a compound scaling method that uniformly scales network depth, width, and input resolution using a principled set of coefficients. This produces a family of models (EfficientNet-B0 through B7) that achieve better accuracy with fewer parameters than previous architectures. EfficientNet-B0, the smallest variant, produces 1,280-dimensional embeddings and is notable for its low computational footprint: it runs faster than most alternatives while retaining competitive feature quality, making it an attractive baseline for resource-constrained deployments @tan2019efficientnet.

=== Vision Transformers

While CNNs excel at capturing local patterns through their layered design, their reliance on small receptive fields, typically 3×3 or 5×5 pixel windows, limits their ability to model relationships between distant image regions. Vision Transformers (ViTs) address this limitation by applying the self-attention mechanism originally developed for natural language processing to image data @dosovitskiy2020vit.

A ViT processes an image by dividing it into a grid of fixed-size patches (typically 16×16 pixels), flattening each patch into a token vector, and feeding the resulting sequence through a stack of transformer encoder layers. The self-attention mechanism within each layer computes pairwise relationships between all patches, allowing the model to capture long-range dependencies, a sleeve pattern that echoes a collar detail, a texture that repeats across a dress, that would require many successive CNN layers to approximate. This global receptive field is especially valuable for fashion retrieval, where the overall composition of a garment matters as much as its local details.

*DINOv2*, developed by Oquab et al., represents a particularly notable ViT variant for this project's use case @oquab2023dinov2. Unlike earlier ViTs trained with human-annotated labels, DINOv2 uses self-supervised learning: it is trained to produce consistent representations across different augmentations of the same image, without requiring any category labels. This self-supervision objective encourages the model to discover semantically meaningful features directly from visual structure. DINOv2 ViT-S/14, the small variant with 14×14 patch size, produces 384-dimensional embeddings and has been shown to achieve strong performance on image retrieval tasks despite its compact size. The absence of any supervised classification head means that features are not biased toward the particular set of categories present in a labelled training dataset, making DINOv2 a strong candidate for open-ended visual search scenarios.

=== CLIP and Multimodal Models

CNNs and ViTs operate purely in the visual domain: they map an image to a point in an embedding space, but that space has no connection to human language. *CLIP* (Contrastive Language-Image Pre-training), introduced by Radford et al., bridges this gap through a dual-tower architecture @radford2021learning. One tower, typically a ViT, encodes images into vectors. A parallel text encoder tower processes natural language descriptions. Both are trained jointly on a dataset of 400 million (image, caption) pairs collected from the public web, using a contrastive objective that pulls matching image-text pairs together in a shared embedding space while pushing non-matching pairs apart.

The result is a *shared latent space* where an image of a dress and the phrase "red floral summer dress" map to nearby vectors, even though they originate from entirely different modalities. This property is the foundation of multimodal search: a user can query with text, with an image, or with a combination of both.

*Fashion-CLIP* extends this paradigm by fine-tuning CLIP on over 700,000 fashion images paired with domain-specific textual descriptions @chia2022fashionclip. The fine-tuning process adjusts the model's weights to emphasise fashion-relevant attributes, garment categories, fabric textures, style descriptors, and occasion labels, while retaining the general visual understanding acquired during CLIP's broad pre-training. Fashion-CLIP uses the ViT-B/16 architecture (a ViT-Base with 16×16 patch size) inherited from CLIP, producing 512-dimensional embeddings. By narrowing the domain focus, Fashion-CLIP improves retrieval quality on fashion queries by approximately 10 to 20% relative to general CLIP, as demonstrated in the original paper and confirmed in the evaluation presented in Chapter 6 of this thesis.

The dual-tower design also makes Fashion-CLIP uniquely capable of multimodal queries. A user searching for "red floral summer dress" does not need to upload a reference image; the text encoder maps the query directly into the same embedding space as the catalog images. Similarly, a hybrid search combining an uploaded photo with a textual refinement, "like this, but in blue", becomes possible by encoding both modalities and merging the resulting vectors.

=== Model Comparison

Table @tbl-model-comparison summarises the models discussed in this section, spanning both convolutional and transformer-based architectures, general-purpose and domain-specific training objectives, and a range of embedding dimensionalities and inference latencies. The inference times are measured on the experimental hardware described in Chapter 6.

#figure(
  table(
    columns: (auto, auto, auto, auto, auto, auto),
    align: (left + horizon, center + horizon, center + horizon, center + horizon, center + horizon, center + horizon),
    stroke: 0.5pt,

    table.header([*Model*], [*Architecture*], [*Embedding Dim*], [*Training Method*], [*Domain*], [*Inference (ms)*]),

    [ResNet-50], [CNN], [2048], [Supervised (ImageNet)], [General], [~15],
    [EfficientNet-B0], [CNN], [1280], [Supervised (ImageNet)], [General], [~21],
    [DINOv2 ViT-S/14], [ViT], [384], [Self-supervised], [General], [~80],
    [CLIP ViT-B/16], [ViT], [512], [Contrastive (image-text)], [General], [~60],
    [Fashion-CLIP ViT-B/16], [ViT], [512], [Contrastive (fashion)], [Fashion], [~68],
  ),
  caption: [
    Comparison of candidate embedding models for fashion product retrieval.
    Inference times are approximate and measured on a mid-range consumer GPU.
    Fashion-CLIP balances moderate dimensionality with domain-specific training and multimodal capability.
  ],
) <tbl-model-comparison>

== Vector Search and Databases

Once images are converted to vector embeddings, those vectors must be stored and searched efficiently. A naive approach, comparing the query vector to every stored vector and sorting by distance, becomes impractical as the catalog grows. For a catalog of $n$ products, each search requires $n$ distance computations. At the scale of 10,000 items this is manageable on modern hardware; at 1,000,000 items it is several orders of magnitude too slow for interactive search.

=== Approximate Nearest Neighbour Search

The solution is *approximate nearest neighbour* (ANN) search. Rather than exhaustively computing the distance to every vector, ANN algorithms use index structures that organise vectors into a graph or tree, enabling the search to navigate directly toward the neighbourhood of likely matches. The key insight is that for product search, an approximate result with 97 to 99% recall of the true top matches is entirely acceptable, and the speed improvement can be several orders of magnitude.

=== HNSW: Hierarchical Navigable Small World

This project uses the *HNSW* algorithm, introduced by Malkov and Yashunin, which is among the most effective ANN approaches @malkov2018efficient. HNSW builds a multi-layer graph where each layer is a navigable small-world network. The topmost layer contains a sparse subset of nodes with long-range connections, functioning like the express lanes in a highway system. Lower layers are progressively denser, with shorter connections that enable fine-grained local search. A query begins at an entry point in the top layer, hops between nodes toward the query region in logarithmic time, then descends to the next layer to refine the result. This layered structure reduces the search complexity from $O(n)$ for brute-force scan to approximately $O(log n)$.

=== pgvector: Vector Search in PostgreSQL

pgvector is an open-source extension that adds vector storage and search operations to PostgreSQL @pgvector2023. It introduces a `VECTOR(d)` column type for d-dimensional vectors, provides cosine distance and Euclidean distance operators directly in SQL, and supports both HNSW and IVFFlat index types for accelerated similarity queries.

The decision to use pgvector rather than a specialised vector database, such as Pinecone, Milvus, or Weaviate, was driven by several practical considerations. First, pgvector stores vectors in the same database tables as the product metadata they describe, meaning that product updates and embedding updates occur within a single ACID transaction. This eliminates a class of bugs known as the *dual-database problem*, where a vector index can drift out of sync with its source of truth because the two stores have independent consistency guarantees. Second, pgvector requires no additional infrastructure: no separate service to deploy, monitor, and secure. For a system that already operates PostgreSQL for its relational data, adding pgvector is a matter of loading one extension. Third, pgvector queries use standard SQL, allowing developers to combine vector similarity search with relational filtering, find products visually similar to this image, but only from the dresses category and priced under one hundred dollars, in a single query with a single query plan. The trade-off is scale: pgvector is well-suited to catalogs in the tens of thousands to low millions of vectors, but may require migration to a dedicated vector database for web-scale deployments beyond that range.

=== Configuration and Distance Metric

Cosine distance is the primary comparison metric used in this project. For normalised embeddings, cosine distance and Euclidean distance are functionally equivalent, sorting by one produces the same ranking as sorting by the other, but cosine distance has the advantage of being bounded between 0 and 2 for normalised vectors, making similarity thresholds more interpretable. HNSW configuration parameters are left at their pgvector defaults (`m = 16` connections per node, `ef_construction = 64`), which provide a satisfactory balance of index build time and query accuracy for the catalog sizes evaluated in this work.

== Related Systems

This section compares the ReSys.Shop platform with existing academic research and commercial products to understand the current state of fashion visual search and how this project contributes to the field.

=== Academic Research in Fashion Retrieval

Visual search for fashion has been an active research area for over a decade. Key developments include:

*DeepFashion Dataset.* The DeepFashion dataset, introduced by Liu et al., with over 800,000 fashion images, established benchmarks for fashion recognition and retrieval @liu2016deepfashion. It provided attribute annotations (colour, pattern, category), landmark annotations (collar, sleeve, hemline positions), and pairs of matching in-shop and consumer photos. This dataset enabled much of the subsequent research in fashion AI.

*Conversational Fashion Retrieval.* More recent work has explored combining images with text feedback. FashionIQ introduced the task of modifying retrieval based on natural language, for example, "like this dress but shorter" @wu2019fashioniq. This requires understanding both images and text modifications. While conceptually compelling, the interactive dialogue paradigm of conversational retrieval was beyond the scope of this project, which focuses on single-turn image and text queries.

*Pre-trained Foundation Models.* Recent trends favour using pre-trained models like CLIP rather than training from scratch. Fashion-CLIP demonstrated that domain-specific fine-tuning of CLIP improves fashion retrieval by 15 to 20% over general CLIP @chia2022fashionclip. This project follows the same approach: using pre-trained Fashion-CLIP rather than training new models, and extending the evaluation to include additional architectures (ResNet, EfficientNet, DINOv2) for systematic comparison.

=== Commercial Visual Search Systems

Several companies have deployed visual search at scale:

#figure(
  table(
    columns: (auto, 1fr, 1fr),
    stroke: 0.5pt,
    align: (center, left, left),
    [*Product*], [*Strengths*], [*Limitations for This Project*],
    [Google Lens], [Massive scale, general purpose], [Closed ecosystem, not customisable],
    [Pinterest Lens], [600M+ monthly searches, style-aware], [Proprietary, requires Pinterest integration],
    [ASOS Style Match], [Fashion-specific accuracy], [Only for ASOS catalog],
    [ViSenze], [API available, good accuracy], [Paid service, recurring costs],
  ),
  caption: [
    Comparison of commercial visual search products.
  ],
) <tbl-commercial-comparison>

These products are impressive but share common limitations for smaller projects: they are proprietary and cannot be studied or modified; API access incurs costs per query; and relying on an external service creates vendor lock-in. This project demonstrates that similar functionality can be achieved with open-source tools, providing a reference implementation and cost-effective solution for smaller applications.

=== Technical Positioning and Contribution

This project distinguishes itself from existing literature by addressing the *engineering gap* between theoretical AI models and production-grade software. While typical research focuses on optimising metric scores (e.g., mAP), this thesis contributes a reference architecture for *operationalising* those models.

==== 1. Polyglot Vertical Slice Architecture

Most open-source implementations force a choice between a monolithic Python web stack (Django/Flask) or a complex microservices mesh. This project introduces a *Distributed Vertical Slice* pattern that:
- Leverages *.NET 10* for strict type safety, high-performance concurrency, and domain logic integrity in the transactional core.
- Isolates *Python 3.12* solely for tensor computations (PyTorch), connected via a resilient HTTP bridge.
- *Differentiation:* This provides the best of both worlds (enterprise-grade backend reliability with access to the bleeding-edge AI ecosystem), without the operational overhead of a full microservices deployment.

==== 2. Vector-Native Data Consistency

A common pitfall in visual search is the dual-database problem, where vector data (stored in a Chroma/Pinecone instance) drifts from the relational source of truth (SQL).
- *Contribution:* This implementation utilises *pgvector* to enforce ACID transactions across both relational entities and vector embeddings.
- *Impact:* Product updates and index re-calculations occur in the same atomic transaction scope, eliminating the class of "stale index" bugs common in distributed systems.

==== 3. Feature Parity on Commodity Hardware

Commercial solutions such as Google Lens rely on massive cloud TPU clusters. This project demonstrates that *Fashion-CLIP* (ViT-B/16) can be effectively served on commodity hardware (mid-range GPU or standard CPU) with sub-100ms latency.
- *Result:* This lowers the barrier to entry for small and medium-sized e-commerce platforms to adopt generative AI and semantic search features without recurring cloud API costs.

==== 4. Applied Evaluation of Foundation Models

Moving beyond the "leaderboard" mentality, this thesis compares *ResNet*, *EfficientNet*, *DINOv2*, and *Fashion-CLIP* specifically within the constraints of a real-time web application.
- *Key Finding:* We demonstrate that while DINOv2 offers superior raw geometric matching, its larger computational cost makes it less viable for CPU-bound environments than the optimised Fashion-CLIP, providing a pragmatic guide for model selection in resource-constrained deployments.

== Technology Stack

The ReSys.Shop platform is built on a polyglot stack spanning three programming languages, C\#, TypeScript, and Python, orchestrated through a unified containerised development environment. Table @tbl-tech-stack summarises the principal technologies and their roles.

#figure(
  table(
    columns: (auto, auto, auto),
    stroke: 0.5pt,
    align: (left + horizon, left + horizon, left + horizon),

    table.header([*Layer*], [*Technology*], [*Purpose*]),

    [Frontend], [Vue 3 + TypeScript + Vite], [
      Customer storefront and admin panel. Vue 3 provides a reactive component model;
      TypeScript adds type safety to frontend code; Vite delivers fast development builds.
      State management is handled by Pinia stores.
    ],

    [Backend API], [.NET 10 + Carter + MediatR], [
      REST endpoints delivered via Carter minimal APIs. MediatR implements CQRS (Command
      Query Responsibility Segregation), cleanly separating read and write operations
      and enforcing the modular boundaries between business domains.
    ],

    [Database], [PostgreSQL + pgvector], [
      Relational data and vector embeddings coexist within a single ACID database.
      The pgvector extension provides HNSW-indexed vector similarity search using
      standard SQL, eliminating the need for a separate vector store.
    ],

    [Caching], [Redis + HybridCache], [
      Multi-tier caching with in-memory L1 (fast, local) and Redis L2 (shared, durable).
      Also serves as the backing store for Hangfire job queues and session state.
    ],

    [ML Sidecar], [Python + FastAPI + PyTorch], [
      Dedicated service for image embedding generation. FastAPI provides async HTTP
      endpoints with built-in OpenAPI documentation. PyTorch executes model inference
      and supports GPU acceleration when available.
    ],

    [Orchestration], [.NET Aspire], [
      Service discovery, container lifecycle management, and local development
      environment. Aspire coordinates the startup and interconnection of the .NET API,
      Python ML sidecar, PostgreSQL, and Redis containers.
    ],

    [Background Jobs], [Hangfire], [
      Persistent job processing for cart expiry (auto-clean after 7 days of inactivity),
      embedding generation queue (decoupling image upload from inference), and
      periodic maintenance tasks.
    ],

    [Authentication], [JWT + ASP.NET Identity], [
      Short-lived access tokens (15-minute lifetime) with refresh token rotation and
      reuse detection. Role-based and permission-based authorisation segregates
      admin functions from customer-facing endpoints.
    ],

  ),
  caption: [
    Technology stack of the ReSys.Shop platform, organised by architectural layer.
  ],
) <tbl-tech-stack>

Each technology was selected to satisfy a specific architectural requirement. Vue 3 and Vite provide a modern frontend development experience with fast iteration cycles appropriate for the scope of a single-developer thesis project. .NET 10 offers the strong type system, high-throughput web server, and mature ORM (Entity Framework Core) that underpin the transactional e-commerce backend. PostgreSQL was a natural choice for the data layer because it hosts both the relational schema and, via pgvector, the vector index, consolidating data management into a single well-understood database. Redis adds the caching tier that keeps repeated queries fast and stores transient state. The Python sidecar exists specifically to host PyTorch, the de facto standard framework for loading and running pre-trained deep learning models, isolated from the main application process so that a failure or resource spike in inference does not affect the availability of the e-commerce API. .NET Aspire unifies these components into a reproducible development environment using container-based orchestration. Hangfire ensures that background operations survive application restarts by persisting job state in Redis. Finally, JWT-based authentication with refresh token rotation follows current best practices for securing browser-based single-page applications.
