== Architectural Style

=== Decision: Modular Monolith

*Decision*: Deploy the entire backend as a single ASP.NET Core process (`service/Api/src/Api`) containing 8 business modules within one assembly (`Module.csproj`).

*Alternatives considered*:

#figure(
  table(
    columns: (auto, 1fr, 1fr, 1fr),
    align: (start, start, start, start),
    [*Alternative*], [*Pros*], [*Cons*], [*Why rejected*],
    [*Microservices*], [Independent deployability, per-module scaling], [Operational complexity (service mesh, distributed tracing, eventual consistency for checkout), overkill for thesis scope], [Thesis requires demonstrability and simplicity over operational scale],
    [*Clean Architecture (per-module assemblies)*], [Strict layer boundaries via compiler], [Slower builds, excessive project count (8 modules × 4 layers = 32 projects), references still cross-cut], [Single assembly with namespace isolation is sufficient for a single team],
    [*Modular Monolith*], [Single deployable, in-process consistency, easier debugging, testable module isolation], [Cannot scale modules independently], [Fits the thesis constraint of "demonstrable single system" while keeping modules logically isolated],
  ),
  caption: [Architectural Style Alternatives],
)

*Justification*: A modular monolith gives us the compile-time boundaries of modular design (no cross-module references) without the operational overhead of microservices. All 8 modules share one `ApplicationDbContext` and one transaction boundary, which simplifies the checkout flow (order + payment + inventory update can be ACID). The trade-off --- inability to scale modules independently --- is acceptable because the thesis evaluates architectural process, not production operational scale.

*Rationale in depth*:

