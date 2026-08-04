# Admin Feature Routes & Views — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Scaffold route definitions and placeholder views for all 11 admin feature modules, wire them into the main router, update the sidebar menu, and disable the auth guard for review.

**Architecture:** Each feature's `routes/index.ts` exports `{module}Routes: RouteRecordRaw[]` and `{module}MenuItems: MenuItem[]`. The main router (`app/router/routes.ts`) imports all feature route arrays and spreads them as children of `AdminLayout`. The sidebar (`AppMenu.vue`) imports all feature menu arrays and renders them. Views are lazy-loaded Vue SFCs from each feature's `views/` folder using `PageShell` wrapper.

**Tech Stack:** Vue 3, Vue Router, TypeScript, PrimeVue (icons only for menu)

## Global Constraints

- All routes are children of `AdminLayout` (except auth and 404)
- Auth guard is commented out with `// TODO: re-enable auth guard after route scaffold review`
- CRUD actions handled via dialogs/drawers in list/detail pages, not separate routes
- Sub-resources shown via tabs in detail pages
- Each placeholder view uses `<PageShell>` with a title prop
- Route names use kebab-case matching URL path segments
- Directory for admin features is always `Features/Admin/` on the C# side; `features/{module}/` on the TS side
- `TreatWarningsAsErrors=true` globally — any TypeScript warning fails build
- Follow existing FSD-inspired directory structure: no cross-feature imports except via shared/ layer

---

### Task 1: Disable auth guard

**Files:**
- Modify: `app/Admin/src/app/router/guards.ts:12-18`

**Interfaces:**
- Produces: Commented-out `router.beforeEach` block leaving only the `afterEach` title guard active

- [ ] **Step 1: Comment out the beforeEach auth guard**

In `app/Admin/src/app/router/guards.ts`, comment out the `beforeEach` block:

```ts
export function setupGuards(router: Router): void {
  // TODO: re-enable auth guard after route scaffold review
  // router.beforeEach((to, _from, next) => {
  //   if (to.meta.requiresAuth && !getAccessToken()) {
  //     return next({ name: 'login', query: { redirect: to.fullPath } })
  //   }
  //   next()
  // })

  router.afterEach((to) => {
    if (to.meta.title) {
      document.title = `${to.meta.title} | ReSys.Shop`
    }
  })
}
```

- [ ] **Step 2: Verify build and tests pass**

Run:
```bash
cd app/Admin && pnpm run build 2>&1 && pnpm run test:unit -- run 2>&1 | tail -5
```
Expected: build succeeds, 307 tests pass.

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/app/router/guards.ts
git commit -m "chore(admin): disable auth guard for route scaffold review"
```

---

### Task 2: Dashboard routes and view

**Files:**
- Create: `app/Admin/src/features/dashboard/routes/index.ts`
- Create: `app/Admin/src/features/dashboard/views/DashboardPage.vue`

**Interfaces:**
- Produces: `dashboardRoutes: Array<{ path: string; name: string; component: () => Promise<typeof import('*.vue')>; meta: object }>`
- Produces: `dashboardMenuItems: Array<{ label: string; to: string }>`

- [ ] **Step 1: Create DashboardPage.vue**

`app/Admin/src/features/dashboard/views/DashboardPage.vue`:
```vue
<script setup lang="ts">
import PageShell from '@ui/PageShell.vue'
</script>

<template>
  <PageShell title="Dashboard">
    <p class="text-muted-color">Dashboard content coming soon.</p>
  </PageShell>
</template>
```

- [ ] **Step 2: Create dashboard routes/index.ts**

`app/Admin/src/features/dashboard/routes/index.ts`:
```ts
import type { RouteRecordRaw } from 'vue-router'

const DashboardPage = () => import('../views/DashboardPage.vue')

export const dashboardRoutes: RouteRecordRaw[] = [
  {
    path: '',
    name: 'dashboard',
    component: DashboardPage,
    meta: { title: 'Dashboard' },
  },
]

export const dashboardMenuItems = [
  {
    label: 'Dashboard',
    icon: 'pi pi-fw pi-chart-bar',
    to: '/',
  },
]
```

- [ ] **Step 3: Verify build and tests pass**

Run `cd app/Admin && pnpm run build 2>&1`.

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/dashboard/
git commit -m "feat(admin): scaffold dashboard routes and placeholder view"
```

