# Codebase Structure

## Core Sections (Required)

### 1) Top-Level Map

| Path | Purpose | Evidence |
|------|---------|----------|
| `service/Api/src/Api/` | ASP.NET Core host — composition root, `Program.cs`, appsettings, launch settings | `service/Api/src/Api/Program.cs:1-66`, `service/Api/src/Api/Api.csproj:1-34` |
| `service/Api/src/Module/` | All 8 business modules in one assembly (`Module.Catalog`, `Module.Identity`, `Module.Inventory`, `Module.Location`, `Module.Ordering`, `Module.Payment`, `Module.Profile`, `Module.Shipping`); each one is a `partial` namespace aggregated via its `*.Extension.cs` | `service/Api/src/Module/Module.csproj:1-21`; per-module files: `Module.Catalog/Catalog.Extension.cs:1-38`, `Module.Ordering/Ordering.Extension.cs`, `Module.Identity/Identity.Extensions.cs`, `Module.Inventory/Inventory.Extension.cs`, `Module.Location/Locations.Extensions.cs`, `Module.Payment/Payment.Extension.cs`, `Module.Profile/Profiles.Extensions.cs`, `Module.Shipping/Shipping.Extension.cs` |
| `service/Api/src/Shared/` | Cross-cutting infrastructure: Application (mediators/endpoints/contracts), Governance (OpenAPI, conventions, validation), Observability, Operational (backgrounds, http, notifications, persistence, storages, webhooks), Performance (caching), Security (authn, authz, identity, anti-forgery, CORS, rate limiting, headers) | `service/Api/src/Shared/Shared.csproj:1-72`; dir contents enumerated in `docs/codebase/.codebase-scan.txt` |
| `service/Api/src/Migrations/` | EF Core migrations assembly (`Api.Migrations`) | `ReSys.Shop.slnx:218`, migrations in `service/Api/src/Migrations/Migrations/` |
| `service/Api/tests/` | xUnit v3 test projects: `Api.Tests` (integration), `Module.UnitTests`, `Shared.UnitTests` | `service/Api/tests/*/*.csproj` |
| `service/Embedding/` | Python FastAPI ML sidecar (Fashion-CLIP) — `pyproject.toml`, `src/embedding/...`, `tests/` | `service/Embedding/pyproject.toml:1-56`, `service/Embedding/src/main.py:1-29` |
| `infra/Aspire/src/ReSys.AppHost/` | .NET Aspire distributed application host (Postgres + Redis + API + 2 SPAs + Embedding) | `infra/Aspire/src/ReSys.AppHost/AppHost.cs:1-49` |
| `infra/Aspire/src/ReSys.ServiceDefaults/` | Aspire service defaults (OTel, health, resilience, service discovery) shared by all services | `infra/Aspire/src/ReSys.ServiceDefaults/Extensions.cs:1-132` |
| `app/Admin/` | Vue 3 + PrimeVue admin SPA (pnpm) | `app/Admin/package.json:1-71` |
| `app/Store/` | Vue 3 + Nuxt UI storefront SPA (pnpm) | `app/Store/package.json:1-56` |
| `app/ReSys.Admin/` | Legacy admin SPA (npm-based, marked for replacement by `app/Admin/`) | `README.md:177`, `.gitignore:154` (`app/ReSys.Admin/`) |
| `ApiTests/` | 49 `.http` files for manual endpoint testing (VS Code REST Client / JetBrains HTTP Client) | `ApiTests/README.md:1-30` |
| `docs/codebase/` | This documentation set (created by `acquire-codebase-knowledge`) | `README.md:158-168` |
| `guide/code-commenting/` | Internal commenting-style guide (XML-based) — referenced by `guide/code-commenting/README.md` | `guide/code-commenting/CommentingRules.xml:1-1340` (file size 63 KB) |
| `Directory.Build.props` / `Directory.Build.targets` / `Directory.Packages.props` / `global.json` / `dotnet-tools.json` | Centralized build, package, SDK, and tool config | as cited in `STACK.md` |
| `ReSys.Shop.slnx` | XML solution file enumerating the 8 .NET projects + 3 test projects + 2 Aspire projects | `ReSys.Shop.slnx:1-30` |
| `.editorconfig` (root) | Cross-language formatting/naming rules | `.editorconfig:1-389` |
| `AGENTS.md` | AI agent entry guide (module list, conventions summary, verification commands) | `AGENTS.md:1-80` |
| `LICENSE` | MIT license | `LICENSE` (1.0 KB) |
| `README.md` | Project intent, getting-started, stack, structure overview | `README.md:1-184` |

