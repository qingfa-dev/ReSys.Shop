== System Architecture & Design

This chapter presents the architectural design of ReSys.Shop, progressing from a high-level system overview through domain modelling, to the structural, data, API, and security layers. The design follows a service-oriented approach with three independently deployable services, a Vue 3 frontend, a .NET 10 modular monolith backend, and a Python machine learning sidecar, each responsible for a distinct technological concern. The chapter is organised into six sections, each accompanied by architectural diagrams that provide visual representations of the system's structure, behaviour, and deployment topology.

=== System Overview

ReSys.Shop is built as a service-oriented system with three distinct services. The frontend is implemented in Vue 3 and TypeScript using the Vite build tool. The backend is a .NET 10 modular monolith using ASP.NET Core for HTTP handling, Entity Framework Core for data access, and Carter for minimal API endpoint registration. The machine learning service is a Python FastAPI application running PyTorch models that generates vector embeddings from product images for visual similarity search.

Table @tbl-system-services summarises the three services, their technology stacks, and their primary responsibilities within the platform.

#figure(
  table(
    columns: (auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center + horizon, left, left),

    table.header([*Service*], [*Technology Stack*], [*Responsibilities*]),

    [Vue Frontend],
    [Vue 3 + TypeScript + Vite],
    [
      - Customer storefront (Nuxt UI)\
      - Administrator dashboard (PrimeVue)\
      - Pinia state management\
      - Image upload and visual search UI
    ],

    [.NET Backend],
    [.NET 10 + ASP.NET Core + Carter + EF Core],
    [
      - REST API endpoints via Carter minimal APIs\
      - Business logic via MediatR CQRS pattern\
      - PostgreSQL persistence with pgvector vector search\
      - JWT authentication and RBAC authorisation
    ],

    [Python ML],
    [Python 3.12 + FastAPI + PyTorch],
    [
      - Fashion-CLIP and other embedding model inference\
      - Vector embedding generation from product images\
      - Multi-model support with lazy-loading strategy
    ],
  ),
  caption: [System services and their technology stacks. Each service is independently deployable and communicates through well-defined HTTP contracts.],
) <tbl-system-services>

The backend is internally organised into eight bounded contexts following the principles of Domain-Driven Design. Each context owns a distinct area of business logic and communicates with other contexts exclusively through MediatR in-process message dispatch, there are no direct namespace references between business modules. Table @tbl-contexts-overview lists each context, its aggregate root, and a representative sample of its domain entities.

#figure(
  table(
    columns: (auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center + horizon, center + horizon, left),

    table.header([*Bounded Context*], [*Aggregate Root*], [*Key Domain Entities*]),

    [Catalog], [Product], [
      Variant, VariantImage, OptionType, OptionValue,
      Classification, Taxonomy, Taxon
    ],

    [Ordering], [Order], [
      LineItem, Adjustment, Shipment
    ],

    [Payment], [PaymentIntent], [
      PaymentCapture
    ],

    [Inventory], [StockItem], [
      StockLocation, StockMovement, StockReservation
    ],

    [Identity], [User], [
      Role, UserRole, RefreshToken, UserLogin
    ],

    [Profile], [UserProfile], [
      Address, Wishlist
    ],

    [Shipping], [ShippingMethod], [
      ShippingRate, ShippingZone
    ],

    [Location], [Country], [
      State
    ],
  ),
  caption: [Bounded contexts with aggregate roots and key domain entities. Each context owns its database schema and communicates with other contexts through MediatR dispatch only.],
) <tbl-contexts-overview>

The separation of concerns across these eight contexts enables independent evolution of each business domain while the modular monolith deployment model avoids the operational complexity of distributed microservices. The following section details the domain-driven design principles that govern these contexts.

=== Domain-Driven Design

The ReSys.Shop platform applies Domain-Driven Design (DDD) to structure its business logic around eight bounded contexts, each with well-defined aggregate roots, domain entities, and invariants. This section presents the context map, the aggregate design with invariants, the ubiquitous language glossary, and the state machines that govern the checkout and payment lifecycles.

==== Bounded Context Map

The eight bounded contexts partition the e-commerce domain along business capability boundaries. Each context owns its data, its domain logic, and its vocabulary, terms that are well-defined within a context may carry different meaning in another. For example, a Variant in the Catalog context is a sellable unit with a SKU and pricing; a LineItem in the Ordering context references that same variant but from the perspective of purchase fulfilment.

The integration between contexts follows the *Conformist* pattern: all contexts conform to a shared technical kernel defined in the Shared layer, which provides the `Result<T>` return type, the `ICommand` and `IQuery` marker interfaces, and the `Entity` base class with audit and versioning columns. Communication occurs exclusively through MediatR `ISender`, a context dispatches a query or publishes a notification, and other contexts react without ever importing one another's namespace. This in-process dispatch model eliminates the network latency of inter-service messaging while preserving the logical isolation of the bounded contexts.

Figure @fig-bounded-context-map depicts the eight contexts and the *Published Language*, the shared identifiers and value types, that flow between them.

#figure(
  image("../../../images/diagrams/06-bounded-context-map.png", width: 100%),
  caption: [Bounded Context Map showing the eight business contexts and the Published Language identifiers exchanged between them. All integration uses in-process MediatR dispatch; no context directly references another context's namespace.],
) <fig-bounded-context-map>

