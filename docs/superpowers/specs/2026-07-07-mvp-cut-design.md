# ReSys.Shop MVP Cut — Design

**Date:** 2026-07-07
**Status:** Approved
**Scope:** Whole solution (modular monolith, Aspire orchestration, Python Embedding sidecar)
**MVP Target:** Local demo — `dotnet run --project infra/Aspire` walks the customer + admin journey end-to-end on a seeded dev DB. Production is not the goal.

## Problem

The EShop Services Compliance Review (`docs/codebase/ESHOP-COMPLIANCE.md`, 2026-07-07) identified the solution at **~80–85% canonical eShop parity**. Multiple high-severity gaps block a local demo: a broken Python Embedding service, missing Profile APIs, missing Shipping admin, missing Webhooks, missing API gateway, no rate limiting, no Cart-expiry job, no multi-provider OAuth. Several medium/low-severity items (gateway, CI/CD, prod secrets, MFA, partial refund) are out of scope for an internal demo.

This spec defines a single MVP cut that ships a working local demo by completing the blocking items and explicitly deferring non-critical items with `[WIP-MVP]` flags. It mirrors the `plan/feature-inventory-mvp-cut-1.md` pattern (smallest shippable, defer the rest).

## Goals (CORE for MVP)

1. **Customer journey:** register → login → browse catalog → add to cart → checkout → place order → receive order confirmation.
2. **Admin journey:** login → manage catalog → manage orders → manage stock (full inventory surface) → manage shipping methods/rates → manage users/roles.
3. **Image search:** upload an image, get visually similar products.
4. **Notification preferences:** customer can opt in/out of email/SMS.
5. **Multi-provider auth:** Google + Facebook + Microsoft scaffolds.
6. **Generic outbound Webhooks service:** subscription entity, dispatcher, Hangfire worker, admin endpoints, events emitted from Ordering + Payment.
7. **Welcome email** on register + **password-reset email** on forgot/reset.
8. **Rate limiting middleware** (auth, register, forgot-password, payment, default policies).
9. **BogusGateway** for offline dev (no Stripe key required).
10. **`PaymentFeatureMetadata` refactor:** extract from `OrderingFeatureMetadata` into its own class.
11. **Wishlists API** (CRUD against existing domain entities).
12. **Cart-expiry Hangfire job** (HostedService + recurring job; cron configurable).
13. **Happy-path integration tests** for Ordering (cart/checkout) + Payment (intent) + Shipping (calculate) + Profile (notification prefs, wishlists) + Webhooks (subscription CRUD).

## Non-Goals (WIP-MVP, deferred to v1.x)

These items remain in the codebase with `[WIP-MVP]` XML docs and a `// TODO [v1.x]` marker so they are visible but disabled.

| # | Item | Where the `[WIP-MVP]` flag lives |
|---|---|---|
| 1 | YARP API gateway | `infra/Aspire/src/ReSys.AppHost/AppHost.cs` (comment on unused `Services.Gateway` constant) |
| 2 | Production-secret management | `service/Api/src/Api/appsettings.Development.json` + new `docs/security/secret-rotation.md` |
| 3 | `ValidateVerticalSliceIsolation` enforcement | `Directory.Build.targets:44` (leave `Condition="false"`) |
| 4 | CI/CD pipeline | New `docs/ci/README.md` documenting intent; no code |
| 5 | Partial refund | `Module/Payment/Features/Admin/Payments/Refund/RefundPayment.cs` — accepts `Amount` parameter, ignores it (full refund only) |
| 6 | MFA / 2FA | `Shared/Security/Authorization/Features/IdentityFeatureMetadata.cs` (comment block) |

## Out of Scope (documented only)

- Catalog `Brand` / `Type` entities — already covered by `Taxons` + `Classifications`. No work needed.
- Dockerfiles — deployment runs raw `dotnet run` / `uv run` per `AGENTS.md`.
- Any production hardening, observability beyond what's already in `Shared/Observability`.

---

