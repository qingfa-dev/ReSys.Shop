# Design: Standardize list responses to the `PagedResult` envelope

Date: 2026-07-31

Status: Approved (in review)

## Goal

Every API endpoint that returns a collection must return the `PagedResult<T>` envelope.
`PagedResult` is the standardized response envelope for list-shaped values; returning it does
not force server-side paging. Endpoints that are not semantically pageable return all items in
a single page when no paging parameters are supplied, and honor real paging when they are.

This removes the current inconsistency where 17 list endpoints return bare
`Result<List<Response>>` or a single-object envelope wrapping one list, while ~30 other
endpoints already return `PagedResult<T>`.

## Non-Goals

- No new paging infrastructure. The repo already has the full mechanism (`IPagedQuery`,
  `IPagedQueryHandler`, `PageModel`, `ToPagedOrAllAsync`, in-memory `ToPagedResult`).
- No new envelope type. `PagedResult<T>` is the only collection envelope.
- No behavior change for callers that do not pass paging parameters: they keep receiving all
  items, just wrapped in the standard envelope.
- Dashboard aggregates, the taxonomy tree, and the availability matrix are single-object
  responses (multi-list widgets / hierarchy / matrix), not "return by list" endpoints. They are
  out of scope.

## Approach

Align every list-returning endpoint with the existing `PagedOrAll` convention already used by
`GetStatePagedOrAll`, `GetWishlists`, `GetCountryPagedOrAll`, and `GetAddresses`.

### Target shape per feature

```
Query(Parameters) : IPagedQuery<Response>               // was IQuery<List<Response>> or IQuery<Response-envelope>
PagedQueryHandler : IPagedQueryHandler<Query, Response> // returns PagedResult<Response>
  - build PageModel from nullable page/pageSize (PageModelExtensions.FromValues)
  - EF-backed: dbContext.Set<T>().Where(...).ToPagedOrAllAsync(m => m.MapToItem<Response>(), page, ct)
  - in-memory: compute the full list, then in-memory ToPagedResult honoring page/pageSize
Endpoint → result.ToPagedResult(), .Produces<PagedResult<Response>>()
```

Semantics:

- No `page`/`pageSize` query params → `PageModel.IsEmpty == true` → return **all** items in
  `PagedResult` (page=1, pageSize=totalCount, totalCount=totalCount).
- `page`/`pageSize` provided → real paging with bounds clamped by `PageBounds`.

## Endpoint inventory

### Tier 1 — flatten directly (currently bare `Result<List<Response>>`)

| Endpoint | Current shape | Target |
|---|---|---|
| Inventory `Admin/StockItems/GetAll/GetAllStockItems` | `Result<List<Response>>` | `PagedResult<Response>` |
| Inventory `Admin/StockItems/Summary/GetStockSummary` | `Result<List<Response>>` | `PagedResult<Response>` |
| Inventory `Admin/StockItems/LowStock/GetLowStockItems` | `Result<List<Response>>` | `PagedResult<Response>` |
| Inventory `Storefront/CartReservations/Status/GetCartReservations` | `Result<List<Response>>` | `PagedResult<Response>` |

### Tier 2 — envelope holds one list → flatten envelope, return `PagedResult<Item>`