Table @tbl-context-responsibilities details each context's business responsibility and the integration data it exposes to the system.

#figure(
  table(
    columns: (auto, 1fr, 1fr),
    stroke: 0.5pt,
    align: (left + horizon, left, left),

    table.header([*Context*], [*Business Responsibility*], [*Published Language*]),

    [Catalog],
    [Manages the product lifecycle: creating products with fashion-specific metadata (style code, season, material, department, gender target), defining sellable variants with SKUs and independent pricing, uploading images with automatic embedding generation, and organising products through hierarchical taxonomies.],
    [ProductId, VariantId, Sku, Price, Slug],

    [Ordering],
    [Orchestrates the customer purchase workflow from cart to completed order. Manages cart with seven-day auto-expiry, forward-only checkout state machine, line items with price snapshots, adjustments, and cancellation at any pre-confirmation stage.],
    [OrderId, OrderNumber, Total, Currency, CheckoutState],

    [Payment],
    [Manages payment intent lifecycle, creation, capture, refund, void, across two gateway implementations. Maintains parallel payment state independent of the gateway for offline operations and consistent behaviour across providers.],
    [PaymentIntentId, PaymentState, Amount],

    [Inventory],
    [Tracks physical stock quantities per warehouse, manages temporary reservations during active checkouts to prevent overselling, records auditable stock movements through an append-only ledger, and handles inter-warehouse transfers.],
    [StockItemId, QuantityOnHand, QuantityReserved],

    [Identity],
    [Provides JWT-based authentication with refresh token rotation and reuse detection, role-based and permission-based authorisation with `domain:category:action` claim format, and guest session management for anonymous browsing.],
    [UserId, Email, PermissionClaim],

    [Profile],
    [Manages user addresses for shipping and billing, wishlists for product bookmarking, and notification preferences controlling email and SMS communication channels.],
    [ProfileId, AddressId],

    [Shipping],
    [Configures delivery methods, standard, express, local pickup, and calculates shipping rates by geographic zone using weight- and distance-based calculators.],
    [ShippingMethodId, Rate],

    [Location],
    [Provides country and state reference data with ISO 3166 codes. This context is read-only reference data shared across Shipping (zone configuration), Profile (address validation), and Ordering (checkout address selection).],
    [CountryId, StateId, IsoCode],
  ),
  caption: [Bounded context responsibilities and Published Language identifiers. The Published Language column lists the value types that other contexts may reference by identifier only, never by importing the source context's namespace.],
) <tbl-context-responsibilities>

==== Aggregates and Invariants

An aggregate is a cluster of domain objects treated as a single consistency boundary. Each aggregate has a root entity through which all modifications must pass. The root enforces invariants, business rules that must hold true at all times within the aggregate boundary. ReSys.Shop takes a pragmatic approach to DDD: it defines aggregate roots and their invariants explicitly but does not require formal value-object base classes or a dedicated domain-event infrastructure for every operation.

The four most architecturally significant aggregates are described below.

*Product (Catalog aggregate root).* The Product aggregate encapsulates a product family and all its variants, images, option configurations, and taxonomy classifications. A product may have one or more variants; exactly one is designated as the master variant displayed on listing pages. The aggregate enforces the following invariants: every product must have a unique slug for SEO-friendly URL generation; a product that declares options (such as size or colour) must have at least one option type defined; and the master variant must exist among the product's own variants. Variant images contain the `embedding` column, a 512-dimensional float vector generated by the ML sidecar, enabling cosine similarity search against the entire image corpus.

*Order (Ordering aggregate root).* The Order aggregate manages the checkout lifecycle from a nascent cart through to a completed purchase. It aggregates line items, each capturing a price snapshot of the variant at the time of purchase, and optional adjustments for discounts or promotions. The aggregate enforces the invariant that `Total = ItemTotal + AdjustmentTotal + ShipmentTotal`, maintaining financial consistency across all modifications. The checkout state progresses forward only, the address, delivery, payment, and confirmation stages must complete in sequence, and once an order is confirmed (finalised), it becomes immutable except for the cancel transition. This forward-only constraint is encoded in the domain entity itself and validated before every state transition.

*PaymentIntent (Payment aggregate root).* The PaymentIntent aggregate models the lifecycle of a customer's intent to pay. It is created with a specified amount and currency, and transitions through states, Pending, RequiresAction, Processing, Succeeded, Canceled, and Failed, based on gateway interactions. The aggregate tracks payment captures, where each capture debits a portion of the authorised amount. The system enforces the invariant that the sum of all captures must not exceed the original intent amount. A separate payment capture goes through its own state transitions: Succeeded → Captured → Refunded / Voided. The system maintains its own payment state in parallel with the gateway state, enabling consistent behaviour whether using the production Stripe gateway or the development Bogus gateway.

*StockItem (Inventory aggregate root).* The StockItem aggregate tracks the physical availability of a product variant at a specific warehouse location. It maintains two quantities: on-hand (physical count from warehouse operations) and reserved (units held for active checkouts). The aggregate enforces the invariant that `QuantityOnHand ≥ 0`, stock cannot go negative. Backorder support allows sales beyond on-hand quantity up to a configured backorder limit, but the system tracks the deficit separately. Quantity changes are not performed directly on StockItem; instead, they must be recorded as StockMovement entries in an append-only ledger, preserving a complete and auditable history of every stock change, including the quantity before and after, the reason, and the operating user.

