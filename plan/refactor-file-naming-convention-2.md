---
goal: Rename Admin SPA Files to Kebab-Case with Dotted Convention
version: 1.0
date_created: 2026-07-20
owner: Admin SPA Team
status: Planned
tags:
  - refactor
  - naming
  - admin-spa
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Rename all files under `app/Admin/src/` to match the legacy Admin SPA naming convention:
- **kebab-case** for all segments (lowercase, hyphens between words)
- **Dotted suffix** indicating file role: `.api.ts`, `.store.ts`, `.schema.ts`, `.type.ts`, `.service.ts`, `.interceptor.ts`, `.handler.ts`, `.mapper.ts`, `.utils.ts`, `.client.ts`, `.builder.ts`, `.model.ts`, `.View.vue`, `.Layout.vue`

Pattern reference: `app/lagacy/Admin/src/` — e.g., `login.schema.ts`, `auth.store.ts`, `option-type.api.ts`, `Login.View.vue`.

## 1. Requirements & Constraints

- **REQ-001**: All filenames use kebab-case segments separated by dots for role suffix (e.g., `auth.store.ts`, `error-handler.ts`, `query-string.builder.ts`).
- **REQ-002**: Vue component filenames keep `PascalCase` for the component name portion: `PascalName.View.vue`, `PascalName.Layout.vue`.
- **REQ-003**: Every renamed file must update all import paths referencing it across the full `src/` tree.
- **REQ-004**: Zero build errors after rename — `vue-tsc --noEmit` and `vite build` must pass.
- **CON-001**: Do not rename files that already follow the convention.
- **CON-002**: `index.ts`, `main.ts`, `auto-imports.d.ts`, `env.d.ts` — conventional names, keep as-is.
- **PAT-001**: Follow legacy pattern exactly: `{entity}.{role}.{ext}` — e.g., `auth.api.ts`, `login.request.type.ts`, `camel-case.interceptor.ts`.

## 2. Implementation Steps

### Implementation Phase 1 — Outlier Renames

- GOAL-001: Rename the 2 files that deviate from kebab-case.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Rename `src/shared/api/query/QueryStringBuilder.ts` → `query-string.builder.ts`. Update import in `query/index.ts`. | | |
| TASK-002 | Rename `src/shared/api/http/interceptors/camelcase.interceptor.ts` → `camel-case.interceptor.ts`. Update import in `api.client.ts`. | | |

### Implementation Phase 2 — Model File Suffix Normalization

- GOAL-002: Add `.model.ts` suffix to flat model files for role clarity.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-003 | Rename `src/shared/models/errors.ts` → `error.model.ts`. Rename `ErrorModel` import in `result.types.ts`. Update `models/index.ts`. | | |
| TASK-004 | Rename `src/shared/models/pagination.ts` → `pagination.model.ts`. Update `models/index.ts`. | | |
| TASK-005 | Rename `src/shared/models/filtering.ts` → `filtering.model.ts`. Update imports in `QueryStringBuilder`. Update `models/index.ts`. | | |
| TASK-006 | Rename `src/shared/models/sorting.ts` → `sorting.model.ts`. Update imports in `QueryStringBuilder`. Update `models/index.ts`. | | |
| TASK-007 | Rename `src/shared/models/searching.ts` → `searching.model.ts`. Update imports in `QueryStringBuilder`. Update `models/index.ts`. | | |
| TASK-008 | Rename `src/shared/models/parameters.ts` → `parameter.model.ts`. Update `models/index.ts`. | | |
| TASK-009 | Rename `src/shared/models/responses.ts` → `response.model.ts`. Update `models/index.ts`. | | |

### Implementation Phase 3 — Shared API File Suffix Normalization

- GOAL-003: Normalize shared API filenames to use consistent dotted suffixes.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-010 | Rename `src/shared/api/http/api.client.ts` → `api.client.ts` (already correct) — no-op. | | |
| TASK-011 | Rename `src/shared/api/http/handlers/error-handler.ts` → `error.handler.ts`. Update `api.client.ts` import. | | |
| TASK-012 | Rename `src/shared/api/http/handlers/refresh-handler.ts` → `refresh.handler.ts`. Update `api.client.ts` import. | | |
| TASK-013 | Rename `src/shared/api/http/interceptors/auth.interceptor.ts` → `auth.interceptor.ts` (already correct) — no-op. | | |
| TASK-014 | Rename `src/shared/api/utils/api.utils.ts` → `api.utils.ts` (already correct) — no-op. | | |
| TASK-015 | Rename `src/shared/mapper/mapper.utils.ts` → `mapper.utils.ts` (already correct) — no-op. | | |
| TASK-016 | Rename `src/shared/api/types/result.types.ts` → `result.type.ts`. Update `api.client.ts`, `error.handler.ts`, `refresh.handler.ts`, `auth.api.ts`, `auth.store.ts`, `shared/index.ts`. | | |

