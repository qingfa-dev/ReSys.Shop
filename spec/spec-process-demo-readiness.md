---
title: Fashion Shop MVP Demo Readiness — API Service Fixes
version: 1.0
date_created: 2026-07-12
last_updated: 2026-07-12
owner: ReSys.Shop Platform Team
tags:
  - process
  - architecture
  - demo-readiness
  - payment
  - security
  - ordering
---

# Introduction

This specification defines the **demo-blocking and demo-visible defects** identified during pre-demo review of the ReSys.Shop API service and the corrective work required to ship the Fashion Shop MVP demo. It is a fix-oriented specification: every requirement traces to a known broken or fragile behavior in the current `dev` branch. The scope is intentionally limited to issues that either (a) will fail during a live demo or (b) will silently degrade the demo experience. Out-of-scope refactors (e.g. full rule-2 module decoupling) are recorded as future work and explicitly excluded from this spec.

The target environment is the Aspire-orchestrated local demo: PostgreSQL 17 with pgvector, Redis 7, the Python embedding sidecar, and the Admin/Storefront SPAs running on `localhost:5173/4173/3000`.

## 1. Purpose & Scope

### 1.1 Purpose

Make the API service demonstrable end-to-end (browse → add to cart → checkout → payment confirmation → order placed) without silent failures, race conditions, or hardcoded secrets.

### 1.2 In-Scope

- Payment module: real Stripe webhook wiring, DI container anti-pattern removal
- Catalog module: stock-based availability computation
- Ordering module: atomic stock deduction during checkout
- Security: dev secret handling, security header binding
- Configuration: fail-fast (`ValidateOnStart`) on all security and operational settings
- Module registration hygiene: Webhooks module resolution, payment gateway defaults
- Order event publishing: stop silent drop

### 1.3 Out-of-Scope (Tracked Elsewhere)

- Full rule-2 (modules never reference each other) decoupling of Ordering/Identity/Payment — see `plan/` once created
- File/class naming consistency for `Identity.Extensions.cs`, `Locations.Extensions.cs`, `Profiles.Extensions.cs`
- Greenfield replacement of `NullOrderEventPublisher` with an in-process channel
- Re-enabling `ValidateVerticalSliceIsolation` build target (AGENTS.md known issue)
- Re-architecture of `CreateOrderFromCart` into a workflow of sub-commands

### 1.4 Audience

Engineers shipping the demo, the reviewer who gates the demo build, and the platform team that will own post-demo cleanup.

### 1.5 Assumptions

- The Python embedding sidecar (`service/Embedding/`) is running in the demo
- The Aspire AppHost brings up PostgreSQL + Redis containers with default credentials
- The Bogus payment gateway remains the default for the demo (no Stripe credentials)
- `appsettings.Development.json` is loaded only when `ASPNETCORE_ENVIRONMENT=Development`
- The `dev` branch is the demo branch; no production deployment is in scope

## 2. Definitions

| Term | Definition |
|------|------------|
| **Demo** | A live walk-through of the Fashion Shop MVP for stakeholders, run against the Aspire local stack |
| **MVP** | Minimum Viable Product — the smallest set of features required to demonstrate browse → purchase |
| **Module** | A bounded context in the C# solution (Catalog, Identity, Inventory, Location, Ordering, Payment, Profile, Shipping) living in a single `Module` assembly |
| **Rule 2** | AGENTS.md non-negotiable rule: "Modules never reference each other — all 8 business modules live in one `Module` assembly but must not cross-reference. Communication via MediatR `ISender` only." |
| **Result** | The `Result<T>` / `Result` discriminated-union return type used by all handlers in lieu of exceptions |
| **Bogus gateway** | A fake payment gateway (`Module.Payment.Services.Provider.Bogus`) that simulates success/failure without external calls; default for the demo |
| **Stripe webhook** | An HTTP POST from Stripe to `/webhooks/stripe` containing a payment event payload signed with a shared secret |
| **ValidateOnStart** | ASP.NET Core options pattern flag that runs configuration validation at host build time rather than first access |
| **Webhook** | In this codebase, distinct from the empty `Module.Webhooks` directory; refers to gateway webhooks (Stripe, Bogus) handled in `Module.Payment` |
| **Idempotent** | An operation that produces the same result whether executed once or many times — required for webhook handlers |
| **Optimistic concurrency** | A concurrency control pattern that uses a version token or WHERE-clause guard to detect conflicting writes |
| **Serializable isolation** | The strictest PostgreSQL transaction isolation level; used to prevent oversell in stock reservation |

