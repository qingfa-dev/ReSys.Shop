---
goal: Scaffold Store frontend app with Nuxt UI Vue, Tailwind CSS v4, and Sass
version: 1.0
date_created: 2026-07-01
status: Planned
tags: feature, frontend, vue, nuxt-ui, store
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Create the Store frontend application under `app/Store/` using Nuxt UI (Vue version) for the component library, Tailwind CSS v4 for utility styling, and Sass for custom SCSS architecture. This follows the same project conventions as the Admin app but replaces PrimeVue with Nuxt UI.

## 1. Requirements & Constraints

- **REQ-001**: Scaffold a new Vue 3 + Vite + TypeScript SPA under `app/Store/`
- **REQ-002**: Use `@nuxt/ui` (Vue plugin) as the primary component library
- **REQ-003**: Use Tailwind CSS v4 (`@tailwindcss/vite`) for utility-first CSS
- **REQ-004**: Use Sass (`sass` package) for custom SCSS files (variables, layout, typography)
- **REQ-005**: Follow the same build tooling as Admin app: Vitest, ESLint, Oxlint, vue-tsc
- **REQ-006**: Include Vue Router for client-side routing
- **REQ-007**: Include Pinia for state management
- **REQ-008**: Register the `@nuxt/ui/vue-plugin` in main.ts
- **REQ-009**: Wrap the app with `UApp` component per Nuxt UI requirements
- **REQ-010**: Add `isolate` class to the root `#app` div in index.html
- **REQ-011**: Auto-generated type declarations (`auto-imports.d.ts`, `components.d.ts`) must be in `.gitignore` and included in `tsconfig.app.json`
- **REQ-012**: TypeScript alias `#build/ui` must be added to `tsconfig.node.json` and `tsconfig.app.json`
- **CON-001**: Node engine must match Admin app (`^20.19.0 || >=22.12.0`)
- **CON-002**: Package manager must be `pnpm` (consistent with Admin)
- **PAT-001**: Follow Admin app's file structure conventions (vite.config, tsconfig layout, src/ layout)

## 2. Implementation Steps

### Phase 1: Scaffold project skeleton

- GOAL-001: Create the directory structure, package.json, tsconfig files, and Vite config

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `app/Store/` directory structure: `.vscode/`, `public/`, `src/`, `src/assets/`, `src/assets/css/`, `src/router/`, `src/stores/`, `src/__tests__/` | | |
| TASK-002 | Create `package.json` with dependencies: `vue`, `vue-router`, `pinia`, `@nuxt/ui`, `@iconify/vue`, `tailwindcss`; devDependencies: `@tailwindcss/vite`, `@vitejs/plugin-vue`, `@vitejs/plugin-vue-jsx`, `typescript`, `vue-tsc`, `vite`, `vitest`, `@vue/test-utils`, `jsdom`, `@vitest/coverage-v8`, `sass`, `eslint`, `@vue/eslint-config-typescript`, `eslint-plugin-vue`, `oxlint`, `oxfmt`, `npm-run-all2`, `@tsconfig/node24`, `@types/node`, `@types/jsdom`, `unplugin-vue-components`, `unplugin-auto-import` | | |
| TASK-003 | Create `tsconfig.json` (references `tsconfig.app.json` and `tsconfig.node.json`) | | |
| TASK-004 | Create `tsconfig.app.json` extending `@vue/tsconfig/tsconfig.dom.json`, include `env.d.ts`, `src/**/*`, `src/**/*.vue`, `auto-imports.d.ts`, `components.d.ts`; add path aliases: `@/*` → `./src/*`, `#build/ui/*` → `./node_modules/.nuxt-ui/ui/*` | | |
| TASK-005 | Create `tsconfig.node.json` extending `@tsconfig/node24`, include `vite.config.*`, `vitest.config.*`, `eslint.config.*`; add path alias `#build/ui` → `./node_modules/.nuxt-ui/ui` | | |
| TASK-006 | Create `tsconfig.vitest.json` extending `tsconfig.app.json` with `@vitest/coverage-v8` types | | |
| TASK-007 | Create `env.d.ts` with `/// <reference types="vite/client" />` and Vue module declaration | | |
| TASK-008 | Create `vite.config.ts` with plugins in order: `@tailwindcss/vite`, `@vitejs/plugin-vue`, `@vitejs/plugin-vue-jsx`, `@nuxt/ui/vite`; configure `@` alias to `./src` | | |
| TASK-009 | Create `vitest.config.ts` importing from `vitest/config` | | |
| TASK-010 | Create `eslint.config.ts` with Vue + TypeScript ESLint config | | |
| TASK-011 | Create `.editorconfig` (copy from Admin app pattern) | | |
| TASK-012 | Create `.gitattributes` | | |
| TASK-013 | Create `.gitignore` including `node_modules/`, `dist/`, `auto-imports.d.ts`, `components.d.ts`, `.nuxt-ui/` | | |
| TASK-014 | Create `.vscode/extensions.json` for recommended extensions (Vue, Tailwind CSS IntelliSense) | | |
| TASK-015 | Create `.vscode/settings.json` with Tailwind CSS class attributes config | | |
| TASK-016 | Create `pnpm-workspace.yaml` | | |

