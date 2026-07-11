# Remove Webhooks Module Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Delete `Module.Webhooks` (24 files) and `Shared.Operational.Webhooks` (19 files). Replace outbound event delivery with a single Hangfire job that POSTs `order.placed` to URLs from config. Keep Stripe inbound webhook in `Payment` untouched.

**Architecture:** Strip the subscription framework entirely — no DB tables, no background sweeper, no HMAC signer, no URL validator. The existing Hangfire infrastructure already handles retry/persistence. `IOrderEventPublisher` interface stays; `NullOrderEventPublisher` becomes the sole registration. A new `OrderPlacedDeliveryJob` POSTs to configured URLs when `Webhooks:Outbound:Enabled: true`.

**Tech Stack:** .NET 10, Hangfire (existing), `IHttpClientFactory` (existing), `IOptions<T>` (existing).

## Global Constraints

- `TreatWarningsAsErrors=true` — zero warnings allowed on build
- No `Module.Webhooks` or `Shared.Operational.Webhooks` namespace may appear in any remaining `.cs` file
- Keep migration files (`Migrations/20260707*`, `Migrations/20260709*`) — orphaned tables are harmless
- Do not touch `Module/Payment/Features/Storefront/Payment/Webhooks/` (Stripe inbound) — it's independent
- Keep `IOrderEventPublisher` interface and `NullOrderEventPublisher` — they are clean and stable

---

## File Structure

### Files to Delete (43 source + 8 test + 1 http = 52 files)

```
DELETE service/Api/src/Module/Webhooks/                              (24 files, whole tree)
DELETE service/Api/src/Shared/Operational/Webhooks/                    (19 files, whole tree)
DELETE service/Api/src/Module/Ordering/Infrastructure/Events/WebhookOrderEventPublisher.cs  (1 file)
DELETE service/Api/tests/Api.Tests/Scenarios/Webhooks/                 (7 files, whole tree)
DELETE ApiTests/Webhooks/Subscriptions.http                            (1 file)
```

### Files to Modify (6 files)

```
MODIFY service/Api/src/Api/Program.cs                                  — 2 lines removed
MODIFY service/Api/src/Shared/Operational/Operational.Extension.cs      — 2 lines removed
MODIFY service/Api/tests/Module.UnitTests/Architecture/ModuleIsolationTests.cs  — 1 line removed
MODIFY .harness/domains.yml                                            — 13 lines removed
MODIFY .harness/quality.yml                                            — 14 lines removed
MODIFY .harness/enforcement.yml                                        — regex narrowed
```

### Files to Create (4 files)

```
CREATE service/Api/src/Module/Ordering/Infrastructure/Options/OutboundWebhookOptions.cs
CREATE service/Api/src/Module/Ordering/Infrastructure/Options/OutboundWebhookOptions.Extensions.cs
CREATE service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/Jobs/OrderPlacedDeliveryJob.cs
```

### Files to Update Config (2 files)

```
MODIFY service/Api/src/Api/appsettings.json         — add Webhooks:Outbound section
MODIFY service/Api/src/Api/appsettings.Development.json — same section (Enabled: false)
```

---

## Task Breakdown

### Task 1: Delete Shared.Operational.Webhooks and Fix Operational.Extension.cs

**Files:**
- Delete: `service/Api/src/Shared/Operational/Webhooks/` (19 files, entire tree)
- Modify: `service/Api/src/Shared/Operational/Operational.Extension.cs` (2 removals)

- [ ] **Step 1: Delete the Shared.Operational.Webhooks directory tree**

```bash
rm -rf service/Api/src/Shared/Operational/Webhooks
```

Verify with `ls service/Api/src/Shared/Operational/Webhooks/` — expect "No such file or directory".

- [ ] **Step 2: Remove the using and method call from Operational.Extension.cs**

Current file (`service/Api/src/Shared/Operational/Operational.Extension.cs`):

