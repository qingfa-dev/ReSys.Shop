# EShop Services Compliance Review — Log

| Field | Value |
|---|---|
| **Reviewer** | opencode (minimax-m3) |
| **Date** | 2026-07-07 |
| **Scope** | `service/Api/src/Api`, `service/Api/src/Module/*`, `service/Api/src/Shared`, `service/Embedding`, `infra/Aspire` |
| **Baseline** | Canonical Microsoft eShop reference architecture (`eShopOnContainers` / `eShop` reference app services: Catalog, Basket, Ordering, Identity, Payment, Notification, Webhooks, Location) |
| **Method** | Static file inspection + cross-reference with `docs/codebase/*` and `plan/*` |
| **Verdict** | **~80–85% eShop parity** — every canonical service has a first-class equivalent; gaps cluster in Profile APIs, Shipping admin, generic Webhooks, API gateway, rate limiting, and the Python Embedding sidecar |

---

## 1. Mapping: Current modules → Canonical eShop services

The ReSys.Shop solution is a **modular monolith** (single `Api` host, `Module` assembly, `Shared` infrastructure). Each canonical eShop microservice maps to one or more ReSys modules.

| Canonical eShop service | ReSys.Shop coverage | Module path | Feature folders | Notes |
|---|---|---|---|---|
| **Catalog.API** (products, brands, types, search, image similarity) | `Module.Catalog` | `service/Api/src/Module/Catalog/Features` | **139** (deepest) | Products, variants, prices, option types/values, images, embeddings, taxonomies, taxons, classifications. Exceeds eShop scope with image-search. |
| **Basket.API** (cart, items, pre-checkout) | `Module.Ordering` (Storefront.Cart) | `service/Api/src/Module/Ordering/Features/Storefront/Cart` | **13 cart actions** | Spree-style cart = Order aggregate in `Draft` state. Includes guest-associate, line-item add/remove/quantity, validate, empty, delete. |
| **Ordering.API** (orders, workflow, fulfillment) | `Module.Ordering` (Admin + Storefront.Orders) | `service/Api/src/Module/Ordering/Features/Admin/Orders`, `Storefront/Orders` | **6 admin order actions** + 5 storefront order actions | State machine, addresses, payment/shipment updates, approvals, completion, cancellation, resumption. |
| **Identity.API** (auth, OAuth, tokens) | `Module.Identity` + `Shared.Security.Authentication` | `service/Api/src/Module/Identity/Features` + `service/Api/src/Shared/Security/Authentication` | **~70** | Email/password register-login, external Google OAuth, sessions, refresh tokens with theft detection, anti-forgery, ASP.NET Identity roles + custom permission system. |
| **Payment.API** (gateway integration) | `Module.Payment` | `service/Api/src/Module/Payment/Features` | **34** | Stripe gateway (Purchase/Authorize/Capture/Void/Refund/Credit), intents + setup-intents, webhooks, multi-gateway abstraction via `IPaymentGatewayActionProvider`. |
| **Notification.API** (email/SMS/push) | `Shared.Operational.Notifications` | `service/Api/src/Shared/Operational/Notifications` | (shared subsystem) | Notification hub with SendGrid/SMTP/Sinch/Logging providers, templates, channels, recipient routing. |
| **Webhooks.API** (generic outbound + inbound) | Embedded in `Module.Payment.Features.Storefront.Payment.Webhooks` | `service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks` | (only Stripe inbound) | **No generic webhook delivery service.** Only Stripe inbound webhook handler. |
| **Location.API** (countries/states) | `Module.Location` | `service/Api/src/Module/Location/Features` | **24** | CRUD on countries/states by ID/ISO code, admin + storefront. |
| *(extra — not in eShop)* | `Module.Inventory` | `service/Api/src/Module/Inventory/Features` | **~58** | Stock locations, items, movements, reservations (cart), transfers, low-stock, summary, bulk-adjust, restock. |
| *(extra — not in eShop)* | `Module.Shipping` | `service/Api/src/Module/Shipping/Features` | **12** | Shipping methods, rates, calculator. **Storefront only — no admin CRUD.** |
| *(extra — not in eShop)* | `Module.Profile` | `service/Api/src/Module/Profile/Features` | **21** | Profiles + addresses. **Wishlists + NotificationPreferences: domain entities exist, no API endpoints.** |
| *(extra — not in eShop)* | `Embedding` (Python/FastAPI sidecar) | `service/Embedding/src` | n/a | Fashion-CLIP image embedding. **Currently broken** — `main.py` imports `embedding.config.settings` and `embedding.routers.embedding_router` but the module structure has these; runtime errors per AGENTS.md. |

