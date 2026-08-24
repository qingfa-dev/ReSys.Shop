=== C4 Architecture

The C4 model structures software architecture across four abstraction levels: system context, container, component, and code. This section presents the first three C4 levels, omitting code-level structures addressed in later implementation sections.

==== System Context

The system context shows how ReSys.Shop fits into its operating environment, including user roles and external dependencies (@fig-c4-context).

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_c4-context.png", width: 100%),
  caption: [System Context diagram: ReSys.Shop system boundary showing user roles and external integration dependencies.],
) <fig-c4-context>

The platform interacts with two human user groups:
- *Customers:* Browse the catalog, run visual and keyword searches, manage carts, and complete checkouts.
- *Administrators:* Manage products, process orders, track inventory, and administer user accounts.

Five external integrations:
- *Stripe:* Payment intent lifecycles and webhook notifications via signature verification.
- *SendGrid:* Transactional emails (order confirmations, password resets, shipping updates).
- *S3-Compatible Storage:* Product asset persistence.
- *Google OAuth:* Customer single sign-on authentication.
- *Python ML Sidecar:* Image embedding generation within the container orchestration boundary.

==== Container

The container view decomposes ReSys.Shop into six standalone deployable processes and data stores (@fig-c4-container).

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_c4-container.png", width: 100%),
  caption: [Container diagram showing Vue 3 SPAs, .NET 10 API backend, Python ML sidecar, PostgreSQL with pgvector, and Redis.],
) <fig-c4-container>

The deployable units:
- *Store & Admin SPAs:* Vue 3 single-page applications interacting with the backend over HTTPS REST endpoints.
- *API Backend:* .NET 10 application executing domain logic via Carter minimal APIs and MediatR CQRS pipelines.
- *Embedding Sidecar:* Python 3.12 FastAPI service loading ML models into GPU/CPU memory for on-demand embedding generation.
- *PostgreSQL 17 (with pgvector):* Relational domain schemas and high-dimensional vector embeddings.
- *Redis 7:* L2 distributed cache for `HybridCache` and persistent job store for Hangfire.

All communication routes through the API Backend; SPAs never query databases or external APIs directly.

==== Component

The component view details the internal structure of the API Backend container (@fig-c4-component).

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_c4-component.png", width: 100%),
  caption: [Component diagram detailing the API Backend architecture and the three-layer Python ML sidecar.],
) <fig-c4-component>

HTTP requests enter through Carter modules, which validate parameters and dispatch commands or queries via MediatR's `ISender` through logging, validation, and exception-handling pipeline behaviors.

Handlers delegate infrastructure tasks to eight internal components:
1. *ApplicationDbContext:* EF Core context with interceptors for auditing, soft-deletes, and concurrency.
2. *Specification DSL:* Composable `IQueryable` extensions for filtering, sorting, paging, and full-text search.
3. *Identity Provider:* ASP.NET Identity with JWT management.
4. *Dynamic Permission Provider:* Runtime resolution of `{domain}:{category}:{action}` policy claims.
5. *Storage Service:* Interchangeable local or S3-compatible file storage with upload validation.
6. *Notification Hub:* Email (SendGrid/SMTP) and SMS (Sinch) routing with fallback support.
7. *Hangfire Engine:* Background task execution (cart expiration, webhook dispatch, maintenance).
8. *HybridCache:* L1 in-memory and L2 Redis caching combined for cross-instance consistency.

The Python ML Sidecar uses a three-layer layout: a FastAPI router, a singleton model registry for lazy loading, and an interchangeable strategy interface supporting Fashion-CLIP, ResNet-50, EfficientNet-B0, and standard CLIP.

==== Deployment

The deployment diagram illustrates the production infrastructure topology (@fig-deployment).

#figure(
  image("../../../../figures/chapters/part2/ch2-design/03-architecture/diagrams/P2S2.2.3_deployment.png", width: 100%),
  caption: [Deployment topology showing containerized orchestration with horizontal scaling.],
) <fig-deployment>

All services run as Docker containers orchestrated via Docker Compose for service discovery and health monitoring. The API Backend scales horizontally across replicas sharing PostgreSQL and Redis. The stateless ML sidecar scales independently. PostgreSQL runs a primary instance for writes alongside read replicas, with `pgvector` enabled across all nodes. Secrets and API keys are injected via environment configuration.