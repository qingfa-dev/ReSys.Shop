# Admin SPA Architecture Cleanup — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix all ~25 findings from the 2026-07-22 architecture review across 5 independently-verifiable phases.

**Architecture:** Phased cleanup of the existing Admin SPA. Each phase is independently verifiable (build + lint + 59/59 tests). Deletions happen first, then consolidation, then extraction, then structural improvements.

**Tech Stack:** Vue 3.5 + TypeScript 6.0 + Pinia 3.0 + PrimeVue 5.0 + Axios + Zod 3.25 + Vitest

## Global Constraints

- All phases independently pass: `cd app/Admin && pnpm run lint && pnpm run build && pnpm run test:unit`
- 59 existing tests must remain green (or be intentionally updated with matching assertions)
- No feature page changes — all 32 pages remain PlaceholderPage stubs
- No changes to PrimeVue preset, design tokens, or visual styles
- Commit after each task

---

### Task 1: Fix API URL doubling in auth.service.ts

**Files:**
- Modify: `app/Admin/src/shared/auth/auth.service.ts:29,41,48`

**Interfaces:**
- Consumes: `apiClient` from `@/shared/api/client` (baseURL already set to `/api`)
- Produces: Same `AuthService` class, corrected URL paths

- [ ] **Step 1: Drop `/api` prefix from all 3 URL paths**

```ts
// L29 — before:
const response = await apiClient.post<Result<LoginResponse>>(
  '/api/store/identity/auth/sessions/login',
  request
)
// after:
const response = await apiClient.post<Result<LoginResponse>>(
  '/store/identity/auth/sessions/login',
  request
)

// L41 — before:
await apiClient.post('/api/store/identity/auth/sessions/logout', {
  refreshToken,
})
// after:
await apiClient.post('/store/identity/auth/sessions/logout', {
  refreshToken,
})

// L48 — before:
const response = await apiClient.get<Result<CurrentUser>>(
  '/api/store/identity/auth/sessions/me'
)
// after:
const response = await apiClient.get<Result<CurrentUser>>(
  '/store/identity/auth/sessions/me'
)
```

- [ ] **Step 2: Verify — lint + build + tests**

Run: `cd app/Admin && pnpm run lint && pnpm run build && pnpm run test:unit`
Expected: lint passes, build passes, 59/59 tests pass

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/shared/auth/auth.service.ts
git commit -m "fix: remove duplicate /api prefix from auth service URLs"
```

---

### Task 2: Fix dark mode bidirectional watch loop

**Files:**
- Modify: `app/Admin/src/app/composables/layout.composable.ts:99-108`

**Interfaces:**
- Consumes: `useDarkMode()` returning `{ isDark, toggle }`
- Produces: Same `useLayout()` API. `toggleDarkMode()` still calls `useDarkMode().toggle()`. `isDarkTheme` returns `isDark`. Forward watch `isDark → layoutConfig.darkTheme` stays. Reverse watch removed.

- [ ] **Step 1: Remove the reverse watch**

In `layout.composable.ts`, remove lines 105-108 (the watch that syncs `layoutConfig.darkTheme` back to `isDark.value`):

```ts
// DELETE this block (lines ~105-108):
watch(() => layoutConfig.darkTheme, (val) => { if (val !== isDarkTheme.value) isDarkTheme.value = val })
```

Keep the forward watch at line ~103:
```ts
watch(isDarkTheme, (val) => { layoutConfig.darkTheme = val })
```

- [ ] **Step 2: Verify — lint + build + tests**

Run: `cd app/Admin && pnpm run lint && pnpm run build && pnpm run test:unit`
Expected: lint passes, build passes, 59/59 tests pass

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/app/composables/layout.composable.ts
git commit -m "fix: remove bidirectional dark mode watch loop in layout composable"
```

---

### Task 3: Fix fragile refresh URL match in error interceptor

**Files:**
- Modify: `app/Admin/src/shared/api/interceptors/error-wrapper.interceptor.ts:14`

**Interfaces:**
- Consumes: `originalRequest.url` from Axios error config
- Produces: Same interceptor, hardened URL match

- [ ] **Step 1: Replace `includes` with `endsWith`**

```ts
// L14 — before:
if (originalRequest.url?.includes('/sessions/refresh')) {
// after:
if (originalRequest.url?.endsWith('/sessions/refresh')) {
```

- [ ] **Step 2: Verify — lint + build + tests**

