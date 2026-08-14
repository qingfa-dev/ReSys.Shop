---
goal: Update the Store and Admin SPA type models and zod schemas to typed string-literal unions for checkout/payment/shipment state (no bare `string`), matching the renamed backend enums.
version: 1.0
date_created: 2026-08-14
last_updated: 2026-08-14
owner: Store / Admin SPA
status: 'Planned'
tags: [refactor, spa, vue, typescript, zod, enum]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

The backend enums are renamed (`CheckoutState`, `OrderPaymentState`,
`OrderShipmentState`) but the SPAs still declare `CheckoutState = 'Address' |
'Delivery' | 'Payment' | 'Confirm' | 'Complete'` and `checkoutState: string` in
`cart.ts`, and `paymentState`/`shipmentState` as bare `string`. This plan aligns the
SPA unions and zod schemas with the backend names and types them everywhere.

**Spec:** `spec/spec-checkout-state-enum-alignment.md` §3.6, §4.3

## 1. Requirements & Constraints

- **REQ-001**: Store & Admin `CheckoutState` union = `'Address' | 'PickDeliveryMethod' | 'PickPaymentMethod' | 'Confirm' | 'Complete'`.
- **REQ-002**: Add `OrderPaymentState` (9 values) and `OrderShipmentState` (6 values) unions; type `paymentState`/`shipmentState` (no bare `string`).
- **REQ-003**: `CartResponse.checkoutState` typed `CheckoutState` (not `string`); zod uses `z.enum([...])`.
- **REQ-004**: Store `useCheckout.stepOf` maps `'PickPaymentMethod' → 3`, `'PickDeliveryMethod' → 2`.
- **CON-001**: SPA lint + unit tests pass with zero warnings (`pnpm run lint`, `pnpm run test:unit`).
- **GUD-001**: Single shared union + `z.enum`, no runtime TS `enum`.
- **GUD-002**: Comments follow the respective SPA AGENTS.md standard.

## 2. Implementation Steps

### Implementation Phase 1 — Store SPA

- GOAL-001: Type Store checkout/payment/shipment states.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Update Store `types/order.ts` + `types/cart.ts` unions. | | |
| TASK-002 | Update Store `validations/order.ts` + `validations/cart.ts` to `z.enum`. | | |
| TASK-003 | Update `useCheckout.ts` `stepOf` + step labels. | | |
| TASK-004 | Update Store test fixtures. | | |

#### TASK-001: Store types

`app/Store/src/features/ordering/types/order.ts`:

```ts
export type CheckoutState = 'Address' | 'PickDeliveryMethod' | 'PickPaymentMethod' | 'Confirm' | 'Complete'
export type OrderPaymentState = 'Completed' | 'Failed' | 'Void' | 'BalanceDue' | 'CreditOwed' | 'Paid' | 'Pending' | 'Checkout' | 'Invalid'
export type OrderShipmentState = 'Pending' | 'Delivered' | 'Partial' | 'Ready' | 'Backorder' | 'Canceled'
```

`OrderDetail.paymentState` → `OrderPaymentState | null`; `shipmentState` → `OrderShipmentState | null`.

`app/Store/src/features/ordering/types/cart.ts`: `checkoutState: CheckoutState` (import the union).

#### TASK-002: Store zod

`validations/order.ts`: `CheckoutStateSchema = z.enum(['Address','PickDeliveryMethod','PickPaymentMethod','Confirm','Complete'])`; `OrderPaymentStateSchema = z.enum([...9])`; `OrderShipmentStateSchema = z.enum([...6])`. Replace `paymentState: z.string().nullable()` / `shipmentState: z.string().nullable()` with the enum schemas. `validations/cart.ts`: `checkoutState: CheckoutStateSchema` (import from order.ts or inline). Re-export in `validations/index.ts`.

#### TASK-003: useCheckout

`app/Store/src/features/ordering/composables/useCheckout.ts` `stepOf`:

```ts
function stepOf(state: string | null): Step {
  switch (state) {
    case 'Address': return 1
    case 'PickDeliveryMethod': return 2
    case 'PickPaymentMethod': return 3
    case 'Confirm': return 4
    case 'Complete': return 5
    default: return 1
  }
}
```

#### TASK-004: Store fixtures

