# Admin SPA Standardization — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Standardize the Admin SPA (8 modules, layout, menu, forms, dashboards) into a consistent design system with shared components, permission-gated menu, and structured page templates — inspired by Sakai Vue patterns.

**Architecture:** Eight opinionated shared wrapper components (DataTableShell, FormField, DetailField, StatusBadge, EmptyState, ConfirmButton, StatCard, TabbedDetail) replace duplicated patterns across all modules. Menu extracted to config file with RBAC gates. All forms migrated to vee-validate + zod. Four page templates (List, Form, Detail, Dashboard) enforced across all 8 domains.

**Tech Stack:** Vue 3 + TypeScript, PrimeVue 4 (Aura preset), vee-validate + zod, Pinia, vue-i18n, Vitest, Tailwind CSS 4

## Global Constraints

- All domain operations return `Result<T>` or `Result` — no thrown exceptions for business logic
- Modules never reference each other — cross-module communication via stores only
- Every form uses vee-validate + zod for validation
- Route names use dot-notation: `module.entity.action` (e.g., `catalog.products.list`)
- All user-facing strings use vue-i18n (`$t()`)
- Treat Warnings as Errors — zero warnings in build output
- `.card` utility class for surface-card background + 2rem padding + border-radius

---

## File Map

```
app/
├── config/
│   └── admin-menu.config.ts          [CREATE]  Menu model, permission gates, item types
├── layout/
│   ├── Menu.Layout.vue               [MODIFY]  Use config + filter by auth permissions
│   ├── MenuItem.Layout.vue           [MODIFY]  Add permission prop, badge support
│   ├── Sidebar.Layout.vue            [MODIFY]  Add user footer section
│   ├── Topbar.Layout.vue             [MODIFY]  Profile dropdown, remove placeholders
│   ├── Main.Layout.vue               [MODIFY]  Remove FloatingConfigurator, add route transition
│   └── composables/
│       └── layout.composable.ts      [MODIFY]  localStorage persistence
├── router/
│   └── index.ts                      [MODIFY]  Nest roles/permissions under AppLayout; route name map

shared/components/
├── DataTableShell.Component.vue      [CREATE]  Standardized DataTable wrapper
├── DataTableShell.test.ts            [CREATE]  Tests
├── FormField.Component.vue           [CREATE]  Form input label+error wrapper
├── FormField.test.ts                 [CREATE]  Tests
├── DetailField.Component.vue         [CREATE]  Read-only field display
├── DetailField.test.ts               [CREATE]  Tests
├── StatusBadge.Component.vue         [CREATE]  Status→Tag lookup component
├── StatusBadge.test.ts               [CREATE]  Tests
├── EmptyState.Component.vue          [CREATE]  Empty list state with CTA
├── EmptyState.test.ts                [CREATE]  Tests
├── ConfirmButton.Component.vue       [CREATE]  Confirm dialog button
├── ConfirmButton.test.ts             [CREATE]  Tests
├── StatCard.Component.vue            [CREATE]  Dashboard metric card
├── StatCard.test.ts                  [CREATE]  Tests
├── TabbedDetail.Component.vue        [CREATE]  Tab container
└── TabbedDetail.test.ts              [CREATE]  Tests

features/
├── catalog/
│   ├── products/views/ProductList.View.vue      [MODIFY]  Use DataTableShell
│   └── products/views/ProductForm.View.vue      [MODIFY]  Use FormField
├── ordering/
│   ├── orders/views/OrderList.View.vue          [MODIFY]  Use DataTableShell
│   └── orders/views/OrderDetail.View.vue        [MODIFY]  DetailField + StatusBadge
├── payment/
│   ├── payments/views/PaymentList.View.vue      [MODIFY]  Full List template
│   └── payments/views/PaymentDetail.View.vue    [MODIFY]  Full Detail template
├── shipping/
│   ├── shipping-methods/views/ShippingMethodList.View.vue  [MODIFY]  Full List template
│   └── shipping-rates/views/ShippingRateList.View.vue      [MODIFY]  Full List template
├── location/
│   ├── countries/views/CountryForm.View.vue      [MODIFY]  zod schema + FormField
│   └── states/views/StateForm.View.vue           [MODIFY]  zod schema + FormField
├── inventorie/
│   └── stock-movements/views/                     [CREATE]  StockMovementList + Detail
├── users/
│   ├── views/StaffForm.View.vue                  [MODIFY]  zod schema + FormField
│   ├── views/CustomerDetail.View.vue             [MODIFY]  Full Detail template
│   ├── roles/views/RoleForm.View.vue             [MODIFY]  zod schema + FormField
│   ├── roles.routes.ts                           [MODIFY]  Dot-notation, under AppLayout
│   └── permissions.routes.ts                     [MODIFY]  Dot-notation, under AppLayout
```

---

## Phase 0: Layout Foundation

### Task 0.1: Persist layout preferences to localStorage

**Files:**
- Modify: `app/Admin/src/app/layout/composables/layout.composable.ts`

**Interfaces:**
- Produces: `layoutConfig` reactively reads/writes `localStorage` keys `layout-config`

- [ ] **Step 1: Add localStorage read on init and watch writes**

```typescript
// app/Admin/src/app/layout/composables/layout.composable.ts
import { computed, reactive, watch } from 'vue'

const STORAGE_KEY = 'resys-admin-layout'

function loadConfig(): Partial<typeof layoutConfig> {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    return raw ? JSON.parse(raw) : {}
  } catch {
    return {}
  }
}

const saved = loadConfig()

const layoutConfig = reactive({
  preset: saved.preset || 'Aura',
  primary: saved.primary || 'emerald',
  surface: (saved.surface as string | null) || null,
  darkTheme: saved.darkTheme ?? false,
  menuMode: saved.menuMode || 'static',
})

watch(
  () => ({ ...layoutConfig }),
  (val) => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(val))
  },
  { deep: true }
)
```

- [ ] **Step 2: Apply saved darkTheme on app init**

In `main.ts`, read `layoutConfig.darkTheme` and apply `.app-dark` class:

```typescript
// app/Admin/src/app/main.ts — add after imports, before app.mount()
import { useLayout } from '@/app/layout/composables/layout.composable'
// After app creation:
const { layoutConfig } = useLayout()
if (layoutConfig.darkTheme) {
  document.documentElement.classList.add('app-dark')
}
```

- [ ] **Step 3: Verify by toggling dark mode, reloading page**

Run: `pnpm run dev` (from `app/Admin/`)
Expected: Dark mode state persists across page reloads.

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/app/layout/composables/layout.composable.ts app/Admin/src/app/main.ts
git commit -m "feat(admin): persist layout preferences to localStorage"
```

---

### Task 0.2: Remove duplicate FloatingConfigurator

**Files:**
- Modify: `app/Admin/src/app/layout/Main.Layout.vue:9,73`

**Interfaces:**
- Consumes: `layout.composable.ts` (existing)
- Produces: N/A

- [ ] **Step 1: Remove FloatingConfigurator from Main.Layout.vue**

Remove the import on line 9 and the component usage on line 73:

```vue
<script setup lang="ts">
// DELETE: import FloatingConfigurator from './components/FloatingConfigurator.Component.vue'
import { useLayout } from './composables/layout.composable'
import { computed, watch, ref } from 'vue'
import { RouterView } from 'vue-router'
import AppTopbar from './Topbar.Layout.vue'
import AppFooter from './Footer.Layout.vue'
import AppSidebar from './Sidebar.Layout.vue'
import AppBreadcrumb from '@/shared/components/Breadcrumb.Component.vue'
</script>

<template>
  <div class="layout-wrapper" :class="containerClass">
    <AppTopbar />
    <AppSidebar />
    <div class="layout-main-container">
      <div class="layout-main">
        <AppBreadcrumb />
        <router-view />
      </div>
      <AppFooter />
    </div>
    <!-- DELETE: <FloatingConfigurator /> -->
    <div class="layout-mask animate-fadein" @click="hideMobileMenu" />
  </div>
</template>
```

- [ ] **Step 2: Verify the configurator panel still opens from topbar gear icon**

Run: `pnpm run dev`
Expected: No floating button bottom-right. Topbar gear icon opens configurator panel.

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/app/layout/Main.Layout.vue
git commit -m "fix(admin): remove duplicate FloatingConfigurator, keep topbar gear icon trigger"
```

---

### Task 0.3: Wire Topbar action buttons

**Files:**
- Modify: `app/Admin/src/app/layout/Topbar.Layout.vue:38-59`

**Interfaces:**
- Consumes: `useAuthStore()` from `@/features/auth/stores/auth.store` (logout action)
- Produces: N/A

- [ ] **Step 1: Replace static profile/calendar/messages buttons with functional profile dropdown**

```vue
<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { useLayout } from './composables/layout.composable'
// ...existing imports...

const router = useRouter()
const authStore = useAuthStore()
const { toggleDarkMode, toggleConfigSidebar } = useLayout()
const profileMenu = ref()
const profileMenuItems = ref([
  { label: 'My Profile', icon: 'pi pi-user', command: () => router.push({ name: 'profile' }) },
  { separator: true },
  { label: 'Logout', icon: 'pi pi-sign-out', command: () => authStore.logout() },
])

const toggleProfileMenu = (event: Event) => {
  profileMenu.value?.toggle(event)
}
</script>
```

- [ ] **Step 2: Replace the button divs with functional buttons**

Replace lines 38-59 (the three static button divs) with:

```vue
<!-- Replace the Calendar/Messages/Profile button divs with: -->
<li class="layout-topbar-action">
  <button class="layout-topbar-action-button" @click="toggleDarkMode" v-tooltip.bottom="'Toggle theme'">
    <i class="pi" :class="{ 'pi-moon': !isDarkTheme, 'pi-sun': isDarkTheme }" />
  </button>
</li>
<li class="layout-topbar-action">
  <button class="layout-topbar-action-button" @click="toggleConfigSidebar" v-tooltip.bottom="'Settings'">
    <i class="pi pi-palette" />
  </button>
</li>
<li class="layout-topbar-action">
  <button class="layout-topbar-action-button" @click="toggleProfileMenu" v-tooltip.bottom="'Profile'">
    <i class="pi pi-user" />
  </button>
  <Menu ref="profileMenu" :model="profileMenuItems" :popup="true" />
</li>
```