Run: `cd app/Admin && pnpm run lint && pnpm run build && pnpm run test:unit`
Expected: lint passes, build passes, 59/59 tests pass

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/shared/api/interceptors/error-wrapper.interceptor.ts
git commit -m "fix: use endsWith for refresh endpoint match to prevent false positives"
```

---

### Task 4: Delete unused code (Phase 2 — 9 deletions + barrel updates)

**Files:**
- Delete: `app/Admin/src/stores/counter.ts`
- Delete: `app/Admin/src/shared/components/navigation/Breadcrumb.vue`
- Delete: `app/Admin/src/shared/utils/debounce.ts`
- Delete: `app/Admin/src/shared/composables/useToastNotify.ts`
- Delete: `app/Admin/src/shared/styles/animations.scss`
- Delete: `app/Admin/src/shared/styles/mixins.scss`
- Delete: `app/Admin/src/shared/styles/typography.scss`
- Delete: `app/Admin/src/shared/styles/variables.scss`
- Delete: `app/Admin/src/shared/services/event-bus.service.ts`
- Delete: `app/Admin/src/shared/services/logger.service.ts`
- Delete: `app/Admin/src/shared/services/notification.service.ts`
- Modify: `app/Admin/src/shared/utils/index.ts` — remove debounce export
- Modify: `app/Admin/src/shared/composables/index.ts` — remove useToastNotify export
- Modify: `app/Admin/src/shared/services/index.ts` — update to only export modal.service

**Interfaces:**
- Consumes: (none — deletions)
- Produces: Cleaned barrel exports; no consumer references to deleted files

- [ ] **Step 1: Delete the 11 files**

```bash
cd app/Admin
rm src/stores/counter.ts
rm src/shared/components/navigation/Breadcrumb.vue
rm src/shared/utils/debounce.ts
rm src/shared/composables/useToastNotify.ts
rm src/shared/styles/animations.scss
rm src/shared/styles/mixins.scss
rm src/shared/styles/typography.scss
rm src/shared/styles/variables.scss
rm src/shared/services/event-bus.service.ts
rm src/shared/services/logger.service.ts
rm src/shared/services/notification.service.ts
```

- [ ] **Step 2: Update `shared/utils/index.ts`**

Remove the `debounce` export line. Read the file first to locate the exact line.

Expected after edit — remove this line:
```ts
export { debounce } from './debounce'
```

- [ ] **Step 3: Update `shared/composables/index.ts`**

Remove the `useToastNotify` export line:
```ts
export { useToastNotify } from './useToastNotify'
```

- [ ] **Step 4: Update `shared/services/index.ts`**

Rewrite to only export modal.service:
```ts
export { useModalService } from './modal.service'
```

- [ ] **Step 5: Remove chart.js from dependencies**

```bash
cd app/Admin && pnpm remove chart.js
```

- [ ] **Step 6: Verify — lint + build + tests**

Run: `cd app/Admin && pnpm run lint && pnpm run build && pnpm run test:unit`
Expected: lint passes, build passes, 59/59 tests pass

- [ ] **Step 7: Commit**

```bash
cd /home/qingfa/Repos/ReSys.Shop
git add app/Admin/
git commit -m "chore: remove unused code (counter store, duplicate breadcrumb, dead services, unused deps)"
```

---

### Task 5: Remove empty directories after deletions

**Files:**
- Remove empty directories: `app/Admin/src/shared/components/navigation/`, `app/Admin/src/shared/styles/`

**Interfaces:**
- Produces: Clean directory tree with no empty orphan directories

- [ ] **Step 1: Delete empty directories**

```bash
cd app/Admin
rmdir src/shared/components/navigation 2>/dev/null || true
rmdir src/shared/styles 2>/dev/null || true
```

- [ ] **Step 2: Verify build still passes**

Run: `cd app/Admin && pnpm run lint && pnpm run build`
Expected: no errors

- [ ] **Step 3: Commit**

```bash
git add app/Admin/
git commit -m "chore: remove empty directories after deletions"
```

---

### Task 6: Absorb useToastNotify into useToast

**Files:**
- Modify: `app/Admin/src/shared/composables/useToast.ts`

**Interfaces:**
- Consumes: `useToast as usePrimeToast` from `primevue/usetoast`
- Produces: `{ showToast, success, error, warn, info }` — same API as old `useToastNotify` + underlying `showToast`

- [ ] **Step 1: Add sugar methods to useToast.ts**

Replace `app/Admin/src/shared/composables/useToast.ts`:

```ts
import { useToast as usePrimeToast } from 'primevue/usetoast'

export function useToast() {
  const toast = usePrimeToast()

  const showToast = (
    severity: 'success' | 'info' | 'warn' | 'error',
    summary: string,
    detail: string,
    life = 3000,
  ) => {
    toast.add({ severity, summary, detail, life })
  }

  const success = (detail: string, summary = 'Success') =>
    showToast('success', summary, detail)
  const error = (detail: string, summary = 'Error') =>
    showToast('error', summary, detail, 5000)
  const warn = (detail: string, summary = 'Warning') =>
    showToast('warn', summary, detail, 4000)
  const info = (detail: string, summary = 'Info') =>
    showToast('info', summary, detail)

  return { showToast, success, error, warn, info }
}
```

- [ ] **Step 2: Verify — lint + build + tests**

Run: `cd app/Admin && pnpm run lint && pnpm run build && pnpm run test:unit`
Expected: lint passes, build passes, 59/59 tests pass

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/shared/composables/useToast.ts
git commit -m "refactor: merge useToastNotify sugar methods into useToast"
```

