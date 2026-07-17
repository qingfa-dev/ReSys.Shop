# Admin Layout Consolidation — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Eliminate double breadcrumb, unify Card wrapping, and standardize page spacer convention across ~35 Admin views via a shared PageShell component.

**Architecture:** Introduce `PageShell.Component.vue` — a wrapper that renders `<div class="p-6"><Card><slot /></Card></div>` with optional `maxWidth`, `card`, and `gap` props. Remove all 28 AppBreadcrumb imports+renders from views (layout already provides it). Apply PageShell + PageHeader to every standard list/form/detail view.

**Tech Stack:** Vue 3, PrimeVue 4, Tailwind 4, TypeScript

## Global Constraints

- `Main.Layout.vue:68` renders `<AppBreadcrumb />` before `<router-view />` — the single source of truth
- No view renders `<AppBreadcrumb />` locally after this plan
- All standard views use `<PageShell>` as outermost wrapper
- All standard views use `<PageHeader>` for the page header
- Manager views (`flex flex-col h-full`) get breadcrumb removal only, no PageShell
- Full-screen views (Login, error pages) not touched
- Dialog-based forms (StateForm, CountryForm) not touched
- Build must remain clean; tests must remain 23/25 passed

---

### Task 1: Create PageShell component

**Files:**
- Create: `app/Admin/src/shared/components/PageShell.Component.vue`

**Interfaces:**
- Produces: `<PageShell>` component with props `maxWidth?` (`'2xl' | '4xl' | '6xl' | '7xl' | 'none'`), `card?` (boolean, default `true`), `gap?` (boolean, default `false`); single default slot for page content

- [ ] **Step 1: Write the component**

```vue
<script setup lang="ts">
withDefaults(defineProps<{
  maxWidth?: '2xl' | '4xl' | '6xl' | '7xl' | 'none'
  card?: boolean
  gap?: boolean
}>(), {
  maxWidth: 'none',
  card: true,
  gap: false,
})
</script>
```

```vue
<template>
  <div
    class="p-6"
    :class="[
      maxWidth !== 'none' ? `max-w-${maxWidth} mx-auto` : '',
      !card && gap ? 'flex flex-col gap-6' : '',
    ]"
  >
    <Card v-if="card">
      <slot />
    </Card>
    <slot v-else />
  </div>
</template>
```

- [ ] **Step 2: Build to verify no TypeScript errors**

```bash
cd app/Admin && pnpm run build
```

- [ ] **Step 3: Run tests to verify no regressions**

```bash
cd app/Admin && pnpm run test:unit
```

Expected: 23/25 passed (5 pre-existing i18n store failures)

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/shared/components/PageShell.Component.vue
git commit -m "feat(admin): add PageShell layout wrapper component"
```

---

### Task 2: Standard list views — breadcrumb + PageShell + PageHeader (9 files)

**Files modified:**
1. `app/Admin/src/features/users/views/AdminUserList.View.vue`
2. `app/Admin/src/features/users/roles/views/RoleList.View.vue`
3. `app/Admin/src/features/ordering/views/OrderList.View.vue`
4. `app/Admin/src/features/ordering/fulfillment/views/FulfillmentQueue.View.vue`
5. `app/Admin/src/features/inventories/views/StockItemList.View.vue`
6. `app/Admin/src/features/inventories/views/StockTransferList.View.vue`
7. `app/Admin/src/features/users/permissions/views/PermissionList.View.vue`
8. `app/Admin/src/features/location/views/CountryList.View.vue`
9. `app/Admin/src/features/location/views/StateList.View.vue`

**Interfaces:**
- Consumes: `PageShell` (from Task 1), `PageHeader` (pre-existing at `@/shared/components/PageHeader.Component.vue`)
- Produces: Each view now emits consistent `<PageShell><PageHeader>...</PageHeader>...</PageShell>` structure

**Pattern: Shared template transformation**

All 9 views follow the same pattern. Before:
```vue
<template>
  <div class="p-6">
    <AppBreadcrumb />                                        <!-- removed -->
    <Card class="rounded-3xl shadow-sm border-none bg-surface-0 dark:bg-surface-900 overflow-hidden">
      <template #title>
        <div class="flex items-center justify-between p-4">
          <div class="flex flex-col gap-1">
            <div class="flex items-center gap-3">
              <span class="text-xl font-bold">{{ t('...') }}</span>
              <Badge :value="totalRecords" severity="info" />
            </div>
            <span class="text-sm text-surface-500">{{ t('...') }}</span>
          </div>
          <Button ... />
        </div>
      </template>
      <template #content>
        <DataTable ... />
      </template>
    </Card>
  </div>
