# Storefront Product Filtering — Id-Based Params Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Remove the `StorefrontProductFilterAliases` abstraction layer and switch storefront product filtering to direct Id-based matching with typed `Guid[]` parameters (`optionValueId`, `taxonId`), while wiring the Storefront SPA to load real facet data and send typed query params. The `OptionType` filter param is dropped.

**Architecture:** The backend will delete the alias interface and implementations, then build predicates directly in `ListProducts.cs` using simple `.Where()` clauses that match by entity Id (array params use `Contains` for OR semantics). The frontend will load filterable option types and taxons from existing endpoints, replace mock colors/sizes/brands with real data, and emit top-level camelCase query params (`optionValueId`, `taxonId`, `minPrice`, `maxPrice`) instead of JSON DSL strings.

**Tech Stack:** C# 13 (.NET 9), Entity Framework Core, Vue 3, TypeScript, Axios

## Global Constraints

- Backend uses `dotnet build` (warnings-as-errors), `dotnet exec` for MTP test runner (no `--filter` flag), integration tests require Docker
- Frontend uses `pnpm run lint`, `pnpm exec vue-tsc --build`, `pnpm run test:unit`
- Backend changes are in `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/`
- Frontend changes are in `app/Storefront/src/features/catalog/`
- Wire param names are camelCase matching the backend property names (`optionValueId`, `taxonId`), so no `[FromQuery(Name=...)]` attributes are needed; `minPrice`, `maxPrice` stay camelCase
- Backend list params are `Guid[]?`; multiple values use OR semantics (a product matching any supplied id is returned)
- Frontend internal field is named `optionTypeId` for the list of option-value ids (named after the grouping option type, but semantically carries option-value ids); taxon list field is `taxonId`
- No new backend endpoints; reuse `GET /api/storefront/option-types` and `GET /api/storefront/taxons`
- Raw DSL `filter=` param continues to coexist with typed params

---

## Task 1: Update ListProducts.Parameters Type Signatures

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/ListProducts.Parameters.cs:1-14`

**Interfaces:**
- Consumes: Nothing (standalone record update)
- Produces: `ListProducts.Parameters` with `Guid[]?` types `OptionValueId` and `TaxonId` (wire names `optionValueId`, `taxonId`); `OptionType` removed

- [ ] **Step 1: Replace the string properties with Guid array id params**

Open `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/ListProducts.Parameters.cs` and replace the entire content:

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

The `new string? Search` hides the base `QueryingParameters.Search` with the same type (intentional shadowing with `new` keyword). The wire names are camelCase and match the property names, so no `[FromQuery(Name = ...)]` attributes are needed — repeated `?optionValueId=<guid>` values bind into the arrays. `OptionType` is dropped entirely.

- [ ] **Step 2: Verify the change compiles**

```bash
dotnet build service/Api
```

Expected: Build succeeds with 0 warnings. The type change from `string?` to `Guid[]?` will cause compilation errors in files that reference these properties — that's expected and will be fixed in Task 3.

- [ ] **Step 3: Commit the type signature change**

```bash
git add service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/ListProducts.Parameters.cs
git commit -m "refactor(catalog): change storefront product filter params to optionValueId and taxonId Guid arrays"
```

---

## Task 2: Delete StorefrontProductFilterAliases.cs

**Files:**
- Delete: `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/StorefrontProductFilterAliases.cs`

**Interfaces:**
- Consumes: Nothing (file removal)
- Produces: Removed `IStorefrontProductAlias` interface, `StorefrontProductFilterAliases` static class, and five alias implementations

- [ ] **Step 1: Delete the alias file**

```bash
rm service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/StorefrontProductFilterAliases.cs
```

This removes:
- `IStorefrontProductAlias` interface
- `StorefrontProductFilterAliases` static class with `All`, `CanonicalFields`, `BuildFilter`
- `OptionValueAlias`, `OptionTypeAlias`, `TaxonAlias`, `MinPriceAlias`, `MaxPriceAlias` implementations

- [ ] **Step 2: Verify compilation errors (expected)**

```bash
dotnet build service/Api
```

Expected: Compilation errors in:
- `ListProducts.cs:43-48` (references `StorefrontProductFilterAliases.All`)
- `ListProducts.Tests.cs:161-182` (references `StorefrontProductFilterAliases.BuildFilter`)

These will be fixed in Tasks 3 and 4.

- [ ] **Step 3: Commit the file deletion**

```bash
git add service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/StorefrontProductFilterAliases.cs
git commit -m "refactor(catalog): delete StorefrontProductFilterAliases abstraction layer"
```

---

## Task 3: Update ListProducts Handler with Direct Predicates

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/ListProducts.cs:1-67`

**Interfaces:**
- Consumes: `ListProducts.Parameters` with `Guid[]?` types (from Task 1)
- Produces: Handler that builds predicates directly from typed params using `.Where()` clauses (array params use `Contains` for OR semantics)

- [ ] **Step 1: Replace the alias loop with direct predicate building**

Open `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/ListProducts.cs` and replace lines 42-48 (the alias loop) with direct predicates:

**Remove:**
```csharp
// Apply: Storefront filter aliases (option value, option type, taxon, price range)
foreach (IStorefrontProductAlias alias in StorefrontProductFilterAliases.All)
{
    var predicate = alias.BuildPredicate(parameters);
    if (predicate is not null)
        query = query.Where(predicate);
}
```

**Replace with:**
```csharp
// Apply: Direct storefront filters by Id (arrays use OR semantics)
if (parameters.OptionValueId is { Length: > 0 })
{
    var optionValueIds = parameters.OptionValueId;
    query = query.Where(p => p.Variants.Any(v =>
        v.OptionValueVariants.Any(ov =>
            ov.OptionValue != null && optionValueIds.Contains(ov.OptionValue.Id))));
}

if (parameters.TaxonId is { Length: > 0 })
{
    var taxonIds = parameters.TaxonId;
    query = query.Where(p => p.Classifications.Any(c =>
        c.Taxon != null && taxonIds.Contains(c.Taxon.Id)));
}

if (parameters.MinPrice.HasValue)
{
    var minPrice = parameters.MinPrice.Value;
    query = query.Where(p => p.Variants.Any(v =>
        v.Prices.Any(pr => pr.Amount >= minPrice)));
}

if (parameters.MaxPrice.HasValue)
{
    var maxPrice = parameters.MaxPrice.Value;
    query = query.Where(p => p.Variants.Any(v =>
        v.Prices.Any(pr => pr.Amount <= maxPrice)));
}
```

