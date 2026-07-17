---
goal: Restructure Admin SPA type and schema files into consistent Parameters/Request/Response/Query pattern
version: 1.0
date_created: 2026-07-17
status: Planned
tags: refactor, typescript, architecture, admin, schema
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Standardize all Admin SPA type definitions and Zod schemas into a uniform pattern: one `{Entity}.Schema.ts` per entity, with four dedicated type files (`Parameters`, `Request`, `Response`, `Query`). This eliminates the current mixture of `domain.types.ts`, `model.types.ts`, and ad-hoc inline types across 18+ entities, making the data layer fully deterministic for AI agents and human developers.

## 1. Requirements & Constraints

- **REQ-001**: Every entity must have exactly one `{Entity}.Schema.ts` file containing its Zod validation schema
- **REQ-002**: Every entity must have `{Entity}.Parameters.Type.ts` — form parameter type derived from the schema
- **REQ-003**: Every entity must have `{Entity}.Request.Type.ts` — request DTO inheriting/extending Parameters; used for form submit values in Views
- **REQ-004**: Every entity must have `{Entity}.Response.Type.ts` — API response models (what currently lives in `*.domain.types.ts`)
- **REQ-005**: Every entity with query/pagination support must have `{Entity}.Query.Type.ts` — extends `ServerQueryingParameters`
- **REQ-006**: All imports in views, services, stores, repositories, components, tests, and mappers must be updated to the new file paths
- **REQ-007**: Zero regressions — all tests must pass after migration
- **CON-001**: File naming must use PascalCase per entity segment: `{Entity}.Schema.ts`, `{Entity}.Parameters.Type.ts`, etc.
- **CON-002**: Schema files must be co-located in per-entity `schemas/` directories
- **CON-003**: Type files must be co-located in per-entity `types/` directories
- **PAT-001**: `Parameters.Type.ts` must be derived from the Zod schema's inferred type (purely structural, no logic)
- **PAT-002**: `Request.Type.ts` must extend `Parameters.Type.ts` via intersection (`&`) or type alias, never duplicating fields
- **PAT-003**: `Response.Type.ts` must be defined independently from schemas (API shapes differ from form shapes)
- **PAT-004**: Child entities nested under a parent module (e.g., `taxa/` under `taxonomies/`) must have their own `schemas/` and `types/` directories

## 2. Implementation Steps

### Phase 1: Shared Infrastructure

- GOAL-001: Establish the base types and conventions that all entities will consume

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Ensure `shared/api/types/query.types.ts` exports `ServerQueryingParameters` with all required fields (PagingParams, SortParams, SearchParams, FilterParams) | | |
| TASK-002 | Ensure `shared/api/types/result.types.ts` exports `ServerResult<T>`, `ServerPagedResult<T>`, `PaginationMeta`, `ServerError` types | | |
| TASK-003 | Audit `shared/api/types/api.types.ts` to confirm `MappedResult<T>`, `SuccessResult<T>`, `FailureResult`, `mapToErrors` are stable | | |
| TASK-004 | Create `plan/refactor-admin-type-schema-1.md` added to tracking | ✅ | 2026-07-17 |

### Phase 2: Catalog — Taxonomies & Taxa

