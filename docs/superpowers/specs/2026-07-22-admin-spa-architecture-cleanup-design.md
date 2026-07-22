# Admin SPA Architecture Cleanup

**Date:** 2026-07-22
**Status:** Approved
**Branch:** feature/implement-admin-panel

## Scope

Fix all ~25 findings from the 2026-07-22 architecture review of `app/Admin/`. The review found 3 critical bugs, 8 high-severity items, 5 medium items, and several low items across the shared infrastructure, layout, and configuration layers. All 32 feature pages remain placeholder stubs — this cleanup establishes a clean baseline before any feature work begins.

## Non-Goals

- No new features or page implementations
- No changes to PrimeVue preset or design tokens
- No test framework changes

---

## Phases

### Phase 1 — Critical Fixes (3 items)

Fix runtime bugs that would cause failures or infinite loops.

| # | Fix | File | Change |
|---|-----|------|--------|
| 1.1 | API URL doubling | `shared/auth/auth.service.ts:29,41,48` | Drop `/api` prefix from all 3 paths. `client.ts` already sets `baseURL: '/api'`, so `/api/store/identity/auth/sessions/login` becomes `/store/identity/auth/sessions/login`. Same for `/logout` and `/me`. |
| 1.2 | Dark mode watch loop | `app/composables/layout.composable.ts:105-108` | Remove reverse watch that syncs `layoutConfig.darkTheme` back to `isDark.value`. Keep the forward watch (`isDark` → `layoutConfig.darkTheme`). `toggleDarkMode()` calls `useDarkMode().toggle()` which handles DOM + localStorage. `layoutConfig.darkTheme` is persisted separately via its own localStorage watch. |
| 1.3 | Fragile refresh URL match | `shared/api/interceptors/error-wrapper.interceptor.ts:14` | Replace `originalRequest.url?.includes('/sessions/refresh')` with `originalRequest.url?.endsWith('/sessions/refresh')`. Prevents false matches on paths like `/shipping/refresh-rates`. |

**Gate:** `pnpm run lint && pnpm run build && pnpm run test:unit` — 59/59 tests

---

### Phase 2 — Deletions (9 items)

Remove all dead code. YAGNI — everything can be recreated when actually needed.

| # | Delete | Reason |
|---|--------|--------|
| 2.1 | `stores/counter.ts` | Scaffold example, zero imports |
| 2.2 | `shared/components/navigation/` | `Breadcrumb.vue` is byte-identical to `app/layout/BreadcrumbLayout.vue` |
| 2.3 | `shared/utils/debounce.ts` | Superseded by `composables/useDebounce.ts` |
| 2.4 | `shared/composables/useToastNotify.ts` | Sugar wrapper — methods merged into `useToast.ts` in Phase 3 |
| 2.5 | `shared/styles/` (4 .scss files) | All placeholder. `assets/styles/` is canonical |
| 2.6 | `shared/services/event-bus.service.ts` | Zero consumers |
| 2.7 | `shared/services/logger.service.ts` | Zero consumers |
| 2.8 | `shared/services/notification.service.ts` | Zero consumers |
| 2.9 | `chart.js` from `dependencies` | No charts; can re-add later |

**Barrel updates:** Remove dead exports from `utils/index.ts`, `composables/index.ts`, `services/index.ts`.

**Gate:** build + lint + 59/59 tests (no test references any deleted file)

---

### Phase 3 — Merge & Consolidate (4 items)

Eliminate overlapping composables and add missing structure.

| # | Action | Detail |
|---|--------|--------|
| 3.1 | Absorb `useToastNotify` into `useToast.ts` | Add `success(detail, summary?)`, `error(detail, summary?)`, `warn(detail, summary?)`, `info(detail, summary?)` wrapper methods to `useToast.ts`. Drop `useToastNotify` from barrel |
| 3.2 | Deduplicate formatter methods | Remove `formatCurrency()` and `formatDate()` from `useFormatter` (already covered by `useCurrency` and `useDate`). `useFormatter` retains only `formatNumber()` and `truncate()`. Update `formatter.spec.ts` |
| 3.3 | `useResponsive` consumes `useWindowSize` | Replace standalone `addEventListener('resize')` in `useResponsive` with `const { width } = useWindowSize()`. Single resize listener system-wide |
| 3.4 | Barrel export for `shared/components/` | Create `shared/components/index.ts` with named re-exports for all 22 components |

**Gate:** build + lint + 59/59 tests. `formatter.spec.ts` updated.

---

### Phase 4 — Extract & Simplify (4 items)

Reduce file sizes and improve code health without changing behavior.