The rejection of microservices is grounded in the observation that distributed systems introduce *accidental complexity* (Brooks, 1986) that overshadows the essential complexity of the problem. For a single-developer thesis project, the operational burden of service discovery, distributed tracing, sagas for multi-service transactions, and independent deployment pipelines would consume the majority of the available time --- leaving insufficient capacity for the actual research contribution (CBIR integration and explicit error handling). As Newman (*Monolith to Microservices*, 2019) argues, microservices are a solution to organizational scale (Conway's Law), not technical scale. A single team does not experience the coordination friction that microservices are designed to solve.

The rejection of per-module Clean Architecture assemblies is similarly pragmatic. While 32 projects (8 modules × 4 layers) would enforce strict compile-time boundaries, the build overhead and reference management would dominate the development workflow. In a thesis timeline, the cost of waiting for incremental builds and resolving circular references across 32 `.csproj` files is not justified by the benefit. Namespace isolation within a single assembly, combined with the `ValidateVerticalSliceIsolation` target (intention, even if currently disabled), provides sufficient boundary enforcement for a demonstrable system.

The modular monolith, therefore, is not a compromise --- it is a *conscious architectural decision* that optimizes for the thesis constraints: demonstrability, single-team development, ACID consistency for checkout, and sufficient module isolation to evaluate the design patterns under study.

*Evidence*: `service/Api/src/Api/Program.cs:38-45` (8 `AddXxxModule()` calls), `service/Api/src/Module/Module.csproj:1-21` (single assembly)

=== Decision: Vertical Slice Architecture

*Decision*: Organize code by _feature_ rather than by _technical layer_. Each feature action lives in `Features/\{Admin\|Storefront\}/\{Feature\}/\{Action\}/` as a `static partial class` split across 5 files: Handler, Endpoint, Request, Response, and Validator.

*Justification*: Traditional horizontal layering (Controllers/Services/Repositories) scatters a single use case across the codebase. Vertical slicing makes each use case self-contained: a reviewer can understand "Create Product" entirely by reading one folder. This is critical for thesis evaluation because examiners can trace a requirement directly to its implementation without cross-referencing multiple layers.

*Evidence*: `Module/Catalog/Features/Admin/Products/Create/CreateProduct.cs`, `CreateProduct.Endpoint.cs`, `CreateProduct.Request.cs`, `CreateProduct.Response.cs`, `CreateProduct.Validator.cs`

=== Decision: CQRS via MediatR

*Decision*: Separate commands (mutations) from queries (reads) using MediatR `ICommand<TResponse>` and `IQuery<TResponse>` contracts. All feature handlers implement these interfaces.

*Justification*: CQRS decouples the transport layer (Carter minimal API endpoint) from the business logic. More importantly, it enables _pipeline behaviors_ --- cross-cutting concerns (logging, validation, exception mapping) that wrap every request without polluting handlers. This demonstrates the Decorator pattern in practice.

*Evidence*: `Shared/Application/Mediators/Commands/ICommand.cs`, `Shared/Application/Mediators/Queries/IQuery.cs`, `Shared/Application/Mediators/Mediator.Extension.cs:46-50` (pipeline registration)

=== Decision: Result Objects (Not Exceptions)

*Decision*: All domain and handler operations return `Result<T>` or `Result`. Exceptions are reserved for unrecoverable infrastructure failures only.

*Justification*: Exception-driven control flow hides error paths in implicit stack unwinding. `Result<T>` makes every failure path explicit, type-safe, and testable. This directly addresses the thesis objective of "predictable error handling." The design follows Railway-Oriented Programming principles.

*Evidence*: `Shared/Application/Models/Results/Result.cs:1-43`, `Result.Method.cs:84-152` (factory methods: `Result.NotFound`, `Result.Conflict`, `Result.Validation`)

== System Context

The C4 Context diagram describes the system's boundaries and external actors. Three primary actors interact with ReSys.Shop:

- *Customer (Storefront)*: Browses the catalog, searches by image, manages cart, and completes checkout via the Storefront SPA.
- *Administrator (Admin)*: Manages products, inventory, orders, users, and system configuration via the Admin SPA.
- *System (Webhooks)*: External services (e.g., Stripe) push events to the backend via webhook endpoints.

The backend is a single ASP.NET Core API (.NET 10) exposing REST endpoints under `/api`. It depends on:

- *PostgreSQL 17 + pgvector*: Primary relational database with vector similarity search support.
- *Redis 7*: Caching (HybridCache L2) and Hangfire job storage.
- *Python ML Sidecar*: FastAPI service running Fashion-CLIP and other embedding models for CBIR.

External integrations include Stripe (payments), SendGrid/SMTP (email), Sinch (SMS), Google OAuth (login), and S3 (file storage).

== Container Diagram

The C4 Container diagram details the runtime components:

- *Store SPA* (Vue 3 + Nuxt UI, port 5174): Customer-facing storefront application.
- *Admin SPA* (Vue 3 + PrimeVue, port 5173): Internal administration dashboard.
- Both SPAs communicate with the backend via HTTP REST API calls to `/api`.

The *ASP.NET Core API* (port 5035) is the central container, hosting:

- *Carter endpoints* (minimal API routing)
- *MediatR pipeline* (Logging → Validation → ExceptionMapping behaviors)
- *8 Module handlers* + Shared infrastructure

Downstream containers:

- *PostgreSQL 17 with pgvector*: Stores all relational data and vector embeddings.
- *Redis 7*: HybridCache and Hangfire persistence.
- *Embedding Sidecar* (port 8000): Python FastAPI service for image vector generation.
- *Stripe Gateway*: Webhook-based payment processing.

== Component Diagram (API Backend)

The C4 Component diagram reveals the internal architecture of the API host:

*MediatR Pipeline* (request processing chain):

1. `LoggingBehavior` --- logs entry with CorrelationId
2. `ValidationBehavior` --- FluentValidation short-circuit on errors
3. `ExceptionMappingBehavior` --- catches exceptions, maps to `Result.Unexpected`

Requests flow through the pipeline to one of three handler types:

- *Commands* (mutations): Create, Update, Delete operations
- *Queries* (reads): Get, List, Search operations
- *IPagedQuery* (paginated): Paginated list operations

All handlers interact with:

- *ApplicationDbContext* (EF Core) with interceptors: `AuditableInterceptor`, `SoftDeletableInterceptor`, `VersionableInterceptor`, and Specification DSL.

Cross-cutting concerns are provided as separate components:

- *Security*: JWT + OAuth authentication
- *Storage*: Local/S3 file storage (Strategy pattern)
- *Notifications*: Email + SMS (SendGrid/SMTP/Sinch)
- *Backgrounds*: Hangfire job processing

== Design Patterns

#figure(
  table(
    columns: (auto, 1fr, 1fr),
    align: (start, start, start),
    [*Pattern*], [*Location*], [*Justification*],
    [*Vertical Slice*], [Every `Features/\{Admin\|Storefront\}/\{Feature\}/\{Action\}/`], [Cohesion: all code for one use case in one place],
    [*CQRS*], [`ICommand<>`, `IQuery<>`, separate handlers], [Read/write optimization; pipeline behaviors],
    [*Pipeline (Decorator)*], [`LoggingBehavior → ValidationBehavior → ExceptionMappingBehavior`], [Cross-cutting concerns without AOP frameworks],
    [*Result / Railway*], [`Result<T>`, `Error`], [Explicit control flow; compiler-enforced error handling],
    [*Module Isolation*], [`AddXxxModule()` extension methods; no `Module.X` → `Module.Y` refs], [Independent reasoning about each domain],
    [*Strategy*], [`IStorageProvider` (Local/S3), `IGatewayRegistry` (Stripe/Bogus), `INotificationService` (SendGrid/SMTP/Sinch)], [Pluggable providers without touching call sites],
    [*Options + FluentValidation*], [Every settings type has a validator], [Fail-fast configuration errors at boot],
    [*Specification*], [`Shared/Operational/Persistence/Specifications/`], [Composable query expressions; declarative filtering],
    [*Repository (Pragmatic)*], [`IApplicationDbContext` as unit-of-work; `DbSet<T>` queried directly], [Avoids unnecessary abstraction; EF Core + interceptors suffice],
    [*Factory*], [Domain entity constructors are `internal`; creation via `MapToDomain()` / factory methods], [Enforces invariants at creation time],
  ),
  caption: [Design Patterns],
)