- GOAL-002: Restructure Taxonomy and Taxon entities as the reference implementation for child-entity pattern

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | Rename `catalog/taxonomies/schemas/taxonomy.schema.ts` → `schemas/Taxonomy.Schema.ts`; update export to `TaxonomySchema` (already PascalCase) | | |
| TASK-006 | Create `catalog/taxonomies/types/Taxonomy.Parameters.Type.ts` — re-export `z.infer<typeof TaxonomySchema>` as `TaxonomyParameters` | | |
| TASK-007 | Create `catalog/taxonomies/types/Taxonomy.Request.Type.ts` — `CreateTaxonomyRequest = TaxonomyParameters`, `UpdateTaxonomyRequest = Partial<TaxonomyParameters>` | | |
| TASK-008 | Create `catalog/taxonomies/types/Taxonomy.Response.Type.ts` — migrate `TaxonomyListItem`, `TaxonNode`, `TaxonomyDetail` from `taxonomy.domain.types.ts` | | |
| TASK-009 | Create `catalog/taxonomies/types/Taxonomy.Query.Type.ts` — `TaxonomyQuery = ServerQueryingParameters` | | |
| TASK-010 | Delete `catalog/taxonomies/types/taxonomy.domain.types.ts` and `taxonomy.request.types.ts` | | |
| TASK-011 | Rename `catalog/taxonomies/taxa/schemas/taxon.schema.ts` → `schemas/Taxon.Schema.ts`; extract `TaxonRuleSchema` to separate `schemas/TaxonRule.Schema.ts` | | |
| TASK-012 | Create `catalog/taxonomies/taxa/types/Taxon.Parameters.Type.ts` — re-export `z.infer<typeof TaxonSchema>` as `TaxonParameters` | | |
| TASK-013 | Create `catalog/taxonomies/taxa/types/TaxonRule.Parameters.Type.ts` — `z.infer<typeof TaxonRuleSchema>` | | |
| TASK-014 | Create `catalog/taxonomies/taxa/types/Taxon.Request.Type.ts` — `CreateTaxonRequest = TaxonParameters & { rules?: TaxonRuleParameters[] }`, `UpdateTaxonRequest = CreateTaxonRequest` | | |
| TASK-015 | Create `catalog/taxonomies/taxa/types/Taxon.Response.Type.ts` — migrate `TaxonListItem`, `TaxonTreeItem`, `TaxonDetail`, `TaxonRuleListItem` from `taxon.domain.types.ts` | | |
| TASK-016 | Create `catalog/taxonomies/taxa/types/Taxon.Query.Type.ts` — `TaxonQuery extends ServerQueryingParameters { taxonomyId?, focusedTaxonId?, includeLeavesOnly?, includeHidden?, maxDepth? }` | | |
| TASK-017 | Delete `catalog/taxonomies/taxa/types/taxon.domain.types.ts` and `taxon.request.types.ts` | | |
| TASK-018 | Update all imports in `catalog/taxonomies/` services, stores, views, components, tests, mapper, repository to point to new type/schema files | | |
| TASK-019 | Update all imports in `catalog/taxonomies/taxa/` services, stores, views, components, tests, mapper, repository to point to new type/schema files | | |
| TASK-020 | Run `pnpm run lint` and `pnpm run test:unit` for the Admin app and fix any errors | | |

### Phase 3: Catalog — Option Types & Option Values

- GOAL-003: Restructure OptionType and child OptionValue entities

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-021 | Rename `catalog/option-types/schemas/option-type.schema.ts` → `schemas/OptionType.Schema.ts` | | |
| TASK-022 | Create `catalog/option-types/types/OptionType.Parameters.Type.ts` — `z.infer<typeof OptionTypeSchema>` | | |
| TASK-023 | Create `catalog/option-types/types/OptionType.Request.Type.ts` — `CreateOptionTypeRequest = OptionTypeParameters`, `UpdateOptionTypeRequest = OptionTypeParameters` | | |
| TASK-024 | Create `catalog/option-types/types/OptionType.Response.Type.ts` — migrate `OptionTypeListItem` from `option-type.domain.types.ts` | | |
| TASK-025 | Create `catalog/option-types/types/OptionType.Query.Type.ts` — `OptionTypeQuery = ServerQueryingParameters` | | |
| TASK-026 | Delete `catalog/option-types/types/option-type.domain.types.ts` and `option-type.request.types.ts` | | |
| TASK-027 | Rename `catalog/option-types/option-values/schemas/option-value.schema.ts` → `schemas/OptionValue.Schema.ts` | | |
| TASK-028 | Create per-entity `types/` for OptionValue (Parameters, Request, Response, Query types) | | |
| TASK-029 | Delete `catalog/option-types/option-values/types/option-value.domain.types.ts` and `option-value.request.types.ts` | | |
| TASK-030 | Update all imports in option-types and option-values services, stores, views, tests, repository | | |
| TASK-031 | Run lint and tests | | |

### Phase 4: Catalog — Property Types

- GOAL-004: Restructure PropertyType entity

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-032 | Rename `catalog/property-types/schemas/property-type.schema.ts` → `schemas/PropertyType.Schema.ts` | | |
| TASK-033 | Create `catalog/property-types/types/PropertyType.Parameters.Type.ts` — `z.infer<typeof PropertyTypeSchema>` | | |
| TASK-034 | Create `PropertyType.Request.Type.ts` — `CreatePropertyTypeRequest = PropertyTypeParameters & { publicMetadata?, privateMetadata? }`, `UpdatePropertyTypeRequest` same | | |
| TASK-035 | Create `PropertyType.Response.Type.ts` — migrate `PropertyTypeListItem` + `PropertyKind` enum from `property-type.domain.types.ts` | | |
| TASK-036 | Create `PropertyType.Query.Type.ts` — `PropertyTypeQuery = ServerQueryingParameters` | | |
| TASK-037 | Delete `catalog/property-types/types/property-type.domain.types.ts` and `property-type.request.types.ts`; delete `property-kind.ts` | | |
| TASK-038 | Update all imports; run lint and tests | | |

