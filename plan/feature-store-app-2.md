---
goal: Scaffold Store frontend SPA with Nuxt UI Vue, Tailwind CSS v4, and Sass
version: 1.0
date_created: 2026-07-01
status: Planned
tags: feature, frontend, vue, nuxt-ui, store, shop
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Create the Store (shop frontend) application under `app/Store/` using Nuxt UI (Vue version) as the component library, Tailwind CSS v4 for utility styling, and Sass for custom storefront SCSS. The app will be a public-facing e-commerce SPA with routes for product listing, product detail, cart, and checkout.

## 1. Requirements & Constraints

- **REQ-001**: Scaffold a new Vue 3 + Vite + TypeScript SPA under `app/Store/`
- **REQ-002**: Use `@nuxt/ui` (Vue plugin) as the UI component library
- **REQ-003**: Use Tailwind CSS v4 via `@tailwindcss/vite` plugin
- **REQ-004**: Use Sass (`sass` package) for custom SCSS variables and base styles
- **REQ-005**: Use Pinia for state management (cart, auth)
- **REQ-006**: Use Vue Router with named routes: `Home`, `Products`, `ProductDetail`, `Cart`, `Checkout`
- **REQ-007**: Register `@nuxt/ui/vue-plugin` in `main.ts`
- **REQ-008**: Wrap root component with `<UApp>` per Nuxt UI requirement
- **REQ-009**: Add `isolate` class to the root `#app` container in `index.html`
- **REQ-010**: Configure Nuxt UI with shop-friendly color palette (primary: `amber`, neutral: `zinc`)
- **REQ-011**: Auto-generated type declarations (`auto-imports.d.ts`, `components.d.ts`) in `.gitignore` and `tsconfig.app.json`
- **REQ-012**: Add TypeScript alias `#build/ui` to `tsconfig.node.json` and `#build/ui/*` to `tsconfig.app.json`
- **CON-001**: Node engine must be `^20.19.0 || >=22.12.0`
- **CON-002**: Package manager must be `pnpm` (monorepo consistency)
- **CON-003**: Build tooling must match Admin app: Vite 8, TypeScript 6, Vitest 4, ESLint 10, Oxlint
- **PAT-001**: Follow Admin app's file structure conventions (config files at root, src layout)

## 2. Implementation Steps

### Implementation Phase 1: Project skeleton and configuration