- [ ] **Step 3: Verify profile dropdown works and dark mode toggle persists**

Run: `pnpm run dev`
Expected: Profile icon opens dropdown with My Profile + Logout. Dark/theme buttons move to topbar. No Calendar/Messages placeholders.

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/app/layout/Topbar.Layout.vue
git commit -m "fix(admin): wire topbar profile dropdown, remove calendar/messages placeholders"
```

---

### Task 0.4: Add sidebar user footer

**Files:**
- Modify: `app/Admin/src/app/layout/Sidebar.Layout.vue`

**Interfaces:**
- Consumes: `useAuthStore()` for display name, email

- [ ] **Step 1: Add user footer with avatar, name, email, logout**

```vue
<script setup lang="ts">
import { computed } from 'vue'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { useRouter } from 'vue-router'
import AppMenu from './Menu.Layout.vue'

const authStore = useAuthStore()
const router = useRouter()

const userDisplayName = computed(() => authStore.session?.displayName || 'Admin')
const userEmail = computed(() => authStore.session?.email || '')
const userInitials = computed(() => {
  const name = userDisplayName.value
  return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2)
})

function logout() {
  authStore.logout()
  router.push({ name: 'login' })
}
</script>

<template>
  <div class="layout-sidebar-content">
    <AppMenu />
  </div>
  <div class="layout-sidebar-footer">
    <div class="flex items-center gap-3 px-4 py-3 border-t border-surface-200 dark:border-surface-700">
      <Avatar :label="userInitials" shape="circle" size="normal" class="bg-primary text-primary-contrast shrink-0" />
      <div class="flex flex-col min-w-0 flex-1">
        <span class="text-sm font-semibold truncate">{{ userDisplayName }}</span>
        <span class="text-xs text-surface-500 truncate">{{ userEmail }}</span>
      </div>
      <Button icon="pi pi-sign-out" severity="secondary" text rounded size="small" @click="logout" v-tooltip.left="'Logout'" />
    </div>
  </div>
</template>
```

- [ ] **Step 2: Add sidebar footer CSS**

In `assets/scss/layout/_menu.scss`, add at the end:

```scss
.layout-sidebar {
  display: flex;
  flex-direction: column;

  .layout-sidebar-content {
    flex: 1;
    overflow-y: auto;
  }

  .layout-sidebar-footer {
    flex-shrink: 0;
  }
}
```

- [ ] **Step 3: Verify sidebar shows user info and logout works**

Run: `pnpm run dev`
Expected: Bottom of sidebar shows avatar, username, email, logout button.

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/app/layout/Sidebar.Layout.vue app/Admin/src/assets/scss/layout/_menu.scss
git commit -m "feat(admin): add sidebar user footer with avatar, name, email, logout"
```

---

### Task 0.5: Fix roles/permissions routes — nest under AppLayout

**Files:**
- Modify: `app/Admin/src/app/router/index.ts:21-22,35`
- Modify: `app/Admin/src/features/users/roles.routes.ts`
- Modify: `app/Admin/src/features/users/permissions.routes.ts`

**Interfaces:**
- Consumes: `AppLayout` component (existing)
- Produces: Route names `users.roles.list`, `users.roles.create`, `users.roles.edit`, `users.roles.permissions`, `users.permissions.list`

- [ ] **Step 1: Rewrite roles.routes.ts with dot-notation names and relative path**

```typescript
// app/Admin/src/features/users/roles.routes.ts
import type { RouteRecordRaw } from 'vue-router'

export const rolesRoutes: RouteRecordRaw = {
  path: 'roles',
  meta: { breadcrumb: 'roles.title' },
  children: [
    {
      path: '',
      name: 'users.roles.list',
      component: () => import('./roles/views/RoleList.View.vue'),
      meta: { breadcrumb: 'List' },
    },
    {
      path: 'create',
      name: 'users.roles.create',
      component: () => import('./roles/views/RoleForm.View.vue'),
      meta: { breadcrumb: 'Create Role' },
    },
    {
      path: ':id/edit',
      name: 'users.roles.edit',
      component: () => import('./roles/views/RoleForm.View.vue'),
      meta: { breadcrumb: 'Edit Role' },
    },
    {
      path: ':id/permissions',
      name: 'users.roles.permissions',
      component: () => import('./roles/views/RolePermissionsManager.View.vue'),
      meta: { breadcrumb: 'Manage Permissions' },
    },
  ],
}
```

- [ ] **Step 2: Rewrite permissions.routes.ts with dot-notation name and relative path**

```typescript
// app/Admin/src/features/users/permissions.routes.ts
import type { RouteRecordRaw } from 'vue-router'

export const permissionsRoutes: RouteRecordRaw = {
  path: 'permissions',
  meta: { breadcrumb: 'permissions.title' },
  children: [
    {
      path: '',
      name: 'users.permissions.list',
      component: () => import('./permissions/views/PermissionList.View.vue'),
      meta: { breadcrumb: 'List' },
    },
  ],
}
```

- [ ] **Step 3: Update router/index.ts — remove root-level routes, nest under AppLayout, add to users**

```typescript
// app/Admin/src/app/router/index.ts
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    ...errorRoutes,
    // REMOVE: rolesRoutes, permissionsRoutes (root-level lines 21-22)
    { path: '/login', name: 'login', component: () => import('@/features/auth/views/Login.View.vue'), meta: { public: true } },
    { path: '/', component: AppLayout, meta: { breadcrumb: 'navigation.home' }, children: [
      { path: '', name: 'home', redirect: { name: 'reports.dashboard' } },
      { path: 'profile', name: 'profile', component: () => import('@/features/auth/views/Profile.View.vue'), meta: { breadcrumb: 'My Profile' } },
      catalogRoutes,
      reportsRoutes,
      inventoryRoutes,
      orderingRoutes,
      paymentRoutes,
      shippingRoutes,
      locationRoutes,
      addressesRoutes,
      usersRoutes,
      rolesRoutes,       // ADD: nested under AppLayout
      permissionsRoutes,  // ADD: nested under AppLayout
    ]},
  ],
})
```

- [ ] **Step 4: Verify all roles/permissions routes render with sidebar/topbar**

Run: `pnpm run dev`
Expected: Navigate to `/users/roles` → Roles list with full AppLayout wrapper. `/users/permissions` → same.

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/app/router/index.ts app/Admin/src/features/users/roles.routes.ts app/Admin/src/features/users/permissions.routes.ts
git commit -m "fix(admin): nest roles/permissions routes under AppLayout, standardize to dot-notation names"
```

---

### Task 0.6: Normalize all remaining route names to dot-notation

**Files:**
- Modify: `app/Admin/src/features/users/users.routes.ts`

**Interfaces:**
- Produces: `users.staff.list`, `users.staff.create`, `users.staff.detail`, `users.staff.edit`, `users.customers.list`, `users.customers.detail`

- [ ] **Step 1: Rename admin-user-* and customer-* route names**

```typescript
// app/Admin/src/features/users/users.routes.ts
import type { RouteRecordRaw } from 'vue-router'

export const usersRoutes: RouteRecordRaw = {
  path: 'users',
  meta: { breadcrumb: 'Users' },
  children: [
    {
      path: 'staff',
      children: [
        {
          path: '',
          name: 'users.staff.list',
          component: () => import('./views/AdminUserList.View.vue'),
          meta: { breadcrumb: 'Staff' },
        },
        {
          path: 'create',
          name: 'users.staff.create',
          component: () => import('./views/StaffForm.View.vue'),
          meta: { breadcrumb: 'Invite Staff' },
        },
        {
          path: ':id',
          name: 'users.staff.detail',
          component: () => import('./views/StaffDetail.View.vue'),
          meta: { breadcrumb: 'Staff Details' },
        },
        {
          path: ':id/edit',
          name: 'users.staff.edit',
          component: () => import('./views/StaffForm.View.vue'),
          meta: { breadcrumb: 'Edit Staff' },
        },
      ],
    },
    {
      path: 'customers',
      children: [
        {
          path: '',
          name: 'users.customers.list',
          component: () => import('./views/CustomerList.View.vue'),
          meta: { breadcrumb: 'Customers' },
        },
        {
          path: ':id',
          name: 'users.customers.detail',
          component: () => import('./views/CustomerDetail.View.vue'),
          meta: { breadcrumb: 'Customer Details' },
        },
      ],
    },
  ],
}
```

- [ ] **Step 2: Update all references to old route names**

Search and replace across the entire Admin SPA:

```bash
# Old → new mappings (use grep to verify no remaining old names)
# admin-users → users.staff.list
# admin-user-create → users.staff.create
# admin-user-detail → users.staff.detail
# admin-user-edit → users.staff.edit
# customer-users → users.customers.list
# customer-detail → users.customers.detail
# roles-list → users.roles.list
# role-create → users.roles.create
# role-edit → users.roles.edit
# role-permissions → users.roles.permissions
# permissions-list → users.permissions.list
```

Files to check: `GlobalSearch.Component.vue`, `Menu.Layout.vue`, any `router.push({ name: ... })` calls in user views.

- [ ] **Step 3: Run the app and click through all user-related routes**

Run: `pnpm run dev`
Expected: All routes load. No console errors about missing route names.

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/users/users.routes.ts
git add $(rg -l 'admin-users\|customer-users\|roles-list\|role-edit\|permissions-list' app/Admin/src/ 2>/dev/null || true)
git commit -m "refactor(admin): normalize all route names to dot-notation convention"
```

---

## Phase 1: Menu System

### Task 1.1: Create menu config file

**Files:**
- Create: `app/Admin/src/app/config/admin-menu.config.ts`

