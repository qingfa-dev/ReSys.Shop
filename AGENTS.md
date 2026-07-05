# ReSys.Shop — AGENTS.md

## Quick start

```bash
# .NET API (the main backend)
dotnet build                          # Build — warnings-as-errors enforced
dotnet test                           # Run all .NET tests (unit + integration)
dotnet test service/Api/tests/Module.UnitTests  # Unit tests only (fast, no Docker)
dotnet test --filter "FullyQualifiedName~Location"  # Filter by module

# Vue frontends (pnpm — not npm)
cd app/Admin && pnpm install && pnpm run dev      # Admin SPA on :5173
cd app/Store && pnpm install && pnpm run dev      # Store SPA on :5174

# Python embedding service (uv — not pip)
cd service/Embedding && uv sync && uv run pytest
uv run uvicorn embedding.main:app --reload        # Dev server

# Aspire orchestration (starts everything)
dotnet run --project infra/Aspire/src/ReSys.AppHost
```

## Architecture essentials

- **Modular monolith**: 4 C# projects — `Api/` (host), `Module/` (business), `Shared/` (infrastructure), `Migrations/` (EF Core). Modules never reference each other.
- **CQRS via MediatR**: All feature endpoints use `ICommand<>` / `IQuery<>` handlers behind **Carter** minimal API endpoints. Pipeline: `LoggingBehavior → ValidationBehavior → ExceptionMappingBehavior`.
- **Shared/ is the backbone**: persistence (EF Core + Npgsql + pgvector), auth (JWT + ASP.NET Identity), storage abstraction (Local/S3/Azure), caching (HybridCache + Redis), notifications (SendGrid/SMTP/Sinch), background jobs (Hangfire).
- **Aspire orchestrates**: `.NET Aspire AppHost` wires PostgreSQL (pgvector:pg17-trixie), Redis (7-alpine), API, Embedding, Admin, Store. Run locally via `dotnet run --project infra/Aspire/src/ReSys.AppHost`.
- **No Dockerfiles exist yet** — deployment runs raw CLI commands.
- **Four business modules**: Catalog (products/variants/taxonomies/option-types), Identity (users/roles/permissions), Location (countries/states), Profile (profiles/addresses/wishlists/notifications).

## Testing quirks

- **Integration tests** (`Api.Tests`) use **Testcontainers** (PostgreSQL + Redis). **Requires Docker**. DB state reset via **Respawn** per test class (via `ApiIntegrationTestBase`).
- **Unit tests** (`Module.UnitTests`, `Shared.UnitTests`) use EF Core InMemory + Moq. No Docker needed.
- **HTTP tests** (`ApiTests/`) are `.http` files for REST Client / JetBrains HTTP Client. Require running API with seeded data.
- Coverage is **opt-in**: `dotnet test /p:CollectCoverage=true`.

## Convention gotchas

- `TreatWarningsAsErrors=true` — any warning fails the build.
- `InternalsVisibleTo` set **globally** in `Directory.Build.props` — test projects see internals automatically.
- Domain abstractions (`result object pattern` via `Result<T>`, `ValueResult<T>`, `PagedResult<T>`) — prefer explicit `.Success()` / `.Failure()` over exceptions.
- `.editorconfig` defines extensive C# naming rules (private fields `_camelCase`, static fields `s_camelCase`, interfaces `IPascalCase`).
- C# feature files organize as `Features/{Admin|Storefront}/{FeatureName}/{Action}/` — not layer-first.
- Frontend `.env.development` files contain `VITE_API_URL=http://localhost:5035`. Aspire overrides this.
- Python project uses **uv** (not pip), **Ruff** linter, `snake_case` modules.

## Known broken / work-in-progress

- **Embedding service** `main.py` imports modules that don't exist (`config.settings`, `routers/embedding_router`). Cannot start.
- **Admin SPA** has empty router and only a placeholder counter store — no real views.
- `Embedding/build/lib/` contains stale build artifacts — should be gitignored.
- Dev JWT secret hardcoded in `appsettings.Development.json`.

## Key references

- `docs/codebase/STACK.md` — full framework versions
- `docs/codebase/ARCHITECTURE.md` — layer responsibilities + data flow
- `docs/codebase/CONCERNS.md` — tech debt, risks
- `docs/codebase/.codebase-scan.txt` — full git churn, file counts
- `guide/code-commenting/CommentingRules.xml` — comment convention rules
- `Directory.Packages.props` — all NuGet package versions centrally
