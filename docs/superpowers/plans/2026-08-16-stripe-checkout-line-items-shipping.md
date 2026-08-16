# Stripe Checkout Per-Product Line Items + Shipping Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Send one Stripe Checkout Session line item per cart product (name, quantity, unit price) plus a fixed-amount shipping charge, instead of a single generic "Order <id>" line.

**Architecture:** Three sequential tasks: (1) enrich the `GetCartForCheckout` projection so line items carry product name/unit price and the response carries shipping; (2) widen the gateway contract (`GatewayOptions`) and populate it in `CreatePaymentIntent`; (3) rewrite `StripeGateway.BuildCheckoutSessionOptions` to emit per-product lines and a shipping option, with an aggregate-line fallback for empty line items.

**Tech Stack:** .NET 10, C# (warnings-as-errors), EF Core InMemory (tests), Stripe.net 52.3.0, xunit v3 (MTP runner) + FluentAssertions.

## Global Constraints

- `TreatWarningsAsErrors=true` — any warning fails the build; test code must not trigger nullable-reference warnings (use `Should().NotBeNull()` + `!` and `.Single()`).
- Result objects, not exceptions (domain factories return `Result<T>`; handlers return `Result`).
- Test runner: `dotnet test --filter` does NOT work (xunit v3 MTP rejects it — "Zero tests ran", exit 5). Run a single class via the built binary's `-class` flag.
- Single assembly `Module` holds all domain types; tests set `ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly]`.
- Stripe-only scope: the Bogus/dev gateway and other `CreateCheckoutSessionAsync` callers must be unaffected (the empty-line-items fallback preserves today's behavior).
- `GatewayConstants.Amounts.CentsMultiplier = 100`; metadata keys `OrderIdKey="order_id"`, `PaymentIdKey="payment_id"`.
- Stripe.net 52.3.0 types verified present: `SessionShippingOptionOptions`, `SessionShippingOptionShippingRateDataOptions`, `SessionShippingOptionShippingRateDataFixedAmountOptions` (properties `Type`, `FixedAmount` (`Amount`, `Currency`), `DisplayName`).

---

## File Structure

- **Modify:** `service/Api/src/Module/Ordering/Features/Storefront/GetCartForCheckout/GetCartForCheckout.cs` — add product/shipping includes + projection.
- **Modify:** `service/Api/src/Module/Ordering/Features/Storefront/GetCartForCheckout/GetCartForCheckout.Response.cs` — add `Name`/`UnitPrice`/`ShipmentTotal`/`ShippingMethodName`.
- **Create:** `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/GetCartForCheckout/GetCartForCheckoutTests.cs` — projection test.
- **Modify:** `service/Api/src/Module/Billing/Services/Provider/GatewayOptions.cs` — add `LineItems`, `ShippingDisplayName`, `GatewayLineItem`.
- **Modify:** `service/Api/src/Module/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs` — populate options.
- **Modify:** `service/Api/tests/Module.UnitTests/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntentTests.cs` — assert options populated.
- **Modify:** `service/Api/src/Module/Billing/Services/Provider/Stripe/StripeGateway.cs` — per-product lines + shipping.
- **Modify:** `service/Api/tests/Module.UnitTests/Billing/Services/Provider/Stripe/StripeGatewayCheckoutSessionTests.cs` — new tests.

---

### Task 1: Enrich `GetCartForCheckout` projection (Ordering)

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/GetCartForCheckout/GetCartForCheckout.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/GetCartForCheckout/GetCartForCheckout.Response.cs`
- Create: `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/GetCartForCheckout/GetCartForCheckoutTests.cs`

**Interfaces:**
- Consumes: `LineItem` (`VariantId`, `Quantity`, `Price`, `Variant` nav → `Product.Name`); `Order` (`LineItems`, `ShipmentTotal`, `ShippingMethod` nav → `ShippingMethod.Name`); factories `OrderMethod.Create`, `LineItemMethod.Create`, `ProductMethod.Create`, `VariantMethod.Create`, `ShippingMethodMethod.Create`.
- Produces: `GetCartForCheckoutResponse` with `CartLineItem { VariantId, Quantity, Name, UnitPrice }`, `ShipmentTotal`, `ShippingMethodName` — consumed by Task 2.

- [ ] **Step 1: Write the failing test**

Create `service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/GetCartForCheckout/GetCartForCheckoutTests.cs`:

```csharp
using Microsoft.EntityFrameworkCore;

using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Variants;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Storefront.GetCartForCheckout;
using Module.Shipping.Domain.ShippingMethods;

namespace Module.UnitTests.Ordering.Features.Storefront.GetCartForCheckout;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "GetCartForCheckout")]
public class GetCartForCheckoutTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;

    public GetCartForCheckoutTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
    }

    [Fact(DisplayName = "projects product name, unit price, shipment total, and shipping method name")]
    public async Task Handle_ProjectsLineItemMetadata_AndShipping()
    {
        var ct = TestContext.Current.CancellationToken;
        var product = ProductMethod.Create("Classic Tee").Value;
        var variant = VariantMethod.Create(product.Id, "TEE-BLK-M").Value;
        var shippingMethod = ShippingMethodMethod.Create("Express", "flat_rate").Value;

        var order = OrderMethod.Create("USD", Guid.NewGuid()).Value;
        order.ShippingMethodId = shippingMethod.Id;
        order.ShipmentTotal = 12.50m;
        order.Total = 37.50m;

        _dbContext.Set<Product>().Add(product);
        _dbContext.Set<Variant>().Add(variant);
        _dbContext.Set<ShippingMethod>().Add(shippingMethod);
        _dbContext.Set<Order>().Add(order);
        _dbContext.Set<LineItem>().Add(LineItemMethod.Create(order.Id, variant.Id, 2, 12.50m).Value);
        await _dbContext.SaveChangesAsync(ct);

        var handler = new GetCartForCheckoutQueryHandler(_dbContext);
        var result = await handler.Handle(new GetCartForCheckoutQuery { CartId = order.Id }, ct);

        result.IsSuccess.Should().BeTrue();
        var line = result.Value.LineItems.Should().ContainSingle().Which;
        line.VariantId.Should().Be(variant.Id);
        line.Quantity.Should().Be(2);
        line.Name.Should().Be("Classic Tee");
        line.UnitPrice.Should().Be(12.50m);
        result.Value.ShipmentTotal.Should().Be(12.50m);
        result.Value.ShippingMethodName.Should().Be("Express");
    }

    public void Dispose() => _dbContext.Dispose();
}
```

- [ ] **Step 2: Run test to verify it fails**

Run:
```bash
dotnet build service/Api/tests/Module.UnitTests/Module.UnitTests.csproj -v q --nologo
```
Expected: FAIL — `CS0117: 'CartLineItem' does not contain a definition for 'Name'` (and `'GetCartForCheckoutResponse' does not contain a definition for 'ShipmentTotal'`).

- [ ] **Step 3: Add the response fields**

In `GetCartForCheckout.Response.cs`, replace the two records with:

```csharp
public sealed record GetCartForCheckoutResponse
{
    public CheckoutState State { get; init; }
    public IReadOnlyList<CartLineItem> LineItems { get; init; } = [];
    public decimal Total { get; init; }
    public decimal ShipmentTotal { get; init; }
    public string? ShippingMethodName { get; init; }
    public string? Email { get; init; }
}

