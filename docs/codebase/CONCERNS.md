# Codebase Concerns

## Core Sections (Required)

### 1) Top Risks (Prioritized)

| Severity | Concern | Evidence | Impact | Suggested action |
|----------|---------|----------|--------|------------------|
| High | No automated E2E tests — 49 `.http` files for manual testing only; no automated integration test pipeline in CI | `.github/workflows/ci.yml` runs only unit tests; `ApiTests/` directory exists but has no automated runner | Regression risk on every deployment; payment/cart flows untested in CI | Add E2E pipeline (Playwright or similar) covering checkout, payment, registration flows |
| High | `CreateOrderFromCart` handler orchestrates too many concerns (payment, stock, inventory, notification) in a single DB transaction — 25 commits in 90 days (highest churn) | `docs/codebase/.codebase-scan.txt:291` (25 commits), `CreateOrderFromCart.cs:92-118` (RepeatableRead transaction with stock deduction loop) | Bug magnet; ordering pipeline fragile under concurrent checkouts; difficult to reason about | Extract to saga/orchestrator pattern; split payment capture, stock deduction, order placement into idempotent steps |
| Medium | `ValidateVerticalSliceIsolation` build target is disabled (`Condition="false"`) — module isolation not enforced at build time | `Directory.Build.targets:44` | Modules could directly reference each other, breaking architectural constraint | Re-enable validation target after auditing existing violations |
| Medium | Dev JWT secret rejected for non-Development environments | AGENTS.md known issues, `JwtSettingsValidator` | Production deployment would fail if dev secrets pattern used | Document production secret requirements explicitly |

### 2) Technical Debt

| Debt item | Why it exists | Where | Risk if ignored | Suggested fix |
|-----------|---------------|-------|-----------------|---------------|
| Legacy admin SPA still referenced | gitignored; directory `app/legacy/ReSys.Admin/` still present in codebase | `.gitignore`, `app/legacy/ReSys.Admin/` | New contributors may find and use outdated code | Remove legacy directory or add deprecation notice |
| Stale Embedding build artifacts | `service/Embedding/build/lib/` not gitignored | AGENTS.md known issues | Confusion about deployable artifacts; git bloat | Add to `.gitignore` and clean |
| `ValidateVerticalSliceIsolation` disabled | Build target explicitly disabled (`Condition="false"`) | `Directory.Build.targets:44` | Architectural erosion over time | Fix violations, then re-enable |
| TODO comments in production code | 3 TODOs found: fulfillment service (TypeScript), StripeWebhookHandler removal, product-type delivery check | `app/legacy/ReSys.Admin/src/features/ordering/fulfillment/services/fulfillment.service.ts:14`, `Payment.Extension.cs:79`, `Order.Method.Checkout.cs:68` | Deferred features rot; legacy handler may cause confusion | Schedule sprint items for each TODO |
| JWT settings validation gap | `JwtSettingsValidator` rejects non-Development secrets; no `JwtSettings.cs` file found (settings are in `appsettings.json` only) | `appsettings.json` `Authentication.Jwt` | Production secrets may fail validation | Audit JWT config validation pipeline |
| EF Core InMemory DB used in unit tests instead of mocking DB entirely | InMemory DB doesn't support all PostgreSQL features (pgvector, transactions, sequences) | All unit test handlers use `UseInMemoryDatabase()` | Tests may pass with InMemory but fail with real PostgreSQL | Add integration test coverage for DB-specific features; consider Testcontainers for critical path tests |

### 3) Security Concerns

