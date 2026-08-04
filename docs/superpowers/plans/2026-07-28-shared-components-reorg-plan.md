# Shared Components Reorganization Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Reorganize `app/Admin/src/shared/components/` into PrimeVue-aligned groups (layout, panel, data, overlay, form), delete 13 unused/demo files, inline 1 thin wrapper, fix FormSection slot bug.

**Architecture:** Rename 5 subdirectories to 5 new ones aligned with PrimeVue taxonomy. Replace vite aliases. Update ~39 import paths across routes.ts and 36 placeholder views. Delete Sakai demo widgets. No behavior changes — purely structural.

**Tech Stack:** Vue 3, TypeScript, Vite, Vitest

## Global Constraints

- Build must succeed with zero errors (`pnpm run build`)
- 357/357 unit tests must pass (`pnpm run test:unit -- run`)
- No behavior changes — only file moves, import path updates, deletions, and one slot-in-Card bug fix
- Test files move with their source components; test imports use relative `../Component.vue` (no alias used in tests)

---

### Task 1: Create new group directories and barrel files

**Files:**
- Create: `app/Admin/src/shared/components/layout/index.ts`
- Create: `app/Admin/src/shared/components/panel/index.ts`
- Create: `app/Admin/src/shared/components/data/index.ts`
- Create: `app/Admin/src/shared/components/overlay/index.ts`
- Create: `app/Admin/src/shared/components/form/index.ts`
- Create: `app/Admin/src/shared/components/messages/index.ts`
- Create: `app/Admin/src/shared/components/menu/index.ts`
- Create: `app/Admin/src/shared/components/file/index.ts`
- Create: `app/Admin/src/shared/components/button/index.ts`

**Interfaces:**
- Produces: 9 group directories ready for component moves; barrel files are empty stubs or placeholders

- [ ] **Step 1: Create directories and barrel files**

```bash
mkdir -p app/Admin/src/shared/components/{layout,panel,data,overlay,form,messages,menu,file,button}
mkdir -p app/Admin/src/shared/components/layout/__tests__
mkdir -p app/Admin/src/shared/components/panel/__tests__
```

- [ ] **Step 2: Write barrel files for the 5 populated groups**

```ts
// layout/index.ts
export { default as AppLayout } from './AppLayout.vue'
export { default as AppTopbar } from './AppTopbar.vue'
export { default as AppSidebar } from './AppSidebar.vue'
export { default as AppMenu } from './AppMenu.vue'
export { default as AppMenuItem } from './AppMenuItem.vue'
export { default as UserMenu } from './UserMenu.vue'
export { default as AppFooter } from './AppFooter.vue'
export { default as AppConfigurator } from './AppConfigurator.vue'
```

```ts
// panel/index.ts
export { default as PageShell } from './PageShell.vue'
export { default as PageHeading } from './PageHeading.vue'
export { default as StatCard } from './StatCard.vue'
export { default as DataTableCard } from './DataTableCard.vue'
export { default as EmptyState } from './EmptyState.vue'
export { default as ErrorPageShell } from './ErrorPageShell.vue'
export { default as AuthLayout } from './AuthLayout.vue'
```

```ts
// data/index.ts
export { default as FilterableDataTable } from './FilterableDataTable.vue'
export { default as CrudToolbar } from './CrudToolbar.vue'
export { default as StatusTag } from './StatusTag.vue'
export { default as RatingBadge } from './RatingBadge.vue'
```

```ts
// overlay/index.ts
export { default as ConfirmDialog } from './ConfirmDialog.vue'
```

```ts
// form/index.ts
export { default as FormField } from './FormField.vue'
export { default as FormSection } from './FormSection.vue'
```

- [ ] **Step 3: Write empty stub barrels for 4 empty groups**

```ts
// messages/index.ts — each empty group just exports nothing
export {}
```

```ts
// menu/index.ts
export {}
```

```ts
// file/index.ts
export {}
```

```ts
// button/index.ts
export {}
```

- [ ] **Step 4: Verify directories exist**

```bash
ls app/Admin/src/shared/components/layout/ app/Admin/src/shared/components/panel/ app/Admin/src/shared/components/data/ app/Admin/src/shared/components/overlay/ app/Admin/src/shared/components/form/
```

Expected: 5 directories shown, each containing `index.ts`.

---

### Task 2: Update Vite and TypeScript aliases

**Files:**
- Modify: `app/Admin/vite.config.ts:27-39`
- Modify: `app/Admin/tsconfig.app.json:15-19`

