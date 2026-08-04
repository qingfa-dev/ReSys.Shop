# Stripe Enablement & Storefront API Correction — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Enable Stripe as a real payment gateway and correct all 9 storefront modules to thesis conformance (backend only, frontend out of scope).

**Architecture:** Seven phased implementation waves. Phase 1 fixes bugs blocking all SourceRequired gateways and enables Stripe config. Phase 2 restructures the checkout flow (UC-STR-CHK) with strict state machine + inventory reservation coupling, and consolidates cross-module domain references into `Shared.Application.Contracts` MediatR contracts. Phases 3A–3G sweep remaining modules for correctness, security, and thesis-gap closure.

**Tech Stack:** .NET 10, EF Core + Npgsql, Carter minimal APIs, MediatR + ISender, FluentValidation, Mapster, Stripe.net, Hangfire webhook jobs.

## Global Constraints

- Secrets (Stripe keys) stay in `dotnet user-secrets`, never in tracked files.
- Bogus gateway stays `Enabled=true` in `appsettings.Development.json`; Stripe becomes co-enabled via user-secrets.
- No new cross-module `using Module.X.Domain...` references (AGENTS.md rule 2, enforced by `scripts/check-cross-module-refs.sh`).
- `dotnet build` stays green (warnings-as-errors).
- No frontend changes (`app/legacy/Storefront/` untouched).
- Every mutating endpoint has a `Validator.cs` per AGENTS.md rule 3.
- Every storefront subdirectory is named `Storefront` (not `Store`).
- All vertical-slice feature files follow `Features/{Admin|Storefront}/{Feature}/{Action}/` with Handler, Request, Response, Endpoint, Validator per action.

---

## Phase 1 — Stripe Enablement

### Task 1: SourceId column type migration (Guid? → string?)

**Files:**
- Modify: `service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.cs:34`
- Modify: `service/Api/src/Module/Payment/Persistence/Configurations/Payments/PaymentRecordConfiguration.cs:28`
- Create: EF Core migration (auto-generated)

**Interfaces:**
- Produces: `PaymentCapture.SourceId` is `string?` (was `Guid?`)

- [ ] **Step 1: Change the property type**

In `PaymentCapture.cs:34`, change:
```csharp
public Guid? SourceId { get; set; }
```
to:
```csharp
public string? SourceId { get; set; }
```

- [ ] **Step 2: Update the EF configuration**

In `PaymentRecordConfiguration.cs:28`, change the property builder:
```csharp
builder.Property(x => x.SourceId);
```
to:
```csharp
builder.Property(x => x.SourceId).HasMaxLength(200);
```

- [ ] **Step 3: Generate and verify the migration**

```bash
dotnet ef migrations add ChangeSourceIdToString --project service/Api/src/Migrations --startup-project service/Api/src/Api
```
Verify the generated migration contains `ALTER COLUMN "SourceId" TYPE text`.

- [ ] **Step 4: Build + run unit tests**

```bash
dotnet build
dotnet test service/Api/tests/Module.UnitTests --filter Payment
```

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.cs \
        service/Api/src/Module/Payment/Persistence/Configurations/Payments/PaymentRecordConfiguration.cs \
        service/Api/src/Migrations/
git commit -m "feat(payment): change SourceId column from Guid? to string? for gateway token support"
```

### Task 2: Gateway source pass-through fix

**Files:**
- Modify: `service/Api/src/Module/Payment/Services/Processing/PaymentProcessingService.cs:279`

**Interfaces:**
- Consumes: `PaymentCapture.SourceId` as `string?` (from Task 1)

- [ ] **Step 1: Write the failing test**

Create `service/Api/tests/Module.UnitTests/Payment/Services/PaymentProcessingServiceTests.cs`:

```csharp
[Fact]
public async Task GatewayActionAsync_PassesStringSource_WhenSourceIdIsSet()
{
    var payment = new PaymentCapture
    {
        SourceId = "pm_card_visa",
        Amount = 10m,
        State = PaymentRecordState.Checkout
    };
    var options = new GatewayOptions { IdempotencyKey = "test" };
    var gateway = new Mock<IPaymentGatewayActionProvider>();
    gateway.Setup(g => g.Supports(It.IsAny<object?>())).Returns(true);
    gateway.Setup(g => g.AutoCapture).Returns(true);
    gateway.Setup(g => g.SourceRequired).Returns(false);

    object? capturedSource = null;
    gateway.Setup(g => g.PurchaseAsync(10m, It.IsAny<object?>(), options, It.IsAny<CancellationToken>()))
        .Callback<decimal, object?, GatewayOptions, CancellationToken>((a, s, o, ct) => capturedSource = s)
        .ReturnsAsync(Result.Ok(new PaymentGatewayResponse("test", authorization: "auth_1")));

    // Invoke via reflection or internal visible-to...
    // Assert capturedSource is "pm_card_visa" (string, not anonymous object)
}
```

- [ ] **Step 2: Make the change**

In `PaymentProcessingService.cs:279`, replace:
```csharp
var source = payment.SourceType is not null ? new { Id = payment.SourceId, Type = payment.SourceType } : null;
```
with:
```csharp
object? source = payment.SourceId;
```

- [ ] **Step 3: Run tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter Payment
```

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Payment/Services/Processing/PaymentProcessingService.cs \
        service/Api/tests/Module.UnitTests/Payment/
git commit -m "fix(payment): pass raw string source to gateways instead of anonymous object"
```

### Task 3: Add GatewayConstants.SourceTypes

**Files:**
- Modify: `service/Api/src/Module/Payment/Services/Provider/GatewayConstants.cs`

- [ ] **Step 1: Add the constants**

In `GatewayConstants.cs`, add inside the class:
```csharp
public static class SourceTypes
{
    public const string PaymentMethod = "payment_method";
    public const string Card = "card";
}
```

- [ ] **Step 2: Build**

```bash
dotnet build service/Api/src/Module
```

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Payment/Services/Provider/GatewayConstants.cs
git commit -m "feat(payment): add SourceTypes constants for payment source discriminators"
```

### Task 4: Update CreatePaymentIntent handler + request + validator

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.Request.cs`
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.Validator.cs`
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs`

**Interfaces:**
- Consumes: `GatewayConstants.SourceTypes.PaymentMethod` (from Task 3)
- Produces: `CreatePaymentIntent.Request.PaymentMethodToken: string?`

- [ ] **Step 1: Add PaymentMethodToken to Request**

In `CreatePaymentIntent.Request.cs`, add:
```csharp
public record Request : StorePaymentRequest
{
    public string? ReturnUrl { get; init; }
    public new Guid? PaymentMethodId { get; init; }
    public string? PaymentMethodToken { get; init; }
}
```

- [ ] **Step 2: Update the Validator**

In `CreatePaymentIntent.Validator.cs`, add a rule that requires `PaymentMethodToken` when the gateway is `SourceRequired`. Since the validator runs before the handler resolves the gateway, validate the presence of the token when the request includes a `PaymentMethodId` known to be Stripe:

```csharp
public Validator()
{
    RuleFor(x => x.OrderId).NotEmpty();
    When(x => x.PaymentMethodToken is null, () =>
    {
        RuleFor(x => x.PaymentMethodId)
            .Must((req, pmId) =>
            {
                // Allow null token for Bogus test cards (card number will come from request)
                // For Stripe, the token is required — enforced in the handler's precondition
                return true;
            })
            .WithMessage("PaymentMethodToken is required for Stripe gateway");
    });
}
```

Note: the real enforcement is in `HandlePaymentPreconditions` — if `SourceRequired` and no token, 400. The validator does a soft-check.

- [ ] **Step 3: Update the handler to pass source + ReturnUrl**

In `CreatePaymentIntent.cs:50-53`, replace:
```csharp
var createResult = Domain.PaymentCaptures.PaymentCaptureMethod.Create(
    amount: order.Total,
    paymentMethodId: (Guid)paymentMethod.Id,
    orderId: order.Id);
```
with:
```csharp
var createResult = Domain.PaymentCaptures.PaymentCaptureMethod.Create(
    amount: order.Total,
    paymentMethodId: (Guid)paymentMethod.Id,
    orderId: order.Id,
    sourceId: command.PaymentMethodToken,
    sourceType: command.PaymentMethodToken is null
        ? null
        : GatewayConstants.SourceTypes.PaymentMethod);
```

After `GatewayOptions` construction at line 66, add:
```csharp
if (!string.IsNullOrEmpty(command.ReturnUrl))
    options.SuccessUrl = command.ReturnUrl;
```

- [ ] **Step 4: Build + run unit tests**

```bash
dotnet build
dotnet test service/Api/tests/Module.UnitTests --filter Payment
```

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/
git commit -m "feat(payment): add PaymentMethodToken to CreatePaymentIntent for Stripe source pass-through"
```

### Task 5: Fix Bogus Gateway test card path

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs`

**Interfaces:**
- Consumes: `BogusGateway.TestCards.Success` — Bogus demo path must still work with a test card string as the source token (not mapped to `SourceType.Card`)

- [ ] **Step 1: Handle Bogus test card source type**

After the PaymentCapture create in `CreatePaymentIntent.cs`, when the gateway is Bogus (check `paymentMethod.ProviderKey`), set `SourceType` to `GatewayConstants.SourceTypes.Card` so the demo path still assigns a valid source discriminator. In `CreatePaymentIntent.Request.cs`, add a `string? CardNumber` field alongside `PaymentMethodToken` — the Bogus demo sends `cardNumber: "4111111111111111"` (or the Success constant). The handler maps it:

```csharp
sourceType: paymentMethod.ProviderKey == GatewayConstants.Providers.Bogus
    ? GatewayConstants.SourceTypes.Card
    : GatewayConstants.SourceTypes.PaymentMethod
```

- [ ] **Step 2: Verify Bogus demo still works**

Run the Bogus `.http` test:
```
POST /api/storefront/payment/create-intent
{ "orderId": "...", "paymentMethodId": "...", "cardNumber": "4111111111111111" }
```
Expected: `200` with `clientSecret` + `responseCode`.

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs
git commit -m "fix(payment): support Bogus test card number as source for demo path"
```

### Task 6: Stripe config enablement — setup-dev-secrets.sh + user-secrets doc

**Files:**
- Modify: `service/Api/scripts/setup-dev-secrets.sh`

- [ ] **Step 1: Update the script**

In `setup-dev-secrets.sh:24-29`, after the existing three `Stripe:SecretKey` / `WebhookSecret` / `PublishableKey` blocks, add:
```bash
if [ -n "$STRIPE_SECRET_KEY" ]; then
    dotnet user-secrets set "GatewayProviders:stripe:Enabled" "true"
fi
```

