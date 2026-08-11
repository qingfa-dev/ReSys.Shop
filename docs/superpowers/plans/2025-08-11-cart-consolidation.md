# Cart Consolidation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move all cart routes from `api/storefront/ordering/cart` to `api/storefront/cart`, consolidate payment routes under cart, correct HTTP methods, merge EmptyCart into DELETE /items, audit ValidateCheckout for side effects.

**Architecture:** Cart becomes a top-level resource at `/storefront/cart`. Payment intent creation/status/confirmation become sub-resources `/cart/payment/intent`. All route constant changes are atomic — no gradual migration. Stripe webhook stays standalone. Cart feature files remain under `Ordering/Features/Storefront/Cart/` — the Ordering module owns the Cart domain entity.

**Tech Stack:** .NET 10, C#, Carter minimal APIs, FluentValidation

## Global Constraints

- .NET 10, TreatWarningsAsErrors=true
- All route constants in `{Module}Feature.Storefront.cs` files
- HTTP methods: PATCH for partial updates, POST for actions, GET for reads, DELETE for removal
- No action verbs in URLs (`empty`, `validate`, `create-intent` removed)
- Feature files follow vertical slice convention
- Atomic migration: all route constants change in one commit
- `dotnet build` must pass after each task

---

### Task 1: Audit ValidateCheckout Handler for Side Effects

**Files:**
- Read: `service/Api/src/Module/Ordering/Features/Storefront/Cart/ValidateCheckout/ValidateCheckout.cs`

**Produces:** Decision: change to GET or keep as POST with rename

- [ ] **Step 1: Read the handler**

```bash
cat service/Api/src/Module/Ordering/Features/Storefront/Cart/ValidateCheckout/ValidateCheckout.cs
```

- [ ] **Step 2: Check for state mutation**

Search for patterns indicating state change:
- `SaveChangesAsync` — if present, handler mutates state
- `AdvanceCheckoutState` — if called, state changes
- Any `Add`, `Update`, `Delete` on DbContext sets

Expected result: This handler should only READ the cart state and return validation requirements. It's a pre-check before checkout — if it mutates state, that's a bug.

- [ ] **Step 3: Record decision**

If handler is side-effect-free: proceed with `GET /cart/checkout`.
If handler mutates state: keep as `POST` but rename route to `POST /cart/checkout/validate`.

Assume side-effect-free for this plan. If audit reveals otherwise, adjust endpoint method in Task 4.

- [ ] **Step 4: Commit**

```bash
git commit --allow-empty -m "audit(ordering): confirm ValidateCheckout handler is side-effect-free"
```

### Task 2: Update OrderingFeature.Storefront.cs — Cart + Orders Routes

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Shared/OrderingFeature.Storefront.cs`

- [ ] **Step 1: Replace all route constants**

Replace the entire file content with:

```csharp
namespace Module.Ordering.Features.Shared;

public static partial class OrderingFeature
{
    public static class Storefront
    {
        public static class Cart
        {
            public static class Create
            {
                public const string Route = "api/storefront/cart";
                public const string Description = "Create a new shopping cart";
                public const string Summary = "Create cart";
            }

            public static class Get
            {
                public const string Route = "api/storefront/cart";
                public const string Description = "Retrieve the current user's shopping cart";
                public const string Summary = "Get cart";
            }

            public static class Update
            {
                public const string Route = "api/storefront/cart";
                public const string Description = "Update cart checkout details (email, addresses, special instructions)";
                public const string Summary = "Update checkout";
            }

            public static class Delete
            {
                public const string Route = "api/storefront/cart";
                public const string Description = "Delete the shopping cart";
                public const string Summary = "Delete cart";
            }

            public static class Associate
            {
                public const string Route = "api/storefront/cart/associate";
                public const string Description = "Associate a guest cart with the current user";
                public const string Summary = "Associate cart";
            }

            public static class AddItem
            {
                public const string Route = "api/storefront/cart/items";
                public const string Description = "Add an item to the shopping cart";
                public const string Summary = "Add to cart";
            }

            public static class UpdateItemQuantity
            {
                public const string Route = "api/storefront/cart/items/{lineItemId:guid}";
                public const string Description = "Update the quantity of a cart line item";
                public const string Summary = "Update cart item quantity";
            }

            public static class RemoveItem
            {
                public const string Route = "api/storefront/cart/items/{lineItemId:guid}";
                public const string Description = "Remove a line item from the cart";
                public const string Summary = "Remove cart item";
            }

            public static class RemoveAllItems
            {
                public const string Route = "api/storefront/cart/items";
                public const string Description = "Remove all items from the cart";
                public const string Summary = "Empty cart";
            }

