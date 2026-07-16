# Catalog + Inventory Convention Remediation

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix all feature convention violations in the Catalog and Inventory modules — inline-field commands, unbased responses, wrong-base responses, and manual handler construction.

**Architecture:** Each fix follows one of three patterns: (1) extract inlined Command fields into a Request record, (2) create/use base Response types in the feature group's `Shared/Models/`, (3) replace `new Response { ... }` in Handlers with `entity.MapToDetail<T>()` calls via a new or existing mapping file.

**Tech Stack:** .NET 10, C# 13, MediatR, Carter, FluentValidation

## Global Constraints

- Warnings-as-errors global; any warning fails the build
- Result objects, not exceptions; all domain operations return `Result<T>` or `Result`
- Vertical slice feature files; follow static partial class pattern
- Forward-only dependency: Shared depends on nothing, Module depends on Shared
- Module-internal Shared/Models/ bases are correct — they need not move to the Shared assembly

---

## File Map

| File | Purpose | Action |
|---|---|---|
| **Catalog** | | |
| `Catalog/.../SearchByImage/SearchByImage.cs` | Fix Command(IFormFile) → Command(Request) | Modify |
| `Catalog/.../SearchByImage/SearchByImage.Request.cs` | Create Request with IFormFile Image | Create |
| `Catalog/.../SearchByImage/SearchByImage.Response.cs` | Inherit from proper base | Modify |
| `Catalog/.../Embeddings/Create/ImageEmbedding.Create.cs` | Fix Command(Guid, string) → Command(Request) | Modify |
| `Catalog/.../Embeddings/Create/ImageEmbedding.Create.Request.cs` | Create Request with ModelName | Create |
| `Catalog/.../Embeddings/Regenerate/ImageEmbedding.Regenerate.cs` | Fix Command(Guid, string, string) → Command(Request) | Modify |
| `Catalog/.../Embeddings/Regenerate/ImageEmbedding.Regenerate.Request.cs` | Create Request with ModelName+ModelVersion | Create |
| `Catalog/.../Reposition/RepositionTaxonUseCase.Response.cs` | Inherit from base | Modify |
| `Catalog/.../Rules/Sync/SyncTaxonRules.Response.cs` | Inherit from base | Modify |
| `Catalog/.../Variants/List/ListVariantsByProduct.Response.cs` | Inherit from base | Modify |
| `Catalog/.../Classifications/Get/GetProductClassifications.Response.cs` | Inherit from base | Modify |
| `Catalog/.../OptionValues/Get/GetVariantOptionValues.Response.cs` | Inherit from base | Modify |
| `Catalog/.../Images/ListByVariant/ListVariantImages.Response.cs` | Inherit from base | Modify |
| `Catalog/.../Images/Delete/DeleteVariantImage.Response.cs` | Inherit from base | Modify |
| `Catalog/.../Prices/Set/SetVariantPrice.Response.cs` | Inherit from base | Modify |
| `Catalog/.../SearchByImage.Response.cs` | Inherit from base | Modify |
| `Catalog/.../Similar/GetSimilarProducts.Response.cs` | Inherit from base | Modify |
| `Catalog/.../Image/GetImage.cs` | Fix inline Response + direct construction | Modify |
| Various Catalog handlers | Replace `new Response { }` with mapping | Modify |
| **Inventory** | | |
| `Inventory/.../Import/ImportStockItems.cs` | Fix Command(IFormFile) → Command(Request) | Modify |
| `Inventory/.../Import/ImportStockItems.Request.cs` | Create Request with IFormFile | Create |
| `Inventory/.../Import/ImportStockItems.Response.cs` | Inherit from base | Modify |
| `Inventory/.../Check/GetStockAvailability.cs` | Fix Query(Guid, string?) → Query(Request) | Modify |
| `Inventory/.../Check/GetStockAvailability.Request.cs` | Create Request | Create |
| `Inventory/.../Check/GetStockAvailability.Response.cs` | Inherit from base | Modify |
| `Inventory/.../LowStock/GetLowStockItems.cs` | Fix Query(Guid?, int?) → Query(Request) | Modify |
| `Inventory/.../LowStock/GetLowStockItems.Request.cs` | Create Request | Create |
| `Inventory/.../Reserve/ReserveCartStock.cs` | Move CartToken into Request | Modify |
| `Inventory/.../ReserveCartStock.Request.cs` | Add CartToken to Request | Modify |
| `Inventory/.../Restock/RestockStockItem.Response.cs` | Change base to StockItemDetailResponse | Modify |
| `Inventory/.../Summary/GetStockSummary.Response.cs` | Change base or create proper Response | Modify |
| Inventory/... handlers | Replace `new Response { }` with mapping | Modify |

