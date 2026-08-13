---
goal: Let storefront customers pick a payment method (Stripe Checkout redirect or Cash on Delivery) and place orders via webhook-driven auto-placement.
version: 1.0
date_created: 2026-08-13
last_updated: 2026-08-13
owner: Billing / Ordering / Store SPA
status: 'Planned'
tags: [feature, billing, ordering, payment, stripe, checkout, store]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

The storefront checkout currently forces a single embedded Stripe card form and
silently picks the first active payment method. This plan replaces that with a
payment-method choice at checkout: **Credit Card** redirects to Stripe Checkout
(hosted page), and **Cash on Delivery** creates a `Pending` payment with no
gateway call. For card payments a webhook (`checkout.session.completed`) marks
the payment `Completed` and auto-places the order server-side via a new Ordering
command; COD orders are placed through the existing explicit Place Order action
with a relaxed completion gate.

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Spec:** `docs/superpowers/specs/2026-08-13-payment-method-selection-design.md`

## Global Constraints

- **CON-GC1**: `TreatWarningsAsErrors=true` — any C# warning fails `dotnet build`.
- **CON-GC2**: Domain operations return `Result<T>`/`Result`; exceptions only for unrecoverable infra failures.
- **CON-GC3**: Modules communicate only via MediatR `ISender`; no direct cross-module assembly references beyond the pre-existing pattern (`CreatePaymentIntent` already references Ordering query/command types).
- **CON-GC4**: Vertical-slice feature files — `static partial class` split across `Features/{Admin|Storefront}/{Feature}/{Action}/`. Cross-module commands/queries use the existing lightweight record + handler pattern (no endpoint).
- **CON-GC5**: Store SPA comments follow `app/Store/AGENTS.md` (`// Label: Sentence.` in script, `<!-- Section: Title — purpose -->` in template). No em dashes in code comments.
- **CON-GC6**: .NET 10 (C# preview), EF Core + Npgsql; Stripe.net 52.1.0; Vue 3 + TS + Vitest (pnpm).
- **CON-GC7**: All changes are additive where possible; the only existing-file removals are the now-dead card-token/`CreateSetupIntent`-free paths.

## 1. Requirements & Constraints

- **REQ-001**: The storefront checkout payment step lists active, customer-facing payment methods (`DisplayOn != Backend`) instead of auto-picking the first method.
- **REQ-002**: Selecting Credit Card (provider `stripe`) creates a Stripe Checkout Session and redirects the customer to the hosted page.
- **REQ-003**: Selecting Cash on Delivery (provider `cash_on_delivery`) creates a `Pending` `PaymentCapture` with no gateway call.
- **REQ-004**: A Stripe `checkout.session.completed` webhook marks the payment `Completed` and auto-places the order server-side.
- **REQ-005**: COD orders are placed via the existing explicit Place Order action; the payment stays `Pending` (cash collected later via admin capture).
- **REQ-006**: Order placement allows `IsCompleted || (State == Pending && IsOffline)`; `MarkPaymentPaid` is skipped for offline payments.
- **REQ-007**: The "payment intent id" passed between SPA and `CreateOrderFromCart` is the `PaymentCapture.Id`; `GetPaymentForCheckout`/`MarkPaymentPaid` match on `Id` with `ResponseCode` as a secondary key.
- **REQ-008**: A new `/checkout/return` route polls `GET api/storefront/cart/payment/intent/{orderId}` until `IsCompleted`, then shows confirmation.
- **SEC-001**: Stripe Checkout Session is created with `SuccessUrl`/`CancelUrl`; the SPA ignores `session_id` and keys off `order` (our own cart id).
- **SEC-002**: Offline detection uses `PaymentCapture.ProviderKey` (set at intent creation), never the nullable `PaymentMethod` navigation.
- **CON-001**: No new `PaymentRecordState` enum values; reuse `Checkout → Processing → Pending/Completed`.
- **CON-002**: `PaymentCapture` gains exactly one new column (`CheckoutUrl`); one EF migration.
- **GUD-001**: `checkout.session.completed` is the single completion source for cards; the legacy `payment_intent.succeeded` handler no-ops for Checkout sessions (documented, not deleted).
- **PAT-001**: Follow the existing gateway-provider pattern: abstract method on `Gateway`/`IPaymentGatewayActionProvider`, real impl in `StripeGateway`, fake impl in `BogusGateway`.
- **PAT-002**: Reuse the existing `GetPaymentStatus` endpoint for the return-page poll; do not add a new status endpoint.

## 2. Implementation Steps

### Phase Index

| Phase | Goal | Tasks |
|-------|------|-------|
| 1 | Gateway + domain primitives (constants, column, session API) | TASK-001..007 |
| 2 | CreatePaymentIntent branching + response DTO | TASK-008..011 |
| 3 | Ordering placement gating + auto-place command | TASK-012..016 |
| 4 | Webhook completion + expiry | TASK-017..018 |
| 5 | Store SPA method selection + return route | TASK-019..022 |
| 6 | Integration tests + full verification | TASK-023..024 |

### Implementation Phase 1: Gateway + domain primitives

- GOAL-001: Add the offline provider key, Checkout Session gateway contract, `PaymentCapture.CheckoutUrl` column, and the COD seed — with no behavior change to the existing Stripe/Bogus direct-intent flow.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Add `CashOnDelivery` provider + `checkout.session.*` webhook events + `IsOffline` helper to `GatewayConstants`. | | |
| TASK-002 | Add `CheckoutUrl` to `PaymentGatewayResponse`. | | |
| TASK-003 | Add `CheckoutUrl` to `PaymentCapture` + EF config + migration `AddPaymentCaptureCheckoutUrl`. | | |
| TASK-004 | Add `CreateCheckoutSessionAsync` to `IPaymentGatewayActionProvider` + `Gateway`. | | |
| TASK-005 | Implement `CreateCheckoutSessionAsync` in `StripeGateway`. | | |
| TASK-006 | Implement `CreateCheckoutSessionAsync` in `BogusGateway`. | | |
| TASK-007 | Seed COD method + filter `DisplayOn != Backend` in `ListPaymentMethods`. | | |

#### TASK-001: Gateway constants (offline provider + webhook events)

**Files:**
- Modify: `service/Api/src/Module/Billing/Services/Provider/GatewayConstants.cs:9-13` (Providers), `:119-129` (WebhookEvents.Stripe)

**Interfaces:**
- Produces: `GatewayConstants.Providers.CashOnDelivery` (`const string = "cash_on_delivery"`), `GatewayConstants.Providers.IsOffline(string) : bool`, `GatewayConstants.WebhookEvents.Stripe.CheckoutSessionCompleted` (`"checkout.session.completed"`), `GatewayConstants.WebhookEvents.Stripe.CheckoutSessionExpired` (`"checkout.session.expired"`).

- [ ] **Step 1: Add the provider constant and offline helper**

In `GatewayConstants.Providers` (after `Bogus`):

```csharp
public const string CashOnDelivery = "cash_on_delivery";

public static bool IsOffline(string providerKey) => providerKey == CashOnDelivery;
```

- [ ] **Step 2: Add the webhook event constants**

In `GatewayConstants.WebhookEvents.Stripe` (after `PaymentIntentCanceled`):

```csharp
public const string CheckoutSessionCompleted = "checkout.session.completed";
public const string CheckoutSessionExpired = "checkout.session.expired";
```

- [ ] **Step 3: Verify the build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: Build succeeds (0 warnings).

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Billing/Services/Provider/GatewayConstants.cs
git commit -m "feat(billing): add COD provider key and checkout.session webhook events"
```

#### TASK-002: PaymentGatewayResponse.CheckoutUrl

**Files:**
- Modify: `service/Api/src/Module/Billing/Services/Provider/PaymentGatewayResponse.cs:6-36`

**Interfaces:**
- Produces: `PaymentGatewayResponse.CheckoutUrl` (`string?`), constructor param `checkoutUrl = null` added after `paymentStatus`.

- [ ] **Step 1: Add the property**

Add after `PaymentStatus`:

```csharp
public string? CheckoutUrl { get; }
```

- [ ] **Step 2: Add the constructor parameter**

Add `string? checkoutUrl = null,` after the `string? paymentStatus = null,` parameter, and assign `CheckoutUrl = checkoutUrl;` in the body (existing call sites use named args, so nothing breaks).

- [ ] **Step 3: Verify the build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: Build succeeds.

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Billing/Services/Provider/PaymentGatewayResponse.cs
git commit -m "feat(billing): add CheckoutUrl to gateway response"
```

#### TASK-003: PaymentCapture.CheckoutUrl column + migration

**Files:**
- Modify: `service/Api/src/Module/Billing/Domain/PaymentCaptures/PaymentCapture.cs:21` (add property near `IntentClientSecret`)
- Modify: `service/Api/src/Module/Billing/Persistence/Configurations/Payments/PaymentRecordConfiguration.cs:21` (add config near `IntentClientSecret`)
- Create: `service/Api/src/Migrations/Migrations/*_AddPaymentCaptureCheckoutUrl.cs` (generated)

**Interfaces:**
- Produces: `PaymentCapture.CheckoutUrl` (`string?`), persisted column `checkout_url` (nvarchar/text, max 2048, nullable).

- [ ] **Step 1: Add the entity property**

In `PaymentCapture.cs`, after `IntentClientSecret`:

```csharp
public string? CheckoutUrl { get; set; }
```

- [ ] **Step 2: Add the EF column config**

In `PaymentRecordConfiguration.cs`, after the `IntentClientSecret` line:

```csharp
builder.Property(x => x.CheckoutUrl).HasMaxLength(2048);
```

- [ ] **Step 3: Generate the migration**

Run:

```bash
dotnet ef migrations add AddPaymentCaptureCheckoutUrl \
  --project service/Api/src/Migrations/Api.Migrations.csproj \
  --startup-project service/Api/src/Api/Api.csproj
```

Expected: generates `Migrations/{timestamp}_AddPaymentCaptureCheckoutUrl.cs` with `AddColumn<string>(name: "checkout_url", ... nullable: true)` and a `DropColumn` in `Down`. Do not hand-edit generated files.

- [ ] **Step 4: Verify the build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: Build succeeds (migration compiles).

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Billing/Domain/PaymentCaptures/PaymentCapture.cs \
        service/Api/src/Module/Billing/Persistence/Configurations/Payments/PaymentRecordConfiguration.cs \
        service/Api/src/Migrations/Migrations/
git commit -m "feat(billing): add PaymentCapture.CheckoutUrl column and migration"
```

#### TASK-004: Gateway abstraction — CreateCheckoutSessionAsync

**Files:**
- Modify: `service/Api/src/Module/Billing/Services/Provider/IPaymentGatewayActionProvider.cs:73` (after `CreateSetupIntentAsync`)
- Modify: `service/Api/src/Module/Billing/Services/Provider/Gateway.cs:29-32` (after `CreateSetupIntentAsync`)

**Interfaces:**
- Produces: `Task<Result<PaymentGatewayResponse>> CreateCheckoutSessionAsync(decimal amount, GatewayOptions options, CancellationToken ct = default)`.

- [ ] **Step 1: Add the interface method**

In `IPaymentGatewayActionProvider`:

```csharp
/// <summary>Creates a hosted checkout session for the amount, returning a redirect URL.</summary>
Task<Result<PaymentGatewayResponse>> CreateCheckoutSessionAsync(
    decimal amount, GatewayOptions options, CancellationToken ct = default);
```

- [ ] **Step 2: Add the abstract base method**

In `Gateway` (abstract class):

```csharp
public abstract Task<Result<PaymentGatewayResponse>> CreateCheckoutSessionAsync(
    decimal amount, GatewayOptions options, CancellationToken ct = default);
```

- [ ] **Step 3: Verify the build (fails — both gateways lack the override)**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: FAIL with CS0534 ("does not implement inherited abstract member CreateCheckoutSessionAsync") on `StripeGateway` and `BogusGateway`.

- [ ] **Step 4: Commit the contract only after TASK-005/TASK-006 (build is red in between)**

Do not commit yet — TASK-005 and TASK-006 restore the build. Commit TASK-004/005/006 together in TASK-006.

#### TASK-005: StripeGateway.CreateCheckoutSessionAsync

**Files:**
- Modify: `service/Api/src/Module/Billing/Services/Provider/Stripe/StripeGateway.cs:12-15` (add field), after `CreateSetupIntentAsync` (add method)

**Interfaces:**
- Consumes: `GatewayOptions` (`SuccessUrl`, `CancelUrl`, `Currency`, `Customer`, `OrderId`, `PaymentId`), `PaymentGatewayResponse(provider, authorization, checkoutUrl)`, `GatewayConstants.Metadata.*`.
- Produces: a Checkout Session whose `Id` (`cs_...`) becomes `ResponseCode` and whose `Url` becomes `CheckoutUrl`.

- [ ] **Step 1: Add the SessionService field**

With the other service fields:

```csharp
private readonly SessionService _sessionService = new();
```

- [ ] **Step 2: Add the method**

After `CreateSetupIntentAsync`:

```csharp
public override async Task<Result<PaymentGatewayResponse>> CreateCheckoutSessionAsync(
    decimal amount, GatewayOptions options, CancellationToken ct = default)
{
    try
    {
        var so = new SessionCreateOptions
        {
            Mode = "payment",
            CustomerEmail = options.Customer,
            SuccessUrl = options.SuccessUrl,
            CancelUrl = options.CancelUrl,
            Metadata = new Dictionary<string, string>
            {
                [GatewayConstants.Metadata.OrderIdKey] = options.OrderId,
                [GatewayConstants.Metadata.PaymentIdKey] = options.PaymentId
            },
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = options.Currency.ToLowerInvariant(),
                        UnitAmount = checked((long)Math.Round(
                            amount * GatewayConstants.Amounts.CentsMultiplier, MidpointRounding.AwayFromZero)),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"Order {options.OrderId}"
                        }
                    }
                }
            ]
        };
        var ro = BuildRequestOptions(options);
        var session = await _sessionService.CreateAsync(so, ro, ct).ConfigureAwait(false);
        return new PaymentGatewayResponse(GatewayConstants.Providers.Stripe,
            authorization: session.Id, checkoutUrl: session.Url);
    }
    catch (StripeException ex) { return MapStripeException(ex); }
}
```

- [ ] **Step 3: Add `using Stripe.Checkout;`** to the top of `StripeGateway.cs` (for `SessionService`, `SessionCreateOptions`, `SessionLineItemOptions`, etc.).

- [ ] **Step 4: Verify the build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: FAIL — only `BogusGateway` still lacks the override.

#### TASK-006: BogusGateway.CreateCheckoutSessionAsync

**Files:**
- Modify: `service/Api/src/Module/Billing/Services/Provider/Bogus/BogusGateway.cs:56-62` (after `CreateSetupIntentAsync`)

**Interfaces:**
- Consumes: `GatewayOptions.SuccessUrl`.
- Produces: fake session `cs_fake_{guid}` with `CheckoutUrl = $"{options.SuccessUrl}?session_id={sessionId}"`.

- [ ] **Step 1: Add the method**

```csharp
public override Task<Result<PaymentGatewayResponse>> CreateCheckoutSessionAsync(
    decimal amount, GatewayOptions options, CancellationToken ct = default)
{
    var sessionId = $"cs_fake_{Guid.NewGuid():N}";
    return Task.FromResult(Result<PaymentGatewayResponse>.Ok(
        new PaymentGatewayResponse(GatewayConstants.Providers.Bogus,
            authorization: sessionId,
            checkoutUrl: $"{options.SuccessUrl}?session_id={sessionId}")));
}
```

- [ ] **Step 2: Verify the build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: Build succeeds (0 warnings).

- [ ] **Step 3: Commit TASK-004/005/006 together**

```bash
git add service/Api/src/Module/Billing/Services/Provider/IPaymentGatewayActionProvider.cs \
        service/Api/src/Module/Billing/Services/Provider/Gateway.cs \
        service/Api/src/Module/Billing/Services/Provider/Stripe/StripeGateway.cs \
        service/Api/src/Module/Billing/Services/Provider/Bogus/BogusGateway.cs