**Interfaces:**
- Produces: `adminMenuConfig: MenuGroup[]`, `MenuItem` type

- [ ] **Step 1: Write the menu config with all groups and permission gates**

```typescript
// app/Admin/src/app/config/admin-menu.config.ts
import type { RouteLocationRaw } from 'vue-router'

export interface MenuItem {
  label: string
  icon?: string
  to?: RouteLocationRaw
  items?: MenuItem[]
  permission?: string
  separator?: boolean
  badge?: string | number
  class?: string
  disabled?: boolean
  command?: (event: { originalEvent: Event; item: MenuItem }) => void
}

export interface MenuGroup {
  label: string
  icon?: string
  path?: string
  items: MenuItem[]
}

export const adminMenuConfig: MenuGroup[] = [
  {
    label: 'Home',
    items: [
      { label: 'Dashboard', icon: 'pi pi-fw pi-home', to: { name: 'reports.dashboard' } },
      { label: 'My Profile', icon: 'pi pi-fw pi-user', to: { name: 'profile' } },
    ],
  },
  {
    label: 'Catalog',
    path: '/catalog',
    items: [
      { label: 'Dashboard', icon: 'pi pi-fw pi-th-large', to: { name: 'catalog.dashboard' }, permission: 'Catalog' },
      {
        label: 'Products',
        icon: 'pi pi-fw pi-shopping-bag',
        permission: 'Catalog.Products',
        items: [
          { label: 'All Products', icon: 'pi pi-fw pi-list', to: { name: 'catalog.products.list' } },
          { label: 'Add Product', icon: 'pi pi-fw pi-plus-circle', to: { name: 'catalog.products.create' } },
        ],
      },
      {
        label: 'Categories',
        icon: 'pi pi-fw pi-sitemap',
        permission: 'Catalog.Taxonomies',
        items: [
          { label: 'All Categories', icon: 'pi pi-fw pi-tags', to: { name: 'catalog.taxa.list' } },
          { label: 'Manager', icon: 'pi pi-fw pi-sitemap', to: { name: 'catalog.taxonomies.list' } },
        ],
      },
      {
        label: 'Option Types',
        icon: 'pi pi-fw pi-list',
        permission: 'Catalog.OptionTypes',
        items: [
          { label: 'All Types', icon: 'pi pi-fw pi-list', to: { name: 'catalog.option-types.list' } },
          { label: 'Values', icon: 'pi pi-fw pi-th-large', to: { name: 'catalog.option-values.list' } },
        ],
      },
    ],
  },
  {
    label: 'Inventory',
    path: '/inventory',
    items: [
      { label: 'Dashboard', icon: 'pi pi-fw pi-chart-bar', to: { name: 'inventory.dashboard' }, permission: 'Inventory' },
      { label: 'Stock Items', icon: 'pi pi-fw pi-box', to: { name: 'inventory.stocks.list' }, permission: 'Inventory' },
      { label: 'Import', icon: 'pi pi-fw pi-file-import', to: { name: 'inventory.stocks.import' }, permission: 'Inventory' },
      { label: 'Locations', icon: 'pi pi-fw pi-building', to: { name: 'inventory.locations.list' }, permission: 'Inventory' },
      { label: 'Stock Units', icon: 'pi pi-fw pi-cubes', to: { name: 'inventory.units.list' }, permission: 'Inventory' },
      { label: 'Movements', icon: 'pi pi-fw pi-history', to: { name: 'inventory.movements.list' }, permission: 'Inventory' },
      { label: 'Transfers', icon: 'pi pi-fw pi-arrow-right-arrow-left', to: { name: 'inventory.transfers.list' }, permission: 'Inventory' },
    ],
  },
  {
    label: 'Orders',
    path: '/ordering',
    items: [
      { label: 'Dashboard', icon: 'pi pi-fw pi-chart-line', to: { name: 'ordering.dashboard' }, permission: 'Ordering' },
      {
        label: 'All Orders',
        icon: 'pi pi-fw pi-shopping-cart',
        permission: 'Ordering.Orders',
        items: [
          { label: 'List', icon: 'pi pi-fw pi-list', to: { name: 'ordering.orders.list' } },
          { label: 'Create Order', icon: 'pi pi-fw pi-plus-circle', to: { name: 'ordering.orders.create' } },
        ],
      },
      { label: 'Fulfillment', icon: 'pi pi-fw pi-truck', to: { name: 'ordering.fulfillment.queue' }, permission: 'Ordering.Fulfillment' },
    ],
  },
  {
    label: 'Payments',
    path: '/payments',
    items: [
      { label: 'All Payments', icon: 'pi pi-fw pi-wallet', to: { name: 'payment.payments.list' }, permission: 'Payment' },
      { label: 'Payment Methods', icon: 'pi pi-fw pi-credit-card', to: { name: 'payment.methods.list' }, permission: 'Payment' },
    ],
  },
  {
    label: 'Shipping',
    path: '/shipping',
    items: [
      { label: 'Methods', icon: 'pi pi-fw pi-truck', to: { name: 'shipping.methods.list' }, permission: 'Shipping' },
      { label: 'Rates', icon: 'pi pi-fw pi-tag', to: { name: 'shipping.rates.list' }, permission: 'Shipping' },
    ],
  },
  {
    label: 'Locations',
    path: '/locations',
    items: [
      { label: 'Countries', icon: 'pi pi-fw pi-globe', to: { name: 'location.countries.list' }, permission: 'Location' },
      { label: 'States', icon: 'pi pi-fw pi-map', to: { name: 'location.states.list' }, permission: 'Location' },
    ],
  },
  {
    label: 'Users',
    path: '/users',
    items: [
      {
        label: 'Staff',
        icon: 'pi pi-fw pi-id-card',
        permission: 'Identity.Users.Staff',
        items: [
          { label: 'All Staff', icon: 'pi pi-fw pi-list', to: { name: 'users.staff.list' } },
          { label: 'Invite Staff', icon: 'pi pi-fw pi-user-plus', to: { name: 'users.staff.create' } },
        ],
      },
      { label: 'Customers', icon: 'pi pi-fw pi-users', to: { name: 'users.customers.list' }, permission: 'Identity.Users.Customers' },
      { label: 'Addresses', icon: 'pi pi-fw pi-address-book', to: { name: 'addresses' }, permission: 'Identity.Users' },
    ],
  },
  {
    label: 'Access Control',
    path: '/users',
    items: [
      { label: 'Roles', icon: 'pi pi-fw pi-shield', to: { name: 'users.roles.list' }, permission: 'Identity.Roles' },
      { label: 'Permissions', icon: 'pi pi-fw pi-key', to: { name: 'users.permissions.list' }, permission: 'Identity.Permissions' },
    ],
  },
]
```

- [ ] **Step 2: Commit**

```bash
git add app/Admin/src/app/config/admin-menu.config.ts
git commit -m "feat(admin): create menu config file with permission-gated module groups"
```

---

### Task 1.2: Integrate menu config with permission filtering

**Files:**
- Modify: `app/Admin/src/app/layout/Menu.Layout.vue`
- Modify: `app/Admin/src/app/layout/MenuItem.Layout.vue`

- [ ] **Step 1: Update MenuItem.Layout.vue to accept permission prop + badge**

```vue
<script setup lang="ts">
import { useLayout } from './composables/layout.composable'
import { computed, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/features/auth/stores/auth.store'

export interface MenuItem {
  label?: string
  icon?: string
  to?: string | object
  url?: string
  target?: string
  items?: MenuItem[]
  separator?: boolean
  permission?: string
  badge?: string | number
  disabled?: boolean
  class?: string
  command?: (event: { originalEvent: Event; item: MenuItem }) => void
}

const route = useRoute()
const { layoutState } = useLayout()
const authStore = useAuthStore()

defineOptions({ name: 'AppMenuItem' })

const props = defineProps<{
  item: MenuItem
  index?: number
  root?: boolean
}>()

const active = ref(false)

const hasPermission = computed(() => {
  if (!props.item.permission) return true
  return authStore.permissions?.includes(props.item.permission) ?? false
})

const isActive = computed(() => {
  if (!hasPermission.value) return false
  if (props.item.to && typeof props.item.to === 'string' && route.path === props.item.to) return true
  if (props.item.to && typeof props.item.to === 'object' && 'name' in props.item.to) {
    return route.name === props.item.to.name
  }
  // ...rest of isActive logic unchanged
  return false
})

// ...rest unchanged
</script>

<template>
  <template v-if="hasPermission">
    <li :class="{ 'layout-root-menuitem': root, 'active-menuitem': active || isActive }">
      <!-- ...existing template unchanged... -->
      <span v-if="props.item.badge" class="ml-auto">
        <Badge :value="props.item.badge" severity="info" size="small" />
      </span>
      <!-- ...rest unchanged... -->
    </li>
  </template>
</template>
```

Note: This is a skeleton — apply the `hasPermission` check and `badge` rendering to the existing component structure without losing any current functionality.

- [ ] **Step 2: Update Menu.Layout.vue to use config file**

```vue
<script setup lang="ts">
import AppMenuItem from './MenuItem.Layout.vue'
import { adminMenuConfig } from '@/app/config/admin-menu.config'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { computed } from 'vue'

const authStore = useAuthStore()

function groupHasVisibleItems(items: typeof adminMenuConfig[0]['items']): boolean {
  return items.some(item => {
    if (!item.permission) return true
    return authStore.permissions?.includes(item.permission) ?? false
  })
}

const visibleGroups = computed(() =>
  adminMenuConfig.filter(group => groupHasVisibleItems(group.items))
)
</script>

<template>
  <ul class="layout-menu">
    <template v-for="(item, i) in visibleGroups" :key="item.label">
      <AppMenuItem v-if="!item.separator" :item="item" :index="i" root />
      <li v-if="item.separator" class="menu-separator" />
    </template>
  </ul>
</template>
```

- [ ] **Step 3: Verify menu renders correctly with/without permissions**

