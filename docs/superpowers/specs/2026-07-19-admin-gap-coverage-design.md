# Admin Gap Coverage — Design

**Date:** 2026-07-19
**Status:** Approved

## Goal

Close the 5 categories of gaps found in the backend↔frontend↔UI coverage audit:

1. Route registration — views exist, inaccessible
2. Missing frontend API — 13 C# endpoints with no TS wrapper
3. Dedicated UI — 4 new views/features for wired-but-unshown APIs
4. Remaining stubs — clean up truly dead stubs
5. Dead code — remove unused identity API layer

---

## Spec 1: Route Registration

12 views across 4 domains exist but have no router config or menu entries. Add routes + nav.

### Routes

```
/payments                    → PaymentList.View.vue
  /payments/:id              → PaymentDetail.View.vue
  /methods                   → PaymentMethodList.View.vue
  /methods/create            → PaymentMethodForm.View.vue
  /methods/:id/edit          → PaymentMethodForm.View.vue

/shipping                    → ShippingMethodList.View.vue
  /methods/create            → ShippingMethodForm.View.vue
  /methods/:id/edit          → ShippingMethodForm.View.vue
  /rates                     → ShippingRateList.View.vue
  /rates/create              → ShippingRateForm.View.vue
  /rates/:id/edit            → ShippingRateForm.View.vue

/locations
  /countries                 → CountryList.View.vue
  /states                    → StateList.View.vue

/addresses                   → AddressList.View.vue (standalone, no user prop)
```

### Menu additions (`Menu.Layout.vue`)

New sections between existing ones:

```
Inventory
Locations   {pi-map-marker}  → Countries, States
Sales
Payments    {pi-credit-card} → Payments, Payment Methods
Shipping    {pi-truck}       → Shipping Methods, Shipping Rates
Identity
+ Addresses {pi-address-book} flat item under Identity
```

### Addresses conversion

`AddressList.View.vue` is a child component expecting `userId` prop. Convert to standalone page:
- Remove `userId` prop
- Add user picker dropdown at top: fetch user list from `userService.list()` + `userService.listCustomers()`, allow searching by name/email
- When a user is selected, call `addressService.getAll(userId)` to populate the table
- If no user selected, show "Select a user to view their addresses" empty state

### Files

| File | Action |
|------|--------|
| `features/payment/payment.routes.ts` | Create |
| `features/shipping/shipping.routes.ts` | Create |
| `features/location/location.routes.ts` | Create |
| `features/profile/addresses/addresses.routes.ts` | Create |
| `app/router/index.ts` | Edit — import + register 4 modules |
| `app/layout/Menu.Layout.vue` | Edit — add 4 sections |
| `profile/addresses/views/AddressList.View.vue` | Edit — remove userId prop, standalone |

---

## Spec 2: Missing Frontend API Functions

13 C# endpoints exist but have no TS API wrapper. Add functions following existing patterns.

### Catalog — sub-resource assign/revoke (5)

| Endpoint | Function | File |
|----------|----------|------|
| `POST /catalog/products/{id}/option-types/assign` | `assignOptionTypes` | `product-option-type.api.ts` |
| `DELETE /catalog/products/{id}/option-types/revoke` | `revokeOptionTypes` | `product-option-type.api.ts` |
| `POST /catalog/products/{id}/classifications/assign` | `assignClassifications` | `product-classification.api.ts` |
| `DELETE /catalog/products/{id}/classifications/revoke` | `revokeClassifications` | `product-classification.api.ts` |
| `GET /catalog/variants/{id}/option-values` | `listVariantOptionValues` | `variant.api.ts` |

### Catalog — variant images (3)

| Endpoint | Function | File |
|----------|----------|------|
| `GET /catalog/variants/images/{id}` | `getImageById` | `image.api.ts` |
| `GET /catalog/variants/images/{id}/download` | `downloadImage` | `image.api.ts` |
| `POST /catalog/variants/images/{id}/embeddings` | `generateEmbedding` | `image.api.ts` |

Note: `downloadImage` uses `responseType: 'blob'` for file download.

### Inventory (1)

| Endpoint | Function | File |
|----------|----------|------|
| `POST /inventory/stock-items/import` | `importStockItems` (FormData, multipart) | `stock.api.ts` |

### Identity — wire 501 stubs (3)

| Stub | Endpoint (exists) | Fix |
|------|-------------------|-----|
| `userService.assignPermission` | `POST /identity/users/{id}/permissions/assign` | Wire to real endpoint |
| `userService.assignRole` | `POST /identity/users/{id}/roles/assign` | Wire to real endpoint |
| `userService.revokeRole` | `POST /identity/users/{id}/roles/revoke` | Add service method, wire |

### Pattern