git commit -m "feat(billing): add CreateCheckoutSessionAsync to gateways (Stripe + Bogus)"
```

#### TASK-007: Seed COD method + ListPaymentMethods filter

**Files:**
- Modify: `service/Api/src/Module/Billing/Persistence/Seeders/PaymentMethod.Seeder.cs:19-29`
- Modify: `service/Api/src/Module/Billing/Features/Storefront/Payment/Methods/ListPaymentMethods.cs:26-28`

**Interfaces:**
- Consumes: `PaymentMethodMethod.Create(name, code, providerKey, autoCapture, displayOn)`, `DisplayOn.Frontend`, `DisplayOn.Backend`, `GatewayConstants.Providers.CashOnDelivery`.

- [ ] **Step 1: Seed Cash on Delivery**

Add to the `methods` array in `PaymentMethodSeeder`:

```csharp
PaymentMethodMethod.Create(
    "Cash on Delivery", "cash_on_delivery", GatewayConstants.Providers.CashOnDelivery,
    displayOn: DisplayOn.Frontend),
```

- [ ] **Step 2: Filter storefront list to customer-facing methods**

In `ListPaymentMethods`, change the `Where` to:

```csharp
.Where(m => m.Active && !m.IsDeleted && m.DisplayOn != DisplayOn.Backend)
```

- [ ] **Step 3: Verify the build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: Build succeeds.

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Billing/Persistence/Seeders/PaymentMethod.Seeder.cs \
        service/Api/src/Module/Billing/Features/Storefront/Payment/Methods/ListPaymentMethods.cs
git commit -m "feat(billing): seed cash-on-delivery and hide backend-only methods"
```

### Implementation Phase 2: CreatePaymentIntent branching + response DTO

- GOAL-002: Make `CreatePaymentIntent` branch on provider key (COD → Pending, Stripe → Checkout Session) and surface `CheckoutUrl` in the storefront response.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-008 | Add `CheckoutUrl` to `StorePaymentDetailResponse` + `MapToStoreDetail`. | | |
| TASK-009 | Add `CancelUrl` to `CreatePaymentIntent.Request`. | | |
| TASK-010 | Rewrite `CreatePaymentIntent.CommandHandler` to branch offline vs Checkout Session. | | |
| TASK-011 | Rewrite `CreatePaymentIntentTests` for the new handler contract. | | |

#### TASK-008: Store response DTO + mapping CheckoutUrl

**Files:**
- Modify: `service/Api/src/Module/Billing/Features/Storefront/Payment/Shared/Models/PaymentStore.Model.Response.cs:5-12`
- Modify: `service/Api/src/Module/Billing/Features/Storefront/Payment/Shared/Mappings/PaymentStore.Mapping.cs:20-36`

**Interfaces:**
- Produces: `StorePaymentDetailResponse.CheckoutUrl` (`string?`); `MapToStoreDetail<PaymentCapture>` sets `CheckoutUrl = payment.CheckoutUrl`.

