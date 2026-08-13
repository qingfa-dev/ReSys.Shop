---
goal: Make completed Stripe Checkout payments reflect locally via browser success_url verification (primary) and Stripe CLI webhook delivery (dev pipeline).
version: 1.0
date_created: 2026-08-13
last_updated: 2026-08-13
owner: Billing / Ordering / Store SPA / Aspire
status: 'Planned'
tags: [feature, billing, stripe, webhook, checkout, store]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

A Stripe Checkout payment succeeds on Stripe's hosted page, but the local app
never advances: the `PaymentCapture` stays `Processing` and the order is not
auto-placed, because Stripe's servers cannot reach `localhost` (no webhook
delivery) and the return page only polls local state. This plan fixes both ends:

- **Verify on return (`success_url`)**: a new authenticated storefront endpoint
  `POST api/storefront/cart/payment/intent/verify-session` verifies the returned
  Checkout Session against Stripe and, when the PaymentIntent is `succeeded`,
  completes the payment and auto-places the order. The `/checkout/return` page
  calls it with the `session_id` Stripe appends to the success URL.
- **Stripe CLI webhook delivery**: pin the API HTTPS port and add a
  `stripe listen` forwarder so the real `checkout.session.completed` /
  `checkout.session.expired` events reach the dev API (the production-shaped
  path, including expiry regression).

Both paths are idempotent with each other via the existing `Complete()` state
guard, `ProcessedStripeEventIds`, and the non-Draft no-op in
`CompleteCheckoutForPayment`.

## 1. Requirements & Constraints

- **REQ-001**: A new storefront endpoint verifies a Checkout Session by `session_id` and, when its PaymentIntent is `succeeded`, completes the `PaymentCapture` and places the order.
- **REQ-002**: The `/checkout/return` SPA page calls the verify endpoint once when `session_id` is present and stops polling when the payment is completed; it keeps the existing 2s/30-attempt poll as a fallback for async/3DS and the webhook path.
- **REQ-003**: A `stripe listen` forwarder delivers real Stripe events to the local API webhook endpoint so the full webhook pipeline (completion, expiry regression) can be tested locally.
- **SEC-001**: The verify endpoint is `RequireAuthorization()` + `RequireRateLimiting("payment")`; it only completes the payment whose `ResponseCode` matches the supplied `session_id` and whose `OrderId` matches the caller's order.
- **SEC-002**: Never trust the client-provided success alone — the backend always confirms the PaymentIntent status via Stripe (secret key) before completing.
- **CON-001**: `TreatWarningsAsErrors=true` — any C# warning fails the build.
- **CON-002**: Stripe operations go through the gateway abstraction (`IPaymentGatewayActionProvider` / `Gateway`); no direct `SessionService` usage outside `StripeGateway`.
- **CON-003**: Store SPA comments follow `app/Store/AGENTS.md` (`// Label: Sentence.`, `<!-- Section: Title — purpose -->`); lines under 100 chars.
- **CON-004**: Aspire orchestrates local dev; the API HTTPS port is pinned to 5001 for a stable webhook forward URL.
- **GUD-001**: The verify path reuses the existing cross-module `CompleteCheckoutForPaymentCommand` (via `ISender`) exactly like the webhook handler.
- **GUD-002**: On verification success, `ResponseCode` is rewritten to the PaymentIntent id so admin refund/void and `charge.*` webhooks correlate correctly (same behavior as the webhook handler).
- **PAT-001**: Gateway capability additions follow the existing pattern — abstract method on `Gateway` + interface, real impl in `StripeGateway`, fake impl in `BogusGateway`.

## 2. Implementation Steps

### Implementation Phase 1: Gateway session→PaymentIntent resolution

- GOAL-001: Add a gateway capability to resolve a Checkout Session id to its PaymentIntent id so the verify endpoint can check the payment status.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Add `GetSessionPaymentIntentIdAsync` to `IPaymentGatewayActionProvider` + abstract `Gateway`. | | |
| TASK-002 | Implement `GetSessionPaymentIntentIdAsync` in `StripeGateway`. | | |
| TASK-003 | Implement `GetSessionPaymentIntentIdAsync` in `BogusGateway`. | | |

#### TASK-001: Gateway contract method

**Files:**
- Modify: `service/Api/src/Module/Billing/Services/Provider/IPaymentGatewayActionProvider.cs` (after `GetPaymentStatusAsync`)
- Modify: `service/Api/src/Module/Billing/Services/Provider/Gateway.cs` (after `GetPaymentStatusAsync`)

