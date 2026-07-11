---
title: Remove Module.Webhooks & Shared.Operational.Webhooks — Replace with Inline Hangfire Job
version: 1.0
date_created: 2026-07-11
owner: ReSys.Shop Team
tags: [design, refactor, webhooks, simplification, cleanup]
---

# Introduction

Remove the entire `Module.Webhooks` (643 LOC, 22 files) and `Shared.Operational.Webhooks` (~check LOC, ~17 files) subscription framework. Replace with a single Hangfire background job that POSTs `order.placed` events to URLs configured in `appsettings.json`. The inbound Stripe webhook in `Module.Payment` is unaffected.

## 1. Purpose & Scope

**Purpose:** Eliminate an over-engineered outbound webhook framework built for a multi-tenant hub scenario that does not exist. The platform has one event type (`order.placed`), one consumer (Ordering module), and zero configured external webhook recipients today.

**Scope:**
- Delete `service/Api/src/Module/Webhooks/` — entire module (22 files, Admin CRUD subscriptions)
- Delete `service/Api/src/Shared/Operational/Webhooks/` — dispatcher, signer, background sweeper, domain entities, EF configs, URL validator
- Remove DI registrations from `Program.cs` and `Operational.Extension.cs`
- Replace `WebhookOrderEventPublisher` → `HangfireOutboundEventPublisher` (or just call `IBackgroundJobClient` directly from the handler)
- Keep `IOrderEventPublisher` interface (clean, low-churn)
- Remove API test files, integration test base, `.http` smoke tests
- Update `.harness/` domain/quality/enforcement YAMLs
- Keep migration files (don't drop tables — ops can drop manually or leave orphaned)

**Out of scope:**
- `Module.Payment/Features/Storefront/Payment/Webhooks/` (Stripe inbound) — kept unchanged
- `Module.Payment/Domain/Gateways/IWebhookHandler.cs` — kept, Payment needs it
- Removing the existing EF migration files for webhook tables
- Adding new external webhook consumers

## 2. Definitions

| Term | Definition |
|------|-----------|
| Outbound webhook | An HTTP POST sent from this platform to an external URL when a business event occurs |
| Inbound webhook | An HTTP POST received by this platform from an external provider (e.g., Stripe) |
| Hangfire | Background job library already in the stack. Handles retry, scheduling, persistence |
| Subscription framework | The Admin CRUD endpoints + DB schema for managing which URLs receive which events |

## 3. Requirements, Constraints & Guidelines

### 3.1 Deletion — Module.Webhooks

- **DEL-001**: Delete `service/Api/src/Module/Webhooks/` entirely:
  - `Webhooks.Extension.cs` — module registration
  - `README.yaml` — module documentation
  - `Domain/WebhookSubscription.Result.cs`
  - `Features/Admin/WebhooksFeature.cs`
  - `Features/Admin/Subscriptions/Create/` (4 files)
  - `Features/Admin/Subscriptions/Get/ById/` (3 files)
  - `Features/Admin/Subscriptions/Get/Paged/` (3 files)
  - `Features/Admin/Subscriptions/Update/` (4 files)
  - `Features/Admin/Subscriptions/Delete/` (3 files)
  - `Features/Admin/Subscriptions/Test/` (3 files)
  - `Persistence/Seeders/WebhookSubscription.Seeder.cs`

- **DEL-002**: Delete `service/Api/src/Shared/Operational/Webhooks/` entirely:
  - `Webhooks.Extensions.cs` — DI registration
  - `Domain/WebhookEnums.cs`
  - `Domain/WebhookSubscription.cs`
  - `Domain/WebhookSubscription.Constant.cs`
  - `Domain/WebhookSubscription.Method.cs`
  - `Domain/WebhookSubscription.Result.cs`
  - `Domain/WebhookDelivery.cs`
  - `Domain/WebhookDelivery.Constant.cs`
  - `Domain/WebhookDelivery.Method.cs`
  - `Domain/WebhookDelivery.Result.cs`
  - `Domain/WebhookUrlValidator.cs`
  - `Services/IWebhookDispatcher.cs`
  - `Services/IWebhookSigner.cs`
  - `Services/WebhookDispatcher.cs`
  - `Services/WebhookSigner.cs`
  - `Backgrounds/WebhookDeliveryJob.cs`
  - `Persistence/WebhookSchema.cs`
  - `Persistence/Configurations/WebhookSubscription.EntityConfiguration.cs`
  - `Persistence/Configurations/WebhookDelivery.EntityConfiguration.cs`

- **DEL-003**: Delete test/smoke files:
  - `ApiTests/Webhooks/Subscriptions.http`
  - `service/Api/tests/Api.Tests/Scenarios/Webhooks/WebhooksIntegrationTestBase.cs`
  - `ApiTests/Payment/Webhooks.http` (Stripe inbound, keep — re-evaluate: Stripe .http tests should stay since Payment is kept)

### 3.2 Modification — Program.cs & DI

- **MOD-001**: Remove `using Module.Webhooks;` from `Program.cs`.
- **MOD-002**: Remove `builder.AddWebhooksModule();` call from `Program.cs`.
- **MOD-003**: Remove the DI swap in `Program.cs`:
  ```csharp
  // DELETE: Swap: Order event publisher from no-op to webhook-backed implementation
  builder.Services.AddScoped<...>();
  ```
- **MOD-004**: Remove `using Shared.Operational.Webhooks;` from `Shared/Operational/Operational.Extension.cs`.
- **MOD-005**: Remove `builder.AddWebhooks();` call from `AddOperational()`.

### 3.3 Replacement — Outbound Event Delivery

- **REP-001**: Add `Webhooks:Outbound:Urls` config section in `appsettings.json`:
  ```json
  {
    "Webhooks": {
      "Outbound": {
        "Enabled": false,
        "Urls": []
      }
    }
  }
  ```

- **REP-002**: Create `Ordering/Features/Storefront/Cart/Checkout/Jobs/OrderPlacedDeliveryJob.cs` — a Hangfire job that reads the configured URLs and POSTs the event payload to each:

  ```csharp
  public static class OrderPlacedDeliveryJob
  {
      public const string JobId = "order-placed-delivery";

      public static async Task RunAsync(
          Guid orderId, string orderNumber, Guid userId, string email,
          decimal total, string currency, DateTimeOffset placedAtUtc)
      {
          // 1. Read Webhooks:Outbound:Urls from config
          // 2. For each URL, POST JSON payload
          // 3. Log success/failure per URL
          // Hangfire retries on transient failure
      }
  }
  ```

- **REP-003**: In `CreateOrderFromCart.cs`, replace `await eventPublisher.PublishAsync(...)` with:

  ```csharp
  await dbContext.SaveChangesAsync(cancellationToken);

  // Enqueue background delivery (fire-and-forget via Hangfire)
  BackgroundJob.Enqueue<OrderPlacedDeliveryJob>(j =>
      j.RunAsync(cart.Id, cart.Number, cart.UserId, cart.Email,
          cart.Total, cart.Currency, cart.CompletedAtUtc!.Value));
  ```

- **REP-004**: Keep `IOrderEventPublisher` interface and `NullOrderEventPublisher` registration — they still compile and the interface is clean. The `NullOrderEventPublisher` becomes the only registration (already the default in `Ordering.Extension.cs`).

- **REP-005**: Remove `WebhookOrderEventPublisher.cs` file (`Ordering/Infrastructure/Events/WebhookOrderEventPublisher.cs`).

### 3.4 Constraints

- **CON-001**: `Module.Webhooks` must not appear in any `using` directive, type reference, or DI call after deletion.
- **CON-002**: `Shared.Operational.Webhooks` namespace must not appear in any file after deletion.
- **CON-003**: The `IWebhookDispatcher` and `IWebhookSigner` interfaces must be deleted — not kept "for later."
- **CON-004**: Build must pass with zero warnings (`TreatWarningsAsErrors=true`) after all deletions.
- **CON-005**: All unit tests must pass after changes.

### 3.5 Guidelines

- **GUD-001**: Keep migration files `AddWebhookSubscriptions` and `RefactorWebhookConstants` in the Migrations project. Do not try to revert them — ops can apply a manual migration if they need to clean up orphaned tables.
- **GUD-002**: Do not touch `Module.Payment/Webhooks/` (Stripe inbound). It's self-contained, correct, and independent.

## 4. Interfaces & Data Contracts

### 4.1 New Config Section

```json
{
  "Webhooks": {
    "Outbound": {
      "Enabled": false,
      "Urls": []
    }
  }
}
```

When `Enabled: false`, `BackgroundJob.Enqueue` must not be called (conditional guard in the handler).

### 4.2 New Hangfire Job

```csharp
namespace Module.Ordering.Features.Storefront.Cart.Checkout.Jobs;

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public sealed class OrderPlacedDeliveryJob(
    IHttpClientFactory httpClientFactory,
    IOptions<OutboundWebhookOptions> options,
    ILogger<OrderPlacedDeliveryJob> logger)
{
    public async Task RunAsync(
        Guid orderId, string orderNumber, Guid userId, string email,
        decimal total, string currency, DateTimeOffset placedAtUtc,
        CancellationToken ct = default)
    {
        if (!options.Value.Enabled || options.Value.Urls.Count == 0)
        {
            logger.LogDebug("Outbound webhooks disabled or no URLs configured. Skipping delivery.");
            return;
        }

        var payload = JsonSerializer.Serialize(new
        {
            Event = "order.placed",
            OrderId = orderId,
            OrderNumber = orderNumber,
            UserId = userId,
            Email = email,
            Total = total,
            Currency = currency,
            PlacedAtUtc = placedAtUtc
        });

        using var client = httpClientFactory.CreateClient("OutboundWebhook");

        foreach (var url in options.Value.Urls)
        {
            try
            {
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content, ct);
                response.EnsureSuccessStatusCode();
                logger.LogInformation("Delivered order.placed to {Url}: {Status}", url, response.StatusCode);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to deliver order.placed to {Url}", url);
                // Hangfire will retry according to its automatic retry policy
            }
        }
    }
}
```

### 4.3 Options Class

```csharp
namespace Module.Ordering.Infrastructure.Options;

public sealed class OutboundWebhookOptions
{
    public const string SectionName = "Webhooks:Outbound";

    public bool Enabled { get; set; }
    public List<string> Urls { get; set; } = [];
}
```

### 4.4 DI Registration

```csharp
// In Ordering.Extension.cs (or a new file Ordering/Infrastructure/Options/OutboundWebhook.Extensions.cs)
services.Configure<OutboundWebhookOptions>(
    configuration.GetSection(OutboundWebhookOptions.SectionName));
services.AddScoped<OrderPlacedDeliveryJob>();
```

### 4.5 `CreateOrderFromCart.cs` — Replacement Call

```csharp
// At line 145-157, replace:
// await eventPublisher.PublishAsync("order.placed", new { ... }, cancellationToken);

// With:
if (webhookOptions.Value.Enabled)
{
    BackgroundJob.Enqueue<OrderPlacedDeliveryJob>(j =>
        j.RunAsync(cart.Id, cart.Number, cart.UserId, cart.Email,
            cart.Total, cart.Currency, cart.CompletedAtUtc!.Value));
}
```

## 5. Acceptance Criteria

- **AC-001**: Given `Webhooks:Outbound:Enabled: false`, When `CreateOrderFromCart` completes, Then no background job is enqueued.
- **AC-002**: Given `Webhooks:Outbound:Enabled: true` with 2 URLs configured, When `CreateOrderFromCart` completes, Then 2 HTTP POST requests are sent (one per URL).
- **AC-003**: Given an unreachable URL, When `OrderPlacedDeliveryJob` runs, Then Hangfire retries according to its default retry policy and logs a warning.
- **AC-004**: Given `dotnet build`, Then zero compilation errors and zero warnings across the entire solution.
- **AC-005**: Given `dotnet test`, Then all existing tests pass with zero regressions.
- **AC-006**: Given `dotnet build`, Then no file references `Module.Webhooks` or `Shared.Operational.Webhooks`.
- **AC-007**: Given `Module.Webhooks/` directory, Then it does not exist after deletion.
- **AC-008**: Given `Shared/Operational/Webhooks/` directory, Then it does not exist after deletion.
- **AC-009**: Given `Program.cs`, Then no `builder.AddWebhooksModule()` line exists and no `using Module.Webhooks` import exists.
- **AC-010**: Given `Operational.Extension.cs`, Then no `builder.AddWebhooks()` call exists and no `using Shared.Operational.Webhooks` import exists.
- **AC-011**: Given the Stripe inbound webhook `POST /api/storefront/webhooks/stripe`, Then it still works unchanged (regression check).
- **AC-012**: Given `.harness/domains.yml`, Then Webhooks domain stanza is removed or marked deprecated.
- **AC-013**: Given `.harness/quality.yml`, Then Webhooks quality scores stanza is removed.

## 6. Test Automation Strategy

### Test Levels
- **Build verification**: `dotnet build` succeeds zero-warnings (Gate 1)
- **Unit**: Existing `NullOrderEventPublisher` test coverage is sufficient. No new unit tests needed for the deletion. New `OutboundWebhookOptions` validation if desired.
- **Integration**: Stripe inbound webhook tests must still pass (`StripeWebhookTests.cs`).
- **Manual**: The Admin UI no longer has "Webhook Subscriptions" menu item — verify removal.

### Coverage
- Regression-only: no new coverage requirements. Existing test coverage unaffected.

## 7. Rationale & Context

**Why drop the Subscription Framework:** The `Module.Webhooks` Admin CRUD + `Shared.Operational.Webhooks` dispatcher/signer/background-sweeper architecture was built as generic infrastructure for an outbound event bus with multiple event types, multiple subscribers, retry with exponential backoff, HMAC signing, URL validation, and an audit trail. The platform has exactly one event type (`order.placed`), one publisher (`Ordering`), and zero external subscribers today. Maintaining 22+ files and 2 DB tables for a feature with no configured consumers is premature generality.

**Why Hangfire:** Hangfire is already in the stack for async notification delivery. It provides automatic retry, persistence across restarts, and a dashboard for monitoring. Using `BackgroundJob.Enqueue` instead of a bespoke background sweeper eliminates an entire class of bugs (race conditions in the polling loop, missed deliveries on restart, inconsistent `SaveChangesAsync` calls).

**Why no subscription CRUD in the replacement:** If a future requirement demands dynamic per-URL configuration, add an API then. For now, static config in `appsettings.json` is the right level of complexity.

**Why keep migrations:** Reversing migrations is risky and provides no value. Orphaned tables are harmless. Ops can `DROP TABLE IF EXISTS webhooks.webhook_subscriptions, webhooks.webhook_deliveries;` when convenient.

## 8. Dependencies & External Integrations

### External Systems
- **EXT-001**: None directly — the new `OrderPlacedDeliveryJob` calls configured HTTP endpoints when `Webhooks:Outbound:Enabled: true`.

### Infrastructure Dependencies
- **INF-001**: Hangfire (already registered in `Shared/Operational/Backgrounds/`). Must remain registered — still used by notifications.

### Technology Platform Dependencies
- **PLT-001**: .NET 10 — no change.
- **PLT-002**: `System.Net.Http.IHttpClientFactory` — already registered via `Shared/Operational/Http/HttpClientExtensions.cs`. The new job uses the existing named client infrastructure.

## 9. Examples & Edge Cases

### Deletion Order (safe sequence)

```
1. Delete Shared.Operational.Webhooks/ files first (lowest dependency)
2. Delete Module.Webhooks/ files
3. Remove from Program.cs, Operational.Extension.cs
4. Delete WebhookOrderEventPublisher.cs
5. Add OrderPlacedDeliveryJob.cs + config
6. Update CreateOrderFromCart handler call site
7. Remove test files
8. Update .harness/ YAML files
9. dotnet build, dotnet test
```

### Edge Cases

| Case | Expected Behavior |
|------|------------------|
| `Webhooks:Outbound:Enabled: true` with empty `Urls` list | No POSTs sent. Warning logged. |
| `Webhooks:Outbound` section missing from `appsettings.json` | `OutboundWebhookOptions.Enabled` defaults to `false`. Safe. |
| Hangfire storage unreachable | `BackgroundJob.Enqueue` throws at runtime. Existing error handling in notification service applies. |
| Orphaned `webhooks` schema tables in PostgreSQL | No impact. Migrations project still has create scripts but nothing references them at runtime. |
| Stripe webhook config references `GatewayProviders:stripe:WebhookSecret` | Unchanged — this config key is in Payment module, not in the deleted code. |
| Existing seed data for webhook subscriptions | Orphaned in DB. No runtime impact. Ops can truncate manually. |

## 10. Validation Criteria

| ID | Criterion | Verification Method |
|----|-----------|-------------------|
| VC-001 | `dotnet build` succeeds with zero warnings | `dotnet build` |
| VC-002 | `dotnet test` passes with no regressions | `dotnet test service/Api/tests/Module.UnitTests` |
| VC-003 | No `Module.Webhooks` usage remains | `grep -r "Module.Webhooks" service/Api/src/ --include="*.cs"` returns empty |
| VC-004 | No `Shared.Operational.Webhooks` usage remains | `grep -r "Shared.Operational.Webhooks" service/Api/src/ --include="*.cs"` returns empty |
| VC-005 | `Module.Webhooks/` directory deleted | `ls service/Api/src/Module/Webhooks/` returns "No such file" |
| VC-006 | `Shared/Operational/Webhooks/` directory deleted | `ls service/Api/src/Shared/Operational/Webhooks/` returns "No such file" |
| VC-007 | `Program.cs` has no `AddWebhooksModule` | Grep of `Program.cs` confirms |
| VC-008 | `Operational.Extension.cs` has no `AddWebhooks` | Grep of file confirms |
| VC-009 | Stripe inbound webhook still works | `dotnet test --filter StripeWebhookTests` |
| VC-010 | `.harness` files updated | Review commits for changes to `domains.yml`, `quality.yml`, `enforcement.yml` |

## 11. Related Specifications / Further Reading

- `docs/codebase/ARCHITECTURE.md` — Architecture and data flow (needs update after this change)
- `.harness/domains.yml` — Webhooks domain stanza (needs removal)
- `.harness/quality.yml` — Webhooks quality scores (needs removal)
- `.harness/enforcement.yml` — Module cross-reference rules (Webhooks entry needs removal)
- `service/Api/src/Shared/Operational/Backgrounds/Background.Extension.cs` — Existing Hangfire setup (unchanged)
- `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs` — Primary call site (needs modification)