- [ ] **Step 1: Add the property**

In `StorePaymentDetailResponse`:

```csharp
public string? CheckoutUrl { get; init; }
```

- [ ] **Step 2: Map it**

In `MapToStoreDetail<PaymentCapture>`, after `PaymentStatus = payment.PaymentStatus,`:

```csharp
CheckoutUrl = payment.CheckoutUrl,
```

- [ ] **Step 3: Verify the build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: Build succeeds.

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Billing/Features/Storefront/Payment/Shared/Models/PaymentStore.Model.Response.cs \
        service/Api/src/Module/Billing/Features/Storefront/Payment/Shared/Mappings/PaymentStore.Mapping.cs
git commit -m "feat(billing): expose CheckoutUrl in storefront payment response"
```

#### TASK-009: CreatePaymentIntent.Request.CancelUrl

**Files:**
- Modify: `service/Api/src/Module/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.Request.cs:9`

**Interfaces:**
- Produces: `CreatePaymentIntent.Request.CancelUrl` (`string?`).

- [ ] **Step 1: Add the property**

In `CreatePaymentIntent.Request`, after `ReturnUrl`:

```csharp
public string? CancelUrl { get; init; }
```

- [ ] **Step 2: Verify the build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: Build succeeds.

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.Request.cs
git commit -m "feat(billing): add CancelUrl to create-payment-intent request"
```

#### TASK-010: Rewrite CreatePaymentIntent.CommandHandler

**Files:**
- Modify: `service/Api/src/Module/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs` (whole handler body, lines 22-146)

**Interfaces:**
- Consumes: `GatewayConstants.Providers.IsOffline(string)`, `gateway.CreateCheckoutSessionAsync(decimal, GatewayOptions, CancellationToken)`, `PaymentCapture.Process()`, `PaymentCapture.Pend()`, `IStockReservationService.ReserveForVariantAsync/ReleaseReservationsAsync`.
- Produces: for COD — `PaymentCapture.State == Pending`, no `ResponseCode`; for Stripe — `ResponseCode == session.Id`, `CheckoutUrl == session.Url`, `State == Processing`. Both set `payment.ProviderKey`.
- Note: the `IPaymentProcessingService processingService` constructor dependency is removed (intent creation no longer purchases/authorizes inline).

- [ ] **Step 1: Update constructor**

Remove `IPaymentProcessingService processingService` from the primary-constructor parameter list and the `using IPaymentProcessingService = ...` alias (the handler no longer uses it).

- [ ] **Step 2: Replace the handler body**

Replace everything from `// Load: First active payment method` through `return payment.MapToStoreDetail<Response>();` with:

```csharp
// Load: Active payment method — explicit id if provided, else first active
var paymentMethod = command.Request.PaymentMethodId.HasValue
    ? await dbContext.Set<PaymentMethod>()
        .FirstOrDefaultAsync(c => c.Id == command.Request.PaymentMethodId.Value && c.Active && !c.IsDeleted, cancellationToken)
    : await dbContext.Set<PaymentMethod>()
        .FirstOrDefaultAsync(c => c.Active && !c.IsDeleted, cancellationToken);
if (paymentMethod is null)
    return PaymentCaptureResult.Failure.NotFound;

var isOffline = GatewayConstants.Providers.IsOffline(paymentMethod.ProviderKey);

// Create: PaymentCapture with no source — offline methods and Checkout Sessions
// are both source-less; the gateway correlates via ResponseCode afterwards.
var createResult = PaymentCaptureMethod.Create(
    amount: cart.Total,
    paymentMethodId: (Guid)paymentMethod.Id,
    orderId: command.Request.OrderId,
    sourceId: null,
    sourceType: null);
if (createResult.IsFailure) return createResult.Errors;

var payment = createResult.Value;
payment.ProviderKey = paymentMethod.ProviderKey;
dbContext.Set<PaymentCapture>().Add(payment);

if (isOffline)
{
    // COD: transition straight to Pending — no gateway, no source.
    payment.Process();
    payment.Pend();
}
else
{
    var gatewayResult = gatewayRegistry.GetGateway(paymentMethod.ProviderKey);
    if (gatewayResult.IsFailure)
        return PaymentCaptureResult.Failure.ProviderNotRegistered(paymentMethod.ProviderKey);
    var gateway = gatewayResult.Value;

    var options = new GatewayOptions
    {
        Email = cart.Email ?? string.Empty,
        Customer = cart.Email ?? string.Empty,
        CustomerId = currentUser.UserId,
        OrderId = $"{command.Request.OrderId}-{payment.Number}",
        PaymentId = payment.Number,
        IdempotencyKey = GatewayConstants.Idempotency.ForPayment(payment.Number),
        StatementDescriptorSuffix = paymentMethod.StatementDescriptorSuffix,
        SuccessUrl = BuildSuccessUrl(command.Request.ReturnUrl, command.Request.OrderId),
        CancelUrl = command.Request.CancelUrl,
        Currency = string.IsNullOrWhiteSpace(command.Request.Currency)
            ? GatewayConstants.Currency.Usd
            : command.Request.Currency,
    };

    // Call: create hosted Checkout Session — no charge yet; webhook completes it.
    var sessionResult = await gateway.CreateCheckoutSessionAsync(cart.Total, options, cancellationToken);
    if (sessionResult.IsFailure)
    {
        await stockReservationService.ReleaseReservationsAsync(
            cartToken: command.Request.OrderId.ToString(), ct: CancellationToken.None);
        return sessionResult.Errors;
    }

    payment.ResponseCode = sessionResult.Value.Authorization;
    payment.CheckoutUrl = sessionResult.Value.CheckoutUrl;
    payment.Process();
}

// Save: PaymentCapture to database
try
{
    await dbContext.SaveChangesAsync(cancellationToken);
}
catch
{
    // Gateway session may have been created; it auto-expires in 24h. Release stock.
    await stockReservationService.ReleaseReservationsAsync(
        cartToken: command.Request.OrderId.ToString(), ct: CancellationToken.None);
    throw;
}

// Advance: Cart state to Payment
await sender.Send(
    new AdvanceCheckoutStateCommand { CartId = command.Request.OrderId, TargetState = "Payment" }, cancellationToken);

// Map: Payment → storefront response DTO
return payment.MapToStoreDetail<Response>();
```

- [ ] **Step 3: Add the private helper** (in the same `static partial class`, below the handler):

```csharp
private static string? BuildSuccessUrl(string? returnUrl, Guid orderId)
    => string.IsNullOrWhiteSpace(returnUrl) ? null : $"{returnUrl}?order={orderId}";
```

- [ ] **Step 4: Fix the remaining body** — the top half (cart validation + stock reservation loop, lines 37-65) is unchanged. Remove the now-unused `using Module.Billing.Services.Provider;` if it becomes unused (check compiler warnings).

- [ ] **Step 5: Verify the build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: Build succeeds; `CreatePaymentIntentTests` still compiles (constructor change is updated in TASK-011).

- [ ] **Step 6: Commit** (build may be red until TASK-011 updates tests; commit 010+011 together at end of TASK-011).

#### TASK-011: Rewrite CreatePaymentIntentTests

**Files:**
- Modify: `service/Api/tests/Module.UnitTests/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntentTests.cs` (constructor + all test bodies)

**Interfaces:**
- Consumes: `CreatePaymentIntent.CommandHandler(IApplicationDbContext, ICurrentUser, IGatewayRegistry, IStockReservationService, ISender)` (5 params, no processing service).
- Produces: tests covering offline-Pending, Stripe-session mapping, gateway-failure no-persist.

- [ ] **Step 1: Update constructor + mocks**

Remove `_processingServiceMock` and its setup; mock `CreateCheckoutSessionAsync` on `_gatewayMock`:

```csharp
_gatewayMock.Setup(x => x.CreateCheckoutSessionAsync(It.IsAny<decimal>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(new PaymentGatewayResponse("stripe", authorization: "cs_test_1", checkoutUrl: "https://checkout.stripe.com/c/pay/cs_test_1"));
```

Instantiate the handler as:

```csharp
_handler = new CreatePaymentIntent.CommandHandler(
    _dbContext, _currentUserMock.Object, _gatewayRegistryMock.Object,
    _reservationServiceMock.Object, _senderMock.Object);
```

- [ ] **Step 2: Rewrite the test methods**

Replace the body of the class with these tests (delete the old `ProcessAsync`-based ones):

