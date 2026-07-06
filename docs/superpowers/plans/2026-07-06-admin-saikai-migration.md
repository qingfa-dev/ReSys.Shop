# Admin SPA — Saikai/PrimeVue Migration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Migrate `app/Admin/` from Vuetify 3 to PrimeVue 4 (Aura/Saikai), port all existing features, and add inventory/ordering/reports features from `app/ReSys.Admin/`.

**Architecture:** Keep current `app/ + features/ + shared/` layout. Layout and SCSS assets copied from ReSys.Admin. Data layer switches from composable-based fetching to Pinia stores. VeeValidate + Zod for forms.

**Tech Stack:** Vue 3.5 + TypeScript 6 + Vite 8 + PrimeVue 4 (Aura) + Tailwind v4 + Pinia 3 + VeeValidate 4 + Zod 3 + Axios + Chart.js

## Global Constraints

- PrimeVue 4 with Aura theme preset
- Pinia for ALL state — no TanStack Query
- VeeValidate `useForm` + Zod `toTypedSchema` for all forms
- Axios single instance per ReSys.Admin's `api.client.ts` pattern
- PrimeIcons (not `@mdi/font`)
- `.app-dark` class on `<html>` for dark mode
- Auto-import PrimeVue components via `unplugin-vue-components` + `PrimeVueResolver`
- All routes lazy-loaded, auth guard via `router.beforeEach`
- Feature routes exported from `{module}.routes.ts`, imported as children of layout route
- Views: `{name}.view.vue`, Components: `{name}.component.vue`, Layouts: `{name}.layout.vue`
- No cross-feature imports; `shared/` never imports from `features/`
- Each phase must pass: `pnpm test:unit && pnpm type-check && pnpm lint`

---

## Task 1: Swap Dependencies + PrimeVue Plugin

**Files:**
- Modify: `app/Admin/package.json`
- Modify: `app/Admin/vite.config.ts`
- Create: `app/Admin/src/app/plugins/primevue.ts`

- [ ] **Step 1: Update package.json**

Remove `vuetify`, `@mdi/font`, `vite-plugin-vuetify`. Add `primevue`, `@primeuix/themes`, `primeicons`, `vee-validate`, `@vee-validate/zod`, `zod` (v3), `tailwindcss-primeui`, `chart.js`, `jwt-decode`, `@primevue/auto-import-resolver`.

- [ ] **Step 2: Install dependencies**

Run: `cd app/Admin && pnpm install`

- [ ] **Step 3: Update vite.config.ts**

Remove `vuetify` import + plugin. Add `PrimeVueResolver`:
```ts
import { PrimeVueResolver } from '@primevue/auto-import-resolver'
// In plugins:
Components({ resolvers: [PrimeVueResolver()] }),
```

- [ ] **Step 4: Create `app/Admin/src/app/plugins/primevue.ts`**

```ts
import PrimeVue from 'primevue/config'
import Aura from '@primeuix/themes/aura'

export function createPrimeVue() {
  return PrimeVue.createApp({
    theme: {
      preset: Aura,
      options: {
        darkModeSelector: '.app-dark',
        transitionDuration: '0.2s',
      },
    },
    ripple: true,
  })
}
```

- [ ] **Step 5: Commit**

```bash
git add -A app/Admin/package.json app/Admin/vite.config.ts app/Admin/src/app/plugins/primevue.ts app/Admin/pnpm-lock.yaml
git commit -m "feat(admin): swap Vuetify deps for PrimeVue + create plugin"
```

---

## Task 2: Layout Shell (from ReSys.Admin)

**Files:** Copy 8 layout files + 1 composable + 1 component from ReSys.Admin.

Copy from `app/ReSys.Admin/src/layout/` to `app/Admin/src/app/layout/`:
- `main.layout.vue` — Shell with topbar, sidebar, router-view, footer, ConfirmDialog
- `topbar.layout.vue` — Logo, menu toggle, GlobalSearch, dark mode, configurator, user menu
- `sidebar.layout.vue` — Nav sidebar wrapper
- `menu.layout.vue` — Navigation tree
- `menu-item.layout.vue` — Recursive menu item
- `footer.layout.vue` — Copyright
- `configurator.layout.vue` — Theme configurator
- `composables/layout.composable.ts` — Reactive layout state
- `components/GlobalSearch.vue` — Popup AutoComplete search

Adapt all import paths from `@/layout/` → `@/app/layout/`.

- [ ] **Step 1-9: Copy each file**
- [ ] **Step 10: Commit**

```bash
git add app/Admin/src/app/layout/
git commit -m "feat(admin): add PrimeVue layout shell from ReSys.Admin"
```

---

## Task 3: SCSS + Tailwind Assets (from ReSys.Admin)

