# Admin Gap Coverage Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Close 5 categories of gaps between backend endpoints and Admin frontend: route registration (12 views), missing API functions (13), dedicated UI (4 features), stub cleanup (1 file), and dead code deletion (1 file).

**Architecture:** Follow existing patterns: Carter-style `.routes.ts` for router modules, `apiClient.get/post` → `mapValue` → `Result<T>` for API wrappers, Pinia stores → services → views for data flow. Menu is a static `ref<MenuItem[]>` array in `Menu.Layout.vue` — each route addition needs a matching menu entry.

**Tech Stack:** Vue 3, TypeScript, Vue Router, axios (via apiClient), PrimeVue components

## Global Constraints

- `TreatWarningsAsErrors=true` — zero TS errors allowed
- All route files export `RouteRecordRaw` or `RouteRecordRaw[]` following existing patterns
- All API functions use `apiClient` from `@/shared/api/http/api.client`
- Menu entries use named routes (`{ name: '...' }`), not literal paths
- No backend changes — all C# endpoints already exist
- Follow existing vertical-slice feature organization

---

### Task 1: Create payment.routes.ts

**Files:**
- Create: `app/Admin/src/features/payment/payment.routes.ts`

**Interfaces:**
- Produces: `export const paymentRoutes: RouteRecordRaw`

- [ ] **Step 1: Create the payment routes file**

```typescript
import type { RouteRecordRaw } from 'vue-router'

export const paymentRoutes: RouteRecordRaw = {
  path: 'payments',
  meta: { breadcrumb: 'Payments' },
  children: [
    {
      path: '',
      name: 'payment.payments.list',
      component: () => import('./payments/views/PaymentList.View.vue'),
      meta: { breadcrumb: 'All Payments' },
    },
    {
      path: ':id',
      name: 'payment.payments.detail',
      component: () => import('./payments/views/PaymentDetail.View.vue'),
      meta: { breadcrumb: 'Payment Details' },
    },
    {
      path: 'methods',
      name: 'payment.methods.list',
      component: () => import('./payment-methods/views/PaymentMethodList.View.vue'),
      meta: { breadcrumb: 'Payment Methods' },
    },
    {
      path: 'methods/create',
      name: 'payment.methods.create',
      component: () => import('./payment-methods/views/PaymentMethodForm.View.vue'),
      meta: { breadcrumb: 'Add Method' },
    },
    {
      path: 'methods/:id/edit',
      name: 'payment.methods.edit',
      component: () => import('./payment-methods/views/PaymentMethodForm.View.vue'),
      meta: { breadcrumb: 'Edit Method' },
    },
  ],
}
```

- [ ] **Step 2: Verify TypeScript compiles**

```bash
cd app/Admin && pnpm run type-check 2>&1 | grep "error TS" | wc -l
```

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/payment/payment.routes.ts
git commit -m "feat(admin): add payment routes (payments list/detail, payment methods CRUD)"
```

---

### Task 2: Create shipping.routes.ts

**Files:**
- Create: `app/Admin/src/features/shipping/shipping.routes.ts`

**Interfaces:**
- Produces: `export const shippingRoutes: RouteRecordRaw`

- [ ] **Step 1: Create the shipping routes file**

```typescript
import type { RouteRecordRaw } from 'vue-router'

export const shippingRoutes: RouteRecordRaw = {
  path: 'shipping',
  meta: { breadcrumb: 'Shipping' },
  children: [
    {
      path: '',
      name: 'shipping.methods.list',
      component: () => import('./shipping-methods/views/ShippingMethodList.View.vue'),
      meta: { breadcrumb: 'Shipping Methods' },
    },
    {
      path: 'methods/create',
      name: 'shipping.methods.create',
      component: () => import('./shipping-methods/views/ShippingMethodForm.View.vue'),
      meta: { breadcrumb: 'Add Method' },
    },
    {
      path: 'methods/:id/edit',
      name: 'shipping.methods.edit',
      component: () => import('./shipping-methods/views/ShippingMethodForm.View.vue'),
      meta: { breadcrumb: 'Edit Method' },
    },
    {
      path: 'rates',
      name: 'shipping.rates.list',
      component: () => import('./shipping-rates/views/ShippingRateList.View.vue'),
      meta: { breadcrumb: 'Shipping Rates' },
    },
    {
      path: 'rates/create',
      name: 'shipping.rates.create',
      component: () => import('./shipping-rates/views/ShippingRateForm.View.vue'),
      meta: { breadcrumb: 'Add Rate' },
    },
    {
      path: 'rates/:id/edit',
      name: 'shipping.rates.edit',
      component: () => import('./shipping-rates/views/ShippingRateForm.View.vue'),
      meta: { breadcrumb: 'Edit Rate' },
    },
  ],
}
```

- [ ] **Step 2: Verify TypeScript compiles**

```bash
cd app/Admin && pnpm run type-check 2>&1 | grep "error TS" | wc -l
```

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/shipping/shipping.routes.ts
git commit -m "feat(admin): add shipping routes (methods, rates CRUD)"
```

---

### Task 3: Create location.routes.ts and addresses.routes.ts

