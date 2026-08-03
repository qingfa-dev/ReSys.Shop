# CLAUDE.md

This file give guidance to Claude Code (claude.ai/code) when work with code in this repo.

## Essential Commands

```bash
# .NET — build and test
dotnet build                                                    # Warnings-as-errors enforced; any warning fails
dotnet test service/Api/tests/Module.UnitTests                  # Fast unit tests (InMemory DB, no Docker)
dotnet test service/Api/tests/Shared.UnitTests                  # Shared infrastructure unit tests
dotnet test service/Api/tests/Api.Tests                         # Integration tests (requires Docker: PostgreSQL + Redis)
dotnet test                                                     # All tests
dotnet test --filter "FullyQualifiedName~ModuleName"            # Filter by module
dotnet test /p:CollectCoverage=true                             # Opt-in code coverage

# Admin SPA (Vue 3 + PrimeVue)
cd app/Admin && pnpm install && pnpm run dev                    # Dev server (port 5173, proxies /api → localhost:5035)
cd app/Admin && pnpm run lint                                   # Oxlint + ESLint
cd app/Admin && pnpm run test:unit                              # Vitest unit tests

# Store SPA (Vue 3 + Nuxt UI)
cd app/Store && pnpm install && pnpm run dev                    # Dev server (port 5174)
cd app/Store && pnpm run lint && pnpm run test:unit

# Python Embedding service
cd service/Embedding && uv sync && uv run uvicorn embedding.main:app --reload  # Port 8000
cd service/Embedding && uv run ruff check . && uv run pytest

# Python Benchmarks
cd benchmarks && uv run ruff check src/ && uv run pytest --ignore=src/tests/integration/

# Aspire orchestration (starts everything: PG, Redis, API, Embedding, both SPAs)
dotnet run --project infra/Aspire/src/ReSys.AppHost

# Drift checks
bash scripts/check-feature-conventions.sh                       # Feature file completeness (AC-001 through AC-005)
bash scripts/check-cross-module-refs.sh                         # Cross-module namespace reference count (baseline: 32)
```

## Architecture

**Modular monolith with CQRS vertical slices.** 8 business modules (Catalog, Identity, Inventory, Location, Ordering, Payment, Profile, Shipping) + 1 unregistered proto-module (Dashboard) live in single `Module` assembly. Each module organized as `Domain/`, `Features/`, `Persistence/`, `Backgrounds/`, `Services/`.

**Request pipeline:**
```
HTTP Request → Carter endpoint → FluentValidation → MediatR pipeline (Logging → Validation → ExceptionMapping)
  → Command/Query Handler → Domain logic + EF Core / external services → Mapster-mapped DTO response
```

**Assembly dependency chain** (forward-only): `Api` → `Module` + `Shared` + `Migrations`; `Module` → `Shared`; `Shared` → `ServiceDefaults`. Modules must not reference each other — cross-module communication use MediatR `ISender.Send(new OtherModule.Command(...))` only. ~32 known violations tracked by `scripts/check-cross-module-refs.sh` (decreasing over time).

**Every feature** is `static partial class` split across files in one directory under `Features/{Admin|Storefront}/{Domain}/{Action}/`:
- `{Action}.cs` — handler (`ICommandHandler` or `IQueryHandler`)
- `{Action}.Endpoint.cs` — Carter `ICarterModule` route mapping
- `{Action}.Request.cs` — command/query DTO (some queries use `.Parameters.cs`)
- `{Action}.Response.cs` — response DTO
- `{Action}.Validator.cs` — FluentValidation rules
- `{Action}.Result.cs` — typed error factories (not all features)

**Result monad, not exceptions.** All domain ops and handlers return `Result<T>` or `Result`. Typed error codes via static factory classes (e.g., `OrderResult.Errors.NotFound(id)` → `"Order.NotFound"`). Endpoints convert `Result<T>` to HTTP responses via `result.ToResult()`. Exceptions only for unrecoverable infrastructure failures.

**Subdirectory convention:** Feature subdirectories use `Storefront` (not `Store`). Some modules (Identity, Location, Profile) still use `Store` — being standardized.

## Key Conventions

### C#
- **Files:** PascalCase, one type per file (except partial class features). Test files: `{Feature}Tests.cs`.
- **Namespaces:** Mirror folder structure. File-scoped namespaces preferred. Using directives outside namespace, System first.
- **Fields:** `_camelCase` (private instance), `s_camelCase` (private static). No `var` for primitives/built-in types.
- **Module registration:** Each module has `{Module}.Extension.cs` with `builder.Add{Module}Module()` pattern, composed in `Program.cs`.
- **Global usings:** `Shared/GlobalUsings.cs` provides common imports.
- **InternalsVisibleTo:** Projects expose internals to `{Name}.Tests`, `{Name}.UnitTests`, `{Name}.IntegrationTests`, and `DynamicProxyGenAssembly2` (Moq).
- **Central Package Management:** All NuGet versions in `Directory.Packages.props`.

