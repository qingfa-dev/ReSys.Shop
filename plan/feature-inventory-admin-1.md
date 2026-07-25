---
goal: Implement admin Inventory module frontend API services
version: 1.0
date_created: 2026-07-25
status: Planned
tags: feature, api, inventory, admin
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Implement the full admin Inventory module in `app/Admin/src/features/inventory/`. 24 backend endpoints across 5 entities (StockLocations, StockItems, StockReservations, StockTransfers, StockMovements) plus an Inventory Dashboard. All pages currently use PlaceholderPage shells — replace with real components following catalog module patterns.

Backend route prefix: `api/inventory` (constants in `InventoryFeature.Admin.cs`)

## 1. Requirements & Constraints

- **REQ-001**: Every backend CRUD endpoint must have a frontend API method
- **REQ-002**: All API methods use shared `apiClient` from `@/shared/api/client`
- **REQ-003**: Response types as camelCase interfaces matching backend C# records
- **REQ-004**: Zod validation schemas for entities with create/update forms
- **REQ-005**: Form-to-request mapper classes with static `toCreate`/`toUpdate`
- **REQ-006**: List pages get Pinia stores; child/read-only entities (StockMovements) do not
- **REQ-007**: Replace PlaceholderPage.vue usage with real component-based pages
- **CON-001**: Follow exactly the catalog module patterns (store, API, schema, mapper, composable conventions)
- **CON-002**: Zero TypeScript errors (TreatWarningsAsErrors)
- **CON-003**: Store IDs use pattern `'inventory-{entity}'` (e.g., `'inventory-stock-location'`)
- **PAT-001**: API classes with static methods wrapping apiClient
- **PAT-002**: Pinia setup stores with readonly refs, defaultListQuery
- **PAT-003**: Form components using vee-validate + zod schemas
- **PAT-004**: Mapper classes with static toCreate/toUpdate
- **PAT-005**: Composable returning `{ id, mode, route, router, toast, api: EntityApi }`
- **PAT-006**: Composable IDs derived from `route.params.id`, mode from route name ends with `.edit`
- **PAT-007**: Response types in `types/{entity}.response.ts`, request types in `types/{entity}.request.ts`
- **PAT-008**: Schema files in `schemas/{entity}.fields.ts` (field classes) and `schemas/{entity}.forms.ts` (form classes)
- **PAT-009**: Barrel exports in each directory + feature-level `index.ts`

## 2. Implementation Steps

### Phase 1: Stock Locations (top-level entity with list page)

- GOAL-001: Implement Stock Locations CRUD: types, schemas, mappers, API, store, composable, pages, components, routes, barrels

Backend endpoints (route prefix `/api/inventory`):
- GET `/stock-locations` — GetPaged
- GET `/stock-locations/{id:guid}` — GetById
- POST `/stock-locations` — Create
- PUT `/stock-locations/{id:guid}` — Update
- DELETE `/stock-locations/{id:guid}` — Delete
- PUT `/stock-locations/{id:guid}/default` — SetDefault

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `types/stock-location.response.ts` — `StockLocationResponse`: id, name, code, address1?, address2?, city?, state?, postalCode?, country?, phone?, isDefault, isActive, createdAt, updatedAt | | |
| TASK-002 | Create `types/stock-location.request.ts` — `CreateStockLocationRequest`/`UpdateStockLocationRequest` (alias from form schemas) | | |
| TASK-003 | Create `schemas/stock-location.fields.ts` — `StockLocationFields`: name (required), code (required), address1 (optional), address2 (optional), city/state/postalCode/country/phone (optional), isDefault (boolean) | | |
| TASK-004 | Create `schemas/stock-location.forms.ts` — `StockLocationForms` with create()/update(), export form types | | |
| TASK-005 | Create `mappers/stock-location.mapper.ts` — `StockLocationFormMapper` with toCreate/toUpdate | | |
| TASK-006 | Create `api/stock-location.api.ts` — `StockLocationApi`: getMany(query), get(id), create(data), update(id, data), delete(id), setDefault(id) — PUT `/stock-locations/${id}/default` returns Result<void> | | |
| TASK-007 | Create `store/stock-location.store.ts` — `useStockLocationStore` with items/loading/error/totalRecords/query/fetchMany/setPage/setSearch/setSort/setFilter/resetQuery | | |
| TASK-008 | Create `composables/useStockLocation.ts` — returns { id, mode, route, router, toast, api: StockLocationApi } | | |
| TASK-009 | Create `components/StockLocationForm.vue` — vee-validate form: name, code, address1, address2, city, state, postalCode, country, phone, isDefault checkbox; load->get; save->create/update | | |
| TASK-010 | Create `components/StockLocationListTable.vue` — DataTable with store, columns: name, code, city, country, isDefault (icon), isActive (icon), ActionMenu | | |
| TASK-011 | Replace `pages/LocationListPage.vue` — PageHeader + StockLocationListTable (title "Stock Locations") | | |
| TASK-012 | Replace `pages/LocationDetailPage.vue` — StockLocationForm | | |
| TASK-013 | Update `routes.ts` — ensure ROUTE constant + route entries share the existing LocationDetailPage for create/view/edit | | |
| TASK-014 | Update all barrel exports (types/, schemas/, mappers/, api/, composables/, index.ts) | | |
| TASK-015 | Verify: `cd app/Admin && npx vue-tsc --noEmit` passes | | |

