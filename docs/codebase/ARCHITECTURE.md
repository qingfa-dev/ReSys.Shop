# Architecture

## Core Sections (Required)

### 1) Architectural Style

- Primary style: **Modular monolith with clean architecture layering** (Shared infrastructure → Business modules → API presentation)
- Why this classification: Four .NET projects (Api, Shared, Module, Migrations) in one solution with defined dependency direction: Api → Module → Shared (infra), Module → Shared (domain/infra). Modules are fully independent from each other (no cross-module references). `Directory.Build.targets` enforces domain layer purity.
- Primary constraints: (1) Domain projects (if separated) cannot reference Infrastructure; (2) Modules cannot reference each other (enforced by `ValidateVerticalSliceIsolation` build target); (3) All feature logic is expressed via MediatR handlers behind Carter endpoints

### 2) System Flow

```text
[HTTP request]
  → Carter endpoint (Api/Module)
    → MediatR pipeline (Logging → Validation → ExceptionMapping)
      → Command/Query Handler
        → Domain logic + EF Core / Storage / External APIs
          → Response (Mapster-mapped DTO)
  → HTTP response
```

When run via Aspire: AppHost starts PostgreSQL (pgvector), Redis, API, Embedding service (FastAPI), Admin SPA, and Store SPA. The API orchestrates everything; Embedding is called for image vector similarity searches. Frontends proxy `/api` to the API via Vite dev server.

### 3) Layer/Module Responsibilities

| Layer or module | Owns | Must not own | Evidence |
|-----------------|------|--------------|----------|
| `Api/` | DI composition, middleware pipeline, config binding, HTTPS redirection | Business logic, domain entities | `Api/Program.cs` |
| `Shared/` | Persistence (EF Core DbContext), authentication, authorization, caching, storage abstraction, observability, notification channels, background jobs | Module-specific features | `Shared/Shared.csproj`, `Shared/Application/Application.Extension.cs` |
| `Module/Catalog/` | Product, variant, taxonomy, option-type domain logic and endpoints | Identity, location, profile data | `Module/Catalog/Catalog.Extension.cs` |
| `Module/Identity/` | User, role, permission management | Catalog products, profile addresses | `Module/Identity/Identity.Extensions.cs` |
| `Module/Location/` | Country/state data and lookups | Payment processing | `Module/Location/Locations.Extensions.cs` |
| `Module/Profile/` | User profiles, addresses, wishlists, preferences, notification settings | Authentication, catalog data | `Module/Profile/Profiles.Extensions.cs` |
| `Migrations/` | EF Core migration history, schema definitions | Business logic | `Migrations/Migrations/` |
| `infra/Aspire/` | Service orchestration, health checks, OpenTelemetry, resource lifecycle | Any feature layer | `infra/Aspire/src/ReSys.AppHost/AppHost.cs` |

### 4) Reused Patterns

| Pattern | Where found | Why it exists |
|---------|-------------|---------------|
| Mediator (MediatR) | All feature handlers via `ICommand<>`, `IQuery<>`, `ICommandHandler<,>`, `IQueryHandler<,>` | Decouples request dispatch from handler logic |
| Pipeline behavior | `LoggingBehavior`, `ValidationBehavior`, `ExceptionMappingBehavior` (MediatR pipeline) | Cross-cutting concerns before/after handler execution |
| Repository abstraction | EF Core DbContext via `IApplicationDbContext` wrapper | Testability and persistence isolation |
| Carter endpoints | All feature `Get/ById`, `Post/Create`, etc. | Minimal API registration without controllers |
| FluentValidation validators | One validator per command/query | Declarative input validation at the boundary |
| Mapster mapping | `Mappings/` folder per feature with `MappingConfig` | Object-to-object mapping between domain and DTOs |
| Strategy pattern | `IStorageProvider` (Local/S3/Azure) | Pluggable file storage backends |
| Provider pattern | `INotificationProvider` (SendGrid/SMTP/Sinch/Logging) | Pluggable notification channels |
| Specification pattern | `Shared/Operational/Persistence/Specifications/` | Composable query building (filtering, sorting, paging, searching) via DSL parsers |
| Result object pattern | `Result`, `ValueResult<T>`, `PagedResult<T>` | Explicit success/failure propagation without exceptions |
| CQRS-like separation | Separate `ICommand` and `IQuery` interfaces | Read/write segregation at the handler level |
| Sidecar pattern | Embedding FastAPI service | Isolated ML model hosting alongside the main API |

### 5) Known Architectural Risks

- **Monolith scaling**: The entire API is a single process. Module isolation is logical only (namespaces within the same project). If Catalog traffic spikes, all modules are affected.
- **Embedding service in early stage**: `/main.py` imports modules that don't exist yet (`config.settings`, `routers.embedding_router`). The service cannot start as-is.
- **No Docker/containerization**: Despite Aspire orchestration, no Dockerfiles exist. Deployment relies on raw `dotnet run` / `uv run` which is fragile in production.
- **Admin SPA is a scaffold**: Empty router, no views, only a placeholder counter store. The Admin app has no real feature implementation yet.

### 6) Evidence

- `service/Api/src/Api/Program.cs` — module registration order
- `Directory.Build.targets` — architecture validation targets
- `service/Api/src/Shared/Application/Mediators/Mediator.Extension.cs` — MediatR pipeline setup
- `service/Api/src/Shared/Operational/Persistence/Persistence.Extensions.cs` — EF Core configuration
- `infra/Aspire/src/ReSys.AppHost/AppHost.cs` — service orchestration topology
- `service/Embedding/src/main.py` — embed service entry (broken imports show early stage)
