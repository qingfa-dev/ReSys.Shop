---
goal: Replace hardcoded ListProducts filter parameters (Q, Color, Size, Material, MinPrice, MaxPrice) with a domain alias map (option_value, option_type, taxon, min_price, max_price) that maps to LINQ predicates while still using the shared QueryingParameters DSL for sort/page/raw-filter
version: 1.2
date_created: 2026-07-04
last_updated: 2026-07-04
owner: Catalog Team
status: Completed
tags: refactor, catalog, storefront, querying, dsl, alias-mapping
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

The current `ListProducts` endpoint (`service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/`) hardcodes six filter properties on its `Parameters` record (`Q`, `Color`, `Size`, `Material`, `MinPrice`, `MaxPrice`) and re-implements their translation to EF Core predicates inside `ListProducts.PagedQueryHandler.Handle`. The hardcoded shape is non-extensible: it can only filter on three pre-chosen option types (`Color`, `Size`, `Material`), cannot filter by any other option type, taxon, or arbitrary property, and bypasses the shared `QueryingParameters` DSL entirely.

The refactor introduces a two-layer design:

1. **Alias surface (consumer-facing)**: short, domain-friendly parameter names exposed on `ListProducts.Parameters` — `option_value`, `option_type`, `taxon`, `min_price`, `max_price`, plus the inherited `search` / `sort` / `page` / `pageSize`. These are the parameters storefront clients see in API docs and OpenAPI schemas.
2. **Alias map (handler-internal)**: a static `StorefrontProductFilterAliases` dictionary that translates each alias into the canonical dot-notation path used by the shared `QueryingParameters` Filter DSL. The handler builds a single synthesized `Filter` string from the aliases, passes it to `parameters.ParseAll(...)`, and lets the existing `ApplyQuerying` extension produce the EF predicate.

The alias map is the single source of truth for "what can be filtered on the storefront" and is also the basis for the `allowedFilterFields` whitelist — both are derived from the same dictionary so they can never drift. The Filter DSL still does the heavy lifting (parsing, validation, expression-tree generation, SQL translation); the alias layer is purely a name-rewriting concern.

The endpoint behavior is preserved end-to-end:
- `?option_value=Red` ≡ `?filter=Variants.OptionValueVariants.OptionValue.Name=Red`
- `?option_type=Color` ≡ `?filter=Variants.OptionValueVariants.OptionValue.OptionType.Name=Color`
- `?taxon=Apparel` ≡ `?filter=Classifications.Taxon.Name=Apparel`
- `?min_price=10&max_price=50` ≡ `?filter=Variants.Prices.Amount>=10,Variants.Prices.Amount<=50`
- `?search=shirt` ≡ `?search=shirt` (passed through unchanged)

## 1. Requirements & Constraints

- **REQ-001**: Delete the six properties `Q`, `Color`, `Size`, `Material`, `MinPrice`, `MaxPrice` from `ListProducts.Parameters`.
- **REQ-002**: Add six new alias properties to `ListProducts.Parameters`: `string? OptionValue`, `string? OptionType`, `string? Taxon`, `decimal? MinPrice`, `decimal? MaxPrice`, plus the inherited `string? Search` (already provided by `QueryingParameters`).
- **REQ-003**: After the edit, `ListProducts.Parameters` MUST inherit from `QueryingParameters` so the `[AsParameters]` binding in `ListProducts.Endpoint.cs` continues to surface `filter`, `sort`, `page`, `pageSize`, and the new aliases in the OpenAPI schema.
- **REQ-004**: Create a new file `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/StorefrontProductFilterAliases.cs` exposing a single static class with:
  - A `IReadOnlyDictionary<string, string>` property `Fields` mapping alias → canonical dot-path: `{"option_value", "Variants.OptionValueVariants.OptionValue.Name"}, {"option_type", "Variants.OptionValueVariants.OptionValue.OptionType.Name"}, {"taxon", "Classifications.Taxon.Name"}, {"min_price", "Variants.Prices.Amount"}, {"max_price", "Variants.Prices.Amount"}`.
  - A `IReadOnlySet<string>` property `CanonicalFields` that is `Fields.Values` materialised once.
  - A static method `string BuildFilter(Parameters parameters)` that reads every alias on a `Parameters` instance and returns the synthesized Filter DSL string (empty string if no alias is set).