```csharp
[Fact(DisplayName = "Handler: COD method creates a Pending payment with no gateway call")]
public async Task Handle_CodMethod_CreatesPendingPayment_NoGateway()
{
    var order = CreateOrder();
    var pm = new PaymentMethod { Name = "Cash on Delivery", Code = "cash_on_delivery",
        ProviderKey = GatewayConstants.Providers.CashOnDelivery, Active = true };
    _dbContext.Set<PaymentMethod>().Add(pm);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    SetupCartForCheckout(order.Id, 100.00m);

    var result = await _handler.Handle(
        new CreatePaymentIntent.Command(new CreatePaymentIntent.Request
            { OrderId = order.Id, PaymentMethodId = pm.Id }),
        TestContext.Current.CancellationToken);

    result.IsSuccess.Should().BeTrue();
    result.Value.State.Should().Be(PaymentRecordState.Pending.ToString());
    result.Value.ResponseCode.Should().BeNull();
    result.Value.CheckoutUrl.Should().BeNull();
    _gatewayMock.Verify(x => x.CreateCheckoutSessionAsync(It.IsAny<decimal>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    _dbContext.Set<PaymentCapture>().Single().ProviderKey.Should().Be(GatewayConstants.Providers.CashOnDelivery);
}

[Fact(DisplayName = "Handler: Stripe method creates a Checkout Session and maps CheckoutUrl")]
public async Task Handle_StripeMethod_CreatesCheckoutSession()
{
    var order = CreateOrder();
    var pm = new PaymentMethod { Name = "Credit Card", Code = "credit_card",
        ProviderKey = GatewayConstants.Providers.Stripe, Active = true };
    _dbContext.Set<PaymentMethod>().Add(pm);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    SetupCartForCheckout(order.Id, 100.00m);

    var result = await _handler.Handle(
        new CreatePaymentIntent.Command(new CreatePaymentIntent.Request
            { OrderId = order.Id, PaymentMethodId = pm.Id, ReturnUrl = "https://store.test/checkout/return" }),
        TestContext.Current.CancellationToken);

    result.IsSuccess.Should().BeTrue();
    result.Value.ResponseCode.Should().Be("cs_test_1");
    result.Value.CheckoutUrl.Should().Be("https://checkout.stripe.com/c/pay/cs_test_1");
}

[Fact(DisplayName = "Handler: does NOT persist PaymentCapture when session creation fails")]
public async Task Handle_SessionFails_NoPaymentPersisted()
{
    _gatewayMock.Setup(x => x.CreateCheckoutSessionAsync(It.IsAny<decimal>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(Error.BadRequest("Stripe.Error", "Session creation failed."));

    var order = CreateOrder();
    var pm = new PaymentMethod { Name = "Credit Card", Code = "credit_card",
        ProviderKey = GatewayConstants.Providers.Stripe, Active = true };
    _dbContext.Set<PaymentMethod>().Add(pm);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    SetupCartForCheckout(order.Id, 100.00m);

    var result = await _handler.Handle(
        new CreatePaymentIntent.Command(new CreatePaymentIntent.Request { OrderId = order.Id, PaymentMethodId = pm.Id }),
        TestContext.Current.CancellationToken);

    result.IsFailure.Should().BeTrue();
    _dbContext.Set<PaymentCapture>().Count().Should().Be(0);
}
```

Keep the existing private helpers `CreateOrder`, `SetupCartForCheckout`, `SetupDefaultSenderResponses`.

- [ ] **Step 3: Add needed usings**

Add `using Module.Billing.Domain.PaymentCaptures;` (for `PaymentRecordState`) and `using Module.Billing.Services.Provider;` (already present). `Error` is from `Shared` (`Shared.Application...`), matching the existing test.

- [ ] **Step 4: Run the tests**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter-query "///*CreatePaymentIntent*"`
Expected: 3/3 pass.

- [ ] **Step 5: Commit TASK-010 + TASK-011 together**

```bash
git add service/Api/src/Module/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs \
        service/Api/tests/Module.UnitTests/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntentTests.cs
git commit -m "feat(billing): branch CreatePaymentIntent for COD and Stripe Checkout"
```

### Implementation Phase 3: Ordering placement gating + auto-place command

- GOAL-003: Relax the order-placement gate for offline payments, key lookups on `PaymentCapture.Id`, and add the cross-module `CompleteCheckoutForPayment` command the webhook will call.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-012 | `GetPaymentForCheckout`: match on `Id` (fallback `ResponseCode`) and expose `State`/`IsOffline`. | | |
| TASK-013 | `MarkPaymentPaid`: match on `Id` and become idempotent. | | |
| TASK-014 | Extract placement tail into `CheckoutPlacementService`. | | |
| TASK-015 | `CreateOrderFromCart`: allow `Pending && IsOffline`, skip `MarkPaymentPaid` for offline, use the service. | | |
| TASK-016 | Add `CompleteCheckoutForPayment` command + handler (no endpoint). | | |

#### TASK-012: GetPaymentForCheckout — identifier + State/IsOffline

**Files:**
- Modify: `service/Api/src/Module/Billing/Features/Storefront/GetPaymentForCheckout/GetPaymentForCheckout.Response.cs`
- Modify: `service/Api/src/Module/Billing/Features/Storefront/GetPaymentForCheckout/GetPaymentForCheckout.cs`

**Interfaces:**
- Produces: `PaymentForCheckoutResponse { decimal Amount; bool IsCompleted; string State; bool IsOffline }`.

- [ ] **Step 1: Extend the response record**

```csharp
public sealed record PaymentForCheckoutResponse
{
    public decimal Amount { get; init; }
    public bool IsCompleted { get; init; }
    public string State { get; init; } = string.Empty;
    public bool IsOffline { get; init; }
}
```

- [ ] **Step 2: Update the handler**

```csharp
using Module.Billing.Domain.PaymentCaptures;
using Module.Billing.Services.Provider;

namespace Module.Billing.Features.Storefront.GetPaymentForCheckout;

public sealed class GetPaymentForCheckoutQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetPaymentForCheckoutQuery, PaymentForCheckoutResponse>
{
    public async Task<Result<PaymentForCheckoutResponse>> Handle(
        GetPaymentForCheckoutQuery query, CancellationToken cancellationToken)
    {
        Guid? parsedId = Guid.TryParse(query.PaymentIntentId, out var g) ? g : null;

        var payment = await dbContext.Set<PaymentCapture>()
            .FirstOrDefaultAsync(
                p => p.OrderId == query.OrderId
                     && ((parsedId.HasValue && p.Id == parsedId.Value)
                          || p.ResponseCode == query.PaymentIntentId),
                cancellationToken);

        return new PaymentForCheckoutResponse
        {
            Amount = payment?.Amount ?? 0m,
            IsCompleted = payment?.State == PaymentRecordState.Completed,
            State = payment?.State.ToString() ?? string.Empty,
            IsOffline = payment is not null
                && GatewayConstants.Providers.IsOffline(payment.ProviderKey)
        };
    }
}
```

- [ ] **Step 3: Verify the build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: Build succeeds.

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Billing/Features/Storefront/GetPaymentForCheckout/
git commit -m "feat(billing): key payment-for-checkout on capture id and expose offline state"
```

#### TASK-013: MarkPaymentPaid — identifier + idempotency

**Files:**
- Modify: `service/Api/src/Module/Billing/Features/Storefront/MarkPaymentPaid/MarkPaymentPaid.cs:5-26`

**Interfaces:**
- Consumes: `MarkPaymentPaidCommand { Guid OrderId; string PaymentIntentId }`.

- [ ] **Step 1: Update the handler**

```csharp
public sealed class MarkPaymentPaidCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<MarkPaymentPaidCommand>
{
    public async Task<Result> Handle(
        MarkPaymentPaidCommand command, CancellationToken cancellationToken)
    {
        Guid? parsedId = Guid.TryParse(command.PaymentIntentId, out var g) ? g : null;

        var payment = await dbContext.Set<PaymentCapture>()
            .FirstOrDefaultAsync(
                p => p.OrderId == command.OrderId
                     && ((parsedId.HasValue && p.Id == parsedId.Value)
                          || p.ResponseCode == command.PaymentIntentId),
                cancellationToken);

        if (payment is null)
            return PaymentCaptureResult.Failure.NotFound;

        if (payment.State != PaymentRecordState.Completed)
        {
            payment.State = PaymentRecordState.Completed;
            payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return Result.Ok();
    }
}
```

- [ ] **Step 2: Verify the build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: Build succeeds.

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Billing/Features/Storefront/MarkPaymentPaid/MarkPaymentPaid.cs
git commit -m "feat(billing): key mark-payment-paid on capture id and make idempotent"
```

#### TASK-014: CheckoutPlacementService

**Files:**
- Create: `service/Api/src/Module/Ordering/Services/CheckoutPlacementService.cs`

**Interfaces:**
- Produces: `CheckoutPlacementService.PlaceAsync(Order cart, string actor, CancellationToken ct) : Task<Result<Order>>`.

- [ ] **Step 1: Write the service**

```csharp
using Module.Ordering.Domain.Orders;
using Module.Inventory.Services.StockReservations;

using Shared.Operational.Notifications.Models;
using Shared.Operational.Notifications.Services;
using Shared.Operational.Notifications.Templates;

namespace Module.Ordering.Services;

/// <summary>Places a draft order: consumes stock, advances to Confirm, generates a number, places, notifies.</summary>
public sealed class CheckoutPlacementService(
    IApplicationDbContext dbContext,
    IStockReservationService stockReservationService,
    INotificationService notificationService,
    ILogger<CheckoutPlacementService> logger)
{
    public async Task<Result<Order>> PlaceAsync(Order cart, string actor, CancellationToken ct)
    {
        var consumeResult = await stockReservationService.ConsumeForOrderAsync(cart.Id, ct);
        if (consumeResult.IsFailure) return consumeResult.Errors;

        var advanceResult = cart.AdvanceCheckoutState(CheckoutState.Confirm);
        if (advanceResult.IsFailure) return advanceResult.Errors;

        var numberResult = await OrderNumber.GenerateAsync(dbContext, ct);
        if (numberResult.IsFailure) return numberResult.Errors;

        var placeResult = cart.Place(numberResult.Value);
        if (placeResult.IsFailure) return placeResult.Errors;

        await dbContext.SaveChangesAsync(ct);

        await SendOrderPlacedNotificationAsync(cart, ct);

        OrderLoggers.Placed(logger, Number: cart.Number, Id: cart.Id, ActionBy: actor);
        return Result<Order>.Ok(cart);
    }

    private async Task SendOrderPlacedNotificationAsync(Order order, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(order.Email)) return;

        var message = NotificationMessage.Create(
            NotificationUseCase.OrderConfirmed,
            NotificationRecipient.Create(order.Email, order.Number),
            NotificationChannel.Email,
            NotificationContext.Create(
                (NotificationParameterType.OrderNumber, order.Number),
                (NotificationParameterType.OrderTotal, order.Total.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)),
                (NotificationParameterType.UserFirstName, order.Email.Split('@')[0])));

        var result = await notificationService.SendAsync(message, ct);
        if (result.IsFailure)
            OrderLoggers.ConfirmationNotificationFailed(logger, order.Id, string.Join("; ", result.Errors.Select(f => f.Message)));
    }
}
```

- [ ] **Step 2: Verify the build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: Build succeeds (unused for now — consumed by TASK-015/TASK-016).

- [ ] **Step 3: Commit** (commit TASK-014 together with TASK-015, since it is dead code until then — see TASK-015).

#### TASK-015: CreateOrderFromCart — gate + offline skip + service

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs:17-96`

