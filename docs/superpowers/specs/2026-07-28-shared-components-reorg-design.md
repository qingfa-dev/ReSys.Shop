---
title: Shared Components Reorganization
date: 2026-07-28
status: approved
context: Brainstorming session — refactor shared/components into PrimeVue-aligned groups, prune demo fluff, inline 1-consumer wrappers
---

## Problem

The `app/Admin/src/shared/components/` directory has 35 components across 5 arbitrary
subdirectories (`navigation/`, `feedback/`, `tables/`, `ui/`, `forms/`) — a mix of
Sakai theme shells, demo marketing widgets with hardcoded fake data, unused-but-useful
admin patterns, and thin wrapper components. The grouping doesn't align with
PrimeVue's own component taxonomy, making it hard to find components or reason about
intent.

## Solution

Reorganize into groups that mirror PrimeVue's component taxonomy (per
`app/legacy/llms.txt`): `layout/`, `overlay/`, `panel/`, `data/`, `form/`. Create
empty stub groups for deferred categories (`messages/`, `menu/`, `file/`, `button/`).
Delete 11 demo/unused files. Inline 1-consumer thin wrappers.

## Architecture

### Group taxonomy (from PrimeVue llms.txt)

| Group | PrimeVue category | Our components |
|-------|-------------------|----------------|
| `layout/` | (Sakai shells, kept for now) | AppLayout, AppTopbar, AppSidebar, AppMenu, AppMenuItem, UserMenu, AppFooter, AppConfigurator |
| `overlay/` | Dialog, Drawer, Popover, ConfirmDialog | ConfirmDialog |
| `panel/` | Card, Panel, Toolbar, Splitter | PageShell, PageHeading, StatCard, DataTableCard, EmptyState, ErrorPageShell, AuthLayout |
| `data/` | DataTable, DataView, Tree, Paginator | FilterableDataTable, CrudToolbar, StatusTag, RatingBadge |
| `form/` | InputText, Select, FloatLabel, Forms | FormField, FormSection |
| `messages/` | Message, Toast | (empty — Toast is in shared/api/notify.ts) |
| `menu/` | Menu, MegaMenu, ContextMenu | (empty — deferred) |
| `file/` | FileUpload | (empty — deferred) |
| `button/` | Button, SpeedDial, SplitButton | (empty — deferred) |

### File tree mapping (before → after)

```
navigation/AppLayout.vue      → layout/AppLayout.vue
navigation/AppTopbar.vue      → layout/AppTopbar.vue
navigation/AppSidebar.vue     → layout/AppSidebar.vue
navigation/AppMenu.vue        → layout/AppMenu.vue
navigation/AppMenuItem.vue    → layout/AppMenuItem.vue
navigation/UserMenu.vue       → layout/UserMenu.vue
ui/AppFooter.vue              → layout/AppFooter.vue
ui/AppConfigurator.vue        → layout/AppConfigurator.vue

ui/PageShell.vue              → panel/PageShell.vue
ui/PageHeading.vue            → panel/PageHeading.vue
ui/StatCard.vue               → panel/StatCard.vue
tables/DataTableCard.vue      → panel/DataTableCard.vue
feedback/EmptyState.vue       → panel/EmptyState.vue
feedback/ErrorPageShell.vue   → panel/ErrorPageShell.vue
forms/AuthLayout.vue          → panel/AuthLayout.vue

tables/FilterableDataTable.vue → data/FilterableDataTable.vue
tables/CrudToolbar.vue        → data/CrudToolbar.vue
ui/StatusTag.vue              → data/StatusTag.vue
ui/RatingBadge.vue             → data/RatingBadge.vue

feedback/ConfirmDialog.vue     → overlay/ConfirmDialog.vue
forms/FormField.vue            → form/FormField.vue
forms/FormSection.vue          → form/FormSection.vue
```

### Inlined (deleted as standalone file)

- `feedback/GradientCard.vue` (20 lines, 1 consumer: ErrorPageShell)
  Inline the 3-div gradient-border-card structure directly into ErrorPageShell.

