---
title: Checkout, Payment & Fulfillment State Model — Enum Alignment & Shipment Aggregate
version: 2.2
date_created: 2026-08-14
last_updated: 2026-08-14
owner: Ordering / Billing / Shipping / Store & Admin SPAs
tags: [refactor, design, architecture, ordering, billing, shipping, checkout, payment, shipment, enum, dto, spa, migration]
---

# Introduction

This specification defines the target state model for the checkout → payment →
fulfillment pipeline. It corrects four related defects and supersedes
`plan/feature-payment-method-selection-1.md`, `plan/refactor-checkout-state-enum-alignment-1.md`,
and the earlier `spec/spec-checkout-state-enum-alignment.md` draft.

1. **No payment-method choice.** The storefront forces a single embedded Stripe
   card form and silently picks the first active payment method; there is no
   hosted Stripe Checkout and no cash-on-delivery path. `CreateOrderFromCart`
   hard-requires `payment.State == Completed`, so offline methods cannot place.
2. **`CheckoutState.Payment` is mislabeled.** That step means "pick a payment
   method" (Credit Card vs COD), not "process the payment". This already caused a
   bug: `AdvanceCheckoutState` stamps `PaymentProcessingAt` on method selection.
3. **Statuses cross boundaries as raw strings.** `TargetState`,
   `CartResponseBase.CheckoutState`, `GetCartForCheckoutResponse.State`,
   `Order.PaymentState`, `Order.ShipmentState`, `UpdateShipmentState.Request.ShipmentState`,
   and untyped `string` fields in both SPAs are all stringly-typed.
4. **Fulfillment is a free-floating status on the Order.** The `Shipping` module
   owns only `ShippingMethod`/`ShippingRate` (including a `TrackingUrl` with a
   `:tracking` placeholder) but has no `Shipment` entity; `Order.ShipmentState` is
   an admin-set enum with no tracking number, carrier/method snapshot, or link to
   the shipping information.

## 1. Purpose & Scope

### Purpose

1. Make the checkout state machine unambiguous: the payment step means "pick a
   payment method"; actual payment processing is tracked separately.
2. Let the customer choose a payment method (Stripe Checkout or Cash on Delivery)
   and auto-place card orders via webhook.