            public static class ValidateCheckout
            {
                public const string Route = "api/storefront/cart/checkout";
                public const string Description = "Validate the current checkout state";
                public const string Summary = "Validate checkout";
            }

            public static class Checkout
            {
                public const string Route = "api/storefront/cart/checkout";
                public const string Description = "Create an order from the current cart";
                public const string Summary = "Checkout";
            }

            public static class SelectShippingRate
            {
                public const string Route = "api/storefront/cart/shipping-rate";
                public const string Description = "Select a shipping rate for the order";
                public const string Summary = "Select shipping rate";
            }

            public static class Payment
            {
                public static class CreateIntent
                {
                    public const string Route = "api/storefront/cart/payment/intent";
                    public const string Description = "Create a payment intent for the current cart";
                    public const string Summary = "Create payment intent";
                }

                public static class GetIntent
                {
                    public const string Route = "api/storefront/cart/payment/intent";
                    public const string Description = "Get active payment session for the current cart";
                    public const string Summary = "Get payment session";
                }

                public static class Confirm
                {
                    public const string Route = "api/storefront/cart/payment/intent/{paymentId:guid}/confirm";
                    public const string Description = "Confirm a payment for the current cart";
                    public const string Summary = "Confirm payment";
                }
            }
        }

        public static class Orders
        {
            public static class List
            {
                public const string Route = "api/storefront/orders";
                public const string Description = "List current user's orders";
                public const string Summary = "List orders";
            }

            public static class GetById
            {
                public const string Route = "api/storefront/orders/{id:guid}";
                public const string Description = "Retrieve an order by identifier";
                public const string Summary = "Get order";
            }

            public static class GetTracking
            {
                public const string Route = "api/storefront/orders/{id:guid}/tracking";
                public const string Description = "Retrieve order tracking timeline";
                public const string Summary = "Get order tracking";
            }

            public static class Cancel
            {
                public const string Route = "api/storefront/orders/{id:guid}/cancel";
                public const string Description = "Cancel an order";
                public const string Summary = "Cancel order";
            }
        }
    }
}
```

- [ ] **Step 2: Build — expect compile errors from endpoint files referencing old constant names**

```bash
dotnet build 2>&1 | head -30
```

Expected errors include:
- `Cart.Empty.Route` not found → `Cart.RemoveAllItems.Route`
- `Cart.Validate.Route` not found → `Cart.ValidateCheckout.Route`
- `Orders.Cancel.Route` still exists but prefix changed

These are fixed in Tasks 3-6.

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Shared/OrderingFeature.Storefront.cs
git commit -m "refactor(ordering): rewrite storefront routes for cart + orders

Cart routes: ordering/cart → /cart (top-level resource)
Orders routes: ordering/orders → /orders
Added Cart.Payment sub-resource (CreateIntent, GetIntent, Confirm)
Added Cart.RemoveAllItems (was Cart.Empty)
Added Cart.ValidateCheckout (was Cart.Validate)
Build broken pending endpoint file updates."
```

