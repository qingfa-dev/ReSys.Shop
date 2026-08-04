# Storefront Application — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build `app/Store/` — a PrimeVue 5 + Aura theme Vue 3.5 SPA storefront wired to the .NET backend API (65+ endpoints), satisfying all 9 thesis storefront use cases.

**Architecture:** Feature-sliced Vue 3 SPA matching Admin conventions. Shared core (API client, Result types, interceptors, querying) ported verbatim from `app/Admin/src/shared/`. Feature modules (catalog, ordering, identity, payment, shipping, inventory, location, profile) rebuild views from scratch using PrimeVue 5 patterns, referencing legacy `app/legacy/Storefront/` only for API wiring and store logic.

**Tech Stack:** Vue 3.5, TypeScript ~6.0, Vite 8, PrimeVue 5 + Aura preset, Tailwind CSS 4, Pinia 4, Axios 1, Vue Router 5, Zod 4, `@primevue/forms` 5, Vitest 4 + jsdom, Oxlint + ESLint, Oxfmt.

## Global Constraints

- PrimeVue version: `primevue@^5.0.0`, `@primeuix/themes@^3.0.0`, `@primevue/forms@^5.0.0`
- TypeScript `noUncheckedIndexedAccess: true` enforced
- Port `shared/api/*`, `shared/types/*`, `shared/composables/*` verbatim from `app/Admin/src/shared/` — do not modify
- API prefixes: `api/storefront` (catalog/ordering/payment/shipping/inventory), `api/store` (identity/profile/location)
- Result envelope uses `{ value, isSuccess, errors }` — NOT legacy `{ data, isFailure }`
- Feature slice: `services/`, `types/`, `validations/`, `views/`, `components/`, `composables/`, `stores/`, `routes/`
- Dual linting (Oxlint + ESLint) with 0 violations. Formatting via Oxfmt.
- Dev server port **5174**, proxy `/api` → `http://localhost:5035`
- Code Commenting Standard v3.0 on all view files
- Guest cart: UUID in localStorage `CART_TOKEN`
- Cart merge: `POST api/storefront/cart/associate` on login success
- Thesis doc: update "PrimeVue 4" → "PrimeVue 5" in `frontend-ux.typ` and `technology-stack.typ`
- AppHost: fix SPA path from `app/Storefront` → `app/Store`
- Risk R1: backend `SearchByImage.Response` must be extended with `similarityScore`, `searchDurationMs`, `model` before Phase 3

---

## Phase 0 — Scaffold & Build Config

### Task 0.1: Create directory structure and package.json

**Files:**
- Create: `app/Store/package.json`
- Create: `app/Store/pnpm-workspace.yaml`

**Interfaces:**
- Produces: `package.json` with all dependencies, `pnpm-workspace.yaml`

- [ ] **Step 1: Create pnpm-workspace.yaml**

```bash
mkdir -p app/Store
```

Write `app/Store/pnpm-workspace.yaml`:
```yaml
packages:
  - .
```

- [ ] **Step 2: Write package.json**

Write `app/Store/package.json`:
```json
{
  "name": "store",
  "version": "0.0.0",
  "private": true,
  "type": "module",
  "scripts": {
    "dev": "vite",
    "build": "run-p type-check \"build-only {@}\" --",
    "preview": "vite preview",
    "test:unit": "vitest",
    "test:coverage": "vitest --coverage",
    "build-only": "vite build",
    "type-check": "vue-tsc --build",
    "lint": "run-s \"lint:*\"",
    "lint:oxlint": "oxlint . --fix",
    "lint:eslint": "eslint . --fix --cache",
    "format": "oxfmt src/"
  },
  "dependencies": {
    "@iconify/vue": "^5.0.0",
    "@primeicons/vue": "^8.0.0",
    "@primeuix/themes": "^3.0.0",
    "@primevue/forms": "^5.0.0",
    "@vee-validate/zod": "^4.15.1",
    "autoprefixer": "^10.5.4",
    "axios": "^1.18.1",
    "jwt-decode": "^4.0.0",
    "pinia": "^4.0.2",
    "postcss": "^8.5.23",
    "primeicons": "^8.0.0",
    "primevue": "^5.0.0",
    "tailwindcss": "^4.3.3",
    "tailwindcss-primeui": "^0.6.1",
    "vee-validate": "^4.15.1",
    "vue": "^3.5.40",
    "vue-router": "^5.2.0",
    "zod": "^4.4.3"
  },
  "devDependencies": {
    "@pinia/testing": "^2.0.1",
    "@primevue/auto-import-resolver": "^5.0.0",
    "@tailwindcss/vite": "^4.3.3",
    "@tsconfig/node24": "^24.0.4",
    "@types/jsdom": "^28.0.3",
    "@types/node": "^26.1.1",
    "@vitejs/plugin-vue": "^6.0.8",
    "@vitejs/plugin-vue-jsx": "^5.1.6",
    "@vitest/coverage-v8": "^4.1.10",
    "@vitest/eslint-plugin": "^1.6.23",
    "@vue/eslint-config-prettier": "^10.2.0",
    "@vue/eslint-config-typescript": "^14.9.0",
    "@vue/test-utils": "^2.4.11",
    "@vue/tsconfig": "^0.9.1",
    "eslint": "^10.8.0",
    "eslint-config-prettier": "^10.1.8",
    "eslint-plugin-oxlint": "~1.73.0",
    "eslint-plugin-vue": "~10.10.0",
    "jiti": "^2.7.0",
    "jsdom": "^29.1.1",
    "npm-run-all2": "^9.0.2",
    "oxlint": "~1.74.0",
    "prettier": "3.9.6",
    "sass": "^1.102.0",
    "typescript": "~6.0.2",
    "unplugin-vue-components": "^32.1.0",
    "vite": "^8.1.5",
    "vite-plugin-vue-devtools": "^8.2.1",
    "vitest": "^4.1.10",
    "vue-eslint-parser": "^10.4.1",
    "vue-tsc": "^3.3.8"
  },
  "engines": {
    "node": "^22.18.0 || >=24.12.0"
  }
}
```

- [ ] **Step 3: Install dependencies**

```bash
cd app/Store && pnpm install
```

- [ ] **Step 4: Commit**

```bash
git add app/Store/package.json app/Store/pnpm-workspace.yaml app/Store/pnpm-lock.yaml
git commit -m "chore(store): scaffold package.json with PrimeVue 5 dependencies"
```

---

### Task 0.2: Create TypeScript and build config

**Files:**
- Create: `app/Store/tsconfig.json`
- Create: `app/Store/tsconfig.app.json`
- Create: `app/Store/tsconfig.node.json`
- Create: `app/Store/tsconfig.vitest.json`
- Create: `app/Store/env.d.ts`
- Create: `app/Store/vite.config.ts`
- Create: `app/Store/vitest.config.ts`

- [ ] **Step 1: Write tsconfig.json**

Write `app/Store/tsconfig.json`:
```json
{
  "files": [],
  "references": [
    { "path": "./tsconfig.node.json" },
    { "path": "./tsconfig.app.json" },
    { "path": "./tsconfig.vitest.json" }
  ]
}
```

- [ ] **Step 2: Write tsconfig.app.json**

Write `app/Store/tsconfig.app.json`:
```json
{
  "extends": "@vue/tsconfig/tsconfig.dom.json",
  "include": ["env.d.ts", "src/**/*", "src/**/*.vue"],
  "exclude": ["src/**/__tests__/*"],
  "compilerOptions": {
    "noUncheckedIndexedAccess": true,
    "paths": {
      "@/*": ["./src/*"],
      "@providers/*": ["./src/app/providers/*"],
      "@router/*": ["./src/app/router/*"],
      "@config/*": ["./src/app/config/*"]
    },
    "tsBuildInfoFile": "./node_modules/.tmp/tsconfig.app.tsbuildinfo"
  }
}
```

- [ ] **Step 3: Write tsconfig.node.json**

Write `app/Store/tsconfig.node.json`:
```json
{
  "extends": "@tsconfig/node24/tsconfig.json",
  "include": [
    "vite.config.*",
    "vitest.config.*",
    "eslint.config.*"
  ],
  "compilerOptions": {
    "noEmit": true,
    "module": "ESNext",
    "moduleResolution": "Bundler",
    "types": ["node"],
    "tsBuildInfoFile": "./node_modules/.tmp/tsconfig.node.tsbuildinfo"
  }
}
```

- [ ] **Step 4: Write tsconfig.vitest.json**

Write `app/Store/tsconfig.vitest.json`:
```json
{
  "extends": "./tsconfig.app.json",
  "include": ["env.d.ts", "src/**/__tests__/*"],
  "exclude": [],
  "compilerOptions": {
    "composite": true,
    "tsBuildInfoFile": "./node_modules/.tmp/tsconfig.vitest.tsbuildinfo"
  }
}
```

- [ ] **Step 5: Write env.d.ts**

Write `app/Store/env.d.ts`:
```ts
/// <reference types="vite/client" />

declare module '*.vue' {
  import type { DefineComponent } from 'vue'
  const component: DefineComponent<object, object, unknown>
  export default component
}

declare const __APP_VERSION__: string

interface ImportMetaEnv {
  readonly VITE_API_URL: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
```

- [ ] **Step 6: Write vite.config.ts**

Write `app/Store/vite.config.ts`:
```ts
import { readFileSync } from 'node:fs'
import { fileURLToPath, URL } from 'node:url'

const pkg = JSON.parse(readFileSync(new URL('./package.json', import.meta.url), 'utf-8'))

import tailwind from '@tailwindcss/vite'
import { PrimeVueResolver } from '@primevue/auto-import-resolver'
import Components from 'unplugin-vue-components/vite'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueJsx from '@vitejs/plugin-vue-jsx'
import vueDevTools from 'vite-plugin-vue-devtools'

export default defineConfig({
  define: {
    __APP_VERSION__: JSON.stringify(pkg.version),
  },
  plugins: [
    vue(),
    vueJsx(),
    vueDevTools(),
    tailwind(),
    Components({
      resolvers: [PrimeVueResolver()],
    }),
  ],
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5035',
        changeOrigin: true,
      },
    },
  },
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
      '@providers': fileURLToPath(new URL('./src/app/providers', import.meta.url)),
      '@router': fileURLToPath(new URL('./src/app/router', import.meta.url)),
      '@config': fileURLToPath(new URL('./src/app/config', import.meta.url)),
    },
  },
})
```

- [ ] **Step 7: Write vitest.config.ts**

Write `app/Store/vitest.config.ts`:
```ts
import { fileURLToPath } from 'node:url'
import { mergeConfig, defineConfig, configDefaults } from 'vitest/config'
import viteConfig from './vite.config'

export default mergeConfig(
  viteConfig,
  defineConfig({
    test: {
      environment: 'jsdom',
      exclude: [...configDefaults.exclude, 'e2e/**'],
      root: fileURLToPath(new URL('./', import.meta.url)),
      globals: true,
      setupFiles: [],
    },
  }),
)
```

- [ ] **Step 8: Run type-check to verify config**

```bash
cd app/Store && pnpm run type-check
```
Expected: error about missing `src/main.ts` — expected at this stage, config itself is valid.

- [ ] **Step 9: Commit**

```bash
git add app/Store/tsconfig*.json app/Store/env.d.ts app/Store/vite.config.ts app/Store/vitest.config.ts
git commit -m "chore(store): add TypeScript and Vite build configuration"
```

---

### Task 0.3: Create lint config

**Files:**
- Create: `app/Store/eslint.config.ts`
- Create: `app/Store/.oxlintrc.json`
- Create: `app/Store/.oxfmtrc.json`
- Create: `app/Store/.editorconfig`
- Create: `app/Store/.gitignore`

- [ ] **Step 1: Write eslint.config.ts**

Write `app/Store/eslint.config.ts` (copy verbatim from `app/Admin/eslint.config.ts`):
```bash
cp app/Admin/eslint.config.ts app/Store/eslint.config.ts
```

- [ ] **Step 2: Write .oxlintrc.json**

Write `app/Store/.oxlintrc.json` (copy from Admin):
```bash
cp app/Admin/.oxlintrc.json app/Store/.oxlintrc.json
```

- [ ] **Step 3: Write .oxfmtrc.json**

Write `app/Store/.oxfmtrc.json`:
```json
{
  "plugins": ["oxfmt"],
  "ignorePatterns": ["dist", "node_modules", "components.d.ts"]
}
```

- [ ] **Step 4: Write .editorconfig**

Write `app/Store/.editorconfig` (copy from Admin):
```bash
cp app/Admin/.editorconfig app/Store/.editorconfig
```

- [ ] **Step 5: Write .gitignore**

Write `app/Store/.gitignore`:
```
node_modules
dist
coverage
*.tsbuildinfo
components.d.ts
```

- [ ] **Step 6: Write .env.development**

Write `app/Store/.env.development`:
```
VITE_API_URL=http://localhost:5035
```

- [ ] **Step 7: Commit**

```bash
git add app/Store/eslint.config.ts app/Store/.oxlintrc.json app/Store/.oxfmtrc.json app/Store/.editorconfig app/Store/.gitignore app/Store/.env.development
git commit -m "chore(store): add lint, format, and environment configuration"
```

---

### Task 0.4: Create application entry point (main.ts, App.vue, assets)

**Files:**
- Create: `app/Store/index.html`
- Create: `app/Store/src/main.ts`
- Create: `app/Store/src/App.vue`
- Create: `app/Store/src/assets/tailwind.css`
- Create: `app/Store/src/assets/styles.scss`
- Create: `app/Store/src/app/config/env.ts`
- Create: `app/Store/src/app/providers/primevue.ts`
- Create: `app/Store/src/app/providers/pinia.ts`

- [ ] **Step 1: Write index.html**

Write `app/Store/index.html`:
```html
<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="UTF-8">
    <link rel="icon" href="/favicon.ico">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>ReSys.Shop</title>
  </head>
  <body>
    <div id="app"></div>
    <script type="module" src="/src/main.ts"></script>
  </body>
</html>
```

- [ ] **Step 2: Create directory structure**

```bash
mkdir -p app/Store/src/{assets,app/{config,providers,router,layouts,components/layout,composables},shared/{api/interceptors,composables,constants,types/querying,utils},features}
```

- [ ] **Step 3: Write env.ts**

Write `app/Store/src/app/config/env.ts`:
```ts
export const env = {
  apiUrl: import.meta.env.VITE_API_URL,
  baseUrl: import.meta.env.BASE_URL,
  appVersion: __APP_VERSION__,
  isDev: import.meta.env.DEV,
  isProd: import.meta.env.PROD,
} as const
```

- [ ] **Step 4: Write primevue.ts**

Write `app/Store/src/app/providers/primevue.ts`:
```ts
import type { App } from 'vue'
import Aura from '@primeuix/themes/aura'
import PrimeVue from 'primevue/config'
import ConfirmationService from 'primevue/confirmationservice'
import ToastService from 'primevue/toastservice'

export function registerPrimeVue(app: App): void {
  app.use(PrimeVue, {
    theme: {
      preset: Aura,
      options: {
        darkModeSelector: '.app-dark',
      },
    },
  })
  app.use(ToastService)
  app.use(ConfirmationService)
}
```

- [ ] **Step 5: Write pinia.ts**

Write `app/Store/src/app/providers/pinia.ts`:
```ts
import type { App } from 'vue'
import { createPinia } from 'pinia'

export function registerPinia(app: App): void {
  app.use(createPinia())
}
```

- [ ] **Step 6: Write tailwind.css**

Write `app/Store/src/assets/tailwind.css`:
```css
@import 'tailwindcss';
@plugin 'tailwindcss-primeui';
```

- [ ] **Step 7: Write styles.scss**

