---
title: Catalog Stock Embedding — Variant-Level Availability in Product Responses
version: 1.0
date_created: 2025-08-11
last_updated: 2025-08-11
owner: ReSys.Shop Platform
tags: design, catalog, inventory, stock, api
---

# Introduction

This specification defines embedding per-variant stock availability directly into the Catalog module's product list and product detail responses. The Store SPA currently makes a separate `checkAvailability()` API call per variant — this is eliminated by including stock fields in the product DTO. The Catalog module already calls `IStockItemService.GetStockAvailabilityAsync` internally; only the DTO mapping needs extension.

## 1. Purpose & Scope

### Purpose

Remove the separate `checkAvailability()` API round-trip from the Store SPA by embedding `inStock`, `availableQuantity`, and `backorderable` fields in product list and detail responses. Delete `useAvailability` composable and `availabilityApi.ts`.

### Scope

- Add 3 fields to `StoreVariant.Model.cs` DTO
- Update `Store.Variant.Mapping.cs` to populate from `VariantStockAvailability`
- Remove `Products/Images/Inferences/` — legacy debug endpoint
- Delete Store SPA files: `availabilityApi.ts`, `useAvailability.ts`
- Update `ProductDetailView.vue` and `useProductDetail.ts` to read stock from product DTO

### Out of Scope

- Variant stock DTO for admin (unchanged)
- Product search/filter by stock level (future enhancement)
- Real-time stock updates via WebSocket (future enhancement)

## 2. Definitions

| Term | Definition |
|------|------------|
| **VariantStockAvailability** | Existing service model: `{ VariantId, TotalOnHand, TotalReserved, TotalAvailable, Backorderable, Locations[] }` |
| **StoreVariantResponse** | Catalog's storefront DTO for variant data within product responses |
| **InStock** | `AvailableQuantity > 0 || Backorderable` — product is purchasable |
| **AvailableQuantity** | `TotalOnHand - TotalReserved` — units immediately available |
| **Backorderable** | Boolean — item can be ordered even when out of stock |

## 3. Requirements, Constraints & Guidelines

### DTO Requirements

- **DTO-001**: `StoreVariantResponse` gains `InStock: bool`, `AvailableQuantity: int`, `Backorderable: bool`
- **DTO-002**: Fields appear in both `GET /products` (list) and `GET /products/{id}` (detail) responses
- **DTO-003**: Fields populated from `IStockItemService.GetStockAvailabilityAsync` — no new database queries

### Deletion Requirements

- **DEL-001**: `Catalog/Features/Storefront/Products/Images/Inferences/` — directory deleted (legacy debug endpoint)
- **DEL-002**: `app/Store/src/features/inventory/services/availabilityApi.ts` — deleted
- **DEL-003**: `app/Store/src/features/inventory/composables/useAvailability.ts` — deleted
- **DEL-004**: `CatalogFeature.Storefront.cs` — remove `Images.Inferences.Models.Route` constant

### Store SPA Requirements

- **SPA-001**: `ProductDetailView.vue` reads `variant.availableQuantity` and `variant.backorderable` from product detail response
- **SPA-002**: `useProductDetail.ts` removes `useAvailability()` import — stock is part of product DTO
- **SPA-003**: `StoreVariantStockInfo` TypeScript interface extends to include new fields
- **SPA-004**: Stock meter/severity/message computed properties in `useProductDetail.ts` use DTO fields instead of `useAvailability` cache

## 4. Interfaces & Data Contracts

### 4.1 DTO Changes

```csharp
// Store.Variant.Model.cs — after
public record StoreVariantResponse
{
    public Guid Id { get; init; }
    public string Sku { get; init; }
    public string OptionsText { get; init; }
    // ... existing price, image, option fields ...

    // Stock-embedded fields (populated from IStockItemService)
    public bool InStock { get; init; }
    public int AvailableQuantity { get; init; }
    public bool Backorderable { get; init; }
}
```

### 4.2 Mapping Extension

```csharp
// Store.Variant.Mapping.cs — after
public static StoreVariantResponse MapToStorefront(
    this Variant variant,
    IReadOnlyList<VariantStockAvailability> availabilityList)
{
    var availability = availabilityList
        .FirstOrDefault(a => a.VariantId == variant.Id);

    return new StoreVariantResponse
    {
        // ... existing mappings ...
        InStock = availability?.TotalAvailable > 0 || availability?.Backorderable == true,
        AvailableQuantity = availability?.TotalAvailable ?? 0,
        Backorderable = availability?.Backorderable ?? false,
    };
}
```

### 4.3 TypeScript Interface Changes

```typescript
// app/Store/src/features/catalog/types/product.ts — after
export interface StoreVariantStockInfo {
  availableQuantity: number
  backorderable: boolean
  inStock: boolean  // NEW
}
```

### 4.4 Store SPA Composable Changes

```typescript
// useProductDetail.ts — after
// REMOVE: import { useAvailability } from '@/features/inventory/composables/useAvailability'

const stockLabel = computed(() => {
  const variant = currentVariant.value
  if (!variant) return ''
  if (variant.availableQuantity === 0 && !variant.backorderable)
    return 'Out of stock'
  if (variant.availableQuantity <= 5)
    return `Only ${variant.availableQuantity} left`
  if (variant.backorderable)
    return 'Available for backorder'
  return 'In stock'
})
```

## 5. Acceptance Criteria

- **AC-001**: `GET /api/storefront/catalog/products` response contains `inStock`, `availableQuantity`, `backorderable` on each variant
- **AC-002**: `GET /api/storefront/catalog/products/{id}` response contains the same fields on each variant
- **AC-003**: `availabilityApi.ts` file does not exist
- **AC-004**: `useAvailability.ts` file does not exist
- **AC-005**: Store SPA compiles without `useAvailability` imports
- **AC-006**: Product detail page displays stock message without separate API call
- **AC-007**: `Inferences/` feature directory does not exist in Catalog storefront features
- **AC-008**: `GET /api/storefront/catalog/products/images/inferences` returns 404
- **AC-009**: `pnpm run test:unit` passes in `app/Store/`

## 6. Rationale & Context

### Why embed stock instead of separate API call?

The Store SPA currently calls `checkAvailability(variantId)` for each variant shown on screen. This creates N+1 API calls: 1 product list call + N availability calls. For a page showing 25 products, that's 26 HTTP round-trips. Embedding stock in the product response reduces this to 1 call. The `IStockItemService.GetStockAvailabilityAsync` already batch-queries all variant IDs — the Catalog handler passes `allVariantIds` as a single parameter.

### Why keep the inventory availability endpoint?

`GET /api/storefront/inventory/stock-items/{variantId}/availability` still exists. It serves non-catalog use cases: stock alerts ("notify me when back in stock"), admin low-stock monitoring, POS integration, inventory dashboard widgets. The Catalog module uses the internal service directly — it does not call the REST endpoint.

### Why delete Inferences endpoint?

`GET /api/storefront/catalog/products/images/inferences` returns a list of available ML inference models. This was a debug endpoint from the image search feature development. It exposes internal infrastructure (model names, configuration) to the public storefront. No frontend code references it. Remove it.

### Why PATCH instead of PUT for customer updates — consistency

All 5 customer endpoints that use `PUT` for partial updates are changed to `PATCH`. The pattern is identical to Cart's `PUT → PATCH` migration: the frontend sends partial JSON bodies (`{ city: "NewCity" }`), not full resource replacements. `PATCH` accurately describes the merge semantics.