There is no `OptionType` predicate — that param was dropped.

The complete handler method should now look like:

```csharp
public async Task<Result<PagedResult<Response>>> Handle(
    Query request,
    CancellationToken cancellationToken)
{
    var parameters = request.Parameters;

    var query = DbContext.Set<Product>()
        .Include(p => p.Variants).ThenInclude(v => v.Prices)
        .Include(p => p.Variants).ThenInclude(v => v.OptionValueVariants)
            .ThenInclude(ov => ov.OptionValue).ThenInclude(ov => ov!.OptionType)
        .Include(p => p.Classifications).ThenInclude(c => c.Taxon)
        .Where(p => !p.IsDeleted && p.AvailableOn <= DateTimeOffset.UtcNow)
        .AsNoTracking();

    // Apply: Direct storefront filters by Id (arrays use OR semantics)
    if (parameters.OptionValueId is { Length: > 0 })
    {
        var optionValueIds = parameters.OptionValueId;
        query = query.Where(p => p.Variants.Any(v =>
            v.OptionValueVariants.Any(ov =>
                ov.OptionValue != null && optionValueIds.Contains(ov.OptionValue.Id))));
    }

    if (parameters.TaxonId is { Length: > 0 })
    {
        var taxonIds = parameters.TaxonId;
        query = query.Where(p => p.Classifications.Any(c =>
            c.Taxon != null && taxonIds.Contains(c.Taxon.Id)));
    }

    if (parameters.MinPrice.HasValue)
    {
        var minPrice = parameters.MinPrice.Value;
        query = query.Where(p => p.Variants.Any(v =>
            v.Prices.Any(pr => pr.Amount >= minPrice)));
    }

    if (parameters.MaxPrice.HasValue)
    {
        var maxPrice = parameters.MaxPrice.Value;
        query = query.Where(p => p.Variants.Any(v =>
            v.Prices.Any(pr => pr.Amount <= maxPrice)));
    }

    // Apply: Querying (search, filter, sort, paging)
    var parsing = parameters.ParseAll(
        allowedFilterFields: ProductConstant.Query.AllowedFilterFields,
        allowedSearchFields: ProductConstant.Query.AllowedSearchFields,
        allowedSortFields: ProductConstant.Query.AllowedSortFields);
    if (parsing.IsFailure)
        return parsing.Errors;

    // Apply: Ordering, paging, and projection
    var ordered = query.OrderByDescending(p => p.CreatedAtUtc);
    var pagedResult = await ordered.ApplyQuerying(parsing.Value)
        .ToPagedOrAllAsync(
            parsing.Value,
            cancellationToken,
            p => p.MapToStoreListItem());

    return Result<PagedResult<Response>>.Success(pagedResult);
}
```

- [ ] **Step 2: Verify the handler compiles**

```bash
dotnet build service/Api
```

Expected: Build succeeds with 0 warnings. The handler now uses direct `.Where()` clauses with `Contains` id checks.

- [ ] **Step 3: Commit the handler update**

```bash
git add service/Api/src/Module/Catalog/Features/Storefront/Products/Get/List/ListProducts.cs
git commit -m "refactor(catalog): replace alias loop with direct Id-based predicates in ListProducts handler"
```

---

## Task 4: Update Backend Unit Tests

**Files:**
- Modify: `service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/List/ListProducts.Tests.cs`

**Interfaces:**
- Consumes: Updated handler (from Task 3) with direct predicates
- Produces: Updated tests that seed real entities and assert Id-based filtering

- [ ] **Step 1: Delete the three BuildFilter tests**

Open `service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/List/ListProducts.Tests.cs` and remove these test methods (lines 158-182):

- `BuildFilter_ReturnsEmpty_WhenNoFilters`
- `BuildFilter_WrapsStringFiltersInContainsSyntax`
- `BuildFilter_FormatsDecimalFiltersCorrectly`

These tests reference the deleted `StorefrontProductFilterAliases.BuildFilter` method.

- [ ] **Step 2: Replace the alias filter test with Id-based tests**

Remove the test `Handle_ReturnsEmpty_WhenAliasFiltersSet_BecauseInMemoryDoesNotSupportILike` (lines 109-124).

Replace it with five new tests that seed real entities and verify Id matching (including multi-id OR semantics):

