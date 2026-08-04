# Storefront Product Filtering — Remove Alias Layer, Match by Id

**Date:** 2026-08-04
**Status:** Approved
**Context:** The storefront product list endpoint (`GET /api/storefront/products`) supports filtering via typed query params declared on `ListProducts.Parameters : QueryingParameters`. However, the actual filtering is applied through an indirection layer — `StorefrontProductFilterAliases` — an `IStorefrontProductAlias` interface plus five implementations that match **by Name** using `EF.Functions.ILike` and render a DSL `filter=` string. The frontend contract this forces is name-based (`?optionValue=Red`), which is fragile (renames break filters) and inconsistent with the rest of the codebase, where related entities are keyed by `Guid` id. The Storefront SPA additionally sends its product filters as a **JSON string** inside the DSL `filter=` param and uses mock facet data (colors/sizes/brands) rather than the real storefront endpoints.

## Goal

Remove the alias abstraction entirely. Filter storefront products directly against the typed `ListProducts.Parameters` properties, matching OptionValue and Taxon **by entity Id** (frontend shows labels, sends ids). Wire the Storefront shop to load real facet data and send the typed top-level camelCase params (`optionValueId`, `taxonId`, `minPrice`, `maxPrice`). The `OptionType` filter param is dropped — option type is used only as a grouping label for the option-value pickers, not as a filter itself. The array filter params are lists of `Guid` (frontend field named `optionTypeId` for the option-value id list).

## Non-Goals

- No new backend endpoints. Reuse `GET /api/storefront/option-types` and `GET /api/storefront/taxons`.
- No changes to the raw DSL `filter=` support — it continues to coexist with the typed params (used by `ListProducts_WithAliasAndRawFilter_ReturnsOk` and future raw filters).
- `GetRelatedProducts.Parameters` and other `QueryingParameters` subclasses are untouched.
- Admin SPA and Dashboard are untouched.

## Backend Changes

### 1. `ListProducts.Parameters` (`service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/ListProducts.Parameters.cs`)

The name-based string params become typed **lists of `Guid`**. `OptionType` is dropped entirely. Wire names are camelCase, so no `[FromQuery(Name=...)]` attributes are needed — property names bind as-is:

```csharp
namespace Module.Catalog.Features.Storefront.Products.Get.List;

public static partial class ListProducts
{
    public record Parameters : QueryingParameters
    {
        public Guid[]? OptionValueId { get; init; }
        public Guid[]? TaxonId { get; init; }
        public decimal? MinPrice { get; init; }
        public decimal? MaxPrice { get; init; }
        public new string? Search { get; init; }
    }
}
```

`[AsParameters]` (from the base record) binds repeated `?optionValueId=<guid>` params (and comma-separated lists) into the `Guid[]`. Malformed GUIDs fail model binding and produce an automatic 400.

### 2. Delete the alias layer

Delete `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/StorefrontProductFilterAliases.cs` entirely:

- `IStorefrontProductAlias` interface
- `OptionValueAlias`, `OptionTypeAlias`, `TaxonAlias`, `MinPriceAlias`, `MaxPriceAlias`
- `StorefrontProductFilterAliases.All`, `.CanonicalFields`, `.BuildFilter`

No production code outside this file references `CanonicalFields` or `BuildFilter`; only tests do (removed below).

### 3. `ListProducts` handler (`service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/ListProducts.cs`)

Replace the `foreach (IStorefrontProductAlias alias in StorefrontProductFilterAliases.All)` loop with direct sequential predicates applied from the typed parameters. Array params use `Contains` (matches a product whose variant/classification references **any** of the supplied ids — OR semantics):

- `OptionValueId` (array): `p => p.Variants.Any(v => v.OptionValueVariants.Any(ov => ov.OptionValue != null && ids.Contains(ov.OptionValue.Id)))` where `ids = parameters.OptionValueId`
- `TaxonId` (array): `p => p.Classifications.Any(c => c.Taxon != null && ids.Contains(c.Taxon.Id))` where `ids = parameters.TaxonId`
- `MinPrice`: `p => p.Variants.Any(v => v.Prices.Any(pr => pr.Amount >= parameters.MinPrice.Value))`
- `MaxPrice`: `p => p.Variants.Any(v => v.Prices.Any(pr => pr.Amount <= parameters.MaxPrice.Value))`

There is no `OptionType` predicate — the param no longer exists.

Id-equality predicates translate under the InMemory provider, so unit tests gain real filtering assertions (the old ILike-based tests could not run on InMemory). The existing `parameters.ParseAll(...)` for search/sort/paging is unchanged.

## Backend Tests

### Unit (`service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/List/ListProducts.Tests.cs`)

