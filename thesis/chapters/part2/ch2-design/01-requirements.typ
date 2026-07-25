== Requirements Analysis

This section defines the scope of the ReSys.Shop platform by identifying its actors, functional and non-functional requirements, key use cases, and the classification of features into research contributions and supporting infrastructure. The analysis establishes what the system must do before proceeding to its architectural design and implementation.

=== System Actors

The platform serves three categories of actors, each with a distinct role, set of permissions, and interaction surface. Table @tbl-system-actors summarises these actors and their primary responsibilities.

#figure(
  table(
    columns: (auto, 1fr, 1fr),
    stroke: 0.5pt,
    align: (center + horizon, left, left),

    table.header([*Actor*], [*Role Description*], [*Interaction Surface*]),

    [Customer
    (Guest +
    Authenticated)], [
      Browses the product catalog, performs keyword and visual searches, manages a shopping cart, completes multi-step checkout, and tracks order history. Guest users can browse and add items to a cart; authenticated users access profile management, wishlists, and personalised features.
    ], [
      Vue 3 Storefront SPA\\
      (Web browser)
    ],

    [Administrator], [
      Manages the full product lifecycle: creating and updating products with fashion-specific metadata, uploading and organising product images, defining taxonomies, monitoring inventory levels, processing order fulfilment, and managing user accounts and permissions.
    ], [
      Vue 3 Admin SPA\\
      (Web browser)
    ],

    [System
    (Background
    Services)], [
      Automated background processes that maintain data consistency and system performance: generating and indexing vector embeddings for newly uploaded images, expiring abandoned carts after a configurable time window, reserving and releasing inventory during checkout, and performing periodic index maintenance.
    ], [
      Internal services\\
      (No direct UI)
    ],
  ),
  caption: [System actors and their roles within the ReSys.Shop platform.],
) <tbl-system-actors>

The Customer and Administrator actors represent human users interacting through browser-based single-page applications. The System actor represents background processes that operate without direct human interaction, executing scheduled and event-driven tasks through Hangfire job workers within the .NET application process. The three actors together define the complete set of interactions supported by the platform.

=== Functional Requirements

The functional capabilities of ReSys.Shop are organised around eight business modules, each responsible for a coherent subset of domain logic. This section describes each module in narrative prose; a summary table at the end of the section consolidates responsibilities and research classification.

==== Catalog Module

The Catalog module manages the product lifecycle: creating products with fashion-specific metadata, including style code, season, material, department, and gender target, defining sellable variants with SKUs, barcodes, and independent pricing, uploading product images with automatic thumbnail generation, and organising products through hierarchical taxonomies that allow browsing by category (e.g., Clothing → Dresses → Evening Dresses). It also hosts the Content-Based Image Retrieval (CBIR) infrastructure: newly uploaded variant images are sent to the Python machine learning sidecar for vectorisation, and the resulting embeddings are stored in PostgreSQL using the pgvector extension for similarity search @pgvector2023. The catalog supports configurable embedding models, allowing the system to switch between Fashion-CLIP, ResNet-50, and other architectures without application changes, a capability that enables the systematic benchmark evaluation presented in Chapter 3.

==== Ordering Module

The Ordering module handles the customer purchase workflow from cart to completed order. Both guest and authenticated users can add products to a cart, which automatically expires after seven days of inactivity to prevent indefinite stock reservation. Checkout proceeds through a forward-only state machine: the customer selects a shipping address, chooses a delivery method, provides payment details, reviews the order summary, and confirms. Once confirmed, the system creates an order record, reserves inventory quantities for each line item, processes the payment intent, and clears the cart. Orders track item totals, price adjustments, shipment costs, and payment state independently, enabling partial fulfilment scenarios. Cancellation is available at any pre-confirmation stage without penalty.

==== Payment Module

The Payment module manages the lifecycle of payment intents, the data structure representing a customer's intent to pay, including creation, capture, refund, and void operations. It supports two gateway providers: the Stripe gateway for production, which validates incoming webhooks using signature verification to prevent spoofed payment confirmations, and a Bogus gateway for development and testing, which simulates the payment lifecycle by automatically transitioning through states without external calls. Payment intents follow their own state machine, Pending, RequiresAction, Processing, and Succeeded or Canceled, and the system maintains its own copy of the payment state in parallel with the gateway's state, enabling offline operations and consistent behaviour across both gateway implementations.

