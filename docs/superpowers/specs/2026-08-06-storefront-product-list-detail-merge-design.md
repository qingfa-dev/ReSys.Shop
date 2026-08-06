# Storefront Product List + Detail Merge Design

**Date:** 2026-08-06
**Status:** Approved
**Scope:** Storefront product catalog — merge Availability into Detail, enrich List with master variant + taxons + stock info

---

## Problem

The storefront has two separate product endpoints:
1. `GetStorefrontProducts` (List) — returns flat product cards with no variant detail, no stock info
2. `GetProductDetail` (Detail) — returns product + variants (no stock status)
3. `GetAvailability` (standalone) — returns stock matrix (axes + cells) but is **never called** by the frontend

The frontend defines `getAvailability` but never imports or uses it. `ProductOptions.vue` renders option buttons with no stock awareness. Customers cannot see which variants are in stock.

## Goal

Unified storefront product endpoints where:
- **List** returns each product with its master variant (images, option values, stock info) and taxon values inline
- **Detail** returns full product details + all variants (each with images, option values, prices, stock info) + taxons with breadcrumbs
- **Availability** endpoint is removed — stock data is embedded per-variant

---

## Design Decisions

### Decision 1: Full Denormalized Fetch (Approach B)

Relationships are embedded inline, not referenced by ID for separate fetches.

**Rationale:**
- Every product card needs image, price, variant info, stock status
- Single request per page load vs 3+ round trips
- List is paginated (20 items/page), payload size is controlled
- Frontend code is simpler — no merge/cache logic needed

### Decision 2: Stock Info — Aggregated Snapshot Only

Expose `StockInfo` per variant (status + availableQuantity + backorderable). No stock locations.

**Rationale:**
- Stock locations are never displayed to customers
- Cart reservation auto-selects locations server-side
- `checkAvailability` per-location endpoint is dead code in the frontend
- Minimum viable info: status for badge, quantity for "Only X left!" urgency

### Decision 3: Option Values as Full Models

Replace `optionValue1`/`optionValue2` with `optionValues: StoreVariantOptionValueResponse[]` containing full option value data + join table ID.

**Rationale:**
- Backend already loads OptionValueVariants → OptionValue → OptionType in the query
- Frontend needs name/presentation to render option buttons
- No extra API call needed
- Flexible for products with 1, 2, or more option dimensions

---

## Response Models

### C# Models

```csharp
// Shared by List and Detail
public record StoreProductVariantResponse : VariantListItemResponse
{
    public List<StoreVariantOptionValueResponse> OptionValues { get; init; } = [];
    public List<StoreVariantImageResponse> Images { get; init; } = [];
    public List<StoreVariantPriceResponse> Prices { get; init; } = [];
    public StoreVariantStockInfo Stock { get; init; } = new();
}

public record StoreVariantOptionValueResponse
{
    public Guid VariantOptionValueId { get; init; }  // PK of OptionValueVariant join table
    public Guid OptionValueId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Presentation { get; init; }
    public int Position { get; init; }
    public Guid OptionTypeId { get; init; }
    public string? OptionTypeName { get; init; }
}

public record StoreVariantStockInfo
{
    public string Status { get; init; } = "unknown";  // in_stock | low_stock | backorderable | out_of_stock
    public int AvailableQuantity { get; init; }
    public bool Backorderable { get; init; }
}

// List response — enriched with master variant + taxons
public record StoreProductListItemResponse : ProductListItemResponse
{
    public StoreProductVariantResponse? MasterVariant { get; init; }
    public List<StoreProductTaxonResponse> Taxons { get; init; } = [];
}

// Detail response — full product + all variants + taxons
public record StoreProductDetailResponse : ProductDetailResponse
{
    public StoreProductVariantResponse? MasterVariant { get; init; }
    public List<StoreProductVariantResponse> Variants { get; init; } = [];
    public List<StoreProductTaxonResponse> Taxons { get; init; } = [];
}
```

### TypeScript Types

