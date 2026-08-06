# Gap 8: Shipment Tracking Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add timestamp fields to Order entity for tracking state transitions. New storefront endpoint returns timestamps. Frontend shows enhanced timeline.

**Architecture:** Backend: new nullable timestamp columns on orders table + new `GET /api/storefront/orders/{id}/tracking` endpoint. Frontend: `OrderTrackingTimeline` component replacing basic timeline on OrderDetailView.

**Tech Stack:** .NET 10 EF Core, Carter, MediatR, Vue 3, PrimeVue Timeline

## Global Constraints

- Warnings-as-errors: `TreatWarningsAsErrors=true` in .csproj
- Vertical slice: each feature in `Features/{Admin|Storefront}/{Feature}/{Action}/`
- Result objects: all operations return `Result<T>`
- All storefront endpoints require `.RequireAuthorization()`
- Order entity already has: `CompletedAtUtc`, `CanceledAtUtc`, `ApprovedAtUtc`, `ShipmentState`

---

## File Structure

| File | Action | Purpose |
|------|--------|---------|
| `service/Api/src/Module/Ordering/Domain/Orders/Order.cs` | MODIFY | Add timestamp fields |
| `service/Api/src/Module/Ordering/Features/Storefront/Orders/GetTracking/GetOrderTracking.cs` | CREATE | Handler |
| `service/Api/src/Module/Ordering/Features/Storefront/Orders/GetTracking/GetOrderTracking.Endpoint.cs` | CREATE | Carter endpoint |
| `service/Api/src/Module/Ordering/Features/Storefront/Orders/GetTracking/GetOrderTracking.Response.cs` | CREATE | Response DTO |
| `service/Api/src/Migrations/` | CREATE | EF Core migration |
| `app/Store/src/features/ordering/components/OrderTrackingTimeline.vue` | CREATE | Timeline component |
| `app/Store/src/features/ordering/views/OrderDetailView.vue` | MODIFY | Use tracking timeline |
| `app/Store/src/features/ordering/services/orderApi.ts` | MODIFY | Add tracking API |

---

## Tasks

### Task 1: Add timestamp fields to Order entity

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.cs`

**Interfaces:**
- Consumes: None
- Produces: New nullable properties on Order entity

- [ ] **Step 1: Read Order.cs**

Read `service/Api/src/Module/Ordering/Domain/Orders/Order.cs` to see existing fields.

- [ ] **Step 2: Add timestamp fields**

After the existing `ApprovedAtUtc` property (line 40), add:

```csharp
public DateTime? PaymentProcessingAt { get; set; }
public DateTime? PaymentCompletedAt { get; set; }
public DateTime? PaymentFailedAt { get; set; }
public DateTime? ShippedAt { get; set; }
public DateTime? DeliveredAt { get; set; }
public DateTime? DeliveryExceptionAt { get; set; }
public DateTime? EstimatedDeliveryAt { get; set; }
```

- [ ] **Step 3: Build backend**

```bash
cd service/Api && dotnet build
```

Expected: PASS

### Task 2: Create EF Core migration

**Files:**
- Create: `service/Api/src/Migrations/YYYYMMDDHHMMSS_AddOrderTrackingTimestamps.cs`

**Interfaces:**
- Consumes: Order entity changes
- Produces: Database migration

- [ ] **Step 1: Create migration**

```bash
cd service/Api && dotnet ef migrations add AddOrderTrackingTimestamps --project src/Migrations --startup-project src/Api
```

- [ ] **Step 2: Verify migration**

Read the generated migration file. Verify it adds nullable `timestamp` columns for each new field.

- [ ] **Step 3: Build backend**

```bash
cd service/Api && dotnet build
```

Expected: PASS

### Task 3: Create GetOrderTracking endpoint

**Files:**
- Create: `service/Api/src/Module/Ordering/Features/Storefront/Orders/GetTracking/` (3 files)

**Interfaces:**
- Consumes: Order entity, user claims
- Produces: `Result<Response>` with all timestamps

- [ ] **Step 1: Create response DTO**

Create `GetOrderTracking.Response.cs`:

```csharp
namespace Module.Ordering.Features.Storefront.Orders.GetTracking;

