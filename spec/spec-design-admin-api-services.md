---
title: Admin SPA API Service Layer - Correct Endpoint Mappings
version: 1.0
date_created: 2026-07-16
owner: Admin Frontend
tags: design, api, frontend, admin
---

# Admin SPA API Service Layer

Correct endpoint mappings for Admin SPA `src/features/*` service layer against actual backend routes. Excludes `.view.vue` files.

## 1. Purpose & Scope

Define exact backend API routes for every service call in the Admin frontend. Currently `constants.ts` paths are wrong — all calls will 404. This spec documents the correct mapping and identifies operations where the frontend invents routes that don't exist on the backend.

Audience: Admin frontend & API developers.

## 2. Definitions

- `baseURL`: Axios default `/api` (set in `api.client.ts:7`)
- `VITE_API_URL`: env var, proxied by Vite dev server at `:5173/api → :5035`
- `createModuleApi<T>({basePath})`: generates `list`, `getById`, `create`, `update`, `delete` using `basePath`
- `createCrudService<T>`: same shape, without sub-resource helpers

## 3. Requirements, Constraints & Guidelines

- **REQ-001**: Every `apiClient.get/post/put/delete/patch` call must resolve to an existing backend route at runtime.
- **REQ-002**: `constants.ts` values must match `*Feature.Admin.Route` constants in backend `service/Api/src/Module/{Module}/Features/Shared/`.
- **REQ-003**: Sub-resource paths must use path params (e.g. `/products/{id}/variants`), not query params (`/products/variants?productId={id}`).
- **CON-001**: No `/admin/` prefix in any backend route — Admin routes are directly under `api/{module}/`.
- **CON-002**: No reverse proxy or path-rewriting middleware exists.
- **PAT-001**: Child resources nested under parent path: `/api/{module}/{parentId}/{child}`.

### Base Route Correction Table

| Constant | Current (broken) | Correct | Backend `*Feature.Admin.Route` |
|---|---|---|---|
| `CATALOG` | `/admin/catalog` | `api/catalog` | `api/catalog` |
| `IDENTITY` | `/admin/identity` | `api/identity` | `api/identity` |
| `LOCATION` | `/admin/location` | `api/locations` | `api/locations` |
| `ORDERS` | `/admin/orders` | `api/ordering` | `api/ordering` |
| `PROFILE` | `/admin/profile` | `api/profiles` | `api/profiles` |
| `INVENTORY` | `/inventories` | `api/inventory` | `api/inventory` |
| `DASHBOARD` | `/admin/dashboard` | N/A — no backend module | see §4 |
| `SEARCH` | `/admin/search` | N/A — no backend module | see §4 |
| `FILES` | `/files` | N/A — no backend module | see §4 |
| `ACCOUNT` | `/account` | N/A — no backend module | see §4 |
| `AUTH` | `/auth` | N/A — no admin auth module | see §4 |

## 4. Interfaces & Data Contracts

### 4.0 Shared Infrastructure

**File**: `src/shared/api/constants.ts`
**Fix**: Replace all 11 constants with correct values listed in correction table above. Remove `DASHBOARD`, `SEARCH`, `FILES`, `ACCOUNT` — no backend routes exist for these. Remove `AUTH` — create `IDENTITY`-based auth endpoints instead.

**File**: `src/shared/api/http/refresh-handler.ts:16`
- Current: `axios.post('/api/auth/session/refresh', ...)`
- Broken: no backend route at `/api/auth/session/refresh`
- Fix: backend identity has no admin refresh endpoint — needs new route or use storefront identity route

**File**: `src/shared/api/services/crud.service.ts`
- ✅ Generic, path-based. No route fixes needed.

**File**: `src/shared/api/services/module-api.factory.ts`
- ✅ Generic. Sub-resource helpers. No route fixes needed.

**File**: `src/shared/api/types/*`
- ✅ Pure types. No route fixes needed.

---

### 4.1 Auth

**Service file**: `src/features/auth/services/auth.service.ts`

**Current base path**: `/auth`