==== Inventory Module

The Inventory module tracks physical stock quantities across warehouse locations, manages temporary reservations during active checkouts to prevent overselling, records stock movements for audit trails, and handles inter-warehouse transfers. Each stock item is associated with a specific product variant and a warehouse location, with quantities maintained as both on-hand (physical count) and reserved (held for active checkouts) values. The reservation mechanism ensures that a variant added to a cart remains visible to other customers as having limited availability but cannot be sold twice.

==== Identity Module

The Identity module provides JWT-based authentication with short-lived access tokens (fifteen-minute lifetime) and refresh token rotation with reuse detection: each refresh token is single-use, and presenting a previously consumed token triggers revocation of all tokens for that user to contain potential compromise. Guest sessions enable anonymous cart usage through cookie-based identifiers that persist across page navigations without requiring account creation. Role-based and permission-based authorisation segregates admin functions from customer-facing endpoints, with permission claims following a `domain:category:action` format that allows fine-grained access control.

==== Supporting Modules

Three additional modules provide complementary infrastructure. The *Profile* module manages user addresses, wishlists, and notification preferences, linking customer identity to personalisation features. The *Shipping* module configures delivery methods, standard, express, and local pickup, and calculates shipping rates by geographic zone. The *Location* module provides country and state reference data with ISO codes, shared across Shipping (for zone configuration) and Profile (for address validation).

==== Summary

Table @tbl-module-summary consolidates the eight business modules with their key responsibilities and research classification relative to this thesis.

#figure(
  table(
    columns: (auto, 1fr, auto),
    stroke: 0.5pt,
    align: (left + horizon, left, center + horizon),

    table.header([*Module*], [*Key Responsibilities*], [*Research Classification*]),

    [Catalog], [
      Product and variant lifecycle management; fashion-specific metadata (style code, season, material, department); hierarchical taxonomies; image upload and management; CBIR infrastructure, embedding generation pipeline and pgvector vector search.
    ], [Core Research],

    [Ordering], [
      Shopping cart with auto-expiry; forward-only checkout state machine (Address → Delivery → Payment → Confirm → Complete); order lifecycle tracking; cancellation and partial fulfilment support.
    ], [Supporting],

    [Payment], [
      Payment intent lifecycle (create, capture, refund, void); Stripe gateway with webhook signature validation; Bogus test gateway; parallel state tracking for offline operations.
    ], [Supporting],

    [Inventory], [
      Per-warehouse stock tracking; checkout-time quantity reservation to prevent overselling; auditable stock movements; inter-warehouse transfers.
    ], [Supporting],

    [Identity], [
      JWT authentication with 15-minute access tokens; refresh token rotation and reuse detection; guest session support; role-based and permission-based authorisation.
    ], [Supporting],

    [Profile], [
      User addresses (shipping and billing); wishlist management; notification preferences.
    ], [Supporting],

    [Shipping], [
      Delivery method configuration; zone-based rate calculation.
    ], [Supporting],

    [Location], [
      Country and state reference data with ISO codes.
    ], [Supporting],
  ),
  caption: [
    Summary of business modules, their key responsibilities, and classification as Core Research or Supporting Infrastructure.
    Only Catalog is classified as Core Research because it hosts the CBIR capability that is the primary subject of evaluation.
  ],
) <tbl-module-summary>

The functional scope of ReSys.Shop extends far beyond the visual search capability at its core. The supporting modules, Ordering, Payment, Inventory, and Identity, provide a realistic e-commerce context in which the research contribution can be meaningfully evaluated. Without a functioning checkout flow, for example, the value of visual search could not be measured through downstream conversion events. Without inventory awareness, search results could include out-of-stock items, undermining the realism of the evaluation.

=== Non-Functional Requirements

Beyond feature completeness, the system must satisfy quantitative and qualitative constraints that determine its fitness for production use. Table @tbl-nfr summarises the non-functional requirements across five quality dimensions.

