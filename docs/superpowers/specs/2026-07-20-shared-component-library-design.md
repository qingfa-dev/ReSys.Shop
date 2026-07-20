# Shared Component Library — Design Spec

**Date**: 2026-07-20
**Scope**: Port and standardize reusable shared UI components from `app/lagacy/Admin/` into `app/Admin/src/shared/components/`, redesigned for PrimeVue 5 + Vue 3.5 Composition API + TypeScript, following Sakai design patterns.

---

## Goals

- Provide a standardized, typed, well-tested component library that every admin feature page can consume
- Encode reusable patterns so features don't reinvent PageHeader, FormField, EmptyState, DataTable shells, etc.
- Follow Sakai design patterns: Tailwind-first, PrimeVue CSS token references, reactive singleton stores
- Do NOT wrap what PrimeVue 5 already provides well (Button, InputText, Select, Dialog, DataTable, etc.)

## Non-Goals

- Do NOT rewrite PrimeVue 5 primitives
- Do NOT build domain-specific components (UserTable, ProductForm — those live in `features/`)
- Do NOT build charts or media wrappers until dashboard designs stabilize
- Do NOT include i18n setup (separate concern, planned for P1 Breadcrumb)

---

## Component Inventory

### P0 — Every Feature Page Needs These (14 components)

| # | Component | Category | Source | Purpose |
|---|-----------|----------|--------|---------|
| 1 | `PageHeader` | layout | legacy | Title + back btn + description + `#actions` slot |
| 2 | `PageContainer` | layout | legacy | Max-width wrapper + optional Card |
| 3 | `Section` | layout | new | Section header with title + actions slot |
| 4 | `FormField` | form | legacy | Label + required `*` + error + hint + default slot |
| 5 | `SearchInput` | form | new | Debounced search with clear btn |
| 6 | `DetailField` | data-display | legacy | Label / value pair with empty placeholder |
| 7 | `DetailGroup` | data-display | new | Group of DetailFields with section header |
| 8 | `DescriptionList` | data-display | new | Multi-column `<dl>` grid |
| 9 | `CopyButton` | data-display | new | Copy to clipboard with animated feedback |
| 10 | `EmptyState` | feedback | legacy | Icon + title + description + action |
| 11 | `SkeletonLoader` | feedback | new | Preset skeletons: table | card | form | detail |
| 12 | `LoadingOverlay` | feedback | new | Full-section spinner overlay |
| 13 | `ConfirmButton` | overlays | legacy | Icon btn → confirm dialog → emit |
| 14 | `DeleteDialog` | overlays | new | Standardized delete confirmation |

### P1 — Most Features Need These (7 components)

| # | Component | Category | Source | Purpose |
|---|-----------|----------|--------|---------|
| 15 | `StatCard` | data-display | legacy | Icon circle + value + label + trend + skeleton |
| 16 | `StatCardGroup` | data-display | new | Responsive grid of StatCards |
| 17 | `TabbedDetail` | data-display | legacy | Tabs wrapping dynamic components |
| 18 | `MetadataEditor` | form | legacy | Dynamic key-value grid |
| 19 | `ImageUploadField` | form | new | Upload + preview grid + reorder |
| 20 | `StatusBadge` | status | legacy | Status → severity Tag via lookup map |
| 21 | `Breadcrumb` | navigation | legacy | Auto-breadcrumb from route meta |

### P2 — Common But Not Universal (6 components)

| # | Component | Category | Source | Purpose |
|---|-----------|----------|--------|---------|
| 22 | `DataTableShell` | tables | legacy | Full DataTable with toolbar + search + pagination |
| 23 | `TableToolbar` | tables | new | SearchInput + Create/Export/Refresh buttons |
| 24 | `Drawer` | overlays | new | Slide-over panel |
| 25 | `Modal` | overlays | new | Centered modal dialog |
| 26 | `Timeline` | data-display | new | Vertical/horizontal timeline |
| 27 | `PageActions` | layout | new | Sticky bottom bar: Cancel + Save |

---

## Component APIs

### Layout

#### `PageHeader.vue`
```ts
props: { title: string; description?: string; backTo?: string; backLabel?: string }
slots: { default?(): any; actions?(): any }
```

#### `PageContainer.vue`
```ts
props: { maxWidth?: string; card?: boolean }  // defaults: 1504px, true
slots: { default(): any }
```