---

### Task 3: Catalog routes and views

**Files:**
- Create: `app/Admin/src/features/catalog/routes/index.ts`
- Create: `app/Admin/src/features/catalog/views/ProductsList.vue`
- Create: `app/Admin/src/features/catalog/views/ProductDetail.vue`
- Create: `app/Admin/src/features/catalog/views/TaxonomiesList.vue`
- Create: `app/Admin/src/features/catalog/views/TaxonomyDetail.vue`
- Create: `app/Admin/src/features/catalog/views/OptionTypesList.vue`
- Create: `app/Admin/src/features/catalog/views/OptionTypeDetail.vue`

**Interfaces:**
- Consumes: `PageShell` from `@ui/PageShell.vue`
- Produces: `catalogRoutes: RouteRecordRaw[]` with paths `/catalog/products`, `/catalog/products/:id`, `/catalog/taxonomies`, `/catalog/taxonomies/:id`, `/catalog/option-types`, `/catalog/option-types/:id`, plus redirect
- Produces: `catalogMenuItems: Array<{ label: string; icon: string; items: MenuItem[] }>`

- [ ] **Step 1: Create all 6 placeholder view files**

Each follows the same pattern. Example for ProductsList.vue:
```vue
<script setup lang="ts">
import PageShell from '@ui/PageShell.vue'
</script>

<template>
  <PageShell title="Products">
    <p class="text-muted-color">Products list coming soon.</p>
  </PageShell>
</template>
```

Create all 6 files:
- `ProductsList.vue` — title "Products"
- `ProductDetail.vue` — title "Product Detail"
- `TaxonomiesList.vue` — title "Taxonomies"
- `TaxonomyDetail.vue` — title "Taxonomy Detail"
- `OptionTypesList.vue` — title "Option Types"
- `OptionTypeDetail.vue` — title "Option Type Detail"

- [ ] **Step 2: Create catalog routes/index.ts**

```ts
import type { RouteRecordRaw } from 'vue-router'

const ProductsList = () => import('../views/ProductsList.vue')
const ProductDetail = () => import('../views/ProductDetail.vue')
const TaxonomiesList = () => import('../views/TaxonomiesList.vue')
const TaxonomyDetail = () => import('../views/TaxonomyDetail.vue')
const OptionTypesList = () => import('../views/OptionTypesList.vue')
const OptionTypeDetail = () => import('../views/OptionTypeDetail.vue')

export const catalogRoutes: RouteRecordRaw[] = [
  {
    path: 'catalog',
    redirect: { name: 'catalog-products' },
  },
  {
    path: 'catalog/products',
    name: 'catalog-products',
    component: ProductsList,
    meta: { title: 'Products' },
  },
  {
    path: 'catalog/products/:id',
    name: 'catalog-product-detail',
    component: ProductDetail,
    meta: { title: 'Product Detail' },
  },
  {
    path: 'catalog/taxonomies',
    name: 'catalog-taxonomies',
    component: TaxonomiesList,
    meta: { title: 'Taxonomies' },
  },
  {
    path: 'catalog/taxonomies/:id',
    name: 'catalog-taxonomy-detail',
    component: TaxonomyDetail,
    meta: { title: 'Taxonomy Detail' },
  },
  {
    path: 'catalog/option-types',
    name: 'catalog-option-types',
    component: OptionTypesList,
    meta: { title: 'Option Types' },
  },
  {
    path: 'catalog/option-types/:id',
    name: 'catalog-option-type-detail',
    component: OptionTypeDetail,
    meta: { title: 'Option Type Detail' },
  },
]

export const catalogMenuItems = [
  {
    label: 'Catalog',
    icon: 'pi pi-fw pi-box',
    items: [
      { label: 'Products', icon: 'pi pi-fw pi-tag', to: '/catalog/products' },
      { label: 'Taxonomies', icon: 'pi pi-fw pi-sitemap', to: '/catalog/taxonomies' },
      { label: 'Option Types', icon: 'pi pi-fw pi-sliders-h', to: '/catalog/option-types' },
    ],
  },
]
```

- [ ] **Step 3: Update catalog views/index.ts barrel**

