Short summary

Top-level layout and where to find key code.

Repository layout (high level)
- `service/Api/` — main backend projects: `Api` (host), `Module` (8 business modules), `Shared` (infrastructure), `Migrations`.
- `infra/Aspire/` — AppHost orchestration projects (Aspire AppHost + service defaults).
- `service/Embedding/` — Python FastAPI embedding sidecar.
- `app/` — front-end applications: active `Admin` (pnpm), legacy `ReSys.Admin` (npm), and `Store`.
- `ApiTests/` — HTTP test files for manual API testing.

Projects & entry points
- Solution: `ReSys.Shop.slnx` references AppHost and backend projects under `service/Api/src/`.
- Backend entry: `service/Api/src/Api/Program.cs`.
- Aspire entry: `infra/Aspire/src/ReSys.AppHost/AppHost.cs`.
- Frontend dev entry: `app/Admin/index.html` and `app/Store/index.html` with Vite configs.
- Python entry: `service/Embedding/src/main.py`.

Evidence
- `ReSys.Shop.slnx`
- `service/Api/src/Api/Program.cs`
- `infra/Aspire/src/ReSys.AppHost/AppHost.cs`
- `app/Admin/index.html`, `app/Store/index.html`
- `service/Embedding/src/main.py`
- `ApiTests/README.md`

[ASK USER]
- `app/ReSys.Admin/` is a legacy npm-based admin SPA. Is it still needed, or can it be removed now that `app/Admin/` is the active pnpm-based SPA?
# Codebase Structure

## Core Sections (Required)

### 1) Top-Level Map

| Path | Purpose | Evidence |
|------|---------|----------|
| `service/Api/src/Api/` | .NET API host — Program.cs, middleware pipeline, config | `service/Api/src/Api/Program.cs` |
| `service/Api/src/Module/` | Business logic — 8 modules: Catalog, Identity, Inventory, Location, Ordering, Payment, Profile, Shipping | `service/Api/src/Module/Module.csproj` |
| `service/Api/src/Shared/` | Shared infrastructure — persistence, auth, storage, caching, notifications, jobs | `service/Api/src/Shared/Shared.csproj` |
| `service/Api/src/Migrations/` | EF Core migrations and database schema snapshots | `service/Api/src/Migrations/Api.Migrations.csproj` |
| `service/Api/tests/` | .NET test projects — `Api.Tests` (integration), `Module.UnitTests`, `Shared.UnitTests` | `service/Api/tests/Api.Tests/Api.Tests.csproj` |
| `service/Embedding/` | Python FastAPI ML sidecar (Fashion-CLIP embeddings, uvicorn) | `service/Embedding/pyproject.toml` |
| `infra/Aspire/src/ReSys.AppHost/` | Aspire orchestration — wires PostgreSQL, Redis, API, Embedding, SPAs | `infra/Aspire/src/ReSys.AppHost/AppHost.cs` |
| `infra/Aspire/src/ReSys.ServiceDefaults/` | Aspire shared defaults — OpenTelemetry, health checks, resilience | `infra/Aspire/src/ReSys.ServiceDefaults/Extensions.cs` |
| `app/Admin/` | Vue 3 Admin SPA — PrimeVue + Tailwind CSS (scaffold/WIP) | `app/Admin/package.json` |
| `app/Store/` | Vue 3 Storefront SPA — Nuxt UI + Tailwind CSS | `app/Store/package.json` |
| `ApiTests/` | HTTP test files (.http) for REST Client — manual API testing | `ApiTests/README.md` |
| `docs/codebase/` | Codebase documentation (STACK, ARCHITECTURE, CONVENTIONS, etc.) | `docs/codebase/STACK.md` |
| `docs/superpowers/` | Implementation plans and specs for recent features | `docs/superpowers/plans/`, `docs/superpowers/specs/` |
| `guide/code-commenting/` | Code commenting conventions and rules | `guide/code-commenting/CommentingRules.xml` |
| `.editorconfig` | Root code style rules for .NET, JSON, XML | `.editorconfig` |
| `Directory.Build.props` | Build-wide MSBuild properties (TFM, analysis, InternalsVisibleTo) | `Directory.Build.props` |
| `Directory.Build.targets` | Architecture validation targets (layer dependency checks) | `Directory.Build.targets` |
| `Directory.Packages.props` | Central Package Management version definitions | `Directory.Packages.props` |

### 2) Entry Points

- Main runtime entry: `service/Api/src/Api/Program.cs` — .NET 10 WebApplication bootstrapper
- Secondary entry points:
  - `infra/Aspire/src/ReSys.AppHost/AppHost.cs` — Aspire orchestration startup (all-in-one)
  - `service/Embedding/src/main.py` — Python FastAPI uvicorn app
  - `app/Admin/src/main.ts` — Vue 3 Admin SPA bootstrap
  - `app/Store/src/main.ts` — Vue 3 Storefront SPA bootstrap