#### `Section.vue`
```ts
props: { title?: string; description?: string; collapsible?: boolean; collapsed?: boolean }
emits: { 'update:collapsed': [value: boolean] }
slots: { default(): any; actions?(): any }
```

#### `PageActions.vue` (P2)
```ts
props: { loading?: boolean; cancelLabel?: string; saveLabel?: string; showCancel?: boolean }
emits: { cancel: []; save: [] }
slots: { extra?(): any }
```

### Form

#### `FormField.vue`
```ts
props: { label: string; forId?: string; required?: boolean; error?: string; hint?: string }
slots: { default(): any }
```

#### `SearchInput.vue`
```ts
props: { placeholder?: string; debounce?: number }  // default debounce: 300ms
model: string
emits: { search: [value: string] }
```

#### `MetadataEditor.vue` (P1)
```ts
props: { keyPlaceholder?: string; valuePlaceholder?: string }
model: Record<string, unknown>
```

#### `ImageUploadField.vue` (P1)
```ts
props: { accept?: string; maxSize?: number; maxFiles?: number; previewHeight?: string }
model: File[]
emits: { upload: [files: File[]]; remove: [index: number] }
```

### Data Display

#### `DetailField.vue`
```ts
props: { label: string; value?: string | number; emptyText?: string }  // default emptyText: '\u2014'
slots: { default?(): any }
```

#### `DetailGroup.vue`
```ts
props: { title: string; columns?: 1 | 2 | 3 | 4 }  // default columns: 2
slots: { default(): any }
```

#### `DescriptionList.vue`
```ts
props: { items: { label: string; value: string | number; emptyText?: string }[]; columns?: 1 | 2 | 3 }
```

#### `CopyButton.vue`
```ts
props: { value: string; label?: string; icon?: string; variant?: 'button' | 'link' }  // default icon: 'pi pi-copy', variant: 'link'
```

#### `StatCard.vue` (P1)
```ts
props: { icon?: string; value: string | number; label: string; trend?: { direction: 'up' | 'down'; percentage: number }; loading?: boolean; iconBg?: string }
```

#### `StatCardGroup.vue` (P1)
```ts
props: { cols?: 2 | 3 | 4 }  // default 4
slots: { default(): any }
```

#### `TabbedDetail.vue` (P1)
```ts
props: { tabs: { title: string; component: Component; visible?: boolean }[]; activeIndex?: number; scrollable?: boolean }
emits: { 'update:activeIndex': [index: number] }
```

#### `Timeline.vue` (P2)
```ts
props: { items: { title: string; description?: string; timestamp: string; icon?: string; color?: string }[]; layout?: 'vertical' | 'horizontal' }
```

### Feedback

#### `EmptyState.vue`
```ts
props: { icon?: string; title: string; description?: string; actionLabel?: string; actionTo?: string; actionIcon?: string }
emits: { action: [] }
```

#### `SkeletonLoader.vue`
```ts
props: { variant: 'table' | 'card' | 'form' | 'detail' | 'list'; rows?: number }  // default rows: 5 (table), 3 (list)
```

#### `LoadingOverlay.vue`
```ts
props: { loading: boolean; message?: string }  // default loading: false
slots: { default(): any }
```

### Overlays

#### `ConfirmButton.vue`
```ts
props: { message: string; header?: string; icon?: string; severity?: 'danger' | 'warn' | 'info'; acceptLabel?: string; rejectLabel?: string; disabled?: boolean; loading?: boolean }
emits: { confirm: []; cancel: [] }
```

#### `DeleteDialog.vue`
```ts
props: { entityName: string; warningText?: string; loading?: boolean; visible: boolean }
emits: { confirm: []; cancel: []; 'update:visible': [value: boolean] }
```

#### `Drawer.vue` (P2)
```ts
props: { visible: boolean; title: string; position?: 'right' | 'left' | 'bottom'; width?: string; loading?: boolean }
emits: { 'update:visible': [value: boolean]; close: [] }
slots: { default(): any; footer?(): any }
```

#### `Modal.vue` (P2)
```ts
props: { visible: boolean; title: string; width?: string; loading?: boolean; closable?: boolean }
emits: { 'update:visible': [value: boolean]; close: [] }
slots: { default(): any; footer?(): any }
```