</template>
```

After:
```vue
<PageShell>
  <PageHeader :title="t('...')" :description="t('...')">
    <template #badge>
      <Badge :value="totalRecords" severity="info" />
    </template>
    <template #actions>
      <Button ... />
    </template>
  </PageHeader>
  <DataTable ... />
</PageShell>
```

- [ ] **Step 1: Add PageShell import to each of the 9 views**

Import line to add (after existing imports, before store/const declarations):
```
import PageShell from '@/shared/components/PageShell.Component.vue'
```

- [ ] **Step 2: Remove AppBreadcrumb import from each view**

Delete the line:
```
import AppBreadcrumb from '@/shared/components/Breadcrumb.Component.vue';
```

Views with AppBreadcrumb import to remove:
1. `AdminUserList.View.vue` — line 9
2. `RoleList.View.vue` — line 8
3. `OrderList.View.vue` — line 15
4. `StockItemList.View.vue` — line 7
5. `StockTransferList.View.vue` — line 8

Views without AppBreadcrumb import (skip removal, just add PageShell):
1. `FulfillmentQueue.View.vue`
2. `PermissionList.View.vue`
3. `CountryList.View.vue`
4. `StateList.View.vue`

- [ ] **Step 3: Transform each view's template**

For each view, three changes to the template block:

**(a)** Remove `<AppBreadcrumb />` line

**(b)** Replace the outer `<div class="p-6">` + `<Card class="...">` wrapper with `<PageShell>`:
- Delete: `<div class="p-6">`, `<AppBreadcrumb />`, `<Card class="rounded-3xl shadow-sm border-none bg-surface-0 dark:bg-surface-900 overflow-hidden">`, `<template #title>`, `<template #content>`, closing `</template>`, closing `</Card>`, closing `</div>`
- Add: `<PageShell>` as outermost, `</PageShell>` as closing

**(c)** Replace the inline header inside `<Card #title>` with `<PageHeader>`:
- Delete the `<div class="flex items-center justify-between p-4">...</div>` content inside `#title`
- Add `<PageHeader>` after `<PageShell>` with title, description, badge slot, actions slot

- [ ] **Step 4: Build to verify**

```bash
cd app/Admin && pnpm run build
```

Fix any broken import references (each view must import `PageShell` and `PageHeader` if not already). Common missing imports will be caught by the build.

- [ ] **Step 5: Run tests**

```bash
cd app/Admin && pnpm run test:unit
```

Expected: 23/25 passed.

- [ ] **Step 6: Commit**

```bash
git add app/Admin/src/features/
git commit -m "refactor(admin): apply PageShell+PageHeader to 9 standard list views, remove duplicate breadcrumb"
```

---

### Task 3: Border-div list views — Card wrapping via PageShell (5 files)

**Files modified:**
1. `app/Admin/src/features/catalog/option-types/option-values/views/OptionValueList.View.vue`
2. `app/Admin/src/features/catalog/option-types/views/OptionTypeList.View.vue`
3. `app/Admin/src/features/catalog/taxonomies/taxa/views/TaxonList.View.vue`
4. `app/Admin/src/features/inventories/views/InventoryUnitList.View.vue`
5. `app/Admin/src/features/catalog/taxonomies/views/TaxonomyList.View.vue`