```csharp
[Fact]
public async Task Handle_FiltersByOptionValueId_ReturnsMatchingProducts()
{
    // Arrange: Create option type, option value, product with variant, and link them
    var optionType = new OptionType { Id = Guid.NewGuid(), Name = "Color", IsFilterable = true };
    var optionValue = new OptionValue { Id = Guid.NewGuid(), OptionTypeId = optionType.Id, Value = "Red" };
    optionType.OptionValues.Add(optionValue);

    var product = ProductMethod.Create("Red T-Shirt", "red-tshirt", "T-Shirt").Value;
    var variant = VariantMethod.Create(product.Id, "Red", "RED", null, null, 29.99m).Value;
    var optionValueVariant = new OptionValueVariant
    {
        OptionValueId = optionValue.Id,
        VariantId = variant.Id,
        OptionValue = optionValue,
        Variant = variant
    };
    variant.OptionValueVariants.Add(optionValueVariant);
    product.Variants.Add(variant);

    DbContext.Set<OptionType>().Add(optionType);
    DbContext.Set<Product>().Add(product);
    await DbContext.SaveChangesAsync();

    var parameters = new ListProducts.Parameters { OptionValueId = [optionValue.Id] };
    var query = new ListProducts.Query(parameters);

    // Act
    var result = await Handler.Handle(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(1);
    result.Value.Items[0].Name.Should().Be("Red T-Shirt");
}

[Fact]
public async Task Handle_FiltersByMultipleOptionValueIds_ReturnsAnyMatching()
{
    // Arrange: Create two option values, two products, each linked to one value
    var optionType = new OptionType { Id = Guid.NewGuid(), Name = "Color", IsFilterable = true };
    var optionValueRed = new OptionValue { Id = Guid.NewGuid(), OptionTypeId = optionType.Id, Value = "Red" };
    var optionValueBlue = new OptionValue { Id = Guid.NewGuid(), OptionTypeId = optionType.Id, Value = "Blue" };
    optionType.OptionValues.AddRange([optionValueRed, optionValueBlue]);

    var redProduct = ProductMethod.Create("Red Shirt", "red-shirt", "T-Shirt").Value;
    var redVariant = VariantMethod.Create(redProduct.Id, "Red", "RED", null, null, 29.99m).Value;
    redVariant.OptionValueVariants.Add(new OptionValueVariant
    {
        OptionValueId = optionValueRed.Id, VariantId = redVariant.Id,
        OptionValue = optionValueRed, Variant = redVariant
    });
    redProduct.Variants.Add(redVariant);

    var blueProduct = ProductMethod.Create("Blue Shirt", "blue-shirt", "T-Shirt").Value;
    var blueVariant = VariantMethod.Create(blueProduct.Id, "Blue", "BLU", null, null, 29.99m).Value;
    blueVariant.OptionValueVariants.Add(new OptionValueVariant
    {
        OptionValueId = optionValueBlue.Id, VariantId = blueVariant.Id,
        OptionValue = optionValueBlue, Variant = blueVariant
    });
    blueProduct.Variants.Add(blueVariant);

    DbContext.Set<OptionType>().Add(optionType);
    DbContext.Set<Product>().AddRange([redProduct, blueProduct]);
    await DbContext.SaveChangesAsync();

    var parameters = new ListProducts.Parameters { OptionValueId = [optionValueRed.Id, optionValueBlue.Id] };
    var query = new ListProducts.Query(parameters);

    // Act
    var result = await Handler.Handle(query);

    // Assert: Both products returned (OR semantics)
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(2);
}

[Fact]
public async Task Handle_FiltersByTaxonId_ReturnsMatchingProducts()
{
    // Arrange: Create taxonomy, taxon, product classification
    var taxonomy = new Taxonomy { Id = Guid.NewGuid(), Name = "Categories" };
    var taxon = new Taxon { Id = Guid.NewGuid(), Name = "Shirts", TaxonomyId = taxonomy.Id };
    taxonomy.Taxons.Add(taxon);

    var product = ProductMethod.Create("Casual Shirt", "casual-shirt", "Shirt").Value;
    var classification = new Classification
    {
        ProductId = product.Id,
        TaxonId = taxon.Id,
        Product = product,
        Taxon = taxon
    };
    product.Classifications.Add(classification);

    DbContext.Set<Taxonomy>().Add(taxonomy);
    DbContext.Set<Product>().Add(product);
    await DbContext.SaveChangesAsync();

    var parameters = new ListProducts.Parameters { TaxonId = [taxon.Id] };
    var query = new ListProducts.Query(parameters);

    // Act
    var result = await Handler.Handle(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(1);
    result.Value.Items[0].Name.Should().Be("Casual Shirt");
}

[Fact]
public async Task Handle_FiltersByMultipleTaxonIds_ReturnsAnyMatching()
{
    // Arrange: Create two taxons, two products, each classified under one
    var taxonomy = new Taxonomy { Id = Guid.NewGuid(), Name = "Categories" };
    var taxonShirts = new Taxon { Id = Guid.NewGuid(), Name = "Shirts", TaxonomyId = taxonomy.Id };
    var taxonPants = new Taxon { Id = Guid.NewGuid(), Name = "Pants", TaxonomyId = taxonomy.Id };
    taxonomy.Taxons.AddRange([taxonShirts, taxonPants]);

    var shirtProduct = ProductMethod.Create("Shirt", "shirt", "Item").Value;
    shirtProduct.Classifications.Add(new Classification
    {
        ProductId = shirtProduct.Id, TaxonId = taxonShirts.Id,
        Product = shirtProduct, Taxon = taxonShirts
    });

    var pantsProduct = ProductMethod.Create("Pants", "pants", "Item").Value;
    pantsProduct.Classifications.Add(new Classification
    {
        ProductId = pantsProduct.Id, TaxonId = taxonPants.Id,
        Product = pantsProduct, Taxon = taxonPants
    });

    DbContext.Set<Taxonomy>().Add(taxonomy);
    DbContext.Set<Product>().AddRange([shirtProduct, pantsProduct]);
    await DbContext.SaveChangesAsync();

    var parameters = new ListProducts.Parameters { TaxonId = [taxonShirts.Id, taxonPants.Id] };
    var query = new ListProducts.Query(parameters);

    // Act
    var result = await Handler.Handle(query);

    // Assert: Both products returned (OR semantics)
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(2);
}

[Fact]
public async Task Handle_ReturnsEmpty_WhenOptionValueIdDoesNotMatch()
{
    // Arrange: Create a product without the matching option value
    var optionType = new OptionType { Id = Guid.NewGuid(), Name = "Color", IsFilterable = true };
    var optionValue = new OptionValue { Id = Guid.NewGuid(), OptionTypeId = optionType.Id, Value = "Blue" };
    optionType.OptionValues.Add(optionValue);

    var product = ProductMethod.Create("Blue T-Shirt", "blue-tshirt", "T-Shirt").Value;
    DbContext.Set<OptionType>().Add(optionType);
    DbContext.Set<Product>().Add(product);
    await DbContext.SaveChangesAsync();

    var parameters = new ListProducts.Parameters { OptionValueId = [Guid.NewGuid()] }; // Non-existent Id
    var query = new ListProducts.Query(parameters);

    // Act
    var result = await Handler.Handle(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
}
```

- [ ] **Step 3: Add a positive test for MinPrice/MaxPrice filtering**

The existing `Handle_AppliesPriceRangeFilters_WhenMinMaxPriceSet` test only checks that filters don't break. Add a positive test that verifies filtering works:

```csharp
[Fact]
public async Task Handle_FiltersByPriceRange_ReturnsProductsInPriceRange()
{
    // Arrange: Create products with different prices
    var cheapProduct = ProductMethod.Create("Cheap Item", "cheap", "Item").Value;
    var cheapVariant = VariantMethod.Create(cheapProduct.Id, "Default", "DEF", null, null, 15.00m).Value;
    cheapProduct.Variants.Add(cheapVariant);

    var midProduct = ProductMethod.Create("Mid Item", "mid", "Item").Value;
    var midVariant = VariantMethod.Create(midProduct.Id, "Default", "DEF", null, null, 50.00m).Value;
    midProduct.Variants.Add(midVariant);

    var expensiveProduct = ProductMethod.Create("Expensive Item", "expensive", "Item").Value;
    var expensiveVariant = VariantMethod.Create(expensiveProduct.Id, "Default", "DEF", null, null, 150.00m).Value;
    expensiveProduct.Variants.Add(expensiveVariant);

    DbContext.Set<Product>().AddRange([cheapProduct, midProduct, expensiveProduct]);
    await DbContext.SaveChangesAsync();

    var parameters = new ListProducts.Parameters { MinPrice = 20.00m, MaxPrice = 100.00m };
    var query = new ListProducts.Query(parameters);

    // Act
    var result = await Handler.Handle(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(1);
    result.Value.Items[0].Name.Should().Be("Mid Item");
}
```