| # | Action | Detail |
|---|--------|--------|
| 4.1 | Split `layout.composable.ts` | **`useLayoutConfig.ts`**: config interface, load/save, localStorage watch, `changeMenuMode()`. **`useLayoutState.ts`**: reactive state object, `hideMobileMenu()`. **`useLayout.ts`**: orchestrator — imports both, adds `toggleMenu()`, `toggleConfigSidebar()`, `toggleDarkMode()`, `isDesktop()`, `hasOpenOverlay`. Same public API, zero consumer changes |
| 4.2 | Extract `isRouteActive()` util | Move duplicated route-matching logic (L24-47 computed + L49-60 watch in MenuItemLayout) into `app/config/route-matcher.ts` as a pure function. `MenuItemLayout.vue` drops ~40 lines |
| 4.3 | Directive for click-outside in MainLayout | Replace L20-55 manual `document.addEventListener('click')` logic with `v-click-outside="hideMobileMenu"` on sidebar div. ~30 lines removed |
| 4.4 | Rename `Error` → `ApiProblemDetail` | `shared/models/result.ts:1`: `export interface Error` renamed to `ApiProblemDetail`. Update all ~15 files that import/use it. Native `instanceof Error` in `useApi.ts` is unaffected (type-only import) |

**Gate:** build + lint + 59/59 tests. Manual smoke: mobile menu outside-click, menu active state highlighting.

---

### Phase 5 — Structural (5 items)

Long-term improvements for maintainability and security.

| # | Action | Detail |
|---|--------|--------|
| 5.1 | `session.ts` → Pinia store | Replace module-level `reactive()` singleton with `stores/useSessionStore.ts` (setup store). Same API shape. Update consumers to use `const session = useSessionStore()`. Gains devtools integration |
| 5.2 | `createDefaultQueryingModel()` factory | New function in `shared/models/querying.ts`. Replaces the 40-line inline default in `usePagedList.ts` with `params.value = createDefaultQueryingModel()` |
| 5.3 | Router auth guard | New `router/guards.ts` with `beforeEach` checking `TokenService.hasValidAccessToken()`. Redirect to `/login` when unauthenticated. Register in `router/index.ts` |
| 5.4 | Fix import ordering | `useDarkMode.ts`: move `const DARK_MODE_CLASS` below `import` statement |
| 5.5 | Final directory cleanup | Move `modal.service.ts` → `composables/useModal.ts`. Remove now-empty `services/` directory. Remove now-empty `components/navigation/` directory |

**Gate:** build + lint + tests. Router guard: verify redirect works with unauthenticated state.

---

## Final Folder Structure

```
src/
  app/
    composables/
      useLayout.ts
      useLayoutConfig.ts          (new — split from layout.composable)
      useLayoutState.ts           (new — split from layout.composable)
    config/
      admin-menu.config.ts
      route-matcher.ts            (new — extracted from MenuItemLayout)
    layout/                       (unchanged except MainLayout)
    plugins/
    routes/
  assets/                         (single source for all styles)
  features/                       (unchanged — 32 placeholder pages)
  router/
    guards.ts                     (new — auth guard)
    index.ts
  shared/
    api/
    auth/
      auth.service.ts             (fixed URL paths)
      session.ts                  (deleted — replaced by Pinia store)
      ...
    components/
      index.ts                    (new barrel)
      data/
      feedback/
      forms/
      layout/
      overlays/
      # navigation/ deleted
    composables/
      index.ts
      useApi.ts
      useConfirm.ts
      useCurrency.ts
      useDarkMode.ts
      useDate.ts
      useDebounce.ts
      useFilePreview.ts
      useFormatter.ts             (formatCurrency/formatDate removed)
      useModal.ts                 (moved from services/)
      usePagedList.ts             (uses factory fn)
      usePagination.ts
      useResponsive.ts            (consumes useWindowSize)
      useToast.ts                 (absorbed useToastNotify)
      useWindowSize.ts
      # useToastNotify.ts deleted
    constants/
    directives/
    enums/
    errors/
    hooks/
    localization/
    models/
      querying.ts                 (added createDefaultQueryingModel)
      result.ts                   (Error → ApiProblemDetail)
    types/
    utils/
      # debounce.ts deleted
    validation/
    # services/ deleted
  stores/
    useSessionStore.ts            (new — from session.ts)
    # counter.ts deleted
```

---

## Verification Summary

Each phase independently passes: `pnpm run lint && pnpm run build && pnpm run test:unit`

| Phase | Files changed | Tests |
|-------|--------------|-------|
| 1 | 2 files | 59/59 |
| 2 | ~14 deletions + 3 barrel updates | 59/59 |
| 3 | 6 edits | 59/59 (formatter test updated) |
| 4 | 7 edits | 59/59 |
| 5 | 5 edits + 2 new files | 59/59 + guard test |

## Risks

- **Phase 1.2 (dark mode):** Removing the reverse watch may break the "config sidebar toggle syncs with dark mode toggle" path. Mitigated by keeping the forward watch and relying on `useDarkMode.isDark` as the single source of truth for DOM state.
- **Phase 4.4 (ApiProblemDetail rename):** Pure mechanical rename. Risk is missing an import — caught by build.
- **Phase 5.3 (auth guard):** No `/login` route exists yet. The guard is wired as `router.beforeEach` checking `TokenService.hasValidAccessToken()` and redirecting to a `/login` route name. Since no login route is registered, the guard will be a no-op until a login page is created — this is intentional scaffolding.

## Non-Risks

- No feature pages are modified — all 32 remain PlaceholderPage stubs
- No API integration points change (only URL prefix fix)
- No design tokens, preset, or visual styles change (only deletion of unused SCSS)