| Method | Current call | Correct backend route | Status |
|---|---|---|---|
| `login` | `POST /auth/login` | `POST api/store/identity/auth/login/password` | ❌ storefront route, no admin login endpoint |
| `refresh` | `POST /auth/session/refresh` | N/A — no admin refresh endpoint | ❌ no equivalent |
| `logout` | `POST /auth/session/logout` | N/A — no admin logout endpoint | ❌ no equivalent |
| `getProfile` | `GET /account/profile` | N/A — no admin profile endpoint | ❌ no equivalent |
| `updateProfile` | `PUT /account/profile` | N/A — no admin profile endpoint | ❌ no equivalent |
| `changePassword` | `POST /auth/password/change` | N/A — no admin password endpoint | ❌ no equivalent |

**Issue**: Backend has zero admin-specific auth endpoints. Identity module storefront routes exist at `api/store/identity/auth/*`. Either:
- Add admin auth endpoints to Identity module, or
- Reuse storefront auth routes (not ideal — different auth flow for admins)

**Types**: `src/features/auth/types/auth.types.ts`
- ✅ `LoginRequest`, `RefreshRequest`, `AuthenticationResponse`, `UserProfile` — pure type contracts.

**Schemas**: `src/features/auth/schemas/auth.schema.ts`
- ✅ Zod schema for login form validation.

**Store**: `src/features/auth/stores/auth.store.ts`
- ✅ State management. `refreshSession()` delegates to `authService.refresh()`.

**Locales**: `src/features/auth/locales/auth.locales.ts`
- ✅ i18n labels. No route concerns.

---

### 4.2 Catalog

**Service file**: `src/features/catalog/services/catalog.api.ts`

**Correct base path**: `api/catalog`

#### Products

| Operation | Current call | Correct backend route |
|---|---|---|
| `products.list` | `GET {CATALOG}/products` | `GET api/catalog/products` |
| `products.getById(id)` | `GET {CATALOG}/products/{id}` | `GET api/catalog/products/{id}` |
| `products.create` | `POST {CATALOG}/products` | `POST api/catalog/products` |
| `products.update(id, ...)` | `PUT {CATALOG}/products/{id}` | `PUT api/catalog/products/{id}` |
| `products.delete(id)` | `DELETE {CATALOG}/products/{id}` | `DELETE api/catalog/products/{id}` |
| `products.getOptionTypes(productId)` | `GET {CATALOG}/products/option-types?productId={productId}` | `GET api/catalog/products/{id}/option-types` |
| `products.updateOptionTypes(productId, ...)` | `PUT {CATALOG}/products/option-types` | `POST api/catalog/products/{id}/option-types/assign` |
| `products.getProperties(productId)` | `GET {CATALOG}/products/properties?productId={id}` | N/A — no product properties backend |
| `products.updateProperties(productId, ...)` | `PUT {CATALOG}/products/properties` | N/A — no product properties backend |
| `products.getImages(productId)` | `GET {CATALOG}/products/images?productId={id}` | `GET api/catalog/products/variants/{variantId}/images` |
| `products.uploadImage(productId, ...)` | `POST {CATALOG}/products/images?productId={id}&role={r}` | `POST api/catalog/products/variants/{variantId}/images` |

**Fixes needed**:
- `getOptionTypes`: use path param `/products/{productId}/option-types` not query param
- `updateOptionTypes`: use `POST api/catalog/products/{productId}/option-types/sync` with body, not PUT
- `getProperties` / `updateProperties`: no backend routes exist for product properties — either add or remove
- `getImages` / `uploadImage` / `updateImage` / `deleteImage`: backend scopes images under **variants** (`/products/variants/{variantId}/images`), not products directly. Restructure.

#### Variants

| Operation | Current call | Correct backend route |
|---|---|---|
| `variants.list` | `GET {CATALOG}/variants` | N/A — no standalone variant list |
| `variants.getById(id)` | `GET {CATALOG}/variants/{id}` | `GET api/catalog/products/variants/{id}` |
| `variants.create(productId, ...)` | `POST {CATALOG}/products/{productId}/variants` | `POST api/catalog/products/{productId}/variants` |
| `variants.update(id, ...)` | `PUT {CATALOG}/variants/{id}` | `PUT api/catalog/products/variants/{id}` |
| `variants.delete(id)` | `DELETE {CATALOG}/variants/{id}` | `DELETE api/catalog/products/variants/{id}` |
| `variants.listByProductId(productId)` | `GET {CATALOG}/products/{productId}/variants` | `GET api/catalog/products/{productId}/variants` |
| `variants.setMaster(id)` | `POST {CATALOG}/variants/{id}/set-master` | N/A — no `set-master` endpoint |
| `variants.updateOptionValues(id, ...)` | `PUT {CATALOG}/variants/{id}/option-values` | `PUT api/catalog/products/variants/{id}/option-values/sync` |

