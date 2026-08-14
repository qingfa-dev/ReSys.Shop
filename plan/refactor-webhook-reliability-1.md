---
goal: Harden the Stripe webhook pipeline — persist events with a unique idempotency key, add an outbox for the payment→order boundary, use Stripe business timestamps, and split the overloaded ResponseCode correlation key.
version: 1.0
date_created: 2026-08-14
last_updated: 2026-08-14
owner: Billing / Ordering
status: 'Planned'
tags: [refactor, billing, webhook, outbox, idempotency, timestamp, reliability]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

The architecture review (`docs/codebase/orders-payments-architecture-review.md`)
found five P0/P1 reliability defects in the webhook pipeline: events are lost after
Hangfire retries (no persistent event store / DLQ), `payment is null` lookups drop
events silently, the payment→order commit is non-atomic, business timestamps use
system processing time instead of Stripe event time, and `ResponseCode` is
overloaded (session id vs PaymentIntent id). This plan fixes them.

**Spec:** `spec/spec-checkout-state-enum-alignment.md` §13–14

## 1. Requirements & Constraints

- **REQ-001**: New `WebhookEvent` entity in Billing (`StripeEventId` unique, `Type`, `Payload`, `State` (Pending/Processing/Processed/Failed), `ProcessedAtUtc`, `AttemptCount`). Endpoint persists the event (unique index) before enqueue.
- **REQ-002**: `ProcessStripeWebhookEventJob` claims an event (Pending → Processing), processes idempotently, marks Processed (or Failed + retry), instead of parsing the raw payload arg.
- **REQ-003**: Outbox — the payment-complete webhook writes the payment state + an outbox record in one transaction; a dispatcher job delivers the cross-module `CompleteCheckoutForPaymentCommand`. The order-placement save point is `CheckoutPlacementService.PlaceAsync` (its `SaveChangesAsync` at `CheckoutPlacementService.cs:33`), not the `CompleteCheckoutForPayment` handler (which only reads via `GetPaymentForCheckoutQuery`); the outbox record must be written in the same transaction there.
- **REQ-004**: `Payment.CompletedAtUtc`/`FailedAtUtc`/`VoidedAtUtc`/`DisputedAtUtc`/`RefundedAtUtc` are set from Stripe's `event.Created` (business time); add `Payment.ProcessedAtUtc` for system time; `RecordOrderPaymentState` mirrors business time.
- **REQ-005**: Replace `ResponseCode` overload with `StripeSessionId` + `StripePaymentIntentId` columns; lookups correlate on the right column.
- **CON-001**: Modules via MediatR `ISender`; zero-warning build; Result objects.
- **PAT-001**: Idempotency via unique constraint on `StripeEventId` + state guard (not the jsonb list).

## 2. Implementation Steps

### Implementation Phase 1 — Webhook event store

- GOAL-001: Persist and idempotently process webhook events.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | `WebhookEvent` entity + EF config + migration (`AddWebhookEvent`). | | |
| TASK-002 | `StripeWebhook.CommandHandler` persists the event (unique `StripeEventId`), then enqueues by event id. | | |
| TASK-003 | `ProcessStripeWebhookEventJob.ExecuteAsync(eventId)` loads/claims/processes/marks. | | |

#### TASK-001: Entity

`service/Api/src/Module/Billing/Domain/WebhookEvents/WebhookEvent.cs` + `Persistence/Configurations/Webhooks/WebhookEventConfiguration.cs`:

```csharp
public sealed class WebhookEvent : Entity
{
    public string StripeEventId { get; set; } = string.Empty; // unique
    public string Type { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public WebhookEventState State { get; set; } = WebhookEventState.Pending;
    public int AttemptCount { get; set; }
    public DateTimeOffset? ProcessedAtUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
// config: HasIndex(StripeEventId).IsUnique(); State stored via HasConversion<string>()
```

Migration `AddWebhookEvent`.

#### TASK-002: Persist before enqueue

`StripeWebhook.CommandHandler`: after `ValidateSignature`, `ParseEvent` (to get the id/type), insert the `WebhookEvent` row (ignore duplicate key → already-processed → return Ok), then `backgroundJobClient.Enqueue<ProcessStripeWebhookEventJob>(job => job.ExecuteAsync(eventId, CancellationToken.None))`.

#### TASK-003: Claim + process

`ProcessStripeWebhookEventJob.ExecuteAsync(Guid eventId, ...)` loads the `WebhookEvent`, sets State=Processing, parses `Payload`, routes to the existing type handlers (unchanged), sets State=Processed. `[AutomaticRetry]` remains as a safety net; on exhaustion the event stays `Failed` for the reconciliation job.