| Endpoint | Current envelope | Target item type |
|---|---|---|
| Catalog `Storefront/Products/Get/Similar/GetSimilarProducts` | `Response.Items: List<SimilarProductItem>` | `PagedResult<SimilarProductItem>` |
| Catalog `Storefront/Products/SearchByImage` | `Response.Items: List<SearchResultItem>` | `PagedResult<SearchResultItem>` |
| Catalog `Admin/Products/Variants/Images/ListByVariant` | `Response.Images: List<VariantImageDetailResponse>` | `PagedResult<VariantImageDetailResponse>` |
| Catalog `Admin/Products/Variants/List/ListVariantsByProduct` | `Response.Items: List<Item>` | `PagedResult<Item>` |
| Catalog `Admin/Products/OptionTypes/Get/GetProductOptionTypes` | `Response.Items: List<OptionTypeItem>` | `PagedResult<OptionTypeItem>` |
| Catalog `Admin/Products/Classifications/Get/GetProductClassifications` | `Response.Items: List<ClassificationItem>` | `PagedResult<ClassificationItem>` |
| Catalog `Admin/Products/Variants/OptionValues/Get/GetVariantOptionValues` | `Response.Items: List<OptionValueItem>` | `PagedResult<OptionValueItem>` |
| Catalog `Admin/Taxonomies/Taxons/Rules/Sync/SyncTaxonRules` (POST) | `Response.Rules: List<TaxonRuleItem>` | `PagedResult<TaxonRuleItem>` |
| Identity `Admin/Users/Roles/Get/GetUserRoles` | `Response.Roles: List<RoleItemResponse>` | `PagedResult<RoleItemResponse>` |
| Shipping `Storefront/Shipping/Methods/GetShippingMethods` | `Response.Methods: List<ShippingMethodDto>` | `PagedResult<ShippingMethodDto>` |
| Inventory `Storefront/StockAvailability/Check/GetStockAvailability` | `Response.LocationAvailability: List<LocationAvailability>` | `PagedResult<LocationAvailability>` |

### Tier 3 — single-object responses, out of scope (unchanged)

- Catalog `Storefront/Taxonomies/Get/Tree` — hierarchical tree.
- Catalog `Storefront/Products/Get/Availability` — matrix of `Axes` + `Cells`.
- Dashboard endpoints: `Dashboard/Admin/Get`, `Catalog/Admin/Dashboard/Get`,
  `Inventory/Admin/Dashboard/Get`, `Ordering/Admin/Dashboard/Get` — multi-list widgets.
- `Shipping/Storefront/Shipping/Calculate` — single computed value (not a collection).

## Per-feature conversion steps

1. **Query**: change marker interface to `IPagedQuery<TItem>`; constructor takes the feature's
   `Parameters` (derived from `QueryingParameters` or a minimal `IPagingParameters` with nullable
   `PageNumber`/`PageSize`).
2. **Handler**: rename to `PagedQueryHandler` implementing `IPagedQueryHandler<Query, TItem>`.
   Build `PageModel` via `PageModelExtensions.FromValues(parameters.PageNumber, parameters.PageSize,
   pageBounds)`; apply to the EF query with `ToPagedOrAllAsync` (projection to item type) or, for
   in-memory computation (summary, cart reservations, search-by-image, similar, stock availability),
   page the computed list with the in-memory `ToPagedResult`.
3. **Response**: delete the envelope record; promote its item record(s) to the feature `Response`
   (or keep a dedicated item type where the payload differs).
4. **Endpoint**: `return result.ToPagedResult();` and `.Produces<PagedResult<TItem>>()`.
5. **Parameters**: add `Parameters` record implementing paging input where absent.
6. **Validator**: validate `PageSize` bounds (mirror `GetStatePagedOrAll.Validator`).
7. **Mapping**: existing item mappers are reused; only the wrapper projection changes.

## Consumers (in scope)

Response JSON changes shape for converted endpoints:
`{ value: { items: [...] }, isSuccess, ... }` becomes `{ items: [...], page, pageSize, totalCount,
isSuccess, ... }`.

- Update `ApiTests/*.http` files that exercise converted endpoints.
- Update Vue `app/Admin` and `app/Store` API services and their unit tests that consume converted
  endpoints.
- Both SPAs already page via the shared list-view/paging patterns; alignment should reduce
  per-endpoint adapter code.

## Testing

- **Unit tests**: for each converted feature, assert `PagedResult` shape: no paging params → all
  items with pageSize==totalCount; `page`/`pageSize` → correct slice and metadata; out-of-range
  params clamped. Follow existing `Module.UnitTests` conventions.
- **Verification**: `dotnet build` (warnings-as-errors), `dotnet test service/Api/tests/Module.UnitTests`,
  `cd app/Admin && pnpm run lint && pnpm run test:unit`, `cd app/Store && pnpm run lint && pnpm run test:unit`.

## Success criteria

- No endpoint returns a bare collection or a single-list envelope; every collection response is
  `PagedResult<T>`.
- `scripts/check-feature-conventions.sh` passes (AC-001/002/003/005 drift checks).
- All unit/lint suites pass.
- Callers that omit `page`/`pageSize` receive identical item data (all items), only the envelope
  is standardized.