### Phase 2: Stock Items (top-level entity with multiple list pages)

- GOAL-002: Implement Stock Items CRUD + low-stock + summary + bulk-adjust + import + restock

Backend endpoints:
- GET `/stock-items` — GetAll (paged)
- GET `/stock-items/{id:guid}` — GetById
- GET `/stock-items/low-stock` — LowStock
- GET `/stock-items/summary` — StockSummary
- POST `/stock-items` — Create
- POST `/stock-items/bulk-adjust` — BulkAdjust
- POST `/stock-items/import` — Import
- POST `/stock-items/{id:guid}/restock` — Restock
- PUT `/stock-items/{id:guid}` — Update
- DELETE `/stock-items/{id:guid}` — Delete

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-016 | Create `types/stock-item.response.ts` — `StockItemResponse`: id, variantId?, variantSku?, variantName?, locationId, locationName?, quantity, reservedQuantity, availableQuantity, lowStockThreshold?, isLowStock, lastRestockedAt?, createdAt, updatedAt; `LowStockResponse`: items: StockItemResponse[]; `StockSummaryResponse`: totalItems, totalQuantity, lowStockCount, outOfStockCount, totalLocations | | |
| TASK-017 | Create `types/stock-item.request.ts` — `CreateStockItemRequest`/`UpdateStockItemRequest` (alias from form); `BulkAdjustRequest`: items: { stockItemId, quantity }[]; `RestockRequest`: quantity: number; `ImportRequest`: file: File | | |
| TASK-018 | Create `schemas/stock-item.fields.ts` — fields: variantId (required), locationId (required), quantity (number), lowStockThreshold (optional number) | | |
| TASK-019 | Create `schemas/stock-item.forms.ts` — `StockItemForms` with create() schema; export types | | |
| TASK-020 | Create `mappers/stock-item.mapper.ts` — `StockItemFormMapper` with toCreate/toUpdate | | |
| TASK-021 | Create `api/stock-item.api.ts` — `StockItemApi`: getMany(query), get(id), getLowStock(), getSummary(), create(data), bulkAdjust(data), importFile(formData), restock(id, data), update(id, data), delete(id) | | |
| TASK-022 | Create `store/stock-item.store.ts` — `useStockItemStore` with standard list state + fetchMany | | |
| TASK-023 | Create `composables/useStockItem.ts` — returns { api: StockItemApi } + standard mode/id | | |
| TASK-024 | Create `components/StockItemForm.vue` — form: variantId (select/search), locationId (select), quantity, lowStockThreshold | | |
| TASK-025 | Create `components/StockItemListTable.vue` — columns: variantSku, variantName, locationName, quantity, reservedQuantity, availableQuantity, lowStockThreshold, status badge (isLowStock), ActionMenu | | |
| TASK-026 | Replace `pages/StockListPage.vue` — PageHeader + StockItemListTable | | |
| TASK-027 | Replace `pages/StockItemDetailPage.vue` — StockItemForm | | |
| TASK-028 | Update routes.ts if needed, update barrels | | |
| TASK-029 | Verify: type-check passes | | |

### Phase 3: Stock Transfers + Reservations + Movements

- GOAL-003: Implement Stock Transfers (CRUD + lifecycle), Stock Reservations (list + cancel), Stock Movements (list)

