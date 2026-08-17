# Design: Order Details Enrichment — Timeline, Payments, Shipments, Status/Timestamps

- Date: 2026-08-16
- Status: Approved (derived timeline; embedded payments/shipments; audit doc)
- Scope: Ordering + Billing/Shipping response DTOs, Admin SPA, Storefront SPA

## Problem

The order-detail pages (Admin + Storefront) under-use the data that already
exists on the `Order` aggregate. `OrderDetailResponse` exposes only
`ApprovedAtUtc/CompletedAtUtc/CanceledAtUtc/CreatedAtUtc/ModifiedAtUtc`; the
payment/shipment timestamps, `PaymentCaptures`, and `Shipments` are excluded
(see `docs/codebase/REVIEW-order-timestamps-status-calculation.md`). The Admin
page has no timeline and double-fetches shipments/payments from separate
endpoints; the Storefront timeline lives only inside the Track dialog.

## Decision

1. Derive an order timeline from the existing timestamps (no persistence).
2. Embed `Payments`, `Shipments`, and the payment/shipment timestamps in
   `OrderDetailResponse` (shared by Admin `GetOrderById` and Storefront
   `GetCustomerOrder`).
3. Update both SPAs to render timeline, payments, shipments, and
   status/timestamps from the payload. The Admin shipments section remains a
   **management panel** (update tracking number + status).

## Backend Changes

### 1. `OrderDetailResponse` (`Ordering/Features/Admin/Shared/Models/Order.Model.cs:61-90`)

Add:
- `DateTimeOffset? PaymentProcessingAtUtc`, `PaymentCompletedAtUtc`, `PaymentFailedAtUtc`
- `DateTimeOffset? ShipmentShippedAtUtc`, `ShipmentDeliveredAtUtc`
- `List<PaymentCaptureSummary> Payments { get; init; } = [];`
- `List<ShipmentSummary> Shipments { get; init; } = [];`
- `List<OrderTimelineEvent> Timeline { get; init; } = [];`

### 2. New summary records

```csharp
public sealed record PaymentCaptureSummary
{
    public Guid Id { get; init; }
    public string Number { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public PaymentRecordState State { get; init; }
    public string? PaymentStatus { get; init; }
    public string? ProviderKey { get; init; }
    public Guid? PaymentMethodId { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? CompletedAtUtc { get; init; }
    public DateTimeOffset? FailedAtUtc { get; init; }
}

public sealed record ShipmentSummary
{
    public Guid Id { get; init; }
    public Guid OrderId { get; init; }
    public Guid? ShippingMethodId { get; init; }
    public string? ShippingMethodName { get; init; }
    public string? TrackingNumber { get; init; }
    public ShipmentStatus Status { get; init; }
    public DateTimeOffset? ShippedAtUtc { get; init; }
    public DateTimeOffset? DeliveredAtUtc { get; init; }
    public DateTimeOffset? EstimatedDeliveryAtUtc { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
}

public sealed record OrderTimelineEvent
{
    public string Type { get; init; } = string.Empty;   // created | placed | approved | payment_processing | payment_completed | payment_failed | shipped | delivered | canceled
    public string Label { get; init; } = string.Empty;
    public DateTimeOffset? OccurredAtUtc { get; init; }
}
```

`ShipmentSummary` deliberately mirrors the fields the Admin management panel
needs (id, orderId, shippingMethodId, trackingNumber, status, timestamps).

### 3. Timeline derivation (in the Ordering mapping)

Build from fixed (timestamp, type, label) pairs, skip nulls, sort ascending by
`OccurredAtUtc`:

| Timestamp | Type | Label |
|---|---|---|
| `CreatedAtUtc` | `created` | "Order created" |
| `CompletedAtUtc` | `placed` | "Order placed" |
| `ApprovedAtUtc` | `approved` | "Order approved" |
| `PaymentProcessingAtUtc` | `payment_processing` | "Payment processing" |
| `PaymentCompletedAtUtc` | `payment_completed` | "Payment completed" |
| `PaymentFailedAtUtc` | `payment_failed` | "Payment failed" |
| `ShipmentShippedAtUtc` | `shipped` | "Order shipped" |
| `ShipmentDeliveredAtUtc` | `delivered` | "Order delivered" |
| `CanceledAtUtc` | `canceled` | "Order canceled" |

### 4. Mapping (`Order.Mapping.cs` `MapToDetailCore`)

Map the five timestamps, `Payments` (from `PaymentCaptures`), `Shipments`
(from `Shipments` incl. `ShippingMethod.Name`), and the derived `Timeline`.

### 5. Handlers

`GetOrderById.cs` and `GetCustomerOrder.cs`: add
`.Include(x => x.PaymentCaptures)` and
`.Include(x => x.Shipments).ThenInclude(s => s.ShippingMethod)`.

## Frontend — Admin (`app/Admin`, `features/ordering/`)

- `types/order.ts`: extend `OrderDetail` with the five timestamps, `lineItems`
  (backend already returns), `payments: PaymentCaptureSummary[]`,
  `shipments: ShipmentSummary[]`, `timeline: OrderTimelineEvent[]`; add the
  three interfaces. The existing `Shipment` interface is superseded by
  `ShipmentSummary` (same fields).
- `views/OrderDetail.vue`:
  - **Timeline section** rendered from `order.timeline` (new).
  - Payment + shipment timestamps added to the Overview card.
  - **Shipments panel stays a management panel**: list from `order.shipments`,
    keep the tracking-number input + status dropdown + Save action
    (`OrderApi.updateShipmentStatus`), refresh via `fetchOrder` after save.
    Remove the `OrderApi.listShipments` call in the detail view.
  - **Payments tab** rendered from `order.payments`; remove the
    `paymentApi.getPayments` fetch in the detail view.
  - Line-items tab keeps its separate lazy fetch (powers add/edit/remove
    management actions).
- Follow `app/Admin/AGENTS.md` comment standard (label comments + section
  comments) for `.vue` edits.

## Frontend — Storefront (`app/Store`, `features/ordering/`)

- `types/order.ts` + `validations/order.ts` (zod): extend `OrderDetail` with
  the five timestamps, `payments`, `shipments`, `timeline`.
- `views/OrderDetailView.vue`:
  - **Timeline section** from `order.timeline` (persistent, not just the Track
    dialog).
  - **Shipments section**: tracking number + status per shipment.
  - **Payments section**: amount + state per capture.
  - Keep the Track dialog and existing summary card.

## Out of Scope

- Persisting `OrderHistory` (derived timeline chosen; audit doc notes it).
- Fixing `GetOrderTracking`'s unused shipment load / `EstimatedDeliveryAt`
  population (recorded in the audit doc only).
- Removing the Storefront Track dialog or the Admin line-items management fetch.
- `OrderApi.listShipments` is **removed** (YAGNI): after the detail view stops
  calling it, no caller remains. Its spec test is updated accordingly.
  `PaymentApi.getPayments` is kept — still used by `usePaymentList.ts`.

## Testing

- **Backend**: `Order.Mapping` timeline-derivation unit tests (null skipping +
  ascending order); `GetOrderById`/`GetCustomerOrder` handler tests asserting
  `Payments`/`Shipments`/`Timeline` populated from seeded navigations.
- **Admin SPA**: `orderApi.spec.ts` updated; `OrderDetail` component tests
  (where infra exists) assert timeline + payments/shipments render.
- **Storefront SPA**: `orderApi`/validation tests updated for the new fields.