Write `app/Store/src/assets/styles.scss`:
```scss
// Storefront Aura theme overrides — defined in Phase 7 polish
body {
  margin: 0;
  font-family: 'Inter', system-ui, -apple-system, sans-serif;
}
```

- [ ] **Step 8: Write main.ts**

Write `app/Store/src/main.ts`:
```ts
import { createApp } from 'vue'
import App from './App.vue'
import router from './app/router'
import { registerPrimeVue } from '@providers/primevue'
import { registerPinia } from '@providers/pinia'

import '@/assets/tailwind.css'
import '@/assets/styles.scss'

const app = createApp(App)

app.use(router)
registerPinia(app)
registerPrimeVue(app)

app.mount('#app')
```

- [ ] **Step 9: Write minimal App.vue**

Write `app/Store/src/App.vue`:
```vue
<script setup lang="ts">
import { useToast } from 'primevue/usetoast'
import { setNotifyToast } from '@/shared/api/notify'

const toast = useToast()
setNotifyToast(toast)
</script>
<template>
  <Toast />
  <router-view />
</template>
```

- [ ] **Step 10: Write minimal router (empty routes, will be filled in Phase 2)**

Write `app/Store/src/app/router/route-meta.ts`:
```ts
import 'vue-router'

declare module 'vue-router' {
  interface RouteMeta {
    requiresAuth?: boolean
    guestOnly?: boolean
    title?: string
  }
}
```

Write `app/Store/src/app/router/routes.ts`:
```ts
import type { RouteRecordRaw } from 'vue-router'

export const routes: RouteRecordRaw[] = []
```

Write `app/Store/src/app/router/guards.ts`:
```ts
import type { Router } from 'vue-router'

let isInitialized = false

export function setupGuards(router: Router): void {
  router.beforeEach(async (to) => {
    // Auth guard — wired in Phase 2 after authStore exists
    if (!isInitialized) {
      isInitialized = true
    }
  })

  router.afterEach((to) => {
    if (to.meta.title) {
      document.title = `${to.meta.title} | ReSys.Shop`
    }
  })
}
```

Write `app/Store/src/app/router/index.ts`:
```ts
import { createRouter, createWebHistory } from 'vue-router'
import { routes } from './routes'
import { setupGuards } from './guards'
import './route-meta'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
})

setupGuards(router)

export default router
```

- [ ] **Step 11: Verify dev server starts**

```bash
cd app/Store && pnpm run dev
```
Expected: Vite starts on port 5174, blank page renders.

- [ ] **Step 12: Verify type-check and lint pass**

```bash
cd app/Store && pnpm run type-check
cd app/Store && pnpm run lint
```
Expected: 0 type errors, 0 lint violations.

- [ ] **Step 13: Commit**

```bash
git add app/Store/index.html app/Store/src/
git commit -m "feat(store): scaffold application entry point with PrimeVue 5 Aura"
```

---

### Task 0.5: Fix AppHost + thesis doc references

**Files:**
- Modify: `infra/Aspire/src/ReSys.AppHost/AppHost.cs`
- Modify: `thesis/chapters/part2/ch2-design/04-implementations/05-frontend-ux/frontend-ux.typ`
- Modify: `thesis/chapters/part2/ch2-design/04-implementations/01-technology-stack/technology-stack.typ`
- Create: `app/Store/AGENTS.md`

- [ ] **Step 1: Fix AppHost SPA path**

Read `infra/Aspire/src/ReSys.AppHost/AppHost.cs`, find the line referencing `app/Storefront`, change to `app/Store`.

```bash
grep -n "Storefront" infra/Aspire/src/ReSys.AppHost/AppHost.cs
```
Expected output shows the line. Edit to replace `Storefront` with `Store`.

- [ ] **Step 2: Update thesis frontend-ux.typ**

Read the file, find "PrimeVue 4 (Aura theme)", replace with "PrimeVue 5 (Aura theme)".

- [ ] **Step 3: Update thesis technology-stack.typ**

Find the PrimeVue version reference, update to match PrimeVue 5.

- [ ] **Step 4: Write AGENTS.md**

Copy from `app/Admin/AGENTS.md`, adapt paths:
```bash
cp app/Admin/AGENTS.md app/Store/AGENTS.md
```
Edit: change "Admin SPA" → "Store SPA" and `app/Admin` → `app/Store` in the path reference.

- [ ] **Step 5: Commit**

```bash
git add infra/Aspire/src/ReSys.AppHost/AppHost.cs thesis/ app/Store/AGENTS.md
git commit -m "chore(store): align AppHost, thesis docs, and AGENTS.md for app/Store"
```

---

## Phase 1 — Shared Core Layer

### Task 1.1: Port shared types (Result, Error, Querying)

**Files:**
- Create: `app/Store/src/shared/types/result.ts`
- Create: `app/Store/src/shared/types/error.ts`
- Create: `app/Store/src/shared/types/querying/index.ts`
- Create: `app/Store/src/shared/types/querying/types.ts`
- Create: `app/Store/src/shared/types/querying/mappers.ts`
- Create: `app/Store/src/shared/types/querying/parsers.ts`

**Interfaces:**
- Produces: `Result<T>`, `PagedResult<T>`, `ApiError`, `StatusCode`, factory functions (`ok`, `created`, `failure`, etc.), `QueryingParameters`, `QueryingModel`, `queryingModelToParams`, `queryingParamsToModel`

- [ ] **Step 1: Port result.ts verbatim from Admin**

```bash
cp app/Admin/src/shared/types/result.ts app/Store/src/shared/types/result.ts
```

- [ ] **Step 2: Port error.ts verbatim from Admin**

```bash
cp app/Admin/src/shared/types/error.ts app/Store/src/shared/types/error.ts
```

- [ ] **Step 3: Port querying/ verbatim from Admin**

```bash
mkdir -p app/Store/src/shared/types/querying
cp app/Admin/src/shared/types/querying/*.ts app/Store/src/shared/types/querying/
```

- [ ] **Step 4: Write querying barrel index**

Write `app/Store/src/shared/types/querying/index.ts`:
```ts
export * from './types'
export * from './mappers'
export * from './parsers'
```

- [ ] **Step 5: Write shared/types barrel index**

Write `app/Store/src/shared/types/index.ts`:
```ts
export * from './result'
export * from './error'
export * from './querying'
```

- [ ] **Step 6: Verify type-check passes**

```bash
cd app/Store && pnpm run type-check
```

- [ ] **Step 7: Write unit tests for result.ts**

Write `app/Store/src/shared/types/__tests__/result.spec.ts`:
```ts
import { describe, it, expect } from 'vitest'
import { ok, created, noContent, failure, badRequest, notFound, unauthorized, pagedOk, pagedFailure, isSuccess, isFailure, StatusCode } from '@/shared/types/result'
import type { ApiError } from '@/shared/types/error'

describe('Result factories', () => {
  it('ok() returns success Result with value', () => {
    const r = ok({ id: '1', name: 'Test' })
    expect(r.isSuccess).toBe(true)
    expect(r.statusCode).toBe(StatusCode.Ok)
    expect(r.value).toEqual({ id: '1', name: 'Test' })
    expect(r.errors).toEqual([])
    expect(isSuccess(r)).toBe(true)
    expect(isFailure(r)).toBe(false)
  })

  it('created() returns 201 with value', () => {
    const r = created({ id: '2' })
    expect(r.isSuccess).toBe(true)
    expect(r.statusCode).toBe(StatusCode.Created)
  })

  it('noContent() returns 204 with null value', () => {
    const r = noContent()
    expect(r.isSuccess).toBe(true)
    expect(r.statusCode).toBe(StatusCode.NoContent)
    expect(r.value).toBeNull()
  })

  it('failure() returns error Result', () => {
    const apiError: ApiError = { code: 'Test.Error', message: 'Something went wrong', type: 500 }
    const r = failure(apiError)
    expect(r.isSuccess).toBe(false)
    expect(r.errors[0]?.code).toBe('Test.Error')
    expect(r.value).toBeNull()
    expect(isFailure(r)).toBe(true)
  })

  it('badRequest() returns 400 with message', () => {
    const r = badRequest('Missing field')
    expect(r.isSuccess).toBe(false)
    expect(r.statusCode).toBe(400)
    expect(r.message).toBe('Missing field')
  })

  it('notFound() returns 404', () => {
    const r = notFound('Product not found')
    expect(r.isSuccess).toBe(false)
    expect(r.statusCode).toBe(404)
  })

  it('unauthorized() returns 401', () => {
    const r = unauthorized()
    expect(r.statusCode).toBe(401)
  })
})

describe('PagedResult factories', () => {
  it('pagedOk() returns paged result with items', () => {
    const items = [{ id: '1' }, { id: '2' }]
    const r = pagedOk(items, 1, 20, 42)
    expect(r.isSuccess).toBe(true)
    expect(r.items).toEqual(items)
    expect(r.page).toBe(1)
    expect(r.pageSize).toBe(20)
    expect(r.totalCount).toBe(42)
    expect(r.totalPages).toBe(3)
  })

  it('pagedOk() with zero pageSize returns 0 totalPages', () => {
    const r = pagedOk([], 1, 0, 10)
    expect(r.totalPages).toBe(0)
  })

  it('pagedFailure() returns error with empty items', () => {
    const r = pagedFailure([{ code: 'E1', message: 'fail', type: 500 }])
    expect(r.isSuccess).toBe(false)
    expect(r.items).toEqual([])
  })
})
```

- [ ] **Step 8: Run unit tests**

```bash
cd app/Store && pnpm run test:unit -- --run
```
Expected: all tests pass.

- [ ] **Step 9: Commit**

```bash
git add app/Store/src/shared/types/
git commit -m "feat(store): port Result, Error, and Querying types from Admin"
```

---

### Task 1.2: Port shared API layer (Axios, client, interceptors, paged)

**Files:**
- Create: `app/Store/src/shared/api/axios.ts` (port from Admin)
- Create: `app/Store/src/shared/api/client.ts` (port from Admin)
- Create: `app/Store/src/shared/api/paged.ts` (port from Admin)
- Create: `app/Store/src/shared/api/errors.ts` (port from Admin)
- Create: `app/Store/src/shared/api/notify.ts` (port from Admin)
- Create: `app/Store/src/shared/api/index.ts`
- Create: `app/Store/src/shared/api/interceptors/auth.ts`
- Create: `app/Store/src/shared/api/interceptors/camelcase.ts`
- Create: `app/Store/src/shared/api/interceptors/error.ts`
- Create: `app/Store/src/shared/api/interceptors/refresh.ts`
- Create: `app/Store/src/shared/constants/storage.ts`
- Create: `app/Store/src/shared/constants/api.ts`

- [ ] **Step 1: Port interceptor files verbatim**

```bash
cp app/Admin/src/shared/api/interceptors/auth.ts app/Store/src/shared/api/interceptors/auth.ts
cp app/Admin/src/shared/api/interceptors/camelcase.ts app/Store/src/shared/api/interceptors/camelcase.ts
cp app/Admin/src/shared/api/interceptors/error.ts app/Store/src/shared/api/interceptors/error.ts
cp app/Admin/src/shared/api/interceptors/refresh.ts app/Store/src/shared/api/interceptors/refresh.ts
```

- [ ] **Step 2: Port shared/api files verbatim**

```bash
cp app/Admin/src/shared/api/axios.ts app/Store/src/shared/api/axios.ts
cp app/Admin/src/shared/api/client.ts app/Store/src/shared/api/client.ts
cp app/Admin/src/shared/api/paged.ts app/Store/src/shared/api/paged.ts
cp app/Admin/src/shared/api/errors.ts app/Store/src/shared/api/errors.ts
cp app/Admin/src/shared/api/notify.ts app/Store/src/shared/api/notify.ts
```

- [ ] **Step 3: Write shared constants**

Write `app/Store/src/shared/constants/storage.ts`:
```ts
export const STORAGE_KEYS = {
  ACCESS_TOKEN: 'accessToken',
  REFRESH_TOKEN: 'refreshToken',
  CART_TOKEN: 'cartToken',
  USER: 'currentUser',
} as const
```

Write `app/Store/src/shared/constants/api.ts`:
```ts
export const API_STOREFRONT = 'api/storefront'
export const API_STORE = 'api/store'

export const ENDPOINTS = {
  // Catalog
  products: `${API_STOREFRONT}/products`,
  productBySlug: (slug: string) => `${API_STOREFRONT}/products/${slug}`,
  productAvailability: `${API_STOREFRONT}/products/availability`,
  productRelated: `${API_STOREFRONT}/products/related`,
  productSimilar: `${API_STOREFRONT}/products/similar`,
  searchByImage: `${API_STOREFRONT}/search-by-image`,
  taxonomies: `${API_STOREFRONT}/taxonomies`,
  taxonomyById: (id: string) => `${API_STOREFRONT}/taxonomies/${id}`,
  taxons: `${API_STOREFRONT}/taxons`,
  taxonProducts: `${API_STOREFRONT}/taxons/products`,
  optionTypes: `${API_STOREFRONT}/option-types`,
  images: `${API_STOREFRONT}/images`,

  // Ordering
  cart: `${API_STOREFRONT}/cart`,
  cartItems: `${API_STOREFRONT}/cart/items`,
  cartItem: (id: string) => `${API_STOREFRONT}/cart/items/${id}`,
  cartEmpty: `${API_STOREFRONT}/cart/empty`,
  cartAssociate: `${API_STOREFRONT}/cart/associate`,
  cartShippingRate: `${API_STOREFRONT}/cart/shipping-rate`,
  cartValidate: `${API_STOREFRONT}/cart/validate`,
  cartCheckout: `${API_STOREFRONT}/cart/checkout`,
  orders: `${API_STOREFRONT}/orders`,
  orderById: (id: string) => `${API_STOREFRONT}/orders/${id}`,
  orderCancel: (id: string) => `${API_STOREFRONT}/orders/${id}/cancel`,

  // Identity
  authLoginPassword: `${API_STORE}/identity/auth/login/password`,
  authLoginExternal: `${API_STORE}/identity/auth/login/external`,
  authLoginProviders: `${API_STORE}/identity/auth/login/providers`,
  authRegister: `${API_STORE}/identity/auth/register`,
  authLogout: `${API_STORE}/identity/auth/logout`,
  sessions: `${API_STORE}/identity/auth/sessions`,
  sessionsRefresh: `${API_STORE}/identity/auth/sessions/refresh`,
  sessionById: (id: string) => `${API_STORE}/identity/auth/sessions/${id}`,
  passwordsForgot: `${API_STORE}/identity/passwords/forgot`,
  passwordsReset: `${API_STORE}/identity/passwords/reset`,
  passwordsChange: `${API_STORE}/identity/passwords/change`,
  emailsChange: `${API_STORE}/identity/emails/change`,
  emailsConfirm: `${API_STORE}/identity/emails/confirm`,
  emailsResend: `${API_STORE}/identity/emails/resend`,

  // Payment
  paymentMethods: `${API_STOREFRONT}/payment/methods`,
  paymentCreateIntent: `${API_STOREFRONT}/payment/create-intent`,
  paymentConfirm: (id: string) => `${API_STOREFRONT}/payment/confirm/${id}`,
  paymentSetupIntent: `${API_STOREFRONT}/payment/setup-intent`,

  // Shipping
  shippingMethods: `${API_STOREFRONT}/shipping/methods`,
  shippingCalculate: `${API_STOREFRONT}/shipping/calculate`,
  shippingRates: `${API_STOREFRONT}/shipping/rates`,

  // Inventory
  availability: (variantId: string) => `${API_STOREFRONT}/availability/${variantId}`,
  cartReserve: `${API_STOREFRONT}/cart/reserve`,
  cartReserveById: (id: string) => `${API_STOREFRONT}/cart/reserve/${id}`,
  cartReserveStatus: `${API_STOREFRONT}/cart/reserve/status`,

  // Profile
  profiles: `${API_STORE}/profiles`,
  addresses: `${API_STORE}/profiles/addresses`,
  addressById: (id: string) => `${API_STORE}/profiles/addresses/${id}`,
  addressDefault: (id: string) => `${API_STORE}/profiles/addresses/${id}/default`,
  wishlists: `${API_STORE}/profiles/wishlists`,
  wishlistById: (id: string) => `${API_STORE}/profiles/wishlists/${id}`,
  wishlistItems: (id: string) => `${API_STORE}/profiles/wishlists/${id}/items`,
  wishlistItem: (listId: string, itemId: string) => `${API_STORE}/profiles/wishlists/${listId}/items/${itemId}`,
  notificationPreferences: `${API_STORE}/profiles/notification-preferences`,

  // Location
  countries: `${API_STORE}/locations/countries`,
  countryByIdOrIso: (idOrIso: string) => `${API_STORE}/locations/countries/${idOrIso}`,
  states: `${API_STORE}/locations/states`,
  stateByIdOrIso: (idOrIso: string) => `${API_STORE}/locations/states/${idOrIso}`,
} as const
```