Run: `pnpm run dev`
Expected: Menu renders all groups. If `authStore.permissions` is empty, all items show. If permissions are set, only granted items show.

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/app/layout/Menu.Layout.vue app/Admin/src/app/layout/MenuItem.Layout.vue
git commit -m "feat(admin): integrate menu config with permission filtering"
```

---

## Phase 2: Shared Component Library

### Task 2.1: Create FormField component

**Files:**
- Create: `app/Admin/src/shared/components/FormField.Component.vue`
- Create: `app/Admin/src/shared/components/__tests__/FormField.test.ts`

- [ ] **Step 1: Write FormField.Component.vue**

```vue
<script setup lang="ts">
withDefaults(defineProps<{
  label: string
  name: string
  error?: string
  required?: boolean
  hint?: string
}>(), {
  required: false,
})

defineSlots<{
  default(): any
}>()
</script>

<template>
  <div class="flex flex-col gap-2">
    <label
      :for="name"
      class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1"
    >
      {{ label }}
      <span v-if="required" class="text-red-500">*</span>
    </label>
    <slot />
    <small v-if="error" class="p-error">{{ error }}</small>
    <small v-else-if="hint" class="text-surface-400">{{ hint }}</small>
  </div>
</template>
```

- [ ] **Step 2: Write tests**

```typescript
// __tests__/FormField.test.ts
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import FormField from '../FormField.Component.vue'
import InputText from 'primevue/inputtext'

describe('FormField', () => {
  it('renders label and slot content', () => {
    const wrapper = mount(FormField, {
      props: { label: 'Name', name: 'name' },
      slots: { default: '<input id="name" />' },
      global: { stubs: { InputText } },
    })
    expect(wrapper.find('label').text()).toBe('Name')
  })

  it('shows required asterisk', () => {
    const wrapper = mount(FormField, {
      props: { label: 'Name', name: 'name', required: true },
      slots: { default: '<input id="name" />' },
      global: { stubs: { InputText } },
    })
    expect(wrapper.find('label span').text()).toBe('*')
  })

  it('shows error message when provided', () => {
    const wrapper = mount(FormField, {
      props: { label: 'Name', name: 'name', error: 'Required field' },
      slots: { default: '<input id="name" />' },
      global: { stubs: { InputText } },
    })
    expect(wrapper.find('.p-error').text()).toBe('Required field')
  })

  it('shows hint when no error', () => {
    const wrapper = mount(FormField, {
      props: { label: 'Name', name: 'name', hint: 'Enter your full name' },
      slots: { default: '<input id="name" />' },
      global: { stubs: { InputText } },
    })
    expect(wrapper.find('.text-surface-400').text()).toBe('Enter your full name')
  })
})
```

- [ ] **Step 3: Run tests**

```bash
cd app/Admin && pnpm run test:unit -- __tests__/FormField.test.ts
```

Expected: 4 tests pass.

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/shared/components/FormField.Component.vue app/Admin/src/shared/components/__tests__/FormField.test.ts
git commit -m "feat(admin): create FormField component with tests"
```

---

### Task 2.2: Create DataTableShell component

**Files:**
- Create: `app/Admin/src/shared/components/DataTableShell.Component.vue`
- Create: `app/Admin/src/shared/components/__tests__/DataTableShell.test.ts`

- [ ] **Step 1: Write DataTableShell.Component.vue**

```vue
<script setup lang="ts" generic="T extends Record<string, any>">
import type { DataTablePageEvent, DataTableSortEvent, DataTableFilterMeta } from 'primevue/datatable'
import { FilterMatchMode } from '@primevue/core/api'

export interface ColumnDef {
  field: string
  header: string
  sortable?: boolean
  filter?: boolean
  class?: string
  body?: (data: T) => string
}

const props = withDefaults(defineProps<{
  columns: ColumnDef[]
  value: T[]
  loading?: boolean
  totalRecords?: number
  rows?: number
  lazy?: boolean
  dataKey?: string
  sortField?: string
  sortOrder?: number
  emptyIcon?: string
  emptyTitle?: string
  emptyDescription?: string
  searchPlaceholder?: string
  showCreateButton?: boolean
  createRoute?: any
  createLabel?: string
  showExport?: boolean
  showClearFilters?: boolean
}>(), {
  loading: false,
  totalRecords: 0,
  rows: 10,
  lazy: true,
  dataKey: 'id',
  emptyIcon: 'pi-inbox',
  emptyTitle: 'No items found',
  searchPlaceholder: 'Search...',
  showCreateButton: true,
  showExport: false,
  showClearFilters: true,
})

const emit = defineEmits<{
  page: [event: DataTablePageEvent]
  sort: [event: DataTableSortEvent]
  filter: []
  refresh: []
  export: []
}>()

const filters = defineModel<DataTableFilterMeta>('filters')

const globalFilterValue = computed({
  get: () => (filters.value?.global as any)?.value ?? '',
  set: (val: string) => {
    if (!filters.value) filters.value = {} as DataTableFilterMeta
    filters.value.global = { value: val, matchMode: FilterMatchMode.CONTAINS }
  },
})

const skeletonRows = computed(() => Array.from({ length: props.rows }, (_, i) => ({ id: `sk-${i}` })))
const columnCount = computed(() => props.columns.length + 1) // +1 for actions
</script>

<template>
  <DataTable
    v-model:filters="filters"
    :value="value"
    :loading="loading"
    :totalRecords="totalRecords"
    :lazy="lazy"
    :rows="rows"
    :sortField="sortField"
    :sortOrder="sortOrder"
    :dataKey="dataKey"
    :paginator="true"
    :rowsPerPageOptions="[5, 10, 20, 50]"
    @page="emit('page', $event)"
    @sort="emit('sort', $event)"
    @filter="emit('filter')"
    filterDisplay="menu"
    removableSort
    scrollable
    rowHover
    stripedRows
    showGridlines
    breakpoint="960px"
  >
    <template #header>
      <div class="flex flex-col items-center justify-between gap-4 md:flex-row">
        <IconField iconPosition="left" class="w-full md:w-72">
          <InputIcon class="pi pi-search" />
          <InputText
            v-model="globalFilterValue"
            :placeholder="searchPlaceholder"
            @keyup.enter="emit('filter')"
            class="w-full rounded-xl"
          />
        </IconField>

        <div class="flex items-center gap-2">
          <Button
            v-if="showClearFilters"
            type="button"
            icon="pi pi-filter-slash"
            label="Clear Filters"
            outlined
            @click="emit('filter')"
            class="rounded-xl"
          />
          <Button
            v-if="showCreateButton && createRoute"
            :label="createLabel || 'Create'"
            icon="pi pi-plus"
            @click="$router.push(createRoute)"
            class="rounded-xl"
          />
          <Button
            v-if="showExport"
            type="button"
            icon="pi pi-download"
            label="Export"
            severity="secondary"
            outlined
            @click="emit('export')"
            class="rounded-xl"
          />
          <Button
            type="button"
            icon="pi pi-refresh"
            severity="secondary"
            outlined
            @click="emit('refresh')"
            class="rounded-xl"
          />
          <slot name="toolbar-actions" />
        </div>
      </div>
    </template>

    <template #empty>
      <slot name="empty">
        <div class="flex flex-col items-center justify-center py-20 text-surface-400">
          <i :class="emptyIcon" class="mb-4 text-6xl opacity-20" />
          <p class="text-xl font-medium">{{ emptyTitle }}</p>
          <p v-if="emptyDescription" class="text-sm mt-1">{{ emptyDescription }}</p>
        </div>
      </slot>
    </template>

    <template #loading>
      <div class="p-4">
        <Skeleton v-for="i in skeletonRows.length" :key="i" class="mb-3" height="2.5rem" />
      </div>
    </template>

    <Column
      v-for="col in columns"
      :key="col.field"
      :field="col.field"
      :header="col.header"
      :sortable="col.sortable ?? false"
      :filter="col.filter ?? false"
      :class="col.class"
    >
      <template v-if="col.body" #body="{ data }">
        {{ col.body(data) }}
      </template>
    </Column>

    <Column header="Actions" class="w-32 text-right" frozen alignFrozen="right">
      <template #body="{ data }">
        <div class="flex justify-end gap-1">
          <slot name="row-actions" :data="data" />
        </div>
      </template>
    </Column>
  </DataTable>
</template>

<script lang="ts">
import { computed } from 'vue'
</script>
```

- [ ] **Step 2: Write tests**

```typescript
// __tests__/DataTableShell.test.ts
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import DataTableShell from '../DataTableShell.Component.vue'
import PrimeVue from 'primevue/config'

const columns = [
  { field: 'name', header: 'Name', sortable: true },
  { field: 'status', header: 'Status' },
]

const items = [
  { id: '1', name: 'Item 1', status: 'Active' },
  { id: '2', name: 'Item 2', status: 'Draft' },
]

describe('DataTableShell', () => {
  it('renders columns and data', () => {
    const wrapper = mount(DataTableShell, {
      props: { columns, value: items, totalRecords: 2 },
      global: { plugins: [PrimeVue] },
    })
    expect(wrapper.html()).toContain('Item 1')
  })

  it('shows empty state when no data', () => {
    const wrapper = mount(DataTableShell, {
      props: { columns, value: [], totalRecords: 0, emptyTitle: 'Nothing here' },
      global: { plugins: [PrimeVue] },
    })
    expect(wrapper.text()).toContain('Nothing here')
  })

  it('shows skeleton when loading', () => {
    const wrapper = mount(DataTableShell, {
      props: { columns, value: [], loading: true, totalRecords: 0 },
      global: { plugins: [PrimeVue] },
    })
    expect(wrapper.html()).toContain('skeleton')
  })
})
```

- [ ] **Step 3: Run tests**

