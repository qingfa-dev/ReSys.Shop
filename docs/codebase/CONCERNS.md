# Codebase Concerns

## Core Sections (Required)

### 1) Top Risks (Prioritized)

| Severity | Concern | Evidence | Impact | Suggested action |
|----------|---------|----------|--------|------------------|
| **High** | `ValidateVerticalSliceIsolation` MSBuild target is disabled (`Condition="false"`) — cross-module references will compile. | `Directory.Build.targets:42-53` | Architectural drift over time; modules can grow tangled coupling. | Either enable the target (and resolve any current offenders) or add an `eslint`/Roslyn analyzer equivalent. |
| **High** | YARP API gateway is deferred; SPAs call the API directly via `VITE_API_URL`. | `infra/Aspire/src/ReSys.AppHost/AppHost.cs:5-7` | No central place to enforce auth, rate limits, or CORS for SPA→API traffic. | Decide a target version for the gateway and track it explicitly; until then, rely on the API-side rate-limit + CORS policies. |
| **High** | Azure Blob storage provider is configured but not implemented. | `appsettings.json:163-168` vs `Shared/Operational/Storages/Storage.Extensions.cs:79-82` (only `Local` and `S3` are registered) | Enabling `Storage.Providers.Azure.IsEnabled=true` in production will throw at runtime. | Either implement or remove the config block and update the README. |
| **Medium** | CI/CD pipeline exists (`ci.yml`) but integration tests (Testcontainers) and coverage are not yet automated; only unit tests and lint run on PR. | `.github/workflows/ci.yml:1-65` | Integration-test regressions can still land without verification. | Add an integration-test job (requires Docker-in-Docker or self-hosted runner) and opt-in coverage reporting to CI. |
| **High** | `service/Embedding/build/lib/` is checked-in to the workspace (per `ls` and AGENTS.md "Known Issues" line) and `service/Embedding/embedding.egg-info/` is also present. The `.gitignore` only matches `*.egg-info/` (file pattern) — not the directory layout, and `service/Embedding/build/` is not ignored. | `docs/codebase/.codebase-scan.txt:336`; `.gitignore:154` (`app/ReSys.Admin/` is the only `app/…` ignore; no `service/Embedding/build/` entry). | Repo bloat + risk that developers accidentally commit build artifacts. | Add `service/Embedding/build/` to `.gitignore` and remove the directory. |
| **Low** | Email-provider folder `Shared/Operational/Notifications/Channels/Emails/Providers/SendGird/` (one `r`, no plural `s`) is a folder-name typo. The runtime is fine: the code uses section `Notification:Channels:Email:Providers:SendGrids` consistently (config in `appsettings.json:206-212` matches `SendGrid.ProviderSetting.cs:12` and `SendGrid.ProviderSetting.Constant.cs:16`). | `appsettings.json:206-212`, `Shared/Operational/Notifications/Channels/Emails/Providers/SendGird/SendGrid.ProviderSetting.cs:12`, `SendGrid.ProviderSetting.Constant.cs:16` | No runtime impact, but folder name is misleading. | Rename folder `SendGird/` → `SendGrid/` (and update the namespace / using directives accordingly). |
| **Medium** | Highest churn files are in `Module/Payment` and `service/Api/src/Api/Program.cs` — see "Fragile Areas" below. | `docs/codebase/.codebase-scan.txt:287-307` (top-20 high-churn list) | Risk of regressions; lots of context to re-learn. | Add integration tests for each Payment handler and gate the AppHost composition order with a smoke test. |
| **Medium** | OTel EF Core instrumentation is on a pre-release (`1.16.0-beta.1`). | `Directory.Packages.props:29` | Pre-release packages can break. | Pin to a stable release or accept the risk with a regression test. |
| **Medium** | Aspire YARP API gateway is "deferred" but `Yarp.ReverseProxy 2.3.0` is still in `Directory.Packages.props:66` and `Microsoft.Extensions.ServiceDiscovery.Yarp` too (`Directory.Packages.props:65`). | `Directory.Packages.props:65-66`, `infra/Aspire/src/ReSys.AppHost/AppHost.cs:5-7` | Unused packages add attack surface and bloat. | Either remove or wire the gateway. |
| **Low** | `service/Api/src/Migrations/Migrations/*Designer.cs` are huge (150 KB) — EF-generated. | `docs/codebase/.codebase-scan.txt:325-328` (largest files) | Slows diffs and reviews. | Unavoidable; consider `git config diff.suppressBlankEmpty` for `*.Designer.cs`. |
| **Low** | Embedding service: `service/Embedding/build/lib/embedding/` (build artifacts) duplicates `src/embedding/`. | `docs/codebase/.codebase-scan.txt` (directory listing) | Disk + git bloat. | Already in [High] above — covered by the `build/` gitignore fix. |
| **Low** | README claims the embedding service "runtime imports are resolved; end-to-end verification pending" (i.e. WIP). | `README.md:175-178` | The docs/codebase knowledge may shift when embedding completes. | Re-run `acquire-codebase-knowledge` after the embedding sidecar's E2E is verified. |
| **Low** | AGENTS.md references `plan/` and `.harness/` directories that do not exist on disk. | `AGENTS.md:13-19` vs `ls -la plan/ .harness/` returns `---DONE---` (empty) | Stale agent instructions. | Either add those directories and contents, or update AGENTS.md. |