**Files:**
- Create: `app/Admin/src/assets/tailwind.css` (Tailwind v4 + tailwindcss-primeui + custom breakpoints + Inter font)
- Create: `app/Admin/src/assets/scss/main.scss` (manifest)
- Create: `app/Admin/src/assets/scss/abstracts/_mixins.scss`
- Create: `app/Admin/src/assets/scss/abstracts/variables/_common.scss`, `_dark.scss`, `_light.scss`
- Create: `app/Admin/src/assets/scss/base/_core.scss`, `_typography.scss`
- Create: `app/Admin/src/assets/scss/layout/_footer.scss`, `_main.scss`, `_menu.scss`, `_preloading.scss`, `_responsive.scss`, `_topbar.scss`, `_utils.scss`
- Create: `app/Admin/src/assets/scss/demo/demo.scss`, `code.scss`, `flags/flags.css`
- Delete: `app/Admin/src/assets/_variables.scss`, `_base.scss`, `_typography.scss`, `_utilities.scss`, `main.css`

Copy verbatim from `app/ReSys.Admin/src/assets/` to `app/Admin/src/assets/`.

- [ ] **Step 1: Copy tailwind.css**
- [ ] **Step 2: Copy all SCSS files**
- [ ] **Step 3: Delete old Vuetify SCSS assets**
- [ ] **Step 4: Commit**

---

## Task 4: Shared API Layer + Composables (from ReSys.Admin)

**Files:**
- Delete: old `shared/api/` (client.ts, errors.ts, envelope.ts, paged-result.ts, fetch-options.ts, file.client.ts, search.client.ts)
- Delete: old `shared/composables/` (use*.ts — all 10)
- Delete: old `shared/ui/` (all 15 App*.vue + index.ts)
- Delete: old `shared/stores/` (confirm.store.ts, toast.store.ts)
- Delete: old `shared/query/` (all files)
- Delete: old `shared/lib/` (all files)
- Delete: old `shared/types/` (all files)
- Delete: old `shared/config/` (all files)
- Create: `shared/api/api.client.ts`, `api.types.ts`, `api.utils.ts`, `api.file.service.ts`, `api.file.types.ts`
- Create: `shared/composables/toast.use.ts`, `api-error-handler.use.ts`, `file-preview.use.ts`, `formatter.use.ts`
- Create: `shared/components/breadcrumb.component.vue`, `ManagerWelcome.vue`
- Create: `shared/locales/locale.types.ts`, `general.locales.ts`
- Create: `shared/utils/query-builder.utils.ts`
- Create: `shared/services/search.service.ts`

Copy verbatim from `app/ReSys.Admin/src/shared/`.

- [ ] **Step 1: Copy shared/api files**
- [ ] **Step 2: Delete old shared/api files**
- [ ] **Step 3: Copy shared/composables**
- [ ] **Step 4: Delete old shared/composables**
- [ ] **Step 5: Copy shared/components, locales, utils, services**
- [ ] **Step 6: Delete old shared/ui, stores, query, lib, types, config**
- [ ] **Step 7: Commit**

---

## Task 5: App Bootstrap + Router + Auth Guard

**Files:**
- Modify: `app/Admin/src/app/main.ts`
- Modify: `app/Admin/src/app/App.vue`
- Modify: `app/Admin/src/app/router/index.ts`
- Delete: `app/Admin/src/app/router/routes.ts`
- Delete: `app/Admin/src/app/plugins/vuetify.ts`
- Delete: `app/Admin/src/app/stores/sidebar.store.ts`, `theme.store.ts`, `tenant.store.ts`
- Delete: `app/Admin/src/app/layout/AppShell.vue`, `AppTopbar.vue`, `AppSidebar.vue`, `AppFooter.vue`

- [ ] **Step 1: Rewrite `main.ts`**

```ts
import { createApp } from 'vue'
import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import ConfirmationService from 'primevue/confirmationservice'
import StyleClass from 'primevue/styleclass'
import Aura from '@primeuix/themes/aura'
import App from './App.vue'
import router from './router'
import { installAuthBootstrap } from './auth/auth-bootstrap'
import 'primeicons/primeicons.css'
import '@/assets/tailwind.css'
import '@/assets/scss/main.scss'

const app = createApp(App)
app.use(createPinia())
app.use(PrimeVue, { theme: { preset: Aura, options: { darkModeSelector: '.app-dark' } }, ripple: true })
app.use(ToastService)
app.use(ConfirmationService)
app.directive('styleclass', StyleClass)
installAuthBootstrap(app)
app.use(router)
app.mount('#app')
```

- [ ] **Step 2: Rewrite `App.vue`**