```typescript
export interface StoreVariantStockInfo {
  status: 'in_stock' | 'low_stock' | 'backorderable' | 'out_of_stock' | 'unknown'
  availableQuantity: number
  backorderable: boolean
}

export interface StoreVariantOptionValueResponse {
  variantOptionValueId: string
  optionValueId: string
  name: string
  presentation: string | null
  position: number
  optionTypeId: string
  optionTypeName: string | null
}

export interface StoreProductVariantResponse {
  id: string
  sku: string | null
  isMaster: boolean
  price: number | null
  currency: string | null
  optionValues: StoreVariantOptionValueResponse[]
  images: StoreProductImageResponse[]
  stock: StoreVariantStockInfo
}

export interface StoreProductListItemResponse {
  id: string
  masterVariantId: string
  name: string
  slug: string
  description: string | null
  status: string
  minPrice: number | null
  currency: string | null
  thumbnailUrl: string | null
  thumbnailAlt: string | null
  styleCode: string | null
  seasonName: string | null
  department: string | null
  genderTarget: string | null
  variantsCount: number
  availableOn: string | null
  masterVariant: StoreProductVariantResponse | null
  taxons: StoreProductTaxonResponse[]
}

export interface StoreProductDetailResponse extends StoreProductListItemResponse {
  masterVariant: StoreProductVariantResponse | null
  variants: StoreProductVariantResponse[]
  taxons: StoreProductTaxonResponse[]
}
```

---

## JSON Schemas

### List Response

```json
{
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "masterVariantId": "660e8400-e29b-41d4-a716-446655440001",
      "name": "Classic Cotton Tee",
      "slug": "classic-cotton-tee",
      "description": "Premium cotton t-shirt",
      "status": "Active",
      "minPrice": 299000,
      "currency": "VND",
      "thumbnailUrl": "https://cdn.example.com/products/tee-thumb.jpg",
      "thumbnailAlt": "Classic Cotton Tee front view",
      "styleCode": "CT-001",
      "seasonName": "Summer 2026",
      "department": "Top",
      "genderTarget": "Unisex",
      "availableOn": "2026-06-01T00:00:00Z",
      "variantsCount": 8,
      "classificationsCount": 2,
      "createdAtUtc": "2026-05-15T10:30:00Z",
      "modifiedAtUtc": "2026-06-01T08:00:00Z",

      "masterVariant": {
        "id": "660e8400-e29b-41d4-a716-446655440001",
        "sku": "CT-BLK-M",
        "isMaster": true,
        "position": 0,
        "trackInventory": true,
        "price": 299000,
        "currency": "VND",
        "costPrice": null,
        "costCurrency": null,
        "weight": null,
        "weightUnit": null,
        "height": null,
        "width": null,
        "depth": null,
        "dimensionsUnit": null,
        "stock": {
          "status": "in_stock",
          "availableQuantity": 47,
          "backorderable": false
        },
        "images": [
          {
            "id": "img-001",
            "url": "https://cdn.example.com/products/tee-black-m-front.jpg",
            "alt": "Front view",
            "position": 0,
            "contentType": "image/jpeg"
          }
        ],
        "optionValues": [
          {
            "variantOptionValueId": "ovv-001",
            "optionValueId": "ov-black",
            "name": "Black",
            "presentation": "Black",
            "position": 0,
            "optionTypeId": "ot-color",
            "optionTypeName": "Color"
          },
          {
            "variantOptionValueId": "ovv-002",
            "optionValueId": "ov-m",
            "name": "M",
            "presentation": "Medium",
            "position": 1,
            "optionTypeId": "ot-size",
            "optionTypeName": "Size"
          }
        ],
        "prices": [
          { "id": "p-001", "amount": 299000, "currency": "VND" }
        ]
      },

      "taxons": [
        {
          "id": "t1-0001",
          "name": "T-Shirts",
          "permalink": "/t-shirts",
          "depth": 1,
          "breadcrumb": [
            { "id": "t0-0001", "name": "Clothing", "permalink": "/clothing" },
            { "id": "t1-0001", "name": "T-Shirts", "permalink": "/t-shirts" }
          ]
        }
      ]
    }
  ],
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 120,
  "facets": {
    "groups": [
      {
        "name": "Color",
        "values": [
          { "id": "ov-black", "label": "Black", "count": 45, "isActive": false },
          { "id": "ov-white", "label": "White", "count": 38, "isActive": false }
        ]
      },
      {
        "name": "Category",
        "values": [
          { "id": "t1-0001", "label": "T-Shirts", "count": 60, "isActive": false }
        ]
      }
    ]
  }
}
```