### 2) Technical Debt

| Debt item | Why it exists | Where | Risk if ignored | Suggested fix |
|-----------|---------------|-------|-----------------|---------------|

| `app/ReSys.Admin/` legacy SPA still in tree | Kept for compatibility; AGENTS.md says "use `app/Admin/` (pnpm) instead." | `app/ReSys.Admin/`, `.gitignore:154` (ignored) | Confusion; possible accidental npm-based setup. | Delete the directory or fully deprecate in the README. |
| `service/Embedding/build/lib/...` artifacts in workspace | Build cache not in `.gitignore`. | `service/Embedding/build/lib/` | Repo bloat. | Add to `.gitignore`; remove from working tree. |
| `service/Embedding/embedding.egg-info/` | Setuptools egg-info dir not in gitignore (only `*.egg-info/` file pattern is). | `service/Embedding/embedding.egg-info/` | Bloat. | Add `embedding.egg-info/` (or `**/embedding.egg-info/`) to `.gitignore`; remove. |
| TODO: `// TODO: Implement complex shipment creation (requires selecting stock location and inventory units)` | Marked WIP. | `app/ReSys.Admin/src/features/ordering/fulfillment/services/fulfillment.service.ts:14` | Admin fulfillment views won't work end-to-end. | Implement the shipment selection flow. |
| TODO: `// old handler bound for now — see plan TODO to remove in a follow-up.` | Likely a deprecated binding still wired. | `service/Api/src/Module/Payment/Payment.Extension.cs:75` | Two paths to the same handler. | Remove the old handler binding per the plan TODO. |
| `Directory.Build.targets:42-53` — `ValidateVerticalSliceIsolation` `Condition="false"` | Disabled, presumably to allow some early cross-module refs. | `Directory.Build.targets:44` | Module isolation is convention-only. | Enable the target, fix offenders. |
| `appsettings.json:206` — `SendGrids` (plural + extra `s`) is the canonical section name. The folder `SendGird/` is just an OCR-style typo in the folder name; the namespace and binding are correct. | Looks like a config-name drift; not yet exercised. | `appsettings.json:206-212`, `SendGrid.ProviderSetting.cs:12` | None (runtime works). | Rename folder to `SendGrid/` to match the section name. |
| `appsettings.json:163-168` — Azure provider block | Decorative config; no implementation. | `appsettings.json:163-168` | Enabling Azure storage in prod will fail. | Implement `AzureStorageProvider` or remove the block. |
| `appsettings.json:48-57` — Facebook + Microsoft OAuth blocks | Decorative; providers are not in `Shared/Security/Authentication/External/Providers/`. | `appsettings.json:48-57` | Enabling will fail. | Implement the providers or remove the config. |
| `appsettings.json:225-239` — Sinch SMS block (disabled) | Decorative; provider is wired. | `appsettings.json:225-239` | Provider is implemented (`Shared/Operational/Notifications/Channels/Sms/Providers/Sinch/`), so this is mostly fine. | Document the enablement steps. |
| `appsettings.json:113-115` — `Caching.Distributed.Type=redis` hard-coded | No alternative; in-memory distributed cache not supported. | `appsettings.json:113-115` | Redis outage = cache outage. | Add an in-memory distributed fallback. |
| `Directory.Build.targets:62-67` — `ValidateSourceLink` checks only run on Pack | CI never validates SourceLink. | `Directory.Build.targets:62-67` | Debug symbols lose source linkage in nuget packages. | Wire into CI. |

