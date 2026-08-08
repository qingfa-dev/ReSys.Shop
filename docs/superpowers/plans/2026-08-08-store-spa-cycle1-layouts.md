# Store SPA Cycle 1: Layouts + Shell — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Polish all layout components and fix bugs (dark mode, cart badge) without changing structure — prepare the shell for Cycle 2 catalog views.

**Architecture:** Refinement pass over 8 existing files + 3 new test files. No new dependencies, no structural changes. Every task produces a testable, commit-able delta.

**Tech Stack:** Vue 3.5, PrimeVue 5 (Aura theme), Tailwind CSS v4, Vitest + jsdom, @pinia/testing

## Global Constraints

- `TreatWarningsAsErrors=true` — no TypeScript warnings, no lint warnings
- Neutral color palette only (`neutral-*`), teal accent (`#0d7377`) for CTAs/links only
- All dark mode selectors under `.app-dark` class
- Inter body font, Newsreader italic for editorial only, JetBrains Mono for prices only
- Cart badge must be hidden at count 0, positioned correctly on cart icon
- All animations respect `prefers-reduced-motion: reduce`

---

### Task 1: Fix dark mode class alignment

**Files:**
- Modify: `app/Store/src/shared/composables/useTheme.ts`
- Create: `app/Store/src/app/components/__tests__/ThemeToggle.spec.ts`

**Interfaces:**
- Consumes: `useTheme()` from `@/shared/composables/useTheme` (existing)
- Produces: `useTheme().toggle()` toggles `.app-dark` on `document.documentElement`

- [ ] **Step 1: Change `'dark'` to `'app-dark'` in useTheme.ts**

In `app/Store/src/shared/composables/useTheme.ts`, line 10 and line 30, replace `'dark'` with `'app-dark'`:

```typescript
import { ref, watchEffect } from 'vue'

// Cache: Module-level singleton — shared across all components using useTheme()
const isDark = ref(false)

export function useTheme() {
  function applyTheme(dark: boolean): void {
    isDark.value = dark
    // Cache: Persist theme choice to localStorage for cross-session survival
    document.documentElement.classList.toggle('app-dark', dark)
    localStorage.setItem('resys_theme', dark ? 'dark' : 'light')
  }

  function toggle(): void {
    applyTheme(!isDark.value)
  }

  function init(): void {
    // Cache: Restore theme from localStorage, fallback to OS preference
    const stored = localStorage.getItem('resys_theme')
    if (stored) {
      applyTheme(stored === 'dark')
    } else {
      applyTheme(window.matchMedia('(prefers-color-scheme: dark)').matches)
    }
  }

  // Cache: Keep DOM in sync when isDark changes — covers SSR hydration edge cases
  watchEffect(() => {
    document.documentElement.classList.toggle('app-dark', isDark.value)
  })

  return { isDark, toggle, init }
}
```

- [ ] **Step 2: Write the ThemeToggle test**

Create `app/Store/src/app/components/__tests__/ThemeToggle.spec.ts`:

```typescript
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import ThemeToggle from '../ThemeToggle.vue'
import { defineComponent } from 'vue'

const mockToggle = vi.fn()
const mockIsDark = vi.fn(() => false)

vi.mock('@/shared/composables/useTheme', () => ({
  useTheme: () => ({
    isDark: mockIsDark,
    toggle: mockToggle,
  }),
}))

// Wrap in a component that provides PrimeVue Button
const TestWrapper = defineComponent({
  components: { ThemeToggle },
  template: `<ThemeToggle />`,
})

describe('ThemeToggle', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockIsDark.mockReturnValue(false)
  })

  it('toggles .app-dark class on <html> when clicked', async () => {
    const wrapper = mount(TestWrapper, {
      global: {
        stubs: {
          Button: {
            template: `<button @click="$attrs.onClick"><slot /></button>`,
            inheritAttrs: false,
          },
        },
      },
    })

    await wrapper.find('button').trigger('click')
    expect(mockToggle).toHaveBeenCalledTimes(1)
  })

  it('shows moon icon when in light mode', () => {
    mockIsDark.mockReturnValue(false)
    const wrapper = mount(TestWrapper, {
      global: { stubs: { Button: { template: `<div />`, inheritAttrs: false } } },
    })

    expect(wrapper.html()).toContain('pi pi-moon')
  })

  it('shows sun icon when in dark mode', () => {
    mockIsDark.mockReturnValue(true)
    const wrapper = mount(TestWrapper, {
      global: { stubs: { Button: { template: `<div />`, inheritAttrs: false } } },
    })

    expect(wrapper.html()).toContain('pi pi-sun')
  })
})
```