## 3. Requirements, Constraints & Guidelines

### 3.1 Payment Webhook Wiring (Critical)

- **REQ-PAY-001**: The dependency injection container MUST resolve `IStripeWebhookService` to the handler that actually processes Stripe events (`Module.Payment.Features.Storefront.Payment.Webhooks.StripeWebhook.CommandHandler`), not the stub at `Module/Payment/Services/Webhook/StripeWebhookService.cs:30-38`.
- **REQ-PAY-002**: The stub `StripeWebhookHandler.HandleAsync` MUST be removed or reduced to a thin adapter that delegates to the real handler.
- **REQ-PAY-003**: All payment webhook handlers MUST be idempotent — replaying the same event MUST NOT double-credit or double-reserve stock.
- **CON-PAY-001**: The Bogus gateway remains the default for the demo. The Stripe webhook path MUST still compile and pass unit tests even when Stripe is disabled.
- **SEC-PAY-001**: The Stripe webhook signature validation MUST run before any payload parsing. A signature failure MUST return HTTP 400, not 200.

### 3.2 Payment DI Anti-Pattern (Critical)

- **REQ-PAY-010**: The `EncryptedDictionaryConverter.Configure(...)` call in `Module/Payment/Payment.Extension.cs:50-54` MUST NOT call `builder.Services.BuildServiceProvider()`. Building a second root container breaks scoped lifetime semantics and silently returns stale or empty service instances.
- **REQ-PAY-011**: `EncryptedDictionaryConverter` MUST resolve `IEncryptionService` lazily through an `IServiceProvider` accessor obtained from the running host, OR `IEncryptionService` MUST be registered before the converter is configured.
- **PAT-PAY-001**: Configuration of static helpers that depend on DI MUST happen at the end of the host pipeline (after `app.Build()`), not during module registration.

### 3.3 Catalog Availability Computation (Critical)

- **REQ-CAT-001**: `Module.Catalog.Features.Storefront.Products.Get.Availability.GetAvailability.QueryHandler` MUST compute per-variant availability from `StockItem.CountOnHand` minus active `StockReservation` quantities, not from `Variant.Prices`.
- **REQ-CAT-002**: The availability matrix response MUST return one of: `in_stock`, `low_stock` (count on hand < threshold), `out_of_stock`, or `backorderable` (when `StockItem.Backorderable == true`).
- **AC-CAT-001**: A priced variant with zero stock on hand MUST return `Status = "out_of_stock"`, not `"in_stock"`.
- **AC-CAT-002**: A variant with no `StockItem` row MUST return `Status = "out_of_stock"`.
- **PAT-CAT-001**: The availability calculation in `GetAvailability` MUST share its core logic with `GetStockAvailability` (`Module/Inventory/Features/Storefront/StockAvailability/Check/GetStockAvailability.cs:43-58`) by extracting a `StockAvailabilityCalculator` service in `Module/Inventory`.

### 3.4 Ordering Stock Deduction (Critical)

- **REQ-ORD-001**: `CreateOrderFromCart.CommandHandler` MUST deduct stock atomically — concurrent checkouts MUST NOT oversell.
- **REQ-ORD-002**: The stock deduction loop at `Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs:107-145` MUST use either:
  - (a) `ExecuteUpdateAsync` with a `WHERE CountOnHand >= take` guard, OR
  - (b) `Serializable` transaction isolation + `SELECT … FOR UPDATE` on the `StockItem` rows, OR
  - (c) Optimistic concurrency token on `StockItem`
- **REQ-ORD-003**: The handler MUST wrap the stock mutation, reservation insert, and movement insert in a single transaction. On any failure inside the loop, all prior mutations in the same checkout MUST roll back.
- **AC-ORD-001**: Given 1 unit of stock on hand and 2 concurrent checkout requests for 1 unit each, only 1 request MUST succeed with HTTP 201; the other MUST return `Result.Failure(InsufficientStock)`.
- **AC-ORD-002**: A `DbUpdateException` thrown after line 121 of `CreateOrderFromCart.cs` MUST leave the cart in `OrderStatus.Draft` with no stock movement, no reservation, and no order number.
- **SEC-ORD-001**: The `OrderNumber` generated by `GenerateOrderNumber` MUST be unique across all orders for a given UTC day. With current `Guid.NewGuid()[..6]` (16M combinations) and a demo load of 10K orders/day, collision probability is non-trivial. Use at least 8 hex characters from a Guid v7 (time-ordered) or a database sequence.