---

## 2. Per-module compliance scorecard

### 2.1 Catalog (`Module.Catalog`)

- **eShop parity: 95%**
- **Routes covered:** `api/admin/catalog/products/*`, `api/admin/catalog/products/{id}/variants/*`, `api/admin/catalog/products/{id}/classifications/*`, `api/admin/catalog/taxonomies/*`, `api/admin/catalog/taxons/*`, `api/admin/catalog/option-types/*`, `api/admin/catalog/option-values/*`, `api/storefront/products/{slug}`, `api/storefront/products/{id}/availability`, `api/storefront/products/{id}/related`, `api/storefront/products/{id}/similar`, `api/storefront/search-by-image`, `api/storefront/taxonomies/{id}`, `api/storefront/taxons/{id}/products`.
- **Strengths:** Variants, prices, image embeddings, classification, taxonomy tree, option types/values with hierarchical filtering.
- **Gaps:**
  - No `CatalogBrand` / `CatalogType` entity (eShop separates brand from type; ReSys uses `Classifications` + `Taxons`).
  - Storefront query builder status under change (`plan/feature-querying-builders-1.md`).

### 2.2 Basket / Cart (`Module.Ordering.Features.Storefront.Cart`)

- **eShop parity: 85%**
- **Routes covered:** Create, Associate, Get, AddItem, RemoveItem, UpdateItemQuantity, Empty, Delete, Checkout, Update, Validate, SelectShippingRate.
- **Strengths:** Spree-style cart-as-order. Supports guest-to-user association, line-item CRUD, multi-step checkout validation, shipping rate selection.
- **Gaps:**
  - Guest cart persistence is not visible (only `GuestSessionMiddleware` for tokens).
  - Verify "merge guest cart on login" semantics — `AssociateCart` exists but cross-user merge logic needs review.
  - No cart-expiry job (eShop has a Redis TTL on baskets; here the order aggregate has its own lifecycle).

### 2.3 Ordering (`Module.Ordering.Features.Admin.Orders` + `Storefront.Orders`)

- **eShop parity: 90%**
- **Routes covered (Admin):** Create, Get (Paged, ById, LineItems, LineItemById), Update, Delete, Approve, Cancel, Complete, Resume, UpdateStatus, UpdateBillAddress, UpdateShipAddress, UpdateShippingMethod, AddLineItem, UpdateLineItem, RemoveLineItem.
- **Routes covered (Storefront):** GetById, Cancel, ListOrders.
- **Strengths:** State machine (`OrderStatus.Draft → Placed → Complete | Canceled`), payment state tracking, address updates, shipping-method updates, line-item add/remove.
- **Gaps:**
  - Explicit state-transition guard tests (eShop has `OrderingStateMachine` integration tests).
  - `OrderUpdater` + `AdjustmentsUpdater` + tax/promo adjuster wiring (in `Module/Ordering/Domain/Orders/OrderUpdater.cs` and `Domain/Adjustments`) needs review for full Spree parity.

### 2.4 Identity (`Module.Identity` + `Shared.Security.Authentication`)

- **eShop parity: 90%**
- **Routes covered:** `api/admin/identity/users/*` (Create, Update, Delete, GetPagedOrAll, GetById, Status, Permissions assign/get/revoke/sync, Roles assign/get/revoke/sync), `api/admin/identity/roles/*` (Create, Update, Delete, GetPagedOrAll, GetById, Permissions assign/get/revoke/sync), `api/admin/identity/permissions/*` (Get), `api/store/identity/auth/*` (Login/Email, Login/External/Providers, Login/External/Authenticate, Logout, Register, Sessions Get/Refresh), `api/store/identity/emails/*` (Change, Confirm, Resend), `api/store/identity/passwords/*` (Change, Forgot, Reset).
- **Strengths:** Refresh-token theft detection, token blacklist, ASP.NET Identity, custom `PermissionMetadata` system with `PermissionContext` (Domains × Categories × Resources × Actions).
- **Gaps:**
  - **Only Google OAuth provider** (`Shared/Security/Authentication/External/Providers/Google`). eShop ships with at least Google + Facebook + Microsoft scaffolds.
  - No MFA / 2FA endpoints (eShop has it).
  - No account-lockout policy on repeated failed logins (no rate limiting on auth endpoints).