---

### Task 7: Deduplicate useFormatter — remove formatCurrency and formatDate

**Files:**
- Modify: `app/Admin/src/shared/composables/useFormatter.ts`
- Modify: `app/Admin/src/shared/composables/__tests__/formatter.spec.ts`

**Interfaces:**
- Consumes: (none — pure utility)
- Produces: `{ formatNumber, truncate }` — stripped of currency/date methods (covered by `useCurrency`/`useDate`)

- [ ] **Step 1: Read current useFormatter.ts, then strip currency/date methods**

After edit, `useFormatter.ts` becomes:

```ts
export function useFormatter() {
  const formatNumber = (value: number | null | undefined, decimals = 0): string => {
    if (value === null || value === undefined) return '-'
    return new Intl.NumberFormat('en-US', {
      minimumFractionDigits: decimals,
      maximumFractionDigits: decimals,
    }).format(value)
  }

  const truncate = (text: string | null | undefined, length: number): string => {
    if (!text) return ''
    if (text.length <= length) return text
    return text.substring(0, length) + '...'
  }

  return { formatNumber, truncate }
}
```

- [ ] **Step 2: Update formatter.spec.ts — replace currency test with formatNumber test**

The current test destructures `{ formatCurrency, truncate }` and tests those. After removing `formatCurrency` from `useFormatter`, the test must be updated. Replace the entire spec file:

```ts
import { describe, it, expect } from 'vitest'
import { useFormatter } from '../useFormatter'

describe('useFormatter', () => {
  const { formatNumber, truncate } = useFormatter()

  describe('formatNumber', () => {
    it('formats integers with comma separators', () => {
      expect(formatNumber(1234)).toBe('1,234')
    })

    it('formats with decimal places', () => {
      expect(formatNumber(1234.567, 2)).toBe('1,234.57')
    })

    it('returns dash for null or undefined', () => {
      expect(formatNumber(null)).toBe('-')
      expect(formatNumber(undefined)).toBe('-')
    })
  })

  describe('truncate', () => {
    it('truncates long strings', () => {
      expect(truncate('Hello World', 5)).toBe('Hello...')
    })

    it('does not truncate short strings', () => {
      expect(truncate('Hello', 10)).toBe('Hello')
    })

    it('handles null or undefined', () => {
      expect(truncate(null, 10)).toBe('')
      expect(truncate(undefined, 5)).toBe('')
    })
  })
})
```

- [ ] **Step 3: Verify — lint + build + tests**

Run: `cd app/Admin && pnpm run lint && pnpm run build && pnpm run test:unit`
Expected: lint passes, build passes, all remaining tests pass

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/shared/composables/useFormatter.ts app/Admin/src/shared/composables/__tests__/formatter.spec.ts
git commit -m "refactor: remove duplicate formatCurrency/formatDate from useFormatter"
```

---

### Task 8: Make useResponsive consume useWindowSize

**Files:**
- Modify: `app/Admin/src/shared/composables/useResponsive.ts`

**Interfaces:**
- Consumes: `{ width }` from `useWindowSize()`
- Produces: `{ isMobile, isTablet, isDesktop, isWide }`

- [ ] **Step 1: Rewrite useResponsive to use useWindowSize internally**

Replace `app/Admin/src/shared/composables/useResponsive.ts`:

```ts
import { computed } from 'vue'
import { useWindowSize } from './useWindowSize'

const BP = { sm: 640, md: 768, lg: 1024, xl: 1280, xxl: 1536 } as const

export function useResponsive() {
  const { width } = useWindowSize()

  const isMobile = computed(() => width.value < BP.md)
  const isTablet = computed(() => width.value >= BP.md && width.value < BP.lg)
  const isDesktop = computed(() => width.value >= BP.lg && width.value < BP.xl)
  const isWide = computed(() => width.value >= BP.xl)

  return { isMobile, isTablet, isDesktop, isWide }
}
```

- [ ] **Step 2: Verify — lint + build + tests**

Run: `cd app/Admin && pnpm run lint && pnpm run build && pnpm run test:unit`
Expected: lint passes, build passes, 59/59 tests pass

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/shared/composables/useResponsive.ts
git commit -m "refactor: make useResponsive consume useWindowSize to deduplicate resize listener"
```

---

### Task 9: Add barrel export for shared/components

**Files:**
- Create: `app/Admin/src/shared/components/index.ts`

**Interfaces:**
- Produces: Named re-exports for all 22 shared components

- [ ] **Step 1: Create component barrel**

Create `app/Admin/src/shared/components/index.ts`:

```ts
export { default as DataTable } from './data/DataTable.vue'
export { default as DetailField } from './data/DetailField.vue'
export { default as StatCard } from './data/StatCard.vue'
export { default as StatusTag } from './data/StatusTag.vue'

export { default as ConfirmDialog } from './feedback/ConfirmDialog.vue'
export { default as EmptyState } from './feedback/EmptyState.vue'
export { default as ErrorState } from './feedback/ErrorState.vue'
export { default as LoadingSkeleton } from './feedback/LoadingSkeleton.vue'

export { default as FormActions } from './forms/FormActions.vue'
export { default as FormField } from './forms/FormField.vue'
export { default as ImageUploader } from './forms/ImageUploader.vue'
export { default as PriceInput } from './forms/PriceInput.vue'
export { default as SearchableSelect } from './forms/SearchableSelect.vue'

export { default as ActionMenu } from './layout/ActionMenu.vue'
export { default as BulkActionBar } from './layout/BulkActionBar.vue'
export { default as PageHeader } from './layout/PageHeader.vue'
export { default as PlaceholderPage } from './layout/PlaceholderPage.vue'
export { default as TableToolbar } from './layout/TableToolbar.vue'

export { default as DetailDrawer } from './overlays/DetailDrawer.vue'
export { default as FilterPanel } from './overlays/FilterPanel.vue'
export { default as Modal } from './overlays/Modal.vue'
```

- [ ] **Step 2: Verify — lint + build**

Run: `cd app/Admin && pnpm run lint && pnpm run build`
Expected: lint passes, build passes

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/shared/components/index.ts
git commit -m "feat: add barrel export for shared components"
```

---

### Task 10: Split layout.composable.ts — extract useLayoutConfig

**Files:**
- Create: `app/Admin/src/app/composables/useLayoutConfig.ts`
- Modify: `app/Admin/src/app/composables/layout.composable.ts`

**Interfaces:**
- Consumes: (none — standalone)
- Produces: `{ layoutConfig, changeMenuMode }`

- [ ] **Step 1: Create useLayoutConfig.ts**

Create `app/Admin/src/app/composables/useLayoutConfig.ts`:

```ts
import { reactive, watch } from 'vue'

const STORAGE_KEY = 'resys-admin-layout'

export interface LayoutConfig {
  preset: string
  primary: string
  surface: string | null
  darkTheme: boolean
  menuMode: string
}

function loadConfig(): Partial<LayoutConfig> {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    return raw ? JSON.parse(raw) : {}
  } catch {
    return {}
  }
}

const saved = loadConfig()

export const layoutConfig = reactive<LayoutConfig>({
  preset: saved.preset || 'Aura',
  primary: saved.primary || 'emerald',
  surface: (saved.surface as string | null) || null,
  darkTheme: saved.darkTheme ?? false,
  menuMode: saved.menuMode || 'static',
})

watch(
  () => ({ ...layoutConfig }),
  (val) => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(val))
  },
  { deep: true },
)

export function changeMenuMode(mode: string) {
  layoutConfig.menuMode = mode
}
```

- [ ] **Step 2: Update layout.composable.ts to import from new file**

In `layout.composable.ts`, remove the `LayoutConfig` interface, `loadConfig`, `saved`, `layoutConfig` reactive, the config localStorage watch, and `changeMenuMode`. Import them from `./useLayoutConfig`:

```ts
import { layoutConfig, changeMenuMode, type LayoutConfig } from './useLayoutConfig'
```

Also add `import { useDarkMode } from '@/shared/composables/useDarkMode'` at top (was previously implicit).

- [ ] **Step 3: Verify — lint + build + tests**

Run: `cd app/Admin && pnpm run lint && pnpm run build && pnpm run test:unit`
Expected: lint passes, build passes, 59/59 tests pass

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/app/composables/
git commit -m "refactor: extract useLayoutConfig from layout.composable"
```

---

### Task 11: Split layout.composable.ts — extract useLayoutState

**Files:**
- Create: `app/Admin/src/app/composables/useLayoutState.ts`
- Modify: `app/Admin/src/app/composables/layout.composable.ts`

**Interfaces:**
- Consumes: `layoutConfig` from `./useLayoutConfig`
- Produces: `{ layoutState, hideMobileMenu }`

- [ ] **Step 1: Create useLayoutState.ts**

Create `app/Admin/src/app/composables/useLayoutState.ts`:

```ts
import { reactive } from 'vue'

export const layoutState = reactive({
  staticMenuInactive: false,
  overlayMenuActive: false,
  profileSidebarVisible: false,
  configSidebarVisible: false,
  sidebarExpanded: false,
  menuHoverActive: false,
  activeMenuItem: null as string | null,
  activePath: null as string | null,
  mobileMenuActive: false,
})

export function hideMobileMenu() {
  layoutState.mobileMenuActive = false
}
```

- [ ] **Step 2: Update layout.composable.ts to import from new file**

Remove `layoutState` reactive and `hideMobileMenu` from `layout.composable.ts`. Import them:

```ts
import { layoutState, hideMobileMenu } from './useLayoutState'
```