**Interfaces:**
- Consumes: new group directories from Task 1
- Produces: 5 new aliases (`@layout`, `@panel`, `@data`, `@overlay`, `@form`); 5 old aliases removed in both config files

- [ ] **Step 1: Replace the alias block in vite.config.ts**

Remove these 5 lines (lines 33-37):
```ts
      '@ui': fileURLToPath(new URL('./src/shared/components/ui', import.meta.url)),
      '@feedback': fileURLToPath(new URL('./src/shared/components/feedback', import.meta.url)),
      '@forms': fileURLToPath(new URL('./src/shared/components/forms', import.meta.url)),
      '@tables': fileURLToPath(new URL('./src/shared/components/tables', import.meta.url)),
      '@navigation': fileURLToPath(new URL('./src/shared/components/navigation', import.meta.url)),
```

Add these 5 new lines in their place:
```ts
      '@layout': fileURLToPath(new URL('./src/shared/components/layout', import.meta.url)),
      '@panel': fileURLToPath(new URL('./src/shared/components/panel', import.meta.url)),
      '@data': fileURLToPath(new URL('./src/shared/components/data', import.meta.url)),
      '@overlay': fileURLToPath(new URL('./src/shared/components/overlay', import.meta.url)),
      '@form': fileURLToPath(new URL('./src/shared/components/form', import.meta.url)),
```

- [ ] **Step 2: Replace the path block in tsconfig.app.json**

Remove these 5 lines (lines 15-19):
```json
      "@ui/*": ["./src/shared/components/ui/*"],
      "@feedback/*": ["./src/shared/components/feedback/*"],
      "@forms/*": ["./src/shared/components/forms/*"],
      "@tables/*": ["./src/shared/components/tables/*"],
      "@navigation/*": ["./src/shared/components/navigation/*"],
```

Add these 5 new lines in their place:
```json
      "@layout/*": ["./src/shared/components/layout/*"],
      "@panel/*": ["./src/shared/components/panel/*"],
      "@data/*": ["./src/shared/components/data/*"],
      "@overlay/*": ["./src/shared/components/overlay/*"],
      "@form/*": ["./src/shared/components/form/*"],
```

- [ ] **Step 3: Verify JSON syntax**

```bash
node -e "JSON.parse(require('fs').readFileSync('app/Admin/tsconfig.app.json','utf8')); console.log('JSON valid')"
```

---

### Task 3: Move layout group (8 components + 2 test files)

**Files:**
- Move (git mv): `navigation/` → `layout/` for all 8 components and 2 test files

**Interfaces:**
- Produces: `layout/` directory populated with components and tests; `navigation/` emptied (still has index.ts for now)

- [ ] **Step 1: Move layout shell components**

```bash
git mv app/Admin/src/shared/components/navigation/AppLayout.vue app/Admin/src/shared/components/layout/AppLayout.vue
git mv app/Admin/src/shared/components/navigation/AppTopbar.vue app/Admin/src/shared/components/layout/AppTopbar.vue
git mv app/Admin/src/shared/components/navigation/AppSidebar.vue app/Admin/src/shared/components/layout/AppSidebar.vue
git mv app/Admin/src/shared/components/navigation/AppMenu.vue app/Admin/src/shared/components/layout/AppMenu.vue
git mv app/Admin/src/shared/components/navigation/AppMenuItem.vue app/Admin/src/shared/components/layout/AppMenuItem.vue
git mv app/Admin/src/shared/components/navigation/UserMenu.vue app/Admin/src/shared/components/layout/UserMenu.vue
```

