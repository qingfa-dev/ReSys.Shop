# Order Details Enrichment Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Enrich the Admin and Storefront order-detail pages with a derived timeline, payment captures, shipments, and the order's payment/shipment timestamps, sourced from the order-detail payload (no persistence, no migration).

**Architecture:** Backend adds `Payments`, `Shipments`, `Timeline`, and five timestamps to the shared `OrderDetailResponse` and maps them in `OrderMapping` (timeline derived from existing `Order` timestamps). Both get-order handlers eager-load `PaymentCaptures` and `Shipments`. Each SPA extends its `OrderDetail` types and renders the new data; the Admin shipments section stays a management panel (tracking + status), now sourced from the payload.

**Tech Stack:** .NET 10 (warnings-as-errors), EF Core InMemory (tests), xunit v3 (MTP runner) + FluentAssertions; Vue 3 + TypeScript + Vitest (pnpm) for both SPAs; PrimeVue.

## Global Constraints

- `TreatWarningsAsErrors=true` — any C# warning fails the build; test code must not trigger nullable-reference warnings.
- Test runner (backend): `dotnet test --filter` does NOT work (xunit v3 MTP rejects it — "Zero tests ran", exit 5). Run a single class via the built binary's `-class` flag.
- Single assembly `Module` holds all domain types; tests set `ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly]`.
- Result objects, not exceptions (domain factories return `Result<T>`; handlers return `Result`).
- Cross-module `using` is permitted: `OrderDetailResponse`/summary records (Ordering) reference `PaymentRecordState` (Billing) and `ShipmentStatus` (Shipping).
- Both SPA view files follow their `AGENTS.md` Code Commenting Standard v3.0 (label comments `// Label: …` in `<script setup>`, `<!-- Section: … -->` in `<template>`, ≤100 chars, no multi-line `/* */` in script).
- SPA verification: `pnpm run lint` and `pnpm run test:unit` (in `app/Admin` and `app/Store`).
- `OrderApi.listShipments` is **removed** (no remaining caller). `PaymentApi.getPayments` stays (used by `usePaymentList.ts`).
- Timeline is derived (no `OrderHistory` table/writes).

---

## File Structure

- **Modify:** `service/Api/src/Module/Ordering/Features/Admin/Shared/Models/Order.Model.cs` — add summary records + `OrderDetailResponse` fields.
- **Modify:** `service/Api/src/Module/Ordering/Features/Admin/Shared/Mappings/Order.Mapping.cs` — map new fields + `BuildTimeline`.
- **Create:** `service/Api/tests/Module.UnitTests/Ordering/Features/Admin/Shared/Mappings/OrderMappingTests.cs`
- **Modify:** `service/Api/src/Module/Ordering/Features/Admin/Orders/Get/ById/GetOrderById.cs` — add Includes.
- **Modify:** `service/Api/src/Module/Ordering/Features/Storefront/Orders/Get/ById/GetCustomerOrder.cs` — add Includes.
- **Create:** `service/Api/tests/Module.UnitTests/Ordering/Features/Admin/Orders/Get/ById/GetOrderByIdTests.cs`
- **Create:** `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Orders/Get/ById/GetCustomerOrderTests.cs`
- **Modify:** `app/Admin/src/features/ordering/types/order.ts`, `services/orderApi.ts`, `__tests__/services/orderApi.spec.ts`
- **Modify:** `app/Admin/src/features/ordering/views/OrderDetail.vue`
- **Modify:** `app/Store/src/features/ordering/types/order.ts`, `validations/order.ts`
- **Modify:** `app/Store/src/features/ordering/views/OrderDetailView.vue`, `views/__tests__/OrderDetailView.spec.ts`

---

### Task 1: Backend — DTO records, mapping, and timeline derivation

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Shared/Models/Order.Model.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Shared/Mappings/Order.Mapping.cs`
- Create: `service/Api/tests/Module.UnitTests/Ordering/Features/Admin/Shared/Mappings/OrderMappingTests.cs`

**Interfaces:**
- Consumes: `Order` entity (`PaymentProcessingAtUtc`, `PaymentCompletedAtUtc`, `PaymentFailedAtUtc`, `ShipmentShippedAtUtc`, `ShipmentDeliveredAtUtc`, `PaymentCaptures`, `Shipments`); `PaymentCapture` (`Number`, `Amount`, `Currency`, `State`, `PaymentStatus`, `ProviderKey`, `PaymentMethodId`, `CreatedAtUtc`, `CompletedAtUtc`, `FailedAtUtc`); `Shipment` (`Id`, `OrderId`, `ShippingMethodId`, `ShippingMethod.Name`, `TrackingNumber`, `Status`, `ShippedAtUtc`, `DeliveredAtUtc`, `EstimatedDeliveryAtUtc`, `CreatedAtUtc`).
- Produces: `OrderDetailResponse` with `Payments`, `Shipments`, `Timeline`, and the five timestamps; `OrderMapping.BuildTimeline(Order)` (internal static).

- [ ] **Step 1: Add the summary records and DTO fields**

In `Order.Model.cs`, add the two usings after the existing ones (line 3):

```csharp
using Module.Billing.Domain.PaymentCaptures;
using Module.Shipping.Domain.Shipments;
```

Add three new records and the new `OrderDetailResponse` properties. Insert the records right after `LineItemResponse` (after line 120), and add the properties to `OrderDetailResponse` (after `ModifiedAtUtc`, line 88) and before `LineItems`:

In `OrderDetailResponse`, insert after `public DateTimeOffset? ModifiedAtUtc { get; init; }`:

```csharp
    public DateTimeOffset? PaymentProcessingAtUtc { get; init; }
    public DateTimeOffset? PaymentCompletedAtUtc { get; init; }
    public DateTimeOffset? PaymentFailedAtUtc { get; init; }
    public DateTimeOffset? ShipmentShippedAtUtc { get; init; }
    public DateTimeOffset? ShipmentDeliveredAtUtc { get; init; }
    public List<PaymentCaptureSummary> Payments { get; init; } = [];
    public List<ShipmentSummary> Shipments { get; init; } = [];
    public List<OrderTimelineEvent> Timeline { get; init; } = [];
