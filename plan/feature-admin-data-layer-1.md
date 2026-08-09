---
goal: Complete the Admin SPA data layer (types, validations, API services, Pinia stores, feature composables) for all 9 business modules, matching every backend endpoint, using the Catalog option-type/option-value feature as the canonical reference pattern.
version: 1.0
date_created: 2026-08-01
last_updated: 2026-08-01
owner: ReSys.Shop Engineering
status: 'Completed'
tags: [`feature`, `admin-spa`, `vue`, `typescript`, `api`, `pinia`, `zod`]
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

The Admin SPA (`app/Admin/`) currently implements a complete data layer only for the Catalog, Location, and Auth features. The remaining modules (Identity, Inventory, Ordering, Payment, Profile, Shipping, Dashboard) have only empty barrel `index.ts` files and placeholder views. Additionally, the Catalog feature's API services drifted from the backend after the flattened-route refactor (routes such as `api/admin/catalog/taxons`, `api/admin/catalog/taxon-rules`, `api/admin/catalog/variant-prices`, `api/admin/catalog/variant-images` replaced the old nested `api/admin/catalog/taxonomies/taxons/...` paths).

This plan completes the Admin SPA data layer for every module by applying the canonical Catalog pattern (types → validations → API service → Pinia store → feature composable → barrel exports → tests) to every entity exposed by the backend, and reconciles the existing Catalog/Location services with the authoritative route constants. The plan is derived from a full scan of every backend `*.Endpoint.cs`, `*.Request.cs`, `*.Response.cs`, and `*Feature*.cs` route-constant file.

## 1. Requirements & Constraints

- **REQ-001**: Every backend Admin endpoint in all 9 modules MUST be covered by a typed, method-level API service in `app/Admin/src/features/<module>/services/`. Endpoints are enumerated per module in Section 2.
- **REQ-002**: Each entity MUST define a `types/<entity>.ts` file following the option-type pattern (`Request`, `ListItem`, `Detail`, `Query` interfaces; `*_FILTER_FIELDS` / `*_SORT_FIELDS` / `*_SEARCH_FIELDS` const arrays; `to<Entity>QueryParams(query)` helper) per PAT-001.
- **REQ-003**: Each mutable entity MUST define a `validations/<entity>.ts` file with zod field schemas + a composed `z.object` schema + `<Entity>Form` type, mirroring the backend FluentValidation rules per PAT-002.
- **REQ-004**: Each entity MUST expose a Pinia store (`stores/<entity>Store.ts`) for list/detail state where the UI requires it, per PAT-003.
- **REQ-005**: Each module MUST expose feature composables (`composables/use<Entity>List.ts`, `composables/use<Entity>Detail.ts`) wrapping the shared `usePagedQuery` + API service, per PAT-004.
- **REQ-006**: Every layer MUST be re-exported through the module barrel `index.ts` files (`types/index.ts`, `validations/index.ts`, `services/index.ts`, `stores/index.ts`, `composables/index.ts`), per PAT-005.
- **REQ-007**: Route strings in services MUST match the authoritative backend route constants (source of truth: `CatalogFeature.Admin.cs`, `Identity.Feature.cs`, `InventoryFeature.Admin.cs`, `LocationFeature.Admin.cs`, `ShippingFeature.Admin.cs`, `PaymentFeature.Admin.cs`, `OrderingFeature.Admin.cs`, `ProfileFeature.cs`, `DashboardFeature.cs`, and the per-module dashboard feature files). Endpoint-file comments are stale and MUST NOT be used.
- **REQ-008**: The Catalog feature is the reference implementation and MUST be reconciled (routes/verbs fixed) rather than rewritten; its services/pattern remain the template for all other modules.
- **REQ-009**: Parent-entity ids (e.g. `taxonId`, `productId`, `variantId`, `userId`) are query-string parameters for GET endpoints and body properties for POST/PUT/DELETE endpoints, exactly as the backend binds them.
- **REQ-010**: A DELETE-with-body capability is required for `RemoveVariantPrice` and `DeleteTaxonRule`; a `delWithBody` helper must be added to `app/Admin/src/shared/api/client.ts` without altering the existing `del` signature.
- **SEC-001**: No credentials, tokens, or secrets may be hardcoded in SPA source; all auth flows continue through the existing interceptor chain.
- **SEC-002**: Client-side validations are mirrors of the server rules for UX; the backend remains the authoritative validation boundary.
- **CON-001**: The shared layer (`shared/api/client.ts`, `shared/api/paged.ts`, `shared/types/querying/*`) MUST NOT be modified except for the additive `delWithBody` helper (REQ-010).
- **CON-002**: `cd app/Admin && pnpm run lint && pnpm run type-check && pnpm run test:unit` MUST complete with 0 errors before this plan is marked complete.
- **CON-003**: Enums (e.g. `ProductStatus`, `OrderStatus`, `ReservationState`, `DisplayOn`, `AddressType`, `TaxonMatchPolicy`, `TaxonSortOrder`) are serialized as camelCase strings; represent them as string-literal unions + exported const arrays in types files.
- **CON-004**: Paged GET endpoints return `PagedResult<T>`; singular GETs return `Result<T>`; commands return `Result` or `Result<T>` per the response records.
- **CON-005**: All new files MUST be created under `app/Admin/src/features/<module>/` mirroring the existing catalog folder layout; no new dependency packages may be added.
- **CON-006**: The Storefront/Auth SPA (`app/Store/`) and the legacy `app/ReSys.Admin/` are out of scope.
- **GUD-001**: Use the module prefix constants from `app/Admin/src/shared/constants/api.ts` (`CATALOG`, `IDENTITY`, `INVENTORY`, `LOCATION`, `ORDERING`, `PAYMENT`, `PROFILE`, `SHIPPING`, `DASHBOARD`) as the base URL for each service.
- **GUD-002**: State-transition endpoints (activate/deactivate/restore/status/toggle) use the PATCH verb via the shared `patch` helper unless the backend route table says otherwise.
- **GUD-003**: Multipart upload endpoints (variant image upload, stock item import) send `FormData` with a `file` field plus scalar fields via the shared `post` helper (no manual `Content-Type` header).
- **GUD-004**: DELETE-with-body endpoints call `delWithBody(url, body)` (REQ-010); all other deletes use `del(url)`.
- **GUD-005**: File names and class names follow the catalog convention: `<entity>Api` class in `services/<entity>Api.ts`; type file `types/<entity>.ts`; validation file `validations/<entity>.ts`; store `stores/<entity>Store.ts`; composables `composables/use<Entity>List.ts` / `use<Entity>Detail.ts`.
- **GUD-006**: Tests mirror `features/catalog/__tests__/services/optionValueApi.spec.ts`: mock `@/shared/api/client` and `@/shared/api`, assert exact URL + verb + body per method.
- **PAT-001**: Type file shape — `export interface XRequest {...}`; `export interface XListItem extends XRequest { id: string; ... }`; `export type XDetail = ...`; `export interface XQuery {...}`; `export const X_FILTER_FIELDS = [...]`; `export const X_SORT_FIELDS = [...]`; `export function toXQueryParams(query: XQuery): QueryingParameters {...}`.
- **PAT-002**: Validation file shape — one `z.string()/z.number()/z.boolean()` field schema per request prop with messages matching backend validators, then `export const xSchema = z.object({...})` and `export type XForm = z.infer<typeof xSchema>`.
- **PAT-003**: Store shape — `defineStore('<entity>s', () => { const items = ref<T[]>([]); const loaded = ref(false); async function fetch...() {...}; return { items, loaded, fetch... } })`.
- **PAT-004**: Composable shape — `export function use<Entity>List(options?: UsePagedQueryOptions) { return usePagedQuery<XListItem>(XApi.getUrl, { allowedFilterFields: X_FILTER_FIELDS, allowedSortFields: X_SORT_FIELDS, ...options }) }` and `use<Entity>Detail(id)` calling `XApi.getX(id)`.
- **PAT-005**: Barrel shape — `export { XApi } from './xApi'` per service; `export type { XRequest, XListItem, XDetail, XQuery } from './x'` + `export { X_FILTER_FIELDS, X_SORT_FIELDS, toXQueryParams } from './x'` per type file; same pattern for validations/stores/composables.

