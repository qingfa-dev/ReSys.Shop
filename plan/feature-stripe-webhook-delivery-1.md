---
goal: Deliver real Stripe webhook events to the local dev API via Stripe CLI and add diagnostic logging across the payment flow so a checkout can be traced end-to-end.
version: 2.0
date_created: 2026-08-13
last_updated: 2026-08-13
owner: Billing / Ordering / Store SPA / Aspire
status: 'Planned'
tags: [feature, billing, stripe, webhook, logging, checkout, store]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

A Stripe Checkout payment succeeds on Stripe's hosted page, but the local app
never advances: the `PaymentCapture` stays `Processing` and the order is not
auto-placed, because Stripe's servers cannot reach `localhost` (no webhook
delivery). Per the thesis (PAY-FR-04: "verify and process incoming gateway
webhooks using HMAC signatures"; UC-STR-PAY E3: "webhook updates state"), the
**webhook is the single source of truth** for payment-state updates. This plan:

1. **Delivers the webhook locally** via the Stripe CLI (`stripe listen`), pinned
   to a stable API HTTPS port (5001) so the real `checkout.session.completed` /
   `checkout.session.expired` events reach the dev API.
2. **Adds diagnostic structured logging** at every boundary of the payment flow
   (intent creation, webhook receipt + signature, session lookup, completion,
   order placement, expiry regression, SPA return polling) so the flow can be
   traced end-to-end and the failing stage identified immediately.

The success_url return page continues to poll local state (which the webhook
updates); no completion happens from the browser return. This matches the
thesis and keeps a single completion source.

## 1. Requirements & Constraints

- **REQ-001**: A `stripe listen` forwarder delivers real Stripe webhook events to the local API webhook endpoint (`api/storefront/billing/webhooks/stripe`).
- **REQ-002**: The API HTTPS port is pinned to a stable value (5001) so the forward URL never changes between Aspire runs.
- **REQ-003**: Structured `[LoggerMessage]` logs exist at every payment-flow boundary: intent creation, webhook receipt, signature validation, session lookup, payment completion, order placement, expiry regression, and SPA return polling.
- **REQ-004**: The `/checkout/return` SPA page logs each polling attempt (order id, `isCompleted`, attempt count) to the browser console.
- **SEC-001**: Webhook signature validation stays mandatory (HMAC via `GatewayProviders:stripe:WebhookSecret`); no logging emits secrets, API keys, or `whsec_...` values.
- **CON-001**: `TreatWarningsAsErrors=true` — any C# warning fails the build.
- **CON-002**: Structured logs use the existing `[LoggerMessage]` source-generated partial-logger pattern (`*Loggers.cs`), with `EventId`s continuing the 5001+/6001+ numbering.
- **CON-003**: Aspire pins the API HTTPS port to 5001; `.WithExternalHttpEndpoints()` is retained.
- **CON-004**: Store SPA comments follow `app/Store/AGENTS.md` (`// Label: Sentence.`); lines under 100 chars.
- **GUD-001**: The webhook remains the single source of truth for payment-state updates (thesis PAY-FR-04 / E3); no success_url completion path is added.
- **PAT-001**: Logging calls follow the existing `ProcessStripeWebhookEventJobLoggers` / `StripeWebhookDispatcherLoggers` partial-class pattern; handler constructors gain `ILogger<T>` only where logging is added.

## 2. Implementation Steps

### Implementation Phase 1: Stripe CLI webhook delivery

- GOAL-001: Deliver real Stripe events to the local API for testing the webhook pipeline.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Pin the API HTTPS port to 5001 in the Aspire AppHost. | | |
| TASK-002 | Add `scripts/dev-stripe-listen.sh` + README documenting the two-terminal flow and webhook-secret wiring. | | |

#### TASK-001: Pin API HTTPS port

**Files:**
- Modify: `infra/Aspire/src/ReSys.AppHost/AppHost.cs` (the `api` resource)

- [ ] On the `api` builder, add a fixed HTTPS endpoint (keep `.WithExternalHttpEndpoints()`):
```csharp
    .WithHttpsEndpoint(port: 5001, name: "https")
    .WithExternalHttpEndpoints()
```
- [ ] Verify: `dotnet build infra/Aspire/src/ReSys.AppHost` — 0 warnings; after `dotnet run --project infra/Aspire/src/ReSys.AppHost`, the API is reachable at `https://localhost:5001` (health check `/health` returns 200).

#### TASK-002: stripe listen script + docs

**Files:**
- Create: `scripts/dev-stripe-listen.sh` (executable, `chmod +x`)
- Modify: `service/Api/README.md` — add a "Stripe webhooks (local)" section

- [ ] `scripts/dev-stripe-listen.sh`:
```bash
#!/usr/bin/env bash
# Forward real Stripe webhook events to the local API.
# Aspire pins the API HTTPS port to 5001 (see infra/Aspire/src/ReSys.AppHost/AppHost.cs).
# Usage: STRIPE_SECRET_KEY=sk_test_... ./scripts/dev-stripe-listen.sh
set -euo pipefail
if [ -z "${STRIPE_SECRET_KEY:-}" ]; then
  echo "Set STRIPE_SECRET_KEY (sk_test_...) first." >&2
  exit 1
fi
echo "Forwarding Stripe events to https://localhost:5001/api/storefront/billing/webhooks/stripe"
echo ""
echo "Copy the printed 'webhook signing secret' (whsec_...) into the API, then restart the API:"
echo "  dotnet user-secrets set \"GatewayProviders:stripe:WebhookSecret\" \"<whsec_...>\" --project service/Api/src/Api/Api.csproj"
echo ""
echo "NOTE: the secret is per 'stripe listen' run. Keep this process running; if you restart it, re-set the secret."
stripe listen --forward-to https://localhost:5001/api/storefront/billing/webhooks/stripe --api-key "$STRIPE_SECRET_KEY"
```
- [ ] README section:
  1. Start the app: `dotnet run --project infra/Aspire/src/ReSys.AppHost` (API on `https://localhost:5001`).
  2. `STRIPE_SECRET_KEY=sk_test_... ./scripts/dev-stripe-listen.sh`.
  3. Copy the printed `whsec_...` into `dotnet user-secrets set "GatewayProviders:stripe:WebhookSecret" "<whsec_...>" --project service/Api/src/Api/Api.csproj`, then restart the API resource.
  4. Pay via a Checkout Session: `checkout.session.completed` fires → payment `Completed` → order auto-placed. Abandon a session: `checkout.session.expired` fires → payment `Void` + stock released + cart regresses to `Delivery`.
- [ ] Commit: `git add infra/Aspire/src/ReSys.AppHost/AppHost.cs scripts/dev-stripe-listen.sh service/Api/README.md` then `git commit -m "chore: pin api https port and add stripe listen dev script"`.

### Implementation Phase 2: Diagnostic logging — webhook delivery path

- GOAL-002: Add structured logs at every boundary so a checkout can be traced end-to-end and the failing stage is obvious.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-003 | Add dispatcher logs: webhook-secret missing, event received, signature verified. | | |
| TASK-004 | Add webhook-job logs: event routed, session lookup, completed, order placed, expired, cart regressed. | | |
| TASK-005 | Add `CreatePaymentIntent` logs: session created, COD created, retry voided stale. | | |
| TASK-006 | Add auto-place logs in `CompleteCheckoutForPayment` + `CheckoutPlacementService`. | | |
| TASK-007 | Add `console.debug` polling logs to `CheckoutReturnView.vue`. | | |

#### TASK-003: StripeWebhookDispatcherLoggers additions

**Files:**
- Modify: `service/Api/src/Module/Billing/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.Loggers.cs`
- Modify: `service/Api/src/Module/Billing/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.cs`

- [ ] Add to `StripeWebhookDispatcherLoggers`:
```csharp
[LoggerMessage(
    EventId = 5020,
    Level = LogLevel.Warning,
    Message = "Stripe webhook secret is not configured (GatewayProviders:stripe:WebhookSecret); all webhooks will be rejected.")]
public static partial void WebhookSecretMissing(ILogger logger);

[LoggerMessage(
    EventId = 5021,
    Level = LogLevel.Information,
    Message = "Stripe webhook signature verified.")]
public static partial void SignatureVerified(ILogger logger);

[LoggerMessage(
    EventId = 5022,
    Level = LogLevel.Information,
    Message = "Stripe webhook event received: {EventType}")]
public static partial void WebhookEventReceived(ILogger logger, string EventType);
```
- [ ] Wire into `StripeWebhookDispatcher`:
  - In `ValidateSignature`: when `WebhookSecret` is empty → `WebhookSecretMissing(_logger); return false;`. On successful `EventUtility.ValidateSignature` → `SignatureVerified(_logger); return true;`.
  - In `ParseEvent`: after a successful parse, `WebhookEventReceived(_logger, parsed.Type);` (only when `parsed` is non-null).

#### TASK-004: ProcessStripeWebhookEventJobLoggers additions

**Files:**
- Modify: `service/Api/src/Module/Billing/Backgrounds/ProcessStripeWebhookEventJob.Loggers.cs`
- Modify: `service/Api/src/Module/Billing/Backgrounds/ProcessStripeWebhookEventJob.cs`

- [ ] Add to `ProcessStripeWebhookEventJobLoggers`:
```csharp
[LoggerMessage(
    EventId = 5014,
    Level = LogLevel.Information,
    Message = "Stripe webhook event routed to handler: {EventType}")]
public static partial void EventRouted(ILogger logger, string EventType);

[LoggerMessage(
    EventId = 5015,
    Level = LogLevel.Debug,
    Message = "Checkout session lookup: SessionId={SessionId}, PaymentFound={Found}, PaymentId={PaymentId}")]
public static partial void SessionLookup(ILogger logger, string SessionId, bool Found, Guid? PaymentId);

[LoggerMessage(
    EventId = 5016,
    Level = LogLevel.Information,
    Message = "Checkout session completed: PaymentId={PaymentId}, PaymentIntentId={PaymentIntentId}")]
public static partial void CheckoutSessionCompleted(ILogger logger, Guid PaymentId, string? PaymentIntentId);

[LoggerMessage(
    EventId = 5017,
    Level = LogLevel.Information,
    Message = "Order placed after checkout session completed: PaymentId={PaymentId}")]
public static partial void OrderPlaced(ILogger logger, Guid PaymentId);

[LoggerMessage(
    EventId = 5018,
    Level = LogLevel.Information,
    Message = "Checkout session expired: PaymentId={PaymentId}, SessionId={SessionId}")]
public static partial void CheckoutSessionExpired(ILogger logger, Guid PaymentId, string SessionId);

[LoggerMessage(
    EventId = 5019,
    Level = LogLevel.Information,
    Message = "Cart regressed to Delivery after session expiry: CartId={CartId}")]
public static partial void CartRegressedToDelivery(ILogger logger, Guid CartId);
```
- [ ] Wire into `ProcessStripeWebhookEventJob`:
  - In `ExecuteAsync`, after the parse-null guard: `EventRouted(_logger, stripeEvent.Type);`.
  - In `HandleCheckoutSessionCompleted`: after the lookup → `SessionLookup(_logger, session.Id, payment is not null, payment?.Id);`; after `payment.ResponseCode = session.PaymentIntentId` → `CheckoutSessionCompleted(_logger, payment.Id, session.PaymentIntentId);`; inside `if (placeResult.IsSuccess)` after recording the event → `OrderPlaced(_logger, payment.Id);`.
  - In `HandleCheckoutSessionExpired`: after the lookup → `SessionLookup(_logger, session.Id, payment is not null, payment?.Id);`; after `Void()` succeeds → `CheckoutSessionExpired(_logger, payment.Id, session.Id);`; after sending `RegressCheckoutStateCommand` → `CartRegressedToDelivery(_logger, payment.OrderId);`.

#### TASK-005: CreatePaymentIntent logging

**Files:**
- Create: `service/Api/src/Module/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.Loggers.cs`
- Modify: `service/Api/src/Module/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs`
- Modify: `service/Api/tests/Module.UnitTests/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntentTests.cs`

- [ ] `CreatePaymentIntent.Loggers.cs`:
```csharp
using Microsoft.Extensions.Logging;

namespace Module.Billing.Features.Storefront.Payment.CreateIntent;

public static partial class CreatePaymentIntentLoggers
{
    [LoggerMessage(
    EventId = 6010,
    Level = LogLevel.Information,
    Message = "Stripe Checkout session created: PaymentId={PaymentId}, SessionId={SessionId}, CheckoutUrl={CheckoutUrl}")]
    public static partial void SessionCreated(ILogger logger, Guid PaymentId, string? SessionId, string? CheckoutUrl);

    [LoggerMessage(
    EventId = 6011,
    Level = LogLevel.Information,
    Message = "COD payment intent created: PaymentId={PaymentId}, State=Pending")]
    public static partial void CodIntentCreated(ILogger logger, Guid PaymentId);

    [LoggerMessage(
    EventId = 6012,
    Level = LogLevel.Information,
    Message = "Retry at Payment: voided {Count} stale capture(s) for OrderId={OrderId}")]
    public static partial void RetryVoidedStale(ILogger logger, int Count, Guid OrderId);
}
```
- [ ] `CreatePaymentIntent.cs`: add `ILogger<CreatePaymentIntent.CommandHandler> logger` to the `CommandHandler` primary-constructor parameter list (after `ISender sender`). Call:
  - In the retry-at-`Payment` block, after the void loop: `CreatePaymentIntentLoggers.RetryVoidedStale(logger, stale.Count, command.Request.OrderId);`
  - In the offline branch after `payment.Pend()`: `CreatePaymentIntentLoggers.CodIntentCreated(logger, payment.Id);`
  - In the Stripe branch after `payment.CheckoutUrl = sessionResult.Value.CheckoutUrl;`: `CreatePaymentIntentLoggers.SessionCreated(logger, payment.Id, sessionResult.Value.Authorization, sessionResult.Value.CheckoutUrl);`
- [ ] `CreatePaymentIntentTests.cs`: update the `CommandHandler` constructor calls (constructor + any helper that builds the handler) to pass `Microsoft.Extensions.Logging.Abstractions.NullLogger<CreatePaymentIntent.CommandHandler>.Instance`. Do not change test assertions.

#### TASK-006: Auto-place logging

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/CompleteCheckoutForPayment/CompleteCheckoutForPayment.cs`
- Modify: `service/Api/src/Module/Ordering/Services/CheckoutPlacementService.cs`

- [ ] `CompleteCheckoutForPayment.cs`: add `ILogger<CompleteCheckoutForPaymentCommandHandler> logger` to the handler constructor and log the outcome (the handler currently has no logger):
  - After loading the draft cart (null branch is the idempotent no-op): `logger.LogDebug("CompleteCheckoutForPayment: CartId={CartId}, PaymentId={PaymentId}, CartFound={Found}", command.CartId, command.PaymentId, cart is not null);`
  - On successful placement: `logger.LogInformation("Order auto-placed from webhook: CartId={CartId}", command.CartId);`
  - On placement failure, before `return placeResult.Errors;`: `logger.LogWarning("Webhook auto-placement failed: CartId={CartId}: {Message}", command.CartId, placeResult.Message);`
- [ ] `CheckoutPlacementService.cs`: at the top of `PlaceAsync`, log placement start: `logger.LogInformation("Placing order {OrderId} (actor={Actor})", cart.Id, actor);` (the service already has `ILogger<CheckoutPlacementService>`).

#### TASK-007: SPA return polling logs

**Files:**
- Modify: `app/Store/src/features/ordering/views/CheckoutReturnView.vue`

- [ ] In `poll()` add a debug log before the result check:
```ts
// Debug: Trace the return-page poll in the browser console for local dev.
console.debug(`[checkout/return] poll order=${orderId} isCompleted=${result.value.isCompleted}`)
```
- [ ] Add a mount log: `console.debug('[checkout/return] mounted, starting poll')` at the top of `onMounted`.
- [ ] Verify from `app/Store`: `pnpm run build-only` (0 warnings) and targeted lint on the file.
- [ ] Commit Phase 2: `git add service/Api/src/Module/Billing/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.Loggers.cs service/Api/src/Module/Billing/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.cs service/Api/src/Module/Billing/Backgrounds/ProcessStripeWebhookEventJob.Loggers.cs service/Api/src/Module/Billing/Backgrounds/ProcessStripeWebhookEventJob.cs service/Api/src/Module/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.Loggers.cs service/Api/src/Module/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs service/Api/tests/Module.UnitTests/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntentTests.cs service/Api/src/Module/Ordering/Features/Storefront/CompleteCheckoutForPayment/CompleteCheckoutForPayment.cs service/Api/src/Module/Ordering/Services/CheckoutPlacementService.cs app/Store/src/features/ordering/views/CheckoutReturnView.vue` then `git commit -m "feat: add diagnostic logging across the payment flow"`.

### Implementation Phase 3: Tests + verification

- GOAL-003: Verify the build/tests and smoke the webhook flow locally.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-008 | Run full backend + SPA + convention verification and a manual `stripe listen` smoke. | | |

#### TASK-008: Full verification

- [ ] `dotnet build` — 0 warnings.
- [ ] `dotnet test service/Api/tests/Module.UnitTests` — all pass (CreatePaymentIntentTests updated in TASK-005).
- [ ] `cd app/Store && pnpm run lint && pnpm run build-only` — 0 lint errors, build 0 warnings (the 41 pre-existing test failures in unrelated files remain documented; not introduced here).
- [ ] `bash scripts/check-feature-conventions.sh` — all PASS.
- [ ] Manual smoke (requires Stripe keys): run AppHost (API on 5001) + `scripts/dev-stripe-listen.sh` with the whsec set; in the API log trace: intent created (SessionCreated) → pay on Stripe → EventRouted(`checkout.session.completed`) → SessionLookup(Found=true) → CheckoutSessionCompleted → OrderPlaced → order visible. Abandon a session → EventRouted(`checkout.session.expired`) → CheckoutSessionExpired → CartRegressedToDelivery → cart returns to the Delivery step in the SPA.
- [ ] Commit any final fixes.

## 3. Alternatives

- **ALT-001**: success_url / return-page verification (completes + places from the browser return) — rejected: not described in the thesis (PAY-FR-04 mandates the webhook as the completion source; UC-STR-PAY E3 says "webhook updates state"), and it would create a second, divergent completion path.
- **ALT-002**: Public tunnel (ngrok/cloudflared) + dashboard webhook endpoint — rejected: exposes the dev API publicly and needs a stable public URL; `stripe listen` forwards signed events locally with no public exposure.

## 4. Dependencies

- **DEP-001**: Stripe CLI (`stripe listen`) — dev-only webhook forwarding.
- **DEP-002**: Aspire — pinned API HTTPS port 5001 (`.WithHttpsEndpoint`).
- **DEP-003**: `ILogger` + `[LoggerMessage]` source generator (built-in) — structured logging.
- **DEP-004**: `service/Api/scripts/setup-dev-secrets.sh` — `STRIPE_WEBHOOK_SECRET` env passthrough for `GatewayProviders:stripe:WebhookSecret`.
- **DEP-005**: `dotnet user-secrets` (id `resys.shop.api`) — per-run `whsec_...` wiring.

## 5. Files

- **FILE-001**: `infra/Aspire/src/ReSys.AppHost/AppHost.cs` — pinned HTTPS port 5001.
- **FILE-002**: `scripts/dev-stripe-listen.sh` (new) — forward + whsec instructions.
- **FILE-003**: `service/Api/README.md` — "Stripe webhooks (local)" section.
- **FILE-004**: `service/Api/src/Module/Billing/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.Loggers.cs` — `WebhookSecretMissing`, `SignatureVerified`, `WebhookEventReceived`.
- **FILE-005**: `service/Api/src/Module/Billing/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.cs` — new log calls.
- **FILE-006**: `service/Api/src/Module/Billing/Backgrounds/ProcessStripeWebhookEventJob.Loggers.cs` — event/session/completion/expiry/regress logs.
- **FILE-007**: `service/Api/src/Module/Billing/Backgrounds/ProcessStripeWebhookEventJob.cs` — new log calls.
- **FILE-008**: `service/Api/src/Module/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.Loggers.cs` (new).
- **FILE-009**: `service/Api/src/Module/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs` — `ILogger` + log calls.
- **FILE-010**: `service/Api/tests/Module.UnitTests/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntentTests.cs` — constructor `NullLogger` arg.
- **FILE-011**: `service/Api/src/Module/Ordering/Features/Storefront/CompleteCheckoutForPayment/CompleteCheckoutForPayment.cs` — outcome logs.
- **FILE-012**: `service/Api/src/Module/Ordering/Services/CheckoutPlacementService.cs` — placement-start log.
- **FILE-013**: `app/Store/src/features/ordering/views/CheckoutReturnView.vue` — `console.debug` polling logs.

## 6. Testing

- **TEST-001**: `dotnet build` — 0 warnings (`TreatWarningsAsErrors`).
- **TEST-002**: `Module.UnitTests` full suite — all pass (constructor change in `CreatePaymentIntentTests` uses `NullLogger`).
- **TEST-003**: SPA `build-only` + lint — 0 warnings/errors.
- **TEST-004**: Manual `stripe listen` smoke — trace the full log chain (intent → event → lookup → completed → placed) and the expiry path (expired → regress).

## 7. Risks & Assumptions

- **RISK-001**: The `whsec_...` from `stripe listen` is per-run; restarting the CLI invalidates the configured secret until re-set (documented in the script).
- **RISK-002**: Pinning port 5001 could collide with another local service; choose a free port if needed (update the script + README to match).
- **RISK-003**: Logging must never include secret keys or full webhook payloads; log only ids, states, and event types.
- **RISK-004**: `ILogger<T>` constructor additions touch tests (CreatePaymentIntentTests) — handled in TASK-005 with `NullLogger`.
- **ASSUMPTION-001**: The API HTTPS port 5001 is acceptable for local dev.
- **ASSUMPTION-002**: The webhook is the single completion source; the return page only polls local state (thesis-aligned).

## 8. Related Specifications / Further Reading

- [Payment method selection design](docs/superpowers/specs/2026-08-13-payment-method-selection-design.md)
- [Thesis PAY-FR-04 / UC-STR-PAY E3](thesis/chapters/part2/ch2-design/01-requirements/01-functional-requirements.typ)
- [Stripe webhooks](https://docs.stripe.com/webhooks)
- [Stripe CLI listen](https://docs.stripe.com/stripe-cli)