```

Append after the `LineItemResponse` record (after its closing `}`):

```csharp
public sealed record PaymentCaptureSummary
{
    public Guid Id { get; init; }
    public string Number { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public PaymentRecordState State { get; init; }
    public string? PaymentStatus { get; init; }
    public string ProviderKey { get; init; } = string.Empty;
    public Guid? PaymentMethodId { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? CompletedAtUtc { get; init; }
    public DateTimeOffset? FailedAtUtc { get; init; }
}

public sealed record ShipmentSummary
{
    public Guid Id { get; init; }
    public Guid OrderId { get; init; }
    public Guid ShippingMethodId { get; init; }
    public string? ShippingMethodName { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
    public ShipmentStatus Status { get; init; }
    public DateTimeOffset? ShippedAtUtc { get; init; }
    public DateTimeOffset? DeliveredAtUtc { get; init; }
    public DateTimeOffset? EstimatedDeliveryAtUtc { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
}

public sealed record OrderTimelineEvent
{
    public string Type { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public DateTimeOffset? OccurredAtUtc { get; init; }
}
```

- [ ] **Step 2: Write the failing test**

Create `service/Api/tests/Module.UnitTests/Ordering/Features/Admin/Shared/Mappings/OrderMappingTests.cs`:

```csharp
using Microsoft.EntityFrameworkCore;

using Module.Billing.Domain.PaymentCaptures;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Shared.Mappings;
using Module.Ordering.Features.Admin.Shared.Models;
using Module.Shipping.Domain.Shipments;
using Module.Shipping.Domain.ShippingMethods;

namespace Module.UnitTests.Ordering.Features.Admin.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "OrderMapping")]
public class OrderMappingTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;

    public OrderMappingTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
    }

    [Fact(DisplayName = "BuildTimeline: skips null timestamps and sorts ascending")]
    public void BuildTimeline_SkipsNulls_AndSortsAscending()
    {
        var order = OrderMethod.Create("USD", Guid.NewGuid()).Value;
        order.CreatedAtUtc = new DateTimeOffset(2026, 8, 1, 10, 0, 0, TimeSpan.Zero);
        order.CompletedAtUtc = new DateTimeOffset(2026, 8, 1, 12, 0, 0, TimeSpan.Zero);
        order.PaymentCompletedAtUtc = new DateTimeOffset(2026, 8, 1, 11, 0, 0, TimeSpan.Zero);
        order.ShipmentShippedAtUtc = new DateTimeOffset(2026, 8, 2, 9, 0, 0, TimeSpan.Zero);
        // PaymentFailedAtUtc / CanceledAtUtc / ApprovedAtUtc remain null.

        var timeline = OrderMapping.BuildTimeline(order);

        timeline.Select(e => e.Type).Should().Equal("created", "payment_completed", "placed", "shipped");
        timeline.Should().BeInAscendingOrder(e => e.OccurredAtUtc);
    }

    [Fact(DisplayName = "MapToDetail: maps payments, shipments, and the five timestamps")]
    public async Task MapToDetail_MapsPaymentsShipmentsTimestamps()
    {
        var ct = TestContext.Current.CancellationToken;
        var order = OrderMethod.Create("USD", Guid.NewGuid()).Value;
        order.PaymentProcessingAtUtc = DateTimeOffset.UtcNow.AddMinutes(-30);
        order.PaymentCompletedAtUtc = DateTimeOffset.UtcNow;
        order.ShipmentShippedAtUtc = DateTimeOffset.UtcNow.AddMinutes(-5);
        _dbContext.Set<Order>().Add(order);

        var method = ShippingMethodMethod.Create("Express", "flat_rate").Value;
        var shipment = ShipmentMethod.Create(order.Id, method.Id).Value;
        shipment.TrackingNumber = "TRK-1";
        shipment.Status = ShipmentStatus.Shipped;
        _dbContext.Set<ShippingMethod>().Add(method);
        _dbContext.Set<Shipment>().Add(shipment);

        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), order.Id).Value;
        payment.State = PaymentRecordState.Completed;
        _dbContext.Set<PaymentCapture>().Add(payment);

        await _dbContext.SaveChangesAsync(ct);

        // Reload: the mapper reads the navigations off the tracked aggregate.
        var entity = await _dbContext.Set<Order>()
            .Include(x => x.PaymentCaptures)
            .Include(x => x.Shipments)
            .AsNoTracking()
            .FirstAsync(x => x.Id == order.Id, ct);

        var response = entity.MapToDetail<OrderDetailResponse>();