**Files:**
- Create: `app/Admin/src/features/location/location.routes.ts`
- Create: `app/Admin/src/features/profile/addresses/addresses.routes.ts`

**Interfaces:**
- Produces: `export const locationRoutes: RouteRecordRaw`
- Produces: `export const addressesRoutes: RouteRecordRaw`

- [ ] **Step 1: Create the location routes file**

```typescript
import type { RouteRecordRaw } from 'vue-router'

export const locationRoutes: RouteRecordRaw = {
  path: 'locations',
  meta: { breadcrumb: 'Locations' },
  children: [
    {
      path: 'countries',
      name: 'location.countries.list',
      component: () => import('../location/countries/views/CountryList.View.vue'),
      meta: { breadcrumb: 'Countries' },
    },
    {
      path: 'states',
      name: 'location.states.list',
      component: () => import('../location/states/views/StateList.View.vue'),
      meta: { breadcrumb: 'States' },
    },
  ],
}
```

- [ ] **Step 2: Create the addresses routes file**

```typescript
import type { RouteRecordRaw } from 'vue-router'

export const addressesRoutes: RouteRecordRaw = {
  path: 'addresses',
  name: 'addresses',
  component: () => import('./views/AddressList.View.vue'),
  meta: { breadcrumb: 'Addresses' },
}
```

- [ ] **Step 3: Verify TypeScript compiles**

```bash
cd app/Admin && pnpm run type-check 2>&1 | grep "error TS" | wc -l
```

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/location/location.routes.ts app/Admin/src/features/profile/addresses/addresses.routes.ts
git commit -m "feat(admin): add location and addresses routes"
```

---

### Task 4: Register routes in router and add menu entries

**Files:**
- Modify: `app/Admin/src/app/router/index.ts`
- Modify: `app/Admin/src/app/layout/Menu.Layout.vue`

**Interfaces:**
- Consumes: `paymentRoutes`, `shippingRoutes`, `locationRoutes`, `addressesRoutes` from Tasks 1-3
- Produces: updated `const router` with 4 new route children; updated `const model` with 4 new menu sections

- [ ] **Step 1: Add imports and register routes in router/index.ts**

Open `app/Admin/src/app/router/index.ts` (38 lines). Add 4 import lines after line 11:

```typescript
import { paymentRoutes } from '@/features/payment/payment.routes'
import { shippingRoutes } from '@/features/shipping/shipping.routes'
import { locationRoutes } from '@/features/location/location.routes'
import { addressesRoutes } from '@/features/profile/addresses/addresses.routes'
```

Then add the routes inside the `children` array of the `AppLayout` route (inside the `{ path: '/', component: AppLayout, ... children: [...] }` block). Insert after `orderingRoutes` on line 27:

```typescript
      paymentRoutes,
      shippingRoutes,
      locationRoutes,
      addressesRoutes,
```

Final routes array order inside AppLayout children should be: profile, catalogRoutes, reportsRoutes, inventoryRoutes, orderingRoutes, paymentRoutes, shippingRoutes, locationRoutes, addressesRoutes, usersRoutes.

- [ ] **Step 2: Add menu entries in Menu.Layout.vue**

Open `app/Admin/src/app/layout/Menu.Layout.vue` (81 lines). Insert these 3 new sections between existing sections. The new menu order will be:

```
Home / Catalog / Inventory → LOCATIONS → Sales → PAYMENTS → SHIPPING → Identity
```

Insert **Locations** section after the Inventory section (after line 53 `],`):

```typescript
  {
    label: 'Locations',
    items: [
      { label: 'Countries', icon: 'pi pi-fw pi-globe', to: { name: 'location.countries.list' } },
      { label: 'States', icon: 'pi pi-fw pi-map', to: { name: 'location.states.list' } },
    ],
  },
```

Insert **Payments** section after Sales section (after line 61 `],` — the Sales closing bracket):

```typescript
  {
    label: 'Payments',
    items: [
      { label: 'Payments', icon: 'pi pi-fw pi-wallet', to: { name: 'payment.payments.list' } },
      { label: 'Methods', icon: 'pi pi-fw pi-credit-card', to: { name: 'payment.methods.list' } },
    ],
  },
```

Insert **Shipping** section after the new Payments section:

```typescript
  {
    label: 'Shipping',
    items: [
      { label: 'Methods', icon: 'pi pi-fw pi-truck', to: { name: 'shipping.methods.list' } },
      { label: 'Rates', icon: 'pi pi-fw pi-tag', to: { name: 'shipping.rates.list' } },
    ],
  },
```

Add **Addresses** item to the Identity & Access section. Inside the `items` array for the `'Identity & Access'` section, append before the closing `]`:

```typescript
      { label: 'Addresses', icon: 'pi pi-fw pi-address-book', to: { name: 'addresses' } },
```

- [ ] **Step 3: Verify TypeScript compiles**

Remaining TS errors should still be 7 (pre-existing):

```bash
cd app/Admin && pnpm run type-check 2>&1 | grep "error TS" | wc -l
```

Expected output: `7` (or fewer — none of these changes introduce new errors)

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/app/router/index.ts app/Admin/src/app/layout/Menu.Layout.vue
git commit -m "feat(admin): register payment/shipping/location/addresses routes and menu entries"
```