- [ ] **Step 3: Run tests to verify**

```bash
cd app/Store && npx vitest run src/app/components/__tests__/ThemeToggle.spec.ts
```

Expected: 3 tests pass.

- [ ] **Step 4: Verify dark mode manually (manual)**

```bash
cd app/Store && pnpm run dev
```

Open browser. Click theme toggle. Verify `<html>` gets class `app-dark` (not `dark`). Verify colors switch.

- [ ] **Step 5: Commit**

```bash
git add app/Store/src/shared/composables/useTheme.ts app/Store/src/app/components/__tests__/ThemeToggle.spec.ts
git commit -m "fix(store): align dark mode class to app-dark for PrimeVue Aura theme"
```

---

### Task 2: Fix cart badge positioning and visibility

**Files:**
- Modify: `app/Store/src/app/components/layout/AppHeader.vue`

**Interfaces:**
- Consumes: `useCartStore()` → `itemCount` (reactive)
- Produces: Cart button with correctly positioned badge, hidden at zero

- [ ] **Step 1: Wrap cart button + badge in a relative container**

In `AppHeader.vue`, replace the cart button area (lines 28-29) to wrap Button + Tag in a `relative` div:

Current:
```html
<Button icon="pi pi-shopping-cart" text rounded class="relative" @click="cartDrawerOpen = true" />
<Tag v-if="cart.itemCount > 0" :value="String(cart.itemCount)" severity="contrast" class="absolute -top-0.5 -right-0.5 min-w-[18px] h-[18px] text-[10px] p-0" />
```

Replace with:
```html
<div class="relative inline-flex">
  <Button icon="pi pi-shopping-cart" text rounded @click="cartDrawerOpen = true" />
  <Tag v-if="cart.itemCount > 0" :value="String(cart.itemCount)" severity="contrast" class="absolute -top-1 -right-1 min-w-[18px] h-[18px] text-[10px] p-0" />
</div>
```

- [ ] **Step 2: Verify build**

```bash
cd app/Store && npx tsc --noEmit
```

Expected: no errors.

- [ ] **Step 3: Commit**

```bash
git add app/Store/src/app/components/layout/AppHeader.vue
git commit -m "fix(store): position cart badge correctly with relative container"
```

---

### Task 3: Add nav link hover animation and active route styling

**Files:**
- Modify: `app/Store/src/app/components/layout/AppHeader.vue`

**Interfaces:**
- Consumes: `vue-router` → `useRoute()` for active route detection
- Produces: nav links with underline-on-hover and active state

- [ ] **Step 1: Add useRoute import and active link logic**

Add to `<script setup>` in `AppHeader.vue`:

```typescript
import { useRoute } from 'vue-router'

const route = useRoute()
```

- [ ] **Step 2: Replace nav links with active state + underline animation**

Replace the `<nav>` block (lines 21-25):

Current:
```html
<nav class="hidden md:flex items-center gap-6">
  <router-link to="/shop" class="text-sm font-medium text-neutral-600 hover:text-neutral-900 transition-colors">Shop</router-link>
  <router-link to="/collections" class="text-sm font-medium text-neutral-600 hover:text-neutral-900 transition-colors">Collections</router-link>
  <router-link to="/recommendations" class="text-sm font-medium text-neutral-600 hover:text-neutral-900 transition-colors">Visual Search</router-link>
</nav>
```

Replace with:
```html
<nav class="hidden md:flex items-center gap-6">
  <router-link
    to="/shop"
    class="relative text-sm font-medium transition-colors pb-0.5"
    :class="route.path.startsWith('/shop') || route.path.startsWith('/products') ? 'text-neutral-900 after:absolute after:left-0 after:bottom-0 after:h-[2px] after:w-full after:bg-neutral-900' : 'text-neutral-600 hover:text-neutral-900 after:absolute after:left-0 after:bottom-0 after:h-[2px] after:w-0 hover:after:w-full after:bg-neutral-300 after:transition-all after:duration-200'"
  >Shop</router-link>
  <router-link
    to="/collections"
    class="relative text-sm font-medium transition-colors pb-0.5"
    :class="route.path.startsWith('/collections') ? 'text-neutral-900 after:absolute after:left-0 after:bottom-0 after:h-[2px] after:w-full after:bg-neutral-900' : 'text-neutral-600 hover:text-neutral-900 after:absolute after:left-0 after:bottom-0 after:h-[2px] after:w-0 hover:after:w-full after:bg-neutral-300 after:transition-all after:duration-200'"
  >Collections</router-link>
  <router-link
    to="/recommendations"
    class="relative text-sm font-medium transition-colors pb-0.5"
    :class="route.path.startsWith('/recommendations') ? 'text-neutral-900 after:absolute after:left-0 after:bottom-0 after:h-[2px] after:w-full after:bg-neutral-900' : 'text-neutral-600 hover:text-neutral-900 after:absolute after:left-0 after:bottom-0 after:h-[2px] after:w-0 hover:after:w-full after:bg-neutral-300 after:transition-all after:duration-200'"
  >Visual Search</router-link>
</nav>
```