        response.PaymentProcessingAtUtc.Should().Be(order.PaymentProcessingAtUtc);
        response.PaymentCompletedAtUtc.Should().Be(order.PaymentCompletedAtUtc);
        response.ShipmentShippedAtUtc.Should().Be(order.ShipmentShippedAtUtc);
        response.PaymentFailedAtUtc.Should().BeNull();
        response.Payments.Should().ContainSingle().Which.Number.Should().Be(payment.Number);
        response.Shipments.Should().ContainSingle().Which.TrackingNumber.Should().Be("TRK-1");
        response.Timeline.Should().NotBeEmpty();
    }

    public void Dispose() => _dbContext.Dispose();
}
```

Note: `ShipmentMethod.Create` and `PaymentCaptureMethod.Create` are the existing factory methods (same assembly); if their exact signatures differ, read them and adapt (the test above matches `ShipmentMethod.Create(Guid orderId, Guid shippingMethodId)` and `PaymentCaptureMethod.Create(decimal amount, Guid paymentMethodId, Guid orderId)` — confirm against `Shipment.Method.Factory.cs` / `PaymentCapture.Method.Factory.cs`).

- [ ] **Step 3: Run test to verify it fails**

Run:
```bash
dotnet build service/Api/tests/Module.UnitTests/Module.UnitTests.csproj -v q --nologo
```
Expected: FAIL — `CS0117: 'OrderMapping' does not contain a definition for 'BuildTimeline'` (and `MapToDetail` returns no `Payments`/`Shipments`/`Timeline`).

- [ ] **Step 4: Add mapping + timeline derivation**

In `Order.Mapping.cs`, inside `MapToDetailCore<T>` (the `new T { … }` object initializer), add after `ModifiedAtUtc = entity.ModifiedAtUtc,`:

```csharp
            PaymentProcessingAtUtc = entity.PaymentProcessingAtUtc,
            PaymentCompletedAtUtc = entity.PaymentCompletedAtUtc,
            PaymentFailedAtUtc = entity.PaymentFailedAtUtc,
            ShipmentShippedAtUtc = entity.ShipmentShippedAtUtc,
            ShipmentDeliveredAtUtc = entity.ShipmentDeliveredAtUtc,
            Payments = entity.PaymentCaptures
                .OrderBy(p => p.CreatedAtUtc)
                .Select(p => new PaymentCaptureSummary
                {
                    Id = p.Id,
                    Number = p.Number,
                    Amount = p.Amount,
                    Currency = p.Currency,
                    State = p.State,
                    PaymentStatus = p.PaymentStatus,
                    ProviderKey = p.ProviderKey,
                    PaymentMethodId = p.PaymentMethodId,
                    CreatedAtUtc = p.CreatedAtUtc,
                    CompletedAtUtc = p.CompletedAtUtc,
                    FailedAtUtc = p.FailedAtUtc,
                }).ToList(),
            Shipments = entity.Shipments
                .OrderBy(s => s.CreatedAtUtc)
                .Select(s => new ShipmentSummary
                {
                    Id = s.Id,
                    OrderId = s.OrderId,
                    ShippingMethodId = s.ShippingMethodId,
                    ShippingMethodName = s.ShippingMethod.Name,
                    TrackingNumber = s.TrackingNumber,
                    Status = s.Status,
                    ShippedAtUtc = s.ShippedAtUtc,
                    DeliveredAtUtc = s.DeliveredAtUtc,
                    EstimatedDeliveryAtUtc = s.EstimatedDeliveryAtUtc,
                    CreatedAtUtc = s.CreatedAtUtc,
                }).ToList(),
            Timeline = BuildTimeline(entity),
```

Add the `BuildTimeline` method to `OrderMapping` (append after `MapToListItem<T>`):

```csharp
    /// <summary>Derives a chronological timeline from the order's existing timestamps (nulls skipped).</summary>
    // Derive: Fixed timestamp -> event mapping, filtered to occurred events and sorted ascending.
    internal static List<OrderTimelineEvent> BuildTimeline(Order entity)
    {
        return new List<OrderTimelineEvent>
        {
            new() { Type = "created", Label = "Order created", OccurredAtUtc = entity.CreatedAtUtc },
            new() { Type = "placed", Label = "Order placed", OccurredAtUtc = entity.CompletedAtUtc },
            new() { Type = "approved", Label = "Order approved", OccurredAtUtc = entity.ApprovedAtUtc },
            new() { Type = "payment_processing", Label = "Payment processing", OccurredAtUtc = entity.PaymentProcessingAtUtc },
            new() { Type = "payment_completed", Label = "Payment completed", OccurredAtUtc = entity.PaymentCompletedAtUtc },
            new() { Type = "payment_failed", Label = "Payment failed", OccurredAtUtc = entity.PaymentFailedAtUtc },
            new() { Type = "shipped", Label = "Order shipped", OccurredAtUtc = entity.ShipmentShippedAtUtc },
            new() { Type = "delivered", Label = "Order delivered", OccurredAtUtc = entity.ShipmentDeliveredAtUtc },
            new() { Type = "canceled", Label = "Order canceled", OccurredAtUtc = entity.CanceledAtUtc },
        }
        .Where(e => e.OccurredAtUtc.HasValue)
        .OrderBy(e => e.OccurredAtUtc)
        .ToList();
    }
```

- [ ] **Step 5: Run tests to verify they pass**

Run:
```bash
dotnet build service/Api/tests/Module.UnitTests/Module.UnitTests.csproj -v q --nologo
cd service/Api/tests/Module.UnitTests/bin/Debug/net10.0
./Module.UnitTests -class "Module.UnitTests.Ordering.Features.Admin.Shared.Mappings.OrderMappingTests"
```
Expected: PASS (`Failed: 0`).

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Admin/Shared/Models/Order.Model.cs \
        service/Api/src/Module/Ordering/Features/Admin/Shared/Mappings/Order.Mapping.cs \
        service/Api/tests/Module.UnitTests/Ordering/Features/Admin/Shared/Mappings/OrderMappingTests.cs
git commit -m "feat(ordering): embed payments, shipments, timeline, and timestamps in order detail"
```

---