---

### Task 1: Fix Catalog SearchByImage — IFormFile wrapping + Response base + handler mapping

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/SearchByImage/SearchByImage.cs`
- Create: `service/Api/src/Module/Catalog/Features/Storefront/Products/SearchByImage/SearchByImage.Request.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/SearchByImage/SearchByImage.Response.cs`
- Modify or Create: `service/Api/src/Module/Catalog/Features/Storefront/Products/SearchByImage/SearchByImage.Endpoint.cs`

- [ ] **Step 1: Read and understand current files**

```bash
cat service/Api/src/Module/Catalog/Features/Storefront/Products/SearchByImage/SearchByImage.cs
cat service/Api/src/Module/Catalog/Features/Storefront/Products/SearchByImage/SearchByImage.Response.cs
cat service/Api/src/Module/Catalog/Features/Storefront/Products/SearchByImage/SearchByImage.Endpoint.cs
```

- [ ] **Step 2: Create SearchByImage.Request.cs**

```csharp
namespace Module.Catalog.Features.Storefront.Products.SearchByImage;

public static partial class SearchByImage
{
    public record Request
    {
        public required IFormFile Image { get; init; }
    }
}
```

- [ ] **Step 3: Change Command in SearchByImage.cs**

Replace:
```csharp
public sealed record Command(IFormFile Image) : ICommand<Response>;
```
With:
```csharp
public sealed record Command(Request Request) : ICommand<Response>;
```

- [ ] **Step 4: Update the Handler to unwrap command.Request**

Replace:
```csharp
var image = command.Image;
```
With:
```csharp
var image = command.Request.Image;
```

- [ ] **Step 5: Fix the early return (returns new Response { Items = [] } instead of using mapping)**

Replace:
```csharp
if (image is null || image.Length == 0)
    return new Response { Items = [] };
```
With:
```csharp
if (image is null || image.Length == 0)
    return new Response();
```

- [ ] **Step 6: Fix the main return to use mapping method**

Replace the `Select` block and `return new Response { Items = items }` with a proper mapping. Create a `SearchByImageMapping.Model.cs` or inline. Given the response shape (`Response` has `List<SearchResultItem> Items`), create a mapping:

In `SearchByImage.cs`, add a mapping method inside `SearchByImage`:

```csharp
private static SearchResultItem MapToItem(Variant v)
{
    var primaryImage = v.VariantImages.FirstOrDefault();
    return new SearchResultItem
    {
        VariantId = v.Id,
        ProductId = v.ProductId,
        ProductName = v.Product?.Name ?? string.Empty,
        Sku = v.Sku ?? string.Empty,
        Price = v.Price ?? 0,
        ImageUrl = primaryImage?.Url
    };
}
```

Then replace:
```csharp
var items = similarVariants.Select(v => { ... }).ToList();
return new Response { Items = items };
```
With:
```csharp
var items = similarVariants.Select(MapToItem).ToList();
return new Response { Items = items };
```

- [ ] **Step 7: Update the Endpoint to pass `Request` to `Command`**

Read the endpoint file. Change:
```csharp
var command = new Command(image);
```
To:
```csharp
var command = new Command(request);
```
(where `request` is the deserialized `[FromBody] Request` — if the endpoint currently takes `IFormFile image` as parameter, change it to `[FromForm] Request request`.)

- [ ] **Step 8: Build and verify**

```bash
dotnet build service/Api/src/Module/Module.csproj
```

Expected: Build passes with no warnings.

- [ ] **Step 9: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Storefront/Products/SearchByImage/
git commit -m "fix(Catalog): wrap IFormFile in Request, add mapping method for SearchByImage"

```