---

### Task 5: Delete dead identity API layer

**Files:**
- Delete: `app/Admin/src/features/identity/api/identity.api.ts`
- Delete: `app/Admin/src/features/identity/api/__tests__/identity.api.spec.ts` (if exists)

**Interfaces:**
- None — this code has zero consumers except its own spec file.

- [ ] **Step 1: Verify no consumers of identity api exist**

```bash
cd app/Admin && rg "from.*identity/api/identity" --include="*.ts" --include="*.vue" src/
```

Expected: only matches in `identity.api.spec.ts` (if that file exists), or no matches at all.

- [ ] **Step 2: Delete the dead files**

```bash
rm -f app/Admin/src/features/identity/api/identity.api.ts
rm -f app/Admin/src/features/identity/api/__tests__/identity.api.spec.ts
```

- [ ] **Step 3: Verify TypeScript compiles**

```bash
cd app/Admin && pnpm run type-check 2>&1 | grep "error TS" | wc -l
```

Expected: `7` (pre-existing errors, no new errors from deletions)

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/identity/api/identity.api.ts app/Admin/src/features/identity/api/__tests__/identity.api.spec.ts
git commit -m "chore(admin): delete unused identity.api.ts (duplicate of user.api.ts)"
```

---

### Task 6: Add catalog assign/revoke API functions

**Files:**
- Modify: `app/Admin/src/features/catalog/products/option-types/api/product-option-type.api.ts`
- Modify: `app/Admin/src/features/catalog/products/classifications/api/product-classification.api.ts`
- Modify: `app/Admin/src/features/catalog/products/variants/api/variant.api.ts`

**Interfaces:**
- Consumes: existing `apiClient`, `CATALOG` constant
- Produces: `productOptionTypeApi.assignOptionTypes()`, `productOptionTypeApi.revokeOptionTypes()`, `productClassificationApi.assignClassifications()`, `productClassificationApi.revokeClassifications()`, `variantRepository.listVariantOptionValues()`

- [ ] **Step 1: Add assign/revoke to product-option-type.api.ts**

Open `app/Admin/src/features/catalog/products/option-types/api/product-option-type.api.ts` (12 lines). Append these two functions before the closing `}`:

```typescript
  assignOptionTypes: (productId: string, optionTypeIds: string[]): Promise<ServerResult<void>> =>
    apiClient.post(`${CATALOG}/products/${productId}/option-types/assign`, { optionTypeIds }).then(res => res.data as ServerResult<void>),

  revokeOptionTypes: (productId: string, optionTypeIds: string[]): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/products/${productId}/option-types/revoke`, { data: { optionTypeIds } }).then(res => res.data as ServerResult<void>),
```

- [ ] **Step 2: Add assign/revoke to product-classification.api.ts**

Open `app/Admin/src/features/catalog/products/classifications/api/product-classification.api.ts` (13 lines). Append these two functions before the closing `}`:

```typescript
  assignClassifications: (productId: string, taxonIds: string[]): Promise<ServerResult<void>> =>
    apiClient.post(`${CATALOG}/products/${productId}/classifications/assign`, { taxonIds }).then(res => res.data as ServerResult<void>),

  revokeClassifications: (productId: string, taxonIds: string[]): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/products/${productId}/classifications/revoke`, { data: { taxonIds } }).then(res => res.data as ServerResult<void>),
```

- [ ] **Step 3: Add listVariantOptionValues to variant.api.ts**

Open `app/Admin/src/features/catalog/products/variants/api/variant.api.ts` (36 lines). Append before the closing `}`:

```typescript
  listVariantOptionValues: (variantId: string): Promise<ServerResult<string[]>> =>
    apiClient.get(`${CATALOG}/variants/${variantId}/option-values`).then(res => res.data as ServerResult<string[]>),
```

- [ ] **Step 4: Verify TypeScript compiles**

```bash
cd app/Admin && pnpm run type-check 2>&1 | grep "error TS" | wc -l
```

Expected: `7` (pre-existing)

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/catalog/products/option-types/api/product-option-type.api.ts app/Admin/src/features/catalog/products/classifications/api/product-classification.api.ts app/Admin/src/features/catalog/products/variants/api/variant.api.ts
git commit -m "feat(admin): add catalog assign/revoke API functions (option-types, classifications, variant option-values)"
```

---

### Task 7: Add variant image API functions (getById, download, embeddings)

**Files:**
- Modify: `app/Admin/src/features/catalog/products/variants/images/api/image.api.ts`

**Interfaces:**
- Consumes: existing `apiClient`, `CATALOG` constant
- Produces: `imageApi.getById()`, `imageApi.download()`, `imageApi.generateEmbedding()`

- [ ] **Step 1: Add 3 new functions to image.api.ts**

Open `app/Admin/src/features/catalog/products/variants/images/api/image.api.ts` (32 lines). Append these three functions before the closing `};`:

```typescript
  getById: (imageId: string): Promise<ServerResult<VariantImage>> =>
    apiClient
      .get(`${CATALOG}/variants/images/${imageId}`)
      .then((res) => res.data as ServerResult<VariantImage>),

  download: (imageId: string): Promise<Blob> =>
    apiClient
      .get(`${CATALOG}/variants/images/${imageId}/download`, { responseType: 'blob' })
      .then((res) => res.data as Blob),

  generateEmbedding: (imageId: string): Promise<ServerResult<void>> =>
    apiClient
      .post(`${CATALOG}/variants/images/${imageId}/embeddings`)
      .then((res) => res.data as ServerResult<void>),
```

- [ ] **Step 2: Verify TypeScript compiles**

```bash
cd app/Admin && pnpm run type-check 2>&1 | grep "error TS" | wc -l
```

Expected: `7` (pre-existing)

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/catalog/products/variants/images/api/image.api.ts
git commit -m "feat(admin): add variant image API functions (getById, download, embeddings)"
```

---

### Task 8: Add stock items import API function

**Files:**
- Modify: `app/Admin/src/features/inventories/stock-items/api/stock.api.ts`

**Interfaces:**
- Consumes: existing `apiClient`, `INVENTORY` constant, `path()` helper
- Produces: `stockRepository.importStockItems(file: File)`

- [ ] **Step 1: Add importStockItems to stock.api.ts**

Open `app/Admin/src/features/inventories/stock-items/api/stock.api.ts` (54 lines). Append this function before the closing `}`:

```typescript
  importStockItems(file: File): Promise<ServerResult<void>> {
    const formData = new FormData()
    formData.append('file', file)
    return apiClient
      .post(path('import'), formData, { headers: { 'Content-Type': 'multipart/form-data' } })
      .then(res => res.data as ServerResult<void>)
  },
```

- [ ] **Step 2: Verify TypeScript compiles**

```bash
cd app/Admin && pnpm run type-check 2>&1 | grep "error TS" | wc -l
```

Expected: `7` (pre-existing)

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/inventories/stock-items/api/stock.api.ts
git commit -m "feat(admin): add stock items import API function"
```

---

### Task 9: Wire user service stubs (assignRole, revokeRole, assignPermission)

**Files:**
- Modify: `app/Admin/src/features/users/services/user.service.ts`
- Check: `app/Admin/src/features/users/api/user.api.ts` (to confirm repository has these functions)

**Interfaces:**
- Consumes: `userRepository` from `../api/user.api`
- Produces: `userService.assignRole()`, `userService.revokeRole()`, `userService.assignPermission()` — all wired to real endpoints

- [ ] **Step 1: Check user.api.ts for assignRole/revokeRole/assignPermission/syncPermissions**

Read `app/Admin/src/features/users/api/user.api.ts` to verify the repository already exports `assignRole`, `revokeRole`, `assignPermission`, and `syncPermissions`. These endpoints exist in C# at `POST/DELETE /identity/users/{id}/roles/assign`, `POST/DELETE /identity/users/{id}/permissions/assign`. If they exist in the repository, proceed to step 2. If not, add them following the existing pattern.

- [ ] **Step 2: Update user.service.ts to wire real endpoints**

Open `app/Admin/src/features/users/services/user.service.ts` (44 lines). Replace the current 4 stubs (lines 27-43, from `assignPermission` through `verifyAccount`) with:

```typescript
  assignRole: userRepository.assignRole,
  revokeRole: userRepository.revokeRole,
  assignPermission: userRepository.assignPermission,
  syncPermissions: userRepository.syncPermissions,
  resetPassword: async (_id: string, _data: { new_password: string }): Promise<ServerResult<void>> => {
    console.warn('resetPassword: no backend route exists.')
    return { isSuccess: false, statusCode: 501, errors: [{ code: 'not_implemented', message: 'Not implemented — no backend route', type: 0, metadata: null }], message: 'Not implemented — no backend route', metadata: null, value: undefined }
  },
  unlockAccount: async (_id: string): Promise<ServerResult<void>> => {
    console.warn('unlockAccount: no backend route exists.')
    return { isSuccess: false, statusCode: 501, errors: [{ code: 'not_implemented', message: 'Not implemented — no backend route', type: 0, metadata: null }], message: 'Not implemented — no backend route', metadata: null, value: undefined }
  },
  verifyAccount: async (_id: string, _data: { verifyEmail?: boolean; verifyPhone?: boolean }): Promise<ServerResult<void>> => {
    console.warn('verifyAccount: no backend route exists.')
    return { isSuccess: false, statusCode: 501, errors: [{ code: 'not_implemented', message: 'Not implemented — no backend route', type: 0, metadata: null }], message: 'Not implemented — no backend route', metadata: null, value: undefined }
  },
```

If `userRepository` does NOT have `assignRole`, `revokeRole`, `assignPermission`, or `syncPermissions`, add them to user.api.ts first (Step 1a):