### 3.5 Security Hardening (Critical)

- **SEC-AUTH-001**: `appsettings.Development.json` MUST NOT contain a hardcoded `Jwt:Secret` value. The secret MUST come from a user-secrets store, environment variable, or Aspire parameter.
- **SEC-AUTH-002**: `appsettings.Development.json` MUST NOT contain a hardcoded `GatewayProviders.SettingsEncryptionKey`. The encryption key MUST be generated per environment.
- **SEC-AUTH-003**: The application MUST refuse to start in `Production` environment if `Jwt:Secret` matches the literal string `"dev-jwt-secret-min-32-chars-for-hs256-algorithm!"`.
- **SEC-AUTH-004**: The application MUST refuse to start in any environment if `GatewayProviders.SettingsEncryptionKey` is empty AND any gateway is enabled.
- **REQ-AUTH-001**: `ExternalAuthenticate.CommandHandler.CreateUserProfileAsync` MUST NOT swallow exceptions silently. If `mediator.Send(CreateProfile.Command)` returns `IsFailure` OR throws, the outer handler MUST return `Result.Failure(ProfileCreationFailed)` and revoke the issued tokens.
- **PAT-AUTH-001**: All token issuance and revocation operations MUST be atomic — if any step fails, no tokens reach the client.

### 3.6 Configuration Fail-Fast (High)

- **REQ-CFG-001**: Every `AddOptions<T>().BindConfiguration(...).ValidateFluentValidation()` call in `service/Api/src/Shared/` MUST be followed by `.ValidateOnStart()`.
- **REQ-CFG-002**: Specifically, the following settings MUST have `ValidateOnStart`:
  - `AntiForgerySetting` (`Shared/Security/AntiForgery/AntiForgery.Extensions.cs:18-20`)
  - `NotificationSetting`, `EmailChannelSetting`, `SmsChannelSetting`, `SendGridProviderSetting`, `SmtpProviderSetting`, `SinchProviderSetting` (`Shared/Operational/Notifications/Notification.Extension.cs:74-106`)
  - `CachingSetting` (`Shared/Performance/Caching/Caching.Extension.cs:40-42`)
  - `BackgroundJobSetting` (`Shared/Operational/Backgrounds/Background.Extension.cs:44-46`)
  - `GuestSessionSetting` (`Shared/Security/Authentication/Guest/GuestSession.Extensions.cs:17-19`)
- **REQ-CFG-003**: `Shared/Security/Headers/SecurityHeaders.Extensions.cs:12` MUST bind `SecurityHeadersSetting` from configuration and add a validator. Currently no `BindConfiguration` is called, so the middleware silently uses defaults.
- **AC-CFG-001**: Given a misconfigured `Notification:Smtp:Host = ""` in `appsettings.json`, the host MUST fail to start with a clear validation error message.

### 3.7 Module Registration Hygiene (Medium)

- **REQ-MOD-001**: The `Module/Webhooks/` directory tree MUST either be removed or populated with code. An empty module is misleading in the repo map and the `AGENTS.md` file references it.
- **REQ-MOD-002**: `OrderingExtension.AddOrderingModule` MUST NOT register `NullOrderEventPublisher` in any environment. Replace with an in-process channel (`System.Threading.Channels.Channel<OrderPlacedEvent>`) in `Development`, and a no-op + warning log in `Production` until the real publisher lands.
- **GUD-MOD-001**: Module extension file names SHOULD be singular (`<Module>.Extension.cs`) matching the namespace. The plural `Identity.Extensions.cs`, `Locations.Extensions.cs`, `Profiles.Extensions.cs` SHOULD be renamed for consistency. (Optional for this spec; tracked in a follow-up.)
- **REQ-MOD-003**: The seeders in `Ordering/Persistence/Seeders/Order.Seeder.cs` and `Inventory/Persistence/Seeders/InventoryStockItem.Seeder.cs` MUST be moved to a flat `seed/dev/` directory because they reach across module boundaries — they cannot be `Module.Add*Module()`-registered per rule 2.
- **CON-MOD-001**: The demo uses `appsettings.Development.json`. `Program.cs:60` (`runSeeders = !app.Environment.IsProduction()`) will run seeders in `Staging`. The demo often runs in `Staging`; gate seeders with `IsDevelopment || (IsStaging && SeedFlag)`.

### 3.8 Host Boot Order (Medium)