`app/Admin/src/features/catalog/views/index.ts`:
```ts
// Barrel exports for catalog/views
export { default as ProductsList } from './ProductsList.vue'
export { default as ProductDetail } from './ProductDetail.vue'
export { default as TaxonomiesList } from './TaxonomiesList.vue'
export { default as TaxonomyDetail } from './TaxonomyDetail.vue'
export { default as OptionTypesList } from './OptionTypesList.vue'
export { default as OptionTypeDetail } from './OptionTypeDetail.vue'
```

- [ ] **Step 4: Verify build passes**

Run `cd app/Admin && pnpm run build 2>&1`.

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/catalog/
git commit -m "feat(admin): scaffold catalog routes and placeholder views"
```

---

### Task 4: Identity routes and views

**Files:**
- Create: `app/Admin/src/features/identity/routes/index.ts`
- Create: `app/Admin/src/features/identity/views/UsersList.vue`
- Create: `app/Admin/src/features/identity/views/UserDetail.vue`
- Create: `app/Admin/src/features/identity/views/RolesList.vue`
- Create: `app/Admin/src/features/identity/views/RoleDetail.vue`
- Create: `app/Admin/src/features/identity/views/PermissionsList.vue`
- Modify: `app/Admin/src/features/identity/views/index.ts`

- [ ] **Step 1: Create 5 placeholder view files**

Each same pattern as Task 3. Titles: "Users", "User Detail", "Roles", "Role Detail", "Permissions"

- [ ] **Step 2: Create identity routes/index.ts**

```ts
import type { RouteRecordRaw } from 'vue-router'

const UsersList = () => import('../views/UsersList.vue')
const UserDetail = () => import('../views/UserDetail.vue')
const RolesList = () => import('../views/RolesList.vue')
const RoleDetail = () => import('../views/RoleDetail.vue')
const PermissionsList = () => import('../views/PermissionsList.vue')

export const identityRoutes: RouteRecordRaw[] = [
  {
    path: 'identity',
    redirect: { name: 'identity-users' },
  },
  {
    path: 'identity/users',
    name: 'identity-users',
    component: UsersList,
    meta: { title: 'Users' },
  },
  {
    path: 'identity/users/:id',
    name: 'identity-user-detail',
    component: UserDetail,
    meta: { title: 'User Detail' },
  },
  {
    path: 'identity/roles',
    name: 'identity-roles',
    component: RolesList,
    meta: { title: 'Roles' },
  },
  {
    path: 'identity/roles/:id',
    name: 'identity-role-detail',
    component: RoleDetail,
    meta: { title: 'Role Detail' },
  },
  {
    path: 'identity/permissions',
    name: 'identity-permissions',
    component: PermissionsList,
    meta: { title: 'Permissions' },
  },
]

export const identityMenuItems = [
  {
    label: 'Identity',
    icon: 'pi pi-fw pi-users',
    items: [
      { label: 'Users', icon: 'pi pi-fw pi-user', to: '/identity/users' },
      { label: 'Roles', icon: 'pi pi-fw pi-shield', to: '/identity/roles' },
      { label: 'Permissions', icon: 'pi pi-fw pi-key', to: '/identity/permissions' },
    ],
  },
]
```

- [ ] **Step 3: Update identity views/index.ts barrel**

```ts
export { default as UsersList } from './UsersList.vue'
export { default as UserDetail } from './UserDetail.vue'
export { default as RolesList } from './RolesList.vue'
export { default as RoleDetail } from './RoleDetail.vue'
export { default as PermissionsList } from './PermissionsList.vue'
```

- [ ] **Step 4: Verify build passes**

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/identity/
git commit -m "feat(admin): scaffold identity routes and placeholder views"
```

---

### Task 5: Inventory routes and views

**Files:**
- Create: `app/Admin/src/features/inventory/routes/index.ts`
- Create: 8 view files in `app/Admin/src/features/inventory/views/`: StockItemsList, StockItemDetail, StockLocationsList, StockLocationDetail, StockReservationsList, StockTransfersList, StockTransferDetail, StockMovementsList
- Modify: `app/Admin/src/features/inventory/views/index.ts`

- [ ] **Step 1: Create 8 placeholder view files**