### Implementation Phase 4 — Feature Auth File Suffix Normalization

- GOAL-004: Normalize auth feature filenames.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-017 | Rename `src/features/auth/api/auth.api.ts` → `auth.api.ts` (already correct) — no-op. | | |
| TASK-018 | Rename `src/features/auth/schemas/login.schema.ts` → `login.schema.ts` (already correct) — no-op. | | |
| TASK-019 | Rename `src/features/auth/services/auth.service.ts` → `auth.service.ts` (already correct) — no-op. | | |
| TASK-020 | Rename `src/features/auth/stores/auth.store.ts` → `auth.store.ts` (already correct) — no-op. | | |
| TASK-021 | Rename `src/features/auth/mappers/auth.mapper.ts` → `auth.mapper.ts` (already correct) — no-op. | | |
| TASK-022 | Rename `src/features/auth/types/auth.model.type.ts` → `auth.model.type.ts` (already correct) — no-op. | | |
| TASK-023 | Rename `src/features/auth/types/auth.request.type.ts` → `auth.request.type.ts` (already correct) — no-op. | | |
| TASK-024 | Rename `src/features/auth/types/login.request.type.ts` → `login.request.type.ts` (already correct) — no-op. | | |
| TASK-025 | Rename `src/features/auth/types/login.response.type.ts` → `login.response.type.ts` (already correct) — no-op. | | |
| TASK-026 | Rename `src/features/auth/views/Login.View.vue` → `login.view.vue`. Update `router/index.ts`. Note: legacy uses PascalCase for view names — keep `Login.View.vue` if matching legacy exactly. | | |
| TASK-027 | Rename `src/features/dashboard/views/Dashboard.View.vue` → `dashboard.view.vue`. Update `router/index.ts`. | | |
| TASK-028 | Rename `src/features/error/views/NotFound.View.vue` → `not-found.view.vue`. Update `router/index.ts`. | | |

### Implementation Phase 5 — App Shell File Suffix Normalization

- GOAL-005: Normalize app shell filenames.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-029 | Rename `src/app/layout/AppFooter.vue` → `app-footer.layout.vue`. Update `AppLayout.vue`. | | |
| TASK-030 | Rename `src/app/layout/AppLayout.vue` → `app-layout.layout.vue`. Update `router/index.ts`. | | |
| TASK-031 | Rename `src/app/layout/AppTopbar.vue` → `app-topbar.layout.vue`. Update `AppLayout.vue`. | | |
| TASK-032 | Rename `src/app/layout/AppSidebar.vue` → `app-sidebar.layout.vue`. Update `AppLayout.vue`. | | |
| TASK-033 | Rename `src/app/layout/AppMenu.vue` → `app-menu.layout.vue`. Update `AppSidebar.vue`. | | |
| TASK-034 | Rename `src/app/layout/AppMenuItem.vue` → `app-menu.item.vue`. Update `AppMenu.vue`. | | |
| TASK-035 | Rename `src/app/layout/composables/layout.ts` → `layout.composable.ts`. Update all files importing `@/app/layout/composables/layout`. | | |

### Implementation Phase 6 — Final Verification

- GOAL-006: Verify all imports resolve, type-check, and build.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-036 | Run `vue-tsc --noEmit` — must pass with zero errors. | | |
| TASK-037 | Run `vite build` — must succeed. | | |
| TASK-038 | Run `pnpm run lint` — must pass. | | |

## 3. Alternatives

- **ALT-001**: Keep current mixed naming. Rejected — consistency with legacy Admin simplifies cross-referencing during feature migration and reduces cognitive overhead.
- **ALT-002**: Use all-lowercase for Vue components too (e.g., `login.view.vue`). Rejected for view/layout components — legacy uses PascalCase.View.vue pattern and it helps distinguish component names from entity names at a glance.
- **ALT-003**: Apply `PascalCase` to all shared files too. Rejected — kebab-case is the standard for non-component TypeScript files in this project and across the wider JavaScript ecosystem.

## 4. Dependencies

- **DEP-001**: `@` alias (`@/` → `src/`) configured in `vite.config.ts` — required for import path updates.
- **DEP-002**: No external tooling needed — file renames and import updates are manual.

## 5. Files