### Task 2: Backend — eager-load navigations in both get-order handlers

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Orders/Get/ById/GetOrderById.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Orders/Get/ById/GetCustomerOrder.cs`
- Create: `service/Api/tests/Module.UnitTests/Ordering/Features/Admin/Orders/Get/ById/GetOrderByIdTests.cs`
- Create: `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Orders/Get/ById/GetCustomerOrderTests.cs`

**Interfaces:**
- Consumes: `OrderMapping.MapToDetailWithLookup<T>` (Task 1); `ProductLookupFactory.BuildAsync` (existing).
- Produces: `GetOrderById.Response` / `GetCustomerOrder.Response` (both `OrderDetailResponse`) now include `Payments`/`Shipments`/`Timeline` because the navigations are loaded.

- [ ] **Step 1: Write the failing tests**

Create `GetOrderByIdTests.cs`:

```csharp
using Microsoft.EntityFrameworkCore;

using Module.Billing.Domain.PaymentCaptures;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Get.ById;
using Module.Shipping.Domain.Shipments;
using Module.Shipping.Domain.ShippingMethods;

namespace Module.UnitTests.Ordering.Features.Admin.Orders.Get.ById;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "GetOrderById")]
public class GetOrderByIdTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;

    public GetOrderByIdTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
    }

    [Fact(DisplayName = "includes payment captures and shipments in the detail response")]
    public async Task Handle_IncludesPaymentsAndShipments()
    {
        var ct = TestContext.Current.CancellationToken;
        var order = OrderMethod.Create("USD", Guid.NewGuid()).Value;
        _dbContext.Set<Order>().Add(order);

        var method = ShippingMethodMethod.Create("Express", "flat_rate").Value;
        var shipment = ShipmentMethod.Create(order.Id, method.Id).Value;
        _dbContext.Set<ShippingMethod>().Add(method);
        _dbContext.Set<Shipment>().Add(shipment);

        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), order.Id).Value;
        _dbContext.Set<PaymentCapture>().Add(payment);

        await _dbContext.SaveChangesAsync(ct);

        var handler = new GetOrderById.QueryHandler(_dbContext);
        var result = await handler.Handle(new GetOrderById.Query(order.Id), ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.Payments.Should().ContainSingle().Which.Id.Should().Be(payment.Id);
        result.Value.Shipments.Should().ContainSingle().Which.Id.Should().Be(shipment.Id);
        result.Value.Timeline.Should().NotBeEmpty();
    }

    public void Dispose() => _dbContext.Dispose();
}
```

Create `GetCustomerOrderTests.cs`:

```csharp
using Microsoft.EntityFrameworkCore;

using Module.Billing.Domain.PaymentCaptures;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.Orders.Get.ById;
using Module.Shipping.Domain.Shipments;
using Module.Shipping.Domain.ShippingMethods;

namespace Module.UnitTests.Ordering.Features.Storefront.Orders.Get.ById;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "GetCustomerOrder")]
public class GetCustomerOrderTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Guid _userId = Guid.NewGuid();

    public GetCustomerOrderTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserId).Returns(_userId.ToString());
    }

    [Fact(DisplayName = "includes payment captures and shipments scoped to the current user")]
    public async Task Handle_IncludesPaymentsAndShipments()
    {
        var ct = TestContext.Current.CancellationToken;
        var order = OrderMethod.Create("USD", _userId).Value;
        _dbContext.Set<Order>().Add(order);

        var method = ShippingMethodMethod.Create("Express", "flat_rate").Value;
        var shipment = ShipmentMethod.Create(order.Id, method.Id).Value;
        _dbContext.Set<ShippingMethod>().Add(method);
        _dbContext.Set<Shipment>().Add(shipment);

        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), order.Id).Value;
        _dbContext.Set<PaymentCapture>().Add(payment);

        await _dbContext.SaveChangesAsync(ct);

        var handler = new GetCustomerOrder.QueryHandler(_dbContext, _currentUserMock.Object);
        var result = await handler.Handle(new GetCustomerOrder.Query(order.Id), ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.Payments.Should().ContainSingle();
        result.Value.Shipments.Should().ContainSingle();
    }

    public void Dispose() => _dbContext.Dispose();
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run:
```bash
dotnet build service/Api/tests/Module.UnitTests/Module.UnitTests.csproj -v q --nologo
cd service/Api/tests/Module.UnitTests/bin/Debug/net10.0
./Module.UnitTests -class "Module.UnitTests.Ordering.Features.Admin.Orders.Get.ById.GetOrderByIdTests"
```
Expected: FAIL — `result.Value.Payments` empty (`.ContainSingle()` fails) because the navigations aren't loaded.

- [ ] **Step 3: Add the Includes**

In `GetOrderById.cs` (line 24-25), replace:

```csharp
            var entity = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .Include(x => x.Adjustments)
```

with:

```csharp
            var entity = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .Include(x => x.Adjustments)
                .Include(x => x.PaymentCaptures)
                .Include(x => x.Shipments)
                    .ThenInclude(s => s.ShippingMethod)
```

In `GetCustomerOrder.cs` (line 30-31), apply the same change (add `.Include(x => x.PaymentCaptures)` and `.Include(x => x.Shipments).ThenInclude(s => s.ShippingMethod)`).

- [ ] **Step 4: Run tests to verify they pass**

Run:
```bash
dotnet build service/Api/tests/Module.UnitTests/Module.UnitTests.csproj -v q --nologo
cd service/Api/tests/Module.UnitTests/bin/Debug/net10.0
./Module.UnitTests -class "Module.UnitTests.Ordering.Features.Admin.Orders.Get.ById.GetOrderByIdTests"
./Module.UnitTests -class "Module.UnitTests.Ordering.Features.Storefront.Orders.Get.ById.GetCustomerOrderTests"
```
Expected: both PASS (`Failed: 0`).

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Admin/Orders/Get/ById/GetOrderById.cs \
        service/Api/src/Module/Ordering/Features/Storefront/Orders/Get/ById/GetCustomerOrder.cs \
        service/Api/tests/Module.UnitTests/Ordering/Features/Admin/Orders/Get/ById/GetOrderByIdTests.cs \
        service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/Orders/Get/ById/GetCustomerOrderTests.cs
