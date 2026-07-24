# Codebase Structure

## Core Sections (Required)

### 1) Top-Level Map

| Path | Purpose | Evidence |
|------|---------|----------|
| `service/` | All server-side code (.NET API + Python ML) | Solution file `ReSys.Shop.slnx` |
| `service/Api/src/Api/` | Thin ASP.NET host: Program.cs, appsettings, startup | `Api.csproj` references Module + Shared + Migrations |
| `service/Api/src/Module/` | 8 business modules (single assembly, no cross-references) | `Module.csproj` depends only on Shared |
| `service/Api/src/Shared/` | Cross-cutting infrastructure (no dependency on Module) | `Shared.csproj` references ReSys.ServiceDefaults |
| `service/Api/src/Migrations/` | EF Core migrations (separate assembly) | `Api.Migrations.csproj` references Shared + Module |
| `service/Api/tests/` | 4 test projects: Module.UnitTests, Shared.UnitTests, Api.Tests (integration), Api.SmokeTests (.http) | `ReSys.Shop.slnx` |
| `service/Embedding/` | Python FastAPI ML sidecar (Fashion-CLIP, ONNX) | `pyproject.toml` |
| `app/Admin/` | Vue 3 Admin SPA (PrimeVue + Sakai theme) | `package.json` |
| `app/Store/` | Vue 3 Storefront SPA (Nuxt UI) | `package.json` |
| `app/legacy/ReSys.Admin/` | Legacy admin SPA (deprecated, use app/Admin) | Directory exists, gitignored |
| `infra/Aspire/` | .NET Aspire orchestration (AppHost + ServiceDefaults) | `ReSys.AppHost.csproj` |
| `benchmarks/` | Python fashion image retrieval benchmarks (11 models) | `pyproject.toml` |
| `docs/codebase/` | Architecture and process documentation | `ARCHITECTURE.md`, `STACK.md`, etc. |
| `guide/` | Coding guidelines (code commenting) | `guide/code-commenting/CommentingRules.xml` |
| `plan/` | Implementation plans | AGENTS.md reference |
| `.harness/` | Agent-first engineering harness (domain boundaries, principles) | `.harness/domains.yml`, `.harness/principles.yml` |
| `.github/workflows/` | CI pipeline (build, test, lint for all stacks) | `.github/workflows/ci.yml` |
| `ApiTests/` | 49 `.http` files for manual endpoint testing | AGENTS.md reference |

### 2) Entry Points

- **Main .NET entry**: `service/Api/src/Api/Program.cs` — composed via `builder.Add<Module>()` extension methods, reads appsettings
- **Secondary entry**: `infra/Aspire/src/ReSys.AppHost/AppHost.cs` — Aspire orchestrator starts PostgreSQL, Redis, Embedding service, .NET API, and both Vite SPAs
- **Embedding entry**: `service/Embedding/embedding/main.py` — FastAPI app (referenced by AppHost as `embedding.main:app`)
- **Admin SPA entry**: `app/Admin/index.html` + Vite (`pnpm run dev`)
- **Store SPA entry**: `app/Store/index.html` + Vite (`pnpm run dev`)
- **Benchmarks entry**: `benchmarks/src/benchmark/cli/benchmark.py` — CLI via Typer (`uv run benchmark`)

### 3) Module Boundaries

| Boundary | What belongs here | What must not be here |
|----------|-------------------|------------------------|
| `Shared/` | Cross-cutting abstractions: Application (Result, Entity, Mediators), Security, Performance, Operational, Observability, Governance | Must not depend on any Module code |
| `Module/` | 8 business domains (9th proto-module Dashboard exists but is unregistered): Catalog, Identity, Inventory, Location, Ordering, Payment, Profile, Shipping | Modules must not reference each other (ISender pattern only). However, 39 cross-module `using Module.X.Domain...` references exist across 7 of 8 modules (Ordering is the worst offender). Build target `ValidateVerticalSliceIsolation` emits warnings but does not fail the build. |
| `Api/` | Thin composition root: Program.cs, appsettings, startup wiring | No business logic |
| `Migrations/` | EF Core migration files and DbContext snapshot | No business logic |
| `Embedding/` | ML inference: image embedding generation, model hosting, ONNX optimization | No business logic |

**Module internal boundaries** (each of 8 modules follows this structure):

| Sub-boundary | What belongs here | What must not be here |
|-------------|-------------------|------------------------|
| `{Module}/Domain/` | Aggregate roots, value objects, domain methods returning Result<T>, enums, validation, error factories | No EF Core, no persistence, no API concerns |
| `{Module}/Features/` | Vertical slice feature files: Handler, Endpoint, Request, Response, Validator (static partial class). Subdirectories use `Admin`/`Storefront` (Profile, Identity, Location currently use `Store` — to be standardized). | No direct module-to-module references (use ISender) |
| `{Module}/Persistence/` | EF Core entity configurations + seeders | No domain logic |
| `{Module}/Backgrounds/` | Hangfire background job handlers | No domain logic |
| `{Module}/Services/` | Domain service interfaces and implementations | No cross-module DI |

### 4) Naming and Organization Rules

- **File naming**: PascalCase for all C# files (e.g., `Order.cs`, `CreateOrderFromCart.cs`, `ICommand.cs`)
- **Directory naming**: PascalCase domain names (e.g., `Catalog/`, `Ordering/`, `Payment/`), PascalCase subdirectories (e.g., `Features/Admin/Orders/Cancel/`)
- **Directory organization**: By feature (vertical slice), not by layer. Each feature action gets its own directory under `Features/{Admin|Storefront}/{Domain}/{Action}/`. Convention is `Storefront` (Profile, Identity, Location currently use `Store` — to be standardized).
- **CS file per concern**: Handler in `{Action}.cs`, endpoint in `{Action}.Endpoint.cs`, request DTO in `{Action}.Request.cs`, response DTO in `{Action}.Response.cs`, validator in `{Action}.Validator.cs`. Some read-only queries use `.Parameters.cs` instead of `.Request.cs`. Some features add `.Result.cs` (error factories).
- **TypeScript path aliases**: `@/*` → `./src/*` in both SPAs (`tsconfig.app.json`)
- **Test file placement**: Mirror source structure under `tests/{Project}/`. Test files are NOT co-located with source — they live in separate test projects.

### 5) Evidence

- `ReSys.Shop.slnx` — solution structure
- `service/Api/src/Api/Program.cs` — composition root
- `service/Api/src/Module/` — 8-module layout
- `service/Api/src/Shared/` — shared infrastructure layout
- `app/Admin/tsconfig.app.json` — `@/` → `./src/` alias
- `app/Store/tsconfig.app.json` — `@/` → `./src/` alias
- `.gitignore` — legacy admin exclusion
- `infra/Aspire/src/ReSys.AppHost/AppHost.cs` — Aspire orchestration composition
