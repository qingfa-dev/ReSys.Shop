# Architecture

## Core Sections (Required)

### 1) Architectural Style

- **Primary style**: Vertical slice architecture (feature-organized) within a modular monolith
- **Why this classification**: Every business operation is a self-contained `static partial class` in `Features/{Admin|Storefront}/{Domain}/{Action}/` split across 4-6 files (Handler, Endpoint, Request, Response, Validator). No layers (controllers/services/repos) exist — each feature contains its own handler, validation, and endpoint routing in a single directory.
- **Primary constraints**:
  1. Modules share one `Module` assembly and may reference each other; cross-module EF Core FK relationships, navigations, and direct service calls are permitted. For behavior, prefer MediatR `ISender` when dispatching to another module so it flows through the pipeline (validation, logging, transaction); call services or navigate relationships directly when that fits the feature slice.
  2. All domain operations return `Result<T>` or `Result` — exceptions only for unrecoverable infrastructure failures
  3. Features are `static partial class` split across files — all related code for one action lives in one directory
  4. Warnings-as-errors globally (`TreatWarningsAsErrors=true` in `Directory.Build.props`)

**Consolidated per-module `Shared/` convention**: Exactly one `Shared` folder per module and area at `{Module}/Features/Admin/Shared/` and `{Module}/Features/Storefront/Shared/`; it contains only `Mappings/`, `Models/`, `Validators/` subfolders. File names are `{Entity}.{Kind}.cs` for Admin and `Storefront.{Entity}.{Kind}.cs` for Storefront; namespace is `Module.{Module}.Features.{Area}.Shared.{KindDir}`. `Services/`, `Clients/`, `Docs/` stay co-located with their consuming feature. Example: `service/Api/src/Module/Ordering/Features/Admin/Shared/Models/Order.Model.cs`.

### 2) System Flow

```text
HTTP Request → Carter Endpoint → FluentValidation → MediatR Pipeline → Handler → Domain Logic → EF Core → PostgreSQL
                                                         |
                                                  (ISender for cross-module)
                                                         |
                                                  Email/SMS (FluentEmail/Sinch)
                                                  Payment (Stripe)
                                                  Embedding (Python FastAPI)
```

**Step-by-step with evidence:**

1. **Request arrives** at a Carter endpoint (`{Action}.Endpoint.cs` implementing `ICarterModule`) — maps route to MediatR command
2. **Validation pipeline** (MediatR behavior) runs FluentValidation (`{Action}.Validator.cs`) before the handler
3. **Handler** (`{Action}.cs`, `ICommandHandler<Command, Response>`) receives validated command
4. **Domain logic** executes in aggregate roots or domain service methods — all return `Result<T>` (e.g., `cart.Place(orderNumber)`)
5. **Cross-module communication** uses `ISender.Send(new OtherModule.Command(...))` (or a direct service call/navigation where that fits) — e.g., `Checkout` sends `ReserveCartStock.Command` to Inventory module
6. **Persistence** via EF Core `IApplicationDbContext` — only entity configurations in `Persistence/Configurations/` (no repositories)
7. **Response** returned as `Result<Response>` → endpoint maps to HTTP status via `result.ToResult()` extension

### 3) Layer/Module Responsibilities

| Layer or module | Owns | Must not own | Evidence |
|-----------------|------|--------------|----------|
| `Api/Program.cs` | DI composition, middleware ordering, module registration | Business logic, feature definitions | `service/Api/src/Api/Program.cs` |
| `Shared/Application/Domain/` | Base classes: `Entity<T>`, `AggregateRoot<T>`, `ValueObject`, cross-cutting concerns (Auditable, SoftDeletable) | Any business domain logic | `service/Api/src/Shared/Application/Domain/Models/Entity.cs` |
| `Shared/Application/Mediators/` | CQRS interfaces: `ICommand<T>`, `IQuery<T>`, pipeline behaviors (Validation, Logging) | Business-specific mediator handlers | `service/Api/src/Shared/Application/Mediators/Commands/ICommand.cs` |
| `Shared/Application/Models/` | `Result<T>`, `Result`, `Error`, `PagedResult<T>` monad types | Business error codes | `service/Api/src/Shared/Application/Models/Results/Result.cs` |
| `Shared/Security/` | Auth (JWT, Google OAuth), authorization, anti-forgery, CORS, rate limiting | Module-specific permissions | `service/Api/src/Shared/Security/Security.Extension.cs` |
| `Shared/Operational/` | Background jobs (Hangfire), notifications (email/SMS), file storage, HTTP resilience, DB persistence infra | Module-specific job handlers | `service/Api/src/Shared/Operational/` |
| `Shared/Performance/` | HybridCache (memory + Redis), caching policies | Module-specific cache keys | `service/Api/src/Shared/Performance/` |
| `Shared/Observability/` | OpenTelemetry, correlation IDs, structured logging, health checks | Business metrics definition | `service/Api/src/Shared/Observability/` |
| `Module/{Domain}/Domain/` | Aggregate roots, value objects, enums, domain methods returning Result, error factories | Persistence, API concerns | `service/Api/src/Module/Ordering/Domain/Orders/Order.cs` |
| `Module/{Domain}/Features/` | Feature handlers, endpoints, request/response DTOs, validators | Business logic that belongs to another module | `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs` |
| `Module/{Domain}/Persistence/` | EF Core entity type configurations (EntityTypeConfiguration<T>), seeders | Domain logic, feature handlers | `service/Api/src/Module/Ordering/Persistence/Configurations/` |
| `infra/Aspire/AppHost/` | Container orchestration: PostgreSQL, Redis, API, Embedding, Admin/Store Vite dev servers | Application logic | `infra/Aspire/src/ReSys.AppHost/AppHost.cs` |
| `infra/Aspire/ServiceDefaults/` | OpenTelemetry setup, health checks, service discovery, default resilience | Business logic | `infra/Aspire/src/ReSys.ServiceDefaults/Extensions.cs` |

