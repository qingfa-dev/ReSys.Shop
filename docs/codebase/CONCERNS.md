Short summary

Known problems, technical debt and areas to watch (evidence-based).

- `service/Embedding` has broken imports and cannot start as noted in AGENTS.md — investigate and fix.
- Admin SPA has placeholder content and limited functionality; some routes are stubs (AGENTS.md).
- Stale build artifacts present in `Embedding/build/lib/` — should be gitignored.
- Dev JWT secret is hardcoded in `appsettings.Development.json` (AGENTS.md note) — security risk.
High-priority concerns (from repo scan / AGENTS.md)
- `service/Embedding` has broken imports and cannot start as noted in AGENTS.md — investigate and fix.
- Admin SPA has placeholder content and limited functionality; some routes are stubs (AGENTS.md).
- Stale build artifacts present in `Embedding/build/lib/` — should be gitignored.
- Dev JWT secret is hardcoded in `appsettings.Development.json` (AGENTS.md note) — security risk.

[Decisions]
- Deployment: Local with Aspire (team choice) — developer workflow runs via `dotnet run --project infra/Aspire/src/ReSys.AppHost`.
- Secrets: Adopt local environment files and Aspire user secrets for development; remove hardcoded dev secrets from committed files.
- CI: No CI present and setup is deferred per team availability.
- `service/Embedding` has broken imports and cannot start as noted in AGENTS.md — investigate and fix.
- Admin SPA has placeholder content and limited functionality; some routes are stubs (AGENTS.md).
- Stale build artifacts present in `Embedding/build/lib/` — should be gitignored.
- Dev JWT secret is hardcoded in `appsettings.Development.json` (AGENTS.md note) — security risk.

High-churn areas
- `app/Admin/package.json`, `service/Api/src/Api/Program.cs`, and `infra/Aspire/src/ReSys.AppHost/AppHost.cs` are among top-churn files (scan output).

Evidence
- [AGENTS.md](AGENTS.md)
- [service/Embedding](service/Embedding)
- [app/ReSys.Admin](app/ReSys.Admin)
- High-churn list in `docs/codebase/.codebase-scan.txt`

[TODO]
- Add remediation backlog items (seeding, secret rotation, embedding fixes) — prioritize with the team.
# Codebase Concerns

## Core Sections (Required)

### 1) Top Risks (Prioritized)

| Severity | Concern | Evidence | Impact | Suggested action |
|----------|---------|----------|--------|------------------|
| High | Dev JWT secret hardcoded in committed config | `service/Api/src/Api/appsettings.Development.json:11` — `"Secret": "ThisIsADevelopmentJwtSecretKeyThatIsLongEnough32!"` | Anyone with repo access can forge valid JWT tokens in dev environment | Move to user secrets or env var; add `.env.example` with documentation |
| High | No CI/CD pipeline | Scan output: "No CI/CD pipelines detected" | No automated build verification, test execution, or lint enforcement before merge | Set up GitHub Actions workflow for build + test + lint |
| High | Module cross-reference enforcement is disabled | `Directory.Build.targets:44` — `Condition="false"` on ValidateVerticalSliceIsolation | Module isolation depends solely on developer discipline; accidental cross-module references won't be caught at build time | Enable the validation target and fix any violations, or remove it to avoid false confidence |
| High | No container images or Dockerfiles exist | README states "No Dockerfiles exist yet — deployment runs raw CLI commands"; scan confirms no Dockerfiles found | Production deployment is undefined; no repeatable build artifact | Create Dockerfile for API service; define multi-stage build |
| Medium | Embedding service has unresolved runtime issues | README marks embedding service as WIP (known broken); AGENTS.md notes `config.settings` import errors | Vector search and image similarity features are inoperable without the ML sidecar | Fix Python imports; verify service starts and generates embeddings end-to-end |
| Medium | Admin SPA is a scaffold with no real views | `app/Admin/src/App.vue` contains placeholder "You did it!"; router has empty routes; only a counter Pinia store | Admin functionality cannot be used at all | Prioritize Admin SPA feature development or clearly mark as low priority |
| Medium | Monolith scaling risk — all modules compile into single deployable | All modules share one process; Hangfire runs in-process with API | Traffic increase to one module affects all modules; background jobs compete for API resources | Plan module extraction strategy; externalize Hangfire to separate worker process |
| Medium | No backend .env.example or configuration documentation | Scan output: "No .env.example or .env.template found" | New developers don't know what env vars to set; onboarding takes longer | Create `.env.example` in `service/Api/src/Api/` listing all required variables |
| Medium | Large migration files may cause startup delays | `service/Api/src/Migrations/Migrations/20260703144227_AddCatalogModuleEntities.Designer.cs` — 98KB; `ApplicationDbContextModelSnapshot.cs` — 97.8KB | Database initialization at startup could be slow with large migration files | Review migration strategy; consider squash/reset for initial schema |
| Low | No HTTPS enforcement guard in code | No middleware found that redirects HTTP to HTTPS beyond `app.UseHttpsRedirection()` in `Program.cs:49` | Mixed content or non-TLS connections in production | Add HSTS middleware; enforce HTTPS in all environments except dev |