### Removed

- `ui/FloatingConfigurator.vue` (28 lines) — theme-switch floating buttons on
  error pages serve no purpose. Remove the import and element from ErrorPageShell.
  AppConfigurator stays (still used by AppTopbar).

### Deleted (11 files — demo fluff / unused)

```
ui/HeroWidget.vue
ui/FeaturesWidget.vue
ui/StatsWidget.vue
ui/PricingWidget.vue
ui/HighlightsWidget.vue
ui/FooterWidget.vue
feedback/NotificationsWidget.vue
ui/TopbarWidget.vue
ui/BlockViewer.vue
ui/ProductCard.vue
ui/CountryFlag.vue
```

All are Sakai marketing/landing-page widgets with hardcoded fake data and zero
imports in the application. ProductCard references a hardcoded Product type;
CountryFlag depends on an external CDN.

### Bug fixes

- **FormSection.vue**: The `<slot>` renders outside the `<Card>` closing tag. Fix
  the structure so `<slot>` is inside Card during the move to `form/`.

## Files

### Created

| File | Purpose |
|------|---------|
| `layout/index.ts` | Barrel for 8 Sakai shell components |
| `panel/index.ts` | Barrel for 7 panel components |
| `data/index.ts` | Barrel for 4 data components |
| `overlay/index.ts` | Barrel for ConfirmDialog |
| `form/index.ts` | Barrel for FormField, FormSection |
| `messages/index.ts` | Empty stub barrel |
| `menu/index.ts` | Empty stub barrel |
| `file/index.ts` | Empty stub barrel |
| `button/index.ts` | Empty stub barrel |

### Deleted

| File | Reason |
|------|--------|
| `navigation/index.ts` | Old barrel, superseded by `layout/index.ts` |
| `feedback/index.ts` | Absorbed into `panel/`, `overlay/` |
| `tables/index.ts` | Absorbed into `panel/`, `data/` |
| `ui/index.ts` | Absorbed into `panel/`, `data/`, `layout/` |
| `forms/index.ts` | Superseded by `form/index.ts` |
| `feedback/GradientCard.vue` | Inlined into ErrorPageShell |
| `ui/FloatingConfigurator.vue` | Theme fluff on error pages |
| 11 demo/unused components | Listed above |

### Moved

14 component files moved across groups (see file tree mapping above).

### Modified

| File | Change |
|------|--------|
| `panel/ErrorPageShell.vue` | Inline GradientCard structure, remove FloatingConfigurator import |
| `form/FormSection.vue` | Fix slot-inside-Card bug (during move from `forms/`) |
| `routes.ts` | Update 3 import paths: `navigation/AppLayout` → `layout/AppLayout`, `forms/AuthLayout` → `panel/AuthLayout`, `feedback/ErrorPageShell` → `panel/ErrorPageShell` |
| ~35 feature view files | `shared/components/ui/PageShell` → `shared/components/panel/PageShell` |

### Moved (tests)

| File | Change |
|------|--------|
| `navigation/AppMenu.spec.ts` | Move to `layout/AppMenu.spec.ts` |
| `navigation/AppMenuItem.spec.ts` | Move to `layout/AppMenuItem.spec.ts` |
| `navigation/UserMenu.spec.ts` | Move to `layout/UserMenu.spec.ts` |

## Verification

```bash
cd app/Admin
pnpm run build                          # Zero errors
pnpm run test:unit -- run               # 357/357 passing
pnpm run lint                           # Clean (ignore pre-existing parsers.spec.ts warnings)
```

## Scope boundary

- **In scope**: File moves, deletions, barrel file updates, import path updates,
  GradientCard inline, FormSection bug fix, FloatingConfigurator removal
- **Out of scope**: Tailwind → PrimeVue native props migration (separate effort),
  Sakai shell replacement with PrimeVue Sidebar/Menu (separate project),
  Writing feature pages that consume EmptyState/ConfirmDialog/FilterableDataTable