**Fixes**:
- `variants.list`: remove — no standalone list, get via product instead
- `setMaster`: backend has no master variant toggle. Add backend or remove frontend.
- `updateOptionValues`: use `sync` sub-path

#### Variants → Prices

Backend has 4 price endpoints at `/products/variants/{variantId}/prices` — frontend wraps `catalog.api.variants` in generic `createModuleApi` which generates wrong URLs. Must create explicit price operations.

#### OptionTypes

| Operation | Current call | Correct backend route |
|---|---|---|
| `optionTypes.list` | `GET {CATALOG}/option-types` | `GET api/catalog/option-types` |
| `optionTypes.getById(id)` | `GET {CATALOG}/option-types/{id}` | `GET api/catalog/option-types/{id}` |
| `optionTypes.create` | `POST {CATALOG}/option-types` | `POST api/catalog/option-types` |
| `optionTypes.update(id)` | `PUT {CATALOG}/option-types/{id}` | `PUT api/catalog/option-types/{id}` |
| `optionTypes.delete(id)` | `DELETE {CATALOG}/option-types/{id}` | `DELETE api/catalog/option-types/{id}` |

✅ All correct when `CATALOG = api/catalog`.

#### OptionValues

| Operation | Current call | Correct backend route |
|---|---|---|
| `optionValues.list` | `GET {CATALOG}/option-values` | `GET api/catalog/option-types/{optionTypeId}/values` |
| `optionValues.getById(id)` | `GET {CATALOG}/option-values/{id}` | `GET api/catalog/option-types/{optionTypeId}/values/{id}` |
| `optionValues.create(data)` | `POST {CATALOG}/option-values` | `POST api/catalog/option-types/{optionTypeId}/values` |
| `optionValues.update(id)` | `PUT {CATALOG}/option-values/{id}` | `PUT api/catalog/option-types/{optionTypeId}/values/{id}` |
| `optionValues.delete(id)` | `DELETE {CATALOG}/option-values/{id}` | `DELETE api/catalog/option-types/{optionTypeId}/values/{id}` |
| `optionValues.reorder(data)` | `PUT {CATALOG}/option-values/positions` | N/A — no positions endpoint |

**Fixes**:
- All CRUD operations use wrong path — backend scopes option-values under **option-types** (`/option-types/{optionTypeId}/values`). Frontend treats them as flat CRUD at `/option-values`.
- `reorder`: no backend equivalent. Add or remove.

#### PropertyTypes (removed)

PropertyTypes feature was scaffolded but never implemented on the backend. No domain entity, database table, migration, or API endpoints exist. The feature was removed from both frontend and backend. See `plan/drop-property-type-admin-1.md`.

#### Taxonomies

| Operation | Current call | Correct backend route |
|---|---|---|
| `taxonomies.list` | `GET {CATALOG}/taxonomies` | `GET api/catalog/taxonomies` |
| `taxonomies.getById(id)` | `GET {CATALOG}/taxonomies/{id}` | `GET api/catalog/taxonomies/{id}` |
| `taxonomies.create` | `POST {CATALOG}/taxonomies` | `POST api/catalog/taxonomies` |
| `taxonomies.update(id)` | `PUT {CATALOG}/taxonomies/{id}` | `PUT api/catalog/taxonomies/{id}` |
| `taxonomies.delete(id)` | `DELETE {CATALOG}/taxonomies/{id}` | `DELETE api/catalog/taxonomies/{id}` |

✅ All correct when `CATALOG = api/catalog`.

**Store extra call** (`taxonomy.store.ts:2243`):
- `POST apiClient.post("/admin/catalog/taxonomies/{id}/rebuild")` — hardcoded wrong path + no backend `rebuild` endpoint. Fix to `POST api/catalog/taxonomies/{id}/restore` or remove.