- **FILE-001**: `src/shared/api/query/QueryStringBuilder.ts` → RENAME to `query-string.builder.ts`
- **FILE-002**: `src/shared/api/query/index.ts` → MODIFY import
- **FILE-003**: `src/shared/api/http/interceptors/camelcase.interceptor.ts` → RENAME to `camel-case.interceptor.ts`
- **FILE-004**: `src/shared/api/http/api.client.ts` → MODIFY import
- **FILE-005**: `src/shared/models/errors.ts` → RENAME to `error.model.ts`
- **FILE-006**: `src/shared/models/pagination.ts` → RENAME to `pagination.model.ts`
- **FILE-007**: `src/shared/models/filtering.ts` → RENAME to `filtering.model.ts`
- **FILE-008**: `src/shared/models/sorting.ts` → RENAME to `sorting.model.ts`
- **FILE-009**: `src/shared/models/searching.ts` → RENAME to `searching.model.ts`
- **FILE-010**: `src/shared/models/parameters.ts` → RENAME to `parameter.model.ts`
- **FILE-011**: `src/shared/models/responses.ts` → RENAME to `response.model.ts`
- **FILE-012**: `src/shared/models/index.ts` → MODIFY all imports
- **FILE-013**: `src/shared/api/http/handlers/error-handler.ts` → RENAME to `error.handler.ts`
- **FILE-014**: `src/shared/api/http/handlers/refresh-handler.ts` → RENAME to `refresh.handler.ts`
- **FILE-015**: `src/shared/api/types/result.types.ts` → RENAME to `result.type.ts`
- **FILE-016**: `src/shared/index.ts` → MODIFY imports
- **FILE-017**: `src/shared/api/utils/api.utils.ts` → import path updates
- **FILE-018**: `src/shared/mapper/mapper.utils.ts` → import path updates
- **FILE-019**: `src/features/auth/stores/auth.store.ts` → import path update for `result.type.ts`
- **FILE-020**: `src/features/auth/api/auth.api.ts` → import path update for `result.type.ts`
- **FILE-021**: `src/app/layout/AppFooter.vue` → RENAME to `app-footer.layout.vue`
- **FILE-022**: `src/app/layout/AppLayout.vue` → RENAME to `app-layout.layout.vue`
- **FILE-023**: `src/app/layout/AppTopbar.vue` → RENAME to `app-topbar.layout.vue`
- **FILE-024**: `src/app/layout/AppSidebar.vue` → RENAME to `app-sidebar.layout.vue`
- **FILE-025**: `src/app/layout/AppMenu.vue` → RENAME to `app-menu.layout.vue`
- **FILE-026**: `src/app/layout/AppMenuItem.vue` → RENAME to `app-menu.item.vue`
- **FILE-027**: `src/app/layout/composables/layout.ts` → RENAME to `layout.composable.ts`
- **FILE-028**: `src/app/router/index.ts` → MODIFY imports for renamed layout/view files
- **FILE-029**: `src/app/main.ts` → MODIFY import for `layout.composable.ts`
- **FILE-030**: `src/features/dashboard/views/Dashboard.View.vue` → RENAME to `dashboard.view.vue`
- **FILE-031**: `src/features/error/views/NotFound.View.vue` → RENAME to `not-found.view.vue`

## 6. Testing

- **TEST-001**: `vue-tsc --build --noEmit` — zero type errors
- **TEST-002**: `vite build` — production build succeeds
- **TEST-003**: `pnpm run lint` — linting passes
- **TEST-004**: Spot-check import resolution — `import { useLayout } from '@/app/layout/composables/layout'` → `import { useLayout } from '@/app/layout/composables/layout.composable'` resolves correctly

## 7. Risks & Assumptions

- **RISK-001**: Missed import references — a renamed file's import may be used in a file not explicitly listed. Mitigation: `vue-tsc --noEmit` catches all unresolved imports at compile time.
- **RISK-002**: Vue component name/auto-import mismatch — Vite may have issues resolving `.vue` files if the new kebab-case filename doesn't match component registration. Mitigation: Vue SFC resolution is case-insensitive on most filesystems; verify with build.
- **ASSUMPTION-001**: Filesystem is case-sensitive. Linux (ext4) is case-sensitive — renames must match exactly.
- **ASSUMPTION-002**: No external consumers import these files outside `app/Admin/src/` — all renames are self-contained within the Admin SPA.

## 8. Related Specifications / Further Reading

- [Legacy Admin naming reference — `app/lagacy/Admin/src/`](/home/qingfa/Repos/ReSys.Shop/app/lagacy/Admin/src/)
- [Coding Conventions — `docs/codebase/CONVENTIONS.md`](/home/qingfa/Repos/ReSys.Shop/docs/codebase/CONVENTIONS.md)
- [Harness enforcement — `.harness/enforcement.yml`](/home/qingfa/Repos/ReSys.Shop/.harness/enforcement.yml)