## 2. Implementation Steps

### Implementation Phase 1 — Shared helper + Catalog reconciliation

- GOAL-001: Reconcile the reference Catalog feature with the flattened backend routes and fill all missing Catalog services/types/validations, keeping the option-type feature untouched as the template.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Add `delWithBody<T>(url: string, body?: unknown, signal?: AbortSignal): Promise<T>` to `app/Admin/src/shared/api/client.ts` (calls `getApiClient().delete(url, { data: body, signal })`), and add a unit test in `shared/api/__tests__/client.spec.ts`. Do not change `del`. | X | 2026-08-01 |
| TASK-002 | Rewrite `app/Admin/src/features/catalog/services/taxonApi.ts`: BASE = `${CATALOG}/taxons`; keep `getTaxons`/`getTaxon`/`getTree` (`getTree` -> `get` of `Result<TaxonTreeItem[]>` at `${BASE}/tree?taxonomyId=`)/`getList`/`createTaxon`/`updateTaxon`/`deleteTaxon`; add `restoreTaxon(id)` using `patch<Result<TaxonListItem>>(\`${BASE}/${id}/restore\`)` and `repositionTaxon(id, request)` using `post<Result<{ id: string }>>(\`${BASE}/${id}/reposition\`, request)`. Update `taxonApi.spec.ts` URLs. | X | 2026-08-01 |
| TASK-003 | Rewrite `app/Admin/src/features/catalog/services/taxonRuleApi.ts`: BASE = `${CATALOG}/taxon-rules`; `getRules(taxonId)` -> `getPaged(`${BASE}?taxonId=${taxonId}`, ...)`; `createRule(request)` -> `post(BASE, request)`; `updateRule(ruleId, request)` -> `put(`${BASE}/${ruleId}`, request)`; `deleteRule(taxonId, ruleId)` -> `delWithBody(`${BASE}/${ruleId}`, { taxonId, ruleId })`; add `syncRules(request)` -> `post(`${BASE}/sync`, request)`. Update `taxonRuleApi.spec.ts`. | X | 2026-08-01 |
| TASK-004 | Rewrite `app/Admin/src/features/catalog/services/productOptionTypeApi.ts`: BASE = `${CATALOG}/product-option-types`; `getOptionTypes(productId)` -> `getPaged(BASE, { ... }, { filter: 'productId=...' })` or `get(`${BASE}?productId=${productId}`)` matching backend (`PagedResult`); add `assignOptionTypes(request)` (`post(`${BASE}/assign`, request)`), `revokeOptionTypes(request)` (`post(`${BASE}/revoke`, request)`), `syncOptionTypes(request)` (`put(`${BASE}/sync`, request)`), where request carries `{ productId, items: [{ optionTypeId, position }] }`. Move `OptionTypeAssignment`/`OptionTypeSyncItem` into `types/productOptionType.ts` (TASK-010). | X | 2026-08-01 |
| TASK-005 | Rewrite `app/Admin/src/features/catalog/services/productClassificationApi.ts`: BASE = `${CATALOG}/product-classifications`; same shape as TASK-004 with `taxonId`/`position` items; add assign/revoke/sync. Move `ClassificationAssignment`/`ClassificationSyncItem` into `types/productClassification.ts` (TASK-010). | X | 2026-08-01 |
| TASK-006 | Rewrite the option-value methods in `app/Admin/src/features/catalog/services/variantApi.ts`: `getOptionValues(variantId)` -> `getPaged(`${CATALOG}/variant-option-values?variantId=${variantId}`, ...)`; `assignOptionValues(variantId, ids)` -> `post(`${CATALOG}/variant-option-values/assign`, { variantId, optionValueIds: ids })`; `revokeOptionValues(...)` -> `post(`${CATALOG}/variant-option-values/revoke`, { variantId, optionValueIds: ids })`; add `syncOptionValues(variantId, ids)` -> `put(`${CATALOG}/variant-option-values/sync`, { variantId, optionValueIds: ids })`. Update `variantApi.spec.ts`. | X | 2026-08-01 |
| TASK-007 | Rewrite `app/Admin/src/features/catalog/services/variantPriceApi.ts`: BASE = `${CATALOG}/variant-prices`; `listPrices(variantId)` -> `getPaged(`${BASE}?variantId=${variantId}`, ...)`; `setPrice(request)` -> `post(BASE, request)` (request now includes `variantId`); `removePrice(variantId, priceId)` -> `delWithBody(`${BASE}/${priceId}`, { variantId, priceId })`; add `syncPrices(request)` -> `post(`${BASE}/sync`, request)`. Move `PriceRequest`/`Price` into `types/variantPrice.ts` (TASK-010). Update `variantPriceApi.spec.ts`. | X | 2026-08-01 |
| TASK-008 | Rewrite `app/Admin/src/features/catalog/services/variantImageApi.ts`: BASE = `${CATALOG}/variant-images`; `listImages(variantId)` -> `getPaged(`${BASE}?variantId=${variantId}`, ...)`; `getImage(id)` -> `get(`${BASE}/${id}`)`; `uploadImage(request)` -> multipart `post(BASE, formData)` where formData includes `variantId`, `file`, `alt?`, `position`, `type`; `updateImage(id, request)` -> `put(`${BASE}/${id}`, request)`; `deleteImage(id)` -> `del(`${BASE}/${id}`)`; `downloadImage(id)` -> `get(`${BASE}/${id}/download`)` (returns `Result<Blob>` via responseType blob). Move `VariantImage` into `types/variantImage.ts` (TASK-010). Update `variantImageApi.spec.ts`. | X | 2026-08-01 |
| TASK-009 | Fix `app/Admin/src/features/catalog/services/productApi.ts`: change `activateProduct(id)` and `discontinueProduct(id)` to use `patch<Result<ProductDetail>>(\`${ProductApi.BASE}/${id}/activate\`)` and `patch<Result<ProductDetail>>(\`${ProductApi.BASE}/${id}/discontinue\`)` with no body. Update `productApi.spec.ts`. | X | 2026-08-01 |
| TASK-010 | Create `types/productOptionType.ts`, `types/productClassification.ts`, `types/variantPrice.ts`, `types/variantImage.ts`, `types/imageEmbedding.ts` in `app/Admin/src/features/catalog/types/` (props exactly per backend response/request records from the endpoint scan; include FILTER/SORT consts and `to*QueryParams` where paged). Create `validations/productOptionType.ts`, `validations/productClassification.ts`, `validations/variantPrice.ts`, `validations/variantImage.ts`, `validations/imageEmbedding.ts` per PAT-002. Update `types/index.ts` and `validations/index.ts`. | X | 2026-08-01 |
| TASK-011 | Create `services/imageEmbeddingApi.ts` in `app/Admin/src/features/catalog/services/`: `create(request)` -> `post(`${CATALOG}/variant-image-embeddings`, request)`, `regenerate(request)` -> `put(`${CATALOG}/variant-image-embeddings/regenerate`, request)`, request = `{ variantImageId, modelName, modelVersion }`. Update `services/index.ts`. | X | 2026-08-01 |
| TASK-012 | Add `restoreTaxonomy(id)` (`patch<Result<void>>(\`${CATALOG}/taxonomies/${id}/restore\`)`) to `app/Admin/src/features/catalog/services/taxonomyApi.ts`; add `getCatalogDashboard()` via a new `services/catalogDashboardApi.ts` (`get(`${CATALOG}/dashboard`)` -> `Result<CatalogDashboard>`) and a matching `types/catalogDashboard.ts`. Update barrels and `taxonomyApi.spec.ts`. | X | 2026-08-01 |
| TASK-013 | Create feature composables in `app/Admin/src/features/catalog/composables/`: `useOptionTypeList`, `useOptionValueList`, `useTaxonList`, `useTaxonTree`, `useTaxonRuleList`, `useProductList`, `useVariantList`, `useVariantPriceList`, `useVariantImageList` per PAT-004 (wrap shared `usePagedQuery` + each API service). Populate `composables/index.ts` and `features/catalog/index.ts`. | X | 2026-08-01 |
| TASK-014 | Update/add unit tests for all changed Catalog services (`taxonApi`, `taxonRuleApi`, `productOptionTypeApi`, `productClassificationApi`, `variantApi`, `variantPriceApi`, `variantImageApi`, `productApi`, `imageEmbeddingApi`, `catalogDashboardApi`, `taxonomyApi`) under `features/catalog/__tests__/services/` asserting exact flat URLs, verbs, and bodies. | X | 2026-08-01 |