### Phase 5: Catalog — Products & Variants

- GOAL-005: Restructure Product and Variant entities (most complex, multiple schema files)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-039 | Rename `catalog/products/schemas/product.schema.ts` → `schemas/ProductClassification.Schema.ts` (rename `ManageClassificationsSchema`) | | |
| TASK-040 | Rename `catalog/products/schemas/product.schemas.ts` → `schemas/CreateProduct.Schema.ts` and `schemas/UpdateProduct.Schema.ts` (split the plural file into per-operation schemas, or consolidate into `Product.Schema.ts` with both `CreateProductSchema` and `UpdateProductSchema` exports) | | |
| TASK-041 | Create `catalog/products/types/Product.Parameters.Type.ts` — unified parameters from create slotted with update-partial semantics | | |
| TASK-042 | Create `Product.Request.Type.ts` — `CreateProductRequest`, `UpdateProductRequest` | | |
| TASK-043 | Create `Product.Response.Type.ts` — migrate `ProductSummary`, `ProductDetail`, `ProductImage`, `ProductClassification`, `VariantOption` from `product.domain.types.ts` | | |
| TASK-044 | Create `Product.Query.Type.ts` — product-specific query fields | | |
| TASK-045 | Delete `product.domain.types.ts`, `product.request.types.ts`, `product.model.types.ts` | | |
| TASK-046 | Create `catalog/products/types/Variant.Parameters.Type.ts` — variant form parameters | | |
| TASK-047 | Create `Variant.Request.Type.ts`, `Variant.Response.Type.ts`, `Variant.Query.Type.ts` | | |
| TASK-048 | Delete `variant.domain.types.ts`, `variant.request.types.ts` | | |
| TASK-049 | Update all imports across products/ components, views, services, stores, tests, repository | | |
| TASK-050 | Run lint and tests | | |

### Phase 6: Auth

- GOAL-006: Restructure Auth entity (clean up `model.types.ts` bridge into Parameters pattern)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-051 | Rename `auth/schemas/auth.schema.ts` → `schemas/Login.Schema.ts` (single-schema file) and `schemas/ChangePassword.Schema.ts` | | |
| TASK-052 | Create `auth/types/Login.Parameters.Type.ts` — `z.infer<typeof LoginSchema>` | | |
| TASK-053 | Create `auth/types/Login.Request.Type.ts` — `LoginRequest = LoginParameters & { ipAddress? }` | | |
| TASK-054 | Create `auth/types/Login.Response.Type.ts` — migrate `AuthenticationResponse` from `auth.response.types.ts` | | |
| TASK-055 | Create `auth/types/ChangePassword.Parameters.Type.ts` and `ChangePassword.Request.Type.ts` | | |
| TASK-056 | Create `auth/types/Auth.Query.Type.ts` if session/status queries exist | | |
| TASK-057 | Delete `auth/types/auth.types.ts`, `auth.model.types.ts`, consolidate into new per-entity files | | |
| TASK-058 | Update all imports; run lint and tests | | |

### Phase 7: Location — Country & State

- GOAL-007: Restructure Country and State with proper per-entity types

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-059 | Rename `location/schemas/country.schema.ts` → `schemas/Country.Schema.ts`; rename `state.schema.ts` → `schemas/State.Schema.ts` | | |
| TASK-060 | Create `location/types/Country.Parameters.Type.ts` and `Country.Request.Type.ts`, `Country.Response.Type.ts`, `Country.Query.Type.ts` | | |
| TASK-061 | Create `location/types/State.Parameters.Type.ts` and `State.Request.Type.ts`, `State.Response.Type.ts`, `State.Query.Type.ts` | | |
| TASK-062 | Delete `location/types/location.domain.types.ts`, `location.model.types.ts`, `location.request.types.ts`, `location.response.types.ts` | | |
| TASK-063 | Update all imports; run lint and tests | | |

### Phase 8: Ordering — Order & Fulfillment

- GOAL-008: Restructure Order and create Fulfillment schema/types

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-064 | Create `ordering/schemas/Order.Schema.ts` with relevant order fields | | |
| TASK-065 | Create `ordering/types/Order.Parameters.Type.ts` — form parameters for order edit/create | | |
| TASK-066 | Create `Order.Request.Type.ts` — `CreateOrderRequest`, `UpdateOrderRequest`, `OrderStatusTransitionRequest` | | |
| TASK-067 | Create `Order.Response.Type.ts` — migrate `OrderSummary`, `OrderDetail`, `OrderItem`, `OrderAddress`, `OrderPayment`, `OrderShipment` from `order.domain.types.ts` | | |
| TASK-068 | Create `Order.Query.Type.ts` | | |
| TASK-069 | Delete `ordering/types/order.domain.types.ts` and `order.request.types.ts` | | |
| TASK-070 | Create `ordering/fulfillment/schemas/Fulfillment.Schema.ts` | | |
| TASK-071 | Create `ordering/fulfillment/types/Fulfillment.Parameters.Type.ts`, `Request.Type.ts`, `Response.Type.ts`, `Query.Type.ts` | | |
| TASK-072 | Update all imports across ordering/; run lint and tests | | |