### 3) Security Concerns

| Risk | OWASP | Evidence | Current mitigation | Gap |
|------|-------|----------|--------------------|-----|
| Dev JWT secret historically in `appsettings.Development.json`; rejected in non-Development via validator | A02 (Cryptographic Failures) | `appsettings.Development.json:30-37`, `Shared/Security/Authentication/Tokens/Tokens.Extensions.cs:38-48`, commit `770b6a06` | `JwtSettingsValidator` rejects dev literal in non-Development; dev secret moved to `dotnet user-secrets` (id `resys.shop.api`) — commit `37170ef7` | None today for production. |
| Test host hardcodes secret values in `ApiFactory.cs` | A05 (Security Misconfiguration) | `service/Api/tests/Api.Tests/Infrastructure/ApiFactory.cs:53,81-83` | Values only used under `Environment=Testing` (`ApiFactory.cs:28`); `appsettings.Testing.json` also exists | Acceptable for tests, but ensure the Testing environment is never used in production. |
| `appsettings.json:38-43` JWT settings allow `RotationEnabled`, `ReuseDetectionEnabled`, etc. | A07 (Identification & Auth Failures) | `appsettings.json:38-43`, `Shared/Security/Authentication/Tokens/Services/Refresh/` (existence) | Refresh token rotation, reuse detection, blacklist service present | None. |
| Anti-forgery default-on with `Required=true` | — | `appsettings.json:69-78` | `X-CSRF-TOKEN` header required; `CookieSameSite=Strict` | None for store; admin SPA must include header. |
| `Storage.MalwareScanner.Enabled=false` by default | A03 (Injection) | `appsettings.json:150-155`, `appsettings.Development.json:52-55` | Local upload path enforces magic-byte + extension + size | In production, `MalwareScanner.Enabled=true` must be set explicitly — no default-on. |
| `Cors.Origins` accepts `http://localhost:3000` in dev | A05 | `appsettings.Development.json:38-45` | `Cors:Origins` empty in `appsettings.json:65-68` (no defaults in prod) | Production must explicitly set CORS origins. |
| `Cors.AllowCredentials=true` in dev | A05 | `appsettings.Development.json:44` | Will be `false` in prod if config inherits the default | Ensure prod keeps `false` or sets a strict allow-list. |
| Rate-limit policies exist for `auth` (5/min), `register` (3/hr), `forgot-password` (3/hr), `payment` (30/min) | A04 (Insecure Design) | `appsettings.json:80-87` | `Microsoft.AspNetCore.RateLimiting` policies bound by name | None. |
| `appsettings.json:80-87` default `default: 100/min` | A04 | `appsettings.json:81` | Reasonable baseline | Tune per traffic profile. |
| Outbound webhooks POST to configured URLs (Hangfire job) | A10 (SSRF) | `docs/codebase/INTEGRATIONS.md` (per README: "Outbound webhooks — Hangfire job POSTs `order.placed` events to configured URLs"). | Webhook code not yet inspected for allow-list / SSRF defense. | **[TODO]** Audit `Module/Ordering/` event publisher (`IOrderEventPublisher`) for URL allow-list / SSRF defense. |
| Storage security enforcer has anti-forgery guard | A04 | `Shared/Operational/Storages/Security/Guard/StorageAntiForgeryGuard.*.cs` | Consecutive-failure lockout (5/15min) per `appsettings.json:146-149` | None. |
| `appsettings.json:101` — `SensitiveHeaders: ["Authorization","Cookie","X-Api-Key"]` | A09 (Logging Failures) | `appsettings.json:101` | OTel trace redacts these headers | None. |
| `Directory.Build.targets:1-67` architecture references — no `Secret scanning` target | A05 | `Directory.Build.targets` has only reference-validation targets | None (no secret scanning in build) | Add a `gitleaks`-style pre-commit / pre-build step. |