#### Taxons

| Operation | Current call | Correct backend route |
|---|---|---|
| `taxons.getTaxons` | `GET {CATALOG}/taxons` | `GET api/catalog/taxonomies/{taxonomyId}/taxons` |
| `taxons.getTree` | `GET {CATALOG}/taxons/tree` | `GET api/catalog/taxonomies/{taxonomyId}/taxons/tree` |
| `taxons.getById(id)` | `GET {CATALOG}/taxons/{id}` | `GET api/catalog/taxonomies/{taxonomyId}/taxons/{id}` |
| `taxons.create` | `POST {CATALOG}/taxons` | `POST api/catalog/taxonomies/{taxonomyId}/taxons` |
| `taxons.update(id)` | `PUT {CATALOG}/taxons/{id}` | `PUT api/catalog/taxonomies/{taxonomyId}/taxons/{id}` |
| `taxons.delete(id)` | `DELETE {CATALOG}/taxons/{id}` | `DELETE api/catalog/taxonomies/{taxonomyId}/taxons/{id}` |
| `taxons.getRules(taxonId)` | `GET {CATALOG}/taxons/{taxonId}/rules` | `GET api/catalog/taxonomies/{taxonomyId}/taxons/{taxonId}/rules` |
| `taxons.addRule(taxonId)` | `POST {CATALOG}/taxons/{taxonId}/rules` | `POST api/catalog/taxonomies/{taxonomyId}/taxons/{taxonId}/rules` |
| `taxons.updateRule(taxonId, ruleId)` | `PUT {CATALOG}/taxons/{taxonId}/rules/{ruleId}` | `PUT api/catalog/taxonomies/{taxonomyId}/taxons/{taxonId}/rules/{ruleId}` |
| `taxons.deleteRule(taxonId, ruleId)` | `DELETE {CATALOG}/taxons/{taxonId}/rules/{ruleId}` | `DELETE api/catalog/taxonomies/{taxonomyId}/taxons/{taxonId}/rules/{ruleId}` |
| `taxons.regenerateProducts(taxonId)` | `POST {CATALOG}/taxons/{taxonId}/rules/regenerate` | `POST api/catalog/taxonomies/{taxonomyId}/taxons/{taxonId}/rules/regenerate` |
| `taxons.getProductPreview(taxonId)` | `GET {CATALOG}/taxons/{taxonId}/preview` | N/A — no backend route for taxon product preview |

**Fixes**:
- All taxon endpoints missing `taxonomyId` in path — backend nests taxons under taxonomy
- All use flat `{CATALOG}/taxons/{tid}/...` should be `{CATALOG}/taxonomies/{taxonomyId}/taxons/{tid}/...`
- `getTaxons` / `getTree` / `getProductPreview` — need `taxonomyId` param added
- Frontend `taxon.service.ts` and `taxon.store.ts` pass `taxonomyId` in query params but backend expects path param

#### Catalog Dashboard

| Operation | Current call | Correct backend route |
|---|---|---|
| `dashboard.getSummary` | `GET /admin/dashboard/catalog-summary` | N/A — no backend route |

**Fix**: No backend route exists. Either add `GET api/catalog/dashboard/summary` to Catalog module or remove frontend call.

**Store**: `src/features/catalog/dashboard/stores/catalog-dashboard.store.ts` — ✅ state management, delegates to service.

---

### 4.3 Identity

**Service file**: `src/features/identity/services/identity.api.ts`

**Correct base path**: `api/identity`

#### Users