- Delete the three `BuildFilter_*` tests (the `BuildFilter` helper no longer exists).
- Replace `Handle_ShouldReturnEmpty_WhenAliasFilterSet_OnInMemory` with tests that seed real OptionType/OptionValue and Taxon entities on variants/classifications and assert:
  - filtering by one `OptionValueId` returns only the matching product
  - filtering by multiple `OptionValueId`s returns products matching any id
  - filtering by one `TaxonId` returns only the matching product
  - filtering by multiple `TaxonId`s returns products matching any id
  - non-matching ids return empty
- Extend `Handle_ShouldApplyPriceRangeViaMinMaxPriceAliases` with a positive case (an in-range product is returned).

### Integration (`service/Api/tests/Api.Tests/Scenarios/Catalog/Storefront/Products/List/ListProducts.IntegrationTests.cs`)

- `ListProducts_WithOptionValueAlias_ReturnsOk`: seed an OptionType + OptionValue via the admin API, then hit `GET /api/storefront/products?optionValueId=<optionValueId>` and assert 200.
- `ListProducts_WithAliasAndRawFilter_ReturnsOk`: seed an OptionType, then hit `GET /api/storefront/products?optionValueId=<optionValueId>&filter=Variants.OptionValueVariants.OptionValue.OptionType.Name=Color` and assert 200 (proves typed + DSL coexist).
- `ListProducts_WithPriceRange_ReturnsOk` and `ListProducts_WithUnwhitelistedRawFilter_ReturnsOk`: unchanged.

## Frontend Changes (Storefront SPA)

### 4. Facet data — replace mocks with real endpoints

- `ShopView.vue` / `ShopFilters.vue`: stop passing mock `colors`/`sizes`/`brands`. Load the real facet sources on mount:
  - `GET /api/storefront/option-types` → filterable option types with ordered values (each value exposes `Id` + `Name`). Rendered as filter sections grouped by option type; a checkbox/select value is the OptionValue `Id`, label is the `Name`. The option type is only a grouping label — it is **not** sent as a filter param.
  - `GET /api/storefront/taxons` → taxon tree for the category filter; value is the Taxon `Id`, label is the `Name`/`Presentation`.
- Keep `FilterPriceRange` for min/max price.

### 5. Params plumbing — typed top-level params instead of JSON DSL

- `ProductFilter` type (`app/Storefront/src/features/catalog/types/index.ts`): replace the JSON-DSL fields with `optionTypeId?: string[]`, `taxonId?: string[]`, `priceMin?`, `priceMax?` (plus existing `search`, `sortBy`, `page`, `pageSize`). `optionTypeId` is the internal frontend name for the list of **option-value** ids (it is named after the option type grouping the values belong to, but semantically it carries option-value ids); `optionType` as a distinct filter field is removed.
- `buildProductFilter` (`app/Storefront/src/features/catalog/types/params/product.params.ts`) and `product.api.ts` `getAll`: map the internal name `optionTypeId` to the wire name `optionValueId`, and `taxonId` to `taxonId`, emitting repeated query params (or comma-separated lists), plus `minPrice`, `maxPrice`, instead of the JSON `filter=` string. (Wire names match the backend `ListProducts.Parameters` property names.)
- `useCatalog` / `productStore` (`app/Storefront/src/features/catalog/store/product.ts`): pass the new filter fields through `fetchProducts`.
- `productService.getProducts`: stop `JSON.stringify`-ing the filter into the DSL `filter` param.

## Error Handling

- Optional typed params: malformed GUID → 400 via model binding; unset params are ignored by the handler.
- Empty facet data: sections simply render nothing (existing `v-if` guards).
- No new error codes or Result paths.

## Verification

- Backend: `dotnet build` (warnings-as-errors); `Module.UnitTests` via `dotnet exec service/Api/tests/Module.UnitTests/bin/Debug/net10.0/Module.UnitTests.dll` (MTP — `--filter` unsupported); integration tests via `dotnet test` (needs Docker).
- Storefront: `pnpm run lint`, `pnpm exec vue-tsc --build`, `pnpm run test:unit`.
- Guard scripts: `bash scripts/check-cross-module-refs.sh`, `bash scripts/check-feature-conventions.sh`.

## Risks / Notes

- Array params require a precise id; a stale/deleted id matches nothing rather than erroring — acceptable, matches id-keyed API conventions elsewhere.
- Multiple ids use OR semantics (a product matching any supplied option value or taxon id is returned).
- The storefront product list include graph already loads `OptionValueVariants` → `OptionValue` → `OptionType` and `Classifications` → `Taxon`, so no `Include` changes are needed.
- `GetAllTaxons` returns a flat list with `ParentId`/`Depth`; building a tree for the category filter is a pure frontend concern.