- [ ] **Step 4: Write shared/api barrel index**

Write `app/Store/src/shared/api/index.ts`:
```ts
export { createApiClient, getApiClient, resetApiClient } from './axios'
export { get, post, put, patch, del, delWithBody, getBlob, setBaseUrl, setAuthToken, HttpError } from './client'
export { getPaged } from './paged'
export type { PagedRequestOptions } from './paged'
export { setNotifyToast } from './notify'
```

- [ ] **Step 5: Write shared composables from Admin**

```bash
cp app/Admin/src/shared/composables/usePagedQuery.ts app/Store/src/shared/composables/usePagedQuery.ts
cp app/Admin/src/shared/composables/useNotify.ts app/Store/src/shared/composables/useNotify.ts
cp app/Admin/src/shared/composables/useApiErrorHandler.ts app/Store/src/shared/composables/useApiErrorHandler.ts
```

- [ ] **Step 6: Write utility files**

Write `app/Store/src/shared/utils/currency.ts`:
```ts
export function formatVnd(amount: number): string {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount)
}
```

Write `app/Store/src/shared/utils/imageUrl.ts`:
```ts
import { ENDPOINTS } from '@/shared/constants/api'
import { env } from '@config/env'

export function getImageUrl(imageId: string): string {
  return `${env.apiUrl}/${ENDPOINTS.images}/${imageId}`
}
```

Write `app/Store/src/shared/utils/postLoginRedirect.ts`:
```ts
export function validateRedirect(path: string | null): string {
  if (!path) return '/'
  // Only allow same-origin relative paths (no //, no http:)
  if (path.startsWith('//') || path.includes('://')) return '/'
  return path.startsWith('/') ? path : '/'
}
```

- [ ] **Step 7: Verify type-check**

```bash
cd app/Store && pnpm run type-check
```

- [ ] **Step 8: Write unit tests for paged.ts**

Write `app/Store/src/shared/api/__tests__/paged.spec.ts`:
```ts
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { getPaged } from '@/shared/api/paged'
import * as client from '@/shared/api/client'

vi.mock('@/shared/api/client', () => ({
  get: vi.fn(),
  HttpError: class HttpError extends Error {
    statusCode: number
    errors: Array<{ code: string; message: string; type: number }>
    constructor(statusCode: number, errors: Array<{ code: string; message: string; type: number }>) {
      super(errors[0]?.message ?? 'HTTP Error')
      this.statusCode = statusCode
      this.errors = errors
    }
  },
}))

describe('getPaged', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('returns paged result on success', async () => {
    const mockResult = {
      isSuccess: true,
      statusCode: 200,
      items: [{ id: '1' }],
      page: 1,
      pageSize: 20,
      totalCount: 1,
      totalPages: 1,
    }
    vi.mocked(client.get).mockResolvedValue(mockResult)

    const result = await getPaged('/api/storefront/products', {
      pageNumber: 1,
      pageSize: 20,
    })

    expect(result.isSuccess).toBe(true)
    expect(result.items).toEqual([{ id: '1' }])
  })

  it('returns paged failure on HttpError', async () => {
    const httpError = new client.HttpError(500, [{ code: 'Server.Error', message: 'Boom', type: 500 }])
    vi.mocked(client.get).mockRejectedValue(httpError)

    const result = await getPaged('/api/test', { pageNumber: 1, pageSize: 20 })

    expect(result.isSuccess).toBe(false)
    expect(result.errors[0]?.code).toBe('Server.Error')
  })
})
```

- [ ] **Step 9: Run unit tests**

```bash
cd app/Store && pnpm run test:unit -- --run
```
Expected: all tests pass.

- [ ] **Step 10: Commit**

```bash
git add app/Store/src/shared/
git commit -m "feat(store): port shared API layer, types, composables from Admin"
```

---

## Phase 2 — App Shell (Router, Layouts, Auth)

### Task 2.1: Create layout components

**Files:**
- Create: `app/Store/src/app/layouts/DefaultLayout.vue`
- Create: `app/Store/src/app/layouts/AuthLayout.vue`
- Create: `app/Store/src/app/layouts/AccountLayout.vue`
- Create: `app/Store/src/app/components/layout/AppHeader.vue`
- Create: `app/Store/src/app/components/layout/AppFooter.vue`
- Create: `app/Store/src/app/components/layout/MobileNav.vue`
- Create: `app/Store/src/app/composables/useCurrency.ts`

- [ ] **Step 1: Write DefaultLayout.vue**

Write `app/Store/src/app/layouts/DefaultLayout.vue`:
```vue
<script setup lang="ts">
import AppHeader from '@/app/components/layout/AppHeader.vue'
import AppFooter from '@/app/components/layout/AppFooter.vue'
</script>
<template>
  <!-- Section: Page Shell -->
  <div class="min-h-screen flex flex-col bg-gray-50">
    <AppHeader />
    <!-- Section: Main Content -->
    <main class="flex-1">
      <router-view />
    </main>
    <AppFooter />
  </div>
</template>
```

- [ ] **Step 2: Write AuthLayout.vue**

Write `app/Store/src/app/layouts/AuthLayout.vue`:
```vue
<template>
  <!-- Section: Auth Shell -->
  <div class="min-h-screen flex items-center justify-center bg-gray-50 px-4">
    <!-- Section: Auth Card -->
    <div class="w-full max-w-md">
      <div class="text-center mb-8">
        <h1 class="text-3xl font-bold text-gray-900">ReSys.Shop</h1>
      </div>
      <router-view />
    </div>
  </div>
</template>
```

- [ ] **Step 3: Write AccountLayout.vue**

Write `app/Store/src/app/layouts/AccountLayout.vue`:
```vue
<script setup lang="ts">
import AppHeader from '@/app/components/layout/AppHeader.vue'
import AppFooter from '@/app/components/layout/AppFooter.vue'
import { useRoute } from 'vue-router'

const route = useRoute()

const navItems = [
  { label: 'Orders', to: '/account/orders', icon: 'pi pi-shopping-bag' },
  { label: 'Addresses', to: '/account/addresses', icon: 'pi pi-map-marker' },
  { label: 'Profile', to: '/account/profile', icon: 'pi pi-user' },
  { label: 'Sessions', to: '/account/sessions', icon: 'pi pi-shield' },
  { label: 'Wishlists', to: '/account/wishlists', icon: 'pi pi-heart' },
  { label: 'Notifications', to: '/account/notifications', icon: 'pi pi-bell' },
]
</script>
<template>
  <!-- Section: Account Shell -->
  <div class="min-h-screen flex flex-col bg-gray-50">
    <AppHeader />
    <!-- Section: Account Body -->
    <div class="flex-1 max-w-7xl mx-auto w-full px-4 sm:px-6 lg:px-8 py-8">
      <div class="flex gap-8">
        <!-- Section: Sidebar Navigation -->
        <nav class="w-56 shrink-0 hidden md:block">
          <ul class="space-y-1">
            <li v-for="item in navItems" :key="item.to">
              <router-link
                :to="item.to"
                class="flex items-center gap-3 px-4 py-2 rounded-lg text-sm font-medium transition-colors"
                :class="route.path === item.to
                  ? 'bg-gray-900 text-white'
                  : 'text-gray-600 hover:bg-gray-100'"
              >
                <i :class="item.icon" />
                {{ item.label }}
              </router-link>
            </li>
          </ul>
        </nav>
        <!-- Section: Content Area -->
        <div class="flex-1 min-w-0">
          <router-view />
        </div>
      </div>
    </div>
    <AppFooter />
  </div>
</template>
```

- [ ] **Step 4: Write AppHeader.vue**

Write `app/Store/src/app/components/layout/AppHeader.vue`:
```vue
<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/features/identity/stores/authStore'
import { useCartStore } from '@/features/ordering/stores/cartStore'

const router = useRouter()
const auth = useAuthStore()
const cart = useCartStore()

const searchQuery = ref('')
const mobileMenuOpen = ref(false)

// Trigger: Execute keyword search
function onSearch(): void {
  if (searchQuery.value.trim()) {
    router.push({ path: '/shop', query: { search: searchQuery.value } })
  }
}
</script>
<template>
  <!-- Section: Header Bar -->
  <header class="bg-white border-b border-gray-200 sticky top-0 z-50">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="flex items-center justify-between h-16 gap-4">
        <!-- Section: Logo -->
        <router-link to="/" class="text-xl font-bold text-gray-900 shrink-0">
          ReSys.Shop
        </router-link>

        <!-- Section: Search Bar (desktop) -->
        <form class="hidden md:flex flex-1 max-w-lg" @submit.prevent="onSearch">
          <span class="p-input-icon-left w-full">
            <i class="pi pi-search" />
            <InputText
              v-model="searchQuery"
              placeholder="Search products..."
              class="w-full"
            />
          </span>
        </form>

        <!-- Section: Header Actions -->
        <div class="flex items-center gap-3">
          <!-- Cart Icon -->
          <router-link
            to="/cart"
            class="relative p-2 text-gray-600 hover:text-gray-900 transition-colors"
          >
            <i class="pi pi-shopping-cart text-xl" />
            <span
              v-if="cart.itemCount > 0"
              class="absolute -top-1 -right-1 bg-red-500 text-white text-xs rounded-full w-5 h-5 flex items-center justify-center"
            >
              {{ cart.itemCount }}
            </span>
          </router-link>

          <!-- User Menu / Login -->
          <template v-if="auth.isAuthenticated">
            <router-link
              to="/account/orders"
              class="hidden md:flex items-center gap-2 text-sm text-gray-600 hover:text-gray-900"
            >
              <i class="pi pi-user" />
              {{ auth.user?.userName ?? 'Account' }}
            </router-link>
            <Button
              label="Logout"
              size="small"
              severity="secondary"
              @click="auth.logout()"
              class="hidden md:inline-flex"
            />
          </template>
          <template v-else>
            <router-link to="/login">
              <Button label="Sign In" size="small" severity="secondary" />
            </router-link>
          </template>

          <!-- Mobile Menu Toggle -->
          <Button
            icon="pi pi-bars"
            severity="secondary"
            text
            class="md:hidden"
            @click="mobileMenuOpen = !mobileMenuOpen"
          />
        </div>
      </div>
    </div>

    <!-- Section: Mobile Navigation -->
    <MobileNav v-if="mobileMenuOpen" @close="mobileMenuOpen = false" />
  </header>
</template>
```

- [ ] **Step 5: Write AppFooter.vue**

Write `app/Store/src/app/components/layout/AppFooter.vue`:
```vue
<template>
  <!-- Section: Footer -->
  <footer class="bg-white border-t border-gray-200 mt-auto">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div class="flex flex-col md:flex-row justify-between items-center gap-4 text-sm text-gray-500">
        <p>&copy; {{ new Date().getFullYear() }} ReSys.Shop. All rights reserved.</p>
        <nav class="flex gap-6">
          <router-link to="/terms" class="hover:text-gray-700">Terms</router-link>
          <router-link to="/privacy" class="hover:text-gray-700">Privacy</router-link>
        </nav>
      </div>
    </div>
  </footer>
</template>
```

- [ ] **Step 6: Write MobileNav.vue (placeholder)**

Write `app/Store/src/app/components/layout/MobileNav.vue`:
```vue
<script setup lang="ts">
defineEmits<{ close: [] }>()
</script>
<template>
  <!-- Section: Mobile Slide-out Nav -->
  <div class="fixed inset-0 z-50 md:hidden">
    <div class="absolute inset-0 bg-black/50" @click="$emit('close')" />
    <nav class="absolute top-0 right-0 bottom-0 w-64 bg-white shadow-lg p-6">
      <div class="flex justify-between items-center mb-6">
        <span class="font-semibold">Menu</span>
        <Button icon="pi pi-times" text severity="secondary" @click="$emit('close')" />
      </div>
      <ul class="space-y-3">
        <li><router-link to="/shop" class="text-gray-700 hover:text-gray-900" @click="$emit('close')">Shop</router-link></li>
        <li><router-link to="/cart" class="text-gray-700 hover:text-gray-900" @click="$emit('close')">Cart</router-link></li>
        <li><router-link to="/account/orders" class="text-gray-700 hover:text-gray-900" @click="$emit('close')">Orders</router-link></li>
      </ul>
    </nav>
  </div>
</template>
```

- [ ] **Step 7: Verify type-check**

```bash
cd app/Store && pnpm run type-check
```
Expected: errors about missing `authStore`, `cartStore`, `useCartStore` — expected, created in later phases. Comment out the imports temporarily to verify layout structure compiles.

- [ ] **Step 8: Commit**

```bash
git add app/Store/src/app/layouts/ app/Store/src/app/components/layout/
git commit -m "feat(store): add layout components (Default, Auth, Account, Header, Footer, MobileNav)"
```

---

### Task 2.2: Implement auth store + token service

**Files:**
- Create: `app/Store/src/features/identity/types/auth.ts`
- Create: `app/Store/src/features/identity/services/tokenService.ts`
- Create: `app/Store/src/features/identity/services/authApi.ts`
- Create: `app/Store/src/features/identity/stores/authStore.ts`
- Create: `app/Store/src/features/identity/routes/index.ts`

- [ ] **Step 1: Write auth types**

Write `app/Store/src/features/identity/types/auth.ts`:
```ts
export interface TokenPair {
  accessToken: string
  accessTokenExpiresIn: number
  refreshToken: string
  refreshTokenExpiresIn: number
}

export interface AuthUser {
  userId: string
  userName: string
  email: string
  roles: string[]
  permissions: string[]
  isAuthenticated: boolean
}

export interface LoginRequest {
  credential: string
  password: string
}

export interface RegisterRequest {
  fullName: string
  email: string
  password: string
}

export interface SessionInfo {
  id: string
  deviceName: string
  ipAddress: string
  lastActivityAt: string
  isCurrent: boolean
}
```

- [ ] **Step 2: Write tokenService.ts**