- **REQ-005**: The `PagedQueryHandler` MUST use `StorefrontProductFilterAliases.BuildFilter(parameters)` to produce the filter string, then concatenate it with any user-supplied `parameters.Filter` (the inherited raw DSL passthrough) using a comma-separator if both are present, and feed the result into `parameters.ParseAll(allowedFilterFields, allowedSearchFields, allowedSortFields)`.
- **REQ-006**: The `allowedFilterFields` whitelist passed to `ParseAll` MUST be `StorefrontProductFilterAliases.CanonicalFields` (i.e. the same set the alias map is built from) so the alias and the whitelist can never diverge.
- **REQ-007**: The `allowedSearchFields` whitelist MUST be `["Name", "Slug", "Description"]` to preserve the previous `Q` substring behavior under the new `Search` property.
- **REQ-008**: The `allowedSortFields` whitelist MUST include `Name`, `Slug`, `AvailableOn`, `CreatedAtUtc`, `Variants.Prices.Amount`.
- **REQ-009**: The pre-existing default behavior (active products only, `AvailableOn <= UtcNow`, soft-deletion excluded) MUST be preserved as the only handler-level predicates; the alias layer adds predicates on top, never replaces these.
- **REQ-010**: `BuildFilter` MUST encode the price range as two conditions: `Variants.Prices.Amount>={minPrice}` and `Variants.Prices.Amount<={maxPrice}`. When only one of `min_price` / `max_price` is supplied, only the corresponding condition is emitted.
- **REQ-011**: The `BuildFilter` output MUST be the empty string when no alias property is set on `Parameters` (rather than a sentinel value) so the `Filter` field stays `null` and `ParseAll` short-circuits.
- **REQ-012**: The handler MUST add a `.Include(x => x.Classifications).ThenInclude(c => c.Taxon)` chain to the EF Core query so the `taxon` alias can resolve the `Classifications.Taxon.Name` navigation.
- **REQ-013**: The synthesized filter MUST be combined with any user-supplied `parameters.Filter` using a single comma join. Order: synthesized conditions first, user filter conditions second, so that the synthesized predicates act as the "outer" group when the user adds more.
- **SEC-001**: The alias map is the only source of dot-paths the handler will accept. The whitelist passed to `ParseAll` is derived from the same map, so any dot-path not present in `Fields.Values` is rejected with a 400.
- **SEC-002**: The alias keys (`option_value`, `option_type`, `taxon`, `min_price`, `max_price`) are case-insensitive — `BuildFilter` MUST match them with `StringComparer.OrdinalIgnoreCase`.
- **SEC-003**: The user-supplied `parameters.Filter` raw DSL MUST remain subject to the same whitelist; concatenating it with the synthesized alias string does not bypass validation.
- **CON-001**: No new NuGet package may be added.
- **CON-002**: No new public type may be added to `Shared/Operational/Persistence/Specifications/Querying/` — the alias map is a consumer-side helper inside the Catalog module.
- **CON-003**: `ListProducts.Endpoint.cs` MUST NOT change in binding shape — `[AsParameters] Parameters` continues to work because the new alias properties are simple primitives with no `[FromQuery]` attribute (the default binder uses the property name, lowercased, e.g. `OptionValue` → `optionValue`).
- **CON-004**: The alias property names MUST use PascalCase (`OptionValue`, `OptionType`, `Taxon`, `MinPrice`, `MaxPrice`) so the ASP.NET Core model binder produces snake_case query keys (`optionValue`, `optionType`, `taxon`, `minPrice`, `maxPrice`) consistent with the previous `q` / `color` / `size` shape.
- **CON-005**: The `List` endpoint route (`CatalogFeature.Storefront.Products.Get.List.Route`) and HTTP verb (GET) MUST NOT change.
- **CON-006**: The seed file `service/Api/src/Module/Catalog/Persistence/Seeders/Option.Seeder.cs` is NOT modified — the existing `Size`, `Color` option types remain in the data and become filterable through the new aliases.
- **GUD-001**: The `Filter` DSL uses comma for AND and pipe for OR. The handler must rely on this rather than chaining ad-hoc `Where` calls.
- **GUD-002**: Alias values are case-insensitive substring matches (mirroring the previous `EF.Functions.ILike` behavior). The alias map does not pre-encode the `*` (contains) operator because `ParseAll` defaults to `=`; `BuildFilter` MUST emit `*value*` for string-typed aliases (`option_value`, `option_type`, `taxon`) so the resulting EF Core predicate is a case-insensitive contains match.
- **GUD-003**: The alias-to-canonical-path mapping is declared in one place. Any new storefront filter MUST be added by editing only `StorefrontProductFilterAliases.cs`.
- **PAT-001**: Follow the existing handler pattern: `parameters.ParseAll(allowedFilterFields, allowedSearchFields, allowedSortFields)` → `ApplyQuerying(model, defaultSearchFields, defaultSortClauses)` → `ToPagedOrAllAsync(model, projection)`.
- **PAT-002**: The price-range query uses the DSL operator `>=`/`<=` on `Variants.Prices.Amount`. Two comma-separated conditions, e.g. `filter=Variants.Prices.Amount>=10,Variants.Prices.Amount<=50`.
- **PAT-003**: String-typed aliases (`option_value`, `option_type`, `taxon`) are wrapped in `*` so they behave as `contains` rather than `equals` (matching the previous `EF.Functions.ILike` behavior in the deleted handler block).

