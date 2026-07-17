---
goal: Migrate All Admin SPA Request/Response DTOs from snake_case to camelCase
version: 1.0
date_created: 2026-07-17
last_updated: 2026-07-17
status: 'Completed'
tags: [refactor, migration, admin-spa, casing, typescript]
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

The .NET backend serializes all JSON responses in **camelCase** (ASP.NET Core default `JsonNamingPolicy.CamelCase`), but the Admin SPA currently defines its DTOs and interfaces with **snake_case** property names. This creates a disconnect: the SPA casts raw HTTP responses into types that don't match the actual data shape, relying on manual mappers in some places (auth) and implicit type assertions in others.

This plan migrates **all** Admin SPA DTOs, request bodies, test mocks, and response-handling code from snake_case to camelCase, so internal TypeScript types align with the actual backend JSON wire format. The existing but unused `toCamelCaseKeys` utility in `mapper.utils.ts` will be wired into the Axios response interceptor as a defensive safety net.

## 1. Requirements & Constraints

- **REQ-001**: All DTO interfaces must use camelCase property names matching the backend's JSON output
- **REQ-002**: All test mock data must use camelCase keys
- **REQ-003**: Manual snake_case→camelCase mappers must be removed (e.g. `mapAuthResponse`)
- **REQ-004**: Manual camelCase→snake_case request body conversions must be removed (e.g. `changePassword`)
- **REQ-005**: `toCamelCaseKeys()` from `mapper.utils.ts` must be wired into the Axios response interceptor to auto-convert any remaining snake_case from exceptional backend paths
- **CON-001**: No breaking changes to feature page rendering or service behavior — all existing functionality must work identically after migration
- **CON-002**: TreatWarningsAsErrors — zero new lint/type errors introduced
- **GUD-001**: All migration changes must be testable in isolation per-DTO before proceeding to next DTO
- **GUD-002**: PascalCase file naming convention (`*.Type.ts`, `*.Schema.ts`, `*.Service.ts`) maintained
- **PAT-001**: Follow existing type definition patterns in `features/*/types/`

## 2. Implementation Steps

### Implementation Phase 1: Wire Global Safety Net in Response Interceptor

GOAL-001: Add `toCamelCaseKeys` conversion to Axios response interceptor so any snake_case keys from the backend are automatically converted to camelCase, ensuring existing code that reads snake_case properties continues to work during the incremental migration.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Import `toCamelCaseKeys` from `@/shared/mapper/mapper.utils.ts` in `shared/api/http/api.client.ts` | | |
| TASK-002 | Add `toCamelCaseKeys(response.data)` call in the success response interceptor (line 37-39) before `return response` | | |
| TASK-003 | Apply `toCamelCaseKeys` also in the error interceptor's constructed `ServerResult<null>` responses to ensure `error_code` becomes `errorCode` | | |
| TASK-004 | Run existing test suite (`pnpm run test:unit` in `app/Admin`) — all tests must pass with interceptor enabled | | |

### Implementation Phase 2: Convert Auth DTO & Remove Manual Auth Mapper

GOAL-002: Convert `AuthDto` from snake_case to camelCase, update `mapAuthResponse` to be a no-op, update all auth consumers (refresh handler, auth service, repository, tests), then remove the mapper entirely.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | Replace `AuthDto` interface in `features/auth/mappers/auth.mapper.ts` with camelCase equivalents: `accessToken`, `accessTokenExpiresIn`, `refreshToken`, `refreshTokenExpiresIn` | | |
| TASK-006 | Remove `mapAuthResponse` function — `AuthDto` properties now match `AuthenticationResponse` properties directly; auth service returns `res.data.value` directly without mapping | | |
| TASK-007 | Update `features/auth/services/auth.service.ts:39` — remove dual-case fallback `data.fullName \|\| data.full_name`, use only `data.fullName` | | |
| TASK-008 | Update `shared/api/http/refresh-handler.ts:19-20` — change `value['access_token']` and `value['refresh_token']` to `value.accessToken` and `value.refreshToken` | | |
| TASK-009 | Update auth test mock data in `features/auth/_tests/auth.service.spec.ts:29-32,57` — replace snake_case keys with camelCase | | |
| TASK-010 | Verify all auth-related tests pass | | |

