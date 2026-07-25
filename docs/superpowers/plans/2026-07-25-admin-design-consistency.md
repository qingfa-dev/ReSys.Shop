# Admin SPA — Design Consistency Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Standardize all 52 admin pages to consistent Sakai/PrimeVue design via shared layout components (ListLayout, DetailLayout, AppCard), fix 10 inconsistency categories, and enforce Tailwind-only grid, i18n, and button patterns.

**Architecture:** Create 3 shared layout components that wrap all pages — ListLayout (list pages with toolbar/table/states), DetailLayout (form pages with header/body/footer), AppCard (unified card with padding variants). Fix existing shared components (FormActions, PageHeader, TableToolbar) to match. Then apply to all 9 feature modules page-by-page.

**Tech Stack:** Vue 3.5 + TypeScript 6, PrimeVue 5 + Aura preset, Tailwind v4, vue-i18n 11.4

## Global Constraints

- No new npm packages — reuse existing primevue, @primevue/themes, tailwindcss, vue-i18n
- No direct module-to-module imports — cross-module via shared components only
- All Pinia stores expose state as readonly() refs
- `vue-tsc --noEmit` passes with zero errors after all changes
- Tailwind CSS grid only (`grid grid-cols-* gap-*`) — no PrimeFlex `grid`/`col-*`
- All buttons use `<Button>` component — no raw `<button class="p-button">`
- All user-facing strings use `t()` (vue-i18n) — no hardcoded English
- All PageHeader instances include `subtitle` prop with i18n key
- All route navigation uses exported `ROUTE` constants
- All catch blocks include `console.error(err)`
- Standard card padding is `p-5` (20px) for AppCard and FormActions

---

### Task 1: Create AppCard shared component

**Files:**
- Create: `app/Admin/src/shared/components/layout/AppCard.vue`
- Modify: `app/Admin/src/shared/components/index.ts`

**Interfaces:**
- Produces: `<AppCard padding="sm|md|lg" as="div|section|article">` component

- [ ] **Step 1: Create AppCard component**

```vue
<!-- app/Admin/src/shared/components/layout/AppCard.vue -->
<script setup lang="ts">
defineProps<{
  padding?: 'sm' | 'md' | 'lg'
  as?: string
}>()

const { padding = 'md', as = 'div' } = defineProps<{
  padding?: 'sm' | 'md' | 'lg'
  as?: string
}>()
</script>

<template>
  <component
    :is="as"
    class="rounded-border border border-surface-200 bg-white dark:border-surface-700 dark:bg-surface-900"
    :class="{
      'p-3': padding === 'sm',
      'p-5': padding === 'md',
      'p-6': padding === 'lg',
    }"
  >
    <slot />
  </component>
</template>
```

- [ ] **Step 2: Register AppCard in barrel export**

Read `app/Admin/src/shared/components/index.ts` and add:
```ts
export { default as AppCard } from './layout/AppCard.vue'
```

- [ ] **Step 3: Verify**

Run: `cd app/Admin && npx vue-tsc --noEmit`
Expected: PASS with zero errors

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/shared/components/layout/AppCard.vue app/Admin/src/shared/components/index.ts
git commit -m "feat: add AppCard shared component with sm/md/lg padding variants"
```

---

### Task 2: Create ListLayout shared component

**Files:**
- Create: `app/Admin/src/shared/components/layout/ListLayout.vue`
- Modify: `app/Admin/src/shared/components/index.ts`

**Interfaces:**
- Produces: `<ListLayout>` with slots `#header`, `#toolbar`, `#default`, `#bulk-actions`

- [ ] **Step 1: Create ListLayout component**

```vue
<!-- app/Admin/src/shared/components/layout/ListLayout.vue -->
<script setup lang="ts">
</script>

<template>
  <div class="flex flex-col gap-4">
    <div v-if="$slots.header">
      <slot name="header" />
    </div>
    <div v-if="$slots.toolbar">
      <slot name="toolbar" />
    </div>
    <div class="flex flex-1 flex-col">
      <slot />
    </div>
    <div v-if="$slots['bulk-actions']">
      <slot name="bulk-actions" />
    </div>
  </div>
</template>
```