```typescript
  assignRole: (userId: string, data: { roleName: string }): Promise<ServerResult<void>> =>
    apiClient.post(`${IDENTITY}/users/${userId}/roles/assign`, data).then(res => res.data as ServerResult<void>),

  revokeRole: (userId: string, data: { roleName: string }): Promise<ServerResult<void>> =>
    apiClient.post(`${IDENTITY}/users/${userId}/roles/revoke`, data).then(res => res.data as ServerResult<void>),

  assignPermission: (userId: string, data: { permissionName: string }): Promise<ServerResult<void>> =>
    apiClient.post(`${IDENTITY}/users/${userId}/permissions/assign`, data).then(res => res.data as ServerResult<void>),

  syncPermissions: (userId: string, data: { permissionNames: string[] }): Promise<ServerResult<void>> =>
    apiClient.put(`${IDENTITY}/users/${userId}/permissions/sync`, data).then(res => res.data as ServerResult<void>),
```

- [ ] **Step 3: Verify TypeScript compiles**

```bash
cd app/Admin && pnpm run type-check 2>&1 | grep "error TS" | wc -l
```

Expected: `7` (pre-existing)

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/users/services/user.service.ts app/Admin/src/features/users/api/user.api.ts
git commit -m "feat(admin): wire user role/permission assign/revoke stubs to real endpoints"
```

---

### Task 10: Remove dead product-level image stubs from product.service.ts

**Files:**
- Modify: `app/Admin/src/features/catalog/products/services/product.service.ts`

**Interfaces:**
- Consumes: nothing new
- Produces: `productService` without `getImages`, `uploadImage`, `deleteImage`, `updateImage`

- [ ] **Step 1: Verify no consumers of product-level image stubs**

```bash
cd app/Admin && rg "productService\.(getImages|uploadImage|deleteImage|updateImage)" --include="*.ts" --include="*.vue" src/
```

Expected: no matches (these stubs were never called from any store or view). If matches exist, these are not truly dead — skip this task and re-evaluate.

- [ ] **Step 2: Remove the 4 dead functions and the unused import**

Open `app/Admin/src/features/catalog/products/services/product.service.ts` (51 lines). Remove:
1. The import: `import type { ProductImage } from "../types/product-image.response.type";` (line 7)
2. The 4 stub functions: `getImages`, `uploadImage`, `deleteImage`, `updateImage` (lines 34-50)

The file should end at line 33 with `};` after `syncClassifications`.

- [ ] **Step 3: Also remove the unused type file if it has no other consumers**

```bash
cd app/Admin && rg "product-image.response.type" --include="*.ts" --include="*.vue" src/
```

If no matches remain, delete the type file:
```bash
rm app/Admin/src/features/catalog/products/types/product-image.response.type.ts
```

- [ ] **Step 4: Verify TypeScript compiles**

```bash
cd app/Admin && pnpm run type-check 2>&1 | grep "error TS" | wc -l
```

Expected: `7` (pre-existing)

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/catalog/products/services/product.service.ts
git commit -m "refactor(admin): remove dead product-level image stubs (backend has variant-level only)"
```

---

### Task 11: Convert AddressList.View.vue to standalone page

**Files:**
- Modify: `app/Admin/src/features/profile/addresses/views/AddressList.View.vue`
- Check: `app/Admin/src/features/profile/addresses/stores/address.store.ts`
- Check: `app/Admin/src/features/users/services/user.service.ts` (for `list` and `listCustomers` imports)

**Interfaces:**
- Consumes: `useAddressStore`, `userService` (for user picker)
- Produces: Standalone `AddressList.View.vue` — select a user, show their addresses

- [ ] **Step 1: Read the address store to understand the API**

Read `app/Admin/src/features/profile/addresses/stores/address.store.ts` and `app/Admin/src/features/profile/addresses/services/address.service.ts` to confirm:
- `fetchAll(userId: string)` method exists and calls the address API
- The store exports `items` and `loading` refs

- [ ] **Step 2: Read the user service for available list methods**

Confirm `userService.list` returns `ServerPagedResult<AdminUserSummaryModel>` and `userService.listCustomers` returns `ServerPagedResult<CustomerSummaryModel>`. The user picker will use both.

- [ ] **Step 3: Rewrite AddressList.View.vue as standalone page**

Replace the entire file (26 lines) with:

```vue
<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import { useAddressStore } from '../stores/address.store'
import { userService } from '@/features/users/services/user.service'
import { storeToRefs } from 'pinia'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Select from 'primevue/select'
import Message from 'primevue/message'
import PageShell from '@/shared/components/PageShell.Component.vue'
import PageHeader from '@/shared/components/PageHeader.Component.vue'
import { useI18n } from 'vue-i18n'

const store = useAddressStore()
const { items, loading } = storeToRefs(store)
const { t } = useI18n()

const selectedUserId = ref<string | null>(null)
const users = ref<Array<{ id: string; label: string }>>([])

onMounted(async () => {
  const [staffResult, customerResult] = await Promise.all([
    userService.list(),
    userService.listCustomers(),
  ])
  const staffOptions = (staffResult.items ?? []).map(u => ({ id: u.id, label: `${u.email ?? u.id}` }))
  const customerOptions = (customerResult.items ?? []).map(u => ({ id: u.id, label: `${u.email ?? u.id}` }))
  users.value = [...staffOptions, ...customerOptions]
})

watch(selectedUserId, async (userId) => {
  if (userId) {
    await store.fetchAll(userId)
  }
})
</script>

<template>
  <PageShell :card="false">
    <PageHeader :title="t('profile.titles.addresses')" />

    <div class="flex flex-col gap-4">
      <Select
        v-model="selectedUserId"
        :options="users"
        optionLabel="label"
        optionValue="id"
        :placeholder="t('profile.placeholders.select_user')"
        filter
        showClear
        class="w-full max-w-md"
      />

      <Message v-if="!selectedUserId" severity="info" :closable="false">
        {{ t('profile.messages.select_user_to_view_addresses') }}
      </Message>

      <DataTable v-if="selectedUserId" :value="items" :loading="loading" dataKey="id" class="mt-4">
        <Column field="address1" :header="t('profile.labels.address')" />
        <Column field="city" :header="t('profile.labels.city')" />
        <Column field="stateProvince" :header="t('profile.labels.state')" />
        <Column field="country" :header="t('profile.labels.country')" />
        <Column field="isDefault" :header="t('profile.labels.default')">
          <template #body="{ data }">
            <i :class="data.isDefault ? 'pi pi-check text-green-500' : 'pi pi-times text-red-500'" />
          </template>
        </Column>
      </DataTable>
    </div>
  </PageShell>
</template>
```

- [ ] **Step 4: Verify TypeScript compiles**

```bash
cd app/Admin && pnpm run type-check 2>&1 | grep "error TS" | wc -l
```

Expected: `7` (pre-existing). If new errors appear from missing i18n keys or incorrect store/service types, fix them now.

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/profile/addresses/views/AddressList.View.vue
git commit -m "feat(admin): convert AddressList to standalone page with user picker"
```

---

### Task 12: Create StockMovementList.View.vue

**Files:**
- Create: `app/Admin/src/features/inventories/stock-movements/views/StockMovementList.View.vue`
- Modify: `app/Admin/src/features/inventories/inventory.routes.ts` (add route)
- Modify: `app/Admin/src/app/layout/Menu.Layout.vue` (add menu item)

**Interfaces:**
- Consumes: `movementService` from `@/features/inventories/stock-movements/services/movement.service`
- Produces: Full-page DataTable with filters, route at `inventory/movements`, menu entry under Inventory

- [ ] **Step 1: Read movement service to confirm API shape**

Read `app/Admin/src/features/inventories/stock-movements/services/movement.service.ts`. Confirm `listMovements` returns `ServerPagedResult<...>` with pagination params. Note the model type used.

- [ ] **Step 2: Read movement store to confirm state**

Read `app/Admin/src/features/inventories/stock-movements/stores/movement.store.ts`. Confirm store shape: `items`, `loading`, `fetchMovements(params)`. If no store exists, use the service directly.

- [ ] **Step 3: Create StockMovementList.View.vue**

```vue
<script setup lang="ts">
import { onMounted } from 'vue'
import { movementService } from '../services/movement.service'
import { ref } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import PageShell from '@/shared/components/PageShell.Component.vue'
import PageHeader from '@/shared/components/PageHeader.Component.vue'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const movements = ref<any[]>([])
const loading = ref(false)
const totalCount = ref(0)
const page = ref(1)
const pageSize = ref(20)

async function fetchMovements() {
  loading.value = true
  const result = await movementService.listMovements({ page: page.value, pageSize: pageSize.value })
  if (result.isSuccess) {
    movements.value = result.items ?? []
    totalCount.value = result.totalCount ?? 0
  }
  loading.value = false
}

onMounted(() => fetchMovements())

function onPage(event: { first: number; rows: number }) {
  page.value = event.first / event.rows + 1
  pageSize.value = event.rows
  fetchMovements()
}
</script>

<template>
  <PageShell :card="false">
    <PageHeader :title="t('inventory.titles.stock_movements')" />
    <DataTable
      :value="movements"
      :loading="loading"
      :paginator="true"
      :rows="pageSize"
      :totalRecords="totalCount"
      lazy
      @page="onPage"
      dataKey="id"
    >
      <Column field="createdAt" :header="t('inventory.labels.date')">
        <template #body="{ data }">
          {{ new Date(data.createdAt).toLocaleDateString() }}
        </template>
      </Column>
      <Column field="movementType" :header="t('inventory.labels.type')" />
      <Column field="variantName" :header="t('inventory.labels.variant')" />
      <Column field="quantity" :header="t('inventory.labels.quantity')" />
      <Column field="locationName" :header="t('inventory.labels.location')" />
      <Column field="reference" :header="t('inventory.labels.reference')" />
    </DataTable>
  </PageShell>
</template>
```

Note: Column field names (`createdAt`, `movementType`, `variantName`, `quantity`, `locationName`, `reference`) assume the C# DTO field names (camelCase via interceptor). Adjust after reading the actual response type from `movement.service.ts`.

- [ ] **Step 4: Add route for stock movements**

Open `app/Admin/src/features/inventories/inventory.routes.ts` (70 lines). Add between the `units` route and `locations` route:

```typescript
    {
      path: 'movements',
      name: 'inventory.movements.list',
      component: () => import('./stock-movements/views/StockMovementList.View.vue'),
      meta: { breadcrumb: 'Stock Movements' },
    },
