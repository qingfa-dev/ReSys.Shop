# Shared Components Sakai Alignment — Design Spec

**Date:** 2026-07-21
**Status:** Approved
**Influenced by:** Sakai Vue reference (`app/references/sakai-vue/`), PrimeVue Tailwind guide (`tailwindcss-primeui`)

## Goal

Refine existing shared components to align with Sakai Vue's lean wrapper pattern, adopt `tailwindcss-primeui` semantic tokens, add missing high-value compositions, and migrate feature-level duplicated patterns to shared components.

## Philosophy: Sakai-Lean

- Use PrimeVue auto-import directly for inputs, buttons, badges, avatars
- Only wrap high-value compositions: tables, layouts, dialogs, stat cards, empty/error states
- No base input wrappers (no `BaseButton`, `BaseInput`, `BaseSelect`)
- Semantic tokens (`text-muted-color`, `bg-primary`, `border-surface`) replace hardcoded CSS

## Section 1: Directory Structure

```text
shared/components/
├── data-display/     # DetailField, MetadataManager, StatCard, TabbedDetail
├── feedback/         # Drawer, EmptyState, ErrorState, ManagerWelcome*, StatusBadge
├── form/             # FormField
├── navigation/       # Breadcrumb, PageHeader, PageShell
├── overlays/         # ConfirmDialog*, ModalDialog*    ← NEW category
├── tables/           # CompactTable*, DataTableShell
└── index.ts          # Barrel export                     ← NEW
```

**Moves:**
- `ConfirmButton` (base/) → `ConfirmDialog` (overlays/) — renamed, enhanced
- `ManagerWelcome` (navigation/) → feedback/ — it's a placeholder, not navigation
- `base/` directory removed (empty after ConfirmButton move)

**Kept:** `shared/fields/` for Zod schema inheritance (separate concern, zero Vue imports)

## Section 2: Tailwind-PrimeUI Token Adoption

`tailwindcss-primeui` is already imported in `src/assets/tailwind.css`. Semantic tokens are available but unused in shared components.

### Token mappings applied to all 15 components:

| Current (raw) | Semantic Token |
|---|---|
| `text-xs text-surface-400`, `#6B7280` | `text-muted-color` |
| `font-bold text-lg`, `dark text` | `text-color-emphasis` |
| `bg-gray-50`, `bg-slate-100` | `bg-emphasis` |
| `border border-gray-200` | `border border-surface` |
| `rounded-lg`, `rounded-xl` | `rounded-border` |
| `bg-white` | `bg-surface-0 dark:bg-surface-900` (card) |
| `#111827`, `text-surface-900` | `text-color` |

Dark mode comes free — `dark:` variants applied where contrast matters.
No feature-level tokens needed (PrimeVue components already use the theme).

## Section 3: Component API Refinements

### Core (3+ feature consumers):

| Component | Changes |
|---|---|
| **PageShell** | Token migration only; `card` → `!mb-0` pattern; add `bg-surface-0 dark:bg-surface-950` for `card=false` |
| **PageHeader** | No API change (already uses tokens correctly); keeps `router.back()` |
| **FormField** | Tokens: `text-surface-500` → `text-muted-color`; add `#description` slot for tooltips |
| **DataTableShell** | `rounded-xl` → `rounded-border` on buttons; empty state delegates to `EmptyState`; `as any` → `RouteLocationRaw` for `createRoute` |
| **DetailField** | Tokens: `text-surface-400` → `text-muted-color`, `text-surface-300` → `text-muted-color` |
| **StatusBadge** | Add `label?` prop (label-only mode), `severity?` fallback (default `info`); remove `as any`; `rounded-xl` → `rounded-border` |

### Secondary (unused or low-usage):

| Component | Changes |
|---|---|
| **EmptyState** | `actionRoute: any` → `RouteLocationRaw`; `text-surface-400` → `text-muted-color` |
| **ErrorState** | Already uses tokens — no change |
| **Drawer** | Replace manual v-model passthrough with `defineModel` (Vue 3.4+) |
| **MetadataManager** | Token migration only |
| **TabbedDetail** | Token migration only |
| **ManagerWelcome** | Move to feedback/; token migration only |
| **StatCard** | Token migration; `card !mb-0` pattern; `text-green-500`/`text-red-500` trends stay |
| **Breadcrumb** | Token migration only |

## Section 4: New Components

### ConfirmDialog (`overlays/ConfirmDialog.vue`)
Renamed + enhanced from `ConfirmButton`. Slot-based trigger (was always a button).

```ts
// Props: icon, severity, header, message, acceptLabel, rejectLabel, loading
// Emits: confirm, cancel
// Slots: default (trigger content)
```

Usage: Replaces `useConfirm().require({...})` boilerplate in 20 files across 8 features.

### ModalDialog (`overlays/ModalDialog.vue`)
Standardizes 14 duplicated raw `Dialog` patterns.

```ts
// defineModel<boolean>('visible') + props: header, maxWidth, closable, dismissableMask
// Slots: default (body), footer (actions)
```

### CompactTable (`tables/CompactTable.vue`)
Lightweight inline table for forms, detail pages, dialogs. No search, pagination, or create button.

```ts
// Props: value, columns (reuses ColumnDef from DataTableShell), rows (5), dataKey, scrollable, loading
```

Replaces 20+ inline `<DataTable>` instances.

## Section 5: Feature Adoption Plan

Six migration patterns in order of impact:

| # | Pattern | Sites | Old Pattern | New Component | Target Features |
|---|---|---|---|---|---|
| 1 | Delete confirmations | ~20 | `useConfirm` + `confirm.require()` | `ConfirmDialog` | catalog, location, users, ordering, inventories, payment, shipping |
| 2 | Status tags | 33 | Inline `<Tag>` + severity logic | `StatusBadge` | users, inventories, catalog, ordering, reports, location, payment |
| 3 | Dialogs | 14 | Raw `<Dialog>` boilerplate | `ModalDialog` | ordering, catalog, inventories |
| 4 | Inline tables | 20+ | Raw `<DataTable>` | `CompactTable` | ordering, catalog, inventories, payment |
| 5 | Form fields | 15+ | Raw `<label>` + input pairs | `FormField` | inventories, ordering, auth, users |
| 6 | Detail fields | 8+ | Raw `<span>` + value pairs | `DetailField` | all detail pages |

**Strategy:** One pattern at a time, feature by feature. Catalog first as canary (covers all 6 patterns). Each batch is a single commit with test verification.

## Constraints

- No new dependencies (tailwindcss-primeui already installed)
- No base input wrappers (Sakai-lean philosophy)
- No router, store, or API changes
- No tailwind.css or CSS layer changes
- Tests colocated in `__tests__/` next to each component
- Type-check must pass after each batch

## Verification

```bash
pnpm run type-check         # vue-tsc --build
pnpm run test:unit          # vitest run
pnpm run lint:eslint        # eslint .
```

## References

- Sakai Vue: `app/references/sakai-vue/src/layout/`, `src/components/dashboard/`
- PrimeVue Tailwind: https://primevue.dev/tailwind/
- PrimeVue LLMs: https://primevue.dev/llms/
- Existing spec: `docs/superpowers/specs/2026-07-21-admin-spa-refactor-design.md`