```csharp
using System.Reflection;

using Microsoft.AspNetCore.Builder;

using Shared.Operational.Backgrounds;
using Shared.Operational.Http;
using Shared.Operational.Notifications;
using Shared.Operational.Persistence;
using Shared.Operational.Storages;
using Shared.Operational.Webhooks;            // <-- REMOVE THIS LINE

namespace Shared.Operational;

public static class OperationalExtension
{
    #region Service Registration

    public static WebApplicationBuilder AddOperational(
        this WebApplicationBuilder builder,
        params Assembly[] additionalAssemblies)
    {
        builder.AddStorage();
        builder.AddPersistence(additionalAssemblies);
        builder.AddNotifications();
        builder.AddBackgroundJobs();
        builder.AddHttpClients();
        builder.AddWebhooks();                  // <-- REMOVE THIS LINE

        return builder;
    }
```

Remove `using Shared.Operational.Webhooks;` (line 10) and `builder.AddWebhooks();` (line 27).

Expected: `dotnet build` fails because `Module.Webhooks/Webhooks.Extension.cs` calls `builder.AddWebhooksModule()` which we haven't deleted yet, and that file imports from Shared.Operational.Webhooks. **(Failure is expected — we fix it in Task 2.)**

---

### Task 2: Delete Module.Webhooks, Fix Program.cs, Remove WebhookOrderEventPublisher

**Files:**
- Delete: `service/Api/src/Module/Webhooks/` (24 files, entire tree)
- Delete: `service/Api/src/Module/Ordering/Infrastructure/Events/WebhookOrderEventPublisher.cs` (1 file)
- Modify: `service/Api/src/Api/Program.cs` (3 removals)

- [ ] **Step 1: Delete the Module.Webhooks directory tree**

```bash
rm -rf service/Api/src/Module/Webhooks
```

Verify: `ls service/Api/src/Module/Webhooks/` → "No such file or directory"

- [ ] **Step 2: Delete WebhookOrderEventPublisher.cs**

```bash
rm service/Api/src/Module/Ordering/Infrastructure/Events/WebhookOrderEventPublisher.cs
```

Verify: `ls service/Api/src/Module/Ordering/Infrastructure/Events/` — should show only `NullOrderEventPublisher.cs`.

- [ ] **Step 3: Remove Webhooks references from Program.cs**

Current file (`service/Api/src/Api/Program.cs`):

```csharp
using System.Reflection;

using Module.Catalog;
using Module.Identity;
using Module.Inventory;
using Module.Location;
using Module.Ordering;
using Module.Payment;
using Module.Profile;
using Module.Shipping;
using Module.Webhooks;                              // <-- REMOVE THIS LINE
// ...
builder.AddShippingModule();
builder.AddWebhooksModule();                         // <-- REMOVE THIS LINE

// Swap: Order event publisher from no-op to webhook-backed implementation
builder.Services.AddScoped<Module.Ordering.Domain.Orders.Contracts.IOrderEventPublisher,    // <-- REMOVE ALL
    Module.Ordering.Infrastructure.Events.WebhookOrderEventPublisher>();                      // <-- 3 LINES
```

Remove:
- `using Module.Webhooks;` (line 11)
- `builder.AddWebhooksModule();` (line 45)
- The 3-line DI swap block (lines 47-49)

Expected: `dotnet build` should now succeed — no remaining references to deleted namespaces.

---

### Task 3: Create Replacement — OutboundWebhookOptions + DI + Config

**Files:**
- Create: `service/Api/src/Module/Ordering/Infrastructure/Options/OutboundWebhookOptions.cs`
- Create: `service/Api/src/Module/Ordering/Infrastructure/Options/OutboundWebhookOptions.Extensions.cs`
- Modify: `service/Api/src/Module/Ordering/Ordering.Extension.cs`
- Modify: `service/Api/src/Api/appsettings.json`
- Modify: `service/Api/src/Api/appsettings.Development.json`

- [ ] **Step 1: Create OutboundWebhookOptions.cs**

