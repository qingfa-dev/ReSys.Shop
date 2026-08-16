# Stripe Checkout PaymentIntent Metadata Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the `payment_intent.*` webhook correlate to a `PaymentCapture` via metadata in the Checkout flow, by propagating `payment_id` onto the Stripe `PaymentIntent` the Checkout Session creates.

**Architecture:** The fix is confined to `StripeGateway.CreateCheckoutSessionAsync`. It currently stamps `Metadata` only on the Checkout **Session**; Stripe does not copy session metadata to the PaymentIntent it spawns. We extract the session-options builder into an `internal static` method (testable via the existing `InternalsVisibleTo Module.UnitTests`) and add `PaymentIntentData.Metadata` mirroring the session metadata. The webhook-side `FindPaymentByIntentAsync` fallback (`p.Number == intent.Metadata["payment_id"]`) then works as originally designed.

**Tech Stack:** .NET 10, C# (warnings-as-errors), Stripe.net 52.3.0, xunit v3 (MTP runner) + FluentAssertions.

## Global Constraints

- `TreatWarningsAsErrors=true` — any warning fails the build; test code must not trigger nullable-reference warnings.
- Result objects, not exceptions (already satisfied; this is a gateway wrapper, no change).
- No schema change, no migration, no behavior change outside `StripeGateway`.
- `Module` assembly has `InternalsVisibleTo Module.UnitTests` (`Module.csproj:10`).
- Test runner: `dotnet test --filter` does NOT work (xunit v3 MTP rejects it — "Zero tests ran", exit 5). Run a single class via the built binary's `-class` flag.
- Metadata keys are constants: `GatewayConstants.Metadata.OrderIdKey = "order_id"`, `PaymentIdKey = "payment_id"` (`GatewayConstants.cs:41-42`).
- `GatewayConstants.Amounts.CentsMultiplier = 100` (`GatewayConstants.cs:107`).

---

## File Structure

- **Modify:** `service/Api/src/Module/Billing/Services/Provider/Stripe/StripeGateway.cs` — extract `BuildCheckoutSessionOptions`, add `PaymentIntentData`.
- **Create:** `service/Api/tests/Module.UnitTests/Billing/Services/Provider/Stripe/StripeGatewayCheckoutSessionTests.cs` — unit tests for the options builder.

---

### Task 1: Extract `BuildCheckoutSessionOptions` (pure refactor) + regression test

**Files:**
- Modify: `service/Api/src/Module/Billing/Services/Provider/Stripe/StripeGateway.cs:188-228`
- Create: `service/Api/tests/Module.UnitTests/Billing/Services/Provider/Stripe/StripeGatewayCheckoutSessionTests.cs`

**Interfaces:**
- Consumes: `StripeGateway` (existing `public sealed class`, `internal static` members visible to `Module.UnitTests` via `InternalsVisibleTo`); `GatewayOptions` record (`Email`, `Customer`, `OrderId`, `PaymentId`, `IdempotencyKey` required; `Currency` defaults to `GatewayConstants.Currency.Usd`).
- Produces: `StripeGateway.BuildCheckoutSessionOptions(decimal amount, GatewayOptions options) : SessionCreateOptions` — used by Task 2 and by `CreateCheckoutSessionAsync`.

- [ ] **Step 1: Write the failing test**

Create `service/Api/tests/Module.UnitTests/Billing/Services/Provider/Stripe/StripeGatewayCheckoutSessionTests.cs`:

```csharp
using Module.Billing.Services.Provider;
using Module.Billing.Services.Provider.Stripe;

using Stripe.Checkout;

namespace Module.UnitTests.Payment.Services.Provider.Stripe;

[Trait("Category", "Unit")]
[Trait("Module", "Billing")]
[Trait("Feature", "StripeGatewayCheckoutSession")]
public class StripeGatewayCheckoutSessionTests
{
    private static GatewayOptions BuildOptions() => new()
    {
        Email = "test@example.com",
        Customer = "test@example.com",
        OrderId = "order-0001",
        PaymentId = "PAY-20260816-ABC123",
        IdempotencyKey = "shop-PAY-20260816-ABC123",
        Currency = "USD",
    };

    [Fact(DisplayName = "BuildCheckoutSessionOptions: preserves session metadata and line item")]
    public void BuildCheckoutSessionOptions_PreservesSessionMetadataAndLineItem()
    {
        var so = StripeGateway.BuildCheckoutSessionOptions(100m, BuildOptions());

        so.Metadata.Should().ContainKey(GatewayConstants.Metadata.OrderIdKey)
            .WhoseValue.Should().Be("order-0001");
        so.Metadata.Should().ContainKey(GatewayConstants.Metadata.PaymentIdKey)
            .WhoseValue.Should().Be("PAY-20260816-ABC123");

        so.LineItems.Should().NotBeNull();
        var line = so.LineItems!.Single();
        line.Quantity.Should().Be(1);
        line.PriceData.Should().NotBeNull();
        line.PriceData!.Currency.Should().Be("usd");
        line.PriceData.UnitAmount.Should().Be(10000);
        line.PriceData.ProductData.Should().NotBeNull();
        line.PriceData.ProductData!.Name.Should().Be("Order order-0001");
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run:
```bash
dotnet build service/Api/tests/Module.UnitTests/Module.UnitTests.csproj -v q --nologo
```
Expected: FAIL — build error `CS0117: 'StripeGateway' does not contain a definition for 'BuildCheckoutSessionOptions'`.

- [ ] **Step 3: Extract the method**

In `StripeGateway.cs`, replace the body of `CreateCheckoutSessionAsync` (lines 188-228) so the `SessionCreateOptions` construction moves into a new `internal static` method. Replace:

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

with:

```csharp
    public override async Task<Result<PaymentGatewayResponse>> CreateCheckoutSessionAsync(
        decimal amount, GatewayOptions options, CancellationToken ct = default)
    {
        try
        {
            var so = BuildCheckoutSessionOptions(amount, options);
            var ro = BuildRequestOptions(options);
            var session = await _sessionService.CreateAsync(so, ro, ct).ConfigureAwait(false);
            return new PaymentGatewayResponse(GatewayConstants.Providers.Stripe,
                authorization: session.Id, checkoutUrl: session.Url);
        }
        catch (StripeException ex) { return MapStripeException(ex); }
    }

    internal static SessionCreateOptions BuildCheckoutSessionOptions(decimal amount, GatewayOptions options)
    {
        return new SessionCreateOptions
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
    }
```

- [ ] **Step 4: Run test to verify it passes**

Run:
```bash
dotnet build service/Api/tests/Module.UnitTests/Module.UnitTests.csproj -v q --nologo
cd service/Api/tests/Module.UnitTests/bin/Debug/net10.0
./Module.UnitTests -class "Module.UnitTests.Payment.Services.Provider.Stripe.StripeGatewayCheckoutSessionTests"
```
Expected: PASS (`Total: 1, Failed: 0`).

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Billing/Services/Provider/Stripe/StripeGateway.cs \
        service/Api/tests/Module.UnitTests/Billing/Services/Provider/Stripe/StripeGatewayCheckoutSessionTests.cs
git commit -m "refactor(billing): extract BuildCheckoutSessionOptions from CreateCheckoutSessionAsync"
```

---

### Task 2: Propagate metadata to the Checkout PaymentIntent

**Files:**
- Modify: `service/Api/src/Module/Billing/Services/Provider/Stripe/StripeGateway.cs` (the `BuildCheckoutSessionOptions` method added in Task 1)
- Modify: `service/Api/tests/Module.UnitTests/Billing/Services/Provider/Stripe/StripeGatewayCheckoutSessionTests.cs`

**Interfaces:**
- Consumes: `StripeGateway.BuildCheckoutSessionOptions(decimal amount, GatewayOptions options)` (Task 1); `SessionCreateOptions.PaymentIntentData` (type `SessionPaymentIntentDataOptions`, `Metadata` property `Dictionary<string, string>`); `GatewayConstants.Metadata.*`.
- Produces: a `SessionCreateOptions` whose `PaymentIntentData.Metadata` carries `order_id` and `payment_id` — consumed by Stripe at checkout to stamp the resulting `PaymentIntent`, which `FindPaymentByIntentAsync` reads (`ProcessStripeWebhookEventJob.cs:531-541`).

- [ ] **Step 1: Write the failing test**

Append to `StripeGatewayCheckoutSessionTests.cs` (before the closing `}` of the class):