### 4) Performance and Scaling Concerns

| Concern | Evidence | Current symptom | Scaling risk | Suggested improvement |
|---------|----------|-----------------|-------------|-----------------------|
| HybridCache `MaximumPayloadBytes=1048576` (1 MB) | `appsettings.json:119` | Large paged queries may fail to cache | Acceptable for most; spec/aggregated paged responses can exceed 1 MB. | Per-cache override. |
| In-process interceptor chain on every save (Auditable, SoftDeletable, Versionable) | `Shared/Operational/Persistence/Interceptors/Auditable.Interceptor.cs` etc. | Linear with entity count | Fine for typical web traffic; can be heavy on bulk import. | Add a bulk-import fast path that bypasses interceptors. |
| Stripe webhook handler does every operation synchronously inside the request | `Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.cs:1-145` | Slow response time on heavy webhooks | Stripe retries on timeout — risk of double-processing. | Acknowledge immediately; enqueue to Hangfire and process async. |
| `CreateProduct.cs:64-65` dispatches `AddVariant.Command` via `ISender.Send` synchronously inside `CreateProduct` | `CreateProduct.cs:60-70` | Round-trip through the MediatR pipeline for every create | Acceptable; another `SaveChanges` roundtrip for the variant | Already low-cost. |
| `CreateProduct.cs:54,70` — two `SaveChanges` per create | `CreateProduct.cs:54,70` | Extra round trip | Acceptable; allows incremental IDs | If throughput matters, use `ExecuteSqlAsync` or batched `SaveChanges`. |
| `CartExpiryJob` is in-process when Hangfire is in-memory | `service/Api/src/Module/Ordering/Backgrounds/CartExpiryJob.cs`, `Shared/Operational/Backgrounds/Background.Extension.cs:62-79` | No cross-instance coordination | Two replicas both run the job → duplicate work | Switch to Redis storage in multi-instance deployments. |
| `appsettings.json:104-122` — `Caching.Distributed.Enabled=true` by default | `appsettings.json:113-115` | All envs try to reach Redis | If Redis is down, HybridCache will fail | Add a fallback or fail-open mode. |
| `Ordering` module's `InProcessOrderEventPublisher` (Channel<T>) and `LoggingNullOrderEventPublisher` (commits `42c4ef14`, `ce6a3000`) are present in git history but deleted from the current working tree (`git status` shows both as deleted). | Imported from the prior commit set as part of a larger deletion sweep. | `Ordering` may not be able to dispatch `OrderPlacedEvent` / similar events at all. | Restore the files (or document the removal as intentional in `Ordering.Extension.cs` and the event-publisher registration). |
| `service/Api/src/Migrations/Migrations/20260711090657_InitialCreate.cs` is 104 KB; `…Designer.cs` is 153 KB | `docs/codebase/.codebase-scan.txt:325-332` | Slow `dotnet ef migrations add`; large diffs | Maintenance burden | Squash into a single baseline migration if the project is pre-release. |
| `app/Admin/src/shared/api/http/api.client.ts:1-92` axios client retries on 401 once via `refreshTokens` | `api.client.ts:65-82` | Adds one network round-trip on 401 | Acceptable | Document the refresh-token contract for new SPAs. |
| `Module.Catalog/Features/Admin/Products/Variants/Images/Embeddings/...` runs the embedding sidecar synchronously per image | `Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Services/ImageEmbedding.Orchestrator.cs` (existence) | Slow image uploads | Acceptable | Add a Hangfire background job. |
| `Domain/.../Method.*` files split per concern | e.g. `Module/Ordering/Domain/Orders/Order.Method.{Availability,Scopes,Searchable,Slugs,Status}.cs` | Compiles to one class — no runtime cost | Slower compile if many partials | Acceptable. |

### 5) Fragile/High-Churn Areas

From the last 90 days of git history (top 20 churn — `docs/codebase/.codebase-scan.txt:287-307`):