### Implementation Phase 2 — Identity module

- GOAL-002: Implement the Identity data layer (Roles, Users, Permissions) matching every `api/identity/**` endpoint.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-015 | Create `types/role.ts` (`RoleRequest`, `RoleListItem`, `RoleDetail`, `RoleQuery`, FILTER/SORT consts, `toRoleQueryParams`), `types/user.ts` (`UserRequest`, `UserListItem`, `UserDetail`, `UserQuery`, consts, `toUserQueryParams`), `types/permission.ts` (`PermissionItem` with `identifier/name/description/action/isAssigned`, `PermissionResource`/`PermissionCategory` group shapes for GET role/user permissions). Populate `types/index.ts`. | X | 2026-08-01 |
| TASK-016 | Create `validations/role.ts` (name required ≤100, description/presentation optional) and `validations/user.ts` (email, userName, firstName, lastName, phone optional) per PAT-002. Populate `validations/index.ts`. | X | 2026-08-01 |
| TASK-017 | Create `services/roleApi.ts` (BASE `${IDENTITY}/roles`): `getRoles(query)` paged, `getRole(id)`, `createRole(request)`, `updateRole(id, request)`, `deleteRole(id)`; plus permission actions `assignPermissions(id, permissions[])` (`put` `/permissions/assign`), `getPermissions(id)` (`get` `/permissions`), `revokePermissions(id, permissions[])` (`del` `/permissions/revoke` with body), `syncPermissions(id, permissions[])` (`patch` `/permissions/sync`). Populate `services/index.ts`. | X | 2026-08-01 |
| TASK-018 | Create `services/userApi.ts` (BASE `${IDENTITY}/users`): `getUsers(query)` paged, `getUser(id)`, `createUser(request)`, `updateUser(id, request)`, `deleteUser(id)`, `toggleStatus(id)` (`patch` `/status`); user roles `getRoles(id)` (`get` `/roles` paged), `assignRoles(id, roleNames[])` (`post` `/roles/assign`), `revokeRoles(id, roleNames[])` (`post` `/roles/revoke`), `syncRoles(id, roleNames[])` (`patch` `/roles/sync`); user permissions `getPermissions(id)`, `assignPermissions(id, permissions[])` (`post` `/permissions/assign`), `revokePermissions(id, permissions[])` (`del` `/permissions/revoke` body), `syncPermissions(id, permissions[])` (`put` `/permissions/sync`). Populate `services/index.ts`. | X | 2026-08-01 |
| TASK-019 | Create `services/permissionApi.ts` (`getPermissions()` -> `getPaged(`${IDENTITY}/permissions`, ...)`) and `stores/roleStore.ts` + `stores/userStore.ts` per PAT-003 (active list + `fetchActive`). Populate `stores/index.ts`. | X | 2026-08-01 |
| TASK-020 | Create `composables/useRoleList.ts`, `useUserList.ts`, `useRoleDetail.ts`, `useUserDetail.ts` per PAT-004. Populate `composables/index.ts`. | X | 2026-08-01 |
| TASK-021 | Add `__tests__/services/roleApi.spec.ts`, `userApi.spec.ts`, `permissionApi.spec.ts`; `__tests__/validations/role.spec.ts`, `user.spec.ts`; `__tests__/types/role.spec.ts`, `user.spec.ts`; `__tests__/stores/roleStore.spec.ts`, `userStore.spec.ts` asserting exact `api/identity/**` URLs/verbs/bodies. | X | 2026-08-01 |

