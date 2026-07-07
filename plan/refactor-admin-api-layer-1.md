---
goal: Restructure Admin SPA API layer and align frontend models with backend domain DTOs
version: 1.0
date_created: 2026-07-07
last_updated: 2026-07-07
status: 'Completed'
tags: refactor, api, admin, models, alignment
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

The Admin SPA `shared/api/` is a flat 7-file directory with no separation of concerns. Its `ApiResponse<T>` envelope and response interceptor assume a response shape (`data`, `meta`, `is_success`, `PaginationMeta` with `page_size`/`total_pages`/`has_next_page`) that **does not match** the actual server's `Result<T>`/`PagedResult<T>` output (`value`, `isSuccess`, `statusCode`, `items`, `page`, `pageSize`, `totalCount`). Feature models inconsistently mix snake_case and camelCase, and some have fields absent from the backend DTOs.

This plan corrects the response contract, reorganizes the API layer by concern, and aligns all frontend model types with the backend's actual JSON wire format (camelCase, `Result<T>`/`PagedResult<T>` envelope).

## 1. Requirements & Constraints

- **REQ-001**: All model property names must match the server's JSON serialization format (camelCase via `System.Text.Json` default).
- **REQ-002**: `ApiResponse<T>`, `PaginationMeta`, `ApiResult<T>`, and `PagedList<T>` must reflect the actual server `Result<T>`, `PagedResult<T>`, and `Error` response shapes.
- **REQ-003**: The response interceptor must unwrap `.value` (not `.data`) for `Result<T>` and `.items`/`.page`/`.pageSize`/`.totalCount` for `PagedResult<T>`.
- **REQ-004**: Error handling must accept the server's `Error` struct (`code`, `message`, `type`, `metadata`).
- **REQ-005**: All feature services must continue to work after the refactor (no breaking changes to their public interface).
- **CON-001**: `pnpm build-only` must pass with zero errors.
- **CON-002**: Backend domain entities must not be referenced — only DTOs (the *Request/*Response records under each feature's `Shared/Models/` directory).
- **GUD-001**: Follow the file-per-concern pattern already established (each concept gets its own `.ts` file).
- **GUD-002**: Use camelCase for all TypeScript property names (JSON wire format).

## 2. Implementation Steps

### Implementation Phase 1: Response envelope types & interceptor alignment

- GOAL-001: Correct `ApiResponse<T>`, `PaginationMeta`, `Error`, and `ApiResult<T>` types, and fix the Axios interceptor to unwrap the server's actual `Result<T>` / `PagedResult<T>` / `Error` shapes.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Replace `api.types.ts` `ApiResponse<T>` with actual server `Result<T>`/`PagedResult<T>` shape: `isSuccess`, `statusCode`, `message`, `errors`, `metadata`, `value` for Result, `items`/`page`/`pageSize`/`totalCount` for PagedResult | ✅ | 2026-07-07 |
| TASK-002 | Define `ServerError` type matching `Error` struct: `code: string`, `message: string`, `type: number`, `metadata?: Record<string, unknown>` | ✅ | 2026-07-07 |
| TASK-003 | Define `PaginationMeta` matching `PagedResult`: `page: number`, `pageSize: number`, `totalCount: number`, `totalPages: number` (computed) | ✅ | 2026-07-07 |
| TASK-004 | Rewrite `ApiResult<T>` discriminated union to correctly unwrap both success and error shapes from the server envelope | ✅ | 2026-07-07 |
| TASK-005 | Fix `api.client.ts` success handler: access `apiResponse.value` (not `data`), set `meta` from `page`/`pageSize`/`totalCount` when present | ✅ | 2026-07-07 |
| TASK-006 | Fix `api.client.ts` error handler: parse server `Error[]` from `errors` array into `Record<string, string[]>` for form validation | ✅ | 2026-07-07 |
| TASK-007 | Fix `api.client.ts` refresh logic: read `refreshResponse.data.value.accessToken` (not `refreshResponse.data.data.accessToken`) | ✅ | 2026-07-07 |
| TASK-008 | Update `parseApiError` in `api.utils.ts` to recognize `isSuccess`/`statusCode`/`errors` (camelCase) alongside existing snake_case fallback | ✅ | 2026-07-07 |
| TASK-009 | Update `api.utils.spec.ts` and `api.client.spec.ts` with new response shapes | ✅ | 2026-07-07 |
| TASK-010 | Run `pnpm build-only` and `pnpm test:unit` to verify no regressions | ✅ | 2026-07-07 |

### Implementation Phase 2: Structural reorganization of shared/api/

- GOAL-002: Split flat `shared/api/` into subdirectories by concern (HTTP transport, types, utilities, base service factories).

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-011 | Create `shared/api/http/` — move `api.client.ts` here, extract refresh-token retry logic into `refresh-handler.ts` | ✅ | 2026-07-07 |
| TASK-012 | Extract 401 interceptor retry logic from `api.client.ts` into `http/refresh-handler.ts` with dedicated `refreshTokens()` function | ✅ | 2026-07-07 |
| TASK-013 | Create `shared/api/types/` — move `api.types.ts` here, split into `result.types.ts` (Result/PagedResult/Error types) and `api.types.ts` (ApiResult, service response types) | ✅ | 2026-07-07 |
| TASK-014 | Create `shared/api/utils/` — move `api.utils.ts` here | ✅ | 2026-07-07 |
| TASK-015 | Create `shared/api/services/` — add `crud.service.ts` with generic CRUD factory (`createCrudService<T, TCreate, TUpdate>(basePath)` → `{ list, getById, create, update, delete }`) | ✅ | 2026-07-07 |
| TASK-016 | Create barrel `shared/api/index.ts` re-exporting all public types and the `apiClient` | ✅ | 2026-07-07 |
| TASK-017 | Update all feature service imports from `@/shared/api/api.client` and `@/shared/api/api.types` to new paths | ✅ | 2026-07-07 |
| TASK-018 | Update `shared/composables/api-error-handler.use.ts` to use new `ServerError` type | ✅ | 2026-07-07 |
| TASK-019 | Run `pnpm build-only` and `pnpm test:unit` to verify | ✅ | 2026-07-07 |

### Implementation Phase 3: Domain model alignment — Catalog module

- GOAL-003: Align all Catalog feature types with backend `ProductParameters`, `ProductDetailResponse`, `VariantParameters`, `OptionTypeParameters`, `TaxonomyParameters`, `TaxonParameters`, etc.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-020 | `ProductSummary`: remove `is_active`/`is_visible`/`is_digital`, add `status: ProductStatus` (union `'Draft'|'Active'|'Archived'`), rename `created_at`→`createdAtUtc`, `updated_at`→`modifiedAtUtc`, `variant_count`→`variantsCount`, `image_url`→`imageUrl` | ✅ | 2026-07-07 |
| TASK-021 | `ProductDetail`: align with `ProductDetailResponse` — remove `brand`/`public_metadata`/`private_metadata` (not in backend DTO), rename `meta_title`→`metaTitle`, etc. | ✅ | 2026-07-07 |
| TASK-022 | `CreateProductRequest`/`UpdateProductRequest`: align with `ProductRequest` — use `availableOn`/`discontinueOn` instead of `is_active`/`is_visible`, `trackInventory` | ✅ | 2026-07-07 |
| TASK-023 | `ProductImage`: align with `VariantImageResponse` — rename `product_id`→`productId`, `variant_id`→`variantId`, `file_size`→`fileSize`, `is_default`→`isDefault`, `status`→ proper `VariantImageType` union | ✅ | 2026-07-07 |
| TASK-024 | `ProductClassification`: align with backend `ProductClassification` model | ✅ | 2026-07-07 |
| TASK-025 | `ProductProperty`: keep as-is (simple key-value, backend has no complex DTO for this) | ✅ | 2026-07-07 |
| TASK-026 | `VariantSummary`/`VariantDetail`: align with `VariantListItemResponse`/`VariantDetailResponse` — rename `compare_at_price`→`compareAtPrice`, `cost_price`→`costPrice`, `is_master`→`isMaster`, `track_inventory`→`trackInventory`, `option_value_ids`→`optionValueIds`, add `weightUnit`/`dimensionsUnit` | ✅ | 2026-07-07 |
| TASK-027 | `OptionTypeListItem`/`OptionTypeDetail`: align with backend — ensure `filterable`, `optionValuesCount`, `productsCount` fields | ✅ | 2026-07-07 |
| TASK-028 | `OptionValueListItem`: align — `optionTypeId`, `position` | ✅ | 2026-07-07 |
| TASK-029 | `TaxonomyListItem`/`TaxonomyDetail`: align with backend — `taxonsCount`, `createdAtUtc`, `modifiedAtUtc` | ✅ | 2026-07-07 |
| TASK-030 | `TaxonListItem`/`TaxonDetail`/`TaxonTreeItem`: align with backend — use `lft`/`rgt`/`depth`, `permalink`, `prettyName`, `childrenCount`, `productCount`, `taxonRuleCount`, `hideFromNav`, `rulesMatchPolicy`, `sortOrder` | ✅ | 2026-07-07 |
| TASK-031 | `TaxonRuleListItem`: align with `TaxonRuleResponse` — `type`, `value`, `matchPolicy` | ✅ | 2026-07-07 |
| TASK-032 | Delete unused types and files, run `pnpm build-only` and `pnpm test:unit` | ✅ | 2026-07-07 |

### Implementation Phase 4: Domain model alignment — Other modules

- GOAL-004: Align Identity, Location, Profile, Ordering, Inventory, and Report types with their respective backend DTOs.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-033 | `AdminUserSummary`: rename `user_name`→`userName`, `first_name`→`firstName`, `last_name`→`lastName`, `full_name`→`fullName`, `role_names`→`roleNames`, `is_active`→`isActive`, `created_at`→`createdAtUtc`, `phone_number`→`phoneNumber`, `email_confirmed`→`emailConfirmed`, `phone_number_confirmed`→`phoneNumberConfirmed`, `access_failed_count`→`accessFailedCount`, `lockout_end`→`lockoutEnd`, `last_sign_in_at`→`lastSignInAtUtc`, `last_ip_address`→`lastIpAddress` | ✅ | 2026-07-07 |
| TASK-034 | `RoleSummary`: rename `display_name`→`displayName`, `is_system_role`→`isSystem`, `is_default`→`isDefault`, `user_count`→`userCount` | ✅ | 2026-07-07 |
| TASK-035 | `PermissionSummary`: align with `PermissionResponse(Identifier, Name, Description, Action)` | ✅ | 2026-07-07 |
| TASK-036 | `CustomerSummary`: rename `first_name`→`firstName`, `last_name`→`lastName`, `full_name`→`fullName`, `order_count`→`ordersCount`, `total_spent_cents`→`totalSpent`, `is_active`→`isActive`, `created_at`→`createdAtUtc` | ✅ | 2026-07-07 |
| TASK-037 | `CreateAdminUserRequest`/`UpdateAdminUserRequest`: align with `UserRequest` — use `firstName`/`lastName`/`phoneNumber`/`emailConfirmed`/`phoneNumberConfirmed` | ✅ | 2026-07-07 |
| TASK-038 | `Country`: rename `isoCode2`→`isoCode`, remove `isoCode3`/`numericCode`, add `statesRequired`, `zipcodeRequired`, `callingCode`, `createdAtUtc`, `modifiedAtUtc` | ✅ | 2026-07-07 |
| TASK-039 | `CountryCreateRequest`/`CountryUpdateRequest`: align with `CountryRequest` (remove extra fields) | ✅ | 2026-07-07 |
| TASK-040 | `State`: rename `countryId`→`countryId` (already correct), add `countryName` | ✅ | 2026-07-07 |
| TASK-041 | `Profile`: add `firstName`, `lastName`, `dateOfBirth`, `gender`, `bio`, `avatarUrl`, `preferences`, `notifications`, `isActive`, `acceptsEmailMarketing`, `createdAtUtc`, `modifiedAtUtc` — align with `ProfileDetailResponse` | ✅ | 2026-07-07 |
| TASK-042 | `ProfileUpdateRequest`: align with `ProfileRequest` — add `dateOfBirth`, `preferences`, `notifications` | ✅ | 2026-07-07 |
| TASK-043 | `OrderListItem`/`OrderDetail`: rename `total_cents`→`totalCents`, `total_display`→`totalDisplay`, `created_at`→`createdAtUtc`, `item_total_cents`→`itemTotal`, etc. Add `paymentState`, `shipmentState`, `email`, `currency` — align with backend order response (verify against actual backend OrderResponse DTO) | ✅ | 2026-07-07 |
| TASK-044 | `AddressDetail`: rename `firstname`→`firstName`, `lastname`→`lastName`, `zipcode`→`zipCode`, `country_code`→`countryCode`, `state_code`→`stateCode` | ✅ | 2026-07-07 |
| TASK-045 | `LineItemDetail`: rename `variant_id`→`variantId`, `unit_price_cents/display`→`unitPriceCents/unitPriceDisplay`, adjust structure per backend OrderItemResponse | ✅ | 2026-07-07 |
| TASK-046 | `StockLocation`: rename `is_default`→`isDefault`, add `position`, `backorderableDefault`, `propagateAllVariants`, `lowStockThreshold` | ✅ | 2026-07-07 |
| TASK-047 | `StockItem`: rename `quantity_on_hand`→`countOnHand`, `quantity_reserved`→ null (backend doesn't expose reserved directly on items), `count_available`→ computed | ✅ | 2026-07-07 |
| TASK-048 | `StockTransfer`: rename `reference_number`→`number`/`reference`, `source_location_id/name`→`sourceLocationId/Name`, `status`→`state` with `TransferState` union (`'Draft'|'InTransit'|'Received'|'Canceled'`) | ✅ | 2026-07-07 |
| TASK-049 | `InventoryUnit`: align `state` with backend `ReservationState` | ✅ | 2026-07-07 |
| TASK-050 | `StockMovement`: rename `balance_before`→`previousCountOnHand`, `balance_after`→ null, `unit_cost`→ null, `type`→`action` | ✅ | 2026-07-07 |
| TASK-051 | `SalesSummary`/`CatalogSummary`/`ActivityItem`: align field naming to camelCase | ✅ | 2026-07-07 |
| TASK-052 | Run `pnpm build-only` and `pnpm test:unit` to verify | ✅ | 2026-07-07 |

### Implementation Phase 5: Service layer consolidation

- GOAL-005: Refactor feature services to use shared CRUD factory and path constants, fix URL prefix inconsistencies.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-053 | Define path constants in `shared/api/constants.ts` — `API_PREFIX`, module base paths (`CATALOG`, `IDENTITY`, `LOCATION`, `PROFILE`, `INVENTORY`, `ORDERS`) | ✅ | 2026-07-07 |
| TASK-054 | Refactor simple CRUD services (option-types, option-values, property-types, taxonomies, taxons, countries, states) to use `createCrudService()` factory | ✅ | 2026-07-07 |
| TASK-055 | Standardize URL prefixes: change `/api/admin/location/*` to `/admin/location/*` (remove inconsistent `/api/` prefix) | ✅ | 2026-07-07 |
| TASK-056 | Run `pnpm build-only` and `pnpm test:unit` | ✅ | 2026-07-07 |

## 3. Alternatives

- **ALT-001**: Keep flat `shared/api/` and just fix types — rejected because the file count is already growing and cross-cutting concerns (HTTP config, types, utils) should be separated.
- **ALT-002**: Keep `ApiResponse<T>` as-is and add a server-side middleware to transform `Result<T>` into the expected `ApiResponse<T>` shape — rejected because this adds server-side complexity and hides the mismatch. Better to align the frontend with the actual wire format.
- **ALT-003**: Use a code generator (OpenAPI → TypeScript) to auto-generate types — rejected as too heavy for now; the backend doesn't export a stable OpenAPI spec yet. Manual alignment is more practical.

## 4. Dependencies

- **DEP-001**: Understanding of the server JSON serialization (no custom naming policy, uses `JsonSerializerDefaults.Web` = camelCase).
- **DEP-002**: All server DTOs under `Features/{Module}/Admin/{Feature}/Shared/Models/` (the `*Response` and `*Request` records — not domain entities).

## 5. Files

- **FILE-001**: `app/Admin/src/shared/api/` — restructured into `http/`, `types/`, `utils/`, `services/`, `constants.ts`
- **FILE-002**: `app/Admin/src/shared/api/api.types.ts` — replaced with correct `Result<T>/PagedResult<T>/ServerError` types
- **FILE-003**: `app/Admin/src/shared/api/api.client.ts` — interceptor fixed for `value`/`items` unwrapping + `Error[]` parsing
- **FILE-004**: `app/Admin/src/shared/api/api.utils.ts` — `parseApiError` updated for server error struct format
- **FILE-005 through FILE-020**: All feature `types/*.types.ts` files in `features/catalog/`, `features/users/`, `features/location/`, `features/profile/`, `features/ordering/`, `features/inventories/`, `features/reports/`, `features/auth/`
- **FILE-021 through FILE-040**: All feature `services/*.service.ts` files (import paths updated + potential simplification)

## 6. Testing

- **TEST-001**: Unit tests in `shared/api/api.client.spec.ts` — assert interceptor unwraps `value` and `items`/`page`/`pageSize`/`totalCount` correctly
- **TEST-002**: Unit tests in `shared/api/api.utils.spec.ts` — assert `parseApiError` handles `Error[]` format
- **TEST-003**: All existing feature unit tests (`vitest`) — run `pnpm test:unit` to confirm no regressions
- **TEST-004**: Build validation — `pnpm build-only` must pass with zero errors

## 7. Risks & Assumptions

- **RISK-001**: Some feature service return types use `as any` assertions that mask type mismatches. Fixed types may expose these in template accesses, requiring template-level fixes in Phase 3-4.
- **RISK-002**: The refresh token endpoint response may use `data` instead of `value` under certain error paths. Verify by reading the actual handler response shape.
- **ASSUMPTION-001**: All server DTOs use `System.Text.Json` defaults (camelCase). If a custom naming policy is added later, this plan's type names will break.
- **ASSUMPTION-002**: Feature pages use the service layer exclusively, never calling `apiClient` directly — this should be verified during Phase 2 import rewrites.

## 8. Related Specifications / Further Reading

- `service/Api/src/Shared/Application/Models/Results/` — `Result.cs`, `ValueResult.cs`, `PagedResult.cs`, `Error.cs`
- `service/Api/src/Shared/Application/Extensions/Results/Result.Http.Extensions.cs` — response serialization
- `service/Api/src/Module/*/Features/Admin/*/Shared/Models/` — all backend DTOs per module
- `docs/codebase/ARCHITECTURE.md` — layer responsibilities