> Note: `.harness/` exists with machine-readable domain boundaries and quality baselines. `plan/` contains implementation plan files. `infra/Storage/` remains absent on disk.

### 2) Entry Points

- **Main backend host (ASP.NET Core):** `service/Api/src/Api/Program.cs:26-66` — `WebApplication.CreateBuilder(args)` → `builder.AddServiceDefaults()`, `AddObservability()`, `AddApplication([Module])`, `AddGovernance([Module])`, `AddPerformance()`, `AddSecurity()`, `AddOperational([Module])`, then per-module `AddXxxModule()` extension calls, then `app.MapDefaultEndpoints()`, `app.UseGovernance()`, `app.UsePerformance()`, `app.UseSecurity()`, `app.UseOperational()`, `app.UseObservability()`, `app.UseApplication()`, `await app.RunAsync()`.
- **Aspire AppHost:** `infra/Aspire/src/ReSys.AppHost/AppHost.cs:9-49` — `DistributedApplication.CreateBuilder(args)`; registers Postgres (with `pgvector` image), Redis, `Api` project reference, Uvicorn Embedding app, Vite Store (port 5173), Vite Admin (port 5174).
- **Python embedding service:** `service/Embedding/src/main.py:10-29` — `app = FastAPI(...)` with CORS, exception handlers, and three routers: `health_router`, `embedding_router` (prefix `/embeddings`), `model_router`.
- **Admin SPA:** `app/Admin/src/app/main.ts:15-22` (`createApp(App)` → Pinia, PrimeVue (Aura preset), Toast, Confirmation, StyleClass, auth bootstrap, router, mount).
- **Store SPA:** `app/Store/src/main.ts` (entry per scan, exact contents not re-read but matches `app/Admin/src/app/main.ts` pattern).
- **Backend tests:** `service/Api/tests/Api.Tests/Api.Tests.csproj` (xUnit v3 + `Microsoft.AspNetCore.Mvc.Testing` + Testcontainers), `service/Api/tests/Module.UnitTests/Module.UnitTests.csproj`, `service/Api/tests/Shared.UnitTests/Shared.UnitTests.csproj`.
- **Embedding tests:** `service/Embedding/tests/conftest.py:1-9` provides a `TestClient` fixture; `service/Embedding/tests/test_*.py` files exist (see scan).
- **Manual API tests:** `ApiTests/run-all.http` is a top-level orchestration file; per-module `.http` files in `ApiTests/<Module>/{Admin|Store|Storefront}/`.
- **Selection mechanism:** Aspire is the documented single-command orchestrator (`README.md:73-75`). The `Program.cs` builds + runs the `Api` project when `dotnet run --project service/Api/src/Api` is invoked; the SPAs are run individually (`pnpm run dev`) or via Aspire.

### 3) Module Boundaries