| Operation | Current call | Correct backend route |
|---|---|---|
| `users.listAdmins(params)` | `GET {IDENTITY}/users` | `GET api/identity/users` |
| `users.getAdminDetail(id)` | `GET {IDENTITY}/users/{id}` | `GET api/identity/users/{id}` |
| `users.createAdmin(data)` | `POST {IDENTITY}/users` | `POST api/identity/users` |
| `users.updateAdmin(id, data)` | `PUT {IDENTITY}/users/{id}` | `PUT api/identity/users/{id}` |
| `users.deleteAdmin(id)` | `DELETE {IDENTITY}/users/{id}` | `DELETE api/identity/users/{id}` |
| `users.updateStatus(id, active)` | `PATCH {IDENTITY}/users/{id}/status` | `PATCH api/identity/users/{id}/status` |
| `users.updateStaffProfile(id)` | `PUT {IDENTITY}/users/{id}/staff-profile` | N/A — no backend route |
| `users.resetPassword(id)` | `POST {IDENTITY}/users/{id}/reset-password` | N/A — no backend route |
| `users.unlockAccount(id)` | `POST {IDENTITY}/users/{id}/unlock` | N/A — no backend route |
| `users.verifyAccount(id)` | `POST {IDENTITY}/users/{id}/verify` | N/A — no backend route |
| `users.getRoles(id)` | `GET {IDENTITY}/users/{id}/roles` | `GET api/identity/users/{id}/roles` |
| `users.assignRole(id, role)` | `POST {IDENTITY}/users/{id}/roles` | `POST api/identity/users/{id}/roles/assign` |
| `users.unassignRole(id, role)` | `DELETE {IDENTITY}/users/{id}/roles/{roleName}` | `POST api/identity/users/{id}/roles/revoke` |
| `users.syncRoles(id, roles)` | `PUT {IDENTITY}/users/{id}/roles` | `PATCH api/identity/users/{id}/roles/sync` |
| `users.getPermissions(id)` | `GET {IDENTITY}/users/{id}/permissions` | `GET api/identity/users/{id}/permissions` |
| `users.assignPermission(id, perm)` | `POST {IDENTITY}/users/{id}/permissions` | `POST api/identity/users/{id}/permissions/assign` |
| `users.unassignPermission(id, perm)` | `DELETE {IDENTITY}/users/{id}/permissions/{perm}` | `DELETE api/identity/users/{id}/permissions/revoke` |
| `users.listCustomers(params)` | `GET {IDENTITY}/users?role=Storefront.Customer` | `GET api/identity/users?role=Storefront.Customer` |

**Fixes needed** when `IDENTITY = api/identity`:
- All frontend sub-resource paths need backend route alignment (method + sub-path)
- `updateStaffProfile`, `resetPassword`, `unlockAccount`, `verifyAccount` have no backend routes

#### Roles

| Operation | Current call | Correct backend route |
|---|---|---|
| `roles.list` | `GET {IDENTITY}/roles` | `GET api/identity/roles` |
| `roles.getById(id)` | `GET {IDENTITY}/roles/{id}` | `GET api/identity/roles/{id}` |
| `roles.create` | `POST {IDENTITY}/roles` | `POST api/identity/roles` |
| `roles.update(id)` | `PUT {IDENTITY}/roles/{id}` | `PUT api/identity/roles/{id}` |
| `roles.delete(id)` | `DELETE {IDENTITY}/roles/{id}` | `DELETE api/identity/roles/{id}` |
| `roles.getUsersInRole(name)` | `GET {IDENTITY}/roles/{name}/users` | N/A — no backend route |
| `roles.assignPermission(id, perm)` | `POST {IDENTITY}/roles/{id}/permissions` | `PUT api/identity/roles/{id}/permissions/assign` |
| `roles.syncPermissions(id, perms)` | `PUT {IDENTITY}/roles/{id}/permissions` | `PATCH api/identity/roles/{id}/permissions/sync` |
| `roles.unassignPermission(id, perm)` | `DELETE {IDENTITY}/roles/{id}/permissions/{perm}` | `DELETE api/identity/roles/{id}/permissions/revoke` |

**Fixes**: Sub-resource HTTP methods and paths don't match backend (frontend uses PUT where backend uses PATCH, etc.)

#### Permissions

| Operation | Current call | Correct backend route |
|---|---|---|
| `permissions.list` | `GET {IDENTITY}/permissions` | `GET api/identity/permissions` |
| `permissions.getSelect` | `GET {IDENTITY}/permissions/select` | N/A — no backend route |

✅ List correct when `IDENTITY = api/identity`. `getSelect` has no backend route.

---

### 4.4 Inventories

**Service file**: `src/features/inventories/services/inventory.api.ts`