### Detail Response

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Classic Cotton Tee",
  "slug": "classic-cotton-tee",
  "description": "Premium cotton t-shirt",
  "status": "Active",
  "styleCode": "CT-001",
  "seasonName": "Summer 2026",
  "materialComposition": "100% Cotton",
  "careInstructions": "Machine wash cold, tumble dry low",
  "fitNotes": "Regular fit, true to size",
  "department": "Top",
  "genderTarget": "Unisex",
  "availableOn": "2026-06-01T00:00:00Z",
  "discontinueOn": null,
  "makeActiveAt": null,
  "metaTitle": "Classic Cotton Tee | ReSys Shop",
  "metaDescription": "Premium cotton t-shirt for everyday wear",
  "metaKeywords": "cotton, t-shirt, casual",
  "masterVariantId": "660e8400-e29b-41d4-a716-446655440001",

  "masterVariant": {
    "id": "660e8400-e29b-41d4-a716-446655440001",
    "sku": "CT-BLK-M",
    "isMaster": true,
    "position": 0,
    "trackInventory": true,
    "price": 299000,
    "currency": "VND",
    "costPrice": 150000,
    "costCurrency": "VND",
    "weight": 200,
    "weightUnit": "g",
    "height": null,
    "width": null,
    "depth": null,
    "dimensionsUnit": null,
    "stock": {
      "status": "in_stock",
      "availableQuantity": 47,
      "backorderable": false
    },
    "images": [
      { "id": "img-001", "url": "https://cdn.example.com/products/tee-black-m-front.jpg", "alt": "Front view", "position": 0, "contentType": "image/jpeg" },
      { "id": "img-002", "url": "https://cdn.example.com/products/tee-black-m-back.jpg", "alt": "Back view", "position": 1, "contentType": "image/jpeg" }
    ],
    "optionValues": [
      { "variantOptionValueId": "ovv-001", "optionValueId": "ov-black", "name": "Black", "presentation": "Black", "position": 0, "optionTypeId": "ot-color", "optionTypeName": "Color" },
      { "variantOptionValueId": "ovv-002", "optionValueId": "ov-m", "name": "M", "presentation": "Medium", "position": 1, "optionTypeId": "ot-size", "optionTypeName": "Size" }
    ],
    "prices": [
      { "id": "p-001", "amount": 299000, "currency": "VND" }
    ]
  },

  "variants": [
    {
      "id": "v-001",
      "sku": "CT-BLK-S",
      "isMaster": false,
      "position": 1,
      "trackInventory": true,
      "price": 299000,
      "currency": "VND",
      "costPrice": 150000,
      "costCurrency": "VND",
      "weight": 190,
      "weightUnit": "g",
      "stock": {
        "status": "in_stock",
        "availableQuantity": 32,
        "backorderable": false
      },
      "images": [
        { "id": "img-010", "url": "https://cdn.example.com/products/tee-black-s.jpg", "alt": "Black Small", "position": 0, "contentType": "image/jpeg" }
      ],
      "optionValues": [
        { "variantOptionValueId": "ovv-010", "optionValueId": "ov-black", "name": "Black", "presentation": "Black", "position": 0, "optionTypeId": "ot-color", "optionTypeName": "Color" },
        { "variantOptionValueId": "ovv-011", "optionValueId": "ov-s", "name": "S", "presentation": "Small", "position": 1, "optionTypeId": "ot-size", "optionTypeName": "Size" }
      ],
      "prices": [
        { "id": "p-010", "amount": 299000, "currency": "VND" }
      ]
    },
    {
      "id": "v-003",
      "sku": "CT-BLK-L",
      "isMaster": false,
      "position": 3,
      "trackInventory": true,
      "price": 299000,
      "currency": "VND",
      "stock": {
        "status": "low_stock",
        "availableQuantity": 3,
        "backorderable": false
      },
      "images": [],
      "optionValues": [
        { "variantOptionValueId": "ovv-030", "optionValueId": "ov-black", "name": "Black", "presentation": "Black", "position": 0, "optionTypeId": "ot-color", "optionTypeName": "Color" },
        { "variantOptionValueId": "ovv-031", "optionValueId": "ov-l", "name": "L", "presentation": "Large", "position": 1, "optionTypeId": "ot-size", "optionTypeName": "Size" }
      ],
      "prices": [
        { "id": "p-030", "amount": 299000, "currency": "VND" }
      ]
    },
    {
      "id": "v-004",
      "sku": "CT-BLK-XL",
      "isMaster": false,
      "position": 4,
      "trackInventory": true,
      "price": 299000,
      "currency": "VND",
      "stock": {
        "status": "out_of_stock",
        "availableQuantity": 0,
        "backorderable": false
      },
      "images": [],
      "optionValues": [
        { "variantOptionValueId": "ovv-040", "optionValueId": "ov-black", "name": "Black", "presentation": "Black", "position": 0, "optionTypeId": "ot-color", "optionTypeName": "Color" },
        { "variantOptionValueId": "ovv-041", "optionValueId": "ov-xl", "name": "XL", "presentation": "X-Large", "position": 1, "optionTypeId": "ot-size", "optionTypeName": "Size" }
      ],
      "prices": [
        { "id": "p-040", "amount": 299000, "currency": "VND" }
      ]
    },
    {
      "id": "v-005",
      "sku": "CT-WHT-S",
      "isMaster": false,
      "position": 5,
      "trackInventory": true,
      "price": 299000,
      "currency": "VND",
      "stock": {
        "status": "backorderable",
        "availableQuantity": 0,
        "backorderable": true
      },
      "images": [],
      "optionValues": [
        { "variantOptionValueId": "ovv-050", "optionValueId": "ov-white", "name": "White", "presentation": "White", "position": 0, "optionTypeId": "ot-color", "optionTypeName": "Color" },
        { "variantOptionValueId": "ovv-051", "optionValueId": "ov-s", "name": "S", "presentation": "Small", "position": 1, "optionTypeId": "ot-size", "optionTypeName": "Size" }
      ],
      "prices": [
        { "id": "p-050", "amount": 299000, "currency": "VND" }
      ]
    }
  ],

  "taxons": [
    {
      "id": "t1-0001",
      "name": "T-Shirts",
      "permalink": "/t-shirts",
      "depth": 1,
      "breadcrumb": [
        { "id": "t0-0001", "name": "Clothing", "permalink": "/clothing" },
        { "id": "t1-0001", "name": "T-Shirts", "permalink": "/t-shirts" }
      ]
    },
    {
      "id": "t2-0003",
      "name": "Summer Collection",
      "permalink": "/collections/summer",
      "depth": 2,
      "breadcrumb": [
        { "id": "t0-0002", "name": "Collections", "permalink": "/collections" },
        { "id": "t1-0003", "name": "Seasonal", "permalink": "/collections/seasonal" },
        { "id": "t2-0003", "name": "Summer Collection", "permalink": "/collections/summer" }
      ]
    }
  ],

  "createdAtUtc": "2026-05-15T10:30:00Z",
  "modifiedAtUtc": "2026-06-01T08:00:00Z"
}
```

---

## Query Strategy

### List Query (`GetStorefrontProducts`)

**Current includes** (already loaded):
- `Variants` → `Prices`, `VariantImages`, `OptionValueVariants` → `OptionValue` → `OptionType`
- `Classifications` → `Taxon`

**Add:**
- Batch stock lookup: `GetAvailableByVariantAsync(masterVariantIds)` + `GetBackorderableByVariantAsync(masterVariantIds)`
- Compute `StockInfo` per product's master variant
- Compute taxon breadcrumbs (reuse logic from Detail handler)

**Query flow:**
1. Load products with all includes (existing)
2. Apply filters (existing)
3. Compute facets (existing)
4. ToPagedOrAll → project to list items (existing)
5. **NEW:** Extract master variant IDs from paged items
6. **NEW:** Batch call `GetAvailableByVariantAsync` + `GetBackorderableByVariantAsync`
7. **NEW:** Load all taxons once, build breadcrumb lookup
8. **NEW:** Attach `StockInfo` + `TaxonBreadcrumb` to each list item

**Key optimization:** 2 batch inventory calls + 1 taxon query regardless of page size.

### Detail Query (`GetProductDetail`)

**Current includes** (already loaded):
- `Variants` → `Prices`, `VariantImages`, `OptionValueVariants` → `OptionValue` → `OptionType`
- `Classifications` → `Taxon`

**Add:**
- Batch stock lookup: `GetAvailableByVariantAsync(allVariantIds)` + `GetBackorderableByVariantAsync(allVariantIds)`
- Compute `StockInfo` per variant
- Compute taxon breadcrumbs (existing logic)

**Query flow:**
1. Load product with all includes (existing)
2. **NEW:** Extract all variant IDs
3. **NEW:** Batch call `GetAvailableByVariantAsync` + `GetBackorderableByVariantAsync`
4. **NEW:** Attach `StockInfo` to each variant
5. Compute taxon breadcrumbs (existing)

---

## Stock Status Computation

```csharp
private static StoreVariantStockInfo ComputeStockInfo(
    int available,
    bool backorderable,
    int lowStockThreshold = 5)
{
    var status = available switch
    {
        > LowStockThreshold.Default => "in_stock",
        > 0 => "low_stock",
        _ when backorderable => "backorderable",
        _ => "out_of_stock"
    };

    return new StoreVariantStockInfo
    {
        Status = status,
        AvailableQuantity = available,
        Backorderable = backorderable,
    };
}
```

**Frontend usage:**
- `status` → color badge (green/yellow/red/gray)
- `availableQuantity <= 5 && status === 'low_stock'` → "Only X left!" urgency text
- `status === 'out_of_stock' && backorderable` → "Available for backorder" badge

---

## File Changes

### Backend

| Action | File | What |
|--------|------|------|
| Modify | `Store.Variant.Model.cs` | Replace option value fields with `StoreVariantOptionValueResponse` list, add `StoreVariantStockInfo` |
| Modify | `Store.Product.Model.cs` | Add `MasterVariant` + `Taxons` to list response |
| Modify | `Store.Variant.Mapping.cs` | Map `OptionValueVariant` → `StoreVariantOptionValueResponse` with join table ID |
| Modify | `Store.Product.Mapping.cs` | Map master variant + taxons for list item |
| Modify | `GetStorefrontProducts.cs` | Inject `IStockAvailabilityCalculator`, batch stock lookup, attach to list items, compute taxon breadcrumbs |
| Modify | `GetProductDetail.cs` | Inject `IStockAvailabilityCalculator`, attach stock info to each variant |
| Modify | `CatalogFeature.Storefront.cs` | Remove `Availability` constants |
| Delete | `Availability/GetAvailability.cs` | Removed |
| Delete | `Availability/GetAvailability.Endpoint.cs` | Removed |
| Delete | `Availability/GetAvailability.Response.cs` | Removed |
| Delete | `Store.Availability.Model.cs` | All availability models removed |

### Frontend

| Action | File | What |
|--------|------|------|
| Modify | `product.ts` (Store) | Update types: add `StoreVariantStockInfo`, `StoreVariantOptionValueResponse`, update `StoreProductVariantResponse`, add `MasterVariant`/`Taxons` to list type, remove availability types |
| Modify | `productApi.ts` (Store) | Remove `getAvailability` function, remove `AvailabilityMatrixResponse` import |
| Modify | `api.ts` (Store) | Remove `productAvailability` endpoint constant |
| Modify | `ProductCard.vue` | Use `masterVariant.stock` for stock badge display |
| Modify | `ProductOptions.vue` | Use `optionValues` array instead of `optionValue1`/`optionValue2`, show stock status per option |
| Modify | `ProductDetailView.vue` | Use `product.variants` with stock info for variant picker |

---

## Acceptance Criteria

1. `GET /api/storefront/products` returns each product with `masterVariant` (images, optionValues, stock) and `taxons` inline
2. `GET /api/storefront/products/{slug}` returns full product detail with all variants (each with stock info) and taxons with breadcrumbs
3. `GET /api/storefront/products/availability` returns 404 (endpoint removed)
4. Frontend `ProductCard` shows stock badge based on `masterVariant.stock.status`
5. Frontend `ProductOptions` shows stock status per option button
6. Frontend `ProductDetailView` shows variant stock status in the variant picker
7. `dotnet build` passes with no warnings
8. All existing unit tests pass
9. Frontend lint passes for both Admin and Store apps