| Boundary | What belongs here | What must not be here |
|----------|-------------------|------------------------|
| `service/Api/src/Api/` (`Api` project) | Composition root: reference all modules + Shared + Migrations, define `Program.cs`/`appsettings.*`, register middleware. | Domain logic, EF entities, business handlers. (Project reference direction: `Api` → `Module`, `Shared`, `Migrations`.) |
| `service/Api/src/Module/<Name>/` | One namespace per module, vertical slices under `Features/{Admin|Storefront}/{Feature}/{Action}/` files (`*.cs`, `*.Endpoint.cs`, `*.Request.cs`, `*.Response.cs`, `*.Validator.cs`); module entry `<Name>.Extension.cs`; `Domain/`, `Persistence/` (DbContext config, schema name, seeders). | Cross-module references (`ValidateVerticalSliceIsolation` exists but is `Condition="false"` — currently a warning, not an error — `Directory.Build.targets:44-53`). No direct references to other `Module.<X>` namespaces. |
| `service/Api/src/Shared/` | Application abstractions (`ICommand`, `IQuery`, `Result`, `Error`, endpoints), Governance, Observability, Operational (persistence, storage, notifications, backgrounds, http, webhooks), Performance (caching), Security (auth/authz/anti-forgery/CORS/headers/identity/rate limiting). | Anything module-specific; only the `Shared.Marker` interface marks the assembly (`service/Api/src/Shared/Shared.Marker.cs:1-3`). |
| `service/Api/src/Migrations/Migrations/` | `DbContext` snapshot + versioned EF migrations. | Module business code; references the entity types via `ApplicationDbContext`. |
| `service/Embedding/src/embedding/` | FastAPI routers, controllers, services, models, schemas, infra (cache, models, preprocessing, storage, utils), middleware, utils. | Domain state of any backend module — the sidecar is stateless and called via HTTP. |
| `app/Admin/src/{app,shared,features}/` | `app/` = bootstrap + router; `shared/` = reusable (`api/http/`, `services/`, `components/`, `composables/`, `config/`, `locales/`, `utils/`); `features/<name>/` = per-domain UI (auth, catalog, identity, inventories, location, ordering, profile, reports, users, dashboard, error). Boundaries enforced by `eslint-plugin-boundaries` (`app/Admin/eslint.config.ts:32-54`: `shared ⊥ features,app`; `features ⊥ features,app`; `app → shared,features`). | Cross-feature imports; direct feature-to-feature coupling. |
| `app/Store/src/{api.ts, router/, stores/, views/, __tests__/}` | App entry, single `api.ts` (likely axios instance — contents not re-read), router config, Pinia stores (`cart.ts`, `catalog.ts`), views (`HomeView`, `ProductsView`, `ProductDetailView`, `CartView`, `CheckoutView`), Vitest specs. | Re-exporting component libraries directly inside stores (a thin pattern). |
| `infra/Aspire/src/ReSys.AppHost/` | Orchestration only (resource graph). | Business code; runtime infrastructure for tests. |
| `infra/Aspire/src/ReSys.ServiceDefaults/` | Service-defaults shared by all services (OTel, health, resilience, discovery). | Domain logic. |

Per `Directory.Build.targets:5-39`, the project also enforces (via MSBuild targets) that:
- `.Domain` projects cannot reference `.Infrastructure` or `.Application`.
- `.Application` projects cannot reference `.Infrastructure`.
- `.Api` / `.Web` projects get a *warning* (not error) for direct `.Infrastructure` references.

> The actual backend uses no per-module `.Domain`/`.Application`/`.Infrastructure` sub-assemblies; everything is a single `Module` assembly. So those targets are dormant unless a project is renamed to match the suffix.

### 4) Naming and Organization Rules

