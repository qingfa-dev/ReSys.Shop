# Ordering Flow Fixes Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix 8 quality gaps in the Ordering backend and Store SPA checkout flow — runtime bugs, design gaps, and dead code.

**Architecture:** Backend handlers for updateItem/removeItem return full `CartDetailResponse` (matching addItem pattern). Order detail response gains line items. Frontend checkout composable loses step control; view handles all transitions including a cart re-fetch + validation call during the Review step. Cart expiry is reduced to Hangfire only.

**Tech Stack:** C# 10 (.NET 10), Carter minimal APIs, EF Core, MediatR, Vue 3, TypeScript, Stripe Elements

## Global Constraints

- Warnings-as-errors (`TreatWarningsAsErrors=true`) — every change must build clean
- Result objects, not exceptions — domain operations return `Result<T>` or `Result`
- Vertical slice feature files — `static partial class` split across Handler/Request/Response/Endpoint/Validator
- No new cross-module namespace references
- All existing unit and integration tests must pass

---

### Task 1: Add Response DTO for UpdateCartItemQuantity

**Files:**
- Create: `service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.Response.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.Endpoint.cs`

**Interfaces:**
- Consumes: `CartDetailResponse` base type, `CartMapping.MapToDetailWithItems<T>()`, `CartItemLookup`
- Produces: `UpdateCartItemQuantity.Response : CartDetailResponse` — full cart after quantity change

- [ ] **Step 1: Create Response record**

Write `service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateItemQuantity/UpdateCartItemQuantity.Response.cs`:

```csharp
using Module.Ordering.Features.Storefront.Cart.Shared.Models;

namespace Module.Ordering.Features.Storefront.Cart.UpdateItemQuantity;

public static partial class UpdateCartItemQuantity
{
    public sealed record Response : CartDetailResponse;
}
```

- [ ] **Step 2: Update handler to return Result<Response> with full cart**

Modify `UpdateCartItemQuantity.cs`:

Change the command type from `ICommand` to `ICommand<Response>`:
```csharp
public sealed record Command(Guid LineItemId, Request Request) : ICommand<Response>;
```

Change handler interface from `ICommandHandler<Command>` to `ICommandHandler<Command, Response>`:
```csharp
public sealed class CommandHandler(
    IApplicationDbContext dbContext,
    ILogger<CommandHandler> logger,
    ICurrentUser currentUser,
    IStockItemService stockItem)
    : ICommandHandler<Command, Response>
```

Change return type from `Task<Result>` to `Task<Result<Response>>`:
```csharp
public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
```

Add imports:
```csharp
using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Variants;
using Module.Ordering.Features.Storefront.Cart.Shared.Mappings;
```

Replace the final return block (lines 61-67):
```csharp
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record quantity change in audit log.
            LineItemLoggers.QuantityUpdated(logger, Id: lineItem.Id, OrderId: cart.Id, Quantity: lineItem.Quantity, ActionBy: currentUser.UserName);

            var variantIds = cart.LineItems.Select(li => li.VariantId).ToList();
            var itemLookup = await BuildCartItemLookupAsync(dbContext, variantIds, cancellationToken);
            return Result<Response>.Ok(cart.MapToDetailWithItems<Response>(itemLookup));
```

Add the private helper method `BuildCartItemLookupAsync` at the end of the `CommandHandler` class (before the closing brace of the class):
```csharp
        private static async Task<Dictionary<Guid, CartItemLookup>> BuildCartItemLookupAsync(
            IApplicationDbContext dbContext,
            IReadOnlyCollection<Guid> variantIds,
            CancellationToken cancellationToken)
        {
            if (variantIds.Count == 0)
                return new Dictionary<Guid, CartItemLookup>();

            var variants = await dbContext.Set<Variant>()
                .Where(v => variantIds.Contains(v.Id))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var productIds = variants.Select(v => v.ProductId).Distinct().ToList();
            var products = await dbContext.Set<Product>()
                .Where(p => productIds.Contains(p.Id))
                .Include(p => p.Variants)
                    .ThenInclude(v => v.VariantImages)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var productsById = products.ToDictionary(p => p.Id);

            return variants.ToDictionary(v => v.Id, v =>
            {
                if (!productsById.TryGetValue(v.ProductId, out var product))
                    return new CartItemLookup { Sku = v.Sku ?? string.Empty };

                var masterVariant = product.Variants.FirstOrDefault(x => x.IsMaster);
                var primaryImageUrl = (masterVariant?.VariantImages.OrderBy(i => i.Position).FirstOrDefault()
                    ?? product.Variants.SelectMany(x => x.VariantImages).OrderBy(i => i.Position).FirstOrDefault())
                    ?.Url;

                return new CartItemLookup
                {
                    Sku = v.Sku ?? string.Empty,
                    ProductName = product.Name,
                    ProductImageUrl = primaryImageUrl,
                };
            });
        }
```