### Implementation Phase 3 — Inventory module

- GOAL-003: Implement the Inventory data layer (StockItems, StockLocations, StockMovements, StockReservations, StockTransfers, InventoryDashboard) matching every `api/admin/inventory/**` endpoint.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-022 | Create `types/stockItem.ts`, `types/stockLocation.ts`, `types/stockMovement.ts`, `types/stockReservation.ts`, `types/stockTransfer.ts` (request/list/detail/query interfaces + consts + `to*QueryParams` per the backend model records; include `ReservationState` union `'Reserved'|'Fulfilled'|'Released'|'Expired'`). Populate `types/index.ts`. | X | 2026-08-01 |
| TASK-023 | Create `validations/stockItem.ts` (stockLocationId/variantId required, countOnHand ≥0, backorderable bool), `validations/stockLocation.ts` (name required ≤255, code/city/postalCode/phone optional), `validations/stockTransfer.ts` (sourceLocationId != destinationLocationId, items non-empty with quantity >0) per PAT-002. Populate `validations/index.ts`. | X | 2026-08-01 |
| TASK-024 | Create `services/stockItemApi.ts` (BASE `${INVENTORY}/stock-items`): `getStockItems(query)` paged, `getStockItem(id)`, `createStockItem(request)`, `updateStockItem(id, request)`, `deleteStockItem(id)`, `bulkAdjustStockItems(request)` (`post` `/bulk-adjust`), `restockStockItem(id, request)` (`post` `/${id}/restock`), `getLowStockItems(params)` (`getPaged` `/low-stock`), `getStockSummary(query)` (`getPaged` `/summary`), `importStockItems(file)` (multipart `post` `/import`). Populate `services/index.ts`. | X | 2026-08-01 |
| TASK-025 | Create `services/stockLocationApi.ts` (BASE `${INVENTORY}/stock-locations`): CRUD + `setDefaultStockLocation(id)` (`put` `/${id}/default`). Populate `services/index.ts`. | X | 2026-08-01 |
| TASK-026 | Create `services/stockMovementApi.ts` (BASE `${INVENTORY}/stock-movements`): `getStockMovements(query)` paged (with `fromUtc`/`toUtc`/`variantId`/`stockLocationId` query params), `getStockMovement(id)`. Populate `services/index.ts`. | X | 2026-08-01 |
| TASK-027 | Create `services/stockReservationApi.ts` (BASE `${INVENTORY}/stock-reservations`): `getStockReservations(query)` paged, `getStockReservation(id)`, `cancelStockReservation(id)` (`post` `/${id}/cancel`). Populate `services/index.ts`. | X | 2026-08-01 |
| TASK-028 | Create `services/stockTransferApi.ts` (BASE `${INVENTORY}/stock-transfers`): CRUD + `transferStockTransfer(id)` (`post` `/${id}/transfer`), `receiveStockTransfer(id, request)` (`post` `/${id}/receive`), `cancelStockTransfer(id)` (`post` `/${id}/cancel`). Populate `services/index.ts`. | X | 2026-08-01 |
| TASK-029 | Create `services/inventoryDashboardApi.ts` (`getInventoryDashboard()` -> `get(`${INVENTORY}/dashboard`)`) and `types/inventoryDashboard.ts`. Create `stores/stockItemStore.ts`, `stores/stockLocationStore.ts`, `stores/stockTransferStore.ts` per PAT-003. Populate `services/index.ts`, `stores/index.ts`, `types/index.ts`. | X | 2026-08-01 |
| TASK-030 | Create `composables/useStockItemList.ts`, `useStockLocationList.ts`, `useStockMovementList.ts`, `useStockReservationList.ts`, `useStockTransferList.ts` per PAT-004. Populate `composables/index.ts`. | X | 2026-08-01 |
| TASK-031 | Add `__tests__/services/*.spec.ts` for all 6 Inventory services, `__tests__/validations/stockItem|stockLocation|stockTransfer.spec.ts`, `__tests__/types/*.spec.ts`, and `__tests__/stores/stockItemStore|stockLocationStore|stockTransferStore.spec.ts` asserting exact `api/admin/inventory/**` URLs/verbs/bodies. | X | 2026-08-01 |