| Area | Why fragile | Churn signal | Safe-change strategy |
|------|-------------|-------------|----------------------|
| `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs` (25 changes) | Order placement is the highest-stakes business flow (price, inventory, payment). | 25 commits | Add exhaustive integration tests for happy + edge paths (stock-out, payment failure, partial cart). |
| `service/Api/src/Api/Program.cs` (18 changes) | Composition root; every cross-cutting change lands here. | 18 commits / 90 days | Add an integration smoke test that boots the host and asserts `/health` and `/alive` work. Avoid adding new services here unless necessary. |
| `service/Api/src/Module/Payment/Payment.Extension.cs` (18 changes) | Gateway DI + the old handler binding TODO. | 18 commits | Resolve the plan TODO first, then gate the module surface with integration tests. |
| `service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.cs` (18 changes) | Stripe signature + dispatch logic; mis-handling → silent double-charge. | 18 commits | Add a Stripe replay integration test; verify idempotency. |
| `service/Api/tests/Module.UnitTests/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntentTests.cs` (18 changes) | Test file in lockstep with `Payment.Extension.cs`. | 18 commits | Keep test fakes for gateway small; consider snapshotting gateway responses. |
| `service/Api/src/Module/Payment/Features/Admin/Payments/Refund/RefundPayment.cs` (16 changes) | Refund flow — money moving backwards. | 16 commits | Add an integration test against the Bogus gateway. |
| `service/Api/src/Module/Ordering/Features/Storefront/Orders/Cancel/CancelOrder.cs` (16 changes) | Order cancellation — inventory release, refund trigger. | 16 commits | Add integration tests for cancellation with partial shipments. |
| `service/Api/tests/Module.UnitTests/Payment/Features/Storefront/Payment/Confirm/ConfirmPaymentTests.cs` (15 changes) | Confirms Stripe payment intent status transitions. | 15 commits | Use a Stripe test fixture if available. |
| `service/Api/tests/Module.UnitTests/Payment/Features/Storefront/Payment/Webhooks/StripeWebhookTests.cs` (15 changes) | In lockstep with `StripeWebhook.cs`. | 15 commits | Same as StripeWebhook. |
| `service/Api/src/Module/Ordering/Features/Storefront/Cart/AssociateCart/AssociateCartWithUser.cs` (15 changes) | Cart ownership transfer — guest to user. | 15 commits | Add integration tests for concurrent cart association. |
| `service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs` (14 changes) | Creates Stripe payment intent; money-moving. | 14 commits | Same as refund. |
| `service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/ConfirmPayment.cs` (14 changes) | Confirm intent state transition. | 14 commits | Same as CreateIntent. |
| `service/Api/src/Module/Ordering/Domain/Orders/Order.Result.cs` (14 changes) | Order result types — central to all ordering flows. | 14 commits | Lock result semantics with spec tests. |
| `service/Api/src/Module/Ordering/Ordering.Extension.cs` (14 changes) | Ordering module DI registration. | 14 commits | Add an integration smoke test for Ordering module boot. |
| `service/Api/tests/Module.UnitTests/Payment/Features/Admin/Payments/Void/VoidPaymentTests.cs` (14 changes) | Same as refund; voids also move money. | 14 commits | Same as refund. |
| `service/Api/src/Module/Ordering/Features/Admin/Orders/Cancel/CancelOrderAdmin.cs` (13 changes) | Admin order cancellation. | 13 commits | Same as CancelOrder. |
| `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateStatus/UpdateOrderStatus.cs` (13 changes) | Order status transitions. | 13 commits | Add state-machine tests. |
| `service/Api/src/Api/appsettings.json` (13 changes) | Config drift; high blast radius. | 13 commits | Add schema validation; keep new keys in `appsettings.Development.json` first. |
| `service/Api/tests/Module.UnitTests/Payment/Features/Admin/Payments/Capture/CapturePaymentTests.cs` (13 changes) | Capture money-moving flow. | 13 commits | Same as refund. |
| `service/Api/tests/Module.UnitTests/Payment/Features/Admin/Payments/Refund/RefundPaymentTests.cs` (13 changes) | Same as refund handler. | 13 commits | Same as refund. |

**Pattern:** Payment is the highest-churn module, both in handler and test code. Treat any change to Payment handlers with extra rigor (integration test + manual smoke).

### 6) `[ASK USER]` Questions

