# Storefront Products Endpoint Consolidation & Images Fix

**Date:** 2026-07-04
**Status:** Approved

## Problem

The Storefront Products area has endpoints with overlapping purposes and an incorrectly implemented Digitals (images) stub.

### Duplicated Endpoints

| Endpoint | Route | Purpose |
|---|---|---|
| Search | `GET /api/storefront/search` | Text search + pagination |
| Filter | `GET /api/storefront/filter` | Faceted filter + pagination |
| Collections | `GET /api/storefront/collections/{season}` | Products by season taxon name |
| Taxons/Products | `GET /api/storefront/taxons/{id}/products` | Products by taxon ID (Nested Set) |

- **Search + Filter** are two facets of the same operation — list products matching criteria. A real storefront search page combines text search and faceted filters on the same results. Having separate endpoints forces the client to choose one or the other.
- **Collections** matches by taxon name string (fragile — casing, ambiguity) and does NOT traverse the Nested Set tree (only direct children). `Taxons/{id}/products` does it correctly by ID with proper `Lft`/`Rgt` range traversal. The client should resolve a taxon name to an ID, then call `Taxons/{id}/products`.
- **NewArrivals** (`GET /api/storefront/new-arrivals`) has a route constant but no implementation — dead code.

### Incorrect Digitals (Images) Implementation

The `GenerateDownloadLink` handler fabricates a URL without querying any services — no `IApplicationDbContext`, no `IStorageService`. It returns fake data with hardcoded values. The concept is actually "download a `VariantImage` file" — the naming "Digitals" is misleading.

## Design

### 1. Consolidated Endpoints

Merge Search + Filter into a single unified listing endpoint. Drop Collections (superseded by Taxons/Products). Remove the dead NewArrivals route. Rename Digitals to Images.

**Final Storefront endpoints:**

| # | Method | Route | Purpose |
|---|--------|-------|---------|
| 1 | GET | `api/storefront/products/{slug}` | Product detail page |
| 2 | GET | `api/storefront/products` | Unified product listing (search + filter) |
| 3 | GET | `api/storefront/products/{id:guid}/availability` | Variant availability matrix |
| 4 | GET | `api/storefront/products/{id:guid}/related` | Related products |
| 5 | GET | `api/storefront/products/{id:guid}/similar` | Visually similar products |
| 6 | POST | `api/storefront/search-by-image` | Visual search by uploaded image |
| 7 | GET | `api/storefront/taxons/{id:guid}/products` | Products by taxon (Nested Set) |
| 8 | GET | `api/storefront/taxonomies/{id:guid}` | Taxonomy tree |
| 9 | GET | `api/storefront/taxons` | List taxons |
| 10 | GET | `api/storefront/option-types` | Option types for facets |
| 11 | GET | `api/storefront/images/{id:guid}/download` | Download VariantImage file |

### 2. Unified Listing Handler

**Location:** `Products/Get/List/` (new directory)

**Parameters** (all optional, composable):
- `q` (`string?`) — ILIKE on name, slug, description
- `color` (`string?`) — ILIKE on option values of type "Color"
- `size` (`string?`) — ILIKE on option values of type "Size"
- `material` (`string?`) — ILIKE on option values of type "Material"
- `minPrice` (`decimal?`) — minimum variant price
- `maxPrice` (`decimal?`) — maximum variant price
- Plus all standard pagination/sort from `QueryingParameters`

**Response:** Returns `PagedResult<StoreProductListItemResponse>` — same shape used by the existing Filter, Collections, Related, and Taxons/Products endpoints.

**Files:**
```
Products/Get/List/
├── ListProducts.cs              # Query record + unified handler
├── ListProducts.Endpoint.cs     # Carter ICarterModule
├── ListProducts.Parameters.cs   # Combined parameters record
└── ListProducts.Response.cs     # Response record
```

No separate Shared/ directory — the handler reuses the existing `ProductStore.Mapping.cs` and `StoreProductListItemResponse` from `Products/Shared/`.

### 3. Images (formerly Digitals) Fix

**Rename:** `Digitals/` → `Images/`

**Route:** `GET /api/storefront/images/{id:guid}/download`

**Handler rewrite:**
1. Inject `IApplicationDbContext` + `IStorageService`
2. Query `VariantImage` by `Id`, include `Variant`/`Product` to verify it exists
3. Return 404 if not found
4. Call `IStorageService.DownloadAsync(image.StoragePath)` to get file stream
5. Return `Results.File(stream, image.ContentType, image.FileName)` with `Content-Disposition: attachment`

**Deleted:** `GenerateDownloadLink.Response.cs` — endpoint returns a file stream, not JSON.

**Response model (`StoreDigitalDownloadResponse`):** Removed from `Images/Shared/Models/`.

### 4. Files to Delete

| Path | Reason |
|---|---|
| `Products/Get/Search/` (entire directory) | Merged into List |
| `Products/Get/Filter/` (entire directory) | Merged into List |
| `Products/Get/Collections/` (entire directory) | Superseded by Taxons/Products |
| `Digitals/` (entire directory, after rename) | Renamed to Images/ |
| `Images/Get/DownloadLink/GenerateDownloadLink.Response.cs` | No JSON response needed |

### 5. Route Constants Changes

In `CatalogFeature.Storefront.cs`:
- **Remove:** `Products.Get.Search`, `Products.Get.Filter`, `Products.Get.NewArrivals`, `Products.Get.Collections`
- **Add:** `Products.Get.List` with route `$"{Products.BaseRoute}"` (= `api/storefront/products`)
- **Rename:** `Digitals` → `Images`, sub-class `DownloadLink` → `Download`
- **Route:** `$"{Storefront.Route}/images/{id:guid}/download"`

### 6. Tests to Update

Tests referencing removed/moved endpoints:
- `SearchProducts.Tests.cs` → `ListProducts.Tests.cs`
- `FilterProductsByAttributes.Tests.cs` → folded into `ListProducts.Tests.cs`
- `GetCollectionPage.Tests.cs` → removed (no replacement endpoint)
- `GenerateDigitalDownloadLink.Tests.cs` → `DownloadImage.Tests.cs`
- Any HTTP test files referencing old routes (`catalog.http`, `digitals.http`, etc.)

## Error Handling

### Unified List handler
- Invalid pagination params → `parsing.IsFailure` returns error result (existing pattern)
- No results → `PagedResult<Response>.Ok([], 0, 0, 0)` (existing pattern)
- Unhandled exceptions → bubble up to global exception handler

### Images download
- VariantImage not found → 404
- Storage download fails → `IStorageService` returns error result, map to appropriate HTTP status
- No special auth required (consistent with all Storefront endpoints)

## Architecture Notes

- Follows existing vertical slice pattern (partial class + Carter endpoint + MediatR handler)
- No new abstractions or interfaces introduced
- `IStorageService` is already registered in DI; handler injects it directly
- Response type for the unified list matches existing `StoreProductListItemResponse` for consistency with Related, Taxons/Products, etc.