**Current base path**: hardcoded `/inventories` — should be `api/inventory`

| Operation | Current call | Correct backend route |
|---|---|---|
| `stocks.list` | `GET /inventories/stocks` | `GET api/inventory/stock-items` |
| `stocks.getById(id)` | `GET /inventories/stocks/{id}` | `GET api/inventory/stock-items/{id}` |
| `stocks.adjust(id, ...)` | `POST /inventories/stocks/{id}/adjust` | `POST api/inventory/stock-items/{id}/restock` |
| `stocks.audit(id, ...)` | `POST /inventories/stocks/{id}/audit` | N/A — no backend audit |
| `stocks.updateBackorderPolicy(id)` | `PUT /inventories/stocks/{id}/backorder-policy` | N/A — no backend route |
| `stocks.delete(id)` | `DELETE /inventories/stocks/{id}` | `DELETE api/inventory/stock-items/{id}` |
| `units.list` | `GET /inventories/units` | `GET api/inventory/stock-reservations` |
| `units.getById(id)` | `GET /inventories/units/{id}` | `GET api/inventory/stock-reservations/{id}` |
| `units.updateSerialNumber(id)` | `PATCH /inventories/units/{id}/serial-number` | N/A — no backend route |
| `units.markDamaged(id)` | `PATCH /inventories/units/{id}/damaged` | N/A — no backend route |
| `units.restore(id)` | `POST /inventories/units/{id}/restore` | N/A — no backend route |
| `movements.list` | `GET /inventories/movements` | `GET api/inventory/stock-movements` |
| `locations.list` | `GET /inventories/locations` | `GET api/inventory/stock-locations` |
| `locations.getTree` | `GET /inventories/locations/tree` | N/A — no backend tree endpoint |
| `locations.getById(id)` | `GET /inventories/locations/{id}` | `GET api/inventory/stock-locations/{id}` |
| `locations.create` | `POST /inventories/locations` | `POST api/inventory/stock-locations` |
| `locations.update(id)` | `PUT /inventories/locations/{id}` | `PUT api/inventory/stock-locations/{id}` |
| `locations.delete(id)` | `DELETE /inventories/locations/{id}` | `DELETE api/inventory/stock-locations/{id}` |
| `locations.toggleStatus(id, active)` | `PATCH /inventories/locations/{id}/toggle-status` | `PUT api/inventory/stock-locations/{id}/default` |
| `transfers.list` | `GET /inventories/transfers` | `GET api/inventory/stock-transfers` |
| `transfers.getById(id)` | `GET /inventories/transfers/{id}` | `GET api/inventory/stock-transfers/{id}` |
| `transfers.create` | `POST /inventories/transfers` | `POST api/inventory/stock-transfers` |
| `transfers.addItem(id, ...)` | `POST /inventories/transfers/{id}/items` | N/A — backend has no add-item sub-route |
| `transfers.ship(id)` | `POST /inventories/transfers/{id}/ship` | `POST api/inventory/stock-transfers/{id}/transfer` |
| `transfers.receive(id)` | `POST /inventories/transfers/{id}/receive` | `POST api/inventory/stock-transfers/{id}/receive` |
| `transfers.cancel(id)` | `POST /inventories/transfers/{id}/cancel` | `POST api/inventory/stock-transfers/{id}/cancel` |

**Major issues**:
1. All routes use `/inventories` (with 's') — backend `api/inventory` (no 's')
2. Resource names mismatched: `stocks` → `stock-items`, `units` → `stock-reservations`, `locations` → `stock-locations`
3. Action endpoints (`adjust`, `audit`, `backorder-policy`, `serial-number`, `damaged`, `restore`, `addItem`, `ship` → `transfer`) don't match backend

---

### 4.5 Location

**Service file**: `src/features/location/services/location.api.ts`

**Correct base path**: `api/locations`

