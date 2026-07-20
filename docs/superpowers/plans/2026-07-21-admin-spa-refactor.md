# Admin SPA Refactor — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Restructure the Admin SPA to 4-layer architecture (app→features→common→shared), apply consistent naming conventions, add shared Zod field inheritance, reorganize shared components, fix 18 code review issues.

**Architecture:** Big Bang by Layer — work bottom-up through `shared/` → `common/` → `features/` → `app/`. Each task produces a verifiable gate (type-check + lint + unit tests pass). Mechanical renames use `git mv` + `sed` bulk replacements. Code fixes use targeted edits.

**Tech Stack:** Vue 3.5, TypeScript 6, Vite 8, Pinia 3, PrimeVue 4, Zod 3, Axios, vitest

## Global Constraints

- `pnpm run type-check` must pass (vue-tsc --build, ~22s)
- `pnpm run lint` must pass (eslint + oxlint)
- `pnpm run test:unit` must pass (vitest, all 50+ tests green)
- Use `git mv` for all file relocations (preserves history)
- 4-layer boundary via eslint-plugin-boundaries (shared ∄ common/features/app; common ∄ features/app; features ∄ app)
- Component naming: `PascalCasePage.vue` (pages), `PascalCaseLayout.vue` (layouts), `PascalCase.vue` (components)
- Type files: drop `.type` infix (.model.type.ts → .model.ts)
- Test dirs: `__tests__/` colocated with source, `*.spec.ts` naming
- Store splitting only for stores >150 lines or >10 actions (currently only `order.store.ts` at ~184 lines qualifies)
- Zod field defs in `types/*.field.ts`, compose from `shared/fields/`
- `services/` + `mappers/` merge into `api/` + `models/`
- `DataTableShell` stays single component (not split)

## File Map

### Step 1 files (create common/, ~40 files moved)

| Operation | Path |
|-----------|------|
| Create dir | `src/common/api/http/interceptors/` |
| Create dir | `src/common/api/http/handlers/` |
| Create dir | `src/common/api/services/` |
| Create dir | `src/common/api/types/` |
| Create dir | `src/common/api/utils/` |
| Create dir | `src/common/composables/` |
| Create dir | `src/common/services/` |
| Create dir | `src/common/mapper/` |
| Create dir | `src/common/utils/` |
| Create dir | `src/common/config/` |
| Create dir | `src/common/test/` |
| Create dir | `src/common/errors/` |
| Move | `src/shared/api/http/api.client.ts` → `src/common/api/http/api.client.ts` |
| Move | `src/shared/api/http/api.file.service.ts` → `src/common/api/http/api.file.service.ts` |
| Move | `src/shared/api/http/interceptors/auth.interceptor.ts` → `src/common/api/http/interceptors/auth.interceptor.ts` |
| Move | `src/shared/api/http/interceptors/camelcase.interceptor.ts` → `src/common/api/http/interceptors/camelcase.interceptor.ts` |
| Move | `src/shared/api/http/interceptors/error-wrapper.interceptor.ts` → `src/common/api/http/interceptors/error-wrapper.interceptor.ts` |
| Move | `src/shared/api/http/handlers/error-handler.ts` → `src/common/api/http/handlers/error-handler.ts` |
| Move | `src/shared/api/http/handlers/refresh-handler.ts` → `src/common/api/http/handlers/refresh-handler.ts` |
| Move | `src/shared/api/services/module-api.factory.ts` → `src/common/api/services/module-api.factory.ts` |
| Move | `src/shared/api/types/result.types.ts` → `src/common/api/types/result.types.ts` |
| Move | `src/shared/api/types/query.types.ts` → `src/common/api/types/query.types.ts` |
| Move | `src/shared/api/types/filter.types.ts` → `src/common/api/types/filter.types.ts` |
| Move | `src/shared/api/types/metadata.types.ts` → `src/common/api/types/metadata.types.ts` |
| Move | `src/shared/api/types/api.file.types.ts` → `src/common/api/types/api.file.types.ts` |
| Move | `src/shared/api/types/index.ts` → `src/common/api/types/index.ts` |
| Move | `src/shared/api/utils/api.utils.ts` → `src/common/api/utils/api.utils.ts` |
| Move | `src/shared/api/utils/result.mapper.ts` → `src/common/api/utils/result.mapper.ts` |
| Move | `src/shared/api/constants.ts` → `src/common/api/constants.ts` |
| Move | `src/shared/api/index.ts` → `src/common/api/index.ts` |
| Move | `src/shared/composables/paged-list.use.ts` → `src/common/composables/paged-list.use.ts` |
| Move | `src/shared/composables/toast.use.ts` → `src/common/composables/toast.use.ts` |
| Move | `src/shared/composables/api-error-handler.use.ts` → `src/common/composables/api-error-handler.use.ts` |
| Move | `src/shared/composables/file-preview.use.ts` → `src/common/composables/file-preview.use.ts` |
| Move | `src/shared/composables/formatter.use.ts` → `src/common/composables/formatter.use.ts` |
| Move | `src/shared/services/search.service.ts` → `src/common/services/search.service.ts` |
| Move | `src/shared/mapper/mapper.utils.ts` → `src/common/mapper/mapper.utils.ts` |
| Move | `src/shared/utils/query-builder.utils.ts` → `src/common/utils/query-builder.utils.ts` |
| Move | `src/shared/utils/transform.ts` → `src/common/utils/transform.ts` |
| Move | `src/shared/config/app.ts` → `src/common/config/app.ts` |
| Move | `src/shared/test/mock-types.ts` → `src/common/test/mock-types.ts` |
| Create | `src/common/errors/ApiError.ts` |
| Delete | `src/shared/api/types/api.types.ts` (duplicate — functions/types already in result.mapper.ts) |
| Move tests | `src/shared/**/__tests__/*` → `src/common/**/__tests__/*` |
| Move tests | `src/shared/**/*.spec.ts` → colocated under `src/common/` |
| Modify | `eslint.config.ts` (add common layer) |
| Modify | `vite.config.ts` (auto-import dirs update) |
| Modify | ~165 files in `src/features/` (~280 import path updates) |
| Modify | ~2 files in `src/app/` (import path updates) |

