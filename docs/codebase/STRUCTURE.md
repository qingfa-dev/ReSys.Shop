# Codebase Structure

## Core Sections (Required)

### 1) Top-Level Map

| Path | Purpose | Evidence |
|------|---------|----------|
| `service/Api/src/Api/` | .NET Web API entry point (Program.cs, startup config) | `service/Api/src/Api/Program.cs` |
| `service/Api/src/Shared/` | Cross-cutting infrastructure (auth, persistence, storage, caching, observability, etc.) | `service/Api/src/Shared/Shared.csproj` |
| `service/Api/src/Module/` | Business modules (Catalog, Identity, Location, Profile) | `service/Api/src/Module/Module.csproj` |
| `service/Api/src/Migrations/` | EF Core database migrations | `service/Api/src/Migrations/Api.Migrations.csproj` |
| `service/Api/tests/` | .NET test projects (Api.Tests, Module.UnitTests, Shared.UnitTests) | `service/Api/tests/` |
| `service/Embedding/` | Python FastAPI ML embedding sidecar service | `service/Embedding/src/main.py` |
| `infra/Aspire/` | .NET Aspire orchestration (AppHost + ServiceDefaults) | `infra/Aspire/src/ReSys.AppHost/AppHost.cs` |
| `infra/Cache/` | Cache infrastructure (placeholder — only .gitkeep) | `infra/Cache/` |
| `infra/Database/` | Database infrastructure (placeholder — only .gitkeep) | `infra/Database/` |
| `app/Admin/` | Vue 3 admin SPA (PrimeVue) — scaffold/early stage | `app/Admin/src/main.ts` |
| `app/Store/` | Vue 3 storefront SPA (Nuxt UI) — more feature-complete | `app/Store/src/main.ts` |
| `ApiTests/` | HTTP API test files (REST Client / JetBrains HTTP Client format) | `ApiTests/README.md` |
| `guide/` | Coding conventions and developer guides | `guide/code-commenting/CommentingRules.xml` |
| `docs/` | Documentation (codebase docs, superpowers plans/specs) | `docs/codebase/`, `docs/superpowers/` |

### 2) Entry Points

- Main runtime entry: `service/Api/src/Api/Program.cs` — .NET WebApplication host
- Aspire orchestration entry: `infra/Aspire/src/ReSys.AppHost/AppHost.cs` — orchestrates API, Embedding, frontends, DB, Redis
- Embedding service entry: `service/Embedding/src/main.py` — FastAPI app (separate process)
- Admin frontend entry: `app/Admin/src/main.ts` — Vue 3 app bootstrap
- Store frontend entry: `app/Store/src/main.ts` — Vue 3 app bootstrap
- How entry is selected: Aspire AppHost starts everything; alternatively, each service can be run independently via `dotnet run`, `uv run uvicorn`, or `pnpm dev`

### 3) Module Boundaries

| Boundary | What belongs here | What must not be here |
|----------|-------------------|------------------------|
| `Shared/` | Cross-cutting infrastructure (persistence, auth, caching, storage, security, observability, governance) | Business logic, feature-specific code |
| `Module/` | Domain entities, feature handlers (Carter endpoints, MediatR), persistence configs, seeders | Infrastructure code (HTTP clients, storage providers) |
| `Module/Catalog/` | Product, variant, taxonomy, option-type domain + features | Identity user management, location data |
| `Module/Identity/` | User, role, permission management + auth endpoints | Catalog or profile entities |
| `Module/Location/` | Country, state domain + lookup endpoints | Payment processing, notifications |
| `Module/Profile/` | User profiles, addresses, wishlists, preferences, notifications | Authentication logic |
| `Api/` | DI composition, middleware pipeline, appsettings | Business logic (must delegate to modules) |
| `infra/Aspire/` | Cloud orchestration, resource references (DB, Redis, services) | Application feature code |

### 4) Naming and Organization Rules

- File naming pattern (.NET): **PascalCase** for all C# files (e.g., `Program.cs`, `CatalogExtension.cs`, `GetProductDetailPage.Tests.cs`)
- File naming pattern (Vue): **PascalCase** for single-file components (e.g., `App.vue`, `HomeView.vue`, `CartView.vue`)
- File naming pattern (Python): **snake_case** for modules (e.g., `main.py`, `health_router.py`, `embedding_service.py`)
- Directory organization pattern: **Feature-first** inside modules (e.g., `Catalog/Features/Admin/Products/Create/`), **Layer-based** inside features (handlers, validators, mappings, models)
- Import aliasing: `@` maps to `./src` in both Vue apps (`vite.config.ts`). In .NET, implicit usings enabled with project references.
- HTTP test files organized by module then concern (e.g., `ApiTests/Catalog/Admin/products.http`)

### 5) Evidence

- `ReSys.Shop.slnx` — solution layout
- `service/Api/src/Api/Program.cs` — entry point + module registration
- `infra/Aspire/src/ReSys.AppHost/AppHost.cs` — orchestration manifest
- `ApiTests/README.md` — API test structure description
