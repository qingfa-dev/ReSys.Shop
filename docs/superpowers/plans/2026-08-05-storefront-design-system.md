# Storefront Design System — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace monochrome gray Aura defaults with fashion boutique teal palette across all PrimeVue 5 components, add dark mode toggle with system preference detection, add Playfair Display + DM Sans fonts, add 4 animation keyframes.

**Architecture:** Override PrimeVue 5 Aura CSS custom properties in `styles.scss` via `:root` and `.app-dark` blocks. `useTheme` composable manages dark mode state machine (light/dark/system) with localStorage persistence and `matchMedia` listener. `ThemeToggle.vue` provides sun/moon icon button in AppHeader. No component template changes needed beyond the toggle button.

**Tech Stack:** Vue 3.5, TypeScript 6.0, PrimeVue 5 Aura preset, Tailwind CSS 4, SCSS.

## Global Constraints

- PrimeVue version: `primevue@^5.0.0`, `@primeuix/themes@^3.0.0`
- `darkModeSelector: '.app-dark'` already configured in `primevue.ts` — do not change
- TypeScript `noUncheckedIndexedAccess: true` enforced
- All new `.ts` files must pass `pnpm run type-check` with 0 errors
- All files must pass `pnpm run lint` with 0 violations
- No component logic changes except adding ThemeToggle to AppHeader
- No new Pinia stores — `useTheme` is a standalone composable with module-level singleton
- Google Fonts loaded via CDN `@import` in `tailwind.css`
- All color hex values match the spec exactly — teal-700 = `#0f766e`

---

### Task 1: Create useTheme composable with tests

**Files:**
- Create: `app/Store/src/shared/composables/useTheme.ts`
- Create: `app/Store/src/shared/composables/__tests__/useTheme.spec.ts`

**Interfaces:**
- Produces: `useTheme()` composable — returns `{ mode, isDark, toggle, setMode }`
- Consumes: nothing (only browser APIs: localStorage, matchMedia)

- [ ] **Step 1: Write the composable**

Write `app/Store/src/shared/composables/useTheme.ts`:

```ts
import { ref, computed, onUnmounted } from 'vue'

export type ThemeMode = 'light' | 'dark' | 'system'

const STORAGE_KEY = 'theme-preference'
const DARK_CLASS = 'app-dark'

let mediaQuery: MediaQueryList | null = null
let mediaListener: ((e: MediaQueryListEvent) => void) | null = null

const currentMode = ref<ThemeMode>(readStoredMode())

function readStoredMode(): ThemeMode {
  try {
    const stored = localStorage.getItem(STORAGE_KEY)
    if (stored === 'light' || stored === 'dark' || stored === 'system') return stored
  } catch { /* localStorage unavailable */ }
  return 'system'
}

function systemPrefersDark(): boolean {
  if (typeof window === 'undefined') return false
  return window.matchMedia('(prefers-color-scheme: dark)').matches
}

function applyClass(dark: boolean): void {
  if (typeof document === 'undefined') return
  document.documentElement.classList.toggle(DARK_CLASS, dark)
}

function persist(mode: ThemeMode): void {
  try { localStorage.setItem(STORAGE_KEY, mode) } catch { /* ignore */ }
}

function startListening(): void {
  if (typeof window === 'undefined') return
  mediaQuery = window.matchMedia('(prefers-color-scheme: dark)')
  mediaListener = () => {
    if (currentMode.value === 'system') {
      applyClass(systemPrefersDark())
    }
  }
  mediaQuery.addEventListener('change', mediaListener)
}

function stopListening(): void {
  if (mediaQuery && mediaListener) {
    mediaQuery.removeEventListener('change', mediaListener)
    mediaQuery = null
    mediaListener = null
  }
}

export function useTheme() {
  const isDark = computed(() => {
    if (currentMode.value === 'dark') return true
    if (currentMode.value === 'light') return false
    return systemPrefersDark()
  })

  function setMode(mode: ThemeMode): void {
    currentMode.value = mode
    persist(mode)
    applyClass(isDark.value)
  }

  function toggle(): void {
    const order: ThemeMode[] = ['light', 'dark', 'system']
    const idx = order.indexOf(currentMode.value)
    setMode(order[(idx + 1) % order.length])
  }

  if (typeof document !== 'undefined') {
    applyClass(isDark.value)
    startListening()
  }

  const cleanup = onUnmounted(() => {
    stopListening()
  })

  return { mode: currentMode, isDark, toggle, setMode }
}
```