| Operation | Current call | Correct backend route |
|---|---|---|
| `countries.list` | `GET {LOCATION}/countries` | `GET api/locations/countries` |
| `countries.getById(id)` | `GET {LOCATION}/countries/{id}` | `GET api/locations/countries/{id}` |
| `countries.create` | `POST {LOCATION}/countries` | `POST api/locations/countries` |
| `countries.update(id)` | `PUT {LOCATION}/countries/{id}` | `PUT api/locations/countries/{id}` |
| `countries.delete(id)` | `DELETE {LOCATION}/countries/{id}` | `DELETE api/locations/countries/{id}` |
| `states.list` | `GET {LOCATION}/states` | `GET api/locations/states` |
| `states.getById(id)` | `GET {LOCATION}/states/{id}` | `GET api/locations/states/{id}` |
| `states.create` | `POST {LOCATION}/states` | `POST api/locations/states` |
| `states.update(id)` | `PUT {LOCATION}/states/{id}` | `PUT api/locations/states/{id}` |
| `states.delete(id)` | `DELETE {LOCATION}/states/{id}` | `DELETE api/locations/states/{id}` |

✅ All correct when `LOCATION = api/locations`. Only issue is constant name mismatch (`LOCATION` → `LOCATIONS` to match backend).

---

### 4.6 Ordering

**Service file**: `src/features/ordering/services/ordering.api.ts`

**Correct base path**: `api/ordering`

| Operation | Current call | Correct backend route |
|---|---|---|
| `orders.list` | `GET {ORDERS}` | `GET api/ordering/orders` |
| `orders.getById(id)` | `GET {ORDERS}/{id}` | `GET api/ordering/orders/{id}` |
| `orders.create` | `POST {ORDERS}` | `POST api/ordering/orders` |
| `orders.update(id)` | `PUT {ORDERS}/{id}` | `PUT api/ordering/orders/{id}` |
| `orders.delete(id)` | `DELETE {ORDERS}/{id}` | `DELETE api/ordering/orders/{id}` |
| `orders.createShipment(orderId, ...)` | `POST {ORDERS}/{orderId}/shipments` | N/A — backend has no shipment sub-route |
| `orders.cancelShipment(orderId, shipmentId)` | `DELETE {ORDERS}/{orderId}/shipments/{shipmentId}` | N/A — no backend route |
| `orders.addItem(id, ...)` | `POST {ORDERS}/{id}/items` | `POST api/ordering/orders/{id}/line-items` |
| `orders.updateAddresses(id, ...)` | `PUT {ORDERS}/{id}/addresses` | `PUT api/ordering/orders/{id}/ship-address` + `PUT .../bill-address` |
| `orders.updateState(id)` | `POST {ORDERS}/{id}/advance` | N/A — backend has no advance endpoint |
| `orders.cancelOrder(id, reason)` | `POST {ORDERS}/{id}/cancel` | `POST api/ordering/orders/{id}/cancel` |
| `orders.refundPayment(orderId, paymentId)` | `POST {ORDERS}/{orderId}/payments/{paymentId}/refund` | N/A — refund is in Payment module |

**Fixes**:
- `items` → `line-items`
- `addresses` → separate ship-address + bill-address calls
- `advance` → no backend route
- `shipments` → no backend sub-resource for shipments in Ordering
- `refundPayment` → belongs in Payment module (`api/payment/payments/{id}/refund`)

**Fulfillment**:
- `fulfillments.getQueue`: `GET {ORDERS}?state=Processing` → `GET api/ordering/orders?state=Processing` ✅ (when `ORDERS` fixed)
- `fulfillment.service.markAsShipped`: returns 501 — placeholder, no backend implementation

---

### 4.7 Profile

**Service file**: `src/features/profile/services/profile.api.ts`

**Correct base path**: `api/profiles`

| Operation | Current call | Correct backend route |
|---|---|---|
| `get()` | `GET {PROFILE}` | N/A — Profile module has no admin endpoints |
| `update(data)` | `PUT {PROFILE}` | N/A — Profile module has no admin endpoints |

**Issue**: Backend Profile module only has storefront routes at `api/store/profiles/*`. No admin profile endpoints exist. Either add to Profile module or remove frontend feature.

---

### 4.8 Reports

**Service file**: `src/features/reports/services/reports.api.ts`

**Current base path**: hardcoded `admin/dashboard/...` (with no leading `/`)