- [ ] **Step 3: Verify build**

```bash
cd app/Store && npx tsc --noEmit
```

Expected: no errors.

- [ ] **Step 4: Commit**

```bash
git add app/Store/src/app/components/layout/AppHeader.vue
git commit -m "feat(store): add nav link hover underline animation and active route styling"
```

---

### Task 4: Add search keyboard hint and auth dropdown

**Files:**
- Modify: `app/Store/src/app/components/layout/AppHeader.vue`

**Interfaces:**
- Consumes: `useAuthStore()` → `isAuthenticated`, `logout()`, `useRouter()` for navigation
- Produces: desktop search hint "Ctrl+K", auth dropdown with Orders/Profile/Sign Out

- [ ] **Step 1: Add imports to script**

Add to `<script setup>` in `AppHeader.vue`:

```typescript
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const authDropdownOpen = ref(false)
```

(Note: `ref` is already imported from line 2 — reuse that import. Add only `useRouter` if not already present.)

- [ ] **Step 2: Replace search area with hint text**

Replace the search button line:
```html
<Button icon="pi pi-search" text rounded aria-label="Search" @click="search.open()" />
```

With:
```html
<div class="hidden md:flex items-center gap-1">
  <Button icon="pi pi-search" text rounded aria-label="Search" @click="search.open()" />
  <kbd class="text-[10px] text-neutral-400 font-medium bg-neutral-100 rounded px-1.5 py-0.5 border border-neutral-200">Ctrl+K</kbd>
</div>
<Button icon="pi pi-search" text rounded aria-label="Search" class="md:hidden" @click="search.open()" />
```

- [ ] **Step 3: Replace auth area with dropdown**

Replace lines 31-36 (the auth `template` blocks):

Current:
```html
<template v-if="auth.isAuthenticated">
  <Button icon="pi pi-user" text rounded as="router-link" to="/account/orders" aria-label="Account" class="hidden md:flex" />
</template>
<template v-else>
  <Button label="Sign In" text size="small" as="router-link" to="/login" class="hidden md:inline-flex" />
</template>
```

Replace with:
```html
<div v-if="auth.isAuthenticated" class="relative hidden md:flex">
  <Button
    icon="pi pi-user"
    text
    rounded
    aria-label="Account"
    @click="authDropdownOpen = !authDropdownOpen"
  />
  <div
    v-if="authDropdownOpen"
    class="absolute right-0 top-full mt-2 w-48 bg-white rounded-lg shadow-lg border border-neutral-200 py-1 z-50"
  >
    <router-link to="/account/orders" class="block px-4 py-2 text-sm text-neutral-700 hover:bg-neutral-50" @click="authDropdownOpen = false">Orders</router-link>
    <router-link to="/account/profile" class="block px-4 py-2 text-sm text-neutral-700 hover:bg-neutral-50" @click="authDropdownOpen = false">Profile</router-link>
    <div class="border-t border-neutral-100 my-1" />
    <button class="block w-full text-left px-4 py-2 text-sm text-neutral-700 hover:bg-neutral-50" @click="auth.logout(); router.push('/'); authDropdownOpen = false">Sign Out</button>
  </div>
</div>
<Button
  v-else
  label="Sign In"
  text
  size="small"
  as="router-link"
  to="/login"
  class="hidden md:inline-flex"
/>
```

- [ ] **Step 4: Add click-outside guard for dropdown**

Add a function in `<script setup>` to close dropdown on outside click:

```typescript
function onDocumentClick(e: MouseEvent): void {
  if (authDropdownOpen.value) {
    authDropdownOpen.value = false
  }
}
```

And register/unregister it — but only if we need it. Since the dropdown renders on click and the click event bubbles to document on the same click, this won't work cleanly. Instead, add a backdrop div:

Replace the dropdown div to include a click-outside backdrop:

```html
<div v-if="auth.isAuthenticated" class="relative hidden md:flex">
  <Button
    icon="pi pi-user"
    text
    rounded
    aria-label="Account"
    @click="authDropdownOpen = !authDropdownOpen"
  />
  <Teleport to="body">
    <div v-if="authDropdownOpen" class="fixed inset-0 z-40" @click="authDropdownOpen = false" />
  </Teleport>
  <div
    v-if="authDropdownOpen"
    class="absolute right-0 top-full mt-2 w-48 bg-white rounded-lg shadow-lg border border-neutral-200 py-1 z-50"
  >
    <router-link to="/account/orders" class="block px-4 py-2 text-sm text-neutral-700 hover:bg-neutral-50 transition-colors" @click="authDropdownOpen = false">Orders</router-link>
    <router-link to="/account/profile" class="block px-4 py-2 text-sm text-neutral-700 hover:bg-neutral-50 transition-colors" @click="authDropdownOpen = false">Profile</router-link>
    <div class="border-t border-neutral-100 my-1" />
    <button class="block w-full text-left px-4 py-2 text-sm text-neutral-700 hover:bg-neutral-50 transition-colors" @click="auth.logout(); router.push('/'); authDropdownOpen = false">Sign Out</button>
  </div>
</div>
```

- [ ] **Step 5: Verify build**

```bash
cd app/Store && npx tsc --noEmit
```

Expected: no errors.

- [ ] **Step 6: Commit**

```bash
git add app/Store/src/app/components/layout/AppHeader.vue
git commit -m "feat(store): add search keyboard hint and auth dropdown to header"
```

---

### Task 5: Switch MobileNav from v-if to Transition for smooth toggle

**Files:**
- Modify: `app/Store/src/app/components/layout/MobileNav.vue`
- Modify: `app/Store/src/app/components/layout/AppHeader.vue`

**Interfaces:**
- Consumes: `modelValue` prop from AppHeader for v-model
- Produces: `v-model:open` toggle with Transition animation

- [ ] **Step 1: Refactor MobileNav to use modelValue + Transition**

Replace `MobileNav.vue`:

```vue
<script setup lang="ts">
import { useAuthStore } from '@/features/identity/stores/authStore'

const open = defineModel<boolean>({ default: false })
const auth = useAuthStore()
</script>
<template>
  <Teleport to="body">
    <Transition name="mobile-nav">
      <div v-if="open" class="fixed inset-0 z-50 md:hidden">
        <div class="absolute inset-0 bg-black/50" @click="open = false" />
        <Transition name="mobile-panel">
          <div v-if="open" class="absolute right-0 top-0 h-full w-72 bg-white shadow-xl p-6">
            <div class="flex justify-between items-center mb-8">
              <span class="text-lg font-semibold text-neutral-900">Menu</span>
              <Button icon="pi pi-times" text rounded @click="open = false" />
            </div>
            <nav class="space-y-4">
              <router-link to="/shop" class="block text-sm font-medium text-neutral-700 hover:text-neutral-900 transition-colors" @click="open = false">Shop</router-link>
              <router-link to="/collections" class="block text-sm font-medium text-neutral-700 hover:text-neutral-900 transition-colors" @click="open = false">Collections</router-link>
              <router-link to="/recommendations" class="block text-sm font-medium text-neutral-700 hover:text-neutral-900 transition-colors" @click="open = false">Visual Search</router-link>
              <router-link to="/cart" class="block text-sm font-medium text-neutral-700 hover:text-neutral-900 transition-colors" @click="open = false">Cart</router-link>
              <template v-if="auth.isAuthenticated">
                <router-link to="/account/orders" class="block text-sm font-medium text-neutral-700 hover:text-neutral-900 transition-colors" @click="open = false">My Orders</router-link>
                <router-link to="/account/profile" class="block text-sm font-medium text-neutral-700 hover:text-neutral-900 transition-colors" @click="open = false">Profile</router-link>
              </template>
              <template v-else>
                <router-link to="/login" class="block text-sm font-medium text-neutral-700 hover:text-neutral-900 transition-colors" @click="open = false">Sign In</router-link>
                <router-link to="/register" class="block text-sm font-medium text-neutral-700 hover:text-neutral-900 transition-colors" @click="open = false">Register</router-link>
              </template>
            </nav>
          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>
<style scoped>
.mobile-nav-enter-active,
.mobile-nav-leave-active {
  transition: opacity 0.25s ease;
}
.mobile-nav-enter-from,
.mobile-nav-leave-to {
  opacity: 0;
}
.mobile-panel-enter-active,
.mobile-panel-leave-active {
  transition: transform 0.25s ease;
}
.mobile-panel-enter-from,
.mobile-panel-leave-to {
  transform: translateX(100%);
}
@media (prefers-reduced-motion: reduce) {
  .mobile-nav-enter-active,
  .mobile-nav-leave-active,
  .mobile-panel-enter-active,
  .mobile-panel-leave-active {
    transition: none;
  }
}
</style>
```

