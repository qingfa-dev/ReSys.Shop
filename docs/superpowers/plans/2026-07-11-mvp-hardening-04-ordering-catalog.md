# Plan 4: Ordering & Catalog Integrity

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix ordering status guards, stock release on cancel, guest cart support, catalog SQL bugs, and validation gaps.

**Architecture:** Add status guards to admin order mutations. Fix stock release in UpdateOrderStatus. Add session-based cart support for guests. Fix catalog SQL table names and validation.

**Tech Stack:** .NET 10, EF Core, Carter, FluentValidation

## Global Constraints

- `TreatWarningsAsErrors=true` globally.
- All handlers return `Result<T>` or `Result`.
- Stock reservations are soft — `CountOnHand` not decremented on reserve.

---

## File Structure

| Action | File | Responsibility |
|--------|------|---------------|
| Modify | `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateStatus/UpdateOrderStatus.cs` | Add stock release + CanceledById |
| Modify | `service/Api/src/Module/Ordering/Features/Admin/Orders/Update/UpdateOrderAdmin.cs` | Add status guard |
| Modify | `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateLineItem/UpdateOrderLineItem.cs` | Add status guard + stock check |
| Modify | `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateShipAddress/UpdateOrderShipAddress.cs` | Add status guard |
| Modify | `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateBillAddress/UpdateOrderBillAddress.cs` | Add status guard |
| Modify | `service/Api/src/Module/Ordering/Domain/Orders/Services/OrderMerger.cs` | Add MaxQuantity check |
| Modify | `service/Api/src/Module/Ordering/Features/Storefront/Cart/EmptyCart/EmptyCart.cs` | Add guest session fallback |
| Modify | `service/Api/src/Module/Ordering/Features/Storefront/Cart/DeleteCart/DeleteCart.cs` | Add guest session fallback |
| Modify | `service/Api/src/Module/Ordering/Features/Storefront/Cart/Get/GetCart.cs` | Fix unauthenticated response |
| Modify | `service/Api/src/Module/Ordering/Domain/LineItems/LineItem.Method.Compute.cs` | Fix FinalAmount double-count |
| Modify | `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/Similar/GetSimilarProducts.cs` | Fix SQL table names + Result wrap |
| Modify | `service/Api/src/Module/Catalog/Features/Admin/Products/Create/CreateProduct.cs` | Add variant result check |
| Modify | `service/Api/src/Module/Catalog/Features/Storefront/Products/SearchByImage/SearchByImage.Endpoint.cs` | Add file size validation |
| Modify | `service/Api/src/Module/Catalog/Features/Admin/Products/Delete/DeleteProduct.cs` | Add double-deletion guard |
| Modify | `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Add/AddVariant.cs` | Add SKU uniqueness check |
| Modify | `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Update/UpdateVariant.cs` | Add SKU uniqueness check |
| Modify | `service/Api/src/Module/Catalog/Features/Admin/Taxonomies/Restore/RestoreTaxonomy.cs` | Cascade-restore children |
| Modify | `service/Api/src/Module/Catalog/Features/Admin/Taxonomies/Create/CreateTaxonomy.cs` | Use shared slug utility |

---

### Task 1: Fix UpdateOrderStatus — Add Stock Release + Audit

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateStatus/UpdateOrderStatus.cs`

**Interfaces:**
- Consumes: `OrderInventoryService` (used in `CancelOrder.cs`)

- [ ] **Step 1: Read both handlers for reference**

Read `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateStatus/UpdateOrderStatus.cs` and `service/Api/src/Module/Ordering/Features/Admin/Orders/Cancel/CancelOrderAdmin.cs` to understand the stock release pattern.

- [ ] **Step 2: Add stock release when canceling from Placed**

In the `Handle` method, after setting `CanceledAtUtc`, add the stock release loop (same as `CancelOrderAdmin.cs`):

```csharp
if (entity.Status == OrderStatus.Placed)
{
    foreach (var li in entity.LineItems)
    {
        var orderInventory = new OrderInventoryService(entity, li, dbContext, stockChecker);
        await orderInventory.RemoveAsync(li.Quantity);
    }
}
```

- [ ] **Step 3: Add CanceledById for audit trail**

After setting `CanceledAtUtc`, add:
```csharp
entity.CanceledById = Guid.TryParse(currentUser.UserId, out var canceledBy) ? canceledBy : null;
```

Inject `ICurrentUser currentUser` into the constructor if not already present.

- [ ] **Step 4: Remove Draft→Placed transition or add guards**

Find the code that transitions Draft→Placed and either remove it or add stock availability + reservation checks. The safest approach is to remove it entirely — admins should use the checkout flow.

- [ ] **Step 5: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Ordering/Module.Ordering.csproj`
Expected: Build succeeds

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateStatus/UpdateOrderStatus.cs
git commit -m "fix(ordering): add stock release on admin cancel via UpdateOrderStatus