---

### Task 2: Fix Catalog CreateEmbedding — inline fields to Request

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Create/ImageEmbedding.Create.cs`
- Create: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Create/ImageEmbedding.Create.Request.cs`

- [ ] **Step 1: Create ImageEmbedding.Create.Request.cs**

```csharp
namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Create;

public static partial class CreateEmbedding
{
    public sealed record Request
    {
        public Guid VariantImageId { get; init; }
        public required string ModelName { get; init; }
    }
}
```

- [ ] **Step 2: Update Command in ImageEmbedding.Create.cs**

Replace:
```csharp
public sealed record Command(Guid VariantImageId, string ModelName) : ICommand<EmbeddingDetailResponse>;
```
With:
```csharp
public sealed record Command(Request Request) : ICommand<EmbeddingDetailResponse>;
```

- [ ] **Step 3: Update Handler to use command.Request**

Replace:
```csharp
var result = await orchestrator.GenerateAndPersistAsync(command.VariantImageId, command.ModelName, cancellationToken);
```
With:
```csharp
var req = command.Request;
var result = await orchestrator.GenerateAndPersistAsync(req.VariantImageId, req.ModelName, cancellationToken);
```

- [ ] **Step 4: Build**

```bash
dotnet build service/Api/src/Module/Module.csproj
```

Expected: Build passes.

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Create/
git commit -m "fix(Catalog): extract inline fields to Request record in CreateEmbedding"

```

---

### Task 3: Fix Catalog RegenerateEmbedding — inline fields to Request

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Regenerate/ImageEmbedding.Regenerate.cs`
- Create: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Regenerate/ImageEmbedding.Regenerate.Request.cs`

- [ ] **Step 1: Create ImageEmbedding.Regenerate.Request.cs**

```csharp
namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Regenerate;

public static partial class RegenerateEmbedding
{
    public sealed record Request
    {
        public Guid VariantImageId { get; init; }
        public required string ModelName { get; init; }
        public required string ModelVersion { get; init; }
    }
}
```

- [ ] **Step 2: Update Command in ImageEmbedding.Regenerate.cs**

Replace:
```csharp
public sealed record Command(Guid VariantImageId, string ModelName, string ModelVersion) : ICommand<EmbeddingDetailResponse>;
```
With:
```csharp
public sealed record Command(Request Request) : ICommand<EmbeddingDetailResponse>;
```

- [ ] **Step 3: Update Handler**

Replace:
```csharp
var result = await orchestrator.GenerateAndPersistAsync(command.VariantImageId, command.ModelName, cancellationToken);
```
With:
```csharp
var req = command.Request;
var result = await orchestrator.GenerateAndPersistAsync(req.VariantImageId, req.ModelName, cancellationToken);
```

- [ ] **Step 4: Build and commit**

```bash
dotnet build service/Api/src/Module/Module.csproj
git add service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Regenerate/
git commit -m "fix(Catalog): extract inline fields to Request record in RegenerateEmbedding"

