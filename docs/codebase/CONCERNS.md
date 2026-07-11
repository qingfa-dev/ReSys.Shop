Short summary

Known problems, technical debt and areas to watch (evidence-based).

High-priority concerns
- Dev JWT secret and DB credentials are hardcoded in committed `appsettings.Development.json`.
- No CI/CD pipeline; builds and tests run manually.
- Module cross-reference build enforcement is disabled.
- No Dockerfiles or production deployment definition.
- `service/Embedding/build/lib/` contains tracked stale build artifacts.
- Admin SPA has layout infrastructure but limited feature routes.

Evidence
- `AGENTS.md`
- `service/Api/src/Api/appsettings.Development.json`
- `service/Embedding/build/lib/`
- `docs/codebase/.codebase-scan.txt`
- `Directory.Build.targets`
- `app/Admin/src/app/router/index.ts`

[ASK USER]
- What is the team's priority order for addressing the top risks (secrets hardening, CI/CD, module isolation enforcement, production deployment)?
# Codebase Concerns

## Core Sections (Required)

### 1) Top Risks (Prioritized)

| Severity | Concern | Evidence | Impact | Suggested action |
|----------|---------|----------|--------|------------------|
| High | Dev JWT secret hardcoded in committed config | `service/Api/src/Api/appsettings.Development.json:28` — `"Secret": "ThisIsADevelopmentJwtSecretKeyThatIsLongEnough32!"` | Anyone with repo access can forge valid JWT tokens in dev environment | Move to user secrets or env var; use the existing `.env.template` for documentation |
| High | No CI/CD pipeline | Scan output: "No CI/CD pipelines detected" | No automated build verification, test execution, or lint enforcement before merge | Set up GitHub Actions / Azure DevOps workflow for build + test + lint |
| High | Module cross-reference enforcement is disabled | `Directory.Build.targets:44` — `Condition="false"` on ValidateVerticalSliceIsolation | Module isolation depends solely on developer discipline; accidental cross-module references won't be caught at build time | Enable the validation target and fix any violations, or remove it to avoid false confidence |
| High | No container images or Dockerfiles exist | README states "No Dockerfiles exist yet"; scan confirms no Dockerfiles found | Production deployment is undefined; no repeatable build artifact | Create Dockerfile for API service; define multi-stage build |
| Medium | Embedding service structure exists but runtime status unverified | `service/Embedding/src/main.py` imports resolve to existing files; `infra/Aspire/src/ReSys.AppHost/AppHost.cs:20-27` wires the service | Vector search and image similarity depend on the ML sidecar being operational | Verify service starts with `uv run uvicorn embedding.main:app`; run `uv run pytest` |
| Medium | Admin SPA has layout infrastructure but minimal feature routes | `app/Admin/src/app/router/index.ts` — routes limited to login, profile, error; layout components exist | Admin functionality is largely non-functional; business feature routes are not connected | Continue Admin SPA feature development following existing layout patterns |
| Medium | Monolith scaling risk — all modules compile into single deployable | All 8 modules share one process; Hangfire runs in-process with API | Traffic increase to one module affects all modules; background jobs compete with API | Plan module extraction strategy; externalize Hangfire to separate worker process |
| Medium | Stale build artifacts tracked in repo | `service/Embedding/build/lib/` contains git-tracked compiled Python files | Developers may run outdated code; repo bloat | Add `build/` and `__pycache__/` to `.gitignore`; remove tracked artifacts |
| Medium | Large migration files may cause startup delays | `ApplicationDbContextModelSnapshot.cs` ~158KB; recent migration designers ~158KB | Database initialization at startup could be slow; seeder failures block deployment | Review migration strategy; consider squash for initial schema |
| Low | No HTTPS enforcement guard beyond `UseHttpsRedirection` | `service/Api/src/Api/Program.cs:55` calls `UseHttpsRedirection()` | Mixed content or non-TLS connections in production if misconfigured | Add HSTS middleware; enforce HTTPS in production config |

### 2) Technical Debt

| Debt item | Why it exists | Where | Risk if ignored | Suggested fix |
|-----------|---------------|-------|-----------------|---------------|
| Embedded `build/lib/` artifacts | Build process leaves stale compiled Python in source tree | `service/Embedding/build/lib/` (git-tracked) | Confusing for developers; risk of running outdated code | Add `build/` and `__pycache__/` to `.gitignore`; remove tracked artifacts |
| Shared.csproj has too many dependencies | Single infrastructure project references nearly every production package | `service/Api/src/Shared/Shared.csproj` — 30+ PackageReference elements | Any NuGet version change affects all modules; tight coupling | Consider splitting Shared into smaller, purpose-specific packages |
| HTTP tests require manual data seeding | .http files must be run against a pre-seeded API instance | `ApiTests/README.md` | Test results are not deterministic; hard to reproduce failures | Add setup/teardown to .http files or migrate critical flows to integration tests |
| Code commenting guide is aspirational | Comprehensive XML guide exists but enforcement is manual | `guide/code-commenting/CommentingRules.xml` (~63KB) | Inconsistent documentation across codebase | Implement linting rules for comment format enforcement or reduce guide to enforceable subset |
| No frontend type-safe API client generation | Both SPAs have hand-rolled `api.ts` fetch wrappers with `any` types | `app/Admin/src/api.ts`, `app/Store/src/api.ts` | API changes can break frontend silently; no compile-time guard | Generate TypeScript client from OpenAPI spec; or use tRPC-style typed contracts |

