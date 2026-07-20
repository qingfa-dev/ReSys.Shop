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

## Tasks

### Task 1: Move infrastructure files from shared/ → common/

**Files:** See File Map — Step 1 (Move operations)

**Interfaces:**
- Produces: `src/common/api/http/api.client.ts`, `src/common/api/types/*` (all types), `src/common/composables/*`, `src/common/mapper/mapper.utils.ts`, `src/common/utils/*`, `src/common/config/app.ts`, `src/common/test/mock-types.ts`, `src/common/services/search.service.ts`

- [ ] **Step 1: Create all common/ subdirectories**

```bash
app/Admin/src/shared/api
cd /home/qingfa/Repos/ReSys.Shop/app/Admin
mkdir -p src/common/api/http/interceptors
mkdir -p src/common/api/http/handlers
mkdir -p src/common/api/services
mkdir -p src/common/api/types
mkdir -p src/common/api/utils
mkdir -p src/common/composables
mkdir -p src/common/services
mkdir -p src/common/mapper
mkdir -p src/common/utils
mkdir -p src/common/config
mkdir -p src/common/test
mkdir -p src/common/errors
```

- [ ] **Step 2: Move API client + interceptors + handlers**

```bash
git mv src/shared/api/http/api.client.ts src/common/api/http/api.client.ts
git mv src/shared/api/http/api.file.service.ts src/common/api/http/api.file.service.ts
git mv src/shared/api/http/interceptors/auth.interceptor.ts src/common/api/http/interceptors/auth.interceptor.ts
git mv src/shared/api/http/interceptors/camelcase.interceptor.ts src/common/api/http/interceptors/camelcase.interceptor.ts
git mv src/shared/api/http/interceptors/error-wrapper.interceptor.ts src/common/api/http/interceptors/error-wrapper.interceptor.ts
git mv src/shared/api/http/handlers/error-handler.ts src/common/api/http/handlers/error-handler.ts
git mv src/shared/api/http/handlers/refresh-handler.ts src/common/api/http/handlers/refresh-handler.ts
git mv src/shared/api/services/module-api.factory.ts src/common/api/services/module-api.factory.ts
```

- [ ] **Step 3: Move API types**

```bash
git mv src/shared/api/types/result.types.ts src/common/api/types/result.types.ts
git mv src/shared/api/types/query.types.ts src/common/api/types/query.types.ts
git mv src/shared/api/types/filter.types.ts src/common/api/types/filter.types.ts
git mv src/shared/api/types/metadata.types.ts src/common/api/types/metadata.types.ts
git mv src/shared/api/types/api.file.types.ts src/common/api/types/api.file.types.ts
git mv src/shared/api/types/index.ts src/common/api/types/index.ts
```

- [ ] **Step 4: Move API utils + constants + barrel**

```bash
git mv src/shared/api/utils/api.utils.ts src/common/api/utils/api.utils.ts
git mv src/shared/api/utils/result.mapper.ts src/common/api/utils/result.mapper.ts
git mv src/shared/api/constants.ts src/common/api/constants.ts
git mv src/shared/api/index.ts src/common/api/index.ts
```

- [ ] **Step 5: Move composables**

```bash
git mv src/shared/composables/paged-list.use.ts src/common/composables/paged-list.use.ts
git mv src/shared/composables/toast.use.ts src/common/composables/toast.use.ts
git mv src/shared/composables/api-error-handler.use.ts src/common/composables/api-error-handler.use.ts
git mv src/shared/composables/file-preview.use.ts src/common/composables/file-preview.use.ts
git mv src/shared/composables/formatter.use.ts src/common/composables/formatter.use.ts
```

- [ ] **Step 6: Move services, mapper, utils, config, test**

```bash
git mv src/shared/services/search.service.ts src/common/services/search.service.ts
git mv src/shared/mapper/mapper.utils.ts src/common/mapper/mapper.utils.ts
git mv src/shared/utils/query-builder.utils.ts src/common/utils/query-builder.utils.ts
git mv src/shared/utils/transform.ts src/common/utils/transform.ts
git mv src/shared/config/app.ts src/common/config/app.ts
git mv src/shared/test/mock-types.ts src/common/test/mock-types.ts
```

- [ ] **Step 7: Verify file moves — old paths empty, new paths exist**

```bash
ls src/shared/api/http/ && echo "FAIL: old api dir should be empty" || echo "OK: old api dir cleaned"
ls src/common/api/http/api.client.ts && echo "OK: client moved" || echo "FAIL: client missing"
ls src/common/composables/paged-list.use.ts && echo "OK" || echo "FAIL"
ls src/common/mapper/mapper.utils.ts && echo "OK" || echo "FAIL"
```

- [ ] **Step 8: Update internal imports within moved files**

Files in `src/common/` that previously imported from `@/shared/...` now need `@/common/...` relative paths. Find and fix:

```bash
# Check which moved files still reference @/shared/ internally
rg "from '@/shared/" src/common/ --files-with-matches
```

For each file found, update the import path. Expected finds:
- `src/common/api/http/interceptors/error-wrapper.interceptor.ts` (imports from `../../types/result.types`, `../../utils/api.utils`, `../handlers/refresh-handler`, `../api.client` — these are relative already, should be fine)
- `src/common/api/http/handlers/refresh-handler.ts` (imports `../../types/result.types` — relative, OK)
- `src/common/api/utils/api.utils.ts` (re-exports `../http/handlers/error-handler` — relative, OK)
- `src/common/api/index.ts` (imports from `./http/api.client`, `./services/module-api.factory`, `./constants`, `./types`, `./http/handlers/refresh-handler` — relative, OK)

If any moved files use `@/shared/` in their own imports, fix to relative or `@/common/`:

```bash
# Bulk fix any @/shared/ references remaining in common/
find src/common -name '*.ts' -exec sed -i "s|from '@/shared/|from '@/common/|g" {} +
```

- [ ] **Step 9: Commit**

```bash
git add -A app/Admin/src/common app/Admin/src/shared
git commit -m "refactor(admin): move infrastructure from shared/ to common/"
```

---

### Task 2: Fix review issues in common/ files + delete api.types.ts

**Files:**
- Modify: `src/common/api/utils/result.mapper.ts`
- Modify: `src/common/api/services/module-api.factory.ts`
- Modify: `src/common/api/http/handlers/error-handler.ts`
- Modify: `src/common/composables/api-error-handler.use.ts`
- Modify: `src/common/composables/paged-list.use.ts`
- Modify: `src/common/composables/toast.use.ts`
- Modify: `src/common/utils/query-builder.utils.ts`
- Modify: `src/common/mapper/mapper.utils.ts`
- Modify: `src/common/api/constants.ts`
- Modify: `src/common/utils/transform.ts`
- Modify: `src/features/auth/api/auth.api.ts`
- Create: `src/common/errors/ApiError.ts`
- Delete: `src/shared/api/types/api.types.ts`
- Modify: `src/app/App.vue` (toast bridge removal)

**Interfaces:**
- Produces: Cleaned versions of all common/ files with fixes applied; no duplicate types; toast bridge removed; error class hierarchy started

- [ ] **Step 1: Fix #1 — Delete duplicate `api.types.ts`**

`src/shared/api/types/api.types.ts` duplicates `SuccessResult`, `FailureResult`, `MappedResult`, `mapToErrors` already in `result.mapper.ts`. Verify nothing imports it:

```bash
rg "from '@/shared/api/types/api.types'" src/features/ src/app/ src/shared/
```

Expected: zero results. Then delete:

```bash
rm src/shared/api/types/api.types.ts
```

- [ ] **Step 2: Fix #4 — Remove unused generics in `module-api.factory.ts`**

Read `src/common/api/services/module-api.factory.ts:7`. Change:

```typescript
export function createModuleApi<T, TCreate = Partial<T>, TUpdate = Partial<T>>(config: ModuleApiConfig) {
```

To:

```typescript
export function createModuleApi<T>(config: ModuleApiConfig) {
```

- [ ] **Step 3: Fix #5 — Log error in `auth.api.ts:21-29`**

Read `src/features/auth/api/auth.api.ts:21-29`. Change the `catch` block:

```typescript
  } catch { /* ignore */ }
```

To:

```typescript
  } catch (e) {
    console.error('[Auth] Failed to fetch session:', e)
  }
```

Note: This file is in `features/` not `common/`, but fixing here since it's an API-layer concern.

- [ ] **Step 4: Fix #6 — Guard debug logs in `api-error-handler.use.ts`**

Read `src/common/composables/api-error-handler.use.ts`. Wrap all `console.log` calls:

```typescript
// Line 19:
if (import.meta.env.DEV) console.log('[API Trace] Handler received parsed error:', apiError)

// Line 23:
if (import.meta.env.DEV) console.log('[API Trace] Validation error dictionary detected.')

// Line 44:
if (import.meta.env.DEV) console.log('[API Trace] Mapping errors to fields:', formErrors)

// Lines 64-66:
if (import.meta.env.DEV) {
  console.log(
    `[API Trace] Showing global toast. Severity: ${severity}, Title: ${toastTitle}, Detail: ${toastDetail}`,
  )
}
```

- [ ] **Step 5: Fix #7 — Replace unsafe cast in `paged-list.use.ts`**