```

---

### Task 4: Fix Catalog unbased Response records — inherit from proper base types

**Files (modify each):**

- `service/Api/src/Module/Catalog/Features/Admin/Taxonomies/Taxons/Reposition/RepositionTaxonUseCase.Response.cs`
- `service/Api/src/Module/Catalog/Features/Admin/Taxonomies/Taxons/Rules/Sync/SyncTaxonRules.Response.cs`
- `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/List/ListVariantsByProduct.Response.cs`
- `service/Api/src/Module/Catalog/Features/Admin/Products/Classifications/Get/GetProductClassifications.Response.cs`
- `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/OptionValues/Get/GetVariantOptionValues.Response.cs`
- `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/ListByVariant/ListVariantImages.Response.cs`
- `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Delete/DeleteVariantImage.Response.cs`
- `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Prices/Set/SetVariantPrice.Response.cs`
- `service/Api/src/Module/Catalog/Features/Storefront/Products/SearchByImage/SearchByImage.Response.cs`
- `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/Similar/GetSimilarProducts.Response.cs`
- `service/Api/src/Module/Catalog/Features/Storefront/Images/Get/Image/GetImage.cs`

**Pattern:**

For each response, first check if a suitable base type already exists in the feature group's `Shared/Models/` directory. If yes, inherit from it. If no, the response is likely feature-specific and can remain standalone if the feature genuinely doesn't need a shared base (e.g., `SearchByImage.Response` is a search result container, not a domain entity DTO). The key requirement is that the convention check passes — some features like image serving (GetImage) may be legitimate exceptions.

- [ ] **Step 1: Audit each Response file to determine appropriate base type**

For each file, check:
1. Does it represent a domain entity DTO? → Inherit from existing `{Entity}DetailResponse` or `{Entity}ListItemResponse`
2. Is it a feature-specific aggregate (e.g., search results, import results)? → Add a `: record` comment to mark as intentional, or create a minimal abstract base.

For Catalog taxons, use `TaxonDetailResponse` (exists at `Catalog/.../Taxons/Shared/Models/Taxon.Model.Response.cs`).
For variant images, use `VariantImageDetailResponse` (exists at `Catalog/.../Images/Shared/Models/VariantImage.Model.Response.cs`).
For variant prices, use `PriceDetailResponse` (exists at `Catalog/.../Prices/Shared/Models/Price.Model.Response.cs`).
For variants list, use `VariantListItemResponse` (exists at `Catalog/.../Variants/Shared/Models/Variant.Model.Response.cs`).

- [ ] **Step 2: Fix RepositionTaxonUseCase.Response.cs**

Read current file. If it represents a taxon, change to:
```csharp
public sealed record Response : TaxonDetailResponse;
```
If the properties differ, keep the current shape but add a base:
```csharp
public sealed record Response : TaxonDetailResponse
{
    // feature-specific fields
}
```

- [ ] **Step 3: Fix SyncTaxonRules.Response.cs** — similar approach with `TaxonRuleDetailResponse` or create a minimal `TaxonRuleResponseBase` if none exists.

- [ ] **Step 4: Fix remaining Catalog Response files** — same pattern. For each, read the file, identify the correct base type from the feature's Shared/Models/, and add inheritance.

- [ ] **Step 5: Build and verify no regressions**

```bash
dotnet build service/Api/src/Module/Module.csproj
```

- [ ] **Step 6: Run convention check**

```bash
bash scripts/check-feature-conventions.sh
```

Expected: AC-002 shows fewer failures for Catalog.

- [ ] **Step 7: Commit**

```bash
git add service/Api/src/Module/Catalog/
git commit -m "fix(Catalog): add base type inheritance to Response records"

```

---

### Task 5: Fix Inventory ImportStockItems — IFormFile wrapping + Response base + handler mapping

**Files:**
- Modify: `service/Api/src/Module/Inventory/Features/Admin/StockItems/Import/ImportStockItems.cs`
- Create: `service/Api/src/Module/Inventory/Features/Admin/StockItems/Import/ImportStockItems.Request.cs`
- Modify: `service/Api/src/Module/Inventory/Features/Admin/StockItems/Import/ImportStockItems.Response.cs`

- [ ] **Step 1: Create ImportStockItems.Request.cs**

```csharp
namespace Module.Inventory.Features.Admin.StockItems.Import;