- **REQ-HOST-001**: `Program.cs:61` (`await app.InitializeDatabaseAsync(...)`) MUST be moved to a `BackgroundService` or behind the liveness probe so that migration failures do not produce a half-started host that returns 200 on `/health/live` and then crashes.
- **AC-HOST-001**: Given a pending migration that fails on `Up()`, `/health/live` MUST return 503 with `database_initialization: failed` until the operator fixes the migration or rolls back.
- **PAT-HOST-001**: Use Aspire's built-in `WithHealthCheck` for the database initialization state, with a custom `IHealthCheck` that reports the init status.

## 4. Interfaces & Data Contracts

### 4.1 Modified: `IStripeWebhookService` Resolution

**Before** (`service/Api/src/Module/Payment/Payment.Extension.cs:76`):
```csharp
services.AddSingleton<IStripeWebhookService, StripeWebhookHandler>();
// StripeWebhookHandler is the stub at Services/Webhook/StripeWebhookService.cs
```

**After**:
```csharp
services.AddSingleton<IStripeWebhookService, StripeWebhookDispatcher>();
// StripeWebhookDispatcher is a new class in Features/Storefront/Payment/Webhooks
// that parses the event and dispatches PaymentIntentSucceeded/Failed,
// ChargeRefunded, DisputeCreated to the real CommandHandlers via ISender
```

### 4.2 Modified: `EncryptedDictionaryConverter` Configuration

**Before** (`service/Api/src/Module/Payment/Payment.Extension.cs:50-54`):
```csharp
EncryptedDictionaryConverter.Configure(() =>
{
    var sp = builder.Services.BuildServiceProvider(); // ANTI-PATTERN
    return sp.GetRequiredService<IEncryptionService>();
});
```

**After**:
```csharp
EncryptedDictionaryConverter.Configure(sp => sp.GetRequiredService<IEncryptionService>());
// The converter receives the request-scoped IServiceProvider at conversion time
// (e.g. when serializing PaymentRecord.Metadata)
```

### 4.3 Modified: `AvailabilityCell.Status` Computation

**Before** (`service/Api/src/Module/Catalog/Features/Storefront/Products/Get/Availability/GetAvailability.cs:86`):
```csharp
Status = firstPrice?.Amount > 0 ? "in_stock" : "unknown",
```

**After**:
```csharp
var (onHand, reserved) = await stockAvailabilityCalculator.GetTotalsAsync(v.Id, ct);
var available = onHand - reserved;
string status = available > LOW_STOCK_THRESHOLD ? "in_stock"
              : available > 0 ? "low_stock"
              : stockItem?.Backorderable == true ? "backorderable"
              : "out_of_stock";
```

### 4.4 New: `IStockAvailabilityCalculator` Service

```csharp
namespace Module.Inventory.Services;

public interface IStockAvailabilityCalculator
{
    Task<StockAvailability> GetForVariantAsync(Guid variantId, CancellationToken ct);
    Task<VariantStockSnapshot> GetTotalsAsync(Guid variantId, CancellationToken ct);
}

public sealed record StockAvailability(int CountOnHand, int Reserved, int Available, bool Backorderable);
public sealed record VariantStockSnapshot(int TotalOnHand, int TotalReserved, int TotalAvailable);
```

### 4.5 New: `IOrderEventPublisher` Implementation

**Before** (silently drops events):
```csharp
public sealed class NullOrderEventPublisher : IOrderEventPublisher
{
    public Task PublishAsync(OrderPlacedEvent evt, CancellationToken ct) => Task.CompletedTask;
}
```

**After** (dev: in-process channel; non-dev: still no-op but logs warning):
```csharp
public sealed class InProcessOrderEventPublisher : IOrderEventPublisher
{
    private readonly Channel<OrderPlacedEvent> _channel =
        Channel.CreateUnbounded<OrderPlacedEvent>(new() { SingleReader = true });
    public ChannelReader<OrderPlacedEvent> Reader => _channel.Reader;
    public Task PublishAsync(OrderPlacedEvent evt, CancellationToken ct) =>
        _channel.Writer.WriteAsync(evt, ct).AsTask();
}
```

### 4.6 New: `IDatabaseInitializationHealthCheck`

```csharp
namespace Shared.Operational.Persistence.Health;

public interface IDatabaseInitializationState
{
    bool IsComplete { get; }
    Exception? Failure { get; }
}

public sealed class DatabaseInitializationState : IDatabaseInitializationState { /* ... */ }
```

## 5. Acceptance Criteria

### 5.1 Payment