public sealed record CartLineItem
{
    public Guid VariantId { get; init; }
    public int Quantity { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
}
```

- [ ] **Step 4: Enrich the query and projection**

In `GetCartForCheckout.cs`, replace the query's `Include` line:

```csharp
        var cart = await dbContext.Set<Order>()
            .Include(x => x.LineItems)
            .FirstOrDefaultAsync(
```

with:

```csharp
        var cart = await dbContext.Set<Order>()
            .Include(x => x.LineItems)
                .ThenInclude(li => li.Variant)
                .ThenInclude(v => v.Product)
            .Include(x => x.ShippingMethod)
            .FirstOrDefaultAsync(
```

And replace the `LineItems` projection block inside the returned `GetCartForCheckoutResponse`:

```csharp
            LineItems = cart.LineItems
                .Select(li => new CartLineItem
                {
                    VariantId = li.VariantId,
                    Quantity = li.Quantity
                })
                .ToList(),
            Total = cart.Total,
            Email = cart.Email
```

with:

```csharp
            LineItems = cart.LineItems
                .Select(li => new CartLineItem
                {
                    VariantId = li.VariantId,
                    Quantity = li.Quantity,
                    Name = li.Variant.Product.Name,
                    UnitPrice = li.Price
                })
                .ToList(),
            Total = cart.Total,
            ShipmentTotal = cart.ShipmentTotal,
            ShippingMethodName = cart.ShippingMethod?.Name,
            Email = cart.Email
```

- [ ] **Step 5: Run test to verify it passes**

Run:
```bash
dotnet build service/Api/tests/Module.UnitTests/Module.UnitTests.csproj -v q --nologo
cd service/Api/tests/Module.UnitTests/bin/Debug/net10.0
./Module.UnitTests -class "Module.UnitTests.Ordering.Features.Storefront.GetCartForCheckout.GetCartForCheckoutTests"
```
Expected: PASS (`Total: 1, Failed: 0`).

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/GetCartForCheckout/ \
        service/Api/tests/Module.UnitTests/Ordering/Features/Storefront/GetCartForCheckout/GetCartForCheckoutTests.cs
git commit -m "feat(ordering): project line-item name, unit price, and shipping in GetCartForCheckout"
```

---

### Task 2: Widen `GatewayOptions` + populate in `CreatePaymentIntent`

**Files:**
- Modify: `service/Api/src/Module/Billing/Services/Provider/GatewayOptions.cs`
- Modify: `service/Api/src/Module/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs`
- Modify: `service/Api/tests/Module.UnitTests/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntentTests.cs`

**Interfaces:**
- Consumes: `GetCartForCheckoutResponse` (`LineItems` with `Name`/`UnitPrice`, `ShipmentTotal`, `ShippingMethodName` — Task 1); existing `CreatePaymentIntent.CommandHandler` and its test fixture (mock `IGatewayRegistry`/`IPaymentGatewayActionProvider` via `_gatewayMock`, mock `ISender` via `_senderMock`, `_reservationServiceMock`).
- Produces: `GatewayOptions` carrying `LineItems` (`IReadOnlyList<GatewayLineItem>`), `Shipping` (decimal), `ShippingDisplayName` (string?) — consumed by Task 3.

- [ ] **Step 1: Write the failing test**

Append to `CreatePaymentIntentTests.cs` (inside the class, before `private Order CreateOrder()`):

```csharp
    [Fact(DisplayName = "Handler: passes per-product line items and shipping to the gateway")]
    public async Task Handle_PassesLineItemsAndShippingToGateway()
    {
        GatewayOptions? captured = null;
        _gatewayMock
            .Setup(x => x.CreateCheckoutSessionAsync(It.IsAny<decimal>(), It.IsAny<GatewayOptions>(), It.IsAny<CancellationToken>()))
            .Callback<decimal, GatewayOptions, CancellationToken>((_, o, _) => captured = o)
            .ReturnsAsync(new PaymentGatewayResponse("stripe", authorization: "cs_test_1", checkoutUrl: "https://checkout.stripe.com/c/pay/cs_test_1"));

        var order = CreateOrder();
        var pm = new PaymentMethod { Name = "Credit Card", Code = "credit_card",
            ProviderKey = GatewayConstants.Providers.Stripe, Active = true };
        _dbContext.Set<PaymentMethod>().Add(pm);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var variantId = Guid.NewGuid();
        _senderMock.Setup(x => x.Send(
            It.Is<GetCartForCheckoutQuery>(q => q.CartId == order.Id),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<GetCartForCheckoutResponse>.Ok(new GetCartForCheckoutResponse
            {
                State = CheckoutState.PickDeliveryMethod,
                Total = 37.50m,
                ShipmentTotal = 12.50m,
                ShippingMethodName = "Express",
                Email = "test@example.com",
                LineItems = [ new() { VariantId = variantId, Quantity = 2, Name = "Classic Tee", UnitPrice = 12.50m } ]
            }));

        var result = await _handler.Handle(
            new CreatePaymentIntent.Command(new CreatePaymentIntent.Request { OrderId = order.Id, PaymentMethodId = pm.Id }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.Shipping.Should().Be(12.50m);
        captured.ShippingDisplayName.Should().Be("Express");
        captured.LineItems.Should().ContainSingle().Which.Should().BeEquivalentTo(
            new GatewayLineItem("Classic Tee", 2, 12.50m));
    }
```

- [ ] **Step 2: Run test to verify it fails**

Run:
```bash
dotnet build service/Api/tests/Module.UnitTests/Module.UnitTests.csproj -v q --nologo
```
Expected: FAIL — `CS0117: 'GatewayOptions' does not contain a definition for 'LineItems'` (and `'ShippingDisplayName'`).

- [ ] **Step 3: Add the contract fields**

In `GatewayOptions.cs`, add to the `GatewayOptions` record body:

```csharp
    public IReadOnlyList<GatewayLineItem> LineItems { get; init; } = [];
    public string? ShippingDisplayName { get; init; }
```

And append the new record at the end of the file (after the closing brace of `GatewayOptions`):

```csharp
public sealed record GatewayLineItem(string Name, int Quantity, decimal UnitPrice);
```

- [ ] **Step 4: Populate options in `CreatePaymentIntent.Handle`**

In `CreatePaymentIntent.cs`, in the `else` branch where `var options = new GatewayOptions { ... }` is built (after the `Currency = ...` line, before the closing `};`), add:

```csharp
                    Shipping = cart.ShipmentTotal,
                    ShippingDisplayName = cart.ShippingMethodName,
                    LineItems = cart.LineItems
                        .Select(li => new GatewayLineItem(li.Name, li.Quantity, li.UnitPrice))
                        .ToList(),
```

- [ ] **Step 5: Run tests to verify they pass**

Run:
```bash
dotnet build service/Api/tests/Module.UnitTests/Module.UnitTests.csproj -v q --nologo
cd service/Api/tests/Module.UnitTests/bin/Debug/net10.0
./Module.UnitTests -class "Module.UnitTests.Payment.Features.Storefront.Payment.CreateIntent.CreatePaymentIntentTests"
```
Expected: PASS (`Failed: 0`).

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Billing/Services/Provider/GatewayOptions.cs \
        service/Api/src/Module/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs \
        service/Api/tests/Module.UnitTests/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntentTests.cs
git commit -m "feat(billing): pass per-product line items and shipping to the gateway"
```

---

### Task 3: Per-product lines + shipping in `StripeGateway.BuildCheckoutSessionOptions`

**Files:**
- Modify: `service/Api/src/Module/Billing/Services/Provider/Stripe/StripeGateway.cs`
- Modify: `service/Api/tests/Module.UnitTests/Billing/Services/Provider/Stripe/StripeGatewayCheckoutSessionTests.cs`

**Interfaces:**
- Consumes: `GatewayOptions.LineItems` (`IReadOnlyList<GatewayLineItem>`), `GatewayOptions.Shipping`, `GatewayOptions.ShippingDisplayName` (Task 2); the existing `internal static SessionCreateOptions BuildCheckoutSessionOptions(decimal amount, GatewayOptions options)`.
- Produces: a `SessionCreateOptions` with per-product `LineItems` and an optional `ShippingOptions` entry (fixed-amount rate), or the aggregate fallback when `LineItems` is empty.

- [ ] **Step 1: Write the failing tests**

Append to `StripeGatewayCheckoutSessionTests.cs` (inside the class, before the closing `}`):

```csharp
    [Fact(DisplayName = "BuildCheckoutSessionOptions: builds one line per product")]
    public void BuildCheckoutSessionOptions_BuildsPerProductLineItems()
    {
        var options = BuildOptions() with
        {
            LineItems =
            [
                new GatewayLineItem("Classic Tee", 2, 12.50m),
                new GatewayLineItem("Jeans", 1, 50.00m)
            ]
        };

        var so = StripeGateway.BuildCheckoutSessionOptions(75m, options);

        so.LineItems.Should().NotBeNull();
        so.LineItems!.Should().HaveCount(2);
        so.LineItems[0].Quantity.Should().Be(2);
        so.LineItems[0].PriceData!.Currency.Should().Be("usd");
        so.LineItems[0].PriceData.UnitAmount.Should().Be(1250);
        so.LineItems[0].PriceData.ProductData!.Name.Should().Be("Classic Tee");
        so.LineItems[1].Quantity.Should().Be(1);
        so.LineItems[1].PriceData!.UnitAmount.Should().Be(5000);
        so.LineItems[1].PriceData.ProductData!.Name.Should().Be("Jeans");
    }

    [Fact(DisplayName = "BuildCheckoutSessionOptions: adds a shipping option when Shipping > 0")]
    public void BuildCheckoutSessionOptions_AddsShippingOption_WhenShippingPositive()
    {
        var options = BuildOptions() with
        {
            LineItems = [ new GatewayLineItem("Classic Tee", 1, 25.00m) ],
            Shipping = 12.50m,
            ShippingDisplayName = "Express"
        };

        var so = StripeGateway.BuildCheckoutSessionOptions(37.50m, options);

        so.ShippingOptions.Should().NotBeNull();
        var ship = so.ShippingOptions!.Single();
        ship.ShippingRateData.Should().NotBeNull();
        ship.ShippingRateData!.DisplayName.Should().Be("Express");
        ship.ShippingRateData.FixedAmount!.Amount.Should().Be(1250);
        ship.ShippingRateData.FixedAmount.Currency.Should().Be("usd");
    }

    [Fact(DisplayName = "BuildCheckoutSessionOptions: omits shipping option when Shipping is zero")]
    public void BuildCheckoutSessionOptions_NoShippingOption_WhenShippingZero()
    {
        var options = BuildOptions() with
        {
            LineItems = [ new GatewayLineItem("Classic Tee", 1, 25.00m) ],
            Shipping = 0m
        };

        var so = StripeGateway.BuildCheckoutSessionOptions(25m, options);

        so.ShippingOptions.Should().BeNull();
    }

    [Fact(DisplayName = "BuildCheckoutSessionOptions: falls back to aggregate line when no line items")]
    public void BuildCheckoutSessionOptions_FallsBackToAggregate_WhenNoLineItems()
    {
        var so = StripeGateway.BuildCheckoutSessionOptions(100m, BuildOptions());

        so.LineItems.Should().NotBeNull();
        var line = so.LineItems!.Single();
        line.Quantity.Should().Be(1);
        line.PriceData!.UnitAmount.Should().Be(10000);
        line.PriceData.ProductData!.Name.Should().Be("Order order-0001");
    }
```

- [ ] **Step 2: Run tests to verify they fail**

Run:
```bash
dotnet build service/Api/tests/Module.UnitTests/Module.UnitTests.csproj -v q --nologo
cd service/Api/tests/Module.UnitTests/bin/Debug/net10.0
./Module.UnitTests -class "Module.UnitTests.Payment.Services.Provider.Stripe.StripeGatewayCheckoutSessionTests"
```
Expected: the four new tests FAIL (the old aggregate behavior means `HaveCount(2)` fails, `ShippingOptions` is null, etc.). The two pre-existing tests (`_PreservesSessionMetadataAndLineItem`, `_PopulatesPaymentIntentMetadata`) still pass.

- [ ] **Step 3: Rewrite `BuildCheckoutSessionOptions`**

Replace the entire `internal static SessionCreateOptions BuildCheckoutSessionOptions(...)` method in `StripeGateway.cs` with:

```csharp
    internal static SessionCreateOptions BuildCheckoutSessionOptions(decimal amount, GatewayOptions options)
    {
        var currency = options.Currency.ToLowerInvariant();

        var lineItems = options.LineItems.Count > 0
            ? options.LineItems
                .Select(li => new SessionLineItemOptions
                {
                    Quantity = li.Quantity,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = currency,
                        UnitAmount = checked((long)Math.Round(
                            li.UnitPrice * GatewayConstants.Amounts.CentsMultiplier, MidpointRounding.AwayFromZero)),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = li.Name
                        }
                    }
                })
                .ToList()
            :
            [
                new SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = currency,
                        UnitAmount = checked((long)Math.Round(
                            amount * GatewayConstants.Amounts.CentsMultiplier, MidpointRounding.AwayFromZero)),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"Order {options.OrderId}"
                        }
                    }
                }
            ];

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
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    [GatewayConstants.Metadata.OrderIdKey] = options.OrderId,
                    [GatewayConstants.Metadata.PaymentIdKey] = options.PaymentId
                }
            },
            LineItems = lineItems,
            ShippingOptions = options.Shipping > 0
                ?
                [
                    new SessionShippingOptionOptions
                    {
                        ShippingRateData = new SessionShippingOptionShippingRateDataOptions
                        {
                            Type = "fixed_amount",
                            FixedAmount = new SessionShippingOptionShippingRateDataFixedAmountOptions
                            {
                                Amount = checked((long)Math.Round(
                                    options.Shipping * GatewayConstants.Amounts.CentsMultiplier, MidpointRounding.AwayFromZero)),
                                Currency = currency
                            },
                            DisplayName = options.ShippingDisplayName ?? "Shipping"
                        }
                    }
                ]
                : null
        };
    }