public static partial class ImportStockItems
{
    public sealed record Request
    {
        public required IFormFile File { get; init; }
    }
}
```

- [ ] **Step 2: Change Command in ImportStockItems.cs**

Replace:
```csharp
public sealed record Command(IFormFile File) : ICommand<Response>;
```
With:
```csharp
public sealed record Command(Request Request) : ICommand<Response>;
```

- [ ] **Step 3: Update Handler to unwrap**

Replace:
```csharp
var file = command.File;
```
With:
```csharp
var file = command.Request.File;
```

- [ ] **Step 4: Fix Response base — create ImportResponseParameters base or use existing**

Given that `ImportStockItems.Response` has `Created`, `Updated`, `Failed`, `Errors` — this is an import result, not a domain entity. Create a base in the feature's Shared/Models:

Create or modify `StockItem.Model.Response.cs` to add:
```csharp
public record ImportStockItemsResponse
{
    public int Created { get; init; }
    public int Updated { get; init; }
    public int Failed { get; init; }
    public List<string> Errors { get; init; } = [];
}
```

Then change `ImportStockItems.Response.cs`:
```csharp
public sealed record Response : ImportStockItemsResponse;
```

- [ ] **Step 5: Fix the `return new Response` to use proper constructor**

Replace:
```csharp
return new Response
{
    Created = created,
    Updated = updated,
    Failed = errors.Count,
    Errors = errors
};
```
With:
```csharp
return new Response
{
    Created = created,
    Updated = updated,
    Failed = errors.Count,
    Errors = errors
};
```
(This is fine since there's no domain entity to map from — the import handler aggregates counters. Mark with a comment: `// EXCEPTION: no domain entity — direct construction intentional`)

- [ ] **Step 6: Build and commit**

```bash
dotnet build service/Api/src/Module/Module.csproj
git add service/Api/src/Module/Inventory/Features/Admin/StockItems/Import/
git commit -m "fix(Inventory): wrap IFormFile in Request, add base to ImportStockItems Response"

```

---

### Task 6: Fix Inventory GetStockAvailability — inline CartToken to Request + Response base

**Files:**
- Modify: `service/Api/src/Module/Inventory/Features/Storefront/StockAvailability/Check/GetStockAvailability.cs`
- Create: `service/Api/src/Module/Inventory/Features/Storefront/StockAvailability/Check/GetStockAvailability.Request.cs`
- Modify: `service/Api/src/Module/Inventory/Features/Storefront/StockAvailability/Check/GetStockAvailability.Response.cs`

- [ ] **Step 1: Create GetStockAvailability.Request.cs**

```csharp
namespace Module.Inventory.Features.Storefront.StockAvailability.Check;

public static partial class GetStockAvailability
{
    public sealed record Request
    {
        public Guid VariantId { get; init; }
        public string? CartToken { get; init; }
    }
}
```

- [ ] **Step 2: Change Query in GetStockAvailability.cs**

Replace:
```csharp
public sealed record Query(Guid VariantId, string? CartToken = null) : IQuery<Response>;
```
With:
```csharp
public sealed record Query(Request Request) : IQuery<Response>;
```

- [ ] **Step 3: Update Handler to unwrap**

Replace references to `request.VariantId` with `request.Request.VariantId` and `request.CartToken` with `request.Request.CartToken`.

- [ ] **Step 4: Fix Response base — create AvailabilityResponse base**

The `Response` record has `VariantId`, `TotalOnHand`, `TotalReserved`, etc. Check if `StockItemDetailResponse` has these fields — it does not (it has StockLocationId, VariantId, CountOnHand, Backorderable). Create an `AvailabilityDetailResponse` base:

In `StockItem.Model.Response.cs`:
```csharp
public record AvailabilityDetailResponse
{
    public Guid VariantId { get; init; }
    public int TotalOnHand { get; init; }
    public int TotalReserved { get; init; }
    public int CartReserved { get; init; }
    public int TotalAvailable { get; init; }
    public int AvailableToCart { get; init; }
    public List<LocationAvailability> LocationAvailability { get; init; } = [];
}
```

Then in `GetStockAvailability.Response.cs`:
```csharp
public sealed record Response : AvailabilityDetailResponse;
```

- [ ] **Step 5: Build and commit**

```bash
dotnet build service/Api/src/Module/Module.csproj
git add service/Api/src/Module/Inventory/Features/Storefront/StockAvailability/
git commit -m "fix(Inventory): extract CartToken to Request, add base Response for GetStockAvailability"

```