## 2. Implementation Steps

### Implementation Phase 1: Create the alias map

- GOAL-001: Introduce the static `StorefrontProductFilterAliases` class that owns the alias → canonical-path mapping and the `BuildFilter` synthesizer.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | **Create** `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/StorefrontProductFilterAliases.cs`. The file MUST contain: `namespace Module.Catalog.Features.Storefront.Products.Get.List;` (line 1), blank line, `public static class StorefrontProductFilterAliases` (line 3), with three members: (a) `public static IReadOnlyDictionary<string, string> Fields { get; }` initialized to a dictionary with case-insensitive `StringComparer.OrdinalIgnoreCase` containing exactly five entries: `["option_value"] = "Variants.OptionValueVariants.OptionValue.Name"`, `["option_type"] = "Variants.OptionValueVariants.OptionValue.OptionType.Name"`, `["taxon"] = "Classifications.Taxon.Name"`, `["min_price"] = "Variants.Prices.Amount"`, `["max_price"] = "Variants.Prices.Amount"`; (b) `public static IReadOnlySet<string> CanonicalFields { get; } = Fields.Values.ToHashSet(StringComparer.OrdinalIgnoreCase);`; (c) `public static string BuildFilter(Parameters parameters)` that returns the empty string when every alias on `parameters` is null/empty and otherwise returns a comma-separated DSL string (see TASK-002 for exact format). |  |  |
| TASK-002 | **Implement** `BuildFilter(Parameters parameters)` per these exact rules in `StorefrontProductFilterAliases.cs`: (1) build a `List<string>` named `conditions`; (2) if `parameters.OptionValue` is non-null/whitespace, append `Fields["option_value"] + "=*" + parameters.OptionValue + "*"`; (3) if `parameters.OptionType` is non-null/whitespace, append `Fields["option_type"] + "=*" + parameters.OptionType + "*"`; (4) if `parameters.Taxon` is non-null/whitespace, append `Fields["taxon"] + "=*" + parameters.Taxon + "*"`; (5) if `parameters.MinPrice.HasValue`, append `Fields["min_price"] + ">=" + parameters.MinPrice.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)`; (6) if `parameters.MaxPrice.HasValue`, append `Fields["max_price"] + "<=" + parameters.MaxPrice.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)`; (7) return `string.Join(",", conditions)` (empty string when no condition was appended). |  |  |

### Implementation Phase 2: Reshape the Parameters record