### 3) Security Concerns

| Risk | OWASP category (if applicable) | Evidence | Current mitigation | Gap |
|------|--------------------------------|----------|--------------------|-----|
| Hardcoded JWT secret in committed config | A02:2021 — Cryptographic Failures | `service/Api/src/Api/appsettings.Development.json:28` — secret is plaintext in git-tracked file | Marked as development-only | Same secret may be accidentally used in production; no rotation mechanism |
| Hardcoded DB credentials in dev config | A07:2021 — Identification/Authentication Failures | `appsettings.Development.json:24` — `Password=postgres` | Development-only | Leaked credentials if repo becomes public; local dev DB may use same creds as production |
| Hardcoded gateway settings encryption key | A02:2021 — Cryptographic Failures | `appsettings.Development.json:3` — `SettingsEncryptionKey: "dev-encryption-key-32-chars-len!"` | Development-only | Encryption key in source weakens protection of payment gateway settings |
| API rate limiting is present with named policies | A04:2021 — Insecure Design | `appsettings.json:76-83` — policies for default (100/min), auth (5/min), register (3/hour), forgot-password (3/hour), payment (30/min); `Program.cs` registers `UseRateLimiter()` | Rate limiter middleware registered | [TODO] — verify policies are enforced per-endpoint; confirm auth/register policies apply to correct endpoints |
| CORS allows localhost origins in dev config | A05:2021 — Security Misconfiguration | `appsettings.Development.json:35-40` — `["http://localhost:5173", "http://localhost:4173", "http://localhost:3000"]` with `AllowCredentials: true` | Development-only | If dev config accidentally used in production, allows credentialed requests from any localhost origin |
| Anti-forgery protection present but scope unknown | A01:2021 — Broken Access Control | `Shared/Security/AntiForgery/AntiForgery.Extensions.cs` | CSRF protection configured | [TODO] — scope not verified: which endpoints are protected, is it applied globally or selectively |
| Security headers present but scope unknown | A05:2021 — Security Misconfiguration | `Shared/Security/Headers/SecurityHeadersMiddleware.cs` | Security header middleware exists | [TODO] — headers configured and enforced not verified |
| Production secrets management undefined | A05:2021 — Security Misconfiguration | `.env.template` documents dev vars; no production secrets strategy | Aspire User Secrets for development | Production secrets management (Azure Key Vault, AWS Secrets Manager, etc.) not defined |

### 4) Performance and Scaling Concerns

| Concern | Evidence | Current symptom | Scaling risk | Suggested improvement |
|---------|----------|-----------------|-------------|-----------------------|
| Database initialization includes migrations + seeding at startup | `service/Api/src/Api/Program.cs:58-60` — `app.InitializeDatabaseAsync(runMigrations: true, runSeeders: runSeeders)` | API startup time increases with migration count; seeder failures block deployment | Every instance update runs migrations against same DB; concurrent migration attempts | Separate migration from application startup (CI/CD migration step); make seeding idempotent |
| Hangfire and API share same process | Hangfire registered in `Shared/Operational/Backgrounds/`; same `WebApplication` host | Background job CPU/memory competes with API request handling | Under heavy job load, API response times degrade; under heavy API load, job processing delays | Externalize Hangfire to separate worker process/container |
| HybridCache with Redis fallback chain unclear | `Shared/Performance/Caching/Caching.Extension.cs` | Cache miss behavior and Redis unavailability strategy undefined | Cache stampede on Redis failure; in-memory caches cause inconsistencies across instances | Document cache architecture; add Redis connection resilience checks |
| No connection pool limits documented | Npgsql and Redis connection strings in dev config have no pool size configuration | Connection pool exhaustion under load | Database connection starvation for burst traffic | Configure Npgsql `MaxPoolSize`, Redis `MaxConnections`; add connection pool monitoring |

### 5) Fragile/High-Churn Areas