- [ ] **Step 2: Register in barrel export**

```ts
export { default as ListLayout } from './layout/ListLayout.vue'
```

- [ ] **Step 3: Verify**

Run: `cd app/Admin && npx vue-tsc --noEmit`
Expected: PASS

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/shared/components/layout/ListLayout.vue app/Admin/src/shared/components/index.ts
git commit -m "feat: add ListLayout shared component with header/toolbar/content/bulk slots"
```

---

### Task 3: Create DetailLayout shared component

**Files:**
- Create: `app/Admin/src/shared/components/layout/DetailLayout.vue`
- Modify: `app/Admin/src/shared/components/index.ts`

**Interfaces:**
- Produces: `<DetailLayout :saving="boolean">` with slots `#header`, `#actions`, `#default`, `#sub-entities`, `#footer`

- [ ] **Step 1: Create DetailLayout component**

```vue
<!-- app/Admin/src/shared/components/layout/DetailLayout.vue -->
<script setup lang="ts">
defineProps<{
  saving?: boolean
}>()
</script>

<template>
  <div class="flex flex-col gap-4">
    <div v-if="$slots.header">
      <slot name="header" />
    </div>
    <div v-if="$slots.actions" class="flex items-center justify-end gap-2">
      <slot name="actions" />
    </div>
    <div class="flex flex-1 flex-col gap-4">
      <slot />
      <div v-if="$slots['sub-entities']">
        <slot name="sub-entities" />
      </div>
    </div>
    <div v-if="$slots.footer">
      <slot name="footer" :saving="saving" />
    </div>
  </div>
</template>
```

- [ ] **Step 2: Register in barrel export**

```ts
export { default as DetailLayout } from './layout/DetailLayout.vue'
```

- [ ] **Step 3: Verify**

Run: `cd app/Admin && npx vue-tsc --noEmit`
Expected: PASS

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/shared/components/layout/DetailLayout.vue app/Admin/src/shared/components/index.ts
git commit -m "feat: add DetailLayout shared component with header/actions/body/sub-entities/footer slots"
```

---

### Task 4: Fix FormActions negative margin

**Files:**
- Modify: `app/Admin/src/shared/components/forms/FormActions.vue`

**Interfaces:**
- Consumes: AppCard default padding = `p-5` (20px). FormActions must use `-mx-5` to match.

- [ ] **Step 1: Read current FormActions**

Read `app/Admin/src/shared/components/forms/FormActions.vue` to find the `-mx-6` class.

- [ ] **Step 2: Replace `-mx-6` with `-mx-5`**

Replace `-mx-6` with `-mx-5` in the root element's class binding.
Also verify `px-6` → `px-5` and `py-4` stays.

- [ ] **Step 3: Verify**

Run: `cd app/Admin && npx vue-tsc --noEmit`
Expected: PASS

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/shared/components/forms/FormActions.vue
git commit -m "fix: change FormActions negative margin from -mx-6 to -mx-5 to match AppCard p-5"
```

---

### Task 5: Add mandatory subtitle prop to PageHeader

**Files:**
- Modify: `app/Admin/src/shared/components/layout/PageHeader.vue`

**Interfaces:**
- Consumes: Existing PageHeader props
- Produces: PageHeader now requires `subtitle` prop (string, i18n key)

- [ ] **Step 1: Read current PageHeader**

Read `app/Admin/src/shared/components/layout/PageHeader.vue` to find current props definition.

- [ ] **Step 2: Add subtitle prop as required**

Make `subtitle` a required string prop (not optional):
```ts
defineProps<{
  title: string
  subtitle: string
  icon?: string
  back?: { label: string; to: string }
}>()
```

- [ ] **Step 3: Render subtitle in template**

Add subtitle rendering below the title:
```html
<p v-if="subtitle" class="text-sm text-surface-500 dark:text-surface-400">
  {{ subtitle }}
</p>
```

- [ ] **Step 4: Verify**