### Implementation Phase 3: Convert File API DTOs

GOAL-003: Convert `FileMetadata` and `FileUploadResponse` interfaces in `shared/api/types/api.file.types.ts` from snake_case to camelCase.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-011 | Rename `file_id` → `fileId`, `file_name` → `fileName`, `original_file_name` → `originalFileName`, `file_size` → `fileSize`, `content_type` → `contentType`, `created_at` → `createdAt`, `is_encrypted` → `isEncrypted`, `modified_at` → `modifiedAt`, `custom_metadata` → `customMetadata` in `FileMetadata` interface | | |
| TASK-012 | Rename `size_bytes` → `sizeBytes`, `saved_name` → `savedName` in `FileUploadResponse` interface | | |
| TASK-013 | Search for all consumers of `FileMetadata` and `FileUploadResponse` properties — update any dot-access to use camelCase | | |

### Implementation Phase 4: Convert Error Handling Types

GOAL-004: Convert `error_code` → `errorCode` in `ParsedApiError` and `FailureResult` interfaces, update all error handling paths.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-014 | Rename `error_code` → `errorCode` in `shared/api/utils/api.utils.ts` `ParsedApiError` interface (line 10) and `parseApiError` function (lines 66, 103) | | |
| TASK-015 | Rename `error_code` → `errorCode` in `shared/api/types/api.types.ts` `FailureResult` interface (line 18) | | |
| TASK-016 | Rename `error_code` → `errorCode` in `shared/api/utils/result.mapper.ts` `FailureResult` interface (line 18) | | |
| TASK-017 | Update `shared/composables/api-error-handler.use.spec.ts:30,142,145` mock data from `error_code` to `errorCode` | | |
| TASK-018 | Update `shared/api/utils/api.utils.spec.ts:85,88-95,103,109` mock data from `error_code` to `errorCode` | | |
| TASK-019 | Update `shared/api/http/api.client.spec.ts:14,114` mock data from `error_code` to `errorCode` | | |
| TASK-020 | Run all shared API tests — must pass | | |

### Implementation Phase 5: Convert Feature-Level DTOs & Inline Conversions

GOAL-005: Convert remaining feature-level snake_case DTOs, request body constructions, and response type assumptions across Catalog, Ordering, Reports, and Search.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-021 | Convert `InventoryStockItem` in `features/catalog/products/components/ProductInventoryManager.Component.vue:22-31` — rename `variant_name` → `variantName`, `stock_location_name` → `stockLocationName`, `quantity_on_hand` → `quantityOnHand`, `quantity_reserved` → `quantityReserved` | | |
| TASK-022 | Convert `GlobalSearchResult` in `shared/services/search.service.ts:4-10` — rename `route_name` → `routeName` | | |
| TASK-023 | Convert `total_count` → `totalCount` in `features/catalog/taxonomies/taxa/services/taxon.service.ts:19` return type | | |
| TASK-024 | Remove manual request body conversion in `features/auth/repositories/auth.repository.ts:41-43` — replace `current_password`, `new_password`, `confirm_new_password` with camelCase `currentPassword`, `newPassword`, `confirmNewPassword` (now matches backend directly) | | |
| TASK-025 | Convert request body keys in `features/catalog/taxonomies/taxa/components/TaxonForm.Component.vue:113-114` — `public_metadata` → `publicMetadata`, `private_metadata` → `privateMetadata` | | |
| TASK-026 | Convert `option_values` → `optionValues`, `name_suffix` → `nameSuffix`, `sku_suffix` → `skuSuffix`, `price_offset` → `priceOffset` in `features/catalog/products/components/dialogs/VariantGenerationDialog.Component.vue:93-96,124` | | |

### Implementation Phase 6: Update Test Mocks