All new functions follow:
```typescript
const fn = async (...args): Promise<Result<T>> => {
  const response = await apiClient.get/post/put/delete(url, payload)
  return mapValue(response, mapResponse)
}
```

### Files

| File | Action |
|------|--------|
| `product-option-type.api.ts` | Edit — add `assignOptionTypes`, `revokeOptionTypes` |
| `product-classification.api.ts` | Edit — add `assignClassifications`, `revokeClassifications` |
| `variant.api.ts` | Edit — add `listVariantOptionValues` |
| `image.api.ts` | Edit — add `getImageById`, `downloadImage`, `generateEmbedding` |
| `stock.api.ts` | Edit — add `importStockItems` |
| `user.service.ts` | Edit — un-stub `assignPermission`, `assignRole`; add `revokeRole` |

---

## Spec 3: Dedicated UI

4 new features that have wired APIs but no dedicated screens.

### Stock movements list

New full-page DataTable with filters: movement type, date range, variant, stock location. Currently only shown as inline mini-table on dashboard and stock-item drawer.

### Stock items import

New page: file upload (CSV), preview grid of parsed rows, confirm button → calls `importStockItems` added in Spec 2.

### OrderDetail additions

- **Resume button** — visible when `status === Canceled`, calls `orderService.resume`
- **Line-item inline edit** — editable quantity field per line item, save/delete actions

### Delivery method

For each feature: create view + route + menu item. Extend existing stores/services as needed (they already exist — just not called from UI).

### Files

| File | Action |
|------|--------|
| `inventories/stock-movements/views/StockMovementList.View.vue` | Create |
| `inventories/stock-movements/stock-movements.routes.ts` | Create (or extend inventory.routes.ts) |
| `inventories/stock-items/views/StockImport.View.vue` | Create |
| `inventories/stock-items/stock-import.routes.ts` | Create (or extend inventory.routes.ts) |
| `ordering/orders/views/OrderDetail.View.vue` | Edit — resume + line-item CRUD |
| `inventory.routes.ts` | Edit — add movements + import routes |
| `Menu.Layout.vue` | Edit — add "Movements" + "Import Stock" items under Inventory |

---

## Spec 4: Clean Up Remaining Stubs

### Remove dead product-level image stubs

Backend has **variant-level** images only (upload/get/update/delete per variant). Product-level `getImages/uploadImage/deleteImage/updateImage` in `product.service.ts` have no backend equivalent. Remove them.

### Keep stubs that need backend work

These stubs represent features that need new C# endpoints before the frontend can wire them. Out of scope for this cycle.

| Stub | Reason |
|------|--------|
| `taxonService.getProductPreview` | Needs new `GET .../rules/preview` backend endpoint (read-only rule evaluation) |
| `taxonService.regenerateProducts` | Calls nonexistent `POST .../rules/regenerate` (404). Needs dedicated backend endpoint. |
| `userService.resetPassword` | No C# admin password-reset endpoint |
| `userService.unlockAccount` | No C# unlock endpoint |
| `userService.verifyAccount` | No C# verify endpoint |
| `orderService.createShipment` | No C# shipment endpoint |
| `optionValueService.reorder` | No C# reorder endpoint |
| `inventoryService.adjustStock` | No C# standalone adjust endpoint |
| `inventoryService.addTransferItem` | No C# transfer-item endpoint |
| `inventoryService.shipTransfer` | No C# ship-transfer endpoint |

### Files

| File | Action |
|------|--------|
| `product.service.ts` | Edit — remove 4 product-level image stubs |

---

## Spec 5: Dead Code Cleanup

| File | Action |
|------|--------|
| `features/identity/api/identity.api.ts` | Delete — duplicate of `features/users/api/user.api.ts` |
| `features/identity/api/__tests__/identity.api.spec.ts` | Delete |

---

## Execution Order

1. Spec 1 — Route registration (lowest risk, highest visible impact)
2. Spec 5 — Dead code (trivial, safe)
3. Spec 2 — Missing APIs (standalone, no UI dependencies)
4. Spec 4 — Clean stubs (depends on Spec 2 for wired functions)
5. Spec 3 — Dedicated UI (depends on Spec 1+2 for routes/APIs)

## Non-Goals

- No backend changes — C# endpoints already exist
- No storefront coverage — admin only
- No test coverage additions (follow project convention: tests per endpoint spec file where applicable)
- No i18n for new UI strings (use existing i18n patterns, add keys as needed)

## Shared Concerns

- All new route files follow existing `.routes.ts` pattern: export `RouteRecordRaw[]` with `beforeEnter: authGuard`
- All new API functions follow existing repository pattern: `apiClient.method(path, payload?)` → `mapValue` → `Result<T>`
- Menu uses static array in `Menu.Layout.vue` — must add items manually alongside each route addition
- AddressList conversion: must preserve existing functionality while removing prop dependency