### Implementation Phase 4 — Ordering module

- GOAL-004: Implement the Ordering data layer (Orders incl. line items + OrderingDashboard) matching every `api/admin/ordering/**` endpoint.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-032 | Create `types/order.ts`: `OrderRequest`, `OrderListItem`, `OrderDetail`, `LineItem`, `OrderQuery`, `OrderStatus` union (`'Draft'|'Placed'|'Canceled'|'Expired'`), `CheckoutState` union, consts, `toOrderQueryParams`. Create `types/orderingDashboard.ts`. Populate `types/index.ts`. | X | 2026-08-01 |
| TASK-033 | Create `validations/order.ts` (currency required, email format if present, lineItem quantity >0, status in enum) per PAT-002. Populate `validations/index.ts`. | X | 2026-08-01 |
| TASK-034 | Create `services/orderApi.ts` (BASE `${ORDERING}/orders`): `getOrders(query)` paged, `getOrder(id)`, `createOrder(request)`, `updateOrder(id, request)`, `deleteOrder(id)`; line items `getLineItems(id, query)` paged, `getLineItem(id, lineItemId)`, `addLineItem(id, request)`, `updateLineItem(id, lineItemId, request)`, `removeLineItem(id, lineItemId)`; actions `cancelOrder(id, request?)` (`post` `/${id}/cancel`), `completeOrder(id)` (`post` `/${id}/complete`), `approveOrder(id)` (`post` `/${id}/approve`), `resumeOrder(id)` (`post` `/${id}/resume`), `updateShipAddress(id, request)` (`put` `/${id}/ship-address`), `updateBillAddress(id, request)` (`put` `/${id}/bill-address`), `updateShippingMethod(id, request)` (`put` `/${id}/shipping-method`), `updateStatus(id, request)` (`put` `/${id}/status`). Populate `services/index.ts`. | X | 2026-08-01 |
| TASK-035 | Create `services/orderingDashboardApi.ts` (`getOrderingDashboard()` -> `get(`${ORDERING}/dashboard`)`). Create `stores/orderStore.ts` per PAT-003. Populate `services/index.ts`, `stores/index.ts`. | X | 2026-08-01 |
| TASK-036 | Create `composables/useOrderList.ts`, `useOrderDetail.ts` per PAT-004. Populate `composables/index.ts`. | X | 2026-08-01 |
| TASK-037 | Add `__tests__/services/orderApi.spec.ts`, `orderingDashboardApi.spec.ts`; `__tests__/validations/order.spec.ts`; `__tests__/types/order.spec.ts`; `__tests__/stores/orderStore.spec.ts` asserting exact `api/admin/ordering/**` URLs/verbs/bodies. | X | 2026-08-01 |

### Implementation Phase 5 — Payment module

- GOAL-005: Implement the Payment data layer (PaymentMethods, Payments) matching every `api/admin/payment/**` endpoint.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-038 | Create `types/paymentMethod.ts` (`PaymentMethodRequest`, `PaymentMethodUpdateRequest`, `PaymentMethodListItem`, `PaymentMethodDetail`, `PaymentMethodQuery`, `DisplayOn` union `'Both'|'Frontend'|'Backend'`, consts, `toPaymentMethodQueryParams`) and `types/payment.ts` (`PaymentListItem`, `PaymentDetail`, `PaymentQuery`, consts, `toPaymentQueryParams`). Populate `types/index.ts`. | X | 2026-08-01 |
| TASK-039 | Create `validations/paymentMethod.ts` (name required, code required + pattern, providerKey required, displayOn in enum) per PAT-002. Populate `validations/index.ts`. | X | 2026-08-01 |
| TASK-040 | Create `services/paymentMethodApi.ts` (BASE `${PAYMENT}/payment-methods`): CRUD + `activatePaymentMethod(id)` (`patch` `/${id}/activate`), `deactivatePaymentMethod(id)` (`patch` `/${id}/deactivate`); update uses `put` with `PaymentMethodUpdateRequest`. Populate `services/index.ts`. | X | 2026-08-01 |
| TASK-041 | Create `services/paymentApi.ts` (BASE `${PAYMENT}/payments`): `getPayments(query)` paged, `getPayment(id)`, `capturePayment(id, request?)` (`post` `/${id}/capture`), `refundPayment(id, request)` (`post` `/${id}/refund`), `voidPayment(id)` (`post` `/${id}/void`). Create `stores/paymentMethodStore.ts` per PAT-003. Populate `services/index.ts`, `stores/index.ts`. | X | 2026-08-01 |
| TASK-042 | Create `composables/usePaymentList.ts`, `usePaymentMethodList.ts`, `usePaymentMethodDetail.ts` per PAT-004. Populate `composables/index.ts`. | X | 2026-08-01 |
| TASK-043 | Add `__tests__/services/paymentMethodApi.spec.ts`, `paymentApi.spec.ts`; `__tests__/validations/paymentMethod.spec.ts`; `__tests__/types/paymentMethod.spec.ts`; `__tests__/stores/paymentMethodStore.spec.ts` asserting exact `api/admin/payment/**` URLs/verbs/bodies. | X | 2026-08-01 |

### Implementation Phase 6 — Profile module