**Interfaces:**
- Consumes: `CheckoutPlacementService.PlaceAsync`, `GetPaymentForCheckoutQuery`, `MarkPaymentPaidCommand`.
- Produces: `CreateOrderFromCart.Response` unchanged (placed order detail).

- [ ] **Step 1: Replace the constructor + handler**

```csharp
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Orders.Shared.Mappings;
using Module.Billing.Features.Storefront.GetPaymentForCheckout;
using Module.Billing.Features.Storefront.MarkPaymentPaid;
using Module.Ordering.Services;

namespace Module.Ordering.Features.Storefront.Cart.Checkout;

public static partial class CreateOrderFromCart
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        ISender sender,
        CheckoutPlacementService placementService)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return OrderResult.Errors.UserNotAuthenticated;

            var cart = await dbContext.Set<Order>()
                .Include(x => x.LineItems)
                .Where(x => x.UserId == userId && x.Status == OrderStatus.Draft)
                .FirstOrDefaultAsync(cancellationToken);
            if (cart is null)
                return OrderResult.Errors.NotFound(Guid.Empty);

            if (cart.CheckoutState != CheckoutState.Payment)
                return OrderResult.Errors.InvalidCheckoutTransition(cart.CheckoutState, CheckoutState.Complete);

            var paymentResult = await sender.Send(
                new GetPaymentForCheckoutQuery { PaymentIntentId = command.Request.PaymentIntentId!, OrderId = cart.Id }, cancellationToken);
            if (paymentResult.IsFailure)
                return paymentResult.Errors;

            var p = paymentResult.Value!;
            var isPaid = p.IsCompleted || (p.State == "Pending" && p.IsOffline);
            if (!isPaid || p.Amount <= 0)
                return OrderResult.Errors.PaymentNotCompleted;

            // COD stays Pending: only gateway-completed payments are marked paid here.
            if (!p.IsOffline)
                await sender.Send(new MarkPaymentPaidCommand
                {
                    OrderId = cart.Id,
                    PaymentIntentId = command.Request.PaymentIntentId!
                }, cancellationToken);

            var placeResult = await placementService.PlaceAsync(cart, currentUser.UserName, cancellationToken);
            if (placeResult.IsFailure)
                return placeResult.Errors;

            return Result<Response>.Created(cart.MapToDetail<Response>());
        }
    }
}
```

- [ ] **Step 2: Remove now-unused usings** (`Module.Inventory.Services.StockReservations`, `Shared.Operational.Notifications.*`) — the compiler flags them (warnings-as-errors).

- [ ] **Step 3: Verify the build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: Build succeeds.

- [ ] **Step 4: Run Ordering tests**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter-query "//*Ordering*"`
Expected: existing Ordering tests pass (or update any that reference the old `CreateOrderFromCart` constructor — add a note if the constructor signature changed).

- [ ] **Step 5: Commit TASK-014 + TASK-015**

```bash
git add service/Api/src/Module/Ordering/Services/CheckoutPlacementService.cs \
        service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs
git commit -m "feat(ordering): gate placement on Pending+offline and extract placement service"
```

#### TASK-016: CompleteCheckoutForPayment command + handler

**Files:**
- Create: `service/Api/src/Module/Ordering/Features/Storefront/CompleteCheckoutForPayment/CompleteCheckoutForPayment.Command.cs`
- Create: `service/Api/src/Module/Ordering/Features/Storefront/CompleteCheckoutForPayment/CompleteCheckoutForPayment.cs`

**Interfaces:**
- Produces: `CompleteCheckoutForPaymentCommand { Guid CartId; Guid PaymentId } : ICommand<CompleteCheckoutForPaymentResponse>` and `CompleteCheckoutForPaymentResponse { Guid OrderId }`.

- [ ] **Step 1: Write the command + response records**

`CompleteCheckoutForPayment.Command.cs`:

```csharp
namespace Module.Ordering.Features.Storefront.CompleteCheckoutForPayment;

public sealed record CompleteCheckoutForPaymentCommand : ICommand<CompleteCheckoutForPaymentResponse>
{
    public Guid CartId { get; init; }
    public Guid PaymentId { get; init; }
}

public sealed record CompleteCheckoutForPaymentResponse
{
    public Guid OrderId { get; init; }
}
```

- [ ] **Step 2: Write the handler**

`CompleteCheckoutForPayment.cs`:

```csharp
using Module.Ordering.Domain.Orders;
using Module.Ordering.Services;

namespace Module.Ordering.Features.Storefront.CompleteCheckoutForPayment;

public sealed class CompleteCheckoutForPaymentCommandHandler(
    IApplicationDbContext dbContext,
    CheckoutPlacementService placementService)
    : ICommandHandler<CompleteCheckoutForPaymentCommand, CompleteCheckoutForPaymentResponse>
{
    public async Task<Result<CompleteCheckoutForPaymentResponse>> Handle(
        CompleteCheckoutForPaymentCommand command, CancellationToken cancellationToken)
    {
        var cart = await dbContext.Set<Order>()
            .Include(x => x.LineItems)
            .Where(x => x.Id == command.CartId && x.Status == OrderStatus.Draft)
            .FirstOrDefaultAsync(cancellationToken);

        // Idempotency: a no-longer-draft order was already placed by an earlier retry.
        if (cart is null)
            return new CompleteCheckoutForPaymentResponse { OrderId = command.CartId };

        if (cart.CheckoutState != CheckoutState.Payment)
            return OrderResult.Errors.InvalidCheckoutTransition(cart.CheckoutState, CheckoutState.Complete);

        var placeResult = await placementService.PlaceAsync(cart, "System", cancellationToken);
        if (placeResult.IsFailure)
            return placeResult.Errors;

        return new CompleteCheckoutForPaymentResponse { OrderId = cart.Id };
    }
}
```

- [ ] **Step 3: Verify the build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: Build succeeds.

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/CompleteCheckoutForPayment/
git commit -m "feat(ordering): add CompleteCheckoutForPayment command for webhook auto-placement"
```

### Implementation Phase 4: Webhook completion + expiry

- GOAL-004: On `checkout.session.completed`, complete the payment and auto-place the order; on `checkout.session.expired`, void and release stock.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-017 | Add `checkout.session.completed`/`expired` to `StripeWebhookDispatcher.SupportedEventTypes`. | | |
| TASK-018 | Handle both events in `ProcessStripeWebhookEventJob` (complete→place, expired→void+release). | | |

#### TASK-017: StripeWebhookDispatcher event types

**Files:**
- Modify: `service/Api/src/Module/Billing/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.cs:24-33`

**Interfaces:**
- Consumes: `GatewayConstants.WebhookEvents.Stripe.CheckoutSessionCompleted/CheckoutSessionExpired`.

- [ ] **Step 1: Add the two event types**

In the `SupportedEventTypes` array (after `PaymentIntentCanceled`):

```csharp
GatewayConstants.WebhookEvents.Stripe.CheckoutSessionCompleted,
GatewayConstants.WebhookEvents.Stripe.CheckoutSessionExpired,
```

- [ ] **Step 2: Verify the build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: Build succeeds.

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Billing/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.cs
git commit -m "feat(billing): subscribe to checkout.session webhook events"
```

#### TASK-018: ProcessStripeWebhookEventJob handlers

**Files:**
- Modify: `service/Api/src/Module/Billing/Backgrounds/ProcessStripeWebhookEventJob.cs:21-29` (constructor), `:44-68` (switch), add handlers after `HandlePaymentIntentCanceled`

**Interfaces:**
- Consumes: `CompleteCheckoutForPaymentCommand`, `IStockReservationService.ReleaseReservationsAsync(orderId, ct)`, `Stripe.Checkout.Session`.
- Produces: `payment.State == Completed` + order placed (session completed); `payment.State == Void` + stock released (session expired).

- [ ] **Step 1: Extend the constructor**

Add `ISender sender` and `IStockReservationService stockReservationService` parameters and store them as `_sender`/`_stockReservationService`. Add `using Module.Ordering.Features.Storefront.CompleteCheckoutForPayment;` and `using Module.Inventory.Services.StockReservations;` and `using Stripe.Checkout;`.

- [ ] **Step 2: Route the two event types**

In the `switch (stripeEvent.Type)`:

```csharp
case GatewayConstants.WebhookEvents.Stripe.CheckoutSessionCompleted:
    await HandleCheckoutSessionCompleted(stripeEvent, ct);
    break;