```vue
<script setup lang="ts">
import { RouterView } from 'vue-router'
import { watch } from 'vue'
import { useToast } from 'primevue/usetoast'
import { toastBus } from '@/shared/composables/toast.use'
const toast = useToast()
watch(toastBus, (newValue) => {
  if (newValue) { toast.add(newValue); toastBus.value = null }
})
</script>
<template>
  <router-view />
  <Toast />
</template>
```

- [ ] **Step 3: Rewrite `router/index.ts`**

```ts
import { createRouter, createWebHistory } from 'vue-router'
import AppLayout from '@/app/layout/main.layout.vue'
import { useAuthStore } from '@/features/auth/stores/auth.store'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/login', name: 'login', component: () => import('@/features/auth/views/login.view.vue'), meta: { public: true } },
    { path: '/', component: AppLayout, meta: { breadcrumb: 'navigation.home' }, children: [
      { path: '', name: 'home', redirect: { name: 'reports.dashboard' } },
      { path: 'profile', name: 'profile', component: () => import('@/features/auth/views/Profile.view.vue'), meta: { breadcrumb: 'My Profile' } },
    ]},
  ],
})

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()
  if (!to.meta.public && !authStore.isAuthenticated) return next('/login')
  if (to.path === '/login' && authStore.isAuthenticated) return next('/')
  next()
})

export default router
```

- [ ] **Step 4: Remove old files** (routes.ts, vuetify.ts, old stores, old layout files)
- [ ] **Step 5: Commit**

---

## Task 6: Auth Feature (Login + Profile)

**Files:** Copy from `app/ReSys.Admin/src/features/auth/`:
- `stores/auth.store.ts`, `services/auth.service.ts`, `schemas/auth.schema.ts`, `types/auth.types.ts`, `locales/auth.locales.ts`
- `views/login.view.vue`, `views/Profile.view.vue`
- `tests/auth.store.spec.ts`, `tests/auth.service.spec.ts`

Delete old `app/Admin/src/features/auth/api/`, `composables/`, `model/`, `index.ts`

- [ ] **Step 1: Copy types, schemas, services, stores**
- [ ] **Step 2: Copy views**
- [ ] **Step 3: Copy locales + tests**
- [ ] **Step 4: Delete old auth files**
- [ ] **Step 5: Run tests**

Run: `cd app/Admin && pnpm test:unit`

- [ ] **Step 6: Verify login page**

Run: `cd app/Admin && pnpm dev`
Open: `http://localhost:5173/login`

- [ ] **Step 7: Commit**

---

## Task 7: Catalog Feature (from ReSys.Admin)

**Files:** Copy from `app/ReSys.Admin/src/features/catalog/`:
- `catalog.routes.ts`
- `dashboard/` (4 files)
- `products/` (22 files — store, service, types, schemas, locales, 3 views, 8 components)
- `taxonomies/` (9 files — store, service, types, schemas, locales, 3 views)
- `taxonomies/taxa/` (12 files — store, service, types, schemas, locales, 3 views, 3 components)
- `option-types/` (9 files — store, service, types, schemas, locales, 3 views)
- `option-types/option-values/` (8 files — store, service, types, schemas, locales, 1 view)
- `property-types/` (9 files — store, service, types, schemas, locales, 2 views)

Delete old `app/Admin/src/features/catalog/api/`, `composables/`, `model/`, `index.ts`

- [ ] **Step 1: Copy routes**
- [ ] **Step 2: Copy dashboard**
- [ ] **Step 3: Copy products**
- [ ] **Step 4: Copy taxonomies + taxa**
- [ ] **Step 5: Copy option-types + option-values**
- [ ] **Step 6: Copy property-types**
- [ ] **Step 7: Delete old catalog files**
- [ ] **Step 8: Run type check**
- [ ] **Step 9: Commit**

---

## Task 8: Users + Roles + Permissions Feature (from ReSys.Admin)

**Files:** Copy from `app/ReSys.Admin/src/features/users/`:
- `users.routes.ts`, `roles.routes.ts`, `permissions.routes.ts`
- `stores/user.store.ts`, `services/*.service.ts` (3), `types/user.types.ts`, `locales/user.locales.ts`
- `views/` (5: admin-user-list, staff-form, staff-detail, customer-list, customer-detail)
- `components/` (3: UserSecurityManager, UserRoleManager, UserPermissionManager)
- `roles/views/` (3: role-list, role-form, role-permissions-manager)
- `permissions/views/` (1: permission-list)

Delete old `app/Admin/src/features/identity/` and `app/Admin/src/features/roles/`.