```

- [ ] **Step 4: Run tests to verify they pass**

Run:
```bash
dotnet build service/Api/tests/Module.UnitTests/Module.UnitTests.csproj -v q --nologo
cd service/Api/tests/Module.UnitTests/bin/Debug/net10.0
./Module.UnitTests -class "Module.UnitTests.Payment.Services.Provider.Stripe.StripeGatewayCheckoutSessionTests"
```
Expected: PASS — all six tests (`Failed: 0`): 2 pre-existing + 4 new.

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Billing/Services/Provider/Stripe/StripeGateway.cs \
        service/Api/tests/Module.UnitTests/Billing/Services/Provider/Stripe/StripeGatewayCheckoutSessionTests.cs
git commit -m "feat(billing): emit per-product lines and shipping in Stripe checkout session"
```

---

### Verification (after all tasks)

```bash
dotnet build service/Api/src/Api/Api.csproj -v q --nologo
cd service/Api/tests/Module.UnitTests/bin/Debug/net10.0
./Module.UnitTests -class "Module.UnitTests.Ordering.Features.Storefront.GetCartForCheckout.GetCartForCheckoutTests"
./Module.UnitTests -class "Module.UnitTests.Payment.Features.Storefront.Payment.CreateIntent.CreatePaymentIntentTests"
./Module.UnitTests -class "Module.UnitTests.Payment.Services.Provider.Stripe.StripeGatewayCheckoutSessionTests"
```