1. **[ASK USER]** Should `app/ReSys.Admin/` (the legacy npm-based admin SPA) be deleted, or is it intentionally preserved for reference? `.gitignore:154` already ignores it, so it's invisible to clean checkouts.
2. **[ASK USER]** Should `service/Embedding/build/lib/` and `service/Embedding/embedding.egg-info/` be added to `.gitignore` (and the artifacts removed from history), or is the build directory expected to ship with the repo?
3. **[ASK USER]** The email-provider folder `SendGird/` is a typo. The runtime section name `Notification:Channels:Email:Providers:SendGrids` is consistent in both `appsettings.json` and the code (`SendGrid.ProviderSetting.cs:12`), so dispatch works. Should the folder be renamed to `SendGrid/` to match? (No code changes needed; just a folder rename + namespace update.)
4. **[ASK USER]** The `Storage.Providers.Azure` block exists in `appsettings.json` but no `AzureStorageProvider` class is registered. Should the block be removed, or is there a plan to implement it?
5. **[ASK USER]** The `ValidateVerticalSliceIsolation` MSBuild target is disabled (`Condition="false"`). Should it be enabled (and any current cross-module references cleaned up), or kept disabled to permit some specific scenario?
6. **[ASK USER]** Are the `Authentication.Facebook` and `Authentication.Microsoft` config blocks (`appsettings.json:48-57`) intentional placeholders, or should they be removed until implemented?
7. **[ASK USER]** Is there a defined target version (e.g. v1.x) for the deferred YARP API gateway (`infra/Aspire/src/ReSys.AppHost/AppHost.cs:5-7`)? Should the `Yarp.ReverseProxy` and `Microsoft.Extensions.ServiceDiscovery.Yarp` packages be removed in the meantime?
8. **[ASK USER]** The embedding sidecar's `README.md:175-178` states "end-to-end verification pending". Is there a target milestone, and is there a design doc or test plan I should reference when this is revisited?
9. **[ASK USER]** Are there any SDLC/security policies (CODEOWNERS, threat model, SBOM, security.txt) that should be referenced or generated? `docs/codebase/.codebase-scan.txt:341` reports no security configs detected.

### 7) Evidence

- `docs/codebase/.codebase-scan.txt` — full scan (high-churn, top files, CI/CD detected, no containerization, no security configs)
- `.gitignore` (root) — confirms `app/ReSys.Admin/` ignored, no `service/Embedding/build/`, no `embedding.egg-info/`
- `Directory.Build.targets:1-68` — architecture validation targets, including the disabled `ValidateVerticalSliceIsolation`
- `service/Api/src/Api/Program.cs:1-66` — composition root (high-churn)
- `service/Api/src/Api/appsettings.json:1-243` — runtime config
- `service/Api/src/Api/appsettings.Development.json:1-84` — dev config
- `infra/Aspire/src/ReSys.AppHost/AppHost.cs:1-49` — Aspire wiring + YARP deferral comment
- `service/Api/src/Shared/Operational/Storages/Storage.Extensions.cs:79-82` — only Local + S3 registered
- `service/Api/src/Shared/Operational/Notifications/Channels/Emails/Providers/SendGird/` (folder name vs `SendGrids` config)
- `service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.cs:1-145` — high-churn handler
- `service/Api/src/Module/Payment/Payment.Extension.cs:75` — old handler binding TODO
- `service/Api/src/Module/Ordering/Backgrounds/CartExpiryJob.cs:1-30+` — Hangfire job
- `app/Admin/src/features/ordering/fulfillment/services/fulfillment.service.ts:14` — `// TODO: Implement complex shipment creation (requires selecting stock location and inventory units)`
- `service/Api/src/Migrations/Migrations/20260711090657_InitialCreate.cs` (104 KB) + `20260712050728_FixPaymentMethodSettingsColumnType.cs` — large migration + a small follow-up fix
- `service/Embedding/build/lib/` — check-in artifacts
- `service/Embedding/embedding.egg-info/` — egg-info
- `app/ReSys.Admin/` — legacy admin SPA
- `README.md:1-184` — intent + WIP notes
- `AGENTS.md:1-80` — agent guide (references `plan/`, `.harness/` which now exist on disk)