public static partial class GetOrderTracking
{
    public sealed record Response
    {
        public Guid OrderId { get; init; }
        public DateTime OrderCreatedAt { get; init; }
        public DateTime? OrderApprovedAt { get; init; }
        public DateTime? OrderCompletedAt { get; init; }
        public DateTime? OrderCanceledAt { get; init; }
        public DateTime? PaymentProcessingAt { get; init; }
        public DateTime? PaymentCompletedAt { get; init; }
        public DateTime? PaymentFailedAt { get; init; }
        public DateTime? ShippedAt { get; init; }
        public DateTime? DeliveredAt { get; init; }
        public DateTime? DeliveryExceptionAt { get; init; }
        public DateTime? EstimatedDeliveryAt { get; init; }
    }
}
```

- [ ] **Step 2: Create handler**

Create `GetOrderTracking.cs`:

```csharp
namespace Module.Ordering.Features.Storefront.Orders.GetTracking;

public static partial class GetOrderTracking
{
    public sealed record Query(Guid OrderId) : IRequest<Result<Response>>;

    internal sealed class Handler(ApplicationDbContext db) : IRequestHandler<Query, Result<Response>>
    {
        public async ValueTask<Result<Response>> Handle(Query query, CancellationToken ct)
        {
            var order = await db.Orders
                .FirstOrDefaultAsync(o => o.Id == query.OrderId, ct);

            if (order is null)
                return Result<Response>.NotFound($"Order '{query.OrderId}' not found.");

            return new Response
            {
                OrderId = order.Id,
                OrderCreatedAt = order.CreatedAtUtc,
                OrderApprovedAt = order.ApprovedAtUtc,
                OrderCompletedAt = order.CompletedAtUtc,
                OrderCanceledAt = order.CanceledAtUtc,
                PaymentProcessingAt = order.PaymentProcessingAt,
                PaymentCompletedAt = order.PaymentCompletedAt,
                PaymentFailedAt = order.PaymentFailedAt,
                ShippedAt = order.ShippedAt,
                DeliveredAt = order.DeliveredAt,
                DeliveryExceptionAt = order.DeliveryExceptionAt,
                EstimatedDeliveryAt = order.EstimatedDeliveryAt,
            };
        }
    }
}
```

- [ ] **Step 3: Create endpoint**

Create `GetOrderTracking.Endpoint.cs`:

```csharp
namespace Module.Ordering.Features.Storefront.Orders.GetTracking;

public static partial class GetOrderTracking
{
    public static void MapEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet(OrderingFeature.Storefront.Orders.GetTracking.Route, async (
            Guid id, ISender sender) =>
        {
            var result = await sender.Send(new Query(id));
            return result.MatchCreated();
        })
        .WithName(nameof(GetOrderTracking))
        .WithTags(OrderingFeature.Tags.Orders)
        .RequireAuthorization();
    }
}
```

- [ ] **Step 4: Register endpoint**

Find `MapOrderingStorefront` and add `GetOrderTracking.MapEndpoint(app)`.

- [ ] **Step 5: Build backend**

```bash
cd service/Api && dotnet build
```

Expected: PASS

- [ ] **Step 6: Commit**

```bash
cd service/Api && git add src/Module/Ordering/Domain/Orders/Order.cs src/Migrations/ src/Module/Ordering/Features/Storefront/Orders/GetTracking/
git commit -m "feat(ordering): add order tracking timestamps and storefront endpoint"
```

### Task 4: Add tracking API call (frontend)

**Files:**
- Modify: `app/Store/src/features/ordering/services/orderApi.ts`

**Interfaces:**
- Consumes: `GET /api/storefront/orders/{id}/tracking`
- Produces: `getOrderTracking(orderId)` function, `OrderTrackingResponse` type

- [ ] **Step 1: Add response type**

```typescript
export interface OrderTrackingResponse {
  orderId: string
  orderCreatedAt: string
  orderApprovedAt: string | null
  orderCompletedAt: string | null
  orderCanceledAt: string | null
  paymentProcessingAt: string | null
  paymentCompletedAt: string | null
  paymentFailedAt: string | null
  shippedAt: string | null
  deliveredAt: string | null
  deliveryExceptionAt: string | null
  estimatedDeliveryAt: string | null
}
```

- [ ] **Step 2: Add API function**

```typescript
export async function getOrderTracking(orderId: string): Promise<Result<OrderTrackingResponse>> {
  return get(`api/storefront/orders/${orderId}/tracking`)
}
```

- [ ] **Step 3: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

### Task 5: Create OrderTrackingTimeline.vue

**Files:**
- Create: `app/Store/src/features/ordering/components/OrderTrackingTimeline.vue`

**Interfaces:**
- Consumes: `tracking: OrderTrackingResponse`
- Produces: No exports — presentational component

- [ ] **Step 1: Create component**

```vue
<script setup lang="ts">
import type { OrderTrackingResponse } from '../services/orderApi'