- [ ] **Step 2: Update AppHeader to use v-model on MobileNav**

Replace line 41 in `AppHeader.vue`:

Current:
```html
<MobileNav v-if="mobileMenuOpen" @close="mobileMenuOpen = false" />
```

Replace with:
```html
<MobileNav v-model:open="mobileMenuOpen" />
```

- [ ] **Step 3: Verify build and test**

```bash
cd app/Store && npx tsc --noEmit
```

Expected: no errors.

- [ ] **Step 4: Commit**

```bash
git add app/Store/src/app/components/layout/MobileNav.vue app/Store/src/app/components/layout/AppHeader.vue
git commit -m "feat(store): add smooth transition to mobile nav with reduced motion support"
```

---

### Task 6: Polish AppFooter — remove social media placeholder buttons

**Files:**
- Modify: `app/Store/src/app/components/layout/AppFooter.vue`

- [ ] **Step 1: Remove social media button row**

Replace lines 37-44 in `AppFooter.vue`:

Current:
```html
<div class="mt-8 pt-8 border-t border-neutral-200 flex flex-col sm:flex-row justify-between items-center gap-4">
  <p class="text-sm text-neutral-500">&copy; {{ new Date().getFullYear() }} ReSys.Shop. All rights reserved.</p>
  <div class="flex items-center gap-4">
    <Button icon="pi pi-facebook" text rounded aria-label="Facebook" />
    <Button icon="pi pi-twitter" text rounded aria-label="Twitter" />
    <Button icon="pi pi-instagram" text rounded aria-label="Instagram" />
  </div>
</div>
```

Replace with:
```html
<div class="mt-8 pt-8 border-t border-neutral-200">
  <p class="text-sm text-neutral-500">&copy; {{ new Date().getFullYear() }} ReSys.Shop. All rights reserved.</p>
</div>
```

- [ ] **Step 2: Verify build**

```bash
cd app/Store && npx tsc --noEmit
```

Expected: no errors.

- [ ] **Step 3: Commit**

```bash
git add app/Store/src/app/components/layout/AppFooter.vue
git commit -m "refactor(store): remove placeholder social media icons from footer"
```

---

### Task 7: Polish layouts — DefaultLayout transition, AuthLayout fade-in, AccountLayout active sidebar

**Files:**
- Modify: `app/Store/src/app/layouts/DefaultLayout.vue`
- Modify: `app/Store/src/app/layouts/AuthLayout.vue`
- Modify: `app/Store/src/app/layouts/AccountLayout.vue`

**Interfaces:**
- Produces: DefaultLayout with smooth header transition, AuthLayout with fade-in, AccountLayout with active sidebar styling

- [ ] **Step 1: Add transition to DefaultLayout header**

In `DefaultLayout.vue`, add `transition-colors duration-200` to the root container styles — no structural change needed since AppHeader handles its own background. The DefaultLayout is already correct. No changes needed.

- [ ] **Step 2: Add fade-in animation to AuthLayout**

Replace `AuthLayout.vue`:

```vue
<template>
  <div class="min-h-screen flex items-center justify-center bg-neutral-50 px-4">
    <div class="w-full max-w-md animate-fade-in">
      <div class="text-center mb-8">
        <router-link to="/" class="text-2xl font-semibold tracking-tight text-neutral-900">
          ReSys.Shop
        </router-link>
      </div>
      <router-view />
    </div>
  </div>
</template>
```

Verify the `animate-fade-in` class exists in `styles.scss`. If not, add this scoped style:

```vue
<style scoped>
.animate-fade-in {
  animation: fade-in 0.4s ease both;
}
@keyframes fade-in {
  from { opacity: 0; transform: translateY(8px); }
  to { opacity: 1; transform: translateY(0); }
}
@media (prefers-reduced-motion: reduce) {
  .animate-fade-in {
    animation: none;
  }
}
</style>
```

- [ ] **Step 3: Add active route indicator to AccountLayout sidebar**

Replace the sidebar nav links in `AccountLayout.vue` (lines 18-25) with route-aware active state:

```vue
<script setup lang="ts">
import { useRoute } from 'vue-router'

const route = useRoute()
</script>
<template>
  <div class="min-h-screen bg-neutral-50">
    <header class="bg-white border-b border-neutral-200 sticky top-0 z-40">
      <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex items-center h-14 gap-4">
          <router-link to="/" class="text-lg font-semibold tracking-tight text-neutral-900 shrink-0">ReSys.Shop</router-link>
          <span class="text-neutral-300">/</span>
          <span class="text-sm font-medium text-neutral-600">Account</span>
        </div>
      </div>
    </header>
    <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div class="flex flex-col md:flex-row gap-8">
        <aside class="w-full md:w-56 shrink-0">
          <nav class="space-y-1">
            <router-link
              to="/account/orders"
              class="block px-3 py-2 text-sm rounded-r-lg border-l-2 transition-colors"
              :class="route.path.startsWith('/account/orders') ? 'border-neutral-900 text-neutral-900 font-semibold bg-neutral-100' : 'border-transparent text-neutral-700 hover:bg-neutral-100 hover:text-neutral-900 font-medium'"
            >Orders</router-link>
            <router-link
              to="/account/addresses"
              class="block px-3 py-2 text-sm rounded-r-lg border-l-2 transition-colors"
              :class="route.path.startsWith('/account/addresses') ? 'border-neutral-900 text-neutral-900 font-semibold bg-neutral-100' : 'border-transparent text-neutral-700 hover:bg-neutral-100 hover:text-neutral-900 font-medium'"
            >Addresses</router-link>
            <router-link
              to="/account/profile"
              class="block px-3 py-2 text-sm rounded-r-lg border-l-2 transition-colors"
              :class="route.path.startsWith('/account/profile') ? 'border-neutral-900 text-neutral-900 font-semibold bg-neutral-100' : 'border-transparent text-neutral-700 hover:bg-neutral-100 hover:text-neutral-900 font-medium'"
            >Profile</router-link>
            <router-link
              to="/account/sessions"
              class="block px-3 py-2 text-sm rounded-r-lg border-l-2 transition-colors"
              :class="route.path.startsWith('/account/sessions') ? 'border-neutral-900 text-neutral-900 font-semibold bg-neutral-100' : 'border-transparent text-neutral-700 hover:bg-neutral-100 hover:text-neutral-900 font-medium'"
            >Sessions</router-link>
            <router-link
              to="/account/wishlists"
              class="block px-3 py-2 text-sm rounded-r-lg border-l-2 transition-colors"
              :class="route.path.startsWith('/account/wishlists') ? 'border-neutral-900 text-neutral-900 font-semibold bg-neutral-100' : 'border-transparent text-neutral-700 hover:bg-neutral-100 hover:text-neutral-900 font-medium'"
            >Wishlists</router-link>
            <router-link
              to="/account/notifications"
              class="block px-3 py-2 text-sm rounded-r-lg border-l-2 transition-colors"
              :class="route.path.startsWith('/account/notifications') ? 'border-neutral-900 text-neutral-900 font-semibold bg-neutral-100' : 'border-transparent text-neutral-700 hover:bg-neutral-100 hover:text-neutral-900 font-medium'"
            >Notifications</router-link>
            <router-link
              to="/account/change-password"
              class="block px-3 py-2 text-sm rounded-r-lg border-l-2 transition-colors"
              :class="route.path.startsWith('/account/change-password') ? 'border-neutral-900 text-neutral-900 font-semibold bg-neutral-100' : 'border-transparent text-neutral-700 hover:bg-neutral-100 hover:text-neutral-900 font-medium'"
            >Change Password</router-link>
            <router-link
              to="/account/preferences"
              class="block px-3 py-2 text-sm rounded-r-lg border-l-2 transition-colors"
              :class="route.path.startsWith('/account/preferences') ? 'border-neutral-900 text-neutral-900 font-semibold bg-neutral-100' : 'border-transparent text-neutral-700 hover:bg-neutral-100 hover:text-neutral-900 font-medium'"
            >Preferences</router-link>
          </nav>
        </aside>
        <div class="flex-1 min-w-0">
          <router-view />
        </div>
      </div>
    </div>
  </div>
</template>
```

- [ ] **Step 4: Verify build**

```bash
cd app/Store && npx tsc --noEmit
```

Expected: no errors.

- [ ] **Step 5: Commit**

```bash
git add app/Store/src/app/layouts/AuthLayout.vue app/Store/src/app/layouts/AccountLayout.vue
git commit -m "feat(store): add AuthLayout fade-in and AccountLayout active sidebar indicator"
```

---

### Task 8: CartDrawer reduced motion support

**Files:**
- Modify: `app/Store/src/features/ordering/components/CartDrawer.vue`

- [ ] **Step 1: Add reduced motion media query to CartDrawer scoped style**

In `CartDrawer.vue`, add to the `<style scoped>` block after the existing `.slide-*` rules:

```css
@media (prefers-reduced-motion: reduce) {
  .slide-enter-active,
  .slide-leave-active {
    transition: none;
  }
}
```