**Interfaces:**
- Consumes: `PageShell` (Task 1), `PageHeader` (pre-existing)
- Produces: These 5 views now wrap DataTable in Card via PageShell, replacing the raw border-div

**Current pattern (4 of 5 views):**
```vue
<template>
  <div class="p-6">
    <AppBreadcrumb />
    ...custom header markup...
    <div class="overflow-hidden border shadow-sm bg-surface-0 dark:bg-surface-900 rounded-2xl border-surface-100 dark:border-surface-800">
      <DataTable ... />
    </div>
  </div>
</template>
```

**Target pattern:**
```vue
<template>
  <PageShell>
    <PageHeader ... />
    <DataTable ... />
  </PageShell>
</template>
```

- [ ] **Step 1: OptionValueList.View.vue** — already has PageHeader, just needs PageShell + breadcrumb removal

Remove import line 18:
```
import AppBreadcrumb from '@/shared/components/Breadcrumb.Component.vue';
```

Add import (after line 17 or equivalent):
```
import PageShell from '@/shared/components/PageShell.Component.vue'
```

Replace template — remove `<div class="p-6">` outer + `<AppBreadcrumb />` line + the border-div wrapping DataTable. The existing `<PageHeader>` stays. Wrap everything in `<PageShell>`.

Edit: replace `<div class="p-6">` with `<PageShell>`, remove `<AppBreadcrumb />` line, remove `<div class="overflow-hidden border shadow-sm...">` and its closing `</div>` (these are the border-div wrapping DataTable). Replace closing `</div>` (the p-6 closer) with `</PageShell>`.

- [ ] **Step 2: OptionTypeList.View.vue** — same pattern, needs full PageShell + PageHeader

Remove import line 11:
```
import AppBreadcrumb from '@/shared/components/Breadcrumb.Component.vue';
```

Add imports:
```
import PageShell from '@/shared/components/PageShell.Component.vue'
import PageHeader from '@/shared/components/PageHeader.Component.vue'
```

Replace template: `<div class="p-6">` → `<PageShell>`, remove `<AppBreadcrumb />`, replace the inline header div (title + description + badge + button) with `<PageHeader>` using appropriate i18n keys, remove the border-div wrapping DataTable, close with `</PageShell>`.

- [ ] **Step 3: TaxonList.View.vue** — same pattern

Remove import line 9:
```
import AppBreadcrumb from '@/shared/components/Breadcrumb.Component.vue';
```

Add imports for `PageShell` and `PageHeader`. Same template transformation as OptionTypeList.

- [ ] **Step 4: InventoryUnitList.View.vue** — same pattern, `max-w-7xl mx-auto` in outer wrapper

Remove import line 7:
```
import AppBreadcrumb from '@/shared/components/Breadcrumb.Component.vue';
```

Add imports for `PageShell` and `PageHeader`. Same template transformation. Note: `max-w-7xl` becomes `maxWidth="7xl"` prop on PageShell.

- [ ] **Step 5: TaxonomyList.View.vue** — uses Sakai `<div class="card">`, has dead import

Remove DEAD import line 17:
```
import AppBreadcrumb from '@/shared/components/Breadcrumb.Component.vue';
```

Add imports for `PageShell` and `PageHeader`. Replace `<div class="card">` + inline header with `<PageShell><PageHeader>...</PageHeader>`. Replace the border-div wrapping DataTable (it uses `overflow-hidden border ...`) with direct DataTable inside PageShell.

- [ ] **Step 6: Build + test + commit**

```bash
cd app/Admin && pnpm run build && pnpm run test:unit
git add app/Admin/src/features/
git commit -m "refactor(admin): wrap 5 border-div list views in PageShell+PageHeader, remove breadcrumb"
```

---