==== Ubiquitous Language Glossary

A key practice in DDD is establishing a ubiquitous language, a shared vocabulary used by all team members and reflected directly in the codebase. Table @tbl-ubiquitous-language presents the core terms of the ReSys.Shop domain with their definitions.

#figure(
  table(
    columns: (auto, auto, 1fr),
    stroke: 0.5pt,
    align: (left + horizon, left, left),

    table.header([*Term*], [*Context*], [*Definition*]),

    [Product], [Catalog], [
      A product family representing a fashion item concept (e.g., "Cotton T-Shirt"). Holds shared metadata: description, slug, status, fashion-specific attributes, and taxonomy classifications. Does not have a price or SKU directly, those belong to Variants.
    ],

    [Variant], [Catalog], [
      A sellable, physical unit of a product (e.g., "Cotton T-Shirt, Red, Large"). Holds SKU, barcode, pricing, inventory tracking flag, and physical dimensions. Each variant belongs to exactly one product.
    ],

    [Master Variant], [Catalog], [
      The designated default variant shown on product listing pages. Every product with variants must have exactly one master variant.
    ],

    [Option Type], [Catalog], [
      Defines a configurable attribute class such as "Colour", "Size", or "Material". Option types are product-independent and reusable across the catalogue.
    ],

    [Taxonomy / Taxon], [Catalog], [
      A hierarchical categorisation tree for organising products. A Taxonomy (e.g., "Department") contains Taxons (e.g., "Clothing" → "Dresses" → "Evening Dresses") forming a nested set structure.
    ],

    [Cart], [Ordering], [
      An ephemeral collection of line items that represents a customer's intent to purchase. Carts automatically expire after seven days of inactivity. The cart is the initial state of the Order aggregate.
    ],

    [Line Item], [Ordering], [
      A single entry in an order or cart, referencing a specific product variant, quantity, and a price snapshot captured at the time of purchase to insulate historical orders from catalogue price changes.
    ],

    [Checkout State], [Ordering], [
      The sequential stage of the purchase process: Address → Delivery → Payment → Confirm → Complete. Progression is forward-only; cancellation is available from any pre-confirmation stage.
    ],

    [Payment Intent], [Payment], [
      A data structure representing a customer's intent to pay a specified amount. Follows a defined state machine through the gateway interaction lifecycle.
    ],

    [Payment Capture], [Payment], [
      A partial or full debit against a payment intent. Multiple captures can be performed against a single intent (e.g., capturing shipping cost after items ship).
    ],

    [Stock Item], [Inventory], [
      The current state of a product variant's availability at a specific warehouse. Maintains on-hand and reserved quantity counters with optimistic concurrency control to prevent overselling.
    ],

    [Stock Movement], [Inventory], [
      An immutable ledger entry recording every change to a stock item's quantity, the delta, the balance before and after, the reason, and the operating user. The single source of truth for inventory changes.
    ],

    [Refresh Token], [Identity], [
      A long-lived credential used to obtain new short-lived access tokens without re-authentication. Each token is single-use; presenting a previously consumed token triggers revocation of all tokens for that user.
    ],
  ),
  caption: [Ubiquitous Language Glossary: core domain terms, their owning bounded context, and their precise definitions as used throughout the codebase and this thesis.],
) <tbl-ubiquitous-language>

==== State Machines

Two explicit state machines govern the most critical transactional workflows in the system: the order checkout process and the payment intent lifecycle. Both are encoded in domain entities, validated before every state transition, and drive the sequence of user-facing and system-level actions.

===== Order Checkout State Machine

The order checkout state machine enforces a forward-only progression through five sequential states: Address, Delivery, Payment, Confirm, and Complete. Each state transition is triggered by a specific user action and validated by the domain entity before being committed. Figure @fig-order-state-machine depicts this lifecycle.

#figure(
  image("../../../images/diagrams/08-order-state-machine.png", width: 80%),
  caption: [Order checkout state machine: five sequential states with cancellation available from any pre-confirmation state. The forward-only constraint prevents regressing to earlier checkout stages.],
) <fig-order-state-machine>

The customer begins by providing a shipping address (Address state), selects a delivery method (Delivery state), chooses a payment method (Payment state), reviews the complete order summary (Confirm state), and finalises the purchase (Complete state). At any point before the Complete state, from Address through Confirm, the customer may cancel the checkout process, which terminates the order without financial consequence.

Once an order reaches the Complete state, it becomes finalised: the order record transitions to Pending status, inventory quantities are reserved for each line item, and the payment intent is processed. From this point forward, the order is immutable except for the cancel transition, which captures the cancellation timestamp and releases reserved inventory. This forward-only design ensures that at every stage of the checkout pipeline, the system can unambiguously determine the customer's position and the next required action.

===== Payment Intent State Machine

The payment intent state machine models the full lifecycle of a payment from creation through to terminal completion, reflecting the state transitions of the Stripe payment gateway while maintaining a parallel system-managed state for offline consistency. Figure @fig-payment-state-machine shows all states and transitions.