## Section 1 — Per-Module Changes

### 1.1 `Module.Payment`

- **Add `BogusGateway`** at `Module/Payment/Infrastructure/Gateways/Bogus/BogusGateway.cs`:
  - Implements `IPaymentGatewayActionProvider` (the existing abstract base `Gateway.cs`).
  - Test card numbers: `4242 4242 4242 4242` → success, `4000 0000 0000 0002` → declined, `4000 0000 0000 9995` → insufficient funds.
  - Registered as the default gateway when `IConfiguration["Payment:UseBogusGateway"] == "true"`. Production setting (`false` or absent) keeps Stripe.
- **Refactor `PaymentFeatureMetadata`**: extract `.Payments` and `.PaymentMethods` permission groups from `Shared/Security/Authorization/Features/OrderingFeatureMetadata.cs:44-76` into a new `Shared/Security/Authorization/Features/PaymentFeatureMetadata.cs`. Update `Module/Payment/Features/Shared/PaymentFeature.Admin.cs:20,28,36,44,52,65,73,81,89,97` to reference the new metadata class. Update `OrderingFeatureMetadata.All` to drop the moved groups.
- **[WIP-MVP] Partial refund:** `Module/Payment/Features/Admin/Payments/Refund/RefundPayment.cs` accepts an optional `Amount` parameter but always refunds the captured total. Add `[WIP-MVP]` XML doc on the handler and a `// TODO [v1.x]` comment on the parameter.

### 1.2 `Module.Ordering`

- **Emit `OrderPlaced` event** for Webhooks service:
  - In `CreateOrderFromCart.cs:144` (after `SaveChangesAsync`), call a new `IOrderEventPublisher.PublishAsync("order.placed", payload, ct)`.
  - The publisher interface lives at `Module/Ordering/Domain/Orders/IOrderEventPublisher.cs` with a no-op default implementation (`NullOrderEventPublisher`) so the module stays decoupled from `Shared/Operational/Webhooks` (no cross-module reference).
  - The actual implementation is registered in `Program.cs` after the Webhooks subsystem is wired, swapping the no-op for a `WebhookOrderEventPublisher` that calls `IWebhookDispatcher.PublishAsync`.
- **Add Cart-expiry Hangfire job:**
  - `Module/Ordering/Backgrounds/CartExpiryJob.cs` — Hangfire `[AutomaticRetry(Attempts = 0)]` method that selects orders in `OrderStatus.Draft` where `ModifiedAtUtc < UtcNow - AfterDays` and transitions them to a new `OrderStatus.Expired` value. Cart contents are soft-deleted (set `IsDeleted = true` on the order + line items); no hard delete.
  - `Module/Ordering/Services/CartExpiryService.cs` — `IHostedService` that registers the Hangfire recurring job on startup using `Ordering:CartExpiry:Cron` from configuration.
  - Wire in `Ordering.Extension.cs` as `builder.Services.AddHostedService<CartExpiryService>()`.
- **[WIP-MVP]** Cart-expiry: a future v1.x should also expire anonymous `StockReservation` rows. Out of scope here; document in the spec only.

### 1.3 `Module.Profile`

- **Add `NotificationPreferences` API** (CORE):
  - `Module/Profile/Features/Store/NotificationPreferences/{Get, Update}/` mirroring the `Addresses` pattern (`.cs`, `.Endpoint.cs`, `.Request.cs`, `.Response.cs`, `.Validator.cs`).
  - Wire to existing `Module/Profile/Domain/Notifications/NotificationPreferences.cs` and `Domain/Preferences/UserPreference.cs` domain entities.
  - Add `ProfileFeatureMetadata.NotificationPreferences` group with `Read` / `Update` permissions.
- **Add `Wishlists` API** (CORE):
  - `Module/Profile/Features/Store/Wishlists/{Add, Remove, List, GetById, Move}/` (5 actions).
  - Wire to existing `Module/Profile/Domain/Wishlists/` + `WishedItems/` entities.
  - Add `ProfileFeatureMetadata.Wishlists` group with `List` / `Read` / `Add` / `Remove` / `Move` permissions.