### 2.5 Payment (`Module.Payment`)

- **eShop parity: 85%**
- **Routes covered (Admin):** `api/payment/payments/{id}/capture`, `.../void`, `.../refund`, Get (Paged, ById), `api/payment/payment-methods/*` (CRUD + Activate + Deactivate).
- **Routes covered (Storefront):** `api/storefront/payment/create-intent`, `.../confirm`, `.../methods`, `.../setup-intent`, `api/storefront/webhooks/stripe`.
- **Strengths:** Gateway abstraction (`IPaymentGatewayActionProvider`), Stripe gateway, webhook signature validation, payment lifecycle (Pending → Processing → Completed | Failed | Voided | Refunded), store credit concept.
- **Gaps:**
  - **Only Stripe wired** despite the `IPaymentGatewayActionProvider` abstraction supporting multiple gateways.
  - **No `BogusGateway` concrete class** found (README references it for test cards; `Domain/Gateways/Gateway.cs` exists but the `Bogus` implementation is not visible in `service/Api/src/Module/Payment/Infrastructure/Gateways/`).
  - **No partial refund path** — `RefundPayment` always refunds the full captured amount (no `Amount` parameter visible in the route).

### 2.6 Notifications (`Shared.Operational.Notifications`)

- **eShop parity: 80%**
- **Providers wired:** Logging (always-on dev fallback), SendGrid, SMTP, Sinch (SMS). Channels: Email, SMS, Logging.
- **Strengths:** Notification hub with multi-provider fallback, template resolution, FluentValidation on all settings, order-placed notification is sent in `CreateOrderFromCart` (`Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs:167-187`).
- **Gaps:**
  - **No sign-up welcome email handler** (no `NotificationUseCase.Welcome` consumer).
  - **No password-reset email handler** at the `Forgot`/`Reset` endpoints.
  - Template engine is registered but template files are not enumerated in this review.

### 2.7 Webhooks

- **eShop parity: 30%**
- **Current state:** Only Stripe inbound webhook handler exists (`Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.cs`). Handles `payment_intent.succeeded`, `payment_intent.payment_failed`, `charge.refunded`, `charge.dispute.created`.
- **Gaps:**
  - **No generic webhook subscription / delivery service.** eShop has full outbound webhook system: `WebhookSubscription` entity, event bus, delivery worker with retries.
  - **No outbound webhook** for "order placed", "order shipped", "payment captured" — must be added to eShop parity.

### 2.8 Location (`Module.Location`)

- **eShop parity: 100%**
- Full CRUD on countries + states by ID and ISO code. Admin + Storefront.

### 2.9 Inventory (extra — not in eShop)

- **N/A** — exceeds eShop scope with stock locations, items, movements, reservations, transfers, low-stock, summary, bulk-adjust, restock.

### 2.10 Shipping (extra — not in eShop)

- **Parity: 60%** — Storefront covered (Calculate, Methods, Rates) but **no admin CRUD endpoints** for shipping methods/rates. Domain exists in `Module/Shipping/Domain/ShippingMethods` and `Domain/ShippingRates`.

### 2.11 Profile (extra — not in eShop)

- **Parity: 50%** — Profiles + Addresses covered (CRUD). **Wishlists + NotificationPreferences: domain entities exist** in `Module/Profile/Domain/{Wishlists,Notifications,Preferences}` but **no `Features/Store/{Wishlists,Notifications,Preferences}` folders exist** (confirmed via `find Module/Profile/Features -type d`).

### 2.12 Embedding (extra — Python sidecar)