```csharp
    [Fact(DisplayName = "BuildCheckoutSessionOptions: propagates metadata to the PaymentIntent")]
    public void BuildCheckoutSessionOptions_PopulatesPaymentIntentMetadata()
    {
        var so = StripeGateway.BuildCheckoutSessionOptions(100m, BuildOptions());

        so.PaymentIntentData.Should().NotBeNull();
        so.PaymentIntentData!.Metadata.Should().ContainKey(GatewayConstants.Metadata.OrderIdKey)
            .WhoseValue.Should().Be("order-0001");
        so.PaymentIntentData.Metadata.Should().ContainKey(GatewayConstants.Metadata.PaymentIdKey)
            .WhoseValue.Should().Be("PAY-20260816-ABC123");
    }
```

- [ ] **Step 2: Run test to verify it fails**

Run:
```bash
dotnet build service/Api/tests/Module.UnitTests/Module.UnitTests.csproj -v q --nologo
cd service/Api/tests/Module.UnitTests/bin/Debug/net10.0
./Module.UnitTests -class "Module.UnitTests.Payment.Services.Provider.Stripe.StripeGatewayCheckoutSessionTests"
```
Expected: FAIL — `Expected so.PaymentIntentData not to be <null>` (before the fix `PaymentIntentData` is never set).

- [ ] **Step 3: Add `PaymentIntentData` to the builder**

In `StripeGateway.BuildCheckoutSessionOptions`, add a `PaymentIntentData` initializer directly after the `Metadata` initializer (inside the `SessionCreateOptions` object initializer):

```csharp
            Metadata = new Dictionary<string, string>
            {
                [GatewayConstants.Metadata.OrderIdKey] = options.OrderId,
                [GatewayConstants.Metadata.PaymentIdKey] = options.PaymentId
            },
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    [GatewayConstants.Metadata.OrderIdKey] = options.OrderId,
                    [GatewayConstants.Metadata.PaymentIdKey] = options.PaymentId
                }
            },
```

- [ ] **Step 4: Run tests to verify they pass**

Run:
```bash
dotnet build service/Api/tests/Module.UnitTests/Module.UnitTests.csproj -v q --nologo
cd service/Api/tests/Module.UnitTests/bin/Debug/net10.0
./Module.UnitTests -class "Module.UnitTests.Payment.Services.Provider.Stripe.StripeGatewayCheckoutSessionTests"
./Module.UnitTests -class "Module.UnitTests.Payment.Services.Provider.Stripe.StripeGatewayTests"
```
Expected: both PASS (`Failed: 0`).

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Billing/Services/Provider/Stripe/StripeGateway.cs \
        service/Api/tests/Module.UnitTests/Billing/Services/Provider/Stripe/StripeGatewayCheckoutSessionTests.cs
git commit -m "fix(billing): propagate payment_id metadata to checkout PaymentIntent"
```

---

### Verification (after both tasks)

```bash
dotnet build service/Api/tests/Module.UnitTests/Module.UnitTests.csproj -v q --nologo
cd service/Api/tests/Module.UnitTests/bin/Debug/net10.0
./Module.UnitTests -class "Module.UnitTests.Payment.Services.Provider.Stripe.StripeGatewayCheckoutSessionTests"
```

Expected: 2 tests pass, build clean (0 warnings / 0 errors).

## Self-Review

- **Spec coverage:** Change → Task 1 (extract) + Task 2 (`PaymentIntentData`); Data flow → Task 2 test asserts `PaymentIntentData.Metadata` keys; Testing (spec §Testing items 1-2) → Task 1 regression + Task 2 metadata test; webhook-side test `HandlePaymentIntentSucceeded_WhenOnlySessionIdStored_FindsByMetadata` remains untouched and now matches production. All covered.
- **Placeholder scan:** no TBD/TODO; full code in every step.
- **Type consistency:** `BuildCheckoutSessionOptions(decimal, GatewayOptions)` matches across Tasks 1-2; `SessionPaymentIntentDataOptions.Metadata` verified present in Stripe.net 52.3.0; `GatewayOptions` required members (`Email`, `Customer`, `OrderId`, `PaymentId`, `IdempotencyKey`) all set in `BuildOptions()`; `CentsMultiplier = 100` so `100m → 10000` cents.
- **Test-nullability:** test code uses `Should().NotBeNull()` + `!` and `.Single()`/`ContainKey(...).WhoseValue` to avoid nullable-reference warnings under `TreatWarningsAsErrors=true`.

## Execution Handoff

Plan complete and saved to `docs/superpowers/plans/2026-08-16-stripe-checkout-paymentintent-metadata.md`. Two execution options:

1. **Subagent-Driven (recommended)** — dispatch a fresh subagent per task, review between tasks.
2. **Inline Execution** — execute tasks in this session with checkpoints.

Which approach?