```csharp
// File: service/Api/src/Module/Ordering/Infrastructure/Options/OutboundWebhookOptions.cs

namespace Module.Ordering.Infrastructure.Options;

public sealed class OutboundWebhookOptions
{
    public const string SectionName = "Webhooks:Outbound";

    public bool Enabled { get; set; }
    public List<string> Urls { get; set; } = [];
}
```

- [ ] **Step 2: Create OutboundWebhookOptions.Extensions.cs**

```csharp
// File: service/Api/src/Module/Ordering/Infrastructure/Options/OutboundWebhookOptions.Extensions.cs

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Module.Ordering.Infrastructure.Options;

public static class OutboundWebhookOptionsExtensions
{
    public static WebApplicationBuilder AddOutboundWebhooks(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<OutboundWebhookOptions>(
            builder.Configuration.GetSection(OutboundWebhookOptions.SectionName));
        return builder;
    }
}
```

- [ ] **Step 3: Register the new options in Ordering.Extension.cs**

Current file (`service/Api/src/Module/Ordering/Ordering.Extension.cs`):

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Module.Ordering.Domain.Orders;
using Module.Ordering.Domain.Orders.Contracts;
using Module.Ordering.Persistence.Seeders;

using Shared.Operational.Persistence.Seeders;

namespace Module.Ordering;

public static class OrderingExtension
{
    public static WebApplicationBuilder AddOrderingModule(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IOrderEventPublisher, Infrastructure.Events.NullOrderEventPublisher>();
        builder.Services.AddScoped<Backgrounds.CartExpiryJob>();
        builder.Services.AddHostedService<Services.CartExpiryService>();

        builder.AddSeeder<OrderSeeder>();
        builder.AddSeeder<PaymentSeeder>();

        return builder;
    }
}
```

Add `using Module.Ordering.Infrastructure.Options;` to usings and `builder.AddOutboundWebhooks();` to the method:

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Module.Ordering.Domain.Orders;
using Module.Ordering.Domain.Orders.Contracts;
using Module.Ordering.Infrastructure.Options;
using Module.Ordering.Persistence.Seeders;

using Shared.Operational.Persistence.Seeders;

namespace Module.Ordering;

public static class OrderingExtension
{
    public static WebApplicationBuilder AddOrderingModule(this WebApplicationBuilder builder)
    {
        builder.AddOutboundWebhooks();
        builder.Services.AddScoped<IOrderEventPublisher, Infrastructure.Events.NullOrderEventPublisher>();
        builder.Services.AddScoped<Backgrounds.CartExpiryJob>();
        builder.Services.AddHostedService<Services.CartExpiryService>();

        builder.AddSeeder<OrderSeeder>();
        builder.AddSeeder<PaymentSeeder>();

        return builder;
    }
}
```

- [ ] **Step 4: Add Webhooks:Outbound section to appsettings.json**

```json
{
  // ... existing content above ...
  "Webhooks": {
    "Outbound": {
      "Enabled": false,
      "Urls": []
    }
  }
}
```

Read the current `appsettings.json` to find where to insert (after the last top-level key).

- [ ] **Step 5: Add Webhooks:Outbound section to appsettings.Development.json**

Same structure but with `"Enabled": false` and optionally a test URL if needed.

Expected: `dotnet build` succeeds.

---

### Task 4: Create OrderPlacedDeliveryJob

**Files:**
- Create: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/Jobs/OrderPlacedDeliveryJob.cs`

- [ ] **Step 1: Create the directory**

```bash
mkdir -p service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/Jobs
```

- [ ] **Step 2: Create OrderPlacedDeliveryJob.cs**

```csharp
// File: service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/Jobs/OrderPlacedDeliveryJob.cs

using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Module.Ordering.Infrastructure.Options;

namespace Module.Ordering.Features.Storefront.Cart.Checkout.Jobs;

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
            logger.LogDebug("Outbound webhooks disabled or no URLs configured. Skipping.");
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
                var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content, ct);
                response.EnsureSuccessStatusCode();
                logger.LogInformation("Delivered order.placed to {Url}: {Status}", url, response.StatusCode);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to deliver order.placed to {Url}", url);
            }
        }
    }
}