== Data Flow

=== Normal Request Flow

The standard request lifecycle follows this sequence:

```
HTTP Request
  → Carter Endpoint (ICarterModule.Map, AddEndpoints scans assemblies)
    → Endpoint calls sender.Send(new Command(request))
      → LoggingBehavior (log entry with CorrelationId)
        → ValidationBehavior (FluentValidation; short-circuit on errors → Result.Validation)
          → ExceptionMappingBehavior (try/catch → Result.Unexpected)
            → Command/Query Handler
              → Domain logic (factory methods, invariants)
              → EF Core SaveChanges / external API call
              → Mapster mapping to Response DTO
    → result.ToResult() → IResult with status code + JSON envelope
  → HTTP Response
```

*Evidence*: `Program.cs:54-65`, `Mediator.Extension.cs:46-50`, `Validation.Behavior.cs:1-67`, `Exception.Behavior.cs:1-42`

=== Image Search Flow (CBIR --- Model-Agnostic)

The image search flow enables customers to find visually similar products:

```
User uploads image
  → Storefront SPA POST /api/catalog/storefront/search-by-image
    → Backend receives image bytes
      → HTTP POST to Python sidecar /embeddings (Aspire service discovery)
        → Sidecar loads configured model (Fashion-CLIP / ResNet-50 / EfficientNet-B0 / CLIP-generic)
        → Sidecar generates vector (dimension varies: 512, 2048, 1280, 512)
      → Backend receives vector + model_name
        → EF Core + pgvector: cosine similarity search filtered by model_name
      → Mapster maps results to Product DTOs
    → JSON response with similar products
```

*Model abstraction*: The sidecar exposes `POST /embeddings` with a configurable `model` parameter (default from env var `EMBEDDING_MODEL`). Each model implements `BaseEmbeddingModel` with `encode_image()` → `np.ndarray`. The database stores `model_name` alongside each embedding to enable per-model indexing and comparison.

*Evidence*: `ImageEmbedding.Inference.cs:21-36`, `Vector.Configuration.cs:1-30+`, `ApiTests/Catalog/Storefront/search-by-image.http`

=== Model Comparison Flow (Evaluation)

The evaluation flow benchmarks multiple embedding models against a ground-truth dataset:

```
Ground-truth dataset (100 images, 10 similarity groups)
  → For each model in [Fashion-CLIP, ResNet-50, EfficientNet-B0, CLIP-generic]:
    → Configure sidecar: EMBEDDING_MODEL=<model_name>
    → Restart sidecar (model swap)
    → Generate embeddings for all catalog images
    → For each query image in ground-truth:
      → POST /embeddings → receive vector
      → Query pgvector top-20
      → Compare retrieved variants against labeled group
      → Record: Precision@20, Recall@20, latency_ms, vector_dim
    → Compute mean ± SD across 100 queries
  → Generate comparison table
```

*Evidence*: `11-evaluation.md:§11.5`

=== Checkout Flow (Critical Path)

The checkout flow is the most critical data path, requiring ACID consistency across order creation, payment intent, and inventory reservation:

```
Cart items present
  → POST /api/ordering/storefront/cart/checkout
    → CreateOrderFromCart handler
      → Validate cart not empty, items in stock
      → Generate order number inside DB transaction (RepeatableRead)
      → Create Order entity with line items
      → Create Payment Intent via gateway (Stripe/Bogus)
      → SaveChanges (Order + PaymentIntent)
    → Return Order DTO with payment client secret
```

*Evidence*: `CreateOrderFromCart.cs`, `Order.cs`, `PaymentIntent.cs`, git log: commits `887a77c7`, `bd042088`

== Evidence

#list(
  [`service/Api/src/Api/Program.cs:1-66` --- composition root and module wiring],
  [`service/Api/src/Shared/Application/Mediators/Mediator.Extension.cs:1-79` --- MediatR + pipeline behaviors],
  [`service/Api/src/Shared/Application/Models/Results/Result.cs:1-43`, `Result.Method.cs:1-191` --- Result pattern],
  [`service/Api/src/Module/Catalog/Features/Admin/Products/Create/*.cs` --- vertical slice anatomy],
  [`service/Api/src/Shared/Operational/Storages/Storage.Extensions.cs:1-115` --- Strategy pattern for storage],
  [`infra/Aspire/src/ReSys.AppHost/AppHost.cs:1-49` --- Aspire orchestration wiring],
  [`service/Embedding/src/main.py:1-29` --- Python sidecar entry],
)