| Risk | OWASP category | Evidence | Current mitigation | Gap |
|------|---------------|----------|--------------------|-----|
| Hardcoded PostgreSQL credentials in dev config | A07:2021 — Identification and Authentication Failures | `appsettings.Development.json`: `Username=postgres;Password=postgres` | Used only in local dev (Aspire containers) | Acceptable for dev but must not reach production |
| Missing `.env.example` — no documented required environment variables | A05:2021 — Security Misconfiguration | Scan output: "No .env.example or .env.template found" | `appsettings.json` has placeholder values; `dotnet user-secrets` used for dev | Create `.env.example` documenting all required secrets for production |
| Anti-forgery protection enabled but configurable | A01:2021 — Broken Access Control | `appsettings.json`: `AntiForgery.IsEnabled = true`, `HeaderName = X-CSRF-TOKEN` | CSRF tokens enforced; configurable cookie policy | Ensure anti-forgery cannot be disabled in production |
| Rate limiting configured (12 policies) | A04:2021 — Insecure Design | `appsettings.json`: `RateLimit.Policies` (auth: 5/60s, register: 3/3600s, payment: 30/60s) | Brute-force protection for auth endpoints | Add rate limiting for Storefront API endpoints (cart, search) |
| File upload malware scanning disabled by default | A03:2021 — Injection | `appsettings.json`: `MalwareScanner.Enabled = false`, `DisableInDevelopment = true` | Magic byte validation; extension allowlist (blocked: .exe, .bat, .ps1, etc.) | Enable ClamAV in production; test file upload pipeline with malicious samples |
| CORS allows credentials in dev | A05:2021 — Security Misconfiguration | `appsettings.Development.json`: `Cors.AllowCredentials = true` with localhost origins | Restricted to localhost origins | Ensure production CORS is locked down |

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
| `Checkout/CreateOrderFromCart.cs` | Multi-concern orchestration (payment, stock, inventory, notification) in single transaction — 25 commits in 90 days | Top of high-churn list (`docs/codebase/.codebase-scan.txt:291`) | Extract coordination logic; add comprehensive integration test before refactoring |
| Stripe webhook handler (`StripeWebhook.cs`) | Payment webhook is inherently async and fault-prone; 18 commits | `docs/codebase/.codebase-scan.txt:293` | Add idempotency checks; ensure webhook signature verification; add dead-letter queue for failed events |
| `Payment.Extension.cs` | 18 commits — payment module DI wiring frequently changes | `docs/codebase/.codebase-scan.txt:294` | Stabilize payment provider interface; reduce churn via provider abstraction |
| `CreatePaymentIntentTests.cs` | 18 commits — tests being rewritten alongside payment changes | `docs/codebase/.codebase-scan.txt:295` | Tests churn is expected during active development; ensure test coverage keeps pace with feature changes |
| `Program.cs` / `appsettings.json` | Frequent configuration changes (18 + 16 commits) | `docs/codebase/.codebase-scan.txt:296-297` | Normal for active development; consider splitting appsettings into more focused config files |
| `CancelOrder.cs` / `RefundPayment.cs` | Cancellation and refund flows are complex state machines — 16 commits each | `docs/codebase/.codebase-scan.txt:298-299` | Ensure state machine transitions are well-tested; add idempotency for cancellation/refund |
| `Order.Result.cs` | 15 commits — error factories grow with new features | `docs/codebase/.codebase-scan.txt:300` | Accept as normal growth; consider code generation for error factories |
| `AssociateCartWithUser.cs` | 15 commits — guest-to-user cart merge is complex | `docs/codebase/.codebase-scan.txt:302` | Add unit tests for all merge edge cases (empty cart, existing cart, concurrent carts) |

### 6) `[ASK USER]` Questions

1. [ASK USER] What is the planned production deployment target? (Kubernetes, VM, cloud service?) Aspire is dev-only; no Dockerfiles or production orchestration config exists.
2. [ASK USER] What is the Bogus payment gateway for? Is it a test double or a real simulator?
3. [ASK USER] Is YARP intended for production use as an API gateway, or is it only for Aspire dev proxying?
4. [ASK USER] What are the production environment variables? No `.env.example` or deployment documentation exists.
5. [ASK USER] What is the target code coverage percentage for the C# and Vue projects? Currently no threshold enforced.
6. [ASK USER] What is the plan for the Embedding service in production? (GPU instances, separate scaling, ONNX-only inference?)
7. [ASK USER] Are there any known flaky tests that need attention?
8. [ASK USER] Is there a plan to re-enable the `ValidateVerticalSliceIsolation` build target? What violations currently exist?

### 7) Evidence

- `docs/codebase/.codebase-scan.txt` — scan output including git churn, TODOs, CI/CD config
- `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs` — high-churn checkout handler
- `Directory.Build.targets:44` — disabled `ValidateVerticalSliceIsolation` target
- `service/Api/src/Api/appsettings.json` — security and integration configuration
- `service/Api/src/Api/appsettings.Development.json` — dev configuration with hardcoded credentials
- `app/legacy/ReSys.Admin/` — legacy admin (gitignored, not removed)
- `.github/workflows/ci.yml` — no E2E or integration test jobs
- `AGENTS.md` — documented known issues