#figure(
  table(
    columns: (auto, 1fr, 1fr),
    stroke: 0.5pt,
    align: (left + horizon, left, left),

    table.header([*Quality*], [*Target*], [*Rationale*]),

    [Performance], [
      CBIR end-to-end search latency under 1 second (image upload through embedding generation, vector database query, and result assembly). Non-search API endpoints respond within 200 milliseconds under normal load. Asynchronous I/O handles concurrent requests without blocking threads.
    ], [
      Real-time visual search requires sub-second response to maintain user engagement. Studies show that search latency above one second measurably increases abandonment rates in e-commerce contexts @manning2008introduction.
    ],

    [Security], [
      JWT access tokens expire after 15 minutes; refresh tokens follow single-use rotation with reuse-detection invalidation. Role-based authorisation enforced per endpoint. Rate limiting on authentication endpoints (five requests per minute for login, three per hour for registration). Security response headers (CSP, HSTS, X-Frame-Options, X-Content-Type-Options) on all HTTP responses. File upload validation: magic-byte verification, extension allowlist, and 10 megabyte size limit.
    ], [
      Browser-based single-page applications are exposed to the open internet and must defend against common web threats. Short-lived tokens and refresh rotation limit the window of token compromise. File upload validation prevents malicious payloads from entering the embedding pipeline.
    ],

    [Modularity], [
      Eight business modules in a single .NET assembly, separated by namespace convention with no direct cross-references. All inter-module communication occurs through MediatR in-process message dispatch. Each module independently testable without loading its neighbours.
    ], [
      The modular monolith pattern preserves the logical separation of microservices while avoiding distributed-system complexity. MediatR dispatch provides a clean integration point that can be replaced with an external message broker if modules are later extracted into separate services.
    ],

    [Observability], [
      OpenTelemetry distributed tracing across .NET API and Python ML sidecar, with trace context propagation through HTTP headers. Structured logging with correlation identifiers on every log entry. Health check endpoints for each service, consumed by .NET Aspire for container orchestration and restart decisions.
    ], [
      In a polyglot architecture spanning C\# and Python, end-to-end request tracing is essential for diagnosing latency bottlenecks and error propagation. Correlation identifiers enable a single request to be followed across service boundaries in log aggregators.
    ],

    [Reliability], [
      Background jobs (cart expiry, embedding generation retries, index maintenance) persist in Redis-backed Hangfire storage, surviving application restarts without data loss. Payment webhooks include idempotency keys that prevent duplicate processing on retry. Cart expiry triggers after fifteen minutes of inactivity, releasing reserved inventory automatically.
    ], [
      Many e-commerce operations are inherently long-running or time-delayed, cart expiry, payment confirmation, and index maintenance. A durable job queue ensures these operations complete reliably, even across process crashes or scheduled restarts.
    ],
  ),
  caption: [
    Non-functional requirements with concrete targets and design rationale.
  ],
) <tbl-nfr>

These non-functional requirements shaped architectural decisions throughout the system. The one-second CBIR latency target influenced the choice of a synchronous embedding pipeline rather than a queued approach; the modularity requirement led to the MediatR-based in-process dispatch model; and the reliability constraint motivated the choice of Hangfire with Redis-backed persistence for background jobs. Each target is revisited in the evaluation chapter, where the benchmark results confirm whether the implemented system meets these stated requirements.

=== Use Cases

This section presents three use cases that represent the system's core functional scenarios: visual search (the primary research capability), checkout (the primary e-commerce transaction), and model benchmark evaluation (the research methodology for Chapter 3). Each use case is described in a compact tabular format comprising the actor, preconditions, main flow as numbered sequential steps, and postconditions. Figure @fig-use-case-diagram provides a visual summary of actor-system interactions.

==== Use Case 1: Visual Search (CBIR)