- [ ] **Step 3: Update endpoint Produces signature**

Modify `UpdateCartItemQuantity.Endpoint.cs` — change:
```csharp
            .Produces<Result>()
```
to:
```csharp
            .Produces<Result<Response>>()
```

- [ ] **Step 4: Build and verify**

Run: `dotnet build service/Api/src/Module/`
Expected: PASS, no warnings

---

### Task 2: Add Response DTO for RemoveCartItem

**Files:**
- Create: `service/Api/src/Module/Ordering/Features/Storefront/Cart/RemoveItem/RemoveCartItem.Response.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/RemoveItem/RemoveCartItem.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/RemoveItem/RemoveCartItem.Endpoint.cs`

**Interfaces:**
- Consumes: `CartDetailResponse`, `CartMapping.MapToDetailWithItems<T>()`, `CartItemLookup`
- Produces: `RemoveCartItem.Response : CartDetailResponse` — full cart after item removal

- [ ] **Step 1: Create Response record**

Write `service/Api/src/Module/Ordering/Features/Storefront/Cart/RemoveItem/RemoveCartItem.Response.cs`:

```csharp
using Module.Ordering.Features.Storefront.Cart.Shared.Models;

namespace Module.Ordering.Features.Storefront.Cart.RemoveItem;

public static partial class RemoveCartItem
{
    public sealed record Response : CartDetailResponse;
}
```

- [ ] **Step 2: Update handler to return Result<Response> with full cart**

Modify `RemoveCartItem.cs`:

Change the command type:
```csharp
public sealed record Command(Guid LineItemId) : ICommand<Response>;
```

Change handler interface:
```csharp
public sealed class CommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser)
    : ICommandHandler<Command, Response>
```

Change return type:
```csharp
public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
```

Add imports:
```csharp
using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Variants;
using Module.Ordering.Features.Storefront.Cart.Shared.Mappings;
```

Replace the final return block (lines 50-53):
```csharp
            dbContext.Set<LineItem>().Remove(removeResult.Value);
            await dbContext.SaveChangesAsync(cancellationToken);

            var variantIds = cart.LineItems.Select(li => li.VariantId).ToList();
            var itemLookup = await BuildCartItemLookupAsync(dbContext, variantIds, cancellationToken);
            return Result<Response>.Ok(cart.MapToDetailWithItems<Response>(itemLookup));
```

Add the same `BuildCartItemLookupAsync` private helper method inside `CommandHandler`:

```csharp
        private static async Task<Dictionary<Guid, CartItemLookup>> BuildCartItemLookupAsync(
            IApplicationDbContext dbContext,
            IReadOnlyCollection<Guid> variantIds,
            CancellationToken cancellationToken)
        {
            if (variantIds.Count == 0)
                return new Dictionary<Guid, CartItemLookup>();

            var variants = await dbContext.Set<Variant>()
                .Where(v => variantIds.Contains(v.Id))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var productIds = variants.Select(v => v.ProductId).Distinct().ToList();
            var products = await dbContext.Set<Product>()
                .Where(p => productIds.Contains(p.Id))
                .Include(p => p.Variants)
                    .ThenInclude(v => v.VariantImages)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var productsById = products.ToDictionary(p => p.Id);

            return variants.ToDictionary(v => v.Id, v =>
            {
                if (!productsById.TryGetValue(v.ProductId, out var product))
                    return new CartItemLookup { Sku = v.Sku ?? string.Empty };

                var masterVariant = product.Variants.FirstOrDefault(x => x.IsMaster);
                var primaryImageUrl = (masterVariant?.VariantImages.OrderBy(i => i.Position).FirstOrDefault()
                    ?? product.Variants.SelectMany(x => x.VariantImages).OrderBy(i => i.Position).FirstOrDefault())
                    ?.Url;

                return new CartItemLookup
                {
                    Sku = v.Sku ?? string.Empty,
                    ProductName = product.Name,
                    ProductImageUrl = primaryImageUrl,
                };
            });
        }
```

