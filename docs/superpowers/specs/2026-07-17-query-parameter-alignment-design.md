# Query Parameter Alignment — API Constant.Query Enforcement + Admin Fixes

**Date:** 2026-07-17
**Status:** Design approved — awaiting implementation plan
**Scope:** ~20 entities, ~55 files across API (C#) and Admin SPA (Vue/TypeScript)

## Problem

The codebase has two sides that have drifted apart:

1. **API side (C#):** 23 `Constant.Query` classes define `AllowedFilterFields`, `AllowedSearchFields`, `AllowedSortFields` for each entity — but zero handlers reference them. Every `ParseAll()` call passes no whitelist, so every property on every entity is queryable. The constants are dead code.

2. **Admin side (TypeScript/Vue):** Query parameters sent by the admin app don't always match what the API supports:
   - 4 URL mismatches in inventory module (missing `stock-` prefix → 404)
   - Default sort field `-created_at` (snake_case) doesn't resolve to `CreatedAtUtc`
   - Sortable columns target DTO-only properties not present on EF entities (silent no-op)
   - No `searchFields` scoping on most list views (search scans ALL entity fields)
   - Dead query-type fields defined but never wired to UI
   - `OptionType` `searchFields` includes `Description` which API doesn't allow

3. **Missing constants** for Identity (User, Role) and Profile (UserProfile) — entities with paged handlers but no `Constant.Query`.

## Design Decisions

| Decision | Choice |
|---|---|
| Scope | All ~20 entities with paged-query endpoints, one pass |
| Enforcement | Strict — `ParseAll()` returns errors on invalid fields (HTTP 400) |
| Custom handler params | Kept separate from DSL (e.g., Products `Status`/`TaxonId`/`Season` remain as dedicated query params) |
| Consumer side | API + admin SPA only; storefront SPA not affected by these changes |

## Sections

### 1. API — Constant.Query Standardization

Every domain entity with a paged-query endpoint gets a consistent structure:

```csharp
public static class XxxConstant
{
    public static class Query
    {
        public static readonly string[] AllowedFilterFields = [...];
        public static readonly string[] AllowedSearchFields = [...];
        public static readonly string[] AllowedSortFields = [...];
    }
}
```

**New constants to create (missing entities):**

- `Identity/Domain/Users/User.Constant.cs` — `UserConstant.Query`
- `Identity/Domain/Roles/Role.Constant.cs` — `RoleConstant.Query`
- `Profile/Domain/UserProfile.Constant.cs` — add `Query` sub-class to existing `UserProfileConstant`

**Constants to restructure (non-standard patterns):**

- `Country.Constant.cs` — Move fields from `CountryConstant.Constraints.Query` to `CountryConstant.Query`
- `State.Constant.cs` — Wrap class-level fields in `StateConstant.Query` sub-class

**Constants unchanged structurally (21 entities):** Their `Query` sub-class structure is fine. Field contents are reviewed for relevance during handler enforcement.

### 2. API — Handler Enforcement via ParseAll()

Every paged-query handler passes its `Constant.Query` fields into `ParseAll()`:

```csharp
// Before (current)
var parsing = parameters.ParseAll();

// After (new)
var parsing = parameters.ParseAll(
    allowedFilterFields: StockItemConstant.Query.AllowedFilterFields,
    allowedSearchFields: StockItemConstant.Query.AllowedSearchFields,
    allowedSortFields: StockItemConstant.Query.AllowedSortFields
);
```

**~26 handlers updated.** Custom parameters (e.g., Product's `Status`/`TaxonId`/`Season`) remain as separate handler-level properties and are NOT folded into the DSL `Constant.Query`.

**Special cases:**
- **Storefront ListProducts** — currently uses custom inline whitelists. Replace with `ProductConstant.Query`.
- **Address handler** — uses manual `Skip/Take`, not `ParseAll()`. Out of scope for this change; `AddressConstant.Query` fields are defined but enforcement is deferred to a follow-up that refactors the handler to use the standard pipeline.
- **Entities with empty search fields** (StockItem, StockTransfer) — pass empty array. Search parameter will be rejected with 400 if sent.

**Convention:** All new paged-query handlers MUST pass `Constant.Query` fields into `ParseAll()`. Documented in `CONVENTIONS.md`.

### 3. Admin — Bug Fixes

#### 3a. Inventory URL Mismatches (4 files)

API expects `stock-` prefix; admin omits it. These return 404 today.

| File | Before | After |
|---|---|---|
| `location.repository.ts:9` | `${INVENTORY}/locations` | `${INVENTORY}/stock-locations` |
| `transfer.repository.ts:9` | `${INVENTORY}/transfers` | `${INVENTORY}/stock-transfers` |
| `movement.repository.ts:8` | `${INVENTORY}/movements` | `${INVENTORY}/stock-movements` |
| `reservation.repository.ts:8` | `${INVENTORY}/reservations` | `${INVENTORY}/stock-reservations` |

`stock-items` URL is already correct — no change needed.

#### 3b. Product Default Sort

`product.store.ts:35`: `sort: ['-created_at']` → `sort: ['-createdAtUtc']`

`-created_at` → `ConvertToPascalCase` → `CreatedAt`, which doesn't match entity property `CreatedAtUtc`. The sort silently no-ops, falling through to the handler's hardcoded `OrderByDescending(x => x.CreatedAtUtc)`.

#### 3c. Sortable Columns on DTO-Only Properties

Sortable columns whose `field` doesn't match an EF entity property. With strict enforcement, these would produce 400 errors. Fix: remove `sortable` from the column.

| View | Column field | Issue |
|---|---|---|
| `ProductList.View.vue` | `sku`, `price` | Not on Product entity (Variant-level) |
| `AdminUserList.View.vue` | `fullName` | Computed from FirstName+LastName, not on User entity |
| `TaxonomyList.View.vue` | `taxonsCount` | Computed DTO field from `Taxons.Count` |
| `StockItemList.View.vue` | `variant_name`, `stock_location_name`, `quantityReserved`, `countAvailable` | None exist on StockItem entity |

#### 3d. Product Filter `Sku*<value>`

`ProductList.View.vue:82`: `builder.where('Sku', '*', value)` — `Sku` is on Variant, not Product. With strict enforcement, this filter expression returns 400.

**Decision:** Remove the SKU column filter from the admin UI. It was silently no-oping before (no `Sku` property on Product entity). Global search via `search` parameter on `Name`/`Description`/`Slug`/`StyleCode` is sufficient.

### 4. Admin — Search Field Scoping

#### 4a. Add `searchFields` to All List Views

Without `searchFields`, the `search` parameter applies to ALL entity properties. With it, search is scoped to human-readable fields only.

| View | `searchFields` |
|---|---|
| `ProductList.View.vue` | `['Name', 'Description', 'Slug', 'StyleCode']` |
| `AdminUserList.View.vue` | `['UserName', 'Email', 'FirstName', 'LastName']` |
| `CustomerList.View.vue` | `['UserName', 'Email', 'FirstName', 'LastName']` |
| `OrderList.View.vue` | `['Number', 'Email']` |
| `TaxonomyList.View.vue` | `['Name', 'Presentation']` |
| `StockItemList.View.vue` | N/A — API has no searchable StockItem fields (empty array). Remove global search input from this view. |

#### 4b. Fix OptionType `searchFields`

`OptionTypeList.View.vue` sends `['Name', 'Presentation', 'Description']`. Remove `'Description'` — API's `OptionTypeConstant.Query.AllowedSearchFields` doesn't include it.

#### 4c. Conditional `searchFields`

Follow existing pattern (OptionValues, PropertyTypes): pass `searchFields` only when search text is non-empty:

```ts
searchFields: globalFilter.value ? ['Name', 'Description', 'Slug'] : undefined
```

### 5. Admin — Dead Query Parameter Cleanup

Remove fields from TypeScript `Query.Type` files that are defined but never sent by any view:

| File | Remove |
|---|---|
| `Order.Query.Type.ts` | `storeId`, `warehouseId`, `fromDate`, `toDate` |
| `InventoryUnit.Query.Type.ts` | `stockItemId`, `orderId`, `shipmentId`, `state` |
| `StockMovement.Query.Type.ts` | `stockItemId`, `type` |
| `Report.Query.Type.ts` | `from`, `to` |
| `User.Query.Type.ts` | `isActive` (keep `role` — used by Customer listing) |

Fields with active usage (e.g., `ProductQuery.status`/`taxonId`/`season`) are kept.

### 6. Entities Without Admin Views

Payment (PaymentCapture, PaymentMethod), Shipping (ShippingRate, ShippingMethod), and Profile entities have `Constant.Query` classes but no admin list views today. Handlers are enforced same as all others. When admin views are added later, the allowed fields are already defined and gated.

### 7. Files Changed (Estimate)

| Area | Count | Type |
|---|---|---|
| New `Constant.Query` (User, Role, UserProfile) | 3 | New .cs files |
| `Constant.Query` restructure (Country, State) | 2 | Move fields |
| Handler enforcement | ~26 | Add args to `ParseAll()` |
| Consumer refs for Country/State restructure | ~4 | Update references |
| Admin URL fixes | 4 | Fix `stock-` prefix |
| Admin sort field fixes | 4 | Remove `sortable` |
| Admin searchFields scoping | 6 | Add `searchFields` |
| Admin dead query fields | 5 | Remove unused fields |
| **Total** | **~55** | |

### 8. Verification

| Check | Command |
|---|---|
| C# build (warnings-as-errors) | `dotnet build` |
| Module unit tests | `dotnet test service/Api/tests/Module.UnitTests` |
| Shared unit tests | `dotnet test service/Api/tests/Shared.UnitTests` |
| Admin type-check | `pnpm run type-check` (in `app/Admin`) |
| Admin lint | `pnpm run lint` |
| Admin unit tests | `pnpm run test:unit` |
| Manual inventory smoke test | Hit 4 fixed inventory endpoints, verify 200 |

## Risks

- **Storefront breakage:** Storefront SPA queries may break if they send invalid sort/filter fields. Assessed: low — storefront uses different endpoints with narrower queries. But verify with a storefront build after implementation.
- **Test failures:** Existing unit tests may stub handlers with field names not in Constant.Query. Each test fix is one-line (align the mock field to an allowed one).
- **Performance:** `ParseAll()` with whitelists adds HashSet lookups per filter/sort/search field. Negligible — O(n) on the number of clauses (typically <5).