### 1.4 `Module.Catalog`

- No new CORE work. `Taxons` + `Classifications` already cover the eShop `CatalogBrand` / `CatalogType` use case.
- No WIP flags needed.

### 1.5 `Module.Shipping`

- **Add admin CRUD endpoints:**
  - `Features/Admin/ShippingMethods/{Create, Get, GetById, GetPaged, Update, Delete, Activate, Deactivate}/` — 8 actions.
  - `Features/Admin/ShippingRates/{Create, Get, GetById, GetPaged, Update, Delete}/` — 6 actions.
  - Pattern: mirror `Module/Payment/Features/Admin/PaymentMethods/*` (`.cs`, `.Endpoint.cs`, `.Request.cs`, `.Response.cs`, `.Validator.cs`).
- **Add `ShippingFeatureMetadata`** at `Shared/Security/Authorization/Features/ShippingFeatureMetadata.cs` with `Methods` (List, Read, Create, Update, Delete, Activate, Deactivate) and `Rates` (List, Read, Create, Update, Delete) permission groups.

### 1.6 `Module.Inventory`

Implement deferred items from `plan/feature-inventory-mvp-cut-1.md` (Post-MVP list):

- Admin `StockItems`:
  - `POST /stock-items/{id}/restock` `Restock`
  - `GET /stock-items/low-stock` `LowStock`
  - `GET /stock-items/summary` `Summary`
  - `POST /stock-items/import` `Import` (CSV upload; reuse `IStorageService` for file handling)
  - `DELETE /stock-items/{id}` `Delete`
- Admin `StockTransfers` (full 6-action flow):
  - `Create`, `Cancel`, `Get` (Paged), `GetPaged`, `Receive`, `Transfer`.
- Admin `StockMovements`:
  - `Get` (Paged), `GetById`.
- Admin `StockReservations`:
  - `Get` (Paged), `GetById`, `Cancel`.
- Admin `StockLocations`:
  - `Delete`.
- Update `InventoryFeatureMetadata.cs` with new permissions for the added endpoints.
- Add migration `AddInventoryAdminFeatures` for any new columns/tables (likely none — most are read paths over existing entities; verify during implementation).

### 1.7 `Module.Identity`

- **Scaffold Facebook + Microsoft OAuth providers** at `Shared/Security/Authentication/External/Providers/`:
  - `Facebook/{Facebook.ExternalProvider.cs, Facebook.TokenValidator.cs, Facebook.TokenValidator.Interface.cs, Options/{FacebookProviderSetting.cs, FacebookProviderSetting.Constant.cs, FacebookProviderSetting.Validator.cs}}`
  - `Microsoft/{Microsoft.ExternalProvider.cs, Microsoft.TokenValidator.cs, Microsoft.TokenValidator.Interface.cs, Options/{MicrosoftProviderSetting.cs, MicrosoftProviderSetting.Constant.cs, MicrosoftProviderSetting.Validator.cs}}`
  - Mirror the existing Google provider structure. No real keys required for MVP demo; the providers are registered in DI behind `IConfiguration["Authentication:External:{Provider}:Enabled"]` flags.

### 1.8 `Shared.Security` — Rate Limiting

- **Add `Shared/Security/RateLimiting/RateLimit.Extensions.cs`** with `AddRateLimiter` policies:
  - `auth` policy: fixed window 5 req/min per IP, applies to `api/store/identity/auth/*`.
  - `register` policy: fixed window 3 req/hour per IP, applies to `api/store/identity/auth/register`.
  - `forgot-password` policy: fixed window 3 req/hour per IP, applies to `api/store/identity/passwords/forgot`.
  - `payment` policy: sliding window 30 req/min per user, applies to `api/storefront/payment/*`.
  - `default` policy: fixed window 100 req/min per IP, global.