#figure(
  image("../../../images/diagrams/09-payment-state-machine.png", width: 80%),
  caption: [Payment intent lifecycle: the state machine reflects Stripe gateway states while maintaining a parallel system copy for offline operations and Bogus gateway compatibility. Terminal states are Failed, Canceled, and Refunded.],
) <fig-payment-state-machine>

A payment intent is created in the Pending state. It may transition directly to RequiresAction when the payment requires 3D Secure or Strong Customer Authentication, or to Processing when the payment method has been attached without additional authentication challenges. From RequiresAction, successful customer authentication advances the intent to Processing; failure or timeout moves it to Canceled.

From Processing, a successful charge transitions the intent to Succeeded, while a declined or errored charge moves it to Failed. From Succeeded, the funds may be captured, transferring them from the customer's account, or the intent may be cancelled before capture. A captured intent may later be refunded, returning funds to the customer and reaching the terminal Refunded state.

The system maintains its own copy of the payment state in parallel with the gateway's representation. This design decision serves two purposes. First, it enables the Bogus test gateway, a development-only implementation that simulates payment lifecycles without external calls, to operate against the same domain entities as the production Stripe gateway. Second, it allows the system to reason about payment state offline without querying the gateway API, which improves resilience during network interruptions and reduces external dependency during business operations.

=== C4 Architecture

The C4 model provides a structured approach to describing software architecture at four levels of abstraction: system context, container, component, and code. This section presents the first three levels for ReSys.Shop, omitting the code-level view as it falls within the scope of the implementation chapter. A deployment diagram complements the C4 views by showing the physical infrastructure.

==== System Context

The system context diagram positions ReSys.Shop within its environment, showing the human users who interact with the platform and the external systems on which it depends. Figure @fig-c4-context presents this highest-level view.

#figure(
  image("../../../images/diagrams/03-c4-context.png", width: 100%),
  caption: [System Context diagram: ReSys.Shop as a single system boundary with customer and administrator users on one side and external payment, email, storage, identity, and ML services on the other. The modular monolith internally handles all eight business domains within one boundary.],
) <fig-c4-context>

Two categories of human users interact with the platform. Customers browse the product catalogue, perform visual and keyword searches, manage a shopping cart, and complete multi-step checkout, all through the Vue 3 storefront SPA. Administrators manage the full product lifecycle, process orders, monitor inventory levels, and administer user accounts through the Vue 3 admin SPA.

The system depends on five external services. Stripe processes payment intents and sends webhook notifications when payment events occur, the backend validates these webhooks using Stripe's signature verification before acting on them. SendGrid delivers transactional emails such as order confirmations, password reset links, and shipping notifications. An S3-compatible object store persists product images uploaded through the admin interface. Google OAuth provides an alternative authentication path, allowing customers to sign in using their Google credentials. The Python ML Sidecar, deployed as a companion service within the Aspire orchestration boundary, generates image embeddings used by the catalogue's visual search feature.

==== Container

The container diagram decomposes ReSys.Shop into its deployable units, the processes and data stores that together constitute the running system. Figure @fig-c4-container presents this view.

#figure(
  image("../../../images/diagrams/04-c4-container.png", width: 100%),
  caption: [Container diagram showing the deployable units of ReSys.Shop: two Vue 3 SPAs, the .NET 10 API backend, the Python ML sidecar, PostgreSQL with pgvector, and Redis. Arrows indicate communication protocols between containers.],
) <fig-c4-container>

The system comprises six deployable containers. The Store SPA and Admin SPA, both Vue 3 applications served as static assets, handle all user interface concerns and communicate with the backend exclusively through the REST API over HTTPS. The API Backend, a .NET 10 application running on ASP.NET Core, contains all business logic across eight modules, exposes Carter minimal API endpoints, and orchestrates the MediatR CQRS pipeline for command and query processing. The Embedding Sidecar, a Python 3.12 FastAPI application, loads machine learning models into GPU or CPU memory and exposes HTTP endpoints for generating image embeddings.

Two persistent data stores support the platform. PostgreSQL 17 with the pgvector extension serves as the primary database, storing both relational transactional data across eight module-specific schemas and high-dimensional vector embeddings for visual similarity search. Redis 7 fills a dual role: as the second-level distributed cache backing the HybridCache abstraction, and as the persistent job store for Hangfire background job processing, enabling cart expiry, webhook dispatch, and periodic maintenance tasks to survive application restarts.

The communication topology reflects deliberate design constraints. The Vue SPAs call the backend synchronously over HTTPS, never directly accessing the database or external services, which ensures all security policies and data validation are enforced server-side. The backend communicates with PostgreSQL and Redis over internal TCP connections, with the ML sidecar over HTTP on the internal Docker network, and with external services over HTTPS. This design centralises all external integration through the backend container, simplifying security management and operational monitoring.

==== Component

The component diagram zooms into the API Backend container, revealing its internal structure: the modules, framework services, and cross-cutting concerns that compose the .NET application. Figure @fig-c4-component presents this view.

#figure(
  image("../../../images/diagrams/05-c4-component.png", width: 100%),
  caption: [Component diagram of the API Backend showing the Carter endpoint layer, the MediatR pipeline, the feature handlers, and eight supporting infrastructure components. The Python ML Sidecar is shown with its internal three-layer architecture alongside.],
) <fig-c4-component>