### Status

#### `StatusBadge.vue` (P1)
```ts
props: { status: string; statusMap: Record<string, { label: string; severity: 'info' | 'warn' | 'danger' | 'success' | 'secondary' | 'contrast' }>; fallback?: string }
```

### Tables

#### `DataTableShell.vue` (P2)
```ts
props: {
  items: any[]; totalRecords: number; loading: boolean
  columns: { field: string; header: string; sortable?: boolean; filterable?: boolean; width?: string; body?: (data: any) => string }[]
  selection?: any[]; selectionMode?: 'single' | 'multiple'
  showCreate?: boolean; showExport?: boolean; showRefresh?: boolean; createLabel?: string
  rows?: number; rowsPerPageOptions?: number[]
  searchPlaceholder?: string
}
emits: {
  'update:selection': [value: any[]]; create: []; export: []; refresh: []
  'update:page': [page: number]; 'update:sort': [sort: { field: string; order: 1 | -1 }]
  'update:search': [query: string]; 'update:rows': [rows: number]; rowClick: [row: any]
}
slots: { 'row-actions'?(slotProps: { row: any }): any; 'toolbar-start'?(): any; 'toolbar-end'?(): any }
```

#### `TableToolbar.vue` (P2)
```ts
props: { showSearch?: boolean; showCreate?: boolean; showExport?: boolean; showRefresh?: boolean; createLabel?: string; searchPlaceholder?: string }
emits: { search: [query: string]; create: []; export: []; refresh: [] }
slots: { start?(): any; end?(): any }
```

### Navigation

#### `Breadcrumb.vue` (P1)
```ts
props: { items?: { label: string; to?: string }[] }
```

---

## File Structure

```
src/shared/components/
├── layout/
│   ├── PageHeader.vue
│   ├── PageContainer.vue
│   ├── Section.vue
│   ├── PageActions.vue          # P2
│   └── index.ts
├── form/
│   ├── FormField.vue
│   ├── SearchInput.vue
│   ├── MetadataEditor.vue        # P1
│   ├── ImageUploadField.vue      # P1
│   └── index.ts
├── data-display/
│   ├── DetailField.vue
│   ├── DetailGroup.vue
│   ├── DescriptionList.vue
│   ├── CopyButton.vue
│   ├── StatCard.vue              # P1
│   ├── StatCardGroup.vue         # P1
│   ├── TabbedDetail.vue          # P1
│   ├── Timeline.vue              # P2
│   └── index.ts
├── feedback/
│   ├── EmptyState.vue
│   ├── SkeletonLoader.vue
│   ├── LoadingOverlay.vue
│   └── index.ts
├── overlays/
│   ├── ConfirmButton.vue
│   ├── DeleteDialog.vue
│   ├── Drawer.vue                # P2
│   ├── Modal.vue                 # P2
│   └── index.ts
├── status/
│   ├── StatusBadge.vue           # P1
│   └── index.ts
├── navigation/
│   ├── Breadcrumb.vue            # P1
│   └── index.ts
├── tables/
│   ├── DataTableShell.vue        # P2
│   ├── TableToolbar.vue          # P2
│   └── index.ts
└── index.ts                      # Re-exports all category barrels
```

**Naming convention**: `PascalCase.vue` (no `.Component` suffix — the directory signals component role).

**Barrels**: Each category directory has an `index.ts` that re-exports its components. Root `index.ts` re-exports all categories.

**Imports**:
```ts
import { PageHeader, PageContainer } from '@/shared/components/layout'
import { FormField, SearchInput } from '@/shared/components/form'
import { DeleteDialog, ConfirmButton } from '@/shared/components/overlays'
```

---

## Design Decisions