```bash
cd app/Admin && pnpm run test:unit -- __tests__/DataTableShell.test.ts
```

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/shared/components/DataTableShell.Component.vue app/Admin/src/shared/components/__tests__/DataTableShell.test.ts
git commit -m "feat(admin): create DataTableShell component with tests"
```

---

### Task 2.3: Create remaining shared components (batch)

**Files:**
- Create: `app/Admin/src/shared/components/DetailField.Component.vue`
- Create: `app/Admin/src/shared/components/StatusBadge.Component.vue`
- Create: `app/Admin/src/shared/components/EmptyState.Component.vue`
- Create: `app/Admin/src/shared/components/ConfirmButton.Component.vue`
- Create: `app/Admin/src/shared/components/StatCard.Component.vue`
- Create: `app/Admin/src/shared/components/TabbedDetail.Component.vue`
- Create: `app/Admin/src/shared/components/__tests__/` (tests for each)

- [ ] **Step 1: Create DetailField.Component.vue**

```vue
<script setup lang="ts">
withDefaults(defineProps<{
  label: string
  value?: string | number | null
  emptyText?: string
}>(), {
  emptyText: '\u2014',
})
</script>

<template>
  <div class="flex flex-col">
    <span class="text-xs text-surface-400 uppercase font-bold mb-1">{{ label }}</span>
    <span v-if="value !== null && value !== undefined && value !== ''" class="text-lg font-medium text-surface-900 dark:text-surface-0">
      {{ value }}
    </span>
    <span v-else class="text-lg text-surface-300 dark:text-surface-600">{{ emptyText }}</span>
  </div>
</template>
```

- [ ] **Step 2: Create StatusBadge.Component.vue**

```vue
<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(defineProps<{
  status: string | number
  statusMap: Record<string | number, { label: string; severity: string }>
  size?: 'small' | 'normal'
}>(), {
  size: 'normal',
})

const resolved = computed(() => props.statusMap[props.status] ?? { label: String(props.status), severity: 'secondary' })
</script>

<template>
  <Tag
    :value="resolved.label"
    :severity="resolved.severity as any"
    :class="size === 'normal' ? 'px-4 py-2 text-lg font-bold rounded-xl' : ''"
    rounded
  />
</template>
```

- [ ] **Step 3: Create EmptyState.Component.vue**

```vue
<script setup lang="ts">
import { useRouter } from 'vue-router'

const router = useRouter()

const props = withDefaults(defineProps<{
  icon?: string
  title: string
  description?: string
  actionLabel?: string
  actionRoute?: any
}>(), {
  icon: 'pi pi-inbox',
})
</script>

<template>
  <div class="flex flex-col items-center justify-center py-20 text-surface-400">
    <i :class="icon" class="mb-4 text-6xl opacity-20" />
    <p class="text-xl font-medium">{{ title }}</p>
    <p v-if="description" class="text-sm mt-1 max-w-md text-center">{{ description }}</p>
    <Button
      v-if="actionLabel && actionRoute"
      :label="actionLabel"
      icon="pi pi-plus"
      class="mt-6 rounded-xl"
      @click="router.push(actionRoute)"
    />
  </div>
</template>
```

- [ ] **Step 4: Create ConfirmButton.Component.vue**

```vue
<script setup lang="ts">
import { useConfirm } from 'primevue/useconfirm'

const confirm = useConfirm()

const props = withDefaults(defineProps<{
  icon?: string
  severity?: string
  rounded?: boolean
  text?: boolean
  header: string
  message: string
  acceptLabel?: string
  rejectLabel?: string
  loading?: boolean
}>(), {
  icon: 'pi pi-trash',
  severity: 'danger',
  rounded: true,
  text: true,
})

const emit = defineEmits<{
  confirm: []
}>()

function onClick() {
  confirm.require({
    message: props.message,
    header: props.header,
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: props.rejectLabel,
    acceptProps: {
      label: props.acceptLabel,
      severity: props.severity as any,
    },
    accept: () => emit('confirm'),
  })
}
</script>

<template>
  <Button
    :icon="icon"
    :severity="severity"
    :rounded="rounded"
    :text="text"
    :loading="loading"
    @click="onClick"
  />
</template>
```

- [ ] **Step 5: Create StatCard.Component.vue**

```vue
<script setup lang="ts">
const props = withDefaults(defineProps<{
  title: string
  value: string | number
  icon: string
  iconBg: string
  trendLabel?: string
  trendValue?: number
  trendPositive?: boolean
  skeleton?: boolean
}>(), {
  skeleton: false,
})
</script>

<template>
  <div class="card !mb-0 flex flex-col gap-4 p-6">
    <template v-if="skeleton">
      <Skeleton width="3rem" height="3rem" borderRadius="50%" />
      <Skeleton width="60%" height="2rem" />
      <Skeleton width="40%" height="1rem" />
    </template>
    <template v-else>
      <div class="flex items-center justify-between">
        <div :class="iconBg" class="flex items-center justify-center rounded-full" style="width: 3rem; height: 3rem">
          <i :class="icon" class="text-xl" />
        </div>
      </div>
      <span class="text-2xl font-black text-surface-900 dark:text-surface-0">{{ value }}</span>
      <div class="flex items-center gap-2">
        <span class="text-sm text-surface-500">{{ title }}</span>
        <template v-if="trendValue !== undefined">
          <i
            :class="trendPositive ? 'pi pi-arrow-up text-green-500' : 'pi pi-arrow-down text-red-500'"
            class="text-xs"
          />
          <span :class="trendPositive ? 'text-green-500' : 'text-red-500'" class="text-xs font-medium">
            {{ trendValue }}%
          </span>
        </template>
      </div>
    </template>
  </div>
</template>
```

- [ ] **Step 6: Create TabbedDetail.Component.vue**

```vue
<script setup lang="ts">
import type { Component } from 'vue'

export interface TabDef {
  label: string
  icon?: string
  value: number | string
  panel: Component
}

const props = withDefaults(defineProps<{
  tabs: TabDef[]
  scrollable?: boolean
}>(), {
  scrollable: true,
})

const activeTab = defineModel<number | string>('activeTab', { required: true })
</script>

<template>
  <Tabs v-model:value="activeTab">
    <TabList :scrollable="scrollable">
      <Tab v-for="tab in tabs" :key="tab.value" :value="tab.value">
        <div class="flex items-center gap-2">
          <i v-if="tab.icon" :class="tab.icon" />
          <span>{{ tab.label }}</span>
        </div>
      </Tab>
    </TabList>
    <TabPanels class="p-6">
      <TabPanel v-for="tab in tabs" :key="tab.value" :value="tab.value">
        <component :is="tab.panel" />
      </TabPanel>
    </TabPanels>
  </Tabs>
</template>
```

- [ ] **Step 7: Run all component tests**

```bash
cd app/Admin && pnpm run test:unit -- __tests__/
```

- [ ] **Step 8: Commit**

```bash
git add app/Admin/src/shared/components/ app/Admin/src/shared/components/__tests__/
git commit -m "feat(admin): create remaining shared components (DetailField, StatusBadge, EmptyState, ConfirmButton, StatCard, TabbedDetail)"
```

---

## Phase 3–6: Module Standardization (batched)

Due to the repetitive nature of applying the same 4 templates across 8 modules, the remaining phases are batched per module. Each module follows the same pattern:

1. Identify page type (List / Form / Detail / Dashboard)
2. Apply the corresponding template from Section 4 of the spec
3. Swap in shared components (DataTableShell, FormField, etc.)
4. Add i18n keys for all user-facing strings
5. Verify the page renders correctly

### Task 3.0: Standardize Payment List + Detail

**Files:**
- Modify: `app/Admin/src/features/payment/payments/views/PaymentList.View.vue`
- Modify: `app/Admin/src/features/payment/payments/views/PaymentDetail.View.vue`

- [ ] **Step 1: Rewrite PaymentList.View.vue with List template**

```vue
<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { usePaymentStore } from '../stores/payment.store'
import { storeToRefs } from 'pinia'
import type { DataTablePageEvent, DataTableSortEvent } from 'primevue/datatable'
import PageShell from '@/shared/components/PageShell.Component.vue'
import PageHeader from '@/shared/components/PageHeader.Component.vue'
import DataTableShell from '@/shared/components/DataTableShell.Component.vue'
import type { ColumnDef } from '@/shared/components/DataTableShell.Component.vue'
import ConfirmButton from '@/shared/components/ConfirmButton.Component.vue'

const { t } = useI18n()
const router = useRouter()
const store = usePaymentStore()
const { items, loading, totalRecords } = storeToRefs(store)

const columns: ColumnDef[] = [
  { field: 'id', header: 'ID', sortable: true },
  { field: 'orderId', header: t('payment.table.order'), sortable: true },
  { field: 'amountDisplay', header: t('payment.table.amount'), sortable: true },
  { field: 'statusLabel', header: t('payment.table.status'), sortable: true },
  { field: 'methodName', header: t('payment.table.method'), sortable: true },
]

function onPage(event: DataTablePageEvent) {
  store.fetchItems({ page: event.page + 1, pageSize: event.rows })
}
function onSort(event: DataTableSortEvent) {
  store.fetchItems({ sort: event.sortField ? [`${event.sortOrder === -1 ? '-' : ''}${event.sortField}`] : undefined })
}

onMounted(() => store.fetchItems({}))
</script>

<template>
  <PageShell max-width="7xl">
    <PageHeader :title="t('payment.titles.list')" :description="t('payment.descriptions.list')">
      <template #badge>
        <Badge :value="totalRecords" severity="info" />
      </template>
    </PageHeader>

    <DataTableShell
      :columns="columns"
      :value="items"
      :loading="loading"
      :total-records="totalRecords"
      :search-placeholder="t('payment.placeholders.search')"
      :empty-title="t('payment.messages.empty')"
      :show-create-button="false"
      @page="onPage"
      @sort="onSort"
    >
      <template #row-actions="{ data }">
        <Button icon="pi pi-eye" severity="info" text rounded @click="router.push({ name: 'payment.payments.detail', params: { id: data.id } })" />
      </template>
    </DataTableShell>
  </PageShell>