- GOAL-001: Create directory structure, package.json, all tsconfig files, Vite config with Nuxt UI plugin, ESLint config

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create directory tree: `app/Store/.vscode/`, `app/Store/public/`, `app/Store/src/assets/css/`, `app/Store/src/assets/shop/`, `app/Store/src/router/`, `app/Store/src/stores/`, `app/Store/src/views/`, `app/Store/src/components/`, `app/Store/src/composables/`, `app/Store/src/__tests__/` | | |
| TASK-002 | Create `app/Store/package.json` with name `resys_store`, version `0.0.0`, private `true`, type `module`. Scripts: `dev`, `build` (`run-p type-check \"build-only {@}\" --`), `preview`, `test:unit`, `build-only`, `type-check`, `lint`, `format`. Dependencies: `vue@^3.5`, `vue-router@^5.1`, `pinia@^3.0`, `@nuxt/ui`, `@iconify/vue`, `tailwindcss@^4.3`. DevDependencies: `@tailwindcss/vite@^4.3`, `@vitejs/plugin-vue@^6.0`, `@vitejs/plugin-vue-jsx@^5.1`, `typescript@~6.0`, `vue-tsc@^3.3`, `vite@^8.0`, `vitest@^4.1`, `@vue/test-utils@^2.4`, `jsdom@^29.1`, `@vitest/coverage-v8@^4.1`, `sass@^1.101`, `eslint@^10.5`, `@vue/eslint-config-typescript@^14.8`, `eslint-plugin-vue@~10.9`, `oxlint@~1.69`, `oxfmt@^0.54`, `npm-run-all2@^9.0`, `@tsconfig/node24@^24.0`, `@types/node@^24.13`, `@types/jsdom@^28.0`. Engines: `node "^20.19.0 || >=22.12.0"` | | |
| TASK-003 | Create `app/Store/tsconfig.json` with `files: []`, `references: [{ path: './tsconfig.app.json' }, { path: './tsconfig.node.json' }]` | | |
| TASK-004 | Create `app/Store/tsconfig.app.json` extending `@vue/tsconfig/tsconfig.dom.json`. Include: `env.d.ts`, `src/**/*`, `src/**/*.vue`, `auto-imports.d.ts`, `components.d.ts`. Exclude: `src/**/__tests__/*`. CompilerOptions: `noUncheckedIndexedAccess: true`, `paths: { "@/*": ["./src/*"], "#build/ui/*": ["./node_modules/.nuxt-ui/ui/*"] }`, `tsBuildInfoFile: "./node_modules/.tmp/tsconfig.app.tsbuildinfo"` | | |
| TASK-005 | Create `app/Store/tsconfig.node.json` extending `@tsconfig/node24/tsconfig.json`. Include: `vite.config.*`, `vitest.config.*`, `eslint.config.*`. CompilerOptions: `module: "preserve"`, `moduleResolution: "bundler"`, `types: ["node"]`, `noEmit: true`, `paths: { "#build/ui": ["./node_modules/.nuxt-ui/ui"] }`, `tsBuildInfoFile: "./node_modules/.tmp/tsconfig.node.tsbuildinfo"` | | |
| TASK-006 | Create `app/Store/tsconfig.vitest.json` extending `tsconfig.app.json`. Include: `src/**/__tests__/*`, `env.d.ts`. CompilerOptions: `types: ["@vitest/coverage-v8"]`, `tsBuildInfoFile: "./node_modules/.tmp/tsconfig.vitest.tsbuildinfo"` | | |
| TASK-007 | Create `app/Store/env.d.ts` with `/// <reference types="vite/client" />` and `declare module '*.vue' { import type { DefineComponent } from 'vue'; const component: DefineComponent<object, object, unknown>; export default component; }` | | |
| TASK-008 | Create `app/Store/vite.config.ts` with plugins in order: `tailwind()`, `vue()`, `vueJsx()`, `ui({ ui: { colors: { primary: 'amber', neutral: 'zinc' } } })`. Configure resolve alias `@` → `./src`. Import from `tailwindcss`, `@vitejs/plugin-vue`, `@vitejs/plugin-vue-jsx`, `@nuxt/ui/vite` | | |
| TASK-009 | Create `app/Store/vitest.config.ts` importing `defineConfig` from `vitest/config`. Export config: `include: ['src/**/__tests__/**']`, `environment: 'jsdom'`, `css: true`, `coverage: { provider: 'v8', include: ['src/**/*.ts', 'src/**/*.vue'], exclude: ['src/**/__tests__/**', 'src/**/*.d.ts'] }` | | |
| TASK-010 | Create `app/Store/eslint.config.ts` — same pattern as Admin app | | |
| TASK-011 | Create `app/Store/.editorconfig` — `root: true`, charset utf-8, indent style space, indent size 2, end of line lf, insert final newline, trim trailing whitespace | | |
| TASK-012 | Create `app/Store/pnpm-workspace.yaml` with `onlyBuiltDependencies: ['esbuild']` | | |

### Implementation Phase 2: Entry points and app shell

- GOAL-002: Wire up index.html, main.ts, App.vue, CSS, and route stubs

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-013 | Create `app/Store/index.html`: `<html lang="en">`, `<body class="antialiased">`, `<div id="app" class="isolate"></div>`, script module src `/src/main.ts` | | |
| TASK-014 | Create `app/Store/src/main.ts`: import `./assets/css/main.css`; createApp App; createPinia; createRouter with routes from `./router`; app.use(pinia); app.use(router); app.use(ui); app.mount('#app') | | |
| TASK-015 | Create `app/Store/src/App.vue` with `<UApp><RouterView /></UApp>` | | |
| TASK-016 | Create `app/Store/src/assets/css/main.css` containing `@import "tailwindcss"; @import "@nuxt/ui";` | | |
| TASK-017 | Create `app/Store/src/router/index.ts`: createRouter with `history: createWebHistory()`, routes array with `Home`, `Products`, `ProductDetail` (path `:id`), `Cart`, `Checkout`. Each route lazy-imports its view component | | |
| TASK-018 | Create stub view files: `src/views/HomeView.vue`, `src/views/ProductsView.vue`, `src/views/ProductDetailView.vue`, `src/views/CartView.vue`, `src/views/CheckoutView.vue`. Each contains `<template><div><h1>PageName</h1></div></template>` with `<script setup lang="ts">` | | |
| TASK-019 | Create `app/Store/public/favicon.ico` | | |

### Implementation Phase 3: Storefront SCSS layer