- [ ] **Step 1: Copy all users files**
- [ ] **Step 2: Delete old identity/ + roles/**
- [ ] **Step 3: Type check + tests**
- [ ] **Step 4: Commit**

---

## Task 9: Location Feature (Port Current Admin to PrimeVue)

**Files:** Port current Admin's location feature from Vuetify to PrimeVue + Pinia stores.

Create: `services/country.service.ts`, `services/state.service.ts`
Create: `stores/country.store.ts`, `stores/state.store.ts`
Create: `types/country.types.ts`, `types/state.types.ts`
Create: `schemas/country.schema.ts`, `schemas/state.schema.ts`
Create: `views/CountryList.view.vue`, `views/CountryForm.view.vue`, `views/StateList.view.vue`, `views/StateForm.view.vue`

Delete old `app/Admin/src/features/location/api/`, `model/`.

- [ ] **Step 1: Create Pinia stores + services**
- [ ] **Step 2: Create views with PrimeVue DataTable + Dialog**
- [ ] **Step 3: Delete old location files**
- [ ] **Step 4: Commit**

---

## Task 10: Profile Feature (Port Current Admin to PrimeVue)

**Files:** Same pattern as Location — port from Vuetify to PrimeVue + Pinia.

Create: `services/profile.service.ts`, `stores/profile.store.ts`, `types/profile.types.ts`, `views/Profile.view.vue`
Delete old `app/Admin/src/features/profile/api/`, `model/`, `index.ts`

- [ ] **Step 1: Create store + service**
- [ ] **Step 2: Create view with PrimeVue components**
- [ ] **Step 3: Delete old profile files**
- [ ] **Step 4: Commit**

---

## Task 11: Inventory Feature (NEW — from ReSys.Admin)

**Files:** Copy from `app/ReSys.Admin/src/features/inventories/`:
- `inventory.routes.ts`, `stores/inventory.store.ts`, `services/inventory.service.ts`, `types/inventory.types.ts`, `locales/inventory.locales.ts`
- `views/` (8: StockItemList, InventoryUnitList, StockLocationManager, StockLocationList, StockLocationForm, StockTransferList, StockTransferForm, StockTransferDetail)
- `components/` (3: StockMovementTimeline, StockAdjustmentDialog, LocationSelector)

- [ ] **Step 1: Copy all files**
- [ ] **Step 2: Type check**
- [ ] **Step 3: Commit**

---

## Task 12: Ordering + Fulfillment Feature (NEW — from ReSys.Admin)

**Files:** Copy from `app/ReSys.Admin/src/features/ordering/`:
- `ordering.routes.ts`, `stores/order.store.ts`, `services/order.service.ts`, `types/order.types.ts`, `locales/order.locales.ts`
- `views/` (3: order-list, order-form, order-detail)
- `components/` (4: AddressDialog, ItemDialog, RefundDialog, ShipmentDialog)
- `fulfillment/` (4 files: store, service, tests, 1 view)

- [ ] **Step 1: Copy all files**
- [ ] **Step 2: Type check**
- [ ] **Step 3: Commit**

---

## Task 13: Reports Feature (NEW — from ReSys.Admin)

**Files:** Copy from `app/ReSys.Admin/src/features/reports/`:
- `reports.routes.ts`, `stores/report.store.ts`, `services/report.service.ts`, `types/report.types.ts`
- `views/dashboard.view.vue` (chart.js dashboard)

- [ ] **Step 1: Copy all files**
- [ ] **Step 2: Type check + tests**
- [ ] **Step 3: Commit**

---

## Task 14: Cleanup + Verification

- [ ] **Step 1: Verify no Vuetify references remain**

Run: `rg "vuetify|v-|@mdi" app/Admin/src/`

- [ ] **Step 2: Verify no old shared references**

Run: `rg "from '@/shared/ui|from '@/shared/query|from '@/shared/lib|from '@/shared/stores|from '@/shared/types|from '@/shared/config'" app/Admin/src/`

- [ ] **Step 3: Remove .gitkeep files**: `find app/Admin/src -name '.gitkeep' -delete`

- [ ] **Step 4: Full build + test**: `cd app/Admin && pnpm build && pnpm lint && pnpm test:unit`

- [ ] **Step 5: Final commit**: `git add -A app/Admin/src/ && git commit -m "chore(admin): cleanup, finalize PrimeVue migration"`

---

## Dependency Graph

```
Task 1 (deps) → Task 2 (layout) → Task 5 (bootstrap)
Task 3 (SCSS) ────────────────────────┘
Task 4 (shared) ───────────────────────┘
                                      ↓
                                  Task 6 (auth) ← gate: login page works
                                      ↓
              ┌──────────────────────┬┴─────────────────────┐
         Task 7 (catalog)       Task 8 (users)         Task 9 (location)
         Task 11 (inventory)    Task 12 (ordering)     Task 10 (profile)
         Task 13 (reports)
                                      ↓
                               Task 14 (cleanup)
```