**Interfaces:**
- Produces: `Task<Result<string?>> GetSessionPaymentIntentIdAsync(string sessionId, CancellationToken ct = default)`.

- [ ] Add to the interface:
```csharp
/// <summary>Resolves a hosted Checkout Session id to its PaymentIntent id.</summary>
Task<Result<string?>> GetSessionPaymentIntentIdAsync(
    string sessionId, CancellationToken ct = default);
```
- [ ] Add the matching abstract method to `Gateway`.
- [ ] Verify: `dotnet build service/Api/src/Api/Api.csproj` — expected FAIL (CS0534) until TASK-002/003 land; commit with TASK-002/003.

#### TASK-002: StripeGateway implementation

**Files:**
- Modify: `service/Api/src/Module/Billing/Services/Provider/Stripe/StripeGateway.cs` (after `GetPaymentStatusAsync`)

**Consumes:** `_sessionService` (already a field), `_options.SecretKey`.

- [ ] Add:
```csharp
public override async Task<Result<string?>> GetSessionPaymentIntentIdAsync(
    string sessionId, CancellationToken ct = default)
{
    try
    {
        var ro = new RequestOptions { ApiKey = _options.SecretKey };
        var session = await _sessionService.GetAsync(sessionId, null, ro, ct).ConfigureAwait(false);
        return Result<string?>.Ok(session.PaymentIntentId);
    }
    catch (StripeException ex) { return MapStripeException(ex); }
}
```
- [ ] Keep the existing `using Stripe.Checkout;`.

#### TASK-003: BogusGateway implementation

**Files:**
- Modify: `service/Api/src/Module/Billing/Services/Provider/Bogus/BogusGateway.cs` (after `GetPaymentStatusAsync`)

- [ ] Add:
```csharp
public override Task<Result<string?>> GetSessionPaymentIntentIdAsync(
    string sessionId, CancellationToken ct = default)
    => Task.FromResult(Result<string?>.Ok($"pi_fake_{Guid.NewGuid():N}"));
```
- [ ] Verify: `dotnet build service/Api/src/Api/Api.csproj` — 0 warnings.
- [ ] Commit all three files: `git add service/Api/src/Module/Billing/Services/Provider/IPaymentGatewayActionProvider.cs service/Api/src/Module/Billing/Services/Provider/Gateway.cs service/Api/src/Module/Billing/Services/Provider/Stripe/StripeGateway.cs service/Api/src/Module/Billing/Services/Provider/Bogus/BogusGateway.cs` then `git commit -m "feat(billing): resolve checkout session to payment intent id on gateways"`.

### Implementation Phase 2: VerifySession backend feature

- GOAL-002: Add the authenticated `verify-session` endpoint that completes the payment and auto-places the order when the PaymentIntent succeeded.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004 | Add route constant + Request/Response DTOs for `VerifySession`. | | |
| TASK-005 | Add `VerifySession` handler (verify → complete → place). | | |
| TASK-006 | Add `VerifySession` endpoint (`POST .../verify-session`, auth + rate limit). | | |

#### TASK-004: Route constant + DTOs

**Files:**
- Modify: `service/Api/src/Module/Billing/Features/Shared/BillingFeature.Storefront.cs` (inside `Payments`)
- Create: `service/Api/src/Module/Billing/Features/Storefront/Payment/VerifySession/VerifySession.Request.cs`
- Create: `service/Api/src/Module/Billing/Features/Storefront/Payment/VerifySession/VerifySession.Response.cs`

**Interfaces:**
- Produces: `VerifySession.Request { Guid OrderId; string SessionId }`, `VerifySession.Response : StorePaymentDetailResponse { bool IsCompleted }`, route `api/storefront/cart/payment/intent/verify-session`.

- [ ] Add to `BillingFeature.Storefront.Payments`:
```csharp
public static class VerifySession
{
    public const string Route = "api/storefront/cart/payment/intent/verify-session";
    public const string Description = "Verify a Stripe Checkout Session on return and complete the payment";
    public const string Summary = "Verify payment session";
}
```
- [ ] `VerifySession.Request.cs`:
```csharp
namespace Module.Billing.Features.Storefront.Payment.VerifySession;

public static partial class VerifySession
{
    public sealed record Request
    {
        public Guid OrderId { get; init; }
        public string SessionId { get; init; } = string.Empty;
    }
}
```
- [ ] `VerifySession.Response.cs`:
```csharp
using Module.Billing.Features.Storefront.Payment.Shared.Models;

namespace Module.Billing.Features.Storefront.Payment.VerifySession;

public static partial class VerifySession
{
    public sealed record Response : StorePaymentDetailResponse
    {
        public bool IsCompleted { get; init; }
    }
}
```