The API Backend is structured as a pipeline. HTTP requests arrive at the Carter endpoints, which are minimal API route groups registered by `ICarterModule` implementations in each module's feature folder. The endpoints are thin, they extract request parameters, dispatch a command or query via `ISender`, and map the `Result<T>` response to an HTTP status code and JSON body. All business logic resides in the feature handlers.

The MediatR pipeline wraps every request with a chain of behaviours: logging captures the request type and timing, validation executes FluentValidation rules before the handler runs, and exception mapping converts unhandled infrastructure failures to standardised problem details. The handlers themselves interact with eight infrastructure components:

- ApplicationDbContext (EF Core 10) with interceptors for auditable timestamps, soft-delete filtering, and row-version concurrency checks.
- A Specification DSL that provides composable `IQueryable` extensions for filtering, sorting, paging, and full-text search, keeping handler code free of query-building boilerplate.
- JWT authentication with ASP.NET Identity, managing access and refresh token issuance, rotation, reuse detection, and token blacklisting.
- A dynamic permission provider that resolves `{domain}:{category}:{action}` permission claims to authorisation policies at runtime without requiring static policy registration.
- A storage service with interchangeable providers (local filesystem or S3-compatible storage) selected via configuration, with built-in file-type validation and anti-forgery guards on uploads.
- A notification hub supporting email (SendGrid/SMTP) and SMS (Sinch) channels with configurable fallback priority.
- Hangfire for background job scheduling and processing, handling cart expiry, webhook dispatch, and periodic health checks.
- HybridCache with two-tier caching: L1 in-memory for sub-millisecond access and L2 Redis for cross-instance consistency.

The Python ML Sidecar follows a three-layer architecture: the FastAPI router handles HTTP request validation and API key authentication, the Embedding Service maintains a singleton model registry with lazy loading and caching, and the model implementations, Fashion-CLIP, ResNet-50, EfficientNet-B0, and generic CLIP, implement a common strategy interface for interchangeable inference backends.

==== Deployment

The deployment diagram illustrates how the containers map to physical or virtual infrastructure in a production configuration. Figure @fig-deployment shows the deployment topology.

#figure(
  image("../../../images/diagrams/10-deployment.png", width: 100%),
  caption: [Deployment diagram showing containerised services within an Aspire orchestration boundary. The API backend is horizontally scalable; the embedding sidecar is stateless; Redis enables distributed state across API replicas.],
) <fig-deployment>

All services are containerised and orchestrated by .NET Aspire, which manages service discovery, configuration injection, and health monitoring during both development and production deployments. The Vue SPAs are served as static bundles from a CDN or reverse proxy, while the backend services run within Docker containers on a single host or across a cluster.

The API backend is horizontally scalable, multiple container instances share PostgreSQL and Redis, enabling round-robin request distribution. The embedding sidecar is stateless: it loads models into memory on startup, caches them, and serves embedding requests without shared state. Any API instance can call any embedding container. Redis provides the distributed state needed for cache coherence and Hangfire job coordination across API replicas.

PostgreSQL is configured with a primary instance for writes and one or more read replicas for reporting and analytical queries. The pgvector extension is installed on both primary and replicas, enabling vector similarity search from any read path. External services, Stripe, SendGrid, S3 storage, and Google OAuth, are accessed over HTTPS from every API instance, with credentials managed through Aspire's configuration system and never baked into container images.

=== Database Design

The ReSys.Shop database is a single PostgreSQL 17 instance organised into per-context schemas, each owned by a bounded context and managed through Entity Framework Core migrations. This section describes the schema organisation, the core entity-relationship model, the pgvector integration for visual search, and the key design decisions that shape the data layer.

==== Schema Organisation

Each of the eight bounded contexts owns its database schema. The Catalog context manages tables for products, variants, variant images, option types, option values, taxonomies, and taxons. The Ordering context owns orders, line items, adjustments, shipments, and inventory units. The Payment context holds payment intents, payment captures, and payment logs. The Inventory context manages stock locations, stock items, stock movements, and stock reservations. The Identity context, built on ASP.NET Identity Core, manages users, roles, user roles, refresh tokens, and external login providers. The Profile context owns user addresses, wishlists, and notification preferences. The Shipping context manages shipping methods, shipping rates, and shipping zones. The Location context provides country and state reference tables.

Cross-context relationships are implemented through identifier references only, there are no foreign key constraints spanning module boundaries. An Order references a UserId (Identity context) and a VariantId (Catalog context), but these are stored as UUIDs without database-level referential integrity constraints. This design preserves the logical isolation of each context as a separate schema while keeping all data in a single database, avoiding the complexity of distributed transactions for the most common business operations.

Entity Framework Core manages all migrations from a dedicated Migrations assembly. Each migration is generated by comparing the current database state against the domain entity model, and migrations are applied as part of the application startup or through standalone migration scripts for production deployments.

==== Core Entity-Relationship Model

Figure @fig-erd-core presents the entity-relationship diagram for the core business entities across the Catalog and Ordering domains, the two contexts that participate most directly in the visual search and checkout workflows.

#figure(
  image("../../../images/diagrams/07-erd-core.png", width: 100%),
  caption: [Core entity-relationship diagram showing the primary domain entities and their relationships across the Catalog and Ordering bounded contexts. Soft deletion, audit columns, and GUID primary keys are used throughout.],
) <fig-erd-core>