- [ ] **Step 4: Run the updated unit tests**

```bash
cd service/Api/tests/Module.UnitTests
dotnet exec bin/Debug/net9.0/Module.UnitTests.dll --filter "FullyQualifiedName~ListProducts.Tests"
```

Expected: All tests pass. The five new Id-based filter tests and the positive price range test should all succeed.

- [ ] **Step 5: Commit the unit test updates**

```bash
git add service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/List/ListProducts.Tests.cs
git commit -m "test(catalog): update ListProducts unit tests for Id-based filtering"
```

---

## Task 5: Update Backend Integration Tests

**Files:**
- Modify: `service/Api/tests/Api.Tests/Scenarios/Catalog/Storefront/Products/List/ListProducts.IntegrationTests.cs`

**Interfaces:**
- Consumes: Updated handler (from Task 3) with Id-based predicates
- Produces: Integration tests that seed real entities and pass valid GUIDs

- [ ] **Step 1: Update ListProducts_WithOptionValueAlias_ReturnsOk**

Open `service/Api/tests/Api.Tests/Scenarios/Catalog/Storefront/Products/List/ListProducts.IntegrationTests.cs` and replace the test (lines 57-66):

**Remove:**
```csharp
[Fact]
public async Task ListProducts_WithOptionValueAlias_ReturnsOk()
{
    var response = await HttpClient.GetAsync("/api/storefront/products?optionValue=Red");
    response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
}
```

**Replace with:**
```csharp
[Fact]
public async Task ListProducts_WithOptionValueIdParam_ReturnsOk()
{
    // Arrange: Create option type and option value
    var optionTypeResponse = await HttpClient.PostAsJsonAsync("/api/catalog/option-types", new
    {
        name = "Color",
        isFilterable = true,
        optionValues = new[] { new { value = "Red", hex = "#FF0000" } }
    });
    optionTypeResponse.EnsureSuccessStatusCode();
    var optionTypeData = await optionTypeResponse.Content.ReadFromJsonAsync<JsonElement>();
    var optionValueId = optionTypeData.GetProperty("optionValues")[0].GetProperty("id").GetString();

    // Act: Filter by option value Id using the wire param name
    var response = await HttpClient.GetAsync($"/api/storefront/products?optionValueId={optionValueId}");

    // Assert
    response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
}
```

- [ ] **Step 2: Update ListProducts_WithAliasAndRawFilter_ReturnsOk**

Replace the test (lines 83-93):

**Remove:**
```csharp
[Fact]
public async Task ListProducts_WithAliasAndRawFilter_ReturnsOk()
{
    var response = await HttpClient.GetAsync(
        "/api/storefront/products?optionValue=Red&filter=Variants.OptionValueVariants.OptionValue.OptionType.Name=Color");
    response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
}
```

**Replace with:**
```csharp
[Fact]
public async Task ListProducts_WithTypedParamAndRawFilter_ReturnsOk()
{
    // Arrange: Create option type
    var optionTypeResponse = await HttpClient.PostAsJsonAsync("/api/catalog/option-types", new
    {
        name = "Color",
        isFilterable = true,
        optionValues = new[] { new { value = "Red", hex = "#FF0000" } }
    });
    optionTypeResponse.EnsureSuccessStatusCode();
    var optionTypeData = await optionTypeResponse.Content.ReadFromJsonAsync<JsonElement>();
    var optionValueId = optionTypeData.GetProperty("optionValues")[0].GetProperty("id").GetString();

    // Act: Use both typed param and raw filter
    var response = await HttpClient.GetAsync(
        $"/api/storefront/products?optionValueId={optionValueId}&filter=Variants.OptionValueVariants.OptionValue.OptionType.Name=Color");

    // Assert
    response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
}
```

- [ ] **Step 3: Verify integration tests compile**

```bash
dotnet build service/Api/tests/Api.Tests
```

Expected: Build succeeds with 0 warnings.

- [ ] **Step 4: Commit the integration test updates**

```bash
git add service/Api/tests/Api.Tests/Scenarios/Catalog/Storefront/Products/List/ListProducts.IntegrationTests.cs
git commit -m "test(catalog): update ListProducts integration tests for Id-based filtering"
```

---

## Task 6: Update Frontend ProductFilter Type

**Files:**
- Modify: `app/Storefront/src/features/catalog/types/index.ts:1-12`

**Interfaces:**
- Consumes: Nothing (standalone type update)
- Produces: `ProductFilter` interface with `optionTypeId?: string[]`, `taxonId?: string[]`, `priceMin?`, `priceMax?`

- [ ] **Step 1: Update the ProductFilter interface**

Open `app/Storefront/src/features/catalog/types/index.ts` and replace the `ProductFilter` interface (lines 1-12):

**Remove:**
```typescript
export interface ProductFilter {
  category?: string
  priceMin?: number
  priceMax?: number
  tags?: string[]
  inStock?: boolean
  sortBy?: 'newest' | 'price-asc' | 'price-desc' | 'popular'
  search?: string
  page?: number
  pageSize?: number
}
```

**Replace with:**
```typescript
export interface ProductFilter {
  optionTypeId?: string[]
  taxonId?: string[]
  priceMin?: number
  priceMax?: number
  tags?: string[]
  inStock?: boolean
  sortBy?: 'newest' | 'price-asc' | 'price-desc' | 'popular'
  search?: string
  page?: number
  pageSize?: number
}
```

`optionTypeId` is the internal frontend name for the list of **option-value** ids (named after the option type grouping the values belong to, but semantically it carries option-value ids). `taxonId` is the list of taxon ids. These will be mapped to the wire names (`optionValueId`, `taxonId`) in the query builder. A separate `optionType` filter field does not exist — option type is only a grouping label, not a filter.

- [ ] **Step 2: Verify TypeScript compilation**