Write `app/Store/src/features/identity/services/tokenService.ts`:
```ts
import { STORAGE_KEYS } from '@/shared/constants/storage'
import type { TokenPair } from '../types/auth'

export function getAccessToken(): string | null {
  try { return localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN) } catch { return null }
}

export function getRefreshToken(): string | null {
  try { return localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN) } catch { return null }
}

export function setTokens(pair: TokenPair): void {
  try {
    localStorage.setItem(STORAGE_KEYS.ACCESS_TOKEN, pair.accessToken)
    localStorage.setItem(`${STORAGE_KEYS.ACCESS_TOKEN}_expires_at`, String(pair.accessTokenExpiresIn))
    localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, pair.refreshToken)
    localStorage.setItem(`${STORAGE_KEYS.REFRESH_TOKEN}_expires_at`, String(pair.refreshTokenExpiresIn))
  } catch { /* localStorage unavailable */ }
}

export function clearTokens(): void {
  try {
    localStorage.removeItem(STORAGE_KEYS.ACCESS_TOKEN)
    localStorage.removeItem(`${STORAGE_KEYS.ACCESS_TOKEN}_expires_at`)
    localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN)
    localStorage.removeItem(`${STORAGE_KEYS.REFRESH_TOKEN}_expires_at`)
  } catch { /* ignore */ }
}

export function hasValidAccessToken(): boolean {
  try {
    const token = localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN)
    if (!token) return false
    const expiresAt = localStorage.getItem(`${STORAGE_KEYS.ACCESS_TOKEN}_expires_at`)
    if (!expiresAt) return true
    return Number(expiresAt) > Date.now() / 1000
  } catch { return false }
}
```

- [ ] **Step 3: Write authApi.ts**

Write `app/Store/src/features/identity/services/authApi.ts`:
```ts
import { post, get } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { Result } from '@/shared/types/result'
import type { TokenPair, LoginRequest, RegisterRequest, AuthUser, SessionInfo } from '../types/auth'

export async function login(req: LoginRequest): Promise<Result<TokenPair>> {
  return post<TokenPair>(ENDPOINTS.authLoginPassword, req)
}

export async function register(req: RegisterRequest): Promise<Result<unknown>> {
  return post(ENDPOINTS.authRegister, req)
}

export async function logout(req?: { revokeAll?: boolean }): Promise<void> {
  await post(ENDPOINTS.authLogout, req)
}

export async function getSession(): Promise<Result<AuthUser>> {
  return get<AuthUser>(ENDPOINTS.sessions)
}

export async function getLoginProviders(): Promise<Result<Array<{ name: string; url: string }>>> {
  return get(ENDPOINTS.authLoginProviders)
}

export async function forgotPassword(email: string): Promise<Result<unknown>> {
  return post(ENDPOINTS.passwordsForgot, { email })
}

export async function resetPassword(token: string, newPassword: string): Promise<Result<unknown>> {
  return post(ENDPOINTS.passwordsReset, { token, newPassword })
}
```

- [ ] **Step 4: Write authStore.ts**

Write `app/Store/src/features/identity/stores/authStore.ts`:
```ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { AuthUser } from '../types/auth'
import * as authApi from '../services/authApi'
import * as tokenService from '../services/tokenService'
import { setTokenGetter } from '@/shared/api/interceptors/auth'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<AuthUser | null>(null)
  const status = ref<'idle' | 'loading' | 'authenticated' | 'error'>('idle')
  const error = ref<string | null>(null)

  const isAuthenticated = computed(() => status.value === 'authenticated' && user.value !== null)

  // Init: Validate token and hydrate session (called once by router guard)
  async function init(): Promise<void> {
    if (!tokenService.hasValidAccessToken()) {
      status.value = 'idle'
      return
    }
    try {
      const result = await authApi.getSession()
      if (result.isSuccess) {
        user.value = {
          userId: result.value.id,
          userName: result.value.userName,
          email: result.value.email,
          roles: result.value.roles,
          permissions: result.value.permissions,
          isAuthenticated: true,
        }
        status.value = 'authenticated'
      } else {
        tokenService.clearTokens()
        status.value = 'idle'
      }
    } catch {
      tokenService.clearTokens()
      status.value = 'idle'
    }
  }

  // Login: Authenticate with password
  async function login(credential: string, password: string): Promise<boolean> {
    status.value = 'loading'
    error.value = null
    const result = await authApi.login({ credential, password })
    if (result.isSuccess) {
      tokenService.setTokens(result.value)
      setTokenGetter(tokenService.getAccessToken)
      const sessionResult = await authApi.getSession()
      if (sessionResult.isSuccess) {
        user.value = {
          userId: sessionResult.value.id,
          userName: sessionResult.value.userName,
          email: sessionResult.value.email,
          roles: sessionResult.value.roles,
          permissions: sessionResult.value.permissions,
          isAuthenticated: true,
        }
        status.value = 'authenticated'
        return true
      }
    }
    status.value = 'error'
    error.value = result.message ?? result.errors[0]?.message ?? 'Login failed'
    return false
  }

  // Login: Redirect to Google OAuth
  async function loginWithGoogle(): Promise<void> {
    const result = await authApi.getLoginProviders()
    if (result.isSuccess) {
      const provider = result.value.find(p => p.name.toLowerCase() === 'google')
      if (provider) {
        window.location.href = provider.url
      }
    }
  }

  // Logout: Revoke tokens and clear state
  async function logout(revokeAll?: boolean): Promise<void> {
    try { await authApi.logout({ revokeAll }) } catch { /* fire-and-forget */ }
    tokenService.clearTokens()
    user.value = null
    status.value = 'idle'
    error.value = null
  }

  return { user, status, error, isAuthenticated, init, login, loginWithGoogle, logout }
})
```

- [ ] **Step 5: Write identity routes**

Write `app/Store/src/features/identity/routes/index.ts`:
```ts
import type { RouteRecordRaw } from 'vue-router'

export const identityRoutes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'login',
    component: () => import('../views/LoginView.vue'),
    meta: { guestOnly: true, title: 'Sign In' },
  },
  {
    path: '/register',
    name: 'register',
    component: () => import('../views/RegisterView.vue'),
    meta: { guestOnly: true, title: 'Create Account' },
  },
  {
    path: '/forgot-password',
    name: 'forgot-password',
    component: () => import('../views/ForgotPasswordView.vue'),
    meta: { guestOnly: true, title: 'Forgot Password' },
  },
  {
    path: '/reset-password',
    name: 'reset-password',
    component: () => import('../views/ResetPasswordView.vue'),
    meta: { guestOnly: true, title: 'Reset Password' },
  },
  {
    path: '/account/sessions',
    name: 'sessions',
    component: () => import('../views/SessionsView.vue'),
    meta: { requiresAuth: true, title: 'Sessions' },
  },
]
```

- [ ] **Step 6: Update guards.ts to wire auth**

Edit `app/Store/src/app/router/guards.ts`:
```ts
import type { Router } from 'vue-router'
import { useAuthStore } from '@/features/identity/stores/authStore'
import { validateRedirect } from '@/shared/utils/postLoginRedirect'

let isInitialized = false

export function setupGuards(router: Router): void {
  router.beforeEach(async (to) => {
    const store = useAuthStore()

    if (!isInitialized) {
      await store.init()
      isInitialized = true
    }

    if (to.meta.guestOnly && store.isAuthenticated) {
      return { path: '/' }
    }

    if (to.meta.requiresAuth && !store.isAuthenticated) {
      return { name: 'login', query: { redirect: to.fullPath } }
    }
  })

  router.afterEach((to) => {
    if (to.meta.title) {
      document.title = `${to.meta.title} | ReSys.Shop`
    }
  })
}
```

- [ ] **Step 7: Re-enable layout imports**

Edit `AppHeader.vue` — the authStore and cartStore imports should now resolve. For cartStore, create a minimal skeleton:

Write `app/Store/src/features/ordering/stores/cartStore.ts`:
```ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useCartStore = defineStore('cart', () => {
  const items = ref<Array<{ id: string; quantity: number }>>([])
  const itemCount = computed(() => items.value.reduce((sum, i) => sum + i.quantity, 0))

  return { items, itemCount }
})
```

- [ ] **Step 8: Verify type-check and lint**

```bash
cd app/Store && pnpm run type-check && pnpm run lint
```
Expected: 0 errors. Stub views may be missing but referenced — create minimal placeholder views:

```bash
# Create stub view files for routes that reference them
for view in LoginView RegisterView ForgotPasswordView ResetPasswordView SessionsView; do
  mkdir -p app/Store/src/features/identity/views
  echo '<template><div>'"$view"'</div></template>' > "app/Store/src/features/identity/views/${view}.vue"
done
```

- [ ] **Step 9: Write postLoginRedirect utils test**

Write `app/Store/src/shared/utils/__tests__/postLoginRedirect.spec.ts`:
```ts
import { describe, it, expect } from 'vitest'
import { validateRedirect } from '@/shared/utils/postLoginRedirect'

describe('validateRedirect', () => {
  it('returns / for null', () => { expect(validateRedirect(null)).toBe('/') })
  it('returns / for empty string', () => { expect(validateRedirect('')).toBe('/') })
  it('returns valid path unchanged', () => { expect(validateRedirect('/account/orders')).toBe('/account/orders') })
  it('strips double-slash attacks', () => { expect(validateRedirect('//evil.com')).toBe('/') })
  it('strips protocol attacks', () => { expect(validateRedirect('https://evil.com')).toBe('/') })
})
```

- [ ] **Step 10: Run tests**

```bash
cd app/Store && pnpm run test:unit -- --run
```

- [ ] **Step 11: Commit**

```bash
git add app/Store/src/features/identity/ app/Store/src/features/ordering/stores/cartStore.ts app/Store/src/app/router/guards.ts app/Store/src/shared/utils/
git commit -m "feat(store): implement auth store, token service, router guards"
```

---

## Phase 3 — Catalog Module

### Task 3.1: Catalog types and services

**Files:**
- Create: `app/Store/src/features/catalog/types/product.ts`
- Create: `app/Store/src/features/catalog/types/taxon.ts`
- Create: `app/Store/src/features/catalog/types/searchByImage.ts`
- Create: `app/Store/src/features/catalog/types/optionType.ts`
- Create: `app/Store/src/features/catalog/services/productApi.ts`
- Create: `app/Store/src/features/catalog/services/taxonApi.ts`
- Create: `app/Store/src/features/catalog/services/optionTypeApi.ts`
- Create: `app/Store/src/features/catalog/services/searchByImageApi.ts`
- Create: `app/Store/src/features/catalog/stores/catalogStore.ts`

**Interfaces:**
- Produces: `StoreProductListItemResponse`, `StoreProductDetailResponse`, `StoreProductVariantResponse`, `StoreProductImageResponse`, `StoreTaxonomyTreeResponse`, `TaxonTreeNode`, `SearchByImageResponse`, `StoreOptionTypeResponse`, `productApi.*`, `taxonApi.*`, `optionTypeApi.*`, `searchByImageApi.*`, `catalogStore`

- [ ] **Step 1: Write product types**

Write `app/Store/src/features/catalog/types/product.ts`:
```ts
export interface StoreProductListItemResponse {
  id: string
  masterVariantId: string
  name: string
  status: string
  description: string | null
  slug: string
  minPrice: number | null
  currency: string | null
  thumbnailUrl: string | null
  thumbnailAlt: string | null
  styleCode: string | null
  seasonName: string | null
  materialComposition: string | null
  careInstructions: string | null
  fitNotes: string | null
  department: string | null
  genderTarget: string | null
  variantsCount: number
  availableOn: string | null
}

export interface StoreProductVariantResponse {
  id: string
  sku: string | null
  isMaster: boolean
  price: number | null
  currency: string | null
  optionValue1: { id: string; name: string; presentation: string | null } | null
  optionValue2: { id: string; name: string; presentation: string | null } | null
  images: StoreProductImageResponse[]
}

export interface StoreProductImageResponse {
  id: string
  url: string
  alt: string | null
  position: number
}

export interface StoreProductDetailResponse extends StoreProductListItemResponse {
  masterVariant: StoreProductVariantResponse | null
  variants: StoreProductVariantResponse[]
  images: StoreProductImageResponse[]
  taxons: StoreProductTaxonResponse[]
}

export interface StoreProductTaxonResponse {
  id: string
  name: string
  permalink: string
  depth: number
}

export interface AvailabilityAxisValue {
  id: string
  name: string
  presentation: string | null
}

export interface AvailabilityCell {
  variantId: string
  optionValue1Id: string
  optionValue2Id: string | null
  status: string
  price: number | null
  currency: string | null
}

export interface AvailabilityMatrixResponse {
  axes: Array<{
    name: string
    presentation: string | null
    values: AvailabilityAxisValue[]
  }>
  cells: AvailabilityCell[]
}
```

- [ ] **Step 2: Write remaining types**

Write `app/Store/src/features/catalog/types/taxon.ts`:
```ts
export interface TaxonTreeNode {
  id: string
  name: string
  presentation: string | null
  permalink: string
  depth: number
  hasChildren: boolean
  children: TaxonTreeNode[]
}

export interface StoreTaxonomyTreeResponse {
  id: string
  name: string
  presentation: string | null
  position: number
  nodes: TaxonTreeNode[]
}

export interface StoreTaxonListItemResponse {
  id: string
  name: string
  permalink: string
  depth: number
  taxonCount: number
  parentId: string | null
  taxonomyId: string
  position: number
  slug: string
  imageUrl: string | null
}
```

Write `app/Store/src/features/catalog/types/searchByImage.ts`:
```ts
export interface SearchByImageResponse {
  variantId: string
  productId: string
  productName: string
  sku: string
  price: number
  imageUrl: string | null
}
```

Write `app/Store/src/features/catalog/types/optionType.ts`:
```ts
export interface StoreOptionValueResponse {
  id: string
  name: string
  presentation: string | null
  position: number
  optionTypeId: string
}

export interface StoreOptionTypeResponse {
  id: string
  name: string
  presentation: string | null
  position: number
  filterable: boolean
  values: StoreOptionValueResponse[]
}
```

- [ ] **Step 3: Write catalog services**

Write `app/Store/src/features/catalog/services/productApi.ts`:
```ts
import { get } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types/result'
import type { QueryingParameters } from '@/shared/types/querying'
import { getPaged } from '@/shared/api/paged'
import type {
  StoreProductListItemResponse,
  StoreProductDetailResponse,
  AvailabilityMatrixResponse,
} from '../types/product'

export function getPagedProducts(params: QueryingParameters): Promise<PagedResult<StoreProductListItemResponse>> {
  return getPaged<StoreProductListItemResponse>(ENDPOINTS.products, params)
}

export function getProductBySlug(slug: string): Promise<Result<StoreProductDetailResponse>> {
  return get<Result<StoreProductDetailResponse>>(ENDPOINTS.productBySlug(slug))
}

export function getAvailability(productId: string): Promise<Result<AvailabilityMatrixResponse>> {
  return get<Result<AvailabilityMatrixResponse>>(`${ENDPOINTS.productAvailability}?productId=${productId}`)
}

export function getSimilarProducts(productId: string, topK = 20): Promise<PagedResult<StoreProductListItemResponse>> {
  return getPaged<StoreProductListItemResponse>(
    `${ENDPOINTS.productSimilar}?productId=${productId}&topK=${topK}`,
    { pageNumber: 1, pageSize: topK },
  )
}

export function getRelatedProducts(productId: string, params: QueryingParameters): Promise<PagedResult<StoreProductListItemResponse>> {
  return getPaged<StoreProductListItemResponse>(
    `${ENDPOINTS.productRelated}?productId=${productId}`,
    params,
  )
}
```

Write `app/Store/src/features/catalog/services/taxonApi.ts`:
```ts
import { get } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types/result'
import type { QueryingParameters } from '@/shared/types/querying'
import { getPaged } from '@/shared/api/paged'
import type { StoreTaxonomyTreeResponse, StoreTaxonListItemResponse } from '../types/taxon'
import type { StoreProductListItemResponse } from '../types/product'

export function getTaxonomyTree(id: string): Promise<Result<StoreTaxonomyTreeResponse>> {
  return get<Result<StoreTaxonomyTreeResponse>>(ENDPOINTS.taxonomyById(id))
}

export function getTaxons(params: QueryingParameters): Promise<PagedResult<StoreTaxonListItemResponse>> {
  return getPaged<StoreTaxonListItemResponse>(ENDPOINTS.taxons, params)
}

export function getTaxonProducts(taxonId: string, params: QueryingParameters): Promise<PagedResult<StoreProductListItemResponse>> {
  return getPaged<StoreProductListItemResponse>(
    `${ENDPOINTS.taxonProducts}?taxonId=${taxonId}`,
    params,
  )
}
```

