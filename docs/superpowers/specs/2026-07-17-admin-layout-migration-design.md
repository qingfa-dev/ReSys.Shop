# Admin Layout Shell Migration Design

**Date:** 2026-07-17
**Status:** Design approved, awaiting implementation plan
**Scope:** Port legacy Sakai layout shell from `app/lagacy/Admin` to `app/Admin`

## Goal

Replace the broken, simplified layout shell in `app/Admin` with a ported, full-featured Sakai admin shell derived from `app/lagacy/Admin`. The feature pages (catalog, inventory, ordering, users, etc.) already exist in the new Admin with PascalCase conventions and are untouched.

End state: a working shell where every visible feature (sidebar menu, dark mode toggle, theme configurator, menu mode switching, search bar, breadcrumb, mobile support, confirmation dialogs) functions correctly, or has been intentionally removed with dead CSS cleaned up.

## Constraints

- Feature pages must not be modified (zero regression risk for domain code)
- SCSS files are carried forward as-is during port; cleanup postponed to debug phase
- Legacy has unknown bugs in layout features; diagnose after port renders
- Sub-framework: PascalCase naming of new Admin must be followed

## Non-Goals

- Rewriting feature pages or domain code
- Rewriting the SCSS layout files
- Adding new layout features not present in legacy

---

## Architecture

### Files to replace (11 files + AppProviders)

Delete all existing layout files in `app/Admin/src/app/layout/` and port the legacy equivalents with PascalCase naming:

| Legacy file | New file | Notes |
|---|---|---|
| `main.layout.vue` | `Main.Layout.vue` | Add `isDarkTheme` binding, restore `<layout-main-container>`, `<AppBreadcrumb />`, `<FloatingConfigurator />`, `<div class="layout-mask">`, `<ConfirmDialog />` |
| `topbar.layout.vue` | `Topbar.Layout.vue` | Restore full legacy DOM: SVG logo, `<GlobalSearch />`, dark toggle, configurator button, user dropdown |
| `sidebar.layout.vue` | `Sidebar.Layout.vue` | Thin wrapper: `<div class="layout-sidebar"><AppMenu /></div>` |
| `menu.layout.vue` | `Menu.Layout.vue` | Reactive menu model, `to`-based named routes, hierarchical groups |
| `menu-item.layout.vue` | `MenuItem.Layout.vue` | Route matching, separator/disabled/visible/command support, `<Transition>` animations |
| `footer.layout.vue` | `Footer.Layout.vue` | "— All rights reserved" text |
| `configurator.layout.vue` | `Configurator.Layout.vue` | Full preset/color/mode configurator sidebar |
| `FloatingConfigurator.vue` | `FloatingConfigurator.Component.vue` | Both floating buttons (dark + palette), `v-styleclass` |
| `GlobalSearch.vue` | `GlobalSearch.Component.vue` | FuncSearch with OverlayPanel, keyboard handling |
| `layout.composable.ts` | `composables/layout.composable.ts` | Rewritten internals, preserved API surface |
| `providers/AppProviders.vue` | `providers/AppProviders.Component.vue` | Restore `<ConfirmDialog />` + `<Toast />` |

### Dependency graph

```
Main.Layout
 ├── Topbar.Layout ──── GlobalSearch.Component
 ├── Sidebar.Layout ─── Menu.Layout ─── MenuItem.Layout (recursive)
 ├── Footer.Layout
 ├── FloatingConfigurator.Component ─── Configurator.Layout
 ├── AppBreadcrumb (from shared/components/breadcrumb.component.vue)
 └── composables/layout.composable.ts  ← shared by all
```

### What does NOT change

Feature pages (catalog, inventory, ordering, users, etc.) import nothing from `app/layout/`. They render inside `<router-view />` within `Main.Layout.vue`. Zero feature file modifications.

---

## Composable Rewrite

Preserves the exact exported API surface but replaces two `reactive()` mega-objects with individual `ref()` values composed into reactive views.

### Internals (individual refs)

```
ref<string>   darkTheme, preset, primary, surface, menuMode
ref<boolean>  staticMenuInactive, overlayMenuActive, mobileMenuActive,
              configSidebarVisible, profileSidebarVisible, sidebarExpanded,
              menuHoverActive, anchored
ref<string|null> activeMenuItem, activePath
```

### Exported API (same shape as legacy)

```
layoutConfig   → { preset, primary, surface, darkTheme, menuMode }   (get/set proxies)
layoutState    → { staticMenuInactive, overlayMenuActive, mobileMenuActive, ... }
isDarkTheme    → computed<boolean>
toggleDarkMode(), toggleMenu(), toggleConfigSidebar(), hideMobileMenu(),
changeMenuMode(mode), isDesktop(), hasOpenOverlay
```

### Changes from legacy composable

- Inline `executeDarkModeToggle` into `toggleDarkMode`
- Keep `document.startViewTransition` for animated dark toggle
- Module-level refs for singleton state
- Function names preserved verbatim

---

## Brand Refresh (during port)

1. **Logo**: Replace inline RESYS SVG in `Topbar.Layout.vue` with "ReSys.Shop" text
2. **Accent color**: No change. `primary: 'emerald'` default in composable; user changes via configurator at runtime
3. **Cleanup**: Delete `app/Admin/src/assets/sekai/` — unused duplicate of `scss/` (12 files, no build impact)
4. No other visual changes — same typography, spacing, layout skeleton

---

## Verification

### Phase 1 gate (port complete)

- `pnpm run lint` passes with zero warnings
- `pnpm run test:unit` passes (8 existing tests; layout has no tests)
- Manual smoke test:
  - [ ] Page loads without white screen or console errors
  - [ ] Sidebar renders with full menu tree
  - [ ] Clicking menu items navigates to correct pages
  - [ ] Dark/light mode toggle works (icon swaps)
  - [ ] Configurator panel opens/closes via floating button
  - [ ] Mobile viewport: hamburger opens sidebar, mask click closes it
  - [ ] Breadcrumb shows active page path
  - [ ] Search input renders and opens OverlayPanel
  - [ ] Footer visible on every page

### Phase 2 gate (debug complete)

- [ ] Each smoke test item confirmed working
- [ ] Broken features diagnosed and fixed or intentionally removed
- [ ] Configurator presets change theme correctly
- [ ] Menu mode switching works (static/overlay/reveal/drawer)
- [ ] Responsive breakpoint works at 992px
- [ ] Keyboard navigation in search works
- [ ] ConfirmDialog works in at least one feature (e.g., delete product)
- [ ] Dead CSS cleaned up

---

## Debug Phase Process

1. Smoke test every layout feature; document broken items
2. Triage each: CSS bug / JS bug / missing feature
3. Fix CSS bugs first (class mismatches, missing rules, z-index)
4. Fix JS bugs second (composable logic, event wiring, reactive state)
5. Remove genuinely broken features if unfixable in reasonable time; clean up associated SCSS
6. SCSS cleanup: delete `sekai/`, remove unused rules