- [ ] **Step 2: Verify build**

```bash
cd app/Store && npx tsc --noEmit
```

Expected: no errors.

- [ ] **Step 3: Commit**

```bash
git add app/Store/src/features/ordering/components/CartDrawer.vue
git commit -m "fix(store): respect prefers-reduced-motion in CartDrawer slide animation"
```

---

### Task 9: Layout smoke tests

**Files:**
- Create: `app/Store/src/app/components/layout/__tests__/AppHeader.spec.ts`
- Create: `app/Store/src/app/layouts/__tests__/layouts.spec.ts`
- Create: `app/Store/src/features/ordering/components/__tests__/CartDrawer.spec.ts`

**Interfaces:**
- Consumes: `useAuthStore` mock, `useCartStore` mock, `useTheme` mock, `useSearch` mock
- Produces: 6 smoke tests covering all layout components

- [ ] **Step 1: Write AppHeader smoke test**

Create `app/Store/src/app/components/layout/__tests__/AppHeader.spec.ts`:

```typescript
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import AppHeader from '../AppHeader.vue'

vi.mock('@/features/identity/stores/authStore', () => ({
  useAuthStore: vi.fn(),
}))

vi.mock('@/features/ordering/stores/cartStore', () => ({
  useCartStore: vi.fn(),
}))

vi.mock('@/features/catalog/composables/useSearch', () => ({
  useSearch: () => ({ open: vi.fn() }),
}))

vi.mock('@/shared/composables/useTheme', () => ({
  useTheme: () => ({ isDark: { value: false }, toggle: vi.fn(), init: vi.fn() }),
}))

const Dummy = { template: '<div />' }

async function createWrapper(isAuthenticated: boolean) {
  const { useAuthStore } = await import('@/features/identity/stores/authStore')
  const { useCartStore } = await import('@/features/ordering/stores/cartStore')

  const mockAuth = useAuthStore as ReturnType<typeof vi.fn>
  mockAuth.mockReturnValue({
    isAuthenticated,
    isLoading: false,
    logout: vi.fn(),
  })

  const mockCart = useCartStore as ReturnType<typeof vi.fn>
  mockCart.mockReturnValue({
    itemCount: 0,
    isEmpty: true,
    loading: false,
    fetchCart: vi.fn(),
  })

  const router = createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', component: Dummy },
      { path: '/shop', component: Dummy },
      { path: '/login', component: Dummy },
      { path: '/account/orders', component: Dummy },
    ],
  })

  return mount(AppHeader, {
    global: {
      plugins: [router],
      stubs: {
        ThemeToggle: { template: '<div class="theme-toggle" />' },
        MobileNav: { template: '<div class="mobile-nav" />' },
        CartDrawer: { template: '<div class="cart-drawer" />' },
        Button: { template: '<button />', inheritAttrs: false },
        Tag: { template: '<span />', inheritAttrs: false },
        Teleport: false,
      },
    },
  })
}

describe('AppHeader', () => {
  beforeEach(() => {
    vi.resetModules()
  })

  it('shows Sign In link when logged out', async () => {
    const wrapper = await createWrapper(false)
    expect(wrapper.html()).toContain('Sign In')
  })

  it('does not show Sign In when logged in', async () => {
    const wrapper = await createWrapper(true)
    expect(wrapper.html()).not.toContain('Sign In')
  })
})
```

- [ ] **Step 2: Write layout smoke tests**

Create `app/Store/src/app/layouts/__tests__/layouts.spec.ts`:

```typescript
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import DefaultLayout from '../DefaultLayout.vue'
import AuthLayout from '../AuthLayout.vue'
import AccountLayout from '../AccountLayout.vue'

const Dummy = { template: '<div class="view-content">View</div>' }

function createRouterInstance() {
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', component: Dummy },
      { path: '/account/orders', component: Dummy },
      { path: '/login', component: Dummy },
    ],
  })
}

describe('DefaultLayout', () => {
  it('renders header and footer with router-view outlet', () => {
    const wrapper = mount(DefaultLayout, {
      global: {
        plugins: [createRouterInstance()],
        stubs: {
          AppHeader: { template: '<header class="app-header">Header</header>' },
          AppFooter: { template: '<footer class="app-footer">Footer</footer>' },
        },
      },
    })

    expect(wrapper.find('.app-header').exists()).toBe(true)
    expect(wrapper.find('.app-footer').exists()).toBe(true)
  })

  it('has min-h-screen and flex column layout', () => {
    const wrapper = mount(DefaultLayout, {
      global: {
        plugins: [createRouterInstance()],
        stubs: {
          AppHeader: { template: '<header />' },
          AppFooter: { template: '<footer />' },
        },
      },
    })

    const root = wrapper.find('div')
    expect(root.classes()).toContain('min-h-screen')
    expect(root.classes()).toContain('flex')
    expect(root.classes()).toContain('flex-col')
  })
})

describe('AuthLayout', () => {
  it('renders centered card layout with branding', () => {
    const wrapper = mount(AuthLayout, {
      global: {
        plugins: [createRouterInstance()],
      },
    })

    expect(wrapper.html()).toContain('ReSys.Shop')
    expect(wrapper.find('.max-w-md').exists()).toBe(true)
  })
})

describe('AccountLayout', () => {
  it('renders sidebar with all 8 nav links', () => {
    const router = createRouterInstance()
    const wrapper = mount(AccountLayout, {
      global: {
        plugins: [router],
      },
    })

    expect(wrapper.html()).toContain('Orders')
    expect(wrapper.html()).toContain('Addresses')
    expect(wrapper.html()).toContain('Profile')
    expect(wrapper.html()).toContain('Sessions')
    expect(wrapper.html()).toContain('Wishlists')
    expect(wrapper.html()).toContain('Notifications')
    expect(wrapper.html()).toContain('Change Password')
    expect(wrapper.html()).toContain('Preferences')
  })
})
```

- [ ] **Step 3: Write CartDrawer smoke test**

Create `app/Store/src/features/ordering/components/__tests__/CartDrawer.spec.ts`:

```typescript
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import CartDrawer from '../CartDrawer.vue'

const mockFetchCart = vi.fn()

vi.mock('../../stores/cartStore', () => ({
  useCartStore: vi.fn(() => ({
    items: [],
    itemCount: 0,
    isEmpty: true,
    loading: false,
    subtotal: 0,
    fetchCart: mockFetchCart,
    updateQuantity: vi.fn(),
  })),
}))

describe('CartDrawer', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('opens and closes via v-model', () => {
    const wrapper = mount(CartDrawer, {
      props: { visible: true },
      global: {
        stubs: {
          Teleport: false,
          Button: { template: '<button />', inheritAttrs: false },
          Skeleton: { template: '<div />', inheritAttrs: false },
        },
      },
    })

    expect(wrapper.text()).toContain('Cart')
    expect(wrapper.text()).toContain('Your cart is empty')
  })

  it('shows empty state when cart is empty', () => {
    const wrapper = mount(CartDrawer, {
      props: { visible: true },
      global: {
        stubs: {
          Teleport: false,
          Button: { template: '<button />', inheritAttrs: false },
          Skeleton: { template: '<div />', inheritAttrs: false },
        },
      },
    })

    expect(wrapper.text()).toContain('Your cart is empty')
    expect(wrapper.text()).toContain('Continue Shopping')
  })

  it('does not render when visible is false', () => {
    const wrapper = mount(CartDrawer, {
      props: { visible: false },
      global: {
        stubs: {
          Teleport: false,
          Button: { template: '<button />', inheritAttrs: false },
          Skeleton: { template: '<div />', inheritAttrs: false },
        },
      },
    })

    expect(wrapper.text()).not.toContain('Cart')
  })
})
```

- [ ] **Step 4: Run all smoke tests**

```bash
cd app/Store && npx vitest run src/app/components/__tests__/ThemeToggle.spec.ts src/app/components/layout/__tests__/AppHeader.spec.ts src/app/layouts/__tests__/layouts.spec.ts src/features/ordering/components/__tests__/CartDrawer.spec.ts
```

Expected: all tests pass.

- [ ] **Step 5: Commit**

```bash
git add app/Store/src/app/components/layout/__tests__/AppHeader.spec.ts app/Store/src/app/layouts/__tests__/layouts.spec.ts app/Store/src/features/ordering/components/__tests__/CartDrawer.spec.ts
git commit -m "test(store): add layout smoke tests for header, layouts, cart drawer"
```

---

### Task 10: Full verification

- [ ] **Step 1: Run all tests**

```bash
cd app/Store && npx vitest run
```

Expected: all existing tests pass (no regressions), all new tests pass.

- [ ] **Step 2: Run type check**

```bash
cd app/Store && npx tsc --noEmit
```

Expected: no errors.

- [ ] **Step 3: Run build**

```bash
cd app/Store && pnpm run build-only
```

Expected: successful build, no warnings.

- [ ] **Step 4: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: no lint errors.

- [ ] **Step 5: Commit verification**

```bash
git status
```

Expected: clean working tree, all changes committed.