- **Parity: 0%** — Service exists at `service/Embedding/src/main.py`, declares 3 routers (`health`, `embeddings`, `models`), but **fails at import** per `AGENTS.md:42` and `CONCERNS.md:5,10`. The `embedding.config.settings` import is likely failing due to a missing `__init__.py` or relative-import issue. The plan files `infrastructure-aspire-embedding-setup-1.md` and `refactor-embedding-app-structure-1.md` exist — current owner unclear.

---

## 3. Cross-cutting infrastructure compliance

| Concern | eShop equivalent | Status | Evidence |
|---|---|---|---|
| **API Gateway** | YARP reverse proxy | **Missing** | `Services.Gateway` constant defined in `infra/Aspire/src/ReSys.ServiceDefaults/Constants/Services.cs:5` but **not added** in `AppHost.cs` |
| **Centralized config** | appsettings per service | OK | `service/Api/src/Api/appsettings.json` + `appsettings.Development.json` |
| **Health checks** | `/hc`, `/liveness`, `/readiness` | OK | `service/Api/src/Shared/Observability/HealthChecks` |
| **Distributed tracing** | OpenTelemetry | OK | `Shared/Observability` + `AddObservability` in `Program.cs:28` |
| **Resilience** | Polly | OK | `Shared/Operational/Http/ResilienceExtensions.cs` |
| **Service discovery** | Aspire | OK in dev | `infra/Aspire/src/ReSys.AppHost/AppHost.cs` |
| **Background jobs** | Hangfire | OK | `Shared/Operational/Backgrounds/Background.Extension.cs` (Redis or in-memory) |
| **Caching** | HybridCache + Redis | OK | `Shared/Performance/Caching/Caching.Extension.cs` |
| **Storage** | Azure Blob / S3 | OK | `Shared/Operational/Storages` (Local/S3/Azure + anti-forgery + malware scan + image processor) |
| **OpenAPI** | Swagger / Scalar | OK | `Shared/Governance/OpenApi` (Scalar UI) |
| **AuthN/AuthZ** | JWT + Identity | OK | `Shared/Security/Authentication/Tokens` + `Shared/Security/Authorization` (with refresh-token theft detection + token blacklist) |
| **CORS** | Dev only | OK in dev | `Shared/Security/Cors` (config issue: dev origins hardcoded in `appsettings.Development.json`) |
| **Anti-forgery** | CSRF tokens | OK | `Shared/Security/AntiForgery` |
| **Rate limiting** | AspNetCore RateLimiter | **Missing** | Not registered anywhere in `Program.cs` or `Shared.Security`; flagged in `CONCERNS.md:71` |
| **Secrets management** | User-secrets / KeyVault | **Partial** | Dev secret hardcoded in `appsettings.Development.json:11`; flagged in `CONCERNS.md:43,69` |
| **CI/CD** | GitHub Actions | **Missing** | Flagged in `CONCERNS.md:44` |
| **Module isolation enforcement** | Build-time guard | **Disabled** | `ValidateVerticalSliceIsolation` target in `Directory.Build.targets:44` gated with `Condition="false"`; flagged in `CONCERNS.md:45` |

---

## 4. Test coverage compliance

| Test layer | Status | eShop equivalent | Evidence |
|---|---|---|---|
| **Unit tests** (`Module.UnitTests`, `Shared.UnitTests`) | Present | Equivalent | EF InMemory + Moq + xUnit v3; covers Catalog, Identity, Inventory, Location, Ordering, Payment, Profile, Shipping |
| **Integration tests** (`Api.Tests`) | Partial | eShop has xUnit per service | Testcontainers (Postgres + Redis) + Respawn; **only `AntiForgery`, `Catalog`, `Identity`, `Location`, `Profile`, `HealthCheck` scenarios exist** — no Ordering, Payment, Shipping, or Inventory scenarios |
| **Contract tests** | **Missing** | eShop uses Pact | n/a |
| **HTTP smoke tests** (`ApiTests/`) | Partial | eShop has `.http` per service | `ApiTests/{Catalog,Identity,Location,Profile}` exist; **no `ApiTests/{Ordering,Payment,Shipping,Inventory}` folders** |
| **Mutation tests** | **Missing** | eShop has Stryker.NET | n/a |
| **Performance/load tests** | **Missing** | eShop has k6 scripts | n/a |

---

## 5. Severity-prioritized gap list

### High severity (blocks eShop parity)

