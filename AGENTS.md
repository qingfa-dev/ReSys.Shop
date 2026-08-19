# ReSys.Shop — Agent Guide

ReSys.Shop e-commerce platform — .NET 10 modular monolith with Vue 3 frontends
and Python ML sidecar. Agents work C#, TypeScript, Python; all service
components start via Aspire orchestration. See `.harness/` for machine-readable
domain boundaries, principles, quality baselines.

## Non-Negotiable Rules

1. **Result objects, not exceptions** — all domain operations return `Result<T>` or `Result`. Exceptions only for unrecoverable infrastructure failures.
2. **Modules share one assembly and may reference each other; behavior may use MediatR `ISender` or direct calls** — all 9 business modules (Catalog, Identity, Inventory, Location, Ordering, Payment, Profile, Shipping, Dashboard) live in one `Module` assembly. Cross-module `using` statements, EF Core FK relationships, navigations, and direct service calls are all permitted (no whitelist, no drift check — removed 2026-08-16). For behavior, prefer MediatR `ISender` when dispatching work to another module so it flows through the pipeline (validation, logging, transaction); call services or navigate relationships directly when that fits the feature slice.
   - **TODO (replace the ISender convention):** the `ISender`-for-behavior convention is retained for now but is itself under review. Proposal options to replace it (see `brainstorming` skill): (a) keep `ISender` as-is, (b) direct cross-module service calls where the type is already referenced, (c) a module gateway/facade layer, (d) splitting modules into separate assemblies with explicit ProjectReferences. Run `/brainstorming` before choosing.
   - **Replacement candidates (audited 2026-08-16):** navigations fit where `Order` is the hub — `GetPaymentForCheckout`/`GetCartForCheckout`/`GetCartForShipping` → load `Order` + `LineItems`/`PaymentCaptures` via `.Include`/direct nav; `MarkPaymentPaid` → `payment.Complete()` on the nav-loaded capture; `RecordOrderPaymentState` → `order.MarkPayment{Completed|Failed|Processing}`; `AdvanceCheckoutState`/`RegressCheckoutState` → `order.AdvanceCheckoutState()`/`Regress*`. Direct service calls fit where a foreign aggregate is created: `CreateShipment` (→ Shipping service), `CreateUserProfile` (→ Customer profile service). Keep `ISender` (pipeline/retry/transaction value): `VoidOrderPayments`, `CompleteCheckoutForPayment`.
3. **Vertical slice feature files** — every C# feature action is `static partial class` split across files in `Features/{Admin|Storefront}/{Feature}/{Action}/`, each with Handler, Request, Response, Endpoint, Validator. Subdirectory always `Storefront` (not `Store`). Read-only queries may omit Request/Validator files.
4. **Warnings-as-errors** — `TreatWarningsAsErrors=true` globally. Any warning fails build.
5. **Forward-only dependency** — `Shared` depends on nothing within `service/`. `Module` depends only on `Shared`. `Api` composes both.
6. **NO git stash / worktree / revert / restore / checkout -- without human permission** — never run `git stash`, `git checkout <ref> -- <path>`, `git restore`, `git revert`, `git reset --hard`, or any command that moves/rewrites/loses working-tree changes, unless the human explicitly asked for it and you got an explicit "yes" to run that specific command. If your tooling or hooks auto-stash or reset the working tree, STOP and report it immediately — do not silently continue. Recover lost changes only after the human confirms. Uncommitted work is precious and irreplaceable.

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
- `docs/failures/` — failure memory: known incidents
- `plan/` — implementation plans (currently consolidating README docs across modules)
- `guide/code-commenting/CommentingRules.xml` — comment convention rules
- `Directory.Packages.props` — central NuGet package versions
- `scripts/check-feature-conventions.sh` — feature file AC-001/002/003/005 drift checks

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
bash scripts/check-feature-conventions.sh             # Feature file completeness check
```

## Code Organization

- **`service/Api/src/Api/`** — thin host: `Program.cs`, appsettings, startup
- **`service/Api/src/Module/`** — 9 business modules (Catalog, Identity, Inventory, Location, Ordering, Payment, Profile, Shipping, Dashboard), each with `Domain/`, `Features/`, `Persistence/`
- **`service/Api/src/Shared/`** — cross-cutting infrastructure: Application abstractions, Security, Operational, Performance, Observability, Governance
- **`service/Api/src/Migrations/`** — EF Core migrations (separate assembly)
- **`app/Admin/`** — Vue 3 Admin SPA (PrimeVue, Sakai theme, pnpm, Vite 8)
- **`app/Store/`** — Vue 3 Storefront SPA (Nuxt UI, pnpm, Vite 8)
- **`service/Embedding/`** — Python FastAPI ML sidecar (uv, Fashion-CLIP, torch)
- **`benchmarks/`** — Python fashion image retrieval benchmark (11 models, 3 modes, thesis protocol) — see `benchmarks/AGENTS.md`
- **`infra/Aspire/`** — orchestration (AppHost + ServiceDefaults)
- **`ApiTests/`** — 49 `.http` files for manual endpoint testing

## Known Issues

- Dev JWT secret for non-Development environments rejected by `JwtSettingsValidator` (commit `770b6a06`); dev secrets in `dotnet user-secrets` (id `resys.shop.api`), bootstrapped via `service/Api/scripts/setup-dev-secrets.sh`
- `app/ReSys.Admin/` legacy admin SPA (npm, older deps) — use `app/Admin/` (pnpm) instead
- `ValidateVerticalSliceIsolation` build target emits `<Warning>` for module ProjectReferences; cross-module references are permitted, so this only guards against future module-split ProjectReference cycles
- CI/CD partial — `.github/workflows/ci.yml` runs build, unit tests, lint on PR/push for .NET, both Vue SPAs, Embedding service, Benchmarks. Integration tests (Testcontainers) and deployment not yet automated
- `service/Embedding/Dockerfile` exists for Python sidecar; no Dockerfiles for .NET API or Vue SPAs (Aspire manages containers for local dev)
- `Embedding/build/lib/` contains stale build artifacts — should be gitignored
- `.harness/domains.yml` LOC counts may drift from actual codebase — re-measure after significant changes