| Decision | Rationale |
|----------|-----------|
| No `base/` category | PrimeVue 5 already provides Button, InputText, Select, Checkbox, etc. Wrapping them adds indirection without value. |
| No `charts/` category | Charts are domain-specific (Dashboard vs Reports). They belong in `features/` with domain-aware wrappers. |
| No `media/` category | PrimeVue provides Image, Avatar, FileUpload. Only `ImageUploadField` justifies a wrapper (upload+preview+reorder pattern). |
| No `actions/` category | ActionBar/BulkActions live inside `tables/` or `features/`. Separate `DeleteButton`/`EditButton` are anti-patterns — use ConfirmButton with different props. |
| Subdirectories by category | 27 components in one flat directory is unmanageable. Categories make intent and discoverability clear. |
| No `.Component` suffix | The `shared/components/` directory already signals role. Inline with user's convention: "Vue components: PascalCase.vue". |
| `defineModel` for v-model | Vue 3.4+ idiomatic pattern. Used in SearchInput, MetadataEditor, ImageUploadField, TabbedDetail, Drawer, Modal. |
| Tailwind + CSS tokens, not SCSS | Follows Sakai patterns. Layout components in `app/layout/` use SCSS because they're fixed (Sakai shell); shared components use Tailwind for flexibility. |
| `withDefaults` for optional props | Clean defaults without null checks in template. |

---

## Sakai Patterns Applied

| Pattern | Where |
|---------|-------|
| Theme token via `getComputedStyle()` | CopyButton (animation colors), StatCard (icon bg), Section (border) |
| `.card` utility class | PageContainer, EmptyState, StatCard, DetailGroup |
| `1504px` max-width | PageContainer (matching Sakai ultra-wide breakpoint) |
| Responsive Tailwind grid | StatCardGroup (`grid grid-cols-1 lg:grid-cols-2 xl:grid-cols-4 gap-8`) |
| 3-state animated feedback | CopyButton (copy → spinner → checkmark, from Sakai BlockViewer) |
| View transition for state changes | LoadingOverlay (fade transition on `loading` toggle) |

---

## Test Strategy

**P0 tests** (ported from legacy, rewritten for Vitest + @vue/test-utils):

| Test file | Tests |
|-----------|-------|
| `ConfirmButton.spec.ts` | Renders icon button, opens confirm dialog, emits confirm, emits cancel |
| `DetailField.spec.ts` | Renders label with value, shows emptyText when value missing, renders slot |
| `EmptyState.spec.ts` | Renders title + description, renders action button when actionLabel set, emits action |
| `FormField.spec.ts` | Renders label, shows required asterisk, shows error text, shows hint, renders slot content |

**P0 new tests**:

| Test file | Tests |
|-----------|-------|
| `SearchInput.spec.ts` | Debounces input, emits search, clears on clear btn click, v-model reactivity |
| `SkeletonLoader.spec.ts` | Renders correct variant skeletons, respects rows prop |
| `LoadingOverlay.spec.ts` | Shows/hides overlay based on loading prop, shows message |
| `DeleteDialog.spec.ts` | Renders entity name, emits confirm, emits cancel, shows loading state |
| `PageHeader.spec.ts` | Renders title, back link when backTo set, renders description, renders actions slot |
| `PageContainer.spec.ts` | Applies max-width, wraps in card when card=true, renders slot |
| `DetailGroup.spec.ts` | Renders title, renders columns grid, renders slot children |
| `DescriptionList.spec.ts` | Renders items, applies columns, shows emptyText |

**P1+P2 tests** (deferred with components):

| Test file | Source |
|-----------|--------|
| `StatCard.spec.ts` | Legacy |
| `TabbedDetail.spec.ts` | Legacy |
| `DataTableShell.spec.ts` | Legacy |
| `MetadataEditor.spec.ts` | New |
| `Drawer.spec.ts` | New |
| `Modal.spec.ts` | New |

---

## Dependency Graph (Build Order)

```
P0 standalone (no deps)     P0 with internal deps       P1                         P2
──────────────────────────────────────────────────────────────────────────────────────────
PageHeader                   ─                           ─                           ─
PageContainer                ─                           ─                           ─
Section                      ─                           ─                           ─
FormField                    ─                           ─                           ─
SearchInput                  ─                           ─                           ─
DetailField                  ─                           DetailGroup (uses DetailField) ─
DescriptionList              ─                           ─                           ─
CopyButton                   ─                           ─                           ─
EmptyState                   ─                           ─                           ─
SkeletonLoader               ─                           ─                           ─
LoadingOverlay               ─                           ─                           ─
ConfirmButton                ─                           ─                           ─
DeleteDialog                 ─                           ─                           ─
─                            ─                           StatCard → StatCardGroup     ─
─                            ─                           TabbedDetail                 ─
─                            ─                           MetadataEditor               ─
─                            ─                           ImageUploadField             ─
─                            ─                           StatusBadge†                 ─
─                            ─                           Breadcrumb†                  ─
─                            ─                           ─                            DataTableShell† → TableToolbar
─                            ─                           ─                            Drawer → LoadingOverlay
─                            ─                           ─                            Modal → LoadingOverlay
─                            ─                           ─                            Timeline
─                            ─                           ─                            PageActions
```

