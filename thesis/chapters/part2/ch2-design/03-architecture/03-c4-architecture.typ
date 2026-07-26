=== C4 Architecture

The C4 model provides a structured approach to describing software architecture at four levels of abstraction: system context, container, component, and code. This section presents the first three levels for ReSys.Shop, omitting the code-level view as it falls within the scope of the implementation chapter. A deployment diagram complements the C4 views by showing the physical infrastructure.

==== System Context

The system context diagram positions ReSys.Shop within its environment, showing the human users who interact with the platform and the external systems on which it depends. @fig-c4-context presents this highest-level view.

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_c4-context.png", width: 100%),
  caption: [System Context diagram: ReSys.Shop as a single system boundary with customer and administrator users on one side and external payment, email, storage, identity, and ML services on the other. The modular monolith internally handles all eight business domains within one boundary.],
) <fig-c4-context>

Two categories of human users interact with the platform. Customers browse the product catalogue, perform visual and keyword searches, manage a shopping cart, and complete multi-step checkout, all through the Vue 3 storefront SPA. Administrators manage the full product lifecycle, process orders, monitor inventory levels, and administer user accounts through the Vue 3 admin SPA.

The system depends on five external services. Stripe processes payment intents and sends webhook notifications when payment events occur, the backend validates these webhooks using Stripe's signature verification before acting on them. SendGrid delivers transactional emails such as order confirmations, password reset links, and shipping notifications. An S3-compatible object store persists product images uploaded through the admin interface. Google OAuth provides an alternative authentication path, allowing customers to sign in using their Google credentials. The Python ML Sidecar, deployed as a companion service within the Aspire orchestration boundary, generates image embeddings used by the catalogue's visual search feature.

==== Container

The container diagram decomposes ReSys.Shop into its deployable units, the processes and data stores that together constitute the running system. @fig-c4-container presents this view.

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_c4-container.png", width: 100%),
  caption: [Container diagram showing the deployable units of ReSys.Shop: two Vue 3 SPAs, the .NET 10 API backend, the Python ML sidecar, PostgreSQL with pgvector, and Redis. Arrows indicate communication protocols between containers.],
) <fig-c4-container>

The system comprises six deployable containers. The Store SPA and Admin SPA, both Vue 3 applications served as static assets, handle all user interface concerns and communicate with the backend exclusively through the REST API over HTTPS. The API Backend, a .NET 10 application running on ASP.NET Core, contains all business logic across eight modules, exposes Carter minimal API endpoints, and orchestrates the MediatR CQRS pipeline for command and query processing. The Embedding Sidecar, a Python 3.12 FastAPI application, loads machine learning models into GPU or CPU memory and exposes HTTP endpoints for generating image embeddings.

Two persistent data stores support the platform. PostgreSQL 17 with the pgvector extension serves as the primary database, storing both relational transactional data across eight module-specific schemas and high-dimensional vector embeddings for visual similarity search. Redis 7 fills a dual role: as the second-level distributed cache backing the HybridCache abstraction, and as the persistent job store for Hangfire background job processing, enabling cart expiry, webhook dispatch, and periodic maintenance tasks to survive application restarts.

The communication topology reflects deliberate design constraints. The Vue SPAs call the backend synchronously over HTTPS, never directly accessing the database or external services, which ensures all security policies and data validation are enforced server-side. The backend communicates with PostgreSQL and Redis over internal TCP connections, with the ML sidecar over HTTP on the internal Docker network, and with external services over HTTPS. This design centralises all external integration through the backend container, simplifying security management and operational monitoring.

==== Component

The component diagram zooms into the API Backend container, revealing its internal structure: the modules, framework services, and cross-cutting concerns that compose the .NET application. @fig-c4-component presents this view.

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_c4-component.png", width: 100%),
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

The deployment diagram illustrates how the containers map to physical or virtual infrastructure in a production configuration. @fig-deployment shows the deployment topology.

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_deployment.png", width: 100%),
  caption: [Deployment diagram showing containerised services within an Aspire orchestration boundary. The API backend is horizontally scalable; the embedding sidecar is stateless; Redis enables distributed state across API replicas.],
) <fig-deployment>

All services are containerised and orchestrated by .NET Aspire, which manages service discovery, configuration injection, and health monitoring during both development and production deployments. The Vue SPAs are served as static bundles from a CDN or reverse proxy, while the backend services run within Docker containers on a single host or across a cluster.

The API backend is horizontally scalable, multiple container instances share PostgreSQL and Redis, enabling round-robin request distribution. The embedding sidecar is stateless: it loads models into memory on startup, caches them, and serves embedding requests without shared state. Any API instance can call any embedding container. Redis provides the distributed state needed for cache coherence and Hangfire job coordination across API replicas.

PostgreSQL is configured with a primary instance for writes and one or more read replicas for reporting and analytical queries. The pgvector extension is installed on both primary and replicas, enabling vector similarity search from any read path. External services, Stripe, SendGrid, S3 storage, and Google OAuth, are accessed over HTTPS from every API instance, with credentials managed through Aspire's configuration system and never baked into container images.
