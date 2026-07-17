# Admin Layout Consolidation — Design

**Date:** 2026-07-18
**Status:** Design approved, awaiting implementation plan
**Precedes:** `2026-07-17-admin-layout-migration-design.md` (preliminary styling fixes)

## Problem

The Admin SPA has three systemic layout inconsistencies discovered during code review:

1. **Double breadcrumb** — `Main.Layout.vue:68` renders `<AppBreadcrumb />` globally before `<router-view />`, but 24 views also render it locally, producing duplicate breadcrumb trails. 4 more views import `AppBreadcrumb` but never render it (dead imports).
2. **No Card wrapper** — 5 list views (`OptionValueList`, `OptionTypeList`, `TaxonList`, `InventoryUnitList`, `TaxonomyList`) use a raw border-div instead of PrimeVue `<Card>`. 3 dashboard views use `<div class="p-6">` with inline tailwind border divs, no Card.
3. **10+ spacer conventions** — `p-6`, `p-6 max-w-7xl mx-auto`, `p-6 max-w-6xl mx-auto`, `p-6 max-w-4xl mx-auto`, `p-6 max-w-2xl mx-auto`, `p-6 max-w-full`, `p-6 pb-0 max-w-full`, `flex flex-col h-full overflow-hidden`, `flex flex-col gap-6 p-6`, `space-y-8`. No shared wrapper component.

Additionally, 15+ views use inline custom header markup instead of the `PageHeader.Component.vue` created in the previous session.

## Design

### 1. Breadcrumb: single source of truth

**Decision:** Keep `<AppBreadcrumb />` in `Main.Layout.vue` only. Remove from all 24 duplicating views and delete 4 dead imports. No view renders breadcrumb locally.

Rationale: The breadcrumb reads `route.meta.breadcrumb` to build the trail — it's route-aware global chrome, like the sidebar and topbar. It belongs in the layout, not duplicated per-page.

### 2. PageShell component

**Decision:** Introduce a shared `PageShell.Component.vue` that renders the standard page wrapper.

```
Props:
  maxWidth?: '2xl' | '4xl' | '6xl' | '7xl' | 'none'  // default 'none' = full width
  card?: boolean                                        // default true — wraps content in <Card>
  gap?: boolean                                         // default false — adds flex-col gap-6 between children

Template (card=true, gap=false):
  <div class="p-6" :class="{ 'max-w-{N}xl mx-auto': maxWidth !== 'none' }">
    <Card>
      <slot />
    </Card>
  </div>

Template (card=false, gap=true):
  <div class="p-6 flex flex-col gap-6" :class="{ 'max-w-{N}xl mx-auto': maxWidth !== 'none' }">
    <slot />
  </div>

Template (card=false, gap=false):
  <div class="p-6" :class="{ 'max-w-{N}xl mx-auto': maxWidth !== 'none' }">
    <slot />
  </div>
```

**maxWidth assignments:**
- List views (`maxWidth="none"`) — DataTables benefit from full width
- Dashboard views (`maxWidth="none"`) — widget grids need full width
- Form views (`maxWidth="4xl"`) — forms look bad stretched full-width; `4xl` (~896px) is comfortable
- Multi-card detail views (`maxWidth="6xl"` or `"7xl"`) — wider than forms but capped

### 3. Card wrapping for 5 border-div list views

The 5 list views that use `overflow-hidden border shadow-sm bg-surface-0 dark:bg-surface-900 rounded-2xl/3xl` instead of `<Card>` get wrapped in `<PageShell>` which provides `<Card>` via the `card` prop. The border-div and adjacent header markup are replaced by PageHeader + DataTable inside PageShell's Card.

### 4. Card wrapping for 3 dashboard views

`CatalogDashboard`, `OrderingDashboard`, `InventoryDashboard` wrap their content in `<PageShell>` (which provides Card). Their inner widget cards remain — PageShell provides the outer container.

### 5. PageHeader rollout

All 15+ views with inline custom headers get `<PageHeader>`:

**List-view pattern:**
```vue
<PageShell>
  <PageHeader :title="t('...')" :description="t('...')">
    <template #badge><Badge :value="totalRecords" severity="info" /></template>
    <template #actions>
      <Button label="New" icon="pi pi-plus" />
    </template>
  </PageHeader>
  <DataTable ... />
</PageShell>
```

**Form-view pattern:**
```vue
<PageShell maxWidth="4xl">
  <PageHeader :title="t('...')" :description="t('...')" back>
    <template #actions>
      <Button label="Cancel" severity="secondary" outlined @click="router.back()" />
      <Button label="Save" icon="pi pi-check" :loading="submitting" @click="onSubmit" />
    </template>
  </PageHeader>
  <form>...</form>
</PageShell>
```

**Detail-view pattern (multi-card, no auto-Card wrapper):**
```vue
<PageShell :card="false" gap maxWidth="6xl">
  <PageHeader :title="name" :description="t('...')" back />
  <Card>...</Card>
  <Card>...</Card>
</PageShell>
```

### 6. Manager views (breadcrumb removal only)

4 files (`TaxonomyManager`, `TaxonTreeManager`, `OptionTypeManager`, `StockLocationManager`) share the same `flex flex-col h-full` layout with flex-row sidebar. Only change: remove `<AppBreadcrumb />` + import. No PageShell — their layout is fundamentally different.

## Views NOT touched

- `Login.View.vue`, `NotFound.View.vue`, `ErrorPage.View.vue`, `EmptyPage.View.vue`, `AccessDenied.View.vue` — full-screen, no layout wrapper
- `StateForm.View.vue`, `CountryForm.View.vue` — Dialog-based, no standalone page
- `StockLocationList.View.vue` — sub-view rendered within `StockLocationManager`, not a standalone page

## File inventory

| Category | Count | Action |
|---|---|---|
| New files | 1 | `PageShell.Component.vue` |
| Views: breadcrumb removal | 24 | Remove `<AppBreadcrumb />` + import |
| Views: dead import removal | 4 | Remove unused `import AppBreadcrumb` |
| Views: PageShell + PageHeader | ~15 | Full wrapper + header replacement |
| Views: PageShell only | ~12 | Already use PageHeader, just add PageShell wrapper |
| Views: no change | ~19 | Manager views (4, breadcrumb only), full-screen (5), dialog forms (2), sub-views (1), already migrated (6) |

## Acceptance criteria

1. `git grep AppBreadcrumb` in `features/` returns zero matches
2. `git grep 'overflow-hidden border shadow-sm'` in `features/` returns zero matches
3. `git grep 'class="p-6"'` in `features/` returns zero matches (all replaced by `<PageShell`)
4. `git grep 'max-w-[0-9]*xl'` in `features/` returns zero matches (all replaced by `maxWidth` prop on PageShell)
5. Every view that uses `<Card>` directly has `:card="false"` on PageShell
6. Build passes (`pnpm run build`)
7. Tests pass (`pnpm run test:unit`, same 23/25 as before — no regressions)
