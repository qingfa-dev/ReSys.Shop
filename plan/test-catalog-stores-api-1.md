---
goal: Add unit tests for catalog stores, API classes, getPagedList helper, enforce consistent naming across all modules, and achieve 85%+ shared coverage
version: 2.0
date_created: 2025-07-25
last_updated: 2025-07-25
owner: Agent
status: In progress
tags: test, catalog, pinia, api-coverage, patterns, migration, all-modules
---

# Introduction

![Status: In progress](https://img.shields.io/badge/status-In%20progress-yellow)

Phase 1-4 (completed): unit tests for 3 catalog Pinia stores, 2 catalog API classes, `getPagedList` / `defaultListQuery` helpers in the shared layer, and `TaxonApi` / `OptionValueApi` contract tests.

Phase 5 (new): Migrate 8 stub modules (inventory, ordering, payment, shipping, location, users, profile, reports) from the old `models/` directory pattern to the canonical `types/` + `schemas/` pattern matching `auth/` and `catalog/`. This unifies file naming and directory structure across ALL 10 feature modules.

Phase 6 (new): Lift shared-layer coverage to 85%+ by adding tests for untested composables, utilities, models, validators, and constants.

## 1. Requirements & Constraints

### Existing (Phase 1-4)
- **REQ-001**: Every store action (fetchMany, setPage, setSearch, setSort, setFilter, resetQuery) must be tested for the happy path
- **REQ-002**: Store error paths must be tested: API returns `isSuccess: false`, and network exceptions
- **REQ-003**: Store initial state must be verified (loading=false, error=null, items=[], totalRecords=0)
- **REQ-004**: `getPagedList` must be tested directly with `apiClient.get` mocked, verifying URL and serialized params match
- **REQ-005**: `defaultListQuery` must be tested for default values, custom pageSize, and immutability
- **REQ-006**: All API classes (TaxonApi, OptionValueApi) must have standalone test files with CRUD coverage
- **SEC-001**: No secrets or tokens in test code
- **CON-001**: Tests must use `setActivePinia(createPinia())` — NOT `createTestingPinia`
- **CON-002**: API classes must be mocked with `vi.mock` at module level, not `vi.spyOn`
- **CON-003**: Store tests must import API class from the barrel (`'../api'`) to verify barrel exports work
- **CON-004**: Store tests must use `vi.hoisted` for mock factory functions
- **GUD-001**: Follow existing auth store test pattern (auth.store.spec.ts:52-60 for setup)
- **PAT-001**: Three stores are structurally identical — test `product.store.ts` exhaustively, then verify `taxonomy.store` and `option-type.store` with a lighter smoke test (initial state + fetchMany success)
- **PAT-002**: `PagedResult` factory helper extracted to avoid repetition

### New (Phase 5 — naming consistency)
- **REQ-007**: All 10 feature modules must use the same canonical directory structure per FEATURE_PATTERN.md: `api/`, `components/`, `composables/`, `pages/`, `store/`, `schemas/`, `types/`, `routes.ts`, `index.ts`. Optional: `mappers/`, `services/`, `utils/`
- **REQ-008**: Replace `models/` directory with `types/` in all 8 stub modules
- **REQ-009**: Add `schemas/` directory to all modules lacking it (8 stub modules)
- **REQ-010**: Rename `.gitkeep` stub files to remove them; remove empty files after migration
- **REQ-011**: Update `infrastructure module-structure.spec.ts` to verify all 10 modules (including `auth`) against the canonical pattern
- **CON-005**: No `.gitkeep` files in final module directories
- **CON-006**: All module `index.ts` barrels must re-export canonical names
- **GUD-002**: `catalog/` structure is the reference pattern for all modules

### New (Phase 6 — shared coverage 85%+)
- **REQ-012**: Each exported function/class in `shared/` must have 85%+ branch coverage
- **REQ-013**: Every shared composable must have a test file covering: initial state, happy path, error/edge case
- **REQ-014**: Every shared utility module must have a test file covering all exported functions
- **REQ-015**: Shared model types/interfaces must have construction/invariance tests
- **CON-007**: Shared component tests are excluded from the 85% threshold (requires component-test setup not yet in place)
- **CON-008**: Pure type-only modules (no runtime exports) are excluded from coverage requirement
- **GUD-003**: Prioritize composables and utilities with the highest usage count first
- **PAT-003**: Group tests by domain: `composables/__tests__/`, `utils/__tests__/`, `validation/__tests__/`, etc.

## 2. Implementation Steps

### Implementation Phase 1: Shared-layer helpers (COMPLETED)

- GOAL-001: Add unit tests for `defaultListQuery` and `getPagedList`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Add `defaultListQuery` unit test to existing `query-serializer.spec.ts`: verify returns correct defaults, custom pageSize, and returns a new object each call | ✅ | 2025-07-25 |
| TASK-002 | Add `getPagedList` direct test file at `shared/api/utils/__tests__/getPagedList.spec.ts`: mock `apiClient.get`, verify URL, params, and response passthrough | ✅ | 2025-07-25 |

### Implementation Phase 2: Product store exhaustive tests (COMPLETED)

- GOAL-002: Exhaustive test for `useProductStore` — covers all actions, both success and error paths

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-003 | Create `features/catalog/store/__tests__/product.store.spec.ts` with `setActivePinia(createPinia())` setup, mock `ProductApi` at module level | ✅ | 2025-07-25 |
| TASK-004 | Test initial state: loading=false, error=null, items=[], totalRecords=0, query has defaults | ✅ | 2025-07-25 |
| TASK-005 | Test `fetchMany` success: items and totalRecords populated, loading toggled | ✅ | 2025-07-25 |
| TASK-006 | Test `fetchMany` failure: API returns `isSuccess: false` — error set, items cleared | ✅ | 2025-07-25 |
| TASK-007 | Test `fetchMany` network error: exception caught — error set to "Failed to load", items cleared | ✅ | 2025-07-25 |
| TASK-008 | Test `setPage`: query.page updated, fetchMany called, state reflects new page | ✅ | 2025-07-25 |
| TASK-009 | Test `setSearch`: query.search set with mode 'Any', page reset to 1, fetchMany called | ✅ | 2025-07-25 |
| TASK-010 | Test `setSort`: query.sort updated with single clause, fetchMany called | ✅ | 2025-07-25 |
| TASK-011 | Test `setFilter`: query.filters set, page reset to 1, fetchMany called | ✅ | 2025-07-25 |
| TASK-012 | Test `resetQuery`: query restored to defaults, fetchMany called | ✅ | 2025-07-25 |

### Implementation Phase 3: Taxonomy and OptionType store smoke tests (COMPLETED)

- GOAL-003: Verify taxonomy and option-type stores work identically to product store with lighter coverage

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-013 | Create `features/catalog/store/__tests__/taxonomy.store.spec.ts`: test initial state + fetchMany success (smoke test) | ✅ | 2025-07-25 |
| TASK-014 | Create `features/catalog/store/__tests__/option-type.store.spec.ts`: test initial state + fetchMany success (smoke test) | ✅ | 2025-07-25 |

### Implementation Phase 4: Taxon and OptionValue API standalone tests (COMPLETED)

- GOAL-004: Add standalone contract tests for the 2 API classes currently covered only as secondary in other test files

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-015 | Create `features/catalog/api/__tests__/taxons.spec.ts`: mock `apiClient`, test getMany/create/update/delete for `TaxonApi` | ✅ | 2025-07-25 |
| TASK-016 | Create `features/catalog/api/__tests__/optionValues.spec.ts`: mock `apiClient`, test getMany/create/update/delete for `OptionValueApi` | ✅ | 2025-07-25 |

### Implementation Phase 5: Consistent naming patterns across all modules

- GOAL-005: Migrate all 8 stub modules from the old `models/` directory to the canonical `types/` + `schemas/` pattern, aligning with FEATURE_PATTERN.md

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-017 | For each stub module (inventory, ordering, payment, shipping, location, users, profile, reports): (a) create `types/` dir, (b) create placeholder `types/index.ts` exporting empty type stubs, (c) remove `models/` dir entirely | | |
| TASK-018 | For each stub module: create `schemas/` dir with placeholder `index.ts` | | |
| TASK-019 | Create `mappers/` directory in modules that will need request/response mapping (inventory, ordering, payment, shipping) — with placeholder `index.ts` | | |
| TASK-020 | Remove all `.gitkeep` files from all 8 stub modules (they serve no purpose after directory migration) | | |
| TASK-021 | Update each stub module's `index.ts` barrel to export from `types/` and `schemas/` instead of `models/` | | |
| TASK-022 | Update infrastructure test (`module-structure.spec.ts`): (a) add `auth` module to the iteration, (b) change required dir check from `models/` to `types/`, (c) add `schemas/` as a required directory, (d) remove `models/` as a required directory | | |

### Implementation Phase 6: Shared-layer coverage to 85%+

- GOAL-006: Lift shared-layer test coverage to 85%+ by adding tests for untested composables, utilities, models, validators, and constants

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-023 | Add `usePagedList` test for `params` property (the 1 uncovered export): verify `params` returns `QueryingModel` with defaults. File: `shared/composables/__tests__/paged-list.spec.ts` | | |
| TASK-024 | Create tests for `useDebounce`: verify debounced function fires after delay, cancels on rapid calls. File: `shared/composables/__tests__/debounce.spec.ts` | | |
| TASK-025 | Create tests for `useCurrency`: verify formatCurrency, formatCompact, parseCurrency. File: `shared/composables/__tests__/currency.spec.ts` | | |
| TASK-026 | Create tests for `useDate`: verify formatDate, formatRelative, formatTime. File: `shared/composables/__tests__/date.spec.ts` | | |
| TASK-027 | Create tests for `useConfirm`: verify dialog opens, confirm/cancel handlers. File: `shared/composables/__tests__/confirm.spec.ts` | | |
| TASK-028 | Create tests for `useModal`: verify open/close state, payload passing. File: `shared/composables/__tests__/modal.spec.ts` | | |
| TASK-029 | Create tests for `useToast`: verify add/dismiss, types (success/error/info). File: `shared/composables/__tests__/toast.spec.ts` | | |
| TASK-030 | Create tests for `usePagination`: verify page change, page range calculation. File: `shared/composables/__tests__/pagination.spec.ts` | | |
| TASK-031 | Create tests for `useDarkMode`: verify toggle, localStorage persistence. File: `shared/composables/__tests__/dark-mode.spec.ts` | | |
| TASK-032 | Create tests for `useResponsive`: verify breakpoint detection. File: `shared/composables/__tests__/responsive.spec.ts` | | |
| TASK-033 | Create tests for `shared/utils/currency.ts`: verify formatCurrency, parseCurrency, formatCompact. File: `shared/utils/__tests__/currency.spec.ts` | | |
| TASK-034 | Create tests for `shared/utils/throttle.ts`: verify throttled function fires at most once per interval. File: `shared/utils/__tests__/throttle.spec.ts` | | |
| TASK-035 | Create tests for `shared/utils/transform.ts`: verify all transformation functions. File: `shared/utils/__tests__/transform.spec.ts` | | |
| TASK-036 | Create tests for `shared/validation/rules.ts` and `validators.ts`: verify each validation rule with valid/invalid inputs. File: `shared/validation/__tests__/rules.spec.ts` | | |
| TASK-037 | Create tests for `shared/constants/permissions.ts`: verify permission string constants match expected format. File: `shared/constants/__tests__/permissions.spec.ts` | | |
| TASK-038 | Create tests for `shared/constants/regex.ts`: verify each regex matches/passes expected patterns. File: `shared/constants/__tests__/regex.spec.ts` | | |
| TASK-039 | Create tests for `shared/utils/status.ts`: verify status mapping functions. File: `shared/utils/__tests__/status.spec.ts` | | |
| TASK-040 | Create tests for `shared/directives/clickOutside.ts`: verify directive triggers callback on outside click. File: `shared/directives/__tests__/clickOutside.spec.ts` | | |
| TASK-041 | Run coverage report: `npx vitest run --coverage` and verify shared directory overall coverage is 85%+. If below threshold, identify gaps and add tasks to fill them | | |

## 3. Alternatives

- **ALT-001**: Use `createTestingPinia` — rejected per CON-001. `createTestingPinia` adds spy requirements and does not work with the existing test patterns.
- **ALT-002**: Parameterized store tests (single spec file testing all 3 stores with a loop) — rejected because vitest module mocking is per-file. Three spec files with distinct `vi.mock` calls avoids mock pollution.
- **ALT-003**: Keep `models/` dir alongside `types/` — rejected per REQ-007/REQ-008. A single canonical pattern (`types/`) prevents confusion.
- **ALT-004**: Add coverage to `shared/components/` — deferred per CON-007. Component testing requires a separate setup (cypress component test runner or vitest with additional plugins) not yet configured.

## 4. Dependencies

- **DEP-001**: `vitest` and `pinia` already in `package.json` devDependencies
- **DEP-002**: `@/shared/api/client` mock already established in existing API test files
- **DEP-003**: `PagedResult<T>`, `Result<T>` types already defined in `shared/models/result.ts`
- **DEP-004**: `@vitest/coverage-v8` already installed in `package.json` devDependencies (for coverage reporting)
- **DEP-005**: `vue-router` and `@vueuse/core` mocked in composable tests as needed

## 5. Files

### Phase 1-4 files (EXISTING)
- **FILE-001**: `app/Admin/src/shared/api/utils/__tests__/query-serializer.spec.ts` — defaultListQuery tests added
- **FILE-002**: `app/Admin/src/shared/api/utils/__tests__/getPagedList.spec.ts` — NEW: getPagedList tests
- **FILE-003**: `app/Admin/src/features/catalog/store/__tests__/product.store.spec.ts` — NEW: exhaustive store tests
- **FILE-004**: `app/Admin/src/features/catalog/store/__tests__/taxonomy.store.spec.ts` — NEW: smoke test
- **FILE-005**: `app/Admin/src/features/catalog/store/__tests__/option-type.store.spec.ts` — NEW: smoke test
- **FILE-006**: `app/Admin/src/features/catalog/api/__tests__/taxons.spec.ts` — NEW: standalone TaxonApi tests
- **FILE-007**: `app/Admin/src/features/catalog/api/__tests__/optionValues.spec.ts` — NEW: standalone OptionValueApi tests

### Phase 5 files (naming consistency)
- **FILE-008**: `app/Admin/src/features/inventory/` — migrate `models/` → `types/` + `schemas/`
- **FILE-009**: `app/Admin/src/features/ordering/` — migrate `models/` → `types/` + `schemas/`
- **FILE-010**: `app/Admin/src/features/payment/` — migrate `models/` → `types/` + `schemas/`
- **FILE-011**: `app/Admin/src/features/shipping/` — migrate `models/` → `types/` + `schemas/`
- **FILE-012**: `app/Admin/src/features/location/` — migrate `models/` → `types/` + `schemas/`
- **FILE-013**: `app/Admin/src/features/users/` — migrate `models/` → `types/` + `schemas/`
- **FILE-014**: `app/Admin/src/features/profile/` — migrate `models/` → `types/` + `schemas/`
- **FILE-015**: `app/Admin/src/features/reports/` — migrate `models/` → `types/` + `schemas/`
- **FILE-016**: `app/Admin/src/__tests__/infrastructure/module-structure.spec.ts` — update dir checks

### Phase 6 files (shared coverage)
- **FILE-017**: `app/Admin/src/shared/composables/__tests__/paged-list.spec.ts` — add `params` test
- **FILE-018**: `app/Admin/src/shared/composables/__tests__/debounce.spec.ts` — NEW
- **FILE-019**: `app/Admin/src/shared/composables/__tests__/currency.spec.ts` — NEW
- **FILE-020**: `app/Admin/src/shared/composables/__tests__/date.spec.ts` — NEW
- **FILE-021**: `app/Admin/src/shared/composables/__tests__/confirm.spec.ts` — NEW
- **FILE-022**: `app/Admin/src/shared/composables/__tests__/modal.spec.ts` — NEW
- **FILE-023**: `app/Admin/src/shared/composables/__tests__/toast.spec.ts` — NEW
- **FILE-024**: `app/Admin/src/shared/composables/__tests__/pagination.spec.ts` — NEW
- **FILE-025**: `app/Admin/src/shared/composables/__tests__/dark-mode.spec.ts` — NEW
- **FILE-026**: `app/Admin/src/shared/composables/__tests__/responsive.spec.ts` — NEW
- **FILE-027**: `app/Admin/src/shared/utils/__tests__/currency.spec.ts` — NEW
- **FILE-028**: `app/Admin/src/shared/utils/__tests__/throttle.spec.ts` — NEW
- **FILE-029**: `app/Admin/src/shared/utils/__tests__/transform.spec.ts` — NEW
- **FILE-030**: `app/Admin/src/shared/utils/__tests__/status.spec.ts` — NEW
- **FILE-031**: `app/Admin/src/shared/validation/__tests__/rules.spec.ts` — NEW
- **FILE-032**: `app/Admin/src/shared/constants/__tests__/permissions.spec.ts` — NEW
- **FILE-033**: `app/Admin/src/shared/constants/__tests__/regex.spec.ts` — NEW
- **FILE-034**: `app/Admin/src/shared/directives/__tests__/clickOutside.spec.ts` — NEW

## 6. Testing

- **TEST-001**: All test files must pass via `npx vitest run` — zero failures
- **TEST-002**: TypeScript must pass via `npx vue-tsc --noEmit` — zero errors
- **TEST-003**: Coverage must be verified via `npx vitest run --coverage` — shared directory overall must be 85%+
- **TEST-004**: Infrastructure test must pass with updated module structure checks

## 7. Risks & Assumptions

- **RISK-001**: Store tests start with `setActivePinia(createPinia())` but may require `localStorage` or `navigate` mocking if stores expand — not needed for current catalog stores
- **RISK-002**: `getPagedList` test mocks `apiClient.get` via `vi.mock('@/shared/api/client')` — if the mock path alias resolution fails, use relative path `../../client` instead
- **RISK-003**: Composable tests that use `useRouter`, `useToast`, or `useConfirm` may need module mocking of `vue-router`, `primevue/usetoast`, etc.
- **RISK-004**: The 85% coverage target may require additional tests beyond the listed tasks if the coverage report reveals gaps — TASK-041 will handle this iteratively
- **RISK-005**: Module migration (models/ → types/ + schemas/) will affect page imports that reference `../../models/` — must update all import paths in page files simultaneously
- **ASSUMPTION-001**: The 3 catalog stores are structurally identical enough that smoke testing taxonomy and option-type after exhaustive product store tests provides adequate coverage
- **ASSUMPTION-002**: `vi.hoisted` can be used for mock factories as in auth store tests
- **ASSUMPTION-003**: Removing `.gitkeep` files from stub modules will not break anything (they are empty placeholders)
- **ASSUMPTION-004**: Existing page files in stub modules that import from `../../models/` will be updated in TASK-017 to import from `../../types/` instead

## 8. Related Specifications / Further Reading

[FEATURE_PATTERN.md](../app/Admin/docs/FEATURE_PATTERN.md)
[2025-07-25-universal-query-converter-design.md](../app/Admin/docs/specs/2025-07-25-universal-query-converter-design.md)