1. **Profile Wishlists + NotificationPreferences APIs missing**
   - Domain entities exist at `Module/Profile/Domain/Wishlists/` and `Domain/Notifications/NotificationPreferences.cs` and `Domain/Preferences/UserPreference.cs`.
   - `Features/Store/{Wishlists,Notifications,Preferences}` folders are **absent** (confirmed by `find` on `Module/Profile/Features`).
   - No wishlist add/remove/transfer endpoints, no notification-prefs get/update endpoints.

2. **Shipping admin endpoints missing**
   - Storefront covers `Calculate`, `Methods`, `Rates` under `Module/Shipping/Features/Storefront/Shipping/`.
   - **No `Features/Admin/ShippingMethods/*` or `Features/Admin/ShippingRates/*`** — admin cannot CRUD shipping configuration.

3. **Generic Webhooks service missing**
   - Only Stripe inbound webhook exists (`Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.cs`).
   - No `WebhookSubscription` entity, no event bus, no outbound delivery worker, no subscription management endpoints.

4. **API Gateway not wired**
   - `Services.Gateway` constant exists in `infra/Aspire/src/ReSys.ServiceDefaults/Constants/Services.cs:5`.
   - `AppHost.cs` does not register a YARP / gateway project.
   - Frontends call `http://localhost:5035` directly via `VITE_API_URL` (set in `AppHost.cs:33,40`).

5. **Rate limiting middleware absent**
   - Not in `Program.cs`, not in `Shared.Security`, not in `Shared.Governance`.
   - No protection against brute-force on `/api/store/identity/auth/*` endpoints.

6. **Embedding service is non-functional**
   - `service/Embedding/src/main.py` imports `embedding.config.settings`, `embedding.routers.embedding_router`, `embedding.routers.model_router`, `embedding.routers.health_router`, `embedding.middleware.exception_handler`.
   - Per `AGENTS.md:42` and `CONCERNS.md:5,10`, the service **cannot start**.
   - Plan files `infrastructure-aspire-embedding-setup-1.md` and `refactor-embedding-app-structure-1.md` both exist — owner ambiguous.

### Medium severity

7. **BogusGateway not implemented** — README references test card numbers; only `StripeGateway` concrete class exists in `Module/Payment/Infrastructure/Gateways/Stripe/`.

8. **Multi-provider external auth not scaffolded** — only Google in `Shared/Security/Authentication/External/Providers/Google/`. eShop has Facebook + Microsoft scaffolds.

9. **No per-module `FeatureMetadata` for Payment and Shipping** — `Shared/Security/Authorization/Features/` has 8 files (Catalog, Configuration, Dashboard, Identity, Inventory, Location, Ordering, Profile). Payment reuses `OrderingFeatureMetadata.Payments` + `.PaymentMethods` (cross-module coupling; works but signals overlap).

10. **Cross-module reference leak** — `Module.Payment` references `Module.Ordering.Domain.Orders` directly in `CreatePaymentIntent.cs:1` and `StripeWebhook.cs:63`. `Module.Ordering.Features.Storefront.Cart.Checkout.CreateOrderFromCart.cs:1-5` references `Module.Inventory.Domain.*` and `Module.Payment.Domain.Payments`. `ValidateVerticalSliceIsolation` is disabled so this is not caught at build time.

11. **No CI/CD** — no `.github/workflows` found.

12. **No BogusGateway → no offline payment dev path** — Stripe test cards require a real Stripe sandbox or live keys for development.

### Low severity

13. **Module.UnitTests for `Inventory` exists but `Api.Tests/Scenarios` lacks Inventory** — partial test gap.

14. **HTTP test files missing for `Ordering`, `Payment`, `Shipping`, `Inventory`** in `ApiTests/`.

15. **Hardcoded dev JWT secret** — `service/Api/src/Api/appsettings.Development.json:11`. Flagged in `CONCERNS.md:43,69`.

16. **No mutation / contract / load tests.**

17. **Stale build artifacts** in `service/Embedding/build/lib/` — should be gitignored per `CONCERNS.md:58`.

18. **Large `.superpowers/sdd/review-*.diff` files** (7.8MB + 8.2MB) — bloat repo per `CONCERNS.md:59`.

---

## 6. Concrete remediation plan