### Implementation Phase 2 — Business timestamps + correlation keys

- GOAL-002: Correct timestamps and split the correlation key.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004 | `Payment` + config: add `ProcessedAtUtc`, `StripeSessionId`, `StripePaymentIntentId`. | | |
| TASK-005 | `ProcessStripeWebhookEventJob` sets business stamps from `event.Created`; lookups use the right column. | | |
| TASK-006 | `CreatePaymentIntent` writes `StripeSessionId`; `CompleteCheckoutForPayment`/webhook read it. | | |

#### TASK-004/005/006 details

- `Payment.CompletedAtUtc = StripeEventCreatedUtc(stripeEvent)` (business time); `ProcessedAtUtc = DateTimeOffset.UtcNow`.
- `checkout.session.completed`: look up by `StripeSessionId == session.Id`; set `StripePaymentIntentId = session.PaymentIntentId` (do NOT overwrite `ResponseCode`). `charge.refunded`/`dispute`/`payment_intent.*` look up by `StripePaymentIntentId`.
- Migration `AddPaymentCorrelationAndProcessedTimestamps`.

### Implementation Phase 3 — Outbox

- GOAL-003: Atomic payment→order cross-module commit.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-007 | `OutboxMessage` entity + config (Shared or Billing) + migration. | | |
| TASK-008 | Write the outbox record inside `CheckoutPlacementService.PlaceAsync` (same `SaveChangesAsync` at `CheckoutPlacementService.cs:33`). | | |
| TASK-009 | `OutboxDispatcherJob` (Hangfire) delivers messages via `ISender`; idempotent delivery (mark Dispatched after success). | | |

## 3. Alternatives

- **ALT-001**: Keep jsonb `ProcessedStripeEventIds` + raw-payload Hangfire arg. Rejected — unbounded growth, silent drops, no DLQ.
- **ALT-002**: Message broker (RabbitMQ/Kafka). Rejected — Postgres outbox + Hangfire suffices at this scale.

## 4. Dependencies

- **DEP-001**: `feature-payment-method-selection-1` (checkout-session flow already shipped).
- **DEP-002**: Hangfire, MediatR, EF Core.

## 5. Files

- **FILE-001**: `service/Api/src/Module/Billing/Domain/WebhookEvents/*` (new).
- **FILE-002**: `service/Api/src/Module/Billing/Features/Storefront/Payment/Webhooks/StripeWebhook.cs` (persist before enqueue).
- **FILE-003**: `service/Api/src/Module/Billing/Backgrounds/ProcessStripeWebhookEventJob.cs` (claim/process + timestamps + correlation).
- **FILE-004**: `service/Api/src/Module/Billing/Domain/PaymentCaptures/PaymentCapture.cs` (+ columns).
- **FILE-005**: `service/Api/src/Module/Billing/Domain/PaymentCaptures/PaymentCapture.Method.State.cs` (accept business time).
- **FILE-006**: `service/Api/src/Module/Ordering/Services/CheckoutPlacementService.cs` (outbox write point) and `Features/Storefront/CompleteCheckoutForPayment/CompleteCheckoutForPayment.cs` (idempotency).
- **FILE-007**: `service/Api/src/Shared/Operational/Outbox/*` (new).
- **FILE-008**: migrations.

## 6. Testing

- **TEST-001**: Duplicate webhook event id → second insert ignored (unique constraint), single processing.
- **TEST-002**: `payment is null` → event marked Failed (not silently dropped) → reconciliation job retries.
- **TEST-003**: Outbox delivers exactly once (idempotent); duplicate dispatch no-ops.
- **TEST-004**: Business timestamps equal Stripe `event.Created`; `ProcessedAtUtc` equals system time.
- **TEST-005**: Correlation: refund/dispute look up by `StripePaymentIntentId` after session.completed.
- **TEST-006**: `dotnet test service/Api/tests/Module.UnitTests` green.

## 7. Risks & Assumptions

- **RISK-001**: Outbox touches the placement hot path; keep the write in the same transaction, delivery async.
- **ASSUMPTION-001**: A reconciliation job is introduced to re-queue `Failed` webhook events (currently only referenced in comments).

## 8. Related Specifications / Further Reading

- [spec-checkout-state-enum-alignment.md](../spec/spec-checkout-state-enum-alignment.md) §13–14
- [docs/codebase/orders-payments-architecture-review.md](../docs/codebase/orders-payments-architecture-review.md)