### 2) Technical Debt

| Debt item | Why it exists | Where | Risk if ignored | Suggested fix |
|-----------|---------------|-------|-----------------|---------------|
| Embedded `build/lib/` artifacts | Build process leaves stale compiled Python in source tree | `service/Embedding/build/lib/` | Confusing for developers; risk of running outdated code | Add `build/` and `__pycache__/` to `.gitignore`; remove tracked artifacts |
| Large `.superpowers/sdd/` review diffs | Two diff files are 7.8MB and 8.2MB each | `.superpowers/sdd/review-6e103fe..32de0cb.diff` (8.2MB), `.superpowers/sdd/review-6e103fe..6e63e8a.diff` (7.8MB) | Bloats repo; slows git operations | Archive old review diffs or add .gitignore rule for large diffs |
| Shared.csproj has too many dependencies | Single infrastructure project references nearly every production package | `service/Api/src/Shared/Shared.csproj` — 30+ PackageReference elements | Any NuGet version change affects all modules; tight coupling | Consider splitting Shared into smaller, purpose-specific packages (Shared.Persistence, Shared.Auth, etc.) |
| HTTP tests require manual data seeding | .http files must be run against a pre-seeded API instance | `ApiTests/README.md` | Test results are not deterministic; hard to reproduce failures | Add setup/teardown to .http files or migrate critical flows to integration tests |
| Code commenting guide is aspirational | Comprehensive XML guide exists but enforcement is manual | `guide/code-commenting/CommentingRules.xml` (63KB) | Inconsistent documentation across codebase | Implement linting rules for comment format enforcement or reduce guide to enforceable subset |
| No frontend type-safe API client generation | Both SPAs have hand-rolled `api.ts` fetch wrappers with `any` types | `app/Admin/src/api.ts`, `app/Store/src/api.ts` | API changes can break frontend silently; no compile-time guard | Generate TypeScript client from OpenAPI spec; or use tRPC-style typed contracts |

### 3) Security Concerns