### TypeScript (Vue SPAs)
- **Admin:** PrimeVue + Sakai theme + Tailwind CSS 4. Path alias `@/` → `./src/`. Form validation with Vee-Validate + Zod. Auto-imports via `unplugin-auto-import`.
- **Store:** Nuxt UI + Tailwind CSS 4. Same path alias pattern.
- **Linting:** Dual linter — Oxlint + ESLint. Formatting via Oxfmt (no Prettier). `noUncheckedIndexedAccess: true` in both SPAs.
- **Tests:** Vitest + jsdom, tests co-located in `src/**/__tests__/`.

### Python
- **Lint/Format:** Ruff (`line-length=100`, `target-version=py312`). Embedding: `["E", "F", "W", "I"]`. Benchmarks: `["E", "F", "I", "UP", "B", "SIM"]`.
- **Tests:** pytest with `asyncio_mode=auto`. Integration tests marked with `@pytest.mark.integration`.
- **Benchmarks:** 60% coverage minimum enforced in CI.

### Temporal Markers
Follow `guide/code-commenting/CommentingRules.xml`. Use `TODO`, `FIXME`, `HACK`, `UNDONE` with structured metadata. Markers may include `// EXCEPTION` comment to opt out of feature-convention checks.

## Testing Strategy

- **Unit tests (.NET):** EF Core InMemory database per test class (`Guid.NewGuid().ToString()` as DB name), isolated via constructor-create/dispose. Moq for external interfaces (`ICurrentUser`, `INotificationService`, etc.). Cross-module calls via real `ISender` against InMemory DB — not mocked. AAA pattern with `[Trait("Category", "Unit")]`, `[Trait("Module", "...")]`, `[Trait("Feature", "...")]`.
- **Integration tests (.NET):** `Api.Tests` project uses Testcontainers for PostgreSQL + Redis, Respawn for DB reset between tests.
- **Vue tests:** Vitest + jsdom + `@vue/test-utils`. Mocking via `vi.mock()` / `vi.fn()`.
- **No automated E2E tests:** `ApiTests/` contains 49 `.http` files for manual endpoint testing only.

## Pitfalls & Known Issues

- **`TreatWarningsAsErrors=true`** — any compiler warning fails the build. Includes unused variables and nullable reference warnings. Always run `dotnet build` before committing C# changes.
- **InMemory DB ≠ real PostgreSQL** — InMemory doesn't support pgvector, transaction isolation levels, or sequences. Tests passing with InMemory may fail against real PostgreSQL. Add integration tests for DB-specific features.
- **Feature subdirectory naming:** Use `Storefront`, not `Store`, for new features. Identity, Location, and Profile still use `Store` — don't replicate in new code.
- **Cross-module references:** When adding features, communicate across modules via `ISender`, never with direct `using Module.X.Domain...` references. Run `bash scripts/check-cross-module-refs.sh` after changes that touch multiple modules.
- **Dashboard module:** Exists but NOT registered in `Program.cs`. Don't add new Dashboard features without registering it first.
- **Dev secrets:** Live in `dotnet user-secrets` (id: `resys.shop.api`), bootstrapped via `service/Api/scripts/setup-dev-secrets.sh`. Dev JWT secret rejected for non-Development environments.
- **Legacy code:** `app/legacy/` directories exist but deprecated and gitignored. Use `app/Admin/` (pnpm) for all admin UI work.
- **Stale Embedding artifacts:** `service/Embedding/build/lib/` not gitignored — don't commit files there.
- **High-churn areas:** `CreateOrderFromCart.cs` (checkout orchestration) and `StripeWebhook.cs` (payment webhook) see most change activity. Be especially careful when modifying — add integration tests.
- **eslint-plugin-boundaries:** Installed as dependency in Admin but NOT activated in `eslint.config.ts`. TypeScript feature layer boundaries unenforced.

## Key Documentation Files

- `docs/codebase/ARCHITECTURE.md` — detailed architecture, layer responsibilities, data flow
- `docs/codebase/STACK.md` — full framework versions and toolchain
- `docs/codebase/CONVENTIONS.md` — naming, formatting, error handling, import rules
- `docs/codebase/TESTING.md` — test frameworks, layout, mocking strategy
- `docs/codebase/CONCERNS.md` — tech debt, security concerns, high-churn areas, resolved decisions
- `docs/codebase/INTEGRATIONS.md` — external services, data stores, secrets, reliability
- `docs/codebase/STRUCTURE.md` — directory layout, entry points, module boundaries
- `AGENTS.md` — agent-specific guide with non-negotiable rules
- `.harness/` — machine-readable domain boundaries, principles, enforcement rules
- `Directory.Packages.props` — all NuGet versions (Central Package Management)