### Task 3: Fix Cart Endpoint Files — Route References + HTTP Methods

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/CreateCart/CreateCart.Endpoint.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Get/GetCart.Endpoint.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateCheckout/UpdateCheckout.Endpoint.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/DeleteCart/DeleteCart.Endpoint.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/AssociateCart/AssociateCartWithUser.Endpoint.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/AddItem/AddToCart.Endpoint.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/RemoveItem/RemoveCartItem.Endpoint.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.Endpoint.cs`

- [ ] **Step 1: Search-and-replace route constants in all endpoint files**

For each endpoint file, find the `.MapGet`, `.MapPost`, `.MapPut`, `.MapDelete` line and update the route reference:

```
OrderingFeature.Storefront.Cart.Create.Route          → OrderingFeature.Storefront.Cart.Create.Route       (unchanged name, but value changed)
OrderingFeature.Storefront.Cart.Get.Route             → OrderingFeature.Storefront.Cart.Get.Route
OrderingFeature.Storefront.Cart.Update.Route          → OrderingFeature.Storefront.Cart.Update.Route
OrderingFeature.Storefront.Cart.Delete.Route          → OrderingFeature.Storefront.Cart.Delete.Route
OrderingFeature.Storefront.Cart.Associate.Route       → OrderingFeature.Storefront.Cart.Associate.Route
OrderingFeature.Storefront.Cart.AddItem.Route         → OrderingFeature.Storefront.Cart.AddItem.Route
OrderingFeature.Storefront.Cart.RemoveItem.Route      → OrderingFeature.Storefront.Cart.RemoveItem.Route
OrderingFeature.Storefront.Cart.Checkout.Route        → OrderingFeature.Storefront.Cart.Checkout.Route
```

Note: The class names are unchanged — only the `public const string Route = "..."` values changed. The build errors are because the constants disappeared/reappeared with different values. Endpoint files reference the same class names, so they should resolve.

Wait — the issue is that `Empty` class was renamed to `RemoveAllItems` and `Validate` was renamed to `ValidateCheckout`. Endpoints referencing `Cart.Empty.Route` or `Cart.Validate.Route` WILL fail. Let me check which files reference these.

Actually: the constant names changed. The endpoint files reference `OrderingFeature.Storefront.Cart.Something.Route`. If the class name changed (Empty → RemoveAllItems, Validate → ValidateCheckout), then endpoint files must be updated.

- [ ] **Step 1a: Find files referencing old constant names**

```bash
rg "Cart\.Empty\." service/Api/src/Module/Ordering/Features/Storefront/Cart/
rg "Cart\.Validate\." service/Api/src/Module/Ordering/Features/Storefront/Cart/
rg "Cart\.Update\." service/Api/src/Module/Ordering/Features/Storefront/Cart/
```

Note: `Cart.Update` still exists as a class name (unchanged). The `Empty` → `RemoveAllItems` and `Validate` → `ValidateCheckout` renames are the only breaking changes.

- [ ] **Step 1b: Fix EmptyCart.Endpoint.cs → reference RemoveAllItems.Route**

Edit `EmptyCart/EmptyCart.Endpoint.cs`:
Change `OrderingFeature.Storefront.Cart.Empty.Route` → `OrderingFeature.Storefront.Cart.RemoveAllItems.Route`
Change `.MapPost` → `.MapDelete`

- [ ] **Step 1c: Fix ValidateCheckout.Endpoint.cs → reference ValidateCheckout.Route**

Edit `ValidateCheckout/ValidateCheckout.Endpoint.cs`:
Change `OrderingFeature.Storefront.Cart.Validate.Route` → `OrderingFeature.Storefront.Cart.ValidateCheckout.Route`
Change `.MapPost` → `.MapGet` (if audit passed)

- [ ] **Step 1d: Fix UpdateCheckout.Endpoint.cs — PUT → PATCH**

Edit `UpdateCheckout/UpdateCheckout.Endpoint.cs`:
Change `.MapPut` → `.MapPatch`

- [ ] **Step 1e: Fix UpdateCartItemQuantity.Endpoint.cs — PUT → PATCH**

Edit `UpdateItemQuantity/UpdateCartItemQuantity.Endpoint.cs`:
Change `.MapPut` → `.MapPatch`

- [ ] **Step 1f: Fix SelectShippingRate.Endpoint.cs — POST → PATCH**

Edit `SelectShippingRate/SelectShippingRate.Endpoint.cs`:
Change `.MapPost` → `.MapPatch`

- [ ] **Step 2: Build**

```bash
dotnet build
```

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/Cart/
git commit -m "refactor(ordering): update cart endpoints — routes + HTTP methods

Cart endpoints now use /storefront/cart prefix.
PUT → PATCH for UpdateCheckout, UpdateCartItemQuantity.
POST → PATCH for SelectShippingRate.
POST /empty → DELETE /items (RemoveAllItems).
POST /validate → GET /checkout (ValidateCheckout — if side-effect-free)."
```

### Task 4: Update Orders Endpoint Files — Route Prefix

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Orders/Cancel/CancelOrder.Endpoint.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Orders/Get/ById/GetCustomerOrder.Endpoint.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Orders/GetTracking/GetOrderTracking.Endpoint.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Orders/ListOrders/ListCustomerOrders.Endpoint.cs`

- [ ] **Step 1: Update all orders endpoint files**

For each endpoint file, the route constant reference changes:
- `OrderingFeature.Storefront.Orders.GetById.Route` — class name unchanged, value changed (prefix `ordering/orders` → `orders`)
- `OrderingFeature.Storefront.Orders.Cancel.Route` — same
- `OrderingFeature.Storefront.Orders.List.Route` — same
- `OrderingFeature.Storefront.Orders.GetTracking.Route` — same

Also: CancelOrder endpoint: change `.MapPut` → `.MapPost`.

```bash
# Verify class names exist in new route file
rg "public static class (GetById|Cancel|List|GetTracking)" service/Api/src/Module/Ordering/Features/Shared/OrderingFeature.Storefront.cs
```

- [ ] **Step 2: Build**

```bash
dotnet build
```

- [ ] **Step 3: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/Orders/
git commit -m "refactor(ordering): update orders endpoints — prefix + cancel method

Orders route prefix: ordering/orders → orders.
Cancel order: PUT → POST."
```