### Phase 2: Create entry points and app shell

- GOAL-002: Wire up the HTML entry, main.ts, App.vue, and CSS entry

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-017 | Create `index.html` with `<div id="app" class="isolate"></div>` per Nuxt UI requirement, reference `src/main.ts` | | |
| TASK-018 | Create `src/assets/css/main.css` with `@import "tailwindcss"; @import "@nuxt/ui";` | | |
| TASK-019 | Create `src/main.ts` importing `./assets/css/main.css`, bootstrapping Vue + Pinia + Vue Router + `@nuxt/ui/vue-plugin` | | |
| TASK-020 | Create `src/App.vue` wrapping `<RouterView />` inside `<UApp>` | | |
| TASK-021 | Create `src/router/index.ts` with `createRouter` and `createWebHistory`, empty routes array | | |
| TASK-022 | Create `src/stores/counter.ts` (basic Pinia store for placeholder) | | |
| TASK-023 | Create `public/favicon.ico` | | |

### Phase 3: SCSS architecture and theming

- GOAL-003: Set up the Sass layer with Nuxt UI theme overrides

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-024 | Create `src/assets/sekai/main.scss` as the SCSS entry point | | |
| TASK-025 | Create `src/assets/sekai/abstracts/variables/_common.scss` for SCSS variables (colors, spacing, breakpoints) | | |
| TASK-026 | Create `src/assets/sekai/base/_core.scss` for base/reset styles | | |
| TASK-027 | Create `src/assets/sekai/base/_typography.scss` for typography overrides | | |
| TASK-028 | Import SCSS files in `main.scss` and import `main.scss` in `main.ts` | | |

### Phase 4: Tests and linting

- GOAL-004: Add placeholder test and verify toolchain

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-029 | Create `src/__tests__/App.spec.ts` with basic Vitest test for App.vue | | |
| TASK-030 | Create `.oxfmtrc.json` (copy from Admin) | | |
| TASK-031 | Create `.oxlintrc.json` (copy from Admin) | | |
| TASK-032 | Verify `pnpm install`, `pnpm type-check`, `pnpm test:unit`, `pnpm build` all pass | | |

## 3. Alternatives

- **ALT-001**: Use PrimeVue (like Admin) — rejected because the Store app should showcase a different UI library (Nuxt UI) for visual distinction and to leverage Nuxt UI's e-commerce-friendly components.
- **ALT-002**: Use Nuxt (full framework) instead of Vue SPA — rejected because the Store is a standalone frontend that doesn't need SSR/SSG; Vue SPA + Vite is simpler and consistent with Admin.
- **ALT-003**: Skip Sass and use only Tailwind — rejected because custom SCSS provides better organization for theme variables and layout partials, matching the Admin app's architecture.

## 4. Dependencies

- **DEP-001**: `vue` ^3.5 — UI framework
- **DEP-002**: `vue-router` ^5.1 — client-side routing
- **DEP-003**: `pinia` ^3.0 — state management
- **DEP-004**: `@nuxt/ui` latest — component library (Vue plugin)
- **DEP-005**: `@iconify/vue` — icon library (peer dependency of Nuxt UI)
- **DEP-006**: `tailwindcss` ^4.3 — utility CSS framework
- **DEP-007**: `@tailwindcss/vite` ^4.3 — Tailwind Vite plugin
- **DEP-008**: `sass` ^1.101 — SCSS compilation
- **DEP-009**: `vite` ^8.0 — build tool
- **DEP-010**: `typescript` ~6.0 — type checking
- **DEP-011**: `vue-tsc` ^3.3 — Vue type checking
- **DEP-012**: `vitest` ^4.1 — unit testing
- **DEP-013**: `@vitejs/plugin-vue` ^6.0 — Vite Vue plugin
- **DEP-014**: `unplugin-vue-components` — auto-import components (bundled with Nuxt UI)
- **DEP-015**: `unplugin-auto-import` — auto-import composables (bundled with Nuxt UI)