case GatewayConstants.WebhookEvents.Stripe.CheckoutSessionExpired:
    await HandleCheckoutSessionExpired(stripeEvent, ct);
    break;
```

- [ ] **Step 3: Add the completed handler**

```csharp
private async Task HandleCheckoutSessionCompleted(Event stripeEvent, CancellationToken ct)
{
    var session = stripeEvent.Data.Object as Session;
    if (session is null) return;

    var payment = await _dbContext.Set<PaymentCapture>()
        .FirstOrDefaultAsync(p => p.ResponseCode == session.Id, ct);
    if (payment is null) return;

    // Dedup: skip only if this exact Stripe event was fully processed before.
    if (payment.ProcessedStripeEventIds.Contains(stripeEvent.Id)) return;

    if (payment.State != PaymentRecordState.Completed)
    {
        var complete = payment.Complete();
        if (complete.IsFailure && payment.State != PaymentRecordState.Completed)
        {
            ProcessStripeWebhookEventJobLoggers.CannotCompletePayment(_logger, payment.Id, payment.State.ToString(), complete.Message);
            return;
        }
        await SaveWithRollbackAsync(payment, ct);
    }

    // Place the order. Idempotent: a no-longer-draft cart is a no-op on retry.
    var placeResult = await _sender.Send(
        new CompleteCheckoutForPaymentCommand { CartId = payment.OrderId, PaymentId = payment.Id }, ct);

    // Record the event as processed only after placement succeeds, so a Hangfire
    // retry re-attempts placement (and does not re-complete, due to the state guard).
    if (placeResult.IsSuccess)
    {
        payment.ProcessedStripeEventIds.Add(stripeEvent.Id);
        await SaveWithRollbackAsync(payment, ct);
    }
}
```

- [ ] **Step 4: Add the expired handler**

```csharp
private async Task HandleCheckoutSessionExpired(Event stripeEvent, CancellationToken ct)
{
    var session = stripeEvent.Data.Object as Session;
    if (session is null) return;

    var payment = await _dbContext.Set<PaymentCapture>()
        .FirstOrDefaultAsync(p => p.ResponseCode == session.Id, ct);
    if (payment is null) return;

    if (payment.ProcessedStripeEventIds.Contains(stripeEvent.Id)) return;
    if (payment.State is PaymentRecordState.Void or PaymentRecordState.Completed) return;

    var voidResult = payment.Void();
    if (voidResult.IsFailure)
    {
        ProcessStripeWebhookEventJobLoggers.CannotVoidPayment(_logger, payment.Id, payment.State.ToString(), voidResult.Message);
        return;
    }

    payment.ProcessedStripeEventIds.Add(stripeEvent.Id);
    await SaveWithRollbackAsync(payment, ct);

    await _stockReservationService.ReleaseReservationsAsync(orderId: payment.OrderId, ct: ct);
}
```

- [ ] **Step 5: Verify the build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: Build succeeds (0 warnings).

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Billing/Backgrounds/ProcessStripeWebhookEventJob.cs
git commit -m "feat(billing): auto-place order on checkout.session.completed and void on expired"
```

### Implementation Phase 5: Store SPA method selection + return route

- GOAL-005: Replace the embedded card form with a method choice, redirect card payers to Stripe Checkout, and add a `/checkout/return` polling page.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-019 | Extend SPA types/validations + add `getPaymentStatus` API. | | |
| TASK-020 | `useCheckout.createPaymentIntent` returns `checkoutUrl`/`state` and keys on `id`. | | |
| TASK-021 | `CheckoutView` renders method list and redirects card payers. | | |
| TASK-022 | Add `/checkout/return` route + `CheckoutReturnView.vue`. | | |

#### TASK-019: SPA types, validations, and getPaymentStatus API

**Files:**
- Modify: `app/Store/src/features/ordering/types/checkout.ts`
- Modify: `app/Store/src/features/ordering/validations/checkout.ts`
- Modify: `app/Store/src/features/payment/types/payment.ts`
- Modify: `app/Store/src/features/payment/services/paymentApi.ts`

**Interfaces:**
- Produces: `CreatePaymentIntentRequest.cancelUrl?: string`, `PaymentIntentResponse.checkoutUrl?: string`/`state?: string`, `PaymentStatusResponse`, `getPaymentStatus(orderId)`.

- [ ] **Step 1: Extend ordering types**

In `checkout.ts`, add `cancelUrl?: string` to `CreatePaymentIntentRequest` and extend `PaymentIntentResponse`:

```ts
export interface CreatePaymentIntentRequest {
  orderId: string
  paymentMethodId: string
  returnUrl?: string
  cancelUrl?: string
}

export interface PaymentIntentResponse {
  id: string
  clientSecret: string
  responseCode?: string
  checkoutUrl?: string
  state?: string
}
```

- [ ] **Step 2: Extend validation schemas**

In `validations/checkout.ts`:

```ts
export const CreatePaymentIntentRequestSchema = z.object({
  orderId: z.string().min(1),
  paymentMethodId: z.string().min(1),
  returnUrl: z.string().url().optional(),
  cancelUrl: z.string().url().optional(),
})

export const PaymentIntentResponseSchema = z.object({
  id: z.string(),
  clientSecret: z.string(),
  responseCode: z.string().optional(),
  checkoutUrl: z.string().optional(),
  state: z.string().optional(),
})
```

- [ ] **Step 3: Add PaymentStatusResponse type**

In `payment/types/payment.ts`:

```ts
// PaymentStatusResponse — GET api/storefront/cart/payment/intent/{orderId} (poll).
export interface PaymentStatusResponse {
  id: string
  orderId: string
  amount: number
  currency: string
  state: string
  isCompleted: boolean
}
```

- [ ] **Step 4: Add getPaymentStatus API**

In `payment/services/paymentApi.ts`, change the import to include `get` and add:

```ts
import { get, getPaged, post } from '@/shared/api'

// Call: Storefront payment API - poll payment status for an order.
export function getPaymentStatus(orderId: string): Promise<Result<PaymentStatusResponse>> {
  return get<Result<PaymentStatusResponse>>(`/api/storefront/cart/payment/intent/${orderId}`)
}
```

And import the type `PaymentStatusResponse` in the same file's type import.

- [ ] **Step 5: Verify SPA build/lint**

Run (in `app/Store`): `pnpm run lint` then `pnpm run build-only`
Expected: both pass with 0 warnings.

- [ ] **Step 6: Commit**

```bash
git add app/Store/src/features/ordering/types/checkout.ts \
        app/Store/src/features/ordering/validations/checkout.ts \
        app/Store/src/features/payment/types/payment.ts \
        app/Store/src/features/payment/services/paymentApi.ts
git commit -m "feat(store): add cancelUrl/checkoutUrl types and payment status API"
```

#### TASK-020: useCheckout.createPaymentIntent returns checkout info

**Files:**
- Modify: `app/Store/src/features/ordering/composables/useCheckout.ts:32-38,100-124,167-198`

**Interfaces:**
- Consumes: `CheckoutApi.createPaymentIntent({ orderId, paymentMethodId, returnUrl?, cancelUrl? })`.
- Produces: `checkoutUrl: Ref<string|null>`, `paymentState: Ref<string|null>` on the checkout store.

- [ ] **Step 1: Add state refs**

Next to `paymentClientSecret`:

```ts
const checkoutUrl = ref<string | null>(null)
const paymentState = ref<string | null>(null)
```

- [ ] **Step 2: Rewrite createPaymentIntent**

```ts
async function createPaymentIntent(methodId: string, opts: { returnUrl?: string; cancelUrl?: string } = {}): Promise<boolean> {
  loading.value = true
  error.value = null
  try {
    const cart = getCart()
    const result = await CheckoutApi.createPaymentIntent({
      orderId: cart.id!,
      paymentMethodId: methodId,
      returnUrl: opts.returnUrl,
      cancelUrl: opts.cancelUrl,
    })
    if (result.isSuccess) {
      // Payment id is the PaymentCapture.Id — stable across COD and gateway paths.
      paymentIntentId.value = result.value.id
      checkoutUrl.value = result.value.checkoutUrl ?? null
      paymentState.value = result.value.state ?? null
      paymentClientSecret.value = result.value.clientSecret
      paymentMethodId.value = methodId
    } else {
      error.value = result.message
    }
    loading.value = false
    return result.isSuccess
  } catch {
    error.value = 'Failed to create payment intent'
    loading.value = false
    return false
  }
}
```

- [ ] **Step 3: Reset the new refs**

In the regression watcher (where `paymentClientSecret` is cleared) and in `reset()`, add:

```ts
checkoutUrl.value = null
paymentState.value = null
```

- [ ] **Step 4: Expose in the returned reactive**

Add `checkoutUrl, paymentState` to the returned object (after `paymentClientSecret`).

- [ ] **Step 5: Verify SPA build/lint + existing tests**

Run (in `app/Store`): `pnpm run lint`, `pnpm run build-only`, `npx vitest run ordering`
Expected: pass; any `useCheckout`/`CheckoutView` tests referencing the old `paymentMethodToken` arg must be updated in TASK-021.

- [ ] **Step 6: Commit**

```bash
git add app/Store/src/features/ordering/composables/useCheckout.ts
git commit -m "feat(store): expose checkoutUrl and paymentState from createPaymentIntent"
```

#### TASK-021: CheckoutView method selection + redirect

**Files:**
- Modify: `app/Store/src/features/ordering/views/CheckoutView.vue` (script imports + payment panel + template step 3)
- Modify: `app/Store/src/features/ordering/views/__tests__/CheckoutView.spec.ts`

