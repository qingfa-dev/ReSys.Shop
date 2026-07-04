---
goal: Create Catalog Seeders for reference and demo product data
version: 1.0
date_created: 2026-07-04
status: Completed
tags: feature, catalog, seeders
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Create data seeders for the Catalog module under `service/Api/src/Module/Catalog/Persistence/Seeders/` following the established `IDataSeeder` / `AbstractDataSeeder` pattern used by Location, Identity, and Profile modules. Seeders will populate option types/values, taxonomies/taxons, and a demo product with variant and price.

## 1. Requirements & Constraints

- **REQ-001**: All seeders must extend `AbstractDataSeeder` and implement `IDataSeeder`
- **REQ-002**: Each seeder must check idempotency via `HasDataAsync<T>()` before inserting
- **REQ-003**: All seeders must use the domain factory methods (`ProductMethod.Create()`, `TaxonomyExtensions.Create()`, `TaxonExtensions.Create()`, `OptionTypeMethod.Create()`, `OptionValueExtensions.Create()`, etc.) for entity creation
- **REQ-004**: All seeders must set audit fields (`CreatedAtUtc`, `CreatedBy = "System"`)
- **REQ-005**: Seeders must be registered in `CatalogExtensions.AddCatalogModule()` via `AddSeeder<T>()`
- **REQ-006**: Order values must be sequential (100+) to avoid conflicts with existing seeders (10-60)
- **REQ-007**: Dependent seeders must query the database for parent entity IDs rather than hardcoding
- **REQ-008**: All files must go under `service/Api/src/Module/Catalog/Persistence/Seeders/`
- **PAT-001**: Follow existing seeder conventions from `CountrySeeder`, `StateSeeder`, `UserSeeder`
- **PAT-002**: Use `WebApplicationBuilder.AddSeeder<TSeeder>()` extension method for registration
- **PAT-003**: Use named parameters convention matching existing seeder codebase style

## 2. Implementation Steps

### Phase 1: Reference Data Seeders

- GOAL-001: Implement `CatalogOptionSeeder` to populate OptionTypes (Size, Color) with their OptionValues

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `Catalog/Persistence/Seeders/CatalogOptionSeeder.cs` — seeds OptionTypes `Size` (with values S, M, L, XL) and `Color` (with values Red, Blue, Green, Black, White, Yellow, Purple) in a single seeder. Sets `Filterable = true` on both. Uses `OptionTypeMethod.Create()` and `OptionValueExtensions.Create()`. Saves OptionTypes first with `SaveChangesAsync`, then queries them by `Name` to create OptionValues with correct `OptionTypeId`. | | |
| TASK-002 | Create `Catalog/Persistence/Seeders/CatalogTaxonomySeeder.cs` — seeds Taxonomies `Categories` (presentation: "Departments") and `Brands` (presentation: "Brands"). Uses `TaxonomyExtensions.Create()`. Sets `Position` values. | | |

### Phase 2: Hierarchical Data Seeders

- GOAL-002: Implement `CatalogTaxonSeeder` to seed taxon hierarchy under each taxonomy

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-003 | Create `Catalog/Persistence/Seeders/CatalogTaxonSeeder.cs` — queries Taxonomies by `Name` to get IDs. Under `Categories` taxonomy, seeds root taxon "Categories" (slug: categories, Lft:1, Rgt:8, Depth:0) with children: Men (Lft:2, Rgt:3, Depth:1), Women (Lft:4, Rgt:5, Depth:1), Accessories (Lft:6, Rgt:7, Depth:1). Under `Brands` taxonomy, seeds root taxon "Brands" (slug: brands, Lft:1, Rgt:12, Depth:0) with children: Nike (Lft:2, Rgt:3, Depth:1), Adidas (Lft:4, Rgt:5, Depth:1), Zara (Lft:6, Rgt:7, Depth:1), H&M (Lft:8, Rgt:9, Depth:1), Uniqlo (Lft:10, Rgt:11, Depth:1). All taxons set `HideFromNav: false`, `Automatic: false`. Uses `TaxonExtensions.Create()` and then sets `Lft`, `Rgt`, `Depth`, `Permalink`, `PrettyName` after creation. | | |

### Phase 3: Demo Product Seeder

- GOAL-003: Implement `CatalogDemoSeeder` to seed a sample product with variant, price, and classification

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004 | Create `Catalog/Persistence/Seeders/CatalogDemoSeeder.cs` — seeds a demo product "Classic Cotton T-Shirt" with slug "classic-cotton-t-shirt", status `ProductStatus.Active`, `AvailableOn = DateTimeOffset.UtcNow`. Creates a master Variant (SKU: "TEE-COTTON-001", IsMaster: true, Price: 29.99). Creates a default Price (Amount: 29.99, Currency: "USD", IsDefault: true). Creates a Classification linking product to the "Men" taxon. Queries Taxons by `Slug` to find "men". Uses `ProductMethod.Create()`, `VariantMethod.Create()`, `PriceMethod.Create()`, `ClassificationMethod.Create()`. Sets `Product.MasterVariantId` after saving the master variant. | | |