- [ ] **Step 2: Write unit tests**

Write `app/Store/src/shared/composables/__tests__/useTheme.spec.ts`:

```ts
import { describe, it, expect, beforeEach, vi } from 'vitest'

describe('useTheme', () => {
  beforeEach(() => {
    localStorage.clear()
    document.documentElement.classList.remove('app-dark')
    vi.restoreAllMocks()
  })

  it('isDark returns false in light mode', async () => {
    localStorage.setItem('theme-preference', 'light')
    const { useTheme } = await import('@/shared/composables/useTheme')
    const { isDark, mode } = useTheme()
    expect(mode.value).toBe('light')
    expect(isDark.value).toBe(false)
  })

  it('isDark returns true in dark mode', async () => {
    localStorage.setItem('theme-preference', 'dark')
    const { useTheme } = await import('@/shared/composables/useTheme')
    const { isDark } = useTheme()
    expect(isDark.value).toBe(true)
  })

  it('applies app-dark class in dark mode', async () => {
    localStorage.setItem('theme-preference', 'dark')
    const { useTheme } = await import('@/shared/composables/useTheme')
    useTheme()
    expect(document.documentElement.classList.contains('app-dark')).toBe(true)
  })

  it('removes app-dark class in light mode', async () => {
    document.documentElement.classList.add('app-dark')
    localStorage.setItem('theme-preference', 'light')
    const { useTheme } = await import('@/shared/composables/useTheme')
    useTheme()
    expect(document.documentElement.classList.contains('app-dark')).toBe(false)
  })

  it('toggle cycles light -> dark -> system -> light', async () => {
    localStorage.setItem('theme-preference', 'light')
    const { useTheme } = await import('@/shared/composables/useTheme')
    const { mode, toggle } = useTheme()
    expect(mode.value).toBe('light')
    toggle()
    expect(mode.value).toBe('dark')
    toggle()
    expect(mode.value).toBe('system')
    toggle()
    expect(mode.value).toBe('light')
  })

  it('setMode persists to localStorage', async () => {
    const { useTheme } = await import('@/shared/composables/useTheme')
    const { setMode } = useTheme()
    setMode('dark')
    expect(localStorage.getItem('theme-preference')).toBe('dark')
  })

  it('defaults to system when no stored preference', async () => {
    const { useTheme } = await import('@/shared/composables/useTheme')
    const { mode } = useTheme()
    expect(mode.value).toBe('system')
  })
})
```

- [ ] **Step 3: Run tests to verify they pass**

```bash
cd app/Store && pnpm run test:unit -- --run
```
Expected: 8 tests pass (useTheme). Other test suites unaffected.

- [ ] **Step 4: Run type-check**

```bash
cd app/Store && pnpm run type-check
```
Expected: 0 errors. `useTheme.ts` has no type errors.

- [ ] **Step 5: Commit**

```bash
git add app/Store/src/shared/composables/useTheme.ts app/Store/src/shared/composables/__tests__/useTheme.spec.ts
git commit -m "feat(store): add useTheme composable with dark mode state machine"
```

---

### Task 2: Rewrite styles.scss with full teal token map

**Files:**
- Modify: `app/Store/src/assets/styles.scss` — replace entire content

**Interfaces:**
- Produces: CSS custom property overrides for all PrimeVue 5 components
- Consumes: existing Aura preset (emits CSS vars, we override in cascade)

- [ ] **Step 1: Write the new styles.scss**

Write `app/Store/src/assets/styles.scss`:

```scss
// Storefront Aura theme overrides — Spec A fashion boutique teal palette.
//
// PrimeVue 5 Aura emits one CSS custom property per semantic token as
// --p-{dotted-path->kebab}, e.g. semantic primary.color -> --p-primary-color.
// These :root overrides win the cascade because they appear after Aura's own
// stylesheet injection.

:root {
  // --- Primary scale: teal (legacy --color-primary: #0f766e) ---
  --p-primary-color: #0f766e;            // teal-700
  --p-primary-hover-color: #0d5d56;      // darkened teal-700
  --p-primary-active-color: #115e59;     // teal-800
  --p-primary-contrast-color: #ffffff;   // white text on teal

  --p-primary-50: #f0fdfa;
  --p-primary-100: #ccfbf1;
  --p-primary-200: #99f6e4;
  --p-primary-300: #5eead4;
  --p-primary-400: #2dd4bf;
  --p-primary-500: #14b8a6;
  --p-primary-600: #0d9488;
  --p-primary-700: #0f766e;
  --p-primary-800: #115e59;
  --p-primary-900: #134e4a;
  --p-primary-950: #042f2e;

  // --- Surface scale: warm stone neutral ---
  --p-surface-0: #ffffff;
  --p-surface-50: #fafaf9;
  --p-surface-100: #f5f5f4;
  --p-surface-200: #e7e5e4;
  --p-surface-300: #d6d3d1;
  --p-surface-400: #a8a29e;
  --p-surface-500: #78716c;
  --p-surface-600: #57534e;
  --p-surface-700: #44403c;
  --p-surface-800: #292524;
  --p-surface-900: #1c1917;
  --p-surface-950: #0c0a09;

  // --- Semantic tokens ---
  --p-content-background: #ffffff;
  --p-content-border-color: #e7e5e4;
  --p-text-color: #1c1917;
  --p-text-muted-color: #a8a29e;
  --p-form-field-background: #ffffff;
  --p-form-field-border-color: #d6d3d1;
  --p-form-field-focus-border-color: #0f766e;
  --p-navigation-item-focus-background: #f0fdfa;
  --p-navigation-item-active-background: #ccfbf1;
  --p-highlight-background: #ccfbf1;
  --p-highlight-color: #0f766e;

  // --- State colors ---
  --p-success-color: #16a34a;
  --p-warning-color: #ca8a04;
  --p-danger-color: #dc2626;
  --p-info-color: #2563eb;

  // --- Legacy v4 aliases ---
  --p-surface-ground: #fafaf9;
  --p-surface-card: #ffffff;
}

// --- Dark mode: invert surface/text, brighten primary ---
.app-dark {
  --p-surface-0: #292524;
  --p-surface-50: #1c1917;
  --p-surface-100: #292524;
  --p-surface-200: #44403c;
  --p-surface-300: #57534e;
  --p-surface-400: #78716c;
  --p-surface-500: #a8a29e;
  --p-surface-600: #d6d3d1;
  --p-surface-700: #e7e5e4;
  --p-surface-800: #f5f5f4;
  --p-surface-900: #fafaf9;
  --p-surface-950: #ffffff;

  --p-content-background: #1c1917;
  --p-content-border-color: #44403c;
  --p-text-color: #fafaf9;
  --p-text-muted-color: #a8a29e;

  --p-primary-color: #14b8a6;
  --p-primary-hover-color: #2dd4bf;
  --p-primary-active-color: #0d9488;
  --p-primary-contrast-color: #042f2e;

  --p-primary-50: #042f2e;
  --p-primary-100: #134e4a;
  --p-primary-200: #115e59;
  --p-primary-300: #0f766e;
  --p-primary-400: #0d9488;
  --p-primary-500: #14b8a6;
  --p-primary-600: #2dd4bf;
  --p-primary-700: #5eead4;
  --p-primary-800: #99f6e4;
  --p-primary-900: #ccfbf1;
  --p-primary-950: #f0fdfa;

  --p-surface-ground: #1c1917;
  --p-surface-card: #1c1917;
}

// --- Typography ---
body {
  margin: 0;
  font-family: 'DM Sans', -apple-system, BlinkMacSystemFont, sans-serif;
  font-size: 16px;
  line-height: 1.5;
  background: var(--p-surface-50);
  color: var(--p-text-color);
  -webkit-font-smoothing: antialiased;
}

h1, h2, h3, h4, h5, h6 {
  font-family: 'Playfair Display', Georgia, serif;
  font-weight: 600;
  line-height: 1.25;
}

// --- Animations ---
@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}
@keyframes slideUp {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}
@keyframes slideDown {
  from { opacity: 0; transform: translateY(-20px); }
  to { opacity: 1; transform: translateY(0); }
}
@keyframes scaleIn {
  from { opacity: 0; transform: scale(0.95); }
  to { opacity: 1; transform: scale(1); }
}

.animate-fadeIn    { animation: fadeIn 250ms ease-out; }
.animate-slideUp   { animation: slideUp 250ms ease-out; }
.animate-slideDown { animation: slideDown 250ms ease-out; }
.animate-scaleIn   { animation: scaleIn 250ms ease-out; }

// --- Accessibility ---
@media (prefers-reduced-motion: reduce) {
  .animate-pulse {
    animation: none;
  }
}
```

