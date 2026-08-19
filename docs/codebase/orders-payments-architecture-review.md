# Orders + Payments + Stripe + Webhook — Architecture Review

Date: 2026-08-14
Status: Reference review (feeds `spec/spec-checkout-state-enum-alignment.md` §13–14)

This document is the full end-to-end architectural review of the Orders +
Payments + Stripe + Webhook + Background-Job system. It is the authoritative
reference for the defects listed in the spec's §13 and the recommended final flow
in §14.

## 1. End-to-End Flow

1. **Cart/checkout (Ordering, sync):** `CreateCart` → `OrderMethod.Create` (Draft,
   `CheckoutState.Address`). `AddToCart`→`AddLineItem`, `UpdateCheckout`→`UpdateDetails`,
   `SelectShippingRate`→`ReplaceShippingAdjustment`. Amount edits advance/regress
   `CheckoutState` (`AdvanceCheckoutState`/`RegressCheckoutIfAmountChanged`).
2. **Payment initiation (Billing, sync):** `CreatePaymentIntent` reads the cart via
   `GetCartForCheckoutQuery`, branches on provider — COD → `Process()`+`Pend()`
   (Pending, no gateway); Stripe → `CreateCheckoutSessionAsync` → `ResponseCode=session.Id`,
   `CheckoutUrl`, `Process()` (Processing). Sends `AdvanceCheckoutStateCommand{PickPaymentMethod}`.
3. **Payment completion (Stripe, async):** webhook → `StripeWebhook.Endpoint`
   (validate HMAC) → `StripeWebhook.CommandHandler` enqueues `ProcessStripeWebhookEventJob`
   on Hangfire → 200. `checkout.session.completed` → lookup by session/PaymentIntent id →
   verify `payment_status==paid` → overwrite `ResponseCode=session.PaymentIntentId` →
   `Complete()` → send `CompleteCheckoutForPaymentCommand`.
4. **Placement (Ordering):** `CompleteCheckoutForPayment` re-checks `IsCompleted`,
   then `CheckoutPlacementService.PlaceAsync` (consume stock → Confirm → number →
   `Place()` → save → notify).
5. **Mirror:** `RecordOrderPaymentStateCommand` stamps `Order.Payment*At`.
6. **Fulfillment/cancel/refund:** admin capture/void/refund via `PaymentProcessingService`;
   `charge.refunded`/`charge.dispute.created`/`payment_intent.canceled`/
   `checkout.session.expired` handled by the job. `CartExpiryJob` expires drafts.

## 2. Flow Chart

```
Client ──▶ Carter endpoint ──▶ MediatR handler ──▶ Domain (Order/Payment)
                                    │                 │
                                    │                 ▼
                                    │            EF Core ──▶ PostgreSQL
                                    │
                                    └──▶ PaymentProcessingService ──▶ Stripe Gateway ──▶ Stripe API

Stripe ──webhook──▶ StripeWebhook.Endpoint ──validate/parse──▶ Hangfire enqueue
                                                                   │
                                                                   ▼
                                              ProcessStripeWebhookEventJob [retry ×3]
                                                                   │
                                              Payment domain transitions ──▶ EF Core ──▶ PostgreSQL
                                                                   │
                                              └─ISender─▶ Ordering (CompleteCheckoutForPayment,
                                                          RecordOrderPaymentState, RegressCheckoutState)
                                                                   │
                                                                   └─▶ PostgreSQL (orders)
Retry/error: job throws ─▶ Hangfire retry ×3 ─▶ Failed (NO DLQ — event lost)
```

## 3. Data Lifecycle

| Data | Origin | Owner | Modified by | Persisted | Events that change it | Consumers | Immutable when |
|------|--------|-------|-------------|-----------|------------------------|-----------|----------------|
| Order | CreateCart/CreateOrder | Ordering | cart ops, placement, admin, CartExpiryJob, mirror cmds | ordering.orders | cart edits, place/cancel/approve, RecordOrderPaymentState | SPA, admin, Billing | Placed (mostly) |
| LineItem | AddToCart/AddOrderLineItem | Ordering | Add/Remove/UpdateQuantity | ordering.line_items | cart edits | totals | after place |
| Payment | CreatePaymentIntent/admin | Billing | webhook job, ProcessingService, admin | billing.payment_captures | gateway + webhooks + admin | Ordering (query), SPA | terminal state |
| Payment.State | Create (Checkout) | Billing | domain transitions | enum→string | Process/Pend/Complete/Fail/Void/Dispute/Invalidate/Capture | gating, DTOs | terminal |
| Payment.ResponseCode | intent creation | Billing | **overwritten cs_→pi_** | column | session.completed | refund/void/dispute correlation | — (fragile) |
| Stripe event | Stripe | — | — | NOT persisted (Hangfire arg + jsonb list) | — | job | — |
| ProcessedStripeEventIds | job | Billing | RecordStripeEventAsync | jsonb on payment | each webhook | dedup | append-only, unbounded |
| Timestamps | domain methods | varies | see §4 | columns | transitions | reporting | business stamps monotonic |

Late/duplicate/out-of-order: dedup by `ProcessedStripeEventIds` + `LastStripeEventCreatedAtUtc`
stale-guard (drops regression events older than last applied). Duplicate → skip.
Out-of-order progress → not guarded (correct). **`payment is null` on lookup → silent return → event lost.**

## 4. Timestamps