- GOAL-003: Create custom SCSS with shop-oriented color variables and base styles (no Sekai admin theme)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-020 | Create `app/Store/src/assets/shop/main.scss` — entry point that imports all shop partials | | |
| TASK-021 | Create `app/Store/src/assets/shop/_variables.scss` — SCSS variables: `$color-primary`, `$color-accent`, `$font-family-body: 'Inter', sans-serif`, `$font-family-heading: 'Inter', sans-serif`, `$max-width-content: 1280px`, `$header-height: 4rem`, breakpoint map (`sm: 640px`, `md: 768px`, `lg: 1024px`, `xl: 1280px`), spacing scale matching Tailwind | | |
| TASK-022 | Create `app/Store/src/assets/shop/_base.scss` — minimal base overrides: `body { font-family: $font-family-body; }`, link styles, focus outlines, selection color | | |
| TASK-023 | Create `app/Store/src/assets/shop/_typography.scss` — heading font families, responsive type scale mixins | | |
| TASK-024 | Create `app/Store/src/assets/shop/_layout.scss` — `.store-container` max-width centered layout, `.store-grid` product grid (responsive columns), `.store-header`/`.store-footer` placeholders | | |
| TASK-025 | Create `app/Store/src/assets/shop/_utilities.scss` — custom utility classes for product cards, price display, rating stars, badge variants | | |
| TASK-026 | In `main.scss`, import `_variables`, `_base`, `_typography`, `_layout`, `_utilities` in order | | |
| TASK-027 | In `src/main.ts`, add `import './assets/shop/main.scss'` after the CSS import | | |

### Implementation Phase 4: Pinia stores

- GOAL-004: Create store modules for cart and catalog state

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-028 | Create `src/stores/cart.ts` — defineStore `cart` with state: `items: CartItem[]`, `isOpen: boolean`; getters: `itemCount`, `subtotal`, `total`; actions: `addItem`, `removeItem`, `updateQuantity`, `clearCart`, `toggleCart`. Export `CartItem` interface (`id`, `name`, `price`, `image`, `quantity`) | | |
| TASK-029 | Create `src/stores/catalog.ts` — defineStore `catalog` with state: `products: Product[]`, `isLoading: boolean`, `error: string | null`; getters: `getProductById`; actions: `fetchProducts`, `fetchProduct`. Export `Product` interface (`id`, `name`, `slug`, `description`, `price`, `images`, `category`, `rating`, `inStock`) | | |

### Implementation Phase 5: Tests, linting, and verification

- GOAL-005: Verify the full toolchain passes

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-030 | Create `src/__tests__/App.spec.ts` — mount App.vue with router and Pinia, assert UApp renders | | |
| TASK-031 | Create `src/__tests__/cart.store.spec.ts` — test addItem, removeItem, updateQuantity, clearCart, and computed getters | | |
| TASK-032 | Create `.gitignore` with `node_modules/`, `dist/`, `auto-imports.d.ts`, `components.d.ts`, `.nuxt-ui/`, `*.tsbuildinfo`, `.tmp/` | | |
| TASK-033 | Create `.gitattributes` with `* text=auto` and `*.ts diff=typescript` | | |
| TASK-034 | Create `.vscode/extensions.json` with recommendations: `Vue.volar`, `bradlc.vscode-tailwindcss` | | |
| TASK-035 | Create `.vscode/settings.json` with `files.associations: { "*.css": "tailwindcss" }`, `tailwindCSS.classAttributes: ["class", "ui"]` | | |
| TASK-036 | Create `.oxfmtrc.json` and `.oxlintrc.json` (empty configs matching Admin) | | |
| TASK-037 | Run `pnpm install` — must complete without errors | | |
| TASK-038 | Run `pnpm type-check` — must pass with zero errors | | |
| TASK-039 | Run `pnpm test:unit` — must pass (App.spec.ts + cart.store.spec.ts) | | |
| TASK-040 | Run `pnpm build` — must produce dist/ without errors | | |
| TASK-041 | Run `pnpm lint` — must pass without warnings | | |

## 3. Alternatives

- **ALT-001**: Use the Sekai/PrimeVue theme like Admin — rejected because the Store is a public-facing shop frontend, not an admin panel. It needs a store-appropriate visual identity (e.g., amber/zinc palette, product card components, cart flows).
- **ALT-002**: Use Nuxt (full framework) with SSR — rejected because the Store app is a pure SPA; no SSR/SSG requirements exist yet. Keeps complexity minimal and consistent with the Admin app.
- **ALT-003**: Skip Sass and use only Tailwind — rejected because SCSS variables and partials provide better organization for layout abstractions (product grid, price display, rating stars) that are storefront-specific.

## 4. Dependencies

- **DEP-001**: `vue@^3.5` — UI framework
- **DEP-002**: `vue-router@^5.1` — client-side routing
- **DEP-003**: `pinia@^3.0` — state management
- **DEP-004**: `@nuxt/ui` — Vue component library (Nuxt UI v4)
- **DEP-005**: `@iconify/vue` — icon framework (peer of Nuxt UI)
- **DEP-006**: `tailwindcss@^4.3` — utility-first CSS
- **DEP-007**: `@tailwindcss/vite@^4.3` — Tailwind Vite plugin
- **DEP-008**: `sass@^1.101` — SCSS compilation
- **DEP-009**: `vite@^8.0` — build tool
- **DEP-010**: `typescript@~6.0` — type safety
- **DEP-011**: `vue-tsc@^3.3` — Vue type-checking
- **DEP-012**: `vitest@^4.1` — unit testing
- **DEP-013**: `@vitejs/plugin-vue@^6.0` — Vue SFC compilation
- **DEP-014**: `unplugin-vue-components` — auto-import (bundled with Nuxt UI)
- **DEP-015**: `unplugin-auto-import` — auto-import composables (bundled with Nuxt UI)