- `CheckoutView.spec.ts` — `checkoutState: 'Payment'`/`'Delivery'` → `'PickPaymentMethod'`/`'PickDeliveryMethod'` (lines 296,316,332,365,393,396,407,411,433,436,451,463,466).
- `CartView.spec.ts` + `CartDrawer.spec.ts` — lowercase `checkoutState: 'address'` → `'Address'` (CartView.spec.ts:120,134; CartDrawer.spec.ts:143,160).
- `OrderDetailView.spec.ts` — no change (only `checkoutState: 'Complete'`).

### Implementation Phase 2 — Admin SPA

- GOAL-002: Type Admin checkout/payment/shipment states.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | Update Admin `types/order.ts` unions + typed fields. | | |
| TASK-006 | Update `OrdersList.vue` `CHECKOUT_STATE_OPTIONS` + shipment options. | | |
| TASK-007 | Update Admin test fixtures. | | |

#### TASK-005: Admin types

`app/Admin/src/features/ordering/types/order.ts`:

```ts
export type CheckoutState = 'Address' | 'PickDeliveryMethod' | 'PickPaymentMethod' | 'Confirm' | 'Complete'
export type OrderPaymentState = 'Completed' | 'Failed' | 'Void' | 'BalanceDue' | 'CreditOwed' | 'Paid' | 'Pending' | 'Checkout' | 'Invalid'
export type ShipmentState = 'Pending' | 'Delivered' | 'Partial' | 'Ready' | 'Backorder' | 'Canceled'
```

> **Casing change:** the current Admin `ShipmentState` union is **lowercase** (`'pending' | 'delivered' | …` at `order.ts:140`), but the backend `OrderShipmentState` enum now serializes **PascalCase** via `JsonStringEnumConverter`. This plan flips the union + `SHIPMENT_STATE_OPTIONS` (`order.ts:142`) and the `SHIPMENT_SEVERITY` keys (`OrdersList.vue:40-47`) to PascalCase to match the wire contract.

Type `paymentState?: string` → `paymentState?: OrderPaymentState`; `shipmentState?: string` → `shipmentState?: ShipmentState`.

#### TASK-006: Views

`OrdersList.vue`: `CHECKOUT_STATE_OPTIONS = ['Address','PickDeliveryMethod','PickPaymentMethod','Confirm','Complete']`; update `SHIPMENT_SEVERITY` keys to PascalCase. `OrderDetail.vue` shipment dropdown consumes `SHIPMENT_STATE_OPTIONS` — verify it renders the PascalCase values.

#### TASK-007: Admin fixtures

Update `__tests__/types/order.spec.ts` (`checkoutState: 'Payment'` → `'PickPaymentMethod'`, line 25) and any `paymentState`/`shipmentState` fixtures to PascalCase union values.

## 3. Alternatives

- **ALT-001**: Runtime TS `enum`. Rejected — matches spec GUD-002 (union + zod, no runtime artifact).

## 4. Dependencies

- **DEP-001**: Backend enum rename (done).
- **DEP-002**: `feature-shipment-aggregate-1` — if `OrderShipmentState` → `OrderFulfillmentState`, update the SPA `OrderShipmentState` union to the derived values (`None|Pending|Partial|Shipped|Delivered|Canceled`) in the same pass.

## 5. Files

- **FILE-001**: `app/Store/src/features/ordering/types/order.ts` / `cart.ts` / `index.ts`.
- **FILE-002**: `app/Store/src/features/ordering/validations/order.ts` / `cart.ts` / `index.ts`.
- **FILE-003**: `app/Store/src/features/ordering/composables/useCheckout.ts`.
- **FILE-004**: `app/Admin/src/features/ordering/types/order.ts`.
- **FILE-005**: `app/Admin/src/features/ordering/views/OrdersList.vue`.
- **FILE-006**: affected `*.spec.ts` fixtures.

## 6. Testing

- **TEST-001**: `cd app/Store && pnpm run lint && pnpm run test:unit` green.
- **TEST-002**: `cd app/Admin && pnpm run lint && pnpm run test:unit` green.

## 7. Risks & Assumptions

- **ASSUMPTION-001**: `OrderFulfillmentState` rename (shipment plan) is applied first or in the same pass; otherwise the `OrderShipmentState` union values match the current backend enum.

## 8. Related Specifications / Further Reading

- [spec-checkout-state-enum-alignment.md](../spec/spec-checkout-state-enum-alignment.md) §3.6, §4.3
- [feature-shipment-aggregate-1.md](./feature-shipment-aggregate-1.md)