Read `src/common/composables/paged-list.use.ts:69-75`. Replace `as unknown as Partial<TParams>` with typed spread:

```typescript
// Before (L69-71):
  function setPage(page: number) {
    return fetch({ page } as unknown as Partial<TParams>);
  }

// After:
  function setPage(page: number) {
    params.value.page = page;
    return fetch();
  }

// Before (L73-75):
  function setSort(sort: string[]) {
    return fetch({ sort } as unknown as Partial<TParams>);
  }

// After:
  function setSort(sort: string[]) {
    params.value.sort = sort;
    return fetch();
  }

// Before (L77-79):
  function setSearch(search: string, searchFields?: string[]) {
    return fetch({ search, searchFields } as unknown as Partial<TParams>);
  }

// After:
  function setSearch(search: string, searchFields?: string[]) {
    params.value.search = search;
    params.value.searchFields = searchFields;
    return fetch();
  }
```

- [ ] **Step 6: Fix #8 — Drop re-throw in `paged-list.use.ts`**

Read `src/common/composables/paged-list.use.ts:56-57`. Remove `throw err`:

```typescript
// Before (L55-58):
    } catch (err) {
      error.value = "An unexpected error occurred";
      throw err;

// After:
    } catch (err) {
      error.value = "An unexpected error occurred";
```

- [ ] **Step 7: Fix #9 — Escape special chars in `query-builder.utils.ts`**

Read `src/common/utils/query-builder.utils.ts:143-146`. Add URL encoding to `formatValue`:

```typescript
// Before:
  private formatValue(value: unknown): string {
    if (value === null || value === undefined) return 'null';
    if (value instanceof Date) return value.toISOString();
    return String(value);
  }

// After:
  private formatValue(value: unknown): string {
    if (value === null || value === undefined) return 'null';
    if (value instanceof Date) return value.toISOString();
    const str = String(value);
    if (str.includes(',') || str.includes('(') || str.includes(')') || str.includes('|')) {
      return encodeURIComponent(str);
    }
    return str;
  }
```

- [ ] **Step 8: Fix #10 — Recursive `toCamelCaseKeys` in `mapper.utils.ts`**

Read `src/common/mapper/mapper.utils.ts:14-16`. Add recursive deep conversion:

```typescript
// Before:
export function toCamelCaseKeys<T extends Record<string, unknown>>(obj: T): Record<string, unknown> {
  return mapKeys(obj, toCamelCase)
}

// After:
export function toCamelCaseKeys<T extends Record<string, unknown>>(obj: T): Record<string, unknown> {
  const result: Record<string, unknown> = {}
  for (const key of Object.keys(obj)) {
    const newKey = toCamelCase(key)
    const val = obj[key]
    if (val !== null && typeof val === 'object' && !Array.isArray(val)) {
      result[newKey] = toCamelCaseKeys(val as Record<string, unknown>)
    } else if (Array.isArray(val)) {
      result[newKey] = val.map(item =>
        item !== null && typeof item === 'object' && !Array.isArray(item)
          ? toCamelCaseKeys(item as Record<string, unknown>)
          : item
      )
    } else {
      result[newKey] = val
    }
  }
  return result
}
```

- [ ] **Step 9: Fix #11 — Remove toast bridge, use PrimeVue's `useToast` directly**

Read `src/common/composables/toast.use.ts`. Replace the custom `toastBus` with a direct wrapper:

```typescript
// Before:
import { ref } from 'vue';
export interface ToastMessage { /* ... */ }
export const toastBus = ref<ToastMessage | null>(null);
export function useToast() {
  const showToast = (severity, summary, detail, life = 3000) => {
    toastBus.value = { severity, summary, detail, life };
  };
  return { showToast, toastBus };
}

// After:
import { useToast as usePrimeToast } from 'primevue/usetoast';
export function useToast() {
  const toast = usePrimeToast();
  const showToast = (
    severity: 'success' | 'info' | 'warn' | 'error',
    summary: string,
    detail: string,
    life = 3000,
  ) => {
    toast.add({ severity, summary, detail, life });
  };
  return { showToast };
}
```

Then read `src/app/App.vue`. Remove the `toastBus` import and watcher:

```vue
<script setup lang="ts">
// Before:
import { watch } from 'vue'
import { useToast } from 'primevue/usetoast'
import { toastBus } from '@/common/composables/toast.use'
const toast = useToast()
watch(toastBus, (newValue) => {
  if (newValue) { toast.add(newValue); toastBus.value = null }
})

// After:
// remove toastBus import, watch, and toast declaration
</script>
```

- [ ] **Step 10: Fix #13 — Add `/` prefix to API constants**

Read `src/common/api/constants.ts`. Add leading `/`:

```typescript
// Before:
export const CATALOG = 'catalog'

// After:
export const CATALOG = '/catalog'
```

Do this for all 8 constants: `CATALOG`, `IDENTITY`, `LOCATIONS`, `PROFILES`, `INVENTORY`, `ORDERS`, `PAYMENTS`, `SHIPPING`.

Then update all API files that construct URLs. Example in `product.api.ts`:

```typescript
// Before: `${CATALOG}/products/${id}`
// After: paths already start with /, so: `${CATALOG}/products/${id}` → works as-is
```

But check for double-slash cases where an API function prepends `/` manually:

```bash
rg "\\\${CATALOG}" src/features/ --files-with-matches
rg "\\\${IDENTITY}" src/features/ --files-with-matches
```

If any file does `\`/${CATALOG}/...\``, it will now produce `//catalog/...`. Fix those to `\`${CATALOG}/...\``.

- [ ] **Step 11: Fix #15 — Normalize error casing in interceptor, simplify `parseApiError`**

Read `src/common/api/http/handlers/error-handler.ts:53-78`. The function checks `statusCode`/`Status`/`status`/`StatusCode` etc. Before parsing, normalize the `apiData` keys to camelCase using the existing `toCamelCaseKeys`. Replace L53-78:

```typescript
  if (axiosError.isAxiosError || axiosError.response || axiosError.request) {
    const apiData = axiosError.response?.data

    if (apiData && typeof apiData === 'object') {
      // Normalize to camelCase first — eliminates PascalCase/snake_case fallback checks
      const data = toCamelCaseKeys(apiData as Record<string, unknown>)

      const statusCode = (data.statusCode ?? axiosError.response?.status) as number | undefined
      const message = data.message as string | undefined
      const isSuccess = data.isSuccess as boolean | undefined
      const rawErrors = data.errors

      const title = (data.title ?? message) as string | undefined
      const detail = (data.detail ?? message) as string | undefined
      const errorCode = data.errorCode as string | undefined
      const resolvedCode = statusCode ?? 500

      return {
        statusCode: resolvedCode,
        title: title ?? (resolvedCode >= 500 ? 'Server Error' : 'Request Error'),
        message: message ?? title ?? null,
        detail: detail ?? null,
        isSuccess: isSuccess ?? false,
        errors: convertServerErrors(rawErrors),
        errorCode: errorCode,
      }
    }
    // ... rest unchanged
```

Import `toCamelCaseKeys` at top of file:

```typescript
import { toCamelCaseKeys } from '@/common/mapper/mapper.utils'
```

- [ ] **Step 12: Fix #16 — Remove unsafe `as ServerResult<AuthSession>` cast in `auth.api.ts`**

Read `src/features/auth/api/auth.api.ts:38-41`. Instead of casting, use the mapper result directly:

```typescript
// Before:
    return {
      ...result,
      value: mapAuthSession(result.value, session),
    } as ServerResult<AuthSession>;

// After:
    const session = await fetchSession();
    return mapAuthSession(result, session);
```

Update `mapAuthSession` in `src/features/identity/mappers/identity.mapper.ts` to accept `ServerResult<LoginResponse>` and return `ServerResult<AuthSession>` directly.

- [ ] **Step 13: Fix #17 — Safe failure path in `transform.ts`**

Read `src/common/utils/transform.ts:4-6`. Strip `.value` on failure:

```typescript
// Before:
export function mapValue<T, R>(result: ServerResult<T>, fn: (dto: T) => R): ServerResult<R> {
  return result.isSuccess && result.value != null
    ? { ...result, value: fn(result.value) }
    : result as unknown as ServerResult<R>
}

// After:
export function mapValue<T, R>(result: ServerResult<T>, fn: (dto: T) => R): ServerResult<R> {
  return result.isSuccess && result.value != null
    ? { ...result, value: fn(result.value) }
    : { ...result, value: undefined as never }
}
```

Same for `mapItems` on L9-12:

```typescript
export function mapItems<T, R>(result: ServerPagedResult<T>, fn: (dto: T) => R): ServerPagedResult<R> {
  return result.isSuccess && result.items
    ? { ...result, items: result.items.map(fn) }
    : { ...result, items: undefined as never }
}
```

- [ ] **Step 14: Create `src/common/errors/ApiError.ts`**

```typescript
export class ApiError extends Error {
  constructor(
    message: string,
    public readonly statusCode: number,
    public readonly errorCode?: string,
    public readonly errors?: Record<string, string[]>,
  ) {
    super(message)
    this.name = 'ApiError'
  }
}
```

- [ ] **Step 15: Commit**