```

- [ ] **Step 5: Add menu item**

Open `app/Admin/src/app/layout/Menu.Layout.vue`. Inside the Inventory section's `items` array, add:

```typescript
      { label: 'Movements', icon: 'pi pi-fw pi-history', to: { name: 'inventory.movements.list' } },
```

- [ ] **Step 6: Verify TypeScript compiles**

```bash
cd app/Admin && pnpm run type-check 2>&1 | grep "error TS" | wc -l
```

Expected: `7` (pre-existing). Fix any type errors in the new view.

- [ ] **Step 7: Commit**

```bash
git add app/Admin/src/features/inventories/stock-movements/views/StockMovementList.View.vue app/Admin/src/features/inventories/inventory.routes.ts app/Admin/src/app/layout/Menu.Layout.vue
git commit -m "feat(admin): add stock movements list page"
```

---

### Task 13: Create StockImport.View.vue

**Files:**
- Create: `app/Admin/src/features/inventories/stock-items/views/StockImport.View.vue`
- Modify: `app/Admin/src/features/inventories/inventory.routes.ts` (add route)
- Modify: `app/Admin/src/app/layout/Menu.Layout.vue` (add menu item)

**Interfaces:**
- Consumes: `stockRepository.importStockItems()` from Task 7, `useApiErrorHandler`
- Produces: File upload page with preview and import button, route at `inventory/stocks/import`, menu entry under Inventory

- [ ] **Step 1: Create StockImport.View.vue**

```vue
<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { stockRepository } from '../api/stock.api'
import { useApiErrorHandler } from '@/shared/composables/api-error-handler.use'
import FileUpload from 'primevue/fileupload'
import Button from 'primevue/button'
import Message from 'primevue/message'
import PageShell from '@/shared/components/PageShell.Component.vue'
import PageHeader from '@/shared/components/PageHeader.Component.vue'
import { useI18n } from 'vue-i18n'

const router = useRouter()
const { t } = useI18n()
const { handleApiResult } = useApiErrorHandler()
const uploading = ref(false)
const selectedFile = ref<File | null>(null)

function onFileSelect(event: { files: File[] }) {
  selectedFile.value = event.files[0]
}

async function onImport() {
  if (!selectedFile.value) return
  uploading.value = true
  const result = await stockRepository.importStockItems(selectedFile.value)
  handleApiResult(result)
  if (result.isSuccess) {
    router.push({ name: 'inventory.stocks.list' })
  }
  uploading.value = false
}
</script>

<template>
  <PageShell :card="false">
    <PageHeader :title="t('inventory.titles.import_stock')" back />

    <div class="flex flex-col gap-6 max-w-2xl">
      <Message severity="info" :closable="false">
        {{ t('inventory.messages.import_stock_csv_info') }}
      </Message>

      <FileUpload
        mode="basic"
        name="file"
        accept=".csv"
        :maxFileSize="10000000"
        @select="onFileSelect"
        :chooseLabel="t('inventory.actions.choose_file')"
        customUpload
        auto
      />

      <div v-if="selectedFile" class="flex items-center gap-4">
        <span class="text-surface-600">{{ t('inventory.labels.selected_file') }}: {{ selectedFile.name }}</span>
        <Button
          :label="t('inventory.actions.import')"
          icon="pi pi-upload"
          :loading="uploading"
          @click="onImport"
        />
      </div>
    </div>
  </PageShell>
</template>
```

- [ ] **Step 2: Add route**

Open `app/Admin/src/features/inventories/inventory.routes.ts`. Add after the `stocks` route (after line 18 `}`):

```typescript
    {
      path: 'stocks/import',
      name: 'inventory.stocks.import',
      component: () => import('./stock-items/views/StockImport.View.vue'),
      meta: { breadcrumb: 'Import Stock' },
    },
```

- [ ] **Step 3: Add menu item**

Open `app/Admin/src/app/layout/Menu.Layout.vue`. Inside the Inventory section's `items` array, add:

```typescript
      { label: 'Import', icon: 'pi pi-fw pi-file-import', to: { name: 'inventory.stocks.import' } },
```

- [ ] **Step 4: Verify TypeScript compiles**

```bash
cd app/Admin && pnpm run type-check 2>&1 | grep "error TS" | wc -l
```

Expected: `7` (pre-existing)

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/inventories/stock-items/views/StockImport.View.vue app/Admin/src/features/inventories/inventory.routes.ts app/Admin/src/app/layout/Menu.Layout.vue
git commit -m "feat(admin): add stock items CSV import page"
```

---

### Task 14: Add resume button and line-item CRUD to OrderDetail.View.vue

**Files:**
- Modify: `app/Admin/src/features/ordering/orders/views/OrderDetail.View.vue`
- Check: `app/Admin/src/features/ordering/orders/stores/order.store.ts` (confirm `resumeOrder` exists and `updateLineItem`/`removeLineItem` exist)

**Interfaces:**
- Consumes: `useOrderStore` — `resumeOrder(id)`, `updateLineItem(orderId, lineItemId, data)`, `removeLineItem(orderId, lineItemId)`, `fetchOrderById(id)` (refresh after mutation)
- Produces: Updated OrderDetail with resume button and inline line-item edit/delete