Titles: "Stock Items", "Stock Item Detail", "Stock Locations", "Stock Location Detail", "Stock Reservations", "Stock Transfers", "Stock Transfer Detail", "Stock Movements"

- [ ] **Step 2: Create inventory routes/index.ts**

```ts
import type { RouteRecordRaw } from 'vue-router'

const StockItemsList = () => import('../views/StockItemsList.vue')
const StockItemDetail = () => import('../views/StockItemDetail.vue')
const StockLocationsList = () => import('../views/StockLocationsList.vue')
const StockLocationDetail = () => import('../views/StockLocationDetail.vue')
const StockReservationsList = () => import('../views/StockReservationsList.vue')
const StockTransfersList = () => import('../views/StockTransfersList.vue')
const StockTransferDetail = () => import('../views/StockTransferDetail.vue')
const StockMovementsList = () => import('../views/StockMovementsList.vue')

export const inventoryRoutes: RouteRecordRaw[] = [
  {
    path: 'inventory',
    redirect: { name: 'inventory-stock-items' },
  },
  {
    path: 'inventory/stock-items',
    name: 'inventory-stock-items',
    component: StockItemsList,
    meta: { title: 'Stock Items' },
  },
  {
    path: 'inventory/stock-items/:id',
    name: 'inventory-stock-item-detail',
    component: StockItemDetail,
    meta: { title: 'Stock Item Detail' },
  },
  {
    path: 'inventory/stock-locations',
    name: 'inventory-stock-locations',
    component: StockLocationsList,
    meta: { title: 'Stock Locations' },
  },
  {
    path: 'inventory/stock-locations/:id',
    name: 'inventory-stock-location-detail',
    component: StockLocationDetail,
    meta: { title: 'Stock Location Detail' },
  },
  {
    path: 'inventory/stock-reservations',
    name: 'inventory-stock-reservations',
    component: StockReservationsList,
    meta: { title: 'Stock Reservations' },
  },
  {
    path: 'inventory/stock-transfers',
    name: 'inventory-stock-transfers',
    component: StockTransfersList,
    meta: { title: 'Stock Transfers' },
  },
  {
    path: 'inventory/stock-transfers/:id',
    name: 'inventory-stock-transfer-detail',
    component: StockTransferDetail,
    meta: { title: 'Stock Transfer Detail' },
  },
  {
    path: 'inventory/stock-movements',
    name: 'inventory-stock-movements',
    component: StockMovementsList,
    meta: { title: 'Stock Movements' },
  },
]

export const inventoryMenuItems = [
  {
    label: 'Inventory',
    icon: 'pi pi-fw pi-warehouse',
    items: [
      { label: 'Stock Items', icon: 'pi pi-fw pi-box', to: '/inventory/stock-items' },
      { label: 'Locations', icon: 'pi pi-fw pi-map-marker', to: '/inventory/stock-locations' },
      { label: 'Reservations', icon: 'pi pi-fw pi-calendar', to: '/inventory/stock-reservations' },
      { label: 'Transfers', icon: 'pi pi-fw pi-arrows-h', to: '/inventory/stock-transfers' },
      { label: 'Movements', icon: 'pi pi-fw pi-history', to: '/inventory/stock-movements' },
    ],
  },
]
```

- [ ] **Step 3: Update inventory views/index.ts barrel**

```ts
export { default as StockItemsList } from './StockItemsList.vue'
export { default as StockItemDetail } from './StockItemDetail.vue'
export { default as StockLocationsList } from './StockLocationsList.vue'
export { default as StockLocationDetail } from './StockLocationDetail.vue'
export { default as StockReservationsList } from './StockReservationsList.vue'
export { default as StockTransfersList } from './StockTransfersList.vue'
export { default as StockTransferDetail } from './StockTransferDetail.vue'
export { default as StockMovementsList } from './StockMovementsList.vue'
```

- [ ] **Step 4: Verify build passes**

- [ ] **Step 5: Commit**

---

### Task 6: Location routes and views

**Files:**
- Create: `app/Admin/src/features/location/routes/index.ts`
- Create: 4 view files: CountriesList, CountryDetail, StatesList, StateDetail
- Modify: `app/Admin/src/features/location/views/index.ts`

