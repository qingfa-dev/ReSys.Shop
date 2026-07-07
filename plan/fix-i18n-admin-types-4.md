---
goal: Eliminate $t() type errors in GlobalSearch.vue by aligning with the existing custom TypeScript locale system
version: 1.0
date_created: 2026-07-07
status: 'Planned'
tags: fix, i18n, typecheck, frontend
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

The Admin app uses a **custom TypeScript-native locale system** (not vue-i18n). Locale objects are imported directly and accessed as typed properties (e.g., `generalLocales.navigation.dashboard`). `GlobalSearch.vue` is the only component that calls `$t()`, which is a vue-i18n function that doesn't exist in this project — it's dead code saved only by `||` fallback strings. This plan replaces those calls with the established custom-locale pattern, eliminating the type errors.

## 1. Requirements & Constraints

- **REQ-001**: Replace `$t()` calls in `GlobalSearch.vue` with the existing custom locale system pattern (import + direct property access)
- **REQ-002**: Add the missing locale strings to the general locales or create a layout-specific locale
- **REQ-003**: Add any missing fields to the `GeneralLocales` interface if needed
- **CON-001**: Do NOT install vue-i18n — the project uses a custom locale system consistently across all 10+ feature modules
- **CON-002**: Do NOT change the functioning `||` fallback behavior — the new code should have the same runtime output
- **CON-003**: Follow the exact pattern used by other components: import locale constant, optionally cast to `Required<FeatureLocales>`, access via dot notation
- **GUD-001**: Use the existing `src/shared/locales/` infrastructure; do not create new i18n infrastructure

## 2. Implementation Steps

### Implementation Phase 1: Add layout-related strings to the general locales

- GOAL-001: Extend the `GeneralLocales` type and data to include the search-related strings used by `GlobalSearch.vue`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Read `src/shared/locales/locale.types.ts` — add a `layout` section to the `GeneralLocales` interface with `search` and `noResults` string fields | | |
| TASK-002 | Read `src/shared/locales/general.locales.ts` — add the `layout` section with `search: 'Search...'` and `noResults: 'No results found'` matching the existing fallback values | | |

### Implementation Phase 2: Fix GlobalSearch.vue

- GOAL-002: Replace the broken `$t()` calls in the template with the proper custom locale access pattern.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-003 | Read `src/app/layout/components/GlobalSearch.vue` | | |
| TASK-004 | Add import: `import { generalLocales } from '@/shared/locales/general.locales'` | | |
| TASK-005 | Replace `:placeholder="$t('layout.search') \|\| 'Search...'"` with `:placeholder="generalLocales.layout.search"` | | |
| TASK-006 | Replace `{{ $t('layout.noResults') \|\| 'No results found' }}` with `{{ generalLocales.layout.noResults }}` | | |

### Implementation Phase 3: Verify

- GOAL-003: Run all verification steps.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-007 | Run `pnpm type-check` — confirm zero errors from `GlobalSearch.vue` (only pre-existing `@primevue/core/api` and `metadata-manager` `Cannot find module` errors remain) | | |
| TASK-008 | Run `pnpm build-only` — confirm Vite build passes | | |
| TASK-009 | Run `pnpm test:unit` — confirm all 107 tests pass | | |

## 3. Alternatives

- **ALT-001**: Install vue-i18n and configure it — rejected because the entire codebase uses a custom locale system across 10+ feature modules, each with 100+ strings. This would be a massive migration with no benefit over fixing 2 lines in one file.
- **ALT-002**: Inline the strings directly in the template without any locale — rejected because the custom locale system exists and should be used consistently; adding 2 template strings to `generalLocales` is minimal effort.

## 4. Dependencies

- **DEP-001**: The existing custom locale system at `src/shared/locales/` (no external packages)

## 5. Files

- **FILE-001**: `src/shared/locales/locale.types.ts` — add `layout` section to `GeneralLocales`
- **FILE-002**: `src/shared/locales/general.locales.ts` — add layout strings data
- **FILE-003**: `src/app/layout/components/GlobalSearch.vue` — replace `$t()` with locale access

## 6. Testing

- **TEST-001**: `pnpm type-check` — zero errors from GlobalSearch.vue
- **TEST-002**: `pnpm build-only` — passes
- **TEST-003**: `pnpm test:unit` — all 107 tests pass

## 7. Risks & Assumptions

- **RISK-001**: None — the 2 locale strings and the 2 template replacements are purely additive, with no runtime behavior change from the current `||` fallbacks
- **ASSUMPTION-001**: The `GeneralLocales` interface allows extension via the `layout` key (it may need a new optional section added)

## 8. Related Specifications / Further Reading

- `plan/refactor-admin-api-layer-1.md` — prior phase contexts
- `plan/fix-type-errors-admin-2.md` — prior type-fix phase
- `plan/refactor-admin-query-params-3.md` — prior query-param alignment phase