- Wire into `Shared/Security/Security.Extension.cs` (`AddSecurity`).
- Add `app.UseRateLimiter()` in `Program.cs` after `UseSecurity()`.

### 1.9 `Shared.Operational` — Webhooks Subsystem

Add new shared subsystem at `Shared/Operational/Webhooks/`:

- **Domain**:
  - `WebhookSubscription` — `Id` (Guid), `Event` (string, indexed), `Url` (string), `Secret` (string, hashed), `Active` (bool), `Headers` (JSON dict), `RetryCount` (int, default 3), `CreatedAt`, `ModifiedAt`.
  - `WebhookDelivery` — `Id`, `SubscriptionId` (FK), `Event`, `Payload` (JSON), `Status` (Pending | Delivered | Failed | Dead), `AttemptCount`, `NextRetryAt`, `LastError`, `CreatedAt`, `ModifiedAt`.
- **Services**:
  - `IWebhookDispatcher` — `PublishAsync(string eventName, object payload, CancellationToken ct)` (called by event-bus adapters like `WebhookOrderEventPublisher`).
  - `IWebhookSigner` — `SignAsync(string payload, string secret)` returns HMAC-SHA256 hex.
  - `WebhookDispatcher` — uses `HttpClient` (named client) with Polly retry policy + HMAC signing.
- **Backgrounds**:
  - `WebhookDeliveryJob` — Hangfire recurring job (every 1 min) that picks `WebhookDelivery` rows in `Pending` or `Failed` with `NextRetryAt <= UtcNow`, re-dispatches, and updates state.
- **Persistence**:
  - `WebhookSchema.cs` (separate schema `webhooks`).
  - `WebhookSubscriptionConfiguration.cs`, `WebhookDeliveryConfiguration.cs`.
- **DI**:
  - `Webhooks.Extension.cs` (`AddWebhooks`) registers services, `HttpClient`, and Hangfire recurring job registration.
- **Migration**:
  - `AddWebhookSubscriptions` — creates `webhooks.webhook_subscriptions` and `webhooks.webhook_deliveries` tables.

### 1.10 `Module.Webhooks` (new module — admin endpoints)

- New module for admin endpoints to manage subscriptions:
  - `Features/Admin/Subscriptions/{Create, Get, GetById, GetPaged, Update, Delete, Test}/` — 7 actions.
  - `Test` action posts a sample event to the subscription URL and returns the response status.
  - Reuse the existing `Module.csproj` (single Module project).
  - `Webhooks.Extension.cs` (`AddWebhooksModule`).
- Wire `builder.AddWebhooksModule()` in `Program.cs` after `AddPaymentModule`.

### 1.11 `service/Embedding` (Python sidecar)

- **Fix broken imports** at `service/Embedding/src/main.py`:
  - Audit all subpackages for missing `__init__.py` (`config/`, `routers/`, `middleware/`, `models/`, `services/`, `utils/`, `dependencies/`, `infra/`, `controllers/`, `schemas/`).
  - Verify `embedding.config.settings.Settings` class is exported via `embedding/config/__init__.py`.
  - Verify router files declare `router = APIRouter(...)` at module level.
  - Verify `embedding.middleware.exception_handler.register_exception_handlers` exists and exports the function.
- Add an integration check to verify `uv run uvicorn embedding.main:app` starts and responds `200` on `/health`.
- **[WIP-MVP] model registry + embedding cache:** in-memory only for MVP. Document in spec.

### 1.12 Tests

- **Happy-path xUnit integration tests** (all under `service/Api/tests/Api.Tests/Scenarios/`):
  - `Ordering/Storefront/Cart/{CreateCart, AddItem, Checkout, Get}.IntegrationTests.cs`
  - `Ordering/Admin/Orders/{GetPaged, GetById, Approve, Cancel, Complete}.IntegrationTests.cs`
  - `Payment/{CreateIntent, Confirm, Methods}.IntegrationTests.cs` (using BogusGateway)
  - `Shipping/{Methods, Rates, Calculate}.IntegrationTests.cs`
  - `Profile/NotificationPreferences/{Get, Update}.IntegrationTests.cs`
  - `Profile/Wishlists/{Add, Remove, List}.IntegrationTests.cs`
  - `Webhooks/Subscriptions/{Create, Get, Update, Delete, Test}.IntegrationTests.cs`