```bash
git add -A app/Admin/src/common app/Admin/src/features/auth/api/auth.api.ts app/Admin/src/features/identity app/Admin/src/app/App.vue
git commit -m "fix(admin): address review issues #1,4-11,13,15-17 in common/ layer"
```

---

### Task 3: Update all imports from @/shared/ → @/common/ + ESLint config

**Files:** ~165 files in `src/features/`, ~2 files in `src/app/`, `eslint.config.ts`, `vite.config.ts`

**Interfaces:**
- Consumes: `src/common/` files placed in Task 1, `eslint.config.ts` current boundaries
- Produces: All imports updated; 4-layer boundaries enforced; auto-import dirs updated

- [ ] **Step 1: Bulk replace @/shared/api/ → @/common/api/ in features + app**

```bash
cd /home/qingfa/Repos/ReSys.Shop/app/Admin
find src/features src/app -name '*.ts' -o -name '*.vue' | xargs sed -i "s|from '@/shared/api/|from '@/common/api/|g"
```

- [ ] **Step 2: Bulk replace @/shared/composables/ → @/common/composables/**

```bash
find src/features src/app -name '*.ts' -o -name '*.vue' | xargs sed -i "s|from '@/shared/composables/|from '@/common/composables/|g"
```

- [ ] **Step 3: Bulk replace other moved paths**

```bash
find src/features src/app -name '*.ts' -o -name '*.vue' | xargs sed -i "s|from '@/shared/services/|from '@/common/services/|g"
find src/features src/app -name '*.ts' -o -name '*.vue' | xargs sed -i "s|from '@/shared/mapper/|from '@/common/mapper/|g"
find src/features src/app -name '*.ts' -o -name '*.vue' | xargs sed -i "s|from '@/shared/config/|from '@/common/config/|g"
find src/features src/app -name '*.ts' -o -name '*.vue' | xargs sed -i "s|from '@/shared/test/|from '@/common/test/|g"
```

- [ ] **Step 4: Handle @/shared/utils/ → split between @/common/utils/ and @/shared/utils/**

```bash
# First, check which files import from @/shared/utils/ and what they import
rg "from '@/shared/utils/" src/features/ src/app/ -l
```

For imports of `query-builder` and `transform` (now in `@/common/utils/`):

```bash
find src/features src/app -name '*.ts' -o -name '*.vue' | xargs sed -i "s|from '@/shared/utils/query-builder.utils'|from '@/common/utils/query-builder.utils'|g"
find src/features src/app -name '*.ts' -o -name '*.vue' | xargs sed -i "s|from '@/shared/utils/transform'|from '@/common/utils/transform'|g"
find src/features src/app -name '*.ts' -o -name '*.vue' | xargs sed -i "s|from '@/shared/utils/enums'|from '@/shared/utils/enums'|g"
find src/features src/app -name '*.ts' -o -name '*.vue' | xargs sed -i "s|from '@/shared/utils/currency'|from '@/shared/utils/currency'|g"
```

- [ ] **Step 5: Verify no stale @/shared/api/ or @/shared/composables/ imports remain**

```bash
rg "from '@/shared/api/" src/features/ src/app/ --count
rg "from '@/shared/composables/" src/features/ src/app/ --count
```

Expected: zero results for both. If any remain, fix manually.

- [ ] **Step 6: Update `eslint.config.ts` — add common/ layer**

Read the current boundaries config. Add `common` element and update rules:

```typescript
// In settings.boundaries/elements array, add:
{ type: 'common', pattern: 'src/common/**/*', mode: 'folder' },

// In rules.boundaries/element-types rules array, add:
{ from: 'common', disallow: ['features', 'app'] },

// Update shared rule:
{ from: 'shared', disallow: ['common', 'features', 'app'] },  // was just ['features', 'app']