- GOAL-006: Implement the Profile data layer (Profiles, Addresses) matching every `api/profiles/**` endpoint.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-044 | Create `types/profile.ts` (`ProfileRequest`, `ProfileListItem`, `ProfileDetail`, `ProfileQuery`, `ProfilePreferences`, `ProfileNotificationPreferences`, consts, `toProfileQueryParams`) and `types/address.ts` (`AddressRequest`, `AddressResponse`, `AddressQuery`, `AddressType` union `'Shipping'|'Billing'|'Other'`, consts, `toAddressQueryParams`). Populate `types/index.ts`. | X | 2026-08-01 |
| TASK-045 | Create `validations/profile.ts` (firstName/lastName/email required, dateOfBirth past, preferences optional) and `validations/address.ts` (addressType in enum, firstName/address1/city/countryName required) per PAT-002. Populate `validations/index.ts`. | X | 2026-08-01 |
| TASK-046 | Create `services/profileApi.ts` (BASE `${PROFILE}/profiles`): `getProfiles(query)` paged (`getPaged` `/all`), `createProfile(request)`, `updateProfile(request)`, `deleteProfile(userId)` (`del` with `?userId=` query). Populate `services/index.ts`. | X | 2026-08-01 |
| TASK-047 | Create `services/addressApi.ts` (BASE `${PROFILE}/addresses`): `getAddresses(userId, query)` paged, `getAddress(userId, id)`, `createAddress(request)`, `updateAddress(id, request)`, `deleteAddress(userId, id)` (`del` `/${id}?userId=`). Create `stores/profileStore.ts`, `stores/addressStore.ts` per PAT-003. Populate `services/index.ts`, `stores/index.ts`. | X | 2026-08-01 |
| TASK-048 | Create `composables/useProfileList.ts`, `useProfileDetail.ts`, `useAddressList.ts`, `useAddressDetail.ts` per PAT-004. Populate `composables/index.ts`. | X | 2026-08-01 |
| TASK-049 | Add `__tests__/services/profileApi.spec.ts`, `addressApi.spec.ts`; `__tests__/validations/profile.spec.ts`, `address.spec.ts`; `__tests__/types/profile.spec.ts`; `__tests__/stores/profileStore.spec.ts`, `addressStore.spec.ts` asserting exact `api/profiles/**` URLs/verbs/bodies. | X | 2026-08-01 |

### Implementation Phase 7 — Shipping module

- GOAL-007: Implement the Shipping data layer (ShippingMethods, ShippingRates) matching every `api/shipping/**` endpoint.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-050 | Create `types/shippingMethod.ts` (`ShippingMethodRequest`, `ShippingMethodListItem`, `ShippingMethodDetail`, `ShippingMethodQuery`, consts, `toShippingMethodQueryParams`) and `types/shippingRate.ts` (`ShippingRateRequest`, `ShippingRateListItem`, `ShippingRateDetail`, `ShippingRateQuery`, consts, `toShippingRateQueryParams`). Populate `types/index.ts`. | X | 2026-08-01 |
| TASK-051 | Create `validations/shippingMethod.ts` (name required, calculatorType required) and `validations/shippingRate.ts` (name required, cost >0, shippingMethodId required) per PAT-002. Populate `validations/index.ts`. | X | 2026-08-01 |
| TASK-052 | Create `services/shippingMethodApi.ts` (BASE `${SHIPPING}/shipping-methods`): CRUD + `activateShippingMethod(id)` (`patch` `/${id}/activate`), `deactivateShippingMethod(id)` (`patch` `/${id}/deactivate`). Create `services/shippingRateApi.ts` (BASE `${SHIPPING}/shipping-rates`): CRUD. Populate `services/index.ts`. | X | 2026-08-01 |
| TASK-053 | Create `stores/shippingMethodStore.ts`, `stores/shippingRateStore.ts` per PAT-003. Populate `stores/index.ts`. | X | 2026-08-01 |
| TASK-054 | Create `composables/useShippingMethodList.ts`, `useShippingMethodDetail.ts`, `useShippingRateList.ts`, `useShippingRateDetail.ts` per PAT-004. Populate `composables/index.ts`. | X | 2026-08-01 |
| TASK-055 | Add `__tests__/services/shippingMethodApi.spec.ts`, `shippingRateApi.spec.ts`; `__tests__/validations/shippingMethod.spec.ts`, `shippingRate.spec.ts`; `__tests__/types/shippingMethod.spec.ts`; `__tests__/stores/shippingMethodStore.spec.ts`, `shippingRateStore.spec.ts` asserting exact `api/shipping/**` URLs/verbs/bodies. | X | 2026-08-01 |

### Implementation Phase 8 — Location module gap fill

- GOAL-008: Fill the Location feature gaps (by-iso lookups) in the existing data layer.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-056 | Add `getCountryByIso(isoCode)` (`get(`${LOCATION}/countries/by-iso/${isoCode}`)` -> `Result<CountryDetail>`) to `app/Admin/src/features/location/services/countryApi.ts` and `getStateByIso(isoCode)` (`get(`${LOCATION}/states/by-iso/${isoCode}`)` -> `Result<StateDetail>`) to `stateApi.ts`. Add specs to `__tests__/services/countryApi.spec.ts` and `stateApi.spec.ts`. | X | 2026-08-01 |

### Implementation Phase 9 — Dashboard module

- GOAL-009: Implement the Dashboard data layer matching `api/dashboard`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-057 | Create `types/dashboard.ts` (`DashboardSummary` with `Sales`, `Inventory`, `Catalog`, `RecentActivities` nested shapes exactly per `GetDashboard.Response`). Populate `types/index.ts`. | X | 2026-08-01 |
| TASK-058 | Create `services/dashboardApi.ts` (`getDashboard()` -> `get(`${DASHBOARD}`)` -> `Result<DashboardSummary>`) and `stores/dashboardStore.ts` per PAT-003. Populate `services/index.ts`, `stores/index.ts`. | X | 2026-08-01 |
| TASK-059 | Create `composables/useDashboard.ts` per PAT-004. Populate `composables/index.ts`. | X | 2026-08-01 |
| TASK-060 | Add `__tests__/services/dashboardApi.spec.ts`, `__tests__/types/dashboard.spec.ts`, `__tests__/stores/dashboardStore.spec.ts` asserting exact `api/dashboard` URL/verb. | X | 2026-08-01 |

### Implementation Phase 10 — Cross-module verification