- How entry is selected:
  - `dotnet run --project infra/Aspire/src/ReSys.AppHost` starts everything via Aspire
  - `dotnet run --project service/Api/src/Api` starts API alone
  - `pnpm run dev` in `app/Admin` or `app/Store` starts SPA dev server alone
  - `uv run uvicorn embedding.main:app` starts embedding service alone

### 3) Module Boundaries

| Boundary | What belongs here | What must not be here |
|----------|-------------------|------------------------|
| `service/Api/src/Api/` | Program.cs, appsettings, launchSettings, DesignTimeDbContextFactory | Business logic, persistence models, domain types |
| `service/Api/src/Shared/` | Cross-cutting infrastructure: persistence (EF Core setup, specs, interceptors), auth (JWT, Identity, OAuth, guest sessions), authorization (feature metadata, permission registry), storage (IStorageProvider, Local/S3 abstractions), caching (HybridCache wrapper), notifications (email/SMS providers), background jobs (Hangfire), observability (OTel, correlation, health checks), governance (OpenAPI, validation conventions, enum/case converters), error handling (Result types, exception mapping) | Business-specific domain logic or module-to-module imports |
| `service/Api/src/Module/Catalog/` | Products, variants, variant images, product options, taxonomies, taxons, taxon rules, option types, classifications, product search/filter, image embeddings, price history | Identity management, location data, user profiles, payment processing |
| `service/Api/src/Module/Identity/` | Users, roles, permissions, auth (login/register/logout/sessions), email confirmation, password management, external OAuth, role-permission assignments | Product data, location data, user profiles/addresses |
| `service/Api/src/Module/Inventory/` | Stock items, stock locations, stock reservations, stock transfers, stock movements | Product catalog data, user profiles, payment processing |
| `service/Api/src/Module/Location/` | Countries, states/provinces, ISO code lookups, country calling codes | User data, product data |
| `service/Api/src/Module/Ordering/` | Cart management, checkout, order lifecycle (create/list/view/status), cart expiry background jobs, order events | Payment processing, shipping calculation |
| `service/Api/src/Module/Payment/` | Payment intents, payment method management, refund processing, webhook handlers, BogusGateway (dev/testing) | Order fulfillment, shipping |
| `service/Api/src/Module/Profile/` | User profiles, addresses, wishlists, notification preferences | Authentication/authorization logic, product data |
| `service/Api/src/Module/Shipping/` | Shipping method CRUD, shipping rate management, rate calculation, address estimation | Payment processing, order fulfillment |
| `service/Api/src/Migrations/` | EF Core migration files (.cs), schema snapshots | Application logic, domain models |
| `service/Embedding/` | Python ML inference (CLIP/Fashion-CLIP models), embedding generation API, image preprocessing, embedding caching | Business logic, database access, user management |
| `app/Admin/` | Admin UI components, admin-specific views and stores | Storefront UI components, backend business logic |
| `app/Store/` | Storefront UI components, customer-facing views, cart/catalog stores, checkout flow | Admin UI components, admin auth logic |

### 4) Naming and Organization Rules

- File naming pattern (C#): PascalCase — e.g., `Country.Extensions.cs`, `GetProductDetail.cs`, `Catalog.Extension.cs`
- File naming pattern (Vue/TS): PascalCase for components — e.g., `App.vue`, `HomeView.vue`; kebab-case for .http tests — e.g., `products.http`
- Directory organization pattern: feature-first vertical slices — `Features/{Admin|Storefront}/{FeatureName}/{Action}/` (e.g., `Features/Admin/Products/Variants/Images/Upload/`, `Features/Storefront/Cart/Checkout/`)
- Domain layer separated: `Domain/{EntityName}/` with partial class files (e.g., `Domain/Products/Variants/Variant.cs`, `Variant.Extension.cs`, `Variant.Validation.cs`)
- C# module entry: each module has `{Module}.Extension.cs` with `IServiceCollection` extension methods (e.g., `Catalog.Extension.cs`, `Identity.Extensions.cs`)
- Import aliasing (frontend): `@` alias maps to `./src` in both Admin and Store SPAs — `app/Admin/vite.config.ts:13`, `app/Store/vite.config.ts:12`

### 5) Evidence

- `.codebase-scan.txt` — Directory tree scan output
- `ReSys.Shop.slnx` — Solution project references
- `service/Api/src/Api/Program.cs` — API entry point and DI wiring
- `infra/Aspire/src/ReSys.AppHost/AppHost.cs` — Aspire service wiring
- `service/Embedding/src/main.py` — Python entry point
- `app/Admin/vite.config.ts` — Admin Vite config with `@` alias
- `app/Store/vite.config.ts` — Store Vite config with `@` alias
- `Directory.Build.targets` — Layer dependency validation rules