- [ ] **Step 3: Verify — lint + build + tests**

Run: `cd app/Admin && pnpm run lint && pnpm run build && pnpm run test:unit`
Expected: lint passes, build passes, 59/59 tests pass

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/app/composables/
git commit -m "refactor: extract useLayoutState from layout.composable"
```

---

### Task 12: Finalize useLayout orchestrator

**Files:**
- Modify: `app/Admin/src/app/composables/layout.composable.ts`

**Interfaces:**
- Consumes: `layoutConfig`, `changeMenuMode` from `./useLayoutConfig`; `layoutState`, `hideMobileMenu` from `./useLayoutState`; `useDarkMode` from `@/shared/composables/useDarkMode`
- Produces: `useLayout()` returning same public API

- [ ] **Step 1: Clean up layout.composable.ts to be pure orchestrator**

After Task 10 and 11's extractions, `layout.composable.ts` should contain only:

```ts
import { computed, watch } from 'vue'
import { useDarkMode } from '@/shared/composables/useDarkMode'
import { layoutConfig, changeMenuMode } from './useLayoutConfig'
import { layoutState, hideMobileMenu } from './useLayoutState'

export function useLayout() {
  const { isDark, toggle } = useDarkMode()

  const isDarkTheme = isDark

  watch(isDarkTheme, (val) => { layoutConfig.darkTheme = val })

  function toggleDarkMode() {
    if (!document.startViewTransition) {
      toggle()
      return
    }
    const transition = document.startViewTransition(() => toggle())
    transition.ready.then(() => {
      const x = window.innerWidth / 2
      const y = window.innerHeight / 2
      const endRadius = Math.hypot(window.innerWidth, window.innerHeight)
      document.documentElement.animate(
        { clipPath: [`circle(0 at ${x}px ${y}px)`, `circle(${endRadius}px at ${x}px ${y}px)`] },
        { duration: 400, easing: 'ease-in', pseudoElement: '::view-transition-new(root)' },
      )
    })
  }

  const isDesktop = () => window.innerWidth > 991

  function toggleMenu() {
    if (isDesktop()) {
      if (layoutConfig.menuMode === 'static') {
        layoutState.staticMenuInactive = !layoutState.staticMenuInactive
      }
      if (layoutConfig.menuMode === 'overlay') {
        layoutState.overlayMenuActive = !layoutState.overlayMenuActive
      }
    } else {
      layoutState.mobileMenuActive = !layoutState.mobileMenuActive
    }
  }

  function toggleConfigSidebar() {
    layoutState.configSidebarVisible = !layoutState.configSidebarVisible
  }

  const hasOpenOverlay = computed(() => layoutState.overlayMenuActive || layoutState.mobileMenuActive)

  return {
    layoutConfig,
    layoutState,
    isDarkTheme,
    toggleDarkMode,
    toggleConfigSidebar,
    toggleMenu,
    hideMobileMenu,
    changeMenuMode,
    isDesktop,
    hasOpenOverlay,
  }
}
```

- [ ] **Step 2: Verify — lint + build + tests**

Run: `cd app/Admin && pnpm run lint && pnpm run build && pnpm run test:unit`
Expected: lint passes, build passes, 59/59 tests pass

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/app/composables/layout.composable.ts
git commit -m "refactor: finalize useLayout as orchestrator over useLayoutConfig and useLayoutState"
```

---

### Task 13: Extract isRouteActive from MenuItemLayout

**Files:**
- Create: `app/Admin/src/app/config/route-matcher.ts`
- Modify: `app/Admin/src/app/layout/MenuItemLayout.vue`

**Interfaces:**
- Consumes: `MenuItem` from `admin-menu.config`, `route` from vue-router
- Produces: `isRouteActive(item, path, name)` — pure boolean function

- [ ] **Step 1: Create route-matcher.ts**

Create `app/Admin/src/app/config/route-matcher.ts`:

```ts
import type { MenuItem } from './admin-menu.config'

function isRouteMatch(
  target: string | { name?: string } | undefined,
  path: string,
  name: string | symbol | null | undefined,
): boolean {
  if (!target) return false
  if (typeof target === 'string') return target === path
  if (typeof target === 'object' && 'name' in target) {
    return name === target.name
  }
  return false
}

export function isRouteActive(
  item: MenuItem,
  path: string,
  name: string | symbol | null | undefined,
): boolean {
  if (isRouteMatch(item.to, path, name)) return true
  if (item.items) {
    return item.items.some((child) => isRouteMatch(child.to, path, name)
      || (child.items ? child.items.some((sub) => isRouteMatch(sub.to, path, name)) : false))
  }
  return false
}
```

- [ ] **Step 2: Update MenuItemLayout.vue — use extracted function**

In `MenuItemLayout.vue`:
- Import `isRouteActive` from `@/app/config/route-matcher`
- Replace the `isActive` computed body (L24-47) with: `return isRouteActive(props.item, route.path, route.name)`
- Replace the watch callback body (L49-60) with the same one-liner
- Remove the duplicated inline matching logic