Write `app/Store/src/features/catalog/services/optionTypeApi.ts`:
```ts
import { getPaged } from '@/shared/api/paged'
import { ENDPOINTS } from '@/shared/constants/api'
import type { PagedResult } from '@/shared/types/result'
import type { QueryingParameters } from '@/shared/types/querying'
import type { StoreOptionTypeResponse } from '../types/optionType'

export function getOptionTypes(params: QueryingParameters): Promise<PagedResult<StoreOptionTypeResponse>> {
  return getPaged<StoreOptionTypeResponse>(ENDPOINTS.optionTypes, params)
}
```

Write `app/Store/src/features/catalog/services/searchByImageApi.ts`:
```ts
import { post } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { PagedResult } from '@/shared/types/result'
import type { SearchByImageResponse } from '../types/searchByImage'

export function searchByImage(image: File, topK = 20, model?: string): Promise<PagedResult<SearchByImageResponse>> {
  const formData = new FormData()
  formData.append('image', image)
  if (topK) formData.append('TopK', String(topK))
  if (model) formData.append('Model', model)
  return post<PagedResult<SearchByImageResponse>>(ENDPOINTS.searchByImage, formData)
}
```

- [ ] **Step 4: Write catalogStore**

Write `app/Store/src/features/catalog/stores/catalogStore.ts`:
```ts
import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useCatalogStore = defineStore('catalog', () => {
  const searchQuery = ref('')
  const selectedTaxonId = ref<string | null>(null)
  const selectedOptionValueIds = ref<string[]>([])
  const minPrice = ref<number | null>(null)
  const maxPrice = ref<number | null>(null)
  const sortField = ref<string | null>(null)
  const sortOrder = ref<number>(1)

  function setSearch(q: string): void {
    searchQuery.value = q
  }

  function setTaxon(id: string | null): void {
    selectedTaxonId.value = id
  }

  function toggleOptionValue(id: string): void {
    const idx = selectedOptionValueIds.value.indexOf(id)
    if (idx >= 0) {
      selectedOptionValueIds.value.splice(idx, 1)
    } else {
      selectedOptionValueIds.value.push(id)
    }
  }

  function setPriceRange(min: number | null, max: number | null): void {
    minPrice.value = min
    maxPrice.value = max
  }

  function clearFilters(): void {
    selectedOptionValueIds.value = []
    minPrice.value = null
    maxPrice.value = null
    selectedTaxonId.value = null
    searchQuery.value = ''
  }

  return {
    searchQuery, selectedTaxonId, selectedOptionValueIds, minPrice, maxPrice, sortField, sortOrder,
    setSearch, setTaxon, toggleOptionValue, setPriceRange, clearFilters,
  }
})
```

- [ ] **Step 5: Verify type-check**

```bash
cd app/Store && pnpm run type-check
```

- [ ] **Step 6: Commit**

```bash
git add app/Store/src/features/catalog/
git commit -m "feat(store): add catalog types, services, and store"
```

---

### Task 3.2: Catalog views (Home, Shop, Product Detail)

**Files:**
- Create: `app/Store/src/features/catalog/views/HomeView.vue`
- Create: `app/Store/src/features/catalog/views/ShopView.vue`
- Create: `app/Store/src/features/catalog/views/ProductDetailView.vue`
- Create: `app/Store/src/features/catalog/views/CollectionsView.vue`
- Create: `app/Store/src/features/catalog/views/VisualSearchView.vue`
- Create: `app/Store/src/features/catalog/components/ProductCard.vue`
- Create: `app/Store/src/features/catalog/components/ProductGrid.vue`
- Create: `app/Store/src/features/catalog/components/ProductGallery.vue`
- Create: `app/Store/src/features/catalog/components/ProductOptions.vue`
- Create: `app/Store/src/features/catalog/components/CategoryTree.vue`
- Create: `app/Store/src/features/catalog/components/FilterSidebar.vue`
- Create: `app/Store/src/features/catalog/components/VisualSearchDropzone.vue`
- Create: `app/Store/src/features/catalog/components/SimilarityBadge.vue`
- Create: `app/Store/src/features/catalog/components/SimilarProductsRow.vue`
- Create: `app/Store/src/features/catalog/routes/index.ts`

This is a large task group. Write each view sequentially, verifying type-check after each.

- [ ] **Step 1: Write ProductCard.vue**

```vue
<script setup lang="ts">
import type { StoreProductListItemResponse } from '../types/product'
import { formatVnd } from '@/shared/utils/currency'
import { getImageUrl } from '@/shared/utils/imageUrl'

const props = defineProps<{ product: StoreProductListItemResponse }>()
const emit = defineEmits<{ addToCart: [productId: string] }>()

// Map: Format price for display
function displayPrice(): string {
  return props.product.minPrice != null ? formatVnd(props.product.minPrice) : 'Contact'
}
</script>
<template>
  <!-- Section: Product Card -->
  <div class="group bg-white rounded-xl border border-gray-200 overflow-hidden shadow-sm hover:shadow-md transition-shadow">
    <!-- Section: Thumbnail -->
    <router-link :to="`/products/${product.slug}`" class="block aspect-square bg-gray-100 relative overflow-hidden">
      <img
        v-if="product.thumbnailUrl"
        :src="product.thumbnailUrl"
        :alt="product.thumbnailAlt ?? product.name"
        class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-200"
      />
      <div v-else class="w-full h-full flex items-center justify-center text-gray-400">
        <i class="pi pi-image text-4xl" />
      </div>
      <!-- Section: Quick Add Overlay -->
      <div class="absolute inset-x-0 bottom-0 p-3 bg-gradient-to-t from-black/60 to-transparent opacity-0 group-hover:opacity-100 transition-opacity">
        <Button
          label="Quick Add"
          icon="pi pi-plus"
          size="small"
          class="w-full"
          @click.prevent="emit('addToCart', product.id)"
        />
      </div>
    </router-link>
    <!-- Section: Product Info -->
    <div class="p-4">
      <router-link :to="`/products/${product.slug}`" class="text-sm font-medium text-gray-900 line-clamp-2 hover:text-gray-600">
        {{ product.name }}
      </router-link>
      <p class="mt-1 text-lg font-bold text-gray-900">{{ displayPrice() }}</p>
    </div>
  </div>
</template>
```

- [ ] **Step 2: Write ProductGrid.vue**

```vue
<script setup lang="ts">
import type { StoreProductListItemResponse } from '../types/product'
import ProductCard from './ProductCard.vue'
import SkeletonGrid from '@/shared/components/SkeletonGrid.vue'

defineProps<{
  products: StoreProductListItemResponse[]
  loading: boolean
  error: string | null
}>()
const emit = defineEmits<{ addToCart: [id: string]; reload: [] }>()
</script>
<template>
  <!-- Section: Product Grid -->
  <div>
    <!-- Section: Error State -->
    <Message v-if="error" severity="error" :closable="false">
      {{ error }}
      <Button label="Reload" severity="secondary" size="small" class="ml-3" @click="emit('reload')" />
    </Message>

    <!-- Section: Loading State -->
    <SkeletonGrid v-if="loading" :count="8" />

    <!-- Section: Empty State -->
    <div v-else-if="!loading && products.length === 0" class="text-center py-16">
      <i class="pi pi-search text-4xl text-gray-300 mb-4" />
      <p class="text-gray-500">No products match your filters.</p>
    </div>

    <!-- Section: Grid -->
    <div v-else class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
      <ProductCard
        v-for="product in products"
        :key="product.id"
        :product="product"
        @add-to-cart="(id) => emit('addToCart', id)"
      />
    </div>
  </div>
</template>
```

- [ ] **Step 3: Write CategoryTree.vue**

```vue
<script setup lang="ts">
import type { TaxonTreeNode } from '../types/taxon'

defineProps<{ nodes: TaxonTreeNode[] }>()
const emit = defineEmits<{ select: [taxonId: string] }>()

function toggle(node: TaxonTreeNode & { _expanded?: boolean }): void {
  ;(node as Record<string, unknown>)._expanded = !(node as Record<string, unknown>)._expanded
}
</script>
<template>
  <!-- Section: Category Tree -->
  <ul class="space-y-1">
    <li v-for="node in nodes" :key="node.id">
      <button
        class="flex items-center gap-2 w-full text-left px-2 py-1.5 rounded text-sm hover:bg-gray-100 transition-colors"
        :class="{ 'font-semibold text-gray-900': node.depth === 0 }"
        @click="emit('select', node.id)"
      >
        <i
          v-if="node.hasChildren"
          class="pi text-xs text-gray-400 transition-transform"
          :class="(node as any)._expanded ? 'pi-chevron-down' : 'pi-chevron-right'"
          @click.stop="toggle(node as any)"
        />
        <span v-else class="w-3" />
        {{ node.presentation ?? node.name }}
      </button>
      <CategoryTree
        v-if="node.hasChildren && (node as any)._expanded"
        :nodes="node.children"
        class="ml-4"
        @select="(id) => emit('select', id)"
      />
    </li>
  </ul>
</template>
```

- [ ] **Step 4: Write ShopView.vue**

```vue
<script setup lang="ts">
// Call: Fetch products, taxons, and option types on mount
import { ref, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { useCatalogStore } from '../stores/catalogStore'
import { getPagedProducts } from '../services/productApi'
import { getTaxonomyTree } from '../services/taxonApi'
import { getOptionTypes } from '../services/optionTypeApi'
import ProductGrid from '../components/ProductGrid.vue'
import CategoryTree from '../components/CategoryTree.vue'
import FilterSidebar from '../components/FilterSidebar.vue'
import type { StoreProductListItemResponse } from '../types/product'
import type { TaxonTreeNode, StoreTaxonomyTreeResponse } from '../types/taxon'
import type { StoreOptionTypeResponse } from '../types/optionType'
import type { Result } from '@/shared/types/result'

const route = useRoute()
const router = useRouter()
const catalog = useCatalogStore()

// Map: Build paged query from catalogStore state
const query = usePagedQuery<StoreProductListItemResponse>(
  () => {
    const params = new URLSearchParams()
    if (catalog.searchQuery) params.set('search', catalog.searchQuery)
    if (catalog.selectedTaxonId) params.append('taxonId', catalog.selectedTaxonId)
    catalog.selectedOptionValueIds.forEach(id => params.append('optionValueId', id))
    if (catalog.minPrice != null) params.set('minPrice', String(catalog.minPrice))
    if (catalog.maxPrice != null) params.set('maxPrice', String(catalog.maxPrice))
    const qs = params.toString()
    return qs ? `${import.meta.env.VITE_API_URL}/api/storefront/products?${qs}` : '/api/storefront/products'
  },
  { defaultPageSize: 20 },
)

// State: Taxonomy tree and filters
const taxonomyTree = ref<StoreTaxonomyTreeResponse | null>(null)
const optionTypes = ref<StoreOptionTypeResponse[]>([])
const treeLoading = ref(true)
const filtersLoading = ref(true)

// Trigger: Load taxonomy and filters
onMounted(async () => {
  // Load taxonomy tree (use a known taxonomy ID or fetch from config)
  const treeResult = await getTaxonomyTree('00000000-0000-0000-0000-000000000001') // default taxonomy
  if (treeResult.isSuccess) taxonomyTree.value = treeResult.value
  treeLoading.value = false

  const otResult = await getOptionTypes({ pageNumber: 1, pageSize: 50 })
  if (otResult.isSuccess) optionTypes.value = otResult.items
  filtersLoading.value = false
})

// Trigger: Sync URL query params on mount
watch(() => route.query.search, (val) => {
  if (typeof val === 'string') catalog.setSearch(val)
}, { immediate: true })
</script>
<template>
  <!-- Section: Shop Page -->
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <div class="flex gap-8">
      <!-- Section: Sidebar Filters -->
      <aside class="w-64 shrink-0 hidden lg:block space-y-6">
        <!-- Section: Category Tree -->
        <div v-if="treeLoading" class="space-y-2">
          <Skeleton width="80%" height="1rem" v-for="i in 5" :key="i" />
        </div>
        <CategoryTree
          v-else-if="taxonomyTree"
          :nodes="taxonomyTree.nodes"
          @select="(id) => { catalog.setTaxon(id); query.refresh() }"
        />

        <!-- Section: Option Filters -->
        <FilterSidebar
          v-if="!filtersLoading"
          :option-types="optionTypes"
          :selected-ids="catalog.selectedOptionValueIds"
          @toggle="(id) => { catalog.toggleOptionValue(id); query.refresh() }"
          @clear="catalog.clearFilters(); query.refresh()"
        />
      </aside>

      <!-- Section: Main Content -->
      <div class="flex-1 min-w-0">
        <!-- Section: Toolbar -->
        <div class="flex items-center justify-between mb-6">
          <p class="text-sm text-gray-500">{{ query.totalCount }} products</p>
          <Select
            :model-value="catalog.sortField"
            :options="[{ label: 'Newest', value: 'createdAtUtc desc' }, { label: 'Price: Low-High', value: 'minPrice asc' }, { label: 'Price: High-Low', value: 'minPrice desc' }]"
            option-label="label"
            option-value="value"
            placeholder="Sort by"
            class="w-48"
            @update:model-value="(val: string) => { catalog.sortField = val; query.refresh() }"
          />
        </div>

        <!-- Section: Product Grid -->
        <ProductGrid
          :products="query.items.value"
          :loading="query.loading.value"
          :error="query.error.value"
          @reload="query.refresh"
          @add-to-cart="(id) => { /* wired in Phase 4 */ }"
        />

        <!-- Section: Pagination -->
        <Paginator
          v-if="query.totalPages.value > 1"
          :rows="query.pageSize.value"
          :total-records="query.totalCount.value"
          :first="(query.page.value - 1) * query.pageSize.value"
          @page="(e: { page: number }) => query.setPage(e.page + 1)"
          class="mt-6"
        />
      </div>
    </div>
  </div>
</template>
```

- [ ] **Step 5: Write ProductDetailView.vue**

