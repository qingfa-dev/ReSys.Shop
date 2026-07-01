---
goal: Integrate Upstream sakai-assets: tailwindcss-primeui, Preloader, Demo, Breakpoint Alignment
version: 3.0
date_created: 2026-07-01
status: 'Completed'
tags: refactor, integration, sakai-assets, tailwind, primevue
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Align the local `sekai` asset files with the official [primefaces/sakai-assets](https://github.com/primefaces/sakai-assets) repository. Adds the `tailwindcss-primeui` Tailwind v4 plugin, restores the `_preloading.scss` and `demo/` directory (present in upstream), aligns responsive breakpoints with upstream values, restores `_utils.scss` helpers, and keeps all previous optimizations (inlined focus styles, `.p-dark` selector, removed redundant `--p-*` aliases).

## 1. Requirements & Constraints

- **REQ-001**: Install `tailwindcss-primeui@^0.6.1` — official PrimeFaces Tailwind v4 plugin for PrimeUI component integration.
- **REQ-002**: Update `src/assets/main.css` with the upstream `tailwind.css` config: `@plugin 'tailwindcss-primeui'`, custom breakpoints (sm: 576, md: 768, lg: 992, xl: 1200, 2xl: 1920), dark variant using `.p-dark` (not `[class*="app-dark"]`), and `@layer base` border-color compat layer.
- **REQ-003**: Restore `_preloading.scss` from upstream (full-screen splash loader with pulsing animation).
- **REQ-004**: Restore `demo/` directory from upstream (demo.scss, code.scss, flags/).
- **REQ-005**: Add `--code-background` and `--code-color` CSS variables back to `_common.scss`.
- **REQ-006**: Update `main.scss` to import preloading and demo layers.
- **REQ-007**: Align responsive breakpoints in `_responsive.scss` and `_topbar.scss` to upstream values: lg at 992px (not 1024px), 2xl at 1920px (not 1536px), mobile at 991px.
- **REQ-008**: Restore `_utils.scss` helpers: `.clearfix`, `.p-toast` position override.
- **CON-001**: Keep all previous optimizations — inlined focus styles (no `_mixins.scss`), `.p-dark` dark mode selector, no redundant CSS variable aliases.
- **CON-002**: The `tailwindcss-primeui` plugin must be compatible with Tailwind v4 (`tailwindcss@^4.3.2`).
- **CON-003**: PrimeVue v4 (`@primevue/themes`) must remain the theming source — the plugin integrates with PrimeUI tokens, not override them.

## 2. Implementation Steps

### Implementation Phase 1: Install & Configure tailwindcss-primeui

- GOAL-001: Install the PrimeUI Tailwind plugin and update the Tailwind entry file.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Run `pnpm add -D tailwindcss-primeui` to install the plugin | | |
| TASK-002 | Update `src/assets/main.css`: add `@plugin 'tailwindcss-primeui'` after `@import "tailwindcss"` | | |
| TASK-003 | Add custom breakpoints to `main.css` via `@theme` block (sm: 576, md: 768, lg: 992, xl: 1200, 2xl: 1920) | | |
| TASK-004 | Add `@custom-variant dark (&:where(.p-dark, .p-dark *))` — uses our `.p-dark` selector, not upstream's `[class*="app-dark"]` | | |
| TASK-005 | Add `@layer base { *, ::after, ::before, ::backdrop, ::file-selector-button { border-color: var(--color-gray-200, currentcolor); } }` for Tailwind v4 border-color compatibility | | |

### Implementation Phase 2: Restore Preloader and Demo Files

- GOAL-002: Restore `_preloading.scss` and `demo/` directory from upstream sakai-assets.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | Create `src/assets/sekai/layout/_preloading.scss` with upstream content (full-screen loader with `::before`/`::after` pulsing animation) | | |
| TASK-007 | Create `src/assets/sekai/demo/demo.scss` with `@use './code.scss'; @use './flags/flags.css';` | | |
| TASK-008 | Create `src/assets/sekai/demo/code.scss` with upstream content (dark code block styling) | | |
| TASK-009 | Create `src/assets/sekai/demo/flags/` directory and fetch `flags.css` from upstream | | |
| TASK-010 | Update `_common.scss`: add `--code-background` and `--code-color` variables | | |

### Implementation Phase 3: Align Layout Files with Upstream

- GOAL-003: Update `main.scss`, `_responsive.scss`, `_topbar.scss`, `_utils.scss` to align with upstream structure while keeping optimizations.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-011 | Update `main.scss`: add `@use 'layout/preloading'` and `@use 'demo/demo'` imports | | |
| TASK-012 | Update `_responsive.scss`: revert breakpoints to upstream values — `min-width: 1920px` (was 1536px), `min-width: 992px` (was 1024px), `max-width: 991px` (was 1023px) | | |
| TASK-013 | Update `_topbar.scss`: revert responsive breakpoint from 1023px to 991px to match upstream | | |
| TASK-014 | Update `_utils.scss`: restore `.clearfix` and `.p-toast` position override from upstream | | |

### Implementation Phase 4: Verify Build and Consistency

- GOAL-004: Run build and type-check, verify no regressions.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-015 | Run `npx vite build` — confirm zero errors | | |
| TASK-016 | Run `npx vue-tsc --build` — confirm zero errors | | |
| TASK-017 | Verify `tailwindcss-primeui` is listed in `package.json` devDependencies | | |

## 3. Alternatives

- **ALT-001**: Skip `tailwindcss-primeui` plugin and keep custom Tailwind utilities. Rejected — the plugin provides PrimeUI-specific color tokens and dark-mode utilities that reduce manual Tailwind config.
- **ALT-002**: Keep our custom breakpoints (1024px, 1536px). Rejected — PrimeVue components internally use 992px as the breakpoint for responsive behavior; mismatched breakpoints cause visual glitches.
- **ALT-003**: Keep demo/ and preloader deleted. Rejected — they're part of the upstream distribution and provide useful scaffolding for demo pages and loading states.

## 4. Dependencies

- **DEP-001**: `tailwindcss-primeui@^0.6.1` — MIT-licensed Tailwind v4 plugin.
- **DEP-002**: `tailwindcss@^4.3.2` — already installed, must remain for v4 plugin compatibility.
- **DEP-003**: PrimeIcons flags CSS (`flags.css`) — from `sakai-assets/demo/flags/`, provides flag icon classes.

## 5. Files

- **FILE-001**: `src/assets/main.css` — add plugin, breakpoints, dark variant, base layer.
- **FILE-002**: `src/assets/sekai/layout/_preloading.scss` — restored from upstream.
- **FILE-003**: `src/assets/sekai/demo/demo.scss` — restored from upstream.
- **FILE-004**: `src/assets/sekai/demo/code.scss` — restored from upstream.
- **FILE-005**: `src/assets/sekai/demo/flags/flags.css` — restored from upstream.
- **FILE-006**: `src/assets/sekai/abstracts/variables/_common.scss` — add `--code-background`, `--code-color`.
- **FILE-007**: `src/assets/sekai/main.scss` — add `preloading` and `demo` imports.
- **FILE-008**: `src/assets/sekai/layout/_responsive.scss` — revert breakpoints.
- **FILE-009**: `src/assets/sekai/layout/_topbar.scss` — revert mobile breakpoint.
- **FILE-010**: `src/assets/sekai/layout/_utils.scss` — restore `clearfix` and `p-toast`.
- **FILE-011**: `package.json` — add `tailwindcss-primeui`.

## 6. Testing

- **TEST-001**: `npx vite build` exits with code 0.
- **TEST-002**: `npx vue-tsc --build` exits with code 0.
- **TEST-003**: `grep 'tailwindcss-primeui' package.json` returns non-empty.
- **TEST-004**: Visual check — preloader animation appears on initial page load.
- **TEST-005**: Visual check — dark mode toggle with `.p-dark` still works.
- **TEST-006**: Visual check — breakpoint at 992px triggers sidebar overlay mode correctly.

## 7. Risks & Assumptions

- **RISK-001**: `tailwindcss-primeui@0.6.1` may not be compatible with the latest Tailwind v4.3.x. Mitigation: test build immediately after install; if incompatible, skip the plugin and use manual Tailwind config.
- **RISK-002**: Restoring `demo/` adds ~2 kB of unused CSS (flag icons, code blocks). Mitigation: negligible for production (tree-shaken if not used).
- **ASSUMPTION-001**: The upstream breakpoint values (991px/992px/1960px) are what PrimeVue components expect for responsive behavior.
- **ASSUMPTION-002**: The `tailwindcss-primeui` plugin does not conflict with `@primevue/themes/aura` runtime tokens.

## 8. Related Specifications / Further Reading

- `plan/refactor-scss-architecture-2.md` — Previous plan (v2) with PrimeVue v4 downgrade and sekai rename.
- https://github.com/primefaces/sakai-assets — Official Sakai assets repository (source of aligned files).
- https://github.com/primefaces/tailwindcss-primeui — PrimeUI Tailwind v4 plugin.
- https://primevue.org/v4/theming/styled/ — PrimeVue v4 theming documentation.