#### TASK-005: VerifySession handler

**Files:**
- Create: `service/Api/src/Module/Billing/Features/Storefront/Payment/VerifySession/VerifySession.cs`

**Consumes:** `CompleteCheckoutForPaymentCommand`, `IGatewayRegistry`, `PaymentCaptureMethod`, `PaymentStoreMapping.MapToStoreDetail<Response>`, `GatewayConstants`.

- [ ] Add:
```csharp
using Module.Billing.Features.Storefront.Payment.Shared.Mappings;
using Module.Ordering.Features.Storefront.CompleteCheckoutForPayment;

using Module.Billing.Domain.PaymentCaptures;
using Module.Billing.Services.Provider;

namespace Module.Billing.Features.Storefront.Payment.VerifySession;

/// <summary>Verifies a Stripe Checkout Session on return and, when succeeded, completes the payment and places the order.</summary>
public static partial class VerifySession
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        IGatewayRegistry gatewayRegistry,
        ISender sender)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var payment = await dbContext.Set<PaymentCapture>()
                .FirstOrDefaultAsync(p => p.ResponseCode == command.Request.SessionId
                                       && p.OrderId == command.Request.OrderId, cancellationToken);
            if (payment is null)
                return PaymentCaptureResult.Failure.NotFound;

            // Idempotent: already completed (webhook may have beaten us).
            if (payment.State == PaymentRecordState.Completed)
                return Map(payment);

            var gatewayResult = gatewayRegistry.GetGateway(GatewayConstants.Providers.Stripe);
            if (gatewayResult.IsFailure)
                return PaymentCaptureResult.Failure.ProviderNotRegistered(GatewayConstants.Providers.Stripe);
            var gateway = gatewayResult.Value;

            // Verify with Stripe — never trust the client's return alone (SEC-002).
            var piResult = await gateway.GetSessionPaymentIntentIdAsync(command.Request.SessionId, cancellationToken);
            if (piResult.IsFailure) return piResult.Errors;
            var paymentIntentId = piResult.Value;
            if (string.IsNullOrEmpty(paymentIntentId))
                return PaymentCaptureResult.Failure.NotFound;

            var status = await gateway.GetPaymentStatusAsync(paymentIntentId, cancellationToken);
            if (!string.Equals(status, GatewayConstants.Stripe.IntentStatus.Succeeded, StringComparison.Ordinal))
                return Map(payment); // not ready — SPA keeps polling

            // Correlate refunds/disputes against the PaymentIntent id.
            payment.ResponseCode = paymentIntentId;
            var complete = payment.Complete();
            if (complete.IsFailure) return complete.Errors;
            await dbContext.SaveChangesAsync(cancellationToken);

            await sender.Send(
                new CompleteCheckoutForPaymentCommand { CartId = payment.OrderId, PaymentId = payment.Id }, cancellationToken);

            return Map(payment);
        }

        private static Response Map(PaymentCapture payment)
        {
            var detail = payment.MapToStoreDetail<Response>();
            return detail with { IsCompleted = payment.State == PaymentRecordState.Completed };
        }
    }
}
```

#### TASK-006: VerifySession endpoint

**Files:**
- Create: `service/Api/src/Module/Billing/Features/Storefront/Payment/VerifySession/VerifySession.Endpoint.cs`

**Consumes:** `BillingFeature.Storefront.Payments.VerifySession.Route`, `Module.Billing.Features.Shared`.

- [ ] Add:
```csharp
using Module.Billing.Features.Shared;

namespace Module.Billing.Features.Storefront.Payment.VerifySession;

public static partial class VerifySession
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(BillingFeature.Storefront.Payments.VerifySession.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(request), ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .RequireRateLimiting("payment")
            .WithName(nameof(VerifySession))
            .WithTags(BillingFeature.Tags.Payment)
            .WithSummary(BillingFeature.Storefront.Payments.VerifySession.Summary)
            .WithDescription(BillingFeature.Storefront.Payments.VerifySession.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result<Response>>(StatusCodes.Status404NotFound);
        }
    }
}
```
- [ ] Verify: `dotnet build service/Api/src/Api/Api.csproj` — 0 warnings.
- [ ] Commit: `git add service/Api/src/Module/Billing/Features/Shared/BillingFeature.Storefront.cs service/Api/src/Module/Billing/Features/Storefront/Payment/VerifySession/` then `git commit -m "feat(billing): verify checkout session on return and complete payment"`.

