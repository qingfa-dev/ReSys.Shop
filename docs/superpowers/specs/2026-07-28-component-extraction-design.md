# Component Extraction — ReSys.Shop Admin SPA

## Problem

`src/views/` contains Sakai theme demo pages mixed with reusable components. The reusable UI blocks need to be extracted into `shared/components/` and the demo pages removed. This enables building actual admin pages later from the extracted building blocks.

## Solution

Extract 18 focused components from `src/views/` into 5 target folders in `shared/components/`, preserving Sakai Vue styles and PrimeVue patterns. Delete `src/views/` after extraction. Drop Sakai marketing components (FeaturesGrid, PricingSection, HeroBlock, etc.) — not relevant to e-commerce admin.

## Extracted Components

### feedback/ (error states, confirmations, empty states)

| Component | Source | Props |
|---|---|---|
| `ErrorPageShell.vue` | NotFound + Access + Error (3 identical layouts) | `statusCode`, `title`, `description`, `gradientColor`, `icon`, `iconColor`, `image`, `buttonLabel` |
| `GradientCard.vue` | NotFound, Access, Error, Login (4 files) | `gradient`, inner slot |
| `ConfirmDialog.vue` | Crud.vue (3 instances) | `visible`, `message`, `@confirm`, `@cancel` |
| `EmptyState.vue` | New (pattern from Empty.vue + no-data states) | `title`, `description`, `icon` |

### forms/ (auth, form layouts, field patterns)

| Component | Source | Props |
|---|---|---|
| `LoginForm.vue` | Login.vue | Self-contained (email, password, remember, forgot link, submit) |
| `AuthLayout.vue` | Login + Blocks sign-in | `title`, `subtitle`, `gradient`, default slot |
| `FormField.vue` | FormLayout.vue (3 layout variants) | `label`, `layout` ('vertical'|'horizontal'|'inline'), `helpText`, `invalid` |
| `FormSection.vue` | FormLayout + all uikit docs | `title`, default slot |

### tables/ (data tables, CRUD patterns, filtering)

| Component | Source | Props |
|---|---|---|
| `CrudToolbar.vue` | Crud.vue | `@new`, `@delete`, `@export`, `deleteDisabled`, `searchPlaceholder` |
| `DataTableCard.vue` | All uikit table panels | `title`, default slot |
| `FilterableDataTable.vue` | TableDoc.vue filtering demo | `columns`, `data`, `filters`, `loading` |

### ui/ (cards, stats, badges, status displays)

| Component | Source | Props |
|---|---|---|
| `StatCard.vue` | StatsWidget + Blocks stat block | `label`, `value`, `icon`, `iconBgClass`, `subText` |
| `PageShell.vue` | Empty.vue + all uikit docs | `title`, default slot |
| `StatusTag.vue` | ListDoc + TableDoc + Crud | `status`, `domain` ('inventory'|'order'|'stock') |
| `ProductCard.vue` | ListDoc list+grid templates | `product`, `layout` ('list'|'grid') |
| `RatingBadge.vue` | ListDoc (reused in list+grid) | `rating` |
| `CountryFlag.vue` | TableDoc + InputDoc | `country` {name, code} |
| `PageHeading.vue` | Blocks.vue page heading | `breadcrumbs[]`, `title`, `stats[]`, `actions[]` |

## Consistency Rules

- All components use `<script setup lang="ts">`
- Props are typed with `interface Props` + `defineProps<Props>()` or `withDefaults`
- Sakai CSS classes preserved: `.card`, font classes (`font-semibold text-xl mb-4`), flex utilities
- Emits use `defineEmits<{...}>()` syntax
- No default exports — all named exports for barrel compatibility

## Non-Goals

- No page-level files created (pages rebuilt later)
- No Sakai marketing/demo components extracted (FeaturesGrid, PricingSection, HeroBlock, CtaSection, SkeletonCard, BannerBar, CheckboxGroup, RadioGroup, ExpandableRowTable, GroupedTable, DocSection, DocCodeBlock, StatsGrid)

## Files Modified

- `shared/components/*/index.ts` — all 5 barrels updated
- No existing shared files changed

## Files Deleted

- `src/views/` — entire directory after extraction

## Verification

- `pnpm run build` — zero TypeScript errors
- `pnpm run test:unit` — all existing tests pass
- Coverage thresholds maintained