```bash
cd app/Storefront
pnpm exec vue-tsc --noEmit
```

Expected: TypeScript errors in files that reference the old `category` field (will be fixed in Task 8).

- [ ] **Step 3: Commit the type update**

```bash
git add app/Storefront/src/features/catalog/types/index.ts
git commit -m "refactor(storefront): update ProductFilter type to optionTypeId and taxonId id lists"
```

---

## Task 7: Update buildProductFilter to Emit Typed Params

**Files:**
- Modify: `app/Storefront/src/features/catalog/types/params/product.params.ts`

**Interfaces:**
- Consumes: `ProductFilter` with `optionTypeId` and `taxonId` list fields (from Task 6)
- Produces: Function that maps internal names to wire names and emits top-level query params (arrays preserved for repeated-key serialization)

- [ ] **Step 1: Replace the buildProductFilter function**

Open `app/Storefront/src/features/catalog/types/params/product.params.ts` and replace the entire function (lines 16-61):

**Remove:**
```typescript
export function buildProductFilter(params: ProductFilter) {
  const builder = queryBuilder<ProductEntity>()

  if (params.category) {
    builder.where('category.slug', '=', params.category)
  }
  if (params.priceMin !== undefined) {
    builder.where('price', '>=', params.priceMin)
  }
  if (params.priceMax !== undefined) {
    builder.where('price', '<=', params.priceMax)
  }
  if (params.tags && params.tags.length > 0) {
    params.tags.forEach((tag) => {
      builder.where('tags', '*', tag)
    })
  }
  if (params.inStock) {
    builder.where('inventory.quantity', '>', 0)
  }
  if (params.search) {
    builder.search(params.search, ['name', 'description', 'category.name'])
  }

  if (params.sortBy) {
    switch (params.sortBy) {
      case 'newest':
        builder.orderBy('createdAt', 'desc')
        break
      case 'price-asc':
        builder.orderBy('price', 'asc')
        break
      case 'price-desc':
        builder.orderBy('price', 'desc')
        break
      case 'popular':
        builder.orderBy('name', 'asc')
        break
    }
  }

  if (params.page !== undefined && params.pageSize !== undefined) {
    builder.page(params.page, params.pageSize)
  }

  return builder.build()
}
```

**Replace with:**
```typescript
export function buildProductFilter(params: ProductFilter) {
  const result: Record<string, any> = {}

  // Map internal names to wire names. optionTypeId carries option-value ids
  // and maps to the optionValueId wire param; arrays are emitted as repeated keys.
  if (params.optionTypeId && params.optionTypeId.length > 0) {
    result.optionValueId = params.optionTypeId
  }
  if (params.taxonId && params.taxonId.length > 0) {
    result.taxonId = params.taxonId
  }
  if (params.priceMin !== undefined) {
    result.minPrice = params.priceMin
  }
  if (params.priceMax !== undefined) {
    result.maxPrice = params.priceMax
  }
  if (params.tags && params.tags.length > 0) {
    result.tags = params.tags.join(',')
  }
  if (params.inStock) {
    result.inStock = true
  }
  if (params.search) {
    result.search = params.search
    result.searchFields = ['name', 'description']
  }

  if (params.sortBy) {
    result.sort = params.sortBy === 'newest' ? '-createdAtUtc'
      : params.sortBy === 'price-asc' ? 'price'
      : params.sortBy === 'price-desc' ? '-price'
      : 'name'
  }

  if (params.page !== undefined) {
    result.pageNumber = params.page
  }
  if (params.pageSize !== undefined) {
    result.pageSize = params.pageSize
  }

  return result
}
```

This function now emits top-level query params instead of using the DSL query builder. Array values (`optionValueId`, `taxonId`) are kept as arrays so the URL serializer in Task 8 can emit repeated keys. The `tags`, `inStock`, `sort`, `pageNumber`, and `pageSize` fields remain for backward compatibility but are no longer the primary filter mechanism.

- [ ] **Step 2: Verify TypeScript compilation**

```bash
cd app/Storefront
pnpm exec vue-tsc --noEmit
```

Expected: TypeScript errors in `product.api.ts` (will be fixed in Task 8).

- [ ] **Step 3: Commit the filter builder update**

```bash
git add app/Storefront/src/features/catalog/types/params/product.params.ts
git commit -m "refactor(storefront): emit typed query params instead of DSL in buildProductFilter"
```

---

## Task 8: Update product.api.ts to Send Typed Params

**Files:**
- Modify: `app/Storefront/src/features/catalog/repositories/product/product.api.ts:24-37`

**Interfaces:**
- Consumes: `buildProductFilter` returning top-level params (from Task 7)
- Produces: HTTP request with typed query params in URL

- [ ] **Step 1: Replace the getAll method**

Open `app/Storefront/src/features/catalog/repositories/product/product.api.ts` and replace the `getAll` method (lines 24-37):

**Remove:**
```typescript
async getAll(params?: { paging?: { page: number; pageSize: number }; filter?: { filter: string }; search?: { search: string; searchFields: string[] }; sort?: { sortBy: string; sortOrder: 'asc' | 'desc' } }): Promise<PagedResult<ProductResponse>> {
  return super.getPaged<ProductResponse>(
    this.endpoint,
    params?.paging,
    params?.filter,
    params?.search,
    params?.sort
  )
}
```

**Replace with:**
```typescript
async getAll(filter?: Record<string, any>): Promise<PagedResult<ProductResponse>> {
  const searchParams = new URLSearchParams()

  if (filter) {
    for (const [key, value] of Object.entries(filter)) {
      if (value === undefined || value === null) continue
      // Arrays are emitted as repeated keys so ASP.NET Core binds them into Guid[]
      if (Array.isArray(value)) {
        for (const item of value) {
          searchParams.append(key, String(item))
        }
      } else {
        searchParams.append(key, String(value))
      }
    }
  }

  const queryString = searchParams.toString()
  const response = await this.client.get<PagedResult<ProductResponse>>(
    `${this.endpoint}${queryString ? `?${queryString}` : ''}`
  )

  return response.data
}
```

This method now accepts a flat object of query params and serializes them directly into the URL. Array values (`optionValueId`, `taxonId`) become repeated keys (`?optionValueId=a&optionValueId=b`).

- [ ] **Step 2: Verify TypeScript compilation**

```bash
cd app/Storefront
pnpm exec vue-tsc --noEmit
```