### Implementation Phase 3: Store SPA return verification

- GOAL-003: Have `/checkout/return` verify the session once and fall back to polling.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-007 | Add `verifyPaymentSession` API client function + type. | | |
| TASK-008 | Update `CheckoutReturnView.vue` to call verify once when `session_id` present. | | |

#### TASK-007: SPA verify API

**Files:**
- Modify: `app/Store/src/features/payment/services/paymentApi.ts`
- Modify: `app/Store/src/features/payment/types/payment.ts`

**Interfaces:**
- Produces: `verifyPaymentSession({ orderId, sessionId }): Promise<Result<PaymentStatusResponse>>`.

- [ ] In `paymentApi.ts`:
```ts
// Call: Storefront payment API - verify a Stripe Checkout Session on return.
export function verifyPaymentSession(req: { orderId: string; sessionId: string }): Promise<Result<PaymentStatusResponse>> {
  return post<Result<PaymentStatusResponse>>('/api/storefront/cart/payment/intent/verify-session', req)
}
```
(`post` is already imported; `PaymentStatusResponse` already imported.)

#### TASK-008: CheckoutReturnView verification call

**Files:**
- Modify: `app/Store/src/features/ordering/views/CheckoutReturnView.vue`

**Consumes:** `verifyPaymentSession`, `route.query.session_id`.

- [ ] Import and add a verify step in `onMounted` (before starting the poll):
```ts
import { getPaymentStatus, verifyPaymentSession } from '@/features/payment/services/paymentApi'

// Verify: Ask the backend to confirm the session with Stripe on first load.
async function verifyOnce(): Promise<void> {
  const orderId = typeof route.query.order === 'string' ? route.query.order : null
  const sessionId = typeof route.query.session_id === 'string' ? route.query.session_id : null
  if (!orderId || !sessionId) return
  const result = await verifyPaymentSession({ orderId, sessionId })
  if (result.isSuccess && result.value.isCompleted) {
    status.value = 'completed'
    stopPolling()
  }
}
```
- [ ] Call `await verifyOnce()` at the top of `onMounted` before starting the interval.
- [ ] Verify from `app/Store`: `pnpm run build-only` (0 warnings) and targeted lint on the two files.
- [ ] Commit: `git add app/Store/src/features/payment/services/paymentApi.ts app/Store/src/features/ordering/views/CheckoutReturnView.vue` then `git commit -m "feat(store): verify stripe session on checkout return"`.

### Implementation Phase 4: Stripe CLI webhook delivery

- GOAL-004: Deliver real Stripe webhook events to the local API for testing the full pipeline.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Pin the API HTTPS port to 5001 in the Aspire AppHost. | | |
| TASK-010 | Add `scripts/dev-stripe-listen.sh` + document the whsec wiring. | | |

#### TASK-009: Pin API HTTPS port

**Files:**
- Modify: `infra/Aspire/src/ReSys.AppHost/AppHost.cs` (the `api` resource)

- [ ] On the `api` builder add a fixed HTTPS endpoint (keep `.WithExternalHttpEndpoints()`):
```csharp
    .WithHttpsEndpoint(port: 5001, name: "https")
    .WithExternalHttpEndpoints()
```
- [ ] Verify: `dotnet build infra/Aspire/src/ReSys.AppHost` — 0 warnings; the API is reachable at `https://localhost:5001`.

#### TASK-010: stripe listen script + docs

**Files:**
- Create: `scripts/dev-stripe-listen.sh` (executable)
- Modify: `service/Api/README.md` (or `service/Api/src/Migrations/GUIDE.yaml` sibling README) — add a "Stripe webhooks (local)" section

