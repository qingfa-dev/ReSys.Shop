# Design: Propagate `payment_id` Metadata to the Checkout PaymentIntent

- Date: 2026-08-16
- Status: Approved (approach A)
- Scope: Billing module — Stripe Checkout payment-intent correlation

## Problem

`ProcessStripeWebhookEventJob.FindPaymentByIntentAsync` correlates a Stripe
`PaymentIntent` (from a `payment_intent.*` webhook) to a local `PaymentCapture`
using three fallbacks:

1. `p.StripePaymentIntentId == intent.Id`
2. `p.ResponseCode == intent.Id`
3. `p.Number == intent.Metadata["payment_id"]`

Fallback (3) is the only one that works in the Checkout flow **before**
`checkout.session.completed` arrives: at intent creation only the session id
(`cs_...`) is stored in `StripeSessionId`/`ResponseCode`, and the `PaymentIntent`
id (`pi_...`) is unknown until the session-completed event correlates it.

The bug: `CreateCheckoutSessionAsync` (`StripeGateway.cs:199-203`) sets
`Metadata` on the **Checkout Session** only. Stripe does **not** copy session
metadata onto the `PaymentIntent` it creates when the customer pays. Therefore
`intent.Metadata` is empty on `payment_intent.succeeded`, `metadataPaymentId` is
always `null`, and fallback (3) is dead code.

Consequence: the metadata fallback — added to close the async-method race where
`payment_intent.succeeded` arrives before `checkout.session.completed` — never
matches, so an order can wait on the session-completed event (or a Hangfire
retry) to complete even though the intent succeeded first.

The existing test `HandlePaymentIntentSucceeded_WhenOnlySessionIdStored_FindsByMetadata`
(`ProcessStripeWebhookEventJobTests.cs:938`) passes only because it manually
injects `["payment_id"]` into the mocked intent; production never does.

## Decision

Approach A: propagate metadata to the Checkout PaymentIntent so the existing
fallback works as designed. No schema change, no behavior change elsewhere.

## Change

`service/Api/src/Module/Billing/Services/Provider/Stripe/StripeGateway.cs`

1. Extract the `SessionCreateOptions` construction from
   `CreateCheckoutSessionAsync` into an `internal static` method
   `BuildCheckoutSessionOptions(decimal amount, GatewayOptions options)` (the
   assembly already has `InternalsVisibleTo Module.UnitTests`).

2. Add `PaymentIntentData` to the session options, mirroring the session-level
   metadata:

```csharp
PaymentIntentData = new SessionPaymentIntentDataOptions
{
    Metadata = new Dictionary<string, string>
    {
        [GatewayConstants.Metadata.OrderIdKey] = options.OrderId,
        [GatewayConstants.Metadata.PaymentIdKey] = options.PaymentId
    }
}
```

`CreateCheckoutSessionAsync` calls `BuildCheckoutSessionOptions` then
`_sessionService.CreateAsync(options, ro, ct)` unchanged. `FindPaymentByIntentAsync`
is unchanged.

## Data Flow (after)

1. `CreatePaymentIntent.Handle` passes `options.PaymentId = payment.Number`
   (format `PAY-XXXXXXXX-XXXXX`).
2. Stripe creates the Session **and** stamps its PaymentIntent with
   `payment_id = <number>` (and `order_id`).
3. On `payment_intent.succeeded`, `intent.Metadata["payment_id"]` is populated,
   so `FindPaymentByIntentAsync` matches `p.Number` even though
   `StripePaymentIntentId`/`ResponseCode` still hold only `cs_...`; the payment
   completes and the order is placed without waiting for
   `checkout.session.completed`.

## Error Handling / Edge Cases

- **In-flight sessions** created before this change still lack the metadata:
  acceptable — transient 24-h sessions; their `checkout.session.completed` event
  still correlates and completes normally.
- **Async methods** (`payment_status=unpaid` then `succeeded`): metadata is on
  the intent from creation, closing the race the same way.
- **Offline (COD)** methods never call `CreateCheckoutSessionAsync` — unaffected.
- **Bogus gateway** (`BogusGateway`) is a fake with no real PaymentIntent and no
  webhook correlation — unaffected.

## Testing

1. **New unit test** in `StripeGatewayTests` (or a new
   `StripeGatewayCheckoutSessionTests`):
   - `BuildCheckoutSessionOptions_PopulatesPaymentIntentMetadata`: build with a
     `GatewayOptions { OrderId, PaymentId, ... }` and assert
     `result.PaymentIntentData.Metadata["payment_id"] == options.PaymentId` and
     `["order_id"] == options.OrderId`.
   - `BuildCheckoutSessionOptions_PopulatesSessionMetadata`: assert the
     session-level `Metadata` still carries both keys (regression guard).
   - `BuildCheckoutSessionOptions_DefaultsLineItem`: assert the single line item
     (quantity 1, currency lowercased, unit amount in cents) is unchanged.
2. **Existing webhook test** `HandlePaymentIntentSucceeded_WhenOnlySessionIdStored_FindsByMetadata`
   remains and is now production-representative; keep it as the end-to-end
   correlation proof.
3. **No integration test** for the live Stripe HTTP call; verify via
   `ApiTests` `.http` when a real Stripe session is available (documented, not
   automated).

## Out of Scope

- Migrating existing in-flight sessions or backfilling metadata.
- Changing `order_id` value semantics (currently `$"{orderId}-{payment.Number}"`).
- The broader cross-module `ISender` convention review (separate decision).