- **AC-PAY-001**: Given a Stripe `payment_intent.succeeded` webhook with valid signature, when the handler runs, then the corresponding `Order.PaymentState` MUST be set to `"paid"` and the `OrderStatus` MUST transition from `Draft` (via checkout) to `Placed`.
- **AC-PAY-002**: Given the same webhook delivered twice (replay), when the second handler runs, then the result MUST be idempotent — no double charge, no double stock deduction, no duplicate `StockMovement`.
- **AC-PAY-003**: Given a Stripe webhook with an invalid signature, when the handler runs, then the response MUST be HTTP 400 with `Stripe.WebhookSignature.Invalid` and no state change.
- **AC-PAY-004**: Given a malformed JSON payload, when the handler runs, then the response MUST be HTTP 400 with `Stripe.WebhookPayload.Malformed` AND a `logger.LogError` entry containing the original exception.
- **AC-PAY-005**: Given `appsettings.json` with `GatewayProviders:SettingsEncryptionKey = ""` and any gateway enabled, when the host starts, then it MUST fail with `OptionsValidationException` referencing `GatewayProvidersOptions.SettingsEncryptionKey`.

### 5.2 Catalog

- **AC-CAT-010**: Given a variant with `CountOnHand = 0` and `Backorderable = false`, when `GetAvailability` runs, then every `AvailabilityCell` for that variant MUST have `Status = "out_of_stock"`.
- **AC-CAT-011**: Given a variant with `CountOnHand = 3` and 1 active reservation, when `GetAvailability` runs, then the cell MUST have `Status = "in_stock"` (assuming `LOW_STOCK_THRESHOLD < 2`).
- **AC-CAT-012**: Given a variant with no `StockItem` rows, when `GetAvailability` runs, then the cell MUST have `Status = "out_of_stock"`.
- **AC-CAT-013**: Given a variant with `CountOnHand = 5` and 4 active reservations, when `GetAvailability` runs, then the cell MUST have `Status = "low_stock"`.

### 5.3 Ordering

- **AC-ORD-010**: Given 1 unit of stock and 2 concurrent `CreateOrderFromCart` requests, when both run in parallel, then exactly 1 MUST return `201 Created` and the other MUST return `Result.Failure(InsufficientStock)`.
- **AC-ORD-011**: Given a `DbUpdateException` thrown by `SaveChangesAsync` after partial stock mutation, when the transaction rolls back, then the cart MUST remain in `OrderStatus.Draft`, no `StockMovement` rows MUST exist, and no `StockReservation` rows MUST be tied to `OrderId`.
- **AC-ORD-012**: Given 10,000 demo orders created on the same UTC day, then the count of duplicate `Order.Number` values MUST be 0 (or within a documented tolerance for `Guid.NewGuid()[..8]`).

### 5.4 Security

- **AC-SEC-010**: Given `appsettings.Development.json` with the current hardcoded `Jwt:Secret`, when the host starts in `Production`, then it MUST throw `OptionsValidationException` referencing the dev secret literal.
- **AC-SEC-011**: Given a new external OAuth login where `mediator.Send(CreateProfile.Command)` throws `DbUpdateException`, when the handler runs, then the response MUST be `Result.Failure(ProfileCreationFailed)` and no JWT MUST be returned to the client.
- **AC-SEC-012**: Given `appsettings.json` with `SecurityHeaders` overrides for `Content-Security-Policy` and `Strict-Transport-Security`, when any response is returned, then the headers MUST reflect the configured values (verifying `BindConfiguration` is wired).

### 5.5 Configuration

- **AC-CFG-010**: Given `Notification:Smtp:Host = ""` in `appsettings.json`, when `dotnet run` starts, then the host MUST fail with `OptionsValidationException` listing `SmtpProviderSetting.Host` within 5 seconds.
- **AC-CFG-011**: Given `Caching:Distributed:Enabled = true` and `Caching:Distributed:RedisConfiguration = ""`, when the host starts, then the host MUST fail with `OptionsValidationException`.

### 5.6 Module Hygiene

- **AC-MOD-010**: Given an `OrderPlaced` event published in `Development`, when the host runs, then a consumer of `InProcessOrderEventPublisher.Reader` MUST receive the event within 100ms.
- **AC-MOD-011**: Given `ASPNETCORE_ENVIRONMENT=Production`, when the host runs, then `OrderingExtension.AddOrderingModule` MUST NOT register `NullOrderEventPublisher`; instead it MUST register a no-op publisher that logs a warning on first publish.