Expected: TypeScript errors in `product.service.ts` (will be fixed in Task 9).

- [ ] **Step 3: Commit the API method update**

```bash
git add app/Storefront/src/features/catalog/repositories/product/product.api.ts
git commit -m "refactor(storefront): send typed query params in product.api.getAll"
```

---

## Task 9: Update product.service.ts to Pass Filter Object

**Files:**
- Modify: `app/Storefront/src/features/catalog/services/product/product.service.ts:20-28`

**Interfaces:**
- Consumes: `ProductFilter` type (from Task 6), `buildProductFilter` (from Task 7), updated `getAll` (from Task 8)
- Produces: Service that calls `getAll` with the filter object (not JSON stringified)

- [ ] **Step 1: Update the getProducts method**

Open `app/Storefront/src/features/catalog/services/product/product.service.ts` and replace the `getProducts` method (lines 20-28):

**Remove:**
```typescript
async getProducts(filter?: ProductFilter, page = 1, pageSize = 12): Promise<PagedResult<Product>> {
  const paging = { page, pageSize }
  const filterStr = filter ? JSON.stringify(filter) : undefined
  const response = await this.productRepository.getAll({
    paging,
    filter: filterStr ? { filter: filterStr } : undefined,
  })
  return {
    ...response,
    items: response.items.map(mapResponseToEntity),
  }
}
```

**Replace with:**
```typescript
async getProducts(filter?: ProductFilter, page = 1, pageSize = 12): Promise<PagedResult<Product>> {
  const params = filter ? buildProductFilter({ ...filter, page, pageSize }) : buildProductFilter({ page, pageSize })
  const response = await this.productRepository.getAll(params)
  return {
    ...response,
    items: response.items.map(mapResponseToEntity),
  }
}
```

This method now passes the filter object directly to `getAll` instead of JSON-stringifying it.

- [ ] **Step 2: Verify TypeScript compilation**

```bash
cd app/Storefront
pnpm exec vue-tsc --noEmit
```

Expected: TypeScript compilation succeeds.

- [ ] **Step 3: Run the product service tests**

```bash
cd app/Storefront
pnpm run test:unit src/features/catalog/services/product/__tests__/product.service.test.ts
```

Expected: All tests pass. The test that used `{ category: 'electronics' }` now uses the new filter shape.

- [ ] **Step 4: Commit the service update**

```bash
git add app/Storefront/src/features/catalog/services/product/product.service.ts
git commit -m "refactor(storefront): pass filter object directly in product.service.getProducts"
```

---

## Task 10: Update product.store.ts and useCatalog Composable

**Files:**
- Modify: `app/Storefront/src/features/catalog/store/product.store.ts`
- Modify: `app/Storefront/src/features/catalog/composables/useCatalog.ts`

**Interfaces:**
- Consumes: `ProductFilter` type (from Task 6), `buildProductFilter` (from Task 7)
- Produces: Store and composable that use the new filter fields

- [ ] **Step 1: Update the product store filter application**

Open `app/Storefront/src/features/catalog/store/product.store.ts` and update any references to the old `category` field. The store should now use `taxonId` for category filtering.

Search for `filter.category` or `category` in the filter context and replace with `taxonId`. For example:

```typescript
// Old
if (filter.category) {
  // ...
}

// New
if (filter.taxonId) {
  // ...
}
```

The store's `fetchProducts` method should pass the filter object to the service without modification.

- [ ] **Step 2: Update the useCatalog composable**

Open `app/Storefront/src/features/catalog/composables/useCatalog.ts` and update any references to `filter.category`. Replace with `filter.taxonId`.

```typescript
// Old
const setCategory = (category: string) => {
  filters.value.category = category
}

// New
const setTaxon = (taxonId: string) => {
  filters.value.taxonId = [taxonId]
}
```

Also update the option-value and price methods to use the new filter field names. The option-value list maps to the frontend `optionTypeId` field (which carries option-value ids):

```typescript
// Old
const setOptionValue = (optionValueId: string) => {
  filters.value.optionValueId = optionValueId
}

// New: accumulates into the optionTypeId list (which maps to the optionValueId wire param)
const setOptionValues = (optionValueIds: string[]) => {
  filters.value.optionTypeId = optionValueIds
}

// Old
const setOptionType = (optionTypeId: string) => {
  filters.value.optionTypeId = optionTypeId
}

// New: option type is not a filter anymore — remove setOptionType entirely
```

- [ ] **Step 3: Run the store and composable tests**

```bash
cd app/Storefront
pnpm run test:unit src/features/catalog/store/__tests__/product.store.test.ts
pnpm run test:unit src/features/catalog/composables/__tests__/useCatalog.test.ts
```

Expected: All tests pass. Update any test fixtures that use the old filter shape.

- [ ] **Step 4: Commit the store and composable updates**

```bash
git add app/Storefront/src/features/catalog/store/product.store.ts
git add app/Storefront/src/features/catalog/composables/useCatalog.ts
git commit -m "refactor(storefront): update product store and useCatalog for Id-based filters"
```

---

## Task 11: Load Real Facet Data in ShopView

**Files:**
- Modify: `app/Storefront/src/features/catalog/views/ShopView.vue`

**Interfaces:**
- Consumes: Updated `useCatalog` composable (from Task 10), existing `GET /api/storefront/option-types` and `GET /api/storefront/taxons` endpoints
- Produces: Component that loads and stores real facet data

- [ ] **Step 1: Add data loading for option types and taxons**

Open `app/Storefront/src/features/catalog/views/ShopView.vue` and add imports:

```typescript
import { OptionTypeApi } from '../api/optionType.api'
import { TaxonApi } from '../api/taxon.api'
import type { OptionType } from '@/features/catalog/types'
import type { Taxon } from '@/features/catalog/types'
```

Add reactive state for facet data:

```typescript
const optionTypes = ref<OptionType[]>([])
const taxons = ref<Taxon[]>([])
const loadingFacets = ref(false)
```

Add a function to load facet data:

```typescript
async function loadFacetData() {
  loadingFacets.value = true
  try {
    const [optionTypesResult, taxonsResult] = await Promise.all([
      OptionTypeApi.getFilterable(),
      TaxonApi.getAll()
    ])
    if (optionTypesResult.isSuccess) {
      optionTypes.value = optionTypesResult.data
    }
    if (taxonsResult.isSuccess) {
      taxons.value = taxonsResult.data
    }
  } finally {
    loadingFacets.value = false
  }
}
```