```vue
<script setup lang="ts">
// Call: Fetch product detail by slug from route params
import { ref, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { getProductBySlug, getSimilarProducts } from '../services/productApi'
import ProductGallery from '../components/ProductGallery.vue'
import ProductOptions from '../components/ProductOptions.vue'
import SimilarProductsRow from '../components/SimilarProductsRow.vue'
import type { StoreProductDetailResponse, StoreProductListItemResponse } from '../types/product'

const route = useRoute()
const product = ref<StoreProductDetailResponse | null>(null)
const similar = ref<StoreProductListItemResponse[]>([])
const loading = ref(true)
const error = ref<string | null>(null)
const selectedVariantId = ref<string | null>(null)
const quantity = ref(1)

// Trigger: Load product when slug changes
async function loadProduct(slug: string): Promise<void> {
  loading.value = true
  error.value = null
  const result = await getProductBySlug(slug)
  if (result.isSuccess) {
    product.value = result.value
    selectedVariantId.value = result.value.masterVariant?.id ?? null
    const simResult = await getSimilarProducts(result.value.id)
    if (simResult.isSuccess) similar.value = simResult.items
  } else {
    error.value = result.message ?? 'Product not found'
  }
  loading.value = false
}

watch(() => route.params.slug, (slug) => {
  if (typeof slug === 'string') loadProduct(slug)
}, { immediate: true })
</script>
<template>
  <!-- Section: Product Detail Page -->
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <!-- Section: Error State -->
    <div v-if="error" class="text-center py-16">
      <i class="pi pi-exclamation-circle text-4xl text-gray-300 mb-4" />
      <h2 class="text-xl font-semibold text-gray-900">{{ error }}</h2>
      <router-link to="/shop" class="text-primary hover:underline mt-2 inline-block">Browse products</router-link>
    </div>

    <!-- Section: Loading State -->
    <div v-else-if="loading" class="animate-pulse space-y-8">
      <div class="flex flex-col md:flex-row gap-8">
        <div class="w-full md:w-1/2 aspect-square bg-gray-200 rounded-xl" />
        <div class="w-full md:w-1/2 space-y-4">
          <div class="h-8 bg-gray-200 rounded w-3/4" />
          <div class="h-6 bg-gray-200 rounded w-1/4" />
          <div class="h-4 bg-gray-200 rounded w-full" />
          <div class="h-12 bg-gray-200 rounded w-1/3" />
        </div>
      </div>
    </div>

    <!-- Section: Product Content -->
    <template v-else-if="product">
      <div class="flex flex-col md:flex-row gap-8">
        <!-- Section: Image Gallery -->
        <div class="w-full md:w-1/2">
          <ProductGallery :images="product.images" :alt="product.name" />
        </div>

        <!-- Section: Product Info -->
        <div class="w-full md:w-1/2 space-y-6">
          <!-- Breadcrumb -->
          <nav class="flex items-center gap-2 text-sm text-gray-500">
            <router-link to="/" class="hover:text-gray-900">Home</router-link>
            <i class="pi pi-chevron-right text-xs" />
            <router-link to="/shop" class="hover:text-gray-900">Shop</router-link>
            <i class="pi pi-chevron-right text-xs" />
            <span class="text-gray-900">{{ product.name }}</span>
          </nav>

          <h1 class="text-2xl font-bold text-gray-900">{{ product.name }}</h1>

          <!-- Price -->
          <p v-if="product.minPrice" class="text-3xl font-bold text-gray-900">
            {{ new Intl.NumberFormat('vi-VN', { style: 'currency', currency: product.currency ?? 'VND' }).format(product.minPrice) }}
          </p>

          <!-- Fashion Metadata -->
          <div v-if="product.styleCode || product.materialComposition" class="flex flex-wrap gap-3 text-sm text-gray-500">
            <span v-if="product.styleCode" class="bg-gray-100 px-2 py-1 rounded">Style: {{ product.styleCode }}</span>
            <span v-if="product.seasonName" class="bg-gray-100 px-2 py-1 rounded">{{ product.seasonName }}</span>
            <span v-if="product.materialComposition" class="bg-gray-100 px-2 py-1 rounded">{{ product.materialComposition }}</span>
            <span v-if="product.department" class="bg-gray-100 px-2 py-1 rounded">{{ product.department }}</span>
            <span v-if="product.genderTarget" class="bg-gray-100 px-2 py-1 rounded">{{ product.genderTarget }}</span>
          </div>

          <!-- Variant Options -->
          <ProductOptions
            v-if="product.variants.length > 0"
            :variants="product.variants"
            :model-value="selectedVariantId"
            @update:model-value="(id: string) => selectedVariantId = id"
          />

          <!-- Quantity + Add to Cart -->
          <div class="flex items-center gap-4">
            <InputNumber v-model="quantity" :min="1" :max="99" class="w-24" />
            <Button label="Add to Cart" icon="pi pi-shopping-cart" class="flex-1" />
          </div>

          <!-- Expandable Sections -->
          <Accordion>
            <AccordionPanel v-if="product.description" header="Description">
              <p class="text-gray-600">{{ product.description }}</p>
            </AccordionPanel>
            <AccordionPanel v-if="product.materialComposition" header="Material & Composition">
              <p class="text-gray-600">{{ product.materialComposition }}</p>
            </AccordionPanel>
            <AccordionPanel v-if="product.careInstructions" header="Care Instructions">
              <p class="text-gray-600">{{ product.careInstructions }}</p>
            </AccordionPanel>
            <AccordionPanel v-if="product.fitNotes" header="Size & Fit">
              <p class="text-gray-600">{{ product.fitNotes }}</p>
            </AccordionPanel>
          </Accordion>
        </div>
      </div>

      <!-- Section: Similar Products -->
      <SimilarProductsRow
        v-if="similar.length > 0"
        :products="similar"
        class="mt-16"
      />
    </template>
  </div>
</template>
```

- [ ] **Step 6: Write HomeView.vue**

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getPaged } from '@/shared/api/paged'
import { ENDPOINTS } from '@/shared/constants/api'
import type { StoreProductListItemResponse } from '../types/product'
import ProductGrid from '../components/ProductGrid.vue'

const newArrivals = ref<StoreProductListItemResponse[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

onMounted(async () => {
  const result = await getPaged<StoreProductListItemResponse>(ENDPOINTS.products, {
    pageNumber: 1,
    pageSize: 8,
    sort: ['createdAtUtc desc'],
  })
  if (result.isSuccess) newArrivals.value = result.items
  else error.value = result.message
  loading.value = false
})
</script>
<template>
  <!-- Section: Home Page -->
  <div>
    <!-- Section: Hero Banner -->
    <section class="bg-gray-900 text-white">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-24 text-center">
        <h1 class="text-4xl md:text-5xl font-bold mb-4">Discover Your Style</h1>
        <p class="text-lg text-gray-300 mb-8 max-w-xl mx-auto">
          Shop the latest fashion trends with visual search. Upload an image, find your look.
        </p>
        <div class="flex justify-center gap-4">
          <router-link to="/shop">
            <Button label="Shop All" size="large" />
          </router-link>
          <router-link to="/recommendations">
            <Button label="Visual Search" severity="secondary" size="large" />
          </router-link>
        </div>
      </div>
    </section>

    <!-- Section: New Arrivals -->
    <section class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
      <h2 class="text-2xl font-bold text-gray-900 mb-8">New Arrivals</h2>
      <ProductGrid
        :products="newArrivals"
        :loading="loading"
        :error="error"
        @reload="() => {}"
      />
      <div class="text-center mt-8">
        <router-link to="/shop">
          <Button label="View All Products" severity="secondary" />
        </router-link>
      </div>
    </section>
  </div>
</template>
```

- [ ] **Step 7: Write VisualSearchView.vue**

See Task 3.3 for the full CBIR view implementation.

- [ ] **Step 8: Write catalog routes**

Write `app/Store/src/features/catalog/routes/index.ts`:
```ts
import type { RouteRecordRaw } from 'vue-router'

export const catalogRoutes: RouteRecordRaw[] = [
  {
    path: '/',
    name: 'home',
    component: () => import('../views/HomeView.vue'),
    meta: { title: 'Home' },
  },
  {
    path: '/shop',
    name: 'shop',
    component: () => import('../views/ShopView.vue'),
    meta: { title: 'Shop' },
  },
  {
    path: '/collections',
    name: 'collections',
    component: () => import('../views/CollectionsView.vue'),
    meta: { title: 'Collections' },
  },
  {
    path: '/products/:slug',
    name: 'product-detail',
    component: () => import('../views/ProductDetailView.vue'),
    meta: { title: 'Product' },
  },
  {
    path: '/recommendations',
    name: 'visual-search',
    component: () => import('../views/VisualSearchView.vue'),
    meta: { title: 'Visual Search' },
  },
]
```

- [ ] **Step 9: Register catalog routes in app router**

Edit `app/Store/src/app/router/routes.ts`:
```ts
import type { RouteRecordRaw } from 'vue-router'
import { catalogRoutes } from '@/features/catalog/routes'
import { identityRoutes } from '@/features/identity/routes'

export const routes: RouteRecordRaw[] = [
  // Public storefront shell
  {
    path: '/',
    component: () => import('@/app/layouts/DefaultLayout.vue'),
    children: [
      ...catalogRoutes,
      // ordering routes (Phase 4)
    ],
  },
  // Auth pages
  {
    path: '/',
    component: () => import('@/app/layouts/AuthLayout.vue'),
    children: [
      ...identityRoutes.filter(r => r.meta?.guestOnly),
    ],
  },
  // Account pages
  {
    path: '/account',
    component: () => import('@/app/layouts/AccountLayout.vue'),
    meta: { requiresAuth: true },
    children: [
      ...identityRoutes.filter(r => r.meta?.requiresAuth),
    ],
  },
  // 404
  {
    path: '/:pathMatch(.*)*',
    name: 'not-found',
    component: () => import('@/features/catalog/views/NotFoundView.vue'),
    meta: { title: 'Not Found' },
  },
]
```

- [ ] **Step 10: Verify type-check**

```bash
cd app/Store && pnpm run type-check
```

- [ ] **Step 11: Commit**

```bash
git add app/Store/src/features/catalog/views/ app/Store/src/features/catalog/components/ app/Store/src/features/catalog/routes/ app/Store/src/app/router/routes.ts
git commit -m "feat(store): add catalog views (Home, Shop, ProductDetail, routes)"
```

---

### Task 3.3: Visual Search (CBIR) — four-state UI

**Files:**
- Create: `app/Store/src/features/catalog/components/VisualSearchDropzone.vue`
- Create: `app/Store/src/features/catalog/composables/useVisualSearch.ts`
- Update: `app/Store/src/features/catalog/views/VisualSearchView.vue`

- [ ] **Step 1: Write useVisualSearch composable**

Write `app/Store/src/features/catalog/composables/useVisualSearch.ts`:
```ts
import { ref } from 'vue'
import { searchByImage } from '../services/searchByImageApi'
import type { SearchByImageResponse } from '../types/searchByImage'

export type VisualSearchState = 'empty' | 'upload' | 'loading' | 'results'

const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp']
const MAX_SIZE = 10 * 1024 * 1024 // 10 MB

export interface ValidationError {
  type: 'type' | 'size'
  message: string
}

export function useVisualSearch() {
  const state = ref<VisualSearchState>('empty')
  const selectedFile = ref<File | null>(null)
  const previewUrl = ref<string | null>(null)
  const results = ref<SearchByImageResponse[]>([])
  const error = ref<string | null>(null)
  const validationError = ref<ValidationError | null>(null)
  const isDragging = ref(false)

  function validateFile(file: File): ValidationError | null {
    if (!ALLOWED_TYPES.includes(file.type)) {
      return { type: 'type', message: 'Please select a JPEG, PNG, or WebP image.' }
    }
    if (file.size > MAX_SIZE) {
      return { type: 'size', message: 'Image must be under 10 MB.' }
    }
    return null
  }

  async function selectFile(file: File): Promise<void> {
    const validationErr = validateFile(file)
    if (validationErr) {
      validationError.value = validationErr
      return
    }
    validationError.value = null
    selectedFile.value = file
    if (previewUrl.value) URL.revokeObjectURL(previewUrl.value)
    previewUrl.value = URL.createObjectURL(file)
    state.value = 'upload'
  }

  function reset(): void {
    if (previewUrl.value) URL.revokeObjectURL(previewUrl.value)
    selectedFile.value = null
    previewUrl.value = null
    results.value = []
    error.value = null
    validationError.value = null
    state.value = 'empty'
  }

  async function search(topK = 20): Promise<void> {
    if (!selectedFile.value) return
    state.value = 'loading'
    error.value = null
    const result = await searchByImage(selectedFile.value, topK)
    if (result.isSuccess) {
      results.value = result.items
      state.value = 'results'
    } else {
      error.value = result.message ?? 'Search failed. Please try again.'
      state.value = 'upload'
    }
  }

  return { state, selectedFile, previewUrl, results, error, validationError, isDragging, validateFile, selectFile, reset, search }
}
```

- [ ] **Step 2: Write VisualSearchDropzone.vue**

```vue
<script setup lang="ts">
import { ref } from 'vue'

const emit = defineEmits<{ fileSelected: [file: File] }>()
const isDragging = ref(false)

const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp']
const MAX_SIZE = 10 * 1024 * 1024

function onDragOver(e: DragEvent): void {
  e.preventDefault()
  isDragging.value = true
}
function onDragLeave(): void { isDragging.value = false }

function onDrop(e: DragEvent): void {
  e.preventDefault()
  isDragging.value = false
  const file = e.dataTransfer?.files?.[0]
  if (file) emit('fileSelected', file)
}

function onFileInput(e: Event): void {
  const input = e.target as HTMLInputElement
  const file = input.files?.[0]
  if (file) emit('fileSelected', file)
  input.value = '' // Reset so same file can be re-selected
}
</script>
<template>
  <!-- Section: CBIR Dropzone -->
  <div
    class="border-2 border-dashed rounded-2xl p-12 text-center transition-all duration-200 cursor-pointer min-h-[300px] flex flex-col items-center justify-center"
    :class="isDragging
      ? 'border-gray-900 bg-gray-50'
      : 'border-gray-300 hover:border-gray-400 bg-white'"
    @dragover="onDragOver"
    @dragleave="onDragLeave"
    @drop="onDrop"
    @click="() => (($refs.fileInput as HTMLInputElement)?.click())"
  >
    <i class="pi pi-cloud-upload text-5xl text-gray-400 mb-4" />
    <p class="text-lg font-medium text-gray-900">Drop an image here or click to browse</p>
    <p class="text-sm text-gray-500 mt-2">JPEG, PNG, or WebP up to 10 MB</p>
    <Button label="Choose an image" severity="secondary" class="mt-6" />
    <input
      ref="fileInput"
      type="file"
      :accept="ALLOWED_TYPES.join(',')"
      class="hidden"
      @change="onFileInput"
    />
  </div>
</template>
```

- [ ] **Step 3: Write VisualSearchView.vue**

```vue
<script setup lang="ts">
import { useVisualSearch } from '../composables/useVisualSearch'
import VisualSearchDropzone from '../components/VisualSearchDropzone.vue'
import ProductCard from '../components/ProductCard.vue'
import SimilarityBadge from '../components/SimilarityBadge.vue'

const vs = useVisualSearch()

function onFileSelected(file: File): void {
  vs.selectFile(file)
}
</script>
<template>
  <!-- Section: Visual Search Page -->
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <h1 class="text-2xl font-bold text-gray-900 mb-8">Visual Search</h1>

    <!-- State: Empty -->
    <VisualSearchDropzone v-if="vs.state.value === 'empty'" @file-selected="onFileSelected" />

    <!-- State: Upload (preview shown) -->
    <template v-if="vs.state.value === 'upload' && vs.previewUrl.value">
      <div class="flex flex-col md:flex-row gap-8">
        <div class="w-full md:w-1/3">
          <img :src="vs.previewUrl.value" alt="Query image" class="w-full rounded-xl shadow" />
          <p class="text-sm text-gray-500 mt-2">{{ vs.selectedFile.value?.name }} ({{ ((vs.selectedFile.value?.size ?? 0) / 1024 / 1024).toFixed(1) }} MB)</p>
        </div>
        <div class="w-full md:w-2/3 flex flex-col justify-center items-center">
          <Button label="Search Similar Products" icon="pi pi-search" size="large" @click="vs.search()" />
          <Button label="Change image" severity="secondary" text class="mt-4" @click="vs.reset()" />
        </div>
      </div>
    </template>

    <!-- State: Loading -->
    <div v-if="vs.state.value === 'loading'" class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
      <div v-for="i in 8" :key="i" class="bg-gray-100 rounded-xl animate-pulse">
        <div class="aspect-square bg-gray-200 rounded-t-xl" />
        <div class="p-4 space-y-2">
          <div class="h-4 bg-gray-200 rounded w-3/4" />
          <div class="h-5 bg-gray-200 rounded w-1/3" />
        </div>
      </div>
    </div>

    <!-- State: Results -->
    <template v-if="vs.state.value === 'results'">
      <div class="flex flex-col md:flex-row gap-8">
        <!-- Query image sidebar -->
        <div class="w-full md:w-1/4 shrink-0">
          <img v-if="vs.previewUrl.value" :src="vs.previewUrl.value" alt="Query image" class="w-full rounded-xl shadow" />
          <Button label="New Search" severity="secondary" class="w-full mt-4" @click="vs.reset()" />
        </div>

        <!-- Results grid -->
        <div class="flex-1">
          <!-- Empty results -->
          <div v-if="vs.results.value.length === 0" class="text-center py-16">
            <i class="pi pi-image text-4xl text-gray-300 mb-4" />
            <h3 class="text-lg font-medium text-gray-900">We couldn't find products similar to your image.</h3>
            <p class="text-gray-500 mt-2">Try a different image or angle.</p>
            <Button label="Try Again" severity="secondary" class="mt-4" @click="vs.reset()" />
          </div>

          <!-- Result cards -->
          <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            <div v-for="item in vs.results.value" :key="item.variantId" class="relative">
              <ProductCard
                :product="{
                  id: item.productId,
                  masterVariantId: item.variantId,
                  name: item.productName,
                  status: '',
                  description: null,
                  slug: item.productId,
                  minPrice: item.price,
                  currency: null,
                  thumbnailUrl: item.imageUrl,
                  thumbnailAlt: item.productName,
                  styleCode: null,
                  seasonName: null,
                  materialComposition: null,
                  careInstructions: null,
                  fitNotes: null,
                  department: null,
                  genderTarget: null,
                  variantsCount: 0,
                  availableOn: null,
                }"
              />
            </div>
          </div>
        </div>
      </div>
    </template>

    <!-- State: Validation Error -->
    <Message v-if="vs.validationError.value" severity="error" :closable="true" @close="vs.validationError.value = null">
      {{ vs.validationError.value.message }}
    </Message>
  </div>