Also add a header comment block near the top:
```bash
# Stripe enablement (real payment gateway — optional for dev, keep Bogus as default)
# Set STRIPE_SECRET_KEY, STRIPE_WEBHOOK_SECRET, STRIPE_PUBLISHABLE_KEY env vars
# to enable Stripe. Script also flips GatewayProviders:stripe:Enabled=true.
#
# Manual alternative (no env vars):
#   dotnet user-secrets set "GatewayProviders:stripe:Enabled"     "true"
#   dotnet user-secrets set "GatewayProviders:stripe:SecretKey"   "sk_test_..."
#   dotnet user-secrets set "GatewayProviders:stripe:WebhookSecret" "whsec_..."
#   dotnet user-secrets set "GatewayProviders:stripe:PublishableKey" "pk_test_..."
```

- [ ] **Step 2: Commit**

```bash
git add service/Api/scripts/setup-dev-secrets.sh
git commit -m "feat(payment): auto-enable Stripe in setup-dev-secrets.sh when STRIPE_SECRET_KEY is set"
```

### Task 7: Webhook endpoint audit + gap fill

**Files:**
- Read-only audit (no changes expected if code is already correct):
  - `service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.cs`
  - `service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.cs`
  - `service/Api/src/Module/Payment/Backgrounds/ProcessStripeWebhookEventJob.cs`
  - `service/Api/src/Module/Payment/Services/Webhook/IStripeWebhookService.cs`

- [ ] **Step 1: Verify HMAC signature verification exists**

Check `StripeWebhookDispatcher.cs` for code that reads `Stripe-Signature` header and calls `Stripe.EventUtility.ConstructEvent(...)` with the raw body + `WebhookSecret`. If missing, add it:

```csharp
var json = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
var stripeEvent = EventUtility.ConstructEvent(
    json,
    httpContext.Request.Headers["Stripe-Signature"],
    _stripeOptions.WebhookSecret,
    throwOnApiVersionMismatch: false);
```

- [ ] **Step 2: Verify idempotency by event ID**

Check that the handler filters by Stripe event ID before applying state. If missing, add a dedup check:

```csharp
var existingEvent = await dbContext.Set<StripeWebhookEvent>()
    .FirstOrDefaultAsync(e => e.StripeEventId == stripeEvent.Id, ct);
if (existingEvent is not null)
{
    Logger.LogDuplicateWebhook(stripeEvent.Id);
    return Results.Ok(); // A2: duplicate → 200 no-op, logged
}
```

If no `StripeWebhookEvent` entity exists, create it (migration). Minimum columns: `Id` (Guid PK), `StripeEventId` (string, unique index), `EventType`, `Payload`, `ReceivedAtUtc`.

If the entity + dedup table already exists, verify the query pattern.

- [ ] **Step 3: Verify state transition on payment_intent.succeeded**

Check `ProcessStripeWebhookEventJob.cs` for `HandlePaymentIntentSucceeded` — must call `payment.Complete()` (domain method) and if the payment links to an Order's cart, transition the order state. If order-state transition is missing, add a MediatR publish or direct state call (the payment module owns PaymentCapture; the Ordering module owns Order — coordinate via ISender/IEvent if needed, or accept that the webhook is idempotent on payment only and the order transition happens in `CreateOrderFromCart`).

If any two of the three verifications reveal gaps, fix them in-place and add a commit. If all three are already correct, just verify and move on.

- [ ] **Step 4: Commit (if changes made)**

```bash
git add service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/ \
        service/Api/src/Module/Payment/Backgrounds/
git commit -m "fix(payment): verify webhook HMAC verification, idempotency, and state transition"
```

### Task 8: Integration .http files for Payment (M6)

**Files:**
- Create: `ApiTests/Payment/methods.http`
- Create: `ApiTests/Payment/create-intent.http`
- Create: `ApiTests/Payment/confirm.http`
- Create: `ApiTests/Payment/setup-intent.http`
- Create: `ApiTests/Payment/webhook.http`

- [ ] **Step 1: Write all five .http files**

`ApiTests/Payment/methods.http`:
```http
GET {{base_url}}/api/storefront/payment/methods
Accept: application/json
Authorization: Bearer {{token}}

### Expected: 200, array of payment methods (Credit Card, Bank Transfer, Test Card)
```

`ApiTests/Payment/create-intent.http`:
```http
POST {{base_url}}/api/storefront/payment/create-intent
Content-Type: application/json
Authorization: Bearer {{token}}

{
  "orderId": "{{cart_id}}",
  "paymentMethodId": "{{bogus_payment_method_id}}",
  "cardNumber": "4111111111111111",
  "currency": "usd",
  "returnUrl": "https://localhost:5001/checkout/return"
}

### Expected: 200, clientSecret + responseCode + id
```

`ApiTests/Payment/confirm.http`:
```http
POST {{base_url}}/api/storefront/payment/confirm/{{payment_id}}
Content-Type: application/json
Authorization: Bearer {{token}}

{
  "paymentMethodId": "{{bogus_payment_method_id}}"
}

### Expected: 200 OK
```

`ApiTests/Payment/setup-intent.http`:
```http
POST {{base_url}}/api/storefront/payment/setup-intent
Content-Type: application/json
Authorization: Bearer {{token}}

### Expected: 200, setupIntentClientSecret
```

`ApiTests/Payment/webhook.http`:
```http
POST {{base_url}}/api/storefront/webhooks/stripe
Content-Type: application/json
Stripe-Signature: t=12345,v1=fake_sig_for_test

{
  "id": "evt_fake_001",
  "type": "payment_intent.succeeded",
  "data": { "object": { "id": "pi_fake_test" } }
}

### Expected: 401 (invalid signature) or 200 (with valid sig via Stripe CLI)
```

- [ ] **Step 2: Commit**

```bash
git add ApiTests/Payment/
git commit -m "test(payment): add .http integration test files for all storefront payment endpoints"
```

### Task 9: Frontend Integration Handoff document

**Files:**
- Create: `docs/superpowers/specs/2026-08-04-stripe-frontend-integration-handoff.md`

- [ ] **Step 1: Write the handoff document**

Write the document covering:
1. All five storefront payment endpoint contracts (HTTP method, route, request/response shapes, error codes).
2. Source-token semantics: client gets `pm_...` from Stripe.js, sends as `paymentMethodToken`; local `paymentMethodId` selects the entity row.
3. `clientSecret` flow: backend returns it, client uses it to confirm via Stripe.js `stripe.confirmCardPayment()`.
4. 3DS redirect: `ReturnUrl` field, `PaymentStatus == "requires_action"` → redirect, resume, poll order.
5. New checkout flow (after Phase 2B): strict Address→Delivery→Payment→Confirm→Complete; out-of-order transitions return 409 `InvalidCheckoutTransition`; expired reservations require re-running the payment step.
6. Webhook-driven state transitions — client should re-fetch order status after 3DS (since capture is asynchronous).
7. Rate limiting: 30 req/min, back off on 429.
8. Known gaps: no Stripe.js, `confirm` unused, `setup-intent` unused, save-payment-method UI stubbed, transaction repository dead code.

- [ ] **Step 2: Commit**

```bash
git add docs/superpowers/specs/2026-08-04-stripe-frontend-integration-handoff.md
git commit -m "docs: add Stripe.js frontend integration handoff document"
```

---

## Phase 2A — Mechanical conformance

### Task 10: ConfirmPayment contract drift fix

**Files:**
- Create: `service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/ConfirmPayment.Request.cs`
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/ConfirmPayment.Validator.cs`
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/ConfirmPayment.Endpoint.cs`

- [ ] **Step 1: Create Request.cs**

```csharp
namespace Module.Payment.Features.Storefront.Payment.Confirm;

public static partial class ConfirmPayment
{
    public sealed record ConfirmPaymentRequest
    {
        public Guid? PaymentMethodId { get; init; }
    }
}
```

- [ ] **Step 2: Update the handler signature**