const props = defineProps<{ tracking: OrderTrackingResponse }>()

function fmt(date: string | null): string {
  if (!date) return '—'
  return new Date(date).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric', hour: '2-digit', minute: '2-digit' })
}

const steps = [
  { label: 'Order Placed', date: props.tracking.orderCreatedAt },
  { label: 'Payment Completed', date: props.tracking.paymentCompletedAt },
  { label: 'Order Approved', date: props.tracking.orderApprovedAt },
  { label: 'Shipped', date: props.tracking.shippedAt },
  { label: 'Estimated Delivery', date: props.tracking.estimatedDeliveryAt },
  { label: 'Delivered', date: props.tracking.deliveredAt },
].filter(s => s.date || s.label === 'Delivered')
</script>
<template>
  <!-- Section: Order Tracking Timeline -->
  <div class="space-y-4">
    <h3 class="text-lg font-semibold text-stone-900">Order Timeline</h3>
    <div class="relative ml-4">
      <div class="absolute left-0 top-0 bottom-0 w-0.5 bg-stone-200" />
      <div v-for="step in steps" :key="step.label" class="relative pl-8 pb-6 last:pb-0">
        <div
          class="absolute left-0 top-1 w-3 h-3 rounded-full -translate-x-1.5"
          :class="step.date ? 'bg-stone-900' : 'bg-stone-300 border-2 border-stone-400'"
        />
        <p class="text-sm font-medium" :class="step.date ? 'text-stone-900' : 'text-stone-400'">
          {{ step.label }}
        </p>
        <p class="text-xs text-stone-500">{{ fmt(step.date) }}</p>
      </div>
    </div>
  </div>
</template>
```

- [ ] **Step 2: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

### Task 6: Wire into OrderDetailView

**Files:**
- Modify: `app/Store/src/features/ordering/views/OrderDetailView.vue`

**Interfaces:**
- Consumes: `getOrderTracking()` from `orderApi.ts`
- Produces: Renders `OrderTrackingTimeline`

- [ ] **Step 1: Read OrderDetailView.vue**

Read `app/Store/src/features/ordering/views/OrderDetailView.vue`.

- [ ] **Step 2: Add import and state**

```typescript
import { getOrderTracking } from '../services/orderApi'
import type { OrderTrackingResponse } from '../services/orderApi'
import OrderTrackingTimeline from '../components/OrderTrackingTimeline.vue'

const tracking = ref<OrderTrackingResponse | null>(null)
```

- [ ] **Step 3: Fetch tracking on mount**

After order loads, add:

```typescript
const trackRes = await getOrderTracking(order.value.id)
if (trackRes.isSuccess) tracking.value = trackRes.value
```

- [ ] **Step 4: Render timeline in template**

After the order summary section, add:

```vue
<!-- Section: Order Tracking -->
<OrderTrackingTimeline v-if="tracking" :tracking="tracking" class="mt-8" />
```

- [ ] **Step 5: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 6: Run unit tests**

```bash
cd app/Store && pnpm run test:unit
```

Expected: PASS

- [ ] **Step 7: Commit**

```bash
cd app/Store && git add src/features/ordering/services/orderApi.ts src/features/ordering/components/OrderTrackingTimeline.vue src/features/ordering/views/OrderDetailView.vue
git commit -m "feat(ordering): add shipment tracking timeline to order detail"
```