- [ ] **Step 2: Verify no build errors after the rewrite**

```bash
cd app/Store && pnpm run dev &
sleep 3
curl -s http://localhost:5174 | head -20
```
Expected: Vite dev server starts on port 5174, HTML response includes `<div id="app">`. Page background now `#fafaf9` (warm stone-50), buttons render with teal primary. Kill the dev server with `kill %1` after verifying.

- [ ] **Step 3: Verify type-check (no TypeScript changes, should pass)**

```bash
cd app/Store && pnpm run type-check
```
Expected: 0 errors.

- [ ] **Step 4: Commit**

```bash
git add app/Store/src/assets/styles.scss
git commit -m "feat(store): replace monochrome gray Aura tokens with teal fashion boutique palette"
```

---

### Task 3: Add Google Fonts import

**Files:**
- Modify: `app/Store/src/assets/tailwind.css` — add 1 line

- [ ] **Step 1: Add @import for Google Fonts**

Write `app/Store/src/assets/tailwind.css`:

```css
@import url('https://fonts.googleapis.com/css2?family=DM+Sans:opsz,wght@9..40,400;500;600;700&family=Playfair+Display:ital,wght@0,400;600;700;1,400&display=swap');
@import 'tailwindcss';
@plugin 'tailwindcss-primeui';
```

Important: the font `@import` must appear BEFORE `@import 'tailwindcss'` or after — Tailwind v4 @import syntax rules. Place it on line 1 before `@import 'tailwindcss'`.

- [ ] **Step 2: Verify fonts load in browser**

```bash
cd app/Store && pnpm run dev &
# Open http://localhost:5174 in browser
# DevTools -> Network -> search "googleapis" -> fonts should load (200)
# DevTools -> Elements -> h1 computed font-family = 'Playfair Display'
# DevTools -> Elements -> body computed font-family = 'DM Sans'
kill %1
```

- [ ] **Step 3: Commit**

```bash
git add app/Store/src/assets/tailwind.css
git commit -m "feat(store): add Google Fonts CDN import for Playfair Display and DM Sans"
```

---

### Task 4: Create ThemeToggle component

**Files:**
- Create: `app/Store/src/app/components/ThemeToggle.vue`

**Interfaces:**
- Consumes: `useTheme()` from `@/shared/composables/useTheme`
- Produces: none (standalone button component)

- [ ] **Step 1: Write ThemeToggle.vue**

Write `app/Store/src/app/components/ThemeToggle.vue`:

```vue
<script setup lang="ts">
import { useTheme } from '@/shared/composables/useTheme'

const { isDark, toggle } = useTheme()
</script>
<template>
  <Button
    :icon="isDark ? 'pi pi-sun' : 'pi pi-moon'"
    severity="secondary"
    text
    rounded
    :aria-label="isDark ? 'Switch to light mode' : 'Switch to dark mode'"
    @click="toggle"
  />
</template>
```

- [ ] **Step 2: Run type-check**

```bash
cd app/Store && pnpm run type-check
```
Expected: 0 errors.

- [ ] **Step 3: Commit**

```bash
git add app/Store/src/app/components/ThemeToggle.vue
git commit -m "feat(store): add ThemeToggle component with sun/moon icon"
```

---

### Task 5: Integrate ThemeToggle into AppHeader

**Files:**
- Modify: `app/Store/src/app/components/layout/AppHeader.vue` — add import + component usage

- [ ] **Step 1: Edit AppHeader.vue**

Read the current file first, then apply these changes:

**Add import** (after existing imports, before the store imports):
```ts
import ThemeToggle from '@/app/components/ThemeToggle.vue'
```

**Insert ThemeToggle in template** (in the header actions div, between cart icon and user menu):