Backend endpoints:
- Transfers: GET `/stock-transfers` (paged), GET `/stock-transfers/{id}`, POST `/stock-transfers`, POST `/{id}/transfer`, POST `/{id}/receive`, POST `/{id}/cancel`
- Reservations: GET `/stock-reservations` (paged), GET `/{id}`, POST `/{id}/cancel`
- Movements: GET `/stock-movements` (paged), GET `/{id}`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-030 | Create `types/stock-transfer.response.ts` — `StockTransferResponse`: id, reference, sourceLocationId, sourceLocationName?, destinationLocationId, destinationLocationName?, status, lineItems: TransferLineItem[], notes?, createdAt, updatedAt; `TransferLineItem`: variantId, variantSku?, quantity, receivedQuantity | | |
| TASK-031 | Create `types/stock-transfer.request.ts` — `CreateStockTransferRequest`: sourceLocationId, destinationLocationId, lineItems: { variantId, quantity }[], notes? | | |
| TASK-032 | Create `api/stock-transfer.api.ts` — `StockTransferApi`: getMany(query), get(id), create(data), transfer(id) POST `/${id}/transfer`, receive(id) POST `/${id}/receive`, cancel(id) POST `/${id}/cancel` | | |
| TASK-033 | Create `types/stock-reservation.response.ts` — `StockReservationResponse`: id, orderId?, orderNumber?, variantId, variantSku?, quantity, status, expiresAt?, createdAt, updatedAt | | |
| TASK-034 | Create `api/stock-reservation.api.ts` — `StockReservationApi`: getMany(query), get(id), cancel(id) | | |
| TASK-035 | Create `types/stock-movement.response.ts` — `StockMovementResponse`: id, stockItemId, variantSku?, locationId, locationName?, quantity, direction, reason, reference?, createdAt | | |
| TASK-036 | Create `api/stock-movement.api.ts` — `StockMovementApi`: getMany(query), get(id) | | |
| TASK-037 | Replace transfer pages: TransferListPage.vue (transfer list table), TransferDetailPage.vue (transfer form with line items) | | |
| TASK-038 | Replace StockReservationListPage.vue (reservation list table) | | |
| TASK-039 | Replace MovementListPage.vue (movement list table) | | |
| TASK-040 | Create `composables/useStockTransfer.ts`, `composables/useStockMovement.ts` | | |
| TASK-041 | Update routes, barrels | | |
| TASK-042 | Verify: type-check passes | | |

### Phase 4: Inventory Dashboard

- GOAL-004: Implement Inventory Dashboard API integration

Backend endpoint: GET `/dashboard` returns `InventoryDashboardResponse`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-043 | Create `types/inventory-dashboard.response.ts` — `InventoryDashboardResponse`: totalStockItems, totalLocations, lowStockCount, outOfStockCount, totalReservedQuantity, totalTransfersPending, recentMovements: RecentMovementData[]; `RecentMovementData`: id, variantSku, locationName, quantity, direction, createdAt | | |
| TASK-044 | Create `api/inventory-dashboard.api.ts` — `InventoryDashboardApi` with get(): GET `/inventory/dashboard` | | |
| TASK-045 | Replace `pages/DashboardPage.vue` — load from API, show stat cards + recent movements table | | |
| TASK-046 | Update barrels, verify type-check passes | | |

## 3. Alternatives

- **ALT-001**: Build StockItems under Variant catalog module — rejected: Inventory is separate bounded context per backend module isolation

## 4. Dependencies

- **DEP-001**: Shared apiClient, Result/PagedResult/ListQuery types from `@/shared/`
- **DEP-002**: PlaceholderPage component replacement must retain route structure

## 5. Files

- **FILE-001** to **FILE-046**: One per task — types, schemas, mappers, API, stores, composables, components, pages, routes, barrels

## 6. Testing

- **TEST-001**: `api/__tests__/stock-locations.spec.ts` — mock apiClient, verify all 6 methods
- **TEST-002**: `api/__tests__/stock-items.spec.ts` — verify all 10 methods
- **TEST-003**: `api/__tests__/stock-transfers.spec.ts` — verify all 6 methods
- **TEST-004**: `api/__tests__/stock-reservations.spec.ts` — verify all 3 methods
- **TEST-005**: `api/__tests__/stock-movements.spec.ts` — verify all 2 methods
- **TEST-006**: `api/__tests__/inventory-dashboard.spec.ts` — verify get()

## 7. Risks & Assumptions

- **RISK-001**: StockItems.getLowStock and .getSummary might return different response shapes than the main paged list — check backend response types
- **ASSUMPTION-001**: Backend stock endpoints follow same Result/PagedResult patterns as catalog

## 8. Related Specifications / Further Reading

Backend: `service/Api/src/Module/Inventory/Features/Admin/`
Route constants: `service/Api/src/Module/Inventory/Features/Shared/InventoryFeature.Admin.cs`