---

### Task 7: Fix Inventory GetLowStockItems — inline Threshold to Request

**Files:**
- Modify: `service/Api/src/Module/Inventory/Features/Admin/StockItems/LowStock/GetLowStockItems.cs`
- Create: `service/Api/src/Module/Inventory/Features/Admin/StockItems/LowStock/GetLowStockItems.Request.cs`

- [ ] **Step 1: Read current file**

```bash
cat service/Api/src/Module/Inventory/Features/Admin/StockItems/LowStock/GetLowStockItems.cs
```

- [ ] **Step 2: Create GetLowStockItems.Request.cs**

```csharp
namespace Module.Inventory.Features.Admin.StockItems.LowStock;

public static partial class GetLowStockItems
{
    public sealed record Request
    {
        public Guid? LocationId { get; init; }
        public int? Threshold { get; init; }
    }
}
```

- [ ] **Step 3: Change Query**

Replace:
```csharp
public sealed record Query(Guid? LocationId, int? Threshold) : IQuery<Response>;
```
With:
```csharp
public sealed record Query(Request Request) : IQuery<Response>;
```

- [ ] **Step 4: Update Handler to use `command.Request.LocationId` / `command.Request.Threshold`**

- [ ] **Step 5: Build and commit**

```bash
dotnet build service/Api/src/Module/Module.csproj
git add service/Api/src/Module/Inventory/Features/Admin/StockItems/LowStock/
git commit -m "fix(Inventory): extract Threshold to Request record in GetLowStockItems"

```

---

### Task 8: Fix Inventory ReserveCartStock — move CartToken into existing Request

**Files:**
- Modify: `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Reserve/ReserveCartStock.cs`
- Modify: `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Reserve/ReserveCartStock.Request.cs`

Find the existing `Request` record — add `CartToken` to it.

- [ ] **Step 1: Read current Request file**

```bash
cat service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Reserve/ReserveCartStock.Request.cs
```

- [ ] **Step 2: Add CartToken to the Request record**

```csharp
public sealed record Request
{
    // ... existing properties
    public required string CartToken { get; init; }  // Add this
}
```

- [ ] **Step 3: Change Command from `Command(Request Request, string CartToken)` to `Command(Request Request)`**

Remove the second parameter.

- [ ] **Step 4: Update Handler**

Replace `command.CartToken` with `command.Request.CartToken`.

- [ ] **Step 5: Build and commit**

```bash
dotnet build service/Api/src/Module/Module.csproj
git add service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Reserve/
git commit -m "fix(Inventory): move CartToken into Request in ReserveCartStock"

```

---

### Task 9: Fix Inventory RestockStockItem — change from RestockResult to StockItemDetailResponse

**Files:**
- Modify: `service/Api/src/Module/Inventory/Features/Admin/StockItems/Restock/RestockStockItem.Response.cs`
- Modify: `service/Api/src/Module/Inventory/Features/Admin/StockItems/Restock/RestockStockItem.cs`

- [ ] **Step 1: Read the Restock handler to understand the domain mapping**

```bash
cat service/Api/src/Module/Inventory/Features/Admin/StockItems/Restock/RestockStockItem.cs
```

- [ ] **Step 2: Change RestockStockItem.Response.cs** to inherit from `StockItemDetailResponse`

```csharp
namespace Module.Inventory.Features.Admin.StockItems.Restock;

public static partial class RestockStockItem
{
    public sealed record Response : StockItemDetailResponse;
}
```

- [ ] **Step 3: Update the Handler** to use `entity.MapToDetail<Response>()` instead of constructing via `RestockResult`.

If the handler returns a `RestockResult` from a service, create a mapping in `StockItem.Mapping.Model.cs` or inline:

```csharp
public static RestockResult ToRestockResult(this Response response) { ... }
```

The exact fix depends on the handler shape — read the handler first.

- [ ] **Step 4: Build and commit**

