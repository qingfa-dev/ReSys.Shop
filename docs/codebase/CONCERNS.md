# Codebase Concerns

## Core Sections (Required)

### 1) Top Risks (Prioritized)

| Severity | Concern | Evidence | Impact | Suggested action |
|----------|---------|----------|--------|------------------|
| High | No automated E2E tests — 49 `.http` files for manual testing only; no automated integration test pipeline in CI | `.github/workflows/ci.yml` runs only unit tests; `ApiTests/` and `Api.SmokeTests` directories exist but have no automated runner | Regression risk on every deployment; payment/cart flows untested in CI | Add E2E pipeline (Playwright or similar) covering checkout, payment, registration flows |
| High | `CreateOrderFromCart` handler orchestrates too many concerns (payment, stock, inventory, notification) in a single DB transaction — 30 commits in 90 days (highest churn) | `docs/codebase/.codebase-scan.txt:291` (30 commits), `CreateOrderFromCart.cs:92-118` (RepeatableRead transaction with stock deduction loop) | Bug magnet; ordering pipeline fragile under concurrent checkouts; difficult to reason about | Extract to saga/orchestrator pattern; split payment capture, stock deduction, order placement into idempotent steps |
| Low | `eslint-plugin-boundaries` — RESOLVED (2026-08-11) | Package removed from `app/Admin/package.json`; was never used. No longer present. | `app/Admin/package.json` | n/a | RESOLVED — dependency pruned |
| Medium | Dev JWT secret rejected for non-Development environments | AGENTS.md known issues, `JwtSettingsValidator` | Production deployment would fail if dev secrets pattern used | Document production secret requirements explicitly |
| Medium | PrimeVue major version bump (^4.5.5 → ^5.0.0) introduces breaking changes | `app/Admin/package.json` | Admin SPA may have breaking API changes or bugs from PrimeVue migration | Audit all PrimeVue 5.0 breaking changes; update components accordingly |

### 2) Technical Debt

| Debt item | Why it exists | Where | Risk if ignored | Suggested fix |
|-----------|---------------|-------|-----------------|---------------|
| Dashboard module — RESOLVED (2026-08-11) | 9 feature files under `Module/Dashboard/` but `builder.AddDashboardModule()` not called in `Program.cs` — now registered via `Dashboard.Extension.cs` | `service/Api/src/Module/Dashboard/` | n/a | RESOLVED — Dashboard now live at `GET /api/admin/dashboard` with `Sales.List` permission |
| `eslint-plugin-boundaries` — RESOLVED (2026-08-11) | Package removed from `app/Admin/package.json`; was never used | `app/Admin/package.json`, `eslint.config.ts` | n/a | RESOLVED |