Call `loadFacetData()` in the component's `onMounted`:

```typescript
onMounted(async () => {
  await loadProducts()
  await loadFacetData()
})
```

- [ ] **Step 2: Remove mock data and pass real data to ShopFilters**

Remove the mock `mockColors`, `mockSizes`, `mockBrands` arrays (lines 24-45).

Update the `ShopFilters` component props (around line 143):

**Remove:**
```vue
<ShopFilters
  :categories="categories"
  :colors="mockColors"
  :sizes="mockSizes"
  :brands="mockBrands"
  @filter-change="handleFilterChange"
/>
```

**Replace with:**
```vue
<ShopFilters
  :categories="taxons"
  :option-types="optionTypes"
  :loading="loadingFacets"
  @filter-change="handleFilterChange"
/>
```

- [ ] **Step 3: Update handleFilterChange to use new filter fields**

Update the `handleFilterChange` function to map the new filter shape:

```typescript
function handleFilterChange(filters: any) {
  const { category, priceMin, priceMax, optionValues } = filters
  
  // Map category to taxonId
  if (category) {
    setTaxon(category)
  }
  
  // Map price range
  if (priceMin !== undefined || priceMax !== undefined) {
    setPriceRange(priceMin, priceMax)
  }
  
  // Map option values (array of option-value Ids) into the optionTypeId list field
  if (optionValues && optionValues.length > 0) {
    setOptionValues(optionValues)
  }
  
  loadProducts()
}
```

There is no `optionTypes` mapping — option type is not a filter param.

- [ ] **Step 4: Verify TypeScript compilation**

```bash
cd app/Storefront
pnpm exec vue-tsc --noEmit
```

Expected: TypeScript errors in `ShopFilters.vue` (will be fixed in Task 12).

- [ ] **Step 5: Commit the ShopView update**

```bash
git add app/Storefront/src/features/catalog/views/ShopView.vue
git commit -m "feat(storefront): load real facet data in ShopView"
```

---

## Task 12: Update ShopFilters Component

**Files:**
- Modify: `app/Storefront/src/features/catalog/components/ShopFilters.vue`

**Interfaces:**
- Consumes: `OptionType[]` and `Taxon[]` data (from Task 11)
- Produces: Component that renders real filter options using entity Ids

- [ ] **Step 1: Update the component props**

Open `app/Storefront/src/features/catalog/components/ShopFilters.vue` and update the props definition:

**Remove:**
```typescript
props: {
  categories: { type: Array as PropType<Category[]>, default: () => [] },
  colors: { type: Array as PropType<Color[]>, default: () => [] },
  sizes: { type: Array as PropType<Size[]>, default: () => [] },
  brands: { type: Array as PropType<Brand[]>, default: () => [] },
}
```

**Replace with:**
```typescript
props: {
  categories: { type: Array as PropType<Taxon[]>, default: () => [] },
  optionTypes: { type: Array as PropType<OptionType[]>, default: () => [] },
  loading: { type: Boolean, default: false },
}
```

Add imports:

```typescript
import type { OptionType, Taxon } from '@/features/catalog/types'
```

- [ ] **Step 2: Replace the colors, sizes, brands filter sections with option types**

Remove the template sections for colors, sizes, and brands (the three `<div>` blocks that iterate over `colors`, `sizes`, `brands`).

Replace them with a dynamic option types section:

```vue
<div v-for="optionType in optionTypes" :key="optionType.id" class="filter-section">
  <h3>{{ optionType.name }}</h3>
  <div class="filter-options">
    <label v-for="optionValue in optionType.optionValues" :key="optionValue.id" class="filter-option">
      <input
        type="checkbox"
        :value="optionValue.id"
        v-model="selectedOptionValues"
        @change="emitFilterChange"
      />
      <span>{{ optionValue.value }}</span>
    </label>
  </div>
</div>
```

Add reactive state for selected option values:

```typescript
const selectedOptionValues = ref<string[]>([])
```

- [ ] **Step 3: Update the categories section to use Taxon data**

The categories section should already work with the new `Taxon[]` type since it has `id`, `name`, and `children`. Update the template to use `taxon.id` as the value:

```vue
<div class="filter-section">
  <h3>Categories</h3>
  <div class="filter-options">
    <label v-for="taxon in categories" :key="taxon.id" class="filter-option">
      <input
        type="radio"
        :value="taxon.id"
        v-model="selectedCategory"
        @change="emitFilterChange"
      />
      <span>{{ taxon.name }}</span>
    </label>
  </div>
</div>
```

- [ ] **Step 4: Update the emitFilterChange function**

Update the filter emission to include the new filter shape:

```typescript
function emitFilterChange() {
  emit('filter-change', {
    category: selectedCategory.value,
    priceMin: priceRange.value[0] > 0 ? priceRange.value[0] : undefined,
    priceMax: priceRange.value[1] < 1000 ? priceRange.value[1] : undefined,
    optionValues: selectedOptionValues.value.length > 0 ? selectedOptionValues.value : undefined,
  })
}
```

Note: We emit `optionValues` as an array of Ids. The backend will handle multiple option values by returning products that match ANY of the selected values (OR logic).

- [ ] **Step 5: Add a loading state**

Show a loading indicator when facet data is being fetched:

```vue
<div v-if="loading" class="filter-loading">
  <i class="pi pi-spin pi-spinner"></i>
  <span>Loading filters...</span>
</div>

<div v-else>
  <!-- Existing filter sections -->
</div>
```

- [ ] **Step 6: Verify TypeScript compilation**

```bash
cd app/Storefront
pnpm exec vue-tsc --noEmit
```

Expected: TypeScript compilation succeeds.

- [ ] **Step 7: Commit the ShopFilters update**

```bash
git add app/Storefront/src/features/catalog/components/ShopFilters.vue
git commit -m "feat(storefront): update ShopFilters to use real option types and taxons"
```

---

## Task 13: Run Full Verification Suite

**Files:**
- No file changes (verification only)

**Interfaces:**
- Consumes: All backend and frontend changes from Tasks 1-12
- Produces: Verification that all tests pass and code compiles

- [ ] **Step 1: Build the backend**

```bash
dotnet build service/Api
```

Expected: Build succeeds with 0 warnings (warnings-as-errors is enabled).

- [ ] **Step 2: Run backend unit tests**