- [ ] `scripts/dev-stripe-listen.sh`:
```bash
#!/usr/bin/env bash
# Forward real Stripe webhook events to the local API (Aspire pins the API HTTPS port to 5001).
# Usage: STRIPE_SECRET_KEY=sk_test_... ./scripts/dev-stripe-listen.sh
set -euo pipefail
if [ -z "${STRIPE_SECRET_KEY:-}" ]; then
  echo "Set STRIPE_SECRET_KEY (sk_test_...) first." >&2
  exit 1
fi
echo "Forwarding Stripe events to https://localhost:5001/api/storefront/billing/webhooks/stripe"
echo "Copy the printed 'webhook signing secret' (whsec_...) into GatewayProviders:stripe:WebhookSecret:"
echo "  dotnet user-secrets set \"GatewayProviders:stripe:WebhookSecret\" \"<whsec_...>\" --project service/Api/src/Api/Api.csproj"
echo "The secret is per stripe listen run - keep this process running and re-set it if you restart."
stripe listen --forward-to https://localhost:5001/api/storefront/billing/webhooks/stripe --api-key "$STRIPE_SECRET_KEY"
```
- [ ] README section: run `scripts/dev-stripe-listen.sh`, set the printed `whsec_...` via `dotnet user-secrets set` (or `STRIPE_WEBHOOK_SECRET` in `setup-dev-secrets.sh`), restart the API; abandoned sessions now fire `checkout.session.expired` (cart regresses to `Delivery`) and paid sessions fire `checkout.session.completed` (auto-place).
- [ ] Commit: `git add scripts/dev-stripe-listen.sh service/Api/README.md infra/Aspire/src/ReSys.AppHost/AppHost.cs` then `git commit -m "chore: pin api https port and add stripe listen dev script"`.

### Implementation Phase 5: Tests + verification

- GOAL-005: Cover the verify handler and gateway capability with unit tests, then run the full verification suite.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-011 | Add unit tests for `VerifySession` handler + `GetSessionPaymentIntentIdAsync`. | | |
| TASK-012 | Run full backend + SPA + convention verification. | | |

#### TASK-011: Unit tests

**Files:**
- Create: `service/Api/tests/Module.UnitTests/Billing/Features/Storefront/Payment/VerifySession/VerifySessionTests.cs`
- Modify: `service/Api/tests/Module.UnitTests/Billing/Backgrounds/ProcessStripeWebhookEventJobTests.cs` (no change — gateway methods covered here or in the new file)

**Tests (InMemory db, Mock<IGatewayRegistry> + Mock<IPaymentGatewayActionProvider> + Mock<ISender>, mirror `CreatePaymentIntentTests` fixtures):**
- [ ] `Handle_SessionSucceeded_CompletesAndPlacesOrder` — seed a `PaymentCapture` (`ProviderKey=stripe`, `ResponseCode="cs_verify_1"`, `State=Processing`, `OrderId=X`); mock `GetSessionPaymentIntentIdAsync` → `"pi_verify_1"`, `GetPaymentStatusAsync` → `"succeeded"`; assert payment becomes `Completed`, `ResponseCode == "pi_verify_1"`, and `CompleteCheckoutForPaymentCommand { CartId = X, PaymentId = payment.Id }` sent once.
- [ ] `Handle_SessionNotSucceeded_ReturnsPending` — `GetPaymentStatusAsync` → `"requires_payment_method"`; assert payment stays `Processing` and no command sent.
- [ ] `Handle_AlreadyCompleted_IsIdempotent` — seed `State=Completed`; assert no gateway call and no command sent.
- [ ] `Handle_MissingPayment_ReturnsNotFound` — no matching capture; assert failure.
- [ ] `StripeGateway.GetSessionPaymentIntentIdAsync` — covered via the mock contract in the handler tests; add a `BogusGateway` direct test asserting it returns a non-empty `pi_fake_...` (optional).

#### TASK-012: Full verification

- [ ] `dotnet build` — 0 warnings.
- [ ] `dotnet test service/Api/tests/Module.UnitTests` — all pass.
- [ ] `cd app/Store && pnpm run lint && pnpm run build-only && pnpm run test:unit` — 0 lint errors, build 0 warnings (the 41 pre-existing test failures in unrelated files remain documented; not introduced here).
- [ ] `bash scripts/check-feature-conventions.sh` — all PASS.
- [ ] Manual smoke (requires Stripe keys): run AppHost (API on 5001), `scripts/dev-stripe-listen.sh`, pay via Checkout → `/checkout/return` verifies + order placed; abandon a session → cart regresses to `Delivery`.
- [ ] Commit any final fixes.

## 3. Alternatives