- GOAL-010: Verify all layers compile, lint cleanly, and pass unit tests; update plan status.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-061 | Run `cd app/Admin && pnpm run type-check` and `pnpm run lint` and fix all errors (warnings-as-errors, 0 expected). | X | 2026-08-01 |
| TASK-062 | Run `cd app/Admin && pnpm run test:unit` and fix all failures; ensure every new spec passes and existing specs (catalog/location/auth) remain green. | X | 2026-08-01 |
| TASK-063 | Grep `app/Admin/src/features` for any remaining stale nested route strings (`/taxonomies/taxons/`, `/products/{productId}/option-types`, `/variants/{variantId}/prices`, `/variants/{variantId}/images`) and fix leftovers; verify `services/*/index.ts` barrels export every new class. | X | 2026-08-01 |
| TASK-064 | Run identifier-uniqueness checks from the plan template against this file; renumber duplicates until checks (1) and (2) return zero rows; mark `status` Completed and set `last_updated`/completion dates. | X | 2026-08-01 |

## 3. Alternatives

- **ALT-001**: Auto-generate the SPA client from backend OpenAPI/Swagger. Rejected: the repo has no Swagger/OpenAPI source for these endpoints, and the SPA uses a hand-written typed-service convention with a camelCase interceptor; generation would produce a different structure and bypass the established pattern.
- **ALT-002**: Introduce a generic CRUD service base class and have every entity inherit from it. Rejected: existing Catalog services are self-contained static classes with explicit typed methods; a generic base would hide per-endpoint verbs/bodies and reduce type safety, diverging from the reference pattern.
- **ALT-003**: Put all module types into one `types.ts` file per module. Rejected: the Catalog pattern is one file per entity; a single file would grow unbounded for modules like Identity and violate the established file layout.
- **ALT-004**: Update the backend routes back to the old nested paths instead of reconciling the SPA. Rejected: the backend flattening (commit `88988cde`) is the agreed API contract; the SPA must follow the backend, not vice versa.

## 4. Dependencies

- **DEP-001**: Backend route constants are the source of truth (`CatalogFeature.Admin.cs`, `Identity.Feature.cs`, `InventoryFeature.Admin.cs`, `LocationFeature.Admin.cs`, `ShippingFeature.Admin.cs`, `PaymentFeature.Admin.cs`, `OrderingFeature.Admin.cs`, `ProfileFeature.cs`, `DashboardFeature.cs`, and the module dashboard feature files). Read them during implementation; do not rely on endpoint-file comments.
- **DEP-002**: Shared API layer in `app/Admin/src/shared/api/` (`client.ts` get/post/put/patch/del + `delWithBody`, `paged.ts` `getPaged`) and shared types in `app/Admin/src/shared/types/` (`Result<T>`, `PagedResult<T>`, `QueryingParameters`).
- **DEP-003**: Shared composables `usePagedQuery`, `useApiErrorHandler`, `useNotify`, `useDataTableExport` in `app/Admin/src/shared/composables/`.
- **DEP-004**: Module prefix constants in `app/Admin/src/shared/constants/api.ts`.
- **DEP-005**: zod + vee-validate for form validations and pinia for stores (already installed; no new packages).
- **DEP-006**: Existing Catalog feature (`app/Admin/src/features/catalog/`) as the canonical reference implementation for file shape, naming, and test style.

## 5. Files

- **FILE-001**: `app/Admin/src/shared/api/client.ts` (+ `shared/api/__tests__/client.spec.ts`) — add `delWithBody`.
- **FILE-002**: Catalog services `taxonApi.ts`, `taxonRuleApi.ts`, `productOptionTypeApi.ts`, `productClassificationApi.ts`, `variantApi.ts`, `variantPriceApi.ts`, `variantImageApi.ts`, `productApi.ts`, `taxonomyApi.ts`, new `imageEmbeddingApi.ts`, `catalogDashboardApi.ts` + `services/index.ts`.
- **FILE-003**: Catalog types `productOptionType.ts`, `productClassification.ts`, `variantPrice.ts`, `variantImage.ts`, `imageEmbedding.ts`, `catalogDashboard.ts` + `types/index.ts`; validations `productOptionType.ts`, `productClassification.ts`, `variantPrice.ts`, `variantImage.ts`, `imageEmbedding.ts` + `validations/index.ts`; new `composables/*.ts` + `composables/index.ts`.
- **FILE-004**: Identity `types/role.ts`, `types/user.ts`, `types/permission.ts`, `validations/role.ts`, `validations/user.ts`, `services/roleApi.ts`, `services/userApi.ts`, `services/permissionApi.ts`, `stores/roleStore.ts`, `stores/userStore.ts`, `composables/useRoleList.ts`, `useUserList.ts`, `useRoleDetail.ts`, `useUserDetail.ts` + all module `index.ts` barrels.
- **FILE-005**: Inventory `types/stockItem.ts`, `types/stockLocation.ts`, `types/stockMovement.ts`, `types/stockReservation.ts`, `types/stockTransfer.ts`, `types/inventoryDashboard.ts`, `validations/stockItem.ts`, `validations/stockLocation.ts`, `validations/stockTransfer.ts`, `services/stockItemApi.ts`, `services/stockLocationApi.ts`, `services/stockMovementApi.ts`, `services/stockReservationApi.ts`, `services/stockTransferApi.ts`, `services/inventoryDashboardApi.ts`, `stores/stockItemStore.ts`, `stores/stockLocationStore.ts`, `stores/stockTransferStore.ts`, `composables/*.ts` + all `index.ts` barrels.
- **FILE-006**: Ordering `types/order.ts`, `types/orderingDashboard.ts`, `validations/order.ts`, `services/orderApi.ts`, `services/orderingDashboardApi.ts`, `stores/orderStore.ts`, `composables/useOrderList.ts`, `useOrderDetail.ts` + barrels.
- **FILE-007**: Payment `types/paymentMethod.ts`, `types/payment.ts`, `validations/paymentMethod.ts`, `services/paymentMethodApi.ts`, `services/paymentApi.ts`, `stores/paymentMethodStore.ts`, `composables/usePaymentList.ts`, `usePaymentMethodList.ts`, `usePaymentMethodDetail.ts` + barrels.
- **FILE-008**: Profile `types/profile.ts`, `types/address.ts`, `validations/profile.ts`, `validations/address.ts`, `services/profileApi.ts`, `services/addressApi.ts`, `stores/profileStore.ts`, `stores/addressStore.ts`, `composables/*.ts` + barrels.
- **FILE-009**: Shipping `types/shippingMethod.ts`, `types/shippingRate.ts`, `validations/shippingMethod.ts`, `validations/shippingRate.ts`, `services/shippingMethodApi.ts`, `services/shippingRateApi.ts`, `stores/shippingMethodStore.ts`, `stores/shippingRateStore.ts`, `composables/*.ts` + barrels.
- **FILE-010**: Location `services/countryApi.ts`, `services/stateApi.ts` (by-iso methods) + their `__tests__`.
- **FILE-011**: Dashboard `types/dashboard.ts`, `services/dashboardApi.ts`, `stores/dashboardStore.ts`, `composables/useDashboard.ts` + barrels.
- **FILE-012**: `__tests__` directories under every module (`__tests__/services/`, `__tests__/types/`, `__tests__/validations/`, `__tests__/stores/`) mirroring the Catalog test layout.

