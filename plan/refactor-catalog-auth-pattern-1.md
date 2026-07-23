---
goal: Refactor Catalog Module to Match Auth Feature Pattern
version: 1.0
date_created: 2026-07-23
owner: Admin SPA team
status: Planned
tags: [refactor, catalog, vue, pattern]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Refactor `features/catalog/` to match auth feature pattern defined in `plan/pattern-feature-module-1.md`.

## 1. Requirements & Constraints

- **PAT-001**: Request types must derive from Zod schemas (alias pattern)
- **PAT-002**: All user-facing strings via `t()` from `vue-i18n`, zero hardcoded strings
- **PAT-003**: Pages are thin wrappers delegating to form components
- **PAT-004**: API layer uses static classes returning raw `Result<T>`
- **PAT-005**: Use existing i18n keys in `catalog.json`
- **CON-001**: All 191 tests must pass after each phase
- **CON-002**: No new npm dependencies
- **CON-003**: `products.api.ts`, `taxonomies.api.ts`, `optionTypes.api.ts` already correct — keep

## 2. Implementation Phases

### Phase 1: Fix Request Types to Derive from Schemas

- GOAL-001: Align `types/product.request.ts`, `types/taxonomy.request.ts`, `types/option-type.request.ts` to derive from Zod schemas

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Fix `types/product.request.ts` — alias `CreateProductRequest = CreateProductForm`, `UpdateProductRequest = UpdateProductForm` | | |
| TASK-002 | Fix `types/taxonomy.request.ts` — same pattern | | |
| TASK-003 | Fix `types/option-type.request.ts` — same pattern | | |
| TASK-004 | Update `mappers/` to use new request type names | | |
| TASK-005 | Verify: tests pass, typecheck clean | | |

### Phase 2: Add Pinia Store + Composable

- GOAL-002: Add `store/catalog.store.ts` + `composables/useCatalog.ts` following auth pattern

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | Create `store/catalog.store.ts` with isLoading/serverErrors/fieldErrors + CRUD actions | | |
| TASK-007 | Create `composables/useCatalog.ts` wrapping store | | |
| TASK-008 | Update `index.ts` barrel to export store + composable | | |
| TASK-009 | Verify: tests pass, typecheck clean | | |

### Phase 3: i18n All Components

- GOAL-003: Replace all hardcoded strings in form components with `t()` calls

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-010 | Fix `ProductForm.vue` — all strings via `t()`, labels, placeholders, button labels, toasts | | |
| TASK-011 | Fix `TaxonomyForm.vue` — same | | |
| TASK-012 | Fix `OptionTypeForm.vue` — same | | |
| TASK-013 | Verify: tests pass, typecheck clean | | |

### Phase 4: i18n Pages + Thin Wrappers

- GOAL-004: Replace all hardcoded strings in page components, delegate to form components

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-014 | Fix `ProductListPage.vue` — PageHeader title/placeholder via `t()`, thin wrapper | | |
| TASK-015 | Fix `ProductDetailPage.vue` — delegate to `ProductForm.vue`, PageHeader via `t()` | | |
| TASK-016 | Fix `TaxonomyListPage.vue` — same | | |
| TASK-017 | Fix `TaxonomyDetailPage.vue` — same (taxon management inline OK) | | |
| TASK-018 | Fix `OptionTypeListPage.vue` — same | | |
| TASK-019 | Fix `OptionTypeDetailPage.vue` — same | | |
| TASK-020 | Fix `DashboardPage.vue` — KPI labels via `t()` | | |
| TASK-021 | Verify: all tests pass, typecheck clean | | |

### Phase 5: Update Tests

- GOAL-005: Ensure all tests use correct mock patterns (mock apiClient for API tests, mock vue-i18n for component tests)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-022 | Update API tests — use `vi.mocked(apiClient.*)` pattern | | |
| TASK-023 | Update page tests — mock `vue-i18n` with identity `t` function | | |
| TASK-024 | Final verification: full test suite, typecheck, lint | | |

## 3. Alternatives

- **ALT-001**: Keep function-based API exports — rejected: auth uses static class pattern
- **ALT-002**: Keep request types as independent interfaces — rejected: causes duplication with schemas
- **ALT-003**: Skip store/composable — accepted temporarily for simple entities (Country, State) but catalog needs it for complex CRUD

## 4. Dependencies

- **DEP-001**: Existing i18n catalog at `src/shared/localization/messages/en/catalog.json`
- **DEP-002**: Existing `vue-i18n` with `useI18n()`
- **DEP-003**: Existing Pinia with `defineStore`

## 5. Files

- **FILE-001**: `features/catalog/types/product.request.ts` — rewrite to derive from schemas
- **FILE-002**: `features/catalog/types/taxonomy.request.ts` — rewrite
- **FILE-003**: `features/catalog/types/option-type.request.ts` — rewrite
- **FILE-004**: `features/catalog/store/catalog.store.ts` — new (Pinia store)
- **FILE-005**: `features/catalog/composables/useCatalog.ts` — new
- **FILE-006**: `features/catalog/components/ProductForm.vue` — i18n fix
- **FILE-007**: `features/catalog/components/TaxonomyForm.vue` — i18n fix
- **FILE-008**: `features/catalog/components/OptionTypeForm.vue` — i18n fix
- **FILE-009 through FILE-015**: 7 page components — i18n fixes + thin wrapper pattern
- **FILE-016**: `features/catalog/index.ts` — add store + composable exports
- **FILE-017 through FILE-023**: Test files — update mocks

## 6. Testing

- **TEST-001**: API tests mock `apiClient`, verify endpoint URL + HTTP method + body
- **TEST-002**: Component tests mock `vue-i18n` `useI18n: () => ({ t: (key: string) => key })`
- **TEST-003**: Page tests provide PrimeVue + ConfirmationService + ToastService + router
- **TEST-004**: All existing 191 tests continue to pass

## 7. Risks & Assumptions

- **RISK-001**: Changing request types may break API contracts if schemas don't match exactly. Mitigation: verify all field names between `types/` and `schemas/` match.
- **RISK-002**: i18n key paths in `catalog.json` may not cover all needed strings. Mitigation: add missing keys to `catalog.json` as needed.
- **ASSUMPTION-001**: All form components already import `vue-i18n` and the `t()` function — verification needed.
- **ASSUMPTION-002**: Tests mock `vue-i18n` at module level — check existing test patterns.

## 8. Related Specifications

- `plan/pattern-feature-module-1.md` — Complete feature module convention reference
- `spec/design-admin-spa-list-detail-pattern.md` — Admin SPA page design spec
- `src/shared/localization/messages/en/catalog.json` — i18n translation keys