| Timestamp | Set by | UTC | Source | Meaning |
|---|---|---|---|---|
| Order.Created/Modified | domain methods | yes | system UtcNow | audit |
| Order.Completed/Canceled/Approved | Place/Finalize/Complete, Cancel, Approve | yes | system | business |
| Payment.Created | Create | yes | system | audit |
| Payment.Completed/Failed/Voided/Disputed/Refunded | domain transitions | yes | **system UtcNow** | ⚠️ processing time |
| Payment.LastStripeEventCreatedAtUtc | RecordStripeEventAsync | yes | **Stripe event.Created** | business/event |
| Order.Payment*At | RecordOrderPaymentState | yes | mirrors Payment.*AtUtc | ⚠️ inherits conflation |

Recommendation: store Stripe `created` as authoritative business time for payment
completion/failure; add separate `ProcessedAtUtc`; never derive the Order timeline
from processing time.

## 5–6. Modules, Services, Entities

- **Ordering**: `Order` (rich aggregate; `OrderMethod.*` enforce totals, forward-only
  checkout, Placed-immutable-except-Cancel), `LineItem`, `Adjustment`, `OrderHistory`
  (unused), `CheckoutPlacementService`, `OrderNumber`.
- **Billing**: `Payment` (**anemic** data bag; `PaymentCaptureMethod.*` transitions +
  `PaymentProcessingService` orchestration), `PaymentMethod`, `StripeWebhookDispatcher`,
  `ProcessStripeWebhookEventJob`.
- `PaymentProcessingService` and the webhook job **duplicate** payment state-transition
  knowledge; `MarkPaymentPaid` and `ConfirmPayment` add two more `Complete()` call sites.

## 7. State Transitions (derived)

```
Order.Status: Draft ─Place─▶ Placed ─Cancel─▶ Canceled ─Resume─▶ Placed
              Draft ─CartExpiryJob─▶ Expired    Placed ─Approve─▶ approved flag

Payment.State: Checkout ─Process─▶ Processing ─Pend─▶ Pending ─Complete/Capture─▶ Completed
               Checkout/Processing/Pending ─Fail─▶ Failed ─Invalidate─▶ Invalid
               Processing/Pending ─Void─▶ Void ─Invalidate─▶ Invalid
               (not Void/Invalid) ─Dispute─▶ Disputed
               Completed/Disputed ─ReconcileRefunded─▶ RefundedAmount↑
```

Inconsistencies: partial capture not modeled (`Capture` sets Completed regardless of
amount); `Refund` caps on authorized not captured; `ReconcileRefunded` has no cap;
`Dispute` accepts from `Failed`; `Resume` has no payment consequence.

## 8–9. Stripe Integration & Background Jobs

- Signature HMAC-SHA256, bypassed in Development only. Gateway idempotency key
  `shop-{paymentNumber}`. Webhook dedup is the local jsonb list, not Stripe idempotency.
- Race: `payment_intent.succeeded` + `checkout.session.completed` both fire; succeeded
  lookup misses until `ResponseCode` overwritten.
- `ProcessStripeWebhookEventJob`: Hangfire `AutomaticRetry ×3, on-exceeded=Fail`; no DLQ;
  no explicit transaction; doesn't catch `DbUpdateConcurrencyException`.
- Recovery: gateway-failure → no mutation; DB-failure-after-Stripe-success → retry
  (state guards); DB-success-then-job-fail → retry (idempotent); lost after 3 retries.

## 10. Old Interface

`Services/Webhook/IWebhookHandler.cs` is deleted and has zero references; replaced by
`IStripeWebhookService` (validate + parse) used by endpoint and job. Commit the deletion.

## 11–12. Recommended Integrations & Persistence

Recommended (solve real problems): webhook event table + idempotency store (P0),
outbox (P0), DLQ/reconciliation job (P1), correlation-id/tracing/metrics (P3).
Not needed: message broker, distributed locking.

Persistence gaps: no unique idempotency table; no `CapturedAmount`; `ResponseCode`
not unique; no FK/table backing `OrderHistory`; order-number collision via retry loop.

## 13. Critical Issues

- **P0**: lost events; non-atomic completion→placement; timestamp conflation; refund
  invariant unenforced; `ResponseCode` correlation-key mutation.
- **P1**: no idempotency store; concurrency handling missing in job; duplicated
  state logic; `succeeded` vs `session.completed` race.
- **P2**: anemic Payment + naming; dead code (`CreateSetupIntent`, direct-intent path,
  `OrderHistory`, legacy `ConfirmPayment`); `Resume` no payment consequence.
- **P3**: correlation/tracing/metrics.

## 14. Final Recommended Flow

| | Current | Recommended |
|---|---|---|
| Ingest | validate → enqueue raw payload | validate → persist WebhookEvent (unique id) → 200 → claim/process |
| Commit | separate commits | outbox |
| Timestamps | UtcNow processing | Stripe `created` business + `ProcessedAtUtc` |
| Amounts | Amount/RefundedAmount | add CapturedAmount, enforce Refunded ≤ Captured ≤ Amount |
| Correlation | ResponseCode overloaded | StripeSessionId + StripePaymentIntentId |
| Transitions | 4 call sites | single PaymentService choke point |
| Failed events | lost | DLQ/reconciliation job |

Concrete change set is listed in `spec/spec-checkout-state-enum-alignment.md` §14.