```bash
dotnet build service/Api/src/Module/Module.csproj
git add service/Api/src/Module/Inventory/Features/Admin/StockItems/Restock/
git commit -m "fix(Inventory): change RestockStockItem Response base from RestockResult to StockItemDetailResponse"

```

---

### Task 10: Fix Inventory GetStockSummary — change from VariantStockSummary to proper base

**Files:**
- Modify: `service/Api/src/Module/Inventory/Features/Admin/StockItems/Summary/GetStockSummary.Response.cs`
- Modify: `service/Api/src/Module/Inventory/Features/Admin/StockItems/Summary/GetStockSummary.cs`

- [ ] **Step 1: Read the handler**

```bash
cat service/Api/src/Module/Inventory/Features/Admin/StockItems/Summary/GetStockSummary.cs
```

- [ ] **Step 2: Change GetStockSummary.Response.cs** to inherit from a new `StockSummaryDetailResponse` base

```csharp
namespace Module.Inventory.Features.Admin.StockItems.Summary;

public static partial class GetStockSummary
{
    public sealed record Response : StockSummaryDetailResponse;
}
```

Add the base to `StockItem.Model.Response.cs`:
```csharp
public record StockSummaryDetailResponse
{
    public Guid VariantId { get; init; }
    public int TotalOnHand { get; init; }
    public int TotalReserved { get; init; }
    public int TotalAvailable { get; init; }
    public List<LocationBreakdown> LocationBreakdown { get; init; } = [];
}
```

- [ ] **Step 3: Update Handler** to use mapping instead of constructing `VariantStockSummary` directly.

- [ ] **Step 4: Build and commit**

```bash
dotnet build service/Api/src/Module/Module.csproj
git add service/Api/src/Module/Inventory/Features/Admin/StockItems/Summary/
git commit -m "fix(Inventory): change GetStockSummary Response base from VariantStockSummary to StockSummaryDetailResponse"

```

---

### Task 11: Fix remaining Catalog handlers — replace manual Response construction

**Files:** All Catalog handlers listed under Category E in the spec that still use `new Response { ... }`.

Pattern for each:

1. Read the handler to identify the domain entity being returned.
2. Find/create `{Entity}.Mapping.Model.cs` in the feature group's `Shared/Mappings/`.
3. Add a `MapToDetail<T>()` or `MapToListItem<T>()` method.
4. Replace `return new Response { ... }` with `return entity.MapToDetail<Response>()`.

- [ ] **Step 1: Fix RepositionTaxon handler** — read, identify domain entity, create mapping, replace.

- [ ] **Step 2: Fix SyncTaxonRules handler** — same pattern.

- [ ] **Step 3: Fix ListVariantsByProduct handler** — same pattern.

- [ ] **Step 4: Fix GetProductClassifications handler** — same pattern.

- [ ] **Step 5: Fix GetVariantOptionValues handler** — same pattern.

- [ ] **Step 6: Fix ListVariantImages handler** — same pattern.

- [ ] **Step 7: Fix DeleteVariantImage handler** — same pattern.

- [ ] **Step 8: Fix SetVariantPrice handler** — same pattern.

- [ ] **Step 9: Fix GetSimilarProducts handler** — same pattern.

- [x] **Step 10: Fix GetImage handler** (already covered in Task 1).

- [ ] **Step 11: Build and run convention checks**

```bash
dotnet build service/Api/src/Module/Module.csproj
bash scripts/check-feature-conventions.sh
```

- [ ] **Step 12: Commit per fix (or batch by feature group)**

```bash
git add service/Api/src/Module/Catalog/
git commit -m "fix(Catalog): replace manual Response construction with mapping methods in handlers"

```

---

### Task 12: Build verification and final check

- [ ] **Step 1: Full build**

```bash
dotnet build
```

Expected: Build passes with zero warnings on the entire solution.

- [ ] **Step 2: Run convention checks**

```bash
bash scripts/check-feature-conventions.sh
```

Expected: All checks PASS.

- [ ] **Step 3: Run unit tests for affected modules**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "Catalog|Inventory"
```

Expected: Tests pass.