</template>
```

- [ ] **Step 4: Write SimilarityBadge.vue**

```vue
<script setup lang="ts">
defineProps<{ score: number }>()

// Map: Color-code similarity score
function badgeClass(score: number): string {
  if (score >= 90) return 'bg-green-100 text-green-700'
  if (score >= 80) return 'bg-amber-100 text-amber-700'
  return 'bg-gray-100 text-gray-600'
}
</script>
<template>
  <span class="text-xs font-medium px-2 py-1 rounded-full" :class="badgeClass(score)">
    {{ Math.round(score) }}% match
  </span>
</template>
```

- [ ] **Step 5: Verify type-check**

```bash
cd app/Store && pnpm run type-check
```

- [ ] **Step 6: Commit**

```bash
git add app/Store/src/features/catalog/components/VisualSearchDropzone.vue app/Store/src/features/catalog/composables/useVisualSearch.ts app/Store/src/features/catalog/views/VisualSearchView.vue app/Store/src/features/catalog/components/SimilarityBadge.vue
git commit -m "feat(store): implement CBIR visual search with four-state UI"
```

---

### Task 2.3: Shared UI components

**Files:**
- Create: `app/Store/src/shared/components/EmptyState.vue`
- Create: `app/Store/src/shared/components/SkeletonCard.vue`
- Create: `app/Store/src/shared/components/SkeletonGrid.vue`
- Create: `app/Store/src/shared/components/StatusTag.vue`
- Create: `app/Store/src/shared/components/ScrollToTop.vue`

- [ ] **Step 1: Write EmptyState.vue**

```vue
<script setup lang="ts">
defineProps<{ icon?: string; message: string; actionLabel?: string; actionTo?: string }>()
</script>
<template>
  <div class="text-center py-16">
    <i v-if="icon" :class="icon" class="text-4xl text-gray-300 mb-4 block" />
    <p class="text-gray-500">{{ message }}</p>
    <router-link v-if="actionLabel && actionTo" :to="actionTo" class="mt-4 inline-block">
      <Button :label="actionLabel" severity="secondary" />
    </router-link>
  </div>
</template>
```

- [ ] **Step 2: Write SkeletonCard.vue**

```vue
<template>
  <div class="bg-white rounded-xl border border-gray-200 overflow-hidden animate-pulse">
    <div class="aspect-square bg-gray-200" />
    <div class="p-4 space-y-2">
      <div class="h-4 bg-gray-200 rounded w-3/4" />
      <div class="h-5 bg-gray-200 rounded w-1/3" />
    </div>
  </div>
</template>
```

- [ ] **Step 3: Write SkeletonGrid.vue**

```vue
<script setup lang="ts">
defineProps<{ count?: number }>()
</script>
<template>
  <div class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
    <SkeletonCard v-for="i in (count ?? 8)" :key="i" />
  </div>
</template>
```

- [ ] **Step 4: Write StatusTag.vue**

```vue
<script setup lang="ts">
defineProps<{ status: string }>()
const colorMap: Record<string, string> = {
  pending: 'bg-amber-100 text-amber-700',
  confirmed: 'bg-blue-100 text-blue-700',
  shipped: 'bg-purple-100 text-purple-700',
  delivered: 'bg-green-100 text-green-700',
  cancelled: 'bg-red-100 text-red-700',
  in_stock: 'bg-green-100 text-green-700',
  low_stock: 'bg-amber-100 text-amber-700',
  out_of_stock: 'bg-red-100 text-red-700',
}
const cls = colorMap[status] ?? 'bg-gray-100 text-gray-600'
</script>
<template>
  <span class="text-xs font-medium px-2 py-0.5 rounded-full" :class="cls">{{ status.replace(/_/g, ' ') }}</span>
</template>
```

- [ ] **Step 5: Write ScrollToTop.vue**

```vue
<script setup lang="ts">
import { watch } from 'vue'
import { useRoute } from 'vue-router'
const route = useRoute()
watch(() => route.fullPath, () => window.scrollTo(0, 0))
</script>
<template><div /></template>
```

Place `<ScrollToTop />` in `App.vue` inside the template alongside `<router-view />`.

- [ ] **Step 6: Commit**

```bash
git add app/Store/src/shared/components/ app/Store/src/App.vue
git commit -m "feat(store): add shared UI components (EmptyState, SkeletonCard, SkeletonGrid, StatusTag, ScrollToTop)"
```

---

## Phase 4 — Ordering Module (Cart, Checkout, Orders)

### Task 4.1: Cart types, services, and enhanced store

**Files:**
- Create: `app/Store/src/features/ordering/types/cart.ts`
- Create: `app/Store/src/features/ordering/services/cartApi.ts`
- Modify: `app/Store/src/features/ordering/stores/cartStore.ts` (replace skeleton)

- [ ] **Step 1: Write cart types**

```ts
// features/ordering/types/cart.ts
export interface CartLineItem {
  lineItemId: string
  variantId: string
  productId: string
  productName: string
  productSlug: string
  sku: string | null
  quantity: number
  unitPrice: number
  currency: string
  thumbnailUrl: string | null
  optionDescription: string | null
  maxQuantity: number
}

export interface CartResponse {
  id: string
  items: CartLineItem[]
  itemCount: number
  subtotal: number
  currency: string
}

export interface AddCartItemRequest {
  variantId: string
  quantity: number
}

export interface UpdateCartItemRequest {
  quantity: number
}
```

- [ ] **Step 2: Write cartApi.ts**

```ts
// features/ordering/services/cartApi.ts
import { get, post, put, del } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { Result } from '@/shared/types/result'
import type { CartResponse, AddCartItemRequest, UpdateCartItemRequest } from '../types/cart'

export function getCart(): Promise<Result<CartResponse>> {
  return get<Result<CartResponse>>(ENDPOINTS.cart)
}

export function addItem(req: AddCartItemRequest): Promise<Result<CartResponse>> {
  return post<Result<CartResponse>>(ENDPOINTS.cartItems, req)
}

export function updateItem(lineItemId: string, req: UpdateCartItemRequest): Promise<Result<CartResponse>> {
  return put<Result<CartResponse>>(ENDPOINTS.cartItem(lineItemId), req)
}

export function removeItem(lineItemId: string): Promise<Result<CartResponse>> {
  return del<Result<CartResponse>>(ENDPOINTS.cartItem(lineItemId))
}

export function emptyCart(): Promise<Result<null>> {
  return post<Result<null>>(ENDPOINTS.cartEmpty)
}

export function associateCart(): Promise<Result<CartResponse>> {
  return post<Result<CartResponse>>(ENDPOINTS.cartAssociate)
}
```

- [ ] **Step 3: Rewrite cartStore.ts**

Replace the Phase 2 skeleton with full implementation:

```ts
// features/ordering/stores/cartStore.ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { STORAGE_KEYS } from '@/shared/constants/storage'
import type { CartLineItem, CartResponse } from '../types/cart'
import * as cartApi from '../services/cartApi'

export const useCartStore = defineStore('cart', () => {
  const id = ref<string | null>(null)
  const items = ref<CartLineItem[]>([])
  const currency = ref('VND')
  const loading = ref(false)
  const error = ref<string | null>(null)

  const itemCount = computed(() => items.value.reduce((sum, i) => sum + i.quantity, 0))
  const subtotal = computed(() => items.value.reduce((sum, i) => sum + i.unitPrice * i.quantity, 0))

  function getCartToken(): string {
    let token = localStorage.getItem(STORAGE_KEYS.CART_TOKEN)
    if (!token) {
      token = crypto.randomUUID()
      localStorage.setItem(STORAGE_KEYS.CART_TOKEN, token)
    }
    return token
  }

  async function fetchCart(): Promise<void> {
    loading.value = true
    error.value = null
    const result = await cartApi.getCart()
    if (result.isSuccess) {
      applyCart(result.value)
    } else {
      error.value = result.message ?? 'Failed to load cart'
    }
    loading.value = false
  }

  function applyCart(cart: CartResponse): void {
    id.value = cart.id
    items.value = cart.items
    currency.value = cart.currency
  }

  async function addItem(variantId: string, quantity = 1): Promise<boolean> {
    const result = await cartApi.addItem({ variantId, quantity })
    if (result.isSuccess) { applyCart(result.value); return true }
    error.value = result.message ?? 'Failed to add item'
    return false
  }

  async function updateQuantity(lineItemId: string, quantity: number): Promise<boolean> {
    const result = await cartApi.updateItem(lineItemId, { quantity })
    if (result.isSuccess) { applyCart(result.value); return true }
    error.value = result.message ?? 'Failed to update quantity'
    return false
  }

  async function removeItem(lineItemId: string): Promise<boolean> {
    const result = await cartApi.removeItem(lineItemId)
    if (result.isSuccess) { applyCart(result.value); return true }
    error.value = result.message ?? 'Failed to remove item'
    return false
  }

  async function clearCart(): Promise<void> {
    await cartApi.emptyCart()
    items.value = []
  }

  async function associate(): Promise<void> {
    const result = await cartApi.associateCart()
    if (result.isSuccess) applyCart(result.value)
  }

  function reset(): void {
    id.value = null
    items.value = []
    error.value = null
  }

  return { id, items, currency, loading, error, itemCount, subtotal, getCartToken, fetchCart, addItem, updateQuantity, removeItem, clearCart, associate, reset }
})
```

- [ ] **Step 4: Verify type-check and commit**

---

### Task 4.2: Cart view and components

**Files:**
- Create: `app/Store/src/features/ordering/components/CartItem.vue`
- Create: `app/Store/src/features/ordering/components/OrderSummary.vue`
- Create: `app/Store/src/features/ordering/views/CartView.vue`

- [ ] **Step 1: Write CartItem.vue**

```vue
<script setup lang="ts">
import type { CartLineItem } from '../types/cart'
import { formatVnd } from '@/shared/utils/currency'

const props = defineProps<{ item: CartLineItem }>()
const emit = defineEmits<{ updateQuantity: [lineItemId: string, qty: number]; remove: [lineItemId: string] }>()
</script>
<template>
  <div class="flex gap-4 py-4 border-b border-gray-200">
    <img
      v-if="item.thumbnailUrl"
      :src="item.thumbnailUrl"
      :alt="item.productName"
      class="w-20 h-20 rounded-lg object-cover bg-gray-100"
    />
    <div class="w-20 h-20 rounded-lg bg-gray-100 flex items-center justify-center text-gray-400" v-else>
      <i class="pi pi-image" />
    </div>
    <div class="flex-1 min-w-0">
      <router-link :to="`/products/${item.productSlug}`" class="text-sm font-medium text-gray-900 hover:text-gray-600">
        {{ item.productName }}
      </router-link>
      <p v-if="item.optionDescription" class="text-xs text-gray-500 mt-1">{{ item.optionDescription }}</p>
      <p class="text-sm font-semibold text-gray-900 mt-1">{{ formatVnd(item.unitPrice) }}</p>
    </div>
    <div class="flex flex-col items-end gap-2">
      <InputNumber :model-value="item.quantity" :min="1" :max="item.maxQuantity" class="w-20" @update:model-value="(v: number) => emit('updateQuantity', item.lineItemId, v)" />
      <Button icon="pi pi-trash" severity="danger" text size="small" @click="emit('remove', item.lineItemId)" />
      <p class="text-sm font-semibold">{{ formatVnd(item.unitPrice * item.quantity) }}</p>
    </div>
  </div>
</template>
```

- [ ] **Step 2: Write OrderSummary.vue**

```vue
<script setup lang="ts">
import { formatVnd } from '@/shared/utils/currency'

defineProps<{ itemCount: number; subtotal: number }>()
</script>
<template>
  <div class="bg-white rounded-xl border border-gray-200 p-6 sticky top-24">
    <h3 class="text-lg font-semibold text-gray-900 mb-4">Order Summary</h3>
    <div class="flex justify-between text-sm text-gray-600 mb-2">
      <span>Items ({{ itemCount }})</span>
      <span>{{ formatVnd(subtotal) }}</span>
    </div>
    <div class="flex justify-between text-sm text-gray-600 mb-2">
      <span>Shipping</span>
      <span class="text-gray-400">Calculated at checkout</span>
    </div>
    <Divider />
    <div class="flex justify-between font-semibold text-gray-900 mb-6">
      <span>Total</span>
      <span>{{ formatVnd(subtotal) }}</span>
    </div>
    <router-link to="/checkout">
      <Button label="Proceed to Checkout" class="w-full" :disabled="itemCount === 0" />
    </router-link>
  </div>
</template>
```

- [ ] **Step 3: Write CartView.vue**

```vue
<script setup lang="ts">
import { onMounted } from 'vue'
import { useCartStore } from '../stores/cartStore'
import CartItem from '../components/CartItem.vue'
import OrderSummary from '../components/OrderSummary.vue'
import EmptyState from '@/shared/components/EmptyState.vue'

const cart = useCartStore()

onMounted(() => cart.fetchCart())
</script>
<template>
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <h1 class="text-2xl font-bold text-gray-900 mb-8">Shopping Cart</h1>
    <EmptyState
      v-if="!cart.loading && cart.items.length === 0"
      icon="pi pi-shopping-bag"
      message="Your cart is empty"
      action-label="Continue Shopping"
      action-to="/shop"
    />
    <div v-else class="flex flex-col lg:flex-row gap-8">
      <div class="flex-1">
        <CartItem
          v-for="item in cart.items"
          :key="item.lineItemId"
          :item="item"
          @update-quantity="(id, qty) => cart.updateQuantity(id, qty)"
          @remove="(id) => cart.removeItem(id)"
        />
      </div>
      <div class="w-full lg:w-80">
        <OrderSummary :item-count="cart.itemCount" :subtotal="cart.subtotal" />
      </div>
    </div>
  </div>