Find the cart `<router-link>` block (lines 45-58 in current file). Immediately after the closing `</router-link>` tag of the cart link, insert:
```vue
<!-- Section: Theme Toggle -->
<ThemeToggle />
```

The resulting order in the actions div is: Cart -> ThemeToggle -> User Menu / Sign In -> Mobile Toggle.

- [ ] **Step 2: Run type-check**

```bash
cd app/Store && pnpm run type-check
```
Expected: 0 errors.

- [ ] **Step 3: Verify lint**

```bash
cd app/Store && pnpm run lint
```
Expected: 0 violations.

- [ ] **Step 4: Manual verification**

```bash
cd app/Store && pnpm run dev &
```
Open `http://localhost:5174`, verify:
- Moon icon visible in header (light mode default)
- Click moon -> sun icon appears, page switches to dark mode
- Click sun -> moon icon appears, page switches to light mode
- `localStorage.getItem('theme-preference')` reflects current mode
- Reload page — preference persists

```bash
kill %1
```

- [ ] **Step 5: Commit**

```bash
git add app/Store/src/app/components/layout/AppHeader.vue
git commit -m "feat(store): add ThemeToggle to AppHeader action bar"
```

---

### Task 6: Initialize theme in App.vue on mount

**Files:**
- Modify: `app/Store/src/App.vue` — add 2 lines

- [ ] **Step 1: Edit App.vue**

Read the current file, then apply changes.

**Add import** (after existing imports):
```ts
import { useTheme } from '@/shared/composables/useTheme'
```

**Add init call** (before `const toast = useToast()`):
```ts
// Init: Apply stored theme preference and register OS dark-mode listener.
useTheme()
```

The side effect of calling `useTheme()` reads localStorage, applies `.app-dark` class if dark mode is active, and registers the `prefers-color-scheme` listener. The return value is deliberately not destructured — we only need the init side effect.

- [ ] **Step 2: Run type-check**

```bash
cd app/Store && pnpm run type-check
```
Expected: 0 errors. The call to `useTheme()` without destructuring is valid — TypeScript does not reject unused return values.

- [ ] **Step 3: Verify lint**

```bash
cd app/Store && pnpm run lint
```
Expected: 0 violations. The unused return value from `useTheme()` is intentional and should not trigger lint warnings (Vue composables are allowed to be called for side effects).

- [ ] **Step 4: Commit**

```bash
git add app/Store/src/App.vue
git commit -m "feat(store): initialize theme on app mount with useTheme composable"
```

---

### Task 7: Final verification — end-to-end

- [ ] **Step 1: Run full type-check + lint + unit tests**

```bash
cd app/Store && pnpm run type-check && pnpm run lint && pnpm run test:unit -- --run
```
Expected: 0 type errors, 0 lint violations, all unit tests pass.

- [ ] **Step 2: Manual dark mode verification checklist**

```bash
cd app/Store && pnpm run dev &
```

1. Open `http://localhost:5174` — PrimeVue Button renders with teal background (`#0f766e`), not gray
2. Open `http://localhost:5174/shop` — Paginator component renders with teal active page
3. Open `http://localhost:5174/login` — InputText focus ring is teal, not slate
4. Click sun/moon icon in AppHeader -> dark mode activates
5. Verify dark mode: card backgrounds darken to `#1c1917`, text lightens to `#fafaf9`, teal primary brightens to `#14b8a6`
6. Reload page -> dark mode persists (localStorage key = 'dark')
7. Toggle to system mode -> if OS is in dark mode, app follows
8. Change OS dark mode while app is in 'system' mode -> app follows automatically

```bash
kill %1
```

- [ ] **Step 3: Commit (if no issues)**

If all checks pass, plan is complete. No further commits needed for verification.

---

## Verification

1. `pnpm run type-check` — 0 errors
2. `pnpm run lint` — 0 violations
3. `pnpm run test:unit -- --run` — all tests pass (including useTheme.spec.ts)
4. Open storefront — teal primary visible on all PrimeVue components
5. Dark mode toggle works, persists across reload
6. System preference detection works when mode = 'system'
7. Playfair Display font renders on headings, DM Sans on body
8. Animations (fadeIn, slideUp, slideDown, scaleIn) defined and usable