- [ ] **Step 3: Update endpoint Produces signature**

Modify `RemoveCartItem.Endpoint.cs` — change:
```csharp
            .Produces<Result>()
```
to:
```csharp
            .Produces<Result<Response>>()
```

- [ ] **Step 4: Build and verify**

Run: `dotnet build service/Api/src/Module/`
Expected: PASS, no warnings

---

### Task 3: Add line items to OrderDetailResponse

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Orders/Shared/Models/Order.Model.Response.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Orders/Shared/Mappings/Order.Mapping.Model.cs`

**Interfaces:**
- Consumes: `LineItemResponse` (already defined), `MapToLineItemResponse<T>()` (already defined)
- Produces: `OrderDetailResponse` now includes `List<LineItemResponse> LineItems`

- [ ] **Step 1: Add LineItems property to OrderDetailResponse**

Modify `Order.Model.Response.cs` — add to `OrderDetailResponse` after line 27 (`DateTimeOffset? ModifiedAtUtc { get; init; }`):

```csharp
    public List<LineItemResponse> LineItems { get; init; } = [];
```

- [ ] **Step 2: Map line items in MapToDetail**

Modify `Order.Mapping.Model.cs` — in `MapToDetail<T>()` (line 16-47), add line items mapping after `ModifiedAtUtc = entity.ModifiedAtUtc,` (line 45):

```csharp
            LineItems = entity.LineItems
                .Select(li => li.MapToLineItemResponse<LineItemResponse>())
                .ToList(),
```

Add import for line item mapping if not already present (line 2 already has `using Module.Ordering.Domain.LineItems;`):
```csharp
using Module.Ordering.Features.Storefront.Cart.Shared.Mappings;
```
is not needed — `MapToLineItemResponse<T>()` is defined in the same `OrderMapping` class.

- [ ] **Step 3: Build and verify**

Run: `dotnet build service/Api/src/Module/`
Expected: PASS, no warnings

- [ ] **Step 4: Run Ordering unit tests**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Ordering" --no-build`
Expected: All existing tests PASS (MapToDetail tests may need their response assertions updated — if any test asserts on the exact response shape, it may fail)

---

### Task 4: Remove CartExpiryService (keep Hangfire)

**Files:**
- Delete: `service/Api/src/Module/Ordering/Services/CartExpiryService.cs`
- Delete: `service/Api/src/Module/Ordering/Services/CartExpiryService.Loggers.cs`
- Modify: `service/Api/src/Module/Ordering/Ordering.Extension.cs`

**Interfaces:**
- Consumes: n/a
- Produces: n/a — removal only, Hangfire `CartExpiryJobScheduler` remains

- [ ] **Step 1: Delete CartExpiryService files**

```bash
rm service/Api/src/Module/Ordering/Services/CartExpiryService.cs
rm service/Api/src/Module/Ordering/Services/CartExpiryService.Loggers.cs
```

- [ ] **Step 2: Remove registration from Ordering.Extension.cs**

Modify `Ordering.Extension.cs` — remove lines 19-26 (the comment block and `CartExpiryService` registration). The resulting `AddOrderingModule` method:

```csharp
    public static WebApplicationBuilder AddOrderingModule(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<Backgrounds.CartExpiryJob>();
        builder.Services.AddHostedService<Backgrounds.CartExpiryJobScheduler>();

        builder.AddSeeder<OrderSeeder>();
        builder.AddSeeder<PaymentSeeder>();

        return builder;
    }
```

- [ ] **Step 3: Build and verify**

Run: `dotnet build service/Api/src/Module/`
Expected: PASS, no warnings

- [ ] **Step 4: Run CartExpiryJob unit tests**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~CartExpiry"`
Expected: All existing tests PASS

---

