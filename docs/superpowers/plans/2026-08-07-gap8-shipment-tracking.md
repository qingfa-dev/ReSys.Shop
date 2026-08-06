# Implementation Plan: Gap 8 — Shipment Tracking

**Spec:** `docs/superpowers/specs/2026-08-07-gap8-shipment-tracking-design.md`
**Estimated effort:** Large (4-6 hours)
**Dependencies:** None

## Tasks

### Backend

#### T1: Add timestamp fields to Order entity
- [ ] Edit `service/Api/src/Module/Ordering/Domain/Orders/Order.cs`
- [ ] Add: `PaymentProcessingAt`, `PaymentCompletedAt`, `PaymentFailedAt`
- [ ] Add: `ShippedAt`, `DeliveredAt`, `DeliveryExceptionAt`
- [ ] Add: `EstimatedDeliveryAt`
- [ ] All nullable DateTime?

#### T2: Create EF Core migration
- [ ] Run `dotnet ef migrations add AddOrderTrackingTimestamps`
- [ ] Verify migration adds nullable timestamp columns to orders table

#### T3: Create GetTracking endpoint
- [ ] Create `Module/Ordering/Features/Storefront/Orders/GetTracking/GetOrderTracking.cs`
- [ ] Create `Module/Ordering/Features/Storefront/Orders/GetTracking/GetOrderTracking.Endpoint.cs`
- [ ] Create `Module/Ordering/Features/Storefront/Orders/GetTracking/GetOrderTracking.Response.cs`
- [ ] Route: `GET api/storefront/orders/{id}/tracking`
- [ ] Auth: must own the order
- [ ] Return all timestamps from Order entity

### Frontend

#### T4: Add tracking API call
- [ ] Edit `app/Store/src/features/ordering/services/orderApi.ts`
- [ ] Add `getOrderTracking(orderId: string)` function
- [ ] Add `OrderTrackingResponse` type

#### T5: Create OrderTrackingTimeline.vue
- [ ] Create `app/Store/src/features/ordering/components/OrderTrackingTimeline.vue`
- [ ] Props: `tracking: OrderTrackingResponse`
- [ ] Timeline showing completed/pending states with dates
- [ ] Legend: ● = completed, ○ = pending

#### T6: Wire into OrderDetailView
- [ ] Edit `app/Store/src/features/ordering/views/OrderDetailView.vue`
- [ ] Fetch tracking data on mount
- [ ] Replace basic timeline with OrderTrackingTimeline
- [ ] Show only when shipment has started

### T7: Verify
- [ ] New timestamp columns exist on orders table
- [ ] GET /api/storefront/orders/{id}/tracking returns timestamps
- [ ] Timeline shows completed/pending states
- [ ] Responsive on mobile

## Verification

```bash
cd service/Api && dotnet build && dotnet test
cd app/Store && pnpm run lint && pnpm run test:unit
```
