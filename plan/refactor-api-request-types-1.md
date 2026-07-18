---
goal: Replace all inline/anonymous object types in api/*.api.ts with named Request/Response model imports
version: 1.0
date_created: 2026-07-18
owner: Agent
status: Planned
tags: refactor, admin-spa, types, api-layer
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Every method parameter and return type in `api/*.api.ts` files must use named Request/Response model types instead of inline object literals or `Record<string, unknown>`. Create missing type files where none exist. Fix 14 api files with ~28 inline type definitions.

## 1. Requirements & Constraints

- **REQ-001**: Every method parameter in every `api/*.api.ts` uses a named type from the entity's `types/` directory
- **REQ-002**: No `Record<string, unknown>` in method signatures — replace with concrete named types
- **REQ-003**: Create missing `*.Request.Type.ts` files where request types don't exist
- **REQ-004**: Import existing request types that currently go unused (option-type, option-value, order addresses, profile addresses)
- **REQ-005**: Nested inline types (e.g., `Array<{ id: string; quantity: number; type: number }>`) get extracted to named interfaces
- **REQ-006**: Return types must use named types, never inline (`Promise<ServerResult<{ id: string }>>` → `Promise<ServerResult<SomeResponse>>`)
- **CON-001**: Never break existing imports — new type files must not change existing exports
- **CON-002**: New type files follow existing naming convention: `{Entity}.Request.Type.ts`, `{Entity}.Response.Type.ts`
- **CON-003**: Request/Response types co-located with their entity (e.g., `stock-items/types/`, `variants/images/types/`)
- **GUD-001**: Use `Pick<T, K>`, `Omit<T, K>`, or `Partial<T>` for variants of existing types where appropriate
- **GUD-002**: Every api file must pass `vue-tsc --build` after changes

## 2. Implementation Steps

### Phase 1: Create missing request type files

- GOAL-001: Create new *.Request.Type.ts files for entities that have inline request types but no type file

| Task | Entity | Create File | Contents |
|------|--------|------------|----------|
| **TYP-001** | variants/images | `variants/images/types/Image.Request.Type.ts` | `export interface UpdateVariantImageRequest { alt?: string; role?: number }` |
| **TYP-002** | variants/prices | `variants/prices/types/Price.Request.Type.ts` | `export interface SetVariantPriceRequest { amount: number; currency: string }` + `export interface SyncVariantPricesRequest { prices: SetVariantPriceRequest[] }` |
| **TYP-003** | products/classifications | `products/classifications/types/Classification.Request.Type.ts` | `export interface SyncClassificationsRequest { taxonIds: string[]; mainTaxonId?: string }` |
| **TYP-004** | stock-items | `stock-items/types/StockItem.Request.Type.ts` (add to existing or extend) | `export interface CreateStockItemRequest { variantId: string; stockLocationId: string; countOnHand?: number }` + `export interface UpdateStockItemRequest { countOnHand?: number; backorderable?: boolean; backorderLimit?: number }` + `export interface BulkAdjustItem { id: string; quantity: number; type: number }` + `export interface BulkAdjustRequest { items: BulkAdjustItem[] }` |
| **TYP-005** | orders | `ordering/orders/types/Order.Request.Type.ts` (add to existing) | `export interface UpdateLineItemRequest { quantity?: number }` + `export interface UpdateOrderStatusRequest { status: string }` + `export interface UpdateAddressesRequest { shippingAddress?: Partial<OrderParameters['shippingAddress']>; billingAddress?: Partial<OrderParameters['billingAddress']> }` |
| **TYP-006** | auth | `features/auth/types/Auth.Request.Type.ts` (add to existing) | `export interface RefreshTokenRequest { refreshToken: string; rememberMe?: boolean }` + `export interface UpdateProfileRequest { email?: string; fullName?: string; phone?: string }` + `interface AuthProfileResponse { id: string; email: string; fullName: string; roles: string[] }` |
| **TYP-007** | users | `users/types/User.Request.Type.ts` (add to existing) | `export interface AssignRoleRequest { roleName: string }` + `export interface SyncRolesRequest { roleNames: string[] }` + `export interface AssignPermissionRequest { permissionName: string }` + `export interface SyncPermissionsRequest { permissionNames: string[] }` + `export interface UpdateUserStatusRequest { isActive: boolean }` |
| **TYP-008** | roles | `users/roles/types/Role.Request.Type.ts` (add to existing) | `export interface AssignRolePermissionRequest { permissionName: string }` + `export interface SyncRolePermissionsRequest { permissionNames: string[] }` |

### Phase 2: Update api files to use named types

- GOAL-002: Every method in 14 api files changes from inline types to imported named types

| Task | Api File | Changes Required |
|------|----------|-----------------|
| **API-001** | `option-types/api/option-type.api.ts` | Import `CreateOptionTypeRequest`, `UpdateOptionTypeRequest` from `../types/OptionType.Request.Type`. Replace inline `{ name: string; presentation: string; filterable?: boolean; position?: number }` with `CreateOptionTypeRequest`. Replace inline `Partial<{...}>` with `UpdateOptionTypeRequest`. |
| **API-002** | `option-values/api/option-value.api.ts` | Import `CreateOptionValueRequest`, `UpdateOptionValueRequest` from `../types/OptionValue.Request.Type`. Replace inline `{ name: string; presentation: string; position?: number }` with `CreateOptionValueRequest`. Replace inline `{ name?: string; presentation?: string; position?: number }` with `UpdateOptionValueRequest`. |
| **API-003** | `variants/images/api/image.api.ts` | Import `UpdateVariantImageRequest` from `../types/Image.Request.Type`. Replace inline `data: { alt?: string; role?: number }` with `data: UpdateVariantImageRequest`. |
| **API-004** | `variants/prices/api/price.api.ts` | Import `SetVariantPriceRequest`, `SyncVariantPricesRequest` from `../types/Price.Request.Type`. Replace inline `data: { amount: number; currency: string }` with `data: SetVariantPriceRequest`. Replace inline `prices: Array<{ amount: number; currency: string }>` with `prices: SetVariantPriceRequest[]`. |
| **API-005** | `products/classifications/api/product-classification.api.ts` | Import `SyncClassificationsRequest` from `../types/Classification.Request.Type`. Replace inline `data: { taxonIds: string[]; mainTaxonId?: string }` with `data: SyncClassificationsRequest`. |
| **API-006** | `stock-items/api/stock.api.ts` | Import `CreateStockItemRequest`, `UpdateStockItemRequest`, `BulkAdjustRequest` from `../types/StockItem.Request.Type`. Replace inline types in `create`, `update`, `bulkAdjust`. |
| **API-007** | `stock-locations/api/location.api.ts` | Change `update` from `data: Partial<CreateStockLocationRequest>` to `data: UpdateStockLocationRequest` (add `export type UpdateStockLocationRequest = Partial<CreateStockLocationRequest>` to the Request type file if not exists). |
| **API-008** | `orders/api/order.api.ts` | Import `UpdateLineItemRequest`, `UpdateOrderStatusRequest`, `UpdateAddressesRequest`, `CancelOrderRequest` from `../types/Order.Request.Type`. Replace `Record<string, unknown>` in `updateShipAddress`/`updateBillAddress`. Replace inline `{ status }` in `updateStatus`. Replace inline `{ reason }` in `cancel`. Replace inline `{ quantity?: number }` in `updateLineItem`. Replace inline `{ shippingMethodId }` in `updateShippingMethod`. |
| **API-009** | `payment-methods/api/payment-method.api.ts` | Import `UpdatePaymentMethodRequest` (already exists on disk) and use it in `update` instead of `Partial<CreatePaymentMethodRequest>`. |
| **API-010** | `users/api/user.api.ts` | Import `UpdateUserStatusRequest`, `AssignRoleRequest`, `SyncRolesRequest`, `AssignPermissionRequest`, `SyncPermissionsRequest`. Replace all inline `{ roleName }`, `{ roleNames }`, `{ permissionName }`, `{ permissionNames }`, `{ isActive }` payloads. |
| **API-011** | `roles/api/role.api.ts` | Import `AssignRolePermissionRequest`, `SyncRolePermissionsRequest`. Replace all inline payloads. |
| **API-012** | `addresses/api/address.api.ts` | Import `CreateAddressRequest`, `UpdateAddressRequest` from `../types/Address.Request.Type`. Replace `Partial<AddressDetail>` in `create` and `update`. |
| **API-013** | `auth/api/auth.api.ts` | Import `RefreshTokenRequest`, `UpdateProfileRequest`, `AuthProfileResponse` from `../types/Auth.Request.Type`. Replace inline `{ refreshToken: string; rememberMe?: boolean }` with `RefreshTokenRequest`. Replace `Record<string, unknown>` in `getProfile` return type with `ServerResult<AuthProfileResponse>`. Replace `Record<string, unknown>` in `updateProfile` data with `UpdateProfileRequest`. |
| **API-014** | `identity/api/identity.api.ts` | Same changes as API-010 + API-011 (identity.api.ts duplicates user/role patterns). Import same types from `users/types/User.Request.Type` and `users/roles/types/Role.Request.Type`. |

### Phase 3: Update service files that pass inline data

- GOAL-003: Service files that construct inline objects to pass to api methods must use the named types

Check each service file that calls an api method with inline object construction:

```bash
grep -rn "apiClient\." app/Admin/src/features/ --include="*.ts" | grep -v "api/" | head -20
```

Most service files simply delegate to the api file (they pass through the typed request), so no changes needed. The exception is services that construct or transform request data before passing to the api.

| Task | Service File | Check |
|------|-------------|-------|
| **SRV-001** | `inventories/services/inventory.service.ts` | Verify all api calls pass typed data (not inline constructed objects) |
| **SRV-002** | `auth/services/auth.service.ts` | Verify uses `RefreshTokenRequest`, `UpdateProfileRequest` |
| **SRV-003** | `users/services/user.service.ts` | Verify uses new request types |
| **SRV-004** | `users/services/role.service.ts` | Verify uses new request types |

### Phase 4: Verification

- GOAL-004: All 14 api files use named types, typecheck passes, no Record<string, unknown> in api signatures

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| **VER-001** | `rg '{ [a-zA-Z]+: [a-zA-Z]+;' app/Admin/src/features/*/api/` — zero inline object type params | | |
| **VER-002** | `rg 'Record<string, unknown>' app/Admin/src/features/*/api/` — zero matches | | |
| **VER-003** | `vue-tsc --build` — zero `Cannot find module` errors | | |
| **VER-004** | `pnpm run lint` — same pre-existing errors only | | |

## 3. Alternatives

- **ALT-001**: Keep inline types — rejected because they're duplicated across files, lose type safety with `Record<string, unknown>`, and make refactoring harder
- **ALT-002**: Use `Pick<ResponseType, ...>` instead of creating Request types — rejected because request shapes often differ from response shapes (different fields, different optionality)
- **ALT-003**: Merge all request types into a single `api.types.ts` per module — rejected; follow existing convention of one type file per entity

## 4. Dependencies

- **DEP-001**: Phase 1 must complete before Phase 2 (types must exist before they're imported)
- **DEP-002**: Phase 2 must complete before Phase 3 (api methods must accept the types before services can use them)

## 5. Files

| Scope | Files Created | Files Modified |
|-------|--------------|----------------|
| Types | ~8 (new `.Request.Type.ts` files) | ~4 (extend existing `.Request.Type.ts`) |
| API | 0 | ~14 (`api/*.api.ts` files) |
| Service | 0 | ~4 (if needed) |

## 6. Testing

- **TEST-001**: `vue-tsc --build` — zero errors
- **TEST-002**: `rg '{ [a-zA-Z]+: [a-zA-Z]+;' app/Admin/src/features/*/api/` — zero inline object type params
- **TEST-003**: `rg 'Record<string, unknown>' app/Admin/src/features/*/api/` — zero matches

## 7. Risks & Assumptions

- **RISK-001**: Service files that construct inline data may not match new request types — mitigated by typecheck catching mismatches
- **RISK-002**: `identity/api/identity.api.ts` duplicates patterns from `user.api.ts` and `role.api.ts` — changes must be applied to both
- **ASSUMPTION-001**: All existing `*.Request.Type.ts` files define the correct type shapes matching the backend API
- **ASSUMPTION-002**: Each entity's type directory exists and has at least `*.Response.Type.ts`

## 8. Related Specifications / Further Reading

- Existing type convention: `types/{Entity}.{Request|Response|Parameters|Query}.Type.ts`
- API layer convention: `api/{entity}.api.ts` with `apiClient` from `@/shared/api/http/api.client`
- Backend endpoint mapping with request/response shapes: documented in `plan/refactor-nested-structure-2.md`