**Interfaces:**
- Consumes: `getPaymentMethods`, `checkout.createPaymentIntent(methodId, { returnUrl, cancelUrl })`, `checkout.checkoutUrl`.

- [ ] **Step 1: Replace card-element imports**

Remove `usePayment` import/usage (`const payment = usePayment()`, `payment.init()`, `mountCard`, `onContinueToReview` card tokenization). Add:

```ts
import { getPaymentMethods } from '@/features/payment/services/paymentApi'
import type { PaymentMethod } from '@/features/payment/types/payment'
```

- [ ] **Step 2: Add method-list state + loader**

```ts
// Methods: Customer-facing payment methods for the payment panel.
const paymentMethods = ref<PaymentMethod[]>([])
const selectedPaymentMethodId = ref<string | null>(null)

// Load: Fetch active, customer-facing payment methods for selection.
async function loadPaymentMethods(): Promise<void> {
  const result = await getPaymentMethods({ pageSize: 50 })
  paymentMethods.value = result.isSuccess ? result.items : []
  selectedPaymentMethodId.value = paymentMethods.value[0]?.id ?? null
}
```

- [ ] **Step 3: Replace the payment-panel watch**

Replace the existing `watch(() => checkout.displayStep, ...)` block that mounted the card with:

```ts
// Watch: Load payment methods when entering the payment panel.
watch(
  () => checkout.displayStep,
  async (step) => {
    if (step === 3) await loadPaymentMethods()
  },
  { immediate: true },
)
```

- [ ] **Step 4: Replace onContinueToReview with a branch**

```ts
// Action: Card → create intent and redirect to Stripe; COD → advance to review.
async function onContinueFromPayment(): Promise<void> {
  const method = paymentMethods.value.find((m) => m.id === selectedPaymentMethodId.value)
  if (!method) return

  const origin = window.location.origin
  const ok = await checkout.createPaymentIntent(method.id, {
    returnUrl: `${origin}/checkout/return`,
    cancelUrl: `${origin}/checkout`,
  })
  if (!ok) return

  // Card: hosted checkout redirect. COD: continue to review + place order.
  if (checkout.checkoutUrl) {
    window.location.href = checkout.checkoutUrl
    return
  }
  await advanceToReview()
}
```

- [ ] **Step 5: Rewrite the Payment `<StepPanel :value="3">` template**

Replace the card container markup with a radio list:

```html
<StepPanel :value="3">
  <div class="max-w-xl space-y-5">
    <Message v-if="checkout.error" severity="error" :closable="false">{{ checkout.error }}</Message>
    <Message v-if="paymentMethods.length === 0 && !checkout.loading" severity="warn" :closable="false">
      No payment methods are available.
    </Message>
    <!-- Section: Payment Methods — radio list of customer-facing methods -->
    <RadioButtonGroup v-model="selectedPaymentMethodId" class="flex flex-col gap-3">
      <div v-for="method in paymentMethods" :key="method.id" class="flex items-center gap-3">
        <RadioButton :input-id="`pm-${method.id}`" :value="method.id" />
        <Label :for="`pm-${method.id}`" class="cursor-pointer">{{ method.name }}</Label>
      </div>
    </RadioButtonGroup>
    <ButtonGroup>
      <Button label="Back" icon="pi pi-arrow-left" variant="text" @click="goToStep(2)" />
      <Button
        label="Continue"
        icon="pi pi-arrow-right"
        iconPos="right"
        :disabled="!selectedPaymentMethodId"
        :loading="checkout.loading"
        @click="onContinueFromPayment"
      />
    </ButtonGroup>
  </div>
</StepPanel>
```

- [ ] **Step 6: Update CheckoutView.spec.ts**

Update the spec to mock `getPaymentMethods` returning a COD + a Stripe method, assert the radio list renders, and assert `window.location.href` is set when a card method is chosen. Remove assertions about the Stripe card element.

- [ ] **Step 7: Verify SPA build/lint + tests**

Run (in `app/Store`): `pnpm run lint`, `pnpm run build-only`, `npx vitest run ordering`
Expected: pass with 0 warnings.

- [ ] **Step 8: Commit**

```bash
git add app/Store/src/features/ordering/views/CheckoutView.vue \
        app/Store/src/features/ordering/views/__tests__/CheckoutView.spec.ts
git commit -m "feat(store): payment method selection with Stripe Checkout redirect"
```

#### TASK-022: /checkout/return route + polling view

**Files:**
- Create: `app/Store/src/features/ordering/views/CheckoutReturnView.vue`
- Modify: `app/Store/src/features/ordering/routes/index.ts`

**Interfaces:**
- Consumes: `getPaymentStatus(orderId)`.
- Produces: a confirmation view that polls until `isCompleted`.

- [ ] **Step 1: Add the route**

In `ordering/routes/index.ts`, after the checkout route:

```ts
{ path: '/checkout/return', name: 'checkout-return', component: () => import('../views/CheckoutReturnView.vue'), meta: { requiresAuth: true, title: 'Payment Return' } },
```

- [ ] **Step 2: Write the view**

```vue
<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { getPaymentStatus } from '@/features/payment/services/paymentApi'

usePageTitle('Processing Payment')

const route = useRoute()
const status = ref<'polling' | 'completed' | 'timeout'>('polling')
const error = ref<string | null>(null)
let timer: ReturnType<typeof setInterval> | null = null

// Poll: Wait for the webhook to complete the payment and auto-place the order.
async function poll(): Promise<void> {
  const orderId = typeof route.query.order === 'string' ? route.query.order : null
  if (!orderId) {
    status.value = 'timeout'
    error.value = 'Missing order reference. Please check your orders.'
    return
  }
  const result = await getPaymentStatus(orderId)
  if (result.isSuccess && result.value.isCompleted) {
    status.value = 'completed'
    stopPolling()
  } else if (!result.isSuccess) {
    error.value = result.message ?? 'Could not read payment status.'
  }
}

function stopPolling(): void {
  if (timer) { clearInterval(timer); timer = null }
}

onMounted(() => {
  void poll()
  let attempts = 0
  timer = setInterval(async () => {
    attempts += 1
    if (attempts > 30) { status.value = 'timeout'; stopPolling(); return }
    await poll()
  }, 2000)
})

onUnmounted(stopPolling)
</script>

<template>
  <div class="mx-auto max-w-xl space-y-5 px-4 py-8">
    <h1 class="text-2xl font-bold">Payment</h1>
    <Message v-if="status === 'polling'" severity="info" :closable="false">
      Confirming your payment…
    </Message>
    <Message v-if="status === 'completed'" severity="success" :closable="false">
      Your order has been placed. A confirmation email is on its way.
    </Message>
    <Message v-if="status === 'timeout'" severity="warn" :closable="false">
      We're still confirming your payment. {{ error ?? 'Check your orders in a moment.' }}
    </Message>
    <Button as="router-link" to="/account/orders" label="View My Orders" icon="pi pi-receipt" />
  </div>
</template>
```

- [ ] **Step 3: Verify SPA build/lint**

Run (in `app/Store`): `pnpm run lint`, `pnpm run build-only`
Expected: pass with 0 warnings.

- [ ] **Step 4: Commit**

```bash
git add app/Store/src/features/ordering/views/CheckoutReturnView.vue \
        app/Store/src/features/ordering/routes/index.ts
git commit -m "feat(store): add checkout return page that polls payment status"
```

### Implementation Phase 6: Integration tests + full verification

- GOAL-006: Prove the end-to-end contract with integration tests and run the full verification suite.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-023 | Add Api.Tests scenarios for COD (Pending) and card (checkoutUrl). | | |
| TASK-024 | Run full backend + SPA + convention verification. | | |

#### TASK-023: Api.Tests integration scenarios

**Files:**
- Modify: `service/Api/tests/Api.Tests/Scenarios/Billing/CreateIntent/CreateIntent.IntegrationTests.cs`

**Interfaces:**
- Consumes: `POST /api/storefront/cart/payment/intent` with `{ orderId, paymentMethodId, returnUrl, cancelUrl }`; seeded `cash_on_delivery` and `stripe` methods.

- [ ] **Step 1: Add a COD scenario**

Follow the existing test's fixture (authenticate, build cart, advance to `Delivery`), then:

```csharp
[Fact(DisplayName = "COD intent returns Pending without a gateway")]
public async Task CreateIntent_Cod_ReturnsPending()
{
    var codMethodId = /* seed/insert a PaymentMethod with ProviderKey = GatewayConstants.Providers.CashOnDelivery */;

    var response = await PostAsync("/api/storefront/cart/payment/intent", new
    {
        orderId = cartId,
        paymentMethodId = codMethodId,
    });

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var body = await ReadJsonAsync(response);
    body.state.Should().Be("Pending");
    body.checkoutUrl.Should().BeNull();
}
```

- [ ] **Step 2: Add a card scenario**

```csharp
[Fact(DisplayName = "Card intent returns a checkout URL")]
public async Task CreateIntent_Card_ReturnsCheckoutUrl()
{
    var stripeMethodId = /* seed/insert a PaymentMethod with ProviderKey = GatewayConstants.Providers.Stripe */;

    var response = await PostAsync("/api/storefront/cart/payment/intent", new
    {
        orderId = cartId,
        paymentMethodId = stripeMethodId,
        returnUrl = "https://store.test/checkout/return",
        cancelUrl = "https://store.test/checkout",
    });

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var body = await ReadJsonAsync(response);
    body.checkoutUrl.Should().NotBeNull();
}
```