3. Eliminate stringly-typed statuses so the compiler (C#) and type checker
   (TypeScript) enforce valid states across modules, DTOs, and SPAs.
4. Model fulfillment as a first-class `Shipment` aggregate so the order's
   fulfillment status is grounded in real shipping information.

### Scope

- Rename `CheckoutState` (`Delivery → PickDeliveryMethod`, `Payment → PickPaymentMethod`; 5 states).
- Stripe Checkout Session + `cash_on_delivery` provider + webhook-driven auto-placement.
- Introduce typed enums for checkout, payment, and shipment status.
- Introduce the `Shipment` aggregate in Shipping with a derived
  `OrderFulfillmentState` cache on Order.
- Convert all status-bearing strings to enums in C# and to typed string-literal
  unions in the SPAs.
- Correct the entity configuration so enum↔string persistence is handled by EF
  Core value converters (no SQL backfill); drop the in-progress migrations and
  create new ones.

### Out of Scope

- Saved payment methods / SetupIntent flow.
- Stripe tax, shipping rates, multi-currency beyond the existing `Currency`.
- Carrier integration (label/rate webhooks) — future.
- Delivery-exception workflow (`DeliveryExceptionAt`) — future.
- Item-level shipment lines (split specific items) — deferred (P1).
- `app/legacy/` SPAs (Admin.V1, Storefront, ReSys.Admin) — deprecated.

### Audience & Assumptions

- Audience: Ordering/Billing/Shipping backend engineers and Store/Admin SPA engineers.
- Assumption: the API host registers `JsonStringEnumConverter` globally (`Program.cs:33`), so enums serialize as their member names.

### Current implementation status (verified against the working tree)

| Item | State |
|------|-------|
| `CheckoutState` rename (`PickDeliveryMethod`/`PickPaymentMethod`, 5 states) | ✅ done — no `PaymentSelected`/stale `Payment` remain |
| `OrderPaymentState` / `OrderShipmentState` / `PaymentTimelineState` enums | ✅ done |
| `TargetState` → `CheckoutState` (no `Enum.TryParse`) | ✅ done |
| `Order.PaymentState`/`ShipmentState` → enum? ; `OrderConstant.*State`/`CheckoutStep` removed | ✅ done |
| `GetCartForCheckoutResponse.State` / `CartResponseBase.CheckoutState` → enum | ✅ done (`CartResponseBase` still `string` — verify) |
| `PaymentForCheckoutResponse.IsPending` + `CreateOrderFromCart` `p.IsPending` | ✅ done |
| Admin `Order.Model.Response` typed enums | ✅ done |
| Payment-method selection (COD, Checkout Session, webhook) | ✅ done (prior plan) |
| Bidirectional value converters (read legacy, §4.6) | ❌ **not done** — still plain `.HasConversion<string>()` (fails on legacy values) |
| `Shipment` aggregate + `ShipmentStatus` + derived `OrderFulfillmentState` | ❌ not started (`OrderShipmentState` still the 6 admin-set values) |
| SPA typed unions (Store + Admin) | ❌ not started (`'Delivery' | 'Payment'` still present; `cart.ts` `checkoutState: string`) |
| Consolidated migration (drop in-progress, regenerate) | ❌ not done (in-progress migrations still on disk) |
| Webhook event store / outbox / business timestamps / refund invariants / correlation keys (§13) | ❌ not started |

This status maps to the phase plans:
`refactor-status-value-converters-1`, `feature-shipment-aggregate-1`,
`refactor-checkout-spa-enums-1`, `refactor-webhook-reliability-1`,
`refactor-payment-invariants-1`.

## 2. Definitions

| Term | Definition |
|------|------------|
| **CheckoutState** | The customer-facing checkout wizard step (`Order.CheckoutState`, enum). |
| **PickPaymentMethod** | The step where the customer chooses Credit Card vs COD. Formerly `Payment`. |
| **PaymentCapture** | Billing's per-attempt payment record (aggregate). |
| **PaymentRecordState** | Billing's `PaymentCapture` lifecycle enum (`Checkout → Processing → Pending/Completed/Failed/Void/…`). Unchanged. |
| **OrderPaymentState** | The Order aggregate's **derived** payment status (`Void`, `BalanceDue`, `CreditOwed`, `Paid`, …). Enum. |
| **PaymentTimelineState** | The timestamp-mirror event kind sent cross-module to `RecordOrderPaymentState` (`Completed`/`Failed`/`Processing`). Enum (was a static string class). |
| **Shipment** | A fulfillment record owned by the Shipping module (tracking number, method, status, timestamps). |
| **ShipmentStatus** | Per-shipment lifecycle enum (`Pending → Ready → Shipped → Delivered`; `Backorder`, `Canceled`). |
| **OrderFulfillmentState** | The Order aggregate's **derived** fulfillment status computed from its shipments. Enum (repurposes the old `OrderShipmentState`). |
| **Checkout Session** | Stripe hosted payment page; its `id` becomes the capture's `ResponseCode`. |
| **COD** | Cash on Delivery — the offline provider key `cash_on_delivery`. |
| **Status string** | Any `string` field constrained to a fixed set (the thing being removed). |
| **Value converter** | An EF Core `ValueConverter<enum,string>` that maps enum members to/from stored strings. Reads both legacy and canonical names; writes canonical names only. |

## 3. Requirements, Constraints & Guidelines

### 3.1 Checkout state machine

- **SM-001**: `CheckoutState` contains exactly `Address, PickDeliveryMethod, PickPaymentMethod, Confirm, Complete`; the spurious 6th `Payment` member is removed.
- **SM-002**: Every C# reference uses `PickDeliveryMethod`/`PickPaymentMethod`; `PaymentSelected` and stale `Payment` are removed everywhere (domain, handlers, Billing call sites, tests).
- **SM-003**: `AdvanceCheckoutState` must **not** call `MarkPaymentProcessing`; selection is not processing.
- **SM-004**: Dead `OrderConstant.CheckoutStep` string helpers (`CurrentCheckoutStep`, `ResolvedCheckoutSteps`, `HasCheckoutStep`, `PassedCheckoutStep`, `CanGoToState`, `CheckoutStepIndex`) are deleted.

### 3.2 Payment method selection & hosted checkout

- **PM-001**: `GatewayConstants.Providers.CashOnDelivery = "cash_on_delivery"`; seed a "Cash on Delivery" `PaymentMethod` (`displayOn: Frontend`, `autoCapture: false`).
- **PM-002**: `ListPaymentMethods` filters `DisplayOn != Backend`.
- **PM-003**: Add nullable `CheckoutUrl` to `PaymentCapture`; `CreateCheckoutSessionAsync` on the gateway abstraction returns `Authorization = session.Id` + `CheckoutUrl`.
- **PM-004**: `CreatePaymentIntent` branches on provider key: COD → `Process()`+`Pend()` (Pending, no `ResponseCode`); Stripe → Checkout Session → `ResponseCode = session.Id`, `CheckoutUrl`, `Process()`.
- **PM-005**: Webhook `checkout.session.completed` → `Complete()` + send `CompleteCheckoutForPayment`; `checkout.session.expired` → `Void()` + release stock.
- **PM-006**: The SPA "payment id" is `PaymentCapture.Id`; `GetPaymentForCheckout`/`MarkPaymentPaid` match on `Id` (fallback `ResponseCode`).
- **PM-007**: `PaymentForCheckoutResponse { Amount, IsCompleted, IsOffline, IsPending, CompletedAtUtc }`; placement allows `IsCompleted || (IsPending && IsOffline)`; offline skips `MarkPaymentPaid`.
- **PM-008**: No new `PaymentRecordState` values.

### 3.3 Processing-timestamp semantics

- **TS-001**: `PaymentProcessingAt` reflects when the payment actually enters processing, not when a method is picked.
- **TS-002**: `CreatePaymentIntent` notifies Ordering of `PaymentTimelineState.Processing` at the point a gateway payment enters `Processing` (via `RecordOrderPaymentStateCommand`).

### 3.4 String → enum (C#)

- **EN-001**: `AdvanceCheckoutStateCommand.TargetState` and `RegressCheckoutStateCommand.TargetState` become `CheckoutState`; handlers drop `Enum.TryParse`.
- **EN-002**: `CartResponseBase.CheckoutState` and `GetCartForCheckoutResponse.State` become `CheckoutState`; mapping drops `.ToString()`.
- **EN-003**: `Order.PaymentState`/`Order.ShipmentState` become `OrderPaymentState?`/`OrderFulfillmentState?`.
- **EN-004**: `RecordOrderPaymentStateCommand.PaymentState` becomes `PaymentTimelineState`; the `OrderPaymentState` static string class is removed.
- **EN-005**: Billing sends enums, not strings (`RegressCheckoutStateCommand.TargetState`, `PaymentTimelineState`, `AdvanceCheckoutStateCommand.TargetState`).
- **EN-006**: `PaymentForCheckoutResponse.State` string is replaced by `bool IsPending`; `CreateOrderFromCart` uses `p.IsPending`.
- **EN-007**: Admin `OrderDetailResponse`/`OrderListItemResponse` `PaymentState`/`ShipmentState` become `OrderPaymentState?`/`OrderFulfillmentState?`.

### 3.5 Shipment aggregate (fulfillment)

- **SH-001**: New `Shipment` aggregate in Shipping (see §4.2); references `OrderId` by Guid only (no navigation).
- **SH-002**: `ShipmentStatus { Pending, Ready, Shipped, Delivered, Backorder, Canceled }`.
- **SH-003**: On order placement, Shipping auto-creates one `Pending` shipment (Ordering sends `CreateShipmentCommand`).
- **SH-004**: Admin advances shipment status / sets `TrackingNumber` (required at `Shipped`) via Shipping endpoints.
- **SH-005**: `Shipment.ShippedAtUtc`/`DeliveredAtUtc` are the authoritative timestamps; `Order.ShippedAt`/`DeliveredAt` become derived mirrors.
- **SH-006**: Shipping recomputes the derived `OrderFulfillmentState` across the order's shipments and sends `RecordOrderShipmentStateCommand { OrderId, FulfillmentState, ShippedAtUtc?, DeliveredAtUtc? }`; Ordering writes the cache.
- **SH-007**: Ordering's `UpdateOrderShipmentState` admin endpoint is removed; status is no longer set directly on the Order.

### 3.6 SPA models (typed string-literal unions)

- **SP-001**: Store & Admin `CheckoutState` unions become `'Address' | 'PickDeliveryMethod' | 'PickPaymentMethod' | 'Confirm' | 'Complete'`.
- **SP-002**: Add `OrderPaymentState`, `OrderFulfillmentState`, and `ShipmentStatus` unions; type `checkoutState`/`paymentState`/`shipmentState` (no bare `string`).
- **SP-003**: Zod schemas use `z.enum([...])`.
- **SP-004**: Store `useCheckout.stepOf` maps `'PickPaymentMethod' → 3`, `'PickDeliveryMethod' → 2`; step 3 renders a method list (card → redirect, COD → place order); `/checkout/return` polls `GetPaymentStatus` until `IsCompleted`.
- **SP-005**: Admin `CHECKOUT_STATE_OPTIONS`/`SHIPMENT_STATE_OPTIONS` reflect PascalCase; order detail renders shipment status + tracking.

### 3.7 Persistence & migration

- **MI-001**: `OrderConfiguration` replaces the plain `string`/default conversions with **bidirectional value converters** (`ValueConverter<enum,string>`) for `CheckoutState`, `PaymentState`, and `ShipmentState`. Each converter: **write** emits the canonical enum name; **read** accepts both the canonical name and every legacy name (see §4.6) and maps it to the enum member. No SQL `UPDATE` backfill.
- **MI-002**: `Shipment` gets its own EF configuration in Shipping (`ShipmentStatus` via `.HasConversion<string>()`, indexes on `OrderId`, `Status`).
- **MI-003**: Drop the in-progress migrations (`RemoveTaxCategoryIdFromShippingMethod`, `AddPaymentBusinessTimestamps`) and reset the model snapshot to the committed baseline (requires explicit approval — AGENTS.md rule 6).
- **MI-004**: Generate new migrations from the corrected model: the `Shipment` table, the payment business-timestamp columns previously carried by the dropped migration, and any column changes induced by the value converters. Because the converters preserve the text column type, the status columns require no data rewrite — legacy values are read-transparent.

### 3.8 Delivery breakdown (phases & priorities)

**P0 — must have (core correctness)**

| ID | Item |
|----|------|
| P0-1 | Rename `CheckoutState` (5 states) and fix all C# references (`PaymentSelected`/`Payment` gone). |
| P0-2 | Remove `MarkPaymentProcessing` from `AdvanceCheckoutState`; stamp `PaymentTimelineState.Processing` on gateway `PaymentCapture.Process()`. |
| P0-3 | Convert all C# status strings to enums (commands, DTOs, domain properties, admin responses). |
| P0-4 | Payment-method selection: `CashOnDelivery` provider + seed, `CheckoutUrl`, `CreateCheckoutSessionAsync`, `CreatePaymentIntent` branching, webhook complete/expire, `IsPending`/`IsOffline` placement gating. |
| P0-5 | `Shipment` aggregate + `ShipmentStatus` + auto-create on placement + `RecordOrderShipmentStateCommand` derived-cache sync. |
| P0-6 | Entity configuration value converters (§4.6) + drop in-progress migrations + regenerate migrations. |
| P0-7 | SPA typed unions (Store + Admin) + zod `z.enum` + `useCheckout.stepOf` + method-list step 3 + `/checkout/return`. |

**P1 — nice to have (fast follow)**

| ID | Item |
|----|------|
| P1-1 | Item-level shipment lines (split specific items across shipments). |
| P1-2 | Tracking URL rendering (`ShippingMethod.TrackingUrl` `:tracking` substitution). |
| P1-3 | `Shipment.EstimatedDeliveryAtUtc` + estimated-delivery display. |

**P2 — future**

| ID | Item |
|----|------|
| P2-1 | Carrier integration (label/rate webhooks). |
| P2-2 | Delivery-exception workflow (`DeliveryExceptionAt`). |

**Suggested implementation order**

1. Domain enums + state machine + timestamp fix (P0-1, P0-2).
2. String→enum wiring across Ordering + Billing (P0-3).
3. Payment-method selection + webhook (P0-4).
4. `Shipment` aggregate + derived cache (P0-5).
5. Value converters + migration cleanup (P0-6).
6. SPA unions + checkout method list (P0-7).

### Constraints

- **CON-001**: `TreatWarningsAsErrors=true` — zero-warning build.
- **CON-002**: Domain operations return `Result`/`Result<T>`; exceptions only for unrecoverable infra failures.
- **CON-003**: Modules communicate only via MediatR `ISender`; no new cross-module assembly references.
- **CON-004**: Vertical-slice feature files (`static partial class` across Handler/Request/Response/Endpoint/Validator); subdirectory is `Storefront`.
- **CON-005**: SPA comments follow `app/Store/AGENTS.md` / `app/Admin/AGENTS.md`; no em dashes.
- **CON-006**: No destructive git (`stash`/`restore`/`revert`/`checkout --`/`reset --hard`) without explicit human "yes".

### Guidelines

- **GUD-001**: Keep state-machine logic in `Order.Method.Checkout.cs`; no new service abstractions.
- **GUD-002**: SPA uses a single shared string-literal union + `z.enum`, not a runtime TS `enum`.

### Patterns

- **PAT-001**: Enum→string persistence uses an EF Core `ValueConverter<enum,string>` that writes the canonical member name and reads both legacy and canonical names, matching `OrderStatus`/`PaymentRecordState` conventions.
- **PAT-002**: Cross-module state mirroring uses a command sent via `ISender` (`RecordOrderPaymentState`, `RecordOrderShipmentState`, `CompleteCheckoutForPayment`, `CreateShipment`).
- **PAT-003**: Legacy↔canonical mappings are centralized in the value converters (single source of truth), not scattered across SQL migration strings.

## 4. Interfaces & Data Contracts

### 4.1 Enum definitions (target)

```csharp
// Ordering — Order.Enumerate.cs
public enum CheckoutState { Address, PickDeliveryMethod, PickPaymentMethod, Confirm, Complete }

public enum OrderPaymentState
{ Completed, Failed, Void, BalanceDue, CreditOwed, Paid, Pending, Checkout, Invalid }

// Ordering — RecordOrderPaymentState feature (was `public static class OrderPaymentState`)
public enum PaymentTimelineState { Completed, Failed, Processing }

// Ordering — derived fulfillment status (repurposes the old OrderShipmentState)
public enum OrderFulfillmentState { None, Pending, Partial, Shipped, Delivered, Canceled }

// Shipping — per-shipment lifecycle
public enum ShipmentStatus { Pending, Ready, Shipped, Delivered, Backorder, Canceled }
```

`PaymentRecordState` (Billing) is unchanged:
`Checkout, Processing, Pending, Completed, Failed, Void, Disputed, Invalid`.

### 4.2 Shipment entity (Shipping)

| Field | Type | Notes |
|-------|------|-------|
| `Id` | Guid | PK |
| `OrderId` | Guid | cross-module reference by ID only (no navigation) |
| `ShippingMethodId` | Guid | FK to `ShippingMethod` |
| `TrackingNumber` | string? | set when marking `Shipped` |
| `Status` | `ShipmentStatus` | lifecycle |
| `ShippedAtUtc` | DateTimeOffset? | authoritative |
| `DeliveredAtUtc` | DateTimeOffset? | authoritative |
| `EstimatedDeliveryAtUtc` | DateTimeOffset? | future (P1) |
| auditing | — | `CreatedAtUtc`, `CreatedBy`, … |

### 4.3 Wire format

Enums serialize as their **name** (global `JsonStringEnumConverter`). Changes:

| Field | Old wire value | New wire value |
|-------|----------------|----------------|
| `checkoutState` | `"Delivery"`, `"Payment"` | `"PickDeliveryMethod"`, `"PickPaymentMethod"` |
| `paymentState` | `"balance_due"`, `"credit_owed"`, `"void"`, `"paid"`, … | `"BalanceDue"`, `"CreditOwed"`, `"Void"`, `"Paid"`, … |
| `shipmentState` (order) | `"pending"`, `"ready"`, `"delivered"`, … | `"None"`, `"Pending"`, `"Partial"`, `"Shipped"`, `"Delivered"`, `"Canceled"` |
| `Shipment.status` | — (new) | `"Pending"`, `"Ready"`, `"Shipped"`, `"Delivered"`, `"Backorder"`, `"Canceled"` |

### 4.4 Command / DTO shape changes

| Type | Before | After |
|------|--------|-------|
| `AdvanceCheckoutStateCommand.TargetState` | `string` | `CheckoutState` |
| `RegressCheckoutStateCommand.TargetState` | `string` | `CheckoutState` |
| `CartResponseBase.CheckoutState` | `string` | `CheckoutState` |
| `GetCartForCheckoutResponse.State` | `string` | `CheckoutState` |
| `Order.PaymentState` | `string?` | `OrderPaymentState?` |
| `Order.ShipmentState` | `string?` | `OrderFulfillmentState?` |
| `RecordOrderPaymentStateCommand.PaymentState` | `string` | `PaymentTimelineState` |
| `PaymentForCheckoutResponse.State` | `string` | `bool IsPending` |
| Admin `OrderDetailResponse/OrderListItemResponse.PaymentState/ShipmentState` | `string?` | `OrderPaymentState?`/`OrderFulfillmentState?` |

### 4.5 API surface

- **New (Shipping, admin):** create shipment, set tracking number, advance/cancel/backorder shipment, list shipments for an order.
- **Removed (Ordering):** `UpdateOrderShipmentState` admin endpoint.
- **Existing (Billing/Storefront):** `create-intent`, `GetPaymentStatus`, webhook routes unchanged.

### 4.6 Entity configuration — value converters (replaces SQL backfill)

No data `UPDATE` is performed. Legacy persisted strings are mapped on read by
bidirectional value converters; writes emit the canonical enum name.

| Enum | Canonical (write) | Legacy names (read) |
|------|--------------------|---------------------|
| `CheckoutState` | `Address`, `PickDeliveryMethod`, `PickPaymentMethod`, `Confirm`, `Complete` | `"Delivery"` → `PickDeliveryMethod`; `"Payment"` → `PickPaymentMethod` |
| `OrderPaymentState` | `Completed`, `Failed`, `Void`, `BalanceDue`, `CreditOwed`, `Paid`, `Pending`, `Checkout`, `Invalid` | lowercase snake_case: `"completed"`, `"failed"`, `"void"`, `"balance_due"`, `"credit_owed"`, `"paid"`, `"pending"`, `"checkout"`, `"invalid"` |
| `OrderFulfillmentState` | `None`, `Pending`, `Partial`, `Shipped`, `Delivered`, `Canceled` | `"pending"`→`Pending`, `"ready"`→`Pending`, `"backorder"`→`Pending`, `"partial"`→`Partial`, `"delivered"`→`Delivered`, `"canceled"`→`Canceled` |
| `ShipmentStatus` | `Pending`, `Ready`, `Shipped`, `Delivered`, `Backorder`, `Canceled` | none (new table) |

```csharp
// OrderConfiguration.cs — example of the read-legacy/write-canonical converter
builder.Property(x => x.CheckoutState)
    .HasConversion(
        v => v.ToString(),                     // write: canonical enum name
        v => v switch {                        // read: legacy + canonical
            "Delivery" => CheckoutState.PickDeliveryMethod,
            "Payment"  => CheckoutState.PickPaymentMethod,
            _ => Enum.Parse<CheckoutState>(v)  // canonical (and any future) names
        });

builder.Property(x => x.PaymentState)
    .HasConversion(
        v => v.ToString(),
        v => v switch {
            "completed"   => OrderPaymentState.Completed,
            "failed"      => OrderPaymentState.Failed,
            "void"        => OrderPaymentState.Void,
            "balance_due" => OrderPaymentState.BalanceDue,
            "credit_owed" => OrderPaymentState.CreditOwed,
            "paid"        => OrderPaymentState.Paid,
            "pending"     => OrderPaymentState.Pending,
            "checkout"    => OrderPaymentState.Checkout,
            "invalid"     => OrderPaymentState.Invalid,
            _ => Enum.Parse<OrderPaymentState>(v)
        });
```

> The status columns remain text; only the read/write mapping changes, so legacy
> rows stay valid indefinitely and new writes store canonical names (data migrates
> lazily on the next write). Exact table/column casing is confirmed against the
> generated migration and `ApplicationDbContextModelSnapshot`.

### 4.7 Data flow & lifecycle

**Checkout lifecycle** (`Order.CheckoutState`)

```
[Address] ──▶ [PickDeliveryMethod] ──▶ [PickPaymentMethod] ──▶ [Confirm] ──▶ [Complete]
  enter addr    choose shipping          choose payment           review        placed
                method / carrier         method (Card | COD)
       └──────────── regress (amount-affecting edit) ──▶ back to PickDeliveryMethod
```

**Payment lifecycle** (`PaymentCapture.State` — Billing)

```
               ┌── COD:  [Checkout] ──▶ [Processing] ──▶ [Pending]  (cash collected later)
create-intent ─┤
               └── Card: [Checkout] ──▶ [Processing] ──▶ [Completed]  (webhook)
                                      └─▶ [Failed] / [Void] / [Disputed] / [Invalid]
```

**Shipment lifecycle** (`Shipment.Status` — Shipping)

```
[Pending] ──▶ [Ready] ──▶ [Shipped] ──▶ [Delivered]
    │            │            │
    │            └────────────┴──▶ [Canceled]
    └──▶ [Backorder] ──▶ [Ready]   (when stock arrives)
```

**Order fulfillment derivation** (`Order.FulfillmentState` — computed by Shipping)

```
none ──▶ pending ──▶ partial ──▶ shipped ──▶ delivered
                       └──────▶ canceled
```

**End-to-end flow** (combining payment-method selection, placement, and fulfillment)

```
pick delivery method ─▶ pick payment method
                          ├─ COD:  create-intent → PaymentCapture Pending → Place Order (explicit)
                          └─ Card: create-intent → Checkout Session → redirect to Stripe
                                    → pay → success_url → /checkout/return (poll GetPaymentStatus)
                                    → webhook checkout.session.completed
                                    → PaymentCapture Completed → CompleteCheckoutForPayment → auto-place

placement ─▶ auto-create Pending Shipment (Shipping)
fulfillment ─▶ admin advances Shipment (Ready → Shipped → Delivered / Backorder / Cancel)
            └─▶ Shipping recomputes → RecordOrderShipmentStateCommand → Order caches FulfillmentState
```

## 5. Acceptance Criteria

- **AC-001**: Given a checkout cart, when it advances to the payment step, then `CheckoutState == PickPaymentMethod` and `PaymentProcessingAt` remains unchanged.
- **AC-002**: The system shall compile `CheckoutState` with exactly five members and zero warnings.
- **AC-003**: No `PaymentSelected`, stale `CheckoutState.Payment`, or `TargetState = "Payment"`/`"Delivery"` string literal remains in `service/Api`.
- **AC-004**: `GetCartForCheckoutResponse.State`, `CartResponseBase.CheckoutState`, `AdvanceCheckoutStateCommand.TargetState`, and `RegressCheckoutStateCommand.TargetState` are typed enums; no `Enum.TryParse<CheckoutState>` remains.
- **AC-005**: `Order.PaymentState`/`Order.ShipmentState` are `OrderPaymentState?`/`OrderFulfillmentState?`; `OrderConstant.PaymentState`/`ShipmentState` string classes are removed.
- **AC-006**: `RecordOrderPaymentStateCommand.PaymentState` is `PaymentTimelineState`; the `OrderPaymentState` static string class is gone.
- **AC-007**: Selecting COD creates a `Pending` capture with no gateway call; selecting Card creates a Checkout Session and maps `CheckoutUrl`/`ResponseCode`.
- **AC-008**: `checkout.session.completed` completes the payment and auto-places the order idempotently; `checkout.session.expired` voids and releases stock.
- **AC-009**: Placement allows `IsCompleted || (IsPending && IsOffline)`; offline skips `MarkPaymentPaid`.
- **AC-010**: A `Pending` `Shipment` is auto-created on placement; admin can advance it `Pending → Ready → Shipped → Delivered` (tracking number at `Shipped`), cancel, and backorder it.
- **AC-011**: Shipping sends `RecordOrderShipmentStateCommand` on every shipment change; Order caches the derived `OrderFulfillmentState` and `ShippedAt`/`DeliveredAt` mirrors.
- **AC-012**: Both SPAs define and use the typed unions; zod uses `z.enum`.
- **AC-013**: The status columns use bidirectional value converters (write canonical, read legacy) with no SQL backfill; the in-progress migrations are dropped and new migrations are generated from the corrected model; `dotnet test service/Api/tests/Module.UnitTests` passes.
- **AC-014**: `pnpm run lint && pnpm run test:unit` pass in `app/Store` and `app/Admin`; `bash scripts/check-feature-conventions.sh` passes; cross-module baseline unchanged.

## 6. Test Automation Strategy

- **Test Levels**: Unit (C# xUnit + FluentAssertions + Moq; TS Vitest), Integration (placement/auto-shipment against seeded PostgreSQL), value-converter legacy-read smoke.
- **Frameworks**: C# xUnit/FluentAssertions/Moq (`Module.UnitTests`); SPA Vitest + Vue Test Utils + Pinia testing + Zod.
- **Test Data Management**: in-memory EF provider for handlers; seeded PostgreSQL (Aspire/Testcontainers) for migration/integration.
- **CI/CD Integration**: `Module.UnitTests` + `pnpm run test:unit` already in `.github/workflows/ci.yml`; value-converter smoke joins the integration job when Testcontainers automation lands.
- **Coverage Requirements**: every renamed state and string→enum conversion has at least one asserting test (opt-in `CollectCoverage=true`).
- **Performance Testing**: not applicable (type/contract refactor; no hot-path change).

### Targeted tests

- `Order.Method.Tests.cs` — transitions with new names; same-state idempotency; no `MarkPaymentProcessing` on the pick step; `RegressCheckoutIfAmountChanged` uses `PickPaymentMethod`.
- `RecordOrderPaymentStateTests` — `PaymentTimelineState` → `MarkPayment*`.
- `CreatePaymentIntentTests` / `ProcessStripeWebhookEventJobTests` — COD vs Stripe branching; enum `TargetState`/`PaymentTimelineState`; idempotent complete/void.
- `CreateOrderFromCartTests` — `IsPending && IsOffline` gating.
- `Shipment` lifecycle tests — transitions, tracking-number requirement at `Shipped`, derived `OrderFulfillmentState` computation, sync command writes the Order cache.
- SPA — `CheckoutView.spec.ts` method list + COD place + step-3 resolution; Admin order detail shipment rendering.

## 7. Rationale & Context

- **Rename, not split**: payment processing is async (Stripe webhook / COD collection) and already modeled by `PaymentRecordState` + order timestamps; a separate "Payment" checkout step would duplicate that signal.
- **PascalCase wire**: `OrderStatus`/`CheckoutState` already serialize enum names; aligning payment/shipment yields one consistent contract.
- **Value converters over SQL backfill**: legacy status strings are read-mapped in the EF configuration rather than rewritten in a data migration — non-destructive, single-source-of-truth, and lazily canonicalized on next write.
- **Clean migrations**: dropping the two in-progress migrations and regenerating from the corrected model keeps history linear and reviewable.
- **Typed unions, not TS `enum`**: matches the existing codebase style; avoids a runtime artifact and `erasableSyntaxOnly` conflicts.
- **Shipment as aggregate**: fulfillment needs tracking number, method snapshot, and per-shipment timestamps; a `ShipmentStatus` on Shipping plus a derived cache on Order gives a single source of truth with cheap order-list reads.
- **Processing-timestamp fix**: stamping on selection records "started paying" before any charge exists; stamping on `PaymentCapture.Process()` restores the timeline's meaning.

## 8. Dependencies & External Integrations

### External Systems
- **EXT-001**: Stripe (payment gateway) — drives `PaymentRecordState` via `checkout.session.*` webhooks.

### Third-Party Services
- **SVC-001**: MediatR — cross-module `ISender` commands (`RecordOrderPaymentStateCommand`, `RecordOrderShipmentStateCommand`, `CompleteCheckoutForPaymentCommand`, `CreateShipmentCommand`).
- **SVC-002**: FluentValidation — `UpdateOrderShipmentState.Validator` / `OrderValidation` / `Shipment` validators use enum members.

### Infrastructure Dependencies
- **INF-001**: PostgreSQL (Npgsql, pgvector) — status text columns + new `Shipment` table; value converters read legacy values in place.

### Data Dependencies
- **DAT-001**: Existing persisted status values (`orders`) — legacy strings are read-mapped by the value converters per §4.6.

### Technology Platform Dependencies
- **PLT-001**: .NET 10 — `System.Text.Json` `JsonStringEnumConverter`.
- **PLT-002**: EF Core — `.HasConversion<string>()` + migration generation.
- **PLT-003**: Vue 3 + TypeScript + Zod — typed unions + `z.enum`.

### Compliance Dependencies
- **COM-001**: None new — PCI-DSS scope unchanged (Stripe handles card data).

## 9. Examples & Edge Cases

### 9.1 Correct handler pattern (before → after)

```csharp
// BEFORE
if (!Enum.TryParse<CheckoutState>(command.TargetState, ignoreCase: true, out var targetState))
    return OrderResult.Errors.CannotAdvanceState;
cart.AdvanceCheckoutState(targetState);
if (targetState == CheckoutState.PaymentSelected)
    cart.MarkPaymentProcessing(DateTimeOffset.UtcNow);

// AFTER
var result = cart.AdvanceCheckoutState(command.TargetState);
if (result.IsFailure) return result.Errors;
// No MarkPaymentProcessing — selection is not processing.
```

### 9.2 Billing → Ordering enum hand-off

```csharp
await _sender.Send(new RegressCheckoutStateCommand
{
    CartId = payment.OrderId,
    TargetState = CheckoutState.PickDeliveryMethod
}, ct);

await _sender.Send(new RecordOrderPaymentStateCommand
{
    OrderId = payment.OrderId,
    PaymentState = PaymentTimelineState.Completed,
    AtUtc = payment.CompletedAtUtc ?? DateTimeOffset.UtcNow
}, ct);
```

### 9.3 Shipping → Ordering fulfillment sync

```csharp
// Shipment status change (Shipping) recomputes the derived state, then:
await _sender.Send(new RecordOrderShipmentStateCommand
{
    OrderId = shipment.OrderId,
    FulfillmentState = OrderFulfillmentState.Shipped,
    ShippedAtUtc = shipment.ShippedAtUtc,
    DeliveredAtUtc = shipment.DeliveredAtUtc
}, ct);
```

### 9.4 Edge cases

- **Free order** (`Total == 0`): `PaymentRequired()` is false, so the payment step is skipped (`PickDeliveryMethod → Confirm`); no `PickPaymentMethod` transition.
- **Amount-affecting edit**: regresses `PickPaymentMethod → PickDeliveryMethod`; SPA watch clears `paymentIntentId`/`paymentClientSecret`.
- **Null status on draft orders**: `PaymentState`/`ShipmentState` may be null pre-event; SPA unions stay `| null`.
- **Split shipment**: two shipments where one is `Delivered` and one `Pending` derive `Partial`; all `Delivered` → `Delivered`; all `Canceled` → `Canceled`; no shipments → `None`.
- **Backorder**: shipment `Pending → Backorder → Ready → Shipped`; the order-level state stays `Pending` until a shipment ships.
- **Unknown legacy value**: an unmatched legacy string falls through to `Enum.Parse` and fails materialization; the value-converter switch cases cover every legacy string the old constants defined (see §4.6).
- **Unknown wire value in SPA**: `z.enum` rejects at runtime rather than propagating an invalid string.

## 10. Validation Criteria

- **VAL-001**: `dotnet build` succeeds with zero warnings.
- **VAL-002**: `dotnet test service/Api/tests/Module.UnitTests` passes.
- **VAL-003**: `bash scripts/check-feature-conventions.sh` passes.
- **VAL-004**: `bash scripts/check-cross-module-refs.sh` reports the baseline unchanged — no new cross-module references.
- **VAL-005**: `cd app/Store && pnpm run lint && pnpm run test:unit` passes.
- **VAL-006**: `cd app/Admin && pnpm run lint && pnpm run test:unit` passes.
- **VAL-007**: Value-converter smoke: legacy strings (`"Delivery"`, `"Payment"`, `"balance_due"`, `"pending"`, `"ready"`, `"backorder"`) read back as the correct enum members; canonical names round-trip unchanged.
- **VAL-008**: No occurrences of removed identifiers (`PaymentSelected`, `CheckoutState.Payment`, `OrderConstant.PaymentState`, `OrderConstant.ShipmentState`, `OrderConstant.CheckoutStep`, the `OrderPaymentState` static class, `p.State == "Pending"`) remain in `service/Api`.

## 11. Related Specifications / Further Reading

- [spec-cart-consolidation.md](./spec-cart-consolidation.md) — checkout route/contract context.
- [plan/refactor-checkout-state-sync-1.md](../plan/refactor-checkout-state-sync-1.md) — checkout state synchronization.
- [plan/refactor-ordering-flow-1.md](../plan/refactor-ordering-flow-1.md) — ordering flow.
- [docs/codebase/ARCHITECTURE.md](../docs/codebase/ARCHITECTURE.md), [docs/codebase/CONVENTIONS.md](../docs/codebase/CONVENTIONS.md).
- [AGENTS.md](../AGENTS.md) — non-negotiable rules.

## 12. Open Questions

- **OQ-1 (engineering):** `OrderFulfillmentState` — confirm the derived value set (`None, Pending, Partial, Shipped, Delivered, Canceled`); should "all backordered" map to `Pending` (current) or a distinct `Backorder` value?
- **OQ-2 (product):** Tracking URL rendering (`ShippingMethod.TrackingUrl` `:tracking` substitution) — P1 scope confirmed?
- **OQ-3 (product):** Item-level shipment lines — confirmed deferred to P1 (v1 shipments are whole-order, manual split only)?
- **OQ-4 (engineering):** Confirm no concurrent editor is touching the working tree before implementation starts.

## 13. Architecture Review Findings (Orders + Payments + Stripe + Webhook)

Full review: `docs/codebase/orders-payments-architecture-review.md`. Summary of the defects that this spec's implementation must address (beyond the rename/enum work):

### 13.1 Priority — critical

- **FND-001 (P0)**: No persistent webhook event store / idempotency store / DLQ. Events are enqueued to Hangfire (`AutomaticRetry ×3`) and lost after exhaustion; dedup lives only in the unbounded `Payment.ProcessedStripeEventIds` jsonb list. A `payment is null` lookup returns silently (event dropped, no retry).
- **FND-002 (P0)**: Payment completion → order placement is non-atomic (separate `SaveChanges` + cross-module command). Requires an outbox.
- **FND-003 (P0)**: Timestamp conflation — `Payment.CompletedAtUtc/FailedAtUtc/VoidedAtUtc` use system `UtcNow` (processing time), not Stripe `event.Created` (business time); the conflation propagates to `Order.PaymentCompletedAt`. Store Stripe business time; add `ProcessedAtUtc` separately.
- **FND-004 (P0)**: Refund/capture invariants unenforced — comment declares `CapturedTotal <= Amount; RefundedTotal <= CapturedTotal` but only `Amount`/`RefundedAmount` exist; `ReconcileRefunded` has no upper bound; `Capture` sets `Completed` even for partial amounts.
- **FND-005 (P0)**: `Payment.ResponseCode` is overloaded as the correlation key and overwritten `cs_… → pi_…` mid-flight — races and dropped refund/dispute lookups. Split into `StripeSessionId` + `StripePaymentIntentId`.

### 13.2 Priority — high

- **FND-006 (P1)**: No idempotency store keyed by Stripe event id (jsonb list grows unbounded, racey).
- **FND-007 (P1)**: `ProcessStripeWebhookEventJob` does not catch `DbUpdateConcurrencyException` (unlike `RefundPayment`); relies on retry.
- **FND-008 (P1)**: Payment state-transition logic is duplicated across `PaymentProcessingService`, `ProcessStripeWebhookEventJob`, `MarkPaymentPaid`, `ConfirmPayment`.
- **FND-009 (P1)**: `payment_intent.succeeded` vs `checkout.session.completed` race (succeeded lookup misses until ResponseCode is overwritten).

### 13.3 Priority — medium

- **FND-010 (P2)**: Anemic `Payment`; naming inconsistency (`Payment` class / `payment_captures` table / `PaymentRecordState` enum).
- **FND-011 (P2)**: Dead code — `CreateSetupIntent`, direct-intent `Purchase/Authorize` path, `OrderHistory` (no writer), legacy `ConfirmPayment` contract.
- **FND-012 (P2)**: `Order.Resume()` (Canceled → Placed) has no payment consequence.

### 13.4 Priority — optional

- **FND-013 (P3)**: No correlation/request ID through the async path; no distributed tracing/metrics.

## 14. Recommended Final Architecture & Implementation Plan

| Aspect | Current | Recommended |
|--------|---------|-------------|
| Webhook ingest | validate → Hangfire enqueue raw payload | validate → **persist `WebhookEvent` row (unique Stripe id)** → 200 → job claims/processes/marks done |
| Payment → Order commit | separate commits | **outbox** (persist state + outbox record in one tx; background dispatcher) |
| Business timestamps | `UtcNow` at processing | Stripe `created` for completion/failure; separate `ProcessedAtUtc` |
| Refund/capture amounts | `Amount`/`RefundedAmount` | add `CapturedAmount`; enforce `Refunded ≤ Captured ≤ Amount` |
| Correlation | `ResponseCode` overloaded | `StripeSessionId` + `StripePaymentIntentId` columns |
| State transitions | scattered across 4 call sites | single `PaymentService` choke point; handlers only route |
| Failed events | lost after 3 retries | DLQ/reconciliation job (referenced in comments, not yet implemented) |

**Concrete change set**

- **Billing**
  - `Domain/PaymentCaptures/PaymentCapture.cs` — add `CapturedAmount`, `StripeSessionId`, `StripePaymentIntentId`, split business vs processed timestamps; retire the jsonb dedup list in favor of a `WebhookEvent` idempotency store.
  - `Domain/PaymentCaptures/PaymentCapture.Method.State.cs` — enforce refund/capture invariants.
  - New `Domain/WebhookEvents/WebhookEvent.cs` + EF config + migration (unique index on `StripeEventId`, `ProcessedAtUtc`, state).
  - `Backgrounds/ProcessStripeWebhookEventJob.cs` — read from `WebhookEvent` table, catch `DbUpdateConcurrencyException`, route only.
  - `Features/Storefront/Payment/Webhooks/StripeWebhook.cs` — persist event before enqueue.
  - `Services/Processing/PaymentProcessingService.cs` — single choke point for state transitions.
- **Ordering**
  - `Features/Storefront/CompleteCheckoutForPayment/CompleteCheckoutForPayment.cs` — outbox + idempotency.
  - `Domain/Orders/Order.Method.Timestamps.cs` — business-time semantics.
- **Shared** — outbox publisher + dispatcher job; reconciliation job (new); correlation-id propagation.
- **Database** — migration: `webhook_events`, `outbox`, new columns, unique constraints.
- **Tests** — webhook idempotency/reorder/DLQ, refund invariants, timestamp semantics, outbox dispatch, value converters (§4.6).