## 6. Test Automation Strategy

### 6.1 Test Levels

- **Unit tests** (`service/Api/tests/Module.UnitTests`): Pure handler logic with mocked `IApplicationDbContext`, `ICurrentUser`, `ISender`. Required for every changed handler.
- **Integration tests** (`service/Api/tests/Module.IntegrationTests`): Real PostgreSQL + Redis via Testcontainers. Required for:
  - AC-ORD-010 (concurrency)
  - AC-ORD-011 (rollback)
  - AC-PAY-002 (idempotency)
  - AC-CFG-010/011 (host boot failure)
- **End-to-end tests**: Optional for the demo — defer to a follow-up spec.

### 6.2 Frameworks

- **MSTest** (existing convention in this repo)
- **FluentAssertions** for assertion readability
- **Moq** or **NSubstitute** for mocking (whichever the repo already uses — check `Module.UnitTests` to confirm)
- **Testcontainers** for PostgreSQL/Redis in integration tests
- **Respawn** for database reset between integration tests

### 6.3 Required Test Cases

| AC | Test Path | Test Name |
|----|-----------|-----------|
| AC-PAY-002 | `Module.UnitTests/Payment/Webhooks/StripeWebhookDispatcherTests.cs` | `ProcessAsync_PaymentIntentSucceeded_PlayedTwice_IsIdempotent` |
| AC-PAY-004 | `Module.UnitTests/Payment/Webhooks/StripeWebhookDispatcherTests.cs` | `ProcessAsync_MalformedPayload_LogsError_Returns400` |
| AC-CAT-010..013 | `Module.UnitTests/Catalog/GetAvailabilityTests.cs` | `Status_OutOfStock`, `Status_LowStock`, etc. |
| AC-ORD-010 | `Module.IntegrationTests/Ordering/CheckoutConcurrencyTests.cs` | `Checkout_TwoConcurrentRequests_OneSucceedsOneFails` |
| AC-ORD-011 | `Module.IntegrationTests/Ordering/CheckoutRollbackTests.cs` | `Checkout_DbUpdateException_LeavesCartInDraft` |
| AC-ORD-012 | `Module.UnitTests/Ordering/OrderNumberGeneratorTests.cs` | `Generate_10000Times_NoCollisions` |
| AC-SEC-010 | `Module.UnitTests/Identity/JwtSecretValidatorTests.cs` | `Production_DevSecretLiteral_FailsValidation` |
| AC-SEC-011 | `Module.UnitTests/Identity/ExternalAuthenticateTests.cs` | `ProfileCreationThrows_NoTokenReturned` |
| AC-CFG-010 | `Module.IntegrationTests/Shared/NotificationOptionsTests.cs` | `EmptySmtpHost_OptionsValidationFails` |

### 6.4 Coverage Requirements

- Lines added or changed by this spec MUST maintain or improve the current per-module coverage baseline (see `docs/codebase/TESTING.md`).
- The 6 critical fixes (PAY-001, PAY-010, CAT-001, ORD-001, AUTH-003, CFG-001) MUST each have at least one integration test that exercises the failure path.

### 6.5 Performance Testing

Defer to post-demo. The concurrency test (AC-ORD-010) serves as a smoke test for the oversell bug.

## 7. Rationale & Context

### 7.1 Why These Fixes First

The six critical issues (PAY-001 webhook stub, PAY-010 BuildServiceProvider, CAT-001 stock-vs-price, ORD-001 missing atomic guard, AUTH-003 dev secret, CFG-001 fail-fast) were selected because each one either:
- Breaks the demo if hit (`CAT-001` will show "in stock" for everything)
- Silently corrupts demo data (`ORD-001` oversells)
- Silently disables a security control (`SEC-AUTH-001/002` ship dev secrets)
- Silently swallows errors that surface 30+ seconds into a flow (CFG-001)

### 7.2 Why `IConfiguration` in `AddToCart` Is Out of Scope

`Module/Ordering/Features/Storefront/Cart/AddItem/AddToCart.cs:20` injects `IConfiguration` directly into a handler. This is a service-locator anti-pattern, but switching to `IOptions<OrderingSettings>` requires a settings class that doesn't exist yet. The fix is mechanical but touches 3 modules' worth of settings registration; deferred to a follow-up.

### 7.3 Why `NullOrderEventPublisher` Is In Scope But Not Fixed

The event publisher registration is in scope (REQ-MOD-002) because the *silence* is the problem — a customer placing an order and getting an order confirmation email but no downstream effects (analytics, fulfillment, etc.) is a real demo issue. The fix introduces an in-process channel for dev; the production implementation is tracked elsewhere.