// Update app rule:
{ from: 'app', allow: ['shared', 'common', 'features'] },  // was just ['shared', 'features']
```

- [ ] **Step 7: Update `vite.config.ts` — auto-import dirs**

Change auto-import dirs from `src/shared/composables` to `src/common/composables`:

```typescript
// In vite.config.ts AutoImport config:
dirs: ['src/common/composables'],  // was: ['src/shared/composables']
```

- [ ] **Step 8: Verification gate**

```bash
pnpm run type-check && pnpm run lint && pnpm run test:unit
```

All three must pass. Fix any failures before proceeding.

- [ ] **Step 9: Commit**

```bash
git add -A app/Admin/src/ app/Admin/eslint.config.ts app/Admin/vite.config.ts
git commit -m "refactor(admin): update @/shared/ imports to @/common/, add 4-layer ESLint boundaries"
```

---

### Task 4: Move and update test files to common/

**Files:** Test files from `src/shared/**/__tests__/`, `src/shared/**/*.spec.ts`

- [ ] **Step 1: Move interceptor tests**

```bash
git mv src/shared/api/http/interceptors/__tests__ src/common/api/http/interceptors/__tests__
```

- [ ] **Step 2: Move handler tests + api.client.spec**

```bash
git mv src/shared/api/http/handlers/__tests__ src/common/api/http/handlers/__tests__
git mv src/shared/api/http/api.client.spec.ts src/common/api/http/api.client.spec.ts
```

- [ ] **Step 3: Move api.utils.spec, composable specs**

```bash
git mv src/shared/api/utils/api.utils.spec.ts src/common/api/utils/__tests__/api.utils.spec.ts
mkdir -p src/common/composables/__tests__
# Check for composable spec files:
ls src/shared/composables/*.spec.ts 2>/dev/null
# Move any found:
find src/shared -name '*.spec.ts' -exec bash -c 'f="{}"; target="${f/shared/common}"; mkdir -p "$(dirname $target)"; git mv "$f" "$target"' \;
```

- [ ] **Step 4: Move shared component tests to __tests__/ under common/ or shared/**

```bash
# Check if shared component __tests__ dir still exists
ls src/shared/components/__tests__ 2>/dev/null
# Move component tests — they stay with shared but update import paths
find src/shared/components/__tests__ -name '*.ts' -exec sed -i "s|from '@/shared/|from '@/common/|g" {} +
```

- [ ] **Step 5: Update import paths in moved test files**

```bash
# Update any remaining @/shared/ → @/common/ in test files
find src/common -name '*.spec.ts' -o -name '*.test.ts' | xargs sed -i "s|from '@/shared/|from '@/common/|g"
find src/shared/components -name '*.spec.ts' -o -name '*.test.ts' | xargs sed -i "s|from '@/shared/|from '@/common/|g"
```

- [ ] **Step 6: Run tests to verify**

```bash
pnpm run test:unit
```

All tests must pass. Fix any import path errors.

- [ ] **Step 7: Commit**

```bash
git add -A app/Admin/src/common app/Admin/src/shared
git commit -m "test(admin): move test files to common/ layer, update import paths"
```

---

### Task 5: Reorganize shared/components/ into subdirectories

**Files:** All files under `src/shared/components/`

- [ ] **Step 1: Create component subdirectories**

```bash
cd /home/qingfa/Repos/ReSys.Shop/app/Admin
mkdir -p src/shared/components/{base,form,tables,data-display,feedback,navigation}
```

- [ ] **Step 2: Move components into subdirectories (drop .Component suffix in filename)**

```bash
# base/
git mv src/shared/components/ConfirmButton.Component.vue src/shared/components/base/ConfirmButton.vue

# form/
git mv src/shared/components/FormField.Component.vue src/shared/components/form/FormField.vue
# SearchInput was recently added — check location:
if [ -f src/shared/components/SearchInput.vue ]; then
  git mv src/shared/components/SearchInput.vue src/shared/components/form/SearchInput.vue
fi

# tables/
git mv src/shared/components/DataTableShell.Component.vue src/shared/components/tables/DataTableShell.vue
if [ -f src/shared/components/LoadingOverlay.vue ]; then
  git mv src/shared/components/LoadingOverlay.vue src/shared/components/tables/LoadingOverlay.vue
fi

# data-display/
git mv src/shared/components/StatCard.Component.vue src/shared/components/data-display/StatCard.vue
git mv src/shared/components/DetailField.Component.vue src/shared/components/data-display/DetailField.vue
git mv src/shared/components/TabbedDetail.Component.vue src/shared/components/data-display/TabbedDetail.vue
git mv src/shared/components/MetadataManager.Component.vue src/shared/components/data-display/MetadataManager.vue
for f in DescriptionList DetailGroup CopyButton; do
  if [ -f "src/shared/components/${f}.vue" ]; then
    git mv "src/shared/components/${f}.vue" "src/shared/components/data-display/${f}.vue"
  fi
done

# feedback/
git mv src/shared/components/EmptyState.Component.vue src/shared/components/feedback/EmptyState.vue
git mv src/shared/components/StatusBadge.Component.vue src/shared/components/feedback/StatusBadge.vue
for f in SkeletonLoader DeleteDialog; do
  if [ -f "src/shared/components/${f}.vue" ]; then
    git mv "src/shared/components/${f}.vue" "src/shared/components/feedback/${f}.vue"
  fi
done

# navigation/
git mv src/shared/components/Breadcrumb.Component.vue src/shared/components/navigation/Breadcrumb.vue
git mv src/shared/components/PageShell.Component.vue src/shared/components/navigation/PageShell.vue
git mv src/shared/components/PageHeader.Component.vue src/shared/components/navigation/PageHeader.vue
git mv src/shared/components/ManagerWelcome.Component.vue src/shared/components/navigation/ManagerWelcome.vue
for f in PageContainer Section; do
  if [ -f "src/shared/components/${f}.vue" ]; then
    git mv "src/shared/components/${f}.vue" "src/shared/components/navigation/${f}.vue"
  fi
done
```

- [ ] **Step 3: Move shared component tests to new subdirectory paths**

```bash
# Move test files alongside their components
if [ -d src/shared/components/__tests__ ]; then
  mkdir -p src/shared/components/{base,form,tables,data-display,feedback,navigation}/__tests__
  # Split tests by component they test — check contents:
  ls src/shared/components/__tests__/
  # Move each test to matching component subdir __tests__/
fi

# Specific test moves:
if [ -f src/shared/components/__tests__/ConfirmButton.test.ts ]; then
  git mv src/shared/components/__tests__/ConfirmButton.test.ts src/shared/components/base/__tests__/ConfirmButton.test.ts
fi
# ... repeat for each test file (StatCard, DataTableShell, etc.)
```

- [ ] **Step 4: Update all feature imports for shared components**

Components that moved to subdirectories need import path updates. Current features import like:

```typescript
import DataTableShell from '@/shared/components/DataTableShell.Component.vue'
```

After this task:

```typescript
import DataTableShell from '@/shared/components/tables/DataTableShell.vue'
```

Bulk replace by component name:

```bash
# ConfirmButton: any import path containing it → new path
find src/features src/app -name '*.vue' -o -name '*.ts' | xargs sed -i "s|from '@/shared/components/ConfirmButton\.Component\.vue'|from '@/shared/components/base/ConfirmButton.vue'|g"

# FormField
find src/features src/app -name '*.vue' -o -name '*.ts' | xargs sed -i "s|from '@/shared/components/FormField\.Component\.vue'|from '@/shared/components/form/FormField.vue'|g"

# DataTableShell
find src/features src/app -name '*.vue' -o -name '*.ts' | xargs sed -i "s|from '@/shared/components/DataTableShell\.Component\.vue'|from '@/shared/components/tables/DataTableShell.vue'|g"

# StatCard
find src/features src/app -name '*.vue' -o -name '*.ts' | xargs sed -i "s|from '@/shared/components/StatCard\.Component\.vue'|from '@/shared/components/data-display/StatCard.vue'|g"

# DetailField
find src/features src/app -name '*.vue' -o -name '*.ts' | xargs sed -i "s|from '@/shared/components/DetailField\.Component\.vue'|from '@/shared/components/data-display/DetailField.vue'|g"

# TabbedDetail
find src/features src/app -name '*.vue' -o -name '*.ts' | xargs sed -i "s|from '@/shared/components/TabbedDetail\.Component\.vue'|from '@/shared/components/data-display/TabbedDetail.vue'|g"

# MetadataManager
find src/features src/app -name '*.vue' -o -name '*.ts' | xargs sed -i "s|from '@/shared/components/MetadataManager\.Component\.vue'|from '@/shared/components/data-display/MetadataManager.vue'|g"

# EmptyState
find src/features src/app -name '*.vue' -o -name '*.ts' | xargs sed -i "s|from '@/shared/components/EmptyState\.Component\.vue'|from '@/shared/components/feedback/EmptyState.vue'|g"

# StatusBadge
find src/features src/app -name '*.vue' -o -name '*.ts' | xargs sed -i "s|from '@/shared/components/StatusBadge\.Component\.vue'|from '@/shared/components/feedback/StatusBadge.vue'|g"

# Breadcrumb
find src/features src/app -name '*.vue' -o -name '*.ts' | xargs sed -i "s|from '@/shared/components/Breadcrumb\.Component\.vue'|from '@/shared/components/navigation/Breadcrumb.vue'|g"

# PageShell
find src/features src/app -name '*.vue' -o -name '*.ts' | xargs sed -i "s|from '@/shared/components/PageShell\.Component\.vue'|from '@/shared/components/navigation/PageShell.vue'|g"

# PageHeader
find src/features src/app -name '*.vue' -o -name '*.ts' | xargs sed -i "s|from '@/shared/components/PageHeader\.Component\.vue'|from '@/shared/components/navigation/PageHeader.vue'|g"

# ManagerWelcome
find src/features src/app -name '*.vue' -o -name '*.ts' | xargs sed -i "s|from '@/shared/components/ManagerWelcome\.Component\.vue'|from '@/shared/components/navigation/ManagerWelcome.vue'|g"
```

- [ ] **Step 5: Verification gate**

```bash
pnpm run type-check && pnpm run lint && pnpm run test:unit
```

- [ ] **Step 6: Commit**

```bash
git add -A app/Admin/src/shared/components app/Admin/src/features app/Admin/src/app
git commit -m "refactor(admin): reorganize shared components into subdirectories, drop .Component suffix"
```

---

### Task 6: Create shared/fields/ — Zod field inheritance

**Files:** Create 6 new files under `src/shared/fields/`

- [ ] **Step 1: Create `src/shared/fields/base.field.ts`**

```typescript
import { z } from 'zod'

export const baseFields = z.object({
  id: z.string(),
  createdAtUtc: z.string().optional(),
  updatedAtUtc: z.string().optional(),
})

export type BaseFields = z.infer<typeof baseFields>
```

- [ ] **Step 2: Create `src/shared/fields/name.field.ts`**

```typescript
import { z } from 'zod'

export const nameFields = z.object({
  name: z.string().min(1),
  slug: z.string().optional(),
  description: z.string().optional(),
})

export type NameFields = z.infer<typeof nameFields>
```

- [ ] **Step 3: Create `src/shared/fields/address.field.ts`**

```typescript
import { z } from 'zod'

export const addressFields = z.object({
  line1: z.string().min(1),
  line2: z.string().optional(),
  city: z.string().min(1),
  state: z.string().optional(),
  postalCode: z.string().optional(),
  countryCode: z.string().optional(),
  phone: z.string().optional(),
})

export type AddressFields = z.infer<typeof addressFields>
```

- [ ] **Step 4: Create `src/shared/fields/money.field.ts`**

```typescript
import { z } from 'zod'

export const moneyFields = z.object({
  amount: z.number().min(0),
  currency: z.string().default('USD'),
})

export type MoneyFields = z.infer<typeof moneyFields>
```

- [ ] **Step 5: Create `src/shared/fields/seo.field.ts`**

```typescript
import { z } from 'zod'

export const seoFields = z.object({
  metaTitle: z.string().optional(),
  metaDescription: z.string().optional(),
  metaKeywords: z.string().optional(),
})

export type SeoFields = z.infer<typeof seoFields>
```

- [ ] **Step 6: Create `src/shared/fields/index.ts`**

```typescript
export { baseFields, type BaseFields } from './base.field'
export { nameFields, type NameFields } from './name.field'
export { addressFields, type AddressFields } from './address.field'
export { moneyFields, type MoneyFields } from './money.field'
export { seoFields, type SeoFields } from './seo.field'
```

- [ ] **Step 7: Commit**

```bash
git add -A app/Admin/src/shared/fields
git commit -m "feat(admin): add shared Zod field inheritance schemas"
```

---

### Task 7: Restructure features (Part 1 — Catalog + Auth + Error + Identity)

**Files:** All files under `src/features/catalog/`, `src/features/auth/`, `src/features/error/`, `src/features/identity/`

This task is the template for all feature restructuring. Repeat the pattern for each feature domain.

- [ ] **Step 1: Catalog feature — rename views/ → pages/, rename .View.vue → Page.vue**

```bash
cd /home/qingfa/Repos/ReSys.Shop/app/Admin

# catalog/dashboard
git mv src/features/catalog/dashboard/views src/features/catalog/dashboard/pages
rename 's/\.View\.vue$/.vue/' src/features/catalog/dashboard/pages/*.vue 2>/dev/null || true
# manually rename:
for f in src/features/catalog/dashboard/pages/*.View.vue; do
  [ -f "$f" ] && git mv "$f" "${f/.View.vue/Page.vue}"
done

# catalog/option-types
git mv src/features/catalog/option-types/views src/features/catalog/option-types/pages
for f in src/features/catalog/option-types/pages/*.View.vue; do
  [ -f "$f" ] && git mv "$f" "${f/.View.vue/Page.vue}"
done

# catalog/option-types/option-values (if views/ exists)
if [ -d src/features/catalog/option-types/option-values/views ]; then
  git mv src/features/catalog/option-types/option-values/views src/features/catalog/option-types/option-values/pages
fi

# catalog/products
git mv src/features/catalog/products/views src/features/catalog/products/pages
for f in src/features/catalog/products/pages/*.View.vue; do
  [ -f "$f" ] && git mv "$f" "${f/.View.vue/Page.vue}"
done

# catalog/taxonomies
git mv src/features/catalog/taxonomies/views src/features/catalog/taxonomies/pages
for f in src/features/catalog/taxonomies/pages/*.View.vue; do
  [ -f "$f" ] && git mv "$f" "${f/.View.vue/Page.vue}"
done

# catalog/taxonomies/taxa (if views/ exists)
if [ -d src/features/catalog/taxonomies/taxa/views ]; then
  git mv src/features/catalog/taxonomies/taxa/views src/features/catalog/taxonomies/taxa/pages
fi
```

- [ ] **Step 2: Catalog feature — rename stores/ → store/**

```bash
# catalog products
git mv src/features/catalog/products/stores src/features/catalog/products/store
# catalog option-types
git mv src/features/catalog/option-types/stores src/features/catalog/option-types/store
# catalog taxonomies
git mv src/features/catalog/taxonomies/stores src/features/catalog/taxonomies/store
```

Repeat for all catalog sub-features with `stores/` directories.

- [ ] **Step 3: Catalog feature — rename schemas/ → types/*.field.ts**

For each schema file, extract Zod definitions into a `.field.ts`:

```bash
# catalog/option-types: option-type.schema.ts → option-type.field.ts
cd src/features/catalog/option-types
if [ -f schemas/option-type.schema.ts ]; then
  git mv schemas/option-type.schema.ts types/option-type.field.ts
fi
# catalog/taxonomies: multiple schema files
cd ../../taxonomies
for f in schemas/*.schema.ts; do
  [ -f "$f" ] && git mv "$f" "types/$(basename $f .schema.ts).field.ts"
done
# catalog/products: create-product.schema.ts, update-product.schema.ts
cd ../products
for f in schemas/*.schema.ts; do
  [ -f "$f" ] && git mv "$f" "types/$(basename $f .schema.ts).field.ts"
done
```

Then refactor each `.field.ts` to use shared field inheritance. Example for `product.field.ts` (combines create + update):

```typescript
import { z } from 'zod'
import { baseFields, nameFields, seoFields } from '@/shared/fields'

export const productFields = baseFields.merge(nameFields).merge(seoFields).extend({
  styleCode: z.string().min(1),
  status: z.number(),
  brandId: z.string().optional(),
  defaultVariantId: z.string().optional(),
})

export type ProductFields = z.infer<typeof productFields>
```

Remove old `schemas/` directories after migration:

```bash
rmdir schemas 2>/dev/null || true
cd /home/qingfa/Repos/ReSys.Shop/app/Admin
```

- [ ] **Step 4: Catalog feature — rename type files (drop .type infix)**

```bash
cd src/features/catalog
# Products
find products/types -name '*.model.type.ts' -exec bash -c 'git mv "$1" "${1/.model.type.ts/.model.ts}"' _ {} \;
find products/types -name '*.request.type.ts' -exec bash -c 'git mv "$1" "${1/.request.type.ts/.request.ts}"' _ {} \;
find products/types -name '*.response.type.ts' -exec bash -c 'git mv "$1" "${1/.response.type.ts/.response.ts}"' _ {} \;
find products/types -name '*.parameters.type.ts' -exec bash -c 'git mv "$1" "${1/.parameters.type.ts/.parameters.ts}"' _ {} \;
find products/types -name '*.query.type.ts' -exec bash -c 'git mv "$1" "${1/.query.type.ts/.query.ts}"' _ {} \;

# Option-types
find option-types/types -name '*.type.ts' -exec bash -c 'git mv "$1" "${1/.type.ts/.ts}"' _ {} \;

# Taxonomies
find taxonomies/types -name '*type.ts' -exec bash -c 'git mv "$1" "${1/.type.ts/.ts}"' _ {} \;

cd /home/qingfa/Repos/ReSys.Shop/app/Admin
```

- [ ] **Step 5: Catalog feature — drop .Component suffix from feature components**

```bash
cd src/features/catalog
find . -name '*.Component.vue' -exec bash -c 'git mv "$1" "${1/.Component.vue/.vue}"' _ {} \;
cd /home/qingfa/Repos/ReSys.Shop/app/Admin
```

- [ ] **Step 6: Catalog feature — merge services/ + mappers/ into api/ + models/**

For each sub-feature with `services/`:
1. Move service logic (HTTP calls + orchestration) into `api/`:
   - `catalog/products/services/product.service.ts` → merge into `catalog/products/api/product.api.ts`
2. Move mapper logic into `models/`:
   - For features with `mappers/` directory, move content into `models/` barrel
3. Delete `services/` directory

Example for catalog/products:

```bash
# Read product.service.ts, find what it does beyond calling product.api.ts
# If service just wraps API with minor mapping: merge into api/product.api.ts
# Then delete the service file
```

Since `option-type.service.ts` is a thin wrapper (just delegates to repository), merge it:

After merging, remove service files:

```bash
rm src/features/catalog/products/services/product.service.ts
rm src/features/catalog/option-types/services/option-type.service.ts
rm src/features/catalog/taxonomies/services/taxonomy.service.ts
rm src/features/catalog/dashboard/services/catalog-dashboard.service.ts
```

- [ ] **Step 7: Catalog feature — standardize test directories**

```bash
# Move _tests/ → __tests__/
cd src/features/catalog
find . -type d -name '_tests' -exec bash -c 'git mv "$1" "${1%_tests}__tests__"' _ {} \;
find . -type d -name 'tests' ! -name '__tests__' -exec bash -c 'git mv "$1" "${1%tests}__tests__"' _ {} \;
cd /home/qingfa/Repos/ReSys.Shop/app/Admin
```

- [ ] **Step 8: Catalog feature — add index.ts barrel + models/ + composables/ directories**

Create barrel files per sub-feature:

```bash
# catalog/products/index.ts
cat > src/features/catalog/products/index.ts << 'EOF'
export { useProductStore } from './store/product.store'
export * from './types/product.field'
export * from './types/product.request'
export * from './types/product.response'
export * from './models/product.model'
export * from './api/product.api'
EOF

# catalog/option-types/index.ts
# ... etc.
```

Create `composables/` directory per feature (empty initially, or move relevant composable logic):

```bash
mkdir -p src/features/catalog/products/composables
mkdir -p src/features/catalog/option-types/composables
mkdir -p src/features/catalog/taxonomies/composables
```

Create `models/` directory for features that had mappers or model files inline:

```bash
# If types/product.model.ts exists, move to models/product.model.ts
if [ -f src/features/catalog/products/types/product.model.ts ]; then
  mkdir -p src/features/catalog/products/models
  git mv src/features/catalog/products/types/product.model.ts src/features/catalog/products/models/product.model.ts
fi
```

- [ ] **Step 9: Auth + Error + Identity features — same pattern**

Apply Steps 1-8 (rename views→pages, stores→store, _tests→__tests__, drop .Component, rename types) to:

```bash
# auth/
find src/features/auth -type d -name 'views' -exec bash -c 'git mv "$1" "${1%views}pages"' _ {} \;
for f in src/features/auth/pages/*.View.vue; do
  [ -f "$f" ] && git mv "$f" "${f/.View.vue/Page.vue}"
done
git mv src/features/auth/stores src/features/auth/store
find src/features/auth -type d -name '_tests' -exec bash -c 'git mv "$1" "${1%_tests}__tests__"' _ {} \;
find src/features/auth/types -name '*.type.ts' -exec bash -c 'git mv "$1" "${1/.type.ts/.ts}"' _ {} \;

# error/ (page renames only — no stores/schemas)
find src/features/error -type d -name 'pages' -exec bash -c 'for f in $1/*.View.vue; do [ -f "$f" ] && git mv "$f" "${f/.View.vue/Page.vue}"; done' _ {} \;
# If error has views/ not pages/, rename:
if [ -d src/features/error/pages ]; then
  for f in src/features/error/pages/*.View.vue; do
    [ -f "$f" ] && git mv "$f" "${f/.View.vue/Page.vue}"
  done
fi

# identity/
find src/features/identity/types -name '*.type.ts' -exec bash -c 'git mv "$1" "${1/.type.ts/.ts}"' _ {} \;
```

- [ ] **Step 10: Update all import paths in changed feature files**

After all renames, imports in feature files need updating. Use sed:

```bash
# Replace .View import references in feature .vue/.ts files
find src/features/catalog src/features/auth src/features/error src/features/identity -name '*.vue' -o -name '*.ts' | xargs sed -i "s|\.View\.vue|Page.vue|g"

# Replace .Component import references
find src/features/catalog src/features/auth src/features/error src/features/identity -name '*.vue' -o -name '*.ts' | xargs sed -i "s|\.Component\.vue|.vue|g"

# Replace stores/ import paths → store/
find src/features/catalog src/features/auth -name '*.vue' -o -name '*.ts' | xargs sed -i "s|/stores/|/store/|g"

# Replace schema import paths → field
find src/features/catalog -name '*.vue' -o -name '*.ts' | xargs sed -i "s|/schemas/|/types/|g"
find src/features/catalog -name '*.vue' -o -name '*.ts' | xargs sed -i "s|\.schema|.field|g"

# Replace type import paths (drop .type infix in imports)
find src/features/catalog src/features/auth src/features/identity -name '*.ts' -o -name '*.vue' | xargs sed -i "s|\.model\.type|.model|g"
find src/features/catalog src/features/auth src/features/identity -name '*.ts' -o -name '*.vue' | xargs sed -i "s|\.request\.type|.request|g"
find src/features/catalog src/features/auth src/features/identity -name '*.ts' -o -name '*.vue' | xargs sed -i "s|\.response\.type|.response|g"
find src/features/catalog src/features/auth src/features/identity -name '*.ts' -o -name '*.vue' | xargs sed -i "s|\.parameters\.type|.parameters|g"
find src/features/catalog src/features/auth src/features/identity -name '*.ts' -o -name '*.vue' | xargs sed -i "s|\.query\.type|.query|g"

# Replace services/ → api/ import paths
find src/features/catalog -name '*.ts' -o -name '*.vue' | xargs sed -i "s|/services/|/api/|g"

# Replace _tests/ → __tests__/ import paths (if any tests import from each other)
find src/features/catalog src/features/auth -name '*.spec.ts' | xargs sed -i "s|/_tests/|/__tests__/|g"
```

- [ ] **Step 11: Verification gate**

```bash
pnpm run type-check && pnpm run lint && pnpm run test:unit
```

Fix any failures. Most will be import path mismatches.

- [ ] **Step 12: Commit**

```bash
git add -A app/Admin/src/features/catalog app/Admin/src/features/auth app/Admin/src/features/error app/Admin/src/features/identity
git commit -m "refactor(admin): restructure catalog, auth, error, identity features"
```

---

### Task 8: Restructure features (Part 2 — Remaining features)

**Files:** `src/features/inventories/`, `ordering/`, `payment/`, `profile/`, `shipping/`, `location/`, `reports/`, `users/`

Same pattern as Task 7 but batched. Execute sequentially per feature domain.

- [ ] **Step 1: inventories/**

```bash
cd /home/qingfa/Repos/ReSys.Shop/app/Admin/src/features/inventories

# Rename views → pages, drop .View suffix
for dir in dashboard inventory-units stock-items stock-locations stock-movements stock-transfers; do
  if [ -d "$dir/views" ]; then git mv "$dir/views" "$dir/pages"; fi
done
find . -name '*.View.vue' -exec bash -c 'git mv "$1" "${1/.View.vue/Page.vue}"' _ {} \;

# stores/ → store/
find . -type d -name 'stores' -exec bash -c 'git mv "$1" "${1%stores}store"' _ {} \;

# _tests/ → __tests__/
find . -type d -name '_tests' -exec bash -c 'git mv "$1" "${1%_tests}__tests__"' _ {} \;

# Drop .Component suffix
find . -name '*.Component.vue' -exec bash -c 'git mv "$1" "${1/.Component.vue/.vue}"' _ {} \;

# Rename types (drop .type infix)
find . -name '*.model.type.ts' -exec bash -c 'git mv "$1" "${1/.model.type.ts/.model.ts}"' _ {} \;
find . -name '*.request.type.ts' -exec bash -c 'git mv "$1" "${1/.request.type.ts/.request.ts}"' _ {} \;
find . -name '*.response.type.ts' -exec bash -c 'git mv "$1" "${1/.response.type.ts/.response.ts}"' _ {} \;
find . -name '*.parameters.type.ts' -exec bash -c 'git mv "$1" "${1/.parameters.type.ts/.parameters.ts}"' _ {} \;
find . -name '*.query.type.ts' -exec bash -c 'git mv "$1" "${1/.query.type.ts/.query.ts}"' _ {} \;

# Merge services + mappers → api + models
# services/inventory.service.ts → api/ inventory functions
# stock-items/services/stock.service.ts → stock-items/api/stock.api.ts
# (merge content, then rm service files)

# Create models/, composables/, index.ts per feature
mkdir -p stock-items/models stock-items/composables stock-locations/models stock-movements/models
cd /home/qingfa/Repos/ReSys.Shop/app/Admin
```

- [ ] **Step 2: ordering/**

```bash
cd /home/qingfa/Repos/ReSys.Shop/app/Admin/src/features/ordering

# views/ → pages/
for dir in dashboard fulfillment orders; do
  if [ -d "$dir/views" ]; then git mv "$dir/views" "$dir/pages"; fi
done
find . -name '*.View.vue' -exec bash -c 'git mv "$1" "${1/.View.vue/Page.vue}"' _ {} \;

# stores/ → store/
find . -type d -name 'stores' -exec bash -c 'git mv "$1" "${1%stores}store"' _ {} \;

# _tests/ + tests/ → __tests__/
find . -type d -name '_tests' -exec bash -c 'git mv "$1" "${1%_tests}__tests__"' _ {} \;
find . -type d -name 'tests' ! -name '__tests__' -exec bash -c 'git mv "$1" "${1%tests}__tests__"' _ {} \;

# Drop .Component suffix
find . -name '*.Component.vue' -exec bash -c 'git mv "$1" "${1/.Component.vue/.vue}"' _ {} \;

# Rename types
find . -name '*.type.ts' -exec bash -c 'git mv "$1" "${1/.type.ts/.ts}"' _ {} \;

# Merge services + mappers
# orders/services/order.service.ts → orders/api/order.api.ts
# orders/mappers/order.mapper.ts → orders/models/order.model.ts
# fulfillment/services/fulfillment.service.ts → fulfillment/api/fulfillment.api.ts

# Create barrel + dirs
mkdir -p orders/models orders/composables fulfillment/models dashboard/models

cd /home/qingfa/Repos/ReSys.Shop/app/Admin
```

- [ ] **Step 3: payment/**

```bash
cd /home/qingfa/Repos/ReSys.Shop/app/Admin/src/features/payment

# views → pages
for dir in payment-methods payments; do
  if [ -d "$dir/views" ]; then git mv "$dir/views" "$dir/pages"; fi
done
find . -name '*.View.vue' -exec bash -c 'git mv "$1" "${1/.View.vue/Page.vue}"' _ {} \;

# stores → store
find . -type d -name 'stores' -exec bash -c 'git mv "$1" "${1%stores}store"' _ {} \;

# _tests → __tests__
find . -type d -name '_tests' -exec bash -c 'git mv "$1" "${1%_tests}__tests__"' _ {} \;

# Drop .Component, rename types
find . -name '*.Component.vue' -exec bash -c 'git mv "$1" "${1/.Component.vue/.vue}"' _ {} \;
find . -name '*.type.ts' -exec bash -c 'git mv "$1" "${1/.type.ts/.ts}"' _ {} \;

# Merge services + mappers
# payments/services/payment.service.ts → payments/api/payment.api.ts
# payments/mappers/payment.mapper.ts → payments/models/payment.model.ts

mkdir -p payments/models payments/composables payment-methods/models
cd /home/qingfa/Repos/ReSys.Shop/app/Admin
```

- [ ] **Step 4: profile/**

```bash
cd /home/qingfa/Repos/ReSys.Shop/app/Admin/src/features/profile

# views → pages (addresses/)
if [ -d addresses/views ]; then git mv addresses/views addresses/pages; fi
find . -name '*.View.vue' -exec bash -c 'git mv "$1" "${1/.View.vue/Page.vue}"' _ {} \;

# stores → store
find . -type d -name 'stores' -exec bash -c 'git mv "$1" "${1%stores}store"' _ {} \;

# _tests → __tests__
find . -type d -name '_tests' -exec bash -c 'git mv "$1" "${1%_tests}__tests__"' _ {} \;

# Drop .Component, rename types
find . -name '*.Component.vue' -exec bash -c 'git mv "$1" "${1/.Component.vue/.vue}"' _ {} \;
find . -name '*.type.ts' -exec bash -c 'git mv "$1" "${1/.type.ts/.ts}"' _ {} \;

# Merge services + mappers
# services/profile.service.ts → api/profile.api.ts
# mappers/profile.mapper.ts → models/profile.model.ts

mkdir -p models composables addresses/models
cd /home/qingfa/Repos/ReSys.Shop/app/Admin
```

- [ ] **Step 5: shipping/**

```bash
cd /home/qingfa/Repos/ReSys.Shop/app/Admin/src/features/shipping

# views → pages
for dir in shipping-methods shipping-rates; do
  if [ -d "$dir/views" ]; then git mv "$dir/views" "$dir/pages"; fi
done
find . -name '*.View.vue' -exec bash -c 'git mv "$1" "${1/.View.vue/Page.vue}"' _ {} \;

# stores → store
find . -type d -name 'stores' -exec bash -c 'git mv "$1" "${1%stores}store"' _ {} \;

# _tests → __tests__
find . -type d -name '_tests' -exec bash -c 'git mv "$1" "${1%_tests}__tests__"' _ {} \;

# Drop .Component, rename types
find . -name '*.Component.vue' -exec bash -c 'git mv "$1" "${1/.Component.vue/.vue}"' _ {} \;
find . -name '*.type.ts' -exec bash -c 'git mv "$1" "${1/.type.ts/.ts}"' _ {} \;

mkdir -p shipping-methods/models shipping-rates/models
cd /home/qingfa/Repos/ReSys.Shop/app/Admin
```

- [ ] **Step 6: location/**

```bash
cd /home/qingfa/Repos/ReSys.Shop/app/Admin/src/features/location

for dir in countries states; do
  if [ -d "$dir/views" ]; then git mv "$dir/views" "$dir/pages"; fi
done
find . -name '*.View.vue' -exec bash -c 'git mv "$1" "${1/.View.vue/Page.vue}"' _ {} \;
find . -type d -name 'stores' -exec bash -c 'git mv "$1" "${1%stores}store"' _ {} \;
find . -name '*.type.ts' -exec bash -c 'git mv "$1" "${1/.type.ts/.ts}"' _ {} \;

mkdir -p countries/models states/models
cd /home/qingfa/Repos/ReSys.Shop/app/Admin
```

- [ ] **Step 7: reports/**

```bash
cd /home/qingfa/Repos/ReSys.Shop/app/Admin/src/features/reports

if [ -d views ]; then git mv views pages; fi
find . -name '*.View.vue' -exec bash -c 'git mv "$1" "${1/.View.vue/Page.vue}"' _ {} \;
find . -type d -name 'stores' -exec bash -c 'git mv "$1" "${1%stores}store"' _ {} \;
find . -name '*.type.ts' -exec bash -c 'git mv "$1" "${1/.type.ts/.ts}"' _ {} \;

mkdir -p models composables
cd /home/qingfa/Repos/ReSys.Shop/app/Admin
```

- [ ] **Step 8: users/**

```bash
cd /home/qingfa/Repos/ReSys.Shop/app/Admin/src/features/users

# views → pages
for dir in . permissions roles; do
  dirpath="$([ "$dir" = "." ] && echo "." || echo "$dir")"
  if [ -d "$dirpath/views" ]; then git mv "$dirpath/views" "$dirpath/pages"; fi
done
find . -name '*.View.vue' -exec bash -c 'git mv "$1" "${1/.View.vue/Page.vue}"' _ {} \;

# stores → store
find . -type d -name 'stores' -exec bash -c 'git mv "$1" "${1%stores}store"' _ {} \;

# Drop .Component
find . -name '*.Component.vue' -exec bash -c 'git mv "$1" "${1/.Component.vue/.vue}"' _ {} \;

# Rename types
find . -name '*.type.ts' -exec bash -c 'git mv "$1" "${1/.type.ts/.ts}"' _ {} \;

mkdir -p models composables permissions/models roles/models
cd /home/qingfa/Repos/ReSys.Shop/app/Admin
```

- [ ] **Step 9: Bulk update import paths across ALL features**

After all renames and moves, run comprehensive import path fixes:

```bash
cd /home/qingfa/Repos/ReSys.Shop/app/Admin

# View → Page in imports
find src/features -name '*.vue' -o -name '*.ts' | xargs sed -i "s|\.View\.vue|Page.vue|g"

# .Component → none
find src/features -name '*.vue' -o -name '*.ts' | xargs sed -i "s|\.Component\.vue|.vue|g"

# stores/ → store/
find src/features -name '*.vue' -o -name '*.ts' | xargs sed -i "s|/stores/|/store/|g"

# schemas/ → types/ + .schema → .field
find src/features -name '*.vue' -o -name '*.ts' | xargs sed -i "s|/schemas/|/types/|g"
find src/features -name '*.vue' -o -name '*.ts' | xargs sed -i "s|\.schema|.field|g"

# Drop .type infix
find src/features -name '*.ts' -o -name '*.vue' | xargs sed -i "s|\.model\.type|.model|g"
find src/features -name '*.ts' -o -name '*.vue' | xargs sed -i "s|\.request\.type|.request|g"
find src/features -name '*.ts' -o -name '*.vue' | xargs sed -i "s|\.response\.type|.response|g"
find src/features -name '*.ts' -o -name '*.vue' | xargs sed -i "s|\.parameters\.type|.parameters|g"
find src/features -name '*.ts' -o -name '*.vue' | xargs sed -i "s|\.query\.type|.query|g"

# services/ → api/ (where merged)
find src/features -name '*.ts' -o -name '*.vue' | xargs sed -i "s|/services/|/api/|g"

# mappers/ → models/
find src/features -name '*.ts' -o -name '*.vue' | xargs sed -i "s|/mappers/|/models/|g"

# _tests/ → __tests__/
find src/features -name '*.spec.ts' | xargs sed -i "s|/_tests/|/__tests__/|g"
find src/features -name '*.spec.ts' | xargs sed -i "s|/tests/|/__tests__/|g"
```

- [ ] **Step 10: Verification gate**

```bash
pnpm run type-check && pnpm run lint && pnpm run test:unit
```

Fix remaining import errors until all three pass.

- [ ] **Step 11: Commit**

```bash
git add -A app/Admin/src/features
git commit -m "refactor(admin): restructure all features — pages, store, types, tests, services merge"
```

---

### Task 9: Rename app layouts + update router imports

**Files:** `src/app/layout/`, `src/app/router/index.ts`, `src/app/main.ts`

- [ ] **Step 1: Rename layout components (drop .Layout qualifier)**

```bash
cd /home/qingfa/Repos/ReSys.Shop/app/Admin/src/app/layout

git mv Main.Layout.vue MainLayout.vue
git mv Sidebar.Layout.vue SidebarLayout.vue
git mv Topbar.Layout.vue TopbarLayout.vue
git mv Menu.Layout.vue MenuLayout.vue
git mv MenuItem.Layout.vue MenuItemLayout.vue
git mv Footer.Layout.vue FooterLayout.vue
git mv Configurator.Layout.vue ConfiguratorLayout.vue

# Layout sub-components (drop .Component):
git mv components/FloatingConfigurator.Component.vue components/FloatingConfigurator.vue
git mv components/GlobalSearch.Component.vue components/GlobalSearch.vue

cd /home/qingfa/Repos/ReSys.Shop/app/Admin
```

- [ ] **Step 2: Update internal layout imports**

```bash
sed -i "s|Main\.Layout\.vue|MainLayout.vue|g" src/app/layout/SidebarLayout.vue
sed -i "s|Menu\.Layout\.vue|MenuLayout.vue|g" src/app/layout/SidebarLayout.vue
sed -i "s|Main\.Layout\.vue|MainLayout.vue|g" src/app/router/index.ts
sed -i "s|Configurator\.Layout\.vue|ConfiguratorLayout.vue|g" src/app/layout/MainLayout.vue

# Update .Component → .vue in layout imports
sed -i "s|\.Component\.vue|.vue|g" src/app/layout/MainLayout.vue
sed -i "s|\.Component\.vue|.vue|g" src/app/layout/SidebarLayout.vue
```

- [ ] **Step 3: Update router/index.ts — all feature page imports**

Read `src/app/router/index.ts`. It imports feature routes by their module names. After Task 8, these paths changed:

```typescript
// Before:
import { catalogRoutes } from '@/features/catalog/catalog.routes'
import { reportsRoutes } from '@/features/reports/reports.routes'

// After: (same — route files didn't move)
```

Route files (`catalog.routes.ts`, etc.) stayed at feature root. But the **lazy-loaded components** within routes changed:

```typescript
// Before:
component: () => import('@/features/catalog/products/views/ProductList.View.vue')
// After:
component: () => import('@/features/catalog/products/pages/ProductListPage.vue')
```

Run bulk fixes on route files:

```bash
cd /home/qingfa/Repos/ReSys.Shop/app/Admin
# Fix lazy imports in all route files
find src/features -name '*.routes.ts' | xargs sed -i "s|/views/|/pages/|g"
find src/features -name '*.routes.ts' | xargs sed -i "s|\.View\.vue|Page.vue|g"

# Fix router's own import of AppLayout
sed -i "s|Main\.Layout\.vue|MainLayout.vue|g" src/app/router/index.ts
```

- [ ] **Step 4: Update main.ts layout import**

```bash
sed -i "s|Main\.Layout\.vue|MainLayout.vue|g" src/app/main.ts || true
```

- [ ] **Step 5: Update layout composable import (if using MainLayout internally)**

```bash
sed -i "s|\.Component\.vue|.vue|g" src/app/layout/composables/layout.composable.ts || true
```

- [ ] **Step 6: Verification gate**

```bash
pnpm run type-check && pnpm run lint && pnpm run test:unit
```

- [ ] **Step 7: Commit**

```bash
git add -A app/Admin/src/app
git commit -m "refactor(admin): rename app layouts, drop .Layout qualifier, update router imports"
```

---

### Task 10: Fix remaining review issues (#2, #3, #12, #14, #18)

**Files:**
- `src/common/api/http/interceptors/error-wrapper.interceptor.ts` — #2
- `src/common/api/http/handlers/refresh-handler.ts` — #3
- `src/shared/components/tables/DataTableShell.vue` — #12
- `src/common/api/http/handlers/refresh-handler.ts` — #3 (same file)
- (all features) — #14 (test dirs already done in Tasks 7-8)
- `src/features/catalog/products/api/product.api.ts` — #18

- [ ] **Step 1: Fix #2 — URL guard in `error-wrapper.interceptor.ts`**

Read L13. Change `/auth/session/refresh` to `/sessions/refresh`:

```typescript
// Before:
if (originalRequest.url?.includes('/auth/session/refresh')) {
// After:
if (originalRequest.url?.includes('/sessions/refresh')) {
```

- [ ] **Step 2: Fix #3 — Move redirect out of `refresh-handler.ts`**

Read `src/common/api/http/handlers/refresh-handler.ts`. Extract the redirect:

```typescript
// Before (L5-8):
  const token = localStorage.getItem('refreshToken')
  if (!token) {
    window.location.href = '/login'
    return false
  }

// After:
  const token = localStorage.getItem('refreshToken')
  if (!token) {
    return false  // caller decides how to handle
  }
```

Also remove `window.location.href = '/login'` from the catch block (L32):

```typescript
// Before (L29-34):
  } catch {
    localStorage.removeItem('accessToken')
    localStorage.removeItem('refreshToken')
    window.location.href = '/login'
    return false
  }

// After:
  } catch {
    localStorage.removeItem('accessToken')
    localStorage.removeItem('refreshToken')
    return false
  }
```

Now update the caller — `error-wrapper.interceptor.ts` — to handle the redirect when refresh fails:

```typescript
// In errorWrapperInterceptor, after the refreshTokens call:
const refreshed = await refreshTokens()
if (refreshed) {
  // ... existing retry logic ...
} else {
  // Redirect to login on failed refresh
  router.push('/login')  // or window.location.href = '/login'
  return Promise.reject(error)
}
```

- [ ] **Step 3: Fix #12 — Type `createRoute` in `DataTableShell.vue`**

Read the `DataTableShell.vue` props. Change:

```typescript
// Before:
  createRoute?: any

// After:
import type { RouteLocationRaw } from 'vue-router'
  createRoute?: RouteLocationRaw
```

- [ ] **Step 4: Fix #14 — Test directory standardization**

Already handled in Tasks 7-8 steps. Verify:

```bash
find src/features -type d -name '_tests' && echo "FAIL: _tests/ still exists" || echo "OK: _tests/ cleaned"
find src/features -type d -name 'tests' ! -path '*/__tests__/*' && echo "FAIL: tests/ still exists" || echo "OK: tests/ cleaned"
```

- [ ] **Step 5: Fix #18 — Unify async style in `product.api.ts`**

Read `src/features/catalog/products/api/product.api.ts`. It mixes `async/await` with `.then()`. Unify to `async/await`:

```typescript
// Before:
list: async (params?: ServerQueryingParameters): Promise<ServerPagedResult<ProductSummaryModel>> => {
    const result = await apiClient.get(`${CATALOG}/products`, { params }).then(res => res.data as ServerPagedResult<ProductSummary>)
    return mapItems(result, d => ({ ...d, statusLabel: ProductStatusMap[d.status] ?? 'Unknown' }))
  },

// After:
list: async (params?: ServerQueryingParameters): Promise<ServerPagedResult<ProductSummaryModel>> => {
    const res = await apiClient.get(`${CATALOG}/products`, { params })
    const result = res.data as ServerPagedResult<ProductSummary>
    return mapItems(result, d => ({ ...d, statusLabel: ProductStatusMap[d.status] ?? 'Unknown' }))
  },
```

Apply same pattern to `getById`, `create`, `update`. Delete remains as `.then()` (one-liner, fine).

- [ ] **Step 6: Verification gate**

```bash
pnpm run type-check && pnpm run lint && pnpm run test:unit
```

- [ ] **Step 7: Commit**

```bash
git add -A app/Admin/src/common/api/http app/Admin/src/shared/components/tables app/Admin/src/features/catalog/products
git commit -m "fix(admin): review issues #2, #3, #12, #14, #18"
```

---

### Task 11: Create new shared components (ErrorState + Drawer)

**Files:**
- Create: `src/shared/components/feedback/ErrorState.vue`
- Create: `src/shared/components/feedback/__tests__/ErrorState.test.ts`
- Create: `src/shared/components/feedback/Drawer.vue`
- Create: `src/shared/components/feedback/__tests__/Drawer.test.ts`

- [ ] **Step 1: Create `ErrorState.vue`**

```vue
<script setup lang="ts">
defineProps<{
  title?: string
  message?: string
  icon?: string
  retryLabel?: string
}>()

const emit = defineEmits<{
  retry: []
}>()

withDefaults(defineProps<{
  title?: string
  message?: string
  icon?: string
  retryLabel?: string
}>(), {
  title: 'Something went wrong',
  message: 'An unexpected error occurred. Please try again.',
  icon: 'pi-exclamation-triangle',
})
</script>

<template>
  <div class="flex flex-col items-center justify-center gap-4 py-16 text-center">
    <i :class="['pi', icon, 'text-6xl text-muted-color']" />
    <h3 class="text-xl font-semibold text-surface-700 dark:text-surface-300">{{ title }}</h3>
    <p class="max-w-md text-muted-color">{{ message }}</p>
    <Button v-if="retryLabel" :label="retryLabel" icon="pi-refresh" @click="emit('retry')" />
  </div>
</template>
```

- [ ] **Step 2: Write the failing test for `ErrorState.vue`**

```typescript
// src/shared/components/feedback/__tests__/ErrorState.test.ts
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import ErrorState from '../ErrorState.vue'

describe('ErrorState', () => {
  it('renders default title and message', () => {
    const wrapper = mount(ErrorState)
    expect(wrapper.text()).toContain('Something went wrong')
    expect(wrapper.text()).toContain('An unexpected error occurred')
  })

  it('renders custom title and message', () => {
    const wrapper = mount(ErrorState, {
      props: { title: 'Custom Error', message: 'Custom message' },
    })
    expect(wrapper.text()).toContain('Custom Error')
    expect(wrapper.text()).toContain('Custom message')
  })

  it('emits retry when button clicked', async () => {
    const wrapper = mount(ErrorState, {
      props: { retryLabel: 'Retry' },
    })
    await wrapper.find('button').trigger('click')
    expect(wrapper.emitted('retry')).toBeTruthy()
  })

  it('hides retry button when no label provided', () => {
    const wrapper = mount(ErrorState)
    expect(wrapper.find('button').exists()).toBe(false)
  })
})
```

- [ ] **Step 3: Run test to verify it fails (or passes)**

```bash
pnpm run test:unit -- ErrorState
```

- [ ] **Step 4: Create `Drawer.vue`**

```vue
<script setup lang="ts">
import { ref, watch } from 'vue'
import Drawer from 'primevue/drawer'

const props = withDefaults(defineProps<{
  visible: boolean
  header?: string
  position?: 'left' | 'right' | 'top' | 'bottom'
  width?: string
}>(), {
  position: 'right',
  width: '30rem',
})

const emit = defineEmits<{
  'update:visible': [value: boolean]
}>()

const localVisible = ref(props.visible)
watch(() => props.visible, (v) => { localVisible.value = v })
watch(localVisible, (v) => { emit('update:visible', v) })
</script>

<template>
  <Drawer v-model:visible="localVisible" :header="header" :position="position" :style="{ width }">
    <slot />
  </Drawer>
</template>
```

- [ ] **Step 5: Write the failing test for `Drawer.vue`**

```typescript
// src/shared/components/feedback/__tests__/Drawer.test.ts
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import Drawer from '../Drawer.vue'
import PrimeVue from 'primevue/config'

describe('Drawer', () => {
  const mountWithPrimevue = (props = {}) =>
    mount(Drawer, {
      props: { visible: true, ...props },
      global: { plugins: [PrimeVue] },
    })

  it('renders when visible', () => {
    const wrapper = mountWithPrimevue()
    expect(wrapper.find('.p-drawer').exists()).toBe(true)
  })

  it('renders header text', () => {
    const wrapper = mountWithPrimevue({ header: 'Test Drawer' })
    expect(wrapper.text()).toContain('Test Drawer')
  })

  it('renders slot content', () => {
    const wrapper = mount(Drawer, {
      props: { visible: true },
      slots: { default: '<div class="slot-content">Content</div>' },
      global: { plugins: [PrimeVue] },
    })
    expect(wrapper.text()).toContain('Content')
  })
})
```

- [ ] **Step 6: Run tests**

```bash
pnpm run test:unit -- ErrorState
pnpm run test:unit -- Drawer
```

- [ ] **Step 7: Verification gate**

```bash
pnpm run type-check && pnpm run lint && pnpm run test:unit
```

- [ ] **Step 8: Commit**

```bash
git add -A app/Admin/src/shared/components/feedback
git commit -m "feat(admin): add ErrorState and Drawer shared components"
```

---

### Task 12: Final verification + cleanup

- [ ] **Step 1: Full verification**

```bash
cd /home/qingfa/Repos/ReSys.Shop/app/Admin
pnpm run type-check && pnpm run lint && pnpm run test:unit
```

- [ ] **Step 2: Check for any stale files or directories**

```bash
# Empty directories that should be removed
find src -type d -empty -not -path '*/node_modules/*' | sort

# Files still using old naming patterns
rg "\.Component\.vue$" src/features/ src/app/ --files-with-matches
rg "\.View\.vue$" src/features/ --files-with-matches
rg "\.Layout\.vue$" src/app/ --files-with-matches

# Stale @/shared/ imports that should be @/common/
rg "from '@/shared/(api|composables|services|mapper|config|test)/" src/features/ src/app/ --count
```

Expected: zero results for old patterns. If any found, fix.

- [ ] **Step 3: Run full test suite one final time**

```bash
pnpm run test:unit
```

- [ ] **Step 4: Build check**

```bash
pnpm run build-only
```

- [ ] **Step 5: Commit**

```bash
git add -A app/Admin
git commit -m "chore(admin): final cleanup — remove stale dirs, verify full refactor"
```