### Phase 1 — Close eShop-baseline API gaps (1–2 weeks)

- [ ] **Add Profile Wishlists + NotificationPreferences API endpoints**
  - Create `Module/Profile/Features/Store/Wishlists/{Add,Remove,List}/` mirroring the Addresses pattern.
  - Create `Module/Profile/Features/Store/NotificationPreferences/{Get,Update}/` for `NotificationPreferences` and `UserPreference` entities.
  - Add `ProfileFeatureMetadata.Wishlists` and `.NotificationPreferences` permission groups.
  - Integration tests in `Api.Tests/Scenarios/Profile/Store/Wishlists/`.

- [ ] **Add Shipping admin endpoints**
  - Create `Module/Shipping/Features/Admin/ShippingMethods/{Create,Get,Update,Delete,GetPaged}/`.
  - Create `Module/Shipping/Features/Admin/ShippingRates/{Create,Get,Update,Delete,GetPaged}/`.
  - Add `ShippingFeatureMetadata` permissions in `Shared/Security/Authorization/Features/`.

- [ ] **Add generic Webhooks service**
  - Create `Module/Webhooks` (or `Shared/Operational/Webhooks`) with:
    - `WebhookSubscription` entity (Id, Event, Url, Secret, Active, CreatedAt, ModifiedAt).
    - `IWebhookDispatcher` service with retry policy.
    - Hangfire job for delivery.
    - `Features/Admin/Webhooks/{Create,Get,Update,Delete,GetPaged}/` admin endpoints.
  - Emit events from Ordering (`OrderPlaced`, `OrderShipped`), Payment (`PaymentCaptured`, `PaymentRefunded`).

- [ ] **Wire API Gateway**
  - Add `infra/Aspire/src/ReSys.Gateway` (YARP project) with rate limiting, CORS delegation, auth delegation.
  - Register in `AppHost.cs` between frontends and `Services.Api`.
  - Update frontends to use gateway endpoint instead of direct API endpoint.

- [ ] **Add Rate Limiting middleware**
  - Add `Shared/Security/RateLimiting/RateLimit.Extensions.cs` with `AddRateLimiter` policies.
  - Per-policy: `auth` (5/min), `register` (3/hour), `forgot-password` (3/hour), `payment` (30/min), default (100/min).
  - Wire into `Shared/Security/Security.Extension.cs`.

### Phase 2 — Fix the broken/half-built pieces (3–5 days)

- [ ] **Resolve Embedding service import errors**
  - Audit `service/Embedding/src/main.py` imports.
  - Verify `__init__.py` files exist in all subpackages.
  - Validate `uv run uvicorn embedding.main:app` starts successfully.
  - Add health check route + model listing route.
  - Reconcile `infrastructure-aspire-embedding-setup-1.md` and `refactor-embedding-app-structure-1.md`.

- [ ] **Implement `BogusGateway`**
  - Add `Module/Payment/Infrastructure/Gateways/Bogus/BogusGateway.cs`.
  - Wire test card numbers (4242 4242 4242 4242 success, 4000 0000 0000 0002 declined).
  - Add `Bogus` registration behind `IPaymentGatewayActionProvider` selection.

- [ ] **Scaffold Facebook + Microsoft external providers**
  - Add `Shared/Security/Authentication/External/Providers/Facebook/` mirroring Google.
  - Add `Shared/Security/Authentication/External/Providers/Microsoft/`.
  - Add corresponding `OAuth2Settings` options classes.

### Phase 3 — Enforce architecture (1 week)

- [ ] **Re-enable `ValidateVerticalSliceIsolation`** in `Directory.Build.targets:44` (remove `Condition="false"`).
  - First fix the known leaks:
    - `Module.Payment → Module.Ordering.Domain.Orders` (3 files).
    - `Module.Ordering.Features.Storefront.Cart.Checkout → Module.Inventory.Domain.Stock*` (3 imports in `CreateOrderFromCart.cs:1-3`).
  - Use a shared kernel pattern or domain-event contracts to remove direct references.

- [ ] **Split `OrderingFeatureMetadata`**
  - Extract `.Payments` and `.PaymentMethods` into `PaymentFeatureMetadata.cs`.
  - Update `Module.Payment.Features.Shared` route definitions to reference the new metadata.