- [ ] **Step 3: Verify — lint + build + tests**

Run: `cd app/Admin && pnpm run lint && pnpm run build && pnpm run test:unit`
Expected: lint passes, build passes, 59/59 tests pass

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/app/config/route-matcher.ts app/Admin/src/app/layout/MenuItemLayout.vue
git commit -m "refactor: extract isRouteActive from MenuItemLayout to route-matcher util"
```

---

### Task 14: Replace MainLayout manual click-outside with directive

**Files:**
- Modify: `app/Admin/src/app/layout/MainLayout.vue`

**Interfaces:**
- Consumes: `v-click-outside` directive from `@/shared/directives` (already registered globally in main.ts)
- Produces: Same layout behavior, ~30 lines removed

- [ ] **Step 1: Rewrite MainLayout.vue**

Replace `app/Admin/src/app/layout/MainLayout.vue`:

```vue
<script setup lang="ts">
import { useLayout } from '@/app/composables/layout.composable'
import { computed } from 'vue'
import { RouterView } from 'vue-router'
import AppTopbar from './TopbarLayout.vue'
import AppSidebar from './SidebarLayout.vue'
import AppFooter from './FooterLayout.vue'
import AppBreadcrumb from './BreadcrumbLayout.vue'

const { layoutConfig, layoutState, hideMobileMenu } = useLayout()

const containerClass = computed(() => ({
  'layout-overlay': layoutConfig.menuMode === 'overlay',
  'layout-static': layoutConfig.menuMode === 'static',
  'layout-static-inactive': layoutState.staticMenuInactive && layoutConfig.menuMode === 'static',
  'layout-overlay-active': layoutState.overlayMenuActive,
  'layout-mobile-active': layoutState.mobileMenuActive,
}))
</script>

<template>
  <div class="layout-wrapper" :class="containerClass">
    <AppTopbar />
    <div v-click-outside="hideMobileMenu">
      <AppSidebar />
    </div>
    <div class="layout-main-container">
      <div class="layout-main">
        <AppBreadcrumb />
        <RouterView v-slot="{ Component, route }">
          <Transition name="layout-main" mode="out-in">
            <component :is="Component" :key="route.path" />
          </Transition>
        </RouterView>
      </div>
      <AppFooter />
    </div>
    <div class="layout-mask" @click="hideMobileMenu" />
  </div>
</template>
```

Removed: `ref`, `watch`, `onUnmounted` imports, `outsideClickListener`, the watch on `mobileMenuActive`, `bindOutsideClickListener`, `unbindOutsideClickListener`, `isOutsideClicked` function.

- [ ] **Step 2: Verify — lint + build + tests**

Run: `cd app/Admin && pnpm run lint && pnpm run build && pnpm run test:unit`
Expected: lint passes, build passes, 59/59 tests pass

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/app/layout/MainLayout.vue
git commit -m "refactor: replace manual outside-click listener with v-click-outside directive"
```

---

### Task 15: Rename Error interface to ApiProblemDetail

**Files:**
- Modify: `app/Admin/src/shared/models/result.ts`
- Modify: all files importing `Error` from `@/shared/models` (~15 files)

**Interfaces:**
- Consumes: (none)
- Produces: `ApiProblemDetail` interface (same shape, new name). All imports updated.

- [ ] **Step 1: Rename in result.ts**

```ts
// result.ts — before:
export interface Error {
  code: string
  ...
}
// after:
export interface ApiProblemDetail {
  code: string
  message: string
  type: number
  metadata: Record<string, unknown> | null
}
```

- [ ] **Step 2: Update models/index.ts barrel**

Change the re-export:
```ts
export type { ApiProblemDetail } from './result'
// remove old `Error` export
```

- [ ] **Step 3: Update models/api.ts**

```ts
import type { ApiProblemDetail } from './result'

export interface ApiError {
  statusCode: number
  message: string
  errors: ApiProblemDetail[]
}
```

- [ ] **Step 4: Update models/result.ts — Result and PagedResult interfaces**

Change `errors: Error[]` to `errors: ApiProblemDetail[]` in both `Result<T>` and `PagedResult<T>`.

- [ ] **Step 5: Find and update all remaining imports**

Run: `cd app/Admin && rg "import.*Error.*from.*@/shared/models" src/ --no-heading -l`
Update each file: change `Error` to `ApiProblemDetail` in imports and usage.

Files to check (non-exhaustive list):
- `shared/api/handlers/error-handler.ts`
- `shared/api/handlers/refresh-handler.ts`
- `shared/api/interceptors/error-wrapper.interceptor.ts`
- `shared/api/utils/result.mapper.ts`
- `shared/api/__tests__/*.spec.ts`
- `shared/composables/useApi.ts`
- `shared/composables/useApiErrorHandler.ts`
- `shared/errors/ApiError.ts`