The Catalog domain centres on the Product entity, which has a one-to-many relationship with Variant. Each Variant represents a specific sellable configuration, for example, "Cotton T-Shirt, Red, Large", with its own SKU, pricing, and inventory tracking flag. Exactly one variant per product is designated as the master variant. Variants relate one-to-many to VariantImages, each of which holds a file path and an optional `embedding` column of type `vector(512)` for pgvector similarity search. Products configure option types, such as Colour or Size, which in turn define option values. Taxonomies contain taxons in a hierarchical structure, with taxons referencing themselves through a parent foreign key to represent tree structures such as "Clothing → Dresses → Evening Dresses". Products are classified by taxons through a many-to-many classification table.

The Ordering domain centres on the Order entity, which has a one-to-many relationship with LineItem. Each line item references a specific variant, not a product directly, because customers purchase specific configurations. The line item captures a price snapshot at the time of purchase, ensuring that historical orders are unaffected by catalogue price changes. Orders are related to users through a UserId reference, optionally nullable to support guest checkout, and track a session identifier for mapping anonymous carts before authentication.

==== pgvector Integration

PostgreSQL's pgvector extension enables vector similarity search directly within the relational database, eliminating the need for a separate vector database. The `variant_images` table contains an `embedding` column of type `vector(512)`, a fixed-length array of 512 IEEE 754 single-precision floating-point numbers representing the visual features extracted from the image by the ML sidecar.

An HNSW (Hierarchical Navigable Small World) index is created on the embedding column to accelerate approximate nearest-neighbour searches. HNSW provides logarithmic search complexity, enabling sub-10-millisecond queries over tens of thousands of vectors. The index is configured for cosine distance, the recommended distance metric for normalised embeddings from CLIP-family models.

Vector similarity queries use the cosine distance operator (`<=>`), which computes the angular distance between two vectors. A representative conceptual query pattern is: retrieve all variant images, compute the cosine distance between each stored embedding and the query embedding, order by ascending distance, and return the top results. The system filters results by a configurable minimum similarity threshold, computed as 1 - cosine_distance, defaulting to 0.7, a level at which fashion images typically exhibit perceptible visual similarity.

Each embedding row includes a `model_name` column identifying which ML model generated the vector. This metadata enables per-model filtered queries: when the system switches the active embedding model, only embeddings from that model's columns participate in similarity search, preventing cross-model vector comparisons that would produce meaningless results. The model name also supports the benchmark evaluation in Chapter 3, where multiple models are compared against the same image corpus.

==== Key Design Decisions

Several design decisions govern the database layer and influence every bounded context:

*Universally Unique Identifiers.* All primary keys are UUIDs (GUIDs in .NET terminology). This decision eliminates reliance on a central sequence generator, allows safe distributed key generation, useful for client-side offline creation of cart or draft-order records, and prevents the information leakage inherent in auto-incrementing integer primary keys.

*Soft Deletion.* Most domain entities carry an `IsDeleted` boolean column. Entity Framework Core applies a global query filter that excludes soft-deleted rows from all standard queries. This preserves referential integrity, a deleted product does not orphan its historical orders, while keeping the active working set clean. Deleted entities can be restored by administrators if the deletion was in error.

*Audit Columns.* Every entity inherits `CreatedAtUtc` and `ModifiedAtUtc` timestamp columns from the `Entity` base class, populated automatically by an EF Core save interceptor. These columns support chronological analysis of catalogue changes, order lifecycle auditing, and debugging data anomalies without external logging correlation.

*Composite Indexes.* Tables serving high-frequency query patterns carry composite indexes tuned to their access patterns. The Orders table includes `(UserId, Status)` for listing a customer's orders filtered by state, and `(SessionId, Status)` for resolving anonymous carts. These composite indexes avoid the need for separate single-column indexes and reduce disk footprint.

*Variable Vector Dimensions.* Different embedding models produce vectors of different dimensionalities: 384 for DINOv2-S, 512 for Fashion-CLIP and CLIP, 768 for DINOv2-B, 1280 for EfficientNet-B0, and 2048 for ResNet-50. PostgreSQL's `vector` type supports this variability, the dimension is part of the column type declaration, and HNSW indexes are built per-dimension. The system stores embeddings from all models in separate rows, each tagged with the model name, and queries against the active model's dimension only.

==== Per-Context Schema Description

The Identity context stores user accounts with Argon2-hashed passwords, security stamps for session invalidation, and refresh tokens with one-time-use semantics. Role and UserRole tables implement RBAC, while UserLogin records link local accounts to external OAuth providers. UserAddresses store shipping and billing locations with ISO country codes.

The Catalog context's Product table carries fashion-specific metadata columns, style code, season name, material composition, department, and gender target, alongside standard catalogue fields. Variants hold SKU, barcode, cost and selling price in decimal precision, and physical dimensions for shipping calculations. The OptionType and OptionValue tables implement an EAV-lite pattern, allowing administrators to define new product attributes without schema migrations. Taxons use a nested set model with left and right bounds, enabling single-query subtree retrieval for mega-menus and breadcrumb navigation.

The Ordering context stores financial values as decimal with 18-digit precision and 2-decimal scale, avoiding floating-point accumulation errors in tax and discount calculations. Orders maintain separate subtotals for items, adjustments, and shipping, with the total computed as their sum. LineItems capture price snapshots at purchase time, decoupling order history from catalogue changes. OrderHistory provides an append-only audit trail of every state transition.