### Phase 9: Users — User, Role & Permission

- GOAL-009: Restructure Users module with per-entity types and schemas

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-073 | Create `users/schemas/User.Schema.ts` — staff/customer form fields | | |
| TASK-074 | Create `users/types/User.Parameters.Type.ts`, `User.Request.Type.ts`, `User.Response.Type.ts`, `User.Query.Type.ts` | | |
| TASK-075 | Create `users/roles/schemas/Role.Schema.ts` — role form fields | | |
| TASK-076 | Create `users/roles/types/Role.Parameters.Type.ts`, `Role.Request.Type.ts`, `Role.Response.Type.ts`, `Role.Query.Type.ts` | | |
| TASK-077 | Create `users/permissions/schemas/Permission.Schema.ts` | | |
| TASK-078 | Create `users/permissions/types/Permission.Parameters.Type.ts`, `Permission.Request.Type.ts`, `Permission.Response.Type.ts`, `Permission.Query.Type.ts` | | |
| TASK-079 | Delete `users/types/user.domain.types.ts` and `user.request.types.ts` (migrate all content) | | |
| TASK-080 | Update `identity/` repo/service imports to point to new type locations | | |
| TASK-081 | Update all imports; run lint and tests | | |

### Phase 10: Inventories — Stock Item, Stock Location, Stock Transfer, Stock Movement, Inventory Unit

- GOAL-010: Restructure flat inventory types into per-entity schemas and types

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-082 | Create `inventories/schemas/StockLocation.Schema.ts` | | |
| TASK-083 | Create `inventories/types/StockLocation.{Parameters,Request,Response,Query}.Type.ts` | | |
| TASK-084 | Create `inventories/schemas/StockTransfer.Schema.ts` | | |
| TASK-085 | Create `inventories/types/StockTransfer.{Parameters,Request,Response,Query}.Type.ts` | | |
| TASK-086 | Create `inventories/schemas/StockItem.Schema.ts` | | |
| TASK-087 | Create `inventories/types/StockItem.{Parameters,Request,Response,Query}.Type.ts` | | |
| TASK-088 | Create `inventories/schemas/InventoryUnit.Schema.ts` | | |
| TASK-089 | Create `inventories/types/InventoryUnit.{Parameters,Request,Response,Query}.Type.ts` | | |
| TASK-090 | Create `inventories/schemas/StockMovement.Schema.ts` | | |
| TASK-091 | Create `inventories/types/StockMovement.{Parameters,Request,Response,Query}.Type.ts` | | |
| TASK-092 | Delete `inventories/types/inventory.domain.types.ts`, `inventory.request.types.ts`, `inventory.response.types.ts` | | |
| TASK-093 | Update all imports across repositories, services, stores, views, components, tests; run lint and tests | | |

### Phase 11: Profile & Reports

- GOAL-011: Restructure Profile and Report entities

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-094 | Create `profile/schemas/Profile.Schema.ts` | | |
| TASK-095 | Create `profile/types/Profile.Parameters.Type.ts`, `Profile.Request.Type.ts`, `Profile.Response.Type.ts`, `Profile.Query.Type.ts` | | |
| TASK-096 | Delete `profile/types/profile.domain.types.ts` and `profile.request.types.ts` | | |
| TASK-097 | Create `reports/schemas/Report.Schema.ts` | | |
| TASK-098 | Create `reports/types/Report.Parameters.Type.ts`, `Report.Request.Type.ts`, `Report.Response.Type.ts`, `Report.Query.Type.ts` | | |
| TASK-099 | Delete `reports/types/report.domain.types.ts` and `report.request.types.ts` | | |
| TASK-100 | Update all imports; run lint and tests | | |

### Phase 12: Cleanup & Verification