```bash
cd service/Api/tests/Module.UnitTests
dotnet exec bin/Debug/net9.0/Module.UnitTests.dll --filter "FullyQualifiedName~ListProducts"
```

Expected: All `ListProducts` tests pass (including the four new Id-based filter tests and the positive price range test).

- [ ] **Step 3: Run backend integration tests (if Docker is available)**

```bash
cd service/Api/tests/Api.Tests
dotnet exec bin/Debug/net9.0/Api.Tests.dll --filter "FullyQualifiedName~ListProducts"
```

Expected: All `ListProducts` integration tests pass. If Docker is not available, skip this step and note that integration tests require manual verification.

- [ ] **Step 4: Build the frontend**

```bash
cd app/Storefront
pnpm exec vue-tsc --build
```

Expected: TypeScript compilation succeeds with no errors.

- [ ] **Step 5: Run frontend linter**

```bash
cd app/Storefront
pnpm run lint
```

Expected: Linter passes with no errors.

- [ ] **Step 6: Run frontend unit tests**

```bash
cd app/Storefront
pnpm run test:unit
```

Expected: All tests pass, including the updated product service, store, and composable tests.

- [ ] **Step 7: Run guard scripts**

```bash
bash scripts/check-cross-module-refs.sh
bash scripts/check-feature-conventions.sh
```

Expected: Both scripts pass with no violations.

- [ ] **Step 8: Manual smoke test (optional)**

Start the backend and frontend:

```bash
# Backend
cd service/Api
dotnet run

# Frontend (in another terminal)
cd app/Storefront
pnpm run dev
```

Open the Storefront shop page and verify:
- Facet data loads from the real endpoints (option types and taxons appear)
- Selecting a filter option updates the URL with typed query params (`?optionValue=<guid>`)
- The product list updates based on the selected filters
- Multiple filters can be combined
- Price range filtering works
- Clear filters button resets all filters

- [ ] **Step 9: Commit verification (if any fixes were needed)**

If you made any fixes during verification, commit them:

```bash
git add -A
git commit -m "fix(storefront): address issues found during verification"
```

---

## Task 14: Update Frontend Tests

**Files:**
- Modify: `app/Storefront/src/features/catalog/services/product/__tests__/product.service.test.ts`
- Modify: `app/Storefront/src/features/catalog/store/__tests__/product.store.test.ts`
- Modify: `app/Storefront/src/features/catalog/composables/__tests__/useCatalog.test.ts`

**Interfaces:**
- Consumes: Updated `ProductFilter` type, `buildProductFilter`, `product.api.getAll`, `product.service.getProducts`, `product.store`, `useCatalog`
- Produces: Updated tests that use the new filter shape

- [ ] **Step 1: Update product.service.test.ts**

Open `app/Storefront/src/features/catalog/services/product/__tests__/product.service.test.ts` and find the test that uses `{ category: 'electronics' }`:

```typescript
it('should apply filters', async () => {
  const result = await productService.getProducts({ category: 'electronics' } as ProductFilter, 1, 10)
  expect(result.isSuccess).toBe(true)
})
```

Update it to use the new filter shape:

```typescript
it('should apply filters', async () => {
  const result = await productService.getProducts({ taxonId: 'some-taxon-id' }, 1, 10)
  expect(result.isSuccess).toBe(true)
})
```

- [ ] **Step 2: Update product.store.test.ts**

Open `app/Storefront/src/features/catalog/store/__tests__/product.store.test.ts` and search for any references to `filter.category` or the old filter shape. Update them to use `filter.taxonId`, `filter.optionValueId`, `filter.optionTypeId`, etc.

For example:

```typescript
// Old
store.setFilter({ category: 'electronics' })

// New
store.setFilter({ taxonId: 'some-taxon-id' })
```

- [ ] **Step 3: Update useCatalog.test.ts**

Open `app/Storefront/src/features/catalog/composables/__tests__/useCatalog.test.ts` and update any references to the old filter methods. For example:

```typescript
// Old
composable.setCategory('electronics')

// New
composable.setTaxon('some-taxon-id')
```

- [ ] **Step 4: Run the updated frontend tests**

```bash
cd app/Storefront
pnpm run test:unit
```

Expected: All tests pass.

- [ ] **Step 5: Commit the test updates**

```bash
git add app/Storefront/src/features/catalog/services/product/__tests__/
git add app/Storefront/src/features/catalog/store/__tests__/
git add app/Storefront/src/features/catalog/composables/__tests__/
git commit -m "test(storefront): update frontend tests for Id-based filtering"
```

---

## Summary

This plan contains **14 tasks** that transform the storefront product filtering from name-based aliases to Id-based typed parameters:

**Backend (Tasks 1-5):**
1. Update `ListProducts.Parameters` type signatures
2. Delete `StorefrontProductFilterAliases.cs`
3. Update handler with direct predicates
4. Update backend unit tests
5. Update backend integration tests

**Frontend (Tasks 6-14):**
6. Update `ProductFilter` type
7. Update `buildProductFilter` to emit typed params
8. Update `product.api.ts` to send typed params
9. Update `product.service.ts` to pass filter object
10. Update `product.store.ts` and `useCatalog`
11. Load real facet data in `ShopView`
12. Update `ShopFilters` component
13. Run full verification suite
14. Update frontend tests

**Total estimated time:** 2-3 hours for an experienced developer.

**Key decisions:**
- Backend uses direct `.Where()` clauses instead of the alias abstraction
- Frontend emits top-level camelCase query params (`optionValueId`, `taxonId`) instead of JSON DSL
- Frontend loads real facet data from existing endpoints
- Mock colors/sizes/brands are replaced with real option types and taxons
- Wire params are camelCase matching property names (`optionValueId`, `taxonId`); the frontend field for the option-value id list is named `optionTypeId`; `OptionType` filter is dropped

**Verification:**
- Backend: `dotnet build`, unit tests, integration tests
- Frontend: `vue-tsc --build`, `pnpm run lint`, `pnpm run test:unit`
- Guard scripts: `check-cross-module-refs.sh`, `check-feature-conventions.sh`

---

## Execution Options

**Option 1: Subagent-Driven (Recommended)**
I'll dispatch a fresh subagent for each task, review the work between tasks, and iterate quickly. This is faster and catches issues early.

**Option 2: Inline Execution**
I'll execute all tasks in this session with batch checkpoints for review.

Which approach would you like to use?
