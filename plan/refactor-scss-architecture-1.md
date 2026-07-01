---
goal: Refactor and Optimize Sakai-Vue-Derived SCSS Architecture for PrimeVue 5 + Tailwind v4
version: 1.0
date_created: 2026-07-01
status: 'Planned'
tags: refactor, scss, architecture, theme, cleanup
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Refactor the current SCSS architecture (ported from PrimeFaces Sakai Vue template) to eliminate redundancy with Tailwind v4 and PrimeVue 5's design-token system, fix dark-mode selector mismatch, remove unused demo/preloader code, and consolidate layout into a maintainable structure. The goal is a lean, predictable SCSS layer that only covers what Tailwind and PrimeVue tokens cannot.

## 1. Requirements & Constraints

- **REQ-001**: All scoped SCSS variables must map to PrimeVue 5 `--p-*` design tokens (source of truth for theming).
- **REQ-002**: Dark mode selector must match PrimeVue 5 config: `.p-dark` (currently uses `[class*='app-dark']`).
- **REQ-003**: No duplicate layout primitives already provided by Tailwind utility classes (flex, grid, spacing, sizing).
- **REQ-004**: Remove Sakai-specific demo code (`demo/`, `flags/`, `code.scss`) unless the app actually needs demo pages.
- **REQ-005**: Keep `primeicons/primeicons.css` import as the only vendor dependency.
- **REQ-006**: All focus-ring styles must use Tailwind's `focus-visible:` utilities in components, not SCSS mixins.
- **REQ-007**: Responsive breakpoints must align with Tailwind's default breakpoints (`sm: 640px`, `md: 768px`, `lg: 1024px`, `xl: 1280px`, `2xl: 1536px`).
- **CON-001**: Must remain compatible with PrimeVue 5's runtime theming (`@primeuix/themes/aura` preset).
- **CON-002**: Must preserve PrimeIcons font loading (currently via `@use 'primeicons/primeicons.css'`).
- **CON-003**: The `@tailwindcss/vite` plugin must remain the sole Tailwind processing pipeline.
- **GUD-001**: Prefer CSS custom properties over SCSS variables for any value derived from PrimeVue tokens.
- **GUD-002**: Extract only structural layout SCSS (topbar, sidebar, footer) — utility/component visual styles belong in Tailwind.
- **PAT-001**: Follow Vite-native SCSS partial naming (`_partial.scss`) with `@use` in `main.scss`.

## 2. Implementation Steps

### Implementation Phase 1: Fix Dark Mode & Variable Redundancy

- GOAL-001: Fix dark-mode selector mismatch and eliminate redundant CSS variable aliases that duplicate `--p-*` tokens.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Update `_dark.scss`: change `:root[class*='app-dark']` to `:root.p-dark` to match PrimeVue 5 config (`darkModeSelector: '.p-dark'`) | | |
| TASK-002 | Inline `_light.scss` and `_dark.scss` values directly into `_common.scss` under `:root` and `:root.p-dark` respectively, removing the separate files | | |
| TASK-003 | Audit every `--surface-*`, `--text-*`, `--focus-*` variable in `_common.scss`: if the alias is an exact 1:1 passthrough of a `--p-*` token (e.g. `--primary-color: var(--p-primary-color)`), replace all usages in SCSS with the native `--p-*` token and delete the alias | | |
| TASK-004 | Remove `_light.scss` and `_dark.scss` files after inlining | | |
| TASK-005 | Update `main.scss` to remove `@use 'abstracts/variables/light'` and `@use 'abstracts/variables/dark'` | | |

### Implementation Phase 2: Remove Sakai-Specific Demo & Preloader

- GOAL-002: Strip unused Sakai Vue demo boilerplate that has no value in the target application.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | Delete `src/assets/scss/demo/` directory entirely (includes `demo.scss`, `code.scss`, and `flags/`) | | |
| TASK-007 | Remove `@use 'demo/demo'` from `main.scss` | | |
| TASK-008 | Delete `src/assets/scss/layout/_preloading.scss` — preloader is an application concern, not a global stylesheet concern | | |
| TASK-009 | Remove `@use 'layout/preloading'` from `main.scss` | | |
| TASK-010 | Remove unused `<mark>`, `<blockquote>`, `<hr>` styles from `_typography.scss` unless the application uses them | | |

### Implementation Phase 3: Consolidate Layout SCSS with Tailwind Primitives