git commit -m "feat(ordering): load payment captures and shipments in get-order handlers"
```

---

### Task 3: Admin — types, API (remove `listShipments`), and tests

**Files:**
- Modify: `app/Admin/src/features/ordering/types/order.ts`
- Modify: `app/Admin/src/features/ordering/services/orderApi.ts`
- Modify: `app/Admin/src/features/ordering/__tests__/services/orderApi.spec.ts`

**Interfaces:**
- Consumes: backend `OrderDetailResponse` new fields (Tasks 1-2).
- Produces: `OrderDetail` TS type with `payments`, `shipments`, `timeline`, five timestamps, `lineItems`; `ShipmentSummary` supersedes `Shipment` for the detail panel; `OrderApi` no longer has `listShipments`.

- [ ] **Step 1: Extend the types**

In `types/order.ts`, add to the `OrderDetail` interface (after `modifiedAtUtc?`):

```ts
  paymentProcessingAtUtc?: string
  paymentCompletedAtUtc?: string
  paymentFailedAtUtc?: string
  shipmentShippedAtUtc?: string
  shipmentDeliveredAtUtc?: string
  lineItems: LineItem[]
  payments: PaymentCaptureSummary[]
  shipments: ShipmentSummary[]
  timeline: OrderTimelineEvent[]
```

Append three new interfaces at the end of the file:

```ts
export interface PaymentCaptureSummary {
  id: string
  number: string
  amount: number
  currency: string
  state: string
  paymentStatus: string | null
  providerKey: string
  paymentMethodId: string | null
  createdAtUtc: string
  completedAtUtc: string | null
  failedAtUtc: string | null
}

export interface ShipmentSummary {
  id: string
  orderId: string
  shippingMethodId: string
  shippingMethodName: string | null
  trackingNumber: string | null
  status: ShipmentStatus
  shippedAtUtc: string | null
  deliveredAtUtc: string | null
  estimatedDeliveryAtUtc: string | null
  createdAtUtc: string
}

export interface OrderTimelineEvent {
  type: string
  label: string
  occurredAtUtc: string | null
}
```

- [ ] **Step 2: Remove `listShipments`**

In `orderApi.ts`, delete the `listShipments` method (lines 106-108):

```ts
  static listShipments(orderId: string): Promise<Result<{ items: Shipment[] }>> {
    return get<Result<{ items: Shipment[] }>>(`/api/admin/shipping/shipments?orderId=${orderId}`)
  }
```

Keep `updateShipmentStatus` and the `Shipment`/`ShipmentStatus` imports (still used by `updateShipmentStatus`).

- [ ] **Step 3: Update the spec**

In `orderApi.spec.ts`, delete the `describe('OrderApi.listShipments', …)` block (lines 239-252).

- [ ] **Step 4: Run tests + lint**

Run:
```bash
cd app/Admin && pnpm run test:unit && pnpm run lint
```
Expected: pass (no failing tests). Fix any type errors from the new fields (e.g. `types/order.spec.ts` or `validations/order.spec.ts` if they snapshot `OrderDetail`).

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/ordering/types/order.ts \
        app/Admin/src/features/ordering/services/orderApi.ts \
        app/Admin/src/features/ordering/__tests__/services/orderApi.spec.ts
git commit -m "feat(admin): extend order types and remove listShipments API"
```

---

### Task 4: Admin — render timeline, payments, shipments from payload (management panel kept)

**Files:**
- Modify: `app/Admin/src/features/ordering/views/OrderDetail.vue`

**Interfaces:**
- Consumes: `OrderDetail.payments/shipments/timeline/lineItems` + five timestamps (Task 3); `OrderApi.updateShipmentStatus`.
- Produces: a timeline section, payment/shipment timestamps in Overview, payments tab and shipments panel sourced from the payload; the shipments panel keeps tracking/status management via `updateShipmentStatus` + `fetchOrder` refresh.

- [ ] **Step 1: Rewire the data sources**

In `<script setup>`:
1. Remove the `PaymentApi`, `toPaymentQueryParams`, `PaymentListItem` imports (lines 23, 26-27); import `ShipmentSummary` from `../types/order` alongside the existing import.
2. Delete the `payments`/`paymentsLoading`/`paymentsLoaded` refs and `loadPayments` (lines 44-46, 256-268).
3. Delete `shipments`/`shipmentsLoading`/`shipmentsLoaded` refs and `loadShipments` (lines 59-61, 108-121).
4. Replace them with computeds and a timeline helper:

```ts
// Shipments: Derive the editable shipment rows from the order detail payload.
const shipments = computed<ShipmentSummary[]>(() => order.value?.shipments ?? [])
// Payments: Derive the payment rows from the order detail payload.
const payments = computed(() => order.value?.payments ?? [])
// Timeline: Derive the status event list from the order detail payload.
const timeline = computed(() => order.value?.timeline ?? [])
```

5. `saveShipmentStatus`: keep the guard + `updateShipmentStatus` call; after success replace the reload with a single re-fetch:

```ts
async function saveShipmentStatus(shipment: ShipmentSummary) {
  // Guard: A tracking number is required to mark a shipment as Shipped.
  if (draftStatus.value[shipment.id] === 'Shipped' && !trackingInputs.value[shipment.id]?.trim()) {
    notify.error('Shipment', 'A tracking number is required to mark the shipment as Shipped.')
    return
  }
  savingShipmentId.value = shipment.id
  // Save: Persist the edited status and tracking number for the shipment.
  const result = await OrderApi.updateShipmentStatus(shipment.id, {
    status: draftStatus.value[shipment.id] ?? shipment.status,
    trackingNumber: trackingInputs.value[shipment.id],
  })
  savingShipmentId.value = null
  if (result.isSuccess) {
    notify.success('Shipment', `Shipment status updated to "${draftStatus.value[shipment.id]}".`)
    // Refresh: Re-fetch the order so shipments and timeline reflect the new status.
    await fetchOrder(orderId.value)
  } else {
    handleResult(result)
  }
}
```