GOAL-006: Update all remaining test files containing snake_case mock data (outside auth and error handling, which were already handled).

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-027 | Update `features/ordering/tests/order.store.spec.ts:63` — `line_items` → `lineItems` | | |
| TASK-028 | Update `shared/utils/query-builder.utils.spec.ts:134-139` — `category_name` → `categoryName`, `is_active` → `isActive` | | |
| TASK-029 | Run full Admin test suite (`pnpm run test:unit`) — all tests must pass | | |

### Implementation Phase 7: Remove Unused Code & Final Cleanup

GOAL-007: Remove dead code, verify zero snake_case remains, run lint + typecheck + full test suite.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-030 | Remove `toSnakeCaseKeys` and `mapDto` from `shared/mapper/mapper.utils.ts` — these were never used | | |
| TASK-031 | Run `pnpm run lint` — zero new errors | | |
| TASK-032 | Run `pnpm run test:unit` — full suite green | | |
| TASK-033 | Grep entire `app/Admin/src` for `_[a-z]` patterns in interface/type definitions to confirm zero remaining snake_case property names | | |

## 3. Alternatives

- **ALT-001 (Global interceptor only, no DTO changes)**: Wire `toCamelCaseKeys` into the interceptor and leave DTOs as-is. Rejected because it creates mental overhead — developers reading DTOs would see snake_case but the runtime values would be camelCase after interception.
- **ALT-002 (DTO changes only, no interceptor)**: Update all DTOs manually without wiring `toCamelCaseKeys` into the interceptor. Rejected because it's fragile — any backend endpoint that still returns snake_case (e.g., third-party integrations, Identity endpoints) would silently break.
- **ALT-003 (Use a library like `humps`)**: Install an npm package for case conversion instead of the hand-rolled `mapper.utils.ts`. Rejected because the hand-rolled utility already exists, is simple, has zero dependencies, and adding a library for 10 lines of code is unnecessary.

## 4. Dependencies

- **DEP-001**: `shared/mapper/mapper.utils.ts` — `toCamelCaseKeys` function must be imported into `api.client.ts` (TASK-001)
- **DEP-002**: All DTO interfaces must be updated before their consumers can drop dual-case fallback logic (e.g. TASK-007 depends on TASK-005)
- **DEP-003**: Test mock updates (Phase 6) must happen after corresponding DTO changes (Phases 2-5)
- **DEP-004**: The `toCamelCaseKeys` interceptor is lossy — if two different snake_case keys map to the same camelCase key (e.g. `is_active` and `isActive` both → `isActive`), the interceptor must be configured to handle duplicates gracefully. The current implementation uses last-wins, which is fine since API responses should never mix casing for the same logical field.

## 5. Files

- **FILE-001**: `app/Admin/src/shared/mapper/mapper.utils.ts` — remove dead `toSnakeCaseKeys`, `mapDto`; keep `toCamelCase`, `toCamelCaseKeys`
- **FILE-002**: `app/Admin/src/shared/api/http/api.client.ts` — wire `toCamelCaseKeys` into response interceptor
- **FILE-003**: `app/Admin/src/shared/api/http/refresh-handler.ts` — camelCase property access
- **FILE-004**: `app/Admin/src/shared/api/types/api.file.types.ts` — 11 snake_case → camelCase renames
- **FILE-005**: `app/Admin/src/shared/api/types/api.types.ts` — `error_code` → `errorCode`
- **FILE-006**: `app/Admin/src/shared/api/utils/api.utils.ts` — `error_code` → `errorCode`
- **FILE-007**: `app/Admin/src/shared/api/utils/result.mapper.ts` — `error_code` → `errorCode`
- **FILE-008**: `app/Admin/src/shared/services/search.service.ts` — `route_name` → `routeName`
- **FILE-009**: `app/Admin/src/features/auth/mappers/auth.mapper.ts` — DTO conversion + remove mapper
- **FILE-010**: `app/Admin/src/features/auth/services/auth.service.ts` — drop dual-case fallback
- **FILE-011**: `app/Admin/src/features/auth/repositories/auth.repository.ts` — drop manual request body conversion
- **FILE-012**: `app/Admin/src/features/auth/_tests/auth.service.spec.ts` — mock data casing
- **FILE-013**: `app/Admin/src/features/catalog/products/components/ProductInventoryManager.Component.vue` — `InventoryStockItem` casing
- **FILE-014**: `app/Admin/src/features/catalog/products/components/dialogs/VariantGenerationDialog.Component.vue` — request body casing
- **FILE-015**: `app/Admin/src/features/catalog/taxonomies/taxa/components/TaxonForm.Component.vue` — request body casing
- **FILE-016**: `app/Admin/src/features/catalog/taxonomies/taxa/services/taxon.service.ts` — return type casing
- **FILE-017**: `app/Admin/src/shared/composables/api-error-handler.use.spec.ts` — test mock casing
- **FILE-018**: `app/Admin/src/shared/api/utils/api.utils.spec.ts` — test mock casing
- **FILE-019**: `app/Admin/src/shared/api/http/api.client.spec.ts` — test mock casing
- **FILE-020**: `app/Admin/src/features/ordering/tests/order.store.spec.ts` — test mock casing
- **FILE-021**: `app/Admin/src/shared/utils/query-builder.utils.spec.ts` — test mock casing

