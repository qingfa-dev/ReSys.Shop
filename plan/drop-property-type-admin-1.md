---
goal: Drop property_type concept from Admin SPA and backend
version: 1.0
date_created: 2026-07-17
status: 'In progress'
tags: [feature, removal, catalog, admin, frontend, backend]
---

# Introduction

![Status: In progress](https://img.shields.io/badge/status-In%20progress-yellow)

Remove the `property_type` / `property-types` concept from the Admin SPA and backend. This feature is scaffolded but non-functional — the frontend has full CRUD views, store, repository, and types, but there is no backend domain entity, database table, migration, or API handler. The repository calls `api/catalog/property-types` which returns 404. Product integration (`ProductPropertyManager`) calls stubbed `getProperties`/`updateProperties` that return empty results. Backend has only permission definitions and an unused storefront response model.

## 1. Requirements & Constraints

- **REQ-001**: Remove all `property-types` feature files from `app/Admin/src/features/catalog/property-types/`
- **REQ-002**: Remove `property-types` route definitions from `catalog.routes.ts`
- **REQ-003**: Remove "Property Types" menu group from `Menu.Layout.vue`
- **REQ-004**: Remove `ProductPropertyManager.Component.vue` and its usage in `ProductForm.View.vue`
- **REQ-005**: Remove `ProductProperty` interface + `properties` field from product response types
- **REQ-006**: Remove `getProperties`/`updateProperties` stubs from `product.service.ts`
- **REQ-007**: Remove `propertyTypeRepository` test from `_tests/catalog.api.spec.ts`
- **REQ-008**: Remove `catalog.property_types.*` i18n section and `catalog.products.messages.property_assigned`/`property_removed` keys from `catalog.json`
- **REQ-009**: Remove `PermissionContext.Resources.PropertyTypes` from `PermissionContext.cs`
- **REQ-010**: Remove `CatalogFeatureMetadata.PropertyTypes` class + inclusion in `All` from `CatalogFeatureMetadata.cs`
- **REQ-011**: Remove `StoreProductPropertyResponse` class + `Properties` field from `ProductStorefront.Model.Response.cs`
- **REQ-012**: Remove `Properties = []` mapping line from `ProductStore.Mapping.cs`
- **REQ-013**: Remove incorrect spec claim about `/api/catalog/property-types` from `spec-design-admin-api-services.md`
- **CON-001**: Must not break the build — `dotnet build` and `pnpm run lint` must pass with zero new errors
- **CON-002**: Must not break unit tests — `dotnet test` and `pnpm run test:unit` must pass (pre-existing failures excluded)
- **CON-003**: Must not touch `SortExpressionBuilder.cs`, `FilterExpressionBuilder.cs` (uses `property.PropertyType` — .NET Reflection, unrelated business concept)
- **CON-004**: Must not touch legacy apps under `app/lagacy/`
- **CON-005**: Must not touch `TaxonRulesManager.Component.vue` (uses `'product_property'` string constant for taxon classification rules — separate concept)
- **CON-006**: Must not touch `catalog.taxa.messages.rule_property` i18n key (taxon rules concept)

## 2. Implementation Steps

### Implementation Phase 1 — Remove property-types feature directory (Admin SPA)

- GOAL-001: Delete all 12 files in `app/Admin/src/features/catalog/property-types/` and the directory itself

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Delete `app/Admin/src/features/catalog/property-types/repositories/property-type.repository.ts` | | |
| TASK-002 | Delete `app/Admin/src/features/catalog/property-types/services/property-type.service.ts` | | |
| TASK-003 | Delete `app/Admin/src/features/catalog/property-types/stores/property-type.store.ts` | | |
| TASK-004 | Delete `app/Admin/src/features/catalog/property-types/schemas/PropertyType.Schema.ts` | | |
| TASK-005 | Delete `app/Admin/src/features/catalog/property-types/types/PropertyType.Response.Type.ts` | | |
| TASK-006 | Delete `app/Admin/src/features/catalog/property-types/types/PropertyType.Request.Type.ts` | | |
| TASK-007 | Delete `app/Admin/src/features/catalog/property-types/types/PropertyType.Query.Type.ts` | | |
| TASK-008 | Delete `app/Admin/src/features/catalog/property-types/types/PropertyType.Parameters.Type.ts` | | |
| TASK-009 | Delete `app/Admin/src/features/catalog/property-types/views/PropertyTypeList.View.vue` | | |
| TASK-010 | Delete `app/Admin/src/features/catalog/property-types/views/PropertyTypeForm.View.vue` | | |
| TASK-011 | Delete `app/Admin/src/features/catalog/property-types/tests/property-type.schema.spec.ts` | | |
| TASK-012 | Delete `app/Admin/src/features/catalog/property-types/tests/property-type.store.spec.ts` | | |

### Implementation Phase 2 — Remove routes and menu items (Admin SPA)

- GOAL-002: Remove the 3 property-type routes and 2 menu items

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-013 | In `app/Admin/src/features/catalog/catalog.routes.ts`: remove lines 129–154 (the `property-types` route block with 3 child routes: `list`, `create`, `edit`) | | |
| TASK-014 | In `app/Admin/src/app/layout/Menu.Layout.vue`: remove lines 42–49 (the "Property Types" menu group with `List` and `Add New` items) | | |

### Implementation Phase 3 — Remove ProductProperty integration (Admin SPA)

- GOAL-003: Remove the dead-end product property management component, type, and stubs

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-015 | Delete `app/Admin/src/features/catalog/products/components/ProductPropertyManager.Component.vue` | | |
| TASK-016 | In `app/Admin/src/features/catalog/products/views/ProductForm.View.vue`: remove `import ProductPropertyManager` (line 15) and remove `<ProductPropertyManager>` usage in template (lines 275–277, the TabPanel with `:value="8"` containing the component) | | |
| TASK-017 | In `app/Admin/src/features/catalog/products/types/Product.Response.Type.ts`: remove `ProductProperty` interface (lines 14–17) and remove `properties: ProductProperty[]` field from `ProductDetail` (line 30) | | |
| TASK-018 | In `app/Admin/src/features/catalog/products/services/product.service.ts`: remove `getProperties` and `updateProperties` stub methods (lines 37–43) | | |

### Implementation Phase 4 — Remove test references (Admin SPA)

- GOAL-004: Remove property-type test imports and test cases

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-019 | In `app/Admin/src/features/catalog/_tests/catalog.api.spec.ts`: remove `import { propertyTypeRepository }` (line 7) and remove the `describe('propertyTypeRepository', ...)` test block (lines 65–69) | | |

### Implementation Phase 5 — Remove i18n keys (Admin SPA)

- GOAL-005: Remove all i18n strings related to property types

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-020 | In `app/Admin/src/shared/locales/messages/en/catalog.json`: remove the entire `catalog.property_types` section (lines 280–330) and remove `catalog.products.messages.property_assigned` (line 163) and `catalog.products.messages.property_removed` (line 164) | | |

### Implementation Phase 6 — Remove backend permission definitions (C#)

- GOAL-006: Remove PropertyTypes resource and feature metadata from RBAC

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-021 | In `service/Api/src/Shared/Security/Authorization/Registry/PermissionContext.cs`: remove `PropertyTypes` field (lines 154–155) | | |
| TASK-022 | In `service/Api/src/Shared/Security/Authorization/Features/CatalogFeatureMetadata.cs`: remove the entire `PropertyTypes` static class (lines 124–137) and remove `.. PropertyTypes.All,` from the `All` aggregator (line 211) | | |

### Implementation Phase 7 — Remove backend storefront response model (C#)

- GOAL-007: Remove unused `StoreProductPropertyResponse` from storefront models

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-023 | In `service/Api/src/Module/Catalog/Features/Storefront/Products/Shared/Models/ProductStorefront.Model.Response.cs`: remove `Properties` property (line 10) from `StoreProductDetailResponse` and remove the entire `StoreProductPropertyResponse` class (lines 43–48) | | |
| TASK-024 | In `service/Api/src/Module/Catalog/Features/Storefront/Products/Shared/Mappings/ProductStore.Mapping.cs`: remove `Properties = []` line (line 32) from the `ToResponse` mapping | | |

### Implementation Phase 8 — Clean up documentation/specs

- GOAL-008: Fix incorrect spec claims

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-025 | In `spec/spec-design-admin-api-services.md`: remove or correct the incorrect claim at lines 184–186 that backend has `/api/catalog/property-types` endpoints | | |

### Implementation Phase 9 — Verification

- GOAL-009: Confirm zero build errors, lint errors, and test failures from this removal

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-026 | Run `dotnet build` — must pass with zero new warnings/errors | | |
| TASK-027 | Run `dotnet test service/Api/tests/Module.UnitTests` — must pass (pre-existing failures in ModuleIsolationTests excluded) | | |
| TASK-028 | Run `dotnet test service/Api/tests/Shared.UnitTests` — must pass | | |
| TASK-029 | Run `cd app/Admin && pnpm run lint` — must pass with zero new errors (pre-existing `no-unused-vars`, `no-conditional-expect`, `require-mock-type-parameters` excluded) | | |
| TASK-030 | Run `cd app/Admin && pnpm run test:unit` — must pass (pre-existing Pinia+vue-i18n setup failures excluded) | | |
| TASK-031 | Run `rg "property.type\|property_type\|PropertyType\|PropertyTypes\|propertyType\|property-type" app/Admin/src service/Api/src --type ts --type cs --type vue -g '!SortExpressionBuilder.cs' -g '!FilterExpressionBuilder.cs' -g '!TaxonRulesManager.Component.vue'` — confirm zero matches remain outside of i18n `rule_property` key | | |

## 3. Alternatives

- **ALT-001**: Keep the frontend scaffold and implement backend property types feature — rejected because the feature was deliberately left unimplemented (stubs + comments confirm "no backend endpoint"), and the product property integration has no real-world usage
- **ALT-002**: Keep permissions and storefront response model "just in case" — rejected because dead code accumulates tech debt and the permission definitions without backing implementation create a misleading API surface

## 4. Dependencies

- **DEP-001**: None — this is a pure removal with no upstream or downstream dependencies

## 5. Files

- **FILE-001**: `app/Admin/src/features/catalog/property-types/` (12 files — to be deleted entirely)
- **FILE-002**: `app/Admin/src/features/catalog/catalog.routes.ts` (remove 3 route definitions)
- **FILE-003**: `app/Admin/src/app/layout/Menu.Layout.vue` (remove 2 menu items)
- **FILE-004**: `app/Admin/src/features/catalog/products/components/ProductPropertyManager.Component.vue` (delete file)
- **FILE-005**: `app/Admin/src/features/catalog/products/views/ProductForm.View.vue` (remove import + template usage)
- **FILE-006**: `app/Admin/src/features/catalog/products/types/Product.Response.Type.ts` (remove `ProductProperty` interface + `properties` field)
- **FILE-007**: `app/Admin/src/features/catalog/products/services/product.service.ts` (remove 2 stub methods)
- **FILE-008**: `app/Admin/src/features/catalog/_tests/catalog.api.spec.ts` (remove import + test block)
- **FILE-009**: `app/Admin/src/shared/locales/messages/en/catalog.json` (remove i18n section + 2 product keys)
- **FILE-010**: `service/Api/src/Shared/Security/Authorization/Registry/PermissionContext.cs` (remove 1 field)
- **FILE-011**: `service/Api/src/Shared/Security/Authorization/Features/CatalogFeatureMetadata.cs` (remove class + aggregator entry)
- **FILE-012**: `service/Api/src/Module/Catalog/Features/Storefront/Products/Shared/Models/ProductStorefront.Model.Response.cs` (remove property + class)
- **FILE-013**: `service/Api/src/Module/Catalog/Features/Storefront/Products/Shared/Mappings/ProductStore.Mapping.cs` (remove 1 line)
- **FILE-014**: `spec/spec-design-admin-api-services.md` (correct 1 spec claim)

## 6. Testing

- **TEST-001**: `dotnet build` — confirms C# code compiles after permission/model removals
- **TEST-002**: `dotnet test service/Api/tests/Module.UnitTests` — confirms no module isolation violations
- **TEST-003**: `dotnet test service/Api/tests/Shared.UnitTests` — confirms shared layer integrity
- **TEST-004**: `pnpm run lint` — confirms zero TS/Vue lint errors after file removals
- **TEST-005**: `pnpm run test:unit` — confirms no test breakage from removed imports/test blocks
- **TEST-006**: Final grep — confirms zero remaining references to removed concept

## 7. Risks & Assumptions

- **RISK-001**: If any other feature imports from the deleted files (beyond what's cataloged), the build will break — TASK-031 grep verification mitigates this
- **ASSUMPTION-001**: The `ProductPropertyManager` component is only used in `ProductForm.View.vue` and nowhere else
- **ASSUMPTION-002**: The `propertyTypeRepository` import in `catalog.api.spec.ts` is the only test that references it
- **ASSUMPTION-003**: Removing `PropertyTypes.All` from `CatalogFeatureMetadata.All` does not affect any other aggregator or RBAC middleware
- **ASSUMPTION-004**: The `Properties = []` mapping in `ProductStore.Mapping.cs` is the only place that references `StoreProductPropertyResponse`
- **ASSUMPTION-005**: Legacy apps (`app/lagacy/`) are intentionally excluded from this plan

## 8. Related Specifications / Further Reading

[spec/spec-design-admin-api-services.md — contains incorrect claim that backend has /api/catalog/property-types]