6. Change `initShipmentDrafts(shipmentList: Shipment[])` param to `ShipmentSummary[]`, `canSaveShipment(shipment: Shipment)` to `ShipmentSummary`, `allowedShipmentTargets` stays. Add a watch to seed drafts when shipments arrive:

```ts
// Seed: Rebuild the per-row status/tracking drafts whenever the payload shipments change.
watch(shipments, (list) => initShipmentDrafts(list), { immediate: true })
```

7. Remove the `onMounted`/route-watch `loadShipments()` calls (lines 286, 293) and the `shipmentsLoaded` reset in the route watch (line 281); keep `loadOrder()` and `loadItems()`.

8. Remove the `watch(activeTab, …)` payment branch (line 272) so only the items tab lazy-loads:

```ts
watch(activeTab, (tab) => {
  if (tab === '1') loadItems()
})
```

- [ ] **Step 2: Update the template**

1. In the Overview card, add four timestamp blocks (Payment Processing, Payment Completed, Payment Failed, Shipped, Delivered) after the existing "Modified" block (line 401), using `formatDate` and the `—` fallback pattern already used:

```html
                  <div>
                    <div class="text-sm text-muted-color">Payment Processing</div>
                    <div class="font-medium">{{ order.paymentProcessingAtUtc ? formatDate(order.paymentProcessingAtUtc) : '—' }}</div>
                  </div>
                  <div>
                    <div class="text-sm text-muted-color">Payment Completed</div>
                    <div class="font-medium">{{ order.paymentCompletedAtUtc ? formatDate(order.paymentCompletedAtUtc) : '—' }}</div>
                  </div>
                  <div>
                    <div class="text-sm text-muted-color">Shipped</div>
                    <div class="font-medium">{{ order.shipmentShippedAtUtc ? formatDate(order.shipmentShippedAtUtc) : '—' }}</div>
                  </div>
                  <div>
                    <div class="text-sm text-muted-color">Delivered</div>
                    <div class="font-medium">{{ order.shipmentDeliveredAtUtc ? formatDate(order.shipmentDeliveredAtUtc) : '—' }}</div>
                  </div>
```

2. Add a Timeline section after the Shipments table (`</DataTable>` closing of shipments, before the closing `</div>` of the Overview card), reusing the PrimeVue `Timeline` component — import it: `import Timeline from 'primevue/timeline'`. Insert:

```html
                <!-- Section: Timeline — chronological status events derived from order timestamps -->
                <div class="mt-6">
                  <h3 class="font-semibold mb-2">Timeline</h3>
                  <Timeline :value="timeline" layout="vertical" align="left">
                    <template #opposite="{ item }">
                      <span class="text-xs text-muted-color">{{ item.occurredAtUtc ? formatDate(item.occurredAtUtc) : '—' }}</span>
                    </template>
                    <template #content="{ item }">
                      <span class="font-medium">{{ item.label }}</span>
                    </template>
                  </Timeline>
                </div>
```

3. In the Shipments table, change the "Shipping Method" column to show the name (fall back to the id) and keep the tracking input, status dropdown, shipped/delivered dates, and Save action exactly as-is (they already use `trackingInputs`/`draftStatus`):

```html
                    <Column header="Shipping Method">
                      <template #body="{ data }">{{ data.shippingMethodName ?? data.shippingMethodId }}</template>
                    </Column>
```

4. In the Payments tab, keep the table but it now binds the computed `payments`; add the payment number column and keep amount/state/paymentStatus. Replace the existing four columns with:

```html
                <DataTable :value="payments" scrollable data-key="id" striped-rows>
                  <Column field="number" header="Number" />
                  <Column field="amount" header="Amount">
                    <template #body="{ data }">{{ formatCurrency(data.amount, data.currency ?? 'USD') }}</template>
                  </Column>
                  <Column field="state" header="State">
                    <template #body="{ data }"><Tag :value="data.state" /></template>
                  </Column>
                  <Column field="paymentStatus" header="Payment Status">
                    <template #body="{ data }">{{ data.paymentStatus ?? '—' }}</template>
                  </Column>
                  <template #empty>No payments recorded.</template>
                </DataTable>
```

Remove the `:loading="paymentsLoading"` binding (payments are now synchronous from the payload).

- [ ] **Step 3: Run lint + tests**

Run:
```bash
cd app/Admin && pnpm run lint && pnpm run test:unit
```
Expected: pass. Follow `app/Admin/AGENTS.md` comment rules — every new `<template>` section carries a `<!-- Section: … -->` comment and non-obvious `<script setup>` blocks carry `// Label:` comments (as shown above).

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/ordering/views/OrderDetail.vue
git commit -m "feat(admin): render order timeline, payments, and shipments from the payload"
```

---

### Task 5: Storefront — types and zod validations

**Files:**
- Modify: `app/Store/src/features/ordering/types/order.ts`
- Modify: `app/Store/src/features/ordering/validations/order.ts`

**Interfaces:**
- Consumes: backend `OrderDetailResponse` new fields.
- Produces: `OrderDetail` TS type + `OrderDetailSchema` zod schema with the new fields.

- [ ] **Step 1: Extend the types**

In `app/Store/src/features/ordering/types/order.ts`, add to `OrderDetail` (after `lineItems`):

```ts
  paymentProcessingAtUtc: string | null
  paymentCompletedAtUtc: string | null
  paymentFailedAtUtc: string | null
  shipmentShippedAtUtc: string | null
  shipmentDeliveredAtUtc: string | null
  payments: PaymentCaptureSummary[]
  shipments: ShipmentSummary[]
  timeline: OrderTimelineEvent[]