#figure(
  table(
    columns: (1fr, 3fr),
    stroke: 0.5pt,
    align: (left + horizon, left),

    [*Actor*], [Customer (Guest or Authenticated)],
    [*Precondition*], [
      The Python ML sidecar service is running and has loaded the configured embedding model into memory. The product catalog contains at least one variant with a stored embedding vector for the active model.
    ],
    [*Main Flow*], [
      1. Customer uploads a reference image (JPEG, PNG, or WebP; maximum ten megabytes) via the storefront visual search interface. \
      2. The Vue frontend sends the image as a multipart form data request to the .NET API endpoint. \
      3. The API validates the image, magic-byte verification, extension check, size limit, then forwards the raw image bytes to the Python ML sidecar. \
      4. The ML sidecar preprocesses the image (resize, normalise) and executes a forward pass through the configured embedding model, producing a floating-point vector. \
      5. The API queries PostgreSQL pgvector using cosine similarity against all stored variant embeddings filtered by the active model name, retrieving the top 20 most similar results. \
      6. The API joins variant data with product metadata, computes similarity scores, filters by a minimum similarity threshold (default 0.7), and returns the ordered results as JSON. \
      7. The Vue storefront renders the results as a grid of product thumbnails with similarity scores and prices.
    ],
    [*Postcondition*], [
      A ranked list of visually similar products is displayed to the customer, ordered by decreasing cosine similarity. Each result includes the product thumbnail, name, price, and similarity score.
    ],
  ),
  caption: [UC-1: Visual Search (CBIR), the primary research use case.],
) <tbl-uc-visual-search>

==== Use Case 2: Multi-Step Checkout

#figure(
  table(
    columns: (1fr, 3fr),
    stroke: 0.5pt,
    align: (left + horizon, left),

    [*Actor*], [Customer (Guest or Authenticated)],
    [*Precondition*], [
      The customer's cart contains at least one valid, in-stock item. The customer has a shipping address on file or is prepared to enter one.
    ],
    [*Main Flow*], [
      1. Customer clicks "Proceed to Checkout" from the cart page. \
      2. System presents the checkout interface, showing the current cart contents with item totals. \
      3. Customer selects or enters a shipping address. \
      4. Customer selects a delivery method from available shipping options. \
      5. Customer selects a payment method and provides payment details. \
      6. Customer reviews the order summary (items, shipping cost, tax, total) and clicks "Place Order". \
      7. System begins an atomic transaction: creates the order record, reserves inventory quantities for each line item, processes the payment through the configured gateway, and clears the cart. \
      8. System displays the order confirmation page with the order number and summary.
    ],
    [*Postcondition*], [
      An order record is created with status "Placed". Inventory quantities for each ordered variant are reserved. A payment intent is linked to the order. The customer's cart is emptied. A confirmation is displayed with the order reference number.
    ],
  ),
  caption: [UC-2: Multi-Step Checkout, the primary e-commerce transaction use case.],
) <tbl-uc-checkout>

==== Use Case 3: Model Benchmark Evaluation

#figure(
  table(
    columns: (1fr, 3fr),
    stroke: 0.5pt,
    align: (left + horizon, left),

    [*Actor*], [Researcher / System],
    [*Precondition*], [
      The benchmark dataset is available on disk, consisting of query images and catalog images organised into human-labelled similarity groups. The Python ML sidecar is running. All candidate embedding model weights are downloaded and accessible.
    ],
    [*Main Flow*], [
      1. Researcher selects a model from the candidate set (e.g., Fashion-CLIP, ResNet-50, DINOv2-S) and configures the ML sidecar via environment variable. \
      2. System generates embedding vectors for all query images and all catalog images using the selected model. \
      3. For each query image, the system executes a top-K (K = 20) similarity search against the catalog embeddings. \
      4. System computes retrieval metrics: Mean Average Precision (mAP), Precision at K, and Recall at K, using the human-labelled groups as ground truth. \
      5. System records operational metrics: average inference time per image, throughput (images per second), disk storage for the embedding index, and RAM consumption. \
      6. Steps 1 to 5 are repeated for each of the 11 candidate models. \
      7. System aggregates all results into comparison tables, ranking models by retrieval accuracy and operational efficiency.
    ],
    [*Postcondition*], [
      A complete benchmark report is produced containing accuracy metrics (mAP, P\@20, R\@20) and efficiency metrics (latency, throughput, storage, RAM) for every evaluated model. The report identifies the optimal model for each deployment scenario (GPU production, CPU-only, maximum accuracy, resource-constrained).
    ],
  ),
  caption: [UC-3: Model Benchmark Evaluation, the research methodology use case.],
) <tbl-uc-benchmark>