- **ALT-001**: Webhook-only (Stripe CLI), no return verification — rejected: local dev would stay blocked until the CLI + secret wiring is complete, and the return page gives no fast confirmation.
- **ALT-002**: Trust the `success_url` return without calling Stripe — rejected (SEC-002): a forged/old return could mark a payment complete without an actual charge.
- **ALT-003**: Rely on the SPA calling the storefront `ConfirmPayment` endpoint instead of a Stripe-verified session check — rejected: `ConfirmPayment` only inspects local state and does not confirm against Stripe.

## 4. Dependencies

- **DEP-001**: Stripe.net 52.1.0 — `SessionService.GetAsync` (existing package).
- **DEP-002**: `IGatewayRegistry` / `IPaymentGatewayActionProvider` (existing) — new session→PI method.
- **DEP-003**: `CompleteCheckoutForPaymentCommand` (Ordering, existing) — auto-place via `ISender`.
- **DEP-004**: `PaymentStoreMapping.MapToStoreDetail<Response>` (existing) — response mapping.
- **DEP-005**: Stripe CLI (`stripe listen`) — dev-only webhook forwarding.
- **DEP-006**: Aspire — pinned API HTTPS port 5001.

## 5. Files

- **FILE-001**: `service/Api/src/Module/Billing/Services/Provider/IPaymentGatewayActionProvider.cs` — `GetSessionPaymentIntentIdAsync`.
- **FILE-002**: `service/Api/src/Module/Billing/Services/Provider/Gateway.cs` — abstract method.
- **FILE-003**: `service/Api/src/Module/Billing/Services/Provider/Stripe/StripeGateway.cs` — session→PI impl.
- **FILE-004**: `service/Api/src/Module/Billing/Services/Provider/Bogus/BogusGateway.cs` — fake impl.
- **FILE-005**: `service/Api/src/Module/Billing/Features/Shared/BillingFeature.Storefront.cs` — `VerifySession` route.
- **FILE-006**: `service/Api/src/Module/Billing/Features/Storefront/Payment/VerifySession/` (new) — Request/Response/Command/Endpoint/Handler.
- **FILE-007**: `app/Store/src/features/payment/services/paymentApi.ts` — `verifyPaymentSession`.
- **FILE-008**: `app/Store/src/features/ordering/views/CheckoutReturnView.vue` — verify-once on return.
- **FILE-009**: `infra/Aspire/src/ReSys.AppHost/AppHost.cs` — pinned HTTPS port 5001.
- **FILE-010**: `scripts/dev-stripe-listen.sh` (new) + `service/Api/README.md` docs.
- **FILE-011**: `service/Api/tests/Module.UnitTests/Billing/Features/Storefront/Payment/VerifySession/VerifySessionTests.cs` (new).

## 6. Testing

- **TEST-001**: `VerifySessionTests` — succeeded → completed + order placed; not-succeeded → pending; already-completed → idempotent; missing → NotFound.
- **TEST-002**: Gateway `GetSessionPaymentIntentIdAsync` — Stripe (session→PI) and Bogus (fake) contracts via handler tests / direct Bogus test.
- **TEST-003**: SPA — `CheckoutReturnView` verify-once path (mock `verifyPaymentSession` returning `isCompleted: true` → completed state).
- **TEST-004**: Manual — `stripe listen` forwards `checkout.session.completed`/`expired`; paid session auto-places order; abandoned session regresses cart to `Delivery`.

## 7. Risks & Assumptions

- **RISK-001**: The `whsec_...` from `stripe listen` is per-run; restarting the CLI invalidates the configured secret until re-set.
- **RISK-002**: Pinning port 5001 could collide with another local service; choose a free port if needed.
- **RISK-003**: Verify + webhook racing is safe (state guard, event-id, non-Draft no-op) but both completing must remain idempotent — covered by tests.
- **ASSUMPTION-001**: The Store SPA can reach the API's verify route with the same auth used for `create-intent`.
- **ASSUMPTION-002**: For card Checkout Sessions the PaymentIntent is `succeeded` immediately on completion; async methods keep polling.
- **ASSUMPTION-003**: The API HTTPS port 5001 is acceptable for local dev.

## 8. Related Specifications / Further Reading

- [Payment method selection design](docs/superpowers/specs/2026-08-13-payment-method-selection-design.md)
- [Stripe webhooks](https://docs.stripe.com/webhooks)
- [Stripe CLI listen](https://docs.stripe.com/stripe-cli)
- [Stripe.net SessionService](https://github.com/stripe/stripe-dotnet)