### Task 5: Update BillingFeature.Storefront.cs — Remove Moved Routes

**Files:**
- Modify: `service/Api/src/Module/Billing/Features/Shared/BillingFeature.Storefront.cs`
- Modify: `service/Api/src/Module/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.Endpoint.cs`
- Modify: `service/Api/src/Module/Billing/Features/Storefront/Payment/Status/GetPaymentStatus.Endpoint.cs`
- Modify: `service/Api/src/Module/Billing/Features/Storefront/Payment/Confirm/ConfirmPayment.Endpoint.cs`

- [ ] **Step 1: Remove Payments.CreateIntent, Confirm, Status from BillingFeature.Storefront.cs**

Remove lines 8-29 (the entire `Payments` nested class with CreateIntent, Confirm, Status).

Keep: `PaymentMethods` (GetAll, SetupIntent) and `Webhooks.Stripe`.

- [ ] **Step 2: Move payment endpoint files to Cart/Payment/**

Create directories:
```bash
mkdir -p service/Api/src/Module/Ordering/Features/Storefront/Cart/Payment/CreateIntent
mkdir -p service/Api/src/Module/Ordering/Features/Storefront/Cart/Payment/GetIntent
mkdir -p service/Api/src/Module/Ordering/Features/Storefront/Cart/Payment/Confirm
```

Copy and update the 3 endpoint files from Billing:
```bash
cp service/Api/src/Module/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.Endpoint.cs \
   service/Api/src/Module/Ordering/Features/Storefront/Cart/Payment/CreateIntent/CreatePaymentIntent.Endpoint.cs
cp service/Api/src/Module/Billing/Features/Storefront/Payment/Status/GetPaymentStatus.Endpoint.cs \
   service/Api/src/Module/Ordering/Features/Storefront/Cart/Payment/GetIntent/GetPaymentIntent.Endpoint.cs
cp service/Api/src/Module/Billing/Features/Storefront/Payment/Confirm/ConfirmPayment.Endpoint.cs \
   service/Api/src/Module/Ordering/Features/Storefront/Cart/Payment/Confirm/ConfirmPayment.Endpoint.cs
```

- [ ] **Step 3: Update the moved endpoint files**

Update route references and namespaces:

`CreatePaymentIntent.Endpoint.cs`:
- namespace → `Module.Ordering.Features.Storefront.Cart.Payment.CreateIntent`
- route → `OrderingFeature.Storefront.Cart.Payment.CreateIntent.Route`
- tag → `OrderingFeature.Tags.Cart` (or keep "Payment")

`GetPaymentIntent.Endpoint.cs`:
- namespace → `Module.Ordering.Features.Storefront.Cart.Payment.GetIntent`
- route → `OrderingFeature.Storefront.Cart.Payment.GetIntent.Route`

`ConfirmPayment.Endpoint.cs`:
- namespace → `Module.Ordering.Features.Storefront.Cart.Payment.Confirm`
- route → `OrderingFeature.Storefront.Cart.Payment.Confirm.Route`

- [ ] **Step 4: Update handlers to read cart from HttpContext.Items**

In each payment handler (the `.cs` file, not just `.Endpoint.cs`):
- Inject `IHttpContextAccessor`
- Replace `command.OrderId` lookup with cart resolved from `HttpContext.Items["CartToken"]` or current user
- The `CreatePaymentIntent.cs` handler (already modified in Task 10 of inventory plan) now receives the cart from context instead of from `command.OrderId` route param

- [ ] **Step 5: Build**

```bash
dotnet build
```

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Billing/Features/Shared/BillingFeature.Storefront.cs
git add service/Api/src/Module/Ordering/Features/Storefront/Cart/Payment/
git commit -m "refactor: move payment endpoints under Cart, update Billing storefront routes

CreateIntent, Confirm, Status routes removed from BillingFeature.Storefront.
Payment endpoints moved to Ordering/Features/Storefront/Cart/Payment/.
Cart identified via HttpContext.Items[CartToken] from CartTokenMiddleware."
```

### Task 6: Full Build + Cross-Module Ref Check

- [ ] **Step 1: Full build**

```bash
dotnet build
```

- [ ] **Step 2: Cross-module check**

```bash
bash scripts/check-cross-module-refs.sh
```

- [ ] **Step 3: Unit tests**

```bash
dotnet test service/Api/tests/Module.UnitTests
dotnet test service/Api/tests/Shared.UnitTests
```

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "chore: verify full build and tests pass after cart consolidation"
```