```

Append:

```ts
export interface PaymentCaptureSummary {
  id: string
  number: string
  amount: number
  currency: string
  state: string
  paymentStatus: string | null
  providerKey: string
  paymentMethodId: string | null
  createdAtUtc: string
  completedAtUtc: string | null
  failedAtUtc: string | null
}

export type ShipmentStatus = 'Pending' | 'Ready' | 'Shipped' | 'Delivered' | 'Backorder' | 'Canceled'

export interface ShipmentSummary {
  id: string
  orderId: string
  shippingMethodId: string
  shippingMethodName: string | null
  trackingNumber: string | null
  status: ShipmentStatus
  shippedAtUtc: string | null
  deliveredAtUtc: string | null
  estimatedDeliveryAtUtc: string | null
  createdAtUtc: string
}

export interface OrderTimelineEvent {
  type: string
  label: string
  occurredAtUtc: string | null
}
```

- [ ] **Step 2: Extend the zod schema**

In `validations/order.ts`, add the three sub-schemas and fields. Add after `OrderLineItemSchema`:

```ts
export const PaymentCaptureSummarySchema = z.object({
  id: z.string(),
  number: z.string(),
  amount: z.number(),
  currency: z.string(),
  state: z.string(),
  paymentStatus: z.string().nullable(),
  providerKey: z.string(),
  paymentMethodId: z.string().nullable(),
  createdAtUtc: z.string(),
  completedAtUtc: z.string().nullable(),
  failedAtUtc: z.string().nullable(),
})

export const ShipmentStatusSchema = z.enum(['Pending', 'Ready', 'Shipped', 'Delivered', 'Backorder', 'Canceled'])

export const ShipmentSummarySchema = z.object({
  id: z.string(),
  orderId: z.string(),
  shippingMethodId: z.string(),
  shippingMethodName: z.string().nullable(),
  trackingNumber: z.string().nullable(),
  status: ShipmentStatusSchema,
  shippedAtUtc: z.string().nullable(),
  deliveredAtUtc: z.string().nullable(),
  estimatedDeliveryAtUtc: z.string().nullable(),
  createdAtUtc: z.string(),
})

export const OrderTimelineEventSchema = z.object({
  type: z.string(),
  label: z.string(),
  occurredAtUtc: z.string().nullable(),
})
```

In `OrderDetailSchema`, add (before `lineItems`):

```ts
  paymentProcessingAtUtc: z.string().nullable(),
  paymentCompletedAtUtc: z.string().nullable(),
  paymentFailedAtUtc: z.string().nullable(),
  shipmentShippedAtUtc: z.string().nullable(),
  shipmentDeliveredAtUtc: z.string().nullable(),
  payments: z.array(PaymentCaptureSummarySchema),
  shipments: z.array(ShipmentSummarySchema),
  timeline: z.array(OrderTimelineEventSchema),
```

- [ ] **Step 3: Run lint + tests**

Run:
```bash
cd app/Store && pnpm run lint && pnpm run test:unit
```
Expected: pass. Fix any `OrderDetailView.spec.ts` or validation-test fixtures that construct `OrderDetail` without the new fields (add empty arrays / nulls as needed).

- [ ] **Step 4: Commit**

```bash
git add app/Store/src/features/ordering/types/order.ts \
        app/Store/src/features/ordering/validations/order.ts
git commit -m "feat(store): extend order detail types and validation schema"
```

---

### Task 6: Storefront — render timeline, shipments, and payments

**Files:**
- Modify: `app/Store/src/features/ordering/views/OrderDetailView.vue`
- Modify: `app/Store/src/features/ordering/views/__tests__/OrderDetailView.spec.ts`

**Interfaces:**
- Consumes: `orders.currentOrder.timeline/payments/shipments` (Task 5).
- Produces: new persistent Timeline, Shipments, and Payments sections on the order detail page; Track dialog and summary remain.

- [ ] **Step 1: Add the sections**

In `OrderDetailView.vue`, add a `Shipments`, a `Payments`, and a `Timeline` section between the Line Items table and the Summary/Shipping grid (after the Line Items `</DataTable>`, before `<!-- Section: Summary & Shipping -->`). Use the imported `Timeline` component and `formatDateTimeUtc`/`formatCurrency` already in scope.

```html
        <!-- Section: Timeline — chronological status events from the order payload -->
        <Card class="mb-6">
          <template #title>Order Timeline</template>
          <template #content>
            <Timeline
              v-if="orders.currentOrder.timeline.length > 0"
              :value="orders.currentOrder.timeline"
              layout="vertical"
              align="left"
            >
              <template #opposite="{ item }">
                <span class="text-xs text-muted">{{ item.occurredAtUtc ? formatDateTimeUtc(item.occurredAtUtc) : '—' }}</span>
              </template>
              <template #content="{ item }">
                <span class="font-medium">{{ item.label }}</span>
              </template>
            </Timeline>
            <Message v-else severity="info" :closable="false">No timeline events available.</Message>
          </template>
        </Card>

        <!-- Section: Shipments — tracking number and status per shipment -->
        <Card class="mb-6">
          <template #title>Shipments</template>
          <template #content>
            <DataTable
              v-if="orders.currentOrder.shipments.length > 0"
              :value="orders.currentOrder.shipments"
              dataKey="id"
              tableStyle="min-width: 30rem"
            >
              <Column header="Carrier">
                <template #body="{ data }">{{ data.shippingMethodName ?? data.shippingMethodId }}</template>
              </Column>
              <Column header="Tracking Number">
                <template #body="{ data }">{{ data.trackingNumber || '—' }}</template>
              </Column>
              <Column header="Status">
                <template #body="{ data }"><Tag :value="data.status" /></template>
              </Column>
              <Column header="Shipped">
                <template #body="{ data }">{{ data.shippedAtUtc ? formatDateTimeUtc(data.shippedAtUtc) : '—' }}</template>
              </Column>
              <Column header="Delivered">
                <template #body="{ data }">{{ data.deliveredAtUtc ? formatDateTimeUtc(data.deliveredAtUtc) : '—' }}</template>
              </Column>
            </DataTable>
            <Message v-else severity="info" :closable="false">No shipments yet.</Message>
          </template>
        </Card>

        <!-- Section: Payments — recorded transactions and their states -->
        <Card class="mb-6">
          <template #title>Payments</template>
          <template #content>
            <DataTable
              v-if="orders.currentOrder.payments.length > 0"
              :value="orders.currentOrder.payments"
              dataKey="id"
              tableStyle="min-width: 30rem"
            >
              <Column header="Amount">
                <template #body="{ data }">{{ formatCurrency(data.amount) }}</template>
              </Column>
              <Column header="State">
                <template #body="{ data }"><Tag :value="data.state" /></template>
              </Column>
              <Column header="Payment Status">
                <template #body="{ data }">{{ data.paymentStatus ?? '—' }}</template>
              </Column>
              <Column header="Completed">
                <template #body="{ data }">{{ data.completedAtUtc ? formatDateTimeUtc(data.completedAtUtc) : '—' }}</template>
              </Column>
            </DataTable>
            <Message v-else severity="info" :closable="false">No payments recorded.</Message>
          </template>
        </Card>