† Requires prerequisite ports: `shared/enums/` for StatusBadge, i18n setup for Breadcrumb, `shared/composables/usePagedList` for DataTableShell.

---

## NOT Built

| Why not | Components |
|---------|------------|
| PrimeVue 5 covers it | Button, InputText, InputNumber, Textarea, Select, MultiSelect, Checkbox, Radio, ToggleSwitch, Dialog, Tag, Badge, Skeleton, DataTable, Column, Paginator, Tabs/TabPanel, Image, Avatar, Toast/ToastService, ConfirmDialog/ConfirmationService, FileUpload, Tooltip (v-tooltip), Drawer, Popover, IconField, FloatLabel, DatePicker, Chart |
| Domain-specific | RoleBadge, PriorityBadge, CategoryBadge, UserTable, UserForm, ProductTable, OrderTable, specific EmptyStates (EmptyUsers, EmptyOrders) |
| Premature | FilterPanel, FilterDrawer, FilterGroup, ActiveFilters (wait until 3+ features need same pattern), CommandPalette, GaugeChart, ChartCard |

---

## Code Conventions

- **`<script setup lang="ts">`** — no Options API
- **Typed props** via `defineProps<T>()`
- **Defaults** via `withDefaults(defineProps<T>(), { ... })`
- **Typed emits** via `defineEmits<{ event: [payload] }>()`
- **Typed slots** via `defineSlots<{ slotName?(): any }>()`
- **Two-way binding** via `defineModel<T>()` for `v-model`
- **Tailwind classes** for all layout/spacing/sizing — no inline styles
- **CSS token references** for colors: `--p-primary-color`, `--p-text-muted-color`, `--p-surface-card`, `--p-content-border-color`
- **No SCSS** in shared components — SCSS stays in `assets/layout/` for the Sakai shell
- **Single-file components** — no separate template/script/style files

---

## Prerequisites for P1/P2

Before starting P1 components, these must exist:

1. **`shared/enums/`** — Port `app/lagacy/Admin/src/shared/utils/enums.ts` → `shared/enums/status-maps.ts`:
   - `ProductStatusMap`, `OrderStatusMap`, `CheckoutStateMap`, `PaymentStateMap`, `ShipmentStateMap`, `TransferStateMap`, `ReservationStateMap`
   - Used by `StatusBadge`

2. **i18n setup** — Port `app/lagacy/Admin/src/shared/locales/` and `app/lagacy/Admin/src/app/plugins/i18n.ts`:
   - Register `vue-i18n` in `app/main.ts`
   - 12 English locale JSON files
   - Used by `Breadcrumb` for route meta label resolution

3. **`shared/composables/`** — Port `usePagedList`, `useFormatter`, `useToast` from legacy:
   - Used by `DataTableShell` (usePagedList for page/sort/search state)
   - Used by `StatCard` (useFormatter for number display)
   - Used by `ConfirmButton`/`DeleteDialog` (useToast for feedback)

---

## Backend Dependencies

Some P0 components reference backend types already ported to `shared/types/`:

- `Response`, `AuditableResponse` (from `response.model.ts`) — used in DetailField labels
- `ServerResult<T>`, `ServerPagedResult<T>` (from `result.type.ts`) — used in DataTableShell data flow

No new backend types needed for P0 components.

---

## Risks

| Risk | Mitigation |
|------|------------|
| PrimeVue 5 API incompatibilities vs legacy PrimeVue 4 | Each component tested individually; legacy source consulted for intent, not copied |
| Tailwind class conflicts with Sakai shell SCSS | Shared components use Tailwind utilities (low specificity); Sakai shell uses SCSS class names (`.layout-topbar`, `.layout-sidebar`). Namespaces don't clash. |
| `defineModel` with Vue 3.4+ only | Current Admin uses Vue 3.5 — confirmed compatible |
| P1 prerequisites (enums, i18n, composables) blocking progress | P0 is fully standalone. P1/P2 are separate implementation phases. |