## 5. Files

- **FILE-001**: `app/Store/package.json` — project manifest and dependencies
- **FILE-002**: `app/Store/vite.config.ts` — Vite build configuration with Nuxt UI plugin
- **FILE-003**: `app/Store/vitest.config.ts` — Vitest test configuration
- **FILE-004**: `app/Store/eslint.config.ts` — ESLint configuration
- **FILE-005**: `app/Store/tsconfig.json` — root TSConfig
- **FILE-006**: `app/Store/tsconfig.app.json` — app TSConfig with path aliases
- **FILE-007**: `app/Store/tsconfig.node.json` — node TSConfig with `#build/ui` alias
- **FILE-008**: `app/Store/tsconfig.vitest.json` — test TSConfig
- **FILE-009**: `app/Store/index.html` — HTML entry with `isolate` class
- **FILE-010**: `app/Store/src/main.ts` — Vue app bootstrap with Nuxt UI plugin
- **FILE-011**: `app/Store/src/App.vue` — root component with `UApp` wrapper
- **FILE-012**: `app/Store/src/router/index.ts` — Vue Router setup
- **FILE-013**: `app/Store/src/stores/counter.ts` — Pinia counter store (placeholder)
- **FILE-014**: `app/Store/src/assets/css/main.css` — Tailwind + Nuxt UI CSS entry
- **FILE-015**: `app/Store/src/assets/sekai/main.scss` — SCSS entry point
- **FILE-016`: `app/Store/src/assets/sekai/abstracts/variables/_common.scss` — SCSS variables
- **FILE-017**: `app/Store/src/assets/sekai/base/_core.scss` — base/reset styles
- **FILE-018**: `app/Store/src/assets/sekai/base/_typography.scss` — typography overrides
- **FILE-019**: `app/Store/src/__tests__/App.spec.ts` — basic App smoke test
- **FILE-020**: `app/Store/.gitignore` — git ignore rules
- **FILE-021**: `app/Store/.editorconfig` — editor settings
- **FILE-022**: `app/Store/.gitattributes` — git attributes
- **FILE-023**: `app/Store/.vscode/extensions.json` — recommended extensions
- **FILE-024**: `app/Store/.vscode/settings.json` — VS Code settings for Tailwind
- **FILE-025**: `app/Store/env.d.ts` — Vite/Vue type declarations
- **FILE-026**: `app/Store/public/favicon.ico` — favicon

## 6. Testing

- **TEST-001**: `pnpm type-check` — TypeScript compiles without errors (tests `vue-tsc --build`)
- **TEST-002**: `pnpm test:unit` — Vitest runs and `App.spec.ts` passes
- **TEST-003**: `pnpm build` — Vite production build completes without errors
- **TEST-004**: `pnpm lint` — ESLint and Oxlint pass without errors
- **TEST-005**: Manual verification: dev server starts with `pnpm dev` and renders UApp wrapper

## 7. Risks & Assumptions

- **RISK-001**: Nuxt UI Vue plugin API may change between versions — pin exact version and verify on each upgrade
- **RISK-002**: `#build/ui` path alias may differ depending on Nuxt UI version — verify after `pnpm install` that `node_modules/.nuxt-ui/ui` exists
- **ASSUMPTION-001**: Admin app's tooling versions (Vite 8, TypeScript 6, etc.) are compatible with `@nuxt/ui` — verify during Phase 4
- **ASSUMPTION-001** (sic): `@iconify/vue` is required as a peer dependency of Nuxt UI for icon support

## 8. Related Specifications / Further Reading

- [Nuxt UI Vue Installation Docs](https://ui.nuxt.com/docs/getting-started/installation/vue)
- [Nuxt UI App Component](https://ui.nuxt.com/docs/components/app)
- [Tailwind CSS v4 Installation](https://tailwindcss.com/docs/installation/vite)
- [Admin app scaffolding](plan/agent-creation.md) — reference for project conventions
