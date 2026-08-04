=== C4 Architecture

The C4 model structures software architecture across four abstraction levels: system context, container, component, and code. This section presents the first three C4 levels for ReSys.Shop alongside a deployment view, omitting code-level structures addressed in later implementation sections.

==== System Context

The system context positions ReSys.Shop within its operational environment, defining user roles and external dependencies (@fig-c4-context).

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_c4-context.png", width: 100%),
  caption: [System Context diagram: ReSys.Shop system boundary showing user roles and external integration dependencies.],
) <fig-c4-context>

The platform interacts with two human user groups:
- *Customers:* Browse the catalog, run visual and keyword searches, manage carts, and complete checkouts via the Vue 3 storefront SPA.
- *Administrators:* Manage products, process orders, track inventory, and administer user accounts via the Vue 3 admin SPA.

Five external integrations support platform operations:
- *Stripe:* Manages payment intent lifecycles and sends webhook notifications validated locally via Stripe signature verification.
- *SendGrid:* Dispatches transactional emails including order confirmations, password reset links, and shipping updates.
- *S3-Compatible Storage:* Persists product assets uploaded through the admin interface.
- *Google OAuth:* Offers customer single sign-on authentication.
- *Python ML Sidecar:* Generates image embeddings for visual search within the Aspire orchestration boundary.

==== Container

The container view decomposes ReSys.Shop into six standalone deployable processes and data stores (@fig-c4-container).

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_c4-container.png", width: 100%),
  caption: [Container diagram showing Vue 3 SPAs, .NET 10 API backend, Python ML sidecar, PostgreSQL with pgvector, and Redis.],
) <fig-c4-container>

The deployable units comprise:
- *Store & Admin SPAs:* Vue 3 single-page applications served as static assets, interacting with the backend strictly over HTTPS REST endpoints.
- *API Backend:* A .NET 10 (ASP.NET Core) application executing domain logic across eight modules via Carter minimal APIs and MediatR CQRS pipelines.
- *Embedding Sidecar:* A Python 3.12 FastAPI service loading ML models into GPU/CPU memory to generate image embeddings on demand over internal HTTP.
- *PostgreSQL 17 (with pgvector):* Stores relational domain schemas and high-dimensional vector embeddings for visual similarity search.
- *Redis 7:* Serves as an L2 distributed cache for `HybridCache` and a persistent job store for Hangfire background processing.

Communication is strictly centralized through the API Backend. SPAs never query databases or external APIs directly, enforcing all security boundaries server-side.

==== Component

The component view details the internal structure of the API Backend container (@fig-c4-component).

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_c4-component.png", width: 100%),
  caption: [Component diagram detailing the API Backend architecture and the three-layer Python ML sidecar.],
) <fig-c4-component>

HTTP requests enter through Carter minimal API modules, which validate parameters and dispatch commands or queries via MediatR's `ISender`. MediatR pipelines wrap execution in logging, FluentValidation checks, and global exception-handling behaviors.

Handlers delegate infrastructure tasks to eight internal components:
1. *ApplicationDbContext:* EF Core 10 context managing interceptors for auditing, soft-deletes, and optimistic concurrency.
2. *Specification DSL:* Composable `IQueryable` extensions for filtering, sorting, paging, and full-text search.
3. *Identity Provider:* ASP.NET Identity with JWT management handling access/refresh token rotation and revocation.
4. *Dynamic Permission Provider:* Resolves `{domain}:{category}:{action}` policy claims dynamically at runtime.
5. *Storage Service:* Provides interchangeable local or S3-compatible file storage with upload validation.
6. *Notification Hub:* Routes email (SendGrid/SMTP) and SMS (Sinch) with fallback routing.
7. *Hangfire Engine:* Executes background tasks including cart expiration, webhook dispatch, and maintenance jobs.
8. *HybridCache:* Combines L1 in-memory caching with L2 Redis caching for cross-instance consistency.

The Python ML Sidecar uses a three-layer layout: a FastAPI router for request validation, a singleton model registry for lazy loading, and an interchangeable strategy interface supporting Fashion-CLIP, ResNet-50, EfficientNet-B0, and standard CLIP.

==== Deployment

The deployment diagram illustrates the production infrastructure topology (@fig-deployment).

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_deployment.png", width: 100%),
  caption: [Deployment topology showing containerized orchestration under .NET Aspire with horizontal scaling.],
) <fig-deployment>

All services run as Docker containers orchestrated by .NET Aspire for service discovery, configuration injection, and health monitoring. Static Vue SPA bundles deploy to a CDN or reverse proxy.

The API Backend scales horizontally across container replicas sharing PostgreSQL and Redis. The stateless ML sidecar scales independently, serving vector generation requests from any API instance. PostgreSQL runs a primary instance for writes alongside read replicas for analytical queries, with `pgvector` enabled across all nodes. External secrets and API keys are injected via Aspire configuration environments at runtime.