- GOAL-002: Replace the six hardcoded properties with five alias properties plus the inherited `Search` field.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-003 | **Edit** `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/ListProducts.Parameters.cs`. Replace the entire file contents with: `using Shared.Operational.Persistence.Specifications.Querying;` on line 1, blank line 2, `namespace Module.Catalog.Features.Storefront.Products.Get.List;` on line 3, blank line 4, `public static partial class ListProducts` on line 5, blank line 6, then a nested `public record Parameters : QueryingParameters` block (lines 7-14) containing six properties in this order: `public string? OptionValue { get; init; }`, `public string? OptionType { get; init; }`, `public string? Taxon { get; init; }`, `public decimal? MinPrice { get; init; }`, `public decimal? MaxPrice { get; init; }`, `public string? Search { get; init; }` (the `Search` property is also inherited from `QueryingParameters` and is re-declared here only to make it visible in IDE intellisense; declare it with `new` modifier, e.g. `public new string? Search { get; init; }`, to suppress CS0108). |  |  |
| TASK-004 | **Add** `using System.Globalization;` to `ListProducts.Parameters.cs` line 2 (between the existing `using` and the `namespace`) so the `BuildFilter` caller can rely on invariant culture for the price serialization. Note: this is only needed inside `StorefrontProductFilterAliases.cs`; the `Parameters` file does not serialize prices directly, so this task is satisfied by TASK-002's `using` already being in place in the new file. |  |  |

### Implementation Phase 3: Rewire the handler to use the alias map

- GOAL-003: Remove every bespoke `EF.Functions.ILike`/`Where` clause from `ListProducts.PagedQueryHandler.Handle` and replace it with a synthesized filter from `StorefrontProductFilterAliases.BuildFilter`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | **Edit** `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/ListProducts.cs`. Delete lines 29-75 (the entire `if (!string.IsNullOrWhiteSpace(parameters.Q))` block through the closing `}` of the `Material` block). Keep the `.Where(x => !x.IsDeleted && x.AvailableOn <= DateTimeOffset.UtcNow)` line (line 26) and the `.AsNoTracking()` line (line 27) intact. |  |  |
| TASK-006 | **Edit** `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/ListProducts.cs`. Add a `.Include(x => x.Classifications).ThenInclude(c => c.Taxon)` call to the `Include` chain (insert between the existing `.Include(x => x.Variants).ThenInclude(v => v.OptionValueVariants).ThenInclude(ov => ov.OptionValue!).ThenInclude(o => o.OptionType!)` block ending at line 25 and the `.Where(...)` line at line 26) so the `taxon` alias can resolve the `Classifications.Taxon.Name` navigation. |  |  |
| TASK-007 | **Edit** `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/ListProducts.cs`. Replace line 77 (`var parsing = parameters.ParseAll();`) with: (a) `string aliasFilter = StorefrontProductFilterAliases.BuildFilter(parameters);` (b) `string combinedFilter = string.IsNullOrEmpty(aliasFilter) ? (parameters.Filter ?? string.Empty) : (string.IsNullOrEmpty(parameters.Filter) ? aliasFilter : $"{aliasFilter},{parameters.Filter}");` (c) `var tempParameters = combinedFilter == parameters.Filter ? parameters : new Parameters { Filter = combinedFilter, OptionValue = parameters.OptionValue, OptionType = parameters.OptionType, Taxon = parameters.Taxon, MinPrice = parameters.MinPrice, MaxPrice = parameters.MaxPrice, Sort = parameters.Sort, PageNumber = parameters.PageNumber, PageSize = parameters.PageSize, Search = parameters.Search, SearchFields = parameters.SearchFields, SearchMode = parameters.SearchMode };` (d) `var parsing = tempParameters.ParseAll(StorefrontProductFilterAliases.CanonicalFields, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Name", "Slug", "Description" }, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Name", "Slug", "AvailableOn", "CreatedAtUtc", "Variants.Prices.Amount" });`. The existing `if (parsing.IsFailure) return parsing.Errors;` (lines 78-79) and the `ApplyQuerying` / `ToPagedOrAllAsync` calls (lines 81-84) remain unchanged. |  |  |
| TASK-008 | **Edit** `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/ListProducts.cs`. Add `using System.Linq.Expressions;` to the top of the file only if not already present. No other `using` directives need to change because `QueryingParametersExtensions` is already in scope through the `QueryingParameters` base import added in TASK-003. |  |  |

### Implementation Phase 4: Adjust unit tests for the new alias surface