- [ ] **Step 1: Read the order store to confirm available methods**

Read `app/Admin/src/features/ordering/orders/stores/order.store.ts`. Confirm:
- `resumeOrder(orderId: string)` method exists and calls `orderService.resume`
- `current_order` has `lineItems?: Array<...>` with `id`, `quantity`, `productName`, `unitPrice` fields
- Line-item update/delete methods exist, or add them now in the store

If `resumeOrder` doesn't exist in the store, add it:
```typescript
async function resumeOrder(orderId: string) {
  submitting.value = true
  const result = await orderService.resume(orderId)
  if (result.isSuccess) {
    await fetchOrderById(orderId)
  }
  submitting.value = false
  return result
}
```

If `updateLineItem` and `removeLineItem` don't exist, add them similarly.

- [ ] **Step 2: Modify OrderDetail.View.vue — add resume button**

Open `app/Admin/src/features/ordering/orders/views/OrderDetail.View.vue` (239 lines). After line 74 (`};` closing `onCancel`), add the resume handler:

```typescript
const onResume = async () => {
  const result = await store.resumeOrder(orderId)
  handleApiResult(result)
}
```

In the template, add the resume button after the cancel button (after line 110 `/>`):

```html
                <Button
                    :label="t('ordering.actions.resume_order')"
                    icon="pi pi-undo"
                    severity="warn"
                    outlined
                    @click="onResume"
                    v-if="current_order.status === 2"
                    class="rounded-xl px-6"
                />
```

Note: The `v-if` on the advance/cancel buttons (`current_order.status !== 1 && current_order.status !== 2`) should be updated so the resume button only shows when status is Cancelled (status=2). The advance/cancel buttons already hide themselves for status 1 (Completed) and 2 (Cancelled).

- [ ] **Step 3: Replace the placeholder line-items section with real table**

Replace lines 137-157 (the placeholder `<div class="text-center py-8...">` through the totals section) with a real line-items DataTable. The replacement should show editable quantity fields with save/cancel per row and delete button per row. Also keep the totals section below it.

The template block starting at line 138 should become:

```html
                        <DataTable :value="current_order.lineItems" v-if="current_order.lineItems?.length">
                            <Column field="productName" :header="t('ordering.labels.product')" />
                            <Column field="sku" :header="t('ordering.labels.sku')" />
                            <Column :header="t('ordering.labels.quantity')">
                                <template #body="{ data: item, index }">
                                    <InputNumber
                                        v-model="item.quantity"
                                        :min="1"
                                        :showButtons="true"
                                        @update:modelValue="(val: number) => store.updateLineItem(orderId, item.id, { quantity: val })"
                                        :disabled="current_order.status === 1 || current_order.status === 2"
                                    />
                                </template>
                            </Column>
                            <Column field="unitPrice" :header="t('ordering.labels.unit_price')" />
                            <Column field="total" :header="t('ordering.labels.subtotal')" />
                            <Column :header="t('ordering.labels.actions')">
                                <template #body="{ data: item }">
                                    <Button
                                        icon="pi pi-trash"
                                        severity="danger"
                                        text
                                        rounded
                                        @click="store.removeLineItem(orderId, item.id).then(handleApiResult)"
                                        :disabled="current_order.status === 1 || current_order.status === 2"
                                    />
                                </template>
                            </Column>
                        </DataTable>
                        <div v-else class="text-center py-8 text-surface-400 italic">
                            {{ t('ordering.messages.no_line_items') }}
                        </div>
```

Note: Import `InputNumber` from `primevue/inputnumber` and `DataTable` from `primevue/datatable` if not already imported. Import `Column` from `primevue/column`. Check existing imports and add missing ones.

- [ ] **Step 4: Verify TypeScript compiles**

```bash
cd app/Admin && pnpm run type-check 2>&1 | grep "error TS" | wc -l
```

Expected: `7` (pre-existing). Fix any type errors from the new components/imports.

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/ordering/orders/views/OrderDetail.View.vue app/Admin/src/features/ordering/orders/stores/order.store.ts
git commit -m "feat(admin): add resume button and line-item CRUD to OrderDetail"
```

---

### Task 15: Final verification

**Files:**
- None — verification only

- [ ] **Step 1: Run full type check**

```bash
cd app/Admin && pnpm run type-check 2>&1 | grep "error TS" | wc -l
```

Expected: `7` (all pre-existing errors from PrimeVue TreeNode imports + option-value store). No new errors introduced.

- [ ] **Step 2: Run all tests**

```bash
cd app/Admin && npx vitest run 2>&1 | grep -E "Test Files|Tests"
```

Expected: All passing (no test regressions).

- [ ] **Step 3: Run lint**

```bash
cd app/Admin && pnpm run lint 2>&1 | tail -5
```

Expected: No new lint errors.

- [ ] **Step 4: Final commit if any remaining changes**

```bash
git status
git add -A
git commit -m "chore(admin): final verification after gap coverage — all TS errors pre-existing, all tests pass"
```