| Risk | OWASP category (if applicable) | Evidence | Current mitigation | Gap |
|------|--------------------------------|----------|--------------------|-----|
| Hardcoded JWT secret in committed config | A02:2021 — Cryptographic Failures | `service/Api/src/Api/appsettings.Development.json:11` — secret is plaintext in git-tracked file | Marked as development-only | Same secret may be accidentally used in production; no rotation mechanism |
| Hardcoded DB credentials in dev config | A07:2021 — Identification/Authentication Failures | `appsettings.Development.json:8` — `Password=postgres` | Development-only | Leaked credentials if repo becomes public; local dev DB may use same creds as production |
| No API rate limiting | A04:2021 — Insecure Design | No rate limiting middleware found in Program.cs or Shared | None | Brute-force attacks on login/register endpoints possible; resource exhaustion risk |
| CORS allows localhost origins in dev config | A05:2021 — Security Misconfiguration | `appsettings.Development.json:20-25` — `["localhost:5173", "localhost:4173", "localhost:3000"]` with `AllowCredentials: true` | Development-only | If dev config accidentally used in production, allows credentialed requests from any localhost origin |
| Anti-forgery protection present but scope unknown | A01:2021 — Broken Access Control | `Shared/Security/AntiForgery/AntiForgery.Extensions.cs` | CSRF protection configured | [TODO] — scope not verified: which endpoints are protected, is it applied globally or selectively |
| Security headers present but scope unknown | A05:2021 — Security Misconfiguration | `Shared/Security/Headers/SecurityHeadersMiddleware.cs` | Security header middleware exists | [TODO] — headers configured and enforced not verified |
| No production secrets management template | A05:2021 — Security Misconfiguration | No `.env.example`, no secrets documentation beyond appsettings | Aspire User Secrets for development | Production secrets management (Azure Key Vault, AWS Secrets Manager, etc.) not defined |

### 4) Performance and Scaling Concerns

| Concern | Evidence | Current symptom | Scaling risk | Suggested improvement |
|---------|----------|-----------------|-------------|-----------------------|
| Database initialization includes migrations + seeding at startup | `service/Api/src/Api/Program.cs:58-60` — `app.InitializeDatabaseAsync(runMigrations: true, runSeeders: true)` | API startup time increases with migration count; seeder failures block deployment | Every instance update runs migrations against same DB; concurrent migration attempts | Separate migration from application startup (CI/CD migration step); make seeding idempotent |
| Hangfire and API share same process | Hangfire registered in `Shared/Operational/Backgrounds/Background.Extension.cs`; same `WebApplication` host | Background job CPU/memory competes with API request handling | Under heavy job load, API response times degrade; under heavy API load, job processing delays | Externalize Hangfire to separate worker process/container |
| HybridCache with Redis fallback chain unclear | `Shared/Performance/Caching/Caching.Extension.cs` | Cache miss behavior and Redis unavailability strategy undefined | Cache stampede on Redis failure; in-memory caches cause inconsistencies across instances | Document cache architecture; add Redis connection resilience checks |
| No connection pool limits documented | Npgsql and Redis connection strings in dev config have no pool size configuration | Connection pool exhaustion under load | Database connection starvation for burst traffic | Configure Npgsql `MaxPoolSize`, Redis `MaxConnections`; add connection pool monitoring |
| Large .superpowers diff files (7.8MB, 8.2MB) | `docs/codebase/.codebase-scan.txt:321-322` | Git operations slower; repo larger than needed | Cloned repo size impact; `git log` and `git blame` slower | Archive or gitignore large review diffs |

### 5) Fragile/High-Churn Areas

| Area | Why fragile | Churn signal | Safe change strategy |
|------|-------------|-------------|----------------------|
| `infra/Aspire/src/ReSys.AppHost/AppHost.cs` | Service wiring changes affect all components | 7 commits in 90 days (highest churn) | Test Aspire orchestration end-to-end after any AppHost change |
| `service/Api/src/Api/Program.cs` | DI wiring, middleware ordering, startup initialization | 6 commits in 90 days | Review middleware order impacts; test startup in all environments |
| `service/Api/src/Shared/Security/Authorization/Features/CatalogFeatureMetadata.cs` | Authorization feature metadata is frequently updated | 5 commits in 90 days | Add/update tests for each permission change; verify no permission leakage |
| `service/Api/src/Migrations/Migrations/ApplicationDbContextModelSnapshot.cs` | Schema changes accumulate in large snapshot file (97.8KB) | 5 commits in 90 days | Review snapshot changes carefully in PRs; consider migration squash |
| `service/Api/src/Module/Catalog/Catalog.Extension.cs` | Catalog module registration changes with new feature additions | 4 commits in 90 days | Ensure DI registration order is correct; test all catalog endpoints after changes |
| `service/Api/tests/Api.Tests/Infrastructure/ApiFactory.cs` | Test infrastructure changes affect all integration tests | 5 commits in 90 days | Run full integration test suite after ApiFactory changes |