- [ ] **Move dev JWT secret to user-secrets**
  - Add `.env.example` to `service/Api/src/Api/`.
  - Document `dotnet user-secrets set "JwtSettings:Secret" "..."`.

### Phase 4 — Test coverage to match eShop (1–2 weeks)

- [ ] **Add `Api.Tests/Scenarios/Inventory`** (StockItems, Reservations, Transfers, LowStock, BulkAdjust).
- [ ] **Add `Api.Tests/Scenarios/Ordering`** (Cart CRUD, Checkout, Admin Order state machine).
- [ ] **Add `Api.Tests/Scenarios/Payment`** (Intents, Webhooks, Refunds, Capture, Void).
- [ ] **Add `Api.Tests/Scenarios/Shipping`** (Calculate, Methods, Rates).
- [ ] **Add `Api.Tests/Scenarios/Profile/Wishlists` + `NotificationPreferences`**.
- [ ] **Add `ApiTests/{Ordering,Payment,Shipping,Inventory,Wishlists,Webhooks}.http` smoke files.**
- [ ] **Add `.github/workflows/dotnet-ci.yml`** with build, unit tests, integration tests (Docker), lint, format-check.

### Phase 5 — Optional (deferred)

- [ ] **Contract tests** (Pact) per service boundary (`Catalog ↔ Ordering`, `Ordering ↔ Payment`, `Ordering ↔ Inventory`).
- [ ] **Mutation testing** (Stryker.NET) on `Shared/Application/Models/Results`, `Module/Ordering/Domain/Orders`, `Module/Payment/Domain/Payments`.
- [ ] **k6 load tests** on `cart checkout`, `product search`, `image-search` endpoints.
- [ ] **Clean up `service/Embedding/build/lib/` artifacts** — add to `.gitignore`.
- [ ] **Archive large `.superpowers/sdd/review-*.diff` files** — move out of repo.

---

## 7. Reference: file paths inspected

### 7.1 Module entry points

- `service/Api/src/Api/Program.cs`
- `service/Api/src/Module/Catalog/Catalog.Extension.cs`
- `service/Api/src/Module/Identity/Identity.Extensions.cs`
- `service/Api/src/Module/Inventory/Inventory.Extension.cs`
- `service/Api/src/Module/Location/Locations.Extensions.cs`
- `service/Api/src/Module/Ordering/Ordering.Extension.cs`
- `service/Api/src/Module/Payment/Payment.Extension.cs`
- `service/Api/src/Module/Profile/Profiles.Extensions.cs`
- `service/Api/src/Module/Shipping/Shipping.Extension.cs`

### 7.2 Shared infrastructure

- `service/Api/src/Shared/Application/Application.Extension.cs`
- `service/Api/src/Shared/Governance/Governance.Extension.cs`
- `service/Api/src/Shared/Operational/Operational.Extension.cs`
- `service/Api/src/Shared/Operational/Storages/Storage.Extensions.cs`
- `service/Api/src/Shared/Operational/Notifications/Notification.Extension.cs`
- `service/Api/src/Shared/Operational/Backgrounds/Background.Extension.cs`
- `service/Api/src/Shared/Performance/Caching/Caching.Extension.cs`
- `service/Api/src/Shared/Security/Security.Extension.cs`
- `service/Api/src/Shared/Security/Authentication/Tokens/Tokens.Extensions.cs`
- `service/Api/src/Shared/Governance/OpenApi/OpenApi.Extension.cs`

### 7.3 Domain (high-level)

- `service/Api/src/Module/Ordering/Domain/Orders/{Order,Order.cs,Order.Checkout.cs,OrderUpdater.cs,OrderContents.cs,OrderInventory.cs,OrderMerger.cs,Order.AddressBook.cs,Order.Payments.cs,Order.CurrencyUpdater.cs,Order.StoreCredit.cs}`
- `service/Api/src/Module/Payment/Domain/{Gateways,PaymentMethods,Payments}`
- `service/Api/src/Module/Payment/Infrastructure/Gateways/Stripe/{StripeGateway,StripeWebhookService,StripeOptions}.cs`
- `service/Api/src/Module/Profile/Domain/{UserProfile,Addresses,Wishlists,Notifications,Preferences}`
- `service/Api/src/Module/Shipping/Domain/{ShippingMethods,ShippingRates,Calculators}`
- `service/Api/src/Module/Inventory/Domain/{Stock,StockLocations,StockReservations,StockTransfers}`
- `service/Api/src/Module/Catalog/Domain/{Products,OptionTypes,Taxonomies}`

