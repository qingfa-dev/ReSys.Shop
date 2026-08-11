# Catalog Stock Embedding Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Embed `inStock`, `availableQuantity`, `backorderable` fields in product list and detail DTOs. Delete `useAvailability` composable and `availabilityApi.ts`. Remove legacy inferences debug endpoint.

**Architecture:** Catalog already calls `IStockItemService.GetStockAvailabilityAsync` for both `GetStorefrontProducts` (list) and `GetProductDetail` (by ID). Extend DTO mapping to populate 3 new stock fields. No new database queries. Store SPA reads stock from product/variant response.

**Tech Stack:** .NET 10, C#, TypeScript 6, Vue 3, pnpm, Vitest

## Global Constraints

- .NET 10, TreatWarningsAsErrors=true
- DTO fields must use init-only setters (`{ get; init; }`)
- Mapping extensions in `Shared/Mappings/` files
- `pnpm run lint && pnpm run test:unit` must pass

---

### Task 1: Add Stock Fields to Catalog DTO

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/Shared/Models/Store.Variant.Model.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/Shared/Mappings/Store.Variant.Mapping.cs`

- [ ] **Step 1: Add stock fields to StoreVariantResponse**

Read the current variant model:

```bash
rg "public record Store" service/Api/src/Module/Catalog/Features/Storefront/Products/Shared/Models/Store.Variant.Model.cs
```

Add these fields to the record:

```csharp
// Stock embedded — populated from IStockItemService.GetStockAvailabilityAsync
public bool InStock { get; init; }
public int AvailableQuantity { get; init; }
public bool Backorderable { get; init; }
```

- [ ] **Step 2: Update variant mapping to populate stock**

In `Store.Variant.Mapping.cs`, the mapping method likely receives variant + product data. Add parameter for `IReadOnlyList<VariantStockAvailability>` or accept it via overload.

Find the mapping method called by both `GetStorefrontProducts` handler and `GetProductDetail` handler. Add stock field population:

```csharp
var availability = availabilityList?.FirstOrDefault(a => a.VariantId == variant.Id);
result.InStock = (availability?.TotalAvailable ?? 0) > 0 || (availability?.Backorderable ?? false);
result.AvailableQuantity = availability?.TotalAvailable ?? 0;
result.Backorderable = availability?.Backorderable ?? false;
```

- [ ] **Step 3: Build**

```bash
dotnet build
```

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Storefront/Products/Shared/
git commit -m "feat(catalog): add InStock, AvailableQuantity, Backorderable to variant DTO

Embedded in both product list and detail responses. Populated from
IStockItemService.GetStockAvailabilityAsync which both handlers already call."
```

### Task 2: Remove Legacy Inferences Endpoint

**Files:**
- Delete: `service/Api/src/Module/Catalog/Features/Storefront/Products/Images/Inferences/` (entire directory)
- Modify: `service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Storefront.cs` (remove inferences route)

- [ ] **Step 1: Delete directory**

```bash
rm -rf service/Api/src/Module/Catalog/Features/Storefront/Products/Images/Inferences/
```

- [ ] **Step 2: Remove route constant**

In `CatalogFeature.Storefront.cs`, remove the class block defining `Images.Inferences.Models.Route`.

- [ ] **Step 3: Build**

```bash
dotnet build
```

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "refactor(catalog): remove legacy inferences debug endpoint

GET /products/images/inferences exposed ML model configs publicly.
No frontend references it. Pruned."
```

### Task 3: Delete Store SPA Availability Files

**Files:**
- Delete: `app/Store/src/features/inventory/services/availabilityApi.ts`
- Delete: `app/Store/src/features/inventory/composables/useAvailability.ts`
- Modify: `app/Store/src/features/inventory/types/availability.ts` (remove `AvailabilityEntry`, keep `CartReservation`/`CartReservationStatus`)
- Modify: `app/Store/src/features/inventory/services/index.ts` (remove export)
- Modify: `app/Store/src/features/inventory/index.ts` (remove export)

- [ ] **Step 1: Delete files**

```bash
rm app/Store/src/features/inventory/services/availabilityApi.ts
rm app/Store/src/features/inventory/composables/useAvailability.ts
```

- [ ] **Step 2: Remove from barrel exports**

Remove `availabilityApi` export from `services/index.ts`.
Remove `useAvailability` export from `index.ts`.

- [ ] **Step 3: Prune AvailabilityEntry from types**

In `availability.ts`, remove the `AvailabilityEntry` interface. Keep `ReserveStockRequest`, `CartReservation`, `CartReservationStatus`.

- [ ] **Step 4: Commit**

```bash
git add app/Store/src/features/inventory/
git commit -m "refactor(store-spa): delete useAvailability composable and availability API

Stock data now embedded in product DTO responses. Separate
availability API call no longer needed."
```

### Task 4: Update Store SPA to Read Stock from Product DTO

**Files:**
- Modify: `app/Store/src/features/catalog/types/product.ts`
- Modify: `app/Store/src/features/catalog/composables/useProductDetail.ts`
- Modify: `app/Store/src/features/catalog/views/ProductDetailView.vue`

- [ ] **Step 1: Add stock fields to TypeScript type**

```typescript
// product.ts — add to StoreVariantStockInfo
export interface StoreVariantStockInfo {
  availableQuantity: number
  backorderable: boolean
  inStock: boolean
}
```

- [ ] **Step 2: Update useProductDetail composable**

Remove `import { useAvailability } from '@/features/inventory/composables/useAvailability'`.

Replace stock label computation to read from variant DTO:

```typescript
const stockLabel = computed(() => {
  const variant = currentVariant.value
  if (!variant) return ''
  if (variant.stockInfo?.availableQuantity === 0 && !variant.stockInfo?.backorderable)
    return 'Out of stock'
  if ((variant.stockInfo?.availableQuantity ?? 0) <= 5)
    return `Only ${variant.stockInfo?.availableQuantity} left`
  if (variant.stockInfo?.backorderable)
    return 'Available for backorder'
  return 'In stock'
})
```

- [ ] **Step 3: Update ProductDetailView.vue**

Remove `useAvailability` usage. Display stock from `variant.stockInfo` directly:

```vue
<!-- Section: Stock Status -->
<div v-if="currentVariant?.stockInfo?.availableQuantity != null">
  {{ stockLabel }}
</div>
```

- [ ] **Step 4: Build + test SPA**

```bash
cd app/Store
pnpm run lint
pnpm run test:unit
pnpm run build
```

- [ ] **Step 5: Commit**

```bash
git add app/Store/src/features/catalog/
git commit -m "refactor(store-spa): read stock from product DTO, drop availability API

ProductDetailView now reads variant.availableQuantity and
variant.backorderable from the product detail response.
Eliminates N+1 API calls (separate availability check per variant)."
```