- **HTTP smoke files** (all under `ApiTests/`):
  - `Ordering/{Cart, Orders}.http`
  - `Payment/{Intents, Webhooks}.http`
  - `Shipping/{Methods, Calculate}.http`
  - `Webhooks/Subscriptions.http`
  - `Embedding/{Health, Search}.http`

### 1.13 Infrastructure

- Update `infra/Aspire/src/ReSys.AppHost/AppHost.cs`:
  - Verify Embedding resource resolves (`AppHost.cs:16-23` is current).
  - Add `WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")` to the API project to ensure dev settings.
  - **No gateway** — frontends call API directly via `VITE_API_URL` (existing wiring `AppHost.cs:33,40`).

---

## Section 2 — Phased Execution Plan

### Phase 0 — Foundation refactors (1–2 days)

| Step | Task | Verification |
|---|---|---|
| 0.1 | Fix `service/Embedding/src/main.py` imports | `uv run uvicorn embedding.main:app` → 200 on `/health` |
| 0.2 | Extract `Shared/Security/Authorization/Features/PaymentFeatureMetadata.cs` from `OrderingFeatureMetadata` | `dotnet build` clean; existing Permission tests pass |
| 0.3 | Update `Module/Payment/Features/Shared/PaymentFeature.Admin.cs` to use new metadata | `dotnet build` clean |
| 0.4 | Add `BogusGateway` + test cards; register as selectable `IPaymentGatewayActionProvider` | Unit test: 4242…4242 success; 4000…0002 declined |
| 0.5 | Add `Shared/Security/RateLimiting/RateLimit.Extensions.cs` with 5 policies; wire into `Security.Extension.cs` and `Program.cs` | Unit test: 6th login in 1 min returns 429 |

### Phase 1 — Webhooks foundation (2–3 days)

| Step | Task | Verification |
|---|---|---|
| 1.1 | Add `Shared/Operational/Webhooks/Domain/{WebhookSubscription,WebhookDelivery}.cs` | `dotnet build` clean |
| 1.2 | Add `Shared/Operational/Webhooks/Persistence/Configurations/*` + `WebhookSchema.cs` | Migration generates |
| 1.3 | Add `IWebhookDispatcher` + `WebhookDispatcher` (HttpClient + Polly retry + HMAC signing) | Unit test: dispatcher posts to test URL with `X-Signature` header |
| 1.4 | Add `WebhookDeliveryJob` (Hangfire recurring) | `dotnet build` clean |
| 1.5 | Add `Webhooks.Extension.cs` (`AddWebhooks`); wire in `Program.cs` | Aspire AppHost resolves webhook resources |

### Phase 2 — Module changes (5–7 days)

