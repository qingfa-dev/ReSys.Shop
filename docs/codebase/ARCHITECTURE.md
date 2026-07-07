Short summary

High-level architecture observed from repo files and AGENTS.md.

Architecture overview
- Modular monolith split into projects: `Api` (host), `Module` (business logic), `Shared` (infrastructure), `Migrations`.
- CQRS via MediatR patterns and handlers; HTTP endpoints implemented as Carter minimal API endpoints that delegate to MediatR handlers.
- Pipeline behaviors: `LoggingBehavior → ValidationBehavior → ExceptionMappingBehavior` (described in AGENTS.md).
- Aspire AppHost (under `infra/Aspire`) orchestrates local dev environment and runs the API + embedding + frontends.

Data flow
- HTTP endpoints (Carter) → MediatR commands/queries → Module handlers → EF Core repositories → PostgreSQL
- Background jobs via Hangfire for asynchronous processing

Evidence
- [AGENTS.md](AGENTS.md)
- [service/Api/src/Module](service/Api/src/Module)
- [infra/Aspire/src/ReSys.AppHost/ReSys.AppHost.csproj](infra/Aspire/src/ReSys.AppHost/ReSys.AppHost.csproj)

[Decision]
- Team decision: keep the modular monolith. Aspire remains the local orchestrator for development; long-term production decomposition is [TODO].
# Architecture

## Core Sections (Required)

### 1) Architectural Style

- Primary style: Modular monolith with CQRS vertical slices
- Why this classification: The solution has a single API host (`Api.csproj`) that references a single `Module` project containing all business logic. Modules are organized as vertical slices under `Module/{ModuleName}/Features/{Admin|Storefront}/{Feature}/{Action}/`. Each module exposes itself via an `IServiceCollection` extension but modules never reference each other. This is enforced by build-time conventions, not the currently-disabled `ValidateVerticalSliceIsolation` target (`Directory.Build.targets:42-53`).
- Primary constraints:
  - TreatWarningsAsErrors — any warning fails the build (`Directory.Build.props:17`)
  - Central Package Management — all NuGet versions in one file (`Directory.Packages.props:4`)
  - Modules share `Shared/` infrastructure but must not cross-reference each other

### 2) System Flow

```text
HTTP request
  → Carter minimal API endpoint (Shared/Application/Endpoints/)
  → LoggingBehavior (request/response logging)
  → ValidationBehavior (FluentValidation rules)
  → Command/Query Handler (MediatR ICommand<> / IQuery<>)
  → Domain logic (Module/{Module}/Domain/)
  → EF Core / Storage / External API (Shared/Operational/)
  → Mapster DTO mapping (response projection)
  → ExceptionMappingBehavior (structured API error response)
  → HTTP response
```

Evidence for each step:
- Carter endpoints registered in `Shared/Application/Endpoints/Endpoint.Extension.cs` via `AddApplication()`
- MediatR pipeline behaviors registered in `Shared/Application/Mediators/Mediator.Extension.cs`
- Behaviors: `Shared/Application/Mediators/Behaviours/` — Logging, Validation, ExceptionMapping
- Handlers: `Module/{Module}/Features/{Admin|Storefront}/{Feature}/{Action}/` with `*Handler.cs` suffix
- Domain logic: `Module/{Module}/Domain/` with entity partial classes
- Data access: `Shared/Operational/Persistence/` (EF Core context, specifications, interceptors)
- Response mapping: `Shared/Application/Mappings/` (IdentityResult, ValidationResult mappers)

### 3) Layer/Module Responsibilities