## 5. Files

- **FILE-001**: `app/Store/package.json`
- **FILE-002**: `app/Store/vite.config.ts`
- **FILE-003**: `app/Store/vitest.config.ts`
- **FILE-004**: `app/Store/eslint.config.ts`
- **FILE-005**: `app/Store/tsconfig.json`
- **FILE-006**: `app/Store/tsconfig.app.json`
- **FILE-007**: `app/Store/tsconfig.node.json`
- **FILE-008**: `app/Store/tsconfig.vitest.json`
- **FILE-009**: `app/Store/env.d.ts`
- **FILE-010**: `app/Store/index.html`
- **FILE-011**: `app/Store/src/main.ts`
- **FILE-012**: `app/Store/src/App.vue`
- **FILE-013**: `app/Store/src/router/index.ts`
- **FILE-014**: `app/Store/src/views/HomeView.vue`
- **FILE-015**: `app/Store/src/views/ProductsView.vue`
- **FILE-016**: `app/Store/src/views/ProductDetailView.vue`
- **FILE-017**: `app/Store/src/views/CartView.vue`
- **FILE-018**: `app/Store/src/views/CheckoutView.vue`
- **FILE-019**: `app/Store/src/stores/cart.ts`
- **FILE-020**: `app/Store/src/stores/catalog.ts`
- **FILE-021**: `app/Store/src/assets/css/main.css`
- **FILE-022**: `app/Store/src/assets/shop/main.scss`
- **FILE-023**: `app/Store/src/assets/shop/_variables.scss`
- **FILE-024**: `app/Store/src/assets/shop/_base.scss`
- **FILE-025**: `app/Store/src/assets/shop/_typography.scss`
- **FILE-026**: `app/Store/src/assets/shop/_layout.scss`
- **FILE-027**: `app/Store/src/assets/shop/_utilities.scss`
- **FILE-028**: `app/Store/src/__tests__/App.spec.ts`
- **FILE-029**: `app/Store/src/__tests__/cart.store.spec.ts`
- **FILE-030**: `app/Store/.gitignore`
- **FILE-031**: `app/Store/.gitattributes`
- **FILE-032**: `app/Store/.editorconfig`
- **FILE-033**: `app/Store/.vscode/extensions.json`
- **FILE-034**: `app/Store/.vscode/settings.json`
- **FILE-035**: `app/Store/.oxfmtrc.json`
- **FILE-036**: `app/Store/.oxlintrc.json`
- **FILE-037**: `app/Store/public/favicon.ico`
- **FILE-038**: `app/Store/pnpm-workspace.yaml`

## 6. Testing

- **TEST-001**: `pnpm type-check` — zero TypeScript errors
- **TEST-002**: `pnpm test:unit` — App.spec.ts mounts correctly, cart.store.spec.ts passes all CRUD operations
- **TEST-003**: `pnpm build` — Vite produces `dist/` without errors
- **TEST-004**: `pnpm lint` — ESLint and Oxlint pass
- **TEST-005**: Manual: `pnpm dev` starts and renders UApp wrapper with amber/zinc color scheme

## 7. Risks & Assumptions

- **RISK-001**: Nuxt UI Vue plugin API may change — pin to exact version and validate on upgrades
- **RISK-002**: `#build/ui` path alias must resolve after `pnpm install` — verify `node_modules/.nuxt-ui/ui/` exists before running `type-check`
- **RISK-003**: `@nuxt/ui` may pull incompatible `@iconify/vue` version — use `pnpm ls @iconify/vue` to verify
- **ASSUMPTION-001**: Admin app's tooling versions (Vite 8, TypeScript 6, ESLint 10) are compatible with `@nuxt/ui` v4
- **ASSUMPTION-002**: Nuxt UI provides storefront-suitable components (UButton, UCard, UInput, UBadge, UDropdownMenu, UModal, UIcon, UAvatar) out of the box

## 8. Related Specifications / Further Reading

- [Nuxt UI Vue Installation](https://ui.nuxt.com/docs/getting-started/installation/vue)
- [Nuxt UI App Component](https://ui.nuxt.com/docs/components/app)
- [Nuxt UI Color System](https://ui.nuxt.com/docs/getting-started/theme/design-system#colors)
- [Tailwind CSS v4 Vite Integration](https://tailwindcss.com/docs/installation/vite)
- [Admin app scaffolding](plan/feature-store-app-1.md) — previous version (replaced)
