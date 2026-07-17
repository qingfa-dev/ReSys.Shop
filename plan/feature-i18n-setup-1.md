---
goal: Switch Admin SPA locales from plain TS objects to vue-i18n with lazy loading
version: 1.0
date_created: 2026-07-17
owner: feat/admin-app
status: 'Completed'
tags: feature, i18n, refactor, locale
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Replace the current plain-TypeScript locale pattern (direct imports of typed objects) with `vue-i18n` using flat key-based messages, lazy loading per feature module, `useI18n()` composable, and proper interpolation support.

## 1. Requirements & Constraints

- **REQ-001**: Install `vue-i18n` v10+ for Vue 3 / Vue Router 5 compatibility
- **REQ-002**: Create i18n plugin instance with lazy-loading per feature namespace
- **REQ-003**: Convert all `.locales.ts` files to flat key-value JSON message files grouped by feature
- **REQ-004**: Update all 50 consuming components to use `const { t } = useI18n()` instead of direct locale imports
- **REQ-005**: Support interpolation (`{year}` → `t('auth.messages.copyright', { year: 2026 })`)
- **REQ-006**: Support language switching (en as default, extendable)
- **CON-001**: Keep `locale.types.ts` and `general.locales.ts` as type reference until fully migrated
- **CON-002**: Preserve existing runtime behavior — no visual changes

## 2. Implementation Steps

### Phase 1 — Install and configure vue-i18n

- GOAL-001: Set up the i18n infrastructure

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | `pnpm add vue-i18n@10` in app/Admin | | |
| TASK-002 | Create `src/app/plugins/i18n.ts` — createI18n with legacy:false, locale:'en', fallbackLocale:'en', lazy loading via `setLocaleMessage` | | |
| TASK-003 | Create `src/shared/locales/messages/` directory for JSON message files | | |
| TASK-004 | Update `src/app/main.ts` — `app.use(i18n)` after router | | |

### Phase 2 — Convert general locales to i18n format

- GOAL-002: Migrate the shared general locales as the first i18n namespace

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | Create `src/shared/locales/messages/en/common.json` → `{ "confirm": "Confirm", "cancel": "Cancel", ... }` | | |
| TASK-006 | Create `src/shared/locales/messages/en/navigation.json` → dashboard, home, catalog, etc. | | |
| TASK-007 | Update `src/shared/components/breadcrumb.component.vue` — use `useI18n()` with `t('navigation.home')` | | |
| TASK-008 | Delete `src/shared/locales/general.locales.ts` and `locale.types.ts` after confirming no remaining references | | |

### Phase 3 — Convert feature locale files (auth, dashboard, error)

- GOAL-003: Migrate auth, dashboard, and error feature locales

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Create `src/shared/locales/messages/en/auth.json` — flat keys: `auth.titles.login`, `auth.messages.login_success`, etc. | | |
| TASK-010 | Update `src/features/auth/views/login.view.vue` — replace `authLocales` with `useI18n()` | | |
| TASK-011 | Delete `src/features/auth/locales/auth.locales.ts` | | |

### Phase 4 — Convert catalog feature locales (largest feature set)

- GOAL-004: Migrate catalog (products, option-types, property-types, taxonomies, taxa)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-012 | Create `src/shared/locales/messages/en/catalog.json` — flat keys for products, option-types, option-values, property-types, taxonomies, taxa | | |
| TASK-013 | Update catalog views: product-form, product-list, product-classification-manager, option-type-form/list/manager, option-value-list, property-type-form/list, taxonomy-form/list/manager, taxon-form/list/tree-manager, taxon-rules-manager, VariantFormDialog, VariantGenerationDialog, ProductVariantManager, ProductImageList, ProductImageUploader, ProductInventoryManager | | |
| TASK-014 | Delete catalog `.locales.ts` files (product, option-type, option-value, property-type, taxonomy, taxon) | | |

### Phase 5 — Convert remaining feature locales

- GOAL-005: Migrate inventory, ordering, users feature locales

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-015 | Create `src/shared/locales/messages/en/inventory.json` | | |
| TASK-016 | Create `src/shared/locales/messages/en/ordering.json` | | |
| TASK-017 | Create `src/shared/locales/messages/en/users.json` | | |
| TASK-018 | Update inventory views (StockItemList, StockLocationForm/List/Manager, StockTransferForm/List/Detail, InventoryUnitList, StockAdjustmentDialog) | | |
| TASK-019 | Update ordering view (order-list) | | |
| TASK-020 | Update users views (admin-user-list, customer-list/detail, staff-detail/form, UserSecurityManager) | | |
| TASK-021 | Delete inventory, ordering, users `.locales.ts` files | | |

### Phase 6 — Clean up

- GOAL-006: Remove legacy infrastructure

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-022 | Delete `src/shared/locales/locale.types.ts` (FeatureLocales type no longer needed) | | |
| TASK-023 | Delete `src/shared/locales/general.locales.ts` | | |
| TASK-024 | Run `npm run type-check` — verify no TS errors beyond pre-existing | | |
| TASK-025 | Run `npx vite build` — verify no build regressions | | |

## 3. Alternatives

- **ALT-001**: Keep plain TS objects but add a thin i18n wrapper — would still require rebuilding interpolation and language switching from scratch; vue-i18n already solves this.
- **ALT-002**: Use ICU MessageFormat instead of flat keys — more powerful but adds a parser dependency and makes JSON less readable; flat keys are simpler for this codebase.

## 4. Dependencies

- **DEP-001**: `vue-i18n@^11` — latest Vue 3 compatible version (v10 deprecated)

## 5. Files

| File | Action |
|------|--------|
| `package.json` | Edit — add `vue-i18n` dep |
| `pnpm-lock.yaml` | Auto — updated by install |
| `src/app/plugins/i18n.ts` | Create — i18n instance |
| `src/app/main.ts` | Edit — `app.use(i18n)` |
| `src/shared/locales/messages/en/*.json` | Create — message files (7 files) |
| `src/shared/locales/locale.types.ts` | Delete |
| `src/shared/locales/general.locales.ts` | Delete |
| `src/features/*/locales/*.locales.ts` | Delete — 14 files |
| `src/features/*/views/*.vue` | Edit — 30+ files |
| `src/features/*/components/*.vue` | Edit — 10+ files |
| `src/shared/components/breadcrumb.component.vue` | Edit |

## 6. Testing

- **TEST-001**: `npm run type-check` passes with zero new TS errors
- **TEST-002**: `npx vite build` succeeds (only pre-existing Vue template errors)
- **TEST-003**: Manual — dev server loads without console errors

## 7. Risks & Assumptions

- **RISK-001**: Flat JSON keys mean every `t()` call must match locale keys exactly — no compile-time safety without additional tooling
- **RISK-002**: vue-i18n v10 may have breaking changes from v9; pin to exact version
- **ASSUMPTION-001**: No vue-i18n compatibility issues with Vue 3.5, Vue Router 5, Vite 8
- **ASSUMPTION-002**: `setLocaleMessage()` lazy loading works with Vite's module resolution

## 8. Related Specifications / Further Reading

- [vue-i18n docs](https://vue-i18n.intlify.dev/)