| Operation | Current call | Correct backend route |
|---|---|---|
| `getSalesSummary` | `GET admin/dashboard/sales-summary` | N/A — no backend route |
| `getInventorySummary` | `GET admin/dashboard/inventory-summary` | N/A — no backend route |
| `getCatalogSummary` | `GET admin/dashboard/catalog-summary` | N/A — no backend route |
| `getRecentActivity` | `GET admin/dashboard/recent-activity` | N/A — no backend route |

**Issue**: All four dashboard endpoints use hardcoded `admin/dashboard/` prefix (no leading `/`) which resolves to `/api/admin/dashboard/*` (via `baseURL: '/api'`). Backend has no dashboard module at all. These all need new backend endpoints or removal.

---

### 4.9 Users

**Service files**: `src/features/users/services/{user,role,permission}.service.ts`

All three are thin wrappers that delegate to `identityApi.*` from the Identity feature. No direct route definitions — see §4.3 for correct identity routes.

---

### 4.10 Dashboard

**File**: `src/features/dashboard/ui/DashboardPage.vue`

No direct service calls. Reads JWT claims from `useAuthStore().user`. No route concerns.

---

### 4.11 Error

**Files**: `src/features/error/error.routes.ts`

No API service calls. Pure routing to static error pages. No route concerns.

---

## 5. Corrected API Constants

Replace `src/shared/api/constants.ts` with:

```typescript
export const CATALOG = 'api/catalog'
export const IDENTITY = 'api/identity'
export const LOCATIONS = 'api/locations'
export const PROFILES = 'api/profiles'
export const INVENTORY = 'api/inventory'
export const ORDERS = 'api/ordering'
export const PAYMENTS = 'api/payment'
export const SHIPPING = 'api/shipping'
```

Remove: `API_PREFIX`, `DASHBOARD`, `SEARCH`, `FILES`, `ACCOUNT`, `AUTH`. Add auth under Identity module.

## 6. Summary of Backend Gaps

Operations where frontend calls a route that has no backend equivalent:

| Feature | Operations missing from backend |
|---|---|
| **Auth** | All 6 auth endpoints (login, refresh, logout, profile, profile update, change password) |
| **Catalog** | Product properties CRUD (get/update), image product-scoped routes (not variant-scoped), variant `set-master`, variant price CRUD (not wired in service), option-value flat CRUD (nested under option-types), option-value `reorder`, taxon flat CRUD (nested under taxonomy), taxon product preview |
| **Identity** | Staff profile update, password reset, account unlock, verify account, role user listing, permission select list |
| **Inventory** | Stock audit, backorder policy update, serial number update, damaged/restore unit, location tree, bulk adjust, low stock, summary, import, stock reservation cancel, add transfer item (frontend posts to `/items`) |
| **Location** | Country/state by-ISO routes (frontend doesn't use them) |
| **Ordering** | Shipment create/cancel, order state advance, refund (belongs in Payment module) |
| **Payment** | No frontend feature folder exists despite 12 backend endpoints |
| **Shipping** | No frontend feature folder exists despite 12 backend endpoints |
| **Profile** | All admin profile endpoints (storefront only) |
| **Reports** | All 4 dashboard/report endpoints |
| **Search** | No backend search endpoint |
| **Files** | No backend file upload endpoint (images go through variant images) |

## 7. Acceptance Criteria

- **AC-001**: All 11 constants in `constants.ts` resolve to existing backend routes with correct pluralization.
- **AC-002**: Catalog product sub-resources use path params (`{CATALOG}/products/{id}/...`) not query params.
- **AC-003**: Catalog taxon sub-resources include `taxonomyId` in path not query.
- **AC-004**: Catalog option-value sub-resources include `optionTypeId` in path not query.
- **AC-005**: Inventory routes use `api/inventory/stock-items`, `api/inventory/stock-locations`, `api/inventory/stock-transfers`, `api/inventory/stock-movements`, `api/inventory/stock-reservations`.
- **AC-006**: Inventory transfer actions use correct backend verb endpoints (`transfer`, `receive`, `cancel`).
- **AC-007**: All hardcoded `/admin/...` prefixes replaced with correct module paths.
- **AC-008**: No frontend API call targets a route absent from backend (marked N/A above) unless that backend route is explicitly planned.
- **AC-009**: `refresh-handler.ts` uses correct Identity refresh endpoint.