### Task 5: Clean up DeliveryRequired TODO

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Order.Method.Checkout.cs`

**Interfaces:**
- Consumes: n/a
- Produces: n/a — comment-only change

- [ ] **Step 1: Remove TODO comment**

Modify `Order.Method.Checkout.cs` line 68-70 — change from:
```csharp
    // TODO: Implement product-type-based delivery check (physical vs digital).
    //       Currently hardcoded true — all orders require delivery.
    public static bool DeliveryRequired() => true;
```
to:
```csharp
    public static bool DeliveryRequired() => true;
```

- [ ] **Step 2: Build and verify**

Run: `dotnet build service/Api/src/Module/`
Expected: PASS, no warnings

---

### Task 6: Fix currentStep race in useCheckout composable

**Files:**
- Modify: `app/Store/src/features/ordering/composables/useCheckout.ts`

**Interfaces:**
- Consumes: n/a
- Produces: `createPaymentIntent()` no longer sets `currentStep` — view controls all step transitions

- [ ] **Step 1: Remove currentStep assignment from createPaymentIntent**

Modify `useCheckout.ts` — delete line 92 (`currentStep.value = 4`).

Add a `validateCheckout` method to the composable at the end of the function (before `return reactive(...)`):

```ts
  async function validateCheckout(): Promise<boolean> {
    loading.value = true
    error.value = null
    try {
      const result = await CheckoutApi.validateCheckout()
      if (!result.isSuccess) {
        error.value = result.message
      }
      loading.value = false
      return result.isSuccess
    } catch {
      error.value = 'Failed to validate checkout'
      loading.value = false
      return false
    }
  }
```

Add `validateCheckout` to the returned reactive object (after `placeOrder` in line 141):
```ts
    init, saveAddress, selectShippingRate, createPaymentIntent, placeOrder, validateCheckout, reset,
```

- [ ] **Step 2: Run Store SPA lint and tests**

Run: `cd app/Store && pnpm run lint && pnpm run test:unit`
Expected: PASS

---

### Task 7: Wire cart re-fetch + validate into Review step

**Files:**
- Modify: `app/Store/src/features/ordering/views/CheckoutView.vue`

**Interfaces:**
- Consumes: `cart.fetchCart()`, `checkout.validateCheckout()` (new method from Task 6)
- Produces: Review step now validates readiness before advancing

- [ ] **Step 1: Update advanceToReview to re-fetch and validate**

Modify `CheckoutView.vue` — change `advanceToReview()` function (lines 231-233) from:
```ts
function advanceToReview(): void {
  if (checkout.paymentClientSecret) checkout.currentStep = 4
}
```
to:
```ts
async function advanceToReview(): Promise<void> {
  if (!checkout.paymentClientSecret) return
  checkout.loading = true
  checkout.error = null
  const cartOk = await cart.fetchCart()
  const validOk = cartOk ? await checkout.validateCheckout() : false
  checkout.loading = false
  if (cartOk && validOk) {
    checkout.currentStep = 4
  }
}
```

- [ ] **Step 2: Run Store SPA lint and tests**

Run: `cd app/Store && pnpm run lint && pnpm run test:unit`
Expected: PASS

---

### Task 8: Full verification

**Files:**
- None — verification only

- [ ] **Step 1: Build entire solution**

Run: `dotnet build`
Expected: PASS, no warnings

- [ ] **Step 2: Run Ordering unit tests**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Ordering"`
Expected: All PASS

- [ ] **Step 3: Run Store SPA verification**

Run: `cd app/Store && pnpm run lint && pnpm run test:unit`
Expected: PASS

- [ ] **Step 4: Run feature conventions check**

Run: `bash scripts/check-feature-conventions.sh`
Expected: PASS (no drift)

- [ ] **Step 5: Commit all changes**

```bash
git add service/Api/src/Module/Ordering/ app/Store/src/features/ordering/
git commit -m "fix(ordering): resolve 8 ordering flow quality gaps

- Return full cart from updateItem/removeItem handlers (fixes Zod parser crash)
- Add line items to OrderDetailResponse (fixes empty table in OrderDetailView)
- Remove duplicate CartExpiryService, keep Hangfire only
- Clean up DeliveryRequired() TODO comment
- Fix currentStep race: composable no longer sets step, view controls all transitions
- Wire cart re-fetch + validateCheckout into Review step"
```
