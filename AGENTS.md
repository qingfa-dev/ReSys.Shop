# ReSys.Shop — Agent Guide

ReSys.Shop is an e-commerce platform — a .NET 10 modular monolith with Vue 3 frontends
and a Python ML sidecar. Agents work across C#, TypeScript, and Python; all service
components start via Aspire orchestration. See `.harness/` for machine-readable
domain boundaries, principles, and quality baselines.

## Non-Negotiable Rules

1. **Result objects, not exceptions** — all domain operations return `Result<T>` or `Result`. Exceptions only for unrecoverable infrastructure failures.
2. **Modules never reference each other** — all 8 business modules live in one `Module` assembly but must not cross-reference. Communication via MediatR `ISender` only.
3. **Vertical slice feature files** — every C# feature action is a `static partial class` split across files in `Features/{Admin|Storefront}/{Feature}/{Action}/`, each with Handler, Request, Response, Endpoint, Validator.
4. **Warnings-as-errors** — `TreatWarningsAsErrors=true` globally. Any warning fails the build.
5. **Forward-only dependency** — `Shared` depends on nothing within `service/`. `Module` depends only on `Shared`. `Api` composes both.

## Repository Map

- `.harness/domains.yml` — 8 business domains + infrastructure + UI domains with layer maps
- `.harness/principles.yml` — golden principles with rationale and enforcement
- `.harness/enforcement.yml` — naming, file limits, logging, import rules
- `.harness/quality.yml` — per-domain quality scores (6 dimensions)
- `docs/codebase/ARCHITECTURE.md` — detailed architecture, layer responsibilities, data flow
- `docs/codebase/STACK.md` — full framework versions and toolchain
- `docs/codebase/CONCERNS.md` — tech debt, risks, security concerns
- `docs/codebase/CONVENTIONS.md` — coding conventions
- `docs/codebase/TESTING.md` — testing strategy
- `docs/codebase/PROCESS.md` — doc-gardening, GC, feedback encoding, escalation boundaries
- `plan/` — implementation plans (currently consolidating README docs across modules)
- `guide/code-commenting/CommentingRules.xml` — comment convention rules
- `Directory.Packages.props` — central NuGet package versions

## Tech Stack

- .NET 10 (C# preview), Vue 3 + TypeScript 6, Python 3.12
- EF Core + Npgsql + pgvector, Carter minimal APIs, MediatR CQRS, FluentValidation, Mapster
- HybridCache + Redis, Hangfire, JWT + ASP.NET Identity, SendGrid/SMTP/Sinch
- Aspire orchestration (PostgreSQL pgvector:pg17-trixie, Redis 7-alpine), OpenTelemetry
- pnpm workspaces for Vue SPAs, uv for Python, Central Package Management for NuGet

## Verification

```bash
dotnet build                                          # C# build (warnings-as-errors)
dotnet test service/Api/tests/Module.UnitTests        # Unit tests (fast, no Docker)
dotnet test service/Api/tests/Shared.UnitTests        # Shared unit tests
dotnet test                                           # All tests (inc. integration — requires Docker)
dotnet test /p:CollectCoverage=true                   # Opt-in coverage
dotnet test --filter "FullyQualifiedName~Location"    # Filter by module
cd app/Admin && pnpm run lint && pnpm run test:unit   # Admin SPA verification
cd app/Store && pnpm run lint && pnpm run test:unit   # Store SPA verification
cd service/Embedding && uv run ruff check . && uv run pytest  # Python verification
cd benchmarks && uv run ruff check src/ && uv run pytest --ignore=src/tests/integration/  # Benchmark verification
```

## Code Organization

- **`service/Api/src/Api/`** — thin host: `Program.cs`, appsettings, startup
- **`service/Api/src/Module/`** — 8 business modules (Catalog, Identity, Inventory, Location, Ordering, Payment, Profile, Shipping), each with `Domain/`, `Features/`, `Persistence/`
- **`service/Api/src/Shared/`** — cross-cutting infrastructure: Application abstractions, Security, Operational, Performance, Observability, Governance
- **`service/Api/src/Migrations/`** — EF Core migrations (separate assembly)
- **`app/Admin/`** — Vue 3 Admin SPA (PrimeVue, Sakai theme, pnpm, Vite 8)
- **`app/Store/`** — Vue 3 Storefront SPA (Nuxt UI, pnpm, Vite 8)
- **`service/Embedding/`** — Python FastAPI ML sidecar (uv, Fashion-CLIP, torch)
- **`benchmarks/`** — Python fashion image retrieval benchmark (11 models, 3 modes, thesis protocol) — see `benchmarks/AGENTS.md`
- **`infra/Aspire/`** — orchestration (AppHost + ServiceDefaults)
- **`ApiTests/`** — 49 `.http` files for manual endpoint testing

## Known Issues

- Dev JWT secret for non-Development environments is rejected by `JwtSettingsValidator` (commit `770b6a06`); dev secrets live in `dotnet user-secrets` (id `resys.shop.api`), bootstrapped via `service/Api/scripts/setup-dev-secrets.sh`
- `app/ReSys.Admin/` is a legacy admin SPA (npm, older deps) — use `app/Admin/` (pnpm) instead
- `ValidateVerticalSliceIsolation` build target is disabled (`Condition="false"` in `Directory.Build.targets:44`)
- CI/CD is partial — `.github/workflows/ci.yml` runs build, unit tests, and lint on PR/push for .NET, both Vue SPAs, Embedding service, and Benchmarks. Integration tests (Testcontainers) and deployment are not yet automated.
- No Dockerfiles — Aspire manages containers for local dev only
- `Embedding/build/lib/` contains stale build artifacts — should be gitignored
- `.harness/domains.yml` LOC counts may drift from actual codebase — re-measure after significant changes