Run: `cd app/Admin && npx vue-tsc --noEmit`
Expected: ERRORS (pages that don't pass subtitle yet — will be fixed in module tasks).

This is expected — subsequent tasks will add subtitle to each page.

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/shared/components/layout/PageHeader.vue
git commit -m "feat: make subtitle a required prop on PageHeader"
```

---

### Task 6: Fix DataTable wrapper for consistent card appearance

**Files:**
- Modify: `app/Admin/src/shared/components/data/DataTable.vue`

**Interfaces:**
- Produces: DataTable wrapper now matches AppCard visual style

- [ ] **Step 1: Read current DataTable wrapper**

Read `app/Admin/src/shared/components/data/DataTable.vue` to find the root wrapper classes.

- [ ] **Step 2: Add bg-white and dark mode classes**

Find the root element classes and ensure they include:
```
rounded-border border border-surface-200 bg-white dark:border-surface-700 dark:bg-surface-900 overflow-hidden
```
If `bg-white` or dark mode counterparts are missing, add them.

- [ ] **Step 3: Verify**

Run: `cd app/Admin && npx vue-tsc --noEmit`
Expected: PASS

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/shared/components/data/DataTable.vue
git commit -m "fix: add bg-white and dark mode classes to DataTable wrapper for AppCard consistency"
```

---

### Task 7: Delete unused DetailDrawer and FilterPanel stubs

**Files:**
- Delete: `app/Admin/src/shared/components/overlays/DetailDrawer.vue`
- Delete: `app/Admin/src/shared/components/overlays/FilterPanel.vue`
- Modify: `app/Admin/src/shared/components/index.ts`

- [ ] **Step 1: Remove barrel export entries**

Read `app/Admin/src/shared/components/index.ts` and remove:
```ts
export { default as DetailDrawer } from './overlays/DetailDrawer.vue'
export { default as FilterPanel } from './overlays/FilterPanel.vue'
```

- [ ] **Step 2: Delete files**

```bash
rm app/Admin/src/shared/components/overlays/DetailDrawer.vue
rm app/Admin/src/shared/components/overlays/FilterPanel.vue
```

- [ ] **Step 3: Verify**

Run: `cd app/Admin && npx vue-tsc --noEmit`
Expected: PASS

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/shared/components/index.ts
git rm app/Admin/src/shared/components/overlays/DetailDrawer.vue app/Admin/src/shared/components/overlays/FilterPanel.vue
git commit -m "chore: remove unused DetailDrawer and FilterPanel stub components"
```

---

### Task 8: Fix reports DashboardPage — replace raw table with DataTable

**Files:**
- Modify: `app/Admin/src/features/reports/pages/DashboardPage.vue`

**Interfaces:**
- Consumes: Shared DataTable, StatCard, ListLayout
- Produces: DashboardPage uses DataTable for the monthly performance table

- [ ] **Step 1: Read current DashboardPage**

Read `app/Admin/src/features/reports/pages/DashboardPage.vue` to find the raw `<table>` element and surrounding structure.

- [ ] **Step 2: Replace raw table with DataTable**

Replace the raw `<table>` block with:
```html
<DataTable :rows="monthlyData" :total-records="monthlyData.length">
  <Column field="month" :header="t('reports.dashboard.columns.month')" />
  <Column field="orders" :header="t('reports.dashboard.columns.orders')" />
  <Column field="revenue" :header="t('reports.dashboard.columns.revenue')" />
  <Column field="avgOrderValue" :header="t('reports.dashboard.columns.avgOrder')" />
</DataTable>
```

- [ ] **Step 3: Wrap page in ListLayout**

Replace the root `<div>` with:
```html
<ListLayout>
  <template #header>
    <PageHeader
      :title="t('reports.dashboard.title')"
      :subtitle="t('reports.dashboard.subtitle')"
      :icon="route.meta?.icon as string"
    />
  </template>
  <template #default>
    <!-- existing stat cards and charts -->
  </template>
</ListLayout>
```

- [ ] **Step 4: Fix hardcoded strings to i18n**

Replace all hardcoded English strings in the template with `t('...')` calls. Check the `<script>` imports to add `useI18n`.

- [ ] **Step 5: Verify**

Run: `cd app/Admin && npx vue-tsc --noEmit`
Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add app/Admin/src/features/reports/pages/DashboardPage.vue
git commit -m "fix: replace raw table with DataTable in reports Dashboard, wrap in ListLayout, i18n all strings"
```

---

### Task 9: Fix catalog module — migrate to ListLayout/DetailLayout/AppCard

**Files:**
- Modify: `app/Admin/src/features/catalog/pages/ProductListPage.vue`
- Modify: `app/Admin/src/features/catalog/pages/ProductDetailPage.vue`
- Modify: `app/Admin/src/features/catalog/pages/VariantListPage.vue`
- Modify: `app/Admin/src/features/catalog/pages/VariantDetailPage.vue`
- Modify: `app/Admin/src/features/catalog/pages/OptionTypeListPage.vue`
- Modify: `app/Admin/src/features/catalog/pages/OptionTypeDetailPage.vue`
- Modify: `app/Admin/src/features/catalog/pages/TaxonomyListPage.vue`
- Modify: `app/Admin/src/features/catalog/pages/TaxonomyDetailPage.vue`
- Modify: `app/Admin/src/features/catalog/pages/DashboardPage.vue`
- Modify: `app/Admin/src/features/catalog/components/ProductForm.vue`
- Modify: `app/Admin/src/features/catalog/components/TaxonForm.vue`
- Modify: `app/Admin/src/features/catalog/components/OptionValueForm.vue`

**Interfaces:**
- Consumes: ListLayout, DetailLayout, AppCard, PageHeader (with required subtitle)

- [ ] **Step 1: Fix ProductListPage**

Read the current file. Wrap in `<ListLayout>` with proper slots:
- `#header`: `<PageHeader :title="t('catalog.products.title')" :subtitle="t('catalog.products.subtitle')" :icon="..." />`
- `#toolbar`: Move `<TableToolbar>` into this slot
- `#default`: Keep DataTable + state guards

Remove the root `<div>` wrapper. Add `const { t } = useI18n()` to script. Add `import { ROUTE } from '../routes'` and replace any bare route strings.

- [ ] **Step 2: Fix ProductDetailPage**

Wrap in `<DetailLayout :saving="saving">`:
- `#header`: `<PageHeader>` with back button, proper title/subtitle/icon
- `#actions`: Show Edit button in view mode
- `#default`: Wrap form in `<AppCard>` with `<LoadingSkeleton>/<ErrorState>/form grid>`
- `#sub-entities`: Sub-entity AppCards for variants, prices, images, classifications
- `#footer`: `<FormActions>` when not in view mode

Use Tailwind grid: `<div class="grid grid-cols-12 gap-4">` for form fields. Replace any PrimeFlex `col-*` with Tailwind equivalents.

- [ ] **Step 3: Fix VariantListPage**

Same pattern as ProductListPage — wrap in `<ListLayout>`.

- [ ] **Step 4: Fix VariantDetailPage**

Same pattern as ProductDetailPage — wrap in `<DetailLayout>`. Use AppCard for form + option value/price/image sub-entities.

- [ ] **Step 5: Fix OptionTypeListPage**

Wrap in `<ListLayout>`.

- [ ] **Step 6: Fix OptionTypeDetailPage**

Wrap in `<DetailLayout>`. Sub-entity: OptionValueManager inside AppCard.

- [ ] **Step 7: Fix TaxonomyListPage**

Wrap in `<ListLayout>`.

- [ ] **Step 8: Fix TaxonomyDetailPage**

Wrap in `<DetailLayout>`. Sub-entity: TaxonManager (MPTT-depth-indented DataTable) inside AppCard.

- [ ] **Step 9: Fix DashboardPage**

Wrap in `<ListLayout>`. No table — stat cards and charts in `#default` slot.

- [ ] **Step 10: Fix ProductForm (PrimeFlex grid → Tailwind grid)**

Find all `class="grid"` (PrimeFlex) containers and replace with `class="grid grid-cols-12 gap-4"`. Replace `col-6` → `col-span-full sm:col-span-6`, `col-12` → `col-span-full`. Replace `class="card"` with `<AppCard>`.

- [ ] **Step 11: Fix TaxonForm and OptionValueForm**

Replace `class="card"` with `<AppCard>`. Ensure error toasts are already working (from bug fix batch 3).

- [ ] **Step 12: Verify**

Run: `cd app/Admin && npx vue-tsc --noEmit`
Expected: PASS

- [ ] **Step 13: Commit**

```bash
git add app/Admin/src/features/catalog/
git commit -m "fix(catalog): migrate all 9 pages to ListLayout/DetailLayout/AppCard, Tailwind grid, i18n subtitles"
```

---

### Task 10: Fix inventory module — migrate to ListLayout/DetailLayout/AppCard

**Files:**
- Modify: `app/Admin/src/features/inventory/pages/DashboardPage.vue`
- Modify: `app/Admin/src/features/inventory/pages/StockListPage.vue`
- Modify: `app/Admin/src/features/inventory/pages/StockItemDetailPage.vue`
- Modify: `app/Admin/src/features/inventory/pages/LocationListPage.vue`
- Modify: `app/Admin/src/features/inventory/pages/LocationDetailPage.vue`
- Modify: `app/Admin/src/features/inventory/pages/MovementListPage.vue`
- Modify: `app/Admin/src/features/inventory/pages/TransferListPage.vue`
- Modify: `app/Admin/src/features/inventory/pages/TransferDetailPage.vue`
- Modify: `app/Admin/src/features/inventory/pages/StockReservationListPage.vue`
- Modify: `app/Admin/src/features/inventory/components/StockItemForm.vue`
- Modify: `app/Admin/src/features/inventory/components/TransferForm.vue`
- Modify: `app/Admin/src/features/inventory/components/StockLocationForm.vue`

**Interfaces:**
- Consumes: ListLayout, DetailLayout, AppCard

- [ ] **Step 1: Fix all 9 inventory pages**

For each page file:
1. Read the current structure
2. List pages → wrap in `<ListLayout>` with `#header` (PageHeader + subtitle), `#toolbar` (TableToolbar), `#default` (DataTable + states)
3. Detail pages → wrap in `<DetailLayout :saving="saving">` with `#header`, `#actions`, `#default` (AppCard + form grid), `#footer` (FormActions)
4. Dashboard → wrap in `<ListLayout>` with stat cards in `#default`
5. Add `const { t } = useI18n()` if missing
6. Add subtitle i18n key: `inventory.stocks.subtitle`, `inventory.locations.subtitle`, etc.
7. Import `ROUTE` from routes and replace bare string route names
8. Replace `class="card"` with `<AppCard>`

- [ ] **Step 2: Fix hardcoded "Stock Items" title in StockListPage**

Replace hardcoded `"Stock Items"` with `t('inventory.stocks.title')`.

- [ ] **Step 3: Fix form components (PrimeFlex grid → Tailwind grid)**

For StockItemForm, TransferForm, StockLocationForm:
Replace `class="grid"` (PrimeFlex) → `class="grid grid-cols-12 gap-4"`.
Replace `col-N` → Tailwind equivalents.
Replace `class="card"` → `<AppCard>`.

- [ ] **Step 4: Verify**

Run: `cd app/Admin && npx vue-tsc --noEmit`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/inventory/
git commit -m "fix(inventory): migrate all 9 pages to ListLayout/DetailLayout/AppCard, Tailwind grid, i18n"
```

---

### Task 11: Fix ordering module — migrate to ListLayout/DetailLayout/AppCard

**Files:**
- Modify: `app/Admin/src/features/ordering/pages/DashboardPage.vue`
- Modify: `app/Admin/src/features/ordering/pages/OrderListPage.vue`
- Modify: `app/Admin/src/features/ordering/pages/OrderDetailPage.vue`
- Modify: `app/Admin/src/features/ordering/pages/FulfillmentQueuePage.vue`
- Modify: `app/Admin/src/features/ordering/components/OrderForm.vue`

**Interfaces:**
- Consumes: ListLayout, DetailLayout, AppCard

- [ ] **Step 1: Fix all ordering pages**

Apply same patterns as Tasks 9-10:
- List pages → ListLayout
- Detail pages → DetailLayout
- Form components → AppCard + Tailwind grid
- Add subtitle i18n keys
- Use ROUTE constants
- Add `useI18n` import where missing

- [ ] **Step 2: Verify**

Run: `cd app/Admin && npx vue-tsc --noEmit`
Expected: PASS

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/ordering/
git commit -m "fix(ordering): migrate all 4 pages to ListLayout/DetailLayout/AppCard, Tailwind grid, i18n subtitles"
```

---

### Task 12: Fix payment module — migrate to ListLayout/DetailLayout/AppCard

**Files:**
- Modify: `app/Admin/src/features/payment/pages/PaymentListPage.vue`
- Modify: `app/Admin/src/features/payment/pages/PaymentDetailPage.vue`
- Modify: `app/Admin/src/features/payment/pages/PaymentMethodListPage.vue`
- Modify: `app/Admin/src/features/payment/pages/PaymentMethodDetailPage.vue`
- Modify: `app/Admin/src/features/payment/components/PaymentMethodForm.vue`

**Interfaces:**
- Consumes: ListLayout, DetailLayout, AppCard

- [ ] **Step 1: Fix all payment pages**

Apply same patterns:
- ListLayout for list pages
- DetailLayout for detail pages (lifecycle buttons in `#actions` slot: capture/void/refund)
- AppCard for forms
- Tailwind grid
- i18n subtitles
- ROUTE constants

- [ ] **Step 2: Verify**

Run: `cd app/Admin && npx vue-tsc --noEmit`
Expected: PASS

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/payment/
git commit -m "fix(payment): migrate all 4 pages to ListLayout/DetailLayout/AppCard, Tailwind grid, i18n"
```

---

### Task 13: Fix shipping module — migrate to ListLayout/DetailLayout/AppCard

**Files:**
- Modify: `app/Admin/src/features/shipping/pages/ShippingMethodListPage.vue`
- Modify: `app/Admin/src/features/shipping/pages/ShippingMethodDetailPage.vue`
- Modify: `app/Admin/src/features/shipping/pages/ShippingRateListPage.vue`
- Modify: `app/Admin/src/features/shipping/pages/ShippingRateDetailPage.vue`
- Modify: `app/Admin/src/features/shipping/components/ShippingMethodForm.vue`
- Modify: `app/Admin/src/features/shipping/components/ShippingRateForm.vue`

**Interfaces:**
- Consumes: ListLayout, DetailLayout, AppCard

- [ ] **Step 1: Fix all shipping pages**

Apply same patterns. Detail pages include activate/deactivate toggle in `#actions` slot.

- [ ] **Step 2: Verify**

Run: `cd app/Admin && npx vue-tsc --noEmit`
Expected: PASS

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/shipping/
git commit -m "fix(shipping): migrate all 4 pages to ListLayout/DetailLayout/AppCard, Tailwind grid, i18n"
```

---

### Task 14: Fix location module — migrate to ListLayout/DetailLayout/AppCard

**Files:**
- Modify: `app/Admin/src/features/location/pages/CountryListPage.vue`
- Modify: `app/Admin/src/features/location/pages/CountryDetailPage.vue`
- Modify: `app/Admin/src/features/location/pages/StateListPage.vue`
- Modify: `app/Admin/src/features/location/pages/StateDetailPage.vue`
- Modify: `app/Admin/src/features/location/components/CountryForm.vue`
- Modify: `app/Admin/src/features/location/components/StateForm.vue`

**Interfaces:**
- Consumes: ListLayout, DetailLayout, AppCard

- [ ] **Step 1: Fix hardcoded strings in CountryForm**

Replace `"Create Country"`, `"Edit:"`, `"Country Details"`, `"Save Country"` with i18n keys:
`t('location.countries.create')`, `t('location.countries.edit')`, etc.

- [ ] **Step 2: Fix all location pages**

Apply same patterns. Replace PrimeFlex grid in CountryForm.

- [ ] **Step 3: Verify**

Run: `cd app/Admin && npx vue-tsc --noEmit`
Expected: PASS

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/location/
git commit -m "fix(location): migrate all 4 pages to ListLayout/DetailLayout/AppCard, Tailwind grid, i18n hardcoded strings"
```

---

### Task 15: Fix profile module — migrate to ListLayout/DetailLayout/AppCard

**Files:**
- Modify: `app/Admin/src/features/profile/pages/ProfilePage.vue`
- Modify: `app/Admin/src/features/profile/pages/AddressListPage.vue`
- Modify: `app/Admin/src/features/profile/pages/AddressDetailPage.vue`
- Modify: `app/Admin/src/features/profile/components/AddressForm.vue`

**Interfaces:**
- Consumes: ListLayout, DetailLayout, AppCard

- [ ] **Step 1: Fix all profile pages**

Apply same patterns. AddressForm already uses ROUTE constants (fixed in bug batch 3).

- [ ] **Step 2: Verify**

Run: `cd app/Admin && npx vue-tsc --noEmit`
Expected: PASS

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/profile/
git commit -m "fix(profile): migrate all 3 pages to ListLayout/DetailLayout/AppCard, Tailwind grid, i18n"
```

---

### Task 16: Fix users module — migrate to ListLayout/DetailLayout/AppCard

**Files:**
- Modify: `app/Admin/src/features/users/pages/StaffListPage.vue`
- Modify: `app/Admin/src/features/users/pages/StaffDetailPage.vue`
- Modify: `app/Admin/src/features/users/pages/CustomerListPage.vue`
- Modify: `app/Admin/src/features/users/pages/CustomerDetailPage.vue`
- Modify: `app/Admin/src/features/users/pages/RoleListPage.vue`
- Modify: `app/Admin/src/features/users/pages/RoleDetailPage.vue`
- Modify: `app/Admin/src/features/users/pages/PermissionListPage.vue`
- Modify: `app/Admin/src/features/users/pages/PermissionDetailPage.vue`
- Modify: `app/Admin/src/features/users/components/UserForm.vue`
- Modify: `app/Admin/src/features/users/components/RoleForm.vue`

**Interfaces:**
- Consumes: ListLayout, DetailLayout, AppCard

- [ ] **Step 1: Fix hardcoded "Staff" title and "Manage staff accounts" subtitle**

Replace with `t('users.staff.title')` and `t('users.staff.subtitle')`.

- [ ] **Step 2: Fix all users pages**

Apply same patterns. StaffDetailPage has sub-entities: UserRoleManager, UserPermissionManager in `#sub-entities` slot. RoleDetailPage has RolePermissionManager.

- [ ] **Step 3: Verify**

Run: `cd app/Admin && npx vue-tsc --noEmit`
Expected: PASS

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/users/
git commit -m "fix(users): migrate all 8 pages to ListLayout/DetailLayout/AppCard, Tailwind grid, i18n subtitles"
```

---

### Task 17: Final validation — full build and lint

**Files:**
- No file changes — verification only.

- [ ] **Step 1: Run TypeScript check**

```bash
cd app/Admin && npx vue-tsc --noEmit
```
Expected: PASS with zero errors.

- [ ] **Step 2: Run lint**

```bash
cd app/Admin && pnpm run lint
```
Expected: PASS with zero errors.

- [ ] **Step 3: Run existing tests to verify no regressions**

```bash
cd app/Admin && pnpm run test:unit
```
Expected: All existing tests still pass.

- [ ] **Step 4: Grep for violations**

```bash
# No PrimeFlex grid usage
cd app/Admin/src && rg 'class="grid"' features/ --files-with-matches && echo "FOUND - fix these" || echo "CLEAN"

# No bare button classes
cd app/Admin/src && rg '<button class="p-button' features/ --files-with-matches && echo "FOUND - fix these" || echo "CLEAN"

# No hardcoded English in templates (heuristic: look for common patterns not in t())
cd app/Admin/src && rg 'title="[A-Z]' features/ --files-with-matches && echo "FOUND - review these" || echo "CLEAN"
```

- [ ] **Step 5: Commit**

```bash
git commit -m "chore: final validation passed - vue-tsc clean, no PrimeFlex grid, no raw buttons" --allow-empty
```
Note: if no changes, use `--allow-empty`. If fixes were needed, add files first.