### 7.4 Why Rule-2 Violations Are Not In Scope

The Ordering module imports 5 other modules (Catalog, Inventory, Payment, Profile, Shipping). Decomposing `CreateOrderFromCart` into 4 sub-commands dispatched via `ISender` is a multi-day refactor that risks breaking the demo. Deferred to `plan/` post-demo.

### 7.5 Why `Webhooks/` Empty Module Is In Scope

The `Module/Webhooks/` directory exists with empty subfolders and is referenced in the AGENTS.md repo map. Demo reviewers will notice the mismatch. Either delete or wire; the cheapest fix is deletion with an updated AGENTS.md note.

## 8. Dependencies & External Integrations

### External Systems

- **EXT-001**: PostgreSQL 17 with pgvector — required for `StockItem`, `StockReservation`, `StockMovement` transactional semantics (Serializable isolation)
- **EXT-002**: Redis 7 — required for `HybridCache` and Hangfire
- **EXT-003**: Stripe API — only required for AC-PAY-001/002/003/004 in `Testing` environment; the demo uses the Bogus gateway

### Third-Party Services

- **SVC-001**: Stripe webhook endpoint — must be reachable from Stripe in `Testing` (use Stripe CLI `stripe listen --forward-to localhost:5000/webhooks/stripe` for local)
- **SVC-002**: SMTP server (MailHog on `localhost:1025` per `appsettings.Development.json:69`) — required for `OrderPlaced` email

### Infrastructure Dependencies

- **INF-001**: Aspire AppHost — must bring up Postgres + Redis containers; demo cannot run without it
- **INF-002**: Python embedding sidecar — required for `SearchByImage` (not in demo critical path)

### Data Dependencies

- **DAT-001**: Seeded catalog — `CatalogDemoSeeder` MUST seed at least 1 product per size/color combination to exercise the availability matrix

### Technology Platform Dependencies

- **PLT-001**: .NET 10 / C# preview — current target framework per `Directory.Build.props`; all async patterns use C# 13 features

### Compliance Dependencies

- **COM-001**: PCI-DSS — the Bogus gateway is acceptable for demo because no real card data is processed; production deployment is out of scope

## 9. Examples & Edge Cases

### 9.1 Concurrent Checkout — The Oversell Bug

```csharp
// service/Api/tests/Module.IntegrationTests/Ordering/CheckoutConcurrencyTests.cs

[TestMethod]
public async Task Checkout_TwoConcurrentRequests_OneSucceedsOneFails()
{
    // Arrange
    var variantId = await SeedVariantWithStockAsync(onHand: 1);
    var userA = await SeedUserAsync();
    var userB = await SeedUserAsync();
    await SeedCartWithItemAsync(userA, variantId, quantity: 1);
    await SeedCartWithItemAsync(userB, variantId, quantity: 1);

    // Act
    var taskA = SendAsync(new CreateOrderFromCart.Command(new()), userA);
    var taskB = SendAsync(new CreateOrderFromCart.Command(new()), userB);
    var results = await Task.WhenAll(taskA, taskB);

    // Assert
    var successCount = results.Count(r => r.IsSuccess);
    var failureCount = results.Count(r => r.IsFailure);
    successCount.Should().Be(1, "only one order can be placed when stock is 1");
    failureCount.Should().Be(1, "the other checkout must fail with InsufficientStock");

    var stockMovements = await DbContext.Set<StockMovement>()
        .Where(m => m.OriginatorType == "Order")
        .CountAsync();
    stockMovements.Should().Be(1, "exactly one shipment movement must be recorded");
}
```

### 9.2 Availability — Price vs Stock Edge Case

```csharp
// service/Api/tests/Module.UnitTests/Catalog/GetAvailabilityTests.cs

[TestMethod]
public async Task Handle_VariantWithPriceButNoStock_ReturnsOutOfStock()
{
    // Arrange — variant priced at $50.00, no StockItem rows
    var productId = await SeedProductAsync();
    var variantId = await SeedVariantAsync(productId, price: 50.00m);
    // Note: NO StockItem seeded

    // Act
    var result = await SendAsync(new GetAvailability.Query(productId));

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Cells.Should().ContainSingle(c =>
        c.VariantId == variantId && c.Status == "out_of_stock");
}
```

### 9.3 Webhook Idempotency