| Layer or module | Owns | Must not own | Evidence |
|-----------------|------|--------------|----------|
| **Api** (host) | Program.cs (DI wiring, middleware order), appsettings, database initialization on startup | Any business logic, domain models, or EF Core entities | `service/Api/src/Api/Program.cs` |
| **Shared** | Cross-cutting infrastructure: EF Core context, Identity configuration, JWT auth, storage providers (Local/S3/Azure), HybridCache, Hangfire jobs, FluentEmail/Sinch notifications, OpenTelemetry, health checks, Swagger/Scalar UI, anti-forgery, correlation middleware, result types (Result\<T\>, PagedResult\<T\>) | Business-specific domain logic, module-to-module orchestration | `service/Api/src/Shared/Shared.csproj` |
| **Module** (assembly) | All business modules in a single project, shared GlobalUsing.cs | Cross-cutting infrastructure (that's in Shared) | `service/Api/src/Module/Module.csproj` |
| **Catalog module** | Products, variants, variant images/embeddings, prices, taxonomies, taxons, rules, option types, classifications, storefront product search/filter/similar | User identity, locations, profiles, inventory (separate module) | `service/Api/src/Module/Catalog/Catalog.Extension.cs` |
| **Identity module** | Users, roles, permissions, auth flows (login/register/logout/refresh), email confirmation, password reset, external OAuth login, session management | Product data, locations, profiles | `service/Api/src/Module/Identity/Identity.Extensions.cs` |
| **Inventory module** | Stock items, stock locations, stock reservations, stock transfers, stock movements | Product catalog data, user profiles | `service/Api/src/Module/Inventory/Inventory.Extension.cs` |
| **Location module** | Countries, states/provinces, lookup by ID or ISO code | User data, product data | `service/Api/src/Module/Location/Locations.Extensions.cs` |
| **Profile module** | User profiles, addresses, wishlists, notification preferences | Authentication/authorization, product data | `service/Api/src/Module/Profile/Profiles.Extensions.cs` |
| **Migrations** | EF Core migration files, database schema snapshot, migration guide | Application logic, domain models | `service/Api/src/Migrations/Api.Migrations.csproj` |
| **Embedding service** | Python ML inference (Fashion-CLIP), embedding generation API, image preprocessing, embedding caching | .NET business logic, database access, user management | `service/Embedding/src/main.py` |

### 4) Reused Patterns

| Pattern | Where found | Why it exists |
|---------|-------------|---------------|
| CQRS (MediatR) | All feature endpoints use `ICommand<>` / `IQuery<>` handlers | Separates read and write concerns; enables pipeline behaviors (logging, validation, exception mapping) |
| Result Object Pattern | `Shared/Application/Models/Results/` — `Result<T>`, `ValueResult<T>`, `PagedResult<T>` | Domain operations return explicit success/failure instead of throwing exceptions; error handling is explicit and composable |
| Specification Pattern | `Shared/Operational/Persistence/Specifications/` | Composable query filters for EF Core; enables dynamic filtering, sorting, and paging |
| Vertical Slice (Feature Folders) | `Module/{Module}/Features/{Admin\|Storefront}/{Feature}/{Action}/` | Each feature action is a self-contained folder with Handler, Request, Response, Validator, Mapper; avoids layer-based coupling |
| Domain Partial Classes | `Module/{Module}/Domain/{Entity}/*.cs` — entities split across multiple files (Constant, Extensions, Loggers, Methods, Result, Validation) | Splits concerns while keeping entity as single logical class; consistent file-per-concern pattern |
| Extension Methods (DI registration) | `{Module}/Extensions.cs` files — `AddCatalogModule()`, `AddIdentityModule()`, etc. | Composable service registration; each module registers its own services independently |
| Pipeline Behaviors | `Shared/Application/Mediators/Behaviours/` | Cross-cutting concerns (logging, validation, exception mapping) applied uniformly to all handlers |
| Storage Provider Abstraction | `Shared/Operational/Storages/Providers/` — IStorageProvider with Local, S3, Azure implementations | Swappable storage backends; file operations are provider-agnostic |

### 5) Known Architectural Risks

- **Module cross-reference enforcement is disabled**: `ValidateVerticalSliceIsolation` target in `Directory.Build.targets:44` is gated with `Condition="false"`. Module isolation relies on convention and code review, not build enforcement.
- **Monolith scaling**: All modules compile into a single deployable; scaling requires scaling the entire API instance. Background jobs (Hangfire) share the same process/connection pool unless externalized.
- **Embedding service is a separate process**: The Python ML sidecar is not integrated into the .NET DI pipeline; the communication protocol (HTTP/gRPC) and failure handling need to be defined. The service exists but may not be fully operational (see CONCERNS.md).
- **No API gateway or reverse proxy defined**: Frontends proxy `/api` to `localhost:5035` in dev; production routing strategy is not yet defined.
- **Shared.csproj has many dependencies**: The Shared project references nearly every production package, creating a large coupling surface. A change to any infrastructure package affects all modules.

### 6) Evidence

- `service/Api/src/Api/Program.cs` — API entry point showing DI wiring order and middleware pipeline
- `service/Api/src/Shared/Application/Mediators/Mediator.Extension.cs` — MediatR registration with pipeline behaviors
- `service/Api/src/Shared/Application/Endpoints/Endpoint.Extension.cs` — Carter endpoint discovery
- `service/Api/src/Module/Catalog/Catalog.Extension.cs` — Module DI extension pattern
- `Directory.Build.targets` — Architecture validation targets (layer dependency enforcement)
- `service/Api/src/Shared/Application/Models/Results/` — Result object pattern implementations
- `infra/Aspire/src/ReSys.AppHost/AppHost.cs` — Service topology (all services wired together)