### 6) Dependency Risks

| Dependency | Risk | Evidence |
|------------|------|----------|
| `Microsoft.EntityFrameworkCore.InMemory` (10.0.9) | InMemory provider behavior differs from PostgreSQL (noSQL, no constraints, no transactions) | `Directory.Packages.props:38` — used in unit tests | Unit tests may pass with InMemory but fail with real PostgreSQL | Supplement unit tests with integration tests for data-critical flows |
| `OpenTelemetry.Instrumentation.EntityFrameworkCore` (1.16.0-beta.1) | Beta package in production | `Directory.Packages.props:29` | Breaking changes in beta updates; potential instability | Monitor for stable release; pin version until then |
| `OpenTelemetry.Instrumentation.StackExchangeRedis` (1.16.0-beta.1) | Beta package in production | `Directory.Packages.props:32` | Same as above for Redis instrumentation | Same mitigation as above |

### 7) `[ASK USER]` Questions

1. [ASK USER] Is the Inventory module (`service/Api/src/Module/Inventory/`) officially part of the product, or is it an experimental/planned module? It appears in the codebase but is not listed in README.md's features or module list.
2. [ASK USER] Is Stripe payment processing currently in use, or is it planned for future implementation? The `Stripe.net` package is listed in Directory.Packages.props but no reference to payment features appears in the README or codebase structure.
3. [ASK USER] Should the `ValidateVerticalSliceIsolation` build target be enabled to enforce module boundaries at build time (currently disabled with `Condition="false"`)?
4. [Answered] Deployment environment: Local with Aspire. Dockerfiles and production deployment model deferred — document later. (Recorded in STACK.md and CONCERNS decisions.)
5. [ASK USER] What is the priority order for the known WIP components: Admin SPA, Embedding service, CI/CD pipeline, Dockerfiles? (Left to backlog/prioritization in CONCERNS)
6. [Answered] Secrets: Adopt local environment files and Aspire user secrets for development. For production, preferred provider not specified yet — please advise when ready.
7. [ASK USER] Are the large `.superpowers/sdd/` review diffs (~15MB total) still needed, or can they be archived/removed?
8. [Done] `.env.template` created at `service/Api/src/Api/.env.template` to document required development env vars. Review and sanitize hardcoded dev secrets from `appsettings.Development.json`.

### 8) Evidence

- `.codebase-scan.txt` — CI/CD absence, containerization absence, security config absence, high-churn files, large file indicators
- `service/Api/src/Api/appsettings.Development.json` — Hardcoded secrets (JWT, DB connection)
- `service/Api/src/Api/Program.cs` — Startup migration/seeding, HTTPS redirect
- `Directory.Build.targets` — Disabled ValidateVerticalSliceIsolation target
- `README.md` — Work-in-progress declarations (Admin SPA, Embedding service, Dockerfiles, CI/CD)
- `AGENTS.md` — Known broken components list
- `service/Embedding/src/main.py` — Embedding service entry point (structure exists, runtime status unverified)
- `app/Admin/src/App.vue` — Admin SPA placeholder content
- `app/Admin/src/router/index.ts` — Empty router
- Git log — High-churn files (AppHost.cs 7, Program.cs 6, CatalogFeatureMetadata.cs 5)
- `Directory.Packages.props` — Stripe.net package (present but feature not in README)
- `guide/code-commenting/CommentingRules.xml` — Aspirational commenting standards
- `service/Embedding/pyproject.toml` — Python dependency requirements (not verified against runtime)