- GOAL-004: Update the existing `ListProductsTests` to validate the alias-driven contract.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | **Edit** `service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/List/ListProducts.Tests.cs`. Add a new `[Fact(DisplayName = "Handler: Should filter products by option_value alias")]` test that seeds one product with a `Color=Red` variant and another with `Color=Blue`, then asserts that `new ListProducts.Query(new ListProducts.Parameters { OptionValue = "Red" })` returns only the red product. |  |  |
| TASK-010 | **Edit** `service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/List/ListProducts.Tests.cs`. Add a new `[Fact(DisplayName = "Handler: Should filter products by option_type alias")]` test that asserts `new ListProducts.Query(new ListProducts.Parameters { OptionType = "Color" })` returns only products whose variants carry a Color option value. |  |  |
| TASK-011 | **Edit** `service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/List/ListProducts.Tests.cs`. Add a new `[Fact(DisplayName = "Handler: Should filter products by taxon alias")]` test that seeds a product classified under taxon `Apparel` and another under `Accessories`, then asserts that `new ListProducts.Query(new ListProducts.Parameters { Taxon = "Apparel" })` returns only the `Apparel` product. |  |  |
| TASK-012 | **Edit** `service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/List/ListProducts.Tests.cs`. Add a new `[Fact(DisplayName = "Handler: Should apply price range via min_price/max_price aliases")]` test that seeds two products with prices `5` and `50`, then asserts that `new ListProducts.Query(new ListProducts.Parameters { MinPrice = 10, MaxPrice = 40 })` returns only the `50`-priced product (the existing variant has price 50 with default 0/None options, so price 50 is in range but price 5 is excluded by the lower bound). |  |  |
| TASK-013 | **Edit** `service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/List/ListProducts.Tests.cs`. Add a new `[Fact(DisplayName = "Handler: Should reject unwhitelisted raw filter field")]` test that asserts a parse failure when `Filter = "SomeSecretProperty=value"` is supplied (because `SomeSecretProperty` is not in `StorefrontProductFilterAliases.CanonicalFields`). |  |  |
| TASK-014 | **Edit** `service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/List/ListProducts.Tests.cs`. Add a new `[Fact(DisplayName = "Alias map: BuildFilter returns empty string when no alias is set")]` test that asserts `StorefrontProductFilterAliases.BuildFilter(new ListProducts.Parameters())` returns `string.Empty`. |  |  |
| TASK-015 | **Edit** `service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/List/ListProducts.Tests.cs`. Add a new `[Fact(DisplayName = "Alias map: BuildFilter wraps string aliases in *value*")]` test that asserts `StorefrontProductFilterAliases.BuildFilter(new ListProducts.Parameters { OptionValue = "Red" })` returns `"Variants.OptionValueVariants.OptionValue.Name=*Red*"`. |  |  |
| TASK-016 | **Edit** `service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/List/ListProducts.Tests.cs`. Add a new `[Fact(DisplayName = "Alias map: BuildFilter emits two conditions for min/max price")]` test that asserts `StorefrontProductFilterAliases.BuildFilter(new ListProducts.Parameters { MinPrice = 10, MaxPrice = 50 })` returns `"Variants.Prices.Amount>=10,Variants.Prices.Amount<=50"` (exact string, comma-joined, invariant-culture formatted). |  |  |
| TASK-017 | **Edit** `service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/List/ListProducts.Tests.cs`. Confirm the four pre-existing tests (lines 33-92) still compile and pass because `new ListProducts.Parameters()` is still a valid call (record with all alias properties null). No code change required inside the four existing test bodies, but rerun them as part of `TEST-001`. |  |  |

### Implementation Phase 5: Update integration tests for the alias surface