| Admin SPA cross-feature imports (deliberate deviation) | 6 views import from other feature modules for read-only lookups: `dashboard/views/DashboardPage.vue`→identity/userStore; `inventory/views/StockItemDetail.vue` + `StockTransferDetail.vue`→catalog/variantApi; `ordering/views/OrderDetail.vue`→payment/paymentApi; `profile/views/AddressDetail.vue` + `AddressesList.vue`→auth/authStore. Violates the plan 2026-07-28-admin-feature-routes.md "no cross-feature imports except via shared/" convention; accepted deliberately to reuse the pre-existing data layer | `app/Admin/src/features/{dashboard,inventory,ordering,profile}/views/` | Feature boundaries erode; a shared/ refactor would centralize variant/payment/auth lookups | Route reads behind `shared/` composables (e.g. `useVariants`) or enforce via `eslint-plugin-boundaries` (already a dead dependency) |
| Legacy admin SPA still referenced | gitignored; directories `app/legacy/ReSys.Admin/`, `app/legacy/Admin/`, `app/legacy/shop/` still present in codebase | `app/legacy/` | New contributors may find and use outdated code; git bloat | Remove legacy directories or add deprecation notices |
| Stale Embedding build artifacts | `service/Embedding/build/lib/` not gitignored | AGENTS.md known issues | Confusion about deployable artifacts; git bloat | Add to `.gitignore` and clean |
| `eslint-plugin-boundaries` installed but dead code — REMOVED | Listed in `dependencies` but not imported in `eslint.config.ts`; no boundaries config exists — PRUNED from `package.json` (2026-08-11) | `app/Admin/package.json` | n/a | RESOLVED — dependency cleaned |
| Admin list views paginate/sort client-side over one fetched page | All DataTable list views (Products, Users, Roles, StockItems, Orders, Payments, ShippingMethods, etc.) use `usePagedQuery` but don't wire paginator `@page`/`:totalRecords` or `@sort`→`setSort`/`setPage`; paginator and `:sortable` headers operate only on the first 20 server rows | `app/Admin/src/features/*/views/*List.vue` | Sorting/pagination silently misleading on large datasets; server sort params never exercised | Build a shared `AppDataTable` wrapper wiring `setPage`/`setSort`/`totalRecords`; roll out across list views |
| TODO comments in production code | 3 TODOs: fulfillment service (TypeScript), StripeWebhookHandler removal, product-type delivery check | `app/legacy/ReSys.Admin/...`, `Payment.Extension.cs:79`, `Order.Method.Checkout.cs:68` | Deferred features rot; legacy handler may cause confusion | Schedule sprint items for each TODO |
| JWT settings validation gap | `JwtSettingsValidator` rejects non-Development secrets | `appsettings.json` `Authentication.Jwt` | Production secrets may fail validation | Audit JWT config validation pipeline |
| EF Core InMemory DB used in unit tests instead of mocking DB entirely | InMemory DB doesn't support all PostgreSQL features (pgvector, transactions, sequences) | All unit test handlers use `UseInMemoryDatabase()` | Tests may pass with InMemory but fail with real PostgreSQL | Add integration test coverage for DB-specific features |
| SECURITY.md missing | No security policy or vulnerability reporting process | (nonexistent) | External researchers have no way to report vulnerabilities | Create SECURITY.md with disclosure policy |
| No coverage threshold enforced (C#/Vue) | Coverage is opt-in only; target is 70-80% | `.github/workflows/ci.yml` — no coverage step for C#/Vue | Unclear code quality baseline | Add coverage to CI: `--cov-fail-under=70` for benchmarks, equivalent thresholds for C# and Vue |

### 3) Security Concerns

| Risk | OWASP category | Evidence | Current mitigation | Gap |
|------|---------------|----------|--------------------|-----|
| Hardcoded test JWT secret in testing config | A07:2021 — Identification and Authentication Failures | `appsettings.Testing.json`: `"integration-test-secret-key-32-chars!!"` | Testing-only environment; not used in production | Acceptable for testing; ensure testing config never reaches production |
| Missing `SECURITY.md` | A05:2021 — Security Misconfiguration | No `SECURITY.md` file exists in repository | (none) | Create `SECURITY.md` with vulnerability disclosure process |
| `.env.template` files exist and are safe | A05:2021 — Security Misconfiguration | `service/Api/src/Api/.env.template`, `service/Embedding/.env.template` — all values are `REPLACE_ME_*` or empty | Templates are intentionally tracked | None; good practice |
| Anti-forgery protection enabled but configurable | A01:2021 — Broken Access Control | `appsettings.json`: `AntiForgery.IsEnabled = true`, `HeaderName = X-CSRF-TOKEN` | CSRF tokens enforced; configurable cookie policy | Ensure anti-forgery cannot be disabled in production |
| Rate limiting configured (12 policies) | A04:2021 — Insecure Design | `appsettings.json`: `RateLimit.Policies` (auth: 5/60s, register: 3/3600s, payment: 30/60s) | Brute-force protection for auth endpoints | Add rate limiting for Storefront API endpoints (cart, search) |
| File upload malware scanning disabled by default | A03:2021 — Injection | `appsettings.json`: `MalwareScanner.Enabled = false`, `DisableInDevelopment = true` | Magic byte validation; extension allowlist | Enable ClamAV in production; test file upload pipeline with malicious samples |
| CORS allows credentials in dev | A05:2021 — Security Misconfiguration | `appsettings.Development.json`: `Cors.AllowCredentials = true` with localhost origins | Restricted to localhost origins | Ensure production CORS is locked down |
| PR template lacks security checklist items | A05:2021 — Security Misconfiguration | `.github/PULL_REQUEST_TEMPLATE.md` — no "secrets not committed", "dependency audit", "input validation" checks | Template exists but focuses on code quality only | Add security-specific review items to PR template |

### 4) Performance and Scaling Concerns

| Concern | Evidence | Current symptom | Scaling risk | Suggested improvement |
|---------|----------|-----------------|-------------|-----------------------|
| Checkout stock deduction loops over all stock locations sequentially | `CreateOrderFromCart.cs:103-118` — `foreach (var si in stockItems)` with linear scan per line item | Under high concurrency, checkout latency grows with stock location count and order line items | Transaction duration increases linearly; RepeatableRead isolation may cause contention | Batch stock deduction via SQL; consider stock reservation pattern |
| In-memory cache in multi-instance setup (HybridCache mitigates this) | `appsettings.json`: `Caching.Hybrid.Enabled = true` with Redis L2 | (Mitigated) — HybridCache uses Redis for distributed L2 | Single-instance memory cache can serve stale data | Ensure Redis L2 is properly configured in production |
| Embedding service called synchronously over HTTP | Aspire `AppHost.cs`: API references Embedding with `WithHttpHealthCheck("/health")` | ML inference latency added to API response time | Scaling: ML service becomes bottleneck under load | Add async/background embedding generation; cache embeddings per image hash |
| No pagination on some list endpoints | [TODO] — needs endpoint audit | Large datasets may cause timeouts or memory issues | Linear scaling with data growth | Audit all list endpoints for pagination; enforce default page size |
| YARP referenced but production config unknown | `Directory.Packages.props`: YARP 2.3.0 referenced but no YARP config found in repo | [TODO] | Unknown if rate limiting / load balancing works in production | Document or create production YARP configuration |

### 5) Fragile/High-Churn Areas

| Area | Why fragile | Churn signal | Safe change strategy |
|------|-------------|-------------|----------------------|
| `Checkout/CreateOrderFromCart.cs` | Multi-concern orchestration (payment, stock, inventory, notification) in single transaction — 30 commits in 90 days | Top of high-churn list (`docs/codebase/.codebase-scan.txt:291`) | Extract coordination logic; add comprehensive integration test before refactoring |
| Stripe webhook handler (`StripeWebhook.cs`) | Payment webhook is inherently async and fault-prone; 20 commits | `docs/codebase/.codebase-scan.txt:293` | Add idempotency checks; ensure webhook signature verification; add dead-letter queue for failed events |
| `Payment.Extension.cs` | 20 commits — payment module DI wiring frequently changes | `docs/codebase/.codebase-scan.txt:294` | Stabilize payment provider interface; reduce churn via provider abstraction |
| `CreatePaymentIntentTests.cs` | 20 commits — tests being rewritten alongside payment changes | `docs/codebase/.codebase-scan.txt:295` | Tests churn is expected during active development; ensure coverage keeps pace |
| `Program.cs` / `appsettings.json` | Frequent configuration changes (19 + 18 commits) | `docs/codebase/.codebase-scan.txt:297-298` | Normal for active development; consider splitting appsettings into more focused config files |
| `CancelOrder.cs` / `RefundPayment.cs` | Cancellation and refund flows are complex state machines — 19 + 18 commits each | `docs/codebase/.codebase-scan.txt:299-300` | Ensure state machine transitions are well-tested; add idempotency for cancellation/refund |

### 6) Resolved Decisions

1. **Production deployment**: Local dev only for now. Aspire is the primary orchestration. No cloud/K8s target yet.
2. **Bogus payment gateway**: Not used — actual payment is via Stripe. Bogus entry is a placeholder/test double.
3. **YARP**: Dev and test only for now. Not configured for production.
4. **Production env vars**: Placeholders in `.env.template` files are intentional. Real values TBD when deployment is scoped.
5. **Code coverage target**: 70-80% minimum across C# and Vue projects.
6. **Embedding service production**: CPU-based deployment first, GPU later. Dockerfile exists for the Python sidecar.
7. **Flaky tests**: None known.
8. **Cross-module references**: Permitted — modules share one assembly, and direct `using`, EF Core FKs, navigations, and service calls are allowed. Prefer MediatR `ISender` for cross-module behavior so it flows through the pipeline; direct calls are fine when they fit the feature slice. No whitelist or drift check (removed 2026-08-16).
9. **Dashboard module**: Should be registered as the 9th module.
10. **Naming convention (`Store` vs `Storefront`)**: Standardize on `Storefront` across all modules.
11. **`eslint-plugin-boundaries`**: REMOVED from package.json (2026-08-11). The dead dependency was pruned.

### 8) Evidence

- `docs/codebase/.codebase-scan.txt` — scan output including git churn, TODOs, CI/CD config
- `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs` — high-churn checkout handler
- `service/Api/src/Api/appsettings.json` — security and integration configuration
- `service/Api/src/Api/appsettings.Development.json` — dev configuration (secrets externalized to user-secrets)
- `app/legacy/ReSys.Admin/` — legacy admin (gitignored, not removed)
- `.github/workflows/ci.yml` — no E2E or integration test jobs
- `AGENTS.md` — documented known issues