## 6. Testing

- **TEST-001**: `shared/api/__tests__/client.spec.ts` — assert `delWithBody` sends `{ data }` on axios delete and leaves `del` unchanged.
- **TEST-002**: Catalog service specs updated for flat routes: `taxonApi`, `taxonRuleApi`, `productOptionTypeApi`, `productClassificationApi`, `variantApi`, `variantPriceApi`, `variantImageApi`, `productApi` (PATCH verbs), `imageEmbeddingApi`, `catalogDashboardApi`, `taxonomyApi`.
- **TEST-003**: Identity specs — `roleApi`, `userApi`, `permissionApi` services; `role`/`user` validations; `role`/`user` types; `roleStore`/`userStore`.
- **TEST-004**: Inventory specs — all 6 services, 3 validations, 5 type files, 3 stores.
- **TEST-005**: Ordering specs — `orderApi` (CRUD + line items + actions), `orderingDashboardApi`, `order` validation, `order` type, `orderStore`.
- **TEST-006**: Payment specs — `paymentMethodApi` (incl. activate/deactivate PATCH), `paymentApi` (capture/refund/void), `paymentMethod` validation, `paymentMethod`/`payment` types, `paymentMethodStore`.
- **TEST-007**: Profile specs — `profileApi`, `addressApi`, `profile`/`address` validations, `profile` type, `profileStore`/`addressStore`.
- **TEST-008**: Shipping specs — `shippingMethodApi`, `shippingRateApi`, validations, types, stores.
- **TEST-009**: Location specs — `countryApi.getCountryByIso`, `stateApi.getStateByIso`.
- **TEST-010**: Dashboard specs — `dashboardApi`, `dashboard` type, `dashboardStore`.
- **TEST-011**: Verification gate — `cd app/Admin && pnpm run lint && pnpm run type-check && pnpm run test:unit` all green.

## 7. Risks & Assumptions

- **RISK-001**: Backend route/verb details could change after this plan is written; mitigation is DEP-001 (always read the authoritative `*Feature*.cs` constants at implementation time) and TASK-063 (final grep for stale routes).
- **RISK-002**: The Catalog reconciliation (Phase 1) touches services consumed by existing views; if a view depends on an old nested URL, it will break. Mitigation: run `pnpm run test:unit` after Phase 1 and fix views, per TASK-014 and TASK-061/062.
- **RISK-003**: DELETE-with-body endpoints (RemoveVariantPrice, DeleteTaxonRule) depend on the new `delWithBody` helper (TASK-001); if the backend rejects DELETE bodies, fall back to sending ids via query params. Assumption below documents the expected contract.
- **RISK-004**: Large surface area (8 modules, ~60 files); parallel implementers may collide on shared barrels. Mitigation: phases are ordered by dependency, and each phase updates only its own module barrels.
- **ASSUMPTION-001**: Backend Admin endpoints and their request/response records are stable and match the endpoint scan summarized in Section 2 (verb, route, body shape per record).
- **ASSUMPTION-002**: Storefront/Auth endpoints (`api/storefront/identity`, `api/storefront/**`, `api/storefront/**`) are NOT in scope for the Admin data layer; the Auth feature already covers the Admin login/session flows it needs.
- **ASSUMPTION-003**: The camelCase interceptor (`shared/api/interceptors/camelcase.ts`) handles JSON property-name conversion, so SPA interfaces use camelCase property names.
- **ASSUMPTION-004**: Paged endpoints accept the shared `QueryingParameters` contract (`filter`, `search`, `searchFields`, `searchMode`, `sort`, `page`, `pageSize`), so `getPaged` + `to<Entity>QueryParams` compose correctly for every list.

## 8. Related Specifications / Further Reading

- [Admin SPA shared API layer](app/Admin/src/shared/api/)
- [Admin SPA querying contract](app/Admin/src/shared/types/querying/)
- [Backend route constants](service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Admin.cs)
- [Backend Identity feature routes](service/Api/src/Module/Identity/Features/Identity.Feature.cs)
- [Backend Inventory feature routes](service/Api/src/Module/Inventory/Features/Shared/)
- [Backend Location feature routes](service/Api/src/Module/Location/Features/Shared/)
- [Backend Shipping feature routes](service/Api/src/Module/Shipping/Features/Shared/)
- [Backend Payment feature routes](service/Api/src/Module/Payment/Features/Shared/)
- [Backend Ordering feature routes](service/Api/src/Module/Ordering/Features/Shared/)
- [Backend Profile feature routes](service/Api/src/Module/Profile/Features/Shared/ProfileFeature.cs)
- [Backend Dashboard feature routes](service/Api/src/Module/Dashboard/Features/Shared/DashboardFeature.cs)
- [Catalog flattened-route refactor plan](plan/refactor-catalog-feature-1.md)