Figure @fig-use-case-diagram positions these three use cases alongside the broader system functionality within a single visual summary.

#figure(
  image("../../../images/diagrams/02-use-case.png", width: 85%),
  caption: [
    System use case diagram showing the three actors, Customer, Administrator, and System background services, and their primary interactions with the ReSys.Shop platform.
  ],
) <fig-use-case-diagram>

The three use cases serve distinct purposes within the thesis. The visual search use case defines the functional behaviour of the system's primary research capability; the checkout use case establishes the realistic e-commerce context in which search success can be measured through downstream conversion events; and the benchmark use case defines the systematic methodology used in Chapter 3 to evaluate and compare embedding models. The breadth of the system, nine background actors and use cases in the diagram, encompassing catalog browsing, account management, product administration, and order processing, reflects the full operational scope of the platform, while the three detailed use cases focus on the scenarios most relevant to the research questions.

=== Feature Classification

Not all features of ReSys.Shop carry equal research significance. Seven feature areas are classified in Table @tbl-feature-classification as either *Core Research* (directly contributing to the thesis's academic objectives) or *Supporting Infrastructure* (providing the realistic e-commerce context in which the research is conducted and evaluated). This distinction is important for two reasons: it clarifies the scope of the thesis's original contribution, and it explains why certain features, shipping calculation, user management, country reference data, exist in the platform but are not discussed in depth in subsequent chapters.

#figure(
  table(
    columns: (auto, auto, 1fr),
    stroke: 0.5pt,
    align: (left + horizon, center + horizon, left),

    table.header([*Feature Area*], [*Classification*], [*Rationale*]),

    [Visual Search (CBIR)], [Core Research], [
      Primary contribution: integrated multi-model CBIR with pluggable architecture enabling image-based product search across multiple embedding models (CNN, ViT, CLIP-based) within a production-style e-commerce platform.
    ],

    [ML Embedding Pipeline], [Core Research], [
      Critical infrastructure: automated ingestion of product images, generation of vector embeddings via the Python sidecar, storage in pgvector, and HNSW indexing, the operational backbone of the visual search capability.
    ],

    [Model Benchmark System], [Core Research], [
      Secondary contribution: systematic protocol for comparing retrieval accuracy and operational efficiency across 11 embedding models, providing a practical guide for model selection in resource-constrained deployments.
    ],

    [Product Catalog], [Supporting Infrastructure], [
      Required context: provides the structured dataset of fashion products, with variants, images, taxonomies, and metadata, that serves as the search target for CBIR evaluation.
    ],

    [Order System], [Supporting Infrastructure], [
      Metric validation: provides conversion events (add-to-cart, checkout completion) that serve as proxy indicators of search success, enabling the evaluation of visual search within a realistic shopping workflow.
    ],

    [Inventory], [Supporting Infrastructure], [
      Realism constraint: ensures that search results reflect actual product availability, preventing the unrealistic scenario where visually similar but out-of-stock items appear in search results.
    ],

    [Authentication], [Supporting Infrastructure], [
      Security baseline: protects administrative functions and user-specific data, enabling the application to operate in a representative security posture without which the system would be a research prototype rather than a deployable platform.
    ],
  ),
  caption: [
    Classification of feature areas into Core Research and Supporting Infrastructure.
    Core Research features represent the thesis's original contributions; Supporting Infrastructure features provide the realistic e-commerce context necessary for meaningful evaluation.
  ],
) <tbl-feature-classification>

The classification makes explicit what the thesis does and does not claim as contribution. The CBIR pipeline, encompassing embedding generation, vector storage, and similarity search, is the core research artefact. The e-commerce modules (Catalog, Ordering, Inventory, Payment, Identity) are supporting infrastructure, built to provide a realistic context that validates the visual search results in a production-like environment. This separation is maintained throughout the thesis: Sections 2.2 and 2.3 devote detailed treatment to the research features, while the supporting infrastructure is described only to the extent necessary to understand the system's design.