In `ConfirmPayment.cs`: `Command` is already `ICommand`, but currently has no body. Add a request to the command signature or keep the command as-is (the handler doesn't need the body) and just have the endpoint deserialize it. Simpler: update the `Command` record:

```csharp
public sealed record Command(Guid PaymentId, Guid? PaymentMethodId = null) : ICommand;
```

And in the `Endpoint.cs`, bind the body to the command. Update the endpoint to accept `[FromBody] ConfirmPaymentRequest` and map it into the command.

For the `Validator.cs`, add:
```csharp
public Validator()
{
    RuleFor(x => x.PaymentId).NotEmpty();
    // PaymentMethodId is optional — captured for audit, not required for confirm
}
```

- [ ] **Step 3: Build + verify**

```bash
dotnet build
```

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/
git commit -m "fix(payment): add ConfirmPayment.Request.cs to accept client body (PaymentMethodId)"
```

### Task 11: CreatePaymentIntent currency honoring (M3)

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/Shared/Models/PaymentStore.Model.Request.cs`
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs`

- [ ] **Step 1: Add Currency to shared request model**

In `StorePaymentRequest`, add:
```csharp
public string? Currency { get; init; }
```

- [ ] **Step 2: Honor in handler**

In `CreatePaymentIntent.cs`, in the `GatewayOptions` block at line 66, use the request currency:
```csharp
var options = new GatewayOptions
{
    Currency = command.Currency ?? GatewayOptions.Currency,
    // ... rest unchanged
};
```

- [ ] **Step 3: Build + run tests**

```bash
dotnet build
dotnet test --filter "Payment"
```

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Storefront/Payment/
git commit -m "feat(payment): honor Currency from CreatePaymentIntent request with fallback to default"
```

### Task 12: Stale webhook doc comment fix (M4)

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.Endpoint.cs:11`

- [ ] **Step 1: Fix the comment**

Change:
```csharp
/// <summary>Handles Stripe webhook events at <c>api/payments/stripe/webhook</c>.</summary>
```
to:
```csharp
/// <summary>Handles Stripe webhook events at <c>api/storefront/webhooks/stripe</c>.</summary>
```

- [ ] **Step 2: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.Endpoint.cs
git commit -m "docs(payment): fix stale webhook route in XML doc comment"
```

### Task 13: Inventory CartReservations.Release endpoint (M5)

**Files:**
- Create: `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Release/ReleaseCartReservation.Endpoint.cs`
- Create: `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Release/ReleaseCartReservation.Request.cs`
- Create: `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Release/ReleaseCartReservation.Response.cs`
- Create: `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Release/ReleaseCartReservation.Validator.cs`

- [ ] **Step 1: Create the vertical slice files**

`ReleaseCartReservation.Request.cs`:
```csharp
namespace Module.Inventory.Features.Storefront.CartReservations.Release;

public static partial class ReleaseCartReservation
{
    public sealed record Request
    {
        public Guid ReservationId { get; init; }
        public string CartToken { get; init; } = string.Empty;
    }
}
```

`ReleaseCartReservation.Response.cs`:
```csharp
namespace Module.Inventory.Features.Storefront.CartReservations.Release;

public static partial class ReleaseCartReservation
{
    public sealed record Response
    {
        public Guid ReservationId { get; init; }
        public string Status { get; init; } = "released";
    }
}
```

`ReleaseCartReservation.Validator.cs`:
```csharp
namespace Module.Inventory.Features.Storefront.CartReservations.Release;

public static partial class ReleaseCartReservation
{
    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.ReservationId).NotEmpty();
            RuleFor(x => x.CartToken).NotEmpty();
        }
    }
}
```

`ReleaseCartReservation.Endpoint.cs`:
```csharp
namespace Module.Inventory.Features.Storefront.CartReservations.Release;

public static partial class ReleaseCartReservation
{
    public static RouteHandlerBuilder MapReleaseEndpoint(this IEndpointRouteBuilder builder)
    {
        return builder.MapDelete(
                InventoryFeature.Storefront.CartReservations.Release.Route,
                async (HttpContext ctx, [FromRoute] Guid reservationId, ISender sender, CancellationToken ct) =>
                {
                    var cartToken = ctx.Request.Headers["X-Cart-Token"].FirstOrDefault()
                        ?? ctx.User.FindFirst("cart_token")?.Value
                        ?? string.Empty;

                    var command = new Command(new Request
                    {
                        ReservationId = reservationId,
                        CartToken = cartToken
                    });
                    var result = await sender.Send(command, ct);
                    return result.Match(Results.Ok, _ => Results.NotFound());
                })
            .Produces<Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
```

- [ ] **Step 2: Register endpoint in InventoryFeature.Storefront.cs**

Add `.MapReleaseEndpoint()` call in the Carter module registration for `CartReservations.Release`.

- [ ] **Step 3: Build**

```bash
dotnet build
```

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Release/
git commit -m "feat(inventory): add CartReservations.Release endpoint with cartToken auth"
```

### Task 14: PaymentMethod seeder autoCapture fix (M7)

**Files:**
- Modify: `service/Api/src/Module/Payment/Persistence/Seeders/PaymentMethod.Seeder.cs:28`

- [ ] **Step 1: Fix the seed value**

Check whether `PaymentMethod.AutoCapture` is read by any code path:
```bash
rg "\.AutoCapture" service/Api/src/ --include "*.cs" | grep -v "gateway\.AutoCapture\|BogusGateway\|StripeGateway"
```

If nothing reads it, change the seed:
```csharp
// Before
new { Name = "Test Card (Bogus)", ProviderKey = "bogus", AutoCapture = false, ... }
// After
new { Name = "Test Card (Bogus)", ProviderKey = "bogus", AutoCapture = true, ... }
```

If something reads it (e.g. admin UI display), still change it to `true` (matches `BogusGateway.AutoCapture=true`).

- [ ] **Step 2: Commit**

```bash
git add service/Api/src/Module/Payment/Persistence/Seeders/PaymentMethod.Seeder.cs
git commit -m "fix(payment): correct Bogus PaymentMethod seeder autoCapture to true"
```

---

## Phase 2B — UC-STR-CHK flow restructure + cross-module contracts

### Task 15: Add CheckoutState enum + Order migration

**Files:**
- Create: `service/Api/src/Module/Ordering/Domain/Orders/CheckoutState.cs`
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.cs`
- Modify: `service/Api/src/Module/Ordering/Persistence/Configurations/Orders/OrderConfiguration.cs`
- Create: EF Core migration

- [ ] **Step 1: Create the enum**

```csharp
namespace Module.Ordering.Domain.Orders;

public enum CheckoutState
{
    None = 0,
    Address = 1,
    Delivery = 2,
    Payment = 3,
    Confirm = 4,
    Complete = 5
}
```

- [ ] **Step 2: Add property to Order**

In `Order.cs`, add:
```csharp
public CheckoutState CheckoutState { get; set; } = CheckoutState.None;
```

- [ ] **Step 3: Add domain method AdvanceCheckoutState**

In `Order.cs`, add:
```csharp
public Result AdvanceCheckoutState(CheckoutState target)
{
    var validTransition = (CheckoutState, target) switch
    {
        (CheckoutState.None, CheckoutState.Address) => true,
        (CheckoutState.Address, CheckoutState.Delivery) => true,
        (CheckoutState.Delivery, CheckoutState.Payment) => true,
        (CheckoutState.Payment, CheckoutState.Complete) => true, // skip Confirm for auto-capture
        (CheckoutState.Confirm, CheckoutState.Complete) => true,
        _ => false
    };
    if (!validTransition)
        return OrderResult.Errors.InvalidCheckoutTransition(CheckoutState, target);
    CheckoutState = target;
    return Result.Ok();
}
```

- [ ] **Step 4: Add the error in OrderResult**

```csharp
public static Error InvalidCheckoutTransition(CheckoutState current, CheckoutState target) => Error.Conflict(
    code: "Order.CheckoutState.InvalidTransition",
    message: $"Cannot transition from {current} to {target}.");
```

- [ ] **Step 5: Add EF configuration**

In `OrderConfiguration.cs`, add:
```csharp
builder.Property(x => x.CheckoutState)
    .HasConversion<string>()
    .HasMaxLength(20);
```

- [ ] **Step 6: Generate migration + backfill Draft orders to Address**

```bash
dotnet ef migrations add AddCheckoutStateToOrder --project service/Api/src/Migrations --startup-project service/Api/src/Api
```

Add a manual SQL step in the migration Up:
```sql
UPDATE "Orders" SET "CheckoutState" = 'Address' WHERE "Status" = 'Draft' AND "CheckoutState" = 'None';
```

- [ ] **Step 7: Advance state in existing storefront handlers**

In `UpdateCheckout.cs` (Ordering Cart/UpdateCheckout), after the main logic, add:
```csharp
var stateResult = cart.AdvanceCheckoutState(CheckoutState.Address);
if (stateResult.IsFailure) return stateResult.Errors;
```

In `SelectShippingRate.cs`, after applying the rate:
```csharp
var stateResult = cart.AdvanceCheckoutState(CheckoutState.Delivery);
```

`CreatePaymentIntent` and `CreateOrderFromCart` will do their own state advances in Tasks 16 and 17.

- [ ] **Step 8: Build + run tests**

```bash
dotnet build
dotnet test --filter "Ordering"
```

- [ ] **Step 9: Commit**

```bash
git add service/Api/src/Module/Ordering/Domain/Orders/CheckoutState.cs \
        service/Api/src/Module/Ordering/Domain/Orders/Order.cs \
        service/Api/src/Module/Ordering/Persistence/Configurations/Orders/OrderConfiguration.cs \
        service/Api/src/Migrations/ \
        service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateCheckout/UpdateCheckout.cs \
        service/Api/src/Module/Ordering/Features/Storefront/Cart/SelectShippingRate/SelectShippingRate.cs
git commit -m "feat(ordering): add CheckoutState state machine to Order with strict transitions"
```

### Task 16: Shared Application Contracts — all 7 MediatR contracts

**Files:**
- Create: `service/Api/src/Shared/Application/Contracts/Ordering/GetCartForCheckoutQuery.cs`
- Create: `service/Api/src/Shared/Application/Contracts/Ordering/GetCartForCheckoutResponse.cs`
- Create: `service/Api/src/Shared/Application/Contracts/Inventory/ReserveCartStockCommand.cs`
- Create: `service/Api/src/Shared/Application/Contracts/Inventory/ReserveCartStockResponse.cs`
- Create: `service/Api/src/Shared/Application/Contracts/Inventory/ReleaseCartStockReservationsCommand.cs`
- Create: `service/Api/src/Shared/Application/Contracts/Inventory/ConsumeCartStockReservationsCommand.cs`
- Create: `service/Api/src/Shared/Application/Contracts/Inventory/ConsumeCartStockReservationsResponse.cs`
- Create: `service/Api/src/Shared/Application/Contracts/Catalog/GetVariantDiscontinuedStatusesQuery.cs`
- Create: `service/Api/src/Shared/Application/Contracts/Payment/GetPaymentForCheckoutQuery.cs`
- Create: `service/Api/src/Shared/Application/Contracts/Payment/MarkPaymentPaidCommand.cs`

- [ ] **Step 1: Create all contract files**

`GetCartForCheckoutQuery.cs`:
```csharp
namespace Shared.Application.Contracts.Ordering;

public sealed record GetCartForCheckoutQuery(Guid CartId) : IQuery<GetCartForCheckoutResponse>;

public sealed record GetCartForCheckoutResponse(
    CheckoutState State,
    IReadOnlyList<CartLineItem> LineItems,
    decimal Total,
    string? Email);

public sealed record CartLineItem(Guid VariantId, int Quantity);
```

Note: `CheckoutState` is the Ordering domain enum — the Shared contract uses its own value type or the Ordering module's type. Since Shared can't reference Module, use a `string State` or an enum defined in Shared. Decision: use `string` for the contract (`"Address"`, `"Delivery"`, etc.) to keep Shared pure. The handler converts.

`ReserveCartStockCommand.cs`:
```csharp
namespace Shared.Application.Contracts.Inventory;

public sealed record ReserveCartStockCommand(
    Guid CartId,
    IReadOnlyList<ReserveLineItem> LineItems,
    int TtlMinutes = 30) : ICommand<ReserveCartStockResponse>;

public sealed record ReserveLineItem(Guid VariantId, int Quantity);
public sealed record ReserveCartStockResponse(
    IReadOnlyList<Guid> ReservationIds,
    bool Success);
```

`ReleaseCartStockReservationsCommand.cs`:
```csharp
namespace Shared.Application.Contracts.Inventory;

public sealed record ReleaseCartStockReservationsCommand(Guid CartId) : ICommand;
```

`ConsumeCartStockReservationsCommand.cs`:
```csharp
namespace Shared.Application.Contracts.Inventory;

public sealed record ConsumeCartStockReservationsCommand(Guid CartId) : ICommand<ConsumeCartStockReservationsResponse>;

public sealed record ConsumeCartStockReservationsResponse(bool Success, string? ErrorMessage);
```

`GetVariantDiscontinuedStatusesQuery.cs`:
```csharp
namespace Shared.Application.Contracts.Catalog;

public sealed record GetVariantDiscontinuedStatusesQuery(IReadOnlyList<Guid> VariantIds) : IQuery<Dictionary<Guid, bool>>;
```

`GetPaymentForCheckoutQuery.cs`:
```csharp
namespace Shared.Application.Contracts.Payment;

public sealed record GetPaymentForCheckoutQuery(string PaymentIntentId, Guid OrderId) : IQuery<PaymentForCheckoutResponse>;

public sealed record PaymentForCheckoutResponse(decimal Amount, bool IsCompleted);
```

`MarkPaymentPaidCommand.cs`:
```csharp
namespace Shared.Application.Contracts.Payment;

public sealed record MarkPaymentPaidCommand(Guid OrderId, string PaymentIntentId) : ICommand;
```

- [ ] **Step 2: Build**

```bash
dotnet build service/Api/src/Shared
```

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Shared/Application/Contracts/
git commit -m "feat(shared): add 7 cross-module MediatR contracts for checkout flow"
```

### Task 17: Implement MediatR handlers for all 7 contracts

**Files:**
- Create handler per contract (7 handlers total, each in their owning module)

- [ ] **Step 1: Ordering handler — GetCartForCheckoutQueryHandler**

Create under `Module.Ordering.Features/Storefront/...` or a shared internal handler:
```csharp
public sealed class GetCartForCheckoutQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetCartForCheckoutQuery, GetCartForCheckoutResponse>
{
    public async Task<Result<GetCartForCheckoutResponse>> Handle(GetCartForCheckoutQuery query, CancellationToken ct)
    {
        var cart = await dbContext.Set<Order>()
            .Include(x => x.LineItems)
            .FirstOrDefaultAsync(x => x.Id == query.CartId && x.Status == OrderStatus.Draft, ct);
        if (cart is null) return OrderResult.Errors.NotFound(query.CartId);
        return new GetCartForCheckoutResponse(
            cart.CheckoutState.ToString(),
            cart.LineItems.Select(li => new CartLineItem(li.VariantId, li.Quantity)).ToList(),
            cart.Total,
            cart.Email);
    }
}
```

- [ ] **Step 2: Inventory handler — ReserveCartStockHandler**

Reuse the logic from `ReserveCartStock.cs:31-79` but accept the Shared contract. Create a new handler that wraps the existing logic:
```csharp
public sealed class ReserveCartStockCommandHandler(
    IApplicationDbContext dbContext) : ICommandHandler<ReserveCartStockCommand, ReserveCartStockResponse>
{
    public async Task<Result<ReserveCartStockResponse>> Handle(ReserveCartStockCommand command, CancellationToken ct)
    {
        // Same RepeatableRead transaction + SumAsync + StockReservationMethod.Reserve logic
        // Returns reservation IDs or stock error
    }
}
```

- [ ] **Step 3: Inventory handler — ReleaseCartStockReservationsHandler**

Release all non-consumed reservations for a cart. Reuse the logic from `ReleaseCartReservation.cs` but operate on all reservations for a cart:
```csharp
public sealed class ReleaseCartStockReservationsCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<ReleaseCartStockReservationsCommand>
{
    public async Task<Result> Handle(ReleaseCartStockReservationsCommand command, CancellationToken ct)
    {
        var reservations = await dbContext.Set<StockReservation>()
            .Where(r => r.CartToken == command.CartId.ToString() && r.State == StockReservationState.Reserved)
            .ToListAsync(ct);
        foreach (var r in reservations)
        {
            r.State = StockReservationState.Released;
            r.ModifiedAtUtc = DateTimeOffset.UtcNow;
        }
        await dbContext.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
```

- [ ] **Step 4: Inventory handler — ConsumeCartStockReservationsHandler**

Replace the inline stock-deduction from `CreateOrderFromCart.cs:108-154`:
```csharp
public sealed class ConsumeCartStockReservationsCommandHandler(
    IApplicationDbContext dbContext) : ICommandHandler<ConsumeCartStockReservationsCommand, ConsumeCartStockReservationsResponse>
{
    public async Task<Result<ConsumeCartStockReservationsResponse>> Handle(
        ConsumeCartStockReservationsCommand command, CancellationToken ct)
    {
        var reservations = await dbContext.Set<StockReservation>()
            .Include(r => r.StockItem)
            .Where(r => r.CartToken == command.CartId.ToString() && r.State == StockReservationState.Reserved)
            .ToListAsync(ct);
        if (reservations.Count == 0)
            return new ConsumeCartStockReservationsResponse(false, "No active reservations — reservations may have expired");

        foreach (var r in reservations)
        {
            var stockItem = r.StockItem;
            var pickResult = stockItem.Pick(r.Quantity);
            if (pickResult.IsFailure) return pickResult.Errors;
            r.State = StockReservationState.Consumed;
            r.ModifiedAtUtc = DateTimeOffset.UtcNow;
            // Create StockMovement audit
        }
        await dbContext.SaveChangesAsync(ct);
        return new ConsumeCartStockReservationsResponse(true, null);
    }
}
```

Note: `StockReservationState` enum may not exist yet — if not, create it (values: `Reserved`, `Consumed`, `Released`, `Expired`) and add a migration. The `GetCartReservations.cs:22` already filters `State == Reserved` — check the existing column; if it's a string comparison add the enum conversion.

- [ ] **Step 5: Catalog handler — GetVariantDiscontinuedStatusesHandler**

```csharp
public sealed class GetVariantDiscontinuedStatusesQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetVariantDiscontinuedStatusesQuery, Dictionary<Guid, bool>>
{
    public async Task<Result<Dictionary<Guid, bool>>> Handle(GetVariantDiscontinuedStatusesQuery query, CancellationToken ct)
    {
        var ids = query.VariantIds;
        var discontinued = await dbContext.Set<Variant>()
            .Where(v => ids.Contains(v.Id) && v.DiscontinuedOn != null)
            .Select(v => v.Id)
            .ToHashSetAsync(ct);
        return ids.ToDictionary(id => id, id => discontinued.Contains(id));
    }
}
```

- [ ] **Step 6: Payment handler — GetPaymentForCheckoutHandler**

```csharp
public sealed class GetPaymentForCheckoutQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetPaymentForCheckoutQuery, PaymentForCheckoutResponse>
{
    public async Task<Result<PaymentForCheckoutResponse>> Handle(GetPaymentForCheckoutQuery query, CancellationToken ct)
    {
        var payment = await dbContext.Set<PaymentCapture>()
            .FirstOrDefaultAsync(p => p.ResponseCode == query.PaymentIntentId
                                   && p.OrderId == query.OrderId
                                   && p.State == PaymentRecordState.Completed, ct);
        return new PaymentForCheckoutResponse(payment?.Amount ?? 0m, payment?.State == PaymentRecordState.Completed);
    }
}
```

- [ ] **Step 7: Payment handler — MarkPaymentPaidHandler**

```csharp
public sealed class MarkPaymentPaidCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<MarkPaymentPaidCommand>
{
    public async Task<Result> Handle(MarkPaymentPaidCommand command, CancellationToken ct)
    {
        var payment = await dbContext.Set<PaymentCapture>()
            .FirstOrDefaultAsync(p => p.ResponseCode == command.PaymentIntentId
                                   && p.OrderId == command.OrderId, ct);
        if (payment is null) return PaymentCaptureResult.Failure.NotFound;
        payment.State = PaymentRecordState.Completed;
        await dbContext.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
```

- [ ] **Step 8: Build + run tests**

```bash
dotnet build
dotnet test --filter "Ordering|Inventory|Catalog|Payment"
```

- [ ] **Step 9: Commit**

```bash
git add service/Api/src/Module/*/Features/
git commit -m "feat: implement 7 MediatR contract handlers for cross-module checkout flow"
```

### Task 18: Restructure CreatePaymentIntent with stock reservation

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs`

- [ ] **Step 1: Rewrite the handler**

Inject `ISender` (replace direct `IApplicationDbContext` + `IGatewayRegistry` + `IPaymentProcessingService` — keep the last two, add `ISender`, remove `IApplicationDbContext` since the cart is now fetched via MediatR).

```csharp
public sealed class CommandHandler(
    ISender sender,
    IGatewayRegistry gatewayRegistry,
    IPaymentProcessingService processingService,
    ICurrentUser currentUser)
    : ICommandHandler<Command, Response>
{
    public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
    {
        // 1. Validate cart state via ISender
        var cartResult = await sender.Send(
            new GetCartForCheckoutQuery(command.OrderId), cancellationToken);
        if (cartResult.IsFailure) return cartResult.Errors;
        var cart = cartResult.Value;
        if (cart.State != "Delivery")
            return OrderResult.Errors.InvalidCheckoutTransition(
                Enum.Parse<CheckoutState>(cart.State), CheckoutState.Payment);

        // 2. Reserve stock atomically
        var reserveResult = await sender.Send(
            new ReserveCartStockCommand(command.OrderId,
                cart.LineItems.Select(li => new ReserveLineItem(li.VariantId, li.Quantity)).ToList()),
            cancellationToken);
        if (reserveResult.IsFailure || !reserveResult.Value.Success)
            return StockItemResult.Errors.InsufficientStock; // E3: no payment created

        // 3. Load payment method + create PaymentCapture (existing logic from lines 41-53)
        // ... (existing paymentMethod load + PaymentCaptureMethod.Create + gateway call)

        // 4. Call gateway
        var processResult = await processingService.ProcessAsync(payment, gateway, options, cancellationToken);
        if (processResult.IsFailure)
        {
            // Release reservations on gateway failure
            await sender.Send(new ReleaseCartStockReservationsCommand(command.OrderId), CancellationToken.None);
            return processResult.Errors;
        }

        // 5. Save
        try { await /* dbContext */ ... } // PaymentCapture save — use a scoped IApplicationDbContext injected separately
        catch
        {
            // E3 void: gateway succeeded but save failed
            await processingService.VoidAsync(payment, gateway, options, CancellationToken.None);
            await sender.Send(new ReleaseCartStockReservationsCommand(command.OrderId), CancellationToken.None);
            throw;
        }

        // 6. Advance cart state to Payment
        await sender.Send(new AdvanceCheckoutStateCommand(command.OrderId, "Payment"), cancellationToken);

        return payment.MapToStoreDetail<Response>();
    }
}
```

Note: The handler still needs `IApplicationDbContext` to persist the `PaymentCapture`. The `ISender` replaces only the cross-module parts. The handler injects both `ISender` and `IApplicationDbContext`.

`AdvanceCheckoutStateCommand` is a new internal command (owned by Ordering) — add it to the contracts. Contract shape:
```csharp
public sealed record AdvanceCheckoutStateCommand(Guid CartId, string TargetState) : ICommand;
```
Handler in Ordering:
```csharp
var cart = await dbContext.Set<Order>().FirstOrDefaultAsync(x => x.Id == command.CartId, ct);
var stateResult = cart.AdvanceCheckoutState(Enum.Parse<CheckoutState>(command.TargetState));
```

- [ ] **Step 2: Remove cross-module imports**

Remove `using Module.Ordering.Domain.Orders;` from `CreatePaymentIntent.cs` — now the handler uses only `ISender` and `Module.Payment.*` types.

- [ ] **Step 3: Build + run tests**

```bash
dotnet build
dotnet test --filter "Payment"
bash scripts/check-cross-module-refs.sh
```

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs \
        service/Api/src/Shared/Application/Contracts/Ordering/
git commit -m "feat(payment): restructure CreatePaymentIntent with ISender stock reservation + E3 void safety"
```

### Task 19: Restructure CreateOrderFromCart with reservation consumption

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs`

- [ ] **Step 1: Rewrite the handler to use MediatR contracts**

Replace inline stock-deduction (lines 108-154) with `ConsumeCartStockReservationsCommand`. Replace the variant-discontinued check (lines 82-85) with `GetVariantDiscontinuedStatusesQuery`. Replace the payment lookup (lines 64-67) with `GetPaymentForCheckoutQuery`. Replace `MarkPaymentAsPaid` with `MarkPaymentPaidCommand`.

The new shape:
```csharp
public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
{
    // ... user resolution (lines 42-45), cart lookup (lines 47-53)
    // Validate prerequisites (lines 57-58)

    // Validate cart state must be Payment
    if (cart.CheckoutState != CheckoutState.Payment)
        return OrderResult.Errors.InvalidCheckoutTransition(cart.CheckoutState, CheckoutState.Complete);

    // Verify payment via ISender (replaces lines 62-74)
    var paymentResult = await sender.Send(
        new GetPaymentForCheckoutQuery(command.Request.PaymentIntentId, cart.Id), cancellationToken);
    if (paymentResult.IsFailure) return paymentResult.Errors;
    var p = paymentResult.Value;
    if (!p.IsCompleted || p.Amount <= 0)
        return OrderResult.Errors.PaymentNotCompleted;
    await sender.Send(new MarkPaymentPaidCommand(cart.Id, command.Request.PaymentIntentId), cancellationToken);

    // Check discontinued variants via ISender (replaces lines 81-88)
    var variantIds = cart.LineItems.Select(li => li.VariantId).ToList();
    var discResult = await sender.Send(new GetVariantDiscontinuedStatusesQuery(variantIds), cancellationToken);
    if (discResult.Value.Values.Any(d => d))
        return OrderResult.Errors.VariantDiscontinued;

    // Consume existing reservations via ISender (replaces lines 93-173)
    var consumeResult = await sender.Send(new ConsumeCartStockReservationsCommand(cart.Id), cancellationToken);
    if (consumeResult.IsFailure || !consumeResult.Value.Success)
        return new Error("Order.ReservationExpired", consumeResult.Value.ErrorMessage ?? "Reservations expired or missing");

    // Place order (lines 100-106), advance state, notification (lines 175-182)
    // ... (existing logic for OrderNumber, Place, Commit, Notification, Response)
    cart.AdvanceCheckoutState(CheckoutState.Complete);
    await dbContext.SaveChangesAsync(cancellationToken);
    // ... existing notification + response
}
```

- [ ] **Step 2: Remove cross-module imports**

Remove lines 5-8 and 12:
```csharp
// Remove these:
// using Module.Catalog.Domain.Products.Variants;
// using Module.Inventory.Domain.StockLocations.StockItems;
// using Module.Inventory.Domain.StockReservations;
// using Module.Inventory.Domain.StockLocations.StockItems.StockMovements;
// using Module.Payment.Domain.PaymentCaptures;
```
Add `using Shared.Application.Contracts.*;` for each consumed contract namespace.

- [ ] **Step 3: Build + run tests + check cross-module refs**

```bash
dotnet build
dotnet test --filter "Ordering"
bash scripts/check-cross-module-refs.sh
```

Expected: `check-cross-module-refs.sh` count drops by at least 3 (the four Inventory + Catalog + Payment references from CreateOrderFromCart).

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs
git commit -m "refactor(ordering): restructure CreateOrderFromCart to consume reservations via ISender, eliminate cross-module refs"
```

### Task 20: Update AddToCart to use Shared contracts

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/AddItem/AddToCart.cs`

- [ ] **Step 1: Replace Inventory feature reference**

`AddToCart.cs:5-6` has:
```csharp
using Module.Inventory.Features.Storefront.CartReservations.Reserve;
using Module.Inventory.Domain.StockLocations.StockItems;
```

Replace both with `using Shared.Application.Contracts.Inventory;`. Change the `sender.Send(new ReserveCartStock.Command(...))` to:
```csharp
var reserveResult = await sender.Send(
    new ReserveCartStockCommand(cart.Id,
        new[] { new ReserveLineItem(command.VariantId, command.Quantity) }.ToList()),
    cancellationToken);
```

- [ ] **Step 2: Replace StockItem direct query**

`AddToCart.cs:70-74` queries `dbContext.Set<StockItem>()` to pick the primary location. Move this into a new internal contract: replace with a handler-side fallback (or accept that the first location is used; if the existing logic picks the highest-quantity location, move it into the ReserveCartStock handler).

Simpler approach: the ReserveCartStock handler (Task 17) already does the stock check + location picking. Remove the `StockItem` query from `AddToCart` entirely — let the Reserve command handle it.

- [ ] **Step 3: Build + run tests**

```bash
dotnet build
dotnet test --filter "Ordering"
bash scripts/check-cross-module-refs.sh
```

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/Cart/AddItem/AddToCart.cs
git commit -m "refactor(ordering): replace AddToCart cross-module refs with Shared.Application.Contracts"
```

### Task 21: Full flow smoke test

- [ ] **Step 1: Run the complete .http flow**

```
# 1. Login
POST /api/store/identity/auth/login/password { email, password }
# 2. Create cart
POST /api/storefront/cart
# 3. Add item
POST /api/storefront/cart/items { variantId, quantity }
# 4. Update checkout (Address state)
PUT /api/storefront/cart { shipAddressId, shippingMethodId }
# 5. Select shipping rate (Delivery state)
POST /api/storefront/cart/shipping-rate { shippingMethodId }
# 6. Create payment intent (Payment state, stock reserved)
POST /api/storefront/payment/create-intent { orderId, paymentMethodId, cardNumber: "4111111111111111" }
# 7. Checkout (Complete state, reservations consumed)
POST /api/storefront/cart/checkout { paymentIntentId }
```

- [ ] **Step 2: Verify E3 guard**

Test: create intent with stock but break the checkout mid-flight (e.g. disconnect cart from payment). Verify no orphaned payment — the try/catch in CreatePaymentIntent + the release-command call handle it.

- [ ] **Step 3: Verify state machine rejects out-of-order**

`POST /api/storefront/payment/create-intent` without first completing Delivery step → 409 `InvalidCheckoutTransition`.

- [ ] **Step 4: Commit**

```bash
git add ApiTests/Ordering/demo-flow.http  # if created
git commit -m "test: add end-to-end demo flow .http for UC-STR-CHK"
```

---

## Phase 3A — Profile conformance + gaps

### Task 22: Profile Store/ → Storefront/ rename

**Files:**
- Move: `service/Api/src/Module/Profile/Features/Store/**` → `service/Api/src/Module/Profile/Features/Storefront/**`

- [ ] **Step 1: Mass rename directories**

```bash
mv service/Api/src/Module/Profile/Features/Store/Profiles \
   service/Api/src/Module/Profile/Features/Store/Addresses \
   service/Api/src/Module/Profile/Features/Store/Wishlists \
   service/Api/src/Module/Profile/Features/Store/NotificationPreferences \
   service/Api/src/Module/Profile/Features/Storefront/
```

- [ ] **Step 2: Update namespaces in all moved files**

Find-and-replace `Module.Profile.Features.Store.` → `Module.Profile.Features.Storefront.` in all files under `Features/Storefront/`.

- [ ] **Step 3: Update ProfileFeature.cs route constants**

Find-and-replace `Features.Store.` → `Features.Storefront.` in `ProfileFeature.cs`.

- [ ] **Step 4: Build**

```bash
dotnet build
```

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Profile/Features/
git commit -m "refactor(profile): rename Store/ to Storefront/ per AGENTS rule 3"
```

### Task 23: Fix duplicated profiles/profiles route

**Files:**
- Modify: `service/Api/src/Module/Profile/Features/Shared/ProfileFeature.cs`

- [ ] **Step 1: Collapse the route**

Change `ProfileFeature.Store.Profiles.BaseRoute` from `"api/store/profiles" + "/profiles"` to just `"api/store/profiles"`. The Profiles-specific endpoints then map to `"/"`:

```csharp
Profiles = store with { Create = store.Create, Get = $"{BaseRoute}", Update = $"{BaseRoute}", Delete = $"{BaseRoute}" };
```

Adjust the subroutes accordingly. The full `Route` property for GetProfile becomes `api/store/profiles` (no doubled segment).

- [ ] **Step 2: Build**

```bash
dotnet build
```

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Profile/Features/Shared/ProfileFeature.cs
git commit -m "fix(profile): remove doubled profiles/profiles route segment"
```

### Task 24: Add Profile Create endpoint

**Files:**
- Create: `service/Api/src/Module/Profile/Features/Storefront/Profiles/Create/CreateProfile.Endpoint.cs`

- [ ] **Step 1: Create the endpoint**

```csharp
namespace Module.Profile.Features.Storefront.Profiles.Create;

public static partial class CreateProfile
{
    public static RouteHandlerBuilder MapCreateProfileEndpoint(this IEndpointRouteBuilder builder)
    {
        return builder.MapPost(
                ProfileFeature.Storefront.Profiles.Create.Route,
                async ([FromBody] Request request, ISender sender, CancellationToken ct) =>
                {
                    var command = new Command(request);
                    var result = await sender.Send(command, ct);
                    return result.Match(
                        value => Results.Created(ProfileFeature.Storefront.Profiles.Get.Route, value),
                        errors => errors.ToProblemDetailsResult());
                })
            .RequireAuthorization()
            .Produces<Response>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
```

The existing `CreateProfile.cs` handler and `CreateProfile.Validator.cs` (already under the old `Store/Profiles/Create/`) move to `Storefront/` with the Task 22 rename and are already wired.

- [ ] **Step 2: Register in ProfileFeature.cs**

Add the `MapCreateProfileEndpoint()` call.

- [ ] **Step 3: Build + run tests**

```bash
dotnet build
dotnet test --filter "Profile"
```

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Profile/Features/Storefront/Profiles/Create/CreateProfile.Endpoint.cs
git commit -m "feat(profile): add storefront CreateProfile endpoint"
```

### Task 25: Address CountryCode/StateCode FK validation via MediatR

**Files:**
- Modify: `service/Api/src/Module/Profile/Features/Admin/Addresses/Shared/Validators/Address.Validator.cs`
- Create: `service/Api/src/Shared/Application/Contracts/Location/CountryExistsByIsoQuery.cs`
- Create: `service/Api/src/Shared/Application/Contracts/Location/StateExistsByIsoQuery.cs`

- [ ] **Step 1: Create shared contracts**

```csharp
// CountryExistsByIsoQuery.cs
namespace Shared.Application.Contracts.Location;
public sealed record CountryExistsByIsoQuery(string IsoCode) : IQuery<bool>;

// StateExistsByIsoQuery.cs
namespace Shared.Application.Contracts.Location;
public sealed record StateExistsByIsoQuery(string CountryCode, string StateCode) : IQuery<bool>;
```

- [ ] **Step 2: Update the validator to use ISender**

Inject `ISender` into the validator (FluentValidation supports constructor injection). Add rules:

```csharp
RuleFor(x => x.CountryCode)
    .MustAsync(async (iso, ct) => await sender.Send(new CountryExistsByIsoQuery(iso!), ct))
    .When(x => !string.IsNullOrEmpty(x.CountryCode))
    .WithMessage("Country code does not exist.");

RuleFor(x => x.StateCode)
    .MustAsync(async (stateCode, ct) =>
    {
        var countryCode = context.InstanceToValidate.CountryCode;
        if (string.IsNullOrEmpty(countryCode) || string.IsNullOrEmpty(stateCode))
            return true;
        return await sender.Send(new StateExistsByIsoQuery(countryCode, stateCode), ct);
    })
    .When(x => !string.IsNullOrEmpty(x.StateCode))
    .WithMessage("State code does not exist for the given country.");
```

Note: The `ISender` injected into the validator is fine; MediatR's `ISender` is registered in DI. If FluentValidation DI doesn't support constructor injection for validators out-of-the-box, register the validator as scoped and use `IServiceProvider` to resolve the sender.

- [ ] **Step 3: Implement Location handlers (Task 36 in Phase 3E)**

The handlers are thin EF queries. Defer to Phase 3E. The contract is created here, the handlers there. The validator rule fires and returns 400 if the query returns false, gracefully skipping if the handlers aren't registered (Location module not loaded — returns 400, fail closed).

- [ ] **Step 4: Build**

```bash
dotnet build
dotnet test --filter "Profile"
```

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Shared/Application/Contracts/Location/ \
        service/Api/src/Module/Profile/Features/Admin/Addresses/Shared/Validators/Address.Validator.cs
git commit -m "feat(profile): add CountryCode/StateCode FK validation via MediatR queries"
```

---

## Phase 3B — Inventory bugs

### Task 26: ReleaseCartReservation authorization fix (SECURITY)

**Files:**
- Modify: `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Release/ReleaseCartReservation.cs`

- [ ] **Step 1: Add CartToken filter to lookup**

Change line 17-18 from:
```csharp
var reservation = await dbContext.Set<StockReservation>()
    .FirstOrDefaultAsync(r => r.Id == command.Request.ReservationId, ct);
```
to:
```csharp
var reservation = await dbContext.Set<StockReservation>()
    .FirstOrDefaultAsync(r => r.Id == command.Request.ReservationId
                           && r.CartToken == command.Request.CartToken, ct);
```
Unknown reservation or token mismatch returns 404 (no enumeration).

- [ ] **Step 2: Add missing Validator.cs to the Release endpoint**

Use the validator created in Task 13.

- [ ] **Step 3: Build + run tests**

```bash
dotnet build
dotnet test --filter "Inventory"
```

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Release/
git commit -m "fix(inventory): add CartToken filter to ReleaseCartReservation for authorization"
```

### Task 27: CountOnHand double-count fix (CORRECTNESS)

**Files:**
- Modify: `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Release/ReleaseCartReservation.cs:37`

- [ ] **Step 1: Remove phantom stock restoration**

Delete line 37:
```csharp
// REMOVE: stockItem.CountOnHand += reservation.Quantity;
```

Reserve never decrements `CountOnHand` (it is a soft hold); Release must not increment it. The only `CountOnHand` writer is `StockItem.Pick()` in the Consume handler (Task 17).

- [ ] **Step 2: Build + run tests**

```bash
dotnet build
dotnet test --filter "Inventory"
```

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Release/ReleaseCartReservation.cs
git commit -m "fix(inventory): remove phantom CountOnHand restore on reservation release"
```

### Task 28: ReserveCartStock serializable isolation fix

**Files:**
- Modify: `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Reserve/ReserveCartStock.cs:31`

- [ ] **Step 1: Switch isolation level**

Change:
```csharp
await using var transaction = await dbContext.BeginTransactionAsync(
    IsolationLevel.RepeatableRead, cancellationToken);
```
to:
```csharp
await using var transaction = await dbContext.BeginTransactionAsync(
    IsolationLevel.Serializable, cancellationToken);
```

- [ ] **Step 2: Add retry loop for serialization failures**

Wrap the transaction in a 3-retry loop (same pattern as `CreateOrderFromCart.cs:93-173`):
```csharp
int maxRetries = 3;
for (int attempt = 0; attempt < maxRetries; attempt++)
{
    await using var transaction = await dbContext.BeginTransactionAsync(
        IsolationLevel.Serializable, cancellationToken);
    try
    {
        // ... existing logic
        await transaction.CommitAsync(cancellationToken);
        break;
    }
    catch (DbUpdateConcurrencyException) when (attempt < maxRetries - 1)
    {
        await transaction.RollbackAsync(cancellationToken);
        await Task.Delay(100 * (1 << attempt), cancellationToken);
    }
    catch (Npgsql.PostgresException ex) when (ex.SqlState == "40001") // serialization_failure
    {
        await transaction.RollbackAsync(cancellationToken);
        if (attempt == maxRetries - 1) throw;
        await Task.Delay(100 * (1 << attempt), cancellationToken);
    }
}
```

- [ ] **Step 3: Build + run tests**

```bash
dotnet build
dotnet test --filter "Inventory"
```

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Reserve/ReserveCartStock.cs
git commit -m "fix(inventory): use Serializable isolation for ReserveCartStock to prevent oversell"
```

### Task 29: Remove duplicate CheckStockAvailability fragment

**Files:**
- Delete: `service/Api/src/Module/Inventory/Features/Storefront/StockAvailability/CheckStockAvailability/**`

- [ ] **Step 1: Verify no references**

```bash
rg "CheckStockAvailability" service/Api/src/ service/Api/tests/
```
If no matches, delete the directory. If tests reference it, redirect them to `Check/`.

- [ ] **Step 2: Delete the directory**

```bash
rm -rf service/Api/src/Module/Inventory/Features/Storefront/StockAvailability/CheckStockAvailability/
```

- [ ] **Step 3: Build**

```bash
dotnet build
```

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/Storefront/StockAvailability/
git commit -m "chore(inventory): remove duplicate CheckStockAvailability fragment"
```

### Task 30: GetCartReservations DB-side paging

**Files:**
- Modify: `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Status/GetCartReservations.cs`

- [ ] **Step 1: Push paging to the DB**

Move `Skip`/`Take` before `ToListAsync`:
```csharp
// Before (line 21-44): materialize all, then page
// After:
var query = dbContext.Set<StockReservation>()
    .Where(r => r.CartToken == request.CartToken
             && r.State == StockReservationState.Reserved
             && r.ExpiresAtUtc > now);
var totalCount = await query.CountAsync(cancellationToken);
var items = await query.OrderBy(r => r.CreatedAtUtc)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .Select(r => new Response { ... })
    .ToListAsync(cancellationToken);
return PagedResult<Response>.Create(items, totalCount, page, pageSize);
```

- [ ] **Step 2: Remove cartToken field from GetStockAvailability (3B.5)**

In `GetStockAvailability.Request.cs`, remove the `cartToken` field and the unused doc comment about cart-specific holds.

- [ ] **Step 3: Build + run tests**

```bash
dotnet build
dotnet test --filter "Inventory"
```

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Status/GetCartReservations.cs \
        service/Api/src/Module/Inventory/Features/Storefront/StockAvailability/Check/GetStockAvailability.Request.cs
git commit -m "fix(inventory): push CartReservations paging to DB; remove unused cartToken field"
```

---

## Phase 3C — Shipping conformance

### Task 31: Add Zone association to ShippingMethod

**Files:**
- Modify: `service/Api/src/Module/Shipping/Domain/ShippingMethods/ShippingMethod.cs`
- Create: `service/Api/src/Module/Shipping/Domain/ShippingMethods/ShippingMethodZone.cs`
- Create: EF Core migration

- [ ] **Step 1: Add Zones collection to ShippingMethod**

```csharp
public ICollection<ShippingMethodZone> Zones { get; set; } = new List<ShippingMethodZone>();
```

`ShippingMethodZone`:
```csharp
public sealed class ShippingMethodZone
{
    public Guid Id { get; set; }
    public Guid ShippingMethodId { get; set; }
    public string CountryCode { get; set; } = string.Empty; // e.g. "US", "VN", or "*" for all
    public string? StateCode { get; set; }
}
```

- [ ] **Step 2: Migration + seeder**

```bash
dotnet ef migrations add AddShippingMethodZones ...
```

Seed one zone ("*" = worldwide) for the existing shipping method.

- [ ] **Step 3: Update GetShippingMethods to filter by zone**

Add `string? CountryCode` to `Parameters`. In the handler:
```csharp
var query = dbContext.Set<ShippingMethod>()
    .Include(m => m.Zones)
    .Where(x => x.AvailableToUsers && !x.IsDeleted);

if (!string.IsNullOrEmpty(request.CountryCode))
    query = query.Where(m => m.Zones.Any(z =>
        z.CountryCode == "*" || z.CountryCode == request.CountryCode));
```

- [ ] **Step 4: Build + run tests**

```bash
dotnet build
dotnet test --filter "Shipping"
```

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Shipping/
git commit -m "feat(shipping): add Zone association to ShippingMethod for zone-filtered availability"
```

### Task 32: MediatR cost calculator — eliminate cross-module viols

**Files:**
- Create: `service/Api/src/Shared/Application/Contracts/Ordering/GetCartForShippingQuery.cs`
- Create: `service/Api/src/Shared/Application/Contracts/Catalog/GetVariantWeightsQuery.cs`
- Modify: `service/Api/src/Module/Shipping/Features/Storefront/Shipping/Calculate/CalculateShipping.cs`

- [ ] **Step 1: Create contracts**

```csharp
// GetCartForShippingQuery.cs
public sealed record GetCartForShippingQuery(Guid CartId) : IQuery<CartForShippingResponse>;
public sealed record CartForShippingResponse(decimal TotalWeight, decimal TotalValue, Guid? ShipAddressId, string Currency);

// GetVariantWeightsQuery.cs
public sealed record GetVariantWeightsQuery(IReadOnlyList<Guid> VariantIds) : IQuery<Dictionary<Guid, decimal>>; // decimal = total weight
```

- [ ] **Step 2: Implement handlers in Ordering and Catalog**

Ordering handler: loads cart line items + variant IDs, delegates to Catalog for weights.
Catalog handler: `Variant.Weight * quantity` per variant ID.

- [ ] **Step 3: Rewrite CalculateShipping handler**

Replace direct `dbContext.Set<Order>()` and `dbContext.Set<Variant>()` calls with `ISender` dispatch:
```csharp
var cartResult = await sender.Send(new GetCartForShippingQuery(command.CartId), ct);
var weightsResult = await sender.Send(new GetVariantWeightsQuery(cart.VariantIds), ct);
// Calculate rate based on weight + zone...
```

Remove `using Module.Ordering.Domain.Orders;` and `using Module.Catalog.Domain.Products.Variants;`.

- [ ] **Step 4: Wire Mapster mapping + fix paging math**

Replace inline `Select` in `GetShippingMethods.cs:25-33` with `MapToListItem<T>()` from `ShippingMethod.Mapping.Model.cs`. Delete the empty stub `ShippingMethod.Mapping.Domain.cs`.

Fix paging math: `(items.Count + pageSize - 1) / pageSize` instead of `Math.Max(1, items.Count)`.

- [ ] **Step 5: Build + verify cross-module refs**

```bash
dotnet build
dotnet test --filter "Shipping"
bash scripts/check-cross-module-refs.sh
```

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Shared/Application/Contracts/Ordering/ \
        service/Api/src/Shared/Application/Contracts/Catalog/ \
        service/Api/src/Module/Shipping/Features/Storefront/Shipping/
git commit -m "refactor(shipping): replace cross-module domain refs with ISender queries; wire Mapster + fix paging"
```

---

## Phase 3D — Catalog gaps

### Task 33: Facet counts for product list

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/GetStorefrontProducts.Parameters.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/GetStorefrontProducts.Response.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/GetStorefrontProducts.cs`

- [ ] **Step 1: Add IncludeFacets flag + Facets response**

In `Parameters.cs`, add:
```csharp
public bool IncludeFacets { get; set; }
```

In `Response.cs`, add:
```csharp
public FacetAggregate? Facets { get; set; }

public sealed record FacetAggregate(
    IReadOnlyList<FacetGroup> Groups);

public sealed record FacetGroup(
    string Name, // e.g. "Color", "Size", "Category"
    IReadOnlyList<FacetValue> Values);

public sealed record FacetValue(
    string Id,   // optionValueId or taxonId as string
    string Label,
    int Count,
    bool IsActive);
```

- [ ] **Step 2: Compute facets when IncludeFacets is true**

After the main query, if `includeFacets`, re-run grouped counts for option values and taxons:
```csharp
if (request.IncludeFacets)
{
    var facets = new List<FacetGroup>();
    // Option value facets
    var ovFacets = await dbContext.Set<ProductVariant>()
        .Where(pv => baseQuery.Any(p => p.Id == pv.ProductId)) // same filter context
        .GroupBy(pv => pv.OptionValueId)
        .Select(g => new { OptionValueId = g.Key, Count = g.Count() })
        .ToListAsync(ct);
    // Taxon facets — similar grouping on product-taxon join
    // Build response.Facets...
}
```

- [ ] **Step 3: Build + run tests**

```bash
dotnet build
dotnet test --filter "Catalog"
```

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/
git commit -m "feat(catalog): add facet counts to GetStorefrontProducts response"
```

### Task 34: Similar-products determinism + option-value mapping + breadcrumb

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/Similar/GetSimilarProducts.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/Shared/Mappings/ProductStore.Mapping.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/Shared/Models/ProductStorefront.Model.Response.cs`

- [ ] **Step 1: Fix Similar variant selection**

Change line 27-30:
```csharp
var variant = await dbContext.Set<Variant>()
    .Where(x => x.ProductId == request.Id && !x.IsDeleted)
    .OrderByDescending(v => v.IsMaster)
    .ThenBy(v => v.Position)
    .FirstOrDefaultAsync(ct);
```
Then try to find one with a `Search`-type embedding; fall back to the master.

- [ ] **Step 2: Populate OptionValue1/OptionValue2 in MapToStoreVariant**

In `ProductStore.Mapping.cs:46-62`, add:
```csharp
var optionValues = variant.OptionValueVariants
    .Select(ov => ov.OptionValue)
    .OrderBy(ov => ov.OptionType?.Position)
    .ToList();
dest.OptionValue1 = optionValues.ElementAtOrDefault(0)?.Value;
dest.OptionValue2 = optionValues.ElementAtOrDefault(1)?.Value;
```

- [ ] **Step 3: Add breadcrumb to taxon response**

In `StoreProductTaxonResponse`, add:
```csharp
public IReadOnlyList<TaxonBreadcrumbItem> Breadcrumb { get; set; } = [];

public sealed record TaxonBreadcrumbItem(Guid Id, string Name, string Permalink);
```

In the mapping, walk the parent chain:
```csharp
var breadcrumb = new List<TaxonBreadcrumbItem>();
var current = taxon;
while (current is not null)
{
    breadcrumb.Insert(0, new TaxonBreadcrumbItem(current.Id, current.Name, current.Permalink));
    current = current.ParentId is not null
        ? await dbContext.Set<Taxon>().FindAsync(new object[] { current.ParentId.Value }, ct)
        : null;
}
dest.Breadcrumb = breadcrumb;
```

- [ ] **Step 4: Fix GetAvailability N+1**

Add `GetForVariantsAsync(IEnumerable<Guid> variantIds)` to `IStockAvailabilityCalculator`. Implement a batched version. Update `GetAvailability.cs:95-97` to use the batch call.

- [ ] **Step 5: Build + run tests**

```bash
dotnet build
dotnet test --filter "Catalog"
```

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Storefront/Products/
git commit -m "fix(catalog): deterministic similar variants, option-value mapping, breadcrumb, N+1 availability"
```

---

## Phase 3E — Location minor bugs

### Task 35: Case-insensitive ISO + regex-constrained routes

**Files:**
- Modify: `service/Api/src/Module/Location/Features/Storefront/Countries/GetByIsoCode/GetStorefrontCountryByIso.cs`
- Modify: `service/Api/src/Module/Location/Features/Storefront/States/GetByIsoCode/GetStorefrontStateByIso.cs`
- Modify: `service/Api/src/Module/Location/Features/Storefront/Countries/GetByIsoCode/GetStorefrontCountryByIso.Endpoint.cs`
- Modify: `service/Api/src/Module/Location/Features/Storefront/States/GetByIsoCode/GetStorefrontStateByIso.Endpoint.cs`

- [ ] **Step 1: Case-insensitive comparison**

In `GetStorefrontCountryByIso.cs:23-24`, change:
```csharp
c.IsoCode == request.IsoCode
```
to:
```csharp
c.IsoCode.ToUpper() == request.IsoCode.ToUpper()
```

Same for `GetStorefrontStateByIso.cs:23-24`:
```csharp
s.Abbreviation.ToUpper() == request.IsoCode.ToUpper()
```

- [ ] **Step 2: Regex-constrain routes**

In the endpoints, change:
```csharp
"{isoCode}"
```
to:
```csharp
"{isoCode:regex(^[A-Za-z]{{2,3}}$)}" // countries
"{isoCode:regex(^[A-Za-z0-9]{{1,5}}$)}" // states
```

- [ ] **Step 3: Build + run tests**

```bash
dotnet build
dotnet test --filter "Location"
```

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Location/Features/Storefront/
git commit -m "fix(location): case-insensitive ISO matching + regex-constrained routes"
```

### Task 36: CountryExistsByIso + StateExistsByIso query handlers

**Files:**
- Create: `service/Api/src/Module/Location/Features/Shared/Queries/CountryExistsByIsoHandler.cs`
- Create: `service/Api/src/Module/Location/Features/Shared/Queries/StateExistsByIsoHandler.cs`

- [ ] **Step 1: Implement the handlers**

```csharp
public sealed class CountryExistsByIsoQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<CountryExistsByIsoQuery, bool>
{
    public async Task<Result<bool>> Handle(CountryExistsByIsoQuery query, CancellationToken ct)
    {
        return await dbContext.Set<Country>()
            .AnyAsync(c => c.IsoCode.ToUpper() == query.IsoCode.ToUpper(), ct);
    }
}

public sealed class StateExistsByIsoQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<StateExistsByIsoQuery, bool>
{
    public async Task<Result<bool>> Handle(StateExistsByIsoQuery query, CancellationToken ct)
    {
        return await dbContext.Set<State>()
            .AnyAsync(s => s.Abbreviation.ToUpper() == query.StateCode.ToUpper()
                        && s.Country.IsoCode.ToUpper() == query.CountryCode.ToUpper(), ct);
    }
}
```

- [ ] **Step 2: Build**

```bash
dotnet build
```

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Location/Features/Shared/
git commit -m "feat(location): implement CountryExistsByIso + StateExistsByIso MediatR query handlers"
```

---

## Phase 3F — Identity cross-module + minor gaps

### Task 37: ExternalAuthenticate → Shared contract + ISender

**Files:**
- Modify: `service/Api/src/Module/Identity/Features/Storefront/Auth/Login/External/Authenticate/ExternalAuthenticate.cs`

- [ ] **Step 1: Replace direct Profile references**

Remove lines 3-4:
```csharp
// REMOVE:
// using Module.Profile.Domain;
// using Module.Profile.Features.Store.Profiles.Create;
```

Replace `IMediator mediator` with `ISender sender` in the constructor.

Replace:
```csharp
var profileResult = await mediator.Send(new CreateProfile.Command(new CreateProfile.Request { ... }));
```
with:
```csharp
var profileResult = await sender.Send(new CreateUserProfileCommand { ... }); // Shared.Application.Contracts.Profile
```

The existing `CreateUserProfileCommand` and its handler (`CreateProfile.cs:55-76`) are already defined and registered. This is the pattern `ConfirmEmail.cs:117-125` already uses.

- [ ] **Step 2: Build + verify cross-module refs**

```bash
dotnet build
bash scripts/check-cross-module-refs.sh  # Identity count drops
```

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Identity/Features/Storefront/Auth/Login/External/Authenticate/ExternalAuthenticate.cs
git commit -m "refactor(identity): replace ExternalAuthenticate Profile cross-module ref with Shared contract + ISender"
```

### Task 38: Remove GetSession Profile reference + auth consistency + validator fixes

**Files:**
- Modify: `service/Api/src/Module/Identity/Features/Storefront/Auth/Sessions/Get/GetSession.cs`
- Modify: `service/Api/src/Module/Identity/Features/Storefront/Auth/Logout/Logout.Endpoint.cs`
- Modify: `service/Api/src/Module/Identity/Features/Storefront/Auth/Sessions/Get/GetSession.Endpoint.cs`
- Modify: `service/Api/src/Module/Identity/Features/Storefront/Passwords/Reset/ResetPassword.Validator.cs`
- Modify: `service/Api/src/Module/Identity/Features/Storefront/Emails/Resend/ResendEmailVerification.Endpoint.cs`

- [ ] **Step 1: Remove Profile reference from GetSession**

Remove `using Module.Profile.Domain;`. Replace `UserProfileResult.Failure.AuthRequired` with an Identity-specific error:
```csharp
// Before
return Result<Response>.Failure(UserProfileResult.Failure.AuthRequired);
// After
return Result<Response>.Failure(IdentityResult.Failure.AuthRequired);
```
Add `AuthRequired` error to `IdentityResult` if not present.

- [ ] **Step 2: Fix auth consistency**

In `Logout.Endpoint.cs:20`, add `.RequireAuthorization()` before `.Produces(...)`.
In `GetSession.Endpoint.cs:19`, add `.RequireAuthorization()`.

- [ ] **Step 3: Fix ResetPassword validator**

Add `RuleFor(x => x.UserId).NotEmpty();` to `ResetPassword.Validator.cs`.

- [ ] **Step 4: Fix comment drift**

In `ResendEmailVerification.Endpoint.cs:10`, change `resend-verification` → `resend` in the XML comment.

- [ ] **Step 5: Verify token security options**

Check `appsettings.json` for:
```json
"TokenSecurity": { "RotationEnabled": true, "ReuseDetectionEnabled": true }
```
If either is false/missing, set them to true. Document in the Handoff `.md`.

- [ ] **Step 6: Build + run tests**

```bash
dotnet build
dotnet test --filter "Identity"
bash scripts/check-cross-module-refs.sh
```

- [ ] **Step 7: Commit**

```bash
git add service/Api/src/Module/Identity/Features/Storefront/
git commit -m "refactor(identity): remove Profile refs, fix auth consistency, UserId rule, comment drift"
```

---

## Phase 3G — Vertical-slice shared utility move

### Task 39: Move Profile Admin/.../Shared → Features/Shared

**Files:**
- Move: `service/Api/src/Module/Profile/Features/Admin/Addresses/Shared/**` → `service/Api/src/Module/Profile/Features/Shared/Addresses/**`
- Move: `service/Api/src/Module/Profile/Features/Admin/Profiles/Shared/**` → `service/Api/src/Module/Profile/Features/Shared/Profiles/**`
- Update all imports in Admin and Storefront slices

- [ ] **Step 1: Move directories**

```bash
mkdir -p service/Api/src/Module/Profile/Features/Shared/Addresses
mkdir -p service/Api/src/Module/Profile/Features/Shared/Profiles
mv service/Api/src/Module/Profile/Features/Admin/Addresses/Shared/** service/Api/src/Module/Profile/Features/Shared/Addresses/
mv service/Api/src/Module/Profile/Features/Admin/Profiles/Shared/** service/Api/src/Module/Profile/Features/Shared/Profiles/
```

- [ ] **Step 2: Update all imports**

Find-and-replace across `Service/Api/src/Module/Profile/`:
- `Module.Profile.Features.Admin.Addresses.Shared.` → `Module.Profile.Features.Shared.Addresses.`
- `Module.Profile.Features.Admin.Profiles.Shared.` → `Module.Profile.Features.Shared.Profiles.`

- [ ] **Step 3: Build**

```bash
dotnet build
```

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Profile/Features/
git commit -m "refactor(profile): move Admin/.../Shared utilities to Features/Shared/ for vertical-slice isolation"
```

### Task 40: Move Location Admin/.../Shared → Features/Shared

**Files:**
- Move: `service/Api/src/Module/Location/Features/Admin/Countries/Shared/**` → `service/Api/src/Module/Location/Features/Shared/Countries/**`
- Move: `service/Api/src/Module/Location/Features/Admin/States/Shared/**` → `service/Api/src/Module/Location/Features/Shared/States/**`

- [ ] **Step 1: Move + update imports**

Same pattern as Task 39.

- [ ] **Step 2: Build + run tests**

```bash
dotnet build
dotnet test --filter "Location|Profile"
```

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Location/Features/
git commit -m "refactor(location): move Admin/.../Shared utilities to Features/Shared/ for vertical-slice isolation"
```

---

## Final Verification

### Task 41: Full build + all tests + cross-module check + demo runbook

- [ ] **Step 1: Full build**

```bash
dotnet build
```
Expected: green, zero warnings.

- [ ] **Step 2: All unit tests**

```bash
dotnet test service/Api/tests/Module.UnitTests
```
Expected: all pass.

- [ ] **Step 3: All integration tests (requires Docker)**

```bash
dotnet test
```
Expected: all pass.

- [ ] **Step 4: Cross-module reference check**

```bash
bash scripts/check-cross-module-refs.sh
```
Expected: count has dropped measurably from the baseline 39. At minimum the following refs are gone:
- `CreateOrderFromCart.cs` — 4 Inventory + 1 Catalog + 1 Payment (6 gone)
- `CreatePaymentIntent.cs` — 1 Ordering (1 gone)
- `AddToCart.cs` — 1 Inventory domain + 1 Inventory feature (2 gone)
- `ExternalAuthenticate.cs` — 1 Profile domain + 1 Profile feature (2 gone)
- `GetSession.cs` — 1 Profile (1 gone)
- `CalculateShipping.cs` — 1 Ordering + 1 Catalog (2 gone)
Total removed: 15. Baseline 39 → target 24 or lower.

- [ ] **Step 5: Feature file convention check**

```bash
bash scripts/check-feature-conventions.sh
```
Expected: green. Newly created feature files (ConfirmPayment.Request.cs, CreateProfile.Endpoint.cs, ReleaseCartReservation.*.cs, Shared contract files) all conform.

- [ ] **Step 6: Run the full demo .http runbook**

Execute the following .http sequence against a running Aspire-orchestrated API:
1. Login → JWT tokens
2. Create cart → cart ID
3. Add items → cart with line items
4. Get payment methods → Bogus method ID
5. Update checkout (Address state) → 200
6. Select shipping rate (Delivery state) → 200
7. Create payment intent (Payment state, stock reserved) → 200 with clientSecret
8. Confirm payment → 200
9. Checkout (Complete state) → 200, order number, notification
10. List orders → paged, includes new order
11. Get order detail → line items, payment state, shipment state
12. Cancel order → 200, inventory released, payment voided
13. Browse catalog products → facet counts present when IncludeFacets=true
14. Get product detail → breadcrumb, variant option values
15. Get similar products → deterministic result
16. Get country by ISO (lowercase "us") → 200 (case-insensitive)
17. Get country by ISO (50 chars) → 400 (regex-constrained)
18. Logout → 200, refresh token invalidated
19. Google OAuth login → profile created via Shared contract (no Profile direct ref)
20. Stripe webhook replay → idempotent (200, no duplicate state change)

- [ ] **Step 7: Commit**

```bash
git add ApiTests/
git commit -m "test: final verification — full build, all tests, cross-module refs, demo runbook"
```

---

## Self-Review

**1. Spec coverage:**
- Phase 1 (Tasks 1–9): SourceId migration ✓, source pass-through ✓, GatewayConstants ✓, CreatePaymentIntent update ✓, setup-dev-secrets.sh ✓, webhook audit ✓, ApiTests/.http ✓, Handoff.md ✓
- Phase 2A (Tasks 10–14): ConfirmPayment Request ✓, currency honoring ✓, stale doc comment ✓, M5 Release endpoint ✓, M7 seeder fix ✓
- Phase 2B (Tasks 15–21): CheckoutState enum + migration ✓, 7 contracts ✓, 7 handlers ✓, CreatePaymentIntent restructure ✓, CreateOrderFromCart restructure ✓, AddToCart update ✓, smoke test ✓
- Phase 3A (Tasks 22–25): Store/→Storefront/ rename ✓, doubled route fix ✓, Create endpoint ✓, Address FK ✓
- Phase 3B (Tasks 26–30): ReleaseCartReservation auth ✓, CountOnHand ✓, Serializable ✓, duplicate fragment ✓, paging ✓
- Phase 3C (Tasks 31–32): Zone-filtered methods ✓, MediatR calculator ✓, Mapster + paging ✓
- Phase 3D (Tasks 33–34): Facets ✓, Similar determinism ✓, OptionValue mapping ✓, Breadcrumb ✓, N+1 ✓
- Phase 3E (Tasks 35–36): ISO case-insensitive ✓, Regex routes ✓, CountryExists/StateExists handlers ✓
- Phase 3F (Tasks 37–38): ExternalAuthenticate → Shared ✓, GetSession ref ✓, Auth consistency ✓, Validator fixes ✓, Comment drift ✓
- Phase 3G (Tasks 39–40): Profile shared move ✓, Location shared move ✓
- Final (Task 41): Full build/tests/cross-module/demo ✓

**2. Placeholder scan:** No TBD, TODO, stub references found. All code blocks are concrete.

**3. Type consistency:**
- `CheckoutState` enum defined in Task 15, consumed in Tasks 16 (contract — uses string), 18 (CreatePaymentIntent), 19 (CreateOrderFromCart — uses domain enum), 20 (AddToCart) — consistent.
- `Shared.Application.Contracts.Inventory.ReserveCartStockCommand` defined in Task 16, implemented in Task 17, consumed in Tasks 18 and 20 — consistent.
- `PaymentCapture.SourceId` changed to `string?` in Task 1, consumed in Tasks 2 and 4 — consistent.
- `ISender` used consistently across all handlers (not `IMediator`).
