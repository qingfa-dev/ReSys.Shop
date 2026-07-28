# Admin User Menu & Logout Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add user avatar+dropdown in the topbar and logout in the sidebar, both consuming the existing `useAuthStore`.

**Architecture:** A new `UserMenu.vue` component renders a PrimeVue `Avatar` + `Popover` in the topbar. The sidebar gets a logout item via the menu model (using `AppMenuItem`'s existing `command` support). Both read `authStore.isLoggingOut` to disable during logout. The `authStore.logout()` gains an `isLoggingOut` reactive flag.

**Tech Stack:** Vue 3 + TypeScript, Pinia (`useAuthStore`), PrimeVue (`Avatar`, `Popover`, `Button`), Vue Router (`router.replace`), `useToast` for notifications, Vitest + `createTestingPinia` for tests.

## Global Constraints

- All components use `<script setup lang="ts">`
- Props typed with `defineProps<Props>()`
- No default exports — named exports for barrel compatibility
- Sakai CSS conventions followed (existing layout classes, PrimeVue utility classes)
- `pnpm run build` must have zero TypeScript errors
- `pnpm run test:unit -- run` must pass all tests
- API calls via existing `authStore.logout()` method

---

## File Structure

| File | Action | Responsibility |
|---|---|---|
| `features/auth/stores/authStore.ts` | Modify | Add `isLoggingOut` ref; wrap logout body |
| `features/auth/stores/__tests__/authStore.spec.ts` | Modify | Test `isLoggingOut` lifecycle |
| `shared/components/navigation/UserMenu.vue` | Create | Avatar + name + popover dropdown |
| `shared/components/navigation/__tests__/UserMenu.spec.ts` | Create | Avatar rendering, popover toggle, logout flow |
| `shared/components/navigation/AppTopbar.vue` | Modify | Remove placeholders, add `<UserMenu />` |
| `shared/components/navigation/AppMenu.vue` | Modify | Add separator + logout to menu model, handleLogout |
| `shared/components/navigation/__tests__/AppMenu.spec.ts` | Create | Logout item rendered in menu |
| `assets/layout/_topbar.scss` | Modify | Add `.logout-item` danger color rule |
| `shared/components/navigation/index.ts` | Modify | Export UserMenu |

---

### Task 1: Add `isLoggingOut` flag to authStore

**Files:**
- Modify: `app/Admin/src/features/auth/stores/authStore.ts`
- Modify: `app/Admin/src/features/auth/stores/__tests__/authStore.spec.ts`

**Interfaces:**
- Produces: `authStore.isLoggingOut: Ref<boolean>` — reactive ref consumed by UserMenu and AppMenu
- Produces: `authStore.logout()` now sets `isLoggingOut = true` before the API call and `false` after

- [ ] **Step 1: Write the failing test for `isLoggingOut` lifecycle**

```ts
// Append inside the existing describe block in authStore.spec.ts

it('sets isLoggingOut to true during logout and false after', async () => {
  const store = useAuthStore()

  // Prime the store as authenticated
  store.user = { userId: 'u1', roles: [], permissions: [], isAuthenticated: true }
  store.status = 'authenticated'

  const promise = store.logout()
  expect(store.isLoggingOut).toBe(true)

  await promise
  expect(store.isLoggingOut).toBe(false)
  expect(store.isAuthenticated).toBe(false)
})

it('sets isLoggingOut to false even when logout API fails', async () => {
  vi.mocked(authApi.logout).mockRejectedValueOnce(new Error('Network error'))
  const store = useAuthStore()
  store.user = { userId: 'u1', roles: [], permissions: [], isAuthenticated: true }
  store.status = 'authenticated'

  await store.logout()
  expect(store.isLoggingOut).toBe(false)
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `pnpm run test:unit -- run --reporter=verbose src/features/auth/stores/__tests__/authStore.spec.ts`
Expected: FAIL — `isLoggingOut` is not defined on store

- [ ] **Step 3: Add `isLoggingOut` ref to authStore**

```ts
// In the store setup function, add before status/error:
const isLoggingOut = ref(false)

// Modify the logout function to wrap with isLoggingOut:
async function logout(revokeAll?: boolean): Promise<void> {
  isLoggingOut.value = true
  try {
    await authApi.logout({ revokeAll })
  } catch {
    // Fire-and-forget — always clear local state
  }

  tokenService.clearTokens()
  user.value = null
  status.value = 'idle'
  error.value = null
  isLoggingOut.value = false
}

// Add isLoggingOut to the return object:
return {
  // ... existing ...
  isLoggingOut,
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `pnpm run test:unit -- run --reporter=verbose src/features/auth/stores/__tests__/authStore.spec.ts`
Expected: all tests PASS including the two new ones

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/auth/stores/authStore.ts app/Admin/src/features/auth/stores/__tests__/authStore.spec.ts
git commit -m "feat(admin): add isLoggingOut flag to authStore logout"
```

---

### Task 2: Create UserMenu component

**Files:**
- Create: `app/Admin/src/shared/components/navigation/UserMenu.vue`
- Create: `app/Admin/src/shared/components/navigation/__tests__/UserMenu.spec.ts`

**Interfaces:**
- Consumes: `useAuthStore()` — `isAuthenticated`, `currentUser`, `isLoggingOut`, `logout()`
- Consumes: `useRouter()` — `router.replace({ name: 'login' })` on logout
- Consumes: `useToast()` — shows success toast on logout
- Produces: `<UserMenu />` — self-contained component with no props, no emits

- [ ] **Step 1: Write the test file**

```ts
// app/Admin/src/shared/components/navigation/__tests__/UserMenu.spec.ts
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createTestingPinia } from '@pinia/testing'
import { createRouter, createWebHistory } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import UserMenu from '../UserMenu.vue'
import { useAuthStore } from '@/features/auth/stores/authStore'
import * as authApi from '@/features/auth/services/authApi'

vi.mock('primevue/usetoast', () => ({
  useToast: vi.fn(() => ({ add: vi.fn() })),
}))

vi.mock('@/features/auth/services/authApi', () => ({
  logout: vi.fn(() => Promise.resolve({ isSuccess: true, value: undefined })),
}))

function createWrapper(authOverrides = {}) {
  const router = createRouter({
    history: createWebHistory(),
    routes: [{ path: '/auth/login', name: 'login', component: { template: '<div>login</div>' } }],
  })

  return mount(UserMenu, {
    global: {
      plugins: [
        createTestingPinia({
          createSpy: vi.fn,
          stubActions: false,
          initialState: {
            auth: {
              user: { userId: 'u1', roles: [], permissions: [], isAuthenticated: true },
              status: 'authenticated',
              isLoggingOut: false,
              ...authOverrides,
            },
          },
        }),
        router,
        PrimeVue,
        ToastService,
      ],
    },
  })
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('UserMenu', () => {
  it('renders avatar and user name when authenticated', () => {
    const wrapper = createWrapper()
    expect(wrapper.text()).toContain('u1')
    expect(wrapper.find('.p-avatar').exists()).toBe(true)
  })

  it('does not render when not authenticated', () => {
    const wrapper = createWrapper({
      user: null,
      status: 'idle',
    })
    expect(wrapper.find('.p-avatar').exists()).toBe(false)
  })

  it('opens popover on avatar click', async () => {
    const wrapper = createWrapper()
    const avatarArea = wrapper.find('.cursor-pointer')
    await avatarArea.trigger('click')
    expect(wrapper.find('.p-popover').exists()).toBe(true)
  })

  it('shows logout button in popover', async () => {
    const wrapper = createWrapper()
    const avatarArea = wrapper.find('.cursor-pointer')
    await avatarArea.trigger('click')
    const logoutBtn = wrapper.find('button.logout-btn')
    expect(logoutBtn.exists()).toBe(true)
  })

  it('calls authStore.logout and shows toast on logout click', async () => {
    const toastAdd = vi.fn()
    vi.mocked(useToast).mockReturnValue({ add: toastAdd } as any)

    const wrapper = createWrapper()
    const avatarArea = wrapper.find('.cursor-pointer')
    await avatarArea.trigger('click')

    const logoutBtn = wrapper.find('button.logout-btn')
    await logoutBtn.trigger('click')

    expect(authApi.logout).toHaveBeenCalled()
    expect(toastAdd).toHaveBeenCalledWith(
      expect.objectContaining({ severity: 'info', summary: 'Logged out' }),
    )
  })

  it('disables logout button while isLoggingOut is true', () => {
    const wrapper = createWrapper({ isLoggingOut: true })
    // button won't even render if we use v-if, but if we use :disabled:
    // check the button attribute
    // Since we pass isLoggingOut to the store state, the button's :disabled binds to it
    // We need to open the popover first
    // Actually, let's test that the popover button is disabled when isLoggingOut
    // The store.isLoggingOut is set in the store — we pass it via createWrapper
    // But the Popover is closed. Let's open it first.
    // Wait — in the component, we bind :disabled="authStore.isLoggingOut" to the button.
    // We need the popover to be open to find the button.
    // This test relies on the popover being unopened by default. 
    // Let's test the disabled prop through the rendering.
    // Since the popover is closed initially, the button isn't in the DOM yet.
    // A simpler test: the topbar area has a class when isLoggingOut
    // Actually, let's just skip the popover open and bind a different way.
    // We'll use the store directly.
    // Hmm, this is hard to test with the popover closed.
    // Let me think of another way...
  })
})
```

Wait, the PrimeVue Popover renders its content only when open. Testing disabled state of a button inside a closed Popover is impossible. Let me simplify — test that the avatar area renders, and the disabled state can be tested after opening the popover:

```ts
// app/Admin/src/shared/components/navigation/__tests__/UserMenu.spec.ts
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createTestingPinia } from '@pinia/testing'
import { createRouter, createWebHistory } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import UserMenu from '../UserMenu.vue'
import * as authApi from '@/features/auth/services/authApi'

vi.mock('primevue/usetoast', () => ({
  useToast: vi.fn(() => ({ add: vi.fn() })),
}))

vi.mock('@/features/auth/services/authApi', () => ({
  logout: vi.fn(() => Promise.resolve({ isSuccess: true, value: undefined })),
}))

function createWrapper(authOverrides = {}) {
  const router = createRouter({
    history: createWebHistory(),
    routes: [{ path: '/auth/login', name: 'login', component: { template: '<div>login</div>' } }],
  })

  return mount(UserMenu, {
    global: {
      plugins: [
        createTestingPinia({
          createSpy: vi.fn,
          stubActions: false,
          initialState: {
            auth: {
              user: { userId: 'u1', roles: [], permissions: [], isAuthenticated: true },
              status: 'authenticated',
              isLoggingOut: false,
              ...authOverrides,
            },
          },
        }),
        router,
        PrimeVue,
        ToastService,
      ],
    },
  })
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('UserMenu', () => {
  it('renders avatar and user ID when authenticated', () => {
    const wrapper = createWrapper()
    expect(wrapper.text()).toContain('u1')
    expect(wrapper.find('.p-avatar').exists()).toBe(true)
  })

  it('does not render when not authenticated', () => {
    const wrapper = createWrapper({
      user: null,
      status: 'idle',
    })
    expect(wrapper.find('.p-avatar').exists()).toBe(false)
  })

  it('opens popover on avatar click', async () => {
    const wrapper = createWrapper()
    const avatarArea = wrapper.find('.cursor-pointer')
    await avatarArea.trigger('click')
    expect(wrapper.find('.p-popover').exists()).toBe(true)
  })

  it('shows logout button in popover', async () => {
    const wrapper = createWrapper()
    await wrapper.find('.cursor-pointer').trigger('click')
    const logoutBtn = wrapper.find('button.logout-btn')
    expect(logoutBtn.exists()).toBe(true)
  })

  it('calls authStore.logout and shows toast on logout click', async () => {
    const toastAdd = vi.fn()
    vi.mocked(useToast).mockReturnValue({ add: toastAdd } as any)

    const wrapper = createWrapper()
    await wrapper.find('.cursor-pointer').trigger('click')
    await wrapper.find('button.logout-btn').trigger('click')

    expect(authApi.logout).toHaveBeenCalled()
    expect(toastAdd).toHaveBeenCalledWith(
      expect.objectContaining({ severity: 'info', summary: 'Logged out' }),
    )
  })

  it('disables logout button when isLoggingOut is true', async () => {
    const wrapper = createWrapper({ isLoggingOut: true })
    await wrapper.find('.cursor-pointer').trigger('click')
    const logoutBtn = wrapper.find('button.logout-btn')
    expect(logoutBtn.attributes('disabled')).toBeDefined()
  })

  it('redirects to login after logout', async () => {
    const wrapper = createWrapper()
    await wrapper.find('.cursor-pointer').trigger('click')
    await wrapper.find('button.logout-btn').trigger('click')

    await wrapper.vm.$nextTick()
    const router = wrapper.router
    expect(router.currentRoute.value.name).toBe('login')
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `pnpm run test:unit -- run --reporter=verbose src/shared/components/navigation/__tests__/UserMenu.spec.ts`
Expected: FAIL — module not found (file doesn't exist yet) or mounting error

- [ ] **Step 3: Write the UserMenu component**

```vue
<!-- app/Admin/src/shared/components/navigation/UserMenu.vue -->
<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import Avatar from 'primevue/avatar'
import Popover from 'primevue/popover'
import Button from 'primevue/button'
import { useAuthStore } from '@/features/auth/stores/authStore'

const authStore = useAuthStore()
const router = useRouter()
const toast = useToast()

const popover = ref<InstanceType<typeof Popover> | null>(null)

function togglePopover(event: Event) {
  ;(popover.value as any)?.toggle(event)
}

async function handleLogout() {
  await authStore.logout()
  toast.add({ severity: 'info', summary: 'Logged out', life: 3000 })
  router.replace({ name: 'login' })
}
</script>

<template>
  <div v-if="authStore.isAuthenticated" class="flex align-items-center gap-2">
    <div class="flex align-items-center gap-2 cursor-pointer" @click="togglePopover">
      <Avatar :label="authStore.currentUser?.userId?.charAt(0)?.toUpperCase() ?? '?'" shape="circle" size="large" />
      <span class="font-medium text-color hidden md:inline">{{ authStore.currentUser?.userId ?? 'User' }}</span>
    </div>

    <Popover ref="popover">
      <div class="flex flex-column gap-3" style="min-width: 16rem">
        <div class="flex flex-column gap-1">
          <span class="text-sm text-color-secondary">{{ authStore.currentUser?.userId }}</span>
        </div>
        <router-link to="/profile" class="flex align-items-center gap-2 p-ripple no-underline text-color p-2 border-round surface-hover">
          <i class="pi pi-user" />
          <span>Profile</span>
        </router-link>
        <div class="border-top-1 surface-border" />
        <Button
          label="Logout"
          icon="pi pi-sign-out"
          severity="danger"
          text
          class="logout-btn w-full justify-content-start"
          :disabled="authStore.isLoggingOut"
          @click="handleLogout"
        />
      </div>
    </Popover>
  </div>
</template>
```

- [ ] **Step 4: Run test to verify it passes**

Run: `pnpm run test:unit -- run --reporter=verbose src/shared/components/navigation/__tests__/UserMenu.spec.ts`
Expected: all 7 tests PASS

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/shared/components/navigation/UserMenu.vue app/Admin/src/shared/components/navigation/__tests__/UserMenu.spec.ts
git commit -m "feat(admin): add UserMenu component with avatar and popover logout"
```

---

### Task 3: Integrate UserMenu into AppTopbar

**Files:**
- Modify: `app/Admin/src/shared/components/navigation/AppTopbar.vue`

**Interfaces:**
- Consumes: `<UserMenu />` — self-contained, no props
- Removes: placeholder buttons (Calendar, Messages, Profile) and the mobile menu button for old placeholder menu

- [ ] **Step 1: Update AppTopbar template**

Replace the `layout-topbar-menu` block (lines 54-76) with `<UserMenu />`:

```vue
<script setup lang="ts">
import { useLayout } from '@/shared/composables/useLayout';
import AppConfigurator from '../ui/AppConfigurator.vue';
import UserMenu from './UserMenu.vue';

const { toggleMenu, toggleDarkMode, isDarkTheme } = useLayout();
</script>

<template>
    <div class="layout-topbar">
        <div class="layout-topbar-logo-container">
            <button class="layout-menu-button layout-topbar-action" @click="toggleMenu">
                <i class="pi pi-bars"></i>
            </button>
            <router-link to="/" class="layout-topbar-logo">
                <svg viewBox="0 0 54 40" fill="none" xmlns="http://www.w3.org/2000/svg">
                    <path
                        fill-rule="evenodd"
                        clip-rule="evenodd"
                        d="M17.1637 19.2467C17.1566 19.4033 17.1529 19.561 17.1529 19.7194C17.1529 25.3503 21.7203 29.915 27.3546 29.915C32.9887 29.915 37.5561 25.3503 37.5561 19.7194C37.5561 19.5572 37.5524 19.3959 37.5449 19.2355C38.5617 19.0801 39.5759 18.9013 40.5867 18.6994L40.6926 18.6782C40.7191 19.0218 40.7326 19.369 40.7326 19.7194C40.7326 27.1036 34.743 33.0896 27.3546 33.0896C19.966 33.0896 13.9765 27.1036 13.9765 19.7194C13.9765 19.374 13.9896 19.0316 14.0154 18.6927L14.0486 18.6994C15.0837 18.9062 16.1223 19.0886 17.1637 19.2467ZM33.3284 11.4538C31.6493 10.2396 29.5855 9.52381 27.3546 9.52381C25.1195 9.52381 23.0524 10.2421 21.3717 11.4603C20.0078 11.3232 18.6475 11.1387 17.2933 10.907C19.7453 8.11308 23.3438 6.34921 27.3546 6.34921C31.36 6.34921 34.9543 8.10844 37.4061 10.896C36.0521 11.1292 34.692 11.3152 33.3284 11.4538ZM43.826 18.0518C43.881 18.6003 43.9091 19.1566 43.9091 19.7194C43.9091 28.8568 36.4973 36.2642 27.3546 36.2642C18.2117 36.2642 10.8 28.8568 10.8 19.7194C10.8 19.1615 10.8276 18.61 10.8816 18.0663L7.75383 17.4411C7.66775 18.1886 7.62354 18.9488 7.62354 19.7194C7.62354 30.6102 16.4574 39.4388 27.3546 39.4388C38.2517 39.4388 47.0855 30.6102 47.0855 19.7194C47.0855 18.9439 47.0407 18.1789 46.9536 17.4267L43.826 18.0518ZM44.2613 9.54743L40.9084 10.2176C37.9134 5.95821 32.9593 3.1746 27.3546 3.1746C21.7442 3.1746 16.7856 5.96385 13.7915 10.2305L10.4399 9.56057C13.892 3.83178 20.1756 0 27.3546 0C34.5281 0 40.8075 3.82591 44.2613 9.54743Z"
                        fill="var(--primary-color)"
                    />
                    <mask id="mask0_1413_1551" style="mask-type: alpha" maskUnits="userSpaceOnUse" x="0" y="8" width="54" height="11">
                        <path d="M27 18.3652C10.5114 19.1944 0 8.88892 0 8.88892C0 8.88892 16.5176 14.5866 27 14.5866C37.4824 14.5866 54 8.88892 54 8.88892C54 8.88892 43.4886 17.5361 27 18.3652Z" fill="var(--primary-color)" />
                    </mask>
                    <g mask="url(#mask0_1413_1551)">
                        <path
                            d="M-4.673e-05 8.88887L3.73084 -1.91434L-8.00806 17.0473L-4.673e-05 8.88887ZM27 18.3652L26.4253 6.95109L27 18.3652ZM54 8.88887L61.2673 17.7127L50.2691 -1.91434L54 8.88887ZM-4.673e-05 8.88887C-8.00806 17.0473 -8.00469 17.0505 -8.00132 17.0538C-8.00018 17.055 -7.99675 17.0583 -7.9944 17.0607C-7.98963 17.0653 -7.98474 17.0701 -7.97966 17.075C-7.96949 17.0849 -7.95863 17.0955 -7.94707 17.1066C-7.92401 17.129 -7.89809 17.1539 -7.86944 17.1812C-7.8122 17.236 -7.74377 17.3005 -7.66436 17.3743C-7.50567 17.5218 -7.30269 17.7063 -7.05645 17.9221C-6.56467 18.3532 -5.89662 18.9125 -5.06089 19.5534C-3.39603 20.83 -1.02575 22.4605 1.98012 24.0457C7.97874 27.2091 16.7723 30.3226 27.5746 29.7793L26.4253 6.95109C20.7391 7.23699 16.0326 5.61231 12.6534 3.83024C10.9703 2.94267 9.68222 2.04866 8.86091 1.41888C8.45356 1.10653 8.17155 0.867278 8.0241 0.738027C7.95072 0.673671 7.91178 0.637576 7.90841 0.634492C7.90682 0.63298 7.91419 0.639805 7.93071 0.65557C7.93897 0.663455 7.94952 0.673589 7.96235 0.686039C7.96883 0.692262 7.97582 0.699075 7.98338 0.706471C7.98719 0.710167 7.99113 0.714014 7.99526 0.718014C7.99729 0.720008 8.00047 0.723119 8.00148 0.724116C8.00466 0.727265 8.00796 0.730446 -4.673e-05 8.88887ZM27.5746 29.7793C37.6904 29.2706 45.9416 26.3684 51.6602 23.6054C54.5296 22.2191 56.8064 20.8465 58.4186 19.7784C59.2265 19.2431 59.873 18.7805 60.3494 18.4257C60.5878 18.2482 60.7841 18.0971 60.9374 17.977C61.014 17.9169 61.0799 17.8645 61.1349 17.8203C61.1624 17.7981 61.1872 17.7781 61.2093 17.7602C61.2203 17.7512 61.2307 17.7427 61.2403 17.7348C61.2452 17.7308 61.2499 17.727 61.2544 17.7233C61.2566 17.7215 61.2598 17.7188 61.261 17.7179C61.2642 17.7153 61.2673 17.7127 54 8.88887C46.7326 0.0650536 46.7357 0.0625219 46.7387 0.0600241C46.7397 0.0592345 46.7427 0.0567658 46.7446 0.0551857C46.7485 0.0520238 46.7521 0.0489887 46.7557 0.0460799C46.7628 0.0402623 46.7694 0.0349487 46.7753 0.0301318C46.7871 0.0204986 46.7966 0.0128495 46.8037 0.00712562C46.818 ... (truncated)
                            fill="var(--primary-color)"
                        />
                    </g>
                </svg>

                <span>SAKAI</span>
            </router-link>
        </div>

        <div class="layout-topbar-actions">
            <div class="layout-config-menu">
                <button type="button" class="layout-topbar-action" @click="toggleDarkMode">
                    <i :class="['pi', { 'pi-moon': isDarkTheme, 'pi-sun': !isDarkTheme }]"></i>
                </button>
                <div class="relative">
                    <button
                        v-styleclass="{ selector: '@next', enterFromClass: 'hidden', enterActiveClass: 'p-anchored-overlay-enter-active', leaveToClass: 'hidden', leaveActiveClass: 'p-anchored-overlay-leave-active', hideOnOutsideClick: true }"
                        type="button"
                        class="layout-topbar-action layout-topbar-action-highlight"
                    >
                        <i class="pi pi-palette"></i>
                    </button>
                    <AppConfigurator />
                </div>
            </div>

            <UserMenu />
        </div>
    </div>
</template>
```

Note: Remove the `v-styleclass` button that controls `layout-topbar-menu`, the `layout-topbar-menu` div itself, and the `layout-topbar-menu-button`. Keep: `toggleDarkMode` button, palette `AppConfigurator`.

- [ ] **Step 2: Run build to verify no TS errors**

Run: `pnpm run build`
Expected: zero errors

- [ ] **Step 3: Run all tests**

Run: `pnpm run test:unit -- run`
Expected: all tests pass

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/shared/components/navigation/AppTopbar.vue
git commit -m "feat(admin): replace topbar placeholder buttons with UserMenu"
```

---

### Task 4: Add logout item to sidebar menu

**Files:**
- Modify: `app/Admin/src/shared/components/navigation/AppMenu.vue`
- Create: `app/Admin/src/shared/components/navigation/__tests__/AppMenu.spec.ts`

**Interfaces:**
- Consumes: `useAuthStore()` — `isLoggingOut`, `logout()`
- Consumes: `useRouter()` — `router.replace({ name: 'login' })`
- Consumes: `useToast()` — success toast

- [ ] **Step 1: Write the test file**

```ts
// app/Admin/src/shared/components/navigation/__tests__/AppMenu.spec.ts
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createTestingPinia } from '@pinia/testing'
import { createRouter, createWebHistory } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import AppMenu from '../AppMenu.vue'
import AppMenuItem from '../AppMenuItem.vue'
import * as authApi from '@/features/auth/services/authApi'

vi.mock('primevue/usetoast', () => ({
  useToast: vi.fn(() => ({ add: vi.fn() })),
}))

vi.mock('@/features/auth/services/authApi', () => ({
  logout: vi.fn(() => Promise.resolve({ isSuccess: true, value: undefined })),
}))

vi.mock('@/features/dashboard/routes', () => ({ dashboardMenuItems: [{ label: 'Dashboard', icon: 'pi pi-home', to: '/' }] }))
vi.mock('@/features/catalog/routes', () => ({ catalogMenuItems: [] }))
vi.mock('@/features/identity/routes', () => ({ identityMenuItems: [] }))
vi.mock('@/features/inventory/routes', () => ({ inventoryMenuItems: [] }))
vi.mock('@/features/location/routes', () => ({ locationMenuItems: [] }))
vi.mock('@/features/ordering/routes', () => ({ orderingMenuItems: [] }))
vi.mock('@/features/payment/routes', () => ({ paymentMenuItems: [] }))
vi.mock('@/features/profile/routes', () => ({ profileMenuItems: [] }))
vi.mock('@/features/shipping/routes', () => ({ shippingMenuItems: [] }))

function createWrapper() {
  const router = createRouter({
    history: createWebHistory(),
    routes: [{ path: '/auth/login', name: 'login', component: { template: '<div>login</div>' } }],
  })

  return mount(AppMenu, {
    global: {
      plugins: [
        createTestingPinia({
          createSpy: vi.fn,
          stubActions: false,
          initialState: {
            auth: {
              user: { userId: 'u1', roles: [], permissions: [], isAuthenticated: true },
              status: 'authenticated',
              isLoggingOut: false,
            },
          },
        }),
        router,
        PrimeVue,
        ToastService,
      ],
      components: { AppMenuItem },
    },
  })
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('AppMenu', () => {
  it('renders logout menu item with sign-out icon', () => {
    const wrapper = createWrapper()
    const logoutItem = wrapper.find('.logout-item')
    expect(logoutItem.exists()).toBe(true)
    expect(logoutItem.text()).toContain('Logout')
  })

  it('renders separator before logout item', () => {
    const wrapper = createWrapper()
    const separators = wrapper.findAll('.menu-separator')
    const logoutItem = wrapper.find('.logout-item')
    // The last separator should be immediately before the logout item
    expect(separators.length).toBeGreaterThanOrEqual(1)
  })

  it('calls authStore.logout and shows toast on logout click', async () => {
    const toastAdd = vi.fn()
    vi.mocked(useToast).mockReturnValue({ add: toastAdd } as any)

    const wrapper = createWrapper()
    await wrapper.find('.logout-item').trigger('click')

    expect(authApi.logout).toHaveBeenCalled()
    expect(toastAdd).toHaveBeenCalledWith(
      expect.objectContaining({ severity: 'info', summary: 'Logged out' }),
    )
  })

  it('redirects to login after logout', async () => {
    const wrapper = createWrapper()
    await wrapper.find('.logout-item').trigger('click')
    await wrapper.vm.$nextTick()
    expect(wrapper.router.currentRoute.value.name).toBe('login')
  })

  it('disables logout item when isLoggingOut', () => {
    // This is tested by checking the opacity and pointer-events classes
    // We'll set up the store with isLoggingOut: true
    // Since we can't change store state after mount easily in this test setup,
    // we create a separate wrapper
  })
})
```

Hmm, the last test is tricky since we create the store state at mount time. Let me simplify to 4 tests:

```ts
// app/Admin/src/shared/components/navigation/__tests__/AppMenu.spec.ts
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createTestingPinia } from '@pinia/testing'
import { createRouter, createWebHistory } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import AppMenu from '../AppMenu.vue'
import AppMenuItem from '../AppMenuItem.vue'
import * as authApi from '@/features/auth/services/authApi'

vi.mock('primevue/usetoast', () => ({
  useToast: vi.fn(() => ({ add: vi.fn() })),
}))

vi.mock('@/features/auth/services/authApi', () => ({
  logout: vi.fn(() => Promise.resolve({ isSuccess: true, value: undefined })),
}))

vi.mock('@/features/dashboard/routes', () => ({ dashboardMenuItems: [{ label: 'Dashboard', icon: 'pi pi-home', to: '/' }] }))
vi.mock('@/features/catalog/routes', () => ({ catalogMenuItems: [] }))
vi.mock('@/features/identity/routes', () => ({ identityMenuItems: [] }))
vi.mock('@/features/inventory/routes', () => ({ inventoryMenuItems: [] }))
vi.mock('@/features/location/routes', () => ({ locationMenuItems: [] }))
vi.mock('@/features/ordering/routes', () => ({ orderingMenuItems: [] }))
vi.mock('@/features/payment/routes', () => ({ paymentMenuItems: [] }))
vi.mock('@/features/profile/routes', () => ({ profileMenuItems: [] }))
vi.mock('@/features/shipping/routes', () => ({ shippingMenuItems: [] }))

function createWrapper(isLoggingOut = false) {
  const router = createRouter({
    history: createWebHistory(),
    routes: [{ path: '/auth/login', name: 'login', component: { template: '<div>login</div>' } }],
  })

  return mount(AppMenu, {
    global: {
      plugins: [
        createTestingPinia({
          createSpy: vi.fn,
          stubActions: false,
          initialState: {
            auth: {
              user: { userId: 'u1', roles: [], permissions: [], isAuthenticated: true },
              status: 'authenticated',
              isLoggingOut,
            },
          },
        }),
        router,
        PrimeVue,
        ToastService,
      ],
      components: { AppMenuItem },
    },
  })
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('AppMenu', () => {
  it('renders logout menu item with sign-out icon', () => {
    const wrapper = createWrapper()
    const logoutItem = wrapper.find('.logout-item')
    expect(logoutItem.exists()).toBe(true)
    expect(logoutItem.text()).toContain('Logout')
  })

  it('calls authStore.logout and shows toast on logout click', async () => {
    const toastAdd = vi.fn()
    vi.mocked(useToast).mockReturnValue({ add: toastAdd } as any)

    const wrapper = createWrapper()
    await wrapper.find('.logout-item').trigger('click')

    expect(authApi.logout).toHaveBeenCalled()
    expect(toastAdd).toHaveBeenCalledWith(
      expect.objectContaining({ severity: 'info', summary: 'Logged out' }),
    )
  })

  it('redirects to login after logout', async () => {
    const wrapper = createWrapper()
    await wrapper.find('.logout-item').trigger('click')
    await wrapper.vm.$nextTick()
    expect(wrapper.router.currentRoute.value.name).toBe('login')
  })

  it('applies disabled styling when isLoggingOut is true', () => {
    const wrapper = createWrapper(true)
    const logoutItem = wrapper.find('.logout-item')
    expect(logoutItem.classes()).toContain('pointer-events-none')
    expect(logoutItem.classes()).toContain('opacity-50')
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

Run: `pnpm run test:unit -- run --reporter=verbose src/shared/components/navigation/__tests__/AppMenu.spec.ts`
Expected: FAIL — no `.logout-item` found

- [ ] **Step 3: Update AppMenu.vue with logout item**

Add to the script section:

```ts
import { useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import { useAuthStore } from '@/features/auth/stores/authStore'

const router = useRouter()
const toast = useToast()
const authStore = useAuthStore()

async function handleLogout() {
  await authStore.logout()
  toast.add({ severity: 'info', summary: 'Logged out', life: 3000 })
  router.replace({ name: 'login' })
}
```

Add to the template, after the `<app-menu-item>` loop and before the closing `</ul>`:

```html
        <template v-for="(item, i) in model" :key="item">
            <app-menu-item v-if="!item.separator" :item="item" :index="i"></app-menu-item>
            <li v-if="item.separator" class="menu-separator"></li>
        </template>
        <li class="menu-separator"></li>
        <li>
            <a class="logout-item flex align-items-center px-3 py-2 cursor-pointer border-round"
               :class="{ 'pointer-events-none opacity-50': authStore.isLoggingOut }"
               @click="handleLogout">
                <i class="pi pi-sign-out layout-menuitem-icon"></i>
                <span class="layout-menuitem-text">Logout</span>
            </a>
        </li>
    </ul>
```

- [ ] **Step 4: Run test to verify it passes**

Run: `pnpm run test:unit -- run --reporter=verbose src/shared/components/navigation/__tests__/AppMenu.spec.ts`
Expected: all 4 tests PASS

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/shared/components/navigation/AppMenu.vue app/Admin/src/shared/components/navigation/__tests__/AppMenu.spec.ts
git commit -m "feat(admin): add logout item to sidebar menu"
```

---

### Task 5: Add logout item danger color CSS

**Files:**
- Modify: `app/Admin/src/assets/layout/_topbar.scss`

**Interfaces:**
- Produces: `.logout-item` CSS class — red text, hover background

- [ ] **Step 1: Add `.logout-item` style rule**

Append at the end of `_topbar.scss`:

```scss
.logout-item {
    color: var(--red-500);

    &:hover {
        background-color: var(--surface-hover);
    }
}
```

- [ ] **Step 2: Run build to verify**

Run: `pnpm run build`
Expected: zero errors

- [ ] **Step 3: Run all tests**

Run: `pnpm run test:unit -- run`
Expected: all tests pass

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/assets/layout/_topbar.scss
git commit -m "style(admin): add logout-item danger color for sidebar"
```

---

### Task 6: Export UserMenu from navigation barrel

**Files:**
- Modify: `app/Admin/src/shared/components/navigation/index.ts`

- [ ] **Step 1: Add UserMenu export**

Read the existing file, add:
```ts
export { default as UserMenu } from './UserMenu.vue'
```

- [ ] **Step 2: Run build to verify tree-shaking**

Run: `pnpm run build`
Expected: zero errors

- [ ] **Step 3: Run all tests**

Run: `pnpm run test:unit -- run`
Expected: all tests pass

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/shared/components/navigation/index.ts
git commit -m "chore(admin): export UserMenu from navigation barrel"
```

---

### Task 7: Final integration verification

- [ ] **Step 1: Build check**

Run: `pnpm run build`
Expected: zero errors, zero warnings

- [ ] **Step 2: Full test suite**

Run: `pnpm run test:unit -- run`
Expected: all tests pass (344 existing + new tests)

- [ ] **Step 3: Lint check**

Run: `pnpm run lint`
Expected: no errors, no warnings

---

## Self-Review

1. **Spec coverage:**
   - [x] Topbar UserMenu with avatar + name + popover → Tasks 2, 3
   - [x] Popover shows user info, profile link, logout button → Task 2
   - [x] Sidebar logout item with separator → Task 4
   - [x] Both call authStore.logout() → Tasks 2, 4
   - [x] Toast on success → Tasks 2, 4
   - [x] Redirect to /auth/login → Tasks 2, 4
   - [x] Disable during logout (isLoggingOut) → Tasks 1, 2, 4
   - [x] Remove placeholder buttons → Task 3
   - [x] Logout-item danger color → Task 5
   - [x] Barrel export → Task 6
   - [x] Tests for UserMenu → Task 2
   - [x] Tests for AppMenu logout → Task 4
   - [x] Tests for isLoggingOut in authStore → Task 1

2. **Placeholder scan:** No TBD, TODO, or vague descriptions. Every step has actual code.

3. **Type consistency:**
   - `isLoggingOut: Ref<boolean>` defined in Task 1, consumed in Tasks 2, 4 → consistent
   - `authStore.logout()` modified in Task 1, called in Tasks 2, 4 → consistent
   - `router.replace({ name: 'login' })` used in Tasks 2, 4 → route `login` exists (verified in auth routes)
   - `<UserMenu />` created in Task 2, imported in Task 3 → consistent
   - `.logout-item` class defined in Task 5, used in Task 4 → Task 4 runs first but CSS is Task 5. This is fine since CSS is additive and the class just won't have the red color until Task 5.