The Inventory context tracks stock items at a variant-location granularity, with `QuantityOnHand` and `QuantityReserved` maintained as separate counters and protected by optimistic concurrency control using PostgreSQL's `xmin` system column. StockMovements form an immutable ledger, every quantity change, whether from receiving, selling, returning, or stock-taking, is recorded with the delta, balances before and after, unit cost, and an external reference such as an order number or purchase order identifier.

=== API Design

The ReSys.Shop API exposes a RESTful interface built on Carter minimal APIs and organised around the MediatR CQRS pattern. This section describes the API architecture, the endpoint organisation scheme, and a summary of the key endpoints that define the platform's external contract.

==== API Architecture

The API layer acts as a thin orchestration boundary. It contains no business logic; instead, it delegates all processing to the MediatR pipeline. Each request follows a consistent path: the Carter endpoint receives the HTTP request, extracts route and body parameters, constructs a MediatR command or query object, dispatches it through `ISender`, and maps the returned `Result<T>` to an HTTP response. This design keeps endpoints concise, typically six to twelve lines, and concentrates all domain logic in the handler layer, where it is testable without HTTP infrastructure.

Carter modules group related endpoints by module and surface. Each module (Catalog, Ordering, Payment, and so on) registers its own `ICarterModule` implementation, which defines the route groups, HTTP methods, and parameter bindings for that module's endpoints. This modular registration avoids a single monolithic route configuration file and enables each bounded context to own its API surface.

FluentValidation provides input validation through validator classes associated with each command and query. Validators run automatically as part of the MediatR pipeline behaviour, before the handler executes, ensuring that handlers never receive invalid input. Validation failures return standardised `400 Bad Request` responses with field-level error details.

==== Endpoint Organisation

Endpoints are organised by two dimensions: the business module that owns the operation, and the surface, Admin or Storefront, that serves as the entry point. The URL pattern follows the convention `/api/{module}/{surface}/{action}`, where module identifies the owning bounded context, surface distinguishes administrative from customer-facing operations, and action names the specific operation.

This two-dimensional organisation serves several purposes. It makes the API self-documenting: the URL alone communicates which business area and which user role the endpoint targets. It simplifies authorisation: Admin surface endpoints share a common authorisation policy requiring an administrator role, while Storefront endpoints apply corresponding customer-level policies. And it enables independent versioning: a module can evolve its endpoints without affecting other modules.

Table @tbl-key-endpoints summarises the most architecturally significant endpoints across the platform. These endpoints represent the primary user-facing capabilities, visual search, catalogue browsing, checkout, order history, authentication, and payment, that together define the complete customer and administrator experience.

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    align: (left + horizon, left, center + horizon, left),

    table.header([*Endpoint*], [*Module*], [*Surface*], [*Description*]),

    [`POST /api/catalog/storefront/search-by-image`], [Catalog], [Storefront], [
      Accepts an uploaded image file, sends it to the ML sidecar for embedding generation, queries pgvector for the nearest neighbour variant images by cosine similarity, and returns matching products ranked by similarity score with variant thumbnails, pricing, and product URLs.
    ],

    [`GET /api/catalog/storefront/products/{slug}`], [Catalog], [Storefront], [
      Returns a product with all its published variants, images, option configurations, and taxonomy classifications, identified by its URL slug. Supports guest access for anonymous browsing.
    ],

    [`POST /api/ordering/storefront/cart/checkout`], [Ordering], [Storefront], [
      Advances the cart through the checkout state machine: setting the shipping address, selecting the delivery method, and confirming the order. Each call transitions the checkout state forward one step.
    ],

    [`GET /api/ordering/storefront/orders/{id}`], [Ordering], [Storefront], [
      Returns the complete order with line items, payment state, shipment state, and status history. Requires authentication; customers may only access their own orders.
    ],

    [`POST /api/identity/store/auth/login`], [Identity], [Storefront], [
      Authenticates a user by email and password, returning a JWT access token (fifteen-minute lifetime) and a refresh token for token rotation. Supports Google OAuth as an alternative login method via a related endpoint.
    ],

    [`POST /api/payment/storefront/payment/create-intent`], [Payment], [Storefront], [
      Creates a payment intent for the specified order amount and currency, initialising the payment state machine.
    ],
  ),
  caption: [Key API endpoints representing the primary user-facing capabilities of the platform. All endpoints in the Storefront surface serve customer interactions; Admin surface endpoints (not shown) mirror these with full CRUD capabilities on all modules.],
) <tbl-key-endpoints>

The admin surface provides full CRUD operations on all module entities, products, variants, orders, inventory, users, shipping methods, and location data, following the same URL pattern with the Admin surface prefix. These endpoints are excluded from the table to maintain focus on the core platform capabilities, but they follow identical architectural patterns: minimal API route groups, MediatR dispatch, FluentValidation, and permission-based authorisation.

=== Security Design

The security architecture of ReSys.Shop addresses three layers: authentication, verifying the identity of callers, authorisation, controlling what authenticated callers may do, and hardening, defensive measures against common attack vectors. This section describes each layer in turn.

==== Authentication