### Step 2 files (reorganize shared/)

| Operation | Path |
|-----------|------|
| Create dir | `src/shared/components/base/` |
| Create dir | `src/shared/components/form/` |
| Create dir | `src/shared/components/tables/` |
| Create dir | `src/shared/components/data-display/` |
| Create dir | `src/shared/components/feedback/` |
| Create dir | `src/shared/components/navigation/` |
| Create dir | `src/shared/fields/` |
| Move+rename | `src/shared/components/ConfirmButton.Component.vue` → `src/shared/components/base/ConfirmButton.vue` |
| Move+rename | `src/shared/components/FormField.Component.vue` → `src/shared/components/form/FormField.vue` |
| Move+rename | `src/shared/components/DataTableShell.Component.vue` → `src/shared/components/tables/DataTableShell.vue` |
| Move+rename | `src/shared/components/StatCard.Component.vue` → `src/shared/components/data-display/StatCard.vue` |
| Move+rename | `src/shared/components/DetailField.Component.vue` → `src/shared/components/data-display/DetailField.vue` |
| Move+rename | `src/shared/components/TabbedDetail.Component.vue` → `src/shared/components/data-display/TabbedDetail.vue` |
| Move+rename | `src/shared/components/MetadataManager.Component.vue` → `src/shared/components/data-display/MetadataManager.vue` |
| Move files | Recently added components into subdirs (SearchInput→form, DescriptionList/DetailGroup/CopyButton→data-display, SkeletonLoader/DeleteDialog/LoadingOverlay→feedback, PageContainer/Section→navigation) |
| Move+rename | `src/shared/components/EmptyState.Component.vue` → `src/shared/components/feedback/EmptyState.vue` |
| Move+rename | `src/shared/components/StatusBadge.Component.vue` → `src/shared/components/feedback/StatusBadge.vue` |
| Move+rename | `src/shared/components/Breadcrumb.Component.vue` → `src/shared/components/navigation/Breadcrumb.vue` |
| Move+rename | `src/shared/components/PageShell.Component.vue` → `src/shared/components/navigation/PageShell.vue` |
| Move+rename | `src/shared/components/PageHeader.Component.vue` → `src/shared/components/navigation/PageHeader.vue` |
| Move+rename | `src/shared/components/ManagerWelcome.Component.vue` → `src/shared/components/navigation/ManagerWelcome.vue` |
| Create | `src/shared/fields/base.field.ts` |
| Create | `src/shared/fields/name.field.ts` |
| Create | `src/shared/fields/address.field.ts` |
| Create | `src/shared/fields/money.field.ts` |
| Create | `src/shared/fields/seo.field.ts` |
| Create | `src/shared/fields/index.ts` |
| Modify | ~49 feature files (~114 shared component import path updates) |

### Step 3 files (restructure features)

| Operation | Path (per feature) |
|-----------|---------------------|
| Move+rename | `views/*.View.vue` → `pages/*Page.vue` (22 view files) |
| Rename dir | `stores/` → `store/` (rename directories per feature) |
| Move+rename | `schemas/*.schema.ts` → `types/*.field.ts` (~25 schema files) |
| Rename | `types/*.model.type.ts` → `types/*.model.ts` |
| Rename | `types/*.request.type.ts` → `types/*.request.ts` |
| Rename | `types/*.response.type.ts` → `types/*.response.ts` |
| Rename | `types/*.parameters.type.ts` → `types/*.parameters.ts` |
| Rename | `types/*.query.type.ts` → `types/*.query.ts` |
| Rename | Feature components drop `.Component` suffix |
| Merge | `services/*.service.ts` content → `api/*.api.ts` |
| Merge | `mappers/*.mapper.ts` content → `models/*.model.ts` |
| Standardize | `_tests/` + `tests/` → `__tests__/` (16 dirs) |
| Create | `index.ts` barrel per feature |
| Create | `composables/` directory per feature |
| Create | `models/` directory per feature (where missing) |
| Rename dir | `app/layout/` — all `*.Layout.vue` → remove `.Layout` qualifier |
| Modify | `src/app/router/index.ts` (view import paths) |
| Modify | Feature test files (update imports after renames) |

### Step 4 files (new components)

| Operation | Path |
|-----------|------|
| Create | `src/shared/components/feedback/ErrorState.vue` |
| Create | `src/shared/components/feedback/__tests__/ErrorState.test.ts` |
| Create | `src/shared/components/feedback/Drawer.vue` |
| Create | `src/shared/components/feedback/__tests__/Drawer.test.ts` |

---