- [ ] **Step 1: Create 4 placeholder view files**

Titles: "Countries", "Country Detail", "States", "State Detail"

- [ ] **Step 2: Create location routes/index.ts**

```ts
import type { RouteRecordRaw } from 'vue-router'

const CountriesList = () => import('../views/CountriesList.vue')
const CountryDetail = () => import('../views/CountryDetail.vue')
const StatesList = () => import('../views/StatesList.vue')
const StateDetail = () => import('../views/StateDetail.vue')

export const locationRoutes: RouteRecordRaw[] = [
  {
    path: 'location',
    redirect: { name: 'location-countries' },
  },
  {
    path: 'location/countries',
    name: 'location-countries',
    component: CountriesList,
    meta: { title: 'Countries' },
  },
  {
    path: 'location/countries/:id',
    name: 'location-country-detail',
    component: CountryDetail,
    meta: { title: 'Country Detail' },
  },
  {
    path: 'location/states',
    name: 'location-states',
    component: StatesList,
    meta: { title: 'States' },
  },
  {
    path: 'location/states/:id',
    name: 'location-state-detail',
    component: StateDetail,
    meta: { title: 'State Detail' },
  },
]

export const locationMenuItems = [
  {
    label: 'Location',
    icon: 'pi pi-fw pi-map',
    items: [
      { label: 'Countries', icon: 'pi pi-fw pi-globe', to: '/location/countries' },
      { label: 'States', icon: 'pi pi-fw pi-flag', to: '/location/states' },
    ],
  },
]
```

- [ ] **Step 3: Update location views/index.ts barrel**

```ts
export { default as CountriesList } from './CountriesList.vue'
export { default as CountryDetail } from './CountryDetail.vue'
export { default as StatesList } from './StatesList.vue'
export { default as StateDetail } from './StateDetail.vue'
```

- [ ] **Step 4: Verify build passes**

- [ ] **Step 5: Commit**

---

### Task 7: Ordering routes and views

**Files:**
- Create: `app/Admin/src/features/ordering/routes/index.ts`
- Create: 2 view files: OrdersList, OrderDetail
- Modify: `app/Admin/src/features/ordering/views/index.ts`

- [ ] **Step 1: Create 2 placeholder view files**

Titles: "Orders", "Order Detail"

- [ ] **Step 2: Create ordering routes/index.ts**

```ts
import type { RouteRecordRaw } from 'vue-router'

const OrdersList = () => import('../views/OrdersList.vue')
const OrderDetail = () => import('../views/OrderDetail.vue')

export const orderingRoutes: RouteRecordRaw[] = [
  {
    path: 'ordering',
    redirect: { name: 'ordering-orders' },
  },
  {
    path: 'ordering/orders',
    name: 'ordering-orders',
    component: OrdersList,
    meta: { title: 'Orders' },
  },
  {
    path: 'ordering/orders/:id',
    name: 'ordering-order-detail',
    component: OrderDetail,
    meta: { title: 'Order Detail' },
  },
]

export const orderingMenuItems = [
  {
    label: 'Ordering',
    icon: 'pi pi-fw pi-shopping-cart',
    items: [
      { label: 'Orders', icon: 'pi pi-fw pi-list', to: '/ordering/orders' },
    ],
  },
]
```

- [ ] **Step 3: Update ordering views/index.ts barrel**

```ts
export { default as OrdersList } from './OrdersList.vue'
export { default as OrderDetail } from './OrderDetail.vue'
```

- [ ] **Step 4: Verify build passes**

- [ ] **Step 5: Commit**

---

### Task 8: Payment routes and views

**Files:**
- Create: `app/Admin/src/features/payment/routes/index.ts`
- Create: 3 view files: PaymentsList, PaymentMethodsList, PaymentMethodDetail
- Modify: `app/Admin/src/features/payment/views/index.ts`

- [ ] **Step 1: Create 3 placeholder view files**

Titles: "Payments", "Payment Methods", "Payment Method Detail"

- [ ] **Step 2: Create payment routes/index.ts**