The platform uses JSON Web Tokens (JWT) for bearer token authentication. Upon successful login, via email and password or Google OAuth, the server issues two tokens: an access token with a fifteen-minute lifetime and a refresh token with a longer lifetime. The access token carries the user's identifier, email, and permission claims in a compact signed payload. All authenticated API requests include the access token in the `Authorization` header as a Bearer token.

The refresh token is a long-lived credential stored server-side in the database. When the access token expires, the client presents the refresh token to obtain a new access token and a new refresh token, a pattern known as refresh token rotation. Each refresh token is single-use: upon successful rotation, the consumed token is marked as used and a replacement is issued. If a previously consumed refresh token is presented again, indicating a potential token theft scenario, the system revokes all tokens for that user, forcing re-authentication. This rotation-with-reuse-detection pattern limits the damage window of a compromised refresh token to the interval between rotations.

Guest users, customers who have not yet authenticated, are assigned a session identifier stored in a browser cookie. This session identifier links their anonymous cart to their browsing context and persists across page navigations. Upon registration or login, the anonymous cart is merged with the authenticated user's cart, preserving the shopping intent built during the guest session.

==== Authorisation

Authorisation is implemented through two complementary mechanisms: role-based access control (RBAC) for broad category restrictions and permission-based claims for fine-grained control.

Roles, such as Customer and Administrator, segregate the Admin and Storefront surfaces. An endpoint in the Admin surface requires the Administrator role; a customer presenting valid credentials without that role receives a `403 Forbidden` response. This coarse check prevents unauthorised access to administrative functions at the infrastructure level, before any business logic executes.

Permissions use a structured claim format: `{domain}:{category}:{action}`. For example, `catalog:products:create` grants permission to create products in the Catalog domain. A dynamic permission provider, `IAuthorizationPolicyProvider`, resolves these claim strings to ASP.NET Core authorisation policies at runtime, eliminating the need for static policy registration for every endpoint. This dynamic resolution enables permission configuration through the database without redeployment: an administrator may create a new role, assign it a set of permission claims, and those permissions take effect across all authorised endpoints immediately.

==== Security Measures

Several defensive measures harden the platform against common web application attack vectors.

*Rate Limiting.* Authentication endpoints are rate-limited to five requests per minute per IP address to prevent credential brute-forcing. Registration endpoints are limited to three requests per hour per IP address to deter automated account creation. Payment endpoints are limited to thirty requests per minute to maintain availability during high-traffic checkout events.

*Security Headers.* All HTTP responses include security headers configured through ASP.NET Core middleware: Content-Security-Policy restricts script and style sources to the application's own domains, HTTP Strict-Transport-Security enforces HTTPS-only connections for a configurable duration, X-Frame-Options prevents the application from being embedded in iframes to block clickjacking, and X-Content-Type-Options prevents MIME-type sniffing by browsers.

*File Upload Validation.* The visual search and product image upload endpoints enforce strict file validation. Uploaded files undergo magic-byte verification, inspecting the file header bytes rather than trusting the file extension, to confirm they are valid JPEG, PNG, or WebP images. A ten-megabyte size limit prevents resource exhaustion from oversized uploads. Server-side validation repeats the client-side checks, as client-side validation is a convenience that an attacker can bypass.

*Payment Webhook Verification.* The Stripe webhook endpoint, which receives payment event notifications, validates each incoming request using Stripe's signature verification algorithm. The webhook payload is hashed with a shared signing secret; if the computed signature does not match the one provided in the Stripe-Signature header, the request is discarded before any business logic processes it. This verification prevents spoofed webhook payloads from injecting fraudulent payment state into the system.

==== Token Flow

The authentication token lifecycle operates as follows. A client authenticates with email and password, receiving an access token and a refresh token. The access token is short-lived and not stored server-side; it is validated by signature verification and expiration check on each request. When the access token expires, the client sends the refresh token to the refresh endpoint. The server validates the refresh token against the database: if it is valid and has not been used before, the server marks it as consumed, issues a new access token and a new refresh token, and returns both to the client. If the presented refresh token has already been consumed, flagged as used from a previous rotation, the server assumes token theft and revokes all refresh tokens associated with that user, logging the security event. The user must then re-authenticate, which invalidates the compromised token chain and issues fresh credentials. This model provides a self-healing defence against refresh token interception without requiring the user to detect or report the compromise.

=== Summary

This section has presented the architectural design of ReSys.Shop across six dimensions. The service-oriented system architecture separates presentation (Vue 3), business logic (.NET 10), and machine learning (Python sidecar) into independently deployable services. Domain-Driven Design partitions the business domain into eight bounded contexts communicating through MediatR in-process dispatch, with four architecturally significant aggregate roots enforcing explicit invariants. The C4 model describes the system at context, container, and component levels of abstraction, revealing the communication paths between deployable units and the internal composition of the .NET backend. The PostgreSQL database uses per-context schemas, pgvector for vector similarity search, and a set of consistent design decisions, GUIDs, soft deletion, audit columns, applied across all contexts. The API layer follows the URL convention `/api/{module}/{surface}/{action}` with Carter minimal APIs and MediatR CQRS. The security architecture covers JWT authentication with refresh token rotation, permission-based authorisation with dynamic policy resolution, and layered defensive measures against common attack vectors. Together, these architectural decisions provide the foundation on which the implementation described in the following section is built.