- [ ] **Step 2: Move remaining layout components from ui/**

```bash
git mv app/Admin/src/shared/components/ui/AppFooter.vue app/Admin/src/shared/components/layout/AppFooter.vue
git mv app/Admin/src/shared/components/ui/AppConfigurator.vue app/Admin/src/shared/components/layout/AppConfigurator.vue
```

- [ ] **Step 3: Move test files**

```bash
git mv app/Admin/src/shared/components/navigation/__tests__/AppMenu.spec.ts app/Admin/src/shared/components/layout/__tests__/AppMenu.spec.ts
git mv app/Admin/src/shared/components/navigation/__tests__/UserMenu.spec.ts app/Admin/src/shared/components/layout/__tests__/UserMenu.spec.ts
```

- [ ] **Step 4: Verify files are in layout/**

```bash
ls app/Admin/src/shared/components/layout/*.vue app/Admin/src/shared/components/layout/__tests__/*.spec.ts
```

Expected: 8 `.vue` files + 2 `.spec.ts` files listed.

---

### Task 4: Move panel group (7 components)

**Files:**
- Move (git mv): components from `ui/`, `tables/`, `feedback/`, `forms/` → `panel/`

**Interfaces:**
- Consumes: new `panel/` directory from Task 1
- Produces: 7 components in `panel/`

- [ ] **Step 1: Move panel components**

```bash
git mv app/Admin/src/shared/components/ui/PageShell.vue app/Admin/src/shared/components/panel/PageShell.vue
git mv app/Admin/src/shared/components/ui/PageHeading.vue app/Admin/src/shared/components/panel/PageHeading.vue
git mv app/Admin/src/shared/components/ui/StatCard.vue app/Admin/src/shared/components/panel/StatCard.vue
git mv app/Admin/src/shared/components/tables/DataTableCard.vue app/Admin/src/shared/components/panel/DataTableCard.vue
git mv app/Admin/src/shared/components/feedback/EmptyState.vue app/Admin/src/shared/components/panel/EmptyState.vue
git mv app/Admin/src/shared/components/feedback/ErrorPageShell.vue app/Admin/src/shared/components/panel/ErrorPageShell.vue
git mv app/Admin/src/shared/components/forms/AuthLayout.vue app/Admin/src/shared/components/panel/AuthLayout.vue
```

- [ ] **Step 2: Verify**

```bash
ls app/Admin/src/shared/components/panel/ | wc -l
```

Expected: 8 lines (7 `.vue` files + `index.ts`).

---

### Task 5: Move data group (4 components)

**Files:**
- Move (git mv): components from `tables/`, `ui/` → `data/`

- [ ] **Step 1: Move data components**

```bash
git mv app/Admin/src/shared/components/tables/FilterableDataTable.vue app/Admin/src/shared/components/data/FilterableDataTable.vue
git mv app/Admin/src/shared/components/tables/CrudToolbar.vue app/Admin/src/shared/components/data/CrudToolbar.vue
git mv app/Admin/src/shared/components/ui/StatusTag.vue app/Admin/src/shared/components/data/StatusTag.vue
git mv app/Admin/src/shared/components/ui/RatingBadge.vue app/Admin/src/shared/components/data/RatingBadge.vue
```

- [ ] **Step 2: Verify**

```bash
ls app/Admin/src/shared/components/data/*.vue | wc -l
```

Expected: 4.

---

### Task 6: Move overlay group (1 component)

**Files:**
- Move (git mv): `feedback/ConfirmDialog.vue` → `overlay/`

- [ ] **Step 1: Move overlay component**

```bash
git mv app/Admin/src/shared/components/feedback/ConfirmDialog.vue app/Admin/src/shared/components/overlay/ConfirmDialog.vue
```

- [ ] **Step 2: Verify**

```bash
ls app/Admin/src/shared/components/overlay/
```

Expected: `ConfirmDialog.vue` and `index.ts`.

---

### Task 7: Move form group (2 components + fix FormSection bug)

**Files:**
- Move: `forms/FormField.vue` → `form/FormField.vue`
- Move: `forms/FormSection.vue` → `form/FormSection.vue`
- Modify: `form/FormSection.vue` — fix slot-inside-Card bug

**Interfaces:**
- Produces: FormSection.vue with `<slot>` rendered inside `<Card>` (was outside)

- [ ] **Step 1: Move form components**

```bash
git mv app/Admin/src/shared/components/forms/FormField.vue app/Admin/src/shared/components/form/FormField.vue
git mv app/Admin/src/shared/components/forms/FormSection.vue app/Admin/src/shared/components/form/FormSection.vue
```

- [ ] **Step 2: Fix FormSection slot-inside-Card bug**

Open `app/Admin/src/shared/components/form/FormSection.vue`. The template currently has:
```html
<Card>
  <div class="flex flex-col gap-4">
    <div class="font-semibold text-xl">{{ title }}</div>
    <div v-if="description" class="text-muted-color mt-1">{{ description }}</div>
  </div>
</Card>
<slot />
```

Move the closing `</Card>` tag to AFTER `<slot />`:
```html
<Card>
  <div class="flex flex-col gap-4">
    <div class="font-semibold text-xl">{{ title }}</div>
    <div v-if="description" class="text-muted-color mt-1">{{ description }}</div>
  </div>
  <slot />
</Card>
```

- [ ] **Step 3: Verify fix**

```bash
cat app/Admin/src/shared/components/form/FormSection.vue
```

Expected: `<slot />` appears between `</div>` (the header div close) and `</Card>`.

---

### Task 8: Delete demo fluff and old component directories

**Files:**
- Delete: 11 demo/unused component files
- Delete: empty `navigation/`, `ui/`, `feedback/`, `forms/`, `tables/` directories

- [ ] **Step 1: Delete demo widgets (9 files)**

```bash
git rm app/Admin/src/shared/components/ui/HeroWidget.vue
git rm app/Admin/src/shared/components/ui/FeaturesWidget.vue
git rm app/Admin/src/shared/components/ui/StatsWidget.vue
git rm app/Admin/src/shared/components/ui/PricingWidget.vue
git rm app/Admin/src/shared/components/ui/HighlightsWidget.vue
git rm app/Admin/src/shared/components/ui/FooterWidget.vue
git rm app/Admin/src/shared/components/feedback/NotificationsWidget.vue
git rm app/Admin/src/shared/components/ui/TopbarWidget.vue
git rm app/Admin/src/shared/components/ui/BlockViewer.vue
```

- [ ] **Step 2: Delete unused domain components (2 files)**

```bash
git rm app/Admin/src/shared/components/ui/ProductCard.vue
git rm app/Admin/src/shared/components/ui/CountryFlag.vue
```

- [ ] **Step 3: Delete old barrel files (5 files)**

```bash
git rm app/Admin/src/shared/components/navigation/index.ts
git rm app/Admin/src/shared/components/ui/index.ts
git rm app/Admin/src/shared/components/feedback/index.ts
git rm app/Admin/src/shared/components/forms/index.ts
git rm app/Admin/src/shared/components/tables/index.ts
```

- [ ] **Step 4: Remove empty source directories**

```bash
rmdir app/Admin/src/shared/components/navigation/__tests__/
rmdir app/Admin/src/shared/components/navigation/ 2>/dev/null
rmdir app/Admin/src/shared/components/ui/ 2>/dev/null
rmdir app/Admin/src/shared/components/feedback/ 2>/dev/null
rmdir app/Admin/src/shared/components/forms/ 2>/dev/null
rmdir app/Admin/src/shared/components/tables/ 2>/dev/null
```

- [ ] **Step 5: Verify deletions**

```bash
ls app/Admin/src/shared/components/
```

Expected: Directories shown are only `layout`, `panel`, `data`, `overlay`, `form`, `messages`, `menu`, `file`, `button`.

---

### Task 9: Inline GradientCard into ErrorPageShell + remove FloatingConfigurator

**Files:**
- Modify: `panel/ErrorPageShell.vue`
- Delete: `feedback/GradientCard.vue` (already deleted in Task 8 since it's in `feedback/` which was cleaned)

**Wait — GradientCard is in `feedback/` which gets cleaned in Task 8. We need to inline it BEFORE deleting it. Let's do this inline + FloatingConfigurator removal together, BEFORE Task 8's directory cleanup.**

Actually, we already deleted `feedback/` contents. The correct order is: inline GradientCard's structure into ErrorPageShell first (while the file still exists), then remove the FloatingConfigurator import from ErrorPageShell, then delete the old files. So this task must run BEFORE Task 8.

Re-order: Task 9 → Task 8. The spec says this is fine — implementation order doesn't have to follow spec order.

**Revised ordering note:** Run this task BEFORE Task 8.

**Files:**
- Modify: `panel/ErrorPageShell.vue` (at this point, the file has been moved via git mv from `feedback/` to `panel/` in Task 4)
- Note: GradientCard.vue still exists at `feedback/GradientCard.vue` until Task 8 cleanup

**Interfaces:**
- Consumes: ErrorPageShell (at new `panel/` path), GradientCard (at old `feedback/` path)
- Produces: ErrorPageShell with inlined gradient card markup, no FloatingConfigurator import

- [ ] **Step 1: Read current GradientCard.vue to capture the markup**

```bash
cat app/Admin/src/shared/components/feedback/GradientCard.vue
```

Expected output (to confirm before inline):
```html
<template>
  <div style="border-radius: 56px; padding: 0.3rem; background: linear-gradient(180deg, color-mix(in srgb, var(--primary-color), transparent 60%), rgba(33, 150, 243, 0) 30%)">
    <div class="w-full border border-surface-200 dark:border-surface-700 rounded-border bg-surface-0 dark:bg-surface-900 py-20 px-8 sm:px-20 flex flex-col items-center">
      <slot></slot>
    </div>
  </div>
</template>
```

- [ ] **Step 2: Read current ErrorPageShell.vue template imports**

```bash
head -5 app/Admin/src/shared/components/panel/ErrorPageShell.vue
```

The import line to remove:
```ts
import GradientCard from '../feedback/GradientCard.vue'
```
Note: After the move to `panel/`, this import path is still `'../feedback/GradientCard.vue'` (relative from `panel/` to `feedback/`). We'll replace it with inlined markup and remove the import entirely.

Also look for:
```ts
import FloatingConfigurator from '../ui/FloatingConfigurator.vue'
```
Remove this import and the `<FloatingConfigurator />` element from the template.

- [ ] **Step 3: Inline GradientCard markup into ErrorPageShell template**

Replace `<GradientCard>` usage in the template with the direct `<div>` structure from Step 1:
```html
<div style="border-radius: 56px; padding: 0.3rem; background: linear-gradient(180deg, color-mix(in srgb, var(--primary-color), transparent 60%), rgba(33, 150, 243, 0) 30%)">
  <div class="w-full border border-surface-200 dark:border-surface-700 rounded-border bg-surface-0 dark:bg-surface-900 py-20 px-8 sm:px-20 flex flex-col items-center">
    <!-- ErrorPageShell's existing slot/default content goes here — just replace <GradientCard> and </GradientCard> with these two divs -->
```

And replace the closing `</GradientCard>` with `</div></div>`.

- [ ] **Step 4: Remove FloatingConfigurator and GradientCard imports from ErrorPageShell**

Remove these two import lines from the `<script setup>` block:
```ts
import GradientCard from './GradientCard.vue'
import FloatingConfigurator from '../ui/FloatingConfigurator.vue'
```
(Note: ErrorPageShell is now at `panel/` after Task 4's move, but we're removing both imports entirely so the old relative paths don't matter.)

Also remove `<FloatingConfigurator />` from the template (appears as a self-closing element near the bottom of the template).

Replace `<GradientCard>` and `</GradientCard>` in the template with the inline divs from Step 1.

---

### Task 10: Update routes.ts imports

**Files:**
- Modify: `app/Admin/src/app/router/routes.ts:2-4`

**Interfaces:**
- Consumes: new alias paths from Task 2
- Produces: routes.ts importing from `@layout`, `@panel` instead of `@navigation`, `@forms`, `@feedback`

- [ ] **Step 1: Replace the 3 import lines**

Replace lines 2-4:
```ts
import AppLayout from '@navigation/AppLayout.vue'
import AuthLayout from '@forms/AuthLayout.vue'
import ErrorPageShell from '@feedback/ErrorPageShell.vue'
```

With:
```ts
import AppLayout from '@layout/AppLayout.vue'
import AuthLayout from '@panel/AuthLayout.vue'
import ErrorPageShell from '@panel/ErrorPageShell.vue'
```

- [ ] **Step 2: Verify changes**

```bash
head -5 app/Admin/src/app/router/routes.ts
```

Expected: Lines 2-4 use `@layout`, `@panel` (x2).

---

### Task 11: Update 36 placeholder view imports

**Files:**
- Modify: 36 `.vue` files across 9 feature modules — each has one import to update

**The 36 files (all follow the same pattern):**
```
app/Admin/src/features/dashboard/views/DashboardPage.vue
app/Admin/src/features/catalog/views/ProductsList.vue
app/Admin/src/features/catalog/views/ProductDetail.vue
app/Admin/src/features/catalog/views/OptionTypesList.vue
app/Admin/src/features/catalog/views/OptionTypeDetail.vue
app/Admin/src/features/catalog/views/TaxonomiesList.vue
app/Admin/src/features/catalog/views/TaxonomyDetail.vue
app/Admin/src/features/identity/views/UsersList.vue
app/Admin/src/features/identity/views/UserDetail.vue
app/Admin/src/features/identity/views/RolesList.vue
app/Admin/src/features/identity/views/RoleDetail.vue
app/Admin/src/features/identity/views/PermissionsList.vue
app/Admin/src/features/inventory/views/StockItemsList.vue
app/Admin/src/features/inventory/views/StockItemDetail.vue
app/Admin/src/features/inventory/views/StockLocationsList.vue
app/Admin/src/features/inventory/views/StockLocationDetail.vue
app/Admin/src/features/inventory/views/StockMovementsList.vue
app/Admin/src/features/inventory/views/StockReservationsList.vue
app/Admin/src/features/inventory/views/StockTransfersList.vue
app/Admin/src/features/inventory/views/StockTransferDetail.vue
app/Admin/src/features/location/views/CountriesList.vue
app/Admin/src/features/location/views/CountryDetail.vue
app/Admin/src/features/location/views/StatesList.vue
app/Admin/src/features/location/views/StateDetail.vue
app/Admin/src/features/ordering/views/OrdersList.vue
app/Admin/src/features/ordering/views/OrderDetail.vue
app/Admin/src/features/payment/views/PaymentMethodsList.vue
app/Admin/src/features/payment/views/PaymentMethodDetail.vue
app/Admin/src/features/payment/views/PaymentsList.vue
app/Admin/src/features/profile/views/ProfilesList.vue
app/Admin/src/features/profile/views/ProfileDetail.vue
app/Admin/src/features/profile/views/AddressesList.vue
app/Admin/src/features/profile/views/AddressDetail.vue
app/Admin/src/features/shipping/views/ShippingMethodsList.vue
app/Admin/src/features/shipping/views/ShippingMethodDetail.vue
app/Admin/src/features/shipping/views/ShippingRatesList.vue
app/Admin/src/features/shipping/views/ShippingRateDetail.vue
```

**Interfaces:**
- Consumes: new `@panel` alias from Task 2
- Produces: all 36 views import from `@panel/PageShell.vue`

Each file has exactly ONE import to change:
```
import PageShell from '@ui/PageShell.vue'
```
becomes:
```
import PageShell from '@panel/PageShell.vue'
```

- [ ] **Step 1: Batch replace across all 36 files**

```bash
for f in $(rg -l '@ui/PageShell.vue' app/Admin/src/features/); do
  sed -i "s|@ui/PageShell.vue|@panel/PageShell.vue|g" "$f"
done
```

- [ ] **Step 2: Verify no stale @ui references remain**

```bash
rg '@ui/' app/Admin/src/ 2>/dev/null
```

Expected: **No output** (zero matches).

- [ ] **Step 3: Verify @panel references are correct**

```bash
rg '@panel/PageShell.vue' app/Admin/src/features/ | wc -l
```

Expected: 36.

---

### Task 12: Build and test verification

**Files:**
- None (read-only verification)

**Interfaces:**
- Consumes: all previous tasks complete
- Produces: confirmation that build succeeds and 357 tests pass

- [ ] **Step 1: Run build**

```bash
cd app/Admin && pnpm run build 2>&1 | tail -5
```

Expected: `✓ built in ...`

- [ ] **Step 2: Run tests**

```bash
cd app/Admin && pnpm run test:unit -- run 2>&1 | tail -5
```

Expected:
```
 Test Files  40 passed (40)
      Tests  357 passed (357)
```

- [ ] **Step 3: If any failures, fix and re-verify**

Common issues:
- Missing import — check that the component was moved correctly
- Stale alias — verify `vite.config.ts` has new aliases and no old ones
- Test imports — tests use `../Component.vue` relative paths; verify the file exists at that path

---

### Task 13: Commit

**Files:**
- All changes from Tasks 1-12

- [ ] **Step 1: Stage everything and commit**

```bash
git add -A app/Admin/src/shared/components/ app/Admin/vite.config.ts app/Admin/src/app/router/routes.ts app/Admin/src/features/
git commit -m "refactor(admin): reorganize shared components into PrimeVue-aligned groups

- Move 24 components into 5 new groups: layout (8), panel (7), data (4),
  overlay (1), form (2)
- Delete 11 Sakai demo widgets + 2 unused components (ProductCard, CountryFlag)
- Delete 5 old barrel files (navigation, ui, feedback, forms, tables)
- Inline GradientCard into ErrorPageShell; remove FloatingConfigurator from
  error pages (theme fluff)
- Fix FormSection slot-inside-Card bug
- Replace vite aliases: @navigation/@ui/@feedback/@forms/@tables →
  @layout/@panel/@data/@overlay/@form
- Update routes.ts (3 imports) and 36 placeholder views (1 import each)
- Move 2 test files (layout/__tests__)
- Create 9 new barrel files and 4 empty stub groups"
```

- [ ] **Step 2: Verify git status clean**

```bash
git status --short
```

Expected: No uncommitted changes in the Admin app.

- [ ] **Final verification: build + tests one more time**

```bash
cd app/Admin && pnpm run build && pnpm run test:unit -- run
```