| Step | Task | Verification |
|---|---|---|
| 2.1 | Add `Module/Shipping/Features/Admin/ShippingMethods/*` + `ShippingRates/*` | Unit tests for Create/Get/Update/Delete |
| 2.2 | Add `Module/Profile/Features/Store/NotificationPreferences/{Get,Update}/` | Unit test: Get returns current prefs; Update persists |
| 2.3 | Add `Module/Profile/Features/Store/Wishlists/{Add,Remove,List,GetById,Move}/` | Unit test: add → list contains; remove → list empty |
| 2.4 | Add `Module/Ordering/Backgrounds/CartExpiryJob.cs` + `Services/CartExpiryService.cs` (HostedService) | `dotnet build` clean; recurring job visible in Hangfire dashboard |
| 2.5 | Add `OrderPlaced` event emission in `CreateOrderFromCart.cs` (calls `IOrderEventPublisher.PublishAsync` with no-op default; swap to webhook publisher in `Program.cs`) | Unit test: place order → `PublishAsync` invoked with "order.placed" |
| 2.6 | Add `Module/Webhooks/Features/Admin/Subscriptions/*` (Create, Get, GetById, GetPaged, Update, Delete, Test) | Unit tests for CRUD + Test endpoint posts a sample event |
| 2.7 | Add `Shared/Security/Authentication/External/Providers/Facebook/*` and `Microsoft/*` (scaffold only; no real keys) | `dotnet build` clean; provider registered in DI |
| 2.8 | Add `Module/Inventory/Features/Admin/StockItems/{Restock,LowStock,Summary,Import,Delete}/` | Unit tests for each |
| 2.9 | Add `Module/Inventory/Features/Admin/StockTransfers/{Create,Cancel,Get,GetPaged,Receive,Transfer}/` | Unit tests for happy path + state transitions |
| 2.10 | Add `Module/Inventory/Features/Admin/StockMovements/{Get,GetById}/` | Unit test: list returns recent movements |
| 2.11 | Add `Module/Inventory/Features/Admin/StockReservations/{Get,GetById,Cancel}/` | Unit test: cancel → reservation soft-deleted |
| 2.12 | Add `Module/Inventory/Features/Admin/StockLocations/Delete` | Unit test: delete → soft-delete + audits |
| 2.13 | Add `Welcome` and `PasswordReset` email templates + wire in `Identity` Register + Passwords.Forgot/Reset handlers | Unit test: register → notification dispatched |
| 2.14 | Add migrations: `AddWebhookSubscriptions`, `AddShippingAdmin`, `AddInventoryAdminFeatures` | `dotnet ef migrations add` succeeds; `dotnet build` clean |
| 2.15 | Update `InventoryFeatureMetadata` and create `ShippingFeatureMetadata` | Permission tests pass |

### Phase 3 — Tests (2–3 days)

| Step | Task | Verification |
|---|---|---|
| 3.1 | `Api.Tests/Scenarios/Ordering/Storefront/Cart/{CreateCart,AddItem,Checkout,Get}.IntegrationTests.cs` | All pass against Testcontainer Postgres |
| 3.2 | `Api.Tests/Scenarios/Ordering/Admin/Orders/{GetPaged,GetById,Approve,Cancel,Complete}.IntegrationTests.cs` | All pass |
| 3.3 | `Api.Tests/Scenarios/Payment/{CreateIntent,Confirm,Methods}.IntegrationTests.cs` (BogusGateway) | All pass |
| 3.4 | `Api.Tests/Scenarios/Shipping/{Methods,Rates,Calculate}.IntegrationTests.cs` | All pass |
| 3.5 | `Api.Tests/Scenarios/Profile/NotificationPreferences/{Get,Update}.IntegrationTests.cs` | All pass |
| 3.6 | `Api.Tests/Scenarios/Profile/Wishlists/{Add,Remove,List}.IntegrationTests.cs` | All pass |
| 3.7 | `Api.Tests/Scenarios/Webhooks/Subscriptions/{Create,Get,Update,Delete}.IntegrationTests.cs` | All pass |
| 3.8 | `ApiTests/Ordering/{Cart,Orders}.http` smoke files | Manual run via REST Client |
| 3.9 | `ApiTests/Payment/{Intents,Webhooks}.http` smoke files | Manual run |
| 3.10 | `ApiTests/Shipping/{Methods,Calculate}.http` smoke files | Manual run |
| 3.11 | `ApiTests/Webhooks/Subscriptions.http` smoke file | Manual run |
| 3.12 | `ApiTests/Embedding/{Health,Search}.http` smoke files | Manual run |

### Phase 4 — End-to-end verification (1 day)