Previously canceled orders did not release stock, causing inventory
discrepancies. Also added CanceledById for audit trail."
```

---

### Task 2: Add Status Guards to Admin Order Mutations

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Orders/Update/UpdateOrderAdmin.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateLineItem/UpdateOrderLineItem.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateShipAddress/UpdateOrderShipAddress.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateBillAddress/UpdateOrderBillAddress.cs`

**Interfaces:**
- Consumes: `order.Uneditable()` or `OrderStatus` check

- [ ] **Step 1: Read UpdateOrderAdmin handler**

Read `service/Api/src/Module/Ordering/Features/Admin/Orders/Update/UpdateOrderAdmin.cs`.

- [ ] **Step 2: Add status guard to UpdateOrderAdmin**

After loading the order, before applying changes, add:
```csharp
if (order.Status != OrderStatus.Draft)
    return Error.Validation("Order.Update.NotDraft", "Only draft orders can be modified.");
```

- [ ] **Step 3: Add status guard to UpdateOrderLineItem**

Read `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateLineItem/UpdateOrderLineItem.cs`.

After loading the order, add:
```csharp
if (order.Status != OrderStatus.Draft)
    return Error.Validation("Order.LineItem.Update.NotDraft", "Only draft orders can have line items modified.");
```

- [ ] **Step 4: Add status guard to UpdateOrderShipAddress**

Read `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateShipAddress/UpdateOrderShipAddress.cs`.

After loading the order, add:
```csharp
if (order.Status != OrderStatus.Draft)
    return Error.Validation("Order.ShipAddress.Update.NotDraft", "Only draft orders can have shipping address modified.");
```

- [ ] **Step 5: Add status guard to UpdateOrderBillAddress**

Same pattern as Step 4 for billing address.

- [ ] **Step 6: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Ordering/Module.Ordering.csproj`
Expected: Build succeeds

- [ ] **Step 7: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Admin/Orders/
git commit -m "fix(ordering): add Draft-only status guards to admin order mutations

Prevents modification of Placed, Canceled, or Completed orders."
```

---

### Task 3: Add MaxQuantity Check to OrderMerger

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/Orders/Services/OrderMerger.cs`

**Interfaces:**
- Consumes: `LineItemConstant.MaxQuantity`

- [ ] **Step 1: Read the current merger**

Read `service/Api/src/Module/Ordering/Domain/Orders/Services/OrderMerger.cs` — find where quantities are summed.

- [ ] **Step 2: Add MaxQuantity guard**

Before `currentLineItem.Quantity += otherLineItem.Quantity;`, add:
```csharp
if (currentLineItem.Quantity + otherLineItem.Quantity > LineItemConstant.MaxQuantity)
    return; // or return an error
```

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Ordering/Module.Ordering.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Ordering/Domain/Orders/Services/OrderMerger.cs
git commit -m "fix(ordering): add MaxQuantity guard when merging guest cart items"
```

---

### Task 4: Add Guest Session Fallback to EmptyCart and DeleteCart

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/EmptyCart/EmptyCart.cs`
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/DeleteCart/DeleteCart.cs`

**Interfaces:**
- Consumes: `currentUser.SessionId`, `currentUser.IsAuthenticated`

- [ ] **Step 1: Read AddToCart for the session fallback pattern**

Read `service/Api/src/Module/Ordering/Features/Storefront/Cart/AddItem/AddToCart.cs` — find how it handles guest users via `currentUser.SessionId`.

- [ ] **Step 2: Add session fallback to EmptyCart**

Read `service/Api/src/Module/Ordering/Features/Storefront/Cart/EmptyCart/EmptyCart.cs`.

Replace the user-ID-only lookup with a fallback:
```csharp
var userId = Guid.TryParse(currentUser.UserId, out var parsed) ? parsed : (Guid?)null;
var sessionId = currentUser.IsAuthenticated ? null : currentUser.SessionId;

var cart = await dbContext.Set<Order>()
    .FirstOrDefaultAsync(o =>
        (userId.HasValue && o.UserId == userId.Value)
        || (sessionId != null && o.SessionId == sessionId),
    cancellationToken);
```

- [ ] **Step 3: Add session fallback to DeleteCart**

Same pattern as Step 2 for `DeleteCart.cs`.

- [ ] **Step 4: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Ordering/Module.Ordering.csproj`
Expected: Build succeeds

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/Cart/EmptyCart/EmptyCart.cs
git add service/Api/src/Module/Ordering/Features/Storefront/Cart/DeleteCart/DeleteCart.cs
git commit -m "fix(ordering): add guest session fallback to EmptyCart and DeleteCart"
```