- GOAL-003: Replace manual CSS layout rules with Tailwind utility equivalents where possible, keeping SCSS only for PrimeVue-specific overrides.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-011 | Replace `html { font-size: 14px; }` in `_core.scss` with Tailwind `text-sm` on `<html>` or remove (Tailwind's default is 16px; if 14px is intentional, keep as one line) | | |
| TASK-012 | Replace `body { margin: 0; padding: 0; -webkit-font-smoothing: antialiased; }` in `_core.scss` with Tailwind `antialiased` class on `<body>` in `index.html` | | |
| TASK-013 | Remove `.layout-wrapper { min-height: 100vh; }` — replace with Tailwind `min-h-screen` on the wrapper element in the Vue template | | |
| TASK-014 | Audit `_typography.scss`: remove h1-h6 margin/size/weight rules if Tailwind's `prose` or utility classes (`text-3xl`, `font-bold`) will be used instead. Keep only if the app relies on bare HTML headings without utility classes | | |
| TASK-015 | Replace `_responsive.scss` breakpoint values to align with Tailwind defaults: `max-width: 991px` → `max-width: 1023px` (Tailwind `lg`), `min-width: 992px` → `min-width: 1024px`, `min-width: 1960px` → `min-width: 1536px` (Tailwind `2xl`) | | |
| TASK-016 | Consolidate sidebar overlay/static/mobile modes in `_responsive.scss` into a single pattern using Tailwind `translate-x-*` and `hidden` classes where the sidebar visibility is toggled by JS state (not CSS classes). Keep SCSS for the transitions only | | |
| TASK-017 | Move `.card` utility from `_utils.scss` to Tailwind — use `bg-[--surface-card] p-8 mb-8 rounded-[--content-border-radius]` as a reusable component or directive, or keep as a SCSS extend-only class (`%card`) | | |
| TASK-018 | Replace `.p-toast` position offset in `_utils.scss` with a PrimeVue toast configuration option in `main.ts` | | |

### Implementation Phase 4: Refactor Mixins & Focus Rings

- GOAL-004: Eliminate SCSS focus-ring mixins in favor of Tailwind's built-in `focus-visible:` ring utilities.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-019 | Delete `_mixins.scss` — both `focused()` and `focused-inset()` mixins | | |
| TASK-020 | Remove `@use '../abstracts/mixins' as *` from `_topbar.scss`, `_menu.scss` | | |
| TASK-021 | Replace `@include focused()` in `_topbar.scss` (logo link, action buttons) with Tailwind `focus-visible:outline-2 focus-visible:outline-[var(--p-focus-ring-color)] focus-visible:outline-offset-2 focus-visible:shadow-[var(--p-focus-ring-shadow)]` applied in the Vue component template | | |
| TASK-022 | Replace `@include focused-inset()` in `_menu.scss` (nav links) with inline Tailwind equivalents or a custom Tailwind plugin | | |
| TASK-023 | Update `main.scss` to remove `@use 'abstracts/mixins' as *` | | |

### Implementation Phase 5: Final Cleanup & Verification

- GOAL-005: Verify SCSS file structure, build, and test that all theme/layout features work correctly.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-024 | Audit `main.scss` import order — ensure `@use 'primeicons/primeicons.css'` comes before any layers that reference PrimeIcons font classes | | |
| TASK-025 | Run `npx vite build` and confirm zero SCSS compilation errors | | |
| TASK-026 | Run `npx vue-tsc --build` and confirm zero type-check errors | | |
| TASK-027 | Verify topbar renders at fixed position with correct z-index and responsive dropdown behavior | | |
| TASK-028 | Verify sidebar navigation renders with correct active-route highlighting and submenu transitions | | |
| TASK-029 | Verify dark mode toggling via `.p-dark` class applies `--surface-ground: var(--p-surface-950)` correctly | | |
| TASK-030 | Verify PrimeIcons render via `<i class="pi pi-check"></i>` in any Vue component | | |
| TASK-031 | Verify Tailwind utility classes still apply alongside SCSS (`text-3xl`, `p-4`, etc.) | | |
| TASK-032 | Remove `clearfix` utility from `_utils.scss` if not used anywhere (Tailwind does not have a clearfix utility, so keep if the app uses float-based clearing) | | |

## 3. Alternatives

- **ALT-001**: Convert everything to Tailwind and delete all SCSS. Rejected because PrimeVue theme overrides require CSS custom property values that are cumbersome to maintain purely in Tailwind config.
- **ALT-002**: Keep the full Sakai Vue 7-1 pattern as-is. Rejected because it introduces substantial dead code (demo, flags, preloader) and duplicates theming that PrimeVue 5's `--p-*` tokens provide natively.
- **ALT-003**: Use CSS Modules instead of SCSS. Rejected because PrimeVue's design-token system uses global CSS variables, making scoped stylesheets counterproductive for layout-level theming.
- **ALT-004**: Use a Tailwind plugin for PrimeVue theme variables. Rejected because `@primeuix/themes` already provides the design-token pipeline at runtime — a Tailwind plugin would duplicate that work.

## 4. Dependencies

- **DEP-001**: PrimeVue 5 (`primevue@5.0.0-rc.1`) with `@primeuix/themes` Aura preset — must remain installed.
- **DEP-002**: Tailwind CSS v4 (`tailwindcss`, `@tailwindcss/vite`) — must remain installed and configured.
- **DEP-003**: Sass (`sass@^1.101.0`) — required to compile `.scss` files; not removable.
- **DEP-004**: PrimeIcons (`primeicons@^7.0.0`) — font files must remain in `node_modules`.
- **DEP-005**: `unplugin-vue-components` + `@primevue/auto-import-resolver` — auto-imports PrimeVue components; already configured in `vite.config.ts`.

## 5. Files

- **FILE-001**: `src/assets/scss/main.scss` — entry point, import list to be trimmed.
- **FILE-002**: `src/assets/scss/abstracts/_mixins.scss` — to be deleted (focus mixins migrated to Tailwind).
- **FILE-003**: `src/assets/scss/abstracts/variables/_common.scss` — to be simplified (remove redundant aliases).
- **FILE-004**: `src/assets/scss/abstracts/variables/_light.scss` — to be inlined into `_common.scss` and deleted.
- **FILE-005**: `src/assets/scss/abstracts/variables/_dark.scss` — to be inlined into `_common.scss` and deleted.
- **FILE-006**: `src/assets/scss/base/_core.scss` — to be trimmed (migrate body/html rules to Tailwind).
- **FILE-007**: `src/assets/scss/base/_typography.scss` — to be trimmed (keep only if app uses bare HTML headings).
- **FILE-008**: `src/assets/scss/layout/_topbar.scss` — to be updated (remove `@use mixins`, move focus styles to templates).
- **FILE-009**: `src/assets/scss/layout/_menu.scss` — to be updated (remove `@use mixins`, move focus styles to templates).
- **FILE-010**: `src/assets/scss/layout/_responsive.scss` — breakpoints realigned to Tailwind defaults.
- **FILE-011**: `src/assets/scss/layout/_utils.scss` — `.card` and `.p-toast` to be replaced.
- **FILE-012**: `src/assets/scss/layout/_preloading.scss` — to be deleted.
- **FILE-013**: `src/assets/scss/demo/` (3 files + flags) — entire directory to be deleted.
- **FILE-014**: `index.html` — add `antialiased` class to `<body>` if body SCSS rules are removed.
- **FILE-015**: `src/main.ts` — move `.p-toast` position config to PrimeVue options if applicable.

## 6. Testing

- **TEST-001**: `npx vite build` exits with code 0 — the SCSS compiler produces valid CSS.
- **TEST-002**: `npx vue-tsc --build` exits with code 0 — no type errors from template changes.
- **TEST-003**: Visual check that topbar renders at the top of the viewport with correct height (4rem) and z-index layering.
- **TEST-004**: Visual check that the sidebar opens/closes with proper transitions in desktop static mode, overlay mode, and mobile mode.
- **TEST-005**: Visual check that dark mode toggle (applying `.p-dark` to `:root`) changes `--surface-ground` from `var(--p-surface-100)` to `var(--p-surface-950)`.
- **TEST-006**: Visual check that PrimeIcons (`<i class="pi pi-check">`) renders correctly.
- **TEST-007**: Visual check that Tailwind classes like `text-3xl font-bold text-blue-600` render alongside SCSS styles without conflict.

## 7. Risks & Assumptions

- **RISK-001**: Removing `_preloading.scss` will break the preloader if any component references the `.preloader` CSS class. Mitigation: grep for `.preloader` usage before deleting.
- **RISK-002**: Removing font-size override on `<html>` (14px) could cause layout shifts in components sized in `rem`. Mitigation: keep the 14px override as a single-line rule in `_core.scss` and explicitly note the deviation from Tailwind's 16px default.
- **RISK-003**: Realigning responsive breakpoints from PrimeVue's 991px/992px to Tailwind's 1023px/1024px could cause sidebar layout glitches at 992-1023px viewport widths. Mitigation: test at 1000px width and adjust if needed.
- **RISK-004**: Moving `.p-toast` positioning from SCSS to PrimeVue config may be impossible if PrimeVue 5 rc.1 does not expose a toast position-offset option. Mitigation: keep the SCSS override if API option is unavailable.
- **ASSUMPTION-001**: The app does not need Sakai's demo pages (code blocks, flag icons). If demo content is added later, styles can be re-imported per-component.
- **ASSUMPTION-002**: All Vue components use Tailwind utility classes for basic styling, not bare HTML elements that depend on `_typography.scss` defaults.

## 8. Related Specifications / Further Reading

- https://github.com/primefaces/sakai-vue — original Sakai Vue template (source of the ported SCSS).
- https://primevue.org/theming/styled/ — PrimeVue 5 design-token documentation.
- https://tailwindcss.com/docs/functions-and-directives — Tailwind v4 CSS-first configuration.
- `app/Admin/src/main.ts` — PrimeVue 5 + Aura theme configuration.
- `app/Admin/vite.config.ts` — Tailwind v4 + PrimeVue auto-import resolver config.