public static class OrderPlacedDeliveryJobDefaults
{
    public const string JobId = "order-placed-delivery";
}
```

- [ ] **Step 3: Register the job in Ordering.Extension.cs**

Add `services.AddScoped<OrderPlacedDeliveryJob>();`:

```csharp
public static WebApplicationBuilder AddOrderingModule(this WebApplicationBuilder builder)
{
    builder.AddOutboundWebhooks();
    builder.Services.AddScoped<IOrderEventPublisher, Infrastructure.Events.NullOrderEventPublisher>();
    builder.Services.AddScoped<Backgrounds.CartExpiryJob>();
    builder.Services.AddHostedService<Services.CartExpiryService>();
    builder.Services.AddScoped<OrderPlacedDeliveryJob>();

    builder.AddSeeder<OrderSeeder>();
    builder.AddSeeder<PaymentSeeder>();

    return builder;
}
```

Expected: `dotnet build` succeeds.

---

### Task 5: Update CreateOrderFromCart to Use Hangfire Job

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs`

- [ ] **Step 1: Read the current CreateOrderFromCart.cs to confirm line numbers**

The current handler injects `IOrderEventPublisher eventPublisher`. We need to:
1. Add `IOptions<OutboundWebhookOptions>` and `IBackgroundJobClient` (or use the static `BackgroundJob.Enqueue` if Hangfire's static API is available)
2. Replace the `PublishAsync` call with `BackgroundJob.Enqueue<OrderPlacedDeliveryJob>`

First check how Hangfire is used in the existing codebase for the notification service pattern:

The notification service uses Hangfire via `IBackgroundJobClient` interface (DI-friendly). Let's check... The notification service injects `IBackgroundJobClient? jobClient`. The `BackgroundJob.Enqueue` static method requires Hangfire's `using Hangfire;` which is already referenced throughout `Shared/Operational/Backgrounds/`.

For DI-friendliness and testability, inject `IBackgroundJobClient` from Hangfire.

- [ ] **Step 2: Modify CreateOrderFromCart.cs**

Current handler signature (around line 22-28):

```csharp
public sealed class CommandHandler(
    IApplicationDbContext dbContext,
    ILogger<CommandHandler> logger,
    ICurrentUser currentUser,
    INotificationService notificationService,
    IOrderEventPublisher eventPublisher)
    : ICommandHandler<Command, Response>
```

Add `IOptions<OutboundWebhookOptions>` and `IBackgroundJobClient`:

```csharp
public sealed class CommandHandler(
    IApplicationDbContext dbContext,
    ILogger<CommandHandler> logger,
    ICurrentUser currentUser,
    INotificationService notificationService,
    IOrderEventPublisher eventPublisher)
    : ICommandHandler<Command, Response>
```

Wait, we're keeping `IOrderEventPublisher` (it still has `NullOrderEventPublisher` as the DI registration). So `eventPublisher` stays injected but becomes a no-op. We add the new dependencies alongside it:

```csharp
public sealed class CommandHandler(
    IApplicationDbContext dbContext,
    ILogger<CommandHandler> logger,
    ICurrentUser currentUser,
    INotificationService notificationService,
    IOrderEventPublisher eventPublisher,
    IOptions<OutboundWebhookOptions> webhookOptions,
    IBackgroundJobClient backgroundJobClient)
    : ICommandHandler<Command, Response>
```

Add usings at the top of the file:

```csharp
using Hangfire;
using Microsoft.Extensions.Options;
using Module.Ordering.Infrastructure.Options;
using Module.Ordering.Features.Storefront.Cart.Checkout.Jobs;
```

Replace the call site (around line 147-157, after `SaveChangesAsync`):

Current:

```csharp
await dbContext.SaveChangesAsync(cancellationToken);

// Publish: Emit order.placed event for downstream consumers (webhooks, analytics).
await eventPublisher.PublishAsync("order.placed", new
{
    OrderId = cart.Id,
    OrderNumber = cart.Number,
    UserId = cart.UserId,
    Email = cart.Email,
    Total = cart.Total,
    Currency = cart.Currency,
    PlacedAtUtc = cart.CompletedAtUtc
}, cancellationToken);
```

Replace with:

```csharp
await dbContext.SaveChangesAsync(cancellationToken);

// Enqueue: Fire-and-forget delivery of order.placed event to configured webhook URLs.
// When Webhooks:Outbound:Enabled is false, the job skips immediately (no-op).
backgroundJobClient.Enqueue<OrderPlacedDeliveryJob>(j =>
    j.RunAsync(cart.Id, cart.Number, cart.UserId, cart.Email,
        cart.Total, cart.Currency, cart.CompletedAtUtc!.Value,
        cancellationToken));
```

The `webhookOptions` injection is consumed inside `OrderPlacedDeliveryJob.RunAsync` via its own DI — we don't need to check here. The job checks `Enabled` internally.

Remove the `eventPublisher.PublishAsync` call (the parameter stays injected; `NullOrderEventPublisher` just returns `Task.CompletedTask`).

Expected: `dotnet build` succeeds.

---

### Task 6: Delete Test Files and Update ModuleIsolationTests

**Files:**
- Delete: `service/Api/tests/Api.Tests/Scenarios/Webhooks/` (7 files, entire tree)
- Delete: `ApiTests/Webhooks/Subscriptions.http` (1 file)
- Modify: `service/Api/tests/Module.UnitTests/Architecture/ModuleIsolationTests.cs` (1 line)

- [ ] **Step 1: Delete the Webhooks integration test tree**

```bash
rm -rf service/Api/tests/Api.Tests/Scenarios/Webhooks
```

Verify: `ls service/Api/tests/Api.Tests/Scenarios/Webhooks/` → "No such file or directory"

- [ ] **Step 2: Delete the Webhooks .http smoke test**

```bash
rm -rf ApiTests/Webhooks
```

Verify: `ls ApiTests/Webhooks/` → "No such file or directory"

Note: `ApiTests/Payment/Webhooks.http` is kept — it tests the Stripe inbound webhook in Payment, not the deleted subscription framework.

- [ ] **Step 3: Update ModuleIsolationTests.cs to remove Webhooks from the module map**

Current code (line 13-24):

```csharp
private static readonly Dictionary<string, string[]> ModuleNamespaces = new()
{
    ["Catalog"] = ["Module.Catalog"],
    ["Identity"] = ["Module.Identity"],
    ["Inventory"] = ["Module.Inventory"],
    ["Location"] = ["Module.Location"],
    ["Ordering"] = ["Module.Ordering"],
    ["Payment"] = ["Module.Payment"],
    ["Profile"] = ["Module.Profile"],
    ["Shipping"] = ["Module.Shipping"],
    ["Webhooks"] = ["Module.Webhooks"],      // <-- REMOVE THIS LINE
};
```

Remove the `["Webhooks"] = ["Module.Webhooks"],` line.

Expected: `dotnet test service/Api/tests/Module.UnitTests --filter "ModuleIsolationTests"` passes.

---

### Task 7: Update .harness/ YAML Files

**Files:**
- Modify: `.harness/domains.yml` (remove Webhooks stanza)
- Modify: `.harness/quality.yml` (remove Webhooks scores)
- Modify: `.harness/enforcement.yml` (narrow regex)

- [ ] **Step 1: Remove Webhooks stanza from domains.yml**

Current (lines 169-181):

```yaml
  - name: Webhooks
    description: Webhook subscription management, delivery, and event bus for cross-module integration
    path: service/Api/src/Module/Webhooks
    layers:
      types: service/Api/src/Module/Webhooks/Domain
      config: service/Api/src/Module/Webhooks/Webhooks.Extension.cs
      domain: service/Api/src/Module/Webhooks/Domain
      features: service/Api/src/Module/Webhooks/Features
    providers:
      - persistence
      - auth
      - observability
    size_loc: 643
```

Remove these 13 lines.

- [ ] **Step 2: Remove Webhooks stanza from quality.yml**

Current (lines 132-145):

```yaml
  Webhooks:
    scores:
      code_quality: B
      test_coverage: D
      documentation: B
      observability: C
      reliability: C
      security: B
    gaps:
      - "Smallest module (643 LOC) — minimal test coverage"
      - "Webhook delivery retry/backoff logic not tested"
      - "Event bus (cross-module events) is new — integration patterns not validated"
    notes: "Recently added module for cross-module event bus. Needs more test investment."
    last_reviewed: "2026-07-11"
```

Remove these 14 lines.

- [ ] **Step 3: Narrow the enforcement.yml regex**

Current (line 94):

```yaml
    - "using Module\\.(Catalog|Identity|Inventory|Location|Ordering|Payment|Profile|Shipping|Webhooks)\\.(?!Shared)"
```

Remove `Webhooks` from the alternation group:

```yaml
    - "using Module\\.(Catalog|Identity|Inventory|Location|Ordering|Payment|Profile|Shipping)\\.(?!Shared)"
```

Expected: `dotnet build` succeeds, YAML is valid.

---

### Task 8: Final Verification

**No file changes.** Run build, tests, and grep checks.

- [ ] **Step 1: Build**

```bash
dotnet build
```

Expected: Build succeeds with exit code 0 and zero warnings (`TreatWarningsAsErrors=true`).

- [ ] **Step 2: Run unit tests**

```bash
dotnet test service/Api/tests/Module.UnitTests
```

Expected: All tests pass. The `ModuleIsolationTests.ModuleTypes_ShouldNotCrossReferenceOtherModules` test should pass (no "Webhooks" entry means no check for Module.Webhooks).

- [ ] **Step 3: Verify no remaining references**

```bash
grep -r "Module\.Webhooks" service/Api/src/ --include="*.cs" || echo "CLEAN: No Module.Webhooks references"
grep -r "Shared\.Operational\.Webhooks" service/Api/src/ --include="*.cs" || echo "CLEAN: No Shared.Operational.Webhooks references"
```

Expected: Both greps return nothing (or "CLEAN" message).

- [ ] **Step 4: Verify directories deleted**

```bash
ls service/Api/src/Module/Webhooks/ 2>/dev/null && echo "EXISTS — BAD" || echo "CLEAN: Module/Webhooks deleted"
ls service/Api/src/Shared/Operational/Webhooks/ 2>/dev/null && echo "EXISTS — BAD" || echo "CLEAN: Shared/Operational/Webhooks deleted"
```

Expected: Both return "CLEAN" messages.

---

## Spec Coverage Check

| Spec Requirement | Task Covering It |
|-----------------|-----------------|
| DEL-001: Delete Module.Webhooks | Task 2 |
| DEL-002: Delete Shared.Operational.Webhooks | Task 1 |
| DEL-003: Delete test/smoke files | Task 6 |
| MOD-001..003: Fix Program.cs | Task 2 |
| MOD-004..005: Fix Operational.Extension.cs | Task 1 |
| REP-001: Add config section | Task 3 |
| REP-002: Create Hangfire job | Task 4 |
| REP-003: Update CreateOrderFromCart | Task 5 |
| REP-004: Keep IOrderEventPublisher + NullOrderEventPublisher | Verified in Task 5 (still injected) |
| REP-005: Remove WebhookOrderEventPublisher | Task 2 |
| CON-001..005: Constraints | Task 8 verifies all |
| AC-001..013: Acceptance criteria | Covered by Task 8 |
| VC-001..010: Validation criteria | Covered by Task 8 |
| .harness/ updates | Task 7 |

**No gaps.** All spec requirements map to at least one task.