### 4) Reused Patterns

| Pattern | Where found | Why it exists |
|---------|-------------|---------------|
| **Vertical slice (static partial class)** | Every feature directory, e.g., `Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs` + `CreateOrderFromCart.Endpoint.cs` + `.Request.cs` + `.Response.cs` + `.Validator.cs` | Co-locates all code for a single use case; partial class reassembles across files |
| **Result monad** | Every handler return type and every domain method | Replaces exceptions for expected failures; `Result<T>` / `Result` with typed error codes |
| **CQRS via MediatR** | All feature handlers implement `ICommandHandler<Command, Response>` or `IQueryHandler<Query, Response>` | Separates reads (queries) from writes (commands); enables cross-cutting pipeline behaviors |
| **ISender cross-module** | `sender.Send(new OtherModule.Command(...))` in handler code | Behavior flows through MediatR (pipeline applies); direct service calls and navigation access are permitted when they fit the feature slice |
| **Static factory methods on domain entities** | `OrderMethod.Create("USD", userId, sessionId)` returns `Result<Order>` | Internal constructors for EF Core; factory methods enforce invariants via Result |
| **Typed error codes** | `OrderResult.Errors.NotFound(id)` returns `Error` with code `"Order.NotFound"` | Structured error codes instead of magic strings; discoverable via static classes |
| **Extension method module registration** | `builder.AddOrderingModule()` pattern in each `{Module}.Extension.cs` | Clean DI registration per module; composed in `Program.cs` |
| **ICarterModule endpoints** | Each feature's `.Endpoint.cs` implements `ICarterModule` | Minimal API routing without controllers; auto-discovered by Carter |
| **Constructor injection** | Every handler receives dependencies via primary constructor | Standard .NET DI pattern; no service locator |
| **EF Core InMemory for tests** | Test constructors use `UseInMemoryDatabase(Guid.NewGuid().ToString())` | Fast, isolated unit tests without Docker |

### 5) Known Architectural Risks

- **ISender convention under review**: cross-module behavior flows through MediatR `ISender`, but this convention is itself up for reconsideration (see `AGENTS.md` rule #2 TODO and the `brainstorming` skill for proposal options).
- **No API gateway in production**: YARP is referenced in packages but [TODO] — no evidence of production gateway configuration. Aspire manages local dev orchestration only.
- **Embedding service is a synchronous dependency**: The .NET API calls the Python Embedding service over HTTP during request handling — a failure in the ML sidecar blocks API responses. No circuit breaker observed in the embedding call path.
- **Duplicate middleware**: `UseRateLimiter()` is called twice in Program.cs — once in `UseSecurity()` and once explicitly at line 62. Harmless but indicative of drift.
- **Dashboard module exists but is unregistered**: 9 feature files exist under `Module/Dashboard/` but `builder.AddDashboardModule()` is not called in `Program.cs`. 9th module in an 8-module architecture.
- **Profile module missing endpoint**: `CreateProfile` feature has no `.Endpoint.cs` file.

### 6) Evidence

- `service/Api/src/Api/Program.cs` — composition root and module registration
- `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs` — representative handler with cross-module orchestration via ISender
- `service/Api/src/Shared/Application/Models/Results/Result.cs` — Result monad implementation
- `service/Api/src/Shared/Application/Mediators/Commands/ICommand.cs` — CQRS interfaces
- `service/Api/src/Shared/Application/Domain/Models/Entity.cs` — base entity class
- `Directory.Build.targets` — architecture validation build targets
- `infra/Aspire/src/ReSys.AppHost/AppHost.cs` — orchestration and service dependencies