Note: card runs against the registered gateway. If the test environment registers `Bogus` for `stripe`, the fake session URL is returned; otherwise the Stripe gateway is stubbed/mocked per the existing fixture. Reuse the fixture's existing gateway-registration mechanism.

- [ ] **Step 3: Run the integration tests**

Run: `dotnet test service/Api/tests/Api.Tests --filter "FullyQualifiedName~CreateIntent"`
Expected: pass (requires Docker/Postgres for Testcontainers per AGENTS.md).

- [ ] **Step 4: Commit**

```bash
git add service/Api/tests/Api.Tests/Scenarios/Billing/CreateIntent/CreateIntent.IntegrationTests.cs
git commit -m "test(billing): cover COD Pending and card checkout-url intents"
```

#### TASK-024: Full verification

- [ ] **Step 1: Backend build + unit tests**

```bash
dotnet build
dotnet test service/Api/tests/Module.UnitTests
dotnet test service/Api/tests/Shared.UnitTests
```

Expected: build 0 warnings; all unit tests pass.

- [ ] **Step 2: Store SPA verification**

```bash
cd app/Store && pnpm run lint && pnpm run build-only && pnpm run test:unit
```

Expected: lint/build 0 warnings; all unit tests pass.

- [ ] **Step 3: Convention drift checks**

```bash
bash scripts/check-feature-conventions.sh
bash scripts/check-cross-module-refs.sh
```

Expected: feature-convention check passes; cross-module check may report the pre-existing Billing→Ordering references plus the new `CompleteCheckoutForPaymentCommand` reference (documented in Risks).

- [ ] **Step 4: Commit any remaining verification fixes**

```bash
git add -A
git commit -m "chore: final verification fixes for payment method selection"
```

## 3. Alternatives

- **ALT-001**: Stripe PaymentIntent redirect (no Checkout Session) — rejected: more frontend Stripe code (`stripe.confirmPayment` with `redirect`), less of a true hosted page, and 3DS/wallet handling stays semi-custom.
- **ALT-002**: Explicit `RequiresGateway` flag on `PaymentMethod` + domain events for placement — rejected: needs a migration + wider CRUD/mapping churn; provider-key dispatch matches the existing pattern.
- **ALT-003**: Reuse `processingService.ProcessAsync` and extend it for Checkout — rejected: intent creation no longer purchases inline; a dedicated `CreateCheckoutSessionAsync` keeps the gateway contract explicit.

## 4. Dependencies

- **DEP-001**: Stripe.net 52.1.0 — `Stripe.Checkout.SessionService` (existing package).
- **DEP-002**: `IGatewayRegistry` / `IPaymentGatewayActionProvider` (existing) — session method added.
- **DEP-003**: `IStockReservationService` (existing) — reserve/release/consume.
- **DEP-004**: `ISender` (MediatR) — `GetPaymentForCheckout`, `AdvanceCheckoutState`, `MarkPaymentPaid`, `CompleteCheckoutForPayment`.
- **DEP-005**: `GetPaymentStatus` endpoint (existing) — return-page poll.
- **DEP-006**: `@stripe/stripe-js` no longer required by checkout (kept for SetupIntent flows only).

## 5. Files

- **FILE-001**: `service/Api/src/Module/Billing/Services/Provider/GatewayConstants.cs` — COD provider + webhook events.
- **FILE-002**: `service/Api/src/Module/Billing/Services/Provider/PaymentGatewayResponse.cs` — `CheckoutUrl`.
- **FILE-003**: `service/Api/src/Module/Billing/Domain/PaymentCaptures/PaymentCapture.cs` — `CheckoutUrl`.
- **FILE-004**: `service/Api/src/Module/Billing/Persistence/Configurations/Payments/PaymentRecordConfiguration.cs` — column config.
- **FILE-005**: `service/Api/src/Migrations/Migrations/*_AddPaymentCaptureCheckoutUrl.cs` (generated).
- **FILE-006**: `service/Api/src/Module/Billing/Services/Provider/IPaymentGatewayActionProvider.cs` + `Gateway.cs` — session method.
- **FILE-007**: `service/Api/src/Module/Billing/Services/Provider/Stripe/StripeGateway.cs` — session impl.
- **FILE-008**: `service/Api/src/Module/Billing/Services/Provider/Bogus/BogusGateway.cs` — fake session.
- **FILE-009**: `service/Api/src/Module/Billing/Persistence/Seeders/PaymentMethod.Seeder.cs` — COD seed.
- **FILE-010**: `service/Api/src/Module/Billing/Features/Storefront/Payment/Methods/ListPaymentMethods.cs` — DisplayOn filter.
- **FILE-011**: `service/Api/src/Module/Billing/Features/Storefront/Payment/Shared/Models/PaymentStore.Model.Response.cs` + `Shared/Mappings/PaymentStore.Mapping.cs` — CheckoutUrl.
- **FILE-012**: `service/Api/src/Module/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.Request.cs` — CancelUrl.
- **FILE-013**: `service/Api/src/Module/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs` — branch.
- **FILE-014**: `service/Api/src/Module/Billing/Features/Storefront/GetPaymentForCheckout/` — identifier + State/IsOffline.
- **FILE-015**: `service/Api/src/Module/Billing/Features/Storefront/MarkPaymentPaid/MarkPaymentPaid.cs` — identifier + idempotency.
- **FILE-016**: `service/Api/src/Module/Ordering/Services/CheckoutPlacementService.cs` (new).
- **FILE-017**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs` — gate + service.
- **FILE-018**: `service/Api/src/Module/Ordering/Features/Storefront/CompleteCheckoutForPayment/` (new).
- **FILE-019**: `service/Api/src/Module/Billing/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.cs` — event types.
- **FILE-020**: `service/Api/src/Module/Billing/Backgrounds/ProcessStripeWebhookEventJob.cs` — handlers.
- **FILE-021**: `app/Store/src/features/ordering/types/checkout.ts` + `validations/checkout.ts`.
- **FILE-022**: `app/Store/src/features/payment/types/payment.ts` + `services/paymentApi.ts`.
- **FILE-023**: `app/Store/src/features/ordering/composables/useCheckout.ts`.
- **FILE-024**: `app/Store/src/features/ordering/views/CheckoutView.vue` (+ spec).
- **FILE-025**: `app/Store/src/features/ordering/views/CheckoutReturnView.vue` (new) + `routes/index.ts`.
- **FILE-026**: `service/Api/tests/Module.UnitTests/Billing/.../CreateIntent/CreatePaymentIntentTests.cs`.
- **FILE-027**: `service/Api/tests/Api.Tests/Scenarios/Billing/CreateIntent/CreateIntent.IntegrationTests.cs`.

## 6. Testing

- **TEST-001**: `CreatePaymentIntentTests` — COD Pending (no gateway), Stripe CheckoutUrl mapping, session-failure no-persist.
- **TEST-002**: Ordering unit tests — `CreateOrderFromCart` allows `Pending+offline`, rejects `Pending+gateway`, skips `MarkPaymentPaid` for offline.
- **TEST-003**: `GetPaymentForCheckout` — `State`/`IsOffline` and `Id`-based lookup (add a unit test if none exists).
- **TEST-004**: Webhook job — `checkout.session.completed` completes + sends `CompleteCheckoutForPayment`; `expired` voids + releases.
- **TEST-005**: `CheckoutView.spec.ts` — method list renders, card selection sets `window.location.href`.
- **TEST-006**: Api.Tests — COD `Pending`, card `checkoutUrl`.
- **TEST-007**: Manual smoke via `ApiTests/Billing/create-intent.http` and `ApiTests/Ordering/demo-flow.http` (updated to pass `cancelUrl` and assert `checkoutUrl`).

## 7. Risks & Assumptions

- **RISK-001**: Billing→Ordering reference for `CompleteCheckoutForPaymentCommand` adds to the known cross-module references (consistent with `AdvanceCheckoutState`). `check-cross-module-refs.sh` will flag it.
- **RISK-002**: `CheckoutPlacementService` moves `OrderLoggers.Placed` actor from `currentUser.UserName`; the webhook path uses `"System"`.
- **RISK-003**: Stripe currency case — `Currency` default is `"USD"`; the session lowercases it. Confirm Stripe accepts the lowercase form in the dev secret.
- **RISK-004**: In-memory `useCart` state is lost across the Stripe redirect; the return page keys off the `order` query param, not cart state.
- **RISK-005**: Hangfire webhook job has no `ICurrentUser`; `CompleteCheckoutForPayment` is designed user-context-free.
- **RISK-006**: The existing `payment_intent.succeeded` handler no-ops for Checkout sessions (documented, not removed).
- **ASSUMPTION-001**: `Session` (`Stripe.Checkout.Session`) and `SessionService` exist in Stripe.net 52.1.0 with `SuccessUrl`/`CancelUrl`/`LineItems`.
- **ASSUMPTION-002**: The storefront `GetPaymentStatus` endpoint stays keyed by `orderId`.
- **ASSUMPTION-003**: COD does not need a gateway; it is purely a domain state (`Pending`) captured later via the admin capture flow.

## 8. Related Specifications / Further Reading

- [Design spec](docs/superpowers/specs/2026-08-13-payment-method-selection-design.md)
- [Store SPA AGENTS.md — comment standard](app/Store/AGENTS.md)
- [EF migration guide](service/Api/src/Migrations/GUIDE.yaml)
- [Stripe Checkout Sessions](https://docs.stripe.com/checkout)
- [Stripe.net SessionService](https://github.com/stripe/stripe-dotnet)