**Important:** In `useApi.ts`, the catch block uses `e instanceof Error` (native JS Error) — do NOT change this. It's checking the native `Error`, not the model interface.

- [ ] **Step 6: Verify — lint + build + tests**

Run: `cd app/Admin && pnpm run lint && pnpm run build && pnpm run test:unit`
Expected: lint passes, build passes, 59/59 tests pass

- [ ] **Step 7: Commit**

```bash
git add app/Admin/src/
git commit -m "refactor: rename Error interface to ApiProblemDetail to avoid shadowing native Error"
```

---

### Task 16: Convert session.ts to Pinia store

**Files:**
- Create: `app/Admin/src/stores/useSessionStore.ts`
- Modify: `app/Admin/src/shared/auth/session.ts` (replace with re-export)
- Modify: any files importing from `@/shared/auth/session`

**Interfaces:**
- Consumes: `defineStore` from pinia
- Produces: `useSessionStore()` — Pinia setup store with same `{ user, isAuthenticated, isLoading, setUser, clear }`

- [ ] **Step 1: Create useSessionStore.ts**

Create `app/Admin/src/stores/useSessionStore.ts`:

```ts
import { ref, computed } from 'vue'
import { defineStore } from 'pinia'

interface CurrentUser {
  id: string
  email: string
  name: string
  role: string
  permissions: string[]
}

export const useSessionStore = defineStore('session', () => {
  const user = ref<CurrentUser | null>(null)
  const isLoading = ref(true)

  const isAuthenticated = computed(() => user.value !== null)

  function setUser(newUser: CurrentUser) {
    user.value = newUser
    isLoading.value = false
  }

  function clear() {
    user.value = null
    isLoading.value = false
  }

  return { user, isAuthenticated, isLoading, setUser, clear }
})
```

- [ ] **Step 2: Update session.ts to re-export from store**

Replace `shared/auth/session.ts` with:
```ts
export { useSessionStore } from '@/stores/useSessionStore'
```

This preserves backwards compatibility for any existing imports.

- [ ] **Step 3: Verify — lint + build + tests**

Run: `cd app/Admin && pnpm run lint && pnpm run build && pnpm run test:unit`
Expected: lint passes, build passes, 59/59 tests pass

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/stores/useSessionStore.ts app/Admin/src/shared/auth/session.ts
git commit -m "refactor: convert session state to Pinia store"
```

---

### Task 17: Add createDefaultQueryingModel factory

**Files:**
- Create: (add function to existing file) `app/Admin/src/shared/models/querying.ts`
- Modify: `app/Admin/src/shared/composables/usePagedList.ts`

**Interfaces:**
- Produces: `createDefaultQueryingModel(pageSize?: number): QueryingModel`

- [ ] **Step 1: Add factory function to querying.ts**

Append to `app/Admin/src/shared/models/querying.ts`:

```ts
export function createDefaultQueryingModel(pageSize = 10): QueryingModel {
  return {
    filter: { conditions: [], allowedFields: [], violations: [] },
    search: {
      term: { value: '', caseSensitive: false },
      fields: [],
      mode: 'Any',
      allowedFields: [],
      violations: [],
    },
    sort: { clauses: [], allowedFields: [], violations: [] },
    page: {
      page: 1,
      pageSize,
      isEmpty: false,
      bounds: { defaultPage: 1, defaultPageSize: pageSize, maxPageSize: 100 },
      violations: [],
    },
  }
}
```

- [ ] **Step 2: Update models/index.ts barrel**

Add the export:
```ts
export { createDefaultQueryingModel } from './querying'
```

- [ ] **Step 3: Update usePagedList.ts to use the factory**

In `usePagedList.ts`, replace the inline default `QueryingModel` (L14-16, the 40-line object) with:

```ts
import { createDefaultQueryingModel } from '@/shared/models'

// In the function body, replace:
const params = ref<QueryingModel>(createDefaultQueryingModel())
```

- [ ] **Step 4: Verify — lint + build + tests**

Run: `cd app/Admin && pnpm run lint && pnpm run build && pnpm run test:unit`
Expected: lint passes, build passes, 59/59 tests pass

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/shared/models/querying.ts app/Admin/src/shared/models/index.ts app/Admin/src/shared/composables/usePagedList.ts
git commit -m "refactor: add createDefaultQueryingModel factory to reduce boilerplate"
```

---

### Task 18: Add router auth guard

**Files:**
- Create: `app/Admin/src/router/guards.ts`
- Modify: `app/Admin/src/router/index.ts`

**Interfaces:**
- Consumes: `TokenService.hasValidAccessToken()` from `@/shared/auth/token.service`
- Produces: `requireAuth` navigation guard

- [ ] **Step 1: Create guards.ts**

Create `app/Admin/src/router/guards.ts`:

```ts
import type { Router } from 'vue-router'
import { TokenService } from '@/shared/auth/token.service'

export function registerAuthGuard(router: Router) {
  router.beforeEach((to, _from, next) => {
    const isAuthenticated = TokenService.hasValidAccessToken()

    if (!isAuthenticated && to.name !== 'login') {
      // Login route not yet implemented — guard is scaffolding.
      // When login route exists, uncomment:
      // next({ name: 'login', query: { redirect: to.fullPath } })
      // return
    }

    next()
  })
}
```

- [ ] **Step 2: Register guard in router/index.ts**

Add after `const router = createRouter(...)`:

```ts
import { registerAuthGuard } from './guards'

const router = createRouter({ ... })
registerAuthGuard(router)
```

- [ ] **Step 3: Verify — lint + build + tests**

Run: `cd app/Admin && pnpm run lint && pnpm run build && pnpm run test:unit`
Expected: lint passes, build passes, 59/59 tests pass

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/router/guards.ts app/Admin/src/router/index.ts
git commit -m "feat: add router auth guard scaffolding"
```

---

### Task 19: Fix import ordering in useDarkMode

**Files:**
- Modify: `app/Admin/src/shared/composables/useDarkMode.ts`

**Interfaces:**
- Consumes: (none)
- Produces: Same API, corrected import ordering

- [ ] **Step 1: Move const after imports**

Replace `app/Admin/src/shared/composables/useDarkMode.ts`:

```ts
import { ref, watchEffect } from 'vue'

const DARK_MODE_CLASS = 'app-dark'

export function useDarkMode() {
  const stored = localStorage.getItem('resys-admin-dark-mode')
  const isDark = ref(stored === 'true')

  watchEffect(() => {
    localStorage.setItem('resys-admin-dark-mode', String(isDark.value))
    document.documentElement.classList.toggle(DARK_MODE_CLASS, isDark.value)
  })

  function toggle() { isDark.value = !isDark.value }
  function enable() { isDark.value = true }
  function disable() { isDark.value = false }

  return { isDark, toggle, enable, disable }
}
```

- [ ] **Step 2: Verify — lint + build + tests**

Run: `cd app/Admin && pnpm run lint && pnpm run build && pnpm run test:unit`
Expected: lint passes, build passes, 59/59 tests pass

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/shared/composables/useDarkMode.ts
git commit -m "style: fix import ordering in useDarkMode"
```

---

### Task 20: Final directory cleanup and move modal.service

**Files:**
- Create: `app/Admin/src/shared/composables/useModal.ts`
- Modify: `app/Admin/src/shared/services/index.ts` (remove modal export)
- Delete: `app/Admin/src/shared/services/modal.service.ts`
- Delete: empty `app/Admin/src/shared/services/` directory

**Interfaces:**
- Consumes: (same modal.service logic, new location)
- Produces: `useModalService()` from composables barrel

- [ ] **Step 1: Create useModal.ts**

Copy `app/Admin/src/shared/services/modal.service.ts` to `app/Admin/src/shared/composables/useModal.ts`. Update the import path at the top:
```ts
import { ref } from 'vue'
```
(same content as modal.service.ts)

- [ ] **Step 2: Add to composables barrel**

In `app/Admin/src/shared/composables/index.ts`, add:
```ts
export { useModalService } from './useModal'
```

- [ ] **Step 3: Update services/index.ts**

Since `modal.service.ts` is the only remaining export, delete `services/index.ts` or replace with a comment noting the directory is now empty.

Actually — just delete `services/index.ts` and the `services/` directory entirely.

- [ ] **Step 4: Delete old files**

```bash
cd app/Admin
rm src/shared/services/modal.service.ts
rm src/shared/services/index.ts 2>/dev/null || true
rmdir src/shared/services 2>/dev/null || true
```

- [ ] **Step 5: Verify — lint + build + tests**

Run: `cd app/Admin && pnpm run lint && pnpm run build && pnpm run test:unit`
Expected: lint passes, build passes, 59/59 tests pass

- [ ] **Step 6: Commit**

```bash
git add app/Admin/src/shared/composables/useModal.ts app/Admin/src/shared/composables/index.ts app/Admin/src/shared/
git commit -m "refactor: move modal.service to composables, remove empty services directory"
```

---

### Task 21: Final verification — full gate

**Files:**
- (none — verification only)

**Interfaces:**
- Produces: Confirmed all gates pass

- [ ] **Step 1: Run full verification**

```bash
cd app/Admin
pnpm run lint
pnpm run build
pnpm run test:unit
```

Expected: all three pass with zero errors/warnings. 59/59 tests.

- [ ] **Step 2: Check final directory structure**

```bash
cd app/Admin && find src/ -type d | sort
```

Expected: no `services/`, no `styles/` under shared, no `navigation/` under components, no `counter.ts` in stores.

- [ ] **Step 3: Commit final state if any remaining changes**

```bash
git add app/Admin/
git commit -m "chore: final verification — all architecture cleanup gates pass"
```