Expected: build 0 warnings / 0 errors; each class `Failed: 0`.

## Self-Review

- **Spec coverage:** §1 (GetCartForCheckout projection) → Task 1; §2 (GatewayOptions contract) + §3 (CreatePaymentIntent populate) → Task 2; §4 (StripeGateway per-product + shipping + fallback) → Task 3. §4 fallback → Task 3 `_FallsBackToAggregate`. All covered.
- **Placeholder scan:** no TBD/TODO; full code in every step.
- **Type consistency:** `GatewayLineItem(string Name, int Quantity, decimal UnitPrice)` matches across Tasks 2-3; `CartLineItem { VariantId, Quantity, Name, UnitPrice }` matches Task 1-2; `GetCartForCheckoutResponse.ShipmentTotal`/`ShippingMethodName` match Task 1-2; Stripe.net type names verified present in the DLL.
- **Test-nullability:** tests use `Should().NotBeNull()` + `!` and `.Single()` to avoid nullable-reference warnings under warnings-as-errors.
- **Regression safety:** the empty-line-items fallback keeps the two pre-existing `StripeGatewayCheckoutSessionTests` green (they call `BuildOptions()` with no line items) and preserves behavior for any caller not supplying line items.

## Execution Handoff

Plan complete and saved to `docs/superpowers/plans/2026-08-16-stripe-checkout-line-items-shipping.md`. Two execution options:

1. **Subagent-Driven (recommended)** — dispatch a fresh subagent per task, review between tasks.
2. **Inline Execution** — execute tasks in this session with checkpoints.

Which approach?