```ts
import type { RouteRecordRaw } from 'vue-router'

const PaymentsList = () => import('../views/PaymentsList.vue')
const PaymentMethodsList = () => import('../views/PaymentMethodsList.vue')
const PaymentMethodDetail = () => import('../views/PaymentMethodDetail.vue')

export const paymentRoutes: RouteRecordRaw[] = [
  {
    path: 'payment',
    redirect: { name: 'payment-payments' },
  },
  {
    path: 'payment/payments',
    name: 'payment-payments',
    component: PaymentsList,
    meta: { title: 'Payments' },
  },
  {
    path: 'payment/payment-methods',
    name: 'payment-methods',
    component: PaymentMethodsList,
    meta: { title: 'Payment Methods' },
  },
  {
    path: 'payment/payment-methods/:id',
    name: 'payment-method-detail',
    component: PaymentMethodDetail,
    meta: { title: 'Payment Method Detail' },
  },
]

export const paymentMenuItems = [
  {
    label: 'Payment',
    icon: 'pi pi-fw pi-credit-card',
    items: [
      { label: 'Payments', icon: 'pi pi-fw pi-dollar', to: '/payment/payments' },
      { label: 'Methods', icon: 'pi pi-fw pi-wallet', to: '/payment/payment-methods' },
    ],
  },
]
```

- [ ] **Step 3: Update payment views/index.ts barrel**

```ts
export { default as PaymentsList } from './PaymentsList.vue'
export { default as PaymentMethodsList } from './PaymentMethodsList.vue'
export { default as PaymentMethodDetail } from './PaymentMethodDetail.vue'
```

- [ ] **Step 4: Verify build passes**

- [ ] **Step 5: Commit**

---

### Task 9: Profile routes and views

**Files:**
- Create: `app/Admin/src/features/profile/routes/index.ts`
- Create: 4 view files: ProfilesList, ProfileDetail, AddressesList, AddressDetail
- Modify: `app/Admin/src/features/profile/views/index.ts`

- [ ] **Step 1: Create 4 placeholder view files**

Titles: "Profiles", "Profile Detail", "Addresses", "Address Detail"

- [ ] **Step 2: Create profile routes/index.ts**

```ts
import type { RouteRecordRaw } from 'vue-router'

const ProfilesList = () => import('../views/ProfilesList.vue')
const ProfileDetail = () => import('../views/ProfileDetail.vue')
const AddressesList = () => import('../views/AddressesList.vue')
const AddressDetail = () => import('../views/AddressDetail.vue')

export const profileRoutes: RouteRecordRaw[] = [
  {
    path: 'profile',
    redirect: { name: 'profile-profiles' },
  },
  {
    path: 'profile/profiles',
    name: 'profile-profiles',
    component: ProfilesList,
    meta: { title: 'Profiles' },
  },
  {
    path: 'profile/profiles/:id',
    name: 'profile-profile-detail',
    component: ProfileDetail,
    meta: { title: 'Profile Detail' },
  },
  {
    path: 'profile/addresses',
    name: 'profile-addresses',
    component: AddressesList,
    meta: { title: 'Addresses' },
  },
  {
    path: 'profile/addresses/:id',
    name: 'profile-address-detail',
    component: AddressDetail,
    meta: { title: 'Address Detail' },
  },
]

export const profileMenuItems = [
  {
    label: 'Profile',
    icon: 'pi pi-fw pi-id-card',
    items: [
      { label: 'Profiles', icon: 'pi pi-fw pi-user-edit', to: '/profile/profiles' },
      { label: 'Addresses', icon: 'pi pi-fw pi-map-marker', to: '/profile/addresses' },
    ],
  },
]
```

- [ ] **Step 3: Update profile views/index.ts barrel**

```ts
export { default as ProfilesList } from './ProfilesList.vue'
export { default as ProfileDetail } from './ProfileDetail.vue'
export { default as AddressesList } from './AddressesList.vue'
export { default as AddressDetail } from './AddressDetail.vue'
```

- [ ] **Step 4: Verify build passes**

- [ ] **Step 5: Commit**

---

### Task 10: Shipping routes and views

**Files:**
- Create: `app/Admin/src/features/shipping/routes/index.ts`
- Create: 4 view files: ShippingMethodsList, ShippingMethodDetail, ShippingRatesList, ShippingRateDetail
- Modify: `app/Admin/src/features/shipping/views/index.ts`

- [ ] **Step 1: Create 4 placeholder view files**