- GOAL-005: Update the integration tests under `service/Api/tests/Api.Tests/Scenarios/Catalog/Storefront/Products/List/` to use the new alias parameters.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-018 | **Edit** `service/Api/tests/Api.Tests/Scenarios/Catalog/Storefront/Products/List/ListProducts.IntegrationTests.cs`. In `ListProducts_WithTextSearch_ReturnsMatchingResults` (line 47), replace the URL string `"/api/storefront/products?q=Searchable"` (line 68) with `"/api/storefront/products?search=Searchable"` to match the new contract. |  |  |
| TASK-019 | **Add** a new `[Fact]` method `ListProducts_WithOptionValueAlias_ReturnsMatchingResults` to `ListProducts.IntegrationTests.cs` that calls `GET /api/storefront/products?optionValue=Red` and asserts the response is `200 OK`. The test must rely on the seeded `Color` option type active (the existing `CatalogOptionSeeder` at `service/Api/src/Module/Catalog/Persistence/Seeders/Option.Seeder.cs:23-35` provides this data). |  |  |
| TASK-020 | **Add** a new `[Fact]` method `ListProducts_WithPriceRange_ReturnsMatchingResults` to `ListProducts.IntegrationTests.cs` that calls `GET /api/storefront/products?minPrice=1&maxPrice=1000` and asserts the response is `200 OK`. |  |  |
| TASK-021 | **Add** a new `[Fact]` method `ListProducts_WithInvalidRawFilter_ReturnsValidationError` to `ListProducts.IntegrationTests.cs` that calls `GET /api/storefront/products?filter=NotARealField=oops` and asserts the response is `400 BadRequest` (the `FilterModel` parse surfaces a `Violations` entry which `ApplyQuerying` propagates). |  |  |
| TASK-022 | **Add** a new `[Fact]` method `ListProducts_WithRawFilterOverridingAlias_ReturnsAliasPlusFilter` to `ListProducts.IntegrationTests.cs` that calls `GET /api/storefront/products?optionValue=Red&filter=Variants.OptionValueVariants.OptionValue.Name=Blue` and asserts the response is `200 OK` (the synthesized alias condition AND the raw filter condition both apply, returning products that are Red AND Blue — which is the empty set, but the response is still 200). |  |  |

## 3. Alternatives

- **ALT-001**: Add `OptionTypeName` and `OptionValueName` as a single combined `Option` property using `key=value` string format (e.g. `?option=Color=Red&option=Size=M`). Rejected because it re-implements a mini-DSL instead of using the existing alias map.
- **ALT-002**: Keep `Color`, `Size`, `Material` as named convenience parameters and additionally expose `Filter`. Rejected because the convenience parameters are a strict subset of what the alias map can express; maintaining both creates two parallel ways to filter the same field.
- **ALT-003**: Introduce a new `StorefrontListFilter` interface (`Color`, `Size`, `Taxon`, `PriceRange`, etc.) implemented by `ListProducts.Parameters`. Rejected because the alias map is a smaller surface and avoids coupling the request DTO to a dedicated interface.
- **ALT-004**: Migrate the handler to use a third-party query library (e.g. Sieve, Strathweb.LinqToQuerystring). Rejected by `CON-001` (no new dependencies) and by the existence of an in-house `Querying` infrastructure that already provides the capability.
- **ALT-005**: Bypass the `Querying` infrastructure entirely and write a chain of `query.Where(x => ...)` LINQ predicates directly in the handler, reading the alias properties. Rejected because (a) it duplicates the work the DSL already does (parsing, validation, expression-tree generation, SQL translation), (b) it loses the cache-friendly `Expression<Func<T, bool>>` keying in `FilterModelEfCoreExtensions.ApplyFilter` (line 49), (c) it removes the ability for callers to extend the filter with raw `?filter=...` DSL strings. The alias layer composes with the DSL rather than replacing it.
- **ALT-006**: Use a single `Options` array property accepting `key=value` pairs (e.g. `?options=Color=Red&options=Size=M`). Rejected because it loses type information at the binding layer (the `decimal?` prices cannot be modeled as strings without manual parsing) and because the alias map already gives a single declaration site.
- **ALT-007**: Resolve the alias → canonical path at `[AsParameters]` binding time via a custom `IModelBinder`. Rejected because it would require infrastructure changes inside `Shared/Operational/Persistence/Specifications/Querying/`, violating `CON-002`.

## 4. Dependencies