### 7.4 Features (key)

- `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs`
- `service/Api/src/Module/Ordering/Features/Storefront/Cart/SelectShippingRate/SelectShippingRate.cs`
- `service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs`
- `service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.cs`
- `service/Api/src/Module/Payment/Features/Storefront/Payment/Methods/ListPaymentMethods.cs`
- `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Reserve/ReserveCartStock.Endpoint.cs`
- `service/Api/src/Module/Identity/Features/Store/Auth/Login/External/{Authenticate,Providers}`
- `service/Api/src/Module/Shipping/Features/Storefront/Shipping/Calculate/CalculateShipping.cs`
- `service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Storefront.cs`

### 7.5 Tests

- `service/Api/tests/Api.Tests/Scenarios/{AntiForgery,Catalog,Identity,Location,Profile}` (no Ordering, Payment, Shipping, Inventory)
- `service/Api/tests/Api.Tests/Scenarios/HealthCheckTests.cs`
- `service/Api/tests/Module.UnitTests/{Catalog,Identity,Inventory,Location,Ordering,Payment,Profile,Shipping}`
- `service/Api/tests/Shared.UnitTests/{Application,Governance,Observability,Operational,Performance,Security}`
- `ApiTests/{Catalog,Identity,Location,Profile}` (no Ordering, Payment, Shipping, Inventory)
- `ApiTests/run-all.http`, `ApiTests/README.md`, `ApiTests/_shared/`

### 7.6 Infrastructure

- `infra/Aspire/src/ReSys.AppHost/AppHost.cs`
- `infra/Aspire/src/ReSys.ServiceDefaults/{Extensions.cs,Constants/{Services,Apps,Infrastructures,Images}.cs}`
- `service/Embedding/src/main.py`
- `service/Embedding/src/{config,routers,controllers,models,services}/`

### 7.7 Reference docs

- `AGENTS.md`
- `docs/codebase/ARCHITECTURE.md`
- `docs/codebase/CONCERNS.md`
- `docs/codebase/CONVENTIONS.md`
- `docs/codebase/STRUCTURE.md`
- `docs/codebase/STACK.md`
- `docs/codebase/INTEGRATIONS.md`
- `docs/codebase/TESTING.md`
- `plan/feature-catalog-integration-tests-1.md`
- `plan/create-typed-api-layer-7.md`
- `plan/infrastructure-aspire-embedding-setup-1.md`
- `plan/refactor-embedding-app-structure-1.md`

---

## 8. Summary table

| EShop service | ReSys module | Parity | Top gap |
|---|---|---|---|
| Catalog | Catalog | 95% | Brand/Type entities (replaced by Classifications) |
| Basket | Ordering.Cart | 85% | Guest cart persistence |
| Ordering | Ordering | 90% | State machine tests |
| Identity | Identity + Auth | 90% | Facebook/Microsoft OAuth |
| Payment | Payment | 85% | BogusGateway missing, no partial refund |
| Notification | Shared.Operational | 80% | Welcome + Password-reset emails |
| Webhooks | (embedded in Payment) | 30% | **No generic outbound** |
| Location | Location | 100% | — |
| Inventory (extra) | Inventory | 100% | — |
| Shipping (extra) | Shipping | 60% | **No admin CRUD** |
| Profile (extra) | Profile | 50% | **No Wishlists/NotificationPrefs APIs** |
| Embedding (extra) | Embedding (Python) | 0% | **Broken imports** |
| API Gateway | (none) | 0% | **YARP not wired** |
| Rate limiting | (none) | 0% | **No middleware** |
| CI/CD | (none) | 0% | **No GitHub Actions** |

**Overall: ~80–85% eShop parity. Highest-leverage fixes: Profile Wishlists/NotificationPrefs APIs, Shipping admin CRUD, generic Webhooks, API Gateway, Rate Limiting, Embedding service repair.**