Titles: "Shipping Methods", "Shipping Method Detail", "Shipping Rates", "Shipping Rate Detail"

- [ ] **Step 2: Create shipping routes/index.ts**

```ts
import type { RouteRecordRaw } from 'vue-router'

const ShippingMethodsList = () => import('../views/ShippingMethodsList.vue')
const ShippingMethodDetail = () => import('../views/ShippingMethodDetail.vue')
const ShippingRatesList = () => import('../views/ShippingRatesList.vue')
const ShippingRateDetail = () => import('../views/ShippingRateDetail.vue')

export const shippingRoutes: RouteRecordRaw[] = [
  {
    path: 'shipping',
    redirect: { name: 'shipping-methods' },
  },
  {
    path: 'shipping/shipping-methods',
    name: 'shipping-methods',
    component: ShippingMethodsList,
    meta: { title: 'Shipping Methods' },
  },
  {
    path: 'shipping/shipping-methods/:id',
    name: 'shipping-method-detail',
    component: ShippingMethodDetail,
    meta: { title: 'Shipping Method Detail' },
  },
  {
    path: 'shipping/shipping-rates',
    name: 'shipping-rates',
    component: ShippingRatesList,
    meta: { title: 'Shipping Rates' },
  },
  {
    path: 'shipping/shipping-rates/:id',
    name: 'shipping-rate-detail',
    component: ShippingRateDetail,
    meta: { title: 'Shipping Rate Detail' },
  },
]

export const shippingMenuItems = [
  {
    label: 'Shipping',
    icon: 'pi pi-fw pi-truck',
    items: [
      { label: 'Methods', icon: 'pi pi-fw pi-cog', to: '/shipping/shipping-methods' },
      { label: 'Rates', icon: 'pi pi-fw pi-ticket', to: '/shipping/shipping-rates' },
    ],
  },
]
```

- [ ] **Step 3: Update shipping views/index.ts barrel**

```ts
export { default as ShippingMethodsList } from './ShippingMethodsList.vue'
export { default as ShippingMethodDetail } from './ShippingMethodDetail.vue'
export { default as ShippingRatesList } from './ShippingRatesList.vue'
export { default as ShippingRateDetail } from './ShippingRateDetail.vue'
```

- [ ] **Step 4: Verify build passes**

- [ ] **Step 5: Commit**

---

### Task 11: Auth routes and view

**Files:**
- Create: `app/Admin/src/features/auth/routes/index.ts`
- Create: `app/Admin/src/features/auth/views/LoginPage.vue`
- Modify: `app/Admin/src/features/auth/views/index.ts`

- [ ] **Step 1: Create LoginPage.vue**

```vue
<script setup lang="ts">
import PageShell from '@ui/PageShell.vue'
</script>

<template>
  <PageShell title="Sign In">
    <p class="text-muted-color">Login form coming soon.</p>
  </PageShell>
</template>
```

- [ ] **Step 2: Create auth routes/index.ts**

```ts
import type { RouteRecordRaw } from 'vue-router'

const LoginPage = () => import('../views/LoginPage.vue')

export const authRoutes: RouteRecordRaw[] = [
  {
    path: 'login',
    name: 'login',
    component: LoginPage,
    meta: { title: 'Sign In', requiresAuth: false },
  },
]
```

Note: Auth routes are standalone (outside AdminLayout). The main router will place them at the top level.

- [ ] **Step 3: Update auth views/index.ts barrel**

```ts
export { default as LoginPage } from './LoginPage.vue'
```

- [ ] **Step 4: Verify build passes**

- [ ] **Step 5: Commit**

---

### Task 12: Main router integration

**Files:**
- Modify: `app/Admin/src/app/router/routes.ts` (replace entire content)

**Interfaces:**
- Consumes: All feature route arrays from Tasks 2-11
- Produces: Consolidated `routes: RouteRecordRaw[]` export

- [ ] **Step 1: Rewrite routes.ts replacing the placeholder routes with feature route imports**