### Task 4: Form views — PageShell + PageHeader (5 files)

**Files modified:**
1. `app/Admin/src/features/users/views/StaffForm.View.vue`
2. `app/Admin/src/features/users/roles/views/RoleForm.View.vue`
3. `app/Admin/src/features/users/roles/views/RolePermissionsManager.View.vue`
4. `app/Admin/src/features/ordering/views/OrderForm.View.vue`
5. `app/Admin/src/features/inventories/views/StockTransferForm.View.vue`

**Interfaces:**
- Consumes: `PageShell`, `PageHeader`
- Produces: Each form view wraps in `<PageShell maxWidth="4xl"><PageHeader back>...form...</PageShell>`

**Pattern for form views with `<Card>` as root (StaffForm, RoleForm, RolePermissionsManager):**

Before:
```vue
<template>
    <Card>
        <div class="flex items-center gap-4 mb-8">
            <Button icon="pi pi-arrow-left" text rounded @click="router.back()" />
            <div>
                <h1 class="text-3xl font-black uppercase tracking-tighter text-surface-900 dark:text-surface-0">
                    {{ isEditMode ? 'Edit Staff' : 'Invite Staff' }}
                </h1>
                <p class="text-surface-500">
                    {{ isEditMode ? 'Update staff...' : 'Create a new...' }}
                </p>
            </div>
        </div>
        <form ...>...</form>
    </Card>
</template>
```

After:
```vue
<template>
    <PageShell maxWidth="4xl">
        <PageHeader
            :title="isEditMode ? 'Edit Staff' : 'Invite Staff'"
            :description="isEditMode ? 'Update staff member details and permissions.' : 'Create a new staff account.'"
            back
        />
        <form ...>...</form>
    </PageShell>
</template>
```

- [ ] **Step 1: StaffForm.View.vue**

Add imports:
```
import PageShell from '@/shared/components/PageShell.Component.vue'
import PageHeader from '@/shared/components/PageHeader.Component.vue'
```

Replace template: `<Card>` → `<PageShell maxWidth="4xl">`, remove inline header div + its `<h1>` + `<p>`, add `<PageHeader back :title="..." :description="..." />`, close with `</PageShell>`. Keep the `<form>` and all form fields exactly as-is.

- [ ] **Step 2: RoleForm.View.vue**

Same pattern as StaffForm. Add PageShell + PageHeader imports. Replace `<Card>` wrapper with `<PageShell maxWidth="4xl">`, replace inline header with `<PageHeader back>`. Keep `<Message severity="warn">` if present and form fields as-is.

- [ ] **Step 3: RolePermissionsManager.View.vue**

Add PageShell + PageHeader imports. Replace `<Card>` with `<PageShell maxWidth="4xl">`, replace inline header with `<PageHeader back title="Manage Permissions" description="..." />`. The save button goes into `<template #actions>` on PageHeader.

- [ ] **Step 4: OrderForm.View.vue** — currently uses `<div class="p-6 max-w-4xl mx-auto">`, no Card

Add imports:
```
import PageShell from '@/shared/components/PageShell.Component.vue'
import PageHeader from '@/shared/components/PageHeader.Component.vue'
```