- **DEP-001**: `service/Api/src/Shared/Operational/Persistence/Specifications/Querying/QueryingParameters.cs` — provides the inherited `Filter`, `Search`, `SearchFields`, `SearchMode`, `Sort`, `PageNumber`, `PageSize` properties.
- **DEP-002**: `service/Api/src/Shared/Operational/Persistence/Specifications/Querying/Querying.Parameters.Extensions.cs` — provides the `ParseAll(allowedFilterFields, allowedSearchFields, allowedSortFields)` factory that the handler calls.
- **DEP-003**: `service/Api/src/Shared/Operational/Persistence/Specifications/Querying/Querying.Model.ApplyExtensions.cs` — provides the `ApplyQuerying` extension that translates the synthesized filter into the EF predicate.
- **DEP-004**: `service/Api/src/Shared/Operational/Persistence/Specifications/Filtering/Expression/FilterExpressionBuilder.cs` — provides the dot-notation path resolution (line 56-71) that makes `Classifications.Taxon.Name=*Apparel*` resolvable as an Expression tree.
- **DEP-005**: `service/Api/src/Module/Catalog/Domain/Products/Classifications/Classification.cs` — exposes the `Classifications` collection on `Product` that the `taxon` alias traverses.
- **DEP-006**: `service/Api/src/Module/Catalog/Domain/Taxonomies/Taxons/Taxon.cs` — exposes the `Name` and `Slug` properties targeted by the `taxon` alias.
- **DEP-007**: `service/Api/src/Module/Catalog/Domain/OptionTypes/OptionType.cs` and `service/Api/src/Module/Catalog/Domain/OptionTypes/Values/OptionValue.cs` — expose the `Name` properties targeted by the `option_type` and `option_value` aliases.
- **DEP-008**: `service/Api/src/Module/Catalog/Persistence/Seeders/Option.Seeder.cs` — unchanged, but supplies the `Color`/`Size` option types that the integration test in TASK-019 relies on.

## 5. Files

- **FILE-001** (CREATE): `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/StorefrontProductFilterAliases.cs` — the new alias map class (TASK-001, TASK-002).
- **FILE-002** (MODIFY): `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/ListProducts.Parameters.cs` — replace the six hardcoded properties with five alias properties plus `Search` (TASK-003).
- **FILE-003** (MODIFY): `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/ListProducts.cs` — delete bespoke predicates (TASK-005), add the `Classifications → Taxon` include (TASK-006), rewire `ParseAll` to consume the synthesized filter (TASK-007), confirm `using` (TASK-008).
- **FILE-004** (UNCHANGED): `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/ListProducts.Endpoint.cs` — no change required per `CON-003`.
- **FILE-005** (UNCHANGED): `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/ListProducts.Response.cs` — no change required.
- **FILE-006** (MODIFY): `service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/List/ListProducts.Tests.cs` — add 8 new test methods (TASK-009 through TASK-016) and rerun the 4 existing tests (TASK-017).
- **FILE-007** (MODIFY): `service/Api/tests/Api.Tests/Scenarios/Catalog/Storefront/Products/List/ListProducts.IntegrationTests.cs` — change one URL string (TASK-018) and add 4 new test methods (TASK-019 through TASK-022).

## 6. Testing

- **TEST-001**: Run `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "Feature=StorefrontListProducts"` and confirm all 12 tests pass (4 existing + 8 new).
- **TEST-002**: Run `dotnet test service/Api/tests/Api.Tests/Api.Tests.csproj --filter "FullyQualifiedName~ListProducts"` and confirm all integration tests pass.
- **TEST-003**: Run `dotnet build service/Api/ReSys.Shop.Api.csproj` and confirm zero warnings, zero errors after all phases.
- **TEST-004**: Manual smoke test: `curl 'http://localhost:5000/api/storefront/products?optionValue=Red&optionType=Color&taxon=Apparel&minPrice=10&maxPrice=50&search=shirt&pageSize=5'` returns a 200 response with at most 5 items.
- **TEST-005**: Manual smoke test: `curl 'http://localhost:5000/api/storefront/products?filter=NotAField=oops'` returns a 400 response (whitelist rejection).
- **TEST-006**: Manual smoke test: `curl 'http://localhost:5000/api/storefront/products?optionValue=Red&filter=Variants.OptionValueVariants.OptionValue.OptionType.Name=Color'` returns a 200 response (alias and raw filter compose).
- **TEST-007**: Manual smoke test: `curl 'http://localhost:5000/api/storefront/products?q=shirt'` returns a 200 response with an unfiltered result set, signaling that `q` is no longer recognized (callers must migrate to `search`).