</template>
```

- [ ] **Step 4: Add ordering routes**

```ts
// features/ordering/routes/index.ts
import type { RouteRecordRaw } from 'vue-router'
export const orderingRoutes: RouteRecordRaw[] = [
  { path: '/cart', name: 'cart', component: () => import('../views/CartView.vue'), meta: { title: 'Cart' } },
  { path: '/checkout', name: 'checkout', component: () => import('../views/CheckoutView.vue'), meta: { requiresAuth: true, title: 'Checkout' } },
  { path: '/account/orders', name: 'orders', component: () => import('../views/OrderListView.vue'), meta: { requiresAuth: true, title: 'Orders' } },
  { path: '/account/orders/:id', name: 'order-detail', component: () => import('../views/OrderDetailView.vue'), meta: { requiresAuth: true, title: 'Order' } },
]
```

- [ ] **Step 5: Verify and commit**

---

### Task 4.3: Checkout — 5-step pipeline

**Files:**
- Create: `app/Store/src/features/ordering/types/checkout.ts`
- Create: `app/Store/src/features/ordering/services/checkoutApi.ts`
- Create: `app/Store/src/features/ordering/stores/checkoutStore.ts`
- Create: `app/Store/src/features/ordering/components/CheckoutStepper.vue`
- Create: `app/Store/src/features/ordering/components/CheckoutStepAddress.vue`
- Create: `app/Store/src/features/ordering/components/CheckoutStepDelivery.vue`
- Create: `app/Store/src/features/ordering/components/CheckoutStepPayment.vue`
- Create: `app/Store/src/features/ordering/components/CheckoutStepConfirm.vue`
- Create: `app/Store/src/features/ordering/components/CheckoutStepComplete.vue`
- Create: `app/Store/src/features/ordering/views/CheckoutView.vue`

- [ ] **Step 1: Write checkoutStore.ts — 5-step state machine**

```ts
// features/ordering/stores/checkoutStore.ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { put, post } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { Result } from '@/shared/types/result'

export type CheckoutStep = 1 | 2 | 3 | 4 | 5

export const useCheckoutStore = defineStore('checkout', () => {
  const currentStep = ref<CheckoutStep>(1)
  const shipAddressId = ref<string | null>(null)
  const shippingMethodId = ref<string | null>(null)
  const paymentIntentId = ref<string | null>(null)
  const orderId = ref<string | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)
  const email = ref('')
  const currency = ref('VND')

  const steps = [
    { label: 'Address', stepNumber: 1 },
    { label: 'Delivery', stepNumber: 2 },
    { label: 'Payment', stepNumber: 3 },
    { label: 'Confirm', stepNumber: 4 },
    { label: 'Complete', stepNumber: 5 },
  ]

  async function goToStep(step: CheckoutStep): Promise<void> {
    loading.value = true
    error.value = null
    // Validate current step before advancing
    const validateResult = await post<Result<unknown>>(ENDPOINTS.cartValidate)
    if (!validateResult.isSuccess && step > currentStep.value) {
      error.value = validateResult.message ?? 'Please complete the current step first.'
      loading.value = false
      return
    }
    currentStep.value = step
    loading.value = false
  }

  async function saveAddress(addressId: string, userEmail: string): Promise<boolean> {
    shipAddressId.value = addressId
    email.value = userEmail
    const result = await put<Result<unknown>>(ENDPOINTS.cart, {
      shipAddressId: addressId,
      currency: currency.value,
      email: userEmail,
    })
    return result.isSuccess
  }

  async function calculateShipping(methodId: string): Promise<boolean> {
    shippingMethodId.value = methodId
    const result = await post<Result<unknown>>(ENDPOINTS.cartShippingRate, { shippingMethodId: methodId })
    return result.isSuccess
  }

  async function createPaymentIntent(methodId: string, amount: number): Promise<string | null> {
    const result = await post<Result<{ id: string; clientSecret: string }>>(ENDPOINTS.paymentCreateIntent, {
      amount, currency: currency.value, paymentMethodId: methodId,
    })
    if (result.isSuccess) {
      paymentIntentId.value = result.value.id
      return result.value.clientSecret
    }
    return null
  }

  async function placeOrder(): Promise<boolean> {
    if (!paymentIntentId.value) return false
    loading.value = true
    const result = await post<Result<{ orderId: string }>>(ENDPOINTS.cartCheckout, {
      paymentIntentId: paymentIntentId.value,
    })
    if (result.isSuccess) {
      orderId.value = result.value.orderId
      currentStep.value = 5
      loading.value = false
      return true
    }
    error.value = result.message ?? 'Failed to place order'
    loading.value = false
    return false
  }

  function reset(): void {
    currentStep.value = 1
    shipAddressId.value = null
    shippingMethodId.value = null
    paymentIntentId.value = null
    orderId.value = null
    error.value = null
  }

  return { currentStep, shipAddressId, shippingMethodId, paymentIntentId, orderId, loading, error, email, currency, steps, goToStep, saveAddress, calculateShipping, createPaymentIntent, placeOrder, reset }
})
```

- [ ] **Step 2: Write CheckoutStepper.vue**

```vue
<script setup lang="ts">
import type { CheckoutStep } from '../stores/checkoutStore'

defineProps<{ steps: Array<{ label: string; stepNumber: number }>; currentStep: CheckoutStep }>()
</script>
<template>
  <div class="flex items-center justify-center mb-8">
    <template v-for="(step, idx) in steps" :key="step.stepNumber">
      <div class="flex items-center">
        <div class="flex items-center gap-2" :class="currentStep >= step.stepNumber ? 'text-gray-900' : 'text-gray-400'">
          <span
            class="w-8 h-8 rounded-full flex items-center justify-center text-sm font-medium border-2"
            :class="currentStep > step.stepNumber
              ? 'bg-gray-900 border-gray-900 text-white'
              : currentStep === step.stepNumber
                ? 'border-gray-900 text-gray-900'
                : 'border-gray-300 text-gray-400'"
          >
            <i v-if="currentStep > step.stepNumber" class="pi pi-check text-xs" />
            <span v-else>{{ step.stepNumber }}</span>
          </span>
          <span class="text-sm font-medium hidden sm:inline">{{ step.label }}</span>
        </div>
        <div v-if="idx < steps.length - 1" class="w-12 sm:w-24 h-px mx-2" :class="currentStep > step.stepNumber ? 'bg-gray-900' : 'bg-gray-300'" />
      </div>
    </template>
  </div>
</template>
```

- [ ] **Step 3: Write CheckoutView.vue**

```vue
<script setup lang="ts">
import { useCheckoutStore } from '../stores/checkoutStore'
import CheckoutStepper from '../components/CheckoutStepper.vue'
import CheckoutStepAddress from '../components/CheckoutStepAddress.vue'
import CheckoutStepDelivery from '../components/CheckoutStepDelivery.vue'
import CheckoutStepPayment from '../components/CheckoutStepPayment.vue'
import CheckoutStepConfirm from '../components/CheckoutStepConfirm.vue'
import CheckoutStepComplete from '../components/CheckoutStepComplete.vue'

const checkout = useCheckoutStore()
</script>
<template>
  <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <h1 class="text-2xl font-bold text-gray-900 mb-8">Checkout</h1>
    <CheckoutStepper :steps="checkout.steps" :current-step="checkout.currentStep" />
    <Message v-if="checkout.error" severity="error" class="mb-6">{{ checkout.error }}</Message>
    <CheckoutStepAddress v-if="checkout.currentStep === 1" />
    <CheckoutStepDelivery v-if="checkout.currentStep === 2" />
    <CheckoutStepPayment v-if="checkout.currentStep === 3" />
    <CheckoutStepConfirm v-if="checkout.currentStep === 4" />
    <CheckoutStepComplete v-if="checkout.currentStep === 5" />
  </div>
</template>
```

- [ ] **Step 4: Commit**

### Task 4.4: Order history — services, store, views

Pattern: `orderApi.ts` (getPaged orders, getOrder, cancelOrder) + `orderStore.ts` + `OrderListView.vue` + `OrderDetailView.vue` + `OrderCard.vue`. Same `usePagedQuery` pattern as ShopView. Order detail shows state-transition timeline with PrimeVue Timeline component.

---

## Phase 5 — Payment, Shipping, Inventory, Location Services

### Task 5.1: Payment services + usePayment composable

**Files:**
- Create: `app/Store/src/features/payment/types/payment.ts`
- Create: `app/Store/src/features/payment/services/paymentApi.ts`
- Create: `app/Store/src/features/payment/composables/usePayment.ts`

```ts
// paymentApi.ts
import { get, post } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { Result } from '@/shared/types/result'

export interface PaymentMethod { id: string; name: string; code: string; iconUrl: string | null }
export interface PaymentIntentResponse { id: string; clientSecret: string }

export function getPaymentMethods(): Promise<Result<PaymentMethod[]>> {
  return get<Result<PaymentMethod[]>>(ENDPOINTS.paymentMethods)
}
export function createPaymentIntent(amount: number, currency: string, methodId: string): Promise<Result<PaymentIntentResponse>> {
  return post<Result<PaymentIntentResponse>>(ENDPOINTS.paymentCreateIntent, { amount, currency, paymentMethodId: methodId })
}
export function confirmPayment(paymentId: string): Promise<Result<unknown>> {
  return post<Result<unknown>>(ENDPOINTS.paymentConfirm(paymentId))
}
```

```ts
// usePayment.ts — loads Stripe.js, mounts Elements
import { ref } from 'vue'
import { loadStripe } from '@stripe/stripe-js'
import type { Stripe, StripeElements } from '@stripe/stripe-js'

const stripePromise = ref<Promise<Stripe | null> | null>(null)

export function usePayment() {
  const elements = ref<StripeElements | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  function init(publishableKey: string): void {
    if (!stripePromise.value) stripePromise.value = loadStripe(publishableKey)
  }

  async function mount(clientSecret: string, container: HTMLElement): Promise<Stripe | null> {
    loading.value = true
    const stripe = await stripePromise.value
    if (!stripe) { error.value = 'Failed to load Stripe'; loading.value = false; return null }
    elements.value = stripe.elements({ clientSecret })
    const card = elements.value.create('card')
    card.mount(container)
    loading.value = false
    return stripe
  }

  function unmount(): void {
    elements.value?.getElement('card')?.unmount()
    elements.value = null
  }

  return { loading, error, init, mount, unmount, stripePromise }
}
```

### Task 5.2: Shipping services

```ts
// features/shipping/services/shippingApi.ts
import { get, post } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { Result } from '@/shared/types/result'

export interface ShippingMethod { id: string; name: string; carrier: string; estimatedDays: number; price: number; currency: string }
export function getShippingMethods(): Promise<Result<ShippingMethod[]>> {
  return get<Result<ShippingMethod[]>>(ENDPOINTS.shippingMethods)
}
export function calculateShipping(methodId: string): Promise<Result<{ rate: number; currency: string }>> {
  return post<Result<{ rate: number; currency: string }>>(ENDPOINTS.shippingCalculate, { shippingMethodId: methodId })
}
```

### Task 5.3: Inventory services (internal — no views)

```ts
// features/inventory/services/availabilityApi.ts
import { get, post, del } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { Result } from '@/shared/types/result'
export function checkAvailability(variantId: string): Promise<Result<{ available: boolean; quantity: number }>> {
  return get<Result<{ available: boolean; quantity: number }>>(ENDPOINTS.availability(variantId))
}
```

### Task 5.4: Location services + useLocationCascade

```ts
// features/location/services/countryApi.ts
import { get } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { Result } from '@/shared/types/result'

export interface Country { id: string; name: string; iso: string }
export interface State { id: string; name: string; iso: string; countryId: string }

export function getCountries(): Promise<Result<Country[]>> {
  return get<Result<Country[]>>(ENDPOINTS.countries)
}
export function getStates(): Promise<Result<State[]>> {
  return get<Result<State[]>>(ENDPOINTS.states)
}
```

```ts
// features/location/composables/useLocationCascade.ts
import { ref, watch } from 'vue'
import { getCountries, getStates } from '../services/countryApi'
import type { Country, State } from '../services/countryApi'

export function useLocationCascade() {
  const countries = ref<Country[]>([])
  const states = ref<State[]>([])
  const selectedCountryId = ref<string | null>(null)
  const selectedStateId = ref<string | null>(null)
  const loading = ref(false)

  async function loadCountries(): Promise<void> {
    loading.value = true
    const result = await getCountries()
    if (result.isSuccess) countries.value = result.value
    loading.value = false
  }

  watch(selectedCountryId, async (countryId) => {
    if (!countryId) { states.value = []; return }
    const result = await getStates()
    if (result.isSuccess) {
      states.value = result.value.filter(s => s.countryId === countryId)
    }
  })

  return { countries, states, selectedCountryId, selectedStateId, loading, loadCountries }
}
```

---

## Phase 6 — Identity Views + Account Module

### Task 6.1: Identity views (Login, Register, Forgot/Reset Password)

All use `@primevue/forms` `<Form :resolver="zodResolver(schema)">` + `<FormField>` + `<Message>` for errors.

### Task 6.2: Sessions view

### Task 6.3: Profile services and views (AddressBook, Wishlists, Notifications, Profile)

### Task 6.4: Wire cart merge on login

Add to `authStore.login()`:
```ts
// After successful login and session fetch:
const { useCartStore } = await import('@/features/ordering/stores/cartStore')
const cart = useCartStore()
await cart.associate()
```

---

## Phase 7 — Polish, Tests, Thesis Alignment

### Task 7.1: Design token pass

Apply Aura preset CSS variable overrides in `styles.scss` matching the spec's color palette (section 9.1):
```scss
:root {
  --p-primary-color: #111827;
  --p-surface-ground: #f9fafb;
  --p-surface-card: #ffffff;
  --p-text-color: #111827;
  --p-text-muted-color: #6b7280;
  --p-content-border-color: #e5e7eb;
}
```

### Task 7.2: Unit tests

Write Pinia store tests (`@pinia/testing` + `vi.mock` on services):
- `authStore.spec.ts`: test login success/failure, init with valid/expired token, logout
- `cartStore.spec.ts`: test addItem, updateQuantity, removeItem, subtotal computation
- `checkoutStore.spec.ts`: test step progression, orchestration sequence
- `catalogStore.spec.ts`: test filter toggle, clear

### Task 7.3: Validate Zod 4 + @primevue/forms@5 compat

Create a simple test form in a sandbox component. If issues arise, fallback to Zod 3.24 or Vee-Validate 4.

### Task 7.4: NFR verification

- Upload >10MB rejected client-side: manual test with large image
- 5xx triggers toast: manual test by killing backend mid-request
- Refresh-token rotation: manual test, wait >15 min, verify refresh
- Guard redirect: navigate to `/account/orders` while logged out, login, verify redirect to `/account/orders`
- `prefers-reduced-motion`: check skeleton animations respect the media query

### Task 7.5: E2E smoke test via Aspire

```bash
dotnet run --project infra/Aspire/src/ReSys.AppHost
```
Exercise on `https://localhost:5174`:
1. Homepage loads → new arrivals grid visible
2. Shop → filter by category → paginate
3. Product detail → gallery, variant selection
4. Add to cart → cart view shows item
5. Login → cart merges (guest items retained)
6. Checkout → complete 5 steps with Stripe test card `4242 4242 4242 4242`
7. Orders → view placed order, cancel if pending
8. Visual search → upload JPEG → ranked grid renders

### Task 7.6: Thesis screenshot harvest

Capture screenshots of all 4 CBIR states, 5 checkout steps, product detail with gallery, and order timeline for the thesis document.

---

## Verification

1. `pnpm run type-check` — 0 errors at every phase
2. `pnpm run lint` — 0 violations at every phase
3. `pnpm run test:unit -- --run` — all shared + store tests pass
4. `dotnet run --project infra/Aspire/src/ReSys.AppHost` — full E2E flow on `https://localhost:5174`
5. `bash scripts/check-cross-module-refs.sh` — .NET tree unaffected