```csharp
// service/Api/tests/Module.UnitTests/Payment/Webhooks/StripeWebhookDispatcherTests.cs

[TestMethod]
public async Task ProcessAsync_PaymentIntentSucceeded_Replayed_IsIdempotent()
{
    // Arrange
    var orderId = await SeedPlacedOrderAsync(total: 100m);
    var payment = await SeedCompletedPaymentAsync(orderId, amount: 100m);
    var evt = BuildStripePaymentIntentSucceededEvent(payment.ResponseCode);

    // Act — first call
    var first = await dispatcher.ProcessAsync(evt, CancellationToken.None);
    // Act — replay
    var second = await dispatcher.ProcessAsync(evt, CancellationToken.None);

    // Assert
    first.IsSuccess.Should().BeTrue();
    second.IsSuccess.Should().BeTrue("replay must be a no-op, not an error");

    var movements = await DbContext.Set<StockMovement>()
        .Where(m => m.OriginatorId == orderId)
        .CountAsync();
    movements.Should().Be(0, "no additional stock movement on replay");
}
```

### 9.4 Configuration Fail-Fast at Boot

```csharp
// service/Api/tests/Module.IntegrationTests/Shared/NotificationOptionsTests.cs

[TestMethod]
public async Task EmptySmtpHost_OptionsValidationFails()
{
    // Arrange
    var builder = WebApplication.CreateBuilder();
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["Notification:Channels:Email:Providers:Smtp:Host"] = "",
        ["Notification:Channels:Email:Providers:Smtp:Port"] = "1025"
    });
    builder.AddNotifications();

    // Act
    var act = () => builder.Build();

    // Assert
    act.Should().Throw<OptionsValidationException>()
       .WithMessage("*SmtpProviderSetting*Host*");
}
```

### 9.5 Edge Case: Empty Cart Cannot Finalize

`CreateOrderFromCart.cs:84-86` already handles this (`if (cart.LineItems.Count == 0) return EmptyOrderCannotFinalize`). No change.

### 9.6 Edge Case: Zero-Total Order Skips Payment

`CreateOrderFromCart.cs:64` (`if (cart.Total > 0m)`) allows 100%-discount orders to skip payment verification. Confirm with the demo stakeholders whether discount codes are in scope; if not, add `if (cart.Total == 0m) return OrderResult.Errors.ZeroTotalOrderNotSupported;` after consulting.

## 10. Validation Criteria

This specification is considered complete when:

- [ ] All 6 critical fixes have merged PRs with passing CI
- [ ] All `Module.UnitTests` and `Module.IntegrationTests` pass with `TreatWarningsAsErrors=true`
- [ ] `dotnet build` succeeds with zero warnings
- [ ] The Aspire AppHost brings up the full stack and the demo can be walked through end-to-end (browse → cart → checkout → payment → order placed) without manual intervention
- [ ] The hardcoded `Jwt:Secret` in `appsettings.Development.json` is replaced with a user-secrets or env-var source
- [ ] All 6 settings missing `ValidateOnStart()` are fixed and verified by a failing-boot integration test
- [ ] `Module/Webhooks/` is either deleted or wired — pick one and update AGENTS.md
- [ ] The reviewer's pre-demo checklist (in `docs/`) is updated to reflect the new failure modes
- [ ] No new `catch (Exception)` is added that silently swallows an error
- [ ] No new `BuildServiceProvider()` call exists in any `*.Extension.cs`

## 11. Related Specifications / Further Reading

- `AGENTS.md` — non-negotiable rules (Result objects, module isolation, warnings-as-errors)
- `.harness/principles.yml` — golden principles with rationale
- `.harness/enforcement.yml` — naming, file limits, logging, import rules
- `docs/codebase/ARCHITECTURE.md` — module boundaries and data flow
- `docs/codebase/CONCERNS.md` — known tech debt and risks
- `docs/codebase/TESTING.md` — testing strategy
- `docs/codebase/PROCESS.md` — escalation boundaries
- `service/Api/src/Api/Program.cs` — host composition
- `service/Api/src/Shared/Security/AntiForgery/AntiForgery.Extensions.cs` — example of a `ValidateFluentValidation` without `ValidateOnStart`
- `service/Api/src/Shared/Operational/Notifications/Notification.Extension.cs` — 6 settings with the same gap
- Stripe Webhooks API documentation — `https://stripe.com/docs/webhooks` (signature verification, idempotency keys)
- PostgreSQL Serializable Isolation documentation — `https://www.postgresql.org/docs/current/transaction-iso.html`
