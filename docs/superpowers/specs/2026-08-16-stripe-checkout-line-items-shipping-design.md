# Design: Per-product Line Items + Shipping in the Stripe Checkout Session

- Date: 2026-08-16
- Status: Approved
- Scope: Billing module (Stripe gateway) + Ordering `GetCartForCheckout` projection

## Problem

`StripeGateway.BuildCheckoutSessionOptions` currently sends a single aggregate
line item to the Stripe Checkout Session: `Name = "Order <orderId>"`,
`Quantity = 1`, `UnitAmount = cart.Total`. The customer sees one generic line
instead of their actual products, and there is no shipping charge displayed.

## Decision

Send one Checkout Session line item per cart line item (product name,
quantity, unit price) plus a separate fixed-amount shipping charge. Stripe
only — the Bogus/dev gateway and other callers are unaffected.

## Changes

### 1. Ordering — `GetCartForCheckout` projection

- `service/Api/src/Module/Ordering/Features/Storefront/GetCartForCheckout/GetCartForCheckout.cs`
  - Load the product graph: add `.ThenInclude(li => li.Variant).ThenInclude(v => v.Product)`
    to the existing `Include(x => x.LineItems)`, and add `Include(x => x.ShippingMethod)`.
  - Project `Name = li.Variant.Product.Name`, `UnitPrice = li.Price`, and the
    new response fields `ShipmentTotal`/`ShippingMethodName` from the order.
- `service/Api/src/Module/Ordering/Features/Storefront/GetCartForCheckout/GetCartForCheckout.Response.cs`
  - `CartLineItem`: add `string Name` and `decimal UnitPrice`.
  - `GetCartForCheckoutResponse`: add `decimal ShipmentTotal` and
    `string? ShippingMethodName`.

### 2. Billing — gateway contract

- `service/Api/src/Module/Billing/Services/Provider/GatewayOptions.cs`
  - Add `IReadOnlyList<GatewayLineItem> LineItems { get; init; } = [];`
  - Add `string? ShippingDisplayName { get; init; }`
  - Add `public sealed record GatewayLineItem(string Name, int Quantity, decimal UnitPrice);`
  - Existing `Shipping` decimal carries the shipping cost; `Tax`/`Subtotal`/
    `Discount` remain as-is (unused in the session build).

### 3. Billing — `CreatePaymentIntent.Handle`

`service/Api/src/Module/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs:140-154`
- Set `options.Shipping = cart.ShipmentTotal`
- Set `options.ShippingDisplayName = cart.ShippingMethodName`
- Set `options.LineItems = cart.LineItems.Select(li => new GatewayLineItem(li.Name, li.Quantity, li.UnitPrice)).ToList()`

### 4. Billing — `StripeGateway.BuildCheckoutSessionOptions`

`service/Api/src/Module/Billing/Services/Provider/Stripe/StripeGateway.cs`
- Replace the single aggregate `LineItems` block with per-product lines:

```csharp
LineItems = options.LineItems
    .Select(li => new SessionLineItemOptions
    {
        Quantity = li.Quantity,
        PriceData = new SessionLineItemPriceDataOptions
        {
            Currency = options.Currency.ToLowerInvariant(),
            UnitAmount = checked((long)Math.Round(
                li.UnitPrice * GatewayConstants.Amounts.CentsMultiplier, MidpointRounding.AwayFromZero)),
            ProductData = new SessionLineItemPriceDataProductDataOptions
            {
                Name = li.Name
            }
        }
    })
    .ToList(),
```

- Add `ShippingOptions` when `options.Shipping > 0` (Stripe.net 52.3.0 types
  verified present):

```csharp
ShippingOptions = options.Shipping > 0
    ? [
        new SessionShippingOptionOptions
        {
            ShippingRateData = new SessionShippingOptionShippingRateDataOptions
            {
                Type = "fixed_amount",
                FixedAmount = new SessionShippingOptionShippingRateDataFixedAmountOptions
                {
                    Amount = checked((long)Math.Round(
                        options.Shipping * GatewayConstants.Amounts.CentsMultiplier, MidpointRounding.AwayFromZero)),
                    Currency = options.Currency.ToLowerInvariant()
                },
                DisplayName = options.ShippingDisplayName ?? "Shipping"
            }
        }
      ]
    : null,
```

- Fallback: when `options.LineItems` is empty, emit today's aggregate line
  (`Name = $"Order {options.OrderId}"`, `Quantity = 1`, `UnitAmount = amount`)
  so behavior is unchanged for any caller that does not supply line items.

## Data Flow (after)

1. `GetCartForCheckout` projects `CartLineItem { VariantId, Quantity, Name, UnitPrice }`
   plus `ShipmentTotal`/`ShippingMethodName`.
2. `CreatePaymentIntent.Handle` maps them into `GatewayOptions.LineItems` and
   sets `Shipping`/`ShippingDisplayName`.
3. Stripe receives one line per product plus a fixed-amount shipping rate.

## Total Exactness & Edge Cases

- `Order.Method.Computation.cs` verifies nothing sets `LineItem.AdjustmentTotal`
  today, so `LineItem.Total = Quantity × Price`, `ItemTotal = Σ(Price × Quantity)`,
  and `Total = ItemTotal + ShipmentTotal`. Therefore per-product lines sum to
  `ItemTotal`, the shipping rate equals `ShipmentTotal`, and Stripe's computed
  total equals `Order.Total` exactly. No balancing line is needed.
- Nonzero adjustments are out of scope (never produced by current code).
- `Shipping = 0` → no `ShippingOptions`; product lines only.
- `options.Currency` is the single session currency; `GatewayLineItem` carries
  no currency (line items inherit the session currency).
- Unit prices are non-negative (domain invariant), so cents conversion cannot
  go negative.

## Out of Scope

- Sending `tax`, per-line discounts, images, or SKU to Stripe.
- Nonzero `AdjustmentTotal` handling (balancing/misc line).
- Enriching the Bogus/dev gateway or shared behavior beyond Stripe.
- The `order_id` metadata value semantics.