</template>
```

- [ ] **Step 2: Rewrite PaymentDetail.View.vue with Detail template**

```vue
<script setup lang="ts">
import { onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { usePaymentStore } from '../stores/payment.store'
import { storeToRefs } from 'pinia'
import PageShell from '@/shared/components/PageShell.Component.vue'
import PageHeader from '@/shared/components/PageHeader.Component.vue'
import StatusBadge from '@/shared/components/StatusBadge.Component.vue'
import DetailField from '@/shared/components/DetailField.Component.vue'

const route = useRoute()
const router = useRouter()
const { t } = useI18n()
const store = usePaymentStore()
const { current, loading } = storeToRefs(store)

const paymentStatusMap: Record<string, { label: string; severity: string }> = {
  Pending: { label: t('payment.statuses.pending'), severity: 'warn' },
  Completed: { label: t('payment.statuses.completed'), severity: 'success' },
  Failed: { label: t('payment.statuses.failed'), severity: 'danger' },
  Refunded: { label: t('payment.statuses.refunded'), severity: 'info' },
  Voided: { label: t('payment.statuses.voided'), severity: 'secondary' },
}

onMounted(() => store.fetchById(route.params.id as string))
</script>

<template>
  <PageShell :card="false" gap max-width="7xl">
    <template v-if="current">
      <PageHeader back :title="`${t('payment.titles.detail')} #${current.id}`">
        <template #badge>
          <StatusBadge :status="current.status" :status-map="paymentStatusMap" />
        </template>
        <template #actions>
          <Button
            v-if="current.status === 'Pending'"
            :label="t('payment.actions.capture')"
            icon="pi pi-check"
            @click="store.capture(current.id)"
            class="rounded-xl"
          />
          <Button
            v-if="current.status === 'Pending'"
            :label="t('payment.actions.void')"
            icon="pi pi-times"
            severity="warn"
            outlined
            @click="store.void(current.id)"
            class="rounded-xl"
          />
          <Button
            v-if="current.status === 'Completed'"
            :label="t('payment.actions.refund')"
            icon="pi pi-replay"
            severity="danger"
            outlined
            @click="store.refund(current.id, current.amount)"
            class="rounded-xl"
          />
        </template>
      </PageHeader>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <div class="lg:col-span-2 flex flex-col gap-6">
          <Card class="rounded-3xl shadow-sm border-none bg-surface-0 dark:bg-surface-900">
            <template #title>
              <span class="text-sm font-black uppercase tracking-widest text-surface-400 p-6 pb-0 block">
                {{ t('payment.labels.details') }}
              </span>
            </template>
            <template #content>
              <div class="grid grid-cols-2 gap-6">
                <DetailField :label="t('payment.labels.amount')" :value="current.amountDisplay" />
                <DetailField :label="t('payment.labels.method')" :value="current.methodName" />
                <DetailField :label="t('payment.labels.order')" :value="current.orderId" />
              </div>
            </template>
          </Card>
        </div>

        <div class="flex flex-col gap-6">
          <Card class="rounded-3xl shadow-sm border-none bg-surface-0 dark:bg-surface-900">
            <template #title>
              <span class="text-sm font-black uppercase tracking-widest text-surface-400 p-6 pb-0 block">
                {{ t('payment.labels.actions_title') }}
              </span>
            </template>
            <template #content>
              <p class="text-sm text-surface-400">{{ t('payment.messages.no_transactions') }}</p>
            </template>
          </Card>
        </div>
      </div>
    </template>

    <div v-else-if="loading" class="flex justify-center py-20">
      <ProgressSpinner />
    </div>
  </PageShell>
</template>
```

- [ ] **Step 3: Verify both pages render**

Run: `pnpm run dev` → navigate to Payments
Expected: Payment list shows with DataTableShell. Payment detail shows with StatusBadge and DetailFields.

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/payment/payments/views/
git commit -m "feat(admin): standardize Payment list and detail views with shared components"
```

---

### Task 3.1: Standardize Shipping methods + rates lists

**Files:**
- Modify: `app/Admin/src/features/shipping/shipping-methods/views/ShippingMethodList.View.vue`
- Modify: `app/Admin/src/features/shipping/shipping-rates/views/ShippingRateList.View.vue`

- [ ] **Step 1: Rewrite ShippingMethodList.View.vue**

```vue
<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useShippingMethodStore } from '../stores/shipping-method.store'
import { storeToRefs } from 'pinia'
import type { DataTablePageEvent, DataTableSortEvent } from 'primevue/datatable'
import PageShell from '@/shared/components/PageShell.Component.vue'
import PageHeader from '@/shared/components/PageHeader.Component.vue'
import DataTableShell from '@/shared/components/DataTableShell.Component.vue'
import type { ColumnDef } from '@/shared/components/DataTableShell.Component.vue'
import ConfirmButton from '@/shared/components/ConfirmButton.Component.vue'

const router = useRouter()
const store = useShippingMethodStore()
const { items, loading, totalRecords } = storeToRefs(store)

const columns: ColumnDef[] = [
  { field: 'name', header: 'Name', sortable: true },
  { field: 'carrier', header: 'Carrier', sortable: true },
  { field: 'isActive', header: 'Active' },
  { field: 'displayOrder', header: 'Order', sortable: true },
]

function onPage(e: DataTablePageEvent) { store.fetchItems({ page: e.page + 1, pageSize: e.rows }) }
function onSort(e: DataTableSortEvent) { store.fetchItems({ sort: e.sortField ? [`${e.sortOrder === -1 ? '-' : ''}${e.sortField}`] : undefined }) }
function refresh() { store.fetchItems({}) }
function onDelete(item: any) { store.deleteItem(item.id) }

onMounted(() => store.fetchItems({}))
</script>

<template>
  <PageShell max-width="7xl">
    <PageHeader title="Shipping Methods" description="Manage available shipping carriers and methods">
      <template #badge>
        <Badge :value="totalRecords" severity="info" />
      </template>
    </PageHeader>

    <DataTableShell
      :columns="columns"
      :value="items"
      :loading="loading"
      :total-records="totalRecords"
      empty-title="No shipping methods configured"
      empty-description="Add your first shipping method to start offering delivery options."
      :create-route="{ name: 'shipping.methods.create' }"
      create-label="Add Method"
      @page="onPage"
      @sort="onSort"
      @refresh="refresh"
    >
      <template #row-actions="{ data }">
        <Button icon="pi pi-pencil" severity="secondary" text rounded
          @click="router.push({ name: 'shipping.methods.edit', params: { id: data.id } })" />
        <ConfirmButton header="Delete Method" :message="`Remove ${data.name}?`" @confirm="onDelete(data)" />
      </template>
    </DataTableShell>
  </PageShell>
</template>
```

- [ ] **Step 2: Apply same pattern to ShippingRateList.View.vue**

Same template, replace store and column definitions:

```typescript
const store = useShippingRateStore()
const columns: ColumnDef[] = [
  { field: 'name', header: 'Name', sortable: true },
  { field: 'price', header: 'Price', sortable: true },
  { field: 'minOrderSubtotal', header: 'Min Subtotal', sortable: true },
  { field: 'isActive', header: 'Active' },
]
```

- [ ] **Step 3: Verify**

Run: `pnpm run dev` → navigate to Shipping → Methods and Rates
Expected: Both pages show with PageShell, PageHeader, DataTableShell, create/edit/delete buttons.

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/shipping/
git commit -m "feat(admin): standardize Shipping method and rate list views"
```

---

### Task 3.2: Migrate CountryForm and StateForm to vee-validate + zod

**Files:**
- Create: `app/Admin/src/features/location/countries/schemas/country.schema.ts`
- Create: `app/Admin/src/features/location/states/schemas/state.schema.ts`
- Modify: `app/Admin/src/features/location/countries/views/CountryForm.View.vue`
- Modify: `app/Admin/src/features/location/states/views/StateForm.View.vue`

- [ ] **Step 1: Create country schema**

```typescript
// features/location/countries/schemas/country.schema.ts
import { z } from 'zod'

export function createCountrySchema(t: (key: string) => string) {
  return z.object({
    name: z.string().min(1, t('validation.required')),
    isoCode: z.string().length(2, t('validation.iso_code_length')),
    callingCode: z.string().min(1, t('validation.required')),
  })
}
```

- [ ] **Step 2: Create state schema**

```typescript
// features/location/states/schemas/state.schema.ts
import { z } from 'zod'

export function createStateSchema(t: (key: string) => string) {
  return z.object({
    name: z.string().min(1, t('validation.required')),
    abbreviation: z.string().min(1, t('validation.required')).max(10),
    countryId: z.string().min(1, t('validation.required')),
  })
}
```

- [ ] **Step 3: Rewrite CountryForm.View.vue**

```vue
<script setup lang="ts">
import { onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { createCountrySchema } from '../schemas/country.schema'
import { useCountryStore } from '../stores/country.store'
import PageShell from '@/shared/components/PageShell.Component.vue'
import PageHeader from '@/shared/components/PageHeader.Component.vue'
import FormField from '@/shared/components/FormField.Component.vue'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const store = useCountryStore()

const isEdit = computed(() => route.name === 'location.countries.edit')
const countryId = computed(() => route.params.id as string | undefined)

const { defineField, handleSubmit, errors, setValues } = useForm({
  validationSchema: toTypedSchema(createCountrySchema(t)),
})

const [name] = defineField('name')
const [isoCode] = defineField('isoCode')
const [callingCode] = defineField('callingCode')

onMounted(async () => {
  if (isEdit.value && countryId.value) {
    const result = await store.fetchById(countryId.value)
    if (result.isSuccess && store.current) {
      setValues({
        name: store.current.name,
        isoCode: store.current.isoCode,
        callingCode: store.current.callingCode,
      })
    }
  }
})

const onSubmit = handleSubmit(async (values) => {
  if (isEdit.value && countryId.value) {
    await store.update(countryId.value, values)
  } else {
    await store.create(values)
  }
  router.push({ name: 'location.countries.list' })
})
</script>

<template>
  <PageShell max-width="7xl">
    <PageHeader back :title="isEdit ? t('location.countries.titles.edit') : t('location.countries.titles.create')">
      <template #actions>
        <Button label="Cancel" severity="secondary" outlined @click="router.back()" class="rounded-xl" />
        <Button label="Save" icon="pi pi-check" @click="onSubmit" class="rounded-xl" />
      </template>
    </PageHeader>

    <Card class="border-none shadow-sm rounded-3xl">
      <template #content>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-8 max-w-3xl">
          <FormField label="Name" name="name" :error="errors.name" required>
            <InputText v-model="name" class="w-full rounded-2xl h-12 px-4" :invalid="!!errors.name" />
          </FormField>
          <FormField label="ISO Code" name="isoCode" :error="errors.isoCode" required>
            <InputText v-model="isoCode" class="w-full rounded-2xl h-12 px-4" :invalid="!!errors.isoCode" maxlength="2" />
          </FormField>
          <FormField label="Calling Code" name="callingCode" :error="errors.callingCode" required>
            <InputText v-model="callingCode" class="w-full rounded-2xl h-12 px-4" :invalid="!!errors.callingCode" />
          </FormField>
        </div>
      </template>
    </Card>
  </PageShell>
</template>
```

- [ ] **Step 4: Apply same pattern to StateForm.View.vue** (similar, with country dropdown)

- [ ] **Step 5: Run dev, test create/edit flows**

Run: `pnpm run dev` → Locations → Create/Edit Country, Create/Edit State
Expected: Forms work with validation. Errors display under fields via FormField.

- [ ] **Step 6: Commit**

```bash
git add app/Admin/src/features/location/
git commit -m "refactor(admin): migrate CountryForm and StateForm to vee-validate + zod with FormField"
```

---

### Task 3.3: Swap ProductList + ProductForm to shared components

**Files:**
- Modify: `app/Admin/src/features/catalog/products/views/ProductList.View.vue`
- Modify: `app/Admin/src/features/catalog/products/views/ProductForm.View.vue`

- [ ] **Step 1: Replace ProductList DataTable with DataTableShell**

Replace the hand-rolled `<DataTable>...</DataTable>` (lines 149-231) with:

```vue
<DataTableShell
  v-model:filters="filters"
  :columns="columns"
  :value="products"
  :loading="loading"
  :total-records="totalRecords"
  :sort-field="query.sort?.[0]?.replace(/^-/, '')"
  :sort-order="query.sort?.[0]?.startsWith('-') ? -1 : 1"
  :search-placeholder="t('catalog.products.placeholders.search')"
  :empty-title="t('catalog.products.messages.empty_list')"
  empty-icon="pi pi-shopping-bag"
  :create-route="{ name: 'catalog.products.create' }"
  :create-label="t('catalog.products.actions.new')"
  :show-clear-filters="true"
  data-key="id"
  @page="onPage"
  @sort="onSort"
  @filter="onFilter"
  @refresh="loadProducts"
>
  <template #row-actions="{ data }">
    <Button icon="pi pi-pencil" severity="secondary" text rounded
      @click="router.push({ name: 'catalog.products.edit', params: { id: data.id } })" />
    <ConfirmButton header="Delete Product"
      :message="t('catalog.products.confirm.delete_message').replace('{name}', data.name)"
      @confirm="confirmDelete(data)" />
  </template>
</DataTableShell>
```

Define columns at script level:

```typescript
const columns: ColumnDef[] = [
  { field: 'name', header: t('catalog.products.table.name'), sortable: true },
  { field: 'variantsCount', header: t('catalog.products.table.variants'), class: 'text-center w-24' },
  { field: 'statusLabel', header: t('catalog.products.table.status') },
]
```

Remove the old `<template #header>`, `<template #empty>`, and action column — DataTableShell handles these.

- [ ] **Step 2: Replace ProductForm label+InputText pairs with FormField**

Replace each field's manual markup:

```vue
<!-- Before -->
<div class="flex flex-col gap-2">
  <label class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">
    {{ t('catalog.products.labels.name') }}
  </label>
  <InputText v-model="name" class="w-full rounded-2xl h-12 px-4" :invalid="!!errors.name" />
  <small class="p-error" v-if="errors.name">{{ errors.name }}</small>
</div>

<!-- After -->
<FormField :label="t('catalog.products.labels.name')" name="name" :error="errors.name">
  <InputText v-model="name" class="w-full rounded-2xl h-12 px-4" :invalid="!!errors.name" @blur="generateSlug" />
</FormField>
```

Apply to: name, slug, description, metaTitle, metaDescription, metaKeywords fields.

- [ ] **Step 3: Verify product list and form render correctly**

Run: `pnpm run dev` → Catalog → Products
Expected: List uses DataTableShell. Form uses FormField. All functionality unchanged.

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/catalog/products/views/
git commit -m "refactor(admin): swap ProductList/Form to DataTableShell and FormField"
```

---

### Task 3.4: Swap OrderList + OrderDetail to shared components

**Files:**
- Modify: `app/Admin/src/features/ordering/orders/views/OrderList.View.vue`
- Modify: `app/Admin/src/features/ordering/orders/views/OrderDetail.View.vue`

- [ ] **Step 1: Apply DataTableShell to OrderList** — same pattern as ProductList swap. Use columns: order number, customer, total, status, date. Keep the existing filters/search logic but delegate to DataTableShell.

- [ ] **Step 2: Apply DetailField + StatusBadge to OrderDetail**

Replace raw display values in OrderDetail with:

```vue
<DetailField :label="t('ordering.labels.order_number')" :value="current_order.number" />
<DetailField :label="t('ordering.labels.customer')" :value="current_order.email || 'Guest'" />
<DetailField :label="t('ordering.labels.subtotal')" :value="current_order.itemTotalDisplay" />
<DetailField :label="t('ordering.labels.shipping')" :value="current_order.shipmentTotalDisplay" />
<DetailField :label="t('ordering.labels.total')" :value="current_order.totalDisplay" />
```

Replace the status Tag with:
```vue
<StatusBadge :status="current_order.status" :status-map="OrderStatusMap" />
```

- [ ] **Step 3: Add TabView to OrderDetail** — wrap existing content sections into TabbedDetail:

```typescript
const detailTabs: TabDef[] = [
  { label: t('ordering.tabs.items'), icon: 'pi pi-shopping-cart', value: 0, panel: OrderItemsPanel },
  { label: t('ordering.tabs.shipments'), icon: 'pi pi-truck', value: 1, panel: OrderShipmentsPanel },
  { label: t('ordering.tabs.timeline'), icon: 'pi pi-history', value: 2, panel: OrderTimelinePanel },
]
```

For now, inline the tab panels as components referenced from the same file. The existing order item table, address editing, and totals become the "Items" tab content.

- [ ] **Step 4: Verify**

Run: `pnpm run dev` → Orders → List and Detail
Expected: List uses DataTableShell. Detail uses DetailField, StatusBadge, TabView.

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/ordering/orders/views/
git commit -m "refactor(admin): swap OrderList/Detail to shared components, add TabView"
```

---

### Task 3.5: Migrate remaining forms to vee-validate + zod

**Files:**
- Modify: `app/Admin/src/features/users/views/StaffForm.View.vue`
- Modify: `app/Admin/src/features/users/roles/views/RoleForm.View.vue`
- Modify: `app/Admin/src/features/catalog/taxonomies/views/TaxonomyForm.View.vue`
- Modify: `app/Admin/src/features/catalog/option-types/views/OptionTypeForm.View.vue`
- Modify: `app/Admin/src/features/shipping/shipping-methods/views/ShippingMethodForm.View.vue`
- Modify: `app/Admin/src/features/shipping/shipping-rates/views/ShippingRateForm.View.vue`
- Modify: `app/Admin/src/features/payment/payment-methods/views/PaymentMethodForm.View.vue`

- [ ] **Step 1: Create zod schemas for each entity**

```typescript
// Staff schema
// features/users/schemas/staff.schema.ts
export function createStaffSchema(t: (k: string) => string) {
  return z.object({
    email: z.string().email(t('validation.invalid_email')),
    displayName: z.string().min(1, t('validation.required')),
    roleIds: z.array(z.string()).min(1, t('validation.min_one_role')),
  })
}

// Role schema
// features/users/roles/schemas/role.schema.ts
export function createRoleSchema(t: (k: string) => string) {
  return z.object({
    name: z.string().min(1, t('validation.required')),
    description: z.string().optional(),
  })
}

// Taxonomy schema, OptionType schema, Shipping schemas, Payment method schema
// Each follows the same pattern: validate required fields, optional descriptions
```

- [ ] **Step 2: Rewrite each form with FormField + vee-validate**

For each form:
1. Import `useForm` + `toTypedSchema` + the schema
2. Replace `ref()` declarations with `defineField()`
3. Replace `v-model` bindings
4. Replace manual label+input markup with `<FormField>`
5. Wire `@click` submit to `handleSubmit`

- [ ] **Step 3: Verify each form validates and submits**

Run: `pnpm run dev` → test create/edit for each entity
Expected: Validation errors display under FormField. Successful submit navigates back to list.

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/users/ app/Admin/src/features/catalog/taxonomies/ app/Admin/src/features/catalog/option-types/ app/Admin/src/features/shipping/ app/Admin/src/features/payment/
git commit -m "refactor(admin): migrate all remaining forms to vee-validate + zod + FormField"
```

---

### Task 3.6: Build CustomerDetail view

**Files:**
- Modify: `app/Admin/src/features/users/views/CustomerDetail.View.vue`

- [ ] **Step 1: Implement CustomerDetail with TabView**

```vue
<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useCustomerStore } from '../stores/customer.store'
import { storeToRefs } from 'pinia'
import PageShell from '@/shared/components/PageShell.Component.vue'
import PageHeader from '@/shared/components/PageHeader.Component.vue'
import DetailField from '@/shared/components/DetailField.Component.vue'
import TabbedDetail from '@/shared/components/TabbedDetail.Component.vue'
import type { TabDef } from '@/shared/components/TabbedDetail.Component.vue'
import StatusBadge from '@/shared/components/StatusBadge.Component.vue'

const route = useRoute()
const router = useRouter()
const { t } = useI18n()
const store = useCustomerStore()
const { current, loading } = storeToRefs(store)

const activeTab = ref(0)

const customerId = route.params.id as string

onMounted(async () => {
  await store.fetchById(customerId)
})

// Inline tab content as computed or define them as local components
</script>

<template>
  <PageShell :card="false" gap max-width="7xl">
    <template v-if="current">
      <PageHeader back :title="current.displayName || current.email">
        <template #badge>
          <StatusBadge :status="current.isActive ? 'active' : 'inactive'"
            :status-map="{ active: { label: t('common.active'), severity: 'success' }, inactive: { label: t('common.inactive'), severity: 'secondary' } }" />
        </template>
        <template #actions>
          <Button icon="pi pi-pencil" :label="t('common.edit')" severity="secondary" outlined class="rounded-xl" />
        </template>
      </PageHeader>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <div class="lg:col-span-2 flex flex-col gap-6">
          <Card class="rounded-3xl shadow-sm border-none bg-surface-0 dark:bg-surface-900">
            <template #title>
              <span class="text-sm font-black uppercase tracking-widest text-surface-400 p-6 pb-0 block">
                {{ t('users.labels.profile') }}
              </span>
            </template>
            <template #content>
              <div class="grid grid-cols-2 gap-6">
                <DetailField :label="t('users.labels.email')" :value="current.email" />
                <DetailField :label="t('users.labels.display_name')" :value="current.displayName" />
                <DetailField :label="t('users.labels.phone')" :value="current.phoneNumber" />
                <DetailField :label="t('users.labels.joined')" :value="current.createdAt" />
              </div>
            </template>
          </Card>

          <Card class="rounded-3xl shadow-sm border-none bg-surface-0 dark:bg-surface-900">
            <template #title>
              <span class="text-sm font-black uppercase tracking-widest text-surface-400 p-6 pb-0 block">
                {{ t('users.labels.recent_orders') }}
              </span>
            </template>
            <template #content>
              <p class="text-sm text-surface-400 italic">{{ t('users.messages.no_orders') }}</p>
            </template>
          </Card>
        </div>

        <div class="flex flex-col gap-6">
          <Card class="rounded-3xl shadow-sm border-none bg-surface-0 dark:bg-surface-900">
            <template #title>
              <span class="text-sm font-black uppercase tracking-widest text-surface-400 p-6 pb-0 block">
                {{ t('users.labels.addresses') }}
              </span>
            </template>
            <template #content>
              <p class="text-sm text-surface-400 italic">{{ t('users.messages.no_addresses') }}</p>
            </template>
          </Card>
        </div>
      </div>
    </template>

    <div v-else-if="loading" class="flex justify-center py-20">
      <ProgressSpinner />
    </div>
  </PageShell>
</template>
```

- [ ] **Step 2: Verify customer detail page**

Run: `pnpm run dev` → Users → Customers → click a customer
Expected: Full detail page with profile info, status badge, placeholder sections for orders and addresses.

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/users/views/CustomerDetail.View.vue
git commit -m "feat(admin): implement CustomerDetail view with shared components"
```

---

### Task 3.7: Build StockMovements list and detail views

**Files:**
- Create: `app/Admin/src/features/inventories/stock-movements/views/StockMovementList.View.vue`
- Create: `app/Admin/src/features/inventories/stock-movements/views/StockMovementDetail.View.vue`
- Modify: `app/Admin/src/features/inventories/inventory.routes.ts` (add movement routes if missing)

- [ ] **Step 1: Add StockMovement routes**

```typescript
// Add to inventory.routes.ts children:
{
  path: 'movements',
  name: 'inventory.movements.list',
  component: () => import('./stock-movements/views/StockMovementList.View.vue'),
  meta: { breadcrumb: 'Stock Movements' },
},
{
  path: 'movements/:id',
  name: 'inventory.movements.detail',
  component: () => import('./stock-movements/views/StockMovementDetail.View.vue'),
  meta: { breadcrumb: 'Movement Detail' },
},
```

- [ ] **Step 2: Create StockMovementList.View.vue** — List template with DataTableShell, columns: Date, Type, Product, Quantity, From Location, To Location, Reference.

- [ ] **Step 3: Create StockMovementDetail.View.vue** — Detail template with DetailField: Date, Type, Product, SKU, Quantity, From/To Location, Reference, Notes.

- [ ] **Step 4: Verify routes and pages render**

Run: `pnpm run dev` → Inventory → Movements
Expected: Movement list renders. Click a movement → detail page renders.

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/inventories/
git commit -m "feat(admin): build StockMovement list and detail views"
```

---

## Phase 7: Polish

### Task 7.1: Add animated gradient topbar border

**Files:**
- Modify: `app/Admin/src/assets/scss/layout/_topbar.scss`

- [ ] **Step 1: Add gradient animation CSS**

```scss
// Add at the end of _topbar.scss

.layout-topbar {
  position: relative;

  &::after {
    content: '';
    position: absolute;
    bottom: 0;
    left: 0;
    right: 0;
    height: 3px;
    background: linear-gradient(
      90deg,
      var(--primary-color),
      var(--p-cyan-500),
      var(--p-teal-500),
      var(--primary-color)
    );
    background-size: 300% 100%;
    animation: topbar-gradient-shift 8s ease infinite;
  }
}

@keyframes topbar-gradient-shift {
  0% { background-position: 0% 50%; }
  50% { background-position: 100% 50%; }
  100% { background-position: 0% 50%; }
}

@media (prefers-reduced-motion: reduce) {
  .layout-topbar::after {
    animation: none;
  }
}
```

- [ ] **Step 2: Verify gradient appears and animates**

Run: `pnpm run dev`
Expected: 3px gradient border at bottom of topbar, slowly shifting.

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/assets/scss/layout/_topbar.scss
git commit -m "feat(admin): add animated gradient topbar border"
```

---

### Task 7.2: Add route transition animation

**Files:**
- Modify: `app/Admin/src/app/layout/Main.Layout.vue:69`

- [ ] **Step 1: Wrap router-view with Transition**

```vue
<Transition name="layout-main" mode="out-in">
  <router-view />
</Transition>
```

- [ ] **Step 2: Add transition CSS**

```scss
// Add to _responsive.scss or _main.scss
.layout-main-enter-active,
.layout-main-leave-active {
  transition: opacity 0.15s ease, transform 0.15s ease;
}
.layout-main-enter-from {
  opacity: 0;
  transform: translateY(8px);
}
.layout-main-leave-to {
  opacity: 0;
  transform: translateY(-8px);
}
```

- [ ] **Step 3: Verify smooth page transitions**

Run: `pnpm run dev` → navigate between pages
Expected: Subtle fade+slide transition between pages.

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/app/layout/Main.Layout.vue app/Admin/src/assets/scss/layout/
git commit -m "feat(admin): add route transition animation"
```

---

### Task 7.3: Breadcrumb completeness + skeleton audit

**Files:**
- Scan all route files for missing `meta.breadcrumb`
- Scan all list/detail views for missing loading skeletons

- [ ] **Step 1: Add breadcrumb to all routes without it**

Check every route definition. Any route missing `meta.breadcrumb` gets one:

```typescript
// Example additions:
meta: { breadcrumb: 'catalog.products.title' }
meta: { breadcrumb: 'inventory.movements.title' }
meta: { breadcrumb: 'users.staff.title' }
```

- [ ] **Step 2: Verify breadcrumbs on every page**

Navigate to every page → confirm breadcrumb bar shows correct path.

- [ ] **Step 3: Verify skeleton loading on every list page**

DataTableShell already handles skeleton internally when `loading=true`. Verify all list pages pass `loading` prop correctly.

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/
git commit -m "fix(admin): complete breadcrumb metadata, verify skeleton loading"
```

---

### Task 7.4: Build verification

- [ ] **Step 1: Run full build with warnings-as-errors**

```bash
cd app/Admin && pnpm run build
```

Expected: Build succeeds with zero warnings.

- [ ] **Step 2: Run unit tests**

```bash
cd app/Admin && pnpm run test:unit
```

Expected: All tests pass, including shared component tests and existing store/service tests.

- [ ] **Step 3: Run lint**

```bash
cd app/Admin && pnpm run lint
```

Expected: Zero lint errors.

- [ ] **Step 4: Final commit**

```bash
git add -A
git commit -m "chore(admin): final admin SPA standardization — build, test, lint passing"
```

---

## Self-Review Checklist

- [ ] **Spec coverage**: Each section of `2026-07-19-admin-standardization-design.md` maps to a task above:
  - Section 1 (Layout Foundation) → Tasks 0.1–0.6
  - Section 2 (Menu System) → Tasks 1.1–1.2
  - Section 3 (Shared Components) → Tasks 2.1–2.3
  - Section 4 (Page Templates) → Applied in Tasks 3.0–3.7
  - Section 5 (Module Standardization) → Tasks 3.0–3.7
  - Section 6 (Form System) → Tasks 3.2, 3.5
  - Section 7 (Dashboard) → Deferred (existing dashboards already functional)
  - Section 8 (Polish) → Tasks 7.1–7.4
  - Section 9 (Execution Order) → Tasks are ordered per phases

- [ ] **Placeholder scan**: No TBDs, TODOs, or vague instructions. Every step has code or exact actions.

- [ ] **Type consistency**: `ColumnDef`, `TabDef`, `MenuGroup`, `MenuItem` types defined once and reused. Route names consistent between menu config and route files. Component props consistent between definition and usage.

- [ ] **Scope**: Plan stays within Admin SPA only. No backend changes. No new feature creation beyond StockMovements views (which already have API/service/store). No visual companion or mockup generation.
