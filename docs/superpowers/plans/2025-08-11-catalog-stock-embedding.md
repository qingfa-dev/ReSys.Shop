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

### Task 1: Verify Stock Already Embedded — Align DTOs if Needed

**Files:**
- Read: `service/Api/src/Module/Catalog/Features/Storefront/Products/Shared/Models/Store.Variant.Model.cs`
- Read: `service/Api/src/Module/Catalog/Features/Storefront/Products/Shared/Mappings/Store.Variant.Mapping.cs`

**FINDING (pre-flight review):** Stock is ALREADY embedded in both list and detail responses. `StoreProductVariantResponse` has a nested `Stock` property of type `StoreVariantStockInfo` (TotalOnHand/TotalReserved/TotalAvailable/Backorderable/Locations), populated by `MapToStockInfo()` in both `GetProductDetail.cs:71` and `GetStorefrontProducts.cs:178`. Both handlers already call `GetStockAvailabilityAsync`. **Do NOT add flat `InStock/AvailableQuantity/Backorderable` fields to the variant record** — that would duplicate the existing `Stock` object and diverge the API contract from the SPA.

- [ ] **Step 1: Verify the existing contract**

```bash
rg "MapToStockInfo|Stock = " service/Api/src/Module/Catalog/Features/Storefront/Products/Get/ById/GetProductDetail.cs service/Api/src/Module/Catalog/Features/Storefront/Products/Get/PagedOrAll/GetStorefrontProducts.cs
```

Expected: both handlers populate `Stock` on the variant DTO. The backend half of this feature is complete — no C# changes required in this task. If a handler is found that does NOT populate `Stock`, add the `GetStockAvailabilityAsync` + `MapToStockInfo` block exactly as the other handler does.

- [ ] **Step 2: Build (confirms nothing broke)**

```bash
dotnet build
```

- [ ] **Step 3: Commit**

```bash
git commit --allow-empty -m "verify(catalog): confirm stock already embedded in list + detail DTOs

StoreProductVariantResponse.Stock is populated by MapToStockInfo in both
GetProductDetail and GetStorefrontProducts handlers. No backend changes
needed — SPA-side cleanup is the remaining work."
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

**FINDING (pre-flight review):** The backend `StoreVariantStockInfo` (C#) serializes as `totalOnHand`, `totalReserved`, `totalAvailable`, `backorderable`, `locations` (camelCase). The current TS type has `availableQuantity`/`backorderable` — `availableQuantity` does NOT match the backend. Fix the TS type to mirror the C# DTO exactly, then read `totalAvailable`.

- [ ] **Step 1: Fix TypeScript type to mirror backend DTO**

```typescript
// product.ts — StoreVariantStockInfo mirrors Module.Catalog StoreVariantStockInfo
export interface StoreVariantStockInfo {
  totalOnHand: number
  totalReserved: number
  totalAvailable: number
  backorderable: boolean
  locations: StoreStockLocationInfo[]
}

export interface StoreStockLocationInfo {
  stockLocationId: string
  stockLocationName: string | null
  countOnHand: number
  reservedCount: number
  availableCount: number
  backorderable: boolean
}
```

- [ ] **Step 2: Update useProductDetail composable**

Remove `import { useAvailability } from '@/features/inventory/composables/useAvailability'` (check current import source — ProductDetailView imports it via `@/features/inventory/composables` barrel; ensure the barrel export is removed in Task 3).

Replace stock label computation to read from variant DTO (uses `totalAvailable`):

```typescript
const stockLabel = computed(() => {
  const stock = selectedVariant.value?.stock
  if (!stock) return null
  if (stock.totalAvailable > 5) return null
  if (stock.totalAvailable > 0) return `Only ${stock.totalAvailable} left`
  if (stock.backorderable) return 'Available for backorder'
  return 'Out of stock'
})
```

The composable already reads `selectedVariant.value?.stock.availableQuantity` — rename those reads to `totalAvailable` and remove the `useAvailability` import if present.

- [ ] **Step 3: Update ProductDetailView.vue**

Remove `import { useAvailability } from '@/features/inventory/composables'` and the `const availability = useAvailability()` line. Display stock from `selectedVariant?.stock` directly:

```vue
<!-- Section: Stock Status -->
<div v-if="stockLabel">
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

ProductDetailView reads variant.stock.totalAvailable and
variant.stock.backorderable from the product detail response. TS type
aligned to the actual backend StoreVariantStockInfo contract. Eliminates
separate availability API call."
```