---

### Task 5: Fix GetCart Unauthenticated Response

**Files:**
- Modify: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Get/GetCart.cs`

**Interfaces:**
- Consumes: `OrderResult.Errors.UserNotAuthenticated`

- [ ] **Step 1: Read the current handler**

Read `service/Api/src/Module/Ordering/Features/Storefront/Cart/Get/GetCart.cs`.

- [ ] **Step 2: Fix the unauthenticated response**

Find where it returns 200 OK with empty response for unauthenticated users without session. Change to:
```csharp
return OrderResult.Errors.UserNotAuthenticated;
```

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Ordering/Module.Ordering.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Ordering/Features/Storefront/Cart/Get/GetCart.cs
git commit -m "fix(ordering): return UserNotAuthenticated for unauthenticated GetCart"
```

---

### Task 6: Fix LineItem.FinalAmount Double-Count

**Files:**
- Modify: `service/Api/src/Module/Ordering/Domain/LineItems/LineItem.Method.Compute.cs`

**Interfaces:**
- N/A

- [ ] **Step 1: Read the current method**

Read `service/Api/src/Module/Ordering/Domain/LineItems/LineItem.Method.Compute.cs`.

- [ ] **Step 2: Fix FinalAmount to not double-count AdjustmentTotal**

Change:
```csharp
return lineItem.Total + lineItem.AdjustmentTotal;
```

To:
```csharp
return lineItem.Total; // Total already includes AdjustmentTotal from RecalculateTotal
```

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Ordering/Module.Ordering.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Ordering/Domain/LineItems/LineItem.Method.Compute.cs
git commit -m "fix(ordering): fix FinalAmount double-counting AdjustmentTotal"
```

---

### Task 7: Fix GetSimilarProducts SQL + Result Wrap

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/Similar/GetSimilarProducts.cs`

**Interfaces:**
- Consumes: `CatalogSchema.TableNames` constants

- [ ] **Step 1: Read the current handler**

Read `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/Similar/GetSimilarProducts.cs`.

- [ ] **Step 2: Fix SQL table names**

Replace the incorrect table names in the raw SQL:
- `"Variants"` → `"catalog.variants"`
- `"VariantImages"` → `"catalog.product_images"`
- `"ImageEmbeddings"` → `"catalog.product_image_embeddings"`

Also fix column names to snake_case if needed (check `CatalogSchema.TableNames`).

- [ ] **Step 3: Fix Result wrap on happy path**

Change:
```csharp
return new Response { Items = items };
```

To:
```csharp
return Result<Response>.Ok(new Response { Items = items });
```

- [ ] **Step 4: Add primary variant ordering**

Change the variant lookup to prioritize master variant:
```csharp
.OrderBy(v => v.Position)
.ThenBy(v => v.IsMaster ? 0 : 1)
```

- [ ] **Step 5: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Catalog/Module.Catalog.csproj`
Expected: Build succeeds

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Storefront/Products/Get/Similar/GetSimilarProducts.cs
git commit -m "fix(catalog): fix SQL table names, Result wrap, and variant ordering

- Use correct schema-qualified table names from CatalogSchema.TableNames
- Wrap response in Result<Response>.Ok()
- Prioritize master variant for similarity lookup"
```

---

### Task 8: Fix CreateProduct — Add Variant Result Check

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Create/CreateProduct.cs`

**Interfaces:**
- N/A

- [ ] **Step 1: Read the current handler**

Read `service/Api/src/Module/Catalog/Features/Admin/Products/Create/CreateProduct.cs`.

- [ ] **Step 2: Add guard before accessing addVariantResult.Value**

Before `product.MasterVariantId = addVariantResult.Value.Id;`, add:
```csharp
if (addVariantResult.IsFailure)
    return addVariantResult.Errors;
```

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Catalog/Module.Catalog.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Admin/Products/Create/CreateProduct.cs
git commit -m "fix(catalog): guard addVariantResult before accessing .Value"
```

---

### Task 9: Add File Size Validation to SearchByImage

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/SearchByImage/SearchByImage.Endpoint.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/SearchByImage/SearchByImage.cs`

**Interfaces:**
- N/A

- [ ] **Step 1: Read the current endpoint/handler**

Read both files to understand the image upload flow.

- [ ] **Step 2: Add file size check in handler**

At the top of the `Handle` method, before the `MemoryStream` allocation:
```csharp
const long MaxFileSize = 10_485_760; // 10 MB
if (image.Length > MaxFileSize)
    return Error.Validation("SearchByImage.FileTooLarge", "Image file must not exceed 10 MB.");

if (!image.ContentType.StartsWith("image/"))
    return Error.Validation("SearchByImage.InvalidContentType", "File must be an image.");