| Step | Task | Verification |
|---|---|---|
| 4.1 | `dotnet build` clean (no warnings, per `Directory.Build.props:17` `TreatWarningsAsErrors=true`) | `dotnet build` returns 0 |
| 4.2 | `dotnet test` all green (unit + integration) | All test projects pass |
| 4.3 | `dotnet run --project infra/Aspire/src/ReSys.AppHost` | Aspire dashboard loads; all resources Healthy |
| 4.4 | Customer happy path: register → login → browse → cart → checkout → order confirmation | Order visible in admin; webhook fires |
| 4.5 | Admin happy path: login → catalog CRUD → order approve → shipping rate create → inventory adjust | No 500s; audit trail present |
| 4.6 | Image search: upload image → similar products returned | Embedding service responds with a ranked list of at least 1 result (storefront defaults to top-5 client-side) |
| 4.7 | Webhook: subscribe to `order.placed` → place order → subscription receives event with valid HMAC | Signature validates |

### Phase 5 — Spec & commit (0.5 day)

| Step | Task |
|---|---|
| 5.1 | Write spec to `docs/superpowers/specs/2026-07-07-mvp-cut-design.md` (this file) |
| 5.2 | Self-review (placeholders, consistency, scope, ambiguity) |
| 5.3 | User reviews spec file |
| 5.4 | Commit |
| 5.5 | Invoke `writing-plans` skill to create implementation plan |

**Total effort estimate:** ~10–15 working days (2–3 weeks).

---

## Section 3 — Risk Register

| Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|
| Embedding Python imports still broken after audit | Medium | Blocks image search | Add explicit `python -c "import embedding.main"` check; if blocked, demote image search to WIP |
| `ValidateVerticalSliceIsolation` re-enabled causes cascade of build errors | High | Multi-day refactor | Already WIP — leave disabled for MVP |
| Hangfire + Redis not available in dev | Low | Cart-expiry job fails | Fallback to in-memory storage (already supported per `Background.Extension.cs:60-65`) |
| BogusGateway leaks into production | Low | Real payments routed to fake gateway | Gate registration behind `IConfiguration["Payment:UseBogusGateway"] == "true"` |
| Webhook delivery fails silently | Medium | Customers miss events | Add dead-letter queue + Hangfire dashboard alerts; spec this in WIP follow-up |
| 11 modules + Aspire wiring takes too long to start | Low | Slow dev loop | Verify cold start < 30s; if not, add `--no-cache` option |
| Cross-module references in `Module.Payment → Module.Ordering.Domain.Orders` violate module isolation | Known | Build target disabled | Already WIP; refactor in v1.x |
| Cart-expiry job deletes active carts on edge cases (clock skew, race conditions) | Low | Data loss | Use optimistic concurrency token; soft-delete only (set `Expired` status, don't hard-delete) |

## Out of Scope Confirmation

The following are **explicitly out of scope** for the MVP spec; will get their own spec in v1.x:

1. YARP API gateway
2. Production-secret management
3. `ValidateVerticalSliceIsolation` enforcement
4. CI/CD pipeline
5. Partial refund
6. MFA / 2FA
7. Catalog Brand/Type entities (Taxons/Classifications cover use case)
8. Production Dockerfiles (raw `dotnet run` + `uv run` per `AGENTS.md`)
9. Wishlists-v2 (share-with-other-users, multi-list per user, etc.)
10. Cart-expiry for `StockReservation` (covered separately by Inventory's `ReservationExpiryService`)

## Verification at a Glance

The MVP is **done** when:

- `dotnet build` is clean.
- `dotnet test` is all green (unit + integration).
- `dotnet run --project infra/Aspire` brings up Postgres, Redis, Embedding, API, Admin SPA, Store SPA with all resources Healthy.
- A new user can register (welcome email sent), log in, browse products, add to cart, check out (payment intent succeeds via BogusGateway or Stripe test mode), and see the order in their account.
- An admin can log in, CRUD products, approve/cancel orders, adjust stock, create shipping methods, manage webhook subscriptions.
- An image upload to `/api/storefront/search-by-image` returns a non-empty ranked list of similar products.
- A webhook subscription to `order.placed` receives a signed event when a new order is placed.
- All 6 `[WIP-MVP]` items have visible markers in the code with `// TODO [v1.x]` comments.