| Area | Why fragile | Churn signal | Safe change strategy |
|------|-------------|-------------|----------------------|
| `service/Api/src/Api/Program.cs` | DI wiring, middleware ordering, startup initialization | 15 commits in 90 days (highest churn) | Review middleware order impacts; test startup in all environments |
| `service/Api/src/Module/Payment/Features/Admin/Payments/Refund/RefundPayment.cs` | Refund logic under active refactoring | 12 commits in 90 days | Add/update tests for each refund change; verify partial refund behavior |
| `service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.cs` | Stripe webhook handler under active refactoring | 11 commits in 90 days | Verify webhook signature validation; test each event type |
| `service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/ConfirmPayment.cs` | Payment confirmation flow under active refactoring | 10 commits in 90 days | Add integration tests for confirmation flows |
| `service/Api/src/Shared/Security/Authorization/Registry/PermissionContext.cs` | Permission context changes affect authorization across modules | 10 commits in 90 days | Add/update tests for each permission change; verify no permission leakage |
| `service/Api/src/Module/Ordering/Features/Storefront/Orders/Cancel/CancelOrder.cs` | Order cancellation under active development | 10 commits in 90 days | Add integration tests for cancellation |
| `service/Api/src/Migrations/Migrations/ApplicationDbContextModelSnapshot.cs` | Schema changes accumulate in large snapshot file (~158KB) | 9 commits in 90 days | Review snapshot changes carefully in PRs; consider migration squash |
| `infra/Aspire/src/ReSys.AppHost/AppHost.cs` | Service wiring changes affect all components | 8 commits in 90 days | Test Aspire orchestration end-to-end after any AppHost change |

### 6) Dependency Risks

| Dependency | Risk | Evidence |
|------------|------|----------|
| `Microsoft.EntityFrameworkCore.InMemory` (10.0.9) | InMemory provider behavior differs from PostgreSQL (noSQL, no constraints, no transactions) | `Directory.Packages.props:38` — used in unit tests | Unit tests may pass with InMemory but fail with real PostgreSQL | Supplement unit tests with integration tests for data-critical flows |
| `OpenTelemetry.Instrumentation.EntityFrameworkCore` (1.16.0-beta.1) | Beta package in production | `Directory.Packages.props:29` | Breaking changes in beta updates; potential instability | Monitor for stable release; pin version until then |
| `OpenTelemetry.Instrumentation.StackExchangeRedis` (1.16.0-beta.1) | Beta package in production | `Directory.Packages.props:32` | Same as above for Redis instrumentation | Same mitigation as above |

### 7) `[ASK USER]` Questions

1. [ASK USER] What is the team's priority order for addressing the top risks: hardcoded dev secrets, CI/CD pipeline, module isolation enforcement, production deployment definition, Admin SPA feature completion, Embedding service runtime verification?
2. [ASK USER] Is Stripe payment processing currently in use, or is it planned for future implementation? The `Stripe.net` package is listed in `Directory.Packages.props` and Stripe gateway code exists, but `appsettings.Development.json` currently enables only `BogusGateway`.
3. [ASK USER] Should the `ValidateVerticalSliceIsolation` build target be enabled to enforce module boundaries at build time (currently disabled with `Condition="false"`)?
4. [ASK USER] Are the `app/ReSys.Admin/` (npm-based) and `app/Admin/` (pnpm-based) directories both needed, or is `app/ReSys.Admin/` a migration artifact that can be removed?
5. [ASK USER] Does the team want to move dev secrets (JWT, DB password, gateway encryption key) from committed `appsettings.Development.json` to Aspire user secrets / `.env` files now?

### 8) Evidence

- `.codebase-scan.txt` — CI/CD absence, containerization absence, security config absence, high-churn files, large file indicators
- `service/Api/src/Api/appsettings.Development.json` — Hardcoded secrets (JWT, DB connection, gateway encryption key)
- `service/Api/src/Api/appsettings.json` — Rate limiting policies, auth config, CORS defaults, storage config
- `service/Api/src/Api/Program.cs` — Startup migration/seeding, middleware order
- `service/Api/src/Api/.env.template` — Environment variable template for dev onboarding
- `Directory.Build.targets` — Disabled ValidateVerticalSliceIsolation target
- `README.md` — Work-in-progress declarations (Admin SPA, Embedding service, Dockerfiles, CI/CD)
- `AGENTS.md` — Non-negotiable rules and known issues
- `service/Embedding/src/main.py` — Embedding service entry point (imports resolve, runtime status unverified)
- `app/Admin/src/app/router/index.ts` — Admin SPA router (limited routes)
- `app/Admin/src/app/layout/main.layout.vue` — Admin layout infrastructure
- Git log — High-churn files (Program.cs 15, Payment refund/webhook/confirm files 10-12)
- `Directory.Packages.props` — Stripe.net package (present but BogusGateway enabled in dev)
- `guide/code-commenting/CommentingRules.xml` — Aspirational commenting standards
- `service/Embedding/pyproject.toml` — Python dependency requirements
- `service/Embedding/build/lib/` — Tracked stale build artifacts