`app/Admin/src/app/router/routes.ts`:
```ts
import type { RouteRecordRaw } from 'vue-router'
import { AdminLayout, AuthLayout, ErrorLayout } from '@/app/layouts'
import { dashboardRoutes } from '@/features/dashboard/routes'
import { catalogRoutes } from '@/features/catalog/routes'
import { identityRoutes } from '@/features/identity/routes'
import { inventoryRoutes } from '@/features/inventory/routes'
import { locationRoutes } from '@/features/location/routes'
import { orderingRoutes } from '@/features/ordering/routes'
import { paymentRoutes } from '@/features/payment/routes'
import { profileRoutes } from '@/features/profile/routes'
import { shippingRoutes } from '@/features/shipping/routes'
import { authRoutes } from '@/features/auth/routes'

export const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: AdminLayout,
    meta: { requiresAuth: true },
    children: [
      ...dashboardRoutes,
      ...catalogRoutes,
      ...identityRoutes,
      ...inventoryRoutes,
      ...locationRoutes,
      ...orderingRoutes,
      ...paymentRoutes,
      ...profileRoutes,
      ...shippingRoutes,
    ],
  },
  {
    path: '/auth',
    component: AuthLayout,
    props: { title: 'Sign In', subtitle: 'Welcome to ReSys.Shop Admin' },
    children: authRoutes,
  },
  {
    path: '/:pathMatch(.*)*',
    component: ErrorLayout,
    meta: {
      statusCode: 404,
      title: 'Not Found',
      description: 'The page you are looking for does not exist.',
      icon: 'pi pi-search',
    },
  },
]
```

- [ ] **Step 2: Verify build passes**

```bash
cd app/Admin && pnpm run build 2>&1
```

- [ ] **Step 3: Verify tests pass**

```bash
cd app/Admin && pnpm run test:unit -- run 2>&1 | tail -5
```
Expected: 307 tests passing.

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/app/router/routes.ts
git commit -m "feat(admin): integrate feature routes into main router"
```

---

### Task 13: Sidebar menu integration

**Files:**
- Modify: `app/Admin/src/shared/components/navigation/AppMenu.vue:5-241` (replace the hardcoded `model` with feature menu imports)

**Interfaces:**
- Consumes: All feature menu item arrays from Tasks 2-11
- Produces: Updated `model` reactive ref with all admin feature menu items

- [ ] **Step 1: Replace AppMenu.vue menu model**

In `app/Admin/src/shared/components/navigation/AppMenu.vue`, replace the lines between the `interface MenuItem` definition and `</script>`:

```ts
import { dashboardMenuItems } from '@/features/dashboard/routes'
import { catalogMenuItems } from '@/features/catalog/routes'
import { identityMenuItems } from '@/features/identity/routes'
import { inventoryMenuItems } from '@/features/inventory/routes'
import { locationMenuItems } from '@/features/location/routes'
import { orderingMenuItems } from '@/features/ordering/routes'
import { paymentMenuItems } from '@/features/payment/routes'
import { profileMenuItems } from '@/features/profile/routes'
import { shippingMenuItems } from '@/features/shipping/routes'

const model = ref<MenuItem[]>([
  ...dashboardMenuItems,
  ...catalogMenuItems,
  ...identityMenuItems,
  ...inventoryMenuItems,
  ...locationMenuItems,
  ...orderingMenuItems,
  ...paymentMenuItems,
  ...profileMenuItems,
  ...shippingMenuItems,
])
```

Replace lines 17-241 (the entire hardcoded `model` array) with the above.

- [ ] **Step 2: Verify build passes**

```bash
cd app/Admin && pnpm run build 2>&1
```

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/shared/components/navigation/AppMenu.vue
git commit -m "feat(admin): integrate feature menu items into sidebar"
```

---

### Task 14: Final verification

**Files:**
- No new files. Verify everything works end-to-end.

- [ ] **Step 1: Full build**

```bash
cd app/Admin && pnpm run build 2>&1
```
Expected: zero errors.

- [ ] **Step 2: Full test suite**

```bash
cd app/Admin && pnpm run test:unit -- run 2>&1 | tail -5
```
Expected: 307 tests passing.

- [ ] **Step 3: Start dev server and verify navigation works**

```bash
cd app/Admin && pnpm run dev &
# Check that navigating to all routes in the browser works without errors
```

- [ ] **Step 4: Commit**

```bash
git commit -m "chore(admin): final verification of feature route scaffold"
```