```

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Catalog/Module.Catalog.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Storefront/Products/SearchByImage/
git commit -m "fix(catalog): add file size and content-type validation to SearchByImage"
```

---

### Task 10: Add Double-Deletion Guard to DeleteProduct

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Delete/DeleteProduct.cs`

**Interfaces:**
- Consumes: `ProductResult.Errors.AlreadyDeleted`

- [ ] **Step 1: Read the current handler**

Read `service/Api/src/Module/Catalog/Features/Admin/Products/Delete/DeleteProduct.cs`.

- [ ] **Step 2: Add guard after loading entity**

After `if (entity is null) return ProductResult.Errors.NotFound(request.Id);`, add:
```csharp
if (entity.IsDeleted)
    return ProductResult.Errors.AlreadyDeleted;
```

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Catalog/Module.Catalog.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Admin/Products/Delete/DeleteProduct.cs
git commit -m "fix(catalog): add double-deletion guard to DeleteProduct"
```

---

### Task 11: Add SKU Uniqueness to AddVariant and UpdateVariant

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Add/AddVariant.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Update/UpdateVariant.cs`

**Interfaces:**
- Consumes: `Variant` entity, `dbContext.Set<Variant>()`

- [ ] **Step 1: Read AddVariant handler**

Read `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Add/AddVariant.cs`.

- [ ] **Step 2: Add SKU uniqueness check to AddVariant**

Before creating the variant, add:
```csharp
var skuExists = await dbContext.Set<Variant>()
    .AnyAsync(x => x.Sku == request.Sku, cancellationToken);

if (skuExists)
    return VariantResult.Errors.SkuAlreadyExists(request.Sku);
```

- [ ] **Step 3: Add SKU uniqueness check to UpdateVariant**

Read `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Update/UpdateVariant.cs`.

Before updating, add:
```csharp
var skuExists = await dbContext.Set<Variant>()
    .AnyAsync(x => x.Sku == request.Sku && x.Id != command.Id, cancellationToken);

if (skuExists)
    return VariantResult.Errors.SkuAlreadyExists(request.Sku);
```

- [ ] **Step 4: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Catalog/Module.Catalog.csproj`
Expected: Build succeeds

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Admin/Products/Variants/
git commit -m "fix(catalog): enforce SKU uniqueness across variants"
```

---

### Task 12: Fix RestoreTaxonomy — Cascade Children

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Taxonomies/Restore/RestoreTaxonomy.cs`

**Interfaces:**
- Consumes: `RestoreTaxon.Command`

- [ ] **Step 1: Read the current handler**

Read `service/Api/src/Module/Catalog/Features/Admin/Taxonomies/Restore/RestoreTaxonomy.cs`.

- [ ] **Step 2: Add cascade restore for child taxons**

After restoring the root taxon, add:
```csharp
foreach (var taxon in entity.Taxons.Where(t => t.IsDeleted))
{
    await sender.Send(new RestoreTaxon.Command(taxon.Id), cancellationToken);
}
```

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Catalog/Module.Catalog.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Admin/Taxonomies/Restore/RestoreTaxonomy.cs
git commit -m "fix(catalog): cascade-restore child taxons when restoring taxonomy"
```

---

### Task 13: Fix CreateTaxonomy Slug Generation

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Taxonomies/Create/CreateTaxonomy.cs`

**Interfaces:**
- Consumes: `ProductMethod.GenerateSlugFromName()` (shared utility)

- [ ] **Step 1: Read the current handler**

Read `service/Api/src/Module/Catalog/Features/Admin/Taxonomies/Create/CreateTaxonomy.cs`.

- [ ] **Step 2: Replace naive slug with shared utility**

Change:
```csharp
Slug = entity.Name.ToLowerInvariant().Replace(' ', '-')
```

To:
```csharp
Slug = ProductMethod.GenerateSlugFromName(entity.Name)
```

Add the using if needed:
```csharp
using Module.Catalog.Domain.Products;
```

- [ ] **Step 3: Verify build compiles**

Run: `dotnet build service/Api/src/Module/Catalog/Module.Catalog.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Admin/Taxonomies/Create/CreateTaxonomy.cs
git commit -m "fix(catalog): use shared GenerateSlugFromName for taxonomy slugs"
```

---

### Task 14: Build and Verify All Changes

- [ ] **Step 1: Full solution build**

Run: `dotnet build`
Expected: Build succeeds with zero warnings

- [ ] **Step 2: Run unit tests**

Run: `dotnet test service/Api/tests/Module.UnitTests`
Expected: All tests pass

- [ ] **Step 3: Commit (if any fixes needed)**

```bash
git commit -m "fix: address build warnings from ordering and catalog fixes"
```