Replace template: `<div class="p-6 max-w-4xl mx-auto">` → `<PageShell maxWidth="4xl">`. Replace inline header with `<PageHeader back title="Create Manual Order" description="..." />`. Wrap the grid content in `<div class="grid grid-cols-1 lg:grid-cols-3 gap-8">` — PageShell already provides the Card, so keep inner Cards as-is (they're semantic Cards for Customer Details, Line Items, etc.). Close with `</PageShell>`.

- [ ] **Step 5: StockTransferForm.View.vue** — has breadcrumb + `<div class="p-6 max-w-2xl mx-auto">`

Remove import line 7:
```
import AppBreadcrumb from '@/shared/components/Breadcrumb.Component.vue';
```

Add PageShell + PageHeader imports. Replace `<div class="p-6 max-w-2xl mx-auto">` → `<PageShell maxWidth="2xl">`. Remove `<AppBreadcrumb />`. Replace inline header with `<PageHeader back>`.

- [ ] **Step 6: Build + test + commit**

```bash
cd app/Admin && pnpm run build && pnpm run test:unit
git add app/Admin/src/features/
git commit -m "refactor(admin): apply PageShell+PageHeader to 5 form views, remove breadcrumb"
```

---

### Task 5: Detail views + remaining list views (6 files)

**Files modified:**
1. `app/Admin/src/features/users/views/StaffDetail.View.vue`
2. `app/Admin/src/features/users/views/CustomerDetail.View.vue`
3. `app/Admin/src/features/ordering/views/OrderDetail.View.vue`
4. `app/Admin/src/features/inventories/views/StockTransferDetail.View.vue`
5. `app/Admin/src/features/users/views/CustomerList.View.vue`
6. `app/Admin/src/features/catalog/products/views/ProductList.View.vue`

**Interfaces:**
- Consumes: `PageShell`, `PageHeader`
- Produces: Detail views wrap in `<PageShell :card="false" gap maxWidth="6xl">`, list views wrap in `<PageShell>`

- [ ] **Step 1: StaffDetail.View.vue** — multi-card detail, breadcrumb + `<div class="p-6 max-w-6xl mx-auto">`

Remove import line 9:
```
import AppBreadcrumb from '@/shared/components/Breadcrumb.Component.vue';
```

Add PageShell + PageHeader imports. Replace `<div class="p-6 max-w-6xl mx-auto">` with `<PageShell :card="false" gap maxWidth="6xl">`. Remove `<AppBreadcrumb />`. Replace the inline header (back-button + h2 + Tag + p) with `<PageHeader back :title="user.fullName || 'Staff Member'" :description="user.email" />`. Keep `<Card>` wrapping tabs as-is (PageShell has `:card="false"`). Close with `</PageShell>`.

- [ ] **Step 2: CustomerDetail.View.vue** — same pattern as StaffDetail

Remove import line 9. Add PageShell + PageHeader imports. Same transformation as StaffDetail (PageShell with `:card="false" gap maxWidth="6xl"`, PageHeader with back button, keep inner Card+tab structure).

- [ ] **Step 3: OrderDetail.View.vue** — already has no breadcrumb, uses `<div class="flex flex-col gap-6 p-6">`

Add PageShell + PageHeader imports. Replace wrapper: `<div class="flex flex-col gap-6 p-6">` → `<PageShell :card="false" gap>`. Replace inline header (back-button + h1) with `<PageHeader back :title="'Order ' + current_order?.number" />`. Keep existing inner Cards. Close with `</PageShell>`.

- [ ] **Step 4: StockTransferDetail.View.vue** — multi-card, breadcrumb + `<div class="p-6 max-w-6xl mx-auto">`

Remove import line 9. Add PageShell + PageHeader imports. Replace wrapper with `<PageShell :card="false" gap maxWidth="6xl">`. Remove `<AppBreadcrumb />`. Replace inline header with `<PageHeader back :title="transfer.referenceNumber">...actions...</PageHeader>`. Keep inner Card grid. Close with `</PageShell>`.

- [ ] **Step 5: CustomerList.View.vue** — breadcrumb INSIDE `<Card><template #content>`, no outer `<div class="p-6">`

Remove import line 8. Add PageShell + PageHeader imports. Replace template: `<Card><template #content><AppBreadcrumb />...header...` → `<PageShell><PageHeader ...>...header...</PageHeader>...DataTable in border-div...</PageShell>`. The DataTable currently sits inside a border-div (`overflow-hidden border shadow-sm...`). That border-div is removed — the Card from PageShell provides the border. Close with `</PageShell>`.

- [ ] **Step 6: ProductList.View.vue** — breadcrumb INSIDE `<Card><template #content>`, same pattern as CustomerList

Remove import line 17. Add PageShell import (PageHeader is already imported). Replace template: `<Card><template #content><AppBreadcrumb />...` → `<PageShell><PageHeader ...>...DataTable...</PageShell>`. Close with `</PageShell>`.

- [ ] **Step 7: Build + test + commit**

```bash
cd app/Admin && pnpm run build && pnpm run test:unit
git add app/Admin/src/features/
git commit -m "refactor(admin): apply PageShell+PageHeader to 4 detail views + 2 list views with internal breadcrumb"
```

---

### Task 6: Dashboard views + manager views + edge cases + final verification (13 files)

**Files modified:**
1. `app/Admin/src/features/catalog/dashboard/views/CatalogDashboard.View.vue`
2. `app/Admin/src/features/ordering/dashboard/views/OrderingDashboard.View.vue`
3. `app/Admin/src/features/inventories/dashboard/views/InventoryDashboard.View.vue`
4. `app/Admin/src/features/reports/views/Dashboard.View.vue`
5. `app/Admin/src/features/catalog/taxonomies/views/TaxonomyManager.View.vue`
6. `app/Admin/src/features/catalog/taxonomies/taxa/views/TaxonTreeManager.View.vue`
7. `app/Admin/src/features/catalog/option-types/views/OptionTypeManager.View.vue`
8. `app/Admin/src/features/inventories/views/StockLocationManager.View.vue`
9. `app/Admin/src/features/catalog/taxonomies/views/TaxonomyForm.View.vue`
10. `app/Admin/src/features/catalog/taxonomies/taxa/views/TaxonForm.View.vue`
11. `app/Admin/src/features/catalog/option-types/views/OptionTypeForm.View.vue`
12. `app/Admin/src/features/inventories/views/StockLocationForm.View.vue`
13. `app/Admin/src/features/catalog/products/views/ProductForm.View.vue`
14. `app/Admin/src/features/auth/views/Profile.View.vue`

**Interfaces:**
- Consumes: `PageShell`, `PageHeader`
- Produces: Dashboards wrapped in PageShell, manager views stripped of breadcrumb, dead imports removed, edge cases handled

- [ ] **Step 1: Dashboard views — wrap in PageShell**

Three files share the same pattern: `<div class="p-6"><div class="mb-8"><h2>...title...</h2><p>...desc...</p></div>...stat cards...</div>`.

For `CatalogDashboard.View.vue`, `OrderingDashboard.View.vue`, `InventoryDashboard.View.vue`:

Add import:
```
import PageShell from '@/shared/components/PageShell.Component.vue'
import PageHeader from '@/shared/components/PageHeader.Component.vue'
```

Replace `<div class="p-6">` with `<PageShell>`. Replace the inline `<div class="mb-8"><h2>...title...</h2><p>...desc...</p></div>` with `<PageHeader :title="t('...')" :description="t('...')" />`. Close outermost `</div>` with `</PageShell>`.

- [ ] **Step 2: Reports Dashboard.View.vue** — special, uses `<div class="flex flex-col gap-8">` with Card wrapper for header

Add PageShell import. Replace `<div class="flex flex-col gap-8">` with `<PageShell :card="false" gap>`. Remove the header `<Card>` wrapper and replace with `<PageHeader title="Dashboard" description="Real-time performance overview">...actions...</PageHeader>`. Close with `</PageShell>`.

- [ ] **Step 3: Manager views — breadcrumb removal only (4 files)**

For `TaxonomyManager.View.vue`, `TaxonTreeManager.View.vue`, `OptionTypeManager.View.vue`, `StockLocationManager.View.vue`:

Remove the AppBreadcrumb import line and `<AppBreadcrumb />` template line. No PageShell — these keep their `flex flex-col h-full` layout.

- [ ] **Step 4: Dead import removal (3 form views with `flex flex-col h-full overflow-hidden`)**

For `TaxonomyForm.View.vue`, `TaxonForm.View.vue`, `OptionTypeForm.View.vue`:

Remove the DEAD `import AppBreadcrumb ...` line. No other changes — these views already have their working layout without breadcrumb.

- [ ] **Step 5: StockLocationForm.View.vue** — conditional breadcrumb, special case

This view uses `<div :class="[hideHeader ? 'p-0' : 'p-6 max-w-4xl mx-auto']">` with conditional `<AppBreadcrumb />` inside `v-if="!hideHeader"`.

Remove import line 7. Remove the conditional `<AppBreadcrumb />` block (the breadcrumb is now provided by layout). Replace the conditional outer wrapper with `<PageShell maxWidth="4xl">` (the `p-0` case doesn't apply to outer wrapper anymore — it was for sub-view rendering which uses a different slot). Replace the inline header (inside `v-if="!hideHeader"`) with `<PageHeader back>`.

- [ ] **Step 6: ProductForm.View.vue** — already has PageHeader, needs breadcrumb removed + PageShell

Remove import line 10:
```
import AppBreadcrumb from '@/shared/components/Breadcrumb.Component.vue';
```

Add import:
```
import PageShell from '@/shared/components/PageShell.Component.vue'
```

Replace `<div class="p-6 max-w-6xl mx-auto">` with `<PageShell maxWidth="6xl">`. Remove `<AppBreadcrumb />`. Existing `<PageHeader>` stays. The `<Card class="border-none shadow-sm rounded-3xl...">` wrapping tabs is removed (PageShell provides Card). Close with `</PageShell>`.

- [ ] **Step 7: Profile.View.vue** — `<div class="space-y-8">`, has Card + ProgressSpinner, no breadcrumb

Add import:
```
import PageShell from '@/shared/components/PageShell.Component.vue'
```

Replace `<div class="space-y-8">` with `<PageShell :card="false" gap>`. Keep internal Cards as-is. Close with `</PageShell>`.

- [ ] **Step 8: Build + test**

```bash
cd app/Admin && pnpm run build && pnpm run test:unit
```

Expected: build clean, 23/25 tests passed.

- [ ] **Step 9: Verify acceptance criteria**

```bash
cd app/Admin
# 1. No AppBreadcrumb in features/
! grep -r "AppBreadcrumb" src/features/ --include="*.vue" --include="*.ts"
# 2. No border-div pattern
! grep -r "overflow-hidden border shadow-sm" src/features/ --include="*.vue"
# 3. No raw p-6 class on outermost div
# (This matches <div class="p-6"> as page wrapper — inner uses are OK)
# Manually verify by skimming git diff
git diff --stat
```

Expected outputs:
- `grep AppBreadcrumb` — no matches (or confirmation with `echo $?` returning 1)
- `grep overflow-hidden border shadow-sm` — no matches (this pattern is only in the old border-div, not in inner Card wrappers)
- `git diff --stat` shows ~35 files modified + 1 new file

- [ ] **Step 10: Final commit**

```bash
git add app/Admin/src/
git commit -m "refactor(admin): wrap dashboards+edge cases in PageShell, remove remaining breadcrumb references"
```

---

### Task 7: Self-review checklist for implementer

After all tasks complete, verify:

- [ ] `pnpm run build` passes clean (chunk size warnings OK)
- [ ] `pnpm run test:unit` passes 23/25 (same 5 i18n store test failures as before)
- [ ] Visual check: spot-check 3-5 views in browser — breadcrumb appears once, Card wraps content, header uses PageHeader
- [ ] No dead imports: `git grep "import AppBreadcrumb" app/Admin/src/features/` returns empty
- [ ] No `<div class="p-6">` as outermost page wrapper in features/ (PageShell handles it)
- [ ] No `<div class="card">` in features/ (TaxonomyList was the only one, now replaced)