- **C# file naming:** `static partial class` is split across `Name.cs` (handler/command), `Name.Endpoint.cs`, `Name.Request.cs`, `Name.Response.cs`, `Name.Validator.cs` — see `service/Api/src/Module/Catalog/Features/Admin/Products/Create/` (5 files, all sharing `public static partial class CreateProduct`).
- **Feature directory layout:** `Features/{Admin|Storefront}/{Feature}/{Action}/` — e.g. `Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.{cs,Endpoint.cs,Request.cs,Response.cs,Validator.cs}` (`service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/`).
- **Domain types:** `Domain/<Aggregate>/<Type>.cs` (split into `Type.Constant.cs`, `Type.Enumerate.cs`, `Type.Method.cs`, `Type.Result.cs`, `Type.Validation.cs`, `Type.Loggers.cs` for large aggregates; e.g. `Module/Ordering/Domain/Orders/Order.{cs,Constant.cs,Checkout.cs,...}`).
- **Persistence:** `Persistence/SchemaName.cs`, `Persistence/Configurations/<Aggregate>/...`, `Persistence/Seeders/...` per module (e.g. `Module/Catalog/Persistence/CatalogSchema.cs:1-31`).
- **Module entry point:** `<Name>.Extension.cs` exporting `Add<Name>Module(this WebApplicationBuilder builder)`; referenced by `service/Api/src/Api/Program.cs:38-45`.
- **Project markers:** `Module/Module.Marker.cs` defines `public interface IModuleMarker;` (used to discover the assembly in `Program.cs:24`); `Shared/Shared.Marker.cs` defines `ISharedMarker`.
- **Frontend Admin:** kebab-case in path (`features/catalog/products/`), PascalCase in Vue component names (`ProductStore`, `ProductService`). Lint rule `eslint-plugin-boundaries` enforces `shared` / `features` / `app` separation (`app/Admin/eslint.config.ts:32-54`).
- **Frontend Store:** `views/*View.vue` (`HomeView.vue`, `CartView.vue`, etc.); `stores/cart.ts` Pinia store; `__tests__/*.spec.ts`.
- **TS path alias:** `@/*` → `./src/*` for both SPAs (`app/Admin/tsconfig.app.json:11`, `app/Store/tsconfig.app.json:11`); Vite `resolve.alias` mirrors (`app/Admin/vite.config.ts:44-46`).
- **Test naming (backend):** `<Type>.Tests.cs` and `<Type>.Validator.Tests.cs` mirroring source files (e.g. `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Create/CreateProduct.Tests.cs:1-5`).
- **Test naming (frontend):** `*.spec.ts` colocated (e.g. `app/Admin/src/features/auth/_tests/auth.service.spec.ts`, `app/Store/src/__tests__/cart.store.spec.ts`).
- **HTTP test files:** `ApiTests/<Module>/{Admin|Store|Storefront}/<concern>.http`; per-file section headers use `### <Description>` (see `ApiTests/Identity/Store/auth-login.http:1-15`).

### 5) Evidence

- `service/Api/src/Api/Program.cs:1-66` — composition root
- `service/Api/src/Api/Api.csproj:1-34`, `service/Api/src/Module/Module.csproj:1-21`, `service/Api/src/Shared/Shared.csproj:1-72` — projects and dependencies
- `service/Api/src/Module/<Name>/<Name>.Extension.cs` — per-module DI registration
- `service/Api/src/Module/Catalog/Features/Admin/Products/Create/*.cs` — vertical-slice file pattern
- `service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.{Admin,Storefront,Tags}.cs` — feature metadata constants
- `service/Api/src/Shared/Application/Mediators/Mediator.Extension.cs:28-55` — MediatR pipeline registration
- `service/Api/src/Shared/Application/Endpoints/Endpoint.Extension.cs:13-49` — Carter module discovery
- `infra/Aspire/src/ReSys.AppHost/AppHost.cs:9-49` — Aspire resource graph
- `infra/Aspire/src/ReSys.ServiceDefaults/Extensions.cs:19-132` — Aspire service defaults
- `service/Embedding/src/main.py:1-29` — Python embedding service entry
- `app/Admin/src/app/main.ts:1-23`, `app/Admin/vite.config.ts:1-54`, `app/Admin/eslint.config.ts:1-57` — Admin SPA bootstrap, Vite, ESLint
- `app/Admin/tsconfig.app.json:1-18`, `app/Store/tsconfig.app.json:11` — `@/*` path alias
- `app/Store/src/__tests__/App.spec.ts:1-28` — Store test layout
- `ApiTests/README.md:1-30`, `ApiTests/_shared/variables.http:1-20` — HTTP test layout
- `docs/codebase/.codebase-scan.txt` — full file/dir enumeration
- `ReSys.Shop.slnx:1-30` — solution layout