## 7. Risks & Assumptions

- **RISK-001**: Existing API consumers who call `?q=`, `?color=`, `?size=`, `?material=`, `?minPrice=`, `?maxPrice` will silently receive an unfiltered result set. Mitigation: document the breaking change in the API changelog and add the smoke test (TEST-007) to make the contract change visible.
- **RISK-002**: The dot-notation path `Variants.OptionValueVariants.OptionValue.OptionType.Name` may produce N+1 query plans if EF Core cannot fold the navigation into a single JOIN. Mitigation: keep the explicit `Include` chain (TASK-006) so the navigation is explicit and `Include`-driven.
- **RISK-003**: Adding a new top-level alias (e.g. `brand`) requires editing the `Fields` dictionary in `StorefrontProductFilterAliases` (TASK-001) — this is intentional and the new alias automatically joins the `CanonicalFields` whitelist, so drift is impossible. Mitigation: add a `// NOTE: keep in sync with the documentation` comment block at the dictionary declaration site.
- **RISK-004**: The `BuildFilter` synthesizer uses `*value*` (contains) for string-typed aliases. If a future caller wants exact equality, they must pass the raw `?filter=...` DSL. This trade-off mirrors the previous `EF.Functions.ILike` behavior; documented in `GUD-002` and `PAT-003`.
- **RISK-005**: The `Parameters` record redeclares the `Search` property from `QueryingParameters` with the `new` modifier. Future maintainers may not understand why and may remove it, breaking the IDE-discoverable API surface. Mitigation: a comment at the `Search` declaration notes that the property is inherited and redeclared for visibility only (TASK-003).
- **ASSUMPTION-001**: The alias-to-canonical-path mapping is sufficient for every storefront filter use case that previously required the six hardcoded parameters. The mapping covers all six previous fields (color → `option_value` + `option_type` combo, size → `option_value`, material → `option_value` + `option_type` combo, min/max price → `min_price`/`max_price`, Q → `search`).
- **ASSUMPTION-002**: No external integration (frontend, third-party clients) is currently calling the `ListProducts` endpoint with `?q=`, `?color=`, `?size=`, `?material=`, `?minPrice=`, or `?maxPrice` in production. Verify with the team before merging.
- **ASSUMPTION-003**: The `CatalogOptionSeeder` continues to seed `Color` and `Size` option types so the integration test in TASK-019 has data to filter against. Confirmed at `service/Api/src/Module/Catalog/Persistence/Seeders/Option.Seeder.cs:23-35`.
- **ASSUMPTION-004**: The ASP.NET Core model binder accepts PascalCase property names and lowercases the first letter for the query-string key (`OptionValue` → `optionValue`). This is the standard `[AsParameters]` binding behavior; verified by inspection of `QueryingParameters.Search` → `search` pattern already in use at `service/Api/src/Shared/Operational/Persistence/Specifications/Querying/Querying.Parameters.cs:54`.

## 8. Related Specifications / Further Reading

- [Querying parameters design](service/Api/src/Shared/Operational/Persistence/Specifications/Querying/Querying.Parameters.cs) — base class that `ListProducts.Parameters` extends.
- [Filter DSL syntax](service/Api/src/Shared/Operational/Persistence/Specifications/Querying/Filtering.Parameters.cs) — the `filter` query-string contract (`?filter=Field=OpValue,Field2=OpValue2`).
- [Filter expression builder](service/Api/src/Shared/Operational/Persistence/Specifications/Filtering/Expression/FilterExpressionBuilder.cs) — dot-notation path resolution (line 56-71) that the alias map's canonical paths feed into.
- [Apply extensions](service/Api/src/Shared/Operational/Persistence/Specifications/Querying/Querying.Model.ApplyExtensions.cs) — the `ApplyQuerying` / `ToPagedOrAllAsync` chain the handler still uses.
- [Related refactor plan: `refactor-optional-removal-1.md`](plan/refactor-optional-removal-1.md) — same template style, prior refactor that removed the `Optional<T>` type.
- [Original v1.0 of this plan](plan/refactor-storefront-listproducts-filtering-1.md) — superseded by this version; the v1 approach exposed the dot-paths directly to API consumers, which the v1.1 alias layer hides behind friendly names.