### Phase 4: Registration

- GOAL-004: Register all catalog seeders in the DI container and verify build

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | Update `CatalogExtensions.AddCatalogModule()` at `Catalog.Extension.cs` to add `builder.AddSeeder<CatalogOptionSeeder>()`, `builder.AddSeeder<CatalogTaxonomySeeder>()`, `builder.AddSeeder<CatalogTaxonSeeder>()`, `builder.AddSeeder<CatalogDemoSeeder>()` with appropriate import `using Module.Catalog.Persistence.Seeders;` | | |
| TASK-006 | Run `dotnet build` in `service/Api/` to verify compilation | | |

## 3. Alternatives

- **ALT-001**: Separate `OptionTypeSeeder` and `OptionValueSeeder` as two files — rejected because OptionType+OptionValue are logically a single data set (Size+S/M/L/XL is one concept), and the single-seeder approach reduces complexity. StateSeeder separately queries CountrySeeder data because Country is shared reference data, whereas OptionValues are owned by OptionTypes.
- **ALT-002**: Separate `TaxonomySeeder` and `TaxonSeeder` into two files — considered but rejected for same reason as ALT-001. A taxonomy's taxon tree is an inherent part of the taxonomy definition.
- **ALT-003**: Use explicit `Guid` constants for parent entity IDs across seeders — rejected in favor of runtime database queries to avoid ID collisions with other environments.

## 4. Dependencies

- **DEP-001**: Existing `AbstractDataSeeder` base class and `IDataSeeder` interface in `Shared/Operational/Persistence/Seeders/`
- **DEP-002**: `WebApplicationBuilder.AddSeeder<T>()` extension in `Shared/Operational/Persistence/Seeders/Seeder.Extension.cs`
- **DEP-003**: Catalog domain entities and factory methods (Product, Variant, Price, OptionType, OptionValue, Taxonomy, Taxon, Classification) must already exist
- **DEP-004**: EF Core migration `20260703144227_AddCatalogModuleEntities` must have been applied (creates all catalog tables)

## 5. Files

- **FILE-001**: `service/Api/src/Module/Catalog/Persistence/Seeders/CatalogOptionSeeder.cs` (new)
- **FILE-002**: `service/Api/src/Module/Catalog/Persistence/Seeders/CatalogTaxonomySeeder.cs` (new)
- **FILE-003**: `service/Api/src/Module/Catalog/Persistence/Seeders/CatalogTaxonSeeder.cs` (new)
- **FILE-004**: `service/Api/src/Module/Catalog/Persistence/Seeders/CatalogDemoSeeder.cs` (new)
- **FILE-005**: `service/Api/src/Module/Catalog/Catalog.Extension.cs` (modified - register seeders)

## 6. Testing

- **TEST-001**: Run `dotnet build` on the solution — must compile without errors
- **TEST-002**: Run `DatabaseInitializer.RunSeedersAsync()` in a test or dev environment — seeders must execute in order 100, 110, 120, 130 without errors
- **TEST-003**: Verify `HasDataAsync<T>()` idempotency — running seeders a second time must not throw or duplicate data
- **TEST-004**: Verify foreign key integrity — `OptionValue.OptionTypeId`, `Taxon.TaxonomyId`, `Variant.ProductId`, `Price.VariantId`, `Classification.ProductId`, `Classification.TaxonId` must all resolve correctly

## 7. Risks & Assumptions

- **ASSUMPTION-001**: EF Core change tracker will correctly associate navigation properties (e.g., adding OptionValues to OptionType.OptionValues collection) when `SaveChangesAsync` is called on the second batch
- **ASSUMPTION-002**: The catalog tables exist in the target database (migration `20260703144227_AddCatalogModuleEntities` has been applied)
- **ASSUMPTION-003**: The `IApplicationDbContext` resolves correctly via DI in the seeder pipeline

## 8. Related Specifications / Further Reading

- [AbstractDataSeeder implementation](../service/Api/src/Shared/Operational/Persistence/Seeders/Seeder.Abstract.cs)
- [IDataSeeder interface](../service/Api/src/Shared/Operational/Persistence/Seeders/Seeder.Interface.cs)  
- [Seeder registration extension](../service/Api/src/Shared/Operational/Persistence/Seeders/Seeder.Extension.cs)
- [CountrySeeder example](../service/Api/src/Module/Location/Persistence/Seeders/Country.Seeder.cs)
- [StateSeeder example](../service/Api/src/Module/Location/Persistence/Seeders/State.Seeder.cs) (shows inter-seeder query pattern)
- [Catalog module extension](../service/Api/src/Module/Catalog/Catalog.Extension.cs)