```

Note: `Timeline`, `Message`, `Tag`, `DataTable`, `Column`, `Card` are auto-imported in this SPA (they are already used in this file). If `Timeline` is not auto-imported, add `import Timeline from 'primevue/timeline'`.

- [ ] **Step 2: Update the spec**

In `OrderDetailView.spec.ts`, extend any mocked `currentOrder` fixture with the new fields so the component does not throw on `orders.currentOrder.timeline.length`:

```ts
  payments: [],
  shipments: [],
  timeline: [],
  paymentProcessingAtUtc: null,
  paymentCompletedAtUtc: null,
  paymentFailedAtUtc: null,
  shipmentShippedAtUtc: null,
  shipmentDeliveredAtUtc: null,
```

Add assertions that the three new section titles render (`Order Timeline`, `Shipments`, `Payments`).

- [ ] **Step 3: Run lint + tests**

Run:
```bash
cd app/Store && pnpm run lint && pnpm run test:unit
```
Expected: pass. Follow `app/Store/AGENTS.md` comment rules for the new `<template>` sections.

- [ ] **Step 4: Commit**

```bash
git add app/Store/src/features/ordering/views/OrderDetailView.vue \
        app/Store/src/features/ordering/views/__tests__/OrderDetailView.spec.ts
git commit -m "feat(store): render order timeline, shipments, and payments"
```

---

### Verification (after all tasks)

```bash
dotnet build service/Api/src/Api/Api.csproj -v q --nologo
cd service/Api/tests/Module.UnitTests/bin/Debug/net10.0
./Module.UnitTests -class "Module.UnitTests.Ordering.Features.Admin.Shared.Mappings.OrderMappingTests"
./Module.UnitTests -class "Module.UnitTests.Ordering.Features.Admin.Orders.Get.ById.GetOrderByIdTests"
./Module.UnitTests -class "Module.UnitTests.Ordering.Features.Storefront.Orders.Get.ById.GetCustomerOrderTests"
cd app/Admin && pnpm run lint && pnpm run test:unit
cd app/Store && pnpm run lint && pnpm run test:unit
```

Expected: C# build 0 warnings/0 errors; backend classes all `Failed: 0`; both SPAs lint + test clean.

## Self-Review

- **Spec coverage:** §Backend §1-§2 → Task 1 (DTO + mapping + timeline) and Task 2 (handlers); §Admin → Task 3 (types/API) + Task 4 (view); §Storefront → Task 5 (types/validations) + Task 6 (view). Timeline event table → `BuildTimeline`. Audit doc already committed separately. All covered.
- **Placeholder scan:** no TBD/TODO; full code in every step.
- **Type consistency:** `PaymentCaptureSummary`/`ShipmentSummary`/`OrderTimelineEvent` field names match across C# records, Admin TS, Storefront TS, and zod. Backend `ShipmentSummary.TrackingNumber` is `string` (Shipment.TrackingNumber is non-null), mapped to TS `trackingNumber: string | null` for leniency. `ShippingMethodId` is `Guid` (non-null on Shipment) → TS `string`. Timeline `Type` values match the spec table.
- **Factory signatures:** the plan assumes `ShipmentMethod.Create(Guid orderId, Guid shippingMethodId)` and `PaymentCaptureMethod.Create(decimal amount, Guid paymentMethodId, Guid orderId)`; Task 1 Step 2 instructs the implementer to confirm against the factory files.
- **Regression safety:** existing `StripeGatewayCheckoutSessionTests`, `CreatePaymentIntentTests` etc. are untouched. Admin line-items management fetch is preserved. `PaymentApi.getPayments` retained (other caller).

## Execution Handoff

Plan complete and saved to `docs/superpowers/plans/2026-08-16-order-details-enrichment.md`. Two execution options:

1. **Subagent-Driven (recommended)** — dispatch a fresh subagent per task, review between tasks.
2. **Inline Execution** — execute tasks in this session with checkpoints.

Which approach?
