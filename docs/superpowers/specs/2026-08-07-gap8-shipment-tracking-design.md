# Gap 8: Shipment Tracking

## Summary

Add timestamp fields to Order entity for tracking state transitions. New storefront endpoint returns timestamps. Frontend shows enhanced timeline on order detail page. No carrier/tracking number fields (demo scope).

## Current State

- Order entity has `ShipmentState` (string) + `ShipmentTotal` (decimal)
- Admin endpoint `UpdateOrderShipmentState` exists
- No storefront-facing shipment/tracking endpoints
- `OrderDetailView.vue` shows basic timeline (Created → Approved → Completed)

## Design

### Backend: Order Entity Changes

**File:** `service/Api/src/Module/Ordering/Domain/Orders/Order.cs`

Add timestamp fields:
```csharp
// Order state timestamps
public DateTime? OrderApprovedAt { get; set; }
public DateTime? OrderCompletedAt { get; set; }
public DateTime? OrderCanceledAt { get; set; }

// Payment state timestamps
public DateTime? PaymentProcessingAt { get; set; }
public DateTime? PaymentCompletedAt { get; set; }
public DateTime? PaymentFailedAt { get; set; }

// Shipping state timestamps
public DateTime? ShipmentLabelCreatedAt { get; set; }
public DateTime? ShipmentInTransitAt { get; set; }
public DateTime? ShipmentDeliveredAt { get; set; }
public DateTime? ShipmentExceptionAt { get; set; }
```

### Backend: New Storefront Endpoint

**Feature:** `Features/Storefront/Orders/GetTracking/`

```
GET /api/storefront/orders/{id}/tracking
```

**Response:**
```json
{
  "orderId": "...",
  "orderCreatedAt": "2026-08-10T10:30:00Z",
  "orderApprovedAt": "2026-08-10T11:00:00Z",
  "orderCompletedAt": null,
  "orderCanceledAt": null,
  "paymentProcessingAt": "2026-08-10T10:31:00Z",
  "paymentCompletedAt": "2026-08-10T10:32:00Z",
  "paymentFailedAt": null,
  "shipmentLabelCreatedAt": "2026-08-11T09:15:00Z",
  "shipmentInTransitAt": "2026-08-11T14:30:00Z",
  "shipmentDeliveredAt": null,
  "shipmentExceptionAt": null
}
```

**Auth:** Must own the order.

### Backend: Migration

New EF Core migration adding timestamp columns to `orders` table. All nullable (existing orders have null values).

### Frontend: `OrderTrackingTimeline.vue`

**Location:** `app/Store/src/features/ordering/components/OrderTrackingTimeline.vue`

**Props:**
```ts
tracking: OrderTrackingResponse
```

**UI:**
```
┌─────────────────────────────────────────────┐
│ Order Timeline                               │
├─────────────────────────────────────────────┤
│                                             │
│  ● Order Placed        Aug 10, 10:30 AM    │
│  │                                          │
│  ● Payment Completed   Aug 10, 10:32 AM    │
│  │                                          │
│  ● Order Approved      Aug 10, 11:00 AM    │
│  │                                          │
│  ● Label Created       Aug 11, 09:15 AM    │
│  │                                          │
│  ● In Transit          Aug 11, 02:30 PM    │
│  │                                          │
│  ○ Estimated Delivery  Aug 15, 2026        │
│  │                                          │
│  ○ Delivered           —                   │
│                                             │
└─────────────────────────────────────────────┘

Legend: ● = completed  ○ = pending
```

### Frontend: OrderDetailView Changes

**File:** `app/Store/src/features/ordering/views/OrderDetailView.vue`

- Fetch tracking data on mount via new API endpoint
- Replace basic timeline with `OrderTrackingTimeline`
- Show tracking section only when `shipmentLabelCreatedAt` is not null

### Frontend: API Service

**File:** `app/Store/src/features/ordering/services/orderApi.ts`

Add:
```ts
getOrderTracking(orderId: string): Promise<OrderTrackingResponse>
```

## Files to Create/Modify

| File | Action |
|------|--------|
| `Module/Ordering/Domain/Orders/Order.cs` | MODIFY — add timestamp fields |
| `Module/Ordering/Features/Storefront/Orders/GetTracking/` | CREATE — new endpoint |
| `Migrations/` | CREATE — new migration |
| `features/ordering/components/OrderTrackingTimeline.vue` | CREATE |
| `features/ordering/views/OrderDetailView.vue` | MODIFY — use tracking timeline |
| `features/ordering/services/orderApi.ts` | MODIFY — add tracking API call |

## Acceptance Criteria

- [ ] New timestamp columns exist on orders table
- [ ] `GET /api/storefront/orders/{id}/tracking` returns timestamps
- [ ] Timeline shows completed/pending states with dates
- [ ] Only shows shipped-related items when shipment has started
- [ ] Timeline is responsive (works on mobile)
- [ ] Existing order detail functionality unaffected