- GOAL-012: Full build, lint, and test pass; remove all legacy files

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-101 | Run `pnpm run lint` across entire Admin app — fix all issues | | |
| TASK-102 | Run `pnpm run test:unit` across entire Admin app — fix all failing tests | | |
| TASK-103 | Remove any remaining `*.domain.types.ts`, `*.model.types.ts`, `*.response.types.ts` files that were not explicitly deleted in earlier phases | | |
| TASK-104 | Remove any remaining `*.schema.ts` (lowercase) files that were renamed | | |
| TASK-105 | Audit all import paths for `@/shared/api/types/query-params.types` (should be `query.types`) and fix any stale references | | |
| TASK-106 | Final `pnpm run typecheck` to confirm zero type errors | | |

## 3. Alternatives

- **ALT-001**: Keep the current `*.domain.types.ts` / `*.request.types.ts` / `*.schema.ts` (lowercase) naming — rejected because it conflates response and domain concepts, lacks a dedicated Parameters bridge, and uses inconsistent file naming across modules
- **ALT-002**: Use a single monolithic `{Entity}.types.ts` file with all types in one place — rejected because it violates single-responsibility, makes imports imprecise, and creates merge conflicts in parallel work
- **ALT-003**: Place all types in a shared `types/` folder at feature-module level (flat) — rejected because per-entity `types/` directories provide better discoverability and prevent type-name collisions
- **ALT-004**: Keep `*.model.types.ts` as separate form-data bridge — rejected because `Parameters.Type.ts` now serves that role directly, derived from the schema

## 4. Dependencies

- **DEP-001**: `shared/api/types/query.types` must export `ServerQueryingParameters` (already exists)
- **DEP-002**: `shared/api/types/result.types` must export `ServerResult<T>`, `ServerPagedResult<T>` (already exists)
- **DEP-003**: Zod must be available across all feature modules (already in `package.json`)
- **DEP-004**: Vee-validate's `toTypedSchema` must remain compatible with Zod schema imports after file moves (uses `z.infer` so structurally unaffected)
- **DEP-005**: No external packages need to be added or upgraded

## 5. Files

- **FILE-001**: ~50 new `{Entity}.Schema.ts` files (one per entity, replacing ~10 current `*.schema.ts` files)
- **FILE-002**: ~200 new `{Entity}.{Parameters|Request|Response|Query}.Type.ts` files (4 per entity)
- **FILE-003**: ~15 deleted legacy `*.domain.types.ts` files
- **FILE-004**: ~15 deleted legacy `*.request.types.ts` files
- **FILE-005**: ~5 deleted legacy `*.model.types.ts` / `*.response.types.ts` files
- **FILE-006**: ~10 renamed schema files (lowercase → PascalCase `.Schema.ts`)
- **FILE-007**: ~80 import-path updates across services, stores, views, components, tests, repositories, mappers

## 6. Testing

- **TEST-001**: Schema validation tests must continue to pass after file moves (schema specs exist for taxonomy, taxon, option-type, option-value, property-type)
- **TEST-002**: Store spec imports must resolve to new type paths; store test assertions against `ServerResult.isSuccess` / `.value` must remain valid
- **TEST-003**: Service spec imports must resolve to new type paths
- **TEST-004**: `pnpm run test:unit` must pass with zero failures after each phase
- **TEST-005**: `pnpm run lint` must pass with zero errors after each phase
- **TEST-006**: TypeScript compiler must not report any `cannot find module` errors after all phases

## 7. Risks & Assumptions

- **RISK-001**: Circular imports may arise if `Parameters.Type.ts` imports schema and schema imports types — mitigated by keeping schema files as the single source of truth with no reverse imports
- **RISK-002**: Vee-validate form bindings use `defineField('fieldName')` which references schema field names directly; renaming schema exports may break these if any field names change — mitigated by preserving all Zod schema field names exactly
- **RISK-003**: Barrel exports (`index.ts` files) at the feature-module or shared level may re-export old paths — each phase must audit and update any `index.ts` re-exports
- **ASSUMPTION-001**: All existing entities follow the convention of having a `types/` and optionally `schemas/` directory — entities missing these (Fulfillment, Role, Permission, inventory sub-entities) need schema creation from scratch based on their service/store usage
- **ASSUMPTION-002**: The pattern applies uniformly to both root entities (Product) and child entities (Variant under Product, Taxon under Taxonomy, OptionValue under OptionType)
- **ASSUMPTION-003**: No runtime behavior changes — only file organization and import path changes

## 8. Related Specifications / Further Reading

- [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/)
- [Zod Documentation](https://zod.dev/)
- [Vee-Validate Zod Integration](https://vee-validate.logaretm.com/v4/integrations/zod/)
- `plan/refactor-api-to-repository-pattern-1.md` — prior Admin SPA data layer refactor
- `plan/refactor-types-decomposition-1.md` — prior type decomposition plan