## 6. Testing

- **TEST-001**: All existing auth service tests pass with camelCase mock data (TASK-010)
- **TEST-002**: All shared API utility/error handler tests pass (TASK-020)
- **TEST-003**: Full Admin unit test suite passes (`pnpm run test:unit`) (TASK-029, TASK-032)
- **TEST-004**: Manual smoke test — login flow works (tokens stored and refreshed correctly)
- **TEST-005**: Manual smoke test — file upload/download works (metadata properties accessible)
- **TEST-006**: Manual smoke test — error notifications display correctly (errorCode accessible)
- **TEST-007**: Lint zero errors (`pnpm run lint`) (TASK-031)
- **TEST-008**: Grep confirmation — zero snake_case property names in `*.ts`, `*.vue` files under `app/Admin/src`

## 7. Risks & Assumptions

- **RISK-001**: Backend Identity endpoints (login, refresh, password reset) may be handled by a different middleware that uses snake_case. If `toCamelCaseKeys` interceptor is active, this is mitigated — the interceptor converts all response data regardless of endpoint. **Mitigation**: Interceptor handles this transparently.
- **RISK-002**: Some backend DTOs may have properties whose camelCase equivalents collide (e.g. `is_active` → `isActive` vs `isActive` → `isActive` — both map to same key). **Mitigation**: `mapKeys` uses last-wins, and the backend should not mix casing within a single response. This is a non-issue in practice.
- **RISK-003**: The `toCamelCaseKeys` interceptor will also convert nested objects and arrays — this is intended behavior and matches what the backend actually sends. **Mitigation**: If a property name coincidentally contains underscores but should not be converted (e.g. `my_key` → `myKey`), this is the desired behavior — it aligns with the backend's camelCase output.
- **ASSUMPTION-001**: The backend always returns camelCase JSON for all standard API endpoints (validated by inspection of ASP.NET Core JSON options — no override found)
- **ASSUMPTION-002**: No third-party API calls from the Admin SPA return snake_case that should be preserved (Auth0, Firebase, etc. — none found in codebase)
- **ASSUMPTION-003**: Store variables (`is_loading`, `current_product`, etc.) are a separate concern from API casing and are not part of this migration — they are internal Vue state, not DTOs

## 8. Related Specifications / Further Reading

- [Auth snake_case fix implementation plan](../../plan/refactor-auth-snake-case-mapping-1.md)
- [Admin layout migration implementation plan](../../docs/superpowers/plans/2026-07-17-admin-layout-migration.md)
- [Shared mapper utilities](app/Admin/src/shared/mapper/mapper.utils.ts)
- [ASP.NET Core JSON serialization defaults](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-results?view=aspnetcore-10.